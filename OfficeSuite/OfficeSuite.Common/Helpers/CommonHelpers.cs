using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Globalization;

namespace OfficeSuite.Common.Helpers
{
    public class FileHelper
    {
        private static readonly Dictionary<string, string> MimeTypes = new Dictionary<string, string>
        {
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".docm", "application/vnd.ms-word.document.macroEnabled.12" },
            { ".dotx", "application/vnd.openxmlformats-officedocument.wordprocessingml.template" },
            { ".dotm", "application/vnd.ms-word.template.macroEnabled.12" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".xlsm", "application/vnd.ms-excel.sheet.macroEnabled.12" },
            { ".xlsb", "application/vnd.ms-excel.sheet.binary.macroEnabled.12" },
            { ".xltx", "application/vnd.openxmlformats-officedocument.spreadsheetml.template" },
            { ".xltm", "application/vnd.ms-excel.template.macroEnabled.12" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".pptm", "application/vnd.ms-powerpoint.presentation.macroEnabled.12" },
            { ".potx", "application/vnd.openxmlformats-officedocument.presentationml.template" },
            { ".potm", "application/vnd.ms-powerpoint.template.macroEnabled.12" },
            { ".pdf", "application/pdf" },
            { ".txt", "text/plain" },
            { ".rtf", "application/rtf" },
            { ".html", "text/html" },
            { ".xml", "application/xml" },
            { ".json", "application/json" }
        };

        public static string GetMimeType(string extension)
        {
            return MimeTypes.TryGetValue(extension.ToLowerInvariant(), out var mimeType) ? mimeType : "application/octet-stream";
        }

        public static string GetExtension(string mimeType)
        {
            var entry = MimeTypes.FirstOrDefault(x => x.Value == mimeType);
            return entry.Key;
        }

        public static bool IsOfficeDocument(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return new[] { ".docx", ".docm", ".xlsx", ".xlsm", ".xlsb", ".pptx", ".pptm" }.Contains(extension);
        }

        public static bool IsWordDocument(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return new[] { ".docx", ".docm", ".dotx", ".dotm" }.Contains(extension);
        }

        public static bool IsSpreadsheetDocument(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return new[] { ".xlsx", ".xlsm", ".xlsb", ".xltx", ".xltm" }.Contains(extension);
        }

        public static bool IsPresentationDocument(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return new[] { ".pptx", ".pptm", ".potx", ".potm" }.Contains(extension);
        }

        public static string GetDocumentType(string filePath)
        {
            if (IsWordDocument(filePath)) return "Word";
            if (IsSpreadsheetDocument(filePath)) return "Spreadsheet";
            if (IsPresentationDocument(filePath)) return "Presentation";
            return "Unknown";
        }

        public static long GetFileSize(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            return fileInfo.Length;
        }

        public static string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int suffixIndex = 0;
            double size = bytes;

            while (size >= 1024 && suffixIndex < suffixes.Length - 1)
            {
                size /= 1024;
                suffixIndex++;
            }

            return $"{size:0.##} {suffixes[suffixIndex]}";
        }

        public static string GetTempFilePath(string extension)
        {
            var tempPath = Path.GetTempPath();
            var fileName = $"{Guid.NewGuid():N}{extension}";
            return Path.Combine(tempPath, fileName);
        }

        public static string GenerateUniqueFileName(string directory, string baseName, string extension)
        {
            var fileName = $"{baseName}{extension}";
            var fullPath = Path.Combine(directory, fileName);
            var counter = 1;

            while (File.Exists(fullPath))
            {
                fileName = $"{baseName} ({counter}){extension}";
                fullPath = Path.Combine(directory, fileName);
                counter++;
            }

            return fullPath;
        }

        public static void EnsureDirectoryExists(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        public static bool CopyFile(string sourcePath, string destinationPath, bool overwrite = true)
        {
            try
            {
                EnsureDirectoryExists(destinationPath);
                File.Copy(sourcePath, destinationPath, overwrite);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool MoveFile(string sourcePath, string destinationPath, bool overwrite = true)
        {
            try
            {
                EnsureDirectoryExists(destinationPath);
                if (overwrite && File.Exists(destinationPath))
                {
                    File.Delete(destinationPath);
                }
                File.Move(sourcePath, destinationPath);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool DeleteFile(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ReadFileText(string filePath, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            return File.ReadAllText(filePath, encoding);
        }

        public static bool WriteFileText(string filePath, string content, Encoding encoding = null)
        {
            try
            {
                encoding ??= Encoding.UTF8;
                EnsureDirectoryExists(filePath);
                File.WriteAllText(filePath, content, encoding);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static byte[] ReadFileBytes(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }

        public static bool WriteFileBytes(string filePath, byte[] data)
        {
            try
            {
                EnsureDirectoryExists(filePath);
                File.WriteAllBytes(filePath, data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string ComputeFileHash(string filePath, HashAlgorithm algorithm = null)
        {
            algorithm ??= SHA256.Create();
            using (var stream = File.OpenRead(filePath))
            using (algorithm)
            {
                var hashBytes = algorithm.ComputeHash(stream);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static bool VerifyFileHash(string filePath, string expectedHash, HashAlgorithm algorithm = null)
        {
            var actualHash = ComputeFileHash(filePath, algorithm);
            return string.Equals(actualHash, expectedHash, StringComparison.OrdinalIgnoreCase);
        }

        public static List<string> GetRecentFiles(string directory, string searchPattern, int maxFiles = 10)
        {
            if (!Directory.Exists(directory))
                return new List<string>();

            return Directory.GetFiles(directory, searchPattern, SearchOption.TopDirectoryOnly)
                .OrderByDescending(f => File.GetLastWriteTime(f))
                .Take(maxFiles)
                .ToList();
        }

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }
        }

        public static DateTime GetFileCreationTime(string filePath)
        {
            return File.GetCreationTime(filePath);
        }

        public static DateTime GetFileLastModifiedTime(string filePath)
        {
            return File.GetLastWriteTime(filePath);
        }

        public static bool SetFileLastModifiedTime(string filePath, DateTime dateTime)
        {
            try
            {
                File.SetLastWriteTime(filePath, dateTime);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string SanitizeFileName(string fileName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            return new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
        }

        public static string GetRelativePath(string fromPath, string toPath)
        {
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme)
                return toPath;

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }

        public static string GetAbsolutePath(string basePath, string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
                return relativePath;

            return Path.GetFullPath(Path.Combine(basePath, relativePath));
        }

        public static string GetDisplayPath(string filePath, int maxLength = 50)
        {
            if (filePath.Length <= maxLength)
                return filePath;

            var fileName = Path.GetFileName(filePath);
            var directory = Path.GetDirectoryName(filePath);

            if (fileName.Length >= maxLength)
                return fileName.Substring(0, maxLength - 3) + "...";

            var availableLength = maxLength - fileName.Length - 4;
            if (availableLength <= 0)
                return fileName;

            return directory.Substring(0, Math.Min(availableLength, directory.Length)) + "...\\" + fileName;
        }
    }

    public class StringHelper
    {
        public static string Truncate(string value, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - suffix.Length) + suffix;
        }

        public static string RemoveExtraSpaces(string value)
        {
            return Regex.Replace(value.Trim(), @"\s+", " ");
        }

        public static string RemoveWhitespace(string value)
        {
            return new string(value.Where(c => !char.IsWhiteSpace(c)).ToArray());
        }

        public static string ToCamelCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var words = value.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i];
                if (i == 0)
                {
                    result.Append(char.ToLowerInvariant(word[0]));
                    if (word.Length > 1)
                        result.Append(word.Substring(1).ToLowerInvariant());
                }
                else
                {
                    result.Append(char.ToUpperInvariant(word[0]));
                    if (word.Length > 1)
                        result.Append(word.Substring(1).ToLowerInvariant());
                }
            }

            return result.ToString();
        }

        public static string ToPascalCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var words = value.Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
            var result = new StringBuilder();

            foreach (var word in words)
            {
                if (word.Length > 0)
                {
                    result.Append(char.ToUpperInvariant(word[0]));
                    if (word.Length > 1)
                        result.Append(word.Substring(1).ToLowerInvariant());
                }
            }

            return result.ToString();
        }

        public static string ToSnakeCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result = Regex.Replace(value, @"([a-z])([A-Z])", "$1_$2");
            result = Regex.Replace(result, @"\s+", "_");
            return result.ToLowerInvariant();
        }

        public static string ToKebabCase(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var result = Regex.Replace(value, @"([a-z])([A-Z])", "$1-$2");
            result = Regex.Replace(result, @"\s+", "-");
            return result.ToLowerInvariant();
        }

        public static string ToTitleCase(string value)
        {
            return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
        }

        public static string Reverse(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var chars = value.ToCharArray();
            Array.Reverse(chars);
            return new string(chars);
        }

        public static string ReverseWords(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            var words = value.Split(' ');
            Array.Reverse(words);
            return string.Join(" ", words);
        }

        public static string CapitalizeFirstLetter(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return char.ToUpperInvariant(value[0]) + value.Substring(1);
        }

        public static string UncapitalizeFirstLetter(string value)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            return char.ToLowerInvariant(value[0]) + value.Substring(1);
        }

        public static int CountWords(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return 0;

            return value.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static int CountLines(string value)
        {
            if (string.IsNullOrEmpty(value))
                return 0;

            return value.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static string Left(string value, int length)
        {
            if (string.IsNullOrEmpty(value) || length <= 0)
                return string.Empty;

            return value.Substring(0, Math.Min(length, value.Length));
        }

        public static string Right(string value, int length)
        {
            if (string.IsNullOrEmpty(value) || length <= 0)
                return string.Empty;

            return value.Substring(Math.Max(0, value.Length - length));
        }

        public static string Mid(string value, int start, int length)
        {
            if (string.IsNullOrEmpty(value) || start < 0 || length <= 0)
                return string.Empty;

            start = Math.Max(0, start - 1);
            if (start >= value.Length)
                return string.Empty;

            return value.Substring(start, Math.Min(length, value.Length - start));
        }

        public static string PadLeft(string value, int totalWidth, char paddingChar = ' ')
        {
            return value.PadLeft(totalWidth, paddingChar);
        }

        public static string PadRight(string value, int totalWidth, char paddingChar = ' ')
        {
            return value.PadRight(totalWidth, paddingChar);
        }

        public static string Insert(string value, int index, string insertValue)
        {
            if (string.IsNullOrEmpty(value) || index < 0)
                return value;

            if (index > value.Length)
                index = value.Length;

            return value.Insert(index, insertValue);
        }

        public static string RemoveAt(string value, int index, int length = 1)
        {
            if (string.IsNullOrEmpty(value) || index < 0 || index >= value.Length)
                return value;

            return value.Remove(index, Math.Min(length, value.Length - index));
        }

        public static string Replace(string value, string oldValue, string newValue, StringComparison comparison = StringComparison.Ordinal)
        {
            var result = value;
            var index = result.IndexOf(oldValue, comparison);

            while (index >= 0)
            {
                result = result.Remove(index, oldValue.Length).Insert(index, newValue);
                index = result.IndexOf(oldValue, index + newValue.Length, comparison);
            }

            return result;
        }

        public static string ReplaceAt(string value, int index, int length, string newValue)
        {
            if (string.IsNullOrEmpty(value) || index < 0 || index >= value.Length)
                return value;

            return value.Remove(index, Math.Min(length, value.Length - index)).Insert(index, newValue);
        }

        public static string Join(string separator, params string[] values)
        {
            return string.Join(separator, values);
        }

        public static string[] Split(string value, string separator, StringSplitOptions options = StringSplitOptions.None)
        {
            return value.Split(new[] { separator }, options);
        }

        public static bool Contains(string value, string searchValue, StringComparison comparison = StringComparison.Ordinal)
        {
            return value.IndexOf(searchValue, comparison) >= 0;
        }

        public static bool StartsWith(string value, string searchValue, StringComparison comparison = StringComparison.Ordinal)
        {
            return value.StartsWith(searchValue, comparison);
        }

        public static bool EndsWith(string value, string searchValue, StringComparison comparison = StringComparison.Ordinal)
        {
            return value.EndsWith(searchValue, comparison);
        }

        public static string ToBase64(string value, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var bytes = encoding.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        public static string FromBase64(string base64Value, Encoding encoding = null)
        {
            encoding ??= Encoding.UTF8;
            var bytes = Convert.FromBase64String(base64Value);
            return encoding.GetString(bytes);
        }

        public static string ToHex(byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        public static byte[] FromHex(string hexValue)
        {
            var hex = hexValue.Replace(" ", "");
            var bytes = new byte[hex.Length / 2];

            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }

            return bytes;
        }

        public static string GenerateRandomString(int length, bool includeLetters = true, bool includeNumbers = true, bool includeSpecialChars = false)
        {
            var chars = new StringBuilder();

            if (includeLetters)
                chars.Append("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz");
            if (includeNumbers)
                chars.Append("0123456789");
            if (includeSpecialChars)
                chars.Append("!@#$%^&*()_+-=[]{}|;:,.<>?");

            var result = new char[length];
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                result[i] = chars[random.Next(chars.Length)];
            }

            return new string(result);
        }

        public static string NormalizeLineEndings(string value, string lineEnding = "\r\n")
        {
            return value.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", lineEnding);
        }

        public static string StripHtml(string html)
        {
            return Regex.Replace(html, "<.*?>", string.Empty);
        }

        public static string StripXml(string xml)
        {
            return Regex.Replace(xml, @"<.*?>", string.Empty, RegexOptions.Singleline);
        }

        public static bool IsNullOrEmpty(string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(string value)
        {
            return string.IsNullOrWhiteSpace(value);
        }
    }

    public class DateTimeHelper
    {
        public static string FormatDate(DateTime date, string format = "d")
        {
            return date.ToString(format);
        }

        public static string FormatTime(DateTime time, string format = "t")
        {
            return time.ToString(format);
        }

        public static string FormatDateTime(DateTime dateTime, string format = "g")
        {
            return dateTime.ToString(format);
        }

        public static string ToIso8601(DateTime dateTime)
        {
            return dateTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
        }

        public static DateTime FromIso8601(string value)
        {
            return DateTime.Parse(value);
        }

        public static string ToUnixTimestamp(DateTime dateTime)
        {
            return ((DateTimeOffset)dateTime).ToUnixTimeSeconds().ToString();
        }

        public static DateTime FromUnixTimestamp(string timestamp)
        {
            var seconds = long.Parse(timestamp);
            return DateTimeOffset.FromUnixTimeSeconds(seconds).DateTime;
        }

        public static DateTime GetStartOfDay(DateTime date)
        {
            return date.Date;
        }

        public static DateTime GetEndOfDay(DateTime date)
        {
            return date.Date.AddDays(1).AddTicks(-1);
        }

        public static DateTime GetStartOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday)
        {
            int diff = (7 + (date.DayOfWeek - startOfWeek)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        public static DateTime GetEndOfWeek(DateTime date, DayOfWeek startOfWeek = DayOfWeek.Sunday)
        {
            return GetStartOfWeek(date, startOfWeek).AddDays(7).AddTicks(-1);
        }

        public static DateTime GetStartOfMonth(DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime GetEndOfMonth(DateTime date)
        {
            return GetStartOfMonth(date).AddMonths(1).AddTicks(-1);
        }

        public static DateTime GetStartOfQuarter(DateTime date)
        {
            int quarter = (date.Month - 1) / 3;
            return new DateTime(date.Year, quarter * 3 + 1, 1);
        }

        public static DateTime GetEndOfQuarter(DateTime date)
        {
            return GetStartOfQuarter(date).AddMonths(3).AddTicks(-1);
        }

        public static DateTime GetStartOfYear(DateTime date)
        {
            return new DateTime(date.Year, 1, 1);
        }

        public static DateTime GetEndOfYear(DateTime date)
        {
            return new DateTime(date.Year, 12, 31, 23, 59, 59, 999);
        }

        public static int GetQuarter(DateTime date)
        {
            return (date.Month - 1) / 3 + 1;
        }

        public static int GetWeekOfYear(DateTime date)
        {
            var culture = CultureInfo.CurrentCulture;
            return culture.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
        }

        public static int GetDayOfYear(DateTime date)
        {
            return date.DayOfYear;
        }

        public static bool IsWeekend(DateTime date)
        {
            return date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsWeekday(DateTime date)
        {
            return !IsWeekend(date);
        }

        public static bool IsLeapYear(int year)
        {
            return DateTime.IsLeapYear(year);
        }

        public static bool IsLeapYear(DateTime date)
        {
            return DateTime.IsLeapYear(date.Year);
        }

        public static int DaysInMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }

        public static int DaysInMonth(DateTime date)
        {
            return DateTime.DaysInMonth(date.Year, date.Month);
        }

        public static DateTime AddBusinessDays(DateTime date, int days)
        {
            var result = date;
            var increment = days > 0 ? 1 : -1;
            var remainingDays = Math.Abs(days);

            while (remainingDays > 0)
            {
                result = result.AddDays(increment);
                if (IsWeekday(result))
                    remainingDays--;
            }

            return result;
        }

        public static int GetBusinessDays(DateTime startDate, DateTime endDate)
        {
            var days = 0;
            var current = startDate;

            while (current <= endDate)
            {
                if (IsWeekday(current))
                    days++;
                current = current.AddDays(1);
            }

            return days;
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

        public static string GetAge(DateTime birthDate)
        {
            var today = DateTime.Today;
            var age = today.Year - birthDate.Year;

            if (birthDate.Date > today.AddYears(-age))
                age--;

            return $"{age} years";
        }

        public static DateTime GetNextOccurrence(DayOfWeek dayOfWeek, DateTime fromDate = default(DateTime))
        {
            fromDate = fromDate == default(DateTime) ? DateTime.Today : fromDate;
            var daysUntil = ((int)dayOfWeek - (int)fromDate.DayOfWeek + 7) % 7;
            return fromDate.AddDays(daysUntil == 0 ? 7 : daysUntil);
        }

        public static bool IsSameDay(DateTime date1, DateTime date2)
        {
            return date1.Date == date2.Date;
        }

        public static bool IsSameMonth(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year && date1.Month == date2.Month;
        }

        public static bool IsSameYear(DateTime date1, DateTime date2)
        {
            return date1.Year == date2.Year;
        }

        public static TimeSpan GetAgeAsTimeSpan(DateTime birthDate)
        {
            return DateTime.Now - birthDate;
        }

        public static int CompareDates(DateTime date1, DateTime date2)
        {
            if (date1 < date2) return -1;
            if (date1 > date2) return 1;
            return 0;
        }

        public static string FormatDuration(TimeSpan duration)
        {
            if (duration.TotalHours >= 1)
                return $"{(int)duration.TotalHours}:{duration.Minutes:D2}:{duration.Seconds:D2}";
            if (duration.TotalMinutes >= 1)
                return $"{duration.Minutes}:{duration.Seconds:D2}";
            return $"{duration.Seconds}s";
        }

        public static TimeSpan ParseDuration(string duration)
        {
            var parts = duration.Split(':');
            if (parts.Length == 3)
                return new TimeSpan(int.Parse(parts[0]), int.Parse(parts[1]), int.Parse(parts[2]));
            if (parts.Length == 2)
                return new TimeSpan(0, int.Parse(parts[0]), int.Parse(parts[1]));
            return TimeSpan.Zero;
        }
    }

    public class MathHelper
    {
        public static int Round(double value)
        {
            return (int)Math.Round(value);
        }

        public static int Round(double value, int digits)
        {
            return (int)Math.Round(value, digits);
        }

        public static int Floor(double value)
        {
            return (int)Math.Floor(value);
        }

        public static int Ceiling(double value)
        {
            return (int)Math.Ceiling(value);
        }

        public static int Truncate(double value)
        {
            return (int)Math.Truncate(value);
        }

        public static double Clamp(double value, double min, double max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static int Clamp(int value, int min, int max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static double Normalize(double value, double min, double max)
        {
            if (max == min) return 0;
            return (value - min) / (max - min);
        }

        public static double Denormalize(double normalized, double min, double max)
        {
            return min + normalized * (max - min);
        }

        public static double Lerp(double a, double b, double t)
        {
            return a + (b - a) * Clamp(t, 0, 1);
        }

        public static double InverseLerp(double a, double b, double value)
        {
            if (a == b) return 0;
            return Clamp((value - a) / (b - a), 0, 1);
        }

        public static double Map(double value, double inMin, double inMax, double outMin, double outMax)
        {
            return outMin + (outMax - outMin) * Normalize(value, inMin, inMax);
        }

        public static double Percentage(double value, double total)
        {
            if (total == 0) return 0;
            return (value / total) * 100;
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * 180 / Math.PI;
        }

        public static double DegreesToRadians(double degrees)
        {
            return degrees * Math.PI / 180;
        }

        public static double Sine(double angle)
        {
            return Math.Sin(DegreesToRadians(angle));
        }

        public static double Cosine(double angle)
        {
            return Math.Cos(DegreesToRadians(angle));
        }

        public static double Tangent(double angle)
        {
            return Math.Tan(DegreesToRadians(angle));
        }

        public static double ArcSine(double value)
        {
            return RadiansToDegrees(Math.Asin(value));
        }

        public static double ArcCosine(double value)
        {
            return RadiansToDegrees(Math.Acos(value));
        }

        public static double ArcTangent(double value)
        {
            return RadiansToDegrees(Math.Atan(value));
        }

        public static double Power(double baseValue, double exponent)
        {
            return Math.Pow(baseValue, exponent);
        }

        public static double SquareRoot(double value)
        {
            if (value < 0) return double.NaN;
            return Math.Sqrt(value);
        }

        public static double CubeRoot(double value)
        {
            return Math.Pow(value, 1.0 / 3.0);
        }

        public static double Absolute(double value)
        {
            return Math.Abs(value);
        }

        public static int Sign(double value)
        {
            if (value > 0) return 1;
            if (value < 0) return -1;
            return 0;
        }

        public static double Modulo(double dividend, double divisor)
        {
            return dividend % divisor;
        }

        public static double Max(params double[] values)
        {
            return values.Max();
        }

        public static double Min(params double[] values)
        {
            return values.Min();
        }

        public static int Max(params int[] values)
        {
            return values.Max();
        }

        public static int Min(params int[] values)
        {
            return values.Min();
        }

        public static double Average(params double[] values)
        {
            return values.Average();
        }

        public static double Sum(params double[] values)
        {
            return values.Sum();
        }

        public static double Product(params double[] values)
        {
            return values.Aggregate(1.0, (a, b) => a * b);
        }

        public static double Factorial(int n)
        {
            if (n < 0) return double.NaN;
            if (n <= 1) return 1;

            double result = 1;
            for (int i = 2; i <= n; i++)
                result *= i;

            return result;
        }

        public static double Permutations(int n, int r)
        {
            if (r > n || n < 0 || r < 0) return 0;
            return Factorial(n) / Factorial(n - r);
        }

        public static double Combinations(int n, int r)
        {
            if (r > n || n < 0 || r < 0) return 0;
            return Factorial(n) / (Factorial(r) * Factorial(n - r));
        }

        public static double StandardDeviation(IEnumerable<double> values)
        {
            var list = values.ToList();
            var avg = list.Average();
            var sumOfSquares = list.Sum(x => Math.Pow(x - avg, 2));
            return Math.Sqrt(sumOfSquares / list.Count);
        }

        public static double Variance(IEnumerable<double> values)
        {
            var list = values.ToList();
            var avg = list.Average();
            return list.Average(x => Math.Pow(x - avg, 2));
        }

        public static double Median(IEnumerable<double> values)
        {
            var sorted = values.OrderBy(x => x).ToList();
            int n = sorted.Count;
            if (n % 2 == 0)
                return (sorted[n / 2 - 1] + sorted[n / 2]) / 2;
            return sorted[n / 2];
        }

        public static double Mode(IEnumerable<double> values)
        {
            return values.GroupBy(x => x).OrderByDescending(g => g.Count()).First().Key;
        }

        public static (double min, double max) GetRange(IEnumerable<double> values)
        {
            var list = values.ToList();
            return (list.Min(), list.Max());
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

        public static int GreatestCommonDivisor(int a, int b)
        {
            while (b != 0)
            {
                int temp = b;
                b = a % b;
                a = temp;
            }
            return a;
        }

        public static int LeastCommonMultiple(int a, int b)
        {
            return (a * b) / GreatestCommonDivisor(a, b);
        }

        public static double Distance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }

        public static double Distance(double x1, double y1, double z1, double x2, double y2, double z2)
        {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2) + Math.Pow(z2 - z1, 2));
        }

        public static double ManhattanDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Abs(x2 - x1) + Math.Abs(y2 - y1);
        }

        public static double ChebyshevDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Max(Math.Abs(x2 - x1), Math.Abs(y2 - y1));
        }
    }

    public class ColorHelper
    {
        private static readonly Dictionary<string, string> NamedColors = new Dictionary<string, string>
        {
            { "Black", "#000000" },
            { "White", "#FFFFFF" },
            { "Red", "#FF0000" },
            { "Green", "#00FF00" },
            { "Blue", "#0000FF" },
            { "Yellow", "#FFFF00" },
            { "Cyan", "#00FFFF" },
            { "Magenta", "#FF00FF" },
            { "Gray", "#808080" },
            { "Silver", "#C0C0C0" },
            { "Maroon", "#800000" },
            { "Olive", "#808000" },
            { "Navy", "#000080" },
            { "Purple", "#800080" },
            { "Teal", "#008080" },
            { "Orange", "#FFA500" },
            { "Pink", "#FFC0CB" },
            { "Brown", "#A52A2A" },
            { "Gold", "#FFD700" },
            { "Silver", "#C0C0C0" },
            { "Lime", "#00FF00" },
            { "Aqua", "#00FFFF" },
            { "Azure", "#F0FFFF" },
            { "Beige", "#F5F5DC" },
            { "Coral", "#FF7F50" },
            { "Crimson", "#DC143C" },
            { "DarkBlue", "#00008B" },
            { "DarkGreen", "#006400" },
            { "DarkRed", "#8B0000" }
        };

        public static string GetNamedColor(string name)
        {
            return NamedColors.TryGetValue(name, out var color) ? color : null;
        }

        public static string GetColorName(string hexColor)
        {
            var entry = NamedColors.FirstOrDefault(x => x.Value.Equals(hexColor, StringComparison.OrdinalIgnoreCase));
            return entry.Key;
        }

        public static (byte r, byte g, byte b) FromHex(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length == 3)
                hex = string.Concat(hex[0], hex[0], hex[1], hex[1], hex[2], hex[2]);

            if (hex.Length != 6)
                return (0, 0, 0);

            return (
                Convert.ToByte(hex.Substring(0, 2), 16),
                Convert.ToByte(hex.Substring(2, 2), 16),
                Convert.ToByte(hex.Substring(4, 2), 16)
            );
        }

        public static string ToHex(byte r, byte g, byte b)
        {
            return $"#{r:X2}{g:X2}{b:X2}";
        }

        public static string ToHex(byte a, byte r, byte g, byte b)
        {
            return $"#{a:X2}{r:X2}{g:X2}{b:X2}";
        }

        public static (byte r, byte g, byte b) FromHsl(double h, double s, double l)
        {
            double r, g, b;

            if (s == 0)
            {
                r = g = b = l;
            }
            else
            {
                double q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                double p = 2 * l - q;
                r = HueToRgb(p, q, h + 1.0 / 3);
                g = HueToRgb(p, q, h);
                b = HueToRgb(p, q, h - 1.0 / 3);
            }

            return ((byte)(r * 255), (byte)(g * 255), (byte)(b * 255));
        }

        private static double HueToRgb(double p, double q, double t)
        {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 1.0 / 2) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }

        public static (double h, double s, double l) ToHsl(byte r, byte g, byte b)
        {
            double rd = r / 255.0;
            double gd = g / 255.0;
            double bd = b / 255.0;

            double max = Math.Max(rd, Math.Max(gd, bd));
            double min = Math.Min(rd, Math.Min(gd, bd));
            double h = 0, s = 0, l = (max + min) / 2;

            if (max != min)
            {
                double d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);

                if (max == rd)
                    h = (gd - bd) / d + (gd < bd ? 6 : 0);
                else if (max == gd)
                    h = (bd - rd) / d + 2;
                else
                    h = (rd - gd) / d + 4;

                h /= 6;
            }

            return (h, s, l);
        }

        public static byte Lighten(byte color, double amount)
        {
            var c = (double)color / 255;
            c = Math.Min(1, c + amount);
            return (byte)(c * 255);
        }

        public static byte Darken(byte color, double amount)
        {
            var c = (double)color / 255;
            c = Math.Max(0, c - amount);
            return (byte)(c * 255);
        }

        public static (byte r, byte g, byte b) Lighten((byte r, byte g, byte b) color, double amount)
        {
            return (Lighten(color.r, amount), Lighten(color.g, amount), Lighten(color.b, amount));
        }

        public static (byte r, byte g, byte b) Darken((byte r, byte g, byte b) color, double amount)
        {
            return (Darken(color.r, amount), Darken(color.g, amount), Darken(color.b, amount));
        }

        public static byte Interpolate(byte start, byte end, double t)
        {
            return (byte)(start + (end - start) * Math.Max(0, Math.Min(1, t)));
        }

        public static (byte r, byte g, byte b) Interpolate((byte r, byte g, byte b) start, (byte r, byte g, byte b) end, double t)
        {
            return (
                Interpolate(start.r, end.r, t),
                Interpolate(start.g, end.g, t),
                Interpolate(start.b, end.b, t)
            );
        }

        public static double GetBrightness(byte r, byte g, byte b)
        {
            return (0.299 * r + 0.587 * g + 0.114 * b) / 255;
        }

        public static bool IsLight(byte r, byte g, byte b)
        {
            return GetBrightness(r, g, b) > 0.5;
        }

        public static bool IsDark(byte r, byte g, byte b)
        {
            return !IsLight(r, g, b);
        }

        public static string GetContrastColor(string hexColor)
        {
            var (r, g, b) = FromHex(hexColor);
            return IsLight(r, g, b) ? "#000000" : "#FFFFFF";
        }

        public static string Blend(string color1, string color2, double ratio = 0.5)
        {
            var (r1, g1, b1) = FromHex(color1);
            var (r2, g2, b2) = FromHex(color2);

            return ToHex(
                Interpolate(r1, r2, ratio),
                Interpolate(g1, g2, ratio),
                Interpolate(b1, b2, ratio)
            );
        }
    }

    public class ValidationHelper
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

            return Uri.TryCreate(url, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
        }

        public static bool IsValidPhoneNumber(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone))
                return false;

            return Regex.IsMatch(phone, @"^\+?[\d\s\-\(\)]+$");
        }

        public static bool IsValidPostalCode(string postalCode)
        {
            if (string.IsNullOrWhiteSpace(postalCode))
                return false;

            return Regex.IsMatch(postalCode, @"^[A-Z0-9\s\-]{3,10}$", RegexOptions.IgnoreCase);
        }

        public static bool IsValidIPv4(string ip)
        {
            if (string.IsNullOrWhiteSpace(ip))
                return false;

            var parts = ip.Split('.');
            if (parts.Length != 4)
                return false;

            foreach (var part in parts)
            {
                if (!int.TryParse(part, out int num) || num < 0 || num > 255)
                    return false;
            }

            return true;
        }

        public static bool IsValidHexColor(string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
                return false;

            return Regex.IsMatch(hex, @"^#?([A-Fa-f0-9]{6}|[A-Fa-f0-9]{3})$");
        }

        public static bool IsValidGuid(string guid)
        {
            if (string.IsNullOrWhiteSpace(guid))
                return false;

            return Guid.TryParse(guid, out _);
        }

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            return Regex.IsMatch(username, @"^[a-zA-Z0-9_\-\.]{3,20}$");
        }

        public static bool IsValidPassword(string password, int minLength = 8)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < minLength)
                return false;

            return Regex.IsMatch(password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)[a-zA-Z\d@$!%*?&]{" + minLength + ",}$");
        }

        public static bool IsValidCreditCard(string cardNumber)
        {
            if (string.IsNullOrWhiteSpace(cardNumber))
                return false;

            cardNumber = Regex.Replace(cardNumber, @"\s", "");

            if (!Regex.IsMatch(cardNumber, @"^\d{13,19}$"))
                return false;

            int sum = 0;
            bool alternate = false;

            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                        n -= 9;
                }

                sum += n;
                alternate = !alternate;
            }

            return sum % 10 == 0;
        }

        public static bool IsValidDate(string date, string format = "MM/dd/yyyy")
        {
            if (string.IsNullOrWhiteSpace(date))
                return false;

            return DateTime.TryParseExact(date, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out _);
        }

        public static bool IsAlphabetic(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Regex.IsMatch(value, @"^[a-zA-Z]+$");
        }

        public static bool IsAlphanumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Regex.IsMatch(value, @"^[a-zA-Z0-9]+$");
        }

        public static bool IsNumeric(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return Regex.IsMatch(value, @"^\d+$");
        }

        public static bool ContainsOnly(string value, params char[] allowedChars)
        {
            if (string.IsNullOrWhiteSpace(value))
                return false;

            return value.All(c => allowedChars.Contains(c));
        }

        public static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static bool IsInRange(double value, double min, double max)
        {
            return value >= min && value <= max;
        }

        public static bool IsLengthInRange(string value, int minLength, int maxLength)
        {
            if (string.IsNullOrEmpty(value))
                return minLength == 0;

            return value.Length >= minLength && value.Length <= maxLength;
        }

        public static List<string> GetValidationErrors(string value, params string[] rules)
        {
            var errors = new List<string>();

            foreach (var rule in rules)
            {
                switch (rule.ToLower())
                {
                    case "required":
                        if (string.IsNullOrWhiteSpace(value))
                            errors.Add("Value is required");
                        break;
                    case "email":
                        if (!IsValidEmail(value))
                            errors.Add("Invalid email format");
                        break;
                    case "url":
                        if (!IsValidUrl(value))
                            errors.Add("Invalid URL format");
                        break;
                    case "phone":
                        if (!IsValidPhoneNumber(value))
                            errors.Add("Invalid phone number format");
                        break;
                    case "postalcode":
                        if (!IsValidPostalCode(value))
                            errors.Add("Invalid postal code format");
                        break;
                    case "ipv4":
                        if (!IsValidIPv4(value))
                            errors.Add("Invalid IPv4 address");
                        break;
                    case "hexcolor":
                        if (!IsValidHexColor(value))
                            errors.Add("Invalid hex color format");
                        break;
                    case "guid":
                        if (!IsValidGuid(value))
                            errors.Add("Invalid GUID format");
                        break;
                    case "alphanumeric":
                        if (!IsAlphanumeric(value))
                            errors.Add("Value must be alphanumeric");
                        break;
                    case "numeric":
                        if (!IsNumeric(value))
                            errors.Add("Value must be numeric");
                        break;
                }
            }

            return errors;
        }
    }

    public class CryptoHelper
    {
        private static readonly byte[] DefaultSalt = Encoding.UTF8.GetBytes("OfficeSuiteDefaultSalt");

        public static string HashPassword(string password)
        {
            return HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt());
        }

        public static string HashPassword(string password, string salt)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, salt);
        }

        public static bool VerifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public static string GenerateSalt()
        {
            return BCrypt.Net.BCrypt.GenerateSalt();
        }

        public static string ComputeMD5Hash(string input)
        {
            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ComputeSHA256Hash(string input)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string ComputeSHA512Hash(string input)
        {
            using (var sha512 = SHA512.Create())
            {
                var hashBytes = sha512.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }
        }

        public static string EncryptString(string plainText, string key)
        {
            using (var aes = Aes.Create())
            {
                var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
                aes.Key = keyBytes;
                aes.GenerateIV();

                using (var encryptor = aes.CreateEncryptor(aes.Key, aes.IV))
                using (var msEncrypt = new MemoryStream())
                {
                    msEncrypt.Write(aes.IV, 0, aes.IV.Length);
                    using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    using (var swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(plainText);
                    }

                    return Convert.ToBase64String(msEncrypt.ToArray());
                }
            }
        }

        public static string DecryptString(string cipherText, string key)
        {
            try
            {
                using (var aes = Aes.Create())
                {
                    var keyBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
                    aes.Key = keyBytes;

                    var fullCipher = Convert.FromBase64String(cipherText);
                    var iv = new byte[16];
                    var cipher = new byte[fullCipher.Length - iv.Length];

                    Array.Copy(fullCipher, 0, iv, 0, iv.Length);
                    Array.Copy(fullCipher, iv.Length, cipher, 0, cipher.Length);

                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor(aes.Key, aes.IV))
                    using (var msDecrypt = new MemoryStream(cipher))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch
            {
                return null;
            }
        }

        public static string GenerateRandomToken(int length = 32)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        public static string GenerateApiKey()
        {
            var timestamp = DateTime.UtcNow.Ticks.ToString("X");
            var random = GenerateRandomToken(16);
            return $"osk_{timestamp}_{random}";
        }

        public static bool ValidateApiKey(string apiKey)
        {
            if (string.IsNullOrWhiteSpace(apiKey))
                return false;

            return apiKey.StartsWith("osk_") && apiKey.Length >= 20;
        }
    }
}

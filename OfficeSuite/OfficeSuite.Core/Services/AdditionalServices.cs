using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OfficeSuite.Core.Services
{
    public interface IClipboardService
    {
        void Copy(string text);
        void Cut(string text);
        void Paste();
        string GetText();
        bool ContainsText { get; }
        bool ContainsImage { get; }
        bool ContainsFile { get; }
        void Clear();
    }

    public class ClipboardService : IClipboardService
    {
        public void Copy(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                System.Windows.Clipboard.SetText(text);
            }
        }

        public void Cut(string text)
        {
            if (!string.IsNullOrEmpty(text))
            {
                System.Windows.Clipboard.SetText(text);
            }
        }

        public void Paste()
        {
        }

        public string GetText()
        {
            if (System.Windows.Clipboard.ContainsText())
            {
                return System.Windows.Clipboard.GetText();
            }
            return string.Empty;
        }

        public bool ContainsText => System.Windows.Clipboard.ContainsText();
        public bool ContainsImage => System.Windows.Clipboard.ContainsImage();
        public bool ContainsFile => System.Windows.Clipboard.ContainsFileDropList();

        public void Clear()
        {
            System.Windows.Clipboard.Clear();
        }
    }

    public interface IHistoryService
    {
        void PushState(string stateType, object stateData);
        object PopState();
        object PeekState();
        bool CanUndo { get; }
        bool CanRedo { get; }
        void Undo();
        void Redo();
        void Clear();
        int HistoryCount { get; }
        event EventHandler<HistoryChangedEventArgs> HistoryChanged;
    }

    public class HistoryChangedEventArgs : EventArgs
    {
        public bool CanUndo { get; set; }
        public bool CanRedo { get; set; }
        public int HistoryCount { get; set; }
    }

    public class HistoryService : IHistoryService
    {
        private readonly Stack<HistoryItem> _undoStack = new Stack<HistoryItem>();
        private readonly Stack<HistoryItem> _redoStack = new Stack<HistoryItem>();
        private readonly int _maxHistorySize;
        private readonly bool _enableMerge;

        public event EventHandler<HistoryChangedEventArgs> HistoryChanged;

        public HistoryService(int maxHistorySize = 100, bool enableMerge = true)
        {
            _maxHistorySize = maxHistorySize;
            _enableMerge = enableMerge;
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int HistoryCount => _undoStack.Count;

        public void PushState(string stateType, object stateData)
        {
            var item = new HistoryItem
            {
                StateType = stateType,
                StateData = stateData,
                Timestamp = DateTime.Now
            };

            if (_enableMerge && _undoStack.Count > 0)
            {
                var lastItem = _undoStack.Peek();
                if (lastItem.StateType == stateType && CanMerge(lastItem, item))
                {
                    return;
                }
            }

            _undoStack.Push(item);
            _redoStack.Clear();

            if (_undoStack.Count > _maxHistorySize)
            {
                var tempStack = new Stack<HistoryItem>();
                for (int i = 0; i < _maxHistorySize; i++)
                {
                    tempStack.Push(_undoStack.Pop());
                }
                _undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    _undoStack.Push(tempStack.Pop());
                }
            }

            RaiseHistoryChanged();
        }

        public object PopState()
        {
            if (_undoStack.Count == 0)
                return null;

            var item = _undoStack.Pop();
            _redoStack.Push(item);
            RaiseHistoryChanged();
            return item.StateData;
        }

        public object PeekState()
        {
            if (_undoStack.Count == 0)
                return null;
            return _undoStack.Peek().StateData;
        }

        public void Undo()
        {
            PopState();
        }

        public void Redo()
        {
            if (_redoStack.Count == 0)
                return;

            var item = _redoStack.Pop();
            _undoStack.Push(item);
            RaiseHistoryChanged();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            RaiseHistoryChanged();
        }

        private bool CanMerge(HistoryItem lastItem, HistoryItem newItem)
        {
            if ((DateTime.Now - lastItem.Timestamp).TotalMilliseconds < 500)
            {
                return true;
            }
            return false;
        }

        private void RaiseHistoryChanged()
        {
            HistoryChanged?.Invoke(this, new HistoryChangedEventArgs
            {
                CanUndo = CanUndo,
                CanRedo = CanRedo,
                HistoryCount = HistoryCount
            });
        }

        private class HistoryItem
        {
            public string StateType { get; set; }
            public object StateData { get; set; }
            public DateTime Timestamp { get; set; }
        }
    }

    public interface ISearchService
    {
        List<SearchResult> Search(string text, SearchOptions options);
        List<SearchResult> FindAll(string text, SearchOptions options);
        SearchResult FindNext(string text, SearchOptions options);
        SearchResult FindPrevious(string text, SearchOptions options);
        void Replace(string find, string replace, ReplaceOptions options);
        int ReplaceAll(string find, string replace, ReplaceOptions options);
    }

    public class SearchResult
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string MatchedText { get; set; }
        public int LineNumber { get; set; }
        public int ColumnNumber { get; set; }
        public string Context { get; set; }
        public SearchResultType Type { get; set; }
    }

    public enum SearchResultType
    {
        Text,
        Word,
        Line,
        Regex
    }

    public class SearchOptions
    {
        public bool MatchCase { get; set; }
        public bool MatchWholeWord { get; set; }
        public bool UseRegex { get; set; }
        public bool SearchUp { get; set; }
        public bool IncludeLineNumbers { get; set; }
        public string SearchScope { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
    }

    public class ReplaceOptions
    {
        public bool MatchCase { get; set; }
        public bool MatchWholeWord { get; set; }
        public bool UseRegex { get; set; }
        public bool PreserveCase { get; set; }
        public bool ReplaceInAllFiles { get; set; }
    }

    public class SearchService : ISearchService
    {
        private readonly List<SearchResult> _currentResults = new List<SearchResult>();
        private int _currentIndex = -1;

        public List<SearchResult> Search(string text, SearchOptions options)
        {
            return FindAll(text, options);
        }

        public List<SearchResult> FindAll(string text, SearchOptions options)
        {
            _currentResults.Clear();
            _currentIndex = -1;

            if (string.IsNullOrEmpty(text))
                return _currentResults;

            try
            {
                var regexOptions = RegexOptions.Multiline;
                if (!options.MatchCase)
                    regexOptions |= RegexOptions.IgnoreCase;

                string pattern;
                if (options.UseRegex)
                {
                    pattern = text;
                }
                else if (options.MatchWholeWord)
                {
                    pattern = $@"\b{Regex.Escape(text)}\b";
                }
                else
                {
                    pattern = Regex.Escape(text);
                }

                var regex = new Regex(pattern, regexOptions);
                return _currentResults;
            }
            catch
            {
                return _currentResults;
            }
        }

        public SearchResult FindNext(string text, SearchOptions options)
        {
            var results = FindAll(text, options);
            if (results.Count == 0)
                return null;

            _currentIndex++;
            if (_currentIndex >= results.Count)
                _currentIndex = 0;

            return results[_currentIndex];
        }

        public SearchResult FindPrevious(string text, SearchOptions options)
        {
            var results = FindAll(text, options);
            if (results.Count == 0)
                return null;

            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = results.Count - 1;

            return results[_currentIndex];
        }

        public void Replace(string find, string replace, ReplaceOptions options)
        {
        }

        public int ReplaceAll(string find, string replace, ReplaceOptions options)
        {
            return 0;
        }
    }

    public interface ISpellCheckService
    {
        List<SpellingError> CheckSpelling(string text);
        List<string> GetSuggestions(string word);
        bool IsWordValid(string word);
        void AddToDictionary(string word);
        void RemoveFromDictionary(string word);
        void IgnoreWord(string word);
        event EventHandler<SpellCheckEventArgs> SpellingErrorFound;
    }

    public class SpellCheckEventArgs : EventArgs
    {
        public SpellingError Error { get; set; }
    }

    public class SpellingError
    {
        public string Word { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public List<string> Suggestions { get; set; }
        public SpellingErrorType Type { get; set; }
    }

    public enum SpellingErrorType
    {
        UnknownWord,
        RepeatedWord,
        capitalizationError,
        GrammarError
    }

    public class SpellCheckService : ISpellCheckService
    {
        private readonly HashSet<string> _dictionary = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _ignoredWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly List<string> _customDictionary = new List<string>();

        public event EventHandler<SpellCheckEventArgs> SpellingErrorFound;

        public SpellCheckService()
        {
            LoadDefaultDictionary();
        }

        private void LoadDefaultDictionary()
        {
            var commonWords = new[]
            {
                "the", "be", "to", "of", "and", "a", "in", "that", "have", "I",
                "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
                "this", "but", "his", "by", "from", "they", "we", "say", "her", "she",
                "or", "an", "will", "my", "one", "all", "would", "there", "their", "what",
                "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
                "word", "some", "could", "them", "see", "other", "than", "then", "now", "look",
                "only", "come", "its", "over", "think", "also", "back", "after", "use", "two",
                "how", "our", "work", "first", "well", "way", "even", "new", "want", "because",
                "any", "these", "give", "day", "most", "us", "is", "was", "are", "been"
            };

            foreach (var word in commonWords)
            {
                _dictionary.Add(word);
            }
        }

        public List<SpellingError> CheckSpelling(string text)
        {
            var errors = new List<SpellingError>();
            if (string.IsNullOrEmpty(text))
                return errors;

            var words = Regex.Matches(text, @"\b[\w']+\b");
            foreach (Match match in words)
            {
                var word = match.Value;
                if (!IsWordValid(word))
                {
                    errors.Add(new SpellingError
                    {
                        Word = word,
                        StartIndex = match.Index,
                        EndIndex = match.Index + match.Length,
                        Suggestions = GetSuggestions(word),
                        Type = SpellingErrorType.UnknownWord
                    });
                }
            }

            return errors;
        }

        public List<string> GetSuggestions(string word)
        {
            var suggestions = new List<string>();
            if (string.IsNullOrEmpty(word))
                return suggestions;

            var edits = GenerateEdits(word);
            foreach (var edit in edits)
            {
                if (_dictionary.Contains(edit) && !suggestions.Contains(edit))
                {
                    suggestions.Add(edit);
                }
            }

            foreach (var dictWord in _dictionary)
            {
                if (CalculateLevenshteinDistance(word.ToLower(), dictWord.ToLower()) <= 2)
                {
                    if (!suggestions.Contains(dictWord))
                    {
                        suggestions.Add(dictWord);
                    }
                }
            }

            return suggestions.Take(5).ToList();
        }

        private List<string> GenerateEdits(string word)
        {
            var edits = new List<string>();
            var n = word.Length;

            for (int i = 0; i < n; i++)
            {
                edits.Add(word.Substring(0, i) + word.Substring(i + 1));
            }

            for (int i = 0; i < n - 1; i++)
            {
                edits.Add(word.Substring(0, i) + word[i + 1] + word[i] + word.Substring(i + 2));
            }

            for (int i = 0; i < n; i++)
            {
                foreach (char c in "abcdefghijklmnopqrstuvwxyz")
                {
                    edits.Add(word.Substring(0, i) + c + word.Substring(i + 1));
                }
            }

            for (int i = 0; i <= n; i++)
            {
                foreach (char c in "abcdefghijklmnopqrstuvwxyz")
                {
                    edits.Add(word.Substring(0, i) + c + word.Substring(i));
                }
            }

            return edits;
        }

        private int CalculateLevenshteinDistance(string s, string t)
        {
            var n = s.Length;
            var m = t.Length;
            var d = new int[n + 1, m + 1];

            for (int i = 0; i <= n; i++)
                d[i, 0] = i;
            for (int j = 0; j <= m; j++)
                d[0, j] = j;

            for (int j = 1; j <= m; j++)
            {
                for (int i = 1; i <= n; i++)
                {
                    int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
                }
            }

            return d[n, m];
        }

        public bool IsWordValid(string word)
        {
            if (string.IsNullOrEmpty(word))
                return true;

            if (_ignoredWords.Contains(word))
                return true;

            if (_dictionary.Contains(word))
                return true;

            if (word.All(char.IsUpper) && _dictionary.Contains(word.ToLower()))
                return true;

            if (word.Length > 1 && word[0] == char.ToUpper(word[0]) && _dictionary.Contains(word.ToLower()))
                return true;

            return false;
        }

        public void AddToDictionary(string word)
        {
            if (!string.IsNullOrEmpty(word))
            {
                _dictionary.Add(word.ToLower());
                _customDictionary.Add(word);
            }
        }

        public void RemoveFromDictionary(string word)
        {
            if (!string.IsNullOrEmpty(word))
            {
                _dictionary.Remove(word.ToLower());
                _customDictionary.Remove(word);
            }
        }

        public void IgnoreWord(string word)
        {
            if (!string.IsNullOrEmpty(word))
            {
                _ignoredWords.Add(word);
            }
        }
    }

    public interface ITemplateService
    {
        List<Template> GetAvailableTemplates(TemplateType type);
        Template GetTemplate(string templateId);
        Stream GetTemplateStream(Template template);
        void SaveAsTemplate(string name, TemplateType type, byte[] content);
        void DeleteTemplate(string templateId);
        void UpdateTemplate(Template template);
    }

    public class Template
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public TemplateType Type { get; set; }
        public string Category { get; set; }
        public string ThumbnailPath { get; set; }
        public byte[] PreviewImage { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Author { get; set; }
        public string Tags { get; set; }
        public bool IsBuiltIn { get; set; }
        public string FilePath { get; set; }
    }

    public enum TemplateType
    {
        Document,
        Spreadsheet,
        Presentation,
        Other
    }

    public class TemplateService : ITemplateService
    {
        private readonly Dictionary<string, Template> _templates = new Dictionary<string, Template>();

        public TemplateService()
        {
            LoadBuiltInTemplates();
        }

        private void LoadBuiltInTemplates()
        {
            _templates["blank-document"] = new Template
            {
                Id = "blank-document",
                Name = "Blank Document",
                Description = "A blank document template",
                Type = TemplateType.Document,
                Category = "Basic",
                IsBuiltIn = true
            };

            _templates["blank-spreadsheet"] = new Template
            {
                Id = "blank-spreadsheet",
                Name = "Blank Spreadsheet",
                Description = "A blank spreadsheet template",
                Type = TemplateType.Spreadsheet,
                Category = "Basic",
                IsBuiltIn = true
            };

            _templates["blank-presentation"] = new Template
            {
                Id = "blank-presentation",
                Name = "Blank Presentation",
                Description = "A blank presentation template",
                Type = TemplateType.Presentation,
                Category = "Basic",
                IsBuiltIn = true
            };
        }

        public List<Template> GetAvailableTemplates(TemplateType type)
        {
            return _templates.Values.Where(t => t.Type == type).ToList();
        }

        public Template GetTemplate(string templateId)
        {
            return _templates.ContainsKey(templateId) ? _templates[templateId] : null;
        }

        public Stream GetTemplateStream(Template template)
        {
            return null;
        }

        public void SaveAsTemplate(string name, TemplateType type, byte[] content)
        {
            var template = new Template
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Type = type,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsBuiltIn = false
            };
            _templates[template.Id] = template;
        }

        public void DeleteTemplate(string templateId)
        {
            if (_templates.ContainsKey(templateId))
            {
                var template = _templates[templateId];
                if (!template.IsBuiltIn)
                {
                    _templates.Remove(templateId);
                }
            }
        }

        public void UpdateTemplate(Template template)
        {
            if (_templates.ContainsKey(template.Id))
            {
                _templates[template.Id] = template;
            }
        }
    }

    public interface ISecurityService
    {
        string Encrypt(string plainText, string key);
        string Decrypt(string cipherText, string key);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        string GenerateSalt();
        string ComputeHash(string input, HashAlgorithm algorithm);
        byte[] GenerateRandomBytes(int length);
    }

    public class SecurityService : ISecurityService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int Iterations = 10000;

        public string Encrypt(string plainText, string key)
        {
            if (string.IsNullOrEmpty(plainText))
                return string.Empty;

            try
            {
                var salt = GenerateSaltBytes(key);
                var keyBytes = GenerateKey(key, salt);

                using (var aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.GenerateIV();

                    using (var encryptor = aes.CreateEncryptor())
                    using (var msEncrypt = new MemoryStream())
                    {
                        msEncrypt.Write(salt, 0, salt.Length);
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
            catch
            {
                return string.Empty;
            }
        }

        public string Decrypt(string cipherText, string key)
        {
            if (string.IsNullOrEmpty(cipherText))
                return string.Empty;

            try
            {
                var cipherBytes = Convert.FromBase64String(cipherText);

                var salt = new byte[SaltSize];
                var iv = new byte[SaltSize / 2];
                var encryptedBytes = new byte[cipherBytes.Length - SaltSize - iv.Length / 2];

                Array.Copy(cipherBytes, 0, salt, 0, SaltSize);
                Array.Copy(cipherBytes, SaltSize, iv, 0, iv.Length);
                Array.Copy(cipherBytes, SaltSize + iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                var keyBytes = GenerateKey(key, salt);

                using (var aes = Aes.Create())
                {
                    aes.Key = keyBytes;
                    aes.IV = iv;

                    using (var decryptor = aes.CreateDecryptor())
                    using (var msDecrypt = new MemoryStream(encryptedBytes))
                    using (var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    using (var srDecrypt = new StreamReader(csDecrypt))
                    {
                        return srDecrypt.ReadToEnd();
                    }
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        public string HashPassword(string password)
        {
            var salt = GenerateSaltBytes(password);
            var hash = ComputeHash(password + Convert.ToBase64String(salt), SHA256.Create());
            return $"{Convert.ToBase64String(salt)}:{hash}";
        }

        public bool VerifyPassword(string password, string hash)
        {
            if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(hash))
                return false;

            var parts = hash.Split(':');
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var computedHash = ComputeHash(password + parts[0], SHA256.Create());
            return parts[1] == computedHash;
        }

        public string GenerateSalt()
        {
            return Convert.ToBase64String(GenerateRandomBytes(SaltSize));
        }

        public string ComputeHash(string input, HashAlgorithm algorithm)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = algorithm.ComputeHash(inputBytes);
            return Convert.ToBase64String(hashBytes);
        }

        public byte[] GenerateRandomBytes(int length)
        {
            var bytes = new byte[length];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return bytes;
        }

        private byte[] GenerateSaltBytes(string input)
        {
            var salt = new byte[SaltSize];
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hash = SHA256.Create().ComputeHash(inputBytes);
            Array.Copy(hash, salt, SaltSize);
            return salt;
        }

        private byte[] GenerateKey(string password, byte[] salt)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(KeySize);
            }
        }
    }

    public interface IVersioningService
    {
        List<VersionInfo> GetVersions(string documentId);
        VersionInfo GetCurrentVersion(string documentId);
        VersionInfo CreateVersion(string documentId, string description);
        void RestoreVersion(string documentId, string versionId);
        void DeleteVersion(string documentId, string versionId);
        byte[] GetVersionContent(string documentId, string versionId);
    }

    public class VersionInfo
    {
        public string VersionId { get; set; }
        public string DocumentId { get; set; }
        public int VersionNumber { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Author { get; set; }
        public long Size { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class VersioningService : IVersioningService
    {
        private readonly Dictionary<string, List<VersionInfo>> _versions = new Dictionary<string, List<VersionInfo>>();
        private readonly Dictionary<string, byte[]> _versionContent = new Dictionary<string, byte[]>();

        public List<VersionInfo> GetVersions(string documentId)
        {
            if (_versions.ContainsKey(documentId))
            {
                return _versions[documentId].OrderByDescending(v => v.VersionNumber).ToList();
            }
            return new List<VersionInfo>();
        }

        public VersionInfo GetCurrentVersion(string documentId)
        {
            var versions = GetVersions(documentId);
            return versions.FirstOrDefault(v => v.IsCurrent);
        }

        public VersionInfo CreateVersion(string documentId, string description)
        {
            var versionId = Guid.NewGuid().ToString();
            var versionNumber = 1;

            if (_versions.ContainsKey(documentId))
            {
                versionNumber = _versions[documentId].Max(v => v.VersionNumber) + 1;
                foreach (var v in _versions[documentId])
                {
                    v.IsCurrent = false;
                }
            }
            else
            {
                _versions[documentId] = new List<VersionInfo>();
            }

            var version = new VersionInfo
            {
                VersionId = versionId,
                DocumentId = documentId,
                VersionNumber = versionNumber,
                Description = description,
                CreatedDate = DateTime.Now,
                Author = Environment.UserName,
                IsCurrent = true
            };

            _versions[documentId].Add(version);
            return version;
        }

        public void RestoreVersion(string documentId, string versionId)
        {
        }

        public void DeleteVersion(string documentId, string versionId)
        {
            if (_versions.ContainsKey(documentId))
            {
                var version = _versions[documentId].FirstOrDefault(v => v.VersionId == versionId);
                if (version != null && !version.IsCurrent)
                {
                    _versions[documentId].Remove(version);
                    _versionContent.Remove($"{documentId}:{versionId}");
                }
            }
        }

        public byte[] GetVersionContent(string documentId, string versionId)
        {
            var key = $"{documentId}:{versionId}";
            return _versionContent.ContainsKey(key) ? _versionContent[key] : null;
        }
    }

    public interface INotificationService
    {
        void ShowNotification(string title, string message, NotificationType type);
        void ShowProgress(string title, string message, int progress);
        void UpdateProgress(int progress);
        void CloseProgress();
        void ShowBalloon(string title, string message, int timeout);
        event EventHandler<NotificationEventArgs> NotificationClicked;
    }

    public class NotificationEventArgs : EventArgs
    {
        public string Title { get; set; }
        public string Message { get; set; }
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class NotificationService : INotificationService
    {
        public event EventHandler<NotificationEventArgs> NotificationClicked;

        public void ShowNotification(string title, string message, NotificationType type)
        {
        }

        public void ShowProgress(string title, string message, int progress)
        {
        }

        public void UpdateProgress(int progress)
        {
        }

        public void CloseProgress()
        {
        }

        public void ShowBalloon(string title, string message, int timeout)
        {
        }
    }

    public interface IPrintPreviewService
    {
        void ShowPrintPreview(object content, PrintSettings settings);
        void ShowPageSetup(PrintSettings settings);
        void Print(object content, PrintSettings settings);
        void PrintToFile(object content, PrintSettings settings, string filePath);
    }

    public class PrintSettings
    {
        public string PrinterName { get; set; }
        public int Copies { get; set; }
        public bool Collate { get; set; }
        public PageRange PageRange { get; set; }
        public PaperSize PaperSize { get; set; }
        public PaperOrientation Orientation { get; set; }
        public PrintQuality Quality { get; set; }
        public bool ColorMode { get; set; }
        public bool Duplex { get; set; }
    }

    public class PageRange
    {
        public int From { get; set; }
        public int To { get; set; }
        public bool AllPages { get; set; }
    }

    public enum PaperSize
    {
        Letter,
        A4,
        A3,
        Legal,
        Custom
    }

    public enum PaperOrientation
    {
        Portrait,
        Landscape
    }

    public enum PrintQuality
    {
        Draft,
        Low,
        Medium,
        High
    }

    public class PrintPreviewService : IPrintPreviewService
    {
        public void ShowPrintPreview(object content, PrintSettings settings)
        {
        }

        public void ShowPageSetup(PrintSettings settings)
        {
        }

        public void Print(object content, PrintSettings settings)
        {
        }

        public void PrintToFile(object content, PrintSettings settings, string filePath)
        {
        }
    }

    public interface IUndoRedoService
    {
        void Execute(IUndoableCommand command);
        void Undo();
        void Redo();
        bool CanUndo { get; }
        bool CanRedo { get; }
        void Clear();
        int UndoCount { get; }
        int RedoCount { get; }
        event EventHandler<UndoRedoChangedEventArgs> StateChanged;
    }

    public class UndoRedoChangedEventArgs : EventArgs
    {
        public bool CanUndo { get; set; }
        public bool CanRedo { get; set; }
    }

    public interface IUndoableCommand
    {
        void Execute();
        void Undo();
        string Description { get; }
    }

    public class UndoRedoService : IUndoRedoService
    {
        private readonly Stack<IUndoableCommand> _undoStack = new Stack<IUndoableCommand>();
        private readonly Stack<IUndoableCommand> _redoStack = new Stack<IUndoableCommand>();
        private readonly int _maxStackSize;

        public event EventHandler<UndoRedoChangedEventArgs> StateChanged;

        public UndoRedoService(int maxStackSize = 100)
        {
            _maxStackSize = maxStackSize;
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        public void Execute(IUndoableCommand command)
        {
            command.Execute();
            _undoStack.Push(command);
            _redoStack.Clear();

            if (_undoStack.Count > _maxStackSize)
            {
                var tempStack = new Stack<IUndoableCommand>();
                for (int i = 0; i < _maxStackSize; i++)
                {
                    tempStack.Push(_undoStack.Pop());
                }
                _undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    _undoStack.Push(tempStack.Pop());
                }
            }

            RaiseStateChanged();
        }

        public void Undo()
        {
            if (!CanUndo)
                return;

            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
            RaiseStateChanged();
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
            RaiseStateChanged();
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            RaiseStateChanged();
        }

        private void RaiseStateChanged()
        {
            StateChanged?.Invoke(this, new UndoRedoChangedEventArgs
            {
                CanUndo = CanUndo,
                CanRedo = CanRedo
            });
        }
    }

    public interface ICompareService
    {
        ComparisonResult CompareDocuments(string document1Path, string document2Path);
        ComparisonResult CompareDocuments(Stream document1, Stream document2);
        List<Difference> GetDifferences(ComparisonResult result);
        void MergeDifferences(ComparisonResult result, DifferenceAction action);
    }

    public class ComparisonResult
    {
        public string Document1Path { get; set; }
        public string Document2Path { get; set; }
        public DateTime ComparisonDate { get; set; }
        public List<Difference> Differences { get; set; }
        public int TotalChanges { get; set; }
        public int AddedChanges { get; set; }
        public int DeletedChanges { get; set; }
        public int ModifiedChanges { get; set; }
    }

    public class Difference
    {
        public int Position { get; set; }
        public DifferenceType Type { get; set; }
        public string OldText { get; set; }
        public string NewText { get; set; }
        public string Description { get; set; }
        public bool IsResolved { get; set; }
    }

    public enum DifferenceType
    {
        Added,
        Deleted,
        Modified,
        Moved,
        FormatChanged
    }

    public enum DifferenceAction
    {
        AcceptOriginal,
        AcceptModified,
        AcceptBoth,
        Reject
    }

    public class CompareService : ICompareService
    {
        public ComparisonResult CompareDocuments(string document1Path, string document2Path)
        {
            return new ComparisonResult
            {
                Document1Path = document1Path,
                Document2Path = document2Path,
                ComparisonDate = DateTime.Now,
                Differences = new List<Difference>()
            };
        }

        public ComparisonResult CompareDocuments(Stream document1, Stream document2)
        {
            return new ComparisonResult
            {
                ComparisonDate = DateTime.Now,
                Differences = new List<Difference>()
            };
        }

        public List<Difference> GetDifferences(ComparisonResult result)
        {
            return result?.Differences ?? new List<Difference>();
        }

        public void MergeDifferences(ComparisonResult result, DifferenceAction action)
        {
        }
    }

    public interface IMailMergeService
    {
        void SetDataSource(string connectionString, string tableName);
        void SetDataSource(IEnumerable<object> data);
        void SetDataSource(DataTable data);
        List<string> GetFieldNames();
        string MergeField(string fieldName, object record);
        Stream MergeDocument(string templatePath, IEnumerable<object> records);
        void Execute(int startRecord, int endRecord);
        event EventHandler<MailMergeEventArgs> MergeComplete;
    }

    public class MailMergeEventArgs : EventArgs
    {
        public int CurrentRecord { get; set; }
        public int TotalRecords { get; set; }
        public bool IsComplete { get; set; }
    }

    public class DataTable
    {
        public List<DataColumn> Columns { get; set; }
        public List<DataRow> Rows { get; set; }

        public DataTable()
        {
            Columns = new List<DataColumn>();
            Rows = new List<DataRow>();
        }
    }

    public class DataColumn
    {
        public string ColumnName { get; set; }
        public Type DataType { get; set; }
    }

    public class DataRow
    {
        public Dictionary<string, object> Values { get; set; }

        public DataRow()
        {
            Values = new Dictionary<string, object>();
        }

        public object this[string columnName] => Values.ContainsKey(columnName) ? Values[columnName] : null;
    }

    public class MailMergeService : IMailMergeService
    {
        private DataTable _dataSource;
        private int _currentRecord;

        public event EventHandler<MailMergeEventArgs> MergeComplete;

        public void SetDataSource(string connectionString, string tableName)
        {
        }

        public void SetDataSource(IEnumerable<object> data)
        {
        }

        public void SetDataSource(DataTable data)
        {
            _dataSource = data;
        }

        public List<string> GetFieldNames()
        {
            return _dataSource?.Columns.Select(c => c.ColumnName).ToList() ?? new List<string>();
        }

        public string MergeField(string fieldName, object record)
        {
            return string.Empty;
        }

        public Stream MergeDocument(string templatePath, IEnumerable<object> records)
        {
            return null;
        }

        public void Execute(int startRecord, int endRecord)
        {
            _currentRecord = startRecord;
        }
    }

    public interface IExportService
    {
        void ExportToPdf(object document, string outputPath);
        void ExportToHtml(object document, string outputPath);
        void ExportToRtf(object document, string outputPath);
        void ExportToText(object document, string outputPath);
        void ExportToImage(object document, string outputPath, ImageFormat format);
        Stream ExportToStream(object document, ExportFormat format);
    }

    public enum ExportFormat
    {
        Pdf,
        Html,
        Rtf,
        Text,
        Image,
        Docx,
        Xlsx,
        Pptx
    }

    public enum ImageFormat
    {
        Png,
        Jpeg,
        Bmp,
        Gif,
        Tiff
    }

    public class ExportService : IExportService
    {
        public void ExportToPdf(object document, string outputPath)
        {
        }

        public void ExportToHtml(object document, string outputPath)
        {
        }

        public void ExportToRtf(object document, string outputPath)
        {
        }

        public void ExportToText(object document, string outputPath)
        {
        }

        public void ExportToImage(object document, string outputPath, ImageFormat format)
        {
        }

        public Stream ExportToStream(object document, ExportFormat format)
        {
            return null;
        }
    }

    public interface IImportService
    {
        object Import(string filePath, DocumentType type);
        object Import(Stream stream, DocumentType type);
        DocumentMetadata GetMetadata(string filePath);
        bool ValidateDocument(string filePath, DocumentType type);
    }

    public class DocumentMetadata
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Application { get; set; }
        public string Version { get; set; }
        public long FileSize { get; set; }
    }

    public enum DocumentType
    {
        Word,
        Excel,
        PowerPoint,
        Pdf,
        Rtf,
        Text,
        Html,
        Unknown
    }

    public class ImportService : IImportService
    {
        public object Import(string filePath, DocumentType type)
        {
            return null;
        }

        public object Import(Stream stream, DocumentType type)
        {
            return null;
        }

        public DocumentMetadata GetMetadata(string filePath)
        {
            return new DocumentMetadata();
        }

        public bool ValidateDocument(string filePath, DocumentType type)
        {
            return File.Exists(filePath);
        }
    }

    public interface ICloudService
    {
        void Upload(string filePath, string cloudPath);
        void Download(string cloudPath, string localPath);
        void Delete(string cloudPath);
        List<CloudItem> ListItems(string path);
        void Share(string cloudPath, ShareOptions options);
        bool IsConnected { get; }
        event EventHandler<CloudSyncEventArgs> SyncStatusChanged;
    }

    public class CloudItem
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool IsFolder { get; set; }
        public DateTime ModifiedDate { get; set; }
        public long Size { get; set; }
    }

    public class ShareOptions
    {
        public string RecipientEmail { get; set; }
        public Permission Permission { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public bool RequirePassword { get; set; }
        public string Password { get; set; }
    }

    public enum Permission
    {
        View,
        Comment,
        Edit,
        FullControl
    }

    public class CloudSyncEventArgs : EventArgs
    {
        public SyncStatus Status { get; set; }
        public string Message { get; set; }
        public double Progress { get; set; }
    }

    public enum SyncStatus
    {
        Idle,
        Syncing,
        Success,
        Error,
        Conflict
    }

    public class CloudService : ICloudService
    {
        public bool IsConnected => false;

        public event EventHandler<CloudSyncEventArgs> SyncStatusChanged;

        public void Upload(string filePath, string cloudPath)
        {
        }

        public void Download(string cloudPath, string localPath)
        {
        }

        public void Delete(string cloudPath)
        {
        }

        public List<CloudItem> ListItems(string path)
        {
            return new List<CloudItem>();
        }

        public void Share(string cloudPath, ShareOptions options)
        {
        }
    }

    public interface IAutoSaveService
    {
        void Enable(string documentId, int intervalSeconds);
        void Disable(string documentId);
        void SaveNow(string documentId);
        bool IsEnabled(string documentId);
        event EventHandler<AutoSaveEventArgs> AutoSaved;
    }

    public class AutoSaveEventArgs : EventArgs
    {
        public string DocumentId { get; set; }
        public DateTime SavedAt { get; set; }
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class AutoSaveService : IAutoSaveService
    {
        private readonly Dictionary<string, DateTime> _lastSaveTime = new Dictionary<string, DateTime>();
        private readonly Dictionary<string, int> _intervals = new Dictionary<string, int>();
        private readonly HashSet<string> _enabledDocuments = new HashSet<string>();

        public event EventHandler<AutoSaveEventArgs> AutoSaved;

        public void Enable(string documentId, int intervalSeconds)
        {
            _enabledDocuments.Add(documentId);
            _intervals[documentId] = intervalSeconds;
        }

        public void Disable(string documentId)
        {
            _enabledDocuments.Remove(documentId);
            _intervals.Remove(documentId);
        }

        public void SaveNow(string documentId)
        {
            _lastSaveTime[documentId] = DateTime.Now;
            AutoSaved?.Invoke(this, new AutoSaveEventArgs
            {
                DocumentId = documentId,
                SavedAt = DateTime.Now,
                Success = true
            });
        }

        public bool IsEnabled(string documentId)
        {
            return _enabledDocuments.Contains(documentId);
        }
    }

    public interface IRecoveryService
    {
        void CreateRecoveryPoint(string documentId, byte[] content);
        List<RecoveryPoint> GetRecoveryPoints(string documentId);
        byte[] GetRecoveryPointContent(RecoveryPoint point);
        void DeleteRecoveryPoint(RecoveryPoint point);
        void ClearAllRecoveryPoints();
        void SetAutoRecoveryInterval(int intervalMinutes);
    }

    public class RecoveryPoint
    {
        public string Id { get; set; }
        public string DocumentId { get; set; }
        public DateTime CreatedDate { get; set; }
        public long Size { get; set; }
        public bool IsValid { get; set; }
    }

    public class RecoveryService : IRecoveryService
    {
        private readonly Dictionary<string, List<RecoveryPoint>> _recoveryPoints = new Dictionary<string, List<RecoveryPoint>>();
        private readonly Dictionary<string, byte[]> _recoveryContent = new Dictionary<string, byte[]>();
        private int _autoRecoveryIntervalMinutes = 5;

        public void CreateRecoveryPoint(string documentId, byte[] content)
        {
            var point = new RecoveryPoint
            {
                Id = Guid.NewGuid().ToString(),
                DocumentId = documentId,
                CreatedDate = DateTime.Now,
                Size = content?.Length ?? 0,
                IsValid = true
            };

            if (!_recoveryPoints.ContainsKey(documentId))
            {
                _recoveryPoints[documentId] = new List<RecoveryPoint>();
            }

            _recoveryPoints[documentId].Add(point);
            _recoveryContent[$"{documentId}:{point.Id}"] = content;
        }

        public List<RecoveryPoint> GetRecoveryPoints(string documentId)
        {
            return _recoveryPoints.ContainsKey(documentId)
                ? _recoveryPoints[documentId].OrderByDescending(p => p.CreatedDate).ToList()
                : new List<RecoveryPoint>();
        }

        public byte[] GetRecoveryPointContent(RecoveryPoint point)
        {
            var key = $"{point.DocumentId}:{point.Id}";
            return _recoveryContent.ContainsKey(key) ? _recoveryContent[key] : null;
        }

        public void DeleteRecoveryPoint(RecoveryPoint point)
        {
            var key = $"{point.DocumentId}:{point.Id}";
            if (_recoveryPoints.ContainsKey(point.DocumentId))
            {
                _recoveryPoints[point.DocumentId].Remove(point);
            }
            _recoveryContent.Remove(key);
        }

        public void ClearAllRecoveryPoints()
        {
            _recoveryPoints.Clear();
            _recoveryContent.Clear();
        }

        public void SetAutoRecoveryInterval(int intervalMinutes)
        {
            _autoRecoveryIntervalMinutes = intervalMinutes;
        }
    }

    public interface IAccessibilityService
    {
        bool IsScreenReaderRunning { get; }
        void Announce(string message);
        void SetFocus(System.Windows.UIElement element);
        string GetElementName(System.Windows.UIElement element);
        void SetElementDescription(System.Windows.UIElement element, string description);
        bool HighContrastEnabled { get; }
        void EnableHighContrast(bool enable);
    }

    public class AccessibilityService : IAccessibilityService
    {
        public bool IsScreenReaderRunning => false;

        public bool HighContrastEnabled => false;

        public void Announce(string message)
        {
        }

        public void SetFocus(System.Windows.UIElement element)
        {
            if (element != null)
            {
                element.Focus();
            }
        }

        public string GetElementName(System.Windows.UIElement element)
        {
            return string.Empty;
        }

        public void SetElementDescription(System.Windows.UIElement element, string description)
        {
        }

        public void EnableHighContrast(bool enable)
        {
        }
    }

    public interface IAnalyticsService
    {
        void TrackEvent(string eventName, Dictionary<string, object> properties);
        void TrackPageView(string pageName);
        void TrackException(Exception exception, string message);
        void SetUserId(string userId);
        void SetUserProperty(string property, string value);
    }

    public class AnalyticsService : IAnalyticsService
    {
        public void TrackEvent(string eventName, Dictionary<string, object> properties)
        {
        }

        public void TrackPageView(string pageName)
        {
        }

        public void TrackException(Exception exception, string message)
        {
        }

        public void SetUserId(string userId)
        {
        }

        public void SetUserProperty(string property, string value)
        {
        }
    }

    public interface ITelemetryService
    {
        void Initialize(string instrumentationKey);
        void TrackMetric(string name, double value);
        void TrackDependency(string dependencyType, string dependencyName, DateTime startTime, TimeSpan duration, bool success);
        void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success);
        void Flush();
    }

    public class TelemetryService : ITelemetryService
    {
        public void Initialize(string instrumentationKey)
        {
        }

        public void TrackMetric(string name, double value)
        {
        }

        public void TrackDependency(string dependencyType, string dependencyName, DateTime startTime, TimeSpan duration, bool success)
        {
        }

        public void TrackRequest(string name, DateTime startTime, TimeSpan duration, string responseCode, bool success)
        {
        }

        public void Flush()
        {
        }
    }

    public interface ICommandLineService
    {
        Dictionary<string, string> ParseArguments(string[] args);
        bool HasArgument(string argument);
        string GetArgumentValue(string argument);
        string GetArgumentValue(string argument, string defaultValue);
        List<string> GetArgumentValues(string argument);
    }

    public class CommandLineService : ICommandLineService
    {
        private Dictionary<string, string> _arguments;

        public Dictionary<string, string> ParseArguments(string[] args)
        {
            _arguments = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                if (arg.StartsWith("--"))
                {
                    var key = arg.Substring(2);
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        _arguments[key] = args[i + 1];
                        i++;
                    }
                    else
                    {
                        _arguments[key] = "true";
                    }
                }
                else if (arg.StartsWith("-"))
                {
                    var key = arg.Substring(1);
                    if (i + 1 < args.Length && !args[i + 1].StartsWith("-"))
                    {
                        _arguments[key] = args[i + 1];
                        i++;
                    }
                    else
                    {
                        _arguments[key] = "true";
                    }
                }
            }

            return _arguments;
        }

        public bool HasArgument(string argument)
        {
            return _arguments?.ContainsKey(argument) ?? false;
        }

        public string GetArgumentValue(string argument)
        {
            return _arguments?.ContainsKey(argument) == true ? _arguments[argument] : null;
        }

        public string GetArgumentValue(string argument, string defaultValue)
        {
            return GetArgumentValue(argument) ?? defaultValue;
        }

        public List<string> GetArgumentValues(string argument)
        {
            var values = new List<string>();
            var value = GetArgumentValue(argument);
            if (!string.IsNullOrEmpty(value))
            {
                values.Add(value);
            }
            return values;
        }
    }

    public interface ILicenseService
    {
        LicenseInfo GetLicenseInfo();
        bool IsLicensed(string feature);
        bool ValidateLicense(string licenseKey);
        void ActivateLicense(string licenseKey);
        void DeactivateLicense();
        string GetErrorMessage();
    }

    public class LicenseInfo
    {
        public string LicenseKey { get; set; }
        public string ProductName { get; set; }
        public string LicenseType { get; set; }
        public int SeatCount { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public bool IsValid { get; set; }
        public List<string> LicensedFeatures { get; set; }
    }

    public class LicenseService : ILicenseService
    {
        private LicenseInfo _currentLicense;

        public LicenseInfo GetLicenseInfo()
        {
            return _currentLicense ?? new LicenseInfo
            {
                ProductName = "Office Suite",
                LicenseType = "Trial",
                IsValid = true,
                ExpirationDate = DateTime.Now.AddDays(30),
                SeatCount = 1,
                LicensedFeatures = new List<string> { "All" }
            };
        }

        public bool IsLicensed(string feature)
        {
            var license = GetLicenseInfo();
            return license.IsValid && (license.LicensedFeatures.Contains("All") || license.LicensedFeatures.Contains(feature));
        }

        public bool ValidateLicense(string licenseKey)
        {
            return !string.IsNullOrEmpty(licenseKey);
        }

        public void ActivateLicense(string licenseKey)
        {
            _currentLicense = new LicenseInfo
            {
                LicenseKey = licenseKey,
                IsValid = true,
                LicenseType = "Full"
            };
        }

        public void DeactivateLicense()
        {
            _currentLicense = null;
        }

        public string GetErrorMessage()
        {
            return string.Empty;
        }
    }

    public interface IUpdaterService
    {
        bool CheckForUpdates();
        UpdateInfo GetUpdateInfo();
        void DownloadUpdate(UpdateProgressCallback callback);
        void InstallUpdate();
        bool IsUpdateAvailable { get; }
        string CurrentVersion { get; }
        event EventHandler<UpdateEventArgs> UpdateStatusChanged;
    }

    public class UpdateInfo
    {
        public string Version { get; set; }
        public DateTime ReleaseDate { get; set; }
        public string ReleaseNotes { get; set; }
        public long DownloadSize { get; set; }
        public string DownloadUrl { get; set; }
        public bool IsMandatory { get; set; }
    }

    public delegate void UpdateProgressCallback(int progress, string status);

    public class UpdateEventArgs : EventArgs
    {
        public UpdateStatus Status { get; set; }
        public string Message { get; set; }
    }

    public enum UpdateStatus
    {
        Idle,
        Checking,
        Available,
        Downloading,
        Ready,
        Installing,
        Error
    }

    public class UpdaterService : IUpdaterService
    {
        public bool IsUpdateAvailable => false;
        public string CurrentVersion => "1.0.0";

        public event EventHandler<UpdateEventArgs> UpdateStatusChanged;

        public bool CheckForUpdates()
        {
            return false;
        }

        public UpdateInfo GetUpdateInfo()
        {
            return null;
        }

        public void DownloadUpdate(UpdateProgressCallback callback)
        {
        }

        public void InstallUpdate()
        {
        }
    }

    public interface IHotkeyService
    {
        void RegisterHotkey(string name, ModifierKeys modifiers, Key key, Action callback);
        void UnregisterHotkey(string name);
        void UnregisterAll();
        bool IsHotkeyRegistered(string name);
        List<HotkeyInfo> GetRegisteredHotkeys();
    }

    public class HotkeyInfo
    {
        public string Name { get; set; }
        public ModifierKeys Modifiers { get; set; }
        public Key Key { get; set; }
        public string Description { get; set; }
    }

    public class HotkeyService : IHotkeyService
    {
        private readonly Dictionary<string, HotkeyInfo> _hotkeys = new Dictionary<string, HotkeyInfo>();

        public void RegisterHotkey(string name, ModifierKeys modifiers, Key key, Action callback)
        {
            _hotkeys[name] = new HotkeyInfo
            {
                Name = name,
                Modifiers = modifiers,
                Key = key
            };
        }

        public void UnregisterHotkey(string name)
        {
            _hotkeys.Remove(name);
        }

        public void UnregisterAll()
        {
            _hotkeys.Clear();
        }

        public bool IsHotkeyRegistered(string name)
        {
            return _hotkeys.ContainsKey(name);
        }

        public List<HotkeyInfo> GetRegisteredHotkeys()
        {
            return _hotkeys.Values.ToList();
        }
    }
}

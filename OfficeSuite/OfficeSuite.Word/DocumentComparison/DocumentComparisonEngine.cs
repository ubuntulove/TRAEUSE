using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeSuite.Word.DocumentComparison
{
    public class DocumentComparisonEngine
    {
        private DocumentComparisonSettings _settings;
        private List<ComparisonDifference> _differences;
        private DocumentNode _originalTree;
        private DocumentNode _revisedTree;

        public DocumentComparisonEngine()
        {
            _settings = new DocumentComparisonSettings();
            _differences = new List<ComparisonDifference>();
        }

        public DocumentComparisonSettings Settings
        {
            get => _settings;
            set => _settings = value ?? new DocumentComparisonSettings();
        }

        public List<ComparisonDifference> Differences => _differences;

        public ComparisonResult Compare(string originalContent, string revisedContent)
        {
            var result = new ComparisonResult
            {
                StartTime = DateTime.Now
            };

            _differences.Clear();

            if (_settings.CompareUsing == CompareMethod.WordLevel)
            {
                CompareWordLevel(originalContent, revisedContent);
            }
            else if (_settings.CompareUsing == CompareMethod.CharLevel)
            {
                CompareCharLevel(originalContent, revisedContent);
            }
            else
            {
                CompareLineLevel(originalContent, revisedContent);
            }

            result.OriginalContent = originalContent;
            result.RevisedContent = revisedContent;
            result.Differences = _differences;
            result.DifferenceCount = _differences.Count;
            result.AddedCount = _differences.Count(d => d.Type == DifferenceType.Added);
            result.DeletedCount = _differences.Count(d => d.Type == DifferenceType.Deleted);
            result.ModifiedCount = _differences.Count(d => d.Type == DifferenceType.Modified);
            result.MovedCount = _differences.Count(d => d.Type == DifferenceType.Moved);
            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;

            GenerateSummary(result);

            return result;
        }

        private void CompareWordLevel(string original, string revised)
        {
            var originalWords = Tokenize(original);
            var revisedWords = Tokenize(revised);

            var lcs = LongestCommonSubsequence(originalWords, revisedWords);
            var lcsIndex = 0;
            var originalIndex = 0;
            var revisedIndex = 0;

            while (originalIndex < originalWords.Count || revisedIndex < revisedWords.Count)
            {
                if (lcsIndex < lcs.Count)
                {
                    while (originalIndex < originalWords.Count && originalWords[originalIndex] != lcs[lcsIndex])
                    {
                        AddDifference(DifferenceType.Deleted, originalWords[originalIndex], originalIndex);
                        originalIndex++;
                    }

                    while (revisedIndex < revisedWords.Count && revisedWords[revisedIndex] != lcs[lcsIndex])
                    {
                        AddDifference(DifferenceType.Added, revisedWords[revisedIndex], revisedIndex);
                        revisedIndex++;
                    }

                    if (originalIndex < originalWords.Count && revisedIndex < revisedWords.Count)
                    {
                        lcsIndex++;
                        originalIndex++;
                        revisedIndex++;
                    }
                }
                else
                {
                    while (originalIndex < originalWords.Count)
                    {
                        AddDifference(DifferenceType.Deleted, originalWords[originalIndex], originalIndex);
                        originalIndex++;
                    }

                    while (revisedIndex < revisedWords.Count)
                    {
                        AddDifference(DifferenceType.Added, revisedWords[revisedIndex], revisedIndex);
                        revisedIndex++;
                    }
                }
            }
        }

        private void CompareCharLevel(string original, string revised)
        {
            var originalChars = original.ToCharArray().ToList();
            var revisedChars = revised.ToCharArray().ToList();

            var lcs = LongestCommonSubsequenceChars(originalChars, revisedChars);
            var lcsIndex = 0;
            var originalIndex = 0;
            var revisedIndex = 0;

            while (originalIndex < originalChars.Count || revisedIndex < revisedChars.Count)
            {
                if (lcsIndex < lcs.Count)
                {
                    while (originalIndex < originalChars.Count && originalChars[originalIndex] != lcs[lcsIndex])
                    {
                        AddDifference(DifferenceType.Deleted, originalChars[originalIndex].ToString(), originalIndex);
                        originalIndex++;
                    }

                    while (revisedIndex < revisedChars.Count && revisedChars[revisedIndex] != lcs[lcsIndex])
                    {
                        AddDifference(DifferenceType.Added, revisedChars[revisedIndex].ToString(), revisedIndex);
                        revisedIndex++;
                    }

                    if (originalIndex < originalChars.Count && revisedIndex < revisedChars.Count)
                    {
                        lcsIndex++;
                        originalIndex++;
                        revisedIndex++;
                    }
                }
                else
                {
                    while (originalIndex < originalChars.Count)
                    {
                        AddDifference(DifferenceType.Deleted, originalChars[originalIndex].ToString(), originalIndex);
                        originalIndex++;
                    }

                    while (revisedIndex < revisedChars.Count)
                    {
                        AddDifference(DifferenceType.Added, revisedChars[revisedIndex].ToString(), revisedIndex);
                        revisedIndex++;
                    }
                }
            }
        }

        private void CompareLineLevel(string original, string revised)
        {
            var originalLines = original.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();
            var revisedLines = revised.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None).ToList();

            var lcs = LongestCommonSubsequenceLines(originalLines, revisedLines);
            var lcsIndex = 0;
            var originalIndex = 0;
            var revisedIndex = 0;

            while (originalIndex < originalLines.Count || revisedIndex < revisedLines.Count)
            {
                if (lcsIndex < lcs.Count)
                {
                    while (originalIndex < originalLines.Count && originalLines[originalIndex] != lcs[lcsIndex])
                    {
                        AddDifference(DifferenceType.Deleted, originalLines[originalIndex], originalIndex);
                        originalIndex++;
                    }

                    while (revisedIndex < revisedLines.Count && revisedLines[revisedIndex] != lcs[lcsIndex])
                    {
                        AddDifference(DifferenceType.Added, revisedLines[revisedIndex], revisedIndex);
                        revisedIndex++;
                    }

                    if (originalIndex < originalLines.Count && revisedIndex < revisedLines.Count)
                    {
                        lcsIndex++;
                        originalIndex++;
                        revisedIndex++;
                    }
                }
                else
                {
                    while (originalIndex < originalLines.Count)
                    {
                        AddDifference(DifferenceType.Deleted, originalLines[originalIndex], originalIndex);
                        originalIndex++;
                    }

                    while (revisedIndex < revisedLines.Count)
                    {
                        AddDifference(DifferenceType.Added, revisedLines[revisedIndex], revisedIndex);
                        revisedIndex++;
                    }
                }
            }

            DetectMoves();
        }

        private List<string> Tokenize(string text)
        {
            var tokens = new List<string>();
            var currentToken = new StringBuilder();
            bool inWord = false;

            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c) || c == '_')
                {
                    currentToken.Append(c);
                    inWord = true;
                }
                else
                {
                    if (inWord)
                    {
                        tokens.Add(currentToken.ToString());
                        currentToken.Clear();
                        inWord = false;
                    }

                    if (!char.IsWhiteSpace(c))
                    {
                        tokens.Add(c.ToString());
                    }
                }
            }

            if (inWord)
            {
                tokens.Add(currentToken.ToString());
            }

            return tokens;
        }

        private List<string> LongestCommonSubsequence(List<string> a, List<string> b)
        {
            int m = a.Count;
            int n = b.Count;
            var dp = new int[m + 1, n + 1];

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (a[i - 1] == b[j - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            var result = new List<string>();
            int x = m, y = n;
            while (x > 0 && y > 0)
            {
                if (a[x - 1] == b[y - 1])
                {
                    result.Insert(0, a[x - 1]);
                    x--;
                    y--;
                }
                else if (dp[x - 1, y] > dp[x, y - 1])
                {
                    x--;
                }
                else
                {
                    y--;
                }
            }

            return result;
        }

        private List<char> LongestCommonSubsequenceChars(List<char> a, List<char> b)
        {
            int m = a.Count;
            int n = b.Count;
            var dp = new int[m + 1, n + 1];

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (a[i - 1] == b[j - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            var result = new List<char>();
            int x = m, y = n;
            while (x > 0 && y > 0)
            {
                if (a[x - 1] == b[y - 1])
                {
                    result.Insert(0, a[x - 1]);
                    x--;
                    y--;
                }
                else if (dp[x - 1, y] > dp[x, y - 1])
                {
                    x--;
                }
                else
                {
                    y--;
                }
            }

            return result;
        }

        private List<string> LongestCommonSubsequenceLines(List<string> a, List<string> b)
        {
            int m = a.Count;
            int n = b.Count;
            var dp = new int[m + 1, n + 1];

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (a[i - 1] == b[j - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            var result = new List<string>();
            int x = m, y = n;
            while (x > 0 && y > 0)
            {
                if (a[x - 1] == b[y - 1])
                {
                    result.Insert(0, a[x - 1]);
                    x--;
                    y--;
                }
                else if (dp[x - 1, y] > dp[x, y - 1])
                {
                    x--;
                }
                else
                {
                    y--;
                }
            }

            return result;
        }

        private void AddDifference(DifferenceType type, string content, int position)
        {
            if (_settings.IgnoreFormatting && IsFormattingOnly(content))
                return;

            if (_settings.IgnoreCase)
            {
                content = content.ToLowerInvariant();
            }

            _differences.Add(new ComparisonDifference
            {
                Id = Guid.NewGuid().ToString(),
                Type = type,
                Content = content,
                Position = position,
                CreatedAt = DateTime.Now
            });
        }

        private bool IsFormattingOnly(string content)
        {
            return string.IsNullOrWhiteSpace(content) || content.All(c => char.IsWhiteSpace(c));
        }

        private void DetectMoves()
        {
            var additions = _differences.Where(d => d.Type == DifferenceType.Added).ToList();
            var deletions = _differences.Where(d => d.Type == DifferenceType.Deleted).ToList();

            foreach (var addition in additions)
            {
                var match = deletions.FirstOrDefault(d => 
                    AreSimilar(d.Content, addition.Content) && !d.IsMarkedAsMoved);

                if (match != null)
                {
                    addition.Type = DifferenceType.Moved;
                    addition.MovedFrom = match.Position;
                    match.IsMarkedAsMoved = true;
                }
            }
        }

        private bool AreSimilar(string a, string b)
        {
            if (_settings.IgnoreCase)
            {
                a = a.ToLowerInvariant();
                b = b.ToLowerInvariant();
            }

            if (a == b)
                return true;

            int distance = LevenshteinDistance(a, b);
            int maxLength = Math.Max(a.Length, b.Length);
            double similarity = 1.0 - (double)distance / maxLength;

            return similarity >= _settings.MoveThreshold;
        }

        private int LevenshteinDistance(string a, string b)
        {
            int m = a.Length;
            int n = b.Length;
            var dp = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) dp[i, 0] = i;
            for (int j = 0; j <= n; j++) dp[0, j] = j;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (a[i - 1] == b[j - 1])
                    {
                        dp[i, j] = dp[i - 1, j - 1];
                    }
                    else
                    {
                        dp[i, j] = 1 + Math.Min(dp[i - 1, j], Math.Min(dp[i, j - 1], dp[i - 1, j - 1]));
                    }
                }
            }

            return dp[m, n];
        }

        private void GenerateSummary(ComparisonResult result)
        {
            result.Summary = new ComparisonSummary
            {
                TotalChanges = result.DifferenceCount,
                Additions = result.AddedCount,
                Deletions = result.DeletedCount,
                Modifications = result.ModifiedCount,
                Moves = result.MovedCount,
                SimilarityPercentage = CalculateSimilarity(result),
                ComparisonMethod = _settings.CompareUsing.ToString(),
                ComparisonDate = DateTime.Now
            };
        }

        private double CalculateSimilarity(ComparisonResult result)
        {
            if (result.OriginalContent.Length == 0 && result.RevisedContent.Length == 0)
                return 100.0;

            if (result.OriginalContent.Length == 0 || result.RevisedContent.Length == 0)
                return 0.0;

            int totalLength = result.OriginalContent.Length + result.RevisedContent.Length;
            int unchangedLength = totalLength - (result.AddedCount + result.DeletedCount) * 2;

            return Math.Max(0, (double)unchangedLength / Math.Max(totalLength, 1) * 100);
        }

        public string GenerateSideBySideView(ComparisonResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== ORIGINAL ===");
            sb.AppendLine(result.OriginalContent);
            sb.AppendLine();
            sb.AppendLine("=== REVISED ===");
            sb.AppendLine(result.RevisedContent);
            return sb.ToString();
        }

        public string GenerateInlineView(ComparisonResult result)
        {
            var sb = new StringBuilder(result.OriginalContent);
            var insertions = new List<(int pos, string text)>();
            var deletions = new List<(int pos, string text)>();

            foreach (var diff in result.Differences)
            {
                if (diff.Type == DifferenceType.Added)
                {
                    insertions.Add((diff.Position, diff.Content));
                }
                else if (diff.Type == DifferenceType.Deleted)
                {
                    deletions.Add((diff.Position, diff.Content));
                }
            }

            foreach (var (pos, text) in insertions)
            {
                sb.Insert(pos, $"[+{text}+]");
            }

            return sb.ToString();
        }

        public List<ComparisonPane> GenerateComparisonPanes(ComparisonResult result)
        {
            var panes = new List<ComparisonPane>();

            panes.Add(new ComparisonPane
            {
                Title = "Original",
                Content = result.OriginalContent,
                Differences = _differences.Where(d => d.Type == DifferenceType.Deleted).ToList()
            });

            panes.Add(new ComparisonPane
            {
                Title = "Revised",
                Content = result.RevisedContent,
                Differences = _differences.Where(d => d.Type == DifferenceType.Added).ToList()
            });

            return panes;
        }

        public void ExportToHtml(ComparisonResult result, string outputPath)
        {
            var html = GenerateHtml(result);
            System.IO.File.WriteAllText(outputPath, html);
        }

        private string GenerateHtml(ComparisonResult result)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><title>Document Comparison</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: Arial, sans-serif; margin: 20px; }");
            sb.AppendLine(".added { background-color: #90EE90; }");
            sb.AppendLine(".deleted { background-color: #FFB6C1; text-decoration: line-through; }");
            sb.AppendLine(".modified { background-color: #FFFF00; }");
            sb.AppendLine(".moved { background-color: #87CEEB; }");
            sb.AppendLine(".summary { background-color: #f0f0f0; padding: 10px; margin-bottom: 20px; }");
            sb.AppendLine("</style></head><body>");

            sb.AppendLine("<div class='summary'>");
            sb.AppendLine($"<h2>Comparison Summary</h2>");
            sb.AppendLine($"<p>Total Differences: {result.DifferenceCount}</p>");
            sb.AppendLine($"<p>Added: {result.AddedCount}</p>");
            sb.AppendLine($"<p>Deleted: {result.DeletedCount}</p>");
            sb.AppendLine($"<p>Modified: {result.ModifiedCount}</p>");
            sb.AppendLine($"<p>Moved: {result.MovedCount}</p>");
            sb.AppendLine($"<p>Similarity: {result.Summary?.SimilarityPercentage:F2}%</p>");
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='original'>");
            sb.AppendLine("<h2>Original</h2>");
            sb.AppendLine(FormatContentWithDifferences(result.OriginalContent, DifferenceType.Deleted));
            sb.AppendLine("</div>");

            sb.AppendLine("<div class='revised'>");
            sb.AppendLine("<h2>Revised</h2>");
            sb.AppendLine(FormatContentWithDifferences(result.RevisedContent, DifferenceType.Added));
            sb.AppendLine("</div>");

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string FormatContentWithDifferences(string content, DifferenceType type)
        {
            var diffs = _differences.Where(d => d.Type == type).OrderByDescending(d => d.Position);
            var result = content;

            foreach (var diff in diffs)
            {
                var className = diff.Type.ToString().ToLower();
                result = result.Insert(diff.Position, $"<span class='{className}'>{diff.Content}</span>");
            }

            return result;
        }
    }

    public class ComparisonResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string OriginalContent { get; set; }
        public string RevisedContent { get; set; }
        public List<ComparisonDifference> Differences { get; set; }
        public int DifferenceCount { get; set; }
        public int AddedCount { get; set; }
        public int DeletedCount { get; set; }
        public int ModifiedCount { get; set; }
        public int MovedCount { get; set; }
        public ComparisonSummary Summary { get; set; }

        public ComparisonResult()
        {
            Differences = new List<ComparisonDifference>();
        }
    }

    public class ComparisonDifference
    {
        public string Id { get; set; }
        public DifferenceType Type { get; set; }
        public string Content { get; set; }
        public int Position { get; set; }
        public int? MovedFrom { get; set; }
        public int? MovedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsMarkedAsMoved { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public ComparisonDifference()
        {
            Id = Guid.NewGuid().ToString();
            Properties = new Dictionary<string, object>();
            CreatedAt = DateTime.Now;
        }
    }

    public class ComparisonSummary
    {
        public int TotalChanges { get; set; }
        public int Additions { get; set; }
        public int Deletions { get; set; }
        public int Modifications { get; set; }
        public int Moves { get; set; }
        public double SimilarityPercentage { get; set; }
        public string ComparisonMethod { get; set; }
        public DateTime ComparisonDate { get; set; }
    }

    public class ComparisonPane
    {
        public string Title { get; set; }
        public string Content { get; set; }
        public List<ComparisonDifference> Differences { get; set; }

        public ComparisonPane()
        {
            Differences = new List<ComparisonDifference>();
        }
    }

    public class DocumentComparisonSettings
    {
        public CompareMethod CompareUsing { get; set; }
        public bool IgnoreFormatting { get; set; }
        public bool IgnoreCase { get; set; }
        public bool IgnoreWhitespace { get; set; }
        public bool DetectMoves { get; set; }
        public double MoveThreshold { get; set; }
        public bool CompareHeaders { get; set; }
        public bool CompareFooters { get; set; }
        public bool Compare footnotes { get; set; }
        public string OutputFormat { get; set; }
        public bool GenerateSummary { get; set; }

        public DocumentComparisonSettings()
        {
            CompareUsing = CompareMethod.WordLevel;
            IgnoreFormatting = true;
            IgnoreCase = false;
            IgnoreWhitespace = true;
            DetectMoves = true;
            MoveThreshold = 0.8;
            CompareHeaders = true;
            CompareFooters = true;
            Compare footnotes = true;
            OutputFormat = "Html";
            GenerateSummary = true;
        }
    }

    public enum CompareMethod
    {
        WordLevel,
        CharLevel,
        LineLevel
    }

    public enum DifferenceType
    {
        Added,
        Deleted,
        Modified,
        Moved,
        Format
    }

    public class DocumentNode
    {
        public string Id { get; set; }
        public DocumentNodeType Type { get; set; }
        public string Content { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public List<DocumentNode> Children { get; set; }
        public DocumentNode Parent { get; set; }
        public Dictionary<string, object> Attributes { get; set; }

        public DocumentNode()
        {
            Id = Guid.NewGuid().ToString();
            Children = new List<DocumentNode>();
            Attributes = new Dictionary<string, object>();
        }

        public void AddChild(DocumentNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void RemoveChild(string childId)
        {
            Children.RemoveAll(c => c.Id == childId);
        }

        public DocumentNode Clone()
        {
            return new DocumentNode
            {
                Id = Guid.NewGuid().ToString(),
                Type = Type,
                Content = Content,
                StartPosition = StartPosition,
                EndPosition = EndPosition,
                Attributes = new Dictionary<string, object>(Attributes)
            };
        }
    }

    public enum DocumentNodeType
    {
        Document,
        Section,
        Paragraph,
        Run,
        Heading,
        Table,
        TableRow,
        TableCell,
        List,
        ListItem,
        Image,
        Hyperlink,
        Bookmark,
        Comment,
        Header,
        Footer,
        Footnote
    }

    public class ComparisonNavigator
    {
        private List<ComparisonDifference> _differences;
        private int _currentIndex;

        public ComparisonNavigator(List<ComparisonDifference> differences)
        {
            _differences = differences.OrderBy(d => d.Position).ToList();
            _currentIndex = 0;
        }

        public ComparisonDifference Current => _currentIndex >= 0 && _currentIndex < _differences.Count 
            ? _differences[_currentIndex] : null;

        public bool HasNext => _currentIndex < _differences.Count - 1;
        public bool HasPrevious => _currentIndex > 0;

        public ComparisonDifference Next()
        {
            if (HasNext)
            {
                _currentIndex++;
                return Current;
            }
            return null;
        }

        public ComparisonDifference Previous()
        {
            if (HasPrevious)
            {
                _currentIndex--;
                return Current;
            }
            return null;
        }

        public void GoTo(int index)
        {
            if (index >= 0 && index < _differences.Count)
            {
                _currentIndex = index;
            }
        }

        public ComparisonDifference First()
        {
            _currentIndex = 0;
            return Current;
        }

        public ComparisonDifference Last()
        {
            _currentIndex = _differences.Count - 1;
            return Current;
        }

        public int Count => _differences.Count;
        public int CurrentPosition => _currentIndex;
    }
}

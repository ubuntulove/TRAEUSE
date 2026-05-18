using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeSuite.Word.Comparison;
using OfficeSuite.Word.SpellCheck;

namespace OfficeSuite.Word.Services
{
    public interface IDocumentComparisonService
    {
        Task<DocumentComparisonResult> CompareAsync(string originalPath, string modifiedPath, DocumentComparisonOptions options = null);
        Task<DocumentComparisonResult> CompareAsync(string originalContent, string modifiedContent, bool isFormatted);
        DocumentComparisonResult Compare(string original, string modified, DocumentComparisonOptions options = null);
    }

    public class DocumentComparisonService : IDocumentComparisonService
    {
        private readonly IWordDocumentService _wordDocumentService;

        public DocumentComparisonService(IWordDocumentService wordDocumentService)
        {
            _wordDocumentService = wordDocumentService;
        }

        public async Task<DocumentComparisonResult> CompareAsync(string originalPath, string modifiedPath, DocumentComparisonOptions options = null)
        {
            options ??= new DocumentComparisonOptions();
            var startTime = DateTime.Now;

            var originalDoc = await _wordDocumentService.OpenAsync(originalPath);
            var modifiedDoc = await _wordDocumentService.OpenAsync(modifiedPath);

            var originalText = ExtractText(originalDoc);
            var modifiedText = ExtractText(modifiedDoc);

            var result = Compare(originalText, modifiedText, options);
            result.ComparisonDuration = DateTime.Now - startTime;

            return result;
        }

        public async Task<DocumentComparisonResult> CompareAsync(string originalContent, string modifiedContent, bool isFormatted)
        {
            var options = new DocumentComparisonOptions
            {
                IgnoreFormattingChanges = !isFormatted
            };

            var startTime = DateTime.Now;
            var result = Compare(originalContent, modifiedContent, options);
            result.ComparisonDuration = DateTime.Now - startTime;

            return await Task.FromResult(result);
        }

        public DocumentComparisonResult Compare(string original, string modified, DocumentComparisonOptions options = null)
        {
            options ??= new DocumentComparisonOptions();
            var result = new DocumentComparisonResult();

            if (string.IsNullOrEmpty(original) && string.IsNullOrEmpty(modified))
            {
                return result;
            }

            var originalWords = Tokenize(original, options);
            var modifiedWords = Tokenize(modified, options);

            var lcs = ComputeLongestCommonSubsequence(originalWords, modifiedWords);
            ComputeDifferences(originalWords, modifiedWords, lcs, result, options);

            result.SimilarityPercentage = CalculateSimilarity(originalWords, modifiedWords);
            ComputeStatistics(originalWords, modifiedWords, result);

            return result;
        }

        private List<string> Tokenize(string text, DocumentComparisonOptions options)
        {
            if (string.IsNullOrEmpty(text))
                return new List<string>();

            var tokens = new List<string>();
            var currentToken = new StringBuilder();
            var inWord = false;

            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                bool isWordChar = char.IsLetterOrDigit(c) || c == '\'' || c == '-' || c == '_';

                if (isWordChar)
                {
                    if (!inWord)
                    {
                        if (currentToken.Length > 0)
                        {
                            tokens.Add(currentToken.ToString());
                            currentToken.Clear();
                        }
                        inWord = true;
                    }
                    currentToken.Append(c);
                }
                else
                {
                    if (inWord)
                    {
                        if (currentToken.Length > 0)
                        {
                            tokens.Add(currentToken.ToString());
                            currentToken.Clear();
                        }
                        inWord = false;
                    }

                    if (!options.IgnoreWhitespaceChanges && !char.IsWhiteSpace(c))
                    {
                        currentToken.Append(c);
                        if (currentToken.Length > 0)
                        {
                            tokens.Add(currentToken.ToString());
                            currentToken.Clear();
                        }
                    }
                    else if (c == ' ' || c == '\n' || c == '\r' || c == '\t')
                    {
                        if (currentToken.Length > 0)
                        {
                            tokens.Add(currentToken.ToString());
                            currentToken.Clear();
                        }
                    }
                }
            }

            if (currentToken.Length > 0)
            {
                tokens.Add(currentToken.ToString());
            }

            return tokens;
        }

        private List<int> ComputeLongestCommonSubsequence(List<string> original, List<string> modified)
        {
            int m = original.Count;
            int n = modified.Count;
            var dp = new int[m + 1, n + 1];

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    if (original[i - 1].Equals(modified[j - 1], StringComparison.OrdinalIgnoreCase))
                    {
                        dp[i, j] = dp[i - 1, j - 1] + 1;
                    }
                    else
                    {
                        dp[i, j] = Math.Max(dp[i - 1, j], dp[i, j - 1]);
                    }
                }
            }

            var lcs = new List<int>();
            int x = m, y = n;
            while (x > 0 && y > 0)
            {
                if (original[x - 1].Equals(modified[y - 1], StringComparison.OrdinalIgnoreCase))
                {
                    lcs.Insert(0, x - 1);
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

            return lcs;
        }

        private void ComputeDifferences(List<string> original, List<string> modified, List<int> lcs, DocumentComparisonResult result, DocumentComparisonOptions options)
        {
            int origIndex = 0, modIndex = 0, lcsIndex = 0;
            var removedItems = new List<int>();
            var addedItems = new List<int>();

            while (origIndex < original.Count || modIndex < modified.Count)
            {
                if (lcsIndex < lcs.Count && origIndex < original.Count && lcs[lcsIndex] == origIndex)
                {
                    if (modIndex < modified.Count && !original[origIndex].Equals(modified[modIndex], StringComparison.OrdinalIgnoreCase))
                    {
                        while (modIndex < modified.Count && !original[origIndex].Equals(modified[modIndex], StringComparison.OrdinalIgnoreCase))
                        {
                            addedItems.Add(modIndex);
                            modIndex++;
                        }
                    }
                    else
                    {
                        if (origIndex < original.Count && modIndex < modified.Count && !original[origIndex].Equals(modified[modIndex]))
                        {
                            result.Differences.Add(new DocumentDifference
                            {
                                Position = origIndex,
                                Length = 1,
                                Type = DifferenceType.Modified,
                                OriginalText = original[origIndex],
                                ModifiedText = modified[modIndex],
                                Category = DifferenceCategory.Text
                            });
                            result.ModifiedCount++;
                        }
                        origIndex++;
                        modIndex++;
                        lcsIndex++;
                    }
                }
                else
                {
                    if (origIndex < original.Count && (lcsIndex >= lcs.Count || lcs[lcsIndex] > origIndex))
                    {
                        removedItems.Add(origIndex);
                        origIndex++;
                    }
                    else if (modIndex < modified.Count)
                    {
                        addedItems.Add(modIndex);
                        modIndex++;
                    }
                }
            }

            foreach (var idx in removedItems)
            {
                result.Differences.Add(new DocumentDifference
                {
                    Position = idx,
                    Length = 1,
                    Type = DifferenceType.Removed,
                    OriginalText = original[idx],
                    Category = DifferenceCategory.Text
                });
                result.RemovedCount++;
            }

            foreach (var idx in addedItems)
            {
                result.Differences.Add(new DocumentDifference
                {
                    Position = idx,
                    Length = 1,
                    Type = DifferenceType.Added,
                    ModifiedText = modified[idx],
                    Category = DifferenceCategory.Text
                });
                result.AddedCount++;
            }

            result.Differences = result.Differences.OrderBy(d => d.Position).ToList();
        }

        private double CalculateSimilarity(List<string> original, List<string> modified)
        {
            if (original.Count == 0 && modified.Count == 0)
                return 100.0;

            if (original.Count == 0 || modified.Count == 0)
                return 0.0;

            var lcs = ComputeLongestCommonSubsequence(original, modified);
            double similarity = (2.0 * lcs.Count) / (original.Count + modified.Count) * 100;

            return Math.Round(similarity, 2);
        }

        private void ComputeStatistics(List<string> original, List<string> modified, DocumentComparisonResult result)
        {
            result.Statistics = new ComparisonStatistics
            {
                TotalWordsOriginal = original.Count,
                TotalWordsModified = modified.Count,
                AddedWords = result.AddedCount,
                RemovedWords = result.RemovedCount,
                ChangedWords = result.ModifiedCount,
                IdenticalWords = original.Count - result.RemovedCount - result.ModifiedCount
            };
        }

        private string ExtractText(object document)
        {
            return document?.ToString() ?? string.Empty;
        }
    }

    public interface ISpellCheckService
    {
        Task<List<SpellCheckResult>> CheckTextAsync(string text, SpellCheckOptions options = null);
        Task<List<SpellCheckResult>> CheckSpellingAsync(string text);
        Task<List<GrammarError>> CheckGrammarAsync(string text);
        Task<bool> AddToDictionaryAsync(string word, string dictionaryName = null);
        Task<bool> RemoveFromDictionaryAsync(string word, string dictionaryName = null);
        List<string> GetSuggestions(string word, int maxSuggestions = 10);
    }

    public class SpellCheckService : ISpellCheckService
    {
        private readonly Dictionary<string, HashSet<string>> _customDictionaries;
        private readonly HashSet<string> _ignoredWords;
        private readonly Dictionary<string, List<string>> _autoCorrectEntries;
        private readonly Dictionary<string, int> _wordFrequency;

        private static readonly HashSet<string> CommonWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "be", "to", "of", "and", "a", "in", "that", "have", "i",
            "it", "for", "not", "on", "with", "he", "as", "you", "do", "at",
            "this", "but", "his", "by", "from", "they", "we", "say", "her", "she",
            "or", "an", "will", "my", "one", "all", "would", "there", "their", "what",
            "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
            "when", "make", "can", "like", "time", "no", "just", "him", "know", "take",
            "people", "into", "year", "your", "good", "some", "could", "them", "see", "other",
            "than", "then", "now", "look", "only", "come", "its", "over", "think", "also",
            "back", "after", "use", "two", "how", "our", "work", "first", "well", "way",
            "even", "new", "want", "because", "any", "these", "give", "day", "most", "us",
            "word", "said", "each", "which", "sheet", "document", "page", "paragraph", "format", "text",
            "file", "save", "open", "close", "edit", "view", "insert", "table", "chart", "image"
        };

        public SpellCheckService()
        {
            _customDictionaries = new Dictionary<string, HashSet<string>>();
            _ignoredWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            _autoCorrectEntries = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            _wordFrequency = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            InitializeAutoCorrect();
        }

        private void InitializeAutoCorrect()
        {
            _autoCorrectEntries["teh"] = new List<string> { "the" };
            _autoCorrectEntries["adn"] = new List<string> { "and" };
            _autoCorrectEntries["thsi"] = new List<string> { "this" };
            _autoCorrectEntries["hte"] = new List<string> { "the" };
            _autoCorrectEntries["dont"] = new List<string> { "don't" };
            _autoCorrectEntries["cant"] = new List<string> { "can't" };
            _autoCorrectEntries["wont"] = new List<string> { "won't" };
            _autoCorrectEntries["im"] = new List<string> { "I'm" };
            _autoCorrectEntries["i"] = new List<string> { "I" };
            _autoCorrectEntries["thats"] = new List<string> { "that's" };
            _autoCorrectEntries["whats"] = new List<string> { "what's" };
        }

        public async Task<List<SpellCheckResult>> CheckTextAsync(string text, SpellCheckOptions options = null)
        {
            options ??= new SpellCheckOptions();
            var results = new List<SpellCheckResult>();

            await Task.Run(() =>
            {
                if (string.IsNullOrEmpty(text))
                    return;

                var words = ExtractWords(text);
                int position = 0;

                foreach (var wordInfo in words)
                {
                    if (ShouldSkipWord(wordInfo.Word, options))
                        continue;

                    bool isCorrect = IsWordCorrect(wordInfo.Word);
                    var result = new SpellCheckResult
                    {
                        Word = wordInfo.Word,
                        Position = wordInfo.Position,
                        Length = wordInfo.Word.Length,
                        IsCorrect = isCorrect,
                        Suggestions = isCorrect ? new List<string>() : GetSuggestions(wordInfo.Word, options.MaxSuggestions)
                    };

                    if (!isCorrect && _autoCorrectEntries.ContainsKey(wordInfo.Word.ToLower()))
                    {
                        result.SuggestedAction = SpellCheckAction.AutoCorrect;
                    }

                    results.Add(result);
                }
            });

            return results;
        }

        public async Task<List<SpellCheckResult>> CheckSpellingAsync(string text)
        {
            return await CheckTextAsync(text, new SpellCheckOptions { CheckGrammar = false });
        }

        public async Task<List<GrammarError>> CheckGrammarAsync(string text)
        {
            var errors = new List<GrammarError>();

            await Task.Run(() =>
            {
                var sentences = SplitIntoSentences(text);
                int position = 0;

                foreach (var sentence in sentences)
                {
                    if (string.IsNullOrWhiteSpace(sentence.Text))
                        continue;

                    if (CheckSentenceCapitalization(sentence.Text, sentence.Position, out var capitalizationError))
                    {
                        errors.Add(capitalizationError);
                    }

                    if (CheckSentencePunctuation(sentence.Text, sentence.Position, out var punctuationError))
                    {
                        errors.Add(punctuationError);
                    }

                    CheckRepeatedWords(sentence.Text, sentence.Position, errors);
                }
            });

            return errors;
        }

        public async Task<bool> AddToDictionaryAsync(string word, string dictionaryName = null)
        {
            dictionaryName ??= "default";

            return await Task.Run(() =>
            {
                if (!_customDictionaries.ContainsKey(dictionaryName))
                {
                    _customDictionaries[dictionaryName] = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                }

                _customDictionaries[dictionaryName].Add(word);
                _ignoredWords.Add(word);

                return true;
            });
        }

        public async Task<bool> RemoveFromDictionaryAsync(string word, string dictionaryName = null)
        {
            dictionaryName ??= "default";

            return await Task.Run(() =>
            {
                if (_customDictionaries.ContainsKey(dictionaryName))
                {
                    _customDictionaries[dictionaryName].Remove(word);
                }

                _ignoredWords.Remove(word);

                return true;
            });
        }

        public List<string> GetSuggestions(string word, int maxSuggestions = 10)
        {
            if (string.IsNullOrEmpty(word))
                return new List<string>();

            var suggestions = new List<(string Word, int Distance)>();

            var allWords = new HashSet<string>(CommonWords, StringComparer.OrdinalIgnoreCase);
            foreach (var dict in _customDictionaries.Values)
            {
                foreach (var w in dict)
                {
                    allWords.Add(w);
                }
            }

            foreach (var candidate in allWords)
            {
                if (candidate.Equals(word, StringComparison.OrdinalIgnoreCase))
                    continue;

                int distance = LevenshteinDistance(word.ToLower(), candidate.ToLower());
                if (distance <= 3)
                {
                    suggestions.Add((candidate, distance));
                }
            }

            return suggestions
                .OrderBy(s => s.Distance)
                .ThenByDescending(s => GetWordFrequency(s.Word))
                .Take(maxSuggestions)
                .Select(s => s.Word)
                .ToList();
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int m = s1.Length;
            int n = s2.Length;
            var dp = new int[m + 1, n + 1];

            for (int i = 0; i <= m; i++) dp[i, 0] = i;
            for (int j = 0; j <= n; j++) dp[0, j] = j;

            for (int i = 1; i <= m; i++)
            {
                for (int j = 1; j <= n; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                    dp[i, j] = Math.Min(
                        Math.Min(dp[i - 1, j] + 1, dp[i, j - 1] + 1),
                        dp[i - 1, j - 1] + cost
                    );
                }
            }

            return dp[m, n];
        }

        private List<(string Word, int Position)> ExtractWords(string text)
        {
            var words = new List<(string, int)>();
            var currentWord = new StringBuilder();
            int position = 0;

            foreach (char c in text)
            {
                if (char.IsLetterOrDigit(c) || c == '\'' || c == '-' || c == '_')
                {
                    if (currentWord.Length == 0)
                    {
                        position = text.IndexOf(c);
                    }
                    currentWord.Append(c);
                }
                else
                {
                    if (currentWord.Length > 0)
                    {
                        words.Add((currentWord.ToString(), position));
                        currentWord.Clear();
                    }
                }
            }

            if (currentWord.Length > 0)
            {
                words.Add((currentWord.ToString(), position));
            }

            return words;
        }

        private bool ShouldSkipWord(string word, SpellCheckOptions options)
        {
            if (string.IsNullOrEmpty(word))
                return true;

            if (_ignoredWords.Contains(word))
                return true;

            if (options.IgnoreWordsInUppercase && word.All(char.IsUpper) && word.Length > 1)
                return true;

            if (options.IgnoreWordsWithNumbers && word.Any(char.IsDigit))
                return true;

            if (options.IgnoreInternetAddresses && (word.Contains("http://") || word.Contains("https://") || word.Contains("www.")))
                return true;

            if (options.IgnoreFileNames && (word.Contains("/") || word.Contains("\\") || word.Contains(".")))
                return true;

            return false;
        }

        private bool IsWordCorrect(string word)
        {
            if (CommonWords.Contains(word))
                return true;

            foreach (var dict in _customDictionaries.Values)
            {
                if (dict.Contains(word))
                    return true;
            }

            return false;
        }

        private int GetWordFrequency(string word)
        {
            return _wordFrequency.TryGetValue(word, out int freq) ? freq : 0;
        }

        private List<(string Text, int Position)> SplitIntoSentences(string text)
        {
            var sentences = new List<(string, int)>();
            var current = new StringBuilder();
            int position = 0;

            foreach (char c in text)
            {
                current.Append(c);

                if (c == '.' || c == '!' || c == '?')
                {
                    if (current.Length > 0)
                    {
                        sentences.Add((current.ToString().Trim(), position));
                        current.Clear();
                        position++;
                    }
                }
            }

            if (current.Length > 0)
            {
                sentences.Add((current.ToString().Trim(), position));
            }

            return sentences;
        }

        private bool CheckSentenceCapitalization(string sentence, int position, out GrammarError error)
        {
            error = null;

            if (string.IsNullOrEmpty(sentence))
                return false;

            var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0)
                return false;

            var firstWord = words[0];
            if (char.IsLetter(firstWord[0]) && char.IsLower(firstWord[0]))
            {
                error = new GrammarError
                {
                    Position = position,
                    Length = firstWord.Length,
                    Text = firstWord,
                    Message = "Sentence should begin with a capital letter.",
                    Type = GrammarErrorType.Grammar,
                    Suggestions = new List<GrammarSuggestion>
                    {
                        new() { SuggestedText = char.ToUpper(firstWord[0]) + firstWord.Substring(1) }
                    }
                };
                return true;
            }

            return false;
        }

        private bool CheckSentencePunctuation(string sentence, int position, out GrammarError error)
        {
            error = null;

            if (string.IsNullOrEmpty(sentence))
                return false;

            if (!sentence.EndsWith(".") && !sentence.EndsWith("!") && !sentence.EndsWith("?"))
            {
                error = new GrammarError
                {
                    Position = position + sentence.Length - 1,
                    Length = 1,
                    Text = sentence.Substring(sentence.Length - 1),
                    Message = "Sentence should end with punctuation.",
                    Type = GrammarErrorType.Punctuation
                };
                return true;
            }

            return false;
        }

        private void CheckRepeatedWords(string sentence, int position, List<GrammarError> errors)
        {
            var words = sentence.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < words.Length - 1; i++)
            {
                if (words[i].Equals(words[i + 1], StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new GrammarError
                    {
                        Position = position + i,
                        Length = words[i].Length * 2 + 1,
                        Text = words[i] + " " + words[i + 1],
                        Message = "Repeated word detected.",
                        Type = GrammarErrorType.Style,
                        Suggestions = new List<GrammarSuggestion>
                        {
                            new() { SuggestedText = words[i] }
                        }
                    });
                }
            }
        }
    }
}

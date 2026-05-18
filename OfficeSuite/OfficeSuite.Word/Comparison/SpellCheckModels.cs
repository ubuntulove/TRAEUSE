using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfficeSuite.Word.Comparison
{
    public class DocumentDifference
    {
        public int Position { get; set; }
        public int Length { get; set; }
        public DifferenceType Type { get; set; }
        public string OriginalText { get; set; } = string.Empty;
        public string ModifiedText { get; set; } = string.Empty;
        public DifferenceCategory Category { get; set; }
    }

    public enum DifferenceType
    {
        Added,
        Removed,
        Modified,
        Moved
    }

    public enum DifferenceCategory
    {
        Text,
        Formatting,
        Paragraph,
        Table,
        Image,
        HeaderFooter,
        Comment
    }

    public class DocumentComparisonResult
    {
        public List<DocumentDifference> Differences { get; set; } = new();
        public int AddedCount { get; set; }
        public int RemovedCount { get; set; }
        public int ModifiedCount { get; set; }
        public double SimilarityPercentage { get; set; }
        public TimeSpan ComparisonDuration { get; set; }
        public ComparisonStatistics Statistics { get; set; } = new();
    }

    public class ComparisonStatistics
    {
        public int TotalWordsOriginal { get; set; }
        public int TotalWordsModified { get; set; }
        public int IdenticalWords { get; set; }
        public int ChangedWords { get; set; }
        public int AddedWords { get; set; }
        public int RemovedWords { get; set; }
    }

    public class DocumentComparisonOptions
    {
        public bool IgnoreFormattingChanges { get; set; }
        public bool IgnoreCaseChanges { get; set; }
        public bool IgnoreWhitespaceChanges { get; set; }
        public bool IgnorePunctuationChanges { get; set; }
        public bool CompareTables { get; set; } = true;
        public bool CompareImages { get; set; } = true;
        public bool CompareHeadersFooters { get; set; } = true;
        public bool CompareComments { get; set; } = true;
        public int SimilarityThreshold { get; set; } = 80;
    }

    public class ComparisonNavigator
    {
        private readonly DocumentComparisonResult _result;
        private int _currentIndex = -1;

        public ComparisonNavigator(DocumentComparisonResult result)
        {
            _result = result;
        }

        public DocumentDifference? Current => _currentIndex >= 0 && _currentIndex < _result.Differences.Count
            ? _result.Differences[_currentIndex]
            : null;

        public bool CanMoveNext => _currentIndex < _result.Differences.Count - 1;
        public bool CanMovePrevious => _currentIndex > 0;
        public int CurrentPosition => _currentIndex + 1;
        public int TotalDifferences => _result.Differences.Count;

        public DocumentDifference? MoveNext()
        {
            if (CanMoveNext)
            {
                _currentIndex++;
                return Current;
            }
            return null;
        }

        public DocumentDifference? MovePrevious()
        {
            if (CanMovePrevious)
            {
                _currentIndex--;
                return Current;
            }
            return null;
        }

        public void MoveTo(int index)
        {
            if (index >= 0 && index < _result.Differences.Count)
            {
                _currentIndex = index;
            }
        }

        public void MoveToStart()
        {
            _currentIndex = 0;
        }

        public void MoveToEnd()
        {
            _currentIndex = _result.Differences.Count - 1;
        }

        public IEnumerable<DocumentDifference> GetDifferencesByType(DifferenceType type)
        {
            return _result.Differences.Where(d => d.Type == type);
        }

        public IEnumerable<DocumentDifference> GetDifferencesByCategory(DifferenceCategory category)
        {
            return _result.Differences.Where(d => d.Category == category);
        }
    }
}

namespace OfficeSuite.Word.SpellCheck
{
    public class SpellCheckResult
    {
        public string Word { get; set; } = string.Empty;
        public int Position { get; set; }
        public int Length { get; set; }
        public bool IsCorrect { get; set; }
        public List<string> Suggestions { get; set; } = new();
        public SpellCheckAction SuggestedAction { get; set; }
        public string? Dictionary { get; set; }
    }

    public enum SpellCheckAction
    {
        None,
        Ignore,
        IgnoreAll,
        AddToDictionary,
        Change,
        ChangeAll,
        AutoCorrect
    }

    public class SpellCheckOptions
    {
        public bool CheckGrammar { get; set; } = true;
        public bool CheckSpelling { get; set; } = true;
        public bool IgnoreWordsInUppercase { get; set; } = true;
        public bool IgnoreWordsWithNumbers { get; set; } = true;
        public bool IgnoreInternetAddresses { get; set; } = true;
        public bool IgnoreFileNames { get; set; } = true;
        public bool SuggestCorrections { get; set; } = true;
        public int MaxSuggestions { get; set; } = 10;
        public string? CustomDictionary { get; set; }
    }

    public class GrammarError
    {
        public int Position { get; set; }
        public int Length { get; set; }
        public string Text { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public GrammarErrorType Type { get; set; }
        public List<GrammarSuggestion> Suggestions { get; set; } = new();
    }

    public enum GrammarErrorType
    {
        Spelling,
        Grammar,
        Punctuation,
        Style,
        Formatting
    }

    public class GrammarSuggestion
    {
        public string SuggestedText { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public GrammarRule? Rule { get; set; }
    }

    public class GrammarRule
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
    }

    public class DictionaryEntry
    {
        public string Word { get; set; } = string.Empty;
        public string? CustomDictionary { get; set; }
        public DateTime AddedDate { get; set; }
    }

    public class AutoCorrectEntry
    {
        public string Original { get; set; } = string.Empty;
        public string Replacement { get; set; } = string.Empty;
        public bool PreserveCapitalization { get; set; }
        public bool CorrectInitialCaps { get; set; }
    }
}

namespace OfficeSuite.Word.TrackChanges
{
    public class ChangeInfo
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public ChangeType Type { get; set; }
        public ChangeAction Action { get; set; }
        public string? Author { get; set; }
        public DateTime Timestamp { get; set; }
        public int Position { get; set; }
        public int Length { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set; }
        public RevisionState State { get; set; } = RevisionState.Accepted;
    }

    public enum ChangeType
    {
        Insertion,
        Deletion,
        FormatChange,
        StyleChange,
        ParagraphChange
    }

    public enum ChangeAction
    {
        Added,
        Deleted,
        Modified,
        Moved,
        Copied
    }

    public enum RevisionState
    {
        Pending,
        Accepted,
        Rejected
    }

    public class AuthorInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Initials { get; set; } = string.Empty;
        public string? Color { get; set; }
        public string? Email { get; set; }
        public DateTime FirstSessionDate { get; set; }
        public int ChangeCount { get; set; }
    }

    public class ReviewSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public List<ChangeInfo> Changes { get; set; } = new();
        public string? Author { get; set; }
    }
}

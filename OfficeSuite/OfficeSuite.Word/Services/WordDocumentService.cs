using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;

namespace OfficeSuite.Word.Services
{
    public class WordDocumentService : IWordDocumentService
    {
        private readonly List<DocumentInfo> _recentDocuments;
        private DocumentInfo? _currentDocument;

        public WordDocumentService()
        {
            _recentDocuments = new List<DocumentInfo>();
        }

        public DocumentInfo? CurrentDocument => _currentDocument;

        public IReadOnlyList<DocumentInfo> RecentDocuments => _recentDocuments.AsReadOnly();

        public Task<DocumentInfo> CreateAsync(string? fileName = null)
        {
            var document = new DocumentInfo
            {
                FileName = fileName ?? "Untitled.docx",
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                WordCount = 0,
                PageCount = 1
            };

            _currentDocument = document;
            return Task.FromResult(document);
        }

        public Task<DocumentInfo> OpenAsync(string filePath)
        {
            var document = new DocumentInfo
            {
                FileName = System.IO.Path.GetFileName(filePath),
                FilePath = filePath,
                CreatedDate = System.IO.File.GetCreationTime(filePath),
                ModifiedDate = System.IO.File.GetLastWriteTime(filePath),
                WordCount = 0,
                PageCount = 1
            };

            _currentDocument = document;

            if (!_recentDocuments.Any(d => d.FilePath == filePath))
            {
                _recentDocuments.Insert(0, document);
                if (_recentDocuments.Count > 10)
                    _recentDocuments.RemoveAt(_recentDocuments.Count - 1);
            }

            return Task.FromResult(document);
        }

        public Task<bool> SaveAsync(DocumentInfo document, string? filePath = null)
        {
            if (filePath != null)
            {
                document.FilePath = filePath;
                document.FileName = System.IO.Path.GetFileName(filePath);
            }

            document.ModifiedDate = DateTime.Now;
            return Task.FromResult(true);
        }

        public Task<bool> CloseAsync(DocumentInfo document)
        {
            if (_currentDocument == document)
            {
                _currentDocument = null;
            }

            return Task.FromResult(true);
        }

        public Task<bool> ExportAsync(DocumentInfo document, ExportFormat format, string filePath)
        {
            return Task.FromResult(true);
        }

        public Task<DocumentStatistics> GetStatisticsAsync(DocumentInfo document)
        {
            var stats = new DocumentStatistics
            {
                PageCount = document.PageCount,
                WordCount = document.WordCount,
                CharacterCount = document.CharacterCount,
                ParagraphCount = document.ParagraphCount,
                LineCount = document.LineCount
            };

            return Task.FromResult(stats);
        }

        public Task<bool> ProtectAsync(DocumentInfo document, ProtectionType type, string? password = null)
        {
            document.IsProtected = true;
            document.ProtectionType = type;
            return Task.FromResult(true);
        }

        public Task<bool> UnprotectAsync(DocumentInfo document, string? password = null)
        {
            document.IsProtected = false;
            document.ProtectionType = null;
            return Task.FromResult(true);
        }
    }

    public class DocumentInfo : INotifyPropertyChanged
    {
        private string _fileName = "Untitled.docx";
        private string _filePath = string.Empty;
        private DateTime _createdDate = DateTime.Now;
        private DateTime _modifiedDate = DateTime.Now;
        private int _wordCount;
        private int _characterCount;
        private int _paragraphCount;
        private int _lineCount;
        private int _pageCount = 1;
        private bool _isModified;
        private bool _isProtected;
        private ProtectionType? _protectionType;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FileName
        {
            get => _fileName;
            set { _fileName = value; OnPropertyChanged(nameof(FileName)); }
        }

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set { _createdDate = value; OnPropertyChanged(nameof(CreatedDate)); }
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set { _modifiedDate = value; OnPropertyChanged(nameof(ModifiedDate)); }
        }

        public int WordCount
        {
            get => _wordCount;
            set { _wordCount = value; OnPropertyChanged(nameof(WordCount)); }
        }

        public int CharacterCount
        {
            get => _characterCount;
            set { _characterCount = value; OnPropertyChanged(nameof(CharacterCount)); }
        }

        public int ParagraphCount
        {
            get => _paragraphCount;
            set { _paragraphCount = value; OnPropertyChanged(nameof(ParagraphCount)); }
        }

        public int LineCount
        {
            get => _lineCount;
            set { _lineCount = value; OnPropertyChanged(nameof(LineCount)); }
        }

        public int PageCount
        {
            get => _pageCount;
            set { _pageCount = value; OnPropertyChanged(nameof(PageCount)); }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(nameof(IsModified)); }
        }

        public bool IsProtected
        {
            get => _isProtected;
            set { _isProtected = value; OnPropertyChanged(nameof(IsProtected)); }
        }

        public ProtectionType? ProtectionType
        {
            get => _protectionType;
            set { _protectionType = value; OnPropertyChanged(nameof(ProtectionType)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DocumentStatistics
    {
        public int PageCount { get; set; }
        public int WordCount { get; set; }
        public int CharacterCount { get; set; }
        public int CharacterCountWithSpaces { get; set; }
        public int ParagraphCount { get; set; }
        public int LineCount { get; set; }
    }

    public enum ExportFormat
    {
        Docx,
        Pdf,
        Rtf,
        Text,
        Html,
        Odt
    }

    public enum ProtectionType
    {
        ReadOnly,
        Comments,
        TrackChanges,
        Forms
    }

    public class UndoRedoManager
    {
        private readonly Stack<IUndoableAction> _undoStack = new();
        private readonly Stack<IUndoableAction> _redoStack = new();
        private readonly int _maxStackSize = 100;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public event EventHandler? StateChanged;

        public void Execute(IUndoableAction action)
        {
            action.Execute();
            _undoStack.Push(action);
            _redoStack.Clear();

            if (_undoStack.Count > _maxStackSize)
            {
                var items = _undoStack.ToArray();
                _undoStack.Clear();
                for (int i = 0; i < _maxStackSize; i++)
                {
                    _undoStack.Push(items[i]);
                }
            }

            StateChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var action = _undoStack.Pop();
                action.Undo();
                _redoStack.Push(action);
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var action = _redoStack.Pop();
                action.Execute();
                _undoStack.Push(action);
                StateChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            StateChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public interface IUndoableAction
    {
        void Execute();
        void Undo();
        string Description { get; }
    }

    public class TextChangeAction : IUndoableAction
    {
        private readonly string _oldText;
        private readonly string _newText;
        private readonly Action<string> _applyAction;

        public string Description => "Text Change";

        public TextChangeAction(string oldText, string newText, Action<string> applyAction)
        {
            _oldText = oldText;
            _newText = newText;
            _applyAction = applyAction;
        }

        public void Execute() => _applyAction(_newText);
        public void Undo() => _applyAction(_oldText);
    }

    public class FormattingChangeAction : IUndoableAction
    {
        private readonly object _oldFormat;
        private readonly object _newFormat;
        private readonly Action<object> _applyAction;

        public string Description => "Formatting Change";

        public FormattingChangeAction(object oldFormat, object newFormat, Action<object> applyAction)
        {
            _oldFormat = oldFormat;
            _newFormat = newFormat;
            _applyAction = applyAction;
        }

        public void Execute() => _applyAction(_newFormat);
        public void Undo() => _applyAction(_oldFormat);
    }

    public class InsertDeleteAction : IUndoableAction
    {
        private readonly int _position;
        private readonly string _content;
        private readonly bool _isInsert;
        private readonly Action<int, string> _insertAction;
        private readonly Action<int, int> _deleteAction;

        public string Description => _isInsert ? "Insert" : "Delete";

        public InsertDeleteAction(int position, string content, bool isInsert,
            Action<int, string> insertAction, Action<int, int> deleteAction)
        {
            _position = position;
            _content = content;
            _isInsert = isInsert;
            _insertAction = insertAction;
            _deleteAction = deleteAction;
        }

        public void Execute()
        {
            if (_isInsert)
                _insertAction(_position, _content);
            else
                _deleteAction(_position, _content.Length);
        }

        public void Undo()
        {
            if (_isInsert)
                _deleteAction(_position, _content.Length);
            else
                _insertAction(_position, _content);
        }
    }

    public class ClipboardManager
    {
        private readonly Stack<ClipboardItem> _clipboardHistory = new();
        private readonly int _maxHistorySize = 20;

        public event EventHandler? ClipboardChanged;

        public void Copy(string content, ClipboardDataType dataType)
        {
            var item = new ClipboardItem
            {
                Content = content,
                DataType = dataType,
                Timestamp = DateTime.Now
            };

            _clipboardHistory.Push(item);

            if (_clipboardHistory.Count > _maxHistorySize)
            {
                var items = _clipboardHistory.ToArray();
                _clipboardHistory.Clear();
                for (int i = 0; i < _maxHistorySize; i++)
                {
                    _clipboardHistory.Push(items[i]);
                }
            }

            ClipboardChanged?.Invoke(this, EventArgs.Empty);
        }

        public ClipboardItem? Paste()
        {
            return _clipboardHistory.Count > 0 ? _clipboardHistory.Peek() : null;
        }

        public IEnumerable<ClipboardItem> GetHistory()
        {
            return _clipboardHistory.ToArray().Reverse();
        }

        public void Clear()
        {
            _clipboardHistory.Clear();
            ClipboardChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class ClipboardItem
    {
        public string Content { get; set; } = string.Empty;
        public ClipboardDataType DataType { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public enum ClipboardDataType
    {
        Text,
        RichText,
        Image,
        Html,
        Custom
    }

    public class SelectionManager
    {
        private int _startPosition;
        private int _endPosition;
        private string _selectedText = string.Empty;
        private bool _hasSelection;

        public int StartPosition
        {
            get => _startPosition;
            set
            {
                _startPosition = value;
                UpdateSelection();
            }
        }

        public int EndPosition
        {
            get => _endPosition;
            set
            {
                _endPosition = value;
                UpdateSelection();
            }
        }

        public string SelectedText => _selectedText;
        public bool HasSelection => _hasSelection;

        public event EventHandler? SelectionChanged;

        private void UpdateSelection()
        {
            _hasSelection = _startPosition != _endPosition;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            _startPosition = 0;
            _endPosition = 0;
            _selectedText = string.Empty;
            _hasSelection = false;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void SelectAll(int documentLength)
        {
            _startPosition = 0;
            _endPosition = documentLength;
            _hasSelection = true;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public (int Start, int End) GetNormalizedRange()
        {
            return _startPosition <= _endPosition
                ? (_startPosition, _endPosition)
                : (_endPosition, _startPosition);
        }
    }

    public class SearchManager
    {
        private readonly List<SearchMatch> _matches = new();
        private int _currentMatchIndex = -1;

        public IReadOnlyList<SearchMatch> Matches => _matches.AsReadOnly();
        public int CurrentMatchIndex => _currentMatchIndex;
        public SearchMatch? CurrentMatch => _currentMatchIndex >= 0 && _currentMatchIndex < _matches.Count
            ? _matches[_currentMatchIndex]
            : null;
        public int MatchCount => _matches.Count;

        public event EventHandler? MatchesChanged;

        public void SetMatches(IEnumerable<SearchMatch> matches)
        {
            _matches.Clear();
            _matches.AddRange(matches);
            _currentMatchIndex = _matches.Count > 0 ? 0 : -1;
            MatchesChanged?.Invoke(this, EventArgs.Empty);
        }

        public void ClearMatches()
        {
            _matches.Clear();
            _currentMatchIndex = -1;
            MatchesChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool MoveNext()
        {
            if (_matches.Count == 0)
                return false;

            _currentMatchIndex = (_currentMatchIndex + 1) % _matches.Count;
            MatchesChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public bool MovePrevious()
        {
            if (_matches.Count == 0)
                return false;

            _currentMatchIndex = _currentMatchIndex <= 0 ? _matches.Count - 1 : _currentMatchIndex - 1;
            MatchesChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        public void GoToMatch(int index)
        {
            if (index >= 0 && index < _matches.Count)
            {
                _currentMatchIndex = index;
                MatchesChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public class SearchMatch
    {
        public int Position { get; set; }
        public int Length { get; set; }
        public string Text { get; set; } = string.Empty;
        public SearchMatchType Type { get; set; }
    }

    public enum SearchMatchType
    {
        Normal,
        Highlight,
        Current
    }

    public class BookmarkManager
    {
        private readonly Dictionary<string, Bookmark> _bookmarks = new();

        public IEnumerable<Bookmark> Bookmarks => _bookmarks.Values;
        public int Count => _bookmarks.Count;

        public event EventHandler? BookmarksChanged;

        public void AddBookmark(string name, int startPosition, int endPosition)
        {
            if (!_bookmarks.ContainsKey(name))
            {
                _bookmarks[name] = new Bookmark
                {
                    Name = name,
                    StartPosition = startPosition,
                    EndPosition = endPosition,
                    CreatedDate = DateTime.Now
                };
                BookmarksChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveBookmark(string name)
        {
            if (_bookmarks.Remove(name))
            {
                BookmarksChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Bookmark? GetBookmark(string name)
        {
            return _bookmarks.TryGetValue(name, out var bookmark) ? bookmark : null;
        }

        public bool HasBookmark(string name) => _bookmarks.ContainsKey(name);

        public IEnumerable<Bookmark> GetBookmarksInRange(int start, int end)
        {
            return _bookmarks.Values.Where(b =>
                (b.StartPosition >= start && b.StartPosition <= end) ||
                (b.EndPosition >= start && b.EndPosition <= end));
        }
    }

    public class Bookmark
    {
        public string Name { get; set; } = string.Empty;
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Content { get; set; }
    }

    public class HyperlinkManager
    {
        private readonly Dictionary<string, Hyperlink> _hyperlinks = new();

        public IEnumerable<Hyperlink> Hyperlinks => _hyperlinks.Values;
        public int Count => _hyperlinks.Count;

        public event EventHandler? HyperlinksChanged;

        public void AddHyperlink(string id, string url, string displayText, int startPosition, int endPosition)
        {
            _hyperlinks[id] = new Hyperlink
            {
                Id = id,
                Url = url,
                DisplayText = displayText,
                StartPosition = startPosition,
                EndPosition = endPosition
            };
            HyperlinksChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RemoveHyperlink(string id)
        {
            if (_hyperlinks.Remove(id))
            {
                HyperlinksChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public Hyperlink? GetHyperlink(string id)
        {
            return _hyperlinks.TryGetValue(id, out var hyperlink) ? hyperlink : null;
        }

        public Hyperlink? GetHyperlinkAtPosition(int position)
        {
            return _hyperlinks.Values.FirstOrDefault(h =>
                position >= h.StartPosition && position <= h.EndPosition);
        }
    }

    public class Hyperlink
    {
        public string Id { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string DisplayText { get; set; } = string.Empty;
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public HyperlinkType Type { get; set; }
    }

    public enum HyperlinkType
    {
        Web,
        File,
        Email,
        Bookmark,
        Slide
    }
}

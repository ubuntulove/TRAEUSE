using System;
using System.Collections.Generic;
using System.Linq;
using OfficeSuite.Core.Models;
using OfficeSuite.Core.ViewModels;

namespace OfficeSuite.Word.ViewModels
{
    public class WordEditorViewModel : ViewModelBase
    {
        private WordDocument _document;
        private Paragraph _currentParagraph;
        private Run _currentRun;
        private int _caretPosition;
        private bool _isEditing;
        private bool _isSelectionActive;
        private CharacterFormat _selectionFormat;
        private ParagraphFormat _paragraphFormat;
        private UndoRedoManager _undoRedoManager;
        private FindReplaceManager _findReplaceManager;
        private Dictionary<string, bool> _formattingStates;

        public WordDocument Document
        {
            get => _document;
            set => SetProperty(ref _document, value);
        }

        public Paragraph CurrentParagraph
        {
            get => _currentParagraph;
            set => SetProperty(ref _currentParagraph, value);
        }

        public Run CurrentRun
        {
            get => _currentRun;
            set => SetProperty(ref _currentRun, value);
        }

        public int CaretPosition
        {
            get => _caretPosition;
            set => SetProperty(ref _caretPosition, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsSelectionActive
        {
            get => _isSelectionActive;
            set => SetProperty(ref _isSelectionActive, value);
        }

        public CharacterFormat SelectionFormat
        {
            get => _selectionFormat ??= new CharacterFormat();
            set => SetProperty(ref _selectionFormat, value);
        }

        public ParagraphFormat ParagraphFormat
        {
            get => _paragraphFormat ??= new ParagraphFormat();
            set => SetProperty(ref _paragraphFormat, value);
        }

        public UndoRedoManager UndoRedoManager
        {
            get => _undoRedoManager ??= new UndoRedoManager();
            set => SetProperty(ref _undoRedoManager, value);
        }

        public FindReplaceManager FindReplaceManager
        {
            get => _findReplaceManager ??= new FindReplaceManager();
            set => SetProperty(ref _findReplaceManager, value);
        }

        public Dictionary<string, bool> FormattingStates
        {
            get => _formattingStates ??= new Dictionary<string, bool>();
            set => SetProperty(ref _formattingStates, value);
        }

        public RelayCommand NewDocumentCommand { get; }
        public RelayCommand OpenDocumentCommand { get; }
        public RelayCommand SaveDocumentCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand PrintCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }
        public RelayCommand CutCommand { get; }
        public RelayCommand CopyCommand { get; }
        public RelayCommand PasteCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand BoldCommand { get; }
        public RelayCommand ItalicCommand { get; }
        public RelayCommand UnderlineCommand { get; }
        public RelayCommand StrikethroughCommand { get; }
        public RelayCommand SubscriptCommand { get; }
        public RelayCommand SuperscriptCommand { get; }
        public RelayCommand FindCommand { get; }
        public RelayCommand ReplaceCommand { get; }
        public RelayCommand InsertPageBreakCommand { get; }
        public RelayCommand InsertTableCommand { get; }
        public RelayCommand InsertImageCommand { get; }
        public RelayCommand SetAlignmentLeftCommand { get; }
        public RelayCommand SetAlignmentCenterCommand { get; }
        public RelayCommand SetAlignmentRightCommand { get; }
        public RelayCommand SetAlignmentJustifyCommand { get; }
        public RelayCommand IncreaseIndentCommand { get; }
        public RelayCommand DecreaseIndentCommand { get; }
        public RelayCommand SetBulletListCommand { get; }
        public RelayCommand SetNumberListCommand { get; }

        public WordEditorViewModel()
        {
            Document = new WordDocument();
            UndoRedoManager = new UndoRedoManager();
            FormattingStates = new Dictionary<string, bool>();

            InitializeCommands();
            InitializeFormattingStates();
        }

        private void InitializeCommands()
        {
            NewDocumentCommand = new RelayCommand(_ => NewDocument());
            OpenDocumentCommand = new RelayCommand(_ => OpenDocument());
            SaveDocumentCommand = new RelayCommand(_ => SaveDocument(), _ => CanSaveDocument());
            SaveAsCommand = new RelayCommand(_ => SaveDocumentAs());
            PrintCommand = new RelayCommand(_ => Print());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo());
            CutCommand = new RelayCommand(_ => Cut(), _ => CanCut());
            CopyCommand = new RelayCommand(_ => Copy(), _ => CanCopy());
            PasteCommand = new RelayCommand(_ => Paste());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => CanDelete());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            BoldCommand = new RelayCommand(_ => ToggleBold());
            ItalicCommand = new RelayCommand(_ => ToggleItalic());
            UnderlineCommand = new RelayCommand(_ => ToggleUnderline());
            StrikethroughCommand = new RelayCommand(_ => ToggleStrikethrough());
            SubscriptCommand = new RelayCommand(_ => ToggleSubscript());
            SuperscriptCommand = new RelayCommand(_ => ToggleSuperscript());
            FindCommand = new RelayCommand(_ => ShowFindDialog());
            ReplaceCommand = new RelayCommand(_ => ShowReplaceDialog());
            InsertPageBreakCommand = new RelayCommand(_ => InsertPageBreak());
            InsertTableCommand = new RelayCommand(_ => InsertTable());
            InsertImageCommand = new RelayCommand(_ => InsertImage());
            SetAlignmentLeftCommand = new RelayCommand(_ => SetAlignment(TextAlignment.Left));
            SetAlignmentCenterCommand = new RelayCommand(_ => SetAlignment(TextAlignment.Center));
            SetAlignmentRightCommand = new RelayCommand(_ => SetAlignment(TextAlignment.Right));
            SetAlignmentJustifyCommand = new RelayCommand(_ => SetAlignment(TextAlignment.Justify));
            IncreaseIndentCommand = new RelayCommand(_ => IncreaseIndent());
            DecreaseIndentCommand = new RelayCommand(_ => DecreaseIndent());
            SetBulletListCommand = new RelayCommand(_ => SetBulletList());
            SetNumberListCommand = new RelayCommand(_ => SetNumberList());
        }

        private void InitializeFormattingStates()
        {
            FormattingStates["Bold"] = false;
            FormattingStates["Italic"] = false;
            FormattingStates["Underline"] = false;
            FormattingStates["Strikethrough"] = false;
            FormattingStates["Subscript"] = false;
            FormattingStates["Superscript"] = false;
            FormattingStates["BulletList"] = false;
            FormattingStates["NumberList"] = false;
        }

        public void NewDocument()
        {
            Document = new WordDocument();
            var paragraph = new Paragraph();
            Document.Paragraphs.Add(paragraph);
            CurrentParagraph = paragraph;
            UndoRedoManager.Clear();
            OnPropertyChanged(nameof(Document));
        }

        public void OpenDocument()
        {
        }

        public void SaveDocument()
        {
        }

        public bool CanSaveDocument()
        {
            return Document != null && Document.Paragraphs.Count > 0;
        }

        public void SaveDocumentAs()
        {
        }

        public void Print()
        {
        }

        public void Undo()
        {
            UndoRedoManager.Undo();
        }

        public bool CanUndo()
        {
            return UndoRedoManager.CanUndo;
        }

        public void Redo()
        {
            UndoRedoManager.Redo();
        }

        public bool CanRedo()
        {
            return UndoRedoManager.CanRedo;
        }

        public void Cut()
        {
        }

        public bool CanCut()
        {
            return IsSelectionActive;
        }

        public void Copy()
        {
        }

        public bool CanCopy()
        {
            return IsSelectionActive;
        }

        public void Paste()
        {
        }

        public void Delete()
        {
        }

        public bool CanDelete()
        {
            return IsSelectionActive || CaretPosition > 0;
        }

        public void SelectAll()
        {
            IsSelectionActive = true;
        }

        public void ToggleBold()
        {
            if (CurrentRun != null)
            {
                CurrentRun.Format.Bold = !CurrentRun.Format.Bold;
                FormattingStates["Bold"] = CurrentRun.Format.Bold;
                OnPropertyChanged(nameof(FormattingStates));
            }
        }

        public void ToggleItalic()
        {
            if (CurrentRun != null)
            {
                CurrentRun.Format.Italic = !CurrentRun.Format.Italic;
                FormattingStates["Italic"] = CurrentRun.Format.Italic;
                OnPropertyChanged(nameof(FormattingStates));
            }
        }

        public void ToggleUnderline()
        {
            if (CurrentRun != null)
            {
                CurrentRun.Format.Underline = !CurrentRun.Format.Underline;
                FormattingStates["Underline"] = CurrentRun.Format.Underline;
                OnPropertyChanged(nameof(FormattingStates));
            }
        }

        public void ToggleStrikethrough()
        {
            if (CurrentRun != null)
            {
                CurrentRun.Format.Strikethrough = !CurrentRun.Format.Strikethrough;
                FormattingStates["Strikethrough"] = CurrentRun.Format.Strikethrough;
                OnPropertyChanged(nameof(FormattingStates));
            }
        }

        public void ToggleSubscript()
        {
            if (CurrentRun != null)
            {
                CurrentRun.Format.Subscript = !CurrentRun.Format.Subscript;
                CurrentRun.Format.Superscript = false;
                FormattingStates["Subscript"] = CurrentRun.Format.Subscript;
                FormattingStates["Superscript"] = false;
                OnPropertyChanged(nameof(FormattingStates));
            }
        }

        public void ToggleSuperscript()
        {
            if (CurrentRun != null)
            {
                CurrentRun.Format.Superscript = !CurrentRun.Format.Superscript;
                CurrentRun.Format.Subscript = false;
                FormattingStates["Superscript"] = CurrentRun.Format.Superscript;
                FormattingStates["Subscript"] = false;
                OnPropertyChanged(nameof(FormattingStates));
            }
        }

        public void ShowFindDialog()
        {
        }

        public void ShowReplaceDialog()
        {
        }

        public void InsertPageBreak()
        {
        }

        public void InsertTable()
        {
        }

        public void InsertImage()
        {
        }

        public void SetAlignment(TextAlignment alignment)
        {
            if (CurrentParagraph != null)
            {
                CurrentParagraph.Format.Alignment = alignment;
                OnPropertyChanged(nameof(ParagraphFormat));
            }
        }

        public void IncreaseIndent()
        {
            if (CurrentParagraph != null)
            {
                CurrentParagraph.Format.LeftIndent += 36;
                OnPropertyChanged(nameof(ParagraphFormat));
            }
        }

        public void DecreaseIndent()
        {
            if (CurrentParagraph != null && CurrentParagraph.Format.LeftIndent >= 36)
            {
                CurrentParagraph.Format.LeftIndent -= 36;
                OnPropertyChanged(nameof(ParagraphFormat));
            }
        }

        public void SetBulletList()
        {
        }

        public void SetNumberList()
        {
        }

        public void ApplyStyle(string styleName)
        {
            var style = Document.Styles.Styles.FirstOrDefault(s => s.Name == styleName);
            if (style != null && CurrentParagraph != null)
            {
                CurrentParagraph.Format = style.Format.Clone() as ParagraphFormat;
            }
        }

        public void InsertText(string text)
        {
            if (CurrentParagraph == null)
            {
                CurrentParagraph = new Paragraph();
                Document.Paragraphs.Add(CurrentParagraph);
            }

            var run = new Run { Text = text };
            if (SelectionFormat != null)
            {
                run.Format = SelectionFormat.Clone() as CharacterFormat;
            }
            CurrentParagraph.Runs.Add(run);
            OnPropertyChanged(nameof(Document));
        }

        public void FormatSelection(Action<CharacterFormat> formatAction)
        {
            if (CurrentRun != null && formatAction != null)
            {
                formatAction(CurrentRun.Format);
                OnPropertyChanged(nameof(CurrentRun));
            }
        }

        public void SetFontFamily(string fontFamily)
        {
            FormatSelection(f => f.FontFamily = fontFamily);
        }

        public void SetFontSize(double size)
        {
            FormatSelection(f => f.FontSize = size);
        }

        public void SetFontColor(System.Windows.Media.Color color)
        {
            FormatSelection(f => f.FontColor = color);
        }

        public void SetHighlight(HighlightColor highlight)
        {
            FormatSelection(f => f.Highlight = highlight);
        }

        public override void Initialize()
        {
            NewDocument();
        }

        public override void Cleanup()
        {
            Document = null;
            CurrentParagraph = null;
            CurrentRun = null;
            UndoRedoManager.Clear();
        }
    }

    public class UndoRedoManager
    {
        private readonly Stack<IUndoableAction> _undoStack = new Stack<IUndoableAction>();
        private readonly Stack<IUndoableAction> _redoStack = new Stack<IUndoableAction>();
        private const int MaxUndoLevels = 100;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Execute(IUndoableAction action)
        {
            action.Execute();
            _undoStack.Push(action);
            _redoStack.Clear();

            while (_undoStack.Count > MaxUndoLevels)
            {
                var items = _undoStack.ToArray();
                _undoStack.Clear();
                for (int i = 0; i < items.Length - 1; i++)
                    _undoStack.Push(items[items.Length - 1 - i]);
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var action = _undoStack.Pop();
                action.Undo();
                _redoStack.Push(action);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var action = _redoStack.Pop();
                action.Execute();
                _undoStack.Push(action);
            }
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }

    public interface IUndoableAction
    {
        void Execute();
        void Undo();
    }

    public class TextChangeAction : IUndoableAction
    {
        private readonly Action _doAction;
        private readonly Action _undoAction;

        public TextChangeAction(Action doAction, Action undoAction)
        {
            _doAction = doAction;
            _undoAction = undoAction;
        }

        public void Execute() => _doAction?.Invoke();
        public void Undo() => _undoAction?.Invoke();
    }

    public class FindReplaceManager
    {
        private string _searchText;
        private string _replaceText;
        private FindOptions _options;
        private int _currentMatchIndex;
        private List<FindMatch> _matches;

        public string SearchText
        {
            get => _searchText;
            set => _searchText = value;
        }

        public string ReplaceText
        {
            get => _replaceText;
            set => _replaceText = value;
        }

        public FindOptions Options
        {
            get => _options;
            set => _options = value;
        }

        public int CurrentMatchIndex
        {
            get => _currentMatchIndex;
            set => _currentMatchIndex = value;
        }

        public List<FindMatch> Matches
        {
            get => _matches ??= new List<FindMatch>();
            set => _matches = value;
        }

        public FindReplaceManager()
        {
            _options = new FindOptions();
            _matches = new List<FindMatch>();
        }

        public void Find(string text, FindOptions options)
        {
            _searchText = text;
            _options = options;
            _matches.Clear();
            _currentMatchIndex = 0;
        }

        public FindMatch FindNext()
        {
            if (_matches.Count == 0) return null;
            _currentMatchIndex = (_currentMatchIndex + 1) % _matches.Count;
            return _matches[_currentMatchIndex];
        }

        public FindMatch FindPrevious()
        {
            if (_matches.Count == 0) return null;
            _currentMatchIndex = (_currentMatchIndex - 1 + _matches.Count) % _matches.Count;
            return _matches[_currentMatchIndex];
        }

        public int ReplaceCurrent(string replacement)
        {
            if (_currentMatchIndex < 0 || _currentMatchIndex >= _matches.Count)
                return 0;

            var match = _matches[_currentMatchIndex];
            return ReplaceMatch(match, replacement);
        }

        public int ReplaceAll(string replacement)
        {
            int count = 0;
            foreach (var match in _matches)
            {
                count += ReplaceMatch(match, replacement);
            }
            return count;
        }

        private int ReplaceMatch(FindMatch match, string replacement)
        {
            return 1;
        }

        public void Clear()
        {
            _matches.Clear();
            _currentMatchIndex = -1;
        }
    }

    public class FindOptions
    {
        public bool MatchCase { get; set; }
        public bool MatchWholeWord { get; set; }
        public bool UseWildcards { get; set; }
        public bool SearchUp { get; set; }
        public bool SearchInSelection { get; set; }

        public FindOptions()
        {
            MatchCase = false;
            MatchWholeWord = false;
            UseWildcards = false;
            SearchUp = false;
            SearchInSelection = false;
        }
    }

    public class FindMatch
    {
        public int StartIndex { get; set; }
        public int Length { get; set; }
        public string Text { get; set; }
        public int ParagraphIndex { get; set; }
    }
}

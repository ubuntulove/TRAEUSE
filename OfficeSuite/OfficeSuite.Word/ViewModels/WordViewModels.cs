using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using OfficeSuite.Word.Models;

namespace OfficeSuite.Word.ViewModels
{
    public class WordViewModel : INotifyPropertyChanged
    {
        private string _title = "Untitled Document";
        private string _content = string.Empty;
        private bool _isModified;
        private string _selectedText = string.Empty;
        private int _cursorPosition;
        private bool _isReadOnly;
        private bool _trackChanges;
        private double _zoomLevel = 100;
        private WordViewMode _viewMode = WordViewMode.Edit;

        public event PropertyChangedEventHandler? PropertyChanged;

        public DocumentViewModel Document { get; }
        public FormattingViewModel Formatting { get; }
        public ParagraphViewModel Paragraph { get; }
        public HeaderFooterViewModel HeaderFooter { get; }
        public TableOfContentsViewModel TableOfContents { get; }
        public FindReplaceViewModel FindReplace { get; }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string Content
        {
            get => _content;
            set { _content = value; OnPropertyChanged(nameof(Content)); }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(nameof(IsModified)); }
        }

        public string SelectedText
        {
            get => _selectedText;
            set { _selectedText = value; OnPropertyChanged(nameof(SelectedText)); }
        }

        public int CursorPosition
        {
            get => _cursorPosition;
            set { _cursorPosition = value; OnPropertyChanged(nameof(CursorPosition)); }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); }
        }

        public bool TrackChanges
        {
            get => _trackChanges;
            set { _trackChanges = value; OnPropertyChanged(nameof(TrackChanges)); }
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set { _zoomLevel = value; OnPropertyChanged(nameof(ZoomLevel)); }
        }

        public WordViewMode ViewMode
        {
            get => _viewMode;
            set { _viewMode = value; OnPropertyChanged(nameof(ViewMode)); }
        }

        public ObservableCollection<string> RecentDocuments { get; } = new();
        public ObservableCollection<Style> Styles { get; } = new();

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand SelectAllCommand { get; }

        public WordViewModel()
        {
            Document = new DocumentViewModel();
            Formatting = new FormattingViewModel();
            Paragraph = new ParagraphViewModel();
            HeaderFooter = new HeaderFooterViewModel();
            TableOfContents = new TableOfContentsViewModel();
            FindReplace = new FindReplaceViewModel();

            NewCommand = new RelayCommand(ExecuteNew);
            OpenCommand = new RelayCommand<string>(ExecuteOpen);
            SaveCommand = new RelayCommand<string>(ExecuteSave);
            SaveAsCommand = new RelayCommand<string>(ExecuteSaveAs);
            PrintCommand = new RelayCommand(ExecutePrint);
            UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo, CanExecuteRedo);
            CutCommand = new RelayCommand(ExecuteCut, CanExecuteCut);
            CopyCommand = new RelayCommand(ExecuteCopy, CanExecuteCopy);
            PasteCommand = new RelayCommand(ExecutePaste, CanExecutePaste);
            FindCommand = new RelayCommand(ExecuteFind);
            ReplaceCommand = new RelayCommand(ExecuteReplace);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);

            InitializeDefaultStyles();
        }

        private void InitializeDefaultStyles()
        {
            Styles.Add(new Style { Name = "Normal", Type = StyleType.Character, IsDefault = true });
            Styles.Add(new Style { Name = "Heading 1", Type = StyleType.Paragraph, BaseStyle = "Normal" });
            Styles.Add(new Style { Name = "Heading 2", Type = StyleType.Paragraph, BaseStyle = "Normal" });
            Styles.Add(new Style { Name = "Heading 3", Type = StyleType.Paragraph, BaseStyle = "Normal" });
            Styles.Add(new Style { Name = "Title", Type = StyleType.Paragraph, BaseStyle = "Normal" });
            Styles.Add(new Style { Name = "Subtitle", Type = StyleType.Paragraph, BaseStyle = "Normal" });
            Styles.Add(new Style { Name = "Quote", Type = StyleType.Paragraph, BaseStyle = "Normal" });
            Styles.Add(new Style { Name = "Code", Type = StyleType.Character, BaseStyle = "Normal" });
        }

        private void ExecuteNew()
        {
            Title = "Untitled Document";
            Content = string.Empty;
            IsModified = false;
            Document.Reset();
        }

        private void ExecuteOpen(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Title = System.IO.Path.GetFileName(path);
                RecentDocuments.Insert(0, path);
                if (RecentDocuments.Count > 10)
                    RecentDocuments.RemoveAt(10);
            }
        }

        private void ExecuteSave(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Title = System.IO.Path.GetFileName(path);
                IsModified = false;
            }
        }

        private void ExecuteSaveAs(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                Title = System.IO.Path.GetFileName(path);
                IsModified = false;
            }
        }

        private void ExecutePrint()
        {
        }

        private void ExecuteUndo()
        {
        }

        private bool CanExecuteUndo() => true;

        private void ExecuteRedo()
        {
        }

        private bool CanExecuteRedo() => true;

        private void ExecuteCut()
        {
        }

        private bool CanExecuteCut() => !string.IsNullOrEmpty(SelectedText);

        private void ExecuteCopy()
        {
        }

        private bool CanExecuteCopy() => !string.IsNullOrEmpty(SelectedText);

        private void ExecutePaste()
        {
        }

        private bool CanExecutePaste() => true;

        private void ExecuteFind()
        {
        }

        private void ExecuteReplace()
        {
        }

        private void ExecuteSelectAll()
        {
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum WordViewMode
    {
        Edit,
        Print,
        WebLayout,
        Outline,
        Draft
    }

    public class DocumentViewModel : INotifyPropertyChanged
    {
        private string _filePath = string.Empty;
        private string _template = "Normal.dotx";
        private int _wordCount;
        private int _characterCount;
        private int _paragraphCount;
        private int _pageCount;
        private int _lineCount;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        public string Template
        {
            get => _template;
            set { _template = value; OnPropertyChanged(nameof(Template)); }
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

        public int PageCount
        {
            get => _pageCount;
            set { _pageCount = value; OnPropertyChanged(nameof(PageCount)); }
        }

        public int LineCount
        {
            get => _lineCount;
            set { _lineCount = value; OnPropertyChanged(nameof(LineCount)); }
        }

        public void Reset()
        {
            FilePath = string.Empty;
            WordCount = 0;
            CharacterCount = 0;
            ParagraphCount = 0;
            PageCount = 0;
            LineCount = 0;
        }

        public void UpdateStatistics(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                Reset();
                return;
            }

            WordCount = content.Split(new[] { ' ', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Length;
            CharacterCount = content.Replace(" ", "").Replace("\n", "").Replace("\r", "").Length;
            ParagraphCount = content.Split(new[] { "\n\n" }, StringSplitOptions.RemoveEmptyEntries).Length;
            LineCount = content.Split('\n').Length;
            PageCount = Math.Max(1, (LineCount + 29) / 30);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FormattingViewModel : INotifyPropertyChanged
    {
        private string _fontFamily = "Calibri";
        private double _fontSize = 11;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderline;
        private bool _isStrikethrough;
        private bool _isSubscript;
        private bool _isSuperscript;
        private string _textColor = "#000000";
        private string _highlightColor = "None";
        private TextAlignment _textAlignment = TextAlignment.Left;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FontFamily
        {
            get => _fontFamily;
            set { _fontFamily = value; OnPropertyChanged(nameof(FontFamily)); }
        }

        public double FontSize
        {
            get => _fontSize;
            set { _fontSize = value; OnPropertyChanged(nameof(FontSize)); }
        }

        public bool IsBold
        {
            get => _isBold;
            set { _isBold = value; OnPropertyChanged(nameof(IsBold)); }
        }

        public bool IsItalic
        {
            get => _isItalic;
            set { _isItalic = value; OnPropertyChanged(nameof(IsItalic)); }
        }

        public bool IsUnderline
        {
            get => _isUnderline;
            set { _isUnderline = value; OnPropertyChanged(nameof(IsUnderline)); }
        }

        public bool IsStrikethrough
        {
            get => _isStrikethrough;
            set { _isStrikethrough = value; OnPropertyChanged(nameof(IsStrikethrough)); }
        }

        public bool IsSubscript
        {
            get => _isSubscript;
            set { _isSubscript = value; OnPropertyChanged(nameof(IsSubscript)); }
        }

        public bool IsSuperscript
        {
            get => _isSuperscript;
            set { _isSuperscript = value; OnPropertyChanged(nameof(IsSuperscript)); }
        }

        public string TextColor
        {
            get => _textColor;
            set { _textColor = value; OnPropertyChanged(nameof(TextColor)); }
        }

        public string HighlightColor
        {
            get => _highlightColor;
            set { _highlightColor = value; OnPropertyChanged(nameof(HighlightColor)); }
        }

        public TextAlignment TextAlignment
        {
            get => _textAlignment;
            set { _textAlignment = value; OnPropertyChanged(nameof(TextAlignment)); }
        }

        public List<string> AvailableFonts { get; } = new()
        {
            "Calibri", "Arial", "Times New Roman", "Verdana", "Tahoma", "Georgia", "Courier New", "Comic Sans MS"
        };

        public List<double> AvailableFontSizes { get; } = new()
        {
            8, 9, 10, 11, 12, 14, 16, 18, 20, 22, 24, 26, 28, 36, 48, 72
        };

        public List<string> HighlightColors { get; } = new()
        {
            "None", "Yellow", "Green", "Cyan", "Pink", "Blue", "Red", "Orange"
        };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ParagraphViewModel : INotifyPropertyChanged
    {
        private int _leftIndent;
        private int _rightIndent;
        private int _beforeSpacing;
        private int _afterSpacing;
        private int _lineSpacing;
        private double _lineSpacingMultiple = 1;
        private bool _keepWithNext;
        private bool _keepTogether;
        private bool _pageBreakBefore;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int LeftIndent
        {
            get => _leftIndent;
            set { _leftIndent = value; OnPropertyChanged(nameof(LeftIndent)); }
        }

        public int RightIndent
        {
            get => _rightIndent;
            set { _rightIndent = value; OnPropertyChanged(nameof(RightIndent)); }
        }

        public int BeforeSpacing
        {
            get => _beforeSpacing;
            set { _beforeSpacing = value; OnPropertyChanged(nameof(BeforeSpacing)); }
        }

        public int AfterSpacing
        {
            get => _afterSpacing;
            set { _afterSpacing = value; OnPropertyChanged(nameof(AfterSpacing)); }
        }

        public int LineSpacing
        {
            get => _lineSpacing;
            set { _lineSpacing = value; OnPropertyChanged(nameof(LineSpacing)); }
        }

        public double LineSpacingMultiple
        {
            get => _lineSpacingMultiple;
            set { _lineSpacingMultiple = value; OnPropertyChanged(nameof(LineSpacingMultiple)); }
        }

        public bool KeepWithNext
        {
            get => _keepWithNext;
            set { _keepWithNext = value; OnPropertyChanged(nameof(KeepWithNext)); }
        }

        public bool KeepTogether
        {
            get => _keepTogether;
            set { _keepTogether = value; OnPropertyChanged(nameof(KeepTogether)); }
        }

        public bool PageBreakBefore
        {
            get => _pageBreakBefore;
            set { _pageBreakBefore = value; OnPropertyChanged(nameof(PageBreakBefore)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HeaderFooterViewModel : INotifyPropertyChanged
    {
        private bool _hasDifferentFirstPage;
        private bool _hasDifferentOddEvenPages;
        private string _firstPageHeader = string.Empty;
        private string _firstPageFooter = string.Empty;
        private string _oddPageHeader = string.Empty;
        private string _oddPageFooter = string.Empty;
        private string _evenPageHeader = string.Empty;
        private string _evenPageFooter = string.Empty;

        public event PropertyChangedEventChangedEventArgs? PropertyChanged;

        public bool HasDifferentFirstPage
        {
            get => _hasDifferentFirstPage;
            set { _hasDifferentFirstPage = value; OnPropertyChanged(nameof(HasDifferentFirstPage)); }
        }

        public bool HasDifferentOddEvenPages
        {
            get => _hasDifferentOddEvenPages;
            set { _hasDifferentOddEvenPages = value; OnPropertyChanged(nameof(HasDifferentOddEvenPages)); }
        }

        public string FirstPageHeader
        {
            get => _firstPageHeader;
            set { _firstPageHeader = value; OnPropertyChanged(nameof(FirstPageHeader)); }
        }

        public string FirstPageFooter
        {
            get => _firstPageFooter;
            set { _firstPageFooter = value; OnPropertyChanged(nameof(FirstPageFooter)); }
        }

        public string OddPageHeader
        {
            get => _oddPageHeader;
            set { _oddPageHeader = value; OnPropertyChanged(nameof(OddPageHeader)); }
        }

        public string OddPageFooter
        {
            get => _oddPageFooter;
            set { _oddPageFooter = value; OnPropertyChanged(nameof(OddPageFooter)); }
        }

        public string EvenPageHeader
        {
            get => _evenPageHeader;
            set { _evenPageHeader = value; OnPropertyChanged(nameof(EvenPageHeader)); }
        }

        public string EvenPageFooter
        {
            get => _evenPageFooter;
            set { _evenPageFooter = value; OnPropertyChanged(nameof(EvenPageFooter)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TableOfContentsViewModel : INotifyPropertyChanged
    {
        private bool _useOutlineStyles = true;
        private bool _includePageNumbers = true;
        private bool _includeHyperlinks = true;
        private int _tabLeader = 1;
        private int _rightTabPosition = 9360;
        private int _fromHeadingLevel = 1;
        private int _toHeadingLevel = 3;
        private bool _showLevelNumbers;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool UseOutlineStyles
        {
            get => _useOutlineStyles;
            set { _useOutlineStyles = value; OnPropertyChanged(nameof(UseOutlineStyles)); }
        }

        public bool IncludePageNumbers
        {
            get => _includePageNumbers;
            set { _includePageNumbers = value; OnPropertyChanged(nameof(IncludePageNumbers)); }
        }

        public bool IncludeHyperlinks
        {
            get => _includeHyperlinks;
            set { _includeHyperlinks = value; OnPropertyChanged(nameof(IncludeHyperlinks)); }
        }

        public int TabLeader
        {
            get => _tabLeader;
            set { _tabLeader = value; OnPropertyChanged(nameof(TabLeader)); }
        }

        public int RightTabPosition
        {
            get => _rightTabPosition;
            set { _rightTabPosition = value; OnPropertyChanged(nameof(RightTabPosition)); }
        }

        public int FromHeadingLevel
        {
            get => _fromHeadingLevel;
            set { _fromHeadingLevel = value; OnPropertyChanged(nameof(FromHeadingLevel)); }
        }

        public int ToHeadingLevel
        {
            get => _toHeadingLevel;
            set { _toHeadingLevel = value; OnPropertyChanged(nameof(ToHeadingLevel)); }
        }

        public bool ShowLevelNumbers
        {
            get => _showLevelNumbers;
            set { _showLevelNumbers = value; OnPropertyChanged(nameof(ShowLevelNumbers)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class FindReplaceViewModel : INotifyPropertyChanged
    {
        private string _findText = string.Empty;
        private string _replaceText = string.Empty;
        private bool _matchCase;
        private bool _matchWholeWord;
        private bool _useWildcards;
        private bool _searchUp;
        private int _foundCount;
        private int _currentMatch;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string FindText
        {
            get => _findText;
            set { _findText = value; OnPropertyChanged(nameof(FindText)); }
        }

        public string ReplaceText
        {
            get => _replaceText;
            set { _replaceText = value; OnPropertyChanged(nameof(ReplaceText)); }
        }

        public bool MatchCase
        {
            get => _matchCase;
            set { _matchCase = value; OnPropertyChanged(nameof(MatchCase)); }
        }

        public bool MatchWholeWord
        {
            get => _matchWholeWord;
            set { _matchWholeWord = value; OnPropertyChanged(nameof(MatchWholeWord)); }
        }

        public bool UseWildcards
        {
            get => _useWildcards;
            set { _useWildcards = value; OnPropertyChanged(nameof(UseWildcards)); }
        }

        public bool SearchUp
        {
            get => _searchUp;
            set { _searchUp = value; OnPropertyChanged(nameof(SearchUp)); }
        }

        public int FoundCount
        {
            get => _foundCount;
            set { _foundCount = value; OnPropertyChanged(nameof(FoundCount)); }
        }

        public int CurrentMatch
        {
            get => _currentMatch;
            set { _currentMatch = value; OnPropertyChanged(nameof(CurrentMatch)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _execute;
        private readonly Func<T?, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<T?> execute, Func<T?, bool>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke((T?)parameter) ?? true;
        public void Execute(object? parameter) => _execute((T?)parameter);
    }
}

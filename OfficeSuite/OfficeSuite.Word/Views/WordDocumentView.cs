using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfficeSuite.Word.Views
{
    public class WordDocumentView : INotifyPropertyChanged
    {
        private string _content = string.Empty;
        private bool _showRuler = true;
        private bool _showStatusBar = true;
        private bool _isReadOnly;
        private double _zoomLevel = 100;
        private WordViewMode _viewMode = WordViewMode.Edit;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<TextChangedEventArgs>? TextChanged;
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;
        public event EventHandler<CaretPositionChangedEventArgs>? CaretPositionChanged;

        public string Content
        {
            get => _content;
            set
            {
                _content = value;
                OnPropertyChanged(nameof(Content));
                TextChanged?.Invoke(this, new TextChangedEventArgs { Text = value });
            }
        }

        public bool ShowRuler
        {
            get => _showRuler;
            set { _showRuler = value; OnPropertyChanged(nameof(ShowRuler)); }
        }

        public bool ShowStatusBar
        {
            get => _showStatusBar;
            set { _showStatusBar = value; OnPropertyChanged(nameof(ShowStatusBar)); }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); }
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

        public string SelectedText { get; private set; } = string.Empty;
        public int CaretLine { get; private set; } = 1;
        public int CaretColumn { get; private set; } = 1;

        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }

        public WordDocumentView()
        {
            UndoCommand = new RelayCommand(ExecuteUndo, CanExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo, CanExecuteRedo);
            CutCommand = new RelayCommand(ExecuteCut, CanExecuteCut);
            CopyCommand = new RelayCommand(ExecuteCopy, CanExecuteCopy);
            PasteCommand = new RelayCommand(ExecutePaste);
            SelectAllCommand = new RelayCommand(ExecuteSelectAll);
            FindCommand = new RelayCommand(ExecuteFind);
            ReplaceCommand = new RelayCommand(ExecuteReplace);
        }

        public void SetSelection(int start, int length)
        {
            if (start >= 0 && start + length <= _content.Length)
            {
                SelectedText = _content.Substring(start, length);
                SelectionChanged?.Invoke(this, new SelectionChangedEventArgs
                {
                    Start = start,
                    Length = length,
                    SelectedText = SelectedText
                });
            }
        }

        public void SetCaretPosition(int line, int column)
        {
            CaretLine = line;
            CaretColumn = column;
            CaretPositionChanged?.Invoke(this, new CaretPositionChangedEventArgs
            {
                Line = line,
                Column = column
            });
        }

        private void ExecuteUndo() { }
        private bool CanExecuteUndo() => true;
        private void ExecuteRedo() { }
        private bool CanExecuteRedo() => true;
        private void ExecuteCut() { }
        private bool CanExecuteCut() => !string.IsNullOrEmpty(SelectedText);
        private void ExecuteCopy() { }
        private bool CanExecuteCopy() => !string.IsNullOrEmpty(SelectedText);
        private void ExecutePaste() { }
        private void ExecuteSelectAll() { }
        private void ExecuteFind() { }
        private void ExecuteReplace() { }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RulerControl : INotifyPropertyChanged
    {
        private double _totalWidth = 800;
        private double _totalHeight = 30;
        private double _leftMargin = 50;
        private double _rightMargin = 50;
        private double _firstLineIndent;
        private double _leftIndent;
        private double _rightIndent;
        private double _tabStop1;
        private double _tabStop2;
        private double _tabStop3;
        private TabAlignment _tabAlignment1 = TabAlignment.Left;
        private TabAlignment _tabAlignment2 = TabAlignment.Left;
        private TabAlignment _tabAlignment3 = TabAlignment.Left;
        private bool _showMinorTicks = true;
        private bool _showMajorTicks = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public double TotalWidth
        {
            get => _totalWidth;
            set { _totalWidth = value; OnPropertyChanged(nameof(TotalWidth)); }
        }

        public double TotalHeight
        {
            get => _totalHeight;
            set { _totalHeight = value; OnPropertyChanged(nameof(TotalHeight)); }
        }

        public double LeftMargin
        {
            get => _leftMargin;
            set { _leftMargin = value; OnPropertyChanged(nameof(LeftMargin)); }
        }

        public double RightMargin
        {
            get => _rightMargin;
            set { _rightMargin = value; OnPropertyChanged(nameof(RightMargin)); }
        }

        public double FirstLineIndent
        {
            get => _firstLineIndent;
            set { _firstLineIndent = value; OnPropertyChanged(nameof(FirstLineIndent)); }
        }

        public double LeftIndent
        {
            get => _leftIndent;
            set { _leftIndent = value; OnPropertyChanged(nameof(LeftIndent)); }
        }

        public double RightIndent
        {
            get => _rightIndent;
            set { _rightIndent = value; OnPropertyChanged(nameof(RightIndent)); }
        }

        public double TabStop1
        {
            get => _tabStop1;
            set { _tabStop1 = value; OnPropertyChanged(nameof(TabStop1)); }
        }

        public double TabStop2
        {
            get => _tabStop2;
            set { _tabStop2 = value; OnPropertyChanged(nameof(TabStop2)); }
        }

        public double TabStop3
        {
            get => _tabStop3;
            set { _tabStop3 = value; OnPropertyChanged(nameof(TabStop3)); }
        }

        public TabAlignment TabAlignment1
        {
            get => _tabAlignment1;
            set { _tabAlignment1 = value; OnPropertyChanged(nameof(TabAlignment1)); }
        }

        public TabAlignment TabAlignment2
        {
            get => _tabAlignment2;
            set { _tabAlignment2 = value; OnPropertyChanged(nameof(TabAlignment2)); }
        }

        public TabAlignment TabAlignment3
        {
            get => _tabAlignment3;
            set { _tabAlignment3 = value; OnPropertyChanged(nameof(TabAlignment3)); }
        }

        public bool ShowMinorTicks
        {
            get => _showMinorTicks;
            set { _showMinorTicks = value; OnPropertyChanged(nameof(ShowMinorTicks)); }
        }

        public bool ShowMajorTicks
        {
            get => _showMajorTicks;
            set { _showMajorTicks = value; OnPropertyChanged(nameof(ShowMajorTicks)); }
        }

        public List<TabStopInfo> TabStops { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class TabStopInfo
    {
        public double Position { get; set; }
        public TabAlignment Alignment { get; set; } = TabAlignment.Left;
        public string? Leader { get; set; }
    }

    public enum TabAlignment
    {
        Left,
        Center,
        Right,
        Decimal,
        Bar
    }

    public class StatusBarControl : INotifyPropertyChanged
    {
        private string _leftMessage = "Ready";
        private string _centerMessage = string.Empty;
        private string _rightMessage = string.Empty;
        private int _pageNumber = 1;
        private int _totalPages = 1;
        private int _lineNumber = 1;
        private int _columnNumber = 1;
        private int _wordCount;
        private bool _trackChangesIndicator;
        private bool _selectionMode;
        private string _selectionInfo = string.Empty;
        private double _zoomLevel = 100;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string LeftMessage
        {
            get => _leftMessage;
            set { _leftMessage = value; OnPropertyChanged(nameof(LeftMessage)); }
        }

        public string CenterMessage
        {
            get => _centerMessage;
            set { _centerMessage = value; OnPropertyChanged(nameof(CenterMessage)); }
        }

        public string RightMessage
        {
            get => _rightMessage;
            set { _rightMessage = value; OnPropertyChanged(nameof(RightMessage)); }
        }

        public int PageNumber
        {
            get => _pageNumber;
            set { _pageNumber = value; OnPropertyChanged(nameof(PageNumber)); }
        }

        public int TotalPages
        {
            get => _totalPages;
            set { _totalPages = value; OnPropertyChanged(nameof(TotalPages)); }
        }

        public int LineNumber
        {
            get => _lineNumber;
            set { _lineNumber = value; OnPropertyChanged(nameof(LineNumber)); }
        }

        public int ColumnNumber
        {
            get => _columnNumber;
            set { _columnNumber = value; OnPropertyChanged(nameof(ColumnNumber)); }
        }

        public int WordCount
        {
            get => _wordCount;
            set { _wordCount = value; OnPropertyChanged(nameof(WordCount)); }
        }

        public bool TrackChangesIndicator
        {
            get => _trackChangesIndicator;
            set { _trackChangesIndicator = value; OnPropertyChanged(nameof(TrackChangesIndicator)); }
        }

        public bool SelectionMode
        {
            get => _selectionMode;
            set { _selectionMode = value; OnPropertyChanged(nameof(SelectionMode)); }
        }

        public string SelectionInfo
        {
            get => _selectionInfo;
            set { _selectionInfo = value; OnPropertyChanged(nameof(SelectionInfo)); }
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set { _zoomLevel = value; OnPropertyChanged(nameof(ZoomLevel)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class MiniToolbarControl : INotifyPropertyChanged
    {
        private bool _isVisible;
        private double _left;
        private double _top;
        private string _selectedFontFamily = "Calibri";
        private double _selectedFontSize = 11;
        private bool _isBold;
        private bool _isItalic;
        private bool _isUnderline;
        private bool _isStrikethrough;
        private string _selectedTextColor = "#000000";
        private string _selectedHighlightColor = "None";
        private TextAlignment _selectedAlignment = TextAlignment.Left;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? DismissRequested;

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public double Left
        {
            get => _left;
            set { _left = value; OnPropertyChanged(nameof(Left)); }
        }

        public double Top
        {
            get => _top;
            set { _top = value; OnPropertyChanged(nameof(Top)); }
        }

        public string SelectedFontFamily
        {
            get => _selectedFontFamily;
            set { _selectedFontFamily = value; OnPropertyChanged(nameof(SelectedFontFamily)); }
        }

        public double SelectedFontSize
        {
            get => _selectedFontSize;
            set { _selectedFontSize = value; OnPropertyChanged(nameof(SelectedFontSize)); }
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

        public string SelectedTextColor
        {
            get => _selectedTextColor;
            set { _selectedTextColor = value; OnPropertyChanged(nameof(SelectedTextColor)); }
        }

        public string SelectedHighlightColor
        {
            get => _selectedHighlightColor;
            set { _selectedHighlightColor = value; OnPropertyChanged(nameof(SelectedHighlightColor)); }
        }

        public TextAlignment SelectedAlignment
        {
            get => _selectedAlignment;
            set { _selectedAlignment = value; OnPropertyChanged(nameof(SelectedAlignment)); }
        }

        public void Show(double left, double top)
        {
            Left = left;
            Top = top;
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
            DismissRequested?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WordViewMode
    {
        public const string Edit = "Edit";
        public const string Print = "Print";
        public const string WebLayout = "WebLayout";
        public const string Outline = "Outline";
        public const string Draft = "Draft";
    }

    public class TextAlignment
    {
        public const string Left = "Left";
        public const string Center = "Center";
        public const string Right = "Right";
        public const string Justify = "Justify";
    }

    public class TextChangedEventArgs : EventArgs
    {
        public string Text { get; set; } = string.Empty;
    }

    public class SelectionChangedEventArgs : EventArgs
    {
        public int Start { get; set; }
        public int Length { get; set; }
        public string SelectedText { get; set; } = string.Empty;
    }

    public class CaretPositionChangedEventArgs : EventArgs
    {
        public int Line { get; set; }
        public int Column { get; set; }
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

    public class CommandManager
    {
        public static event EventHandler? RequerySuggested;
    }

    public class NavigationPaneView : INotifyPropertyChanged
    {
        private NavigationPaneMode _currentMode = NavigationPaneMode.Navigation;
        private string _searchText = string.Empty;
        private bool _isSearching;

        public event PropertyChangedEventHandler? PropertyChanged;

        public NavigationPaneMode CurrentMode
        {
            get => _currentMode;
            set { _currentMode = value; OnPropertyChanged(nameof(CurrentMode)); }
        }

        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(nameof(SearchText)); }
        }

        public bool IsSearching
        {
            get => _isSearching;
            set { _isSearching = value; OnPropertyChanged(nameof(IsSearching)); }
        }

        public ObservableCollection<NavigationItem> NavigationItems { get; } = new();
        public ObservableCollection<SearchResult> SearchResults { get; } = new();
        public ObservableCollection<HeadingItem> Headings { get; } = new();
        public ObservableCollection<ThumbnailItem> Thumbnails { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum NavigationPaneMode
    {
        Navigation,
        Search,
        Headings,
        Thumbnails
    }

    public class NavigationItem
    {
        public string Title { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public NavigationPaneMode Mode { get; set; }
    }

    public class SearchResult
    {
        public string Text { get; set; } = string.Empty;
        public string Context { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public int LineNumber { get; set; }
    }

    public class HeadingItem
    {
        public int Level { get; set; }
        public string Text { get; set; } = string.Empty;
        public int PageNumber { get; set; }
    }

    public class ThumbnailItem
    {
        public int Index { get; set; }
        public int PageNumber { get; set; }
        public bool IsSelected { get; set; }
    }

    public class DocumentOutlineView : INotifyPropertyChanged
    {
        private HeadingItem? _selectedHeading;
        private bool _showAllHeadings = true;
        private bool _showLevel1 = true;
        private bool _showLevel2 = true;
        private bool _showLevel3 = true;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<HeadingSelectedEventArgs>? HeadingSelected;

        public HeadingItem? SelectedHeading
        {
            get => _selectedHeading;
            set
            {
                _selectedHeading = value;
                OnPropertyChanged(nameof(SelectedHeading));
                if (value != null)
                {
                    HeadingSelected?.Invoke(this, new HeadingSelectedEventArgs { Heading = value });
                }
            }
        }

        public bool ShowAllHeadings
        {
            get => _showAllHeadings;
            set { _showAllHeadings = value; OnPropertyChanged(nameof(ShowAllHeadings)); }
        }

        public bool ShowLevel1
        {
            get => _showLevel1;
            set { _showLevel1 = value; OnPropertyChanged(nameof(ShowLevel1)); }
        }

        public bool ShowLevel2
        {
            get => _showLevel2;
            set { _showLevel2 = value; OnPropertyChanged(nameof(ShowLevel2)); }
        }

        public bool ShowLevel3
        {
            get => _showLevel3;
            set { _showLevel3 = value; OnPropertyChanged(nameof(ShowLevel3)); }
        }

        public ObservableCollection<HeadingItem> Headings { get; } = new();

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class HeadingSelectedEventArgs : EventArgs
    {
        public HeadingItem? Heading { get; set; }
    }
}

namespace System.Collections.ObjectModel
{
    public class ObservableCollection<T> : List<T>
    {
        public event EventHandler? CollectionChanged;

        public new void Add(T item)
        {
            base.Add(item);
            OnCollectionChanged();
        }

        public new void Remove(T item)
        {
            base.Remove(item);
            OnCollectionChanged();
        }

        public new void Clear()
        {
            base.Clear();
            OnCollectionChanged();
        }

        protected virtual void OnCollectionChanged()
        {
            CollectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

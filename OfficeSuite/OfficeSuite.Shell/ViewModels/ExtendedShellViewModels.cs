using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace OfficeSuite.Shell.ViewModels
{
    public class ShellViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _currentDocumentTitle;
        private bool _isDocumentModified;
        private bool _isApplicationBusy;
        private string _statusMessage;
        private int _selectedTabIndex;
        private DocumentViewModel _activeDocument;
        private WindowState _windowState;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string CurrentDocumentTitle
        {
            get => _currentDocumentTitle;
            set => SetProperty(ref _currentDocumentTitle, value);
        }

        public bool IsDocumentModified
        {
            get => _isDocumentModified;
            set
            {
                if (SetProperty(ref _isDocumentModified, value))
                    UpdateTitle();
            }
        }

        public bool IsApplicationBusy
        {
            get => _isApplicationBusy;
            set => SetProperty(ref _isApplicationBusy, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public DocumentViewModel ActiveDocument
        {
            get => _activeDocument;
            set => SetProperty(ref _activeDocument, value);
        }

        public WindowState WindowState
        {
            get => _windowState;
            set => SetProperty(ref _windowState, value);
        }

        public ObservableCollection<DocumentViewModel> OpenDocuments { get; }
        public ObservableCollection<RecentFileViewModel> RecentFiles { get; }
        public ObservableCollection<WorkspaceViewModel> Workspaces { get; }
        public RibbonViewModel Ribbon { get; }
        public StatusBarViewModel StatusBar { get; }
        public NavigationViewModel Navigation { get; }
        public QuickAccessToolbarViewModel QuickAccessToolbar { get; }
        public CommandManagerViewModel CommandManager { get; }

        public ICommand NewDocumentCommand { get; }
        public ICommand OpenDocumentCommand { get; }
        public ICommand SaveDocumentCommand { get; }
        public ICommand SaveDocumentAsCommand { get; }
        public ICommand CloseDocumentCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand OptionsCommand { get; }
        public ICommand AboutCommand { get; }

        public ShellViewModel()
        {
            _title = "OfficeSuite";
            _statusMessage = "Ready";
            _windowState = WindowState.Normal;

            OpenDocuments = new ObservableCollection<DocumentViewModel>();
            RecentFiles = new ObservableCollection<RecentFileViewModel>();
            Workspaces = new ObservableCollection<WorkspaceViewModel>();
            Ribbon = new RibbonViewModel();
            StatusBar = new StatusBarViewModel();
            Navigation = new NavigationViewModel();
            QuickAccessToolbar = new QuickAccessToolbarViewModel();
            CommandManager = new CommandManagerViewModel();

            NewDocumentCommand = new RelayCommand(_ => NewDocument());
            OpenDocumentCommand = new RelayCommand(_ => OpenDocument());
            SaveDocumentCommand = new RelayCommand(_ => SaveDocument(), _ => CanSaveDocument());
            SaveDocumentAsCommand = new RelayCommand(_ => SaveDocumentAs());
            CloseDocumentCommand = new RelayCommand(_ => CloseDocument(), _ => CanCloseDocument());
            PrintCommand = new RelayCommand(_ => Print(), _ => CanPrint());
            ExportCommand = new RelayCommand(_ => Export());
            ExitCommand = new RelayCommand(_ => Exit());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo());
            CutCommand = new RelayCommand(_ => Cut(), _ => CanCut());
            CopyCommand = new RelayCommand(_ => Copy(), _ => CanCopy());
            PasteCommand = new RelayCommand(_ => Paste(), _ => CanPaste());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => CanDelete());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            FindCommand = new RelayCommand(_ => Find(), _ => CanFind());
            ReplaceCommand = new RelayCommand(_ => Replace(), _ => CanReplace());
            OptionsCommand = new RelayCommand(_ => ShowOptions());
            AboutCommand = new RelayCommand(_ => ShowAbout());

            UpdateTitle();
        }

        private void UpdateTitle()
        {
            var title = "OfficeSuite";
            if (!string.IsNullOrEmpty(_currentDocumentTitle))
            {
                title = $"{_currentDocumentTitle} - {title}";
                if (_isDocumentModified)
                    title = $"*{title}";
            }
            Title = title;
        }

        public void NewDocument()
        {
            var document = new DocumentViewModel
            {
                Title = $"Untitled {OpenDocuments.Count + 1}",
                DocumentType = DocumentType.Word
            };
            OpenDocuments.Add(document);
            ActiveDocument = document;
            CurrentDocumentTitle = document.Title;
            StatusMessage = "New document created";
        }

        public void OpenDocument()
        {
            StatusMessage = "Opening document...";
        }

        public void SaveDocument()
        {
            if (ActiveDocument != null)
            {
                ActiveDocument.Save();
                IsDocumentModified = false;
                StatusMessage = "Document saved";
            }
        }

        private bool CanSaveDocument() => ActiveDocument != null;

        public void SaveDocumentAs()
        {
            StatusMessage = "Save as...";
        }

        public void CloseDocument()
        {
            if (ActiveDocument != null)
            {
                OpenDocuments.Remove(ActiveDocument);
                ActiveDocument = OpenDocuments.FirstOrDefault();
                CurrentDocumentTitle = ActiveDocument?.Title;
            }
        }

        private bool CanCloseDocument() => ActiveDocument != null;

        public void Print()
        {
            StatusMessage = "Printing...";
        }

        private bool CanPrint() => ActiveDocument != null;

        public void Export()
        {
            StatusMessage = "Exporting...";
        }

        public void Exit()
        {
            System.Windows.Application.Current.Shutdown();
        }

        public void Undo()
        {
            CommandManager.Undo();
            StatusMessage = "Undo";
        }

        private bool CanUndo() => CommandManager.CanUndo;

        public void Redo()
        {
            CommandManager.Redo();
            StatusMessage = "Redo";
        }

        private bool CanRedo() => CommandManager.CanRedo;

        public void Cut()
        {
            StatusMessage = "Cut";
        }

        private bool CanCut() => ActiveDocument?.HasSelection == true;

        public void Copy()
        {
            StatusMessage = "Copy";
        }

        private bool CanCopy() => ActiveDocument?.HasSelection == true;

        public void Paste()
        {
            StatusMessage = "Paste";
        }

        private bool CanPaste() => true;

        public void Delete()
        {
            StatusMessage = "Delete";
        }

        private bool CanDelete() => ActiveDocument?.HasSelection == true;

        public void SelectAll()
        {
            StatusMessage = "Select All";
        }

        public void Find()
        {
            StatusMessage = "Find";
        }

        private bool CanFind() => ActiveDocument != null;

        public void Replace()
        {
            StatusMessage = "Replace";
        }

        private bool CanReplace() => ActiveDocument != null;

        public void ShowOptions()
        {
            StatusMessage = "Options";
        }

        public void ShowAbout()
        {
            StatusMessage = "About";
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DocumentViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _title;
        private string _filePath;
        private DocumentType _documentType;
        private bool _isModified;
        private bool _isReadOnly;
        private DateTime _createdDate;
        private DateTime _modifiedDate;
        private byte[] _content;
        private bool _hasSelection;
        private int _selectionStart;
        private int _selectionLength;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }

        public DocumentType DocumentType
        {
            get => _documentType;
            set => SetProperty(ref _documentType, value);
        }

        public bool IsModified
        {
            get => _isModified;
            set => SetProperty(ref _isModified, value);
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => SetProperty(ref _isReadOnly, value);
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public DateTime ModifiedDate
        {
            get => _modifiedDate;
            set => SetProperty(ref _modifiedDate, value);
        }

        public byte[] Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public bool HasSelection
        {
            get => _hasSelection;
            set => SetProperty(ref _hasSelection, value);
        }

        public int SelectionStart
        {
            get => _selectionStart;
            set => SetProperty(ref _selectionStart, value);
        }

        public int SelectionLength
        {
            get => _selectionLength;
            set => SetProperty(ref _selectionLength, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand PrintCommand { get; }

        public DocumentViewModel()
        {
            _id = Guid.NewGuid().ToString();
            _createdDate = DateTime.Now;
            _modifiedDate = DateTime.Now;

            SaveCommand = new RelayCommand(_ => Save());
            SaveAsCommand = new RelayCommand(_ => SaveAs());
            CloseCommand = new RelayCommand(_ => Close());
            PrintCommand = new RelayCommand(_ => Print());
        }

        public void Save()
        {
            _modifiedDate = DateTime.Now;
            IsModified = false;
        }

        public void SaveAs()
        {
            _modifiedDate = DateTime.Now;
        }

        public void Close()
        {
        }

        public void Print()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonViewModel : INotifyPropertyChanged
    {
        private int _selectedTabIndex;
        private bool _isMinimized;
        private bool _isExpanded;

        public int SelectedTabIndex
        {
            get => _selectedTabIndex;
            set => SetProperty(ref _selectedTabIndex, value);
        }

        public bool IsMinimized
        {
            get => _isMinimized;
            set => SetProperty(ref _isMinimized, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public ObservableCollection<RibbonTabViewModel> Tabs { get; }

        public RibbonViewModel()
        {
            Tabs = new ObservableCollection<RibbonTabViewModel>();
            InitializeDefaultTabs();
        }

        private void InitializeDefaultTabs()
        {
            Tabs.Add(new RibbonTabViewModel { Header = "File" });
            Tabs.Add(new RibbonTabViewModel { Header = "Home" });
            Tabs.Add(new RibbonTabViewModel { Header = "Insert" });
            Tabs.Add(new RibbonTabViewModel { Header = "Draw" });
            Tabs.Add(new RibbonTabViewModel { Header = "Design" });
            Tabs.Add(new RibbonTabViewModel { Header = "Layout" });
            Tabs.Add(new RibbonTabViewModel { Header = "References" });
            Tabs.Add(new RibbonTabViewModel { Header = "Mailings" });
            Tabs.Add(new RibbonTabViewModel { Header = "Review" });
            Tabs.Add(new RibbonTabViewModel { Header = "View" });
        }

        public void AddTab(string header)
        {
            Tabs.Add(new RibbonTabViewModel { Header = header });
        }

        public void RemoveTab(string header)
        {
            var tab = Tabs.FirstOrDefault(t => t.Header == header);
            if (tab != null)
                Tabs.Remove(tab);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonTabViewModel : INotifyPropertyChanged
    {
        private string _header;
        private bool _isSelected;
        private bool _isVisible;

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public ObservableCollection<RibbonGroupViewModel> Groups { get; }

        public RibbonTabViewModel()
        {
            Groups = new ObservableCollection<RibbonGroupViewModel>();
            _isVisible = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonGroupViewModel : INotifyPropertyChanged
    {
        private string _header;
        private bool _isVisible;

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public ObservableCollection<RibbonButtonViewModel> Buttons { get; }
        public ObservableCollection<RibbonMenuViewModel> Menus { get; }
        public ObservableCollection<RibbonGalleryViewModel> Galleries { get; }

        public RibbonGroupViewModel()
        {
            Buttons = new ObservableCollection<RibbonButtonViewModel>();
            Menus = new ObservableCollection<RibbonMenuViewModel>();
            Galleries = new ObservableCollection<RibbonGalleryViewModel>();
            _isVisible = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonButtonViewModel : INotifyPropertyChanged
    {
        private string _label;
        private string _tooltip;
        private string _imageSource;
        private bool _isEnabled;
        private bool _isChecked;
        private bool _isVisible;
        private ICommand _command;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        public RibbonButtonViewModel()
        {
            _isEnabled = true;
            _isVisible = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonMenuViewModel : INotifyPropertyChanged
    {
        private string _label;
        private string _tooltip;
        private bool _isEnabled;
        private bool _isVisible;
        private bool _isSplitButton;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsSplitButton
        {
            get => _isSplitButton;
            set => SetProperty(ref _isSplitButton, value);
        }

        public ObservableCollection<RibbonMenuItemViewModel> Items { get; }

        public RibbonMenuViewModel()
        {
            Items = new ObservableCollection<RibbonMenuItemViewModel>();
            _isEnabled = true;
            _isVisible = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonMenuItemViewModel : INotifyPropertyChanged
    {
        private string _label;
        private string _imageSource;
        private string _shortcutKeyDisplay;
        private bool _isEnabled;
        private bool _isVisible;
        private bool _isChecked;
        private ICommand _command;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public string ShortcutKeyDisplay
        {
            get => _shortcutKeyDisplay;
            set => SetProperty(ref _shortcutKeyDisplay, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        public ObservableCollection<RibbonMenuItemViewModel> Items { get; }

        public RibbonMenuItemViewModel()
        {
            Items = new ObservableCollection<RibbonMenuItemViewModel>();
            _isEnabled = true;
            _isVisible = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonGalleryViewModel : INotifyPropertyChanged
    {
        private string _label;
        private bool _isEnabled;
        private bool _isVisible;
        private int _columns;
        private int _rows;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public int Columns
        {
            get => _columns;
            set => SetProperty(ref _columns, value);
        }

        public int Rows
        {
            get => _rows;
            set => SetProperty(ref _rows, value);
        }

        public ObservableCollection<RibbonGalleryItemViewModel> Items { get; }

        public RibbonGalleryViewModel()
        {
            Items = new ObservableCollection<RibbonGalleryItemViewModel>();
            _isEnabled = true;
            _isVisible = true;
            _columns = 4;
            _rows = 3;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonGalleryItemViewModel : INotifyPropertyChanged
    {
        private string _label;
        private string _imageSource;
        private bool _isSelected;
        private bool _isEnabled;
        private ICommand _command;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public string ImageSource
        {
            get => _imageSource;
            set => SetProperty(ref _imageSource, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        public RibbonGalleryItemViewModel()
        {
            _isEnabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StatusBarViewModel : INotifyPropertyChanged
    {
        private string _statusMessage;
        private bool _showZoom;
        private int _zoomLevel;
        private bool _showPageNumber;
        private int _currentPage;
        private int _totalPages;
        private int _wordCount;
        private bool _showLanguage;
        private string _language;
        private bool _showLineNumber;
        private int _lineNumber;
        private bool _showColumnNumber;
        private int _columnNumber;
        private bool _showSelectionInfo;
        private int _selectionCount;

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool ShowZoom
        {
            get => _showZoom;
            set => SetProperty(ref _showZoom, value);
        }

        public int ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        public bool ShowPageNumber
        {
            get => _showPageNumber;
            set => SetProperty(ref _showPageNumber, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public int WordCount
        {
            get => _wordCount;
            set => SetProperty(ref _wordCount, value);
        }

        public bool ShowLanguage
        {
            get => _showLanguage;
            set => SetProperty(ref _showLanguage, value);
        }

        public string Language
        {
            get => _language;
            set => SetProperty(ref _language, value);
        }

        public bool ShowLineNumber
        {
            get => _showLineNumber;
            set => SetProperty(ref _showLineNumber, value);
        }

        public int LineNumber
        {
            get => _lineNumber;
            set => SetProperty(ref _lineNumber, value);
        }

        public bool ShowColumnNumber
        {
            get => _showColumnNumber;
            set => SetProperty(ref _showColumnNumber, value);
        }

        public int ColumnNumber
        {
            get => _columnNumber;
            set => SetProperty(ref _columnNumber, value);
        }

        public bool ShowSelectionInfo
        {
            get => _showSelectionInfo;
            set => SetProperty(ref _showSelectionInfo, value);
        }

        public int SelectionCount
        {
            get => _selectionCount;
            set => SetProperty(ref _selectionCount, value);
        }

        public StatusBarViewModel()
        {
            _statusMessage = "Ready";
            _showZoom = true;
            _zoomLevel = 100;
            _showPageNumber = true;
            _showLanguage = true;
            _language = "English (US)";
            _showLineNumber = true;
            _showColumnNumber = true;
            _showSelectionInfo = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NavigationViewModel : INotifyPropertyChanged
    {
        private bool _isVisible;
        private bool _isAutoHide;
        private NavigationPanePosition _position;
        private double _width;
        private string _selectedView;
        private bool _isExpanded;

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsAutoHide
        {
            get => _isAutoHide;
            set => SetProperty(ref _isAutoHide, value);
        }

        public NavigationPanePosition Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public string SelectedView
        {
            get => _selectedView;
            set => SetProperty(ref _selectedView, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public NavigationPaneViewModel NavigationPane { get; }
        public ThumbnailPaneViewModel Thumbnails { get; }
        public OutlinePaneViewModel Outline { get; }
        public SearchPaneViewModel Search { get; }

        public NavigationViewModel()
        {
            _isVisible = true;
            _position = NavigationPanePosition.Left;
            _width = 250;
            _selectedView = "Thumbnails";
            _isExpanded = true;
            NavigationPane = new NavigationPaneViewModel();
            Thumbnails = new ThumbnailPaneViewModel();
            Outline = new OutlinePaneViewModel();
            Search = new SearchPaneViewModel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NavigationPaneViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<NavigationItemViewModel> Items { get; }

        public NavigationPaneViewModel()
        {
            Items = new ObservableCollection<NavigationItemViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class NavigationItemViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _icon;
        private bool _isSelected;
        private bool _isExpanded;
        private ICommand _command;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        public ObservableCollection<NavigationItemViewModel> Children { get; }

        public NavigationItemViewModel()
        {
            Children = new ObservableCollection<NavigationItemViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThumbnailPaneViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<ThumbnailItemViewModel> Thumbnails { get; }

        public ThumbnailPaneViewModel()
        {
            Thumbnails = new ObservableCollection<ThumbnailItemViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ThumbnailItemViewModel : INotifyPropertyChanged
    {
        private string _id;
        private int _pageNumber;
        private byte[] _thumbnail;
        private bool _isSelected;
        private bool _isVisible;
        private bool _isLocked;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public int PageNumber
        {
            get => _pageNumber;
            set => SetProperty(ref _pageNumber, value);
        }

        public byte[] Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public bool IsLocked
        {
            get => _isLocked;
            set => SetProperty(ref _isLocked, value);
        }

        public ThumbnailItemViewModel()
        {
            _isVisible = true;
            _isLocked = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OutlinePaneViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<OutlineItemViewModel> Items { get; }

        public OutlinePaneViewModel()
        {
            Items = new ObservableCollection<OutlineItemViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OutlineItemViewModel : INotifyPropertyChanged
    {
        private string _title;
        private int _level;
        private int _pageNumber;
        private bool _isSelected;
        private bool _isExpanded;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public int Level
        {
            get => _level;
            set => SetProperty(ref _level, value);
        }

        public int PageNumber
        {
            get => _pageNumber;
            set => SetProperty(ref _pageNumber, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public ObservableCollection<OutlineItemViewModel> Children { get; }

        public OutlineItemViewModel()
        {
            Children = new ObservableCollection<OutlineItemViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SearchPaneViewModel : INotifyPropertyChanged
    {
        private string _searchText;
        private bool _matchCase;
        private bool _matchWholeWord;
        private bool _useWildcards;
        private bool _searchUp;
        private bool _searchInHeaders;
        private bool _searchInFooters;
        private int _resultsCount;
        private int _currentResult;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public bool MatchCase
        {
            get => _matchCase;
            set => SetProperty(ref _matchCase, value);
        }

        public bool MatchWholeWord
        {
            get => _matchWholeWord;
            set => SetProperty(ref _matchWholeWord, value);
        }

        public bool UseWildcards
        {
            get => _useWildcards;
            set => SetProperty(ref _useWildcards, value);
        }

        public bool SearchUp
        {
            get => _searchUp;
            set => SetProperty(ref _searchUp, value);
        }

        public bool SearchInHeaders
        {
            get => _searchInHeaders;
            set => SetProperty(ref _searchInHeaders, value);
        }

        public bool SearchInFooters
        {
            get => _searchInFooters;
            set => SetProperty(ref _searchInFooters, value);
        }

        public int ResultsCount
        {
            get => _resultsCount;
            set => SetProperty(ref _resultsCount, value);
        }

        public int CurrentResult
        {
            get => _currentResult;
            set => SetProperty(ref _currentResult, value);
        }

        public ObservableCollection<SearchResultViewModel> Results { get; }

        public SearchPaneViewModel()
        {
            Results = new ObservableCollection<SearchResultViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SearchResultViewModel : INotifyPropertyChanged
    {
        private string _text;
        private int _pageNumber;
        private int _lineNumber;
        private bool _isSelected;
        private string _context;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public int PageNumber
        {
            get => _pageNumber;
            set => SetProperty(ref _pageNumber, value);
        }

        public int LineNumber
        {
            get => _lineNumber;
            set => SetProperty(ref _lineNumber, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string Context
        {
            get => _context;
            set => SetProperty(ref _context, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class QuickAccessToolbarViewModel : INotifyPropertyChanged
    {
        private bool _showBelowRibbon;
        private bool _showAboveRibbon;

        public bool ShowBelowRibbon
        {
            get => _showBelowRibbon;
            set => SetProperty(ref _showBelowRibbon, value);
        }

        public bool ShowAboveRibbon
        {
            get => _showAboveRibbon;
            set => SetProperty(ref _showAboveRibbon, value);
        }

        public ObservableCollection<QuickAccessItemViewModel> Items { get; }

        public QuickAccessToolbarViewModel()
        {
            Items = new ObservableCollection<QuickAccessItemViewModel>();
            InitializeDefaultItems();
        }

        private void InitializeDefaultItems()
        {
            Items.Add(new QuickAccessItemViewModel { Label = "Save", Icon = "Save", Command = new RelayCommand(_ => { }) });
            Items.Add(new QuickAccessItemViewModel { Label = "Undo", Icon = "Undo", Command = new RelayCommand(_ => { }) });
            Items.Add(new QuickAccessItemViewModel { Label = "Redo", Icon = "Redo", Command = new RelayCommand(_ => { }) });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class QuickAccessItemViewModel : INotifyPropertyChanged
    {
        private string _label;
        private string _icon;
        private string _tooltip;
        private bool _isEnabled;
        private ICommand _command;

        public string Label
        {
            get => _label;
            set => SetProperty(ref _label, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string Tooltip
        {
            get => _tooltip;
            set => SetProperty(ref _tooltip, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
        }

        public QuickAccessItemViewModel()
        {
            _isEnabled = true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CommandManagerViewModel : INotifyPropertyChanged
    {
        private bool _canUndo;
        private bool _canRedo;
        private string _lastUndoDescription;
        private string _lastRedoDescription;
        private int _undoCount;
        private int _redoCount;

        public bool CanUndo
        {
            get => _canUndo;
            set => SetProperty(ref _canUndo, value);
        }

        public bool CanRedo
        {
            get => _canRedo;
            set => SetProperty(ref _canRedo, value);
        }

        public string LastUndoDescription
        {
            get => _lastUndoDescription;
            set => SetProperty(ref _lastUndoDescription, value);
        }

        public string LastRedoDescription
        {
            get => _lastRedoDescription;
            set => SetProperty(ref _lastRedoDescription, value);
        }

        public int UndoCount
        {
            get => _undoCount;
            set => SetProperty(ref _undoCount, value);
        }

        public int RedoCount
        {
            get => _redoCount;
            set => SetProperty(ref _redoCount, value);
        }

        public ObservableCollection<CommandHistoryItemViewModel> UndoHistory { get; }
        public ObservableCollection<CommandHistoryItemViewModel> RedoHistory { get; }

        public CommandManagerViewModel()
        {
            UndoHistory = new ObservableCollection<CommandHistoryItemViewModel>();
            RedoHistory = new ObservableCollection<CommandHistoryItemViewModel>();
        }

        public void Undo()
        {
        }

        public void Redo()
        {
        }

        public void ClearHistory()
        {
            UndoHistory.Clear();
            RedoHistory.Clear();
            UndoCount = 0;
            RedoCount = 0;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class CommandHistoryItemViewModel : INotifyPropertyChanged
    {
        private string _description;
        private DateTime _timestamp;
        private string _commandType;

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        public string CommandType
        {
            get => _commandType;
            set => SetProperty(ref _commandType, value);
        }

        public CommandHistoryItemViewModel()
        {
            _timestamp = DateTime.Now;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RecentFileViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _path;
        private string _fileType;
        private DateTime _accessedDate;
        private int _accessCount;
        private bool _isPinned;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Path
        {
            get => _path;
            set => SetProperty(ref _path, value);
        }

        public string FileType
        {
            get => _fileType;
            set => SetProperty(ref _fileType, value);
        }

        public DateTime AccessedDate
        {
            get => _accessedDate;
            set => SetProperty(ref _accessedDate, value);
        }

        public int AccessCount
        {
            get => _accessCount;
            set => SetProperty(ref _accessCount, value);
        }

        public bool IsPinned
        {
            get => _isPinned;
            set => SetProperty(ref _isPinned, value);
        }

        public RecentFileViewModel()
        {
            _id = Guid.NewGuid().ToString();
            _accessedDate = DateTime.Now;
            _accessCount = 1;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class WorkspaceViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _description;
        private bool _isActive;
        private bool _isDefault;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public bool IsDefault
        {
            get => _isDefault;
            set => SetProperty(ref _isDefault, value);
        }

        public ObservableCollection<DocumentViewModel> Documents { get; }

        public WorkspaceViewModel()
        {
            _id = Guid.NewGuid().ToString();
            Documents = new ObservableCollection<DocumentViewModel>();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum DocumentType
    {
        Word,
        Excel,
        PowerPoint,
        Unknown
    }

    public enum NavigationPanePosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public enum WindowState
    {
        Normal,
        Maximized,
        Minimized
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }
}

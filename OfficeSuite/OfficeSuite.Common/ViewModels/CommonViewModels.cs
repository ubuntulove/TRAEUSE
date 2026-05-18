using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfficeSuite.Common.ViewModels
{
    public class ApplicationViewModel : INotifyPropertyChanged
    {
        private string _title = "OfficeSuite";
        private string _statusMessage = "Ready";
        private bool _isBusy;
        private string _currentModule = "Word";
        private int _totalDocuments = 1;
        private double _memoryUsage;
        private DateTime _startTime = DateTime.Now;
        private bool _isMaximized;
        private bool _isMinimized;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Word.ViewModels.WordViewModel WordViewModel { get; }
        public Spreadsheet.ViewModels.SpreadsheetViewModel SpreadsheetViewModel { get; }
        public Presentation.ViewModels.PresentationViewModel PresentationViewModel { get; }
        public SettingsViewModel Settings { get; }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(nameof(StatusMessage)); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(nameof(IsBusy)); }
        }

        public string CurrentModule
        {
            get => _currentModule;
            set { _currentModule = value; OnPropertyChanged(nameof(CurrentModule)); }
        }

        public int TotalDocuments
        {
            get => _totalDocuments;
            set { _totalDocuments = value; OnPropertyChanged(nameof(TotalDocuments)); }
        }

        public double MemoryUsage
        {
            get => _memoryUsage;
            set { _memoryUsage = value; OnPropertyChanged(nameof(MemoryUsage)); }
        }

        public DateTime StartTime
        {
            get => _startTime;
            set { _startTime = value; OnPropertyChanged(nameof(StartTime)); }
        }

        public bool IsMaximized
        {
            get => _isMaximized;
            set { _isMaximized = value; OnPropertyChanged(nameof(IsMaximized)); }
        }

        public bool IsMinimized
        {
            get => _isMinimized;
            set { _isMinimized = value; OnPropertyChanged(nameof(IsMinimized)); }
        }

        public TimeSpan RunningTime => DateTime.Now - StartTime;

        public ICommand NewDocumentCommand { get; }
        public ICommand OpenDocumentCommand { get; }
        public ICommand SaveDocumentCommand { get; }
        public ICommand SaveAllCommand { get; }
        public ICommand CloseDocumentCommand { get; }
        public ICommand CloseAllCommand { get; }
        public ICommand SwitchToWordCommand { get; }
        public ICommand SwitchToSpreadsheetCommand { get; }
        public ICommand SwitchToPresentationCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }

        public ApplicationViewModel()
        {
            WordViewModel = new Word.ViewModels.WordViewModel();
            SpreadsheetViewModel = new Spreadsheet.ViewModels.SpreadsheetViewModel();
            PresentationViewModel = new Presentation.ViewModels.PresentationViewModel();
            Settings = new SettingsViewModel();

            NewDocumentCommand = new RelayCommand<string>(ExecuteNewDocument);
            OpenDocumentCommand = new RelayCommand<string>(ExecuteOpenDocument);
            SaveDocumentCommand = new RelayCommand<string>(ExecuteSaveDocument);
            SaveAllCommand = new RelayCommand(ExecuteSaveAll);
            CloseDocumentCommand = new RelayCommand(ExecuteCloseDocument);
            CloseAllCommand = new RelayCommand(ExecuteCloseAll);
            SwitchToWordCommand = new RelayCommand(() => SwitchModule("Word"));
            SwitchToSpreadsheetCommand = new RelayCommand(() => SwitchModule("Spreadsheet"));
            SwitchToPresentationCommand = new RelayCommand(() => SwitchModule("Presentation"));
            ShowSettingsCommand = new RelayCommand(ExecuteShowSettings);
            ShowAboutCommand = new RelayCommand(ExecuteShowAbout);
            ExitCommand = new RelayCommand(ExecuteExit);
            RefreshCommand = new RelayCommand(ExecuteRefresh);
            UndoCommand = new RelayCommand(ExecuteUndo);
            RedoCommand = new RelayCommand(ExecuteRedo);
        }

        private void ExecuteNewDocument(string? module)
        {
            switch (module?.ToLower())
            {
                case "word":
                    WordViewModel.NewCommand.Execute(null);
                    CurrentModule = "Word";
                    break;
                case "spreadsheet":
                    SpreadsheetViewModel.NewCommand.Execute(null);
                    CurrentModule = "Spreadsheet";
                    break;
                case "presentation":
                    PresentationViewModel.NewCommand.Execute(null);
                    CurrentModule = "Presentation";
                    break;
                default:
                    WordViewModel.NewCommand.Execute(null);
                    CurrentModule = "Word";
                    break;
            }

            StatusMessage = "New document created";
        }

        private void ExecuteOpenDocument(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                StatusMessage = $"Opening {Path.GetFileName(path)}...";
            }
        }

        private void ExecuteSaveDocument(string? path)
        {
            if (!string.IsNullOrEmpty(path))
            {
                StatusMessage = $"Saving {Path.GetFileName(path)}...";
            }
        }

        private void ExecuteSaveAll()
        {
            StatusMessage = "Saving all documents...";
        }

        private void ExecuteCloseDocument()
        {
            StatusMessage = "Closing document...";
        }

        private void ExecuteCloseAll()
        {
            StatusMessage = "Closing all documents...";
        }

        private void SwitchModule(string module)
        {
            CurrentModule = module;
            StatusMessage = $"Switched to {module}";
        }

        private void ExecuteShowSettings()
        {
            StatusMessage = "Opening settings...";
        }

        private void ExecuteShowAbout()
        {
            StatusMessage = "Showing about dialog...";
        }

        private void ExecuteExit()
        {
            StatusMessage = "Exiting application...";
        }

        private void ExecuteRefresh()
        {
            StatusMessage = "Refreshing...";
            UpdateMemoryUsage();
        }

        private void ExecuteUndo()
        {
            StatusMessage = "Undo";
        }

        private void ExecuteRedo()
        {
            StatusMessage = "Redo";
        }

        private void UpdateMemoryUsage()
        {
            MemoryUsage = GC.GetTotalMemory(false) / (1024.0 * 1024.0);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SettingsViewModel : INotifyPropertyChanged
    {
        private string _defaultSaveLocation = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        private string _defaultFileFormat = "DOCX";
        private int _autoSaveInterval = 5;
        private bool _autoSaveEnabled = true;
        private int _recentDocumentsMax = 10;
        private bool _showStatusBar = true;
        private bool _showRuler = true;
        private bool _showMiniToolbar = true;
        private string _defaultFont = "Calibri";
        private int _defaultFontSize = 11;
        private string _defaultLanguage = "en-US";
        private bool _checkSpelling = true;
        private bool _highlightFormatting = true;
        private bool _enableHardwareAcceleration = true;
        private bool _useSystemTheme = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string DefaultSaveLocation
        {
            get => _defaultSaveLocation;
            set { _defaultSaveLocation = value; OnPropertyChanged(nameof(DefaultSaveLocation)); }
        }

        public string DefaultFileFormat
        {
            get => _defaultFileFormat;
            set { _defaultFileFormat = value; OnPropertyChanged(nameof(DefaultFileFormat)); }
        }

        public int AutoSaveInterval
        {
            get => _autoSaveInterval;
            set { _autoSaveInterval = value; OnPropertyChanged(nameof(AutoSaveInterval)); }
        }

        public bool AutoSaveEnabled
        {
            get => _autoSaveEnabled;
            set { _autoSaveEnabled = value; OnPropertyChanged(nameof(AutoSaveEnabled)); }
        }

        public int RecentDocumentsMax
        {
            get => _recentDocumentsMax;
            set { _recentDocumentsMax = value; OnPropertyChanged(nameof(RecentDocumentsMax)); }
        }

        public bool ShowStatusBar
        {
            get => _showStatusBar;
            set { _showStatusBar = value; OnPropertyChanged(nameof(ShowStatusBar)); }
        }

        public bool ShowRuler
        {
            get => _showRuler;
            set { _showRuler = value; OnPropertyChanged(nameof(ShowRuler)); }
        }

        public bool ShowMiniToolbar
        {
            get => _showMiniToolbar;
            set { _showMiniToolbar = value; OnPropertyChanged(nameof(ShowMiniToolbar)); }
        }

        public string DefaultFont
        {
            get => _defaultFont;
            set { _defaultFont = value; OnPropertyChanged(nameof(DefaultFont)); }
        }

        public int DefaultFontSize
        {
            get => _defaultFontSize;
            set { _defaultFontSize = value; OnPropertyChanged(nameof(DefaultFontSize)); }
        }

        public string DefaultLanguage
        {
            get => _defaultLanguage;
            set { _defaultLanguage = value; OnPropertyChanged(nameof(DefaultLanguage)); }
        }

        public bool CheckSpelling
        {
            get => _checkSpelling;
            set { _checkSpelling = value; OnPropertyChanged(nameof(CheckSpelling)); }
        }

        public bool HighlightFormatting
        {
            get => _highlightFormatting;
            set { _highlightFormatting = value; OnPropertyChanged(nameof(HighlightFormatting)); }
        }

        public bool EnableHardwareAcceleration
        {
            get => _enableHardwareAcceleration;
            set { _enableHardwareAcceleration = value; OnPropertyChanged(nameof(EnableHardwareAcceleration)); }
        }

        public bool UseSystemTheme
        {
            get => _useSystemTheme;
            set { _useSystemTheme = value; OnPropertyChanged(nameof(UseSystemTheme)); }
        }

        public List<string> AvailableLanguages { get; } = new()
        {
            "en-US", "es-ES", "fr-FR", "de-DE", "zh-CN", "ja-JP"
        };

        public List<string> AvailableFonts { get; } = new()
        {
            "Calibri", "Arial", "Times New Roman", "Verdana", "Tahoma", "Georgia", "Courier New"
        };

        public List<string> AvailableFileFormats { get; } = new()
        {
            "DOCX", "PDF", "RTF", "TXT", "XLSX", "CSV", "PPTX"
        };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DocumentTabViewModel : INotifyPropertyChanged
    {
        private string _title = "Document";
        private string _filePath = string.Empty;
        private bool _isModified;
        private bool _isSelected;
        private bool _isReadOnly;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        public string FilePath
        {
            get => _filePath;
            set
            {
                _filePath = value;
                OnPropertyChanged(nameof(FilePath));
                if (string.IsNullOrEmpty(_title))
                    Title = Path.GetFileName(value);
            }
        }

        public bool IsModified
        {
            get => _isModified;
            set { _isModified = value; OnPropertyChanged(nameof(IsModified)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(nameof(IsReadOnly)); }
        }

        public string DocumentType { get; set; } = "Word";

        public string ToolTip => IsModified ? $"{FilePath} (Modified)" : FilePath;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class StatusBarViewModel : INotifyPropertyChanged
    {
        private string _leftMessage = "Ready";
        private string _centerMessage = string.Empty;
        private string _rightMessage = string.Empty;
        private double _zoomLevel = 100;
        private string _lineColumnInfo = "Ln 1, Col 1";
        private int _pageNumber = 1;
        private int _totalPages = 1;
        private int _wordCount;
        private int _characterCount;
        private bool _trackChangesEnabled;
        private string _selectionInfo = string.Empty;

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

        public double ZoomLevel
        {
            get => _zoomLevel;
            set { _zoomLevel = value; OnPropertyChanged(nameof(ZoomLevel)); }
        }

        public string LineColumnInfo
        {
            get => _lineColumnInfo;
            set { _lineColumnInfo = value; OnPropertyChanged(nameof(LineColumnInfo)); }
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

        public bool TrackChangesEnabled
        {
            get => _trackChangesEnabled;
            set { _trackChangesEnabled = value; OnPropertyChanged(nameof(TrackChangesEnabled)); }
        }

        public string SelectionInfo
        {
            get => _selectionInfo;
            set { _selectionInfo = value; OnPropertyChanged(nameof(SelectionInfo)); }
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

    public class CommandManager
    {
        public static event EventHandler? RequerySuggested;
    }
}

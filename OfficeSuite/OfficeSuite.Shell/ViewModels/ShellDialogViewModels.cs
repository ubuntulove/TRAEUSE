using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace OfficeSuite.Shell.ViewModels
{
    public class BackstageViewModel : INotifyPropertyChanged
    {
        private bool _isOpen;
        private BackstageTabViewModel _selectedTab;
        private ObservableCollection<BackstageTabViewModel> _tabs;
        private ObservableCollection<RecentFileViewModel> _recentFiles;
        private ObservableCollection<PinnedFileViewModel> _pinnedFiles;
        private ObservableCollection<TemplateCategoryViewModel> _templateCategories;
        private string _searchText;
        private string _currentView;

        public bool IsOpen
        {
            get => _isOpen;
            set => SetProperty(ref _isOpen, value);
        }

        public BackstageTabViewModel SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public ObservableCollection<BackstageTabViewModel> Tabs
        {
            get => _tabs;
            set => SetProperty(ref _tabs, value);
        }

        public ObservableCollection<RecentFileViewModel> RecentFiles
        {
            get => _recentFiles;
            set => SetProperty(ref _recentFiles, value);
        }

        public ObservableCollection<PinnedFileViewModel> PinnedFiles
        {
            get => _pinnedFiles;
            set => SetProperty(ref _pinnedFiles, value);
        }

        public ObservableCollection<TemplateCategoryViewModel> TemplateCategories
        {
            get => _templateCategories;
            set => SetProperty(ref _templateCategories, value);
        }

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }

        public ICommand NewCommand { get; }
        public ICommand OpenCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAsCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand AccountCommand { get; }
        public ICommand OptionsCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand PinFileCommand { get; }
        public ICommand UnpinFileCommand { get; }
        public ICommand RemoveFileCommand { get; }
        public ICommand OpenFolderCommand { get; }

        public BackstageViewModel()
        {
            _tabs = new ObservableCollection<BackstageTabViewModel>();
            _recentFiles = new ObservableCollection<RecentFileViewModel>();
            _pinnedFiles = new ObservableCollection<PinnedFileViewModel>();
            _templateCategories = new ObservableCollection<TemplateCategoryViewModel>();
            _currentView = "Info";

            InitializeTabs();
            InitializeCommands();
        }

        private void InitializeTabs()
        {
            Tabs.Add(new BackstageTabViewModel { Name = "Info", Icon = "Info" });
            Tabs.Add(new BackstageTabViewModel { Name = "New", Icon = "New" });
            Tabs.Add(new BackstageTabViewModel { Name = "Open", Icon = "Open" });
            Tabs.Add(new BackstageTabViewModel { Name = "Save", Icon = "Save" });
            Tabs.Add(new BackstageTabViewModel { Name = "Save As", Icon = "SaveAs" });
            Tabs.Add(new BackstageTabViewModel { Name = "Export", Icon = "Export" });
            Tabs.Add(new BackstageTabViewModel { Name = "Print", Icon = "Print" });
            Tabs.Add(new BackstageTabViewModel { Name = "Share", Icon = "Share" });
            Tabs.Add(new BackstageTabViewModel { Name = "Account", Icon = "Account" });
            Tabs.Add(new BackstageTabViewModel { Name = "Options", Icon = "Options" });

            SelectedTab = Tabs.FirstOrDefault();
        }

        private void InitializeCommands()
        {
            NewCommand = new RelayCommand(_ => New());
            OpenCommand = new RelayCommand(_ => Open());
            SaveCommand = new RelayCommand(_ => Save());
            SaveAsCommand = new RelayCommand(_ => SaveAs());
            ExportCommand = new RelayCommand(_ => Export());
            PrintCommand = new RelayCommand(_ => Print());
            CloseCommand = new RelayCommand(_ => Close());
            AccountCommand = new RelayCommand(_ => ShowAccount());
            OptionsCommand = new RelayCommand(_ => ShowOptions());
            ExitCommand = new RelayCommand(_ => Exit());
            PinFileCommand = new RelayCommand(_ => PinFile(_ as RecentFileViewModel));
            UnpinFileCommand = new RelayCommand(_ => UnpinFile(_ as PinnedFileViewModel));
            RemoveFileCommand = new RelayCommand(_ => RemoveFile(_ as RecentFileViewModel));
            OpenFolderCommand = new RelayCommand(_ => OpenFolder(_ as RecentFileViewModel));
        }

        public void New()
        {
            IsOpen = false;
        }

        public void Open()
        {
        }

        public void Save()
        {
        }

        public void SaveAs()
        {
        }

        public void Export()
        {
        }

        public void Print()
        {
        }

        public void Close()
        {
            IsOpen = false;
        }

        public void ShowAccount()
        {
        }

        public void ShowOptions()
        {
        }

        public void Exit()
        {
        }

        public void PinFile(RecentFileViewModel file)
        {
            if (file != null)
            {
                PinnedFiles.Add(new PinnedFileViewModel
                {
                    Name = file.Name,
                    Path = file.Path,
                    FileType = file.FileType
                });
                RecentFiles.Remove(file);
            }
        }

        public void UnpinFile(PinnedFileViewModel file)
        {
            if (file != null)
            {
                RecentFiles.Add(new RecentFileViewModel
                {
                    Name = file.Name,
                    Path = file.Path,
                    FileType = file.FileType
                });
                PinnedFiles.Remove(file);
            }
        }

        public void RemoveFile(RecentFileViewModel file)
        {
            if (file != null)
            {
                RecentFiles.Remove(file);
            }
        }

        public void OpenFolder(RecentFileViewModel file)
        {
            if (file != null && !string.IsNullOrEmpty(file.Path))
            {
                var folder = System.IO.Path.GetDirectoryName(file.Path);
                if (System.IO.Directory.Exists(folder))
                {
                }
            }
        }

        public void AddRecentFile(string name, string path, string fileType)
        {
            var existing = RecentFiles.FirstOrDefault(f => f.Path == path);
            if (existing != null)
            {
                RecentFiles.Remove(existing);
            }

            RecentFiles.Insert(0, new RecentFileViewModel
            {
                Name = name,
                Path = path,
                FileType = fileType,
                AccessedDate = DateTime.Now
            });

            while (RecentFiles.Count > 20)
            {
                RecentFiles.RemoveAt(RecentFiles.Count - 1);
            }
        }

        public void ClearRecentFiles()
        {
            RecentFiles.Clear();
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

    public class BackstageTabViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _icon;
        private bool _isSelected;
        private bool _isEnabled;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
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

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public BackstageTabViewModel()
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

    public class PinnedFileViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _path;
        private string _fileType;
        private DateTime _pinnedDate;
        private byte[] _thumbnail;

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

        public DateTime PinnedDate
        {
            get => _pinnedDate;
            set => SetProperty(ref _pinnedDate, value);
        }

        public byte[] Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public PinnedFileViewModel()
        {
            _pinnedDate = DateTime.Now;
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

    public class TemplateCategoryViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _icon;
        private string _description;
        private bool _isExpanded;
        private bool _isSelected;
        private ObservableCollection<TemplateItemViewModel> _templates;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ObservableCollection<TemplateItemViewModel> Templates
        {
            get => _templates;
            set => SetProperty(ref _templates, value);
        }

        public TemplateCategoryViewModel()
        {
            _templates = new ObservableCollection<TemplateItemViewModel>();
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

    public class TemplateItemViewModel : INotifyPropertyChanged
    {
        private string _id;
        private string _name;
        private string _description;
        private string _thumbnailPath;
        private byte[] _thumbnail;
        private string _category;
        private string _tags;
        private DateTime _createdDate;
        private bool _isBuiltIn;
        private bool _isSelected;

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

        public string ThumbnailPath
        {
            get => _thumbnailPath;
            set => SetProperty(ref _thumbnailPath, value);
        }

        public byte[] Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set => SetProperty(ref _createdDate, value);
        }

        public bool IsBuiltIn
        {
            get => _isBuiltIn;
            set => SetProperty(ref _isBuiltIn, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public TemplateItemViewModel()
        {
            _id = Guid.NewGuid().ToString();
            _createdDate = DateTime.Now;
            _isBuiltIn = false;
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

    public class InfoPaneViewModel : INotifyPropertyChanged
    {
        private string _documentTitle;
        private string _documentAuthor;
        private DateTime _documentCreated;
        private DateTime _documentModified;
        private string _documentSize;
        private string _documentTags;
        private string _documentCategory;
        private string _documentSubject;
        private string _documentKeywords;
        private string _documentComments;
        private string _documentStatus;
        private bool _isReadOnly;
        private bool _hasPassword;
        private bool _hasDigitalSignature;
        private byte[] _thumbnail;
        private ObservableCollection<PropertyViewModel> _customProperties;

        public string DocumentTitle
        {
            get => _documentTitle;
            set => SetProperty(ref _documentTitle, value);
        }

        public string DocumentAuthor
        {
            get => _documentAuthor;
            set => SetProperty(ref _documentAuthor, value);
        }

        public DateTime DocumentCreated
        {
            get => _documentCreated;
            set => SetProperty(ref _documentCreated, value);
        }

        public DateTime DocumentModified
        {
            get => _documentModified;
            set => SetProperty(ref _documentModified, value);
        }

        public string DocumentSize
        {
            get => _documentSize;
            set => SetProperty(ref _documentSize, value);
        }

        public string DocumentTags
        {
            get => _documentTags;
            set => SetProperty(ref _documentTags, value);
        }

        public string DocumentCategory
        {
            get => _documentCategory;
            set => SetProperty(ref _documentCategory, value);
        }

        public string DocumentSubject
        {
            get => _documentSubject;
            set => SetProperty(ref _documentSubject, value);
        }

        public string DocumentKeywords
        {
            get => _documentKeywords;
            set => SetProperty(ref _documentKeywords, value);
        }

        public string DocumentComments
        {
            get => _documentComments;
            set => SetProperty(ref _documentComments, value);
        }

        public string DocumentStatus
        {
            get => _documentStatus;
            set => SetProperty(ref _documentStatus, value);
        }

        public bool IsReadOnly
        {
            get => _isReadOnly;
            set => SetProperty(ref _isReadOnly, value);
        }

        public bool HasPassword
        {
            get => _hasPassword;
            set => SetProperty(ref _hasPassword, value);
        }

        public bool HasDigitalSignature
        {
            get => _hasDigitalSignature;
            set => SetProperty(ref _hasDigitalSignature, value);
        }

        public byte[] Thumbnail
        {
            get => _thumbnail;
            set => SetProperty(ref _thumbnail, value);
        }

        public ObservableCollection<PropertyViewModel> CustomProperties
        {
            get => _customProperties;
            set => SetProperty(ref _customProperties, value);
        }

        public InfoPaneViewModel()
        {
            _customProperties = new ObservableCollection<PropertyViewModel>();
            _documentCreated = DateTime.Now;
            _documentModified = DateTime.Now;
        }

        public void LoadFromDocument(DocumentViewModel document)
        {
            DocumentTitle = document.Title;
            DocumentCreated = document.CreatedDate;
            DocumentModified = document.ModifiedDate;
            IsReadOnly = document.IsReadOnly;
        }

        public void SaveToDocument(DocumentViewModel document)
        {
            document.Title = DocumentTitle;
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

    public class PropertyViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _value;
        private string _type;
        private bool _isEditable;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public string Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public bool IsEditable
        {
            get => _isEditable;
            set => SetProperty(ref _isEditable, value);
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

    public class FindReplaceViewModel : INotifyPropertyChanged
    {
        private string _searchText;
        private string _replaceText;
        private bool _matchCase;
        private bool _matchWholeWord;
        private bool _useWildcards;
        private bool _useRegex;
        private bool _searchUp;
        private bool _searchInHeaders;
        private bool _searchInFooters;
        private bool _searchInComments;
        private bool _wrapSearch;
        private int _matchCount;
        private int _currentMatch;
        private ObservableCollection<FindResultViewModel> _results;

        public string SearchText
        {
            get => _searchText;
            set => SetProperty(ref _searchText, value);
        }

        public string ReplaceText
        {
            get => _replaceText;
            set => SetProperty(ref _replaceText, value);
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

        public bool UseRegex
        {
            get => _useRegex;
            set => SetProperty(ref _useRegex, value);
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

        public bool SearchInComments
        {
            get => _searchInComments;
            set => SetProperty(ref _searchInComments, value);
        }

        public bool WrapSearch
        {
            get => _wrapSearch;
            set => SetProperty(ref _wrapSearch, value);
        }

        public int MatchCount
        {
            get => _matchCount;
            set => SetProperty(ref _matchCount, value);
        }

        public int CurrentMatch
        {
            get => _currentMatch;
            set => SetProperty(ref _currentMatch, value);
        }

        public ObservableCollection<FindResultViewModel> Results
        {
            get => _results;
            set => SetProperty(ref _results, value);
        }

        public ICommand FindNextCommand { get; }
        public ICommand FindPreviousCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand ReplaceAllCommand { get; }
        public ICommand FindAllCommand { get; }
        public ICommand CloseCommand { get; }

        public FindReplaceViewModel()
        {
            _results = new ObservableCollection<FindResultViewModel>();
            _wrapSearch = true;

            FindNextCommand = new RelayCommand(_ => FindNext(), _ => CanFindNext());
            FindPreviousCommand = new RelayCommand(_ => FindPrevious(), _ => CanFindPrevious());
            ReplaceCommand = new RelayCommand(_ => Replace(), _ => CanReplace());
            ReplaceAllCommand = new RelayCommand(_ => ReplaceAll(), _ => CanReplaceAll());
            FindAllCommand = new RelayCommand(_ => FindAll());
            CloseCommand = new RelayCommand(_ => Close());
        }

        public void FindNext()
        {
            CurrentMatch++;
            if (CurrentMatch >= MatchCount && WrapSearch)
                CurrentMatch = 0;
        }

        private bool CanFindNext() => !string.IsNullOrEmpty(SearchText);

        public void FindPrevious()
        {
            CurrentMatch--;
            if (CurrentMatch < 0 && WrapSearch)
                CurrentMatch = Math.Max(0, MatchCount - 1);
        }

        private bool CanFindPrevious() => !string.IsNullOrEmpty(SearchText);

        public void Replace()
        {
        }

        private bool CanReplace() => !string.IsNullOrEmpty(SearchText) && CurrentMatch >= 0;

        public void ReplaceAll()
        {
        }

        private bool CanReplaceAll() => !string.IsNullOrEmpty(SearchText);

        public void FindAll()
        {
            Results.Clear();
        }

        public void Close()
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

    public class FindResultViewModel : INotifyPropertyChanged
    {
        private string _text;
        private string _context;
        private int _pageNumber;
        private int _lineNumber;
        private int _columnNumber;
        private bool _isSelected;
        private string _documentPath;

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public string Context
        {
            get => _context;
            set => SetProperty(ref _context, value);
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

        public int ColumnNumber
        {
            get => _columnNumber;
            set => SetProperty(ref _columnNumber, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public string DocumentPath
        {
            get => _documentPath;
            set => SetProperty(ref _documentPath, value);
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

    public class GoToViewModel : INotifyPropertyChanged
    {
        private GoToType _goToType;
        private int _pageNumber;
        private int _lineNumber;
        private int _bookmarkNumber;
        private int _footnoteNumber;
        private int _endnoteNumber;
        private int _commentNumber;
        private int _sectionNumber;
        private int _graphicNumber;
        private int _tableNumber;
        private int _equationNumber;
        private int _objectNumber;

        public GoToType GoToType
        {
            get => _goToType;
            set => SetProperty(ref _goToType, value);
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

        public int BookmarkNumber
        {
            get => _bookmarkNumber;
            set => SetProperty(ref _bookmarkNumber, value);
        }

        public int FootnoteNumber
        {
            get => _footnoteNumber;
            set => SetProperty(ref _footnoteNumber, value);
        }

        public int EndnoteNumber
        {
            get => _endnoteNumber;
            set => SetProperty(ref _endnoteNumber, value);
        }

        public int CommentNumber
        {
            get => _commentNumber;
            set => SetProperty(ref _commentNumber, value);
        }

        public int SectionNumber
        {
            get => _sectionNumber;
            set => SetProperty(ref _sectionNumber, value);
        }

        public int GraphicNumber
        {
            get => _graphicNumber;
            set => SetProperty(ref _graphicNumber, value);
        }

        public int TableNumber
        {
            get => _tableNumber;
            set => SetProperty(ref _tableNumber, value);
        }

        public int EquationNumber
        {
            get => _equationNumber;
            set => SetProperty(ref _equationNumber, value);
        }

        public int ObjectNumber
        {
            get => _objectNumber;
            set => SetProperty(ref _objectNumber, value);
        }

        public ICommand GoToCommand { get; }
        public ICommand CloseCommand { get; }

        public GoToViewModel()
        {
            GoToCommand = new RelayCommand(_ => GoTo());
            CloseCommand = new RelayCommand(_ => Close());
        }

        public void GoTo()
        {
        }

        public void Close()
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

    public enum GoToType
    {
        Page,
        Line,
        Bookmark,
        Footnote,
        Endnote,
        Comment,
        Section,
        Graphic,
        Table,
        Equation,
        Object,
        Heading
    }

    public class SpellingViewModel : INotifyPropertyChanged
    {
        private string _misspelledWord;
        private string _suggestion;
        private ObservableCollection<string> _suggestions;
        private string _context;
        private int _ignoreCount;
        private int _currentIndex;
        private int _totalCount;
        private bool _ignoreAll;
        private bool _addToDictionary;
        private bool _autoCorrect;
        private ObservableCollection<SpellingErrorViewModel> _errors;

        public string MisspelledWord
        {
            get => _misspelledWord;
            set => SetProperty(ref _misspelledWord, value);
        }

        public string Suggestion
        {
            get => _suggestion;
            set => SetProperty(ref _suggestion, value);
        }

        public ObservableCollection<string> Suggestions
        {
            get => _suggestions;
            set => SetProperty(ref _suggestions, value);
        }

        public string Context
        {
            get => _context;
            set => SetProperty(ref _context, value);
        }

        public int IgnoreCount
        {
            get => _ignoreCount;
            set => SetProperty(ref _ignoreCount, value);
        }

        public int CurrentIndex
        {
            get => _currentIndex;
            set => SetProperty(ref _currentIndex, value);
        }

        public int TotalCount
        {
            get => _totalCount;
            set => SetProperty(ref _totalCount, value);
        }

        public bool IgnoreAll
        {
            get => _ignoreAll;
            set => SetProperty(ref _ignoreAll, value);
        }

        public bool AddToDictionary
        {
            get => _addToDictionary;
            set => SetProperty(ref _addToDictionary, value);
        }

        public bool AutoCorrect
        {
            get => _autoCorrect;
            set => SetProperty(ref _autoCorrect, value);
        }

        public ObservableCollection<SpellingErrorViewModel> Errors
        {
            get => _errors;
            set => SetProperty(ref _errors, value);
        }

        public ICommand IgnoreOnceCommand { get; }
        public ICommand IgnoreAllCommand { get; }
        public ICommand AddToDictionaryCommand { get; }
        public ICommand ChangeCommand { get; }
        public ICommand ChangeAllCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand OptionsCommand { get; }

        public SpellingViewModel()
        {
            _suggestions = new ObservableCollection<string>();
            _errors = new ObservableCollection<SpellingErrorViewModel>();

            IgnoreOnceCommand = new RelayCommand(_ => IgnoreOnce());
            IgnoreAllCommand = new RelayCommand(_ => IgnoreAll());
            AddToDictionaryCommand = new RelayCommand(_ => AddToDictionary());
            ChangeCommand = new RelayCommand(_ => Change(), _ => CanChange());
            ChangeAllCommand = new RelayCommand(_ => ChangeAll(), _ => CanChange());
            UndoCommand = new RelayCommand(_ => Undo());
            CloseCommand = new RelayCommand(_ => Close());
            OptionsCommand = new RelayCommand(_ => ShowOptions());
        }

        public void IgnoreOnce()
        {
            NextError();
        }

        public void IgnoreAll()
        {
            IgnoreAll = true;
            NextError();
        }

        public void AddToDictionary()
        {
            AddToDictionary = true;
            NextError();
        }

        public void Change()
        {
        }

        private bool CanChange() => !string.IsNullOrEmpty(Suggestion);

        public void ChangeAll()
        {
        }

        public void Undo()
        {
        }

        public void Close()
        {
        }

        public void ShowOptions()
        {
        }

        public void NextError()
        {
            if (CurrentIndex < TotalCount - 1)
            {
                CurrentIndex++;
            }
        }

        public void PreviousError()
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
            }
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

    public class SpellingErrorViewModel : INotifyPropertyChanged
    {
        private string _word;
        private string _context;
        private int _pageNumber;
        private int _lineNumber;
        private bool _isIgnored;
        private bool _isInDictionary;
        private ObservableCollection<string> _suggestions;

        public string Word
        {
            get => _word;
            set => SetProperty(ref _word, value);
        }

        public string Context
        {
            get => _context;
            set => SetProperty(ref _context, value);
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

        public bool IsIgnored
        {
            get => _isIgnored;
            set => SetProperty(ref _isIgnored, value);
        }

        public bool IsInDictionary
        {
            get => _isInDictionary;
            set => SetProperty(ref _isInDictionary, value);
        }

        public ObservableCollection<string> Suggestions
        {
            get => _suggestions;
            set => SetProperty(ref _suggestions, value);
        }

        public SpellingErrorViewModel()
        {
            _suggestions = new ObservableCollection<string>();
            _isIgnored = false;
            _isInDictionary = false;
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

    public class PrintPreviewViewModel : INotifyPropertyChanged
    {
        private string _printerName;
        private int _copies;
        private bool _collate;
        private bool _colorPrinting;
        private bool _duplexPrinting;
        private DuplexMode _duplexMode;
        private PaperSize _paperSize;
        private PaperSource _paperSource;
        private bool _printToFile;
        private string _outputFileName;
        private int _firstPage;
        private int _lastPage;
        private int _pagesPerSheet;
        private int _currentPage;
        private int _totalPages;
        private double _zoomLevel;
        private bool _showThumbnails;
        private bool _showRuler;
        private bool _landscape;
        private ObservableCollection<PrintPageViewModel> _pages;

        public string PrinterName
        {
            get => _printerName;
            set => SetProperty(ref _printerName, value);
        }

        public int Copies
        {
            get => _copies;
            set => SetProperty(ref _copies, value);
        }

        public bool Collate
        {
            get => _collate;
            set => SetProperty(ref _collate, value);
        }

        public bool ColorPrinting
        {
            get => _colorPrinting;
            set => SetProperty(ref _colorPrinting, value);
        }

        public bool DuplexPrinting
        {
            get => _duplexPrinting;
            set => SetProperty(ref _duplexPrinting, value);
        }

        public DuplexMode DuplexMode
        {
            get => _duplexMode;
            set => SetProperty(ref _duplexMode, value);
        }

        public PaperSize PaperSize
        {
            get => _paperSize;
            set => SetProperty(ref _paperSize, value);
        }

        public PaperSource PaperSource
        {
            get => _paperSource;
            set => SetProperty(ref _paperSource, value);
        }

        public bool PrintToFile
        {
            get => _printToFile;
            set => SetProperty(ref _printToFile, value);
        }

        public string OutputFileName
        {
            get => _outputFileName;
            set => SetProperty(ref _outputFileName, value);
        }

        public int FirstPage
        {
            get => _firstPage;
            set => SetProperty(ref _firstPage, value);
        }

        public int LastPage
        {
            get => _lastPage;
            set => SetProperty(ref _lastPage, value);
        }

        public int PagesPerSheet
        {
            get => _pagesPerSheet;
            set => SetProperty(ref _pagesPerSheet, value);
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

        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        public bool ShowThumbnails
        {
            get => _showThumbnails;
            set => SetProperty(ref _showThumbnails, value);
        }

        public bool ShowRuler
        {
            get => _showRuler;
            set => SetProperty(ref _showRuler, value);
        }

        public bool Landscape
        {
            get => _landscape;
            set => SetProperty(ref _landscape, value);
        }

        public ObservableCollection<PrintPageViewModel> Pages
        {
            get => _pages;
            set => SetProperty(ref _pages, value);
        }

        public ICommand PrintCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PageSetupCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand FitToWindowCommand { get; }
        public ICommand PreviousPageCommand { get; }
        public ICommand NextPageCommand { get; }
        public ICommand FirstPageCommand { get; }
        public ICommand LastPageCommand { get; }

        public PrintPreviewViewModel()
        {
            _pages = new ObservableCollection<PrintPageViewModel>();
            _copies = 1;
            _collate = true;
            _firstPage = 1;
            _pagesPerSheet = 1;
            _zoomLevel = 100;

            PrintCommand = new RelayCommand(_ => Print());
            CancelCommand = new RelayCommand(_ => Cancel());
            PageSetupCommand = new RelayCommand(_ => PageSetup());
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            FitToWindowCommand = new RelayCommand(_ => FitToWindow());
            PreviousPageCommand = new RelayCommand(_ => PreviousPage(), _ => CanPreviousPage());
            NextPageCommand = new RelayCommand(_ => NextPage(), _ => CanNextPage());
            FirstPageCommand = new RelayCommand(_ => GoToFirstPage(), _ => CanGoToFirstPage());
            LastPageCommand = new RelayCommand(_ => GoToLastPage(), _ => CanGoToLastPage());
        }

        public void Print()
        {
        }

        public void Cancel()
        {
        }

        public void PageSetup()
        {
        }

        public void ZoomIn()
        {
            ZoomLevel = Math.Min(400, ZoomLevel + 10);
        }

        public void ZoomOut()
        {
            ZoomLevel = Math.Max(25, ZoomLevel - 10);
        }

        public void FitToWindow()
        {
            ZoomLevel = 100;
        }

        public void PreviousPage()
        {
            if (CurrentPage > 1)
                CurrentPage--;
        }

        private bool CanPreviousPage() => CurrentPage > 1;

        public void NextPage()
        {
            if (CurrentPage < TotalPages)
                CurrentPage++;
        }

        private bool CanNextPage() => CurrentPage < TotalPages;

        public void GoToFirstPage()
        {
            CurrentPage = 1;
        }

        private bool CanGoToFirstPage() => CurrentPage > 1;

        public void GoToLastPage()
        {
            CurrentPage = TotalPages;
        }

        private bool CanGoToLastPage() => CurrentPage < TotalPages;

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

    public class PrintPageViewModel : INotifyPropertyChanged
    {
        private int _pageNumber;
        private byte[] _thumbnail;
        private bool _isSelected;
        private bool _isPrinted;

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

        public bool IsPrinted
        {
            get => _isPrinted;
            set => SetProperty(ref _isPrinted, value);
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

    public enum DuplexMode
    {
        Simplex,
        Horizontal,
        Vertical
    }

    public enum PaperSize
    {
        Letter,
        Legal,
        A4,
        A3,
        A5,
        B4,
        B5,
        Custom
    }

    public enum PaperSource
    {
        Auto,
        Manual,
        Tray1,
        Tray2,
        Tray3,
        Cassette
    }

    public class OptionsViewModel : INotifyPropertyChanged
    {
        private string _selectedCategory;
        private ObservableCollection<OptionsCategoryViewModel> _categories;
        private bool _hasChanges;
        private bool _isApplying;

        public string SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        public ObservableCollection<OptionsCategoryViewModel> Categories
        {
            get => _categories;
            set => SetProperty(ref _categories, value);
        }

        public bool HasChanges
        {
            get => _hasChanges;
            set => SetProperty(ref _hasChanges, value);
        }

        public bool IsApplying
        {
            get => _isApplying;
            set => SetProperty(ref _isApplying, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ApplyCommand { get; }

        public OptionsViewModel()
        {
            _categories = new ObservableCollection<OptionsCategoryViewModel>();
            _selectedCategory = "General";

            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => Cancel());
            ResetCommand = new RelayCommand(_ => Reset());
            ApplyCommand = new RelayCommand(_ => Apply());

            InitializeCategories();
        }

        private void InitializeCategories()
        {
            Categories.Add(new OptionsCategoryViewModel { Name = "General", Icon = "Settings" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Documents", Icon = "Document" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Spelling", Icon = "Spelling" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Save", Icon = "Save" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Display", Icon = "Display" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Proofing", Icon = "Proofing" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Advanced", Icon = "Advanced" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Customize", Icon = "Customize" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Add-ins", Icon = "AddIn" });
            Categories.Add(new OptionsCategoryViewModel { Name = "Trust Center", Icon = "Security" });
        }

        public void Save()
        {
            HasChanges = false;
        }

        public void Cancel()
        {
        }

        public void Reset()
        {
        }

        public void Apply()
        {
            IsApplying = true;
            HasChanges = false;
            IsApplying = false;
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

    public class OptionsCategoryViewModel : INotifyPropertyChanged
    {
        private string _name;
        private string _icon;
        private string _description;
        private bool _isSelected;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Icon
        {
            get => _icon;
            set => SetProperty(ref _icon, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
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

    public class NotificationViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _message;
        private NotificationType _type;
        private bool _isVisible;
        private DateTime _displayedAt;
        private int _displayDuration;
        private ICommand _actionCommand;
        private string _actionText;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public NotificationType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public DateTime DisplayedAt
        {
            get => _displayedAt;
            set => SetProperty(ref _displayedAt, value);
        }

        public int DisplayDuration
        {
            get => _displayDuration;
            set => SetProperty(ref _displayDuration, value);
        }

        public ICommand ActionCommand
        {
            get => _actionCommand;
            set => SetProperty(ref _actionCommand, value);
        }

        public string ActionText
        {
            get => _actionText;
            set => SetProperty(ref _actionText, value);
        }

        public NotificationViewModel()
        {
            _displayDuration = 5000;
        }

        public void Show()
        {
            IsVisible = true;
            DisplayedAt = DateTime.Now;
        }

        public void Hide()
        {
            IsVisible = false;
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

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class TaskPaneViewModel : INotifyPropertyChanged
    {
        private string _title;
        private bool _isVisible;
        private bool _isAutoHide;
        private TaskPanePosition _position;
        private double _width;
        private double _height;
        private ObservableCollection<TaskPaneItemViewModel> _items;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

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

        public TaskPanePosition Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public double Width
        {
            get => _width;
            set => SetProperty(ref _width, value);
        }

        public double Height
        {
            get => _height;
            set => SetProperty(ref _height, value);
        }

        public ObservableCollection<TaskPaneItemViewModel> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public TaskPaneViewModel()
        {
            _items = new ObservableCollection<TaskPaneItemViewModel>();
            _width = 300;
            _height = 400;
        }

        public void Show()
        {
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void Toggle()
        {
            IsVisible = !IsVisible;
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

    public enum TaskPanePosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public class TaskPaneItemViewModel : INotifyPropertyChanged
    {
        private string _title;
        private string _content;
        private bool _isExpanded;
        private bool _isSelected;
        private ICommand _command;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        public ICommand Command
        {
            get => _command;
            set => SetProperty(ref _command, value);
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
}

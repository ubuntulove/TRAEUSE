using System;
using System.Collections.Generic;
using System.Linq;
using OfficeSuite.Core.Models;
using OfficeSuite.Core.ViewModels;

namespace OfficeSuite.Shell.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private string _title;
        private string _statusMessage;
        private bool _isDocumentOpen;
        private DocumentType _currentDocumentType;
        private ViewModelBase _activeViewModel;
        private List<DocumentInfo> _recentDocuments;
        private List<RibbonTab> _ribbonTabs;
        private List<StatusBarItem> _statusBarItems;
        private bool _isFullScreen;
        private bool _isMaximized;
        private double _windowWidth;
        private double _windowHeight;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public bool IsDocumentOpen
        {
            get => _isDocumentOpen;
            set => SetProperty(ref _isDocumentOpen, value);
        }

        public DocumentType CurrentDocumentType
        {
            get => _currentDocumentType;
            set => SetProperty(ref _currentDocumentType, value);
        }

        public ViewModelBase ActiveViewModel
        {
            get => _activeViewModel;
            set => SetProperty(ref _activeViewModel, value);
        }

        public List<DocumentInfo> RecentDocuments
        {
            get => _recentDocuments ??= new List<DocumentInfo>();
            set => SetProperty(ref _recentDocuments, value);
        }

        public List<RibbonTab> RibbonTabs
        {
            get => _ribbonTabs ??= new List<RibbonTab>();
            set => SetProperty(ref _ribbonTabs, value);
        }

        public List<StatusBarItem> StatusBarItems
        {
            get => _statusBarItems ??= new List<StatusBarItem>();
            set => SetProperty(ref _statusBarItems, value);
        }

        public bool IsFullScreen
        {
            get => _isFullScreen;
            set => SetProperty(ref _isFullScreen, value);
        }

        public bool IsMaximized
        {
            get => _isMaximized;
            set => SetProperty(ref _isMaximized, value);
        }

        public double WindowWidth
        {
            get => _windowWidth;
            set => SetProperty(ref _windowWidth, value);
        }

        public double WindowHeight
        {
            get => _windowHeight;
            set => SetProperty(ref _windowHeight, value);
        }

        public RelayCommand NewWordDocumentCommand { get; }
        public RelayCommand NewSpreadsheetCommand { get; }
        public RelayCommand NewPresentationCommand { get; }
        public RelayCommand OpenCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand PrintCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }
        public RelayCommand CutCommand { get; }
        public RelayCommand CopyCommand { get; }
        public RelayCommand PasteCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand FindCommand { get; }
        public RelayCommand ReplaceCommand { get; }
        public RelayCommand CloseCommand { get; }
        public RelayCommand NewWindowCommand { get; }
        public RelayCommand CloseAllCommand { get; }
        public RelayCommand FullScreenCommand { get; }
        public RelayCommand MaximizeCommand { get; }
        public RelayCommand MinimizeCommand { get; }
        public RelayCommand NormalSizeCommand { get; }
        public RelayCommand AboutCommand { get; }
        public RelayCommand OptionsCommand { get; }

        public WordEditorViewModel WordEditorViewModel { get; }
        public SpreadsheetEditorViewModel SpreadsheetEditorViewModel { get; }
        public PresentationEditorViewModel PresentationEditorViewModel { get; }

        public MainWindowViewModel()
        {
            Title = "Office Suite";
            StatusMessage = "Ready";
            _recentDocuments = new List<DocumentInfo>();
            _ribbonTabs = new List<RibbonTab>();
            _statusBarItems = new List<StatusBarItem>();
            _windowWidth = 1200;
            _windowHeight = 800;

            WordEditorViewModel = new WordEditorViewModel();
            SpreadsheetEditorViewModel = new SpreadsheetEditorViewModel();
            PresentationEditorViewModel = new PresentationEditorViewModel();

            InitializeCommands();
            InitializeRibbon();
            InitializeStatusBar();
        }

        private void InitializeCommands()
        {
            NewWordDocumentCommand = new RelayCommand(_ => CreateNewDocument(DocumentType.Word));
            NewSpreadsheetCommand = new RelayCommand(_ => CreateNewDocument(DocumentType.Spreadsheet));
            NewPresentationCommand = new RelayCommand(_ => CreateNewDocument(DocumentType.Presentation));
            OpenCommand = new RelayCommand(_ => OpenDocument());
            SaveCommand = new RelayCommand(_ => SaveDocument(), _ => CanSaveDocument());
            SaveAsCommand = new RelayCommand(_ => SaveDocumentAs(), _ => CanSaveDocument());
            PrintCommand = new RelayCommand(_ => PrintDocument(), _ => CanSaveDocument());
            ExitCommand = new RelayCommand(_ => ExitApplication());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo());
            CutCommand = new RelayCommand(_ => Cut(), _ => CanCut());
            CopyCommand = new RelayCommand(_ => Copy(), _ => CanCopy());
            PasteCommand = new RelayCommand(_ => Paste());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => CanDelete());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            FindCommand = new RelayCommand(_ => Find());
            ReplaceCommand = new RelayCommand(_ => Replace(), _ => CanReplace());
            CloseCommand = new RelayCommand(_ => CloseDocument(), _ => CanCloseDocument());
            NewWindowCommand = new RelayCommand(_ => OpenNewWindow());
            CloseAllCommand = new RelayCommand(_ => CloseAllDocuments());
            FullScreenCommand = new RelayCommand(_ => ToggleFullScreen());
            MaximizeCommand = new RelayCommand(_ => MaximizeWindow());
            MinimizeCommand = new RelayCommand(_ => MinimizeWindow());
            NormalSizeCommand = new RelayCommand(_ => NormalWindowSize());
            AboutCommand = new RelayCommand(_ => ShowAbout());
            OptionsCommand = new RelayCommand(_ => ShowOptions());
        }

        private void InitializeRibbon()
        {
            RibbonTabs = new List<RibbonTab>
            {
                new RibbonTab { Title = "Home", Index = 0, Groups = CreateHomeGroups() },
                new RibbonTab { Title = "Insert", Index = 1, Groups = CreateInsertGroups() },
                new RibbonTab { Title = "Design", Index = 2, Groups = CreateDesignGroups() },
                new RibbonTab { Title = "Layout", Index = 3, Groups = CreateLayoutGroups() },
                new RibbonTab { Title = "Review", Index = 4, Groups = CreateReviewGroups() },
                new RibbonTab { Title = "View", Index = 5, Groups = CreateViewGroups() },
                new RibbonTab { Title = "File", Index = 6, Groups = CreateFileGroups() }
            };
        }

        private List<RibbonGroup> CreateHomeGroups()
        {
            return new List<RibbonGroup>
            {
                new RibbonGroup { Title = "Clipboard", Commands = GetClipboardCommands() },
                new RibbonGroup { Title = "Font", Commands = GetFontCommands() },
                new RibbonGroup { Title = "Paragraph", Commands = GetParagraphCommands() },
                new RibbonGroup { Title = "Styles", Commands = GetStyleCommands() },
                new RibbonGroup { Title = "Editing", Commands = GetEditingCommands() }
            };
        }

        private List<RibbonGroup> CreateInsertGroups()
        {
            return new List<RibbonGroup>
            {
                new RibbonGroup { Title = "Tables", Commands = GetTableCommands() },
                new RibbonGroup { Title = "Illustrations", Commands = GetIllustrationCommands() },
                new RibbonGroup { Title = "Links", Commands = GetLinkCommands() },
                new RibbonGroup { Title = "Comments", Commands = GetCommentCommands() },
                new RibbonGroup { Title = "Header & Footer", Commands = GetHeaderFooterCommands() }
            };
        }

        private List<RibbonGroup> CreateDesignGroups()
        {
            return new List<RibbonGroup>
            {
                new RibbonGroup { Title = "Document Formatting", Commands = GetDesignCommands() },
                new RibbonGroup { Title = "Page Background", Commands = GetBackgroundCommands() }
            };
        }

        private List<RibbonGroup> CreateLayoutGroups()
        {
            return new List<RibbonGroup>
            {
                new RibbonGroup { Title = "Page Setup", Commands = GetPageSetupCommands() },
                new RibbonGroup { Title = "Page Layout", Commands = GetPageLayoutCommands() },
                new RibbonGroup { Title = "Arrange", Commands = GetArrangeCommands() }
            };
        }

        private List<RibbonGroup> CreateReviewGroups()
        {
            return new List<RibbonGroup>
            {
                new RibbonGroup { Title = "Proofing", Commands = GetProofingCommands() },
                new RibbonGroup { Title = "Comments", Commands = GetReviewCommentCommands() },
                new RibbonGroup { Title = "Changes", Commands = GetChangesCommands() }
            };
        }

        private List<RibbonGroup> CreateViewGroups()
        {
            return new List<RibbonGroup>
            {
                new RibbonGroup { Title = "Document Views", Commands = GetViewCommands() },
                new RibbonGroup { Title = "Show/Hide", Commands = GetShowHideCommands() },
                new RibbonGroup { Title = "Zoom", Commands = GetZoomCommands() },
                new RibbonGroup { Title = "Window", Commands = GetWindowCommands() }
            };
        }

        private List<RibbonGroup> CreateFileGroups()
        {
            return new List<RibbonGroup>
            {
                new RibbonGroup { Title = "Info", Commands = GetInfoCommands() },
                new RibbonGroup { Title = "New", Commands = GetNewCommands() },
                new RibbonGroup { Title = "Open", Commands = GetOpenCommands() },
                new RibbonGroup { Title = "Save", Commands = GetSaveCommands() },
                new RibbonGroup { Title = "Export", Commands = GetExportCommands() },
                new RibbonGroup { Title = "Print", Commands = GetPrintCommands() },
                new RibbonGroup { Title = "Close", Commands = GetCloseCommands() }
            };
        }

        private List<CommandDefinition> GetClipboardCommands()
        {
            return new List<CommandDefinition>
            {
                new CommandDefinition { Id = "Paste", Name = "Paste", Category = "Clipboard" },
                new CommandDefinition { Id = "Cut", Name = "Cut", Category = "Clipboard" },
                new CommandDefinition { Id = "Copy", Name = "Copy", Category = "Clipboard" },
                new CommandDefinition { Id = "FormatPainter", Name = "Format Painter", Category = "Clipboard" }
            };
        }

        private List<CommandDefinition> GetFontCommands()
        {
            return new List<CommandDefinition>
            {
                new CommandDefinition { Id = "Bold", Name = "Bold", Category = "Font" },
                new CommandDefinition { Id = "Italic", Name = "Italic", Category = "Font" },
                new CommandDefinition { Id = "Underline", Name = "Underline", Category = "Font" },
                new CommandDefinition { Id = "Strikethrough", Name = "Strikethrough", Category = "Font" },
                new CommandDefinition { Id = "Subscript", Name = "Subscript", Category = "Font" },
                new CommandDefinition { Id = "Superscript", Name = "Superscript", Category = "Font" }
            };
        }

        private List<CommandDefinition> GetParagraphCommands()
        {
            return new List<CommandDefinition>
            {
                new CommandDefinition { Id = "AlignLeft", Name = "Align Left", Category = "Paragraph" },
                new CommandDefinition { Id = "AlignCenter", Name = "Center", Category = "Paragraph" },
                new CommandDefinition { Id = "AlignRight", Name = "Align Right", Category = "Paragraph" },
                new CommandDefinition { Id = "AlignJustify", Name = "Justify", Category = "Paragraph" },
                new CommandDefinition { Id = "Bullets", Name = "Bullets", Category = "Paragraph" },
                new CommandDefinition { Id = "Numbering", Name = "Numbering", Category = "Paragraph" }
            };
        }

        private List<CommandDefinition> GetStyleCommands()
        {
            return new List<CommandDefinition>
            {
                new CommandDefinition { Id = "Styles", Name = "Styles", Category = "Styles" },
                new CommandDefinition { Id = "SelectStyles", Name = "Select", Category = "Styles" }
            };
        }

        private List<CommandDefinition> GetEditingCommands()
        {
            return new List<CommandDefinition>
            {
                new CommandDefinition { Id = "Find", Name = "Find", Category = "Editing" },
                new CommandDefinition { Id = "Replace", Name = "Replace", Category = "Editing" },
                new CommandDefinition { Id = "Select", Name = "Select", Category = "Editing" }
            };
        }

        private List<CommandDefinition> GetTableCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetIllustrationCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetLinkCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetCommentCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetHeaderFooterCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetDesignCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetBackgroundCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetPageSetupCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetPageLayoutCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetArrangeCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetProofingCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetReviewCommentCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetChangesCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetViewCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetShowHideCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetZoomCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetWindowCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetInfoCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetNewCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetOpenCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetSaveCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetExportCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetPrintCommands() => new List<CommandDefinition>();
        private List<CommandDefinition> GetCloseCommands() => new List<CommandDefinition>();

        private void InitializeStatusBar()
        {
            StatusBarItems = new List<StatusBarItem>
            {
                new StatusBarItem { Id = "DocumentInfo", Text = "Document 1", Index = 0 },
                new StatusBarItem { Id = "PageInfo", Text = "Page 1 of 1", Index = 1 },
                new StatusBarItem { Id = "WordCount", Text = "Words: 0", Index = 2 },
                new StatusBarItem { Id = "Language", Text = "English (US)", Index = 3, Type = StatusBarItemType.Language },
                new StatusBarItem { Id = "Zoom", Text = "100%", Index = 4 }
            };
        }

        public void CreateNewDocument(DocumentType type)
        {
            CurrentDocumentType = type;
            IsDocumentOpen = true;

            switch (type)
            {
                case DocumentType.Word:
                    ActiveViewModel = WordEditorViewModel;
                    WordEditorViewModel.Initialize();
                    Title = "Word Document - Office Suite";
                    break;
                case DocumentType.Spreadsheet:
                    ActiveViewModel = SpreadsheetEditorViewModel;
                    SpreadsheetEditorViewModel.Initialize();
                    Title = "Spreadsheet - Office Suite";
                    break;
                case DocumentType.Presentation:
                    ActiveViewModel = PresentationEditorViewModel;
                    PresentationEditorViewModel.Initialize();
                    Title = "Presentation - Office Suite";
                    break;
            }

            StatusMessage = $"New {type} document created";
        }

        public void OpenDocument()
        {
        }

        public void SaveDocument()
        {
            StatusMessage = "Document saved";
        }

        public bool CanSaveDocument()
        {
            return IsDocumentOpen;
        }

        public void SaveDocumentAs()
        {
            StatusMessage = "Document saved as";
        }

        public void PrintDocument()
        {
            StatusMessage = "Printing document...";
        }

        public void ExitApplication()
        {
        }

        public void Undo()
        {
        }

        public bool CanUndo()
        {
            return IsDocumentOpen;
        }

        public void Redo()
        {
        }

        public bool CanRedo()
        {
            return IsDocumentOpen;
        }

        public void Cut()
        {
        }

        public bool CanCut()
        {
            return IsDocumentOpen;
        }

        public void Copy()
        {
        }

        public bool CanCopy()
        {
            return IsDocumentOpen;
        }

        public void Paste()
        {
        }

        public void Delete()
        {
        }

        public bool CanDelete()
        {
            return IsDocumentOpen;
        }

        public void SelectAll()
        {
        }

        public void Find()
        {
        }

        public void Replace()
        {
        }

        public bool CanReplace()
        {
            return IsDocumentOpen;
        }

        public void CloseDocument()
        {
            IsDocumentOpen = false;
            ActiveViewModel = null;
            Title = "Office Suite";
            StatusMessage = "Document closed";
        }

        public bool CanCloseDocument()
        {
            return IsDocumentOpen;
        }

        public void OpenNewWindow()
        {
        }

        public void CloseAllDocuments()
        {
            CloseDocument();
        }

        public void ToggleFullScreen()
        {
            IsFullScreen = !IsFullScreen;
        }

        public void MaximizeWindow()
        {
            IsMaximized = true;
        }

        public void MinimizeWindow()
        {
            IsMaximized = false;
        }

        public void NormalWindowSize()
        {
            WindowWidth = 1200;
            WindowHeight = 800;
            IsMaximized = false;
        }

        public void ShowAbout()
        {
            StatusMessage = "Office Suite Version 1.0";
        }

        public void ShowOptions()
        {
        }

        public void AddRecentDocument(DocumentInfo document)
        {
            if (!RecentDocuments.Any(d => d.FilePath == document.FilePath))
            {
                RecentDocuments.Insert(0, document);
                if (RecentDocuments.Count > 10)
                    RecentDocuments.RemoveAt(RecentDocuments.Count - 1);
            }
        }

        public void UpdateStatus(string message)
        {
            StatusMessage = message;
        }

        public override void Initialize()
        {
        }

        public override void Cleanup()
        {
            WordEditorViewModel?.Cleanup();
            SpreadsheetEditorViewModel?.Cleanup();
            PresentationEditorViewModel?.Cleanup();
        }
    }
}

namespace OfficeSuite.Shell.ViewModels
{
    using WordEditorViewModel = OfficeSuite.Word.ViewModels.WordEditorViewModel;
    using SpreadsheetEditorViewModel = OfficeSuite.Spreadsheet.ViewModels.SpreadsheetEditorViewModel;
    using PresentationEditorViewModel = OfficeSuite.Presentation.ViewModels.PresentationEditorViewModel;
}

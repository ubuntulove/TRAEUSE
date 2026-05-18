using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfficeSuite.Shell.ViewModels
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly Dictionary<string, ViewModelBase> _documents = new Dictionary<string, ViewModelBase>();
        private ViewModelBase _activeDocument;
        private string _statusMessage;
        private bool _isBusy;
        private string _busyMessage;

        public ViewModelBase ActiveDocument
        {
            get => _activeDocument;
            set
            {
                if (SetProperty(ref _activeDocument, value))
                {
                    OnPropertyChanged(nameof(HasActiveDocument));
                    OnActiveDocumentChanged();
                }
            }
        }

        public bool HasActiveDocument => _activeDocument != null;
        public string StatusMessage { get => _statusMessage; set => SetProperty(ref _statusMessage, value); }
        public bool IsBusy { get => _isBusy; set => SetProperty(ref _isBusy, value); }
        public string BusyMessage { get => _busyMessage; set => SetProperty(ref _busyMessage, value); }

        public ICommand NewDocumentCommand { get; }
        public ICommand OpenDocumentCommand { get; }
        public ICommand SaveDocumentCommand { get; }
        public ICommand SaveAsDocumentCommand { get; }
        public ICommand CloseDocumentCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand PasteCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand SelectAllCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand PrintCommand { get; }
        public ICommand PrintPreviewCommand { get; }
        public ICommand ExitCommand { get; }

        public ShellViewModel()
        {
            NewDocumentCommand = new RelayCommand(async _ => await NewDocumentAsync());
            OpenDocumentCommand = new RelayCommand(async _ => await OpenDocumentAsync());
            SaveDocumentCommand = new RelayCommand(async _ => await SaveDocumentAsync(), _ => CanSaveDocument());
            SaveAsDocumentCommand = new RelayCommand(async _ => await SaveDocumentAsAsync(), _ => CanSaveDocument());
            CloseDocumentCommand = new RelayCommand(async _ => await CloseDocumentAsync(), _ => CanCloseDocument());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo());
            CutCommand = new RelayCommand(_ => Cut(), _ => CanCut());
            CopyCommand = new RelayCommand(_ => Copy(), _ => CanCopy());
            PasteCommand = new RelayCommand(_ => Paste(), _ => CanPaste());
            DeleteCommand = new RelayCommand(_ => Delete(), _ => CanDelete());
            SelectAllCommand = new RelayCommand(_ => SelectAll(), _ => CanSelectAll());
            FindCommand = new RelayCommand(_ => Find(), _ => CanFind());
            ReplaceCommand = new RelayCommand(_ => Replace(), _ => CanReplace());
            PrintCommand = new RelayCommand(async _ => await PrintAsync(), _ => CanPrint());
            PrintPreviewCommand = new RelayCommand(_ => PrintPreview(), _ => CanPrint());
            ExitCommand = new RelayCommand(_ => Exit());
        }

        public event EventHandler<DocumentEventArgs> DocumentOpened;
        public event EventHandler<DocumentEventArgs> DocumentClosed;
        public event EventHandler<DocumentEventArgs> DocumentSaved;

        protected virtual void OnDocumentOpened(DocumentEventArgs e) => DocumentOpened?.Invoke(this, e);
        protected virtual void OnDocumentClosed(DocumentEventArgs e) => DocumentClosed?.Invoke(this, e);
        protected virtual void OnDocumentSaved(DocumentEventArgs e) => DocumentSaved?.Invoke(this, e);

        protected virtual async Task NewDocumentAsync()
        {
            IsBusy = true;
            BusyMessage = "Creating new document...";

            try
            {
                await Task.Delay(100);

                var document = CreateNewDocument();
                var id = Guid.NewGuid().ToString();
                _documents[id] = document;
                ActiveDocument = document;

                StatusMessage = "New document created";
                OnDocumentOpened(new DocumentEventArgs { DocumentId = id });
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual async Task OpenDocumentAsync()
        {
            IsBusy = true;
            BusyMessage = "Opening document...";

            try
            {
                await Task.Delay(100);
                StatusMessage = "Document opened";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual async Task SaveDocumentAsync()
        {
            if (!CanSaveDocument())
                return;

            IsBusy = true;
            BusyMessage = "Saving document...";

            try
            {
                await Task.Delay(100);
                StatusMessage = "Document saved";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual async Task SaveDocumentAsAsync()
        {
            if (!CanSaveDocument())
                return;

            IsBusy = true;
            BusyMessage = "Saving document as...";

            try
            {
                await Task.Delay(100);
                StatusMessage = "Document saved";
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual async Task CloseDocumentAsync()
        {
            if (!CanCloseDocument())
                return;

            IsBusy = true;
            BusyMessage = "Closing document...";

            try
            {
                await Task.Delay(100);

                var id = _documents.FirstOrDefault(x => x.Value == ActiveDocument).Key;
                _documents.Remove(id);
                ActiveDocument = _documents.Values.FirstOrDefault();

                StatusMessage = "Document closed";
                OnDocumentClosed(new DocumentEventArgs { DocumentId = id });
            }
            finally
            {
                IsBusy = false;
            }
        }

        protected virtual void Undo() { }
        protected virtual void Redo() { }
        protected virtual void Cut() { }
        protected virtual void Copy() { }
        protected virtual void Paste() { }
        protected virtual void Delete() { }
        protected virtual void SelectAll() { }
        protected virtual void Find() { }
        protected virtual void Replace() { }
        protected virtual async Task PrintAsync() { await Task.Delay(100); }
        protected virtual void PrintPreview() { }
        protected virtual void Exit() { }

        protected virtual bool CanSaveDocument() => ActiveDocument != null;
        protected virtual bool CanCloseDocument() => ActiveDocument != null;
        protected virtual bool CanUndo() => false;
        protected virtual bool CanRedo() => false;
        protected virtual bool CanCut() => false;
        protected virtual bool CanCopy() => false;
        protected virtual bool CanPaste() => false;
        protected virtual bool CanDelete() => false;
        protected virtual bool CanSelectAll() => false;
        protected virtual bool CanFind() => false;
        protected virtual bool CanReplace() => false;
        protected virtual bool CanPrint() => ActiveDocument != null;

        protected virtual ViewModelBase CreateNewDocument() => new ViewModelBase();
        protected virtual void OnActiveDocumentChanged() { }
    }

    public class DocumentEventArgs : EventArgs
    {
        public string DocumentId { get; set; }
        public string FilePath { get; set; }
        public bool IsModified { get; set; }
    }

    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        public virtual void Initialize() { }
        public virtual void Cleanup() { }
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

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object parameter) => _execute(parameter);
        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, Task> _execute;
        private readonly Predicate<object> _canExecute;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<object, Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    CommandManager.InvalidateRequerySuggested();
                    await _execute(parameter);
                }
                finally
                {
                    _isExecuting = false;
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    public class RibbonViewModel : ViewModelBase
    {
        private RibbonTabViewModel _selectedTab;
        private bool _isMinimized;
        private ObservableCollection<RibbonTabViewModel> _tabs = new ObservableCollection<RibbonTabViewModel>();

        public RibbonTabViewModel SelectedTab
        {
            get => _selectedTab;
            set => SetProperty(ref _selectedTab, value);
        }

        public bool IsMinimized
        {
            get => _isMinimized;
            set => SetProperty(ref _isMinimized, value);
        }

        public ObservableCollection<RibbonTabViewModel> Tabs
        {
            get => _tabs;
            set => SetProperty(ref _tabs, value);
        }

        public ICommand MinimizeCommand { get; }
        public ICommand MaximizeCommand { get; }
        public ICommand SelectTabCommand { get; }

        public RibbonViewModel()
        {
            MinimizeCommand = new RelayCommand(_ => IsMinimized = true);
            MaximizeCommand = new RelayCommand(_ => IsMinimized = false);
            SelectTabCommand = new RelayCommand(p => SelectedTab = p as RibbonTabViewModel);

            InitializeTabs();
        }

        protected virtual void InitializeTabs() { }
    }

    public class RibbonTabViewModel : ViewModelBase
    {
        private string _name;
        private string _title;
        private bool _isSelected;
        private ObservableCollection<RibbonGroupViewModel> _groups = new ObservableCollection<RibbonGroupViewModel>();

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
        public ObservableCollection<RibbonGroupViewModel> Groups { get => _groups; set => SetProperty(ref _groups, value); }
    }

    public class RibbonGroupViewModel : ViewModelBase
    {
        private string _name;
        private string _title;
        private ObservableCollection<RibbonItemViewModelBase> _items = new ObservableCollection<RibbonItemViewModelBase>();
        private bool _isCollapsed;
        private int _columnCount;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public ObservableCollection<RibbonItemViewModelBase> Items { get => _items; set => SetProperty(ref _items, value); }
        public bool IsCollapsed { get => _isCollapsed; set => SetProperty(ref _isCollapsed, value); }
        public int ColumnCount { get => _columnCount; set => SetProperty(ref _columnCount, value); }
    }

    public class RibbonItemViewModelBase : ViewModelBase
    {
        private string _name;
        private string _title;
        private string _description;
        private string _iconPath;
        private bool _isEnabled;
        private bool _isVisible;
        private bool _isPressed;
        private ToolTip _toolTip;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string IconPath { get => _iconPath; set => SetProperty(ref _iconPath, value); }
        public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
        public bool IsPressed { get => _isPressed; set => SetProperty(ref _isPressed, value); }
        public ToolTip ToolTip { get => _toolTip; set => SetProperty(ref _toolTip, value); }
    }

    public class RibbonButtonViewModel : RibbonItemViewModelBase
    {
        private ICommand _command;
        private object _commandParameter;

        public ICommand Command { get => _command; set => SetProperty(ref _command, value); }
        public object CommandParameter { get => _commandParameter; set => SetProperty(ref _commandParameter, value); }
    }

    public class RibbonToggleButtonViewModel : RibbonItemViewModelBase
    {
        private bool _isChecked;
        private ICommand _command;

        public bool IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }
        public ICommand Command { get => _command; set => SetProperty(ref _command, value); }
    }

    public class RibbonDropDownButtonViewModel : RibbonItemViewModelBase
    {
        private ObservableCollection<RibbonMenuItemViewModel> _items = new ObservableCollection<RibbonMenuItemViewModel>();
        private RibbonMenuItemViewModel _selectedItem;

        public ObservableCollection<RibbonMenuItemViewModel> Items { get => _items; set => SetProperty(ref _items, value); }
        public RibbonMenuItemViewModel SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
    }

    public class RibbonSplitButtonViewModel : RibbonItemViewModelBase
    {
        private ObservableCollection<RibbonMenuItemViewModel> _items = new ObservableCollection<RibbonMenuItemViewModel>();
        private RibbonMenuItemViewModel _selectedItem;
        private ICommand _command;

        public ObservableCollection<RibbonMenuItemViewModel> Items { get => _items; set => SetProperty(ref _items, value); }
        public RibbonMenuItemViewModel SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public ICommand Command { get => _command; set => SetProperty(ref _command, value); }
    }

    public class RibbonMenuItemViewModel : RibbonItemViewModelBase
    {
        private ObservableCollection<RibbonMenuItemViewModel> _items = new ObservableCollection<RibbonMenuItemViewModel>();
        private ICommand _command;
        private object _commandParameter;
        private KeyGesture _inputGesture;
        private bool _isCheckable;
        private bool _isChecked;
        private bool _isSeparator;

        public ObservableCollection<RibbonMenuItemViewModel> Items { get => _items; set => SetProperty(ref _items, value); }
        public ICommand Command { get => _command; set => SetProperty(ref _command, value); }
        public object CommandParameter { get => _commandParameter; set => SetProperty(ref _commandParameter, value); }
        public KeyGesture InputGesture { get => _inputGesture; set => SetProperty(ref _inputGesture, value); }
        public bool IsCheckable { get => _isCheckable; set => SetProperty(ref _isCheckable, value); }
        public bool IsChecked { get => _isChecked; set => SetProperty(ref _isChecked, value); }
        public bool IsSeparator { get => _isSeparator; set => SetProperty(ref _isSeparator, value); }
    }

    public class RibbonComboBoxViewModel : RibbonItemViewModelBase
    {
        private ObservableCollection<RibbonComboBoxItemViewModel> _items = new ObservableCollection<RibbonComboBoxItemViewModel>();
        private RibbonComboBoxItemViewModel _selectedItem;
        private string _text;
        private bool _isEditable;
        private int _selectedIndex;

        public ObservableCollection<RibbonComboBoxItemViewModel> Items { get => _items; set => SetProperty(ref _items, value); }
        public RibbonComboBoxItemViewModel SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public bool IsEditable { get => _isEditable; set => SetProperty(ref _isEditable, value); }
        public int SelectedIndex { get => _selectedIndex; set => SetProperty(ref _selectedIndex, value); }
    }

    public class RibbonComboBoxItemViewModel : ViewModelBase
    {
        private string _text;
        private string _value;
        private bool _isSelected;

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public string Value { get => _value; set => SetProperty(ref _value, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class RibbonTextBoxViewModel : RibbonItemViewModelBase
    {
        private string _text;
        private int _maxLength;
        private string _placeholderText;

        public string Text { get => _text; set => SetProperty(ref _text, value); }
        public int MaxLength { get => _maxLength; set => SetProperty(ref _maxLength, value); }
        public string PlaceholderText { get => _placeholderText; set => SetProperty(ref _placeholderText, value); }
    }

    public class RibbonGalleryViewModel : RibbonItemViewModelBase
    {
        private ObservableCollection<RibbonGalleryItemViewModel> _items = new ObservableCollection<RibbonGalleryItemViewModel>();
        private RibbonGalleryItemViewModel _selectedItem;
        private int _selectedIndex;
        private int _columns;
        private int _rows;

        public ObservableCollection<RibbonGalleryItemViewModel> Items { get => _items; set => SetProperty(ref _items, value); }
        public RibbonGalleryItemViewModel SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public int SelectedIndex { get => _selectedIndex; set => SetProperty(ref _selectedIndex, value); }
        public int Columns { get => _columns; set => SetProperty(ref _columns, value); }
        public int Rows { get => _rows; set => SetProperty(ref _rows, value); }
    }

    public class RibbonGalleryItemViewModel : ViewModelBase
    {
        private string _name;
        private string _iconPath;
        private string _tooltip;
        private bool _isSelected;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string IconPath { get => _iconPath; set => SetProperty(ref _iconPath, value); }
        public string Tooltip { get => _tooltip; set => SetProperty(ref _tooltip, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class ObservableCollection<T> : System.Collections.ObjectModel.ObservableCollection<T>
    {
        public ObservableCollection() { }
        public ObservableCollection(IEnumerable<T> collection) : base(collection) { }
    }

    public class ToolTip
    {
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class KeyGesture
    {
        public Key Key { get; set; }
        public ModifierKeys Modifiers { get; set; }

        public KeyGesture(Key key, ModifierKeys modifiers = ModifierKeys.None)
        {
            Key = key;
            Modifiers = modifiers;
        }

        public override string ToString()
        {
            var parts = new List<string>();
            if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            parts.Add(Key.ToString());
            return string.Join("+", parts);
        }
    }

    public class StatusBarViewModel : ViewModelBase
    {
        private string _leftStatusText;
        private string _centerStatusText;
        private string _rightStatusText;
        private bool _showLeft;
        private bool _showCenter;
        private bool _showRight;
        private ICommand _leftClickCommand;
        private ICommand _centerClickCommand;
        private ICommand _rightClickCommand;

        public string LeftStatusText { get => _leftStatusText; set => SetProperty(ref _leftStatusText, value); }
        public string CenterStatusText { get => _centerStatusText; set => SetProperty(ref _centerStatusText, value); }
        public string RightStatusText { get => _rightStatusText; set => SetProperty(ref _rightStatusText, value); }
        public bool ShowLeft { get => _showLeft; set => SetProperty(ref _showLeft, value); }
        public bool ShowCenter { get => _showCenter; set => SetProperty(ref _showCenter, value); }
        public bool ShowRight { get => _showRight; set => SetProperty(ref _showRight, value); }
        public ICommand LeftClickCommand { get => _leftClickCommand; set => SetProperty(ref _leftClickCommand, value); }
        public ICommand CenterClickCommand { get => _centerClickCommand; set => SetProperty(ref _centerClickCommand, value); }
        public ICommand RightClickCommand { get => _rightClickCommand; set => SetProperty(ref _rightClickCommand, value); }
    }

    public class BackstageViewModel : ViewModelBase
    {
        private bool _isOpen;
        private ObservableCollection<BackstageTabViewModel> _tabs = new ObservableCollection<BackstageTabViewModel>();
        private BackstageTabViewModel _selectedTab;

        public bool IsOpen { get => _isOpen; set => SetProperty(ref _isOpen, value); }
        public ObservableCollection<BackstageTabViewModel> Tabs { get => _tabs; set => SetProperty(ref _tabs, value); }
        public BackstageTabViewModel SelectedTab { get => _selectedTab; set => SetProperty(ref _selectedTab, value); }
    }

    public class BackstageTabViewModel : ViewModelBase
    {
        private string _name;
        private string _title;
        private string _iconPath;
        private ObservableCollection<BackstageItemViewModel> _items = new ObservableCollection<BackstageItemViewModel>();

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string IconPath { get => _iconPath; set => SetProperty(ref _iconPath, value); }
        public ObservableCollection<BackstageItemViewModel> Items { get => _items; set => SetProperty(ref _items, value); }
    }

    public class BackstageItemViewModel : ViewModelBase
    {
        private string _title;
        private string _description;
        private ICommand _command;
        private bool _isEnabled;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public ICommand Command { get => _command; set => SetProperty(ref _command, value); }
        public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
    }

    public class NavigationPaneViewModel : ViewModelBase
    {
        private ObservableCollection<NavigationItemViewModel> _items = new ObservableCollection<NavigationItemViewModel>();
        private NavigationItemViewModel _selectedItem;
        private bool _isExpanded;
        private int _width;

        public ObservableCollection<NavigationItemViewModel> Items { get => _items; set => SetProperty(ref _items, value); }
        public NavigationItemViewModel SelectedItem { get => _selectedItem; set => SetProperty(ref _selectedItem, value); }
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
        public int Width { get => _width; set => SetProperty(ref _width, value); }
    }

    public class NavigationItemViewModel : ViewModelBase
    {
        private string _name;
        private string _title;
        private string _iconPath;
        private ObservableCollection<NavigationItemViewModel> _children = new ObservableCollection<NavigationItemViewModel>();
        private bool _isExpanded;
        private bool _isSelected;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string IconPath { get => _iconPath; set => SetProperty(ref _iconPath, value); }
        public ObservableCollection<NavigationItemViewModel> Children { get => _children; set => SetProperty(ref _children, value); }
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
        public bool IsSelected { get => _isSelected; set => SetProperty(ref _isSelected, value); }
    }

    public class QuickAccessToolbarViewModel : ViewModelBase
    {
        private ObservableCollection<QuickAccessItemViewModel> _items = new ObservableCollection<QuickAccessItemViewModel>();
        private bool _showBelowRibbon;

        public ObservableCollection<QuickAccessItemViewModel> Items { get => _items; set => SetProperty(ref _items, value); }
        public bool ShowBelowRibbon { get => _showBelowRibbon; set => SetProperty(ref _showBelowRibbon, value); }
    }

    public class QuickAccessItemViewModel : ViewModelBase
    {
        private string _name;
        private string _iconPath;
        private ICommand _command;
        private string _tooltip;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string IconPath { get => _iconPath; set => SetProperty(ref _iconPath, value); }
        public ICommand Command { get => _command; set => SetProperty(ref _command, value); }
        public string Tooltip { get => _tooltip; set => SetProperty(ref _tooltip, value); }
    }

    public class DialogViewModel : ViewModelBase
    {
        private string _title;
        private bool _isOpen;
        private object _content;
        private ObservableCollection<DialogButtonViewModel> _buttons = new ObservableCollection<DialogButtonViewModel>();

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public bool IsOpen { get => _isOpen; set => SetProperty(ref _isOpen, value); }
        public object Content { get => _content; set => SetProperty(ref _content, value); }
        public ObservableCollection<DialogButtonViewModel> Buttons { get => _buttons; set => SetProperty(ref _buttons, value); }

        public event EventHandler<DialogResultEventArgs> DialogClosed;

        protected virtual void OnDialogClosed(DialogResultEventArgs e) => DialogClosed?.Invoke(this, e);
    }

    public class DialogButtonViewModel : ViewModelBase
    {
        private string _content;
        private string _name;
        private ButtonStyle _style;
        private ICommand _command;
        private bool _isDefault;
        private bool _isCancel;

        public string Content { get => _content; set => SetProperty(ref _content, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public ButtonStyle Style { get => _style; set => SetProperty(ref _style, value); }
        public ICommand Command { get => _command; set => SetProperty(ref _command, value); }
        public bool IsDefault { get => _isDefault; set => SetProperty(ref _isDefault, value); }
        public bool IsCancel { get => _isCancel; set => SetProperty(ref _isCancel, value); }
    }

    public enum ButtonStyle { Primary, Secondary, Cancel }

    public class DialogResultEventArgs : EventArgs
    {
        public string ButtonName { get; set; }
        public bool? Result { get; set; }
    }

    public class PropertyWindowViewModel : ViewModelBase
    {
        private ObservableCollection<PropertyCategoryViewModel> _categories = new ObservableCollection<PropertyCategoryViewModel>();
        private PropertyCategoryViewModel _selectedCategory;
        private object _selectedObject;

        public ObservableCollection<PropertyCategoryViewModel> Categories { get => _categories; set => SetProperty(ref _categories, value); }
        public PropertyCategoryViewModel SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
        public object SelectedObject { get => _selectedObject; set => SetProperty(ref _selectedObject, value); }
    }

    public class PropertyCategoryViewModel : ViewModelBase
    {
        private string _name;
        private string _displayName;
        private ObservableCollection<PropertyItemViewModel> _properties = new ObservableCollection<PropertyItemViewModel>();
        private bool _isExpanded;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        public ObservableCollection<PropertyItemViewModel> Properties { get => _properties; set => SetProperty(ref _properties, value); }
        public bool IsExpanded { get => _isExpanded; set => SetProperty(ref _isExpanded, value); }
    }

    public class PropertyItemViewModel : ViewModelBase
    {
        private string _name;
        private string _displayName;
        private object _value;
        private Type _valueType;
        private bool _isReadOnly;
        private string _category;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        public object Value { get => _value; set => SetProperty(ref _value, value); }
        public Type ValueType { get => _valueType; set => SetProperty(ref _valueType, value); }
        public bool IsReadOnly { get => _isReadOnly; set => SetProperty(ref _isReadOnly, value); }
        public string Category { get => _category; set => SetProperty(ref _category, value); }
    }

    public class TaskPaneViewModel : ViewModelBase
    {
        private string _title;
        private object _content;
        private bool _isVisible;
        private DockPosition _dockPosition;
        private double _width;
        private double _height;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public object Content { get => _content; set => SetProperty(ref _content, value); }
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
        public DockPosition DockPosition { get => _dockPosition; set => SetProperty(ref _dockPosition, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
    }

    public enum DockPosition { Left, Right, Top, Bottom, Float }

    public class NotificationViewModel : ViewModelBase
    {
        private string _title;
        private string _message;
        private NotificationType _type;
        private bool _isVisible;
        private int _timeout;

        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Message { get => _message; set => SetProperty(ref _message, value); }
        public NotificationType Type { get => _type; set => SetProperty(ref _type, value); }
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
        public int Timeout { get => _timeout; set => SetProperty(ref _timeout, value); }
    }

    public enum NotificationType { Info, Success, Warning, Error }

    public class ProgressDialogViewModel : DialogViewModel
    {
        private string _progressText;
        private double _progressValue;
        private bool _isIndeterminate;
        private bool _cancelRequested;
        private ICommand _cancelCommand;

        public string ProgressText { get => _progressText; set => SetProperty(ref _progressText, value); }
        public double ProgressValue { get => _progressValue; set => SetProperty(ref _progressValue, value); }
        public bool IsIndeterminate { get => _isIndeterminate; set => SetProperty(ref _isIndeterminate, value); }
        public bool CancelRequested { get => _cancelRequested; set => SetProperty(ref _cancelRequested, value); }
        public ICommand CancelCommand { get => _cancelCommand; set => SetProperty(ref _cancelCommand, value); }
    }

    public class FindReplaceViewModel : ViewModelBase
    {
        private string _searchText;
        private string _replaceText;
        private bool _matchCase;
        private bool _matchWholeWord;
        private bool _searchUp;
        private bool _useWildcards;
        private bool _useRegularExpression;
        private int _matchCount;
        private ICommand _findNextCommand;
        private ICommand _replaceCommand;
        private ICommand _replaceAllCommand;
        private ICommand _closeCommand;

        public string SearchText { get => _searchText; set => SetProperty(ref _searchText, value); }
        public string ReplaceText { get => _replaceText; set => SetProperty(ref _replaceText, value); }
        public bool MatchCase { get => _matchCase; set => SetProperty(ref _matchCase, value); }
        public bool MatchWholeWord { get => _matchWholeWord; set => SetProperty(ref _matchWholeWord, value); }
        public bool SearchUp { get => _searchUp; set => SetProperty(ref _searchUp, value); }
        public bool UseWildcards { get => _useWildcards; set => SetProperty(ref _useWildcards, value); }
        public bool UseRegularExpression { get => _useRegularExpression; set => SetProperty(ref _useRegularExpression, value); }
        public int MatchCount { get => _matchCount; set => SetProperty(ref _matchCount, value); }
        public ICommand FindNextCommand { get => _findNextCommand; set => SetProperty(ref _findNextCommand, value); }
        public ICommand ReplaceCommand { get => _replaceCommand; set => SetProperty(ref _replaceCommand, value); }
        public ICommand ReplaceAllCommand { get => _replaceAllCommand; set => SetProperty(ref _replaceAllCommand, value); }
        public ICommand CloseCommand { get => _closeCommand; set => SetProperty(ref _closeCommand, value); }
    }

    public class GoToViewModel : ViewModelBase
    {
        private GoToType _gotoType;
        private int _pageNumber;
        private int _lineNumber;
        private int _bookmarkIndex;
        private string _bookmarkName;
        private ICommand _goCommand;
        private ICommand _closeCommand;

        public GoToType GotoType { get => _gotoType; set => SetProperty(ref _gotoType, value); }
        public int PageNumber { get => _pageNumber; set => SetProperty(ref _pageNumber, value); }
        public int LineNumber { get => _lineNumber; set => SetProperty(ref _lineNumber, value); }
        public int BookmarkIndex { get => _bookmarkIndex; set => SetProperty(ref _bookmarkIndex, value); }
        public string BookmarkName { get => _bookmarkName; set => SetProperty(ref _bookmarkName, value); }
        public ICommand GoCommand { get => _goCommand; set => SetProperty(ref _goCommand, value); }
        public ICommand CloseCommand { get => _closeCommand; set => SetProperty(ref _closeCommand, value); }
    }

    public enum GoToType { Page, Line, Section, Bookmark, Comment, Footnote, Endnote, Table, Graphic, Equation, Object }

    public class SpellingViewModel : ViewModelBase
    {
        private string _currentWord;
        private string _suggestion;
        private ObservableCollection<string> _suggestions = new ObservableCollection<string>();
        private int _suggestionIndex;
        private bool _ignoreOnce;
        private bool _ignoreAll;
        private bool _addToDictionary;
        private ICommand _ignoreOnceCommand;
        private ICommand _ignoreAllCommand;
        private ICommand _AddToDictionaryCommand;
        private ICommand _ChangeCommand;
        private ICommand _ChangeAllCommand;
        private ICommand _CloseCommand;

        public string CurrentWord { get => _currentWord; set => SetProperty(ref _currentWord, value); }
        public string Suggestion { get => _suggestion; set => SetProperty(ref _suggestion, value); }
        public ObservableCollection<string> Suggestions { get => _suggestions; set => SetProperty(ref _suggestions, value); }
        public int SuggestionIndex { get => _suggestionIndex; set => SetProperty(ref _suggestionIndex, value); }
        public bool IgnoreOnce { get => _ignoreOnce; set => SetProperty(ref _ignoreOnce, value); }
        public bool IgnoreAll { get => _ignoreAll; set => SetProperty(ref _ignoreAll, value); }
        public bool AddToDictionary { get => _addToDictionary; set => SetProperty(ref _addToDictionary, value); }
        public ICommand IgnoreOnceCommand { get => _ignoreOnceCommand; set => SetProperty(ref _ignoreOnceCommand, value); }
        public ICommand IgnoreAllCommand { get => _ignoreAllCommand; set => SetProperty(ref _ignoreAllCommand, value); }
        public ICommand AddToDictionaryCommand { get => _addToDictionaryCommand; set => SetProperty(ref _addToDictionaryCommand, value); }
        public ICommand ChangeCommand { get => _changeCommand; set => SetProperty(ref _changeCommand, value); }
        public ICommand ChangeAllCommand { get => _changeAllCommand; set => SetProperty(ref _changeAllCommand, value); }
        public ICommand CloseCommand { get => _closeCommand; set => SetProperty(ref _closeCommand, value); }
    }

    public class PrintViewModel : ViewModelBase
    {
        private string _printerName;
        private ObservableCollection<string> _printers = new ObservableCollection<string>();
        private int _copies;
        private bool _collate;
        private PaperSize _paperSize;
        private PaperOrientation _orientation;
        private string _printRange;
        private int _fromPage;
        private int _toPage;
        private bool _printToFile;
        private string _printToFileName;
        private ICommand _printCommand;
        private ICommand _cancelCommand;
        private ICommand _propertiesCommand;

        public string PrinterName { get => _printerName; set => SetProperty(ref _printerName, value); }
        public ObservableCollection<string> Printers { get => _printers; set => SetProperty(ref _printers, value); }
        public int Copies { get => _copies; set => SetProperty(ref _copies, value); }
        public bool Collate { get => _collate; set => SetProperty(ref _collate, value); }
        public PaperSize PaperSize { get => _paperSize; set => SetProperty(ref _paperSize, value); }
        public PaperOrientation Orientation { get => _orientation; set => SetProperty(ref _orientation, value); }
        public string PrintRange { get => _printRange; set => SetProperty(ref _printRange, value); }
        public int FromPage { get => _fromPage; set => SetProperty(ref _fromPage, value); }
        public int ToPage { get => _toPage; set => SetProperty(ref _toPage, value); }
        public bool PrintToFile { get => _printToFile; set => SetProperty(ref _printToFile, value); }
        public string PrintToFileName { get => _printToFileName; set => SetProperty(ref _printToFileName, value); }
        public ICommand PrintCommand { get => _printCommand; set => SetProperty(ref _printCommand, value); }
        public ICommand CancelCommand { get => _cancelCommand; set => SetProperty(ref _cancelCommand, value); }
        public ICommand PropertiesCommand { get => _propertiesCommand; set => SetProperty(ref _propertiesCommand, value); }
    }

    public class PageSetupViewModel : ViewModelBase
    {
        private PaperSize _paperSize;
        private PaperOrientation _orientation;
        private double _pageWidth;
        private double _pageHeight;
        private double _topMargin;
        private double _bottomMargin;
        private double _leftMargin;
        private double _rightMargin;
        private double _headerMargin;
        private double _footerMargin;
        private int _pageNumberStart;
        private PageNumberFormat _pageNumberFormat;
        private bool _differentFirstPage;
        private bool _differentOddAndEvenPages;
        private ICommand _okCommand;
        private ICommand _CancelCommand;

        public PaperSize PaperSize { get => _paperSize; set => SetProperty(ref _paperSize, value); }
        public PaperOrientation Orientation { get => _orientation; set => SetProperty(ref _orientation, value); }
        public double PageWidth { get => _pageWidth; set => SetProperty(ref _pageWidth, value); }
        public double PageHeight { get => _pageHeight; set => SetProperty(ref _pageHeight, value); }
        public double TopMargin { get => _topMargin; set => SetProperty(ref _topMargin, value); }
        public double BottomMargin { get => _bottomMargin; set => SetProperty(ref _bottomMargin, value); }
        public double LeftMargin { get => _leftMargin; set => SetProperty(ref _leftMargin, value); }
        public double RightMargin { get => _rightMargin; set => SetProperty(ref _rightMargin, value); }
        public double HeaderMargin { get => _headerMargin; set => SetProperty(ref _headerMargin, value); }
        public double FooterMargin { get => _footerMargin; set => SetProperty(ref _footerMargin, value); }
        public int PageNumberStart { get => _pageNumberStart; set => SetProperty(ref _pageNumberStart, value); }
        public PageNumberFormat PageNumberFormat { get => _pageNumberFormat; set => SetProperty(ref _pageNumberFormat, value); }
        public bool DifferentFirstPage { get => _differentFirstPage; set => SetProperty(ref _differentFirstPage, value); }
        public bool DifferentOddAndEvenPages { get => _differentOddAndEvenPages; set => SetProperty(ref _differentOddAndEvenPages, value); }
        public ICommand OkCommand { get => _okCommand; set => SetProperty(ref _okCommand, value); }
        public ICommand CancelCommand { get => _cancelCommand; set => SetProperty(ref _cancelCommand, value); }
    }

    public enum PaperSize { Letter, Legal, A4, A5, B5, Executive, Envelope10, EnvelopeDL, EnvelopeMonarch }
    public enum PaperOrientation { Portrait, Landscape }
    public enum PageNumberFormat { Arabic, RomanUpper, RomanLower, LetterUpper, LetterLower }

    public class MacroDialogViewModel : ViewModelBase
    {
        private ObservableCollection<MacroItemViewModel> _macros = new ObservableCollection<MacroItemViewModel>();
        private MacroItemViewModel _selectedMacro;
        private string _macroName;
        private bool _editMode;
        private ICommand _runCommand;
        private ICommand _createCommand;
        private ICommand _deleteCommand;
        private ICommand _editCommand;
        private ICommand _closeCommand;

        public ObservableCollection<MacroItemViewModel> Macros { get => _macros; set => SetProperty(ref _macros, value); }
        public MacroItemViewModel SelectedMacro { get => _selectedMacro; set => SetProperty(ref _selectedMacro, value); }
        public string MacroName { get => _macroName; set => SetProperty(ref _macroName, value); }
        public bool EditMode { get => _editMode; set => SetProperty(ref _editMode, value); }
        public ICommand RunCommand { get => _runCommand; set => SetProperty(ref _runCommand, value); }
        public ICommand CreateCommand { get => _createCommand; set => SetProperty(ref _createCommand, value); }
        public ICommand DeleteCommand { get => _deleteCommand; set => SetProperty(ref _deleteCommand, value); }
        public ICommand EditCommand { get => _editCommand; set => SetProperty(ref _editCommand, value); }
        public ICommand CloseCommand { get => _closeCommand; set => SetProperty(ref _closeCommand, value); }
    }

    public class MacroItemViewModel : ViewModelBase
    {
        private string _name;
        private string _description;
        private string _moduleName;
        private bool _isEnabled;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string ModuleName { get => _moduleName; set => SetProperty(ref _moduleName, value); }
        public bool IsEnabled { get => _isEnabled; set => SetProperty(ref _isEnabled, value); }
    }

    public class AboutViewModel : ViewModelBase
    {
        private string _productName;
        private string _version;
        private string _copyright;
        private string _description;
        private string _logoPath;
        private ICommand _closeCommand;
        private ICommand _visitWebsiteCommand;
        private ICommand _sendFeedbackCommand;

        public string ProductName { get => _productName; set => SetProperty(ref _productName, value); }
        public string Version { get => _version; set => SetProperty(ref _version, value); }
        public string Copyright { get => _copyright; set => SetProperty(ref _copyright, value); }
        public string Description { get => _description; set => SetProperty(ref _description, value); }
        public string LogoPath { get => _logoPath; set => SetProperty(ref _logoPath, value); }
        public ICommand CloseCommand { get => _closeCommand; set => SetProperty(ref _closeCommand, value); }
        public ICommand VisitWebsiteCommand { get => _visitWebsiteCommand; set => SetProperty(ref _visitWebsiteCommand, value); }
        public ICommand SendFeedbackCommand { get => _sendFeedbackCommand; set => SetProperty(ref _sendFeedbackCommand, value); }
    }

    public class OptionsViewModel : ViewModelBase
    {
        private ObservableCollection<OptionsCategoryViewModel> _categories = new ObservableCollection<OptionsCategoryViewModel>();
        private OptionsCategoryViewModel _selectedCategory;
        private string _title;
        private ICommand _okCommand;
        private ICommand _CancelCommand;
        private ICommand _ApplyCommand;

        public ObservableCollection<OptionsCategoryViewModel> Categories { get => _categories; set => SetProperty(ref _categories, value); }
        public OptionsCategoryViewModel SelectedCategory { get => _selectedCategory; set => SetProperty(ref _selectedCategory, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public ICommand OkCommand { get => _okCommand; set => SetProperty(ref _okCommand, value); }
        public ICommand CancelCommand { get => _cancelCommand; set => SetProperty(ref _cancelCommand, value); }
        public ICommand ApplyCommand { get => _applyCommand; set => SetProperty(ref _applyCommand, value); }
    }

    public class OptionsCategoryViewModel : ViewModelBase
    {
        private string _name;
        private string _displayName;
        private string _iconPath;
        private object _content;

        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string DisplayName { get => _displayName; set => SetProperty(ref _displayName, value); }
        public string IconPath { get => _iconPath; set => SetProperty(ref _iconPath, value); }
        public object Content { get => _content; set => SetProperty(ref _content, value); }
    }
}

namespace OfficeSuite.Shell.Services
{
    public interface INavigationService
    {
        bool CanGoBack { get; }
        bool CanGoForward { get; }
        void Navigate(string viewName, object parameter = null);
        void NavigateToDocument(string documentId);
        void GoBack();
        void GoForward();
        void ClearHistory();
        event EventHandler<NavigationEventArgs> Navigated;
        event EventHandler<NavigationEventArgs> NavigationFailed;
    }

    public class NavigationService : INavigationService
    {
        private readonly Stack<NavigationEntry> _backStack = new Stack<NavigationEntry>();
        private readonly Stack<NavigationEntry> _forwardStack = new Stack<NavigationEntry>();
        private NavigationEntry _currentEntry;

        public bool CanGoBack => _backStack.Count > 0;
        public bool CanGoForward => _forwardStack.Count > 0;

        public event EventHandler<NavigationEventArgs> Navigated;
        public event EventHandler<NavigationEventArgs> NavigationFailed;

        public void Navigate(string viewName, object parameter = null)
        {
            try
            {
                if (_currentEntry != null)
                    _backStack.Push(_currentEntry);

                _forwardStack.Clear();

                _currentEntry = new NavigationEntry
                {
                    ViewName = viewName,
                    Parameter = parameter,
                    Timestamp = DateTime.Now
                };

                OnNavigated(new NavigationEventArgs { ViewName = viewName, Parameter = parameter });
            }
            catch (Exception ex)
            {
                OnNavigationFailed(new NavigationEventArgs { ViewName = viewName, Parameter = parameter, Error = ex });
            }
        }

        public void NavigateToDocument(string documentId)
        {
            Navigate("Document", documentId);
        }

        public void GoBack()
        {
            if (!CanGoBack)
                return;

            if (_currentEntry != null)
                _forwardStack.Push(_currentEntry);

            _currentEntry = _backStack.Pop();
            OnNavigated(new NavigationEventArgs { ViewName = _currentEntry.ViewName, Parameter = _currentEntry.Parameter });
        }

        public void GoForward()
        {
            if (!CanGoForward)
                return;

            if (_currentEntry != null)
                _backStack.Push(_currentEntry);

            _currentEntry = _forwardStack.Pop();
            OnNavigated(new NavigationEventArgs { ViewName = _currentEntry.ViewName, Parameter = _currentEntry.Parameter });
        }

        public void ClearHistory()
        {
            _backStack.Clear();
            _forwardStack.Clear();
        }

        protected virtual void OnNavigated(NavigationEventArgs e) => Navigated?.Invoke(this, e);
        protected virtual void OnNavigationFailed(NavigationEventArgs e) => NavigationFailed?.Invoke(this, e);
    }

    public class NavigationEntry
    {
        public string ViewName { get; set; }
        public object Parameter { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class NavigationEventArgs : EventArgs
    {
        public string ViewName { get; set; }
        public object Parameter { get; set; }
        public Exception Error { get; set; }
    }

    public interface IDialogService
    {
        Task<MessageBoxResult> ShowMessageBoxAsync(string message, string caption, MessageBoxButton button, MessageBoxImage icon);
        Task<string> ShowOpenFileDialogAsync(string filter, string initialDirectory = null);
        Task<string> ShowSaveFileDialogAsync(string filter, string defaultFileName = null, string initialDirectory = null);
        Task<string> ShowFolderBrowserDialogAsync(string description = null);
        Task<OptionsDialogResult> ShowOptionsDialogAsync(string title, IEnumerable<OptionsCategoryViewModel> categories);
        Task<bool> ShowConfirmDialogAsync(string message, string caption);
    }

    public class DialogService : IDialogService
    {
        public Task<MessageBoxResult> ShowMessageBoxAsync(string message, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Task.FromResult(MessageBoxResult.OK);
        }

        public Task<string> ShowOpenFileDialogAsync(string filter, string initialDirectory = null)
        {
            return Task.FromResult<string>(null);
        }

        public Task<string> ShowSaveFileDialogAsync(string filter, string defaultFileName = null, string initialDirectory = null)
        {
            return Task.FromResult<string>(null);
        }

        public Task<string> ShowFolderBrowserDialogAsync(string description = null)
        {
            return Task.FromResult<string>(null);
        }

        public Task<OptionsDialogResult> ShowOptionsDialogAsync(string title, IEnumerable<OptionsCategoryViewModel> categories)
        {
            return Task.FromResult(new OptionsDialogResult());
        }

        public Task<bool> ShowConfirmDialogAsync(string message, string caption)
        {
            return Task.FromResult(true);
        }
    }

    public class MessageBoxResult
    {
        public static readonly MessageBoxResult OK = new MessageBoxResult();
        public static readonly MessageBoxResult Cancel = new MessageBoxResult();
        public static readonly MessageBoxResult Yes = new MessageBoxResult();
        public static readonly MessageBoxResult No = new MessageBoxResult();
    }

    public enum MessageBoxButton { OK, OKCancel, YesNo, YesNoCancel }
    public enum MessageBoxImage { None, Information, Question, Warning, Error }

    public class OptionsDialogResult
    {
        public bool? DialogResult { get; set; }
        public object Settings { get; set; }
    }

    public interface IDocumentService
    {
        Task<Document> CreateDocumentAsync(DocumentType type);
        Task<Document> OpenDocumentAsync(string filePath);
        Task SaveDocumentAsync(Document document, string filePath);
        Task SaveDocumentAsAsync(Document document, string filePath);
        Task CloseDocumentAsync(Document document);
        Task<Document> ImportDocumentAsync(string filePath);
        Task ExportDocumentAsync(Document document, string filePath, ExportFormat format);
    }

    public enum DocumentType { Word, Excel, PowerPoint }
    public enum ExportFormat { PDF, XPS, DOCX, XLSX, PPTX, HTML, RTF, TXT }

    public class Document { }
    public class DocumentService : IDocumentService
    {
        public Task<Document> CreateDocumentAsync(DocumentType type) => Task.FromResult(new Document());
        public Task<Document> OpenDocumentAsync(string filePath) => Task.FromResult<Document>(null);
        public Task SaveDocumentAsync(Document document, string filePath) => Task.CompletedTask;
        public Task SaveDocumentAsAsync(Document document, string filePath) => Task.CompletedTask;
        public Task CloseDocumentAsync(Document document) => Task.CompletedTask;
        public Task<Document> ImportDocumentAsync(string filePath) => Task.FromResult<Document>(null);
        public Task ExportDocumentAsync(Document document, string filePath, ExportFormat format) => Task.CompletedTask;
    }

    public interface IThemeService
    {
        string CurrentTheme { get; }
        string CurrentAccentColor { get; set; }
        void ApplyTheme(string themeName);
        void ApplyAccentColor(string colorName);
        event EventHandler<ThemeChangedEventArgs> ThemeChanged;
    }

    public class ThemeService : IThemeService
    {
        public string CurrentTheme { get; private set; } = "Light";
        public string CurrentAccentColor { get; set; } = "Blue";

        public event EventHandler<ThemeChangedEventArgs> ThemeChanged;

        public void ApplyTheme(string themeName)
        {
            CurrentTheme = themeName;
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs { ThemeName = themeName });
        }

        public void ApplyAccentColor(string colorName)
        {
            CurrentAccentColor = colorName;
            ThemeChanged?.Invoke(this, new ThemeChangedEventArgs { AccentColor = colorName });
        }
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public string ThemeName { get; set; }
        public string AccentColor { get; set; }
    }

    public interface ISettingsService
    {
        T GetSetting<T>(string key, T defaultValue = default);
        void SetSetting<T>(string key, T value);
        void SaveSettings();
        void LoadSettings();
        void ResetToDefaults();
    }

    public class SettingsService : ISettingsService
    {
        private readonly Dictionary<string, object> _settings = new Dictionary<string, object>();

        public T GetSetting<T>(string key, T defaultValue = default)
        {
            return _settings.TryGetValue(key, out var value) ? (T)value : defaultValue;
        }

        public void SetSetting<T>(string key, T value)
        {
            _settings[key] = value;
        }

        public void SaveSettings() { }
        public void LoadSettings() { }
        public void ResetToDefaults() { _settings.Clear(); }
    }

    public interface ILoggingService
    {
        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message, Exception exception = null);
        void LogDebug(string message);
    }

    public class LoggingService : ILoggingService
    {
        public void LogInfo(string message) { }
        public void LogWarning(string message) { }
        public void LogError(string message, Exception exception = null) { }
        public void LogDebug(string message) { }
    }

    public interface INotificationService
    {
        void ShowNotification(string title, string message, NotificationType type);
        void ShowProgress(string title, double progress);
        void UpdateProgress(double progress);
        void HideProgress();
    }

    public enum NotificationType { Info, Success, Warning, Error }

    public class NotificationService : INotificationService
    {
        public void ShowNotification(string title, string message, NotificationType type) { }
        public void ShowProgress(string title, double progress) { }
        public void UpdateProgress(double progress) { }
        public void HideProgress() { }
    }
}

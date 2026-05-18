using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace OfficeSuite.Common.Ribbon
{
    public class RibbonViewModel : INotifyPropertyChanged
    {
        private bool _isMinimized;
        private string _selectedTab = "Home";
        private bool _isQuickAccessToolbarVisible = true;
        private bool _isRibbonVisible = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<RibbonTabViewModel> Tabs { get; } = new();
        public ObservableCollection<RibbonCommand> QuickAccessToolbarCommands { get; } = new();
        public ObservableCollection<RibbonCommand> RecentDocuments { get; } = new();

        public bool IsMinimized
        {
            get => _isMinimized;
            set { _isMinimized = value; OnPropertyChanged(nameof(IsMinimized)); }
        }

        public string SelectedTab
        {
            get => _selectedTab;
            set { _selectedTab = value; OnPropertyChanged(nameof(SelectedTab)); }
        }

        public bool IsQuickAccessToolbarVisible
        {
            get => _isQuickAccessToolbarVisible;
            set { _isQuickAccessToolbarVisible = value; OnPropertyChanged(nameof(IsQuickAccessToolbarVisible)); }
        }

        public bool IsRibbonVisible
        {
            get => _isRibbonVisible;
            set { _isRibbonVisible = value; OnPropertyChanged(nameof(IsRibbonVisible)); }
        }

        public ICommand MinimizeCommand { get; }
        public ICommand CustomizeQuickAccessCommand { get; }
        public ICommand ShowRibbonCommand { get; }

        public RibbonViewModel()
        {
            MinimizeCommand = new RelayCommand(ExecuteMinimize);
            CustomizeQuickAccessCommand = new RelayCommand(ExecuteCustomizeQuickAccess);
            ShowRibbonCommand = new RelayCommand<string>(ExecuteShowRibbon);

            InitializeTabs();
        }

        private void InitializeTabs()
        {
            var homeTab = new RibbonTabViewModel
            {
                Name = "Home",
                Label = "Home",
                Groups = new ObservableCollection<RibbonGroupViewModel>
                {
                    new() { Name = "Clipboard", Label = "Clipboard" },
                    new() { Name = "Font", Label = "Font" },
                    new() { Name = "Paragraph", Label = "Paragraph" },
                    new() { Name = "Styles", Label = "Styles" },
                    new() { Name = "Editing", Label = "Editing" }
                }
            };

            var insertTab = new RibbonTabViewModel
            {
                Name = "Insert",
                Label = "Insert",
                Groups = new ObservableCollection<RibbonGroupViewModel>
                {
                    new() { Name = "Pages", Label = "Pages" },
                    new() { Name = "Tables", Label = "Tables" },
                    new() { Name = "Illustrations", Label = "Illustrations" },
                    new() { Name = "Links", Label = "Links" },
                    new() { Name = "Comments", Label = "Comments" },
                    new() { Name = "HeaderFooter", Label = "Header & Footer" },
                    new() { Name = "Text", Label = "Text" },
                    new() { Name = "Symbols", Label = "Symbols" }
                }
            };

            var layoutTab = new RibbonTabViewModel
            {
                Name = "Layout",
                Label = "Layout",
                Groups = new ObservableCollection<RibbonGroupViewModel>
                {
                    new() { Name = "PageSetup", Label = "Page Setup" },
                    new() { Name = "PageBackground", Label = "Page Background" },
                    new() { Name = "Paragraph", Label = "Paragraph" },
                    new() { Name = "Arrange", Label = "Arrange" }
                }
            };

            var referencesTab = new RibbonTabViewModel
            {
                Name = "References",
                Label = "References",
                Groups = new ObservableCollection<RibbonGroupViewModel>
                {
                    new() { Name = "TableOfContents", Label = "Table of Contents" },
                    new() { Name = "Footnotes", Label = "Footnotes" },
                    new() { Name = "Citations", Label = "Citations & Bibliography" },
                    new() { Name = "Captions", Label = "Captions" },
                    new() { Name = "Index", Label = "Index" },
                    new() { Name = "TableOfAuthorities", Label = "Table of Authorities" }
                }
            };

            var mailingsTab = new RibbonTabViewModel
            {
                Name = "Mailings",
                Label = "Mailings",
                Groups = new ObservableCollection<RibbonGroupViewModel>
                {
                    new() { Name = "Create", Label = "Create" },
                    new() { Name = "StartMailMerge", Label = "Start Mail Merge" },
                    new() { Name = "WriteFields", Label = "Write & Insert Fields" },
                    new() { Name = "PreviewResults", Label = "Preview Results" },
                    new() { Name = "Finish", Label = "Finish" }
                }
            };

            var reviewTab = new RibbonTabViewModel
            {
                Name = "Review",
                Label = "Review",
                Groups = new ObservableCollection<RibbonGroupViewModel>
                {
                    new() { Name = "Proofing", Label = "Proofing" },
                    new() { Name = "Comments", Label = "Comments" },
                    new() { Name = "Tracking", Label = "Tracking" },
                    new() { Name = "Changes", Label = "Changes" },
                    new() { Name = "Compare", Label = "Compare" },
                    new() { Name = "Protect", Label = "Protect" }
                }
            };

            var viewTab = new RibbonTabViewModel
            {
                Name = "View",
                Label = "View",
                Groups = new ObservableCollection<RibbonGroupViewModel>
                {
                    new() { Name = "DocumentViews", Label = "Document Views" },
                    new() { Name = "Show", Label = "Show" },
                    new() { Name = "Zoom", Label = "Zoom" },
                    new() { Name = "Window", Label = "Window" },
                    new() { Name = "Macros", Label = "Macros" }
                }
            };

            Tabs.Add(homeTab);
            Tabs.Add(insertTab);
            Tabs.Add(layoutTab);
            Tabs.Add(referencesTab);
            Tabs.Add(mailingsTab);
            Tabs.Add(reviewTab);
            Tabs.Add(viewTab);
        }

        private void ExecuteMinimize()
        {
            IsMinimized = !IsMinimized;
        }

        private void ExecuteCustomizeQuickAccess()
        {
        }

        private void ExecuteShowRibbon(string? tab)
        {
            if (!string.IsNullOrEmpty(tab))
            {
                SelectedTab = tab;
                IsMinimized = false;
            }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonTabViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _label = string.Empty;
        private bool _isSelected;
        private bool _isEnabled = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<RibbonGroupViewModel> Groups { get; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(nameof(Label)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonGroupViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _label = string.Empty;
        private bool _isEnabled = true;
        private bool _isCollapsed;
        private RibbonGroupSize _size = RibbonGroupSize.Normal;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<RibbonItemViewModel> Items { get; } = new();
        public ObservableCollection<RibbonCommand> Commands { get; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(nameof(Label)); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public bool IsCollapsed
        {
            get => _isCollapsed;
            set { _isCollapsed = value; OnPropertyChanged(nameof(IsCollapsed)); }
        }

        public RibbonGroupSize Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(nameof(Size)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonItemViewModel : INotifyPropertyChanged
    {
        private string _id = Guid.NewGuid().ToString();
        private string _label = string.Empty;
        private string? _tooltip;
        private string? _description;
        private string? _icon;
        private bool _isEnabled = true;
        private bool _isVisible = true;
        private bool _isChecked;
        private RibbonItemSize _size = RibbonItemSize.Normal;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(nameof(Label)); }
        }

        public string? Tooltip
        {
            get => _tooltip;
            set { _tooltip = value; OnPropertyChanged(nameof(Tooltip)); }
        }

        public string? Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public string? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); }
        }

        public RibbonItemSize Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(nameof(Size)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonCommand : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _label = string.Empty;
        private string? _tooltip;
        private string? _icon;
        private string? _smallIcon;
        private string? _largeIcon;
        private bool _isEnabled = true;
        private bool _isVisible = true;
        private bool _isChecked;
        private string? _keyTip;
        private string? _keyboardShortcut;
        private RibbonCommandSize _size = RibbonCommandSize.Normal;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler? Executed;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(nameof(Label)); }
        }

        public string? Tooltip
        {
            get => _tooltip;
            set { _tooltip = value; OnPropertyChanged(nameof(Tooltip)); }
        }

        public string? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        public string? SmallIcon
        {
            get => _smallIcon;
            set { _smallIcon = value; OnPropertyChanged(nameof(SmallIcon)); }
        }

        public string? LargeIcon
        {
            get => _largeIcon;
            set { _largeIcon = value; OnPropertyChanged(nameof(LargeIcon)); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); }
        }

        public string? KeyTip
        {
            get => _keyTip;
            set { _keyTip = value; OnPropertyChanged(nameof(KeyTip)); }
        }

        public string? KeyboardShortcut
        {
            get => _keyboardShortcut;
            set { _keyboardShortcut = value; OnPropertyChanged(nameof(KeyboardShortcut)); }
        }

        public RibbonCommandSize Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(nameof(Size)); }
        }

        public void Execute()
        {
            Executed?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonSplitButton : RibbonCommand
    {
        public ObservableCollection<RibbonCommand> Items { get; } = new();
        public RibbonCommand? SelectedItem { get; set; }
    }

    public class RibbonComboBox : RibbonCommand
    {
        private string _text = string.Empty;
        private int _selectedIndex = -1;
        private object? _selectedItem;

        public ObservableCollection<RibbonComboBoxItem> Items { get; } = new();
        public ObservableCollection<string> Values { get; } = new();

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set { _selectedIndex = value; OnPropertyChanged(nameof(SelectedIndex)); }
        }

        public object? SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); }
        }
    }

    public class RibbonComboBoxItem : INotifyPropertyChanged
    {
        private string _text = string.Empty;
        private string? _value;
        private bool _isSelected;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Text
        {
            get => _text;
            set { _text = value; OnPropertyChanged(nameof(Text)); }
        }

        public string? Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(nameof(Value)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }
    }

    public class RibbonColorPicker : RibbonCommand
    {
        private string _selectedColor = "#000000";
        private bool _isAutomatic = true;

        public ObservableCollection<RibbonColorItem> Colors { get; } = new();
        public ObservableCollection<RibbonColorItem> StandardColors { get; } = new();
        public ObservableCollection<RibbonColorItem> ThemeColors { get; } = new();

        public string SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; OnPropertyChanged(nameof(SelectedColor)); }
        }

        public bool IsAutomatic
        {
            get => _isAutomatic;
            set { _isAutomatic = value; OnPropertyChanged(nameof(IsAutomatic)); }
        }
    }

    public class RibbonColorItem : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _color = "#000000";

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string Color
        {
            get => _color;
            set { _color = value; OnPropertyChanged(nameof(Color)); }
        }
    }

    public class RibbonGallery : RibbonCommand
    {
        private int _columns = 4;
        private int _rows = 3;
        private RibbonGalleryItem? _selectedItem;

        public ObservableCollection<RibbonGalleryItem> Items { get; } = new();
        public ObservableCollection<RibbonGalleryCategory> Categories { get; } = new();

        public int Columns
        {
            get => _columns;
            set { _columns = value; OnPropertyChanged(nameof(Columns)); }
        }

        public int Rows
        {
            get => _rows;
            set { _rows = value; OnPropertyChanged(nameof(Rows)); }
        }

        public RibbonGalleryItem? SelectedItem
        {
            get => _selectedItem;
            set { _selectedItem = value; OnPropertyChanged(nameof(SelectedItem)); }
        }
    }

    public class RibbonGalleryItem : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string? _icon;
        private bool _isSelected;
        private object? _tag;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }

        public string? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); }
        }

        public object? Tag
        {
            get => _tag;
            set { _tag = value; OnPropertyChanged(nameof(Tag)); }
        }
    }

    public class RibbonGalleryCategory : INotifyPropertyChanged
    {
        private string _name = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<RibbonGalleryItem> Items { get; } = new();

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(nameof(Name)); }
        }
    }

    public enum RibbonGroupSize
    {
        Normal,
        Medium,
        Large
    }

    public enum RibbonItemSize
    {
        Normal,
        Large,
        Small
    }

    public enum RibbonCommandSize
    {
        Normal,
        Large,
        Small
    }

    public class RibbonMenuViewModel : INotifyPropertyChanged
    {
        private string _label = string.Empty;
        private string? _icon;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<RibbonMenuItem> Items { get; } = new();
        public ObservableCollection<RibbonMenuSeparator> Separators { get; } = new();

        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(nameof(Label)); }
        }

        public string? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }
    }

    public class RibbonMenuItem : INotifyPropertyChanged
    {
        private string _label = string.Empty;
        private string? _icon;
        private string? _tooltip;
        private string? _keyboardShortcut;
        private bool _isEnabled = true;
        private bool _isVisible = true;
        private bool _isChecked;
        private RibbonCommand? _command;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<RibbonMenuItem> Items { get; } = new();

        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(nameof(Label)); }
        }

        public string? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        public string? Tooltip
        {
            get => _tooltip;
            set { _tooltip = value; OnPropertyChanged(nameof(Tooltip)); }
        }

        public string? KeyboardShortcut
        {
            get => _keyboardShortcut;
            set { _keyboardShortcut = value; OnPropertyChanged(nameof(KeyboardShortcut)); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }

        public bool IsChecked
        {
            get => _isChecked;
            set { _isChecked = value; OnPropertyChanged(nameof(IsChecked)); }
        }

        public RibbonCommand? Command
        {
            get => _command;
            set { _command = value; OnPropertyChanged(nameof(Command)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RibbonMenuSeparator : INotifyPropertyChanged
    {
        private bool _isVisible = true;

        public event PropertyChangedEventHandler? PropertyChanged;

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; OnPropertyChanged(nameof(IsVisible)); }
        }
    }

    public class OfficeMenuViewModel : INotifyPropertyChanged
    {
        private string _title = "File";

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<OfficeMenuItem> Items { get; } = new();
        public ObservableCollection<string> RecentDocuments { get; } = new();

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(nameof(Title)); }
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class OfficeMenuItem : INotifyPropertyChanged
    {
        private string _label = string.Empty;
        private string? _icon;
        private string? _description;
        private bool _isEnabled = true;
        private RibbonCommand? _command;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Label
        {
            get => _label;
            set { _label = value; OnPropertyChanged(nameof(Label)); }
        }

        public string? Icon
        {
            get => _icon;
            set { _icon = value; OnPropertyChanged(nameof(Icon)); }
        }

        public string? Description
        {
            get => _description;
            set { _description = value; OnPropertyChanged(nameof(Description)); }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; OnPropertyChanged(nameof(IsEnabled)); }
        }

        public RibbonCommand? Command
        {
            get => _command;
            set { _command = value; OnPropertyChanged(nameof(Command)); }
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
            add { }
            remove { }
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
            add { }
            remove { }
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

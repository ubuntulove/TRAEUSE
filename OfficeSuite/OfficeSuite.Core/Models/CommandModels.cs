using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace OfficeSuite.Core.Models
{
    public class CommandDefinition : ModelBase
    {
        private string _id;
        private string _name;
        private string _description;
        private string _category;
        private string _iconPath;
        private KeyGesture _gesture;
        private bool _isEnabled;
        private bool _isVisible;
        private bool _isChecked;
        private Action<object> _execute;
        private Func<object, bool> _canExecute;

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

        public string Category
        {
            get => _category;
            set => SetProperty(ref _category, value);
        }

        public string IconPath
        {
            get => _iconPath;
            set => SetProperty(ref _iconPath, value);
        }

        public KeyGesture Gesture
        {
            get => _gesture;
            set => SetProperty(ref _gesture, value);
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

        public Action<object> Execute
        {
            get => _execute;
            set => SetProperty(ref _execute, value);
        }

        public Func<object, bool> CanExecute
        {
            get => _canExecute;
            set => SetProperty(ref _canExecute, value);
        }

        public CommandDefinition()
        {
            _id = Guid.NewGuid().ToString();
            _isEnabled = true;
            _isVisible = true;
            _isChecked = false;
        }

        public bool CanExecuteCommand(object parameter)
        {
            return _canExecute?.Invoke(parameter) ?? true;
        }

        public void ExecuteCommand(object parameter)
        {
            _execute?.Invoke(parameter);
        }
    }

    public class RibbonTab : ModelBase
    {
        private string _id;
        private string _title;
        private int _index;
        private bool _isVisible;
        private List<RibbonGroup> _groups;

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

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public List<RibbonGroup> Groups
        {
            get => _groups ??= new List<RibbonGroup>();
            set => SetProperty(ref _groups, value);
        }

        public RibbonTab()
        {
            _id = Guid.NewGuid().ToString();
            _isVisible = true;
            _groups = new List<RibbonGroup>();
        }
    }

    public class RibbonGroup : ModelBase
    {
        private string _id;
        private string _title;
        private int _index;
        private bool _isVisible;
        private List<CommandDefinition> _commands;

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

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public List<CommandDefinition> Commands
        {
            get => _commands ??= new List<CommandDefinition>();
            set => SetProperty(ref _commands, value);
        }

        public RibbonGroup()
        {
            _id = Guid.NewGuid().ToString();
            _isVisible = true;
            _commands = new List<CommandDefinition>();
        }
    }

    public class MenuItemDefinition : ModelBase
    {
        private string _id;
        private string _header;
        private string _iconPath;
        private string _shortcut;
        private bool _isEnabled;
        private bool _isVisible;
        private bool _isChecked;
        private string _commandId;
        private List<MenuItemDefinition> _children;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        public string IconPath
        {
            get => _iconPath;
            set => SetProperty(ref _iconPath, value);
        }

        public string Shortcut
        {
            get => _shortcut;
            set => SetProperty(ref _shortcut, value);
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

        public string CommandId
        {
            get => _commandId;
            set => SetProperty(ref _commandId, value);
        }

        public List<MenuItemDefinition> Children
        {
            get => _children ??= new List<MenuItemDefinition>();
            set => SetProperty(ref _children, value);
        }

        public MenuItemDefinition()
        {
            _id = Guid.NewGuid().ToString();
            _isEnabled = true;
            _isVisible = true;
            _isChecked = false;
            _children = new List<MenuItemDefinition>();
        }
    }

    public class StatusBarItem : ModelBase
    {
        private string _id;
        private string _text;
        private string _iconPath;
        private StatusBarItemType _type;
        private int _index;
        private bool _isVisible;

        public string Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }

        public string IconPath
        {
            get => _iconPath;
            set => SetProperty(ref _iconPath, value);
        }

        public StatusBarItemType Type
        {
            get => _type;
            set => SetProperty(ref _type, value);
        }

        public int Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        public bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }

        public StatusBarItem()
        {
            _id = Guid.NewGuid().ToString();
            _isVisible = true;
        }
    }

    public enum StatusBarItemType
    {
        Text,
        Progress,
        Indicator,
        Language
    }
}

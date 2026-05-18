using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xaml.Behaviors;

namespace OfficeSuite.Common.Behaviors
{
    public class DragMoveBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1)
            {
                if (AssociatedObject.WindowState == WindowState.Maximized)
                {
                    AssociatedObject.WindowState = WindowState.Normal;
                }
                AssociatedObject.DragMove();
            }
        }
    }

    public class WindowStateBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty MinimizeCommandProperty =
            DependencyProperty.Register("MinimizeCommand", typeof(ICommand), typeof(WindowStateBehavior));

        public ICommand MinimizeCommand
        {
            get => (ICommand)GetValue(MinimizeCommandProperty);
            set => SetValue(MinimizeCommandProperty, value);
        }

        public static readonly DependencyProperty MaximizeCommandProperty =
            DependencyProperty.Register("MaximizeCommand", typeof(ICommand), typeof(WindowStateBehavior));

        public ICommand MaximizeCommand
        {
            get => (ICommand)GetValue(MaximizeCommandProperty);
            set => SetValue(MaximizeCommandProperty, value);
        }

        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(WindowStateBehavior));

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.StateChanged += OnStateChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.StateChanged -= OnStateChanged;
        }

        private void OnStateChanged(object sender, EventArgs e)
        {
        }
    }

    public class TextBoxSelectAllBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotFocus += OnGotFocus;
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotFocus -= OnGotFocus;
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        }

        private void OnGotFocus(object sender, RoutedEventArgs e)
        {
            AssociatedObject.SelectAll();
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AssociatedObject.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                AssociatedObject.Focus();
            }
        }
    }

    public class ScrollIntoViewBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssociatedObject.SelectedItem != null)
            {
                AssociatedObject.ScrollIntoView(AssociatedObject.SelectedItem);
            }
        }
    }

    public class DoubleClickBehavior : Behavior<Control>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(DoubleClickBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(DoubleClickBehavior));

        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                if (Command?.CanExecute(CommandParameter) == true)
                {
                    Command.Execute(CommandParameter);
                }
            }
        }
    }

    public class ResizeBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ResizeDirectionProperty =
            DependencyProperty.Register("ResizeDirection", typeof(ResizeDirection), typeof(ResizeBehavior));

        public ResizeDirection ResizeDirection
        {
            get => (ResizeDirection)GetValue(ResizeDirectionProperty);
            set => SetValue(ResizeDirectionProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
            AssociatedObject.MouseMove += OnMouseMove;
            AssociatedObject.MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
            AssociatedObject.MouseMove -= OnMouseMove;
            AssociatedObject.MouseLeftButtonUp -= OnMouseLeftButtonUp;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }
    }

    public enum ResizeDirection
    {
        Left,
        Right,
        Top,
        Bottom,
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }

    public class FormatPainterBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register("IsEnabled", typeof(bool), typeof(FormatPainterBehavior), new PropertyMetadata(false));

        public bool IsEnabled
        {
            get => (bool)GetValue(IsEnabledProperty);
            set => SetValue(IsEnabledProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
        }
    }

    public class KeyboardFocusBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty FocusOnLoadProperty =
            DependencyProperty.Register("FocusOnLoad", typeof(bool), typeof(KeyboardFocusBehavior), new PropertyMetadata(false));

        public bool FocusOnLoad
        {
            get => (bool)GetValue(FocusOnLoadProperty);
            set => SetValue(FocusOnLoadProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (FocusOnLoad)
            {
                AssociatedObject.Loaded += OnLoaded;
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            AssociatedObject.Focus();
        }
    }

    public class AutoSelectBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.GotKeyboardFocus += OnGotKeyboardFocus;
            AssociatedObject.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.GotKeyboardFocus -= OnGotKeyboardFocus;
            AssociatedObject.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
        }

        private void OnGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            AssociatedObject.SelectAll();
        }

        private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!AssociatedObject.IsKeyboardFocusWithin)
            {
                e.Handled = true;
                AssociatedObject.Focus();
            }
        }
    }

    public class EnterKeyBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(EnterKeyBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += OnKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.KeyDown -= OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (Command?.CanExecute(null) == true)
                {
                    Command.Execute(null);
                }
            }
        }
    }

    public class EscapeKeyBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(EscapeKeyBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.KeyDown += OnKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.KeyDown -= OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (Command?.CanExecute(null) == true)
                {
                    Command.Execute(null);
                }
            }
        }
    }

    public class UpdateBindingOnPasswordChangeBehavior : Behavior<PasswordBox>
    {
        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.Register("Password", typeof(string), typeof(UpdateBindingOnPasswordChangeBehavior),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnPasswordChanged));

        public string Password
        {
            get => (string)GetValue(PasswordProperty);
            set => SetValue(PasswordProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PasswordChanged += OnAssociatedObjectPasswordChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PasswordChanged -= OnAssociatedObjectPasswordChanged;
        }

        private void OnAssociatedObjectPasswordChanged(object sender, RoutedEventArgs e)
        {
            Password = AssociatedObject.Password;
        }

        private static void OnPasswordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PasswordBox passwordBox && passwordBox.Password != e.NewValue as string)
            {
                passwordBox.Password = e.NewValue as string;
            }
        }
    }

    public class ScrollIntoViewForComboBoxBehavior : Behavior<ComboBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (AssociatedObject.IsDropDownOpen && AssociatedObject.SelectedItem != null)
            {
                var container = AssociatedObject.ItemContainerGenerator.ContainerFromItem(AssociatedObject.SelectedItem) as FrameworkElement;
                container?.BringIntoView();
            }
        }
    }

    public class DragDropBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty DropCommandProperty =
            DependencyProperty.Register("DropCommand", typeof(ICommand), typeof(DragDropBehavior));

        public ICommand DropCommand
        {
            get => (ICommand)GetValue(DropCommandProperty);
            set => SetValue(DropCommandProperty, value);
        }

        public static readonly DependencyProperty DragOverCommandProperty =
            DependencyProperty.Register("DragOverCommand", typeof(ICommand), typeof(DragDropBehavior));

        public ICommand DragOverCommand
        {
            get => (ICommand)GetValue(DragOverCommandProperty);
            set => SetValue(DragOverCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.AllowDrop = true;
            AssociatedObject.Drop += OnDrop;
            AssociatedObject.DragOver += OnDragOver;
            AssociatedObject.DragEnter += OnDragEnter;
            AssociatedObject.DragLeave += OnDragLeave;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Drop -= OnDrop;
            AssociatedObject.DragOver -= OnDragOver;
            AssociatedObject.DragEnter -= OnDragEnter;
            AssociatedObject.DragLeave -= OnDragLeave;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (DropCommand?.CanExecute(e.Data) == true)
            {
                DropCommand.Execute(e.Data);
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (DragOverCommand?.CanExecute(e.Data) == true)
            {
                DragOverCommand.Execute(e.Data);
            }
            e.Handled = true;
        }

        private void OnDragEnter(object sender, DragEventArgs e)
        {
        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
        }
    }

    public class ClipboardPasteBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty PasteCommandProperty =
            DependencyProperty.Register("PasteCommand", typeof(ICommand), typeof(ClipboardPasteBehavior));

        public ICommand PasteCommand
        {
            get => (ICommand)GetValue(PasteCommandProperty);
            set => SetValue(PasteCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.PreviewKeyDown += OnPreviewKeyDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.PreviewKeyDown -= OnPreviewKeyDown;
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.V && Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (PasteCommand?.CanExecute(null) == true)
                {
                    PasteCommand.Execute(null);
                    e.Handled = true;
                }
            }
        }
    }

    public class SelectionChangedBehavior : Behavior<Selector>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(SelectionChangedBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += OnSelectionChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.SelectionChanged -= OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Command?.CanExecute(AssociatedObject.SelectedItem) == true)
            {
                Command.Execute(AssociatedObject.SelectedItem);
            }
        }
    }

    public class MouseDoubleClickBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(MouseDoubleClickBehavior));

        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.MouseLeftButtonDown += OnMouseLeftButtonDown;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.MouseLeftButtonDown -= OnMouseLeftButtonDown;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2 && Command?.CanExecute(AssociatedObject.DataContext) == true)
            {
                Command.Execute(AssociatedObject.DataContext);
                e.Handled = true;
            }
        }
    }

    public class ContextMenuBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty ContextMenuCommandProperty =
            DependencyProperty.Register("ContextMenuCommand", typeof(ICommand), typeof(ContextMenuBehavior));

        public ICommand ContextMenuCommand
        {
            get => (ICommand)GetValue(ContextMenuCommandProperty);
            set => SetValue(ContextMenuCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.ContextMenuOpening += OnContextMenuOpening;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.ContextMenuOpening -= OnContextMenuOpening;
        }

        private void OnContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (ContextMenuCommand?.CanExecute(null) == true)
            {
                ContextMenuCommand.Execute(null);
            }
        }
    }

    public class DataGridCellEditBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommitEditCommandProperty =
            DependencyProperty.Register("CommitEditCommand", typeof(ICommand), typeof(DataGridCellEditBehavior));

        public ICommand CommitEditCommand
        {
            get => (ICommand)GetValue(CommitEditCommandProperty);
            set => SetValue(CommitEditCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.CellEditEnding += OnCellEditEnding;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.CellEditEnding -= OnCellEditEnding;
        }

        private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && CommitEditCommand?.CanExecute(e) == true)
            {
                CommitEditCommand.Execute(e);
            }
        }
    }

    public class WindowClosingBehavior : Behavior<Window>
    {
        public static readonly DependencyProperty CloseCommandProperty =
            DependencyProperty.Register("CloseCommand", typeof(ICommand), typeof(WindowClosingBehavior));

        public ICommand CloseCommand
        {
            get => (ICommand)GetValue(CloseCommandProperty);
            set => SetValue(CloseCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Closing += OnClosing;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Closing -= OnClosing;
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (CloseCommand?.CanExecute(null) == true)
            {
                CloseCommand.Execute(null);
            }
        }
    }

    public class LoadedBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty LoadedCommandProperty =
            DependencyProperty.Register("LoadedCommand", typeof(ICommand), typeof(LoadedBehavior));

        public ICommand LoadedCommand
        {
            get => (ICommand)GetValue(LoadedCommandProperty);
            set => SetValue(LoadedCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (LoadedCommand?.CanExecute(null) == true)
            {
                LoadedCommand.Execute(null);
            }
        }
    }

    public class VisibilityBehavior : Behavior<FrameworkElement>
    {
        public static readonly DependencyProperty IsVisibleCommandProperty =
            DependencyProperty.Register("IsVisibleCommand", typeof(ICommand), typeof(VisibilityBehavior));

        public ICommand IsVisibleCommand
        {
            get => (ICommand)GetValue(IsVisibleCommandProperty);
            set => SetValue(IsVisibleCommandProperty, value);
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged += OnIsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.IsVisibleChanged -= OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (IsVisibleCommand?.CanExecute(e.NewValue) == true)
            {
                IsVisibleCommand.Execute(e.NewValue);
            }
        }
    }

    public class FocusOnVisibleBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.IsVisibleChanged += OnIsVisibleChanged;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.IsVisibleChanged -= OnIsVisibleChanged;
        }

        private void OnIsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is true)
            {
                AssociatedObject.Focus();
            }
        }
    }
}

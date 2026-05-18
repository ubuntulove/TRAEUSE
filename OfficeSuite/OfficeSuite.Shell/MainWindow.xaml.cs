using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OfficeSuite.Shell.ViewModels;

namespace OfficeSuite.Shell
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel ViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += OnLoaded;
            Closing += OnClosing;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Initialize();
            }

            SetupKeyboardShortcuts();
        }

        private void OnClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.Cleanup();
            }
        }

        private void SetupKeyboardShortcuts()
        {
            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.NewWordDocumentCommand.Execute(null)),
                Key.N, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.OpenCommand.Execute(null)),
                Key.O, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.SaveCommand.Execute(null)),
                Key.S, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.SaveAsCommand.Execute(null)),
                Key.S, ModifierKeys.Control | ModifierKeys.Shift));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.PrintCommand.Execute(null)),
                Key.P, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.UndoCommand.Execute(null)),
                Key.Z, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.RedoCommand.Execute(null)),
                Key.Y, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.RedoCommand.Execute(null)),
                Key.Z, ModifierKeys.Control | ModifierKeys.Shift));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.CutCommand.Execute(null)),
                Key.X, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.CopyCommand.Execute(null)),
                Key.C, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.PasteCommand.Execute(null)),
                Key.V, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.DeleteCommand.Execute(null)),
                Key.Delete, ModifierKeys.None));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.SelectAllCommand.Execute(null)),
                Key.A, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.FindCommand.Execute(null)),
                Key.F, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.ReplaceCommand.Execute(null)),
                Key.H, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.FullScreenCommand.Execute(null)),
                Key.F11, ModifierKeys.None));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.CloseCommand.Execute(null)),
                Key.W, ModifierKeys.Control));

            InputBindings.Add(new KeyBinding(
                new RelayCommand(_ => ViewModel?.ExitCommand.Execute(null)),
                Key.F4, ModifierKeys.Alt));
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape && ViewModel?.IsFullScreen == true)
            {
                ViewModel.FullScreenCommand.Execute(null);
                e.Handled = true;
            }
        }

        private void OnNewWordDocument_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.NewWordDocumentCommand.Execute(null);
        }

        private void OnNewSpreadsheet_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.NewSpreadsheetCommand.Execute(null);
        }

        private void OnNewPresentation_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.NewPresentationCommand.Execute(null);
        }

        private void OnOpen_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.OpenCommand.Execute(null);
        }

        private void OnSave_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.SaveCommand.Execute(null);
        }

        private void OnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.SaveAsCommand.Execute(null);
        }

        private void OnPrint_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.PrintCommand.Execute(null);
        }

        private void OnExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnUndo_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.UndoCommand.Execute(null);
        }

        private void OnRedo_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.RedoCommand.Execute(null);
        }

        private void OnCut_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.CutCommand.Execute(null);
        }

        private void OnCopy_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.CopyCommand.Execute(null);
        }

        private void OnPaste_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.PasteCommand.Execute(null);
        }

        private void OnSelectAll_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.SelectAllCommand.Execute(null);
        }

        private void OnFind_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.FindCommand.Execute(null);
        }

        private void OnReplace_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.ReplaceCommand.Execute(null);
        }

        private void OnAbout_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.AboutCommand.Execute(null);
        }

        private void OnOptions_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.OptionsCommand.Execute(null);
        }

        private void OnCloseDocument_Click(object sender, RoutedEventArgs e)
        {
            ViewModel?.CloseCommand.Execute(null);
        }
    }
}

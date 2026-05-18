using System;
using System.Windows;
using OfficeSuite.Shell.ViewModels;

namespace OfficeSuite.Shell
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = new MainWindow();
            var viewModel = new MainWindowViewModel();
            mainWindow.DataContext = viewModel;
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);
        }
    }
}

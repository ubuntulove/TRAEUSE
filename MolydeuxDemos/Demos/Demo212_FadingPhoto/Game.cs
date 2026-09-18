using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo212_FadingPhoto
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 212: Be A Fading Photograph";
            Instructions = "You are a photograph slowly losing its color. Click the drifting color motes to restore yourself before you fade to nothing.";
            WinText = "The memory lives on - colors restored!";
            LoseText = "You faded into oblivion!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=12; timeSec=25; targetBrush=Brushes.CornflowerBlue;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

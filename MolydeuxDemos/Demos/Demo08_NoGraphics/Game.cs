using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo08_NoGraphics
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 08: No Graphics";
            Instructions = "Is it possible to make a videogame with NO graphics? Find out. Just wait.";
            WinText = "You experienced pure nothingness.";
            LoseText = "You interfered with the void!";
            GameBackground = new SolidColorBrush(Color.FromRgb(0,0,0));
            timeSec=10;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo17_GameoverLocked
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 17: Gameover, Can't Continue";
            Instructions = "The game begins with a gameover screen. You can't continue. But maybe... click enough times?";
            WinText = "You found the secret reset code!";
            LoseText = "Game over. You can't continue.";
            GameBackground = new SolidColorBrush(Color.FromRgb(5,5,5));
            need=12; timeSec=30; targetBrush=Brushes.DarkRed;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

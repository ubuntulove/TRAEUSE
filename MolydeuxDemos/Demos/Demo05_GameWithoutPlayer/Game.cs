using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo05_GameWithoutPlayer
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 05: Game Without A Player";
            Instructions = "All games require someone to play them. This game goes against that very concept. Don't interfere.";
            WinText = "The game played itself - perfection!";
            LoseText = "You interfered with the experiment!";
            GameBackground = new SolidColorBrush(Color.FromRgb(5,5,5));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

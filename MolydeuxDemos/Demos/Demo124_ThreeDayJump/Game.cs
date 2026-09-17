using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo124_ThreeDayJump
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 124: Jump With 3 Day Countdown";
            Instructions = "Imagine pressing jump and having a 3 day countdown until you jump. Search for a gap within 3 days. Wait for the right moment.";
            WinText = "The 3 day wait paid off - perfect jump!";
            LoseText = "Impatience ruined the jump!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

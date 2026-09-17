using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo54_CommunicateWithSand
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 54: Communicate With Sand";
            Instructions = "Communication with sand. Listen to the grains. They have stories. Don't disturb them.";
            WinText = "The sand spoke to you!";
            LoseText = "You scattered the conversation!";
            GameBackground = new SolidColorBrush(Color.FromRgb(50,40,25));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

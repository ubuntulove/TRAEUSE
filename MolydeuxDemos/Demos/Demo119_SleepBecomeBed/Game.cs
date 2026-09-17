using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo119_SleepBecomeBed
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 119: Sleep, Become The Bed";
            Instructions = "You're in a room with just a bed. When you sleep you become the bed. How does it feel to be a bed? Degrading? Observe.";
            WinText = "You accepted your new life as a bed!";
            LoseText = "You refused to become furniture!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

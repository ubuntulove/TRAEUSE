using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo52_OneMinuteStreamed
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 52: One Minute, One Person, Streamed";
            Instructions = "A game that can only be played 1 minute at a time by 1 different person worldwide, streamed 24/7. Wait your minute.";
            WinText = "Your minute was perfect!";
            LoseText = "You overran your minute!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

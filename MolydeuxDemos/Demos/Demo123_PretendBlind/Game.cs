using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo123_PretendBlind
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 123: Pretend To Be Blind";
            Instructions = "Play a character PRETENDING to be blind. Bump into things to not arouse suspicion, but dodge the truly dangerous obstacles.";
            WinText = "Nobody suspected you can see!";
            LoseText = "Your fake blindness got you killed!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            timeSec=22; itemBrush=Brushes.Gray; fallSpeed=200;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

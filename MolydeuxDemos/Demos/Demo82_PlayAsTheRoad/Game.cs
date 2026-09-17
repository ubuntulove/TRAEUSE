using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo82_PlayAsTheRoad
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 82: Play As The Road";
            Instructions = "Have you ever played a racing game and wanted to play as the road rather than the cars? You ARE the road. Dodge the cars!";
            WinText = "You survived the entire race day!";
            LoseText = "A car drove right through you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,25,30));
            timeSec=22; itemBrush=Brushes.Silver; itemShape="square"; fallSpeed=320;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

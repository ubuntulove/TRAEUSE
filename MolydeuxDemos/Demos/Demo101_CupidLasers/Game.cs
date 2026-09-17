using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo101_CupidLasers
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 101: Cupid's Laser Match";
            Instructions = "You're Cupid. Find compatible partners - one arrow in each, connected with lasers. If any NPC touches the laser, game over!";
            WinText = "All compatible couples matched!";
            LoseText = "An NPC walked into your laser - awkward!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,50));
            need=8; timeSec=25; targetBrush=Brushes.Pink;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

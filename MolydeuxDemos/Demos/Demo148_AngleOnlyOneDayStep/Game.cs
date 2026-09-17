using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo148_AngleOnlyOneDayStep
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 148: Angle Only, 1 Day = 1 Step";
            Instructions = "A game where you only control the angle of your character. 1 day = 1 step forward. Imagine the patience required. Just wait.";
            WinText = "You took the perfect step!";
            LoseText = "Impatience ruined the angle!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

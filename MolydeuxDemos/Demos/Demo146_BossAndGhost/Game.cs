using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo146_BossAndGhost
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 146: Boss Has A Ghost - Spot Difference";
            Instructions = "A boss that also has a ghost. Play spot the difference to find weak spots. Ghost has 7 eyes? Shoot the 8th eye on the boss!";
            WinText = "All weak spots found - boss defeated!";
            LoseText = "You shot the wrong eye - the ghost cursed you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            need=10; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

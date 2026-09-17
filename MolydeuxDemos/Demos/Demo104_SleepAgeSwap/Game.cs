using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo104_SleepAgeSwap
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 104: Sleep Cycle Age Swap";
            Instructions = "Every hour you sleep, your child grows older by a week. Every hour they sleep, you get younger. Watch the impossible math.";
            WinText = "You both reached perfect equilibrium!";
            LoseText = "The math broke reality!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo61_BabyCycle5s
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 61: Baby Born Cycle In 5 Seconds";
            Instructions = "Play as a baby, grow up, give birth, play as the newborn - all in 5 second cycles. Watch the generations roll.";
            WinText = "Witnessed infinite generations!";
            LoseText = "You interrupted the cycle of life!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,30,50));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo35_ConstantlyFalling
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 35: Constantly Falling";
            Instructions = "An action game where your character is constantly falling. You can hang onto things but only for 1-3 seconds.";
            WinText = "You survived the endless fall!";
            LoseText = "You fell too fast - splat!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,50));
            timeSec=22; itemBrush=Brushes.Brown; itemShape="square"; fallSpeed=180;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

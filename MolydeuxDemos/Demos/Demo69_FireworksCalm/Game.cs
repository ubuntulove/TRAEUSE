using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo69_FireworksCalm
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 69: Fireworks Calm / Weapon";
            Instructions = "Stealth game: the only way to calm down is by setting off fireworks - which can also be weapons. Dodge debris!";
            WinText = "You kept calm through the storm!";
            LoseText = "Panic overwhelmed you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,40));
            timeSec=22; itemBrush=Brushes.Gold; itemShape="triangle"; fallSpeed=280;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

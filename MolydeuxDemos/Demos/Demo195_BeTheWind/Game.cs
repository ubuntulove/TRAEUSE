using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo195_BeTheWind
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 195: Be The Wind";
            Instructions = "You ARE the wind. Push fallen leaves to their resting place - you can't touch them directly, but your gusts carry them. Catch the leaves!";
            WinText = "Autumn's leaves all settled!";
            LoseText = "The leaves scattered to the wind!";
            GameBackground = new SolidColorBrush(Color.FromRgb(70,90,120));
            need=10; playerBrush=Brushes.LightSkyBlue; itemBrush=Brushes.Orange;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

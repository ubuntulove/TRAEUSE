using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo30_BeACoin
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 30: Be A Coin";
            Instructions = "Play as a coin being passed around. Travel as far as possible by placing yourself where you'll be picked up.";
            WinText = "You traveled the whole economy!";
            LoseText = "You got lost in a sofa cushion!";
            GameBackground = new SolidColorBrush(Color.FromRgb(50,40,15));
            need=12; playerBrush=Brushes.Goldenrod; itemBrush=Brushes.LimeGreen;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

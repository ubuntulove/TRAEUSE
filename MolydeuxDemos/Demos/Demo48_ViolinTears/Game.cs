using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo48_ViolinTears
{
    public class MainWindow : CatchGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 48: Violin Tears As Currency";
            Instructions = "Play sad music on your violin to suck in people's tears, then trade them for weapons. Catch the tears!";
            WinText = "Wallet full of tears - gun shop opens!";
            LoseText = "Not enough tears for a single bullet!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            itemBrush=Brushes.LightBlue; itemShape="triangle"; need=12; miss=4;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

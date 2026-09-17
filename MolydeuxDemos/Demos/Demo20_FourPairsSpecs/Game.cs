using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo20_FourPairsSpecs
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 20: Four Pairs Of Specs";
            Instructions = "The game comes with 4 pairs of specs. Each one shows the world differently. Find the right lens for each target.";
            WinText = "You saw through every lens!";
            LoseText = "Blind without the right spectacles!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,50));
            need=8; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

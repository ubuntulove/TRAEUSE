using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo204_BoomerangPath
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 204: Be A Boomerang";
            Instructions = "You ARE a boomerang. Click to set your throw trajectory - you must hit the target and return to the hand.";
            WinText = "Perfect throw - hit and returned!";
            LoseText = "You flew off into the bush!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,40,20));
            need=10; timeSec=25; targetBrush=Brushes.Goldenrod;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

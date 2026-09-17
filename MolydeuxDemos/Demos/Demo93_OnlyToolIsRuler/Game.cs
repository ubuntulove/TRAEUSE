using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo93_OnlyToolIsRuler
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 93: Your Only Tool Is A Ruler";
            Instructions = "Designer workshop challenge: make a game where the player's only tool is a ruler. Click to measure precisely!";
            WinText = "All measurements perfect!";
            LoseText = "Your ruler snapped under pressure!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=12; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

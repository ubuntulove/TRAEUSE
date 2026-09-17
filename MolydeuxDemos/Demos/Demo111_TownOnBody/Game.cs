using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo111_TownOnBody
{
    public class MainWindow : ProtectGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 111: Town On Your Body";
            Instructions = "Play a character with a fully functional town on his body. Need to fix your leg? Build and fund a hospital on it. Defend the town!";
            WinText = "Your body-town prospered!";
            LoseText = "Invaders sacked your body-town!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,40));
            timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

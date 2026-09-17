using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo172_TurnBasedBoneFighter
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 172: Turn-Based Bone-Level Fighter";
            Instructions = "Turn-based 1v1 fighting: level up individual bones and set which order joints move and rotate to create attacks. Click to power up bones!";
            WinText = "Your skeleton warrior is unmatched!";
            LoseText = "Your bones shattered in the ring!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

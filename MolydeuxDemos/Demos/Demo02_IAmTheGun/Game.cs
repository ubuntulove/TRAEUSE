using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo02_IAmTheGun
{
    public class MainWindow : FollowGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 02: I Am The Gun";
            Instructions = "You ARE a gun. Keep close to your owner - if you wander too far for too long, you'll be lost forever.";
            WinText = "Faithful gun reunited with its owner!";
            LoseText = "You lost your owner in the crowd!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,40));
            timeSec=18; radius=110; targetBrush=Brushes.Silver; targetRadius=18;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

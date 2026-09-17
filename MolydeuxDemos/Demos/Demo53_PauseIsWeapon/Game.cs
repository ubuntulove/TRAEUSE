using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo53_PauseIsWeapon
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 53: Pause Button Is A Weapon";
            Instructions = "What if the pause button was a weapon? Click pause at exactly the right moment to freeze enemies.";
            WinText = "All enemies frozen in time!";
            LoseText = "You paused at the wrong moment!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,15,40));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

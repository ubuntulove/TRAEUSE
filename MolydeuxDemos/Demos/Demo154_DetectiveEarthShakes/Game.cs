using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo154_DetectiveEarthShakes
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 154: Detective - Talking Shakes Earth";
            Instructions = "You're a detective solving a vital case, but every time you talk to someone the Earth shakes and the environment falls apart. Click carefully!";
            WinText = "Case solved before the city collapsed!";
            LoseText = "Your questions leveled the city!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            need=8; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

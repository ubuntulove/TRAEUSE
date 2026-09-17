using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo128_TwitterDoorCodes
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 128: Twitter Door Codes Daily";
            Instructions = "Adventure game where you need to follow a twitter account that provides door codes every day. Wait for today's code.";
            WinText = "Today's code arrived - door unlocked!";
            LoseText = "You missed today's tweet - locked out!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

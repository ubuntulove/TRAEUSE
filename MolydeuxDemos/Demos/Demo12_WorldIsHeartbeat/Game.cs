using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo12_WorldIsHeartbeat
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 12: World Is Your Heartbeat";
            Instructions = "'The world around you is your heartbeat.' (Phil Collins). Watch the world pulse with your heart. Don't interfere.";
            WinText = "Your heart kept beating - the world lived!";
            LoseText = "Your heart skipped - the world shattered!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,15,30));
            timeSec=18;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

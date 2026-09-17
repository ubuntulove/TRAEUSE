using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo152_HeartbeatRumble
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 152: Pad Heartbeat Stops At Death";
            Instructions = "Imagine a constant rumble in the pad throughout the game - your heartbeat. At the end, you die and it just stops. Feel it fade.";
            WinText = "Your heartbeat faded peacefully.";
            LoseText = "Your heart stopped suddenly - tragedy!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,15,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

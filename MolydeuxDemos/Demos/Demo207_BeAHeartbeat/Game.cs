using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo207_BeAHeartbeat
{
    public class MainWindow : RhythmGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 207: Be A Single Heartbeat";
            Instructions = "You are one beat of a heart. Strike exactly on the rhythm to keep the body alive - mistime and the pulse falters.";
            WinText = "The pulse stayed steady - life endured!";
            LoseText = "Cardiac arrest - flatline!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,15,30));
            need=10;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

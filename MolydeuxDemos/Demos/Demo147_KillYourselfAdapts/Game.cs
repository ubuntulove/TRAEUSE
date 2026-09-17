using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo147_KillYourselfAdapts
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 147: Kill Yourself - World Adapts";
            Instructions = "A game with 1 goal: kill yourself. But the environment constantly adapts to stop you. Click to attempt suicide - the world reacts!";
            WinText = "You finally found a way out!";
            LoseText = "The world kept saving you - eternal suffering!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,40));
            need=12; timeSec=30; targetBrush=Brushes.DarkRed;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

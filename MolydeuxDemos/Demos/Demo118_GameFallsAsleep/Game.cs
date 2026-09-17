using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo118_GameFallsAsleep
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 118: Game Falls Asleep If Tedious";
            Instructions = "What if a game fell asleep if your actions were so tedious and repetitive? Wait... don't bore it.";
            WinText = "The game stayed awake for you!";
            LoseText = "You bored the game to sleep!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

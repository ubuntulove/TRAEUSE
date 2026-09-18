using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo201_SpreadYawn
{
    public class MainWindow : FollowGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 201: Be A Yawn";
            Instructions = "You are a yawn. Spread from one tired NPC to the next - stay close long enough to infect them. Get the whole room asleep.";
            WinText = "The whole room nodded off!";
            LoseText = "Someone resisted your yawn!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,50));
            timeSec=22; radius=70; targetBrush=Brushes.Lavender;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

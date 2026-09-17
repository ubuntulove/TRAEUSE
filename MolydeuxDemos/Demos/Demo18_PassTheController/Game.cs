using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo18_PassTheController
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 18: Pass The Controller";
            Instructions = "Play for 5 minutes, then send to a friend who plays 5 more. Imagine the handoff. Just wait your turn.";
            WinText = "Your 5 minutes are up - pass it on!";
            LoseText = "You hogged the controller!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

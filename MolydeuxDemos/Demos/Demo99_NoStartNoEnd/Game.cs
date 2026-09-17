using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo99_NoStartNoEnd
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 99: No Start, No End, Just Middle";
            Instructions = "What if there was a game that had no start or end, but just a middle? You're already in the middle. Stay there.";
            WinText = "You existed in the eternal middle!";
            LoseText = "You tried to find an end - there is none!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

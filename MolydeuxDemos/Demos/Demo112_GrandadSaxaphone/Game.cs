using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo112_GrandadSaxaphone
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 112: Grandad's Saxophone Escape";
            Instructions = "Play as a grandad. You can only interact with your grandchildren by playing a saxaphone - required to escape a dream castle.";
            WinText = "You saxophoned your way out of the dream!";
            LoseText = "You hit a sour note - the dream trapped you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

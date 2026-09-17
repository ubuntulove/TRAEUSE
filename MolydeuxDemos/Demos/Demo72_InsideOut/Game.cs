using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo72_InsideOut
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 72: Game Turns Itself Inside Out";
            Instructions = "We've had games that go sideways, up, down, rotate - but what about a game that turns itself inside out? Watch it transform.";
            WinText = "You witnessed the inversion!";
            LoseText = "You disturbed the inversion!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo32_VerticalOnly
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 32: Horizontal Is Impossible";
            Instructions = "In this world horizontal is impossible - you're forever falling or attached to verticals. Climb up only.";
            WinText = "Reached the top of the vertical world!";
            LoseText = "You fell into the horizontal void!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            need=8; speed=180; playerBrush=Brushes.White;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

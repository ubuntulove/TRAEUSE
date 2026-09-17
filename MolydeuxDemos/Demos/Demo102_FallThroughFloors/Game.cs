using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo102_FallThroughFloors
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 102: Falling Through Floors";
            Instructions = "What if falling through floors was a FEATURE not a bug? What lies below? Risk your life to find out. Fall through safely!";
            WinText = "You reached the very bottom of the world!";
            LoseText = "You fell into the void!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,40));
            timeSec=22; itemBrush=Brushes.Brown; itemShape="square"; fallSpeed=200;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo160_TouchFloorDie
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 160: Touch Floor And Die";
            Instructions = "If you touch the floor you die. The only way to progress is to kill enemies and use their bodies as platforms. Collect enemies to stand on!";
            WinText = "You crossed the chasm of bodies!";
            LoseText = "You touched the floor - dead!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            need=10; wallsKill=true; speed=200; playerBrush=Brushes.White; itemBrush=Brushes.DarkRed;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

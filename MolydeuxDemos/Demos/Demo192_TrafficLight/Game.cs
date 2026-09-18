using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo192_TrafficLight
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 192: Control The Traffic Light";
            Instructions = "You control a single traffic light. Click to switch colors - green to let cars through, red to stop them before collisions.";
            WinText = "Traffic flowed all day - no accidents!";
            LoseText = "A multi-car pileup - chaos!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,40));
            need=10; timeSec=25; targetBrush=Brushes.LimeGreen;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

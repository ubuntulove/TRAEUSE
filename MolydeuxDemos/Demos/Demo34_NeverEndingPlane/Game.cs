using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo34_NeverEndingPlane
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 34: Never Ending Plane";
            Instructions = "You awake on a plane and walk down the aisle, but it keeps looping. What's going on? Just observe the loop.";
            WinText = "You accepted the loop - peace!";
            LoseText = "You broke the loop - the plane crashed!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,25,35));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

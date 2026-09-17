using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo27_KinectStillHorror
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 27: Kinect Survival Horror";
            Instructions = "Survival horror where at key moments you must stay ABSOLUTELY still to avoid being found.";
            WinText = "You weren't discovered - escaped alive!";
            LoseText = "The monster heard you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(5,5,15));
            timeSec=14;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

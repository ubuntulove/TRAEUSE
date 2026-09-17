using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo158_PowerMeterBridge
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 158: Power Meter As A Bridge";
            Instructions = "Use your power meter as a bridge. To make the bridge reach bigger gaps, you need to build up your power meter. Click to charge!";
            WinText = "Your bridge reached the far shore!";
            LoseText = "Your power meter was too low - fell in!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=12; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

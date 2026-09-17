using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo177_ControlWaterfall
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 177: Control A Waterfall";
            Instructions = "Control a waterfall and travel the world hiding outdoor passages from adventurers. Observe the eternal flow.";
            WinText = "You hid every passage - secret kept!";
            LoseText = "An adventurer found a way through!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,40,50));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

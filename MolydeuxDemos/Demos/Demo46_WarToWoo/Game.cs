using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo46_WarToWoo
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 46: Explosive War Turns To Woo";
            Instructions = "A fast paced explosive war game that halfway through suddenly becomes: woo a lady. Click hearts!";
            WinText = "She said yes - mission accomplished!";
            LoseText = "She left you standing at the altar!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,40));
            need=10; timeSec=25; targetBrush=Brushes.Pink;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

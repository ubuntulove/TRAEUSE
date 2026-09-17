using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo51_HundredChildren
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 51: 100 Children, 1 Bullet Each";
            Instructions = "You have 100 children. Each time you fire a bullet, a child dies. Be precise - one shot, one target.";
            WinText = "All targets neutralized - minimal losses!";
            LoseText = "Too many wasted bullets - too few children!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,15,15));
            need=6; timeSec=20; targetBrush=Brushes.DarkRed;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

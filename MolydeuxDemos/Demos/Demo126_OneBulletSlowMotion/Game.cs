using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo126_OneBulletSlowMotion
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 126: 1 Bullet, Slow Motion";
            Instructions = "You have 1 bullet. Once fired it enters slow motion, taking hours to reach its target. Things you do affect its direction. Steer it!";
            WinText = "The bullet found its target!";
            LoseText = "The bullet veered off course!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            need=8; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

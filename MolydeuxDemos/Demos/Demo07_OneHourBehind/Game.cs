using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo07_OneHourBehind
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 07: One Hour Behind";
            Instructions = "You're 1 hour behind reality. Wait for the enemy to finish their phone call, THEN click the booth.";
            WinText = "Time finally caught up with you.";
            LoseText = "Too soon! You're still 1 hour behind!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,30));
            need=6; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

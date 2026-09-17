using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo57_BeACloud
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 57: Be A Cloud";
            Instructions = "When people talk about cloud gaming, I'm not thinking about the internet. Imagine how it must feel to BE a cloud.";
            WinText = "You rained at the perfect moment!";
            LoseText = "You evaporated from pressure!";
            GameBackground = new SolidColorBrush(Color.FromRgb(100,100,120));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

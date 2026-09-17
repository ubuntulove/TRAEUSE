using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo105_SlowFrameRate
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 105: Slow Frame Rate As A Feature";
            Instructions = "Bring in multiple characters to slow the frame rate down, enabling you to pass obstacles. Watch the slowdown work.";
            WinText = "You exploited the lag to pass!";
            LoseText = "Too many characters - crash!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

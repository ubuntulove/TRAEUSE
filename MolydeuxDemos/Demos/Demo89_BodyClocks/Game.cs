using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo89_BodyClocks
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 89: Adjust Visible Body Clocks";
            Instructions = "What if everyone had visible body clocks above them and you had the power to adjust those clocks? Click to advance them!";
            WinText = "All body clocks synchronized!";
            LoseText = "Time ran out for everyone!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,30,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

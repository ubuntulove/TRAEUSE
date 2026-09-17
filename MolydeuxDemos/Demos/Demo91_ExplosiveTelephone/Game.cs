using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo91_ExplosiveTelephone
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 91: Explosive Telephones In Bodies";
            Instructions = "Everyone has an explosive telephone in their body. Find out their number to detonate! Click the right numbers.";
            WinText = "All targets detonated!";
            LoseText = "Wrong number - you blew yourself up!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,30));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

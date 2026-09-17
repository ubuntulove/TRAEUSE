using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo45_WasCPU
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 45: Was It Your Friend, Or The CPU?";
            Instructions = "Play against a friend online... afterwards you find it was actually the CPU. Click the imposters!";
            WinText = "You unmasked the CPU impostor!";
            LoseText = "The CPU fooled you completely!";
            GameBackground = new SolidColorBrush(Color.FromRgb(15,15,30));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

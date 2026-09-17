using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo180_VentilationFamily
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 180: Secretly Support Family Via Vents";
            Instructions = "Imagine having to secretly support your family via complex ventilation passages in your large industrial home. Stay hidden.";
            WinText = "Your family thrived without ever seeing you!";
            LoseText = "They found you in the vents - police!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

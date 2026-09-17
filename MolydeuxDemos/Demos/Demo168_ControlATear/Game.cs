using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo168_ControlATear
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 168: Control A Tear On A Body";
            Instructions = "Imagine controlling a tear, similar to Flower but instead of fields you navigate a naked body. Travel the contours. Collect the path!";
            WinText = "Your tear reached its destination!";
            LoseText = "Your tear dried up too soon!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; playerBrush=Brushes.LightBlue; itemBrush=Brushes.Pink;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo66_HalfManHalfTree
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 66: Half Man, Half Tree";
            Instructions = "Half man, half tree. You can only fight when in soil and you constantly grow. Collect soil patches to grow stronger.";
            WinText = "Fully grown - you became a forest!";
            LoseText = "You withered without soil!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,40,15));
            need=10; playerBrush=Brushes.Olive; itemBrush=Brushes.Sienna;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

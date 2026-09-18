using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo213_BeTheBreadcrumbs
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 213: Be The Breadcrumbs";
            Instructions = "You are breadcrumbs. Place yourself at the right spots to guide the lost child home through the dark forest.";
            WinText = "The child found their way home!";
            LoseText = "The child is lost in the woods forever!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,25,15));
            need=10; playerBrush=Brushes.Tan; itemBrush=Brushes.LimeGreen;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

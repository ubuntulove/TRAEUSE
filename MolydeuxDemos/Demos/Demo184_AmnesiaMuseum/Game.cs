using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo184_AmnesiaMuseum
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 184: Amnesia Museum Of Your Life";
            Instructions = "3D adventure where you have amnesia and wake up in a gigantic museum where every room is devoted to a year of your life. Collect memories!";
            WinText = "You recovered your entire life!";
            LoseText = "Some years remain locked away forever!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=12; playerBrush=Brushes.White; itemBrush=Brushes.Goldenrod;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

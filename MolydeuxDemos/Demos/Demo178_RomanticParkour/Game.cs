using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo178_RomanticParkour
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 178: Romantic Parkour Holding Hands";
            Instructions = "Romantic parkour where you and the love of your life must hold hands and jump around a city evading death and injury. Collect the moments!";
            WinText = "You and your love escaped the city!";
            LoseText = "Death found you both - tragic end!";
            GameBackground = new SolidColorBrush(Color.FromRgb(50,30,40));
            need=10; playerBrush=Brushes.Pink; itemBrush=Brushes.Gold;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

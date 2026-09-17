using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo38_TouringClown
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 38: Touring Clown To Chaplin";
            Instructions = "You're a touring clown - start in the streets, work through circuses, become a film star like Chaplin. Collect applause!";
            WinText = "From street to silver screen - a star!";
            LoseText = "The audience hated you - tomatoes!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=12; playerBrush=Brushes.White; itemBrush=Brushes.Gold;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

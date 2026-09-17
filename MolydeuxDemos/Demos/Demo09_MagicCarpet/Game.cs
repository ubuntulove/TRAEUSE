using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo09_MagicCarpet
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 09: Magic Carpet";
            Instructions = "All these years and still nobody dared to make another magic carpet game. Ride through the skies.";
            WinText = "Magic carpet ride complete!";
            LoseText = "You fell off the carpet!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,40,80));
            need=10; speed=260; playerBrush=Brushes.MediumPurple; itemBrush=Brushes.White; itemRadius=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

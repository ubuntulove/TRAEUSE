using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo125_FortressOfPlants
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 125: Fortress Of Plant Family";
            Instructions = "Awake in a fortress. Your family are little plants. Escape with all plants - water them and keep them in sunlight. Collect them all!";
            WinText = "Every plant family member rescued!";
            LoseText = "Your family withered in the dark!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,40,20));
            need=10; playerBrush=Brushes.Sienna; itemBrush=Brushes.LimeGreen;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

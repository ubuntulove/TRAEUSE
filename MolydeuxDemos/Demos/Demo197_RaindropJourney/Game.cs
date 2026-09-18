using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo197_RaindropJourney
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 197: Be A Raindrop";
            Instructions = "You are a single raindrop in a storm. Fall through the cloud, dodge the lightning, land in the right garden.";
            WinText = "You watered the rose - it bloomed!";
            LoseText = "You evaporated in the heat!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,60,90));
            need=10; playerBrush=Brushes.LightBlue; itemBrush=Brushes.LimeGreen;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

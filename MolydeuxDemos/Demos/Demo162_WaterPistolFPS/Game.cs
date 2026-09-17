using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo162_WaterPistolFPS
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 162: Water Pistol FPS In Contaminated Town";
            Instructions = "An FPS with school children and water pistols, waterbombs, sprinklers in a town where the water has been contaminated. Dodge the splashes!";
            WinText = "You stayed dry through the whole town!";
            LoseText = "You got contaminated - mutation!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,40,50));
            timeSec=22; itemBrush=Brushes.LightBlue; fallSpeed=240;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

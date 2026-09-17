using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo115_EarthquakePlatformer
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 115: Earthquake Jumps";
            Instructions = "A platformer where landing on the floor creates earthquakes destroying little tribes. Don't jump. Stay perfectly still.";
            WinText = "The tribes survived - you held still!";
            LoseText = "You jumped - a tribe was wiped out!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            timeSec=14;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

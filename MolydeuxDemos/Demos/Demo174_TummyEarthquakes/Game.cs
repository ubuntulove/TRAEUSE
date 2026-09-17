using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo174_TummyEarthquakes
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 174: Tummy Rumble Earthquakes";
            Instructions = "What if your tummy rumbling created earthquakes? In a world where food is rare. Stay perfectly still to avoid rumbling!";
            WinText = "No rumbles - the world is safe!";
            LoseText = "Your tummy rumbled - earthquake!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

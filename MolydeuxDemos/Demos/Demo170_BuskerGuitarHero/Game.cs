using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo170_BuskerGuitarHero
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 170: Busker Whose Music Changes The City";
            Instructions = "In new Guitar Hero you play as a busker. Witness your city evolve as your music changes the decisions of people around you. Watch it change.";
            WinText = "Your music reshaped the city!";
            LoseText = "Your music was drowned out!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo37_SesameActors
{
    public class MainWindow : TypeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 37: Sesame Street: The Actors";
            Instructions = "Don't play the Sesame Street characters - play the actors INSIDE the costumes. Type the cues the director shouts.";
            WinText = "Curtain call - bravo!";
            LoseText = "Forgotten lines - booed off stage!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,30,50));
            need=10; timeSec=70;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

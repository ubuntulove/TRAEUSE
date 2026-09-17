using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo26_GameTurnsBoard
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 26: Videogame Becomes Boardgame";
            Instructions = "A videogame that turns into a boardgame halfway through. Wait for the transformation.";
            WinText = "Boardgame mode unlocked!";
            LoseText = "You skipped the transformation!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo110_SandcastleWarGames
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 110: Sandcastles Like WarGames";
            Instructions = "Build/destroy sandcastles but discover it's having an impact on the world. Like the movie 'WarGames' but with nature. Observe.";
            WinText = "You saw the world change with every castle!";
            LoseText = "You built too many - tsunami!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,40,25));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

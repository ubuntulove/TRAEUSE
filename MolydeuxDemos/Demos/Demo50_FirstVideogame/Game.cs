using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo50_FirstVideogame
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 50: Invent The First Videogame";
            Instructions = "Go back in time and create the first ever videogame. Click the pixels into existence!";
            WinText = "You birthed an entire medium!";
            LoseText = "The pixels wouldn't cooperate!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,20));
            need=15; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

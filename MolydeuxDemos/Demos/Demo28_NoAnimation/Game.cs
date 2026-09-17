using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo28_NoAnimation
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 28: No Animation";
            Instructions = "Is it possible to make a videogame with NO animation? Does input always require animation? Wait and see.";
            WinText = "Static victory!";
            LoseText = "You disturbed the stillness!";
            GameBackground = new SolidColorBrush(Color.FromRgb(10,10,10));
            timeSec=10;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

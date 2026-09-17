using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo173_GirlAndKite
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 173: Girl And Talking Kite";
            Instructions = "You're a small girl flying a talking kite. The kite seems to know about an upcoming major terrorist attack and floats towards clues. Follow the kite!";
            WinText = "You uncovered the terrorist plot!";
            LoseText = "The kite led you astray - trap!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,40,60));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

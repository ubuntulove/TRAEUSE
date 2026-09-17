using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo63_MagicianNoPowers
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 63: Magician With No Powers";
            Instructions = "Play as a magician with no magic powers. Entertain goblins to escape a castle by deceiving them with illusions.";
            WinText = "Goblins amazed - you escaped the castle!";
            LoseText = "Goblins saw through your tricks - devoured!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

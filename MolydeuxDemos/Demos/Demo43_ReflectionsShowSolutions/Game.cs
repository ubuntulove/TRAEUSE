using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo43_ReflectionsShowSolutions
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 43: Reflections Show Solutions";
            Instructions = "Can't get past a door? Make a puddle and look into it for the answer. Click the puddles to see solutions.";
            WinText = "All puzzles solved via reflection!";
            LoseText = "You stared too long at your own reflection!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,30,40));
            need=8; timeSec=30; targetBrush=Brushes.LightBlue;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

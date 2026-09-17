using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo116_MusicSolvesPuzzles
{
    public class MainWindow : TypeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 116: Custom Music Solves Puzzles";
            Instructions = "Custom music is the key to solving puzzles. Need cure? Classical. Attack? Rock. Fire? A song with the word 'fire' in it. Type!";
            WinText = "You orchestrated every solution!";
            LoseText = "Your playlist was off-key!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=70;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

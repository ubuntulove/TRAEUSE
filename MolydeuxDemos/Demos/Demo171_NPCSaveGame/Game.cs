using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo171_NPCSaveGame
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 171: NPCs Save Your Game";
            Instructions = "NPCs can load and save your game. Keep on their side or they won't save your game - and load old saves. Click them to stay friendly!";
            WinText = "All NPCs on your side - saves secured!";
            LoseText = "An NPC betrayed you - save wiped!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

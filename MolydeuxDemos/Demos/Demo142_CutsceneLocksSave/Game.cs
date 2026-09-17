using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo142_CutsceneLocksSave
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 142: Cutscene Locks Your Save";
            Instructions = "You know in cut scenes when it says '3 months later...'? What if the game ACTUALLY locked your save file for 3 months? Wait.";
            WinText = "You waited the full 3 months!";
            LoseText = "You couldn't wait - save corrupted!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,30));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

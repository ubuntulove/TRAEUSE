using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo76_DeathCutscene
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 76: 1 Minute Cutscene Per Kill";
            Instructions = "An action game where upon killing each enemy you see a 1 minute cutscene flashing through their entire life. Watch respectfully.";
            WinText = "You mourned every life you took.";
            LoseText = "You skipped the mourning - disrespectful!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,25));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

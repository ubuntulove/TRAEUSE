using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo175_BuryPartner
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 175: Co-op Bury Your Partner";
            Instructions = "Co-op game where if your partner dies, you spend 30 minutes digging a hole to bury them before continuing. Mourn and wait.";
            WinText = "You laid your partner to rest - continuing!";
            LoseText = "You abandoned the body - cursed!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,30));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo131_ThreePausesGameover
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 131: 3 Pauses And Game Over";
            Instructions = "3 pauses and it's game over. The game plays door bells and ringtones trying to make you accidentally pause. Don't touch anything!";
            WinText = "You never accidentally paused!";
            LoseText = "A doorbell tricked you - pause 3, game over!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo44_LookAway
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 44: Press X To Look Away";
            Instructions = "'Press X to look AWAY from the big exciting event about to happen.' Don't look. Stay still.";
            WinText = "You missed the spectacle - perfect!";
            LoseText = "You peeked - the event overwhelmed you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,15,15));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

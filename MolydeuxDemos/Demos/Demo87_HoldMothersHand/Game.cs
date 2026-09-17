using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo87_HoldMothersHand
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 87: Hold Mother's Hand";
            Instructions = "Hold onto your mother's hand. Let go for more than 1 second and it's game over. Don't move the mouse at all.";
            WinText = "You held her hand the whole way home!";
            LoseText = "You let go - lost in the crowd!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo49_MagnetVictims
{
    public class MainWindow : FollowGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 49: War Hero With Magnetic Victims";
            Instructions = "A war hero whose victims stick to him like magnets - Katamari but with a message. Keep close to gather them all.";
            WinText = "You carried your burden home.";
            LoseText = "You dropped your burden - they scattered!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,25));
            timeSec=22; radius=120; targetBrush=Brushes.DarkRed;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

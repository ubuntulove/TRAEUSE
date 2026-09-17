using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo183_AvatarFearsYou
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 183: Avatar Fears You";
            Instructions = "Imagine if your player avatar feared YOU. Press the buttons too hard and fast and the avatar becomes anxious. Stay gentle.";
            WinText = "Your avatar trusts you now!";
            LoseText = "You terrified your avatar - it ran!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

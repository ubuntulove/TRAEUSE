using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo96_TwoHundredSixBones
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 96: 206 Bones Multiplayer";
            Instructions = "Imagine a 206-player multiplayer game where each person controls a single bone. Imagine the coordination. Just watch.";
            WinText = "The body moved in perfect unison!";
            LoseText = "The skeleton collapsed - discoordination!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

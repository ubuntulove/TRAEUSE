using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo58_BoyAndHands
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 58: Boy And His Big Hands";
            Instructions = "Control a boy playing with action figures - you only control his big hands and his mouth for SFX. Click the toys!";
            WinText = "The boy's playtime was epic!";
            LoseText = "The boy got bored of you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

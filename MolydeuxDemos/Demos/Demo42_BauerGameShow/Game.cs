using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo42_BauerGameShow
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 42: Bauer In A Gameshow";
            Instructions = "You're like Jack Bauer, but to kill enemies you're transported to a gameshow. Correct answers = kills. Click fast!";
            WinText = "Show won - season finale kill count reached!";
            LoseText = "Wrong answers - the terrorists win!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            need=10; timeSec=18;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

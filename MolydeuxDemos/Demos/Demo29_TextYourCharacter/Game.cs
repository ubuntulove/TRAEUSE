using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo29_TextYourCharacter
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 29: Text Your Character";
            Instructions = "Text your character and watch them receive it. They might refuse to look. Click to nudge them to check their phone.";
            WinText = "They finally read your text!";
            LoseText = "They refused to look - heartbroken!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=8; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

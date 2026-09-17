using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo95_BearHugOxygen
{
    public class MainWindow : FollowGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 95: Bear Whose Oxygen Is Hugs";
            Instructions = "You're a bear whose oxygen comes from hugging people. Problem: hugging people breaks their bones. Stay close to refill!";
            WinText = "Oxygen refilled - you didn't break anyone!";
            LoseText = "You suffocated - or broke too many bones!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,25,15));
            timeSec=20; radius=80; targetBrush=Brushes.Pink;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

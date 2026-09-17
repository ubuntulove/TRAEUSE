using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo106_PuppetEmotions
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 106: Steal Puppets To Show Emotion";
            Instructions = "In a world where people convey feelings only through puppets, steal someone else's puppet to show your emotion. Click puppets!";
            WinText = "All emotions stolen and expressed!";
            LoseText = "You remained emotionless - puppetless!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo166_GhostInWaitingRoom
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 166: Ghost Inspires Lego In Waiting Room";
            Instructions = "You're a ghost in a doctor's waiting room - only children can see you. Inspire them to make the best lego constructions. Click to inspire!";
            WinText = "Children built masterpieces - healed!";
            LoseText = "The children grew bored - left!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

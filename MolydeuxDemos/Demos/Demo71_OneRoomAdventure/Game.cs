using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo71_OneRoomAdventure
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 71: One Room Adventure";
            Instructions = "An entire adventure game based in ONE room. Movies have done it, why not games? Click everything to find every secret.";
            WinText = "Every secret of the room uncovered!";
            LoseText = "The room kept its mysteries!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            need=12; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

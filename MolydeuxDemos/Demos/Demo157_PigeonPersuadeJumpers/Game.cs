using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo157_PigeonPersuadeJumpers
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 157: Pigeon Saves Jumpers";
            Instructions = "You're a pigeon. Persuade businessmen not to jump off buildings by retrieving items from their homes. Click to deliver items in time!";
            WinText = "All jumpers persuaded to step back!";
            LoseText = "A businessman jumped - you failed!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

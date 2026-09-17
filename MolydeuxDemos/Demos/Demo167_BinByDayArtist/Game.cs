using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo167_BinByDayArtist
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 167: Bin By Day, Modern Art By Night";
            Instructions = "You're a bin. By day people throw litter in you, but by night you use that litter to highlight issues in the city through modern art. Observe.";
            WinText = "Your art changed the city!";
            LoseText = "The bin was emptied prematurely!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

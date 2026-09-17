using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo10_BeAStatue
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 10: Be A Statue";
            Instructions = "Play as a statue. Statues don't move. Don't move.";
            WinText = "Statue perfection - utterly still!";
            LoseText = "Statues don't move - you moved!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,40,50));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

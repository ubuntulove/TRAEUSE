using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo78_HDvsSD
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 78: HD And SD Both Vital";
            Instructions = "A game where you can switch between HD and SD - both play a vital part in solving puzzles. Click HD/SD targets!";
            WinText = "Both resolutions solved every puzzle!";
            LoseText = "Wrong resolution - puzzle unsolvable!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,30,50));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo129_TimeRiverFishing
{
    public class MainWindow : CatchGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 129: Fish From Time Rivers";
            Instructions = "A fishing game where rivers are different dimensions in time. Reel in an old toy from your childhood. Catch the temporal fish!";
            WinText = "You reeled in pieces of your past!";
            LoseText = "The river carried away your memories!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,30,50));
            itemBrush=Brushes.CornflowerBlue; itemShape="triangle"; need=10; miss=3;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo145_TwoSpeedsRacing
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 145: Racing: 0 MPH Or 10,000 MPH";
            Instructions = "Racing game with only 2 speeds: 0 MPH and 10,000 MPH. Did you just run over a small child? Who knows? Dodge the obstacles!";
            WinText = "You survived the brutal commute!";
            LoseText = "At 10,000 MPH you hit a wall!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            timeSec=22; itemBrush=Brushes.Silver; fallSpeed=320;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

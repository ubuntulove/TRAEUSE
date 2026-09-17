using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo165_CelebritySerialKiller
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 165: Celebrity Serial Killer Autographs";
            Instructions = "Play a celebrity serial killer whose cult following exchanges ammunition/weapons for autographs. Sign autographs to get ammo!";
            WinText = "Fully stocked on ammo from adoring fans!";
            LoseText = "You ran out of ink - and ammo!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

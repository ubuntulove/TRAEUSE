using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo163_PluggedInGuns
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 163: Guns Plugged Into Wall Sockets";
            Instructions = "All guns are required to be plugged into wall sockets. Now THAT would be a tactical shooter. Dodge enemies who can't reach their sockets!";
            WinText = "You outmaneuvered the tethered shooters!";
            LoseText = "An enemy found a long extension cord!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            timeSec=22; itemBrush=Brushes.Yellow; fallSpeed=200;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

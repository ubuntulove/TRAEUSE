using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo74_AliveHelicopterNuke
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 74: Alive Helicopter With A Nuke";
            Instructions = "You're a small boy with an alive remote control helicopter - your friend. Then you discover a nuclear missile inside it.";
            WinText = "You disarmed the missile safely!";
            LoseText = "The nuke detonated - end of the city!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            timeSec=22; itemBrush=Brushes.OrangeRed; itemShape="triangle"; fallSpeed=280;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

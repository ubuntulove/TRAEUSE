using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo206_MeteorTrail
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 206: Be A Meteor";
            Instructions = "You are a meteor falling through the atmosphere. Dodge the debris and satellites - stay burning all the way down.";
            WinText = "You streaked across the night sky!";
            LoseText = "You burned up mid-air!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            timeSec=22; itemBrush=Brushes.Gray; fallSpeed=300;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

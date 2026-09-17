using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo114_BodyIsHourglass
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 114: Body Is An Hourglass";
            Instructions = "Your body is essentially an hourglass - an intense acrobatic game where you must make sure sand is always falling. Click to flip!";
            WinText = "Sand never stopped - perfect flow!";
            LoseText = "The sand ran out - time's up!";
            GameBackground = new SolidColorBrush(Color.FromRgb(50,40,20));
            need=12; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

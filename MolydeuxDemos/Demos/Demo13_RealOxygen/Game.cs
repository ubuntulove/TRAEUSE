using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo13_RealOxygen
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 13: A Real Sense Of Oxygen";
            Instructions = "Games always show oxygen as a draining bar. Imagine a real sense of oxygen around you - breathe slowly, don't move.";
            WinText = "You felt the oxygen - pure and present!";
            LoseText = "You suffocated on panic!";
            GameBackground = new SolidColorBrush(Color.FromRgb(10,30,50));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

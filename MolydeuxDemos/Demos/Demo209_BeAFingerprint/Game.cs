using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo209_BeAFingerprint
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 209: Be A Fingerprint";
            Instructions = "You are a fingerprint. Trace the correct ridge pattern through the maze to prove the identity.";
            WinText = "Identity confirmed - access granted!";
            LoseText = "Wrong ridge - access denied!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; playerBrush=Brushes.PaleTurquoise; itemBrush=Brushes.White;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

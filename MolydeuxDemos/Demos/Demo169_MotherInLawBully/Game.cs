using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo169_MotherInLawBully
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 169: Mother-In-Law Bully";
            Instructions = "Play a mother-in-law who bullies your son's young wife. Aim: have her thrown out within X months. Click to scheme!";
            WinText = "She was thrown out - victory!";
            LoseText = "Your son defended her - you're out!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo109_Boss10Minions
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 109: Boss With 10 Minions";
            Instructions = "A final boss has 10 minions. Your choice of interactions alters how the final boss battles you. Click each one carefully!";
            WinText = "You shaped the final battle!";
            LoseText = "You antagonized the wrong minion!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,40));
            need=10; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

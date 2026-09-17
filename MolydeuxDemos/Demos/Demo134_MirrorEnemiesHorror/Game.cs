using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo134_MirrorEnemiesHorror
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 134: Enemies Only In Mirrors";
            Instructions = "Survival horror where the enemies can ONLY be seen in mirrors. Dirty mirrors need cleaning first. Click the mirror-images!";
            WinText = "All mirror-enemies destroyed!";
            LoseText = "An enemy crept up behind you unseen!";
            GameBackground = new SolidColorBrush(Color.FromRgb(15,15,30));
            need=10; timeSec=25; targetBrush=Brushes.Silver;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

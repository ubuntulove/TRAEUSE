using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo121_HideAndSeekGhosts
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 121: Hide And Seek Ghosts";
            Instructions = "You're a child playing hide and seek. As you hide, the world fast forwards. Everyone has died. Find your friend's ghosts.";
            WinText = "All friend ghosts found - peace!";
            LoseText = "The ghosts eluded you forever!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,30));
            need=10; playerBrush=Brushes.PaleVioletRed; itemBrush=Brushes.LightGray;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

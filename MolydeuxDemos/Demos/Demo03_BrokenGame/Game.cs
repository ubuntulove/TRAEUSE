using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo03_BrokenGame
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 03: Broken Game";
            Instructions = "Dying broke the game. Normally you'd have to fix it in ANOTHER game, but here: click the broken bits to patch it.";
            WinText = "Game patched! Continue from save.";
            LoseText = "Unfixable corruption - install disc error!";
            GameBackground = new SolidColorBrush(Color.FromRgb(10,5,15));
            need=8; timeSec=25; targetBrush=Brushes.OrangeRed;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo14_LoveAsEnemy
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 14: Love Is The Enemy";
            Instructions = "Love in games is always emotion. Here, love is a physical enemy. Dodge the kisses of doom.";
            WinText = "You resisted love - cold and alone!";
            LoseText = "Love struck - game over!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,20,40));
            timeSec=25; itemBrush=Brushes.HotPink; itemShape="triangle"; fallSpeed=240;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

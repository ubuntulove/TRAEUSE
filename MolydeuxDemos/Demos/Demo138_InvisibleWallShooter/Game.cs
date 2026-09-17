using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo138_InvisibleWallShooter
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 138: Corridor Shooter Invisible Walls";
            Instructions = "A corridor shooter where all walls are invisible. Constantly worry if that enemy has open space to aim at you. Dodge!";
            WinText = "You navigated the invisible maze!";
            LoseText = "You walked into an invisible wall - shot!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            timeSec=22; itemBrush=Brushes.Red; fallSpeed=260;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

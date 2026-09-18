using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo205_SmellOfBread
{
    public class MainWindow : FollowGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 205: Be The Smell Of Fresh Bread";
            Instructions = "You are the smell of fresh bread. Drift through the street, passing close to hungry NPCs to lure them to the bakery.";
            WinText = "The bakery sold out - line around the block!";
            LoseText = "The smell dissipated in the wind!";
            GameBackground = new SolidColorBrush(Color.FromRgb(180,140,80));
            timeSec=22; radius=80; targetBrush=Brushes.PeachPuff;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

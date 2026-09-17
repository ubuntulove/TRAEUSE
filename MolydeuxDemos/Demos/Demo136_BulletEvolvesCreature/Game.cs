using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo136_BulletEvolvesCreature
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 136: Bullets Evolve Into Creatures";
            Instructions = "Every bullet you fire hits the floor and spends 10 minutes evolving into a fast creature that chases you down. Dodge your own bullets!";
            WinText = "You survived your own creations!";
            LoseText = "Your bullets came back to haunt you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,20));
            timeSec=22; itemBrush=Brushes.Gold; fallSpeed=200;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo79_PramInBattle
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 79: Pram In The Battlezone";
            Instructions = "Battle out of a battlezone while maneuvering a pram with a baby. Armor the pram and use it as cover. Dodge incoming fire!";
            WinText = "Baby extracted from the warzone alive!";
            LoseText = "The pram took a direct hit!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,20));
            timeSec=24; itemBrush=Brushes.DarkGray; fallSpeed=260;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

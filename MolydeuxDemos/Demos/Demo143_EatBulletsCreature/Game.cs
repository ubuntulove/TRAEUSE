using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo143_EatBulletsCreature
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 143: Eat Bullets To Survive";
            Instructions = "You're a creature who can only eat bullets for survival. Get people to shoot you, then eat the bullets. Dodge the non-bullet hazards!";
            WinText = "You gorged on enough bullets!";
            LoseText = "You starved - or choked on a non-bullet!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,20));
            timeSec=22; itemBrush=Brushes.Yellow; fallSpeed=280;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

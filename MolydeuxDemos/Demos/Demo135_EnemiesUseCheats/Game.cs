using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo135_EnemiesUseCheats
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 135: Enemies Use Cheat Codes";
            Instructions = "Imagine a game where the enemies are actually able to use cheat codes! Dodge their god-mode projectiles!";
            WinText = "You beat the cheating AIs!";
            LoseText = "They no-clipped behind you - dead!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,30));
            timeSec=22; itemBrush=Brushes.Magenta; fallSpeed=300;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

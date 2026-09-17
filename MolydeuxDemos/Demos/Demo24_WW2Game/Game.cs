using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo24_WW2Game
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 24: World War 2, Unlike Any Other";
            Instructions = "A WW2 game unlike any other. You're a single civilian dodging falling bombs.";
            WinText = "You survived the bombing raid!";
            LoseText = "Caught in the blast!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,20));
            timeSec=25; itemBrush=Brushes.DarkGray; itemShape="triangle"; fallSpeed=300;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo06_BeTheBall
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 06: Be The Ball";
            Instructions = "In this football game you don't control the players - you ARE the ball. Avoid the boots!";
            WinText = "You survived the entire match!";
            LoseText = "Punted into the stands!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,55,30));
            timeSec=22; itemBrush=Brushes.SaddleBrown; itemShape="square"; itemRadius=22; fallSpeed=260;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

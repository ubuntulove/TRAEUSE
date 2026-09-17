using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo41_SandpaperMan
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 41: Sandpaper Man";
            Instructions = "SANDPAPER MAN. Everything abrasive grates on you. Dodge the rough patches!";
            WinText = "Smooth sailing - sanded to perfection!";
            LoseText = "You got grated - ouch!";
            GameBackground = new SolidColorBrush(Color.FromRgb(50,30,15));
            timeSec=22; itemBrush=Brushes.Peru; itemShape="square"; fallSpeed=260;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

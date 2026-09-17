using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo60_ColorInEnemy
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 60: Color In Your Enemy";
            Instructions = "A fighting game where the mechanic is to 'colour in' parts of your enemy to create weak spots. Click to color!";
            WinText = "All weak spots revealed - KO!";
            LoseText = "You colored outside the lines!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            need=12; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

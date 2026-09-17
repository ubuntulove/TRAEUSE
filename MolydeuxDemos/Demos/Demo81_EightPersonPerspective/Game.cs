using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo81_EightPersonPerspective
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 81: Eight Person Perspective (EPS)";
            Instructions = "The world's first EPS - 8 peoples' visions overlaid on top of each other. Watch the chaos of 8 perspectives.";
            WinText = "You witnessed the 8-fold vision!";
            LoseText = "You closed your eyes - missed the moment!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

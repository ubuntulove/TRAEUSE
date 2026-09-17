using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo40_VelcroMan
{
    public class MainWindow : CatchGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 40: Velcro Man";
            Instructions = "You're VELCRO MAN. Stuff sticks to you automatically - but only soft items. Catch the right ones!";
            WinText = "Fully velcroed!";
            LoseText = "You grabbed non-velcro junk!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,40,30));
            itemBrush=Brushes.Yellow; itemShape="triangle"; need=10; miss=3;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

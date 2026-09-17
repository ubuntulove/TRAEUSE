using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo22_NoGuidePossible
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 22: Impossible To Guide";
            Instructions = "Design an adventure game that is impossible to write a gamefaqs guide for. The items move randomly every restart.";
            WinText = "You beat the unguideable!";
            LoseText = "Lost forever - no guide can save you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(25,20,30));
            need=12; speed=300;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

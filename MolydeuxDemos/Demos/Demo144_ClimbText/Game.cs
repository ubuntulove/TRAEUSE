using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo144_ClimbText
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 144: Climb The Text Around People";
            Instructions = "Everyone has text around them expressing feelings. Imagine in this game you can CLIMB the text! Collect the words to build your ladder.";
            WinText = "You climbed every emotional word!";
            LoseText = "You slipped on someone's lie!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            need=12; playerBrush=Brushes.White; itemBrush=Brushes.LightYellow;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

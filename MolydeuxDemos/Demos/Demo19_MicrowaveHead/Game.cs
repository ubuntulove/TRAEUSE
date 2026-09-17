using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo19_MicrowaveHead
{
    public class MainWindow : CatchGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 19: Microwave For A Head";
            Instructions = "You have a microwave for a head. Grab enemies and toss them in to cook them.";
            WinText = "All enemies thoroughly cooked!";
            LoseText = "Too many raw enemies slipped past!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,30,20));
            itemBrush=Brushes.OrangeRed; itemShape="square"; need=10; miss=3;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

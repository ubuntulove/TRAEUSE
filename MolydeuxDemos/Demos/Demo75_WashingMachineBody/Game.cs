using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo75_WashingMachineBody
{
    public class MainWindow : CatchGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 75: Body Is A Washing Machine";
            Instructions = "Your body is a washing machine - you must wash your enemies before you can kill them. Catch them and rinse!";
            WinText = "All enemies washed and rinsed!";
            LoseText = "Too many dirty enemies slipped away!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,40,60));
            itemBrush=Brushes.LightBlue; itemShape="square"; need=10; miss=3;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

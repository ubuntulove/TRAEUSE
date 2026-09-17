using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo01_SupermarketArm
{
    public class MainWindow : CatchGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 01: Supermarket Arm";
            Instructions = "Your left arm IS a supermarket! Catch falling groceries before they hit the floor.";
            WinText = "Arm fully stocked - checkout complete!";
            LoseText = "Too many groceries spilled on the floor!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,25,35));
            itemBrush=Brushes.Gold; itemShape="square"; need=10; miss=3; fallSpeed=220;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

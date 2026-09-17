using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo59_VendingMachine
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 59: Play As A Vending Machine";
            Instructions = "Could playing as a vending machine be fun? Dispense the right items to the right customers!";
            WinText = "All customers served - profit!";
            LoseText = "Wrong items dispensed - complaints!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

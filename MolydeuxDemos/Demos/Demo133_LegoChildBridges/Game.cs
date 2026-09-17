using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo133_LegoChildBridges
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 133: Lego Child Bridges";
            Instructions = "Your child is made of lego. Detach bricks to make bridges. Keep adding bricks to stop him getting depressed. Collect bricks!";
            WinText = "Your child is whole and smiling!";
            LoseText = "Your child crumbled - lego depression!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=12; playerBrush=Brushes.Cyan; itemBrush=Brushes.Red;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

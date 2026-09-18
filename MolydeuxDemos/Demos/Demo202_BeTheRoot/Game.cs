using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo202_BeTheRoot
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 202: Be A Growing Root";
            Instructions = "You are a seedling's root. Push downward through soil to find water - avoid the hard rocks. Grow deep.";
            WinText = "You struck the water table!";
            LoseText = "You hit bedrock - withered!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,10));
            need=10; playerBrush=Brushes.Sienna; itemBrush=Brushes.RoyalBlue;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

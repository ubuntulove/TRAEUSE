using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo04_SharedArms
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 04: Shared Arms Co-op";
            Instructions = "Co-op: you and a friend control one arm each of the protagonist. Imagine your friend is here too - use arrows + WASD.";
            WinText = "Both arms in sync - reached the goal!";
            LoseText = "Arms tangled - cooperation failed!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,30,40));
            need=8; speed=240; playerBrush=Brushes.Cyan; itemBrush=Brushes.MediumPurple;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

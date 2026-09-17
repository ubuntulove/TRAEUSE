using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo64_RepeatingStairs
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 64: Repeating Staircase";
            Instructions = "Love repeating staircases like in M64 and FF7 - disappointed when they end. Here, they never end. Keep climbing.";
            WinText = "You accepted eternal ascent!";
            LoseText = "You stopped - the staircase collapsed!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            timeSec=20;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

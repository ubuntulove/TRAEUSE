using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo108_StealAnimationFrames
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 108: Steal Animation Frames";
            Instructions = "Steal animation frames from other NPCs' animation cycles and give them to yourself or others to solve puzzles. Click frames!";
            WinText = "You assembled a perfect new animation!";
            LoseText = "Your stolen frames glitched the world!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=12; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

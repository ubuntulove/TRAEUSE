using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo98_BabyOldManLoop
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 98: Baby To Old Man Loop";
            Instructions = "You grow from baby to old man and back to baby, looping every 10 seconds. Time small gaps to slip through. Watch the loop.";
            WinText = "You accepted the eternal loop!";
            LoseText = "You broke the cycle of life!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

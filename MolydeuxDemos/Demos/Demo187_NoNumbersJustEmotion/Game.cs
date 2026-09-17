using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo187_NoNumbersJustEmotion
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 187: No Numbers, Just Smile Or Tear";
            Instructions = "Fed up with numbers in games. Don't show me how I'm doing with a number - show me with a smile or a tear. Just feel the result.";
            WinText = "You felt the smile of victory!";
            LoseText = "You felt the tear of defeat!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

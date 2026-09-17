using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo62_TVAndEmotion
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 62: Character On TV, Emotion Off-Screen";
            Instructions = "Control a character on the TV screen, but on ANOTHER screen you see their emotion. Click to manage both.";
            WinText = "Emotions perfectly managed!";
            LoseText = "Emotional breakdown on screen two!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo150_UndergroundWords
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 150: Hunt Words: I, Love, You";
            Instructions = "A world where people don't talk - physical words are hidden underground. Travel the world hunting 'I', 'love', 'you'. Collect them!";
            WinText = "You assembled 'I love you'!";
            LoseText = "The words remained scattered!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=12; playerBrush=Brushes.White; itemBrush=Brushes.HotPink;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

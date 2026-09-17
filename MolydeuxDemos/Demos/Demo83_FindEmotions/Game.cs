using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo83_FindEmotions
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 83: Find 120 Emotions";
            Instructions = "Start with no emotion. Find and obtain them, then use them to find more. Collect 8 distinct emotions to win.";
            WinText = "You became fully emotional!";
            LoseText = "You remained emotionally dead!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,40));
            need=8; playerBrush=Brushes.White; itemBrush=Brushes.HotPink;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

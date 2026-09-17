using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo176_PlayAsAHole
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 176: You Play A Hole";
            Instructions = "You play a hole. Move around an environment making certain elements fall into correct targets at the right time. Collect falling items!";
            WinText = "All elements fell through you correctly!";
            LoseText = "Items missed your hole - disaster!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,30));
            need=10; playerBrush=Brushes.Black; itemBrush=Brushes.White;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

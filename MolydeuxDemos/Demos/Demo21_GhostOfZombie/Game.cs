using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo21_GhostOfZombie
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 21: Ghost Of A Zombie";
            Instructions = "You're the ghost of a zombie, reliving memory flashbacks of the person you were. Collect your memories.";
            WinText = "All memories recovered - peace at last!";
            LoseText = "Your memories scattered to the wind!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,25));
            need=10; playerBrush=Brushes.PaleGreen; itemBrush=Brushes.Lavender;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

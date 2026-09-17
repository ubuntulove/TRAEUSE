using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo107_ResistCompleting
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 107: Resist Completing The Game";
            Instructions = "What if the aim of the game was to RESIST actually completing it? That kept me up all night. Don't touch anything.";
            WinText = "You successfully avoided finishing it!";
            LoseText = "You accidentally completed it - damn!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

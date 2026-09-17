using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo117_StickyNotesReality
{
    public class MainWindow : TypeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 117: Sticky Notes Become Reality";
            Instructions = "Write and place sticky notes in a virtual world and what they say becomes reality. Type the words to reshape the world!";
            WinText = "Reality rewritten to your liking!";
            LoseText = "Your sticky notes contradicted each other!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,40,25));
            need=8; timeSec=70;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo16_TextWorldAdventure
{
    public class MainWindow : TypeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 16: Text World Adventure";
            Instructions = "Imagine a 3D adventure where all objects are made of text. Type the words you encounter to absorb them.";
            WinText = "You absorbed the lexicon!";
            LoseText = "The words escaped you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            need=10; timeSec=75;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

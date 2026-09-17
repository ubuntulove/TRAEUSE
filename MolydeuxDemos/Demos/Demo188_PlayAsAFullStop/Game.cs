using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo188_PlayAsAFullStop
{
    public class MainWindow : RhythmGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 188: Play As A Full Stop";
            Instructions = "You control a full stop. Enemies read you stories and you have to jump in between words at the right time. Tap on the beat!";
            WinText = "All sentences punctuated perfectly!";
            LoseText = "Your timing was off - run-on sentence!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            need=10;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

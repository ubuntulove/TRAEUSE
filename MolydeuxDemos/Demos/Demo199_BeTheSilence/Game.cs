using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo199_BeTheSilence
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 199: Be The Silence Between Notes";
            Instructions = "You are the silence between musical notes. Without you the music is chaos. Hold the silence - don't move.";
            WinText = "The symphony played perfectly!";
            LoseText = "You broke the silence - cacophony!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            timeSec=14;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

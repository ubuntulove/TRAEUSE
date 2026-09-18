using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo208_CanyonEcho
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 208: Echo In A Canyon";
            Instructions = "Your shout echoes around the canyon. Click the right returning echo path to hear the message clearly.";
            WinText = "You decoded the canyon's whisper!";
            LoseText = "The echo was lost in the rocks!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,40,30));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

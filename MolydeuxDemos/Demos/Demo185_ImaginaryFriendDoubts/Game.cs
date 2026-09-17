using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo185_ImaginaryFriendDoubts
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 185: Convince Imaginary Friend";
            Instructions = "An imaginary friend who doesn't believe you are real. You must convince them that you are. Click to make your case!";
            WinText = "You convinced your imaginary friend!";
            LoseText = "Your imaginary friend faded away!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

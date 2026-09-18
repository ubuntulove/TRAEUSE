using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo193_BeAnEcho
{
    public class MainWindow : TypeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 193: Be An Echo";
            Instructions = "You are an echo in a canyon. You must repeat exactly what people shout. Type the words you hear to echo them back.";
            WinText = "Your echoes rang true - the canyon sang!";
            LoseText = "Your echo faded - silence fell!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,40,30));
            need=10; timeSec=70;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

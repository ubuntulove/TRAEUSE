using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo200_MirrorWorld
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 200: Be The Mirror";
            Instructions = "You are a mirror. Reflect the right object to the observer. Click what they need to see - the truth, not illusions.";
            WinText = "Truth reflected - mystery solved!";
            LoseText = "You cracked - illusions shattered!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,60,80));
            need=10; timeSec=25; targetBrush=Brushes.White;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

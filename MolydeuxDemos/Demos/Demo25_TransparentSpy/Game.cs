using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo25_TransparentSpy
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 25: Transparent World Spy";
            Instructions = "You're a spy in a world where the entire environment is transparent - you can't hide. Dodge the searchlights.";
            WinText = "Evaded detection!";
            LoseText = "Spotted in the transparent world!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,40,40));
            timeSec=22; itemBrush=Brushes.White; itemShape="square";
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

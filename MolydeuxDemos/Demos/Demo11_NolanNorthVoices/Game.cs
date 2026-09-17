using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo11_NolanNorthVoices
{
    public class MainWindow : TypeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 11: Nolan North, Voice Actor";
            Instructions = "Play as Nolan North recording lines. Each enemy needs a unique voice - type the word on screen with flair!";
            WinText = "All lines delivered - performance of a lifetime!";
            LoseText = "Director fired you - fluffed your lines!";
            GameBackground = new SolidColorBrush(Color.FromRgb(25,20,35));
            need=10; timeSec=75;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

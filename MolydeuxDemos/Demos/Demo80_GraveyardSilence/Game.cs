using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo80_GraveyardSilence
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 80: Graveyard Of The Fallen";
            Instructions = "A war game where at the end you walk through a graveyard looking at every headstone of those you killed - then 1 minute silence.";
            WinText = "You honored every one of your victims.";
            LoseText = "You broke the silence - dishonorable!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,25));
            timeSec=20;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

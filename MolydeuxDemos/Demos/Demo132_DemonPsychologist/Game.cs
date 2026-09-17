using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo132_DemonPsychologist
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 132: Demon Psychologist Disguise";
            Instructions = "The world is being taken over by demons. You play a human disguised as the demon psychologist of the demon leader. Choose your words!";
            WinText = "You psychoanalyzed the demon leader to death!";
            LoseText = "The demons saw through your disguise!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,15,40));
            need=8; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

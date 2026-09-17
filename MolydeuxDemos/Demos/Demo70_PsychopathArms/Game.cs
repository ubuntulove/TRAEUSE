using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo70_PsychopathArms
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 70: FPS With Psychopath Arms";
            Instructions = "FPS where your arms are controlled by a psychopath firing at innocents. Turn away from them and run. Dodge the bullets!";
            WinText = "You escaped the psychopath's arms!";
            LoseText = "The arms gunned down innocents through you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,15,20));
            timeSec=22; itemBrush=Brushes.Yellow; fallSpeed=320;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

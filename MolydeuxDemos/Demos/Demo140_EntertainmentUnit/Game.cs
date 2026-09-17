using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo140_EntertainmentUnit
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 140: Family Entertainment Unit";
            Instructions = "Control the entertainment unit in a large family home. Choose their games, music, films - all of which influence their future. Watch.";
            WinText = "You shaped the family's destiny!";
            LoseText = "The family fell apart under your curation!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

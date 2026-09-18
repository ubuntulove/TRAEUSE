using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo210_BeTheMoon
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 210: Be The Moon";
            Instructions = "You are the moon. Your gravity controls the tides. Watch them rise and fall - the sea creatures depend on your rhythm.";
            WinText = "The tides sustained the coast!";
            LoseText = "Your orbit shifted - floods!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,50));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo149_DeathAnimationBurns
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 149: Death Animation Burns Into Screen";
            Instructions = "When you kill someone, their death animation loops and burns into your screen so you have to watch it for the rest of the game. Mourn.";
            WinText = "You carried every death to the end.";
            LoseText = "You couldn't bear the burning images!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,25));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

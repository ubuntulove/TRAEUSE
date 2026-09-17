using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo55_TutorialAfterCredits
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 55: Tutorial After Credits";
            Instructions = "I'd like to make a game where the tutorial comes up AFTER the credits. Wait through them all.";
            WinText = "Tutorial unlocked - finally you can play!";
            LoseText = "You skipped the credits - no tutorial for you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(10,10,20));
            timeSec=18;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

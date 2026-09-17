using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo100_SingLullabiesHorror
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 100: Sing Lullabies In Survival Horror";
            Instructions = "Survival horror: sing lullabies to your 2 year old to stop them screaming and giving your position away. Don't move a muscle.";
            WinText = "The baby slept - you survived the night!";
            LoseText = "The baby cried - the monsters found you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(15,10,20));
            timeSec=14;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

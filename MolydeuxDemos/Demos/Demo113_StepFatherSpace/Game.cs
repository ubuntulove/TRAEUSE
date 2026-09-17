using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo113_StepFatherSpace
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 113: Step Father In Space";
            Instructions = "Your abusive step father chases you into a space rocket that accidentally blasts off. Just you, an 8 year old, and him. Alone.";
            WinText = "You survived the long silence of space!";
            LoseText = "The stepfather found you in the void!";
            GameBackground = new SolidColorBrush(Color.FromRgb(10,10,30));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

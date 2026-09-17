using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo137_CorpsesPointExit
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 137: Corpses Point To Exit";
            Instructions = "Killing enemies makes them fall to the floor and point towards the exit - that's the only way to find your way out. Click them!";
            WinText = "The corpses guided you out!";
            LoseText = "You killed in the wrong direction!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

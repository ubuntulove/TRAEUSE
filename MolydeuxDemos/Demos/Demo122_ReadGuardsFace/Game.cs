using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo122_ReadGuardsFace
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 122: Read Guard's Face";
            Instructions = "Spy on a guard's face from afar and know which route he'll take just by reading his expressions. Click the correct route!";
            WinText = "You read every guard perfectly!";
            LoseText = "You misread the face - caught!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

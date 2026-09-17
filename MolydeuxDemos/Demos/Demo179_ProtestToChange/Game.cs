using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo179_ProtestToChange
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 179: Join Protests To Change Rules";
            Instructions = "Join protests to make changes to the rules within the game's world. Click to join each protest and force the changes!";
            WinText = "All rules changed - revolution complete!";
            LoseText = "The protests were crushed!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

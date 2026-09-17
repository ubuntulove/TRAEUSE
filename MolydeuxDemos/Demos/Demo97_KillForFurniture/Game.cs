using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo97_KillForFurniture
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 97: Every Kill = New Furniture";
            Instructions = "Want our game to appeal to both sexes: for every kill you get a new piece of furniture to decorate your house. Click enemies!";
            WinText = "Your house is fully furnished!";
            LoseText = "Your house remains bare and empty!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

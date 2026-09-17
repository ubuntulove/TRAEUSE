using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo139_CutUmbilicalCord
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 139: Cut The Umbilical Cord";
            Instructions = "Like 'Cut the Rope' but called 'Cut the umbilical cord'. Don't care about giving candy to a frog. Click to cut precisely!";
            WinText = "The cord was cut cleanly!";
            LoseText = "You severed too much - bleeding!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,40));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

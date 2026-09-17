using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo56_SportsIntervalsWoo
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 56: Sports + Woo + Killer";
            Instructions = "A sports game where during intervals you must woo a special lady while tracking her killer stalker. Multitask!";
            WinText = "Won the match AND caught the stalker!";
            LoseText = "Distracted by love - the killer escaped!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

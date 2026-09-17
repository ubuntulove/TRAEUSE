using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo86_HouseOfGuns
{
    public class MainWindow : ProtectGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 86: House Made Of Guns";
            Instructions = "You live in a little house made of guns. You need many guns to fight invaders but also need a roof over your children. Defend them!";
            WinText = "Family safe and armed!";
            LoseText = "Invaders breached the gun-house!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,25));
            timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

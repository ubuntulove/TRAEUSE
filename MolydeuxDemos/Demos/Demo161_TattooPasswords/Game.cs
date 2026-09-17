using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo161_TattooPasswords
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 161: Tattoo Passwords On Enemies";
            Instructions = "Enemies have tattoo passwords for doors/upgrades. Get close to see - harder enemies have smaller, more discreet tattoos. Click precisely!";
            WinText = "All tattoos read - doors unlocked!";
            LoseText = "You misread a tattoo - locked forever!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

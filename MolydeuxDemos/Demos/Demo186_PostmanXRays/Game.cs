using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo186_PostmanXRays
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 186: Postman With X-Ray Eyes";
            Instructions = "You're a postman with x-ray eyes. Do you post that nasty letter? Do you add extra money to the little boy's birthday card? Make choices!";
            WinText = "You delivered justice through the mail!";
            LoseText = "Your choices ruined too many lives!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

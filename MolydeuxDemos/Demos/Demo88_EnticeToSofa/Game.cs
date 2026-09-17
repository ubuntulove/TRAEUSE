using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo88_EnticeToSofa
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 88: Entice People To The Sofa";
            Instructions = "Like Crazy Taxi but without wheels. Entice as many people as possible to sit on the sofa within the time limit.";
            WinText = "The sofa is full - party started!";
            LoseText = "Not enough people on the sofa!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

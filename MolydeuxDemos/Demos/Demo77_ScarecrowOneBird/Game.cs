using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo77_ScarecrowOneBird
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 77: Scarecrow With One Bird";
            Instructions = "You are a scarecrow in a world with just 1 bird. Stay still - if you scare the bird away, you've failed.";
            WinText = "The bird rested on your shoulder - peace.";
            LoseText = "You flinched - the bird flew away!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,50,15));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

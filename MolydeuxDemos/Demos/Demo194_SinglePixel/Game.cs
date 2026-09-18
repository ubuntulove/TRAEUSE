using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo194_SinglePixel
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 194: Be A Single Pixel";
            Instructions = "You are one pixel in a vast pixel-art world. Navigate the giant canvas - collect colors to complete the picture.";
            WinText = "You completed the masterpiece!";
            LoseText = "Your pixel was lost in the canvas!";
            GameBackground = new SolidColorBrush(Color.FromRgb(10,10,20));
            need=12; playerBrush=Brushes.White; itemBrush=Brushes.Red;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo67_WhiteBoxDreams
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 67: White Box Open World";
            Instructions = "Open world game set in a white box. At night you dream and place dream elements into the blank world. Collect them.";
            WinText = "Your dreams filled the blank world!";
            LoseText = "Your dreams turned to nightmares!";
            GameBackground = new SolidColorBrush(Color.FromRgb(240,240,240));
            need=10; playerBrush=Brushes.Black; itemBrush=Brushes.MediumPurple;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

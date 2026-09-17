using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo94_KatamariParts
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 94: Attach Every Enemy Part";
            Instructions = "Attach a part from EVERY enemy to yourself Katamari-style, building your powers. Collect the parts!";
            WinText = "You became a patchwork god!";
            LoseText = "You missed too many vital parts!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,30));
            need=12; playerBrush=Brushes.MediumSlateBlue; itemBrush=Brushes.OrangeRed;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo92_TransferFear
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 92: Extract And Transfer Fear";
            Instructions = "How to get past a fearless guard? Extract the fear of a pigeon you attacked and transfer it to the guard. Sequence the clicks!";
            WinText = "Guard now fears everything - walk past!";
            LoseText = "The pigeon pecked you to death!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,20));
            need=8; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo181_RadioactiveBabyTorch
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 181: Radioactive Baby As Torch";
            Instructions = "Carry a radioactive baby in a pitch black environment - the baby acts as a torch. Rocking the baby intensifies the glow. Find your way!";
            WinText = "You found your way through the dark!";
            LoseText = "The baby's glow faded - lost forever!";
            GameBackground = new SolidColorBrush(Color.FromRgb(10,10,20));
            need=10; playerBrush=Brushes.LawnGreen; itemBrush=Brushes.Yellow;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

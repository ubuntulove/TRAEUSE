using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo15_CunningAnt
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 15: The Cunning Ant";
            Instructions = "Why must bosses always be MASSIVE monsters? Fight a very cunning ant. It's tiny and fast.";
            WinText = "Ant squashed - victory!";
            LoseText = "The ant outwitted you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,30,30));
            need=10; timeSec=20; targetRadius=10; targetBrush=Brushes.Black;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

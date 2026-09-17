using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo159_SecretlyMakeFight
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 159: Make People Fight For Art";
            Instructions = "Secretly make people fight each other, then use their bodies to make artwork that enriches NPC lives. Click to orchestrate fights!";
            WinText = "Your masterpiece enriched the whole city!";
            LoseText = "The fights escalated - city in chaos!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

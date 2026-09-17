using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo164_MysticalRabbitBus
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 164: Mystical Rabbit Misses Buses";
            Instructions = "Control a mystical rabbit at a bus stop in winter. Find creative ways to make people miss their buses. Click at the right moments!";
            WinText = "Everyone missed their buses - chaos!";
            LoseText = "A bus got away - the rabbit wept!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,40,50));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

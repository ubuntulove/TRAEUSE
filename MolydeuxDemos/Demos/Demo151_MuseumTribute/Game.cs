using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo151_MuseumTribute
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 151: Build A Tribute Museum";
            Instructions = "A game where you must pay tribute to someone - collect everything about them and create a museum in their honor. Click the artifacts!";
            WinText = "The museum opened to critical acclaim!";
            LoseText = "You couldn't find enough artifacts!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=12; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

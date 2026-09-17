using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo130_SuckPolygons1995
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 130: Suck Polygons To 1995";
            Instructions = "Suck polygons out of enemies - initially they look next-gen but you can drain them to look like they're from 1995. Click to drain!";
            WinText = "All enemies reduced to PS1 graphics!";
            LoseText = "You couldn't quite downgrade them!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            need=12; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

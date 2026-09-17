using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo120_RobotSerialKiller
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 120: Robot And The Boy Serial Killer";
            Instructions = "You play a robot that must entertain a child. As he grows up, the boy becomes a serial killer. Do you stop him or help him? Click decisions!";
            WinText = "You shaped the boy's future!";
            LoseText = "You failed your programming directive!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,30));
            need=8; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

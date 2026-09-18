using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo189_BeTheDoor
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 189: Play As A Door";
            Instructions = "You ARE the door. Decide who passes through. Click visitors to inspect - let the right ones in, turn away the suspicious.";
            WinText = "Curated the perfect party inside!";
            LoseText = "An impostor slipped through the door!";
            GameBackground = new SolidColorBrush(Color.FromRgb(45,30,20));
            need=10; timeSec=25; targetBrush=Brushes.SaddleBrown;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

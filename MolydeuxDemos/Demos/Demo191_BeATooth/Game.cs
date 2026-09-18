using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo191_BeATooth
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 191: Play As A Tooth";
            Instructions = "You are a single tooth. Bite the right food at the right moment. Click soft foods, avoid biting hard stones!";
            WinText = "Chewed through the whole meal!";
            LoseText = "You cracked on a stone - ouch!";
            GameBackground = new SolidColorBrush(Color.FromRgb(255,250,240));
            need=10; timeSec=22; targetBrush=Brushes.White;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

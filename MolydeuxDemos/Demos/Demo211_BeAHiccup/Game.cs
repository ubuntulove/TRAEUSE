using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo211_BeAHiccup
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 211: Be A Hiccup";
            Instructions = "You are a hiccup. You threaten to expose a stealth mission. Dodge the guards' hearing cones - stay quiet as a hiccup can.";
            WinText = "Sneaked past undetected!";
            LoseText = "A guard heard your hiccup!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            timeSec=22; itemBrush=Brushes.Pink; fallSpeed=180;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

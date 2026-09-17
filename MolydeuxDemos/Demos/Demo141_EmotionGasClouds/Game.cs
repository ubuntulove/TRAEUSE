using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo141_EmotionGasClouds
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 141: Emotion Gas Clouds";
            Instructions = "People's emotions create gas clouds in the air which can be inhaled, mixed or blown into other people's directions. Dodge the bad ones!";
            WinText = "You inhaled only good vibes!";
            LoseText = "You choked on someone's rage cloud!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            timeSec=22; itemBrush=Brushes.Purple; fallSpeed=180;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

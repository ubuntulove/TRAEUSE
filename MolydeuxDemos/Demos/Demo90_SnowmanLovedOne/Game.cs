using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo90_SnowmanLovedOne
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 90: Snowman Loved One, Hot Body";
            Instructions = "Your loved one turned into a snowman. Yet your body needs to be as hot as an oven to survive. Dodge the cold snowballs!";
            WinText = "You stayed hot enough to survive!";
            LoseText = "You froze - your snowman wept!";
            GameBackground = new SolidColorBrush(Color.FromRgb(60,30,30));
            timeSec=22; itemBrush=Brushes.White; fallSpeed=240;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

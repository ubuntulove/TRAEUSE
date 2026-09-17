using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo33_OnlyHug
{
    public class MainWindow : FollowGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 33: Only Hug";
            Instructions = "In a world where all anyone can do is hurt each other, your only ability is to hug. Reach people to hug them.";
            WinText = "You hugged the hurting world better!";
            LoseText = "You couldn't reach them in time!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,20,40));
            timeSec=20; radius=60; targetBrush=Brushes.Pink;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo196_BeAFlag
{
    public class MainWindow : StillGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 196: Be A Flag";
            Instructions = "You are a flag. You must keep waving - but the wind is fickle. Keep up tiny motion to stay aloft without going still.";
            WinText = "The flag never stopped waving!";
            LoseText = "The flag went limp - surrender!";
            GameBackground = new SolidColorBrush(Color.FromRgb(180,30,30));
            timeSec=14;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

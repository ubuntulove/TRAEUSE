using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo23_CharityEntity
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 23: Charity Single Entity";
            Instructions = "Everyone pays to control one single entity. The further it gets, the more cash raised. Don't touch - watch it go.";
            WinText = "Massive funds raised!";
            LoseText = "You hijacked the charity run!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,40,30));
            timeSec=20;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

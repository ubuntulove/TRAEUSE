using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo47_SaveByQTE
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 47: Save Your Game Via QTE";
            Instructions = "What if the way to save your game was through a QTE? Click fast to write the save file before it corrupts!";
            WinText = "Game saved successfully!";
            LoseText = "Save file corrupted!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,30,20));
            need=12; timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo103_ListenForKeywords
{
    public class MainWindow : TypeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 103: Listen For Keywords";
            Instructions = "Go around listening to people, taking key words and transforming them into real objects to help you progress. Type the keywords!";
            WinText = "You wove their words into objects!";
            LoseText = "You couldn't catch the right words!";
            GameBackground = new SolidColorBrush(Color.FromRgb(25,20,35));
            need=10; timeSec=70;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

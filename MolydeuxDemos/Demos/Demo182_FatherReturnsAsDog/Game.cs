using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo182_FatherReturnsAsDog
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 182: Father Dies, Returns As Dog";
            Instructions = "Play a father whose family dog has gone missing. You die looking for it and return home in the body of the missing dog. Return home.";
            WinText = "You returned to your family as the dog.";
            LoseText = "The family shooed you away - strays!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,35));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

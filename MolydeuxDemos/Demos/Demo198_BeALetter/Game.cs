using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo198_BeALetter
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 198: Be A Letter In A Word";
            Instructions = "You are a letter. Race to your correct slot to form a meaningful word. Get there before the sentence breaks!";
            WinText = "The sentence read perfectly!";
            LoseText = "You spelled gibberish!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,40));
            need=10; playerBrush=Brushes.White; itemBrush=Brushes.Gold;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

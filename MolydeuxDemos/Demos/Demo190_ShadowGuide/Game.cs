using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo190_ShadowGuide
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 190: Be The Sleepwalker's Shadow";
            Instructions = "You are the shadow of a sleepwalker. Guide them around the cliff's edge by moving your shadow to lead them safely. Collect dream motes.";
            WinText = "Dawn broke - they woke safe!";
            LoseText = "They walked off the cliff - nightmare!";
            GameBackground = new SolidColorBrush(Color.FromRgb(15,15,30));
            need=10; playerBrush=Brushes.DimGray; itemBrush=Brushes.PaleGoldenrod;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

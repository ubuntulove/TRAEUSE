using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo65_PianoHeadEnemies
{
    public class MainWindow : RhythmGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 65: Piano-Keyboard Head Enemies";
            Instructions = "Enemies have dirty piano keyboards for heads. Kill them by cleaning the piano without making a noise. Tap on beat!";
            WinText = "Pianos cleaned in perfect silence!";
            LoseText = "You made a noise - they devoured you!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,20));
            need=10;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

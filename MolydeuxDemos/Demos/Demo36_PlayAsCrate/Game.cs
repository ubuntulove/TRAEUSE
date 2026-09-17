using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo36_PlayAsCrate
{
    public class MainWindow : CatchGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 36: Play As A Crate";
            Instructions = "So many games have crates, but in none can you actually PLAY as a crate. Catch falling items into your crate-top!";
            WinText = "Crate fully loaded!";
            LoseText = "Your crate tipped over!";
            GameBackground = new SolidColorBrush(Color.FromRgb(55,40,20));
            itemBrush=Brushes.Sienna; itemShape="square"; need=10; miss=3;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

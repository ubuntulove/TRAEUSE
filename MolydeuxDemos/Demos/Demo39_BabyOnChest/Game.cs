using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo39_BabyOnChest
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 39: Baby Strapped To Chest";
            Instructions = "Action game: female protagonist with a baby strapped to her chest. Must not wake the baby. Move smoothly to avoid waking.";
            WinText = "Baby slept through the whole mission!";
            LoseText = "The baby woke up crying!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,25,30));
            timeSec=22; itemBrush=Brushes.LightPink; fallSpeed=180;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

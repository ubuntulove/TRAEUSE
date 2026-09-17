using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo155_InventorySeed
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 155: Inventory Becomes A Seed";
            Instructions = "When you're about to die, make your inventory a seed and plant it. Then die, go to the spot, and wait for your inventory to grow back.";
            WinText = "Your inventory sprouted anew!";
            LoseText = "You forgot where you planted it!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,40,25));
            timeSec=15;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

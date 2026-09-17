using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo85_FaceIsMirror
{
    public class MainWindow : IdleGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 85: Your Face Is A Mirror";
            Instructions = "What if your face was a mirror? Reflect the world. Don't move - just reflect.";
            WinText = "You reflected the world perfectly!";
            LoseText = "You cracked the mirror - bad luck!";
            GameBackground = new SolidColorBrush(Color.FromRgb(40,40,60));
            timeSec=12;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

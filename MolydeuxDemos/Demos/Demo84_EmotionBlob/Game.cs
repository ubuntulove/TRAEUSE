using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo84_EmotionBlob
{
    public class MainWindow : MoveGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 84: Force Emotions On Your Blob";
            Instructions = "Like 'A Boy and his Blob' but instead of beans you force emotions on him. Collect the right emotions to feed your blob.";
            WinText = "Your blob became content with its feelings!";
            LoseText = "Your blob became emotionally unstable!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,40));
            need=10; playerBrush=Brushes.PaleVioletRed; itemBrush=Brushes.Gold;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

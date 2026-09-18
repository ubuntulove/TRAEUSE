using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo203_SingleFrame
{
    public class MainWindow : RhythmGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 203: Be A Single Frame";
            Instructions = "You are one frame of animation. You only exist for a split second - jump into the right position exactly on the beat.";
            WinText = "Animation played out smoothly!";
            LoseText = "You dropped a frame - stutter!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,30));
            need=10;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

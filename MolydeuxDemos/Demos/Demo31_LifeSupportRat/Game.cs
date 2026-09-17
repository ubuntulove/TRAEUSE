using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo31_LifeSupportRat
{
    public class MainWindow : ProtectGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 31: Life Support Machine / Diseased Rat";
            Instructions = "By day you're a life support machine. By night you turn into a diseased rat. Defend the patient!";
            WinText = "The patient survived the night!";
            LoseText = "The patient flatlined!";
            GameBackground = new SolidColorBrush(Color.FromRgb(15,30,15));
            timeSec=28;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

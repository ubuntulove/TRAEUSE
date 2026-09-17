using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo156_FatherFacePortal
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 156: Father's Face Is A Portal";
            Instructions = "Your face and your father's face is a portal. You're on Earth, he is in Space. Pass items to each other to get back together. Click the items!";
            WinText = "Father and child reunited across dimensions!";
            LoseText = "The portal closed forever!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,20,40));
            need=10; timeSec=25;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

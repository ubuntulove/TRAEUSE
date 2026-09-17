using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo127_ToxicPuppetWorld
{
    public class MainWindow : ProtectGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 127: Toxic Puppet World";
            Instructions = "A large open world full of toxic poison. Everyone wears gas masks and uses puppets for communication. Defend the puppet-master!";
            WinText = "The puppet-master survived!";
            LoseText = "The gas seeped through the masks!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,30,30));
            timeSec=28;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

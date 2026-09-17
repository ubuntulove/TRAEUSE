using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo73_StretchSpell
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 73: Stretch And Squeeze Spells";
            Instructions = "Imagine being able to create a spell and stretch and squeeze it with your fingertips. Click to mold the spell to shape.";
            WinText = "Spell perfectly molded - cast!";
            LoseText = "Spell slipped through your fingers!";
            GameBackground = new SolidColorBrush(Color.FromRgb(30,20,50));
            need=10; timeSec=22;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

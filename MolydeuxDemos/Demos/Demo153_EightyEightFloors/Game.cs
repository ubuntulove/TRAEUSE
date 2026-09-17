using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo153_EightyEightFloors
{
    public class MainWindow : ClickGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 153: 88 Floors, 88 Souls";
            Instructions = "Awake in an 88 floor skyscraper alone. Every floor is haunted by one of 88 innocent people you killed in an illegal war. Click to ascend.";
            WinText = "You reached the top - atoned for 88 souls!";
            LoseText = "The guilt crushed you on a lower floor!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,25));
            need=12; timeSec=30;
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

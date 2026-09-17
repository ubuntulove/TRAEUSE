using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace Demo68_TransformersAbuse
{
    public class MainWindow : DodgeGame
    {
        public MainWindow()
        {
            DemoTitle = "Demo 68: Transformers In Abusive Home";
            Instructions = "A Transformers game where you control a boy playing with figures in a house filled with family abuse. Dodge the yelling.";
            WinText = "You escaped into imagination!";
            LoseText = "The violence found you in your fantasy!";
            GameBackground = new SolidColorBrush(Color.FromRgb(20,15,25));
            timeSec=22; itemBrush=Brushes.DarkRed; itemShape="triangle";
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

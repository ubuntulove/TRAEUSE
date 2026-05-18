using System.Windows;

namespace PlatformGameEngine
{
    public partial class MainWindow : Window
    {
        private GameEngine _gameEngine;

        public MainWindow()
        {
            InitializeComponent();
            _gameEngine = new GameEngine(GameCanvas);
            Loaded += (s, e) => _gameEngine.Start();
            Closed += (s, e) => _gameEngine.Stop();
        }
    }
}
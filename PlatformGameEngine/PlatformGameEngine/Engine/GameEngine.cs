using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace PlatformGameEngine.Engine
{
    public class GameEngine
    {
        private Canvas _canvas;
        private DispatcherTimer _gameTimer;
        private List<GameObject> _gameObjects;
        private Player _player;
        private float _gravity;
        private const float TargetFps = 60;
        private const float DeltaTime = 1f / TargetFps;

        public GameEngine(Canvas canvas)
        {
            _canvas = canvas;
            _gameObjects = new List<GameObject>();
            _gravity = 980f;
            SetupInputHandling();
            CreateSampleLevel();
        }

        private void SetupInputHandling()
        {
            _canvas.Focusable = true;
            _canvas.KeyDown += (s, e) => InputManager.KeyDown(e.Key);
            _canvas.KeyUp += (s, e) => InputManager.KeyUp(e.Key);
        }

        public void Start()
        {
            _canvas.Focus();
            _gameTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(DeltaTime)
            };
            _gameTimer.Tick += GameLoop;
            _gameTimer.Start();
        }

        public void Stop()
        {
            _gameTimer?.Stop();
        }

        private void GameLoop(object sender, EventArgs e)
        {
            InputManager.Update();
            Update();
            Render();
        }

        private void Update()
        {
            foreach (var obj in _gameObjects)
            {
                if (!obj.IsStatic && !obj.IsActive) continue;
                
                if (!obj.IsStatic)
                {
                    obj.Velocity += new Vector2(0, _gravity) * DeltaTime;
                }
                obj.Update(DeltaTime);
            }

            HandleCollisions();
        }

        private void HandleCollisions()
        {
            foreach (var obj in _gameObjects)
            {
                if (obj.IsStatic || !obj.IsActive) continue;

                foreach (var other in _gameObjects)
                {
                    if (obj == other || !other.IsActive) continue;

                    if (CollisionDetector.CheckCollision(obj, other))
                    {
                        if (obj is Player player)
                        {
                            if (other is Collectible collectible && !collectible.Collected)
                            {
                                collectible.Collect();
                                continue;
                            }

                            if (other is Enemy)
                            {
                                player.Position = new Vector2(100, 100);
                                player.Velocity = Vector2.Zero;
                                continue;
                            }
                        }

                        Vector2 normal = CollisionDetector.GetCollisionNormal(obj.Bounds, other.Bounds);
                        float overlap = CollisionDetector.GetOverlapAmount(obj.Bounds, other.Bounds);

                        obj.Position += normal * overlap;
                        
                        if (normal.Y != 0)
                        {
                            obj.Velocity = new Vector2(obj.Velocity.X, 0);
                        }
                        if (normal.X != 0)
                        {
                            obj.Velocity = new Vector2(0, obj.Velocity.Y);
                        }

                        if (obj is Player p)
                        {
                            if (normal.Y < 0)
                            {
                                p.IsGrounded = true;
                            }
                        }
                    }
                }
            }
        }

        private void Render()
        {
            _canvas.Children.Clear();

            foreach (var obj in _gameObjects)
            {
                if (!obj.IsActive) continue;

                var rectangle = new Rectangle
                {
                    Width = obj.Width,
                    Height = obj.Height,
                    Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(obj.Color.R, obj.Color.G, obj.Color.B))
                };

                Canvas.SetLeft(rectangle, obj.Position.X);
                Canvas.SetTop(rectangle, obj.Position.Y);
                _canvas.Children.Add(rectangle);
            }
        }

        public void AddGameObject(GameObject obj)
        {
            _gameObjects.Add(obj);
            if (obj is Player)
            {
                _player = (Player)obj;
            }
        }

        private void CreateSampleLevel()
        {
            _player = new Player(new Vector2(100, 100));
            AddGameObject(_player);

            AddGameObject(new Platform(new Vector2(0, 500), 200, 50, Color.Green));
            AddGameObject(new Platform(new Vector2(250, 450), 150, 30, Color.Green));
            AddGameObject(new Platform(new Vector2(450, 400), 150, 30, Color.Green));
            AddGameObject(new Platform(new Vector2(350, 300), 200, 30, Color.Green));
            AddGameObject(new Platform(new Vector2(100, 250), 150, 30, Color.Green));
            AddGameObject(new Platform(new Vector2(550, 200), 150, 30, Color.Green));
            AddGameObject(new Platform(new Vector2(600, 350), 150, 30, Color.Green));

            AddGameObject(new Enemy(new Vector2(300, 420), 100));
            AddGameObject(new Enemy(new Vector2(500, 370), 120));

            AddGameObject(new Collectible(new Vector2(300, 280)));
            AddGameObject(new Collectible(new Vector2(450, 280)));
            AddGameObject(new Collectible(new Vector2(600, 180)));
        }
    }
}
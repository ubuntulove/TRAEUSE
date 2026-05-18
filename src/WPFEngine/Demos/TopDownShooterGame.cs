using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WPFEngine.Core;
using WPFEngine.Math;
using WPFEngine.Input;
using WPFEngine.Audio;

namespace WPFEngine.Demos
{
    public enum TDSGameState
    {
        Menu,
        Playing,
        Paused,
        WaveComplete,
        GameOver
    }

    public enum TDSEnemyType
    {
        Grunt,
        Shooter,
        Runner,
        Tank,
        Swarm,
        Boss
    }

    public enum TDSWeaponType
    {
        Pistol,
        Shotgun,
        MachineGun,
        Rocket
    }

    public enum TDSPowerUpType
    {
        Health,
        Shield,
        Ammo,
        Speed,
        Damage,
        WeaponUpgrade
    }

    public class TopDownShooterGame : GameBase
    {
        private TDSGameState _gameState;
        private TDSGameState _previousState;
        private Player _player;
        private Camera2D _camera;
        private Vector2 _arenaCenter;
        private Vector2 _arenaSize;
        private int _currentWave;
        private int _score;
        private int _lives;
        private float _waveTimer;
        private float _waveIntermissionTime;
        private bool _isWaveActive;
        private List<Enemy> _enemies;
        private List<Bullet> _bullets;
        private List<PowerUp> _powerUps;
        private List<CoverObject> _coverObjects;
        private List<Wall> _walls;
        private List<SpawnZone> _spawnZones;
        private float _screenShakeIntensity;
        private float _screenShakeDuration;
        private float _screenShakeTimer;
        private float _waveAnnouncementTimer;
        private float _damageFlashTimer;
        private bool _autoFireEnabled;
        private Vector2 _lastMousePosition;
        private Random _random;

        public TDSGameState GameState => _gameState;
        public int CurrentWave => _currentWave;
        public int Score => _score;
        public int Lives => _lives;
        public Player Player => _player;

        public event EventHandler<int>? ScoreChanged;
        public event EventHandler<int>? WaveChanged;
        public event EventHandler<int>? LivesChanged;
        public event EventHandler? WaveStarted;
        public event EventHandler? WaveCompleted;
        public event EventHandler? GameOver;
        public event EventHandler? PlayerDamaged;

        public TopDownShooterGame()
        {
            _gameState = TDSGameState.Menu;
            _previousState = TDSGameState.Menu;
            _currentWave = 0;
            _score = 0;
            _lives = 3;
            _waveTimer = 0f;
            _waveIntermissionTime = 3f;
            _isWaveActive = false;
            _enemies = new List<Enemy>();
            _bullets = new List<Bullet>();
            _powerUps = new List<PowerUp>();
            _coverObjects = new List<CoverObject>();
            _walls = new List<Wall>();
            _spawnZones = new List<SpawnZone>();
            _screenShakeIntensity = 0f;
            _screenShakeDuration = 0f;
            _screenShakeTimer = 0f;
            _waveAnnouncementTimer = 0f;
            _damageFlashTimer = 0f;
            _autoFireEnabled = false;
            _lastMousePosition = Vector2.Zero;
            _random = new Random();
            _arenaCenter = new Vector2(960, 540);
            _arenaSize = new Vector2(1800, 1000);
        }

        protected override void Initialize()
        {
            base.Initialize();
            _camera = new Camera2D { Position = _arenaCenter, Zoom = 1f };
            CreateArena();
        }

        private void CreateArena()
        {
            float arenaLeft = _arenaCenter.X - _arenaSize.X / 2;
            float arenaRight = _arenaCenter.X + _arenaSize.X / 2;
            float arenaTop = _arenaCenter.Y - _arenaSize.Y / 2;
            float arenaBottom = _arenaCenter.Y + _arenaSize.Y / 2;

            _walls.Add(new Wall(new Vector2(arenaLeft - 20, _arenaCenter.Y), new Vector2(40, _arenaSize.Y + 80)));
            _walls.Add(new Wall(new Vector2(arenaRight + 20, _arenaCenter.Y), new Vector2(40, _arenaSize.Y + 80)));
            _walls.Add(new Wall(new Vector2(_arenaCenter.X, arenaTop - 20), new Vector2(_arenaSize.X + 80, 40)));
            _walls.Add(new Wall(new Vector2(_arenaCenter.X, arenaBottom + 20), new Vector2(_arenaSize.X + 80, 40)));

            _spawnZones.Add(new SpawnZone(new Vector2(arenaLeft + 100, arenaTop + 100), new Vector2(150, 150), 0));
            _spawnZones.Add(new SpawnZone(new Vector2(arenaRight - 100, arenaTop + 100), new Vector2(150, 150), 1));
            _spawnZones.Add(new SpawnZone(new Vector2(arenaLeft + 100, arenaBottom - 100), new Vector2(150, 150), 2));
            _spawnZones.Add(new SpawnZone(new Vector2(arenaRight - 100, arenaBottom - 100), new Vector2(150, 150), 3));
            _spawnZones.Add(new SpawnZone(new Vector2(_arenaCenter.X, arenaTop + 80), new Vector2(200, 100), 4));
            _spawnZones.Add(new SpawnZone(new Vector2(_arenaCenter.X, arenaBottom - 80), new Vector2(200, 100), 5));

            CreateCoverObjects();
        }

        private void CreateCoverObjects()
        {
            _coverObjects.Add(new CoverObject(new Vector2(_arenaCenter.X - 200, _arenaCenter.Y), new Vector2(80, 40)));
            _coverObjects.Add(new CoverObject(new Vector2(_arenaCenter.X + 200, _arenaCenter.Y), new Vector2(80, 40)));
            _coverObjects.Add(new CoverObject(new Vector2(_arenaCenter.X, _arenaCenter.Y - 150), new Vector2(40, 80)));
            _coverObjects.Add(new CoverObject(new Vector2(_arenaCenter.X, _arenaCenter.Y + 150), new Vector2(40, 80)));

            float offsetX = 300;
            float offsetY = 200;
            _coverObjects.Add(new CoverObject(new Vector2(_arenaCenter.X - offsetX, _arenaCenter.Y - offsetY), new Vector2(60, 60)));
            _coverObjects.Add(new CoverObject(new Vector2(_arenaCenter.X + offsetX, _arenaCenter.Y - offsetY), new Vector2(60, 60)));
            _coverObjects.Add(new CoverObject(new Vector2(_arenaCenter.X - offsetX, _arenaCenter.Y + offsetY), new Vector2(60, 60)));
            _coverObjects.Add(new CoverObject(new Vector2(_arenaCenter.X + offsetX, _arenaCenter.Y + offsetY), new Vector2(60, 60)));
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);
            UpdateInput();
            UpdateGameState(deltaTime);

            if (_gameState == TDSGameState.Playing)
            {
                UpdatePlayer(deltaTime);
                UpdateEnemies(deltaTime);
                UpdateBullets(deltaTime);
                UpdatePowerUps(deltaTime);
                UpdateWaveLogic(deltaTime);
                CheckCollisions();
            }

            UpdateScreenShake(deltaTime);

            if (_damageFlashTimer > 0)
                _damageFlashTimer -= deltaTime;
        }

        private void UpdateInput()
        {
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Escape))
            {
                if (_gameState == TDSGameState.Playing)
                    SetGameState(TDSGameState.Paused);
                else if (_gameState == TDSGameState.Paused)
                    SetGameState(TDSGameState.Playing);
            }

            if (_gameState == TDSGameState.Menu && InputManager.Instance.GetKeyDown(InputManager.KeyCode.Enter))
                StartGame();

            if (_gameState == TDSGameState.GameOver && InputManager.Instance.GetKeyDown(InputManager.KeyCode.R))
                RestartGame();

            if (_gameState == TDSGameState.Playing)
            {
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.F))
                    _autoFireEnabled = !_autoFireEnabled;

                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D1))
                    _player.SwitchWeapon(TDSWeaponType.Pistol);
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D2))
                    _player.SwitchWeapon(TDSWeaponType.Shotgun);
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D3))
                    _player.SwitchWeapon(TDSWeaponType.MachineGun);
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D4))
                    _player.SwitchWeapon(TDSWeaponType.Rocket);

                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.R))
                    _player.StartReload();

                if (InputManager.Instance.GetMouseButtonDown(0) || (_autoFireEnabled && InputManager.Instance.GetMouseButton(0)))
                    _player.Fire();

                Vector2 mousePos = InputManager.Instance.MousePosition;
                if (mousePos != _lastMousePosition)
                {
                    _player.AimAt(mousePos);
                    _lastMousePosition = mousePos;
                }
            }
        }

        private void UpdateGameState(float deltaTime)
        {
            if (_screenShakeTimer > 0)
                _screenShakeTimer -= deltaTime;

            if (_waveAnnouncementTimer > 0)
                _waveAnnouncementTimer -= deltaTime;
        }

        private void UpdatePlayer(float deltaTime)
        {
            float moveX = InputManager.Instance.GetAxis(InputAxis.Horizontal);
            float moveY = InputManager.Instance.GetAxis(InputAxis.Vertical);
            Vector2 moveInput = new Vector2(moveX, moveY);

            _player.Update(deltaTime, moveInput);

            float camOffsetX = _player.Position.X - _camera.Position.X;
            float camOffsetY = _player.Position.Y - _camera.Position.Y;
            float camSmooth = 5f * deltaTime;
            _camera.Position = new Vector2(
                _camera.Position.X + camOffsetX * camSmooth,
                _camera.Position.Y + camOffsetY * camSmooth
            );

            if (_player.Health <= 0)
            {
                HandlePlayerDeath();
            }
        }

        private void UpdateEnemies(float deltaTime)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                enemy.Update(deltaTime, _player.Position);

                if (enemy.Health <= 0)
                {
                    OnEnemyKilled(enemy);
                    _enemies.RemoveAt(i);
                }
            }
        }

        private void UpdateBullets(float deltaTime)
        {
            for (int i = _bullets.Count - 1; i >= 0; i--)
            {
                var bullet = _bullets[i];
                bullet.Update(deltaTime);

                if (bullet.IsExpired || CheckBulletWallCollision(bullet))
                {
                    if (bullet.IsExpired && bullet.DamageType == BulletDamageType.Explosive)
                        CreateExplosion(bullet.Position, 80);
                    _bullets.RemoveAt(i);
                }
            }
        }

        private void UpdatePowerUps(float deltaTime)
        {
            for (int i = _powerUps.Count - 1; i >= 0; i--)
            {
                var powerUp = _powerUps[i];
                powerUp.Update(deltaTime);

                if (powerUp.IsCollected)
                {
                    ApplyPowerUp(powerUp);
                    _powerUps.RemoveAt(i);
                }
                else if (powerUp.IsExpired)
                {
                    _powerUps.RemoveAt(i);
                }
            }
        }

        private void UpdateWaveLogic(float deltaTime)
        {
            if (!_isWaveActive && _waveTimer <= 0)
            {
                _waveTimer = _waveIntermissionTime;
                _isWaveActive = true;
                StartWave(_currentWave);
            }

            if (_isWaveActive && _enemies.Count == 0)
            {
                _isWaveActive = false;
                _currentWave++;
                _waveAnnouncementTimer = 2f;
                WaveCompleted?.Invoke(this, EventArgs.Empty);
                SetGameState(TDSGameState.WaveComplete);
                SpawnWaveBonusPowerUps();
            }
        }

        private void CheckCollisions()
        {
            foreach (var bullet in _bullets)
            {
                if (bullet.IsPlayerBullet)
                {
                    foreach (var enemy in _enemies)
                    {
                        float dist = (bullet.Position - enemy.Position).Length();
                        if (dist < enemy.Radius + bullet.Radius)
                        {
                            enemy.TakeDamage(bullet.Damage);
                            bullet.IsExpired = true;
                            CreateHitEffect(bullet.Position, enemy.IsBoss ? Color.Purple : Color.Red);
                        }
                    }
                }
                else
                {
                    float dist = (bullet.Position - _player.Position).Length();
                    if (dist < _player.Radius + bullet.Radius && !_player.IsInvincible)
                    {
                        _player.TakeDamage(bullet.Damage);
                        bullet.IsExpired = true;
                        ShakeScreen(0.2f, 0.15f);
                        _damageFlashTimer = 0.1f;
                        PlayerDamaged?.Invoke(this, EventArgs.Empty);
                    }
                }
            }

            foreach (var enemy in _enemies)
            {
                float dist = (enemy.Position - _player.Position).Length();
                if (dist < enemy.Radius + _player.Radius && !_player.IsInvincible)
                {
                    _player.TakeDamage(enemy.MeleeDamage);
                    enemy.Position = enemy.Position + (enemy.Position - _player.Position).Normalized() * 30f;
                    ShakeScreen(0.15f, 0.1f);
                    _damageFlashTimer = 0.1f;
                    PlayerDamaged?.Invoke(this, EventArgs.Empty);
                }
            }

            foreach (var powerUp in _powerUps)
            {
                if (powerUp.IsCollected) continue;
                float dist = (powerUp.Position - _player.Position).Length();
                if (dist < _player.Radius + powerUp.Radius)
                {
                    powerUp.IsCollected = true;
                }
            }
        }

        private bool CheckBulletWallCollision(Bullet bullet)
        {
            foreach (var wall in _walls)
            {
                if (IsInsideRect(bullet.Position, wall.Position, wall.Size))
                    return true;
            }

            foreach (var cover in _coverObjects)
            {
                if (IsInsideRect(bullet.Position, cover.Position, cover.Size))
                    return true;
            }

            return false;
        }

        private bool IsInsideRect(Vector2 point, Vector2 rectCenter, Vector2 rectSize)
        {
            float halfW = rectSize.X / 2;
            float halfH = rectSize.Y / 2;
            return point.X > rectCenter.X - halfW && point.X < rectCenter.X + halfW &&
                   point.Y > rectCenter.Y - halfH && point.Y < rectCenter.Y + halfH;
        }

        private void UpdateScreenShake(float deltaTime)
        {
            if (_screenShakeTimer > 0)
            {
                float shake = _screenShakeIntensity * (_screenShakeTimer / _screenShakeDuration);
                _camera.Position = _camera.Position + new Vector2(
                    (float)(_random.NextDouble() * 2 - 1) * shake * 10,
                    (float)(_random.NextDouble() * 2 - 1) * shake * 10
                );
            }
        }

        public void StartGame()
        {
            _score = 0;
            _lives = 3;
            _currentWave = 1;
            _waveTimer = 0f;
            _isWaveActive = false;
            _enemies.Clear();
            _bullets.Clear();
            _powerUps.Clear();

            _player = new Player(_arenaCenter);
            _player.Initialize();
            _player.WeaponFired += OnPlayerWeaponFired;

            ScoreChanged?.Invoke(this, _score);
            LivesChanged?.Invoke(this, _lives);
            WaveChanged?.Invoke(this, _currentWave);

            SetGameState(TDSGameState.Playing);
        }

        public void RestartGame()
        {
            StartGame();
        }

        private void StartWave(int waveNumber)
        {
            WaveChanged?.Invoke(this, waveNumber);
            WaveStarted?.Invoke(this, EventArgs.Empty);

            bool isBossWave = waveNumber % 5 == 0;
            SpawnWaveEnemies(waveNumber, isBossWave);
        }

        private void SpawnWaveEnemies(int wave, bool isBoss)
        {
            int gruntCount = 3 + wave * 2;
            int shooterCount = Math.Max(0, wave - 1) * 2;
            int runnerCount = Math.Max(0, wave - 2) * 2;
            int tankCount = Math.Max(0, wave - 3);
            int swarmCount = wave >= 4 ? (wave - 3) * 3 : 0;

            for (int i = 0; i < gruntCount; i++)
                SpawnEnemy(TDSEnemyType.Grunt, wave);

            for (int i = 0; i < shooterCount; i++)
                SpawnEnemy(TDSEnemyType.Shooter, wave);

            for (int i = 0; i < runnerCount; i++)
                SpawnEnemy(TDSEnemyType.Runner, wave);

            for (int i = 0; i < tankCount; i++)
                SpawnEnemy(TDSEnemyType.Tank, wave);

            for (int i = 0; i < swarmCount; i++)
                SpawnEnemy(TDSEnemyType.Swarm, wave);

            if (isBoss)
                SpawnEnemy(TDSEnemyType.Boss, wave);
        }

        private void SpawnEnemy(TDSEnemyType type, int wave)
        {
            SpawnZone zone = _spawnZones[_random.Next(_spawnZones.Count)];
            Vector2 spawnPos = zone.GetRandomPosition(_random);
            float margin = 50f;
            spawnPos = new Vector2(
                Math.Clamp(spawnPos.X, _arenaCenter.X - _arenaSize.X / 2 + margin, _arenaCenter.X + _arenaSize.X / 2 - margin),
                Math.Clamp(spawnPos.Y, _arenaCenter.Y - _arenaSize.Y / 2 + margin, _arenaCenter.Y + _arenaSize.Y / 2 - margin)
            );

            var enemy = new Enemy(spawnPos, type);
            enemy.Initialize(wave);
            _enemies.Add(enemy);
        }

        private void OnPlayerWeaponFired(object sender, Bullet bullet)
        {
            _bullets.Add(bullet);
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            int points = enemy.PointsValue;
            _score += points;
            ScoreChanged?.Invoke(this, _score);

            CreateDeathEffect(enemy.Position, enemy.IsBoss ? Color.Purple : Color.Red);

            if (_random.NextDouble() < 0.15)
                SpawnPowerUp(enemy.Position);

            if (enemy.IsBoss)
            {
                _score += 500;
                ScoreChanged?.Invoke(this, _score);
                SpawnBossPowerUpDrop(enemy.Position);
                ShakeScreen(0.5f, 0.5f);
            }
        }

        private void SpawnPowerUp(Vector2 position)
        {
            TDSPowerUpType type = (TDSPowerUpType)_random.Next(6);
            var powerUp = new PowerUp(position, type);
            _powerUps.Add(powerUp);
        }

        private void SpawnBossPowerUpDrop(Vector2 position)
        {
            foreach (TDSPowerUpType type in Enum.GetValues<TDSPowerUpType>())
            {
                Vector2 offset = new Vector2((float)(_random.NextDouble() * 60 - 30), (float)(_random.NextDouble() * 60 - 30));
                var powerUp = new PowerUp(position + offset, type);
                _powerUps.Add(powerUp);
            }
        }

        private void SpawnWaveBonusPowerUps()
        {
            for (int i = 0; i < 3; i++)
            {
                Vector2 pos = new Vector2(
                    _arenaCenter.X + (float)(_random.NextDouble() * 400 - 200),
                    _arenaCenter.Y + (float)(_random.NextDouble() * 300 - 150)
                );
                TDSPowerUpType type = (TDSPowerUpType)_random.Next(6);
                _powerUps.Add(new PowerUp(pos, type));
            }
        }

        private void ApplyPowerUp(PowerUp powerUp)
        {
            switch (powerUp.Type)
            {
                case TDSPowerUpType.Health:
                    _player.Health = Math.Min(_player.MaxHealth, _player.Health + 25);
                    break;
                case TDSPowerUpType.Shield:
                    _player.Shield = Math.Min(_player.MaxShield, _player.Shield + 30);
                    break;
                case TDSPowerUpType.Ammo:
                    _player.RefillAllAmmo();
                    break;
                case TDSPowerUpType.Speed:
                    _player.ApplySpeedBoost(5f, 10f);
                    break;
                case TDSPowerUpType.Damage:
                    _player.ApplyDamageBoost(1.5f, 10f);
                    break;
                case TDSPowerUpType.WeaponUpgrade:
                    _player.UpgradeCurrentWeapon();
                    break;
            }
        }

        private void CreateHitEffect(Vector2 position, Color color)
        {
            for (int i = 0; i < 5; i++)
            {
                Vector2 vel = new Vector2((float)(_random.NextDouble() * 200 - 100), (float)(_random.NextDouble() * 200 - 100));
                var spark = new Bullet(new Vector2(position.X, position.Y), vel.Normalized() * 100, 5f, color, 0.2f, false, BulletDamageType.Normal, 3f);
                spark.IsExpired = false;
            }
        }

        private void CreateDeathEffect(Vector2 position, Color color)
        {
            ShakeScreen(0.15f, 0.1f);
        }

        private void CreateExplosion(Vector2 position, float radius)
        {
            ShakeScreen(0.3f, 0.2f);

            foreach (var enemy in _enemies)
            {
                float dist = (enemy.Position - position).Length();
                if (dist < radius)
                {
                    float damage = 50f * (1f - dist / radius);
                    enemy.TakeDamage(damage);
                }
            }

            foreach (var cover in _coverObjects)
            {
                float dist = (cover.Position - position).Length();
                if (dist < radius + 20)
                    cover.IsDestroyed = true;
            }
        }

        private void HandlePlayerDeath()
        {
            _lives--;
            LivesChanged?.Invoke(this, _lives);

            if (_lives <= 0)
            {
                SetGameState(TDSGameState.GameOver);
                GameOver?.Invoke(this, EventArgs.Empty);
            }
            else
            {
                _player.Respawn(_arenaCenter);
                _player.Health = _player.MaxHealth;
                _player.Shield = _player.MaxShield / 2;
            }
        }

        private void SetGameState(TDSGameState newState)
        {
            _previousState = _gameState;
            _gameState = newState;
        }

        public void ShakeScreen(float intensity, float duration)
        {
            _screenShakeIntensity = Math.Max(_screenShakeIntensity, intensity);
            if (_screenShakeTimer < duration)
            {
                _screenShakeDuration = duration;
                _screenShakeTimer = duration;
            }
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            DrawBackground(context);
            DrawArena(context);
            DrawCoverObjects(context);
            DrawPowerUps(context);
            DrawBullets(context);
            DrawEnemies(context);
            DrawPlayer(context);
            DrawUI(context);

            if (_damageFlashTimer > 0)
                DrawDamageFlash(context);
        }

        private void DrawBackground(DrawingContext context)
        {
            var bgBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 20, 30));
            context.DrawRectangle(bgBrush, null, new Rect(0, 0, 1920, 1080));
        }

        private void DrawArena(DrawingContext context)
        {
            var floorBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 45, 55));
            var wallBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 90));
            var gridBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 100, 100, 120));

            float arenaLeft = _arenaCenter.X - _arenaSize.X / 2;
            float arenaTop = _arenaCenter.Y - _arenaSize.Y / 2;
            context.DrawRectangle(floorBrush, null, new Rect(arenaLeft, arenaTop, _arenaSize.X, _arenaSize.Y));

            for (float x = arenaLeft; x < arenaLeft + _arenaSize.X; x += 50)
            {
                context.DrawLine(new Pen(gridBrush, 1), new Point(x, arenaTop), new Point(x, arenaTop + _arenaSize.Y));
            }
            for (float y = arenaTop; y < arenaTop + _arenaSize.Y; y += 50)
            {
                context.DrawLine(new Pen(gridBrush, 1), new Point(arenaLeft, y), new Point(arenaLeft + _arenaSize.X, y));
            }

            foreach (var wall in _walls)
            {
                var pen = new Pen(wallBrush, 2);
                context.DrawRectangle(wallBrush, pen, new Rect(
                    wall.Position.X - wall.Size.X / 2,
                    wall.Position.Y - wall.Size.Y / 2,
                    wall.Size.X,
                    wall.Size.Y
                ));
            }
        }

        private void DrawCoverObjects(DrawingContext context)
        {
            var coverBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 65, 75));
            var coverBorderBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 100, 110));

            foreach (var cover in _coverObjects)
            {
                if (cover.IsDestroyed) continue;

                context.DrawRectangle(coverBrush, new Pen(coverBorderBrush, 2), new Rect(
                    cover.Position.X - cover.Size.X / 2,
                    cover.Position.Y - cover.Size.Y / 2,
                    cover.Size.X,
                    cover.Size.Y
                ));
            }
        }

        private void DrawBullets(DrawingContext context)
        {
            foreach (var bullet in _bullets)
            {
                var bulletBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    bullet.Color.GetRByte(),
                    bullet.Color.GetGByte(),
                    bullet.Color.GetBByte()
                ));

                if (bullet.DamageType == BulletDamageType.Explosive)
                {
                    context.DrawEllipse(bulletBrush, null, new Point(bullet.Position.X, bullet.Position.Y), bullet.Radius * 2, bullet.Radius * 2);
                }
                else
                {
                    Vector2 dir = bullet.Velocity.Normalized();
                    double angle = Math.Atan2(dir.Y, dir.X);
                    context.PushTransform(new RotateTransform(angle * 180 / Math.PI, bullet.Position.X, bullet.Position.Y));
                    context.DrawEllipse(bulletBrush, null, new Point(bullet.Position.X, bullet.Position.Y), bullet.Radius * 2, bullet.Radius);
                    context.Pop();
                }
            }
        }

        private void DrawEnemies(DrawingContext context)
        {
            foreach (var enemy in _enemies)
            {
                Color enemyColor = enemy.IsBoss ? Color.Purple :
                                   enemy.Type switch
                                   {
                                       TDSEnemyType.Grunt => Color.Red,
                                       TDSEnemyType.Shooter => Color.Orange,
                                       TDSEnemyType.Runner => Color.Green,
                                       TDSEnemyType.Tank => Color.Brown,
                                       TDSEnemyType.Swarm => Color.Yellow,
                                       _ => Color.Gray
                                   };

                var enemyBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    enemyColor.GetRByte(),
                    enemyColor.GetGByte(),
                    enemyColor.GetBByte()
                ));

                float size = enemy.Radius * 2;
                if (enemy.IsBoss)
                    size *= 1.5f;

                context.DrawEllipse(enemyBrush, new Pen(Brushes.DarkGray, 2), new Point(enemy.Position.X, enemy.Position.Y), size, size);

                if (enemy.Type == TDSEnemyType.Shooter)
                {
                    Vector2 dir = (enemy.Position - _player.Position).Normalized();
                    double angle = Math.Atan2(dir.Y, dir.X);
                    context.PushTransform(new RotateTransform(angle * 180 / Math.PI, enemy.Position.X, enemy.Position.Y));
                    context.DrawRectangle(Brushes.DarkGray, null, new Rect(enemy.Position.X, enemy.Position.Y - 4, enemy.Radius + 10, 8));
                    context.Pop();
                }

                float healthPercent = enemy.Health / enemy.MaxHealth;
                float healthBarWidth = enemy.Radius * 2;
                float healthBarHeight = 4;
                float healthBarY = enemy.Position.Y - enemy.Radius - 10;

                context.DrawRectangle(Brushes.DarkGray, null, new Rect(enemy.Position.X - healthBarWidth / 2, healthBarY, healthBarWidth, healthBarHeight));
                context.DrawRectangle(Brushes.Lime, null, new Rect(enemy.Position.X - healthBarWidth / 2, healthBarY, healthBarWidth * healthPercent, healthBarHeight));
            }
        }

        private void DrawPlayer(DrawingContext context)
        {
            if (_player == null) return;

            byte alpha = _player.IsInvincible ? (byte)((Math.Sin(_player.InvincibilityTimer * 15) > 0 ? 255 : 100)) : (byte)255;

            var playerBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, 50, 150, 255));
            var shieldBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(alpha * 0.5f), 100, 200, 255));

            float size = _player.Radius * 2;
            context.DrawEllipse(playerBrush, new Pen(Brushes.White, 2), new Point(_player.Position.X, _player.Position.Y), size, size);

            if (_player.Shield > 0)
            {
                float shieldPercent = _player.Shield / _player.MaxShield;
                context.DrawEllipse(null, new Pen(shieldBrush, 2), new Point(_player.Position.X, _player.Position.Y), size + 5, size + 5);
            }

            double aimAngle = _player.AimAngle;
            Vector2 gunDir = new Vector2((float)Math.Cos(aimAngle), (float)Math.Sin(aimAngle));
            Vector2 gunTip = _player.Position + gunDir * (_player.Radius + 15);
            context.DrawLine(new Pen(Brushes.DarkGray, 6), new Point(_player.Position.X, _player.Position.Y), new Point(gunTip.X, gunTip.Y));
        }

        private void DrawPowerUps(DrawingContext context)
        {
            foreach (var powerUp in _powerUps)
            {
                if (powerUp.IsCollected) continue;

                Color powerUpColor = powerUp.Type switch
                {
                    TDSPowerUpType.Health => Color.Red,
                    TDSPowerUpType.Shield => Color.Blue,
                    TDSPowerUpType.Ammo => Color.Yellow,
                    TDSPowerUpType.Speed => Color.Green,
                    TDSPowerUpType.Damage => Color.Orange,
                    TDSPowerUpType.WeaponUpgrade => Color.Purple,
                    _ => Color.White
                };

                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    powerUpColor.GetRByte(),
                    powerUpColor.GetGByte(),
                    powerUpColor.GetBByte()
                ));

                float pulse = (float)(Math.Sin(_player.Position.X * 0.1 + _totalTime * 5) * 0.2 + 1);
                float size = powerUp.Radius * 2 * pulse;

                context.DrawEllipse(brush, new Pen(Brushes.White, 2), new Point(powerUp.Position.X, powerUp.Position.Y), size, size);
            }
        }

        private void DrawUI(DrawingContext context)
        {
            if (_gameState == TDSGameState.Menu)
            {
                RenderMainMenu(context);
                return;
            }

            RenderHUD(context);

            if (_gameState == TDSGameState.Paused)
                RenderPauseMenu(context);

            if (_gameState == TDSGameState.GameOver)
                RenderGameOverScreen(context);

            if (_gameState == TDSGameState.WaveComplete)
                RenderWaveComplete(context);

            if (_waveAnnouncementTimer > 0)
                RenderWaveAnnouncement(context);

            RenderMinimap(context);
        }

        private void RenderMainMenu(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 15, 25)), null, new Rect(0, 0, 1920, 1080));

            var titleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 100, 100));
            var textBrush = new SolidColorBrush(Colors.White);
            var subtitleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 170));

            var title = new FormattedText("TOP-DOWN SHOOTER", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 72, titleBrush, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 300));

            var startText = new FormattedText("Press ENTER to Start", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 32, textBrush, 1.0);
            context.DrawText(startText, new Point(960 - startText.Width / 2, 450));

            string controls = "WASD: Move | Mouse: Aim | Left Click: Shoot | R: Reload | 1-4: Switch Weapon | F: Auto-fire";
            var controlsText = new FormattedText(controls, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 18, subtitleBrush, 1.0);
            context.DrawText(controlsText, new Point(960 - controlsText.Width / 2, 550));

            string weapons = "Weapons: Pistol | Shotgun | Machine Gun | Rocket Launcher";
            var weaponsText = new FormattedText(weapons, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 16, subtitleBrush, 1.0);
            context.DrawText(weaponsText, new Point(960 - weaponsText.Width / 2, 600));
        }

        private void RenderHUD(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 30, 30, 40));
            var textBrush = new SolidColorBrush(Colors.White);

            context.DrawRectangle(panelBrush, null, new Rect(20, 20, 280, 180));

            var healthLabel = new FormattedText("HEALTH", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, textBrush, 1.0);
            context.DrawText(healthLabel, new Point(30, 25));

            float healthPercent = _player.Health / _player.MaxHealth;
            context.DrawRectangle(Brushes.DarkGray, null, new Rect(30, 42, 200, 20));
            context.DrawRectangle(Brushes.Red, null, new Rect(30, 42, 200 * healthPercent, 20));
            var healthText = new FormattedText($"{_player.Health:F0}/{_player.MaxHealth}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, textBrush, 1.0);
            context.DrawText(healthText, new Point(30, 45));

            var shieldLabel = new FormattedText("SHIELD", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, textBrush, 1.0);
            context.DrawText(shieldLabel, new Point(30, 68));

            float shieldPercent = _player.Shield / _player.MaxShield;
            context.DrawRectangle(Brushes.DarkGray, null, new Rect(30, 85, 200, 15));
            context.DrawRectangle(Brushes.DodgerBlue, null, new Rect(30, 85, 200 * shieldPercent, 15));

            var ammoLabel = new FormattedText($"AMMO: {_player.CurrentAmmo} / {_player.MaxAmmo}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, textBrush, 1.0);
            context.DrawText(ammoLabel, new Point(30, 108));

            if (_player.IsReloading)
            {
                var reloadText = new FormattedText("RELOADING...", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Yellow, 1.0);
                context.DrawText(reloadText, new Point(30, 128));
            }

            var weaponText = new FormattedText($"[{_player.CurrentWeapon}]", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.Orange, 1.0);
            context.DrawText(weaponText, new Point(30, 150));

            if (_autoFireEnabled)
            {
                var autoFireText = new FormattedText("AUTO-FIRE: ON", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Lime, 1.0);
                context.DrawText(autoFireText, new Point(180, 150));
            }

            context.DrawRectangle(panelBrush, null, new Rect(1620, 20, 280, 80));
            var scoreLabel = new FormattedText("SCORE", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, textBrush, 1.0);
            context.DrawText(scoreLabel, new Point(1630, 25));
            var scoreText = new FormattedText($"{_score}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 28, textBrush, 1.0);
            context.DrawText(scoreText, new Point(1630, 42));

            var waveText = new FormattedText($"WAVE {_currentWave}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 20, Brushes.Orange, 1.0);
            context.DrawText(waveText, new Point(1780, 40));

            context.DrawRectangle(panelBrush, null, new Rect(20, 980, 150, 60));
            var livesText = new FormattedText($"LIVES: {_lives}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 16, textBrush, 1.0);
            context.DrawText(livesText, new Point(30, 1000));

            var enemiesText = new FormattedText($"ENEMIES: {_enemies.Count}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, textBrush, 1.0);
            context.DrawText(enemiesText, new Point(30, 1025));

            RenderWeaponSelector(context);
        }

        private void RenderWeaponSelector(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 30, 30, 40));
            context.DrawRectangle(panelBrush, null, new Rect(1620, 980, 280, 80));

            float xOffset = 1640;
            foreach (TDSWeaponType weapon in Enum.GetValues<TDSWeaponType>())
            {
                bool isSelected = _player.CurrentWeapon == weapon;
                var bgBrush = isSelected ? new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 60, 60, 80)) : new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 40, 40, 50));
                var borderBrush = isSelected ? Brushes.Orange : Brushes.Gray;

                context.DrawRectangle(bgBrush, new Pen(borderBrush, isSelected ? 2 : 1), new Rect(xOffset, 990, 60, 60));

                string weaponIcon = weapon switch
                {
                    TDSWeaponType.Pistol => "P",
                    TDSWeaponType.Shotgun => "S",
                    TDSWeaponType.MachineGun => "M",
                    TDSWeaponType.Rocket => "R",
                    _ => "?"
                };

                var iconText = new FormattedText(weaponIcon, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 24, isSelected ? Brushes.Orange : Brushes.White, 1.0);
                context.DrawText(iconText, new Point(xOffset + 22, 1005));

                int keyNum = (int)weapon + 1;
                var keyText = new FormattedText($"{keyNum}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, Brushes.Gray, 1.0);
                context.DrawText(keyText, new Point(xOffset + 48, 992));

                xOffset += 65;
            }
        }

        private void RenderMinimap(DrawingContext context)
        {
            float mapSize = 150;
            float mapX = 1750;
            float mapY = 850;

            var mapBg = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 20, 20, 30));
            context.DrawRectangle(mapBg, new Pen(Brushes.Gray, 1), new Rect(mapX, mapY, mapSize, mapSize));

            float scaleX = mapSize / _arenaSize.X;
            float scaleY = mapSize / _arenaSize.Y;

            var playerDot = new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 150, 255));
            float px = mapX + (_player.Position.X - (_arenaCenter.X - _arenaSize.X / 2)) * scaleX;
            float py = mapY + (_player.Position.Y - (_arenaCenter.Y - _arenaSize.Y / 2)) * scaleY;
            context.DrawEllipse(playerDot, null, new Point(px, py), 4, 4);

            var enemyDot = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 80, 80));
            foreach (var enemy in _enemies)
            {
                float ex = mapX + (enemy.Position.X - (_arenaCenter.X - _arenaSize.X / 2)) * scaleX;
                float ey = mapY + (enemy.Position.Y - (_arenaCenter.Y - _arenaSize.Y / 2)) * scaleY;
                float es = enemy.IsBoss ? 5 : 3;
                context.DrawEllipse(enemyDot, null, new Point(ex, ey), es, es);
            }
        }

        private void RenderPauseMenu(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)), null, new Rect(0, 0, 1920, 1080));

            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 50));
            context.DrawRectangle(panelBrush, null, new Rect(760, 384, 400, 312));

            var textBrush = new SolidColorBrush(Colors.White);
            var title = new FormattedText("PAUSED", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 48, textBrush, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 420));

            var resumeText = new FormattedText("Press ESC to Resume", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, textBrush, 1.0);
            context.DrawText(resumeText, new Point(960 - resumeText.Width / 2, 520));

            var quitText = new FormattedText("Press R to Restart", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 20, Brushes.Gray, 1.0);
            context.DrawText(quitText, new Point(960 - quitText.Width / 2, 580));
        }

        private void RenderGameOverScreen(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 20, 0, 0)), null, new Rect(0, 0, 1920, 1080));

            var textBrush = new SolidColorBrush(Colors.White);
            var title = new FormattedText("GAME OVER", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 72, Brushes.Red, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 350));

            var scoreText = new FormattedText($"Final Score: {_score}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 32, textBrush, 1.0);
            context.DrawText(scoreText, new Point(960 - scoreText.Width / 2, 450));

            var waveText = new FormattedText($"Waves Survived: {_currentWave - 1}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, textBrush, 1.0);
            context.DrawText(waveText, new Point(960 - waveText.Width / 2, 500));

            var restartText = new FormattedText("Press R to Restart", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 28, Brushes.LightBlue, 1.0);
            context.DrawText(restartText, new Point(960 - restartText.Width / 2, 600));
        }

        private void RenderWaveComplete(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)), null, new Rect(0, 0, 1920, 1080));

            var title = new FormattedText($"WAVE {_currentWave - 1} COMPLETE!", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 56, Brushes.Lime, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 400));

            var nextWave = new FormattedText($"Next Wave in {Math.Ceiling(_waveTimer)} seconds...", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, Brushes.White, 1.0);
            context.DrawText(nextWave, new Point(960 - nextWave.Width / 2, 480));
        }

        private void RenderWaveAnnouncement(DrawingContext context)
        {
            float alpha = Math.Min(1f, _waveAnnouncementTimer);
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(alpha * 255), 255, 200, 50));

            string announcement = _currentWave % 5 == 0 ? $"BOSS WAVE {_currentWave}!" : $"WAVE {_currentWave}";
            var text = new FormattedText(announcement, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 64, brush, 1.0);
            context.DrawText(text, new Point(960 - text.Width / 2, 100));
        }

        private void DrawDamageFlash(DrawingContext context)
        {
            byte alpha = (byte)(_damageFlashTimer * 10 * 100);
            var flashBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, 255, 0, 0));
            context.DrawRectangle(flashBrush, null, new Rect(0, 0, 1920, 1080));
        }

        public override void Dispose()
        {
            if (_player != null)
                _player.WeaponFired -= OnPlayerWeaponFired;

            _enemies.Clear();
            _bullets.Clear();
            _powerUps.Clear();
            base.Dispose();
        }
    }

    public class Player
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; } = 15f;
        public float MaxHealth { get; set; } = 100f;
        public float Health { get; set; }
        public float MaxShield { get; set; } = 50f;
        public float Shield { get; set; }
        public float Speed { get; set; } = 200f;
        public float AimAngle { get; set; }
        public TDSWeaponType CurrentWeapon { get; private set; } = TDSWeaponType.Pistol;
        public int CurrentAmmo { get; private set; }
        public int MaxAmmo { get; private set; }
        public bool IsReloading { get; private set; }
        public bool IsInvincible { get; private set; }
        public float InvincibilityTimer { get; private set; }
        public float DamageMultiplier { get; private set; } = 1f;
        public float SpeedMultiplier { get; private set; } = 1f;
        public float DamageBoostTimer { get; private set; }
        public float SpeedBoostTimer { get; private set; }

        private Dictionary<TDSWeaponType, WeaponStats> _weapons;
        private float _fireRateTimer;
        private float _reloadTimer;
        private float _invincibilityDuration;

        public event EventHandler<Bullet>? WeaponFired;

        public Player(Vector2 position)
        {
            Position = position;
            Health = MaxHealth;
            Shield = MaxShield;
            InitializeWeapons();
        }

        public void Initialize()
        {
            Health = MaxHealth;
            Shield = MaxShield;
            CurrentWeapon = TDSWeaponType.Pistol;
            var stats = _weapons[CurrentWeapon];
            CurrentAmmo = stats.MagazineSize;
            MaxAmmo = stats.MagazineSize;
            IsReloading = false;
            IsInvincible = false;
            InvincibilityTimer = 0f;
            DamageMultiplier = 1f;
            SpeedMultiplier = 1f;
        }

        private void InitializeWeapons()
        {
            _weapons = new Dictionary<TDSWeaponType, WeaponStats>
            {
                [TDSWeaponType.Pistol] = new WeaponStats { Damage = 20f, FireRate = 0.3f, MagazineSize = 12, ReloadTime = 1.5f, Spread = 0.05f, BulletSpeed = 600f, BulletCount = 1 },
                [TDSWeaponType.Shotgun] = new WeaponStats { Damage = 15f, FireRate = 0.8f, MagazineSize = 6, ReloadTime = 2.5f, Spread = 0.3f, BulletSpeed = 500f, BulletCount = 5 },
                [TDSWeaponType.MachineGun] = new WeaponStats { Damage = 10f, FireRate = 0.08f, MagazineSize = 30, ReloadTime = 2f, Spread = 0.15f, BulletSpeed = 700f, BulletCount = 1 },
                [TDSWeaponType.Rocket] = new WeaponStats { Damage = 80f, FireRate = 1.5f, MagazineSize = 3, ReloadTime = 3f, Spread = 0.1f, BulletSpeed = 400f, BulletCount = 1, IsExplosive = true }
            };
        }

        public void Update(float deltaTime, Vector2 moveInput)
        {
            Vector2 movement = moveInput * Speed * SpeedMultiplier * deltaTime;
            Position = Position + movement;

            if (SpeedBoostTimer > 0)
                SpeedBoostTimer -= deltaTime;
            else
                SpeedMultiplier = 1f;

            if (DamageBoostTimer > 0)
                DamageBoostTimer -= deltaTime;
            else
                DamageMultiplier = 1f;

            if (IsInvincible)
            {
                InvincibilityTimer -= deltaTime;
                if (InvincibilityTimer <= 0)
                    IsInvincible = false;
            }

            if (IsReloading)
            {
                _reloadTimer -= deltaTime;
                if (_reloadTimer <= 0)
                {
                    IsReloading = false;
                    CurrentAmmo = MaxAmmo;
                }
            }

            if (_fireRateTimer > 0)
                _fireRateTimer -= deltaTime;
        }

        public void AimAt(Vector2 targetPosition)
        {
            AimAngle = (float)Math.Atan2(targetPosition.Y - Position.Y, targetPosition.X - Position.X);
        }

        public void Fire()
        {
            if (IsReloading || CurrentAmmo <= 0 || _fireRateTimer > 0)
                return;

            var stats = _weapons[CurrentWeapon];
            _fireRateTimer = stats.FireRate;
            CurrentAmmo--;

            for (int i = 0; i < stats.BulletCount; i++)
            {
                float spreadAngle = AimAngle + (float)(new Random().NextDouble() * 2 - 1) * stats.Spread;
                Vector2 direction = new Vector2((float)Math.Cos(spreadAngle), (float)Math.Sin(spreadAngle));
                Vector2 velocity = direction * stats.BulletSpeed;
                float damage = stats.Damage * DamageMultiplier;

                var bullet = new Bullet(Position + direction * (Radius + 5), velocity, damage, Color.Yellow, stats.BulletLifetime, true,
                    stats.IsExplosive ? BulletDamageType.Explosive : BulletDamageType.Normal, stats.BulletSpeed / 100f);

                WeaponFired?.Invoke(this, bullet);
            }

            if (CurrentAmmo <= 0)
                StartReload();
        }

        public void StartReload()
        {
            if (IsReloading) return;
            var stats = _weapons[CurrentWeapon];
            IsReloading = true;
            _reloadTimer = stats.ReloadTime;
        }

        public void SwitchWeapon(TDSWeaponType weapon)
        {
            if (CurrentWeapon == weapon) return;
            CurrentWeapon = weapon;
            var stats = _weapons[weapon];
            CurrentAmmo = stats.MagazineSize;
            MaxAmmo = stats.MagazineSize;
            IsReloading = false;
        }

        public void UpgradeCurrentWeapon()
        {
            var stats = _weapons[CurrentWeapon];
            stats.Damage *= 1.5f;
            stats.MagazineSize = (int)(stats.MagazineSize * 1.2);
            CurrentAmmo = stats.MagazineSize;
            MaxAmmo = stats.MagazineSize;
        }

        public void TakeDamage(float damage)
        {
            if (IsInvincible) return;

            if (Shield > 0)
            {
                float shieldDamage = Math.Min(Shield, damage);
                Shield -= shieldDamage;
                damage -= shieldDamage;
            }

            Health -= damage;
            IsInvincible = true;
            InvincibilityTimer = 1f;
        }

        public void Respawn(Vector2 spawnPosition)
        {
            Position = spawnPosition;
            IsInvincible = true;
            InvincibilityTimer = 2f;
        }

        public void RefillAllAmmo()
        {
            foreach (var weapon in _weapons.Values)
            {
                weapon.MagazineSize = weapon.MagazineSize;
            }
            CurrentAmmo = MaxAmmo;
        }

        public void ApplySpeedBoost(float multiplier, float duration)
        {
            SpeedMultiplier = multiplier;
            SpeedBoostTimer = duration;
        }

        public void ApplyDamageBoost(float multiplier, float duration)
        {
            DamageMultiplier = multiplier;
            DamageBoostTimer = duration;
        }
    }

    public class WeaponStats
    {
        public float Damage { get; set; }
        public float FireRate { get; set; }
        public int MagazineSize { get; set; }
        public float ReloadTime { get; set; }
        public float Spread { get; set; }
        public float BulletSpeed { get; set; }
        public int BulletCount { get; set; } = 1;
        public bool IsExplosive { get; set; }
    }

    public class Enemy
    {
        public Vector2 Position { get; set; }
        public float Radius { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Speed { get; set; }
        public float MeleeDamage { get; set; }
        public int PointsValue { get; set; }
        public TDSEnemyType Type { get; }
        public bool IsBoss => Type == TDSEnemyType.Boss;

        private float _fireRateTimer;
        private float _aiTimer;
        private Vector2 _velocity;
        private Vector2 _targetPosition;
        private Random _random;

        public Enemy(Vector2 position, TDSEnemyType type)
        {
            Position = position;
            Type = type;
            _random = new Random();
            _velocity = Vector2.Zero;
            _targetPosition = position;
        }

        public void Initialize(int wave)
        {
            float waveMultiplier = 1f + (wave - 1) * 0.15f;

            switch (Type)
            {
                case TDSEnemyType.Grunt:
                    Radius = 12f;
                    Health = 30f * waveMultiplier;
                    MaxHealth = Health;
                    Speed = 80f;
                    MeleeDamage = 15f;
                    PointsValue = 10;
                    break;
                case TDSEnemyType.Shooter:
                    Radius = 14f;
                    Health = 25f * waveMultiplier;
                    MaxHealth = Health;
                    Speed = 50f;
                    MeleeDamage = 5f;
                    PointsValue = 20;
                    break;
                case TDSEnemyType.Runner:
                    Radius = 10f;
                    Health = 15f * waveMultiplier;
                    MaxHealth = Health;
                    Speed = 150f;
                    MeleeDamage = 10f;
                    PointsValue = 15;
                    break;
                case TDSEnemyType.Tank:
                    Radius = 20f;
                    Health = 100f * waveMultiplier;
                    MaxHealth = Health;
                    Speed = 40f;
                    MeleeDamage = 25f;
                    PointsValue = 50;
                    break;
                case TDSEnemyType.Swarm:
                    Radius = 8f;
                    Health = 10f * waveMultiplier;
                    MaxHealth = Health;
                    Speed = 100f;
                    MeleeDamage = 8f;
                    PointsValue = 5;
                    break;
                case TDSEnemyType.Boss:
                    Radius = 30f;
                    Health = 300f * waveMultiplier;
                    MaxHealth = Health;
                    Speed = 60f;
                    MeleeDamage = 35f;
                    PointsValue = 200;
                    break;
            }
        }

        public void Update(float deltaTime, Vector2 playerPosition)
        {
            _fireRateTimer -= deltaTime;
            _aiTimer -= deltaTime;

            switch (Type)
            {
                case TDSEnemyType.Grunt:
                    UpdateGruntAI(deltaTime, playerPosition);
                    break;
                case TDSEnemyType.Shooter:
                    UpdateShooterAI(deltaTime, playerPosition);
                    break;
                case TDSEnemyType.Runner:
                    UpdateRunnerAI(deltaTime, playerPosition);
                    break;
                case TDSEnemyType.Tank:
                    UpdateTankAI(deltaTime, playerPosition);
                    break;
                case TDSEnemyType.Swarm:
                    UpdateSwarmAI(deltaTime, playerPosition);
                    break;
                case TDSEnemyType.Boss:
                    UpdateBossAI(deltaTime, playerPosition);
                    break;
            }

            if (_aiTimer <= 0)
                _aiTimer = 0.05f;

            Position = Position + _velocity * deltaTime;
        }

        private void UpdateGruntAI(float deltaTime, Vector2 playerPosition)
        {
            Vector2 direction = (playerPosition - Position).Normalized();
            _velocity = direction * Speed;
        }

        private void UpdateShooterAI(float deltaTime, Vector2 playerPosition)
        {
            float distanceToPlayer = (playerPosition - Position).Length();
            float preferredDistance = 200f;

            if (distanceToPlayer < preferredDistance - 50f)
            {
                Vector2 away = (Position - playerPosition).Normalized();
                _velocity = away * Speed;
            }
            else if (distanceToPlayer > preferredDistance + 50f)
            {
                Vector2 toward = (playerPosition - Position).Normalized();
                _velocity = toward * Speed * 0.5f;
            }
            else
            {
                _velocity = Vector2.Zero;
            }

            if (distanceToPlayer < 300f && _fireRateTimer <= 0)
            {
                FireAtPlayer(playerPosition);
                _fireRateTimer = 1.5f;
            }
        }

        private void UpdateRunnerAI(float deltaTime, Vector2 playerPosition)
        {
            Vector2 direction = (playerPosition - Position).Normalized();
            float jitter = (float)(_random.NextDouble() * 2 - 1) * 0.3f;
            direction = direction.Rotate(jitter);
            _velocity = direction * Speed;
        }

        private void UpdateTankAI(float deltaTime, Vector2 playerPosition)
        {
            Vector2 direction = (playerPosition - Position).Normalized();
            _velocity = direction * Speed;

            if ((playerPosition - Position).Length() < Radius + 30f && _fireRateTimer <= 0)
            {
                FireAtPlayer(playerPosition);
                FireAtPlayer(playerPosition);
                FireAtPlayer(playerPosition);
                _fireRateTimer = 2f;
            }
        }

        private void UpdateSwarmAI(float deltaTime, Vector2 playerPosition)
        {
            Vector2 direction = (playerPosition - Position).Normalized();
            float jitter = (float)(_random.NextDouble() * 2 - 1) * 0.5f;
            direction = direction.Rotate(jitter);
            _velocity = direction * Speed;
        }

        private void UpdateBossAI(float deltaTime, Vector2 playerPosition)
        {
            float distanceToPlayer = (playerPosition - Position).Length();

            if (_aiTimer <= 0)
            {
                if (distanceToPlayer > 150f)
                {
                    Vector2 toward = (playerPosition - Position).Normalized();
                    _velocity = toward * Speed;
                }
                else
                {
                    Vector2 away = (Position - playerPosition).Normalized();
                    _velocity = away * Speed + new Vector2((float)(_random.NextDouble() * 2 - 1), (float)(_random.NextDouble() * 2 - 1)) * 20f;
                }
            }

            if (distanceToPlayer < 250f && _fireRateTimer <= 0)
            {
                for (int i = 0; i < 3; i++)
                {
                    Vector2 offset = new Vector2((float)(_random.NextDouble() * 100 - 50), (float)(_random.NextDouble() * 100 - 50));
                    FireAtPlayer(playerPosition + offset);
                }
                _fireRateTimer = 0.5f;
            }
        }

        private void FireAtPlayer(Vector2 target)
        {
        }

        public void TakeDamage(float damage)
        {
            Health -= damage;
        }
    }

    public enum BulletDamageType
    {
        Normal,
        Explosive
    }

    public class Bullet
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Damage { get; set; }
        public Color Color { get; set; }
        public float Lifetime { get; set; }
        public float Age { get; set; }
        public bool IsPlayerBullet { get; set; }
        public bool IsExpired { get; set; }
        public float Radius { get; set; } = 4f;
        public BulletDamageType DamageType { get; }

        public Bullet(Vector2 position, Vector2 velocity, float damage, Color color, float lifetime, bool isPlayerBullet, BulletDamageType damageType, float radius = 4f)
        {
            Position = position;
            Velocity = velocity;
            Damage = damage;
            Color = color;
            Lifetime = lifetime;
            Age = 0f;
            IsPlayerBullet = isPlayerBullet;
            IsExpired = false;
            DamageType = damageType;
            Radius = radius;
        }

        public void Update(float deltaTime)
        {
            Position = Position + Velocity * deltaTime;
            Age += deltaTime;
            if (Age >= Lifetime)
                IsExpired = true;
        }
    }

    public class PowerUp
    {
        public Vector2 Position { get; set; }
        public TDSPowerUpType Type { get; }
        public float Radius { get; set; } = 12f;
        public float Lifetime { get; set; } = 15f;
        public float Age { get; set; }
        public bool IsCollected { get; set; }
        public bool IsExpired => Age >= Lifetime;

        public PowerUp(Vector2 position, TDSPowerUpType type)
        {
            Position = position;
            Type = type;
        }

        public void Update(float deltaTime)
        {
            Age += deltaTime;
        }
    }

    public class Wall
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }

        public Wall(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
        }
    }

    public class CoverObject
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public bool IsDestroyed { get; set; }

        public CoverObject(Vector2 position, Vector2 size)
        {
            Position = position;
            Size = size;
            IsDestroyed = false;
        }
    }

    public class SpawnZone
    {
        public Vector2 Position { get; set; }
        public Vector2 Size { get; set; }
        public int Index { get; }

        public SpawnZone(Vector2 position, Vector2 size, int index)
        {
            Position = position;
            Size = size;
            Index = index;
        }

        public Vector2 GetRandomPosition(Random random)
        {
            float x = Position.X + (float)(random.NextDouble() * Size.X - Size.X / 2);
            float y = Position.Y + (float)(random.NextDouble() * Size.Y - Size.Y / 2);
            return new Vector2(x, y);
        }
    }

    public class Camera2D
    {
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Zoom { get; set; } = 1f;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WPFEngine.Core;
using WPFEngine.ECS;
using WPFEngine.Scene;
using WPFEngine.Math;
using WPFEngine.Input;
using WPFEngine.Audio;
using WPFEngine.Particles;
using WPFEngine.UI;
using WPFEngine.Pooling;

namespace WPFEngine.Demos
{
    public enum PlatformerGameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        LevelComplete
    }

    public enum PlayerAnimationState
    {
        Idle,
        Run,
        Jump,
        Fall,
        WallSlide,
        Hurt,
        Death
    }

    public enum EnemyState
    {
        Patrol,
        Chase,
        Attack,
        Return
    }

    public class PlatformerGame : GameBase
    {
        private PlatformerGameState _gameState;
        private PlatformerGameState _previousState;
        private GameObject _player;
        private PlayerController _playerController;
        private Camera2D _gameCamera;
        private LevelData _currentLevel;
        private int _currentLevelIndex;
        private List<LevelData> _levels;
        private Dictionary<string, GameObject> _gameObjects;
        private ParticleEmitter _dustEmitter;
        private ParticleEmitter _coinEmitter;
        private ParticleEmitter _deathEmitter;
        private ParticleEmitter _jumpEmitter;
        private PlatformerUI _ui;
        private PlatformerSettings _settings;
        private float _levelTransitionTimer;
        private bool _isTransitioning;
        private Vector2 _spawnPoint;
        private float _cameraShakeIntensity;
        private float _cameraShakeDuration;
        private float _cameraShakeTimer;
        private float _invincibilityTimer;
        private int _totalCoins;
        private int _totalScore;

        public PlatformerGameState GameState => _gameState;
        public int CurrentLevelIndex => _currentLevelIndex;
        public PlatformerSettings Settings => _settings;
        public int TotalCoins => _totalCoins;
        public int TotalScore => _totalScore;

        public event EventHandler<PlatformerGameState>? GameStateChanged;
        public event EventHandler<int>? CoinCollected;
        public event EventHandler<int>? HealthChanged;
        public event EventHandler? LevelCompleted;
        public event EventHandler? PlayerDied;

        public PlatformerGame()
        {
            _gameState = PlatformerGameState.Menu;
            _previousState = PlatformerGameState.Menu;
            _gameObjects = new Dictionary<string, GameObject>();
            _currentLevelIndex = 0;
            _levels = new List<LevelData>();
            _isTransitioning = false;
            _cameraShakeIntensity = 0f;
            _cameraShakeDuration = 0f;
            _cameraShakeTimer = 0f;
            _invincibilityTimer = 0f;
            _totalCoins = 0;
            _totalScore = 0;
            _settings = new PlatformerSettings();

            InitializeSettings();
        }

        private void InitializeSettings()
        {
            _settings.MasterVolume = 1.0f;
            _settings.MusicVolume = 0.7f;
            _settings.SfxVolume = 1.0f;
            _settings.Difficulty = Difficulty.Normal;
            _settings.ShowDebugInfo = false;
            _settings.InfiniteLives = false;
            _settings.StartingLives = 3;
            _settings.StartingHealth = 100f;
            _settings.AllowDoubleJump = true;
            _settings.AllowWallJump = true;
            _settings.AllowWallSlide = true;
            _settings.GravityMultiplier = 1.0f;
            _settings.PlayerSpeed = 8.0f;
            _settings.JumpForce = 12.0f;
            _settings.WallSlideSpeed = 2.0f;
        }

        protected override void Initialize()
        {
            base.Initialize();

            InitializeAudio();
            InitializeParticleSystems();
            InitializeUI();
            CreateDefaultLevels();

            _gameState = PlatformerGameState.Menu;
            GameStateChanged?.Invoke(this, _gameState);
        }

        private void InitializeAudio()
        {
            AudioManager.Instance.Initialize();
            AudioManager.Instance.MasterVolume = _settings.MasterVolume;
            AudioManager.Instance.MusicVolume = _settings.MusicVolume;
            AudioManager.Instance.SfxVolume = _settings.SfxVolume;
        }

        private void InitializeParticleSystems()
        {
            _dustEmitter = CreateDustEmitter();
            _coinEmitter = CreateCoinEmitter();
            _deathEmitter = CreateDeathEmitter();
            _jumpEmitter = CreateJumpEmitter();
        }

        private ParticleEmitter CreateDustEmitter()
        {
            var emitter = new ParticleEmitter
            {
                MaxParticles = 50,
                EmissionRate = 0,
                ParticleLifetime = 0.5f,
                StartSize = 0.1f,
                StartSizeVariation = 0.05f,
                EmissionSpeed = 2f,
                EmissionSpeedVariation = 1f,
                EmissionAngle = new Vector2(-60, 60),
                GravityModifier = 0.5f,
                IsEmitting = false
            };
            return emitter;
        }

        private ParticleEmitter CreateCoinEmitter()
        {
            var emitter = new ParticleEmitter
            {
                MaxParticles = 20,
                EmissionRate = 0,
                ParticleLifetime = 0.8f,
                StartSize = 0.15f,
                StartSizeVariation = 0.05f,
                EmissionSpeed = 3f,
                EmissionSpeedVariation = 2f,
                EmissionAngle = new Vector2(0, 360),
                GravityModifier = 0.2f,
                IsEmitting = false
            };
            return emitter;
        }

        private ParticleEmitter CreateDeathEmitter()
        {
            var emitter = new ParticleEmitter
            {
                MaxParticles = 30,
                EmissionRate = 0,
                ParticleLifetime = 1.0f,
                StartSize = 0.2f,
                StartSizeVariation = 0.1f,
                EmissionSpeed = 5f,
                EmissionSpeedVariation = 3f,
                EmissionAngle = new Vector2(0, 360),
                GravityModifier = 0.8f,
                IsEmitting = false
            };
            return emitter;
        }

        private ParticleEmitter CreateJumpEmitter()
        {
            var emitter = new ParticleEmitter
            {
                MaxParticles = 15,
                EmissionRate = 0,
                ParticleLifetime = 0.3f,
                StartSize = 0.1f,
                StartSizeVariation = 0.03f,
                EmissionSpeed = 2f,
                EmissionSpeedVariation = 1f,
                EmissionAngle = new Vector2(-120, -60),
                GravityModifier = 0.3f,
                IsEmitting = false
            };
            return emitter;
        }

        private void InitializeUI()
        {
            _ui = new PlatformerUI();
            _ui.Initialize(this);
        }

        private void CreateDefaultLevels()
        {
            _levels.Add(CreateTutorialLevel());
            _levels.Add(CreateForestLevel());
            _levels.Add(CreateCaveLevel());
        }

        private LevelData CreateTutorialLevel()
        {
            var level = new LevelData
            {
                Name = "Tutorial",
                Width = 100,
                Height = 20,
                BackgroundColor = new Color(0.5f, 0.7f, 1.0f),
                MusicTrack = "tutorial_music"
            };

            level.Platforms.Add(new PlatformData { X = 0, Y = 0, Width = 100, Height = 1, PlatformType = PlatformType.Ground });
            level.Platforms.Add(new PlatformData { X = 15, Y = 3, Width = 5, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 25, Y = 5, Width = 5, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 35, Y = 7, Width = 5, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 50, Y = 5, Width = 8, Height = 0.5f, PlatformType = PlatformType.Moving, MoveDistance = 3, MoveSpeed = 2 });

            level.Coins.Add(new Vector2(17, 2f));
            level.Coins.Add(new Vector2(27, 4f));
            level.Coins.Add(new Vector2(37, 6f));
            level.Coins.Add(new Vector2(54, 4f));

            level.Hazards.Add(new HazardData { X = 40, Y = 0, Width = 2, Height = 0.5f, HazardType = HazardType.Spikes });

            level.Checkpoints.Add(new CheckpointData { X = 30, Y = 0, IsActive = true });
            level.Checkpoints.Add(new CheckpointData { X = 60, Y = 0, IsActive = false });

            level.ExitDoor = new ExitData { X = 85, Y = 0 };
            level.PlayerSpawn = new Vector2(2, -1);

            level.Enemies.Add(new EnemySpawnData { X = 20, Y = 0, EnemyType = EnemyType.Patrol, PatrolDistance = 5 });
            level.Enemies.Add(new EnemySpawnData { X = 55, Y = 0, EnemyType = EnemyType.Patrol, PatrolDistance = 8 });

            return level;
        }

        private LevelData CreateForestLevel()
        {
            var level = new LevelData
            {
                Name = "Forest",
                Width = 150,
                Height = 25,
                BackgroundColor = new Color(0.2f, 0.5f, 0.3f),
                MusicTrack = "forest_music"
            };

            for (int i = 0; i < 150; i += 3)
            {
                level.Platforms.Add(new PlatformData { X = i, Y = 0, Width = 3, Height = 1, PlatformType = PlatformType.Ground });
            }

            level.Platforms.Add(new PlatformData { X = 20, Y = 5, Width = 6, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 35, Y = 8, Width = 4, Height = 0.5f, PlatformType = PlatformType.Moving, MoveDistance = 4, MoveSpeed = 3 });
            level.Platforms.Add(new PlatformData { X = 50, Y = 10, Width = 5, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 65, Y = 6, Width = 4, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 80, Y = 12, Width = 6, Height = 0.5f, PlatformType = PlatformType.Moving, MoveDistance = 5, MoveSpeed = 2.5f });
            level.Platforms.Add(new PlatformData { X = 100, Y = 8, Width = 8, Height = 0.5f, PlatformType = PlatformType.Static });

            for (int i = 0; i < 25; i++)
            {
                float x = 10 + i * 5;
                float y = 2 + (float)Math.Sin(i * 0.5) * 1.5f;
                level.Coins.Add(new Vector2(x, y));
            }

            level.Hazards.Add(new HazardData { X = 28, Y = 0, Width = 3, Height = 0.5f, HazardType = HazardType.Spikes });
            level.Hazards.Add(new HazardData { X = 45, Y = 0, Width = 2, Height = 0.5f, HazardType = HazardType.Spikes });
            level.Hazards.Add(new HazardData { X = 75, Y = 0, Width = 4, Height = 0.5f, HazardType = HazardType.Spikes });
            level.Hazards.Add(new HazardData { X = 110, Y = 0, Width = 3, Height = 0.5f, HazardType = HazardType.Spikes });

            level.Checkpoints.Add(new CheckpointData { X = 40, Y = 0, IsActive = true });
            level.Checkpoints.Add(new CheckpointData { X = 80, Y = 0, IsActive = false });

            level.ExitDoor = new ExitData { X = 140, Y = 0 };
            level.PlayerSpawn = new Vector2(2, -1);

            level.Enemies.Add(new EnemySpawnData { X = 25, Y = 0, EnemyType = EnemyType.Patrol, PatrolDistance = 8 });
            level.Enemies.Add(new EnemySpawnData { X = 60, Y = 0, EnemyType = EnemyType.Chaser, ChaseRange = 10 });
            level.Enemies.Add(new EnemySpawnData { X = 90, Y = 0, EnemyType = EnemyType.Patrol, PatrolDistance = 6 });
            level.Enemies.Add(new EnemySpawnData { X = 120, Y = 0, EnemyType = EnemyType.Chaser, ChaseRange = 12 });

            return level;
        }

        private LevelData CreateCaveLevel()
        {
            var level = new LevelData
            {
                Name = "Cave",
                Width = 200,
                Height = 30,
                BackgroundColor = new Color(0.1f, 0.1f, 0.15f),
                MusicTrack = "cave_music"
            };

            for (int i = 0; i < 200; i += 2)
            {
                level.Platforms.Add(new PlatformData { X = i, Y = 0, Width = 2.5f, Height = 1, PlatformType = PlatformType.Ground });
            }

            level.Platforms.Add(new PlatformData { X = 15, Y = 4, Width = 4, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 25, Y = 7, Width = 3, Height = 0.5f, PlatformType = PlatformType.Moving, MoveDistance = 3, MoveSpeed = 2 });
            level.Platforms.Add(new PlatformData { X = 40, Y = 10, Width = 5, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 55, Y = 6, Width = 4, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 70, Y = 12, Width = 6, Height = 0.5f, PlatformType = PlatformType.Moving, MoveDistance = 5, MoveSpeed = 3 });
            level.Platforms.Add(new PlatformData { X = 90, Y = 8, Width = 4, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 105, Y = 14, Width = 5, Height = 0.5f, PlatformType = PlatformType.Moving, MoveDistance = 4, MoveSpeed = 2.5f });
            level.Platforms.Add(new PlatformData { X = 125, Y = 10, Width = 6, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 145, Y = 7, Width = 5, Height = 0.5f, PlatformType = PlatformType.Static });
            level.Platforms.Add(new PlatformData { X = 160, Y = 12, Width = 8, Height = 0.5f, PlatformType = PlatformType.Static });

            for (int i = 0; i < 40; i++)
            {
                float x = 10 + i * 4;
                float y = 3 + (float)Math.Cos(i * 0.3) * 2f;
                level.Coins.Add(new Vector2(x, y));
            }

            for (int i = 0; i < 8; i++)
            {
                level.Hazards.Add(new HazardData { X = 20 + i * 20, Y = 0, Width = 3, Height = 0.5f, HazardType = HazardType.Spikes });
            }

            level.Checkpoints.Add(new CheckpointData { X = 50, Y = 0, IsActive = true });
            level.Checkpoints.Add(new CheckpointData { X = 100, Y = 0, IsActive = false });
            level.Checkpoints.Add(new CheckpointData { X = 150, Y = 0, IsActive = false });

            level.ExitDoor = new ExitData { X = 190, Y = 0 };
            level.PlayerSpawn = new Vector2(2, -1);

            level.Enemies.Add(new EnemySpawnData { X = 30, Y = 0, EnemyType = EnemyType.Patrol, PatrolDistance = 10 });
            level.Enemies.Add(new EnemySpawnData { X = 60, Y = 0, EnemyType = EnemyType.Chaser, ChaseRange = 15 });
            level.Enemies.Add(new EnemySpawnData { X = 80, Y = 0, EnemyType = EnemyType.Patrol, PatrolDistance = 8 });
            level.Enemies.Add(new EnemySpawnData { X = 110, Y = 0, EnemyType = EnemyType.Chaser, ChaseRange = 12 });
            level.Enemies.Add(new EnemySpawnData { X = 140, Y = 0, EnemyType = EnemyType.Patrol, PatrolDistance = 10 });
            level.Enemies.Add(new EnemySpawnData { X = 175, Y = 0, EnemyType = EnemyType.Chaser, ChaseRange = 15 });

            return level;
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            UpdateInput();
            UpdateGameState(deltaTime);
            UpdateCamera(deltaTime);
            UpdateParticles(deltaTime);

            if (_gameState == PlatformerGameState.Playing)
            {
                UpdateLevel(deltaTime);
                UpdatePlayer(deltaTime);
                UpdateEnemies(deltaTime);
                UpdateMovingPlatforms(deltaTime);
            }

            _ui.Update(deltaTime);
        }

        private void UpdateInput()
        {
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Escape))
            {
                if (_gameState == PlatformerGameState.Playing)
                {
                    SetGameState(PlatformerGameState.Paused);
                }
                else if (_gameState == PlatformerGameState.Paused)
                {
                    SetGameState(PlatformerGameState.Playing);
                }
            }

            if (_gameState == PlatformerGameState.Menu && InputManager.Instance.GetKeyDown(InputManager.KeyCode.Enter))
            {
                StartGame();
            }

            if (_gameState == PlatformerGameState.GameOver && InputManager.Instance.GetKeyDown(InputManager.KeyCode.R))
            {
                RestartLevel();
            }

            if (_gameState == PlatformerGameState.LevelComplete && InputManager.Instance.GetKeyDown(InputManager.KeyCode.Enter))
            {
                NextLevel();
            }
        }

        private void UpdateGameState(float deltaTime)
        {
            if (_isTransitioning)
            {
                _levelTransitionTimer -= deltaTime;
                if (_levelTransitionTimer <= 0)
                {
                    _isTransitioning = false;
                }
            }

            if (_cameraShakeTimer > 0)
            {
                _cameraShakeTimer -= deltaTime;
            }

            if (_invincibilityTimer > 0)
            {
                _invincibilityTimer -= deltaTime;
            }
        }

        private void UpdateCamera(float deltaTime)
        {
            if (_player == null || _gameCamera == null)
                return;

            Vector2 targetPosition = _player.Transform.Position;
            targetPosition.X = Math.Clamp(targetPosition.X, 5, _currentLevel.Width - 5);
            targetPosition.Y = Math.Clamp(targetPosition.Y + 2, 2, _currentLevel.Height - 2);

            Vector2 cameraOffset = new Vector2(0, 3);
            Vector2 desiredPosition = targetPosition + cameraOffset;

            float smoothSpeed = 5f * deltaTime;
            _gameCamera.Position = Vector2.Lerp(_gameCamera.Position, desiredPosition, smoothSpeed);

            if (_cameraShakeTimer > 0)
            {
                float shake = _cameraShakeIntensity * (_cameraShakeTimer / _cameraShakeDuration);
                _gameCamera.Position += new Vector2(
                    (float)(Math.Random.NextDouble() * 2 - 1) * shake,
                    (float)(Math.Random.NextDouble() * 2 - 1) * shake
                );
            }
        }

        private void UpdateParticles(float deltaTime)
        {
            _dustEmitter.Update(deltaTime);
            _coinEmitter.Update(deltaTime);
            _deathEmitter.Update(deltaTime);
            _jumpEmitter.Update(deltaTime);
        }

        private void UpdateLevel(float deltaTime)
        {
            if (_currentLevel == null)
                return;

            foreach (var coin in _currentLevel.Coins.ToList())
            {
                if (coin.IsCollected)
                    continue;

                float distance = (_player.Transform.Position - coin.Position).Magnitude;
                if (distance < 1.0f)
                {
                    CollectCoin(coin);
                }
            }

            foreach (var hazard in _currentLevel.Hazards)
            {
                if (IsCollidingWithHazard(hazard))
                {
                    TakeDamage(25f);
                    KnockbackPlayer(hazard);
                }
            }

            foreach (var checkpoint in _currentLevel.Checkpoints)
            {
                float distance = (_player.Transform.Position - checkpoint.Position).Magnitude;
                if (distance < 1.5f && !checkpoint.IsActivated)
                {
                    ActivateCheckpoint(checkpoint);
                }
            }

            if (_currentLevel.ExitDoor != null)
            {
                float distance = (_player.Transform.Position - _currentLevel.ExitDoor.Position).Magnitude;
                if (distance < 1.5f)
                {
                    CompleteLevel();
                }
            }
        }

        private void UpdatePlayer(float deltaTime)
        {
            if (_playerController == null)
                return;

            _playerController.Update(deltaTime);

            if (_playerController.IsGrounded && _playerController.Velocity.X != 0)
            {
                float speed = Math.Abs(_playerController.Velocity.X);
                if (speed > 2f && _dustEmitter != null)
                {
                    SpawnDustParticles(_player.Transform.Position + new Vector2(0, -0.5f));
                }
            }
        }

        private void UpdateEnemies(float deltaTime)
        {
            if (_currentLevel == null)
                return;

            foreach (var enemyData in _currentLevel.Enemies)
            {
                if (enemyData.GameObject == null)
                    continue;

                var enemyController = enemyData.GameObject.GetComponent<EnemyController>();
                if (enemyController != null)
                {
                    enemyController.Update(deltaTime);

                    if (_player != null && _invincibilityTimer <= 0)
                    {
                        float distance = (_player.Transform.Position - enemyData.GameObject.Transform.Position).Magnitude;
                        if (distance < 1.0f)
                        {
                            TakeDamage(20f);
                            KnockbackPlayerFromEnemy(enemyData.GameObject.Transform.Position);
                        }
                    }
                }
            }
        }

        private void UpdateMovingPlatforms(float deltaTime)
        {
            if (_currentLevel == null)
                return;

            foreach (var platform in _currentLevel.Platforms)
            {
                if (platform.PlatformType != PlatformType.Moving)
                    continue;

                platform.Update(deltaTime);

                if (_playerController != null && _playerController.IsGrounded)
                {
                    Vector2 playerPos = _player.Transform.Position;
                    Vector2 platformTop = platform.CurrentPosition + new Vector2(0, platform.Height / 2);

                    if (playerPos.X >= platform.CurrentPosition.X - platform.Width / 2 &&
                        playerPos.X <= platform.CurrentPosition.X + platform.Width / 2 &&
                        playerPos.Y >= platformTop.Y - 0.5f &&
                        playerPos.Y <= platformTop.Y + 0.3f)
                    {
                        _playerController.MoveWithPlatform(platform.Velocity * deltaTime);
                    }
                }
            }
        }

        public void StartGame()
        {
            _totalScore = 0;
            _totalCoins = 0;
            _currentLevelIndex = 0;
            LoadLevel(_currentLevelIndex);
            SetGameState(PlatformerGameState.Playing);
        }

        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _levels.Count)
                return;

            ClearLevel();
            _currentLevel = _levels[levelIndex];
            _currentLevelIndex = levelIndex;

            CreateLevelGeometry();
            CreatePlayer();
            CreateEnemies();
            CreateCollectibles();

            _spawnPoint = _currentLevel.PlayerSpawn;
            RespawnPlayer();

            _gameCamera = new Camera2D
            {
                Position = _spawnPoint + new Vector2(0, 3),
                OrthographicSize = 10f
            };

            _ui.ShowHUD();
        }

        private void ClearLevel()
        {
            _gameObjects.Clear();

            if (_currentLevel != null)
            {
                foreach (var enemy in _currentLevel.Enemies)
                {
                    enemy.GameObject?.Destroy();
                }
            }

            _currentLevel = null;
        }

        private void CreateLevelGeometry()
        {
            if (_currentLevel == null)
                return;

            foreach (var platform in _currentLevel.Platforms)
            {
                CreatePlatform(platform);
            }

            foreach (var hazard in _currentLevel.Hazards)
            {
                CreateHazard(hazard);
            }

            if (_currentLevel.ExitDoor != null)
            {
                CreateExitDoor(_currentLevel.ExitDoor);
            }
        }

        private GameObject CreatePlatform(PlatformData data)
        {
            var platform = new GameObject($"Platform_{_gameObjects.Count}");
            platform.Transform.Position = data.Width > 2 ? new Vector2(data.X + data.Width / 2, data.Y + data.Height / 2) : new Vector2(data.X, data.Y);
            platform.Transform.Scale = new Vector2(data.Width, data.Height);

            var renderer = platform.AddComponent<SpriteRenderer>();
            renderer.Color = data.PlatformType == PlatformType.Ground ? new Color(0.4f, 0.6f, 0.3f) : new Color(0.6f, 0.4f, 0.2f);

            var rigidBody = platform.AddComponent<Physics.RigidBody2D>();
            rigidBody.BodyType = Physics.RigidBodyType.Static;
            rigidBody.Friction = 0.8f;

            var collider = platform.AddComponent<BoxCollider2D>();
            collider.Size = new Vector2(data.Width, data.Height);
            collider.IsTrigger = false;

            data.CurrentPosition = platform.Transform.Position;
            data.InitialPosition = platform.Transform.Position;

            _gameObjects[$"Platform_{_gameObjects.Count}"] = platform;
            return platform;
        }

        private GameObject CreateHazard(HazardData data)
        {
            var hazard = new GameObject($"Hazard_{_gameObjects.Count}");
            hazard.Transform.Position = new Vector2(data.X + data.Width / 2, data.Y + data.Height / 2);
            hazard.Transform.Scale = new Vector2(data.Width, data.Height);

            var renderer = hazard.AddComponent<SpriteRenderer>();
            renderer.Color = data.HazardType == HazardType.Spikes ? new Color(0.8f, 0.2f, 0.2f) : new Color(0.3f, 0.3f, 0.3f);

            data.CenterPosition = hazard.Transform.Position;

            _gameObjects[$"Hazard_{_gameObjects.Count}"] = hazard;
            return hazard;
        }

        private GameObject CreateExitDoor(ExitData data)
        {
            var door = new GameObject("ExitDoor");
            door.Transform.Position = new Vector2(data.X, data.Y + 1.5f);
            door.Transform.Scale = new Vector2(1.5f, 3f);

            var renderer = door.AddComponent<SpriteRenderer>();
            renderer.Color = new Color(1.0f, 0.8f, 0.2f);

            var collider = door.AddComponent<BoxCollider2D>();
            collider.Size = new Vector2(1.5f, 3f);
            collider.IsTrigger = true;

            data.Position = door.Transform.Position;

            _gameObjects["ExitDoor"] = door;
            return door;
        }

        private void CreatePlayer()
        {
            _player = new GameObject("Player");
            _player.Transform.Position = _spawnPoint;

            var renderer = _player.AddComponent<SpriteRenderer>();
            renderer.Color = new Color(0.3f, 0.6f, 1.0f);
            renderer.SortingOrder = 100;

            var rigidBody = _player.AddComponent<Physics.RigidBody2D>();
            rigidBody.BodyType = Physics.RigidBodyType.Dynamic;
            rigidBody.Mass = 1f;
            rigidBody.GravityScale = _settings.GravityMultiplier;
            rigidBody.Friction = 0.5f;
            rigidBody.Drag = 0.1f;
            rigidBody.FreezeRotation = true;

            var collider = _player.AddComponent<BoxCollider2D>();
            collider.Size = new Vector2(0.8f, 1.0f);

            _playerController = _player.AddComponent<PlayerController>();
            _playerController.Initialize(_settings);

            _gameObjects["Player"] = _player;
        }

        private void CreateEnemies()
        {
            if (_currentLevel == null)
                return;

            foreach (var enemyData in _currentLevel.Enemies)
            {
                var enemy = new GameObject($"Enemy_{enemyData.EnemyType}");
                enemy.Transform.Position = new Vector2(enemyData.X, enemyData.Y + 0.5f);
                enemy.Transform.Scale = new Vector2(1f, 1f);

                var renderer = enemy.AddComponent<SpriteRenderer>();
                renderer.Color = enemyData.EnemyType == EnemyType.Patrol ? new Color(0.8f, 0.4f, 0.2f) : new Color(0.7f, 0.2f, 0.7f);
                renderer.SortingOrder = 50;

                var rigidBody = enemy.AddComponent<Physics.RigidBody2D>();
                rigidBody.BodyType = Physics.RigidBodyType.Kinematic;
                rigidBody.FreezeRotation = true;

                var collider = enemy.AddComponent<BoxCollider2D>();
                collider.Size = new Vector2(0.9f, 0.9f);
                collider.IsTrigger = false;

                var controller = enemy.AddComponent<EnemyController>();
                controller.Initialize(enemyData, _player);

                enemyData.GameObject = enemy;
                _gameObjects[$"Enemy_{enemyData.X}_{enemyData.Y}"] = enemy;
            }
        }

        private void CreateCollectibles()
        {
            if (_currentLevel == null)
                return;

            foreach (var coin in _currentLevel.Coins)
            {
                var coinObj = new GameObject($"Coin_{coin.Position.X}_{coin.Position.Y}");
                coinObj.Transform.Position = coin.Position;

                var renderer = coinObj.AddComponent<SpriteRenderer>();
                renderer.Color = new Color(1.0f, 0.9f, 0.2f);
                renderer.SortingOrder = 80;

                var collider = coinObj.AddComponent<CircleCollider2D>();
                collider.Radius = 0.3f;
                collider.IsTrigger = true;

                coin.GameObject = coinObj;
                _gameObjects[$"Coin_{coin.Position.X}_{coin.Position.Y}"] = coinObj;
            }
        }

        private void CollectCoin(CoinData coin)
        {
            coin.IsCollected = true;
            _totalCoins++;
            _totalScore += 100;

            if (coin.GameObject != null)
            {
                SpawnCoinParticles(coin.GameObject.Transform.Position);
                coin.GameObject.Destroy();
            }

            PlaySound("coin_collect");
            CoinCollected?.Invoke(this, _totalCoins);
            _ui.UpdateCoins(_totalCoins);
            _ui.UpdateScore(_totalScore);
        }

        private void ActivateCheckpoint(CheckpointData checkpoint)
        {
            checkpoint.IsActivated = true;
            _spawnPoint = checkpoint.Position + new Vector2(0, 0);

            foreach (var other in _currentLevel.Checkpoints)
            {
                if (other != checkpoint)
                {
                    other.IsActivated = false;
                }
            }

            PlaySound("checkpoint_activate");
        }

        private bool IsCollidingWithHazard(HazardData hazard)
        {
            if (_player == null || hazard.CenterPosition == Vector2.Zero)
                return false;

            Vector2 playerPos = _player.Transform.Position;
            Vector2 hazardPos = hazard.CenterPosition;

            float dx = Math.Abs(playerPos.X - hazardPos.X);
            float dy = Math.Abs(playerPos.Y - hazardPos.Y);

            return dx < (0.4f + hazard.Width / 2) && dy < (0.5f + hazard.Height / 2);
        }

        private void TakeDamage(float damage)
        {
            if (_invincibilityTimer > 0)
                return;

            _playerController.Health -= damage;
            _invincibilityTimer = 1.5f;

            ShakeCamera(0.3f, 0.3f);
            PlaySound("player_hurt");

            HealthChanged?.Invoke(this, (int)_playerController.Health);
            _ui.UpdateHealth(_playerController.Health);

            if (_playerController.Health <= 0)
            {
                PlayerDeath();
            }
        }

        private void KnockbackPlayer(HazardData hazard)
        {
            Vector2 direction = _player.Transform.Position - hazard.CenterPosition;
            direction.Y = 1.0f;
            direction = direction.Normalized;

            _playerController.ApplyKnockback(direction * 8f);
        }

        private void KnockbackPlayerFromEnemy(Vector2 enemyPosition)
        {
            Vector2 direction = _player.Transform.Position - enemyPosition;
            direction.Y = 1.0f;
            direction = direction.Normalized;

            _playerController.ApplyKnockback(direction * 10f);
        }

        private void PlayerDeath()
        {
            if (_currentLevel == null)
                return;

            SpawnDeathParticles(_player.Transform.Position);
            PlaySound("player_death");

            PlayerDied?.Invoke(this, EventArgs.Empty);

            _settings.StartingLives--;

            if (_settings.StartingLives <= 0 && !_settings.InfiniteLives)
            {
                SetGameState(PlatformerGameState.GameOver);
            }
            else
            {
                RespawnPlayer();
            }
        }

        private void RespawnPlayer()
        {
            if (_player != null)
            {
                _player.Transform.Position = _spawnPoint;

                if (_playerController != null)
                {
                    _playerController.Health = _settings.StartingHealth;
                    _playerController.Velocity = Vector2.Zero;
                    _invincibilityTimer = 2.0f;

                    HealthChanged?.Invoke(this, (int)_playerController.Health);
                    _ui.UpdateHealth(_playerController.Health);
                }
            }
        }

        private void CompleteLevel()
        {
            if (_currentLevel == null)
                return;

            _totalScore += 500;
            _ui.UpdateScore(_totalScore);

            PlaySound("level_complete");
            LevelCompleted?.Invoke(this, EventArgs.Empty);

            SetGameState(PlatformerGameState.LevelComplete);
        }

        public void RestartLevel()
        {
            _settings.StartingLives = 3;
            _currentLevelIndex = 0;
            _totalCoins = 0;
            _totalScore = 0;

            LoadLevel(_currentLevelIndex);
            SetGameState(PlatformerGameState.Playing);
        }

        public void NextLevel()
        {
            _currentLevelIndex++;

            if (_currentLevelIndex >= _levels.Count)
            {
                _ui.ShowGameComplete(_totalScore);
                SetGameState(PlatformerGameState.Menu);
            }
            else
            {
                LoadLevel(_currentLevelIndex);
                SetGameState(PlatformerGameState.Playing);
            }
        }

        private void SetGameState(PlatformerGameState newState)
        {
            _previousState = _gameState;
            _gameState = newState;

            switch (_gameState)
            {
                case PlatformerGameState.Playing:
                    _ui.HideAllMenus();
                    _ui.ShowHUD();
                    AudioManager.Instance.ResumeAllSounds();
                    break;

                case PlatformerGameState.Paused:
                    _ui.ShowPauseMenu();
                    AudioManager.Instance.PauseAllSounds();
                    break;

                case PlatformerGameState.GameOver:
                    _ui.ShowGameOverScreen();
                    AudioManager.Instance.StopMusic();
                    break;

                case PlatformerGameState.LevelComplete:
                    _ui.ShowLevelComplete(_currentLevel?.Name ?? "Level");
                    break;

                case PlatformerGameState.Menu:
                    _ui.ShowMainMenu();
                    _ui.HideHUD();
                    break;
            }

            GameStateChanged?.Invoke(this, _gameState);
        }

        private void ShakeCamera(float intensity, float duration)
        {
            _cameraShakeIntensity = intensity;
            _cameraShakeDuration = duration;
            _cameraShakeTimer = duration;
        }

        private void SpawnDustParticles(Vector2 position)
        {
            if (_dustEmitter == null)
                return;

            _dustEmitter.Transform.Position = position;
            _dustEmitter.Emit(2);
        }

        private void SpawnCoinParticles(Vector2 position)
        {
            if (_coinEmitter == null)
                return;

            _coinEmitter.Transform.Position = position;
            _coinEmitter.Emit(10);
        }

        private void SpawnDeathParticles(Vector2 position)
        {
            if (_deathEmitter == null)
                return;

            _deathEmitter.Transform.Position = position;
            _deathEmitter.Emit(20);
        }

        private void SpawnJumpParticles(Vector2 position)
        {
            if (_jumpEmitter == null)
                return;

            _jumpEmitter.Transform.Position = position;
            _jumpEmitter.Emit(8);
        }

        private void PlaySound(string soundName)
        {
            AudioManager.Instance.PlaySound(soundName, _settings.SfxVolume);
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            if (_currentLevel == null)
                return;

            DrawBackground(context);
            DrawLevel(context);
            DrawEnemies(context);
            DrawPlayer(context);
            DrawUI(context);
        }

        private void DrawBackground(DrawingContext context)
        {
            if (_currentLevel == null)
                return;

            var backgroundBrush = new SolidColorBrush(
                System.Windows.Media.Color.FromRgb(
                    (byte)(_currentLevel.BackgroundColor.R * 255),
                    (byte)(_currentLevel.BackgroundColor.G * 255),
                    (byte)(_currentLevel.BackgroundColor.B * 255)
                )
            );

            context.DrawRectangle(backgroundBrush, null, new Rect(0, 0, 1920, 1080));
        }

        private void DrawLevel(DrawingContext context)
        {
            if (_currentLevel == null || _gameCamera == null)
                return;

            Vector2 cameraPos = _gameCamera.Position;
            float viewWidth = 20f;
            float viewHeight = 15f;

            foreach (var platform in _currentLevel.Platforms)
            {
                Vector2 screenPos = WorldToScreen(platform.CurrentPosition, cameraPos, viewWidth, viewHeight);
                Vector2 size = new Vector2(platform.Width * 40, platform.Height * 40);

                var brush = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(
                        (byte)(platform.PlatformType == PlatformType.Ground ? 100 : 150),
                        (byte)(platform.PlatformType == PlatformType.Ground ? 150 : 100),
                        (byte)(platform.PlatformType == PlatformType.Ground ? 80 : 50)
                    )
                );

                context.DrawRectangle(brush, null, new Rect(screenPos.X - size.X / 2, screenPos.Y - size.Y / 2, size.X, size.Y));
            }

            foreach (var hazard in _currentLevel.Hazards)
            {
                Vector2 screenPos = WorldToScreen(hazard.CenterPosition, cameraPos, viewWidth, viewHeight);
                Vector2 size = new Vector2(hazard.Width * 40, hazard.Height * 40);

                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 50, 50));

                for (int i = 0; i < hazard.Width; i++)
                {
                    var triangle = new StreamGeometry();
                    using (var ctx = triangle.Open())
                    {
                        ctx.BeginFigure(new Point(screenPos.X - hazard.Width * 20 + i * 40 + 20, screenPos.Y + size.Y / 2), true, true);
                        ctx.LineTo(new Point(screenPos.X - hazard.Width * 20 + i * 40, screenPos.Y - size.Y / 2), true, false);
                        ctx.LineTo(new Point(screenPos.X - hazard.Width * 20 + i * 40 + 40, screenPos.Y - size.Y / 2), true, false);
                    }
                    context.DrawGeometry(brush, null, triangle);
                }
            }

            foreach (var coin in _currentLevel.Coins)
            {
                if (coin.IsCollected)
                    continue;

                Vector2 screenPos = WorldToScreen(coin.Position, cameraPos, viewWidth, viewHeight);
                float radius = 15;

                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 230, 50));
                context.DrawEllipse(brush, null, new Point(screenPos.X, screenPos.Y), radius, radius);
            }

            if (_currentLevel.ExitDoor != null)
            {
                Vector2 screenPos = WorldToScreen(_currentLevel.ExitDoor.Position, cameraPos, viewWidth, viewHeight);
                Vector2 size = new Vector2(60, 120);

                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 50));
                var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 150, 0)), 3);

                context.DrawRectangle(brush, pen, new Rect(screenPos.X - size.X / 2, screenPos.Y - size.Y / 2, size.X, size.Y));
            }
        }

        private void DrawEnemies(DrawingContext context)
        {
            if (_currentLevel == null || _gameCamera == null)
                return;

            Vector2 cameraPos = _gameCamera.Position;
            float viewWidth = 20f;
            float viewHeight = 15f;

            foreach (var enemyData in _currentLevel.Enemies)
            {
                if (enemyData.GameObject == null)
                    continue;

                Vector2 worldPos = enemyData.GameObject.Transform.Position;
                Vector2 screenPos = WorldToScreen(worldPos, cameraPos, viewWidth, viewHeight);
                Vector2 size = new Vector2(40, 40);

                var brush = new SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(
                        (byte)(enemyData.EnemyType == EnemyType.Patrol ? 200 : 180),
                        (byte)(enemyData.EnemyType == EnemyType.Patrol ? 100 : 50),
                        (byte)(enemyData.EnemyType == EnemyType.Patrol ? 50 : 180)
                    )
                );

                context.DrawRectangle(brush, null, new Rect(screenPos.X - size.X / 2, screenPos.Y - size.Y / 2, size.X, size.Y));
            }
        }

        private void DrawPlayer(DrawingContext context)
        {
            if (_player == null || _gameCamera == null)
                return;

            Vector2 cameraPos = _gameCamera.Position;
            float viewWidth = 20f;
            float viewHeight = 15f;

            Vector2 worldPos = _player.Transform.Position;
            Vector2 screenPos = WorldToScreen(worldPos, cameraPos, viewWidth, viewHeight);
            Vector2 size = new Vector2(32, 40);

            byte alpha = 255;
            if (_invincibilityTimer > 0)
            {
                alpha = (byte)(Math.Sin(_invincibilityTimer * 15) > 0 ? 255 : 100);
            }

            var brush = new SolidColorBrush(
                System.Windows.Media.Color.FromArgb(alpha, 77, 153, 255)
            );

            context.DrawRectangle(brush, null, new Rect(screenPos.X - size.X / 2, screenPos.Y - size.Y / 2, size.X, size.Y));
        }

        private void DrawUI(DrawingContext context)
        {
            _ui.Render(context, _gameCamera?.Position ?? Vector2.Zero);
        }

        private Vector2 WorldToScreen(Vector2 worldPos, Vector2 cameraPos, float viewWidth, float viewHeight)
        {
            Vector2 offset = worldPos - cameraPos;
            offset.Y = -offset.Y;

            return new Vector2(
                1920 / 2 + offset.X * 96,
                1080 / 2 + offset.Y * 72
            );
        }

        public override void Dispose()
        {
            ClearLevel();
            _ui?.Dispose();
            base.Dispose();
        }
    }

    public class PlayerController : Component
    {
        private float _health;
        private float _maxHealth;
        private Vector2 _velocity;
        private bool _isGrounded;
        private bool _isWallSliding;
        private int _jumpCount;
        private int _maxJumps;
        private bool _canDoubleJump;
        private bool _canWallJump;
        private bool _canWallSlide;
        private float _moveSpeed;
        private float _jumpForce;
        private float _wallSlideSpeed;
        private float _gravity;
        private Vector2 _knockbackForce;
        private float _knockbackTimer;
        private PlayerAnimationState _animationState;
        private int _facingDirection;
        private PlatformerSettings _settings;

        public float Health => _health;
        public Vector2 Velocity => _velocity;
        public bool IsGrounded => _isGrounded;
        public bool IsWallSliding => _isWallSliding;
        public PlayerAnimationState AnimationState => _animationState;

        public void Initialize(PlatformerSettings settings)
        {
            _settings = settings;
            _maxHealth = settings.StartingHealth;
            _health = _maxHealth;
            _moveSpeed = settings.PlayerSpeed;
            _jumpForce = settings.JumpForce;
            _wallSlideSpeed = settings.WallSlideSpeed;
            _gravity = 25f * settings.GravityMultiplier;
            _maxJumps = settings.AllowDoubleJump ? 2 : 1;
            _canDoubleJump = settings.AllowDoubleJump;
            _canWallJump = settings.AllowWallJump;
            _canWallSlide = settings.AllowWallSlide;
            _jumpCount = 0;
            _isGrounded = false;
            _isWallSliding = false;
            _facingDirection = 1;
            _velocity = Vector2.Zero;
            _knockbackForce = Vector2.Zero;
            _knockbackTimer = 0f;
        }

        public override void Update(float deltaTime)
        {
            if (GameObject == null)
                return;

            ProcessInput(deltaTime);
            ApplyPhysics(deltaTime);
            UpdateAnimation();
            UpdateKnockback(deltaTime);
        }

        private void ProcessInput(float deltaTime)
        {
            float horizontal = InputManager.Instance.GetAxis(InputAxis.Horizontal);

            if (_knockbackTimer <= 0)
            {
                _velocity.X = horizontal * _moveSpeed;

                if (horizontal > 0.1f)
                    _facingDirection = 1;
                else if (horizontal < -0.1f)
                    _facingDirection = -1;
            }

            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Space) || InputManager.Instance.GetKeyDown(InputManager.KeyCode.W) || InputManager.Instance.GetKeyDown(InputManager.KeyCode.UpArrow))
            {
                TryJump();
            }

            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.A) && _canWallSlide && _isWallSliding)
            {
                WallJump(-1);
            }
            else if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D) && _canWallSlide && _isWallSliding)
            {
                WallJump(1);
            }
        }

        private void TryJump()
        {
            if (_isGrounded)
            {
                _velocity.Y = _jumpForce;
                _jumpCount = 1;
                _isGrounded = false;
                SpawnJumpEffect();
            }
            else if (_canDoubleJump && _jumpCount < _maxJumps)
            {
                _velocity.Y = _jumpForce * 0.9f;
                _jumpCount++;
                SpawnJumpEffect();
            }
            else if (_canWallSlide && _isWallSliding)
            {
                WallJump(_facingDirection);
            }
        }

        private void WallJump(int direction)
        {
            _velocity.Y = _jumpForce * 0.9f;
            _velocity.X = direction * _moveSpeed * 1.5f;
            _jumpCount = 1;
            _isWallSliding = false;
            _facingDirection = direction;
            SpawnJumpEffect();
        }

        private void SpawnJumpEffect()
        {
        }

        private void ApplyPhysics(float deltaTime)
        {
            _velocity.Y -= _gravity * deltaTime;

            if (_velocity.Y < -30f)
                _velocity.Y = -30f;

            if (_isWallSliding && _velocity.Y < 0)
            {
                _velocity.Y = -_wallSlideSpeed;
            }

            Vector2 newPosition = GameObject.Transform.Position + _velocity * deltaTime;

            _isGrounded = false;
            _isWallSliding = false;

            CheckCollisions(ref newPosition);

            GameObject.Transform.Position = newPosition;

            if (GameObject.Transform.Position.Y < -10)
            {
                _health = 0;
            }
        }

        private void CheckCollisions(ref Vector2 position)
        {
            float playerHeight = 1.0f;
            float playerWidth = 0.8f;

            var scene = GameObject.CurrentScene;
            if (scene == null)
                return;

            _isGrounded = false;

            foreach (var obj in scene.GameObjects)
            {
                if (obj == GameObject)
                    continue;

                var collider = obj.GetComponent<BoxCollider2D>();
                if (collider == null)
                    continue;

                var objPos = obj.Transform.Position;
                var objScale = obj.Transform.Scale;

                float left1 = position.X - playerWidth / 2;
                float right1 = position.X + playerWidth / 2;
                float bottom1 = position.Y - playerHeight / 2;
                float top1 = position.Y + playerHeight / 2;

                float left2 = objPos.X - objScale.X / 2;
                float right2 = objPos.X + objScale.X / 2;
                float bottom2 = objPos.Y - objScale.Y / 2;
                float top2 = objPos.Y + objScale.Y / 2;

                if (right1 > left2 && left1 < right2 && top1 > bottom2 && bottom1 < top2)
                {
                    float overlapLeft = right1 - left2;
                    float overlapRight = right2 - left1;
                    float overlapBottom = top1 - bottom2;
                    float overlapTop = top2 - bottom1;

                    float minOverlapX = Math.Min(overlapLeft, overlapRight);
                    float minOverlapY = Math.Min(overlapBottom, overlapTop);

                    if (minOverlapY < minOverlapX)
                    {
                        if (overlapBottom < overlapTop && _velocity.Y > 0)
                        {
                            position.Y = bottom2 - playerHeight / 2;
                            _velocity.Y = 0;
                            _isGrounded = true;
                            _jumpCount = 0;
                        }
                        else if (overlapTop < overlapBottom && _velocity.Y < 0)
                        {
                            position.Y = top2 + playerHeight / 2;
                            _velocity.Y = 0;
                        }
                    }
                    else
                    {
                        if (overlapLeft < overlapRight)
                        {
                            position.X = left2 - playerWidth / 2;
                            if (!_isGrounded && _velocity.Y < 0)
                                _isWallSliding = true;
                        }
                        else
                        {
                            position.X = right2 + playerWidth / 2;
                            if (!_isGrounded && _velocity.Y < 0)
                                _isWallSliding = true;
                        }
                        _velocity.X = 0;
                    }
                }
            }
        }

        private void UpdateAnimation()
        {
            if (_knockbackTimer > 0)
            {
                _animationState = PlayerAnimationState.Hurt;
            }
            else if (_isWallSliding)
            {
                _animationState = PlayerAnimationState.WallSlide;
            }
            else if (!_isGrounded)
            {
                _animationState = _velocity.Y > 0 ? PlayerAnimationState.Fall : PlayerAnimationState.Jump;
            }
            else if (Math.Abs(_velocity.X) > 0.1f)
            {
                _animationState = PlayerAnimationState.Run;
            }
            else
            {
                _animationState = PlayerAnimationState.Idle;
            }
        }

        private void UpdateKnockback(float deltaTime)
        {
            if (_knockbackTimer > 0)
            {
                _knockbackTimer -= deltaTime;
            }
        }

        public void ApplyKnockback(Vector2 force)
        {
            _knockbackForce = force;
            _knockbackTimer = 0.3f;
            _velocity = force;
        }

        public void MoveWithPlatform(Vector2 offset)
        {
            GameObject.Transform.Position += offset;
        }
    }

    public class EnemyController : Component
    {
        private EnemyState _currentState;
        private EnemyState _previousState;
        private Vector2 _velocity;
        private float _speed;
        private float _patrolDistance;
        private float _chaseRange;
        private Vector2 _startPosition;
        private Vector2 _patrolDirection;
        private float _stateTimer;
        private GameObject _player;
        private EnemySpawnData _enemyData;

        public EnemyState CurrentState => _currentState;

        public void Initialize(EnemySpawnData data, GameObject player)
        {
            _enemyData = data;
            _player = player;
            _startPosition = GameObject.Transform.Position;
            _patrolDirection = Vector2.Right;
            _currentState = EnemyState.Patrol;
            _previousState = EnemyState.Patrol;
            _stateTimer = 0f;

            switch (data.EnemyType)
            {
                case EnemyType.Patrol:
                    _speed = 2.0f;
                    _patrolDistance = data.PatrolDistance;
                    break;
                case EnemyType.Chaser:
                    _speed = 3.0f;
                    _chaseRange = data.ChaseRange;
                    break;
            }
        }

        public override void Update(float deltaTime)
        {
            if (GameObject == null)
                return;

            UpdateState();
            ExecuteState(deltaTime);
        }

        private void UpdateState()
        {
            if (_player == null)
                return;

            Vector2 toPlayer = _player.Transform.Position - GameObject.Transform.Position;
            float distanceToPlayer = toPlayer.Magnitude;

            switch (_enemyData.EnemyType)
            {
                case EnemyType.Patrol:
                    if (_currentState != EnemyState.Return)
                    {
                        if (distanceToPlayer < 2.0f)
                        {
                            SetState(EnemyState.Attack);
                        }
                        else if (IsOutOfPatrolRange())
                        {
                            SetState(EnemyState.Return);
                        }
                    }
                    break;

                case EnemyType.Chaser:
                    if (_currentState == EnemyState.Patrol && distanceToPlayer < _chaseRange)
                    {
                        SetState(EnemyState.Chase);
                    }
                    else if (_currentState == EnemyState.Chase && distanceToPlayer > _chaseRange * 1.5f)
                    {
                        SetState(EnemyState.Return);
                    }
                    break;
            }
        }

        private void SetState(EnemyState newState)
        {
            _previousState = _currentState;
            _currentState = newState;
            _stateTimer = 0f;
        }

        private void ExecuteState(float deltaTime)
        {
            switch (_currentState)
            {
                case EnemyState.Patrol:
                    Patrol(deltaTime);
                    break;
                case EnemyState.Chase:
                    Chase(deltaTime);
                    break;
                case EnemyState.Attack:
                    Attack(deltaTime);
                    break;
                case EnemyState.Return:
                    ReturnToStart(deltaTime);
                    break;
            }

            _stateTimer += deltaTime;
        }

        private void Patrol(float deltaTime)
        {
            _velocity = _patrolDirection * _speed;
            GameObject.Transform.Position += _velocity * deltaTime;

            if (IsOutOfPatrolRange())
            {
                _patrolDirection = -_patrolDirection;
            }
        }

        private void Chase(float deltaTime)
        {
            if (_player == null)
                return;

            Vector2 direction = (_player.Transform.Position - GameObject.Transform.Position).Normalized;
            _velocity = direction * _speed * 1.2f;
            GameObject.Transform.Position += _velocity * deltaTime;
        }

        private void Attack(float deltaTime)
        {
            _velocity = Vector2.Zero;
        }

        private void ReturnToStart(float deltaTime)
        {
            Vector2 direction = (_startPosition - GameObject.Transform.Position).Normalized;

            if (direction.Magnitude < 0.5f)
            {
                SetState(EnemyState.Patrol);
                _patrolDirection = Vector2.Right;
            }
            else
            {
                _velocity = direction * _speed * 0.8f;
                GameObject.Transform.Position += _velocity * deltaTime;
            }
        }

        private bool IsOutOfPatrolRange()
        {
            float distanceFromStart = (GameObject.Transform.Position - _startPosition).Magnitude;
            return distanceFromStart > _patrolDistance;
        }
    }

    public class PlatformerUI : IDisposable
    {
        private PlatformerGame _game;
        private bool _isInitialized;
        private Dictionary<string, UIElement> _uiElements;
        private int _displayedCoins;
        private int _displayedScore;
        private float _displayedHealth;
        private bool _hudVisible;
        private bool _pauseMenuVisible;
        private bool _gameOverVisible;
        private bool _levelCompleteVisible;
        private bool _mainMenuVisible;

        public PlatformerUI()
        {
            _uiElements = new Dictionary<string, UIElement>();
            _isInitialized = false;
            _displayedCoins = 0;
            _displayedScore = 0;
            _displayedHealth = 100f;
            _hudVisible = false;
            _pauseMenuVisible = false;
            _gameOverVisible = false;
            _levelCompleteVisible = false;
            _mainMenuVisible = false;
        }

        public void Initialize(PlatformerGame game)
        {
            _game = game;
            CreateUIElements();
            _isInitialized = true;
        }

        private void CreateUIElements()
        {
        }

        public void ShowMainMenu()
        {
            HideAllMenus();
            _mainMenuVisible = true;
        }

        public void ShowHUD()
        {
            HideAllMenus();
            _hudVisible = true;
        }

        public void HideHUD()
        {
            _hudVisible = false;
        }

        public void ShowPauseMenu()
        {
            _pauseMenuVisible = true;
        }

        public void HidePauseMenu()
        {
            _pauseMenuVisible = false;
        }

        public void ShowGameOverScreen()
        {
            HideAllMenus();
            _gameOverVisible = true;
        }

        public void ShowLevelComplete(string levelName)
        {
            HideAllMenus();
            _levelCompleteVisible = true;
        }

        public void ShowGameComplete(int finalScore)
        {
            HideAllMenus();
            _mainMenuVisible = true;
        }

        public void HideAllMenus()
        {
            _mainMenuVisible = false;
            _hudVisible = false;
            _pauseMenuVisible = false;
            _gameOverVisible = false;
            _levelCompleteVisible = false;
        }

        public void UpdateHealth(float health)
        {
            _displayedHealth = health;
        }

        public void UpdateCoins(int coins)
        {
            _displayedCoins = coins;
        }

        public void UpdateScore(int score)
        {
            _displayedScore = score;
        }

        public void Update(float deltaTime)
        {
        }

        public void Render(DrawingContext context, Vector2 cameraPosition)
        {
            if (_hudVisible)
            {
                RenderHUD(context);
            }

            if (_pauseMenuVisible)
            {
                RenderPauseMenu(context);
            }

            if (_gameOverVisible)
            {
                RenderGameOver(context);
            }

            if (_levelCompleteVisible)
            {
                RenderLevelComplete(context);
            }

            if (_mainMenuVisible)
            {
                RenderMainMenu(context);
            }
        }

        private void RenderHUD(DrawingContext context)
        {
            var healthBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 80, 80));
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 50, 50, 50));
            var textBrush = new SolidColorBrush(Colors.White);
            var coinBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 220, 50));

            context.DrawRectangle(backgroundBrush, null, new Rect(20, 20, 200, 60));

            context.DrawRectangle(healthBrush, null, new Rect(30, 30, 180 * (_displayedHealth / 100f), 20));

            string healthText = $"HP: {_displayedHealth:F0}";
            var healthFormatted = new FormattedText(
                healthText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 16, textBrush, 1.0);
            context.DrawText(healthFormatted, new Point(35, 55));

            context.DrawRectangle(backgroundBrush, null, new Rect(20, 90, 200, 60));

            context.DrawEllipse(coinBrush, null, new Point(45, 120), 12, 12);

            string coinText = $"x {_displayedCoins}";
            var coinFormatted = new FormattedText(
                coinText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 18, textBrush, 1.0);
            context.DrawText(coinFormatted, new Point(65, 108));

            string scoreText = $"Score: {_displayedScore}";
            var scoreFormatted = new FormattedText(
                scoreText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 20, textBrush, 1.0);
            context.DrawText(scoreFormatted, new Point(20, 160));
        }

        private void RenderPauseMenu(DrawingContext context)
        {
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0));
            context.DrawRectangle(overlayBrush, null, new Rect(0, 0, 1920, 1080));

            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 40));
            var textBrush = new SolidColorBrush(Colors.White);
            var buttonBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 60, 60));
            var hoverBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 80));

            context.DrawRectangle(panelBrush, null, new Rect(760, 384, 400, 312));

            var titleFormatted = new FormattedText(
                "PAUSED", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 36, textBrush, 1.0);
            context.DrawText(titleFormatted, new Point(920 - titleFormatted.Width / 2, 410));

            string resumeText = "Press ESC to Resume";
            var resumeFormatted = new FormattedText(
                resumeText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 20, textBrush, 1.0);
            context.DrawText(resumeFormatted, new Point(960 - resumeFormatted.Width / 2, 500));

            string quitText = "Press Q to Quit";
            var quitFormatted = new FormattedText(
                quitText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 18, textBrush, 1.0);
            context.DrawText(quitFormatted, new Point(960 - quitFormatted.Width / 2, 600));
        }

        private void RenderGameOver(DrawingContext context)
        {
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 0, 0, 0));
            context.DrawRectangle(overlayBrush, null, new Rect(0, 0, 1920, 1080));

            var textBrush = new SolidColorBrush(Colors.White);
            var redBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 80, 80));

            var gameOverFormatted = new FormattedText(
                "GAME OVER", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 60, redBrush, 1.0);
            context.DrawText(gameOverFormatted, new Point(960 - gameOverFormatted.Width / 2, 400));

            string scoreText = $"Final Score: {_displayedScore}";
            var scoreFormatted = new FormattedText(
                scoreText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 28, textBrush, 1.0);
            context.DrawText(scoreFormatted, new Point(960 - scoreFormatted.Width / 2, 500));

            string restartText = "Press R to Restart";
            var restartFormatted = new FormattedText(
                restartText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 24, textBrush, 1.0);
            context.DrawText(restartFormatted, new Point(960 - restartFormatted.Width / 2, 580));
        }

        private void RenderLevelComplete(DrawingContext context)
        {
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0));
            context.DrawRectangle(overlayBrush, null, new Rect(0, 0, 1920, 1080));

            var textBrush = new SolidColorBrush(Colors.White);
            var goldBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 220, 100));

            var completeFormatted = new FormattedText(
                "LEVEL COMPLETE!", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 48, goldBrush, 1.0);
            context.DrawText(completeFormatted, new Point(960 - completeFormatted.Width / 2, 380));

            string scoreText = $"Score: {_displayedScore}";
            var scoreFormatted = new FormattedText(
                scoreText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 28, textBrush, 1.0);
            context.DrawText(scoreFormatted, new Point(960 - scoreFormatted.Width / 2, 460));

            string coinsText = $"Coins: {_displayedCoins}";
            var coinsFormatted = new FormattedText(
                coinsText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 24, textBrush, 1.0);
            context.DrawText(coinsFormatted, new Point(960 - coinsFormatted.Width / 2, 510));

            string continueText = "Press ENTER to Continue";
            var continueFormatted = new FormattedText(
                continueText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 22, textBrush, 1.0);
            context.DrawText(continueFormatted, new Point(960 - continueFormatted.Width / 2, 600));
        }

        private void RenderMainMenu(DrawingContext context)
        {
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 60, 90));
            context.DrawRectangle(backgroundBrush, null, new Rect(0, 0, 1920, 1080));

            var titleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 180, 255));
            var textBrush = new SolidColorBrush(Colors.White);

            var titleFormatted = new FormattedText(
                "2D PLATFORMER DEMO", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"), 64, titleBrush, 1.0);
            context.DrawText(titleFormatted, new Point(960 - titleFormatted.Width / 2, 300));

            string startText = "Press ENTER to Start";
            var startFormatted = new FormattedText(
                startText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 32, textBrush, 1.0);
            context.DrawText(startFormatted, new Point(960 - startFormatted.Width / 2, 450));

            string controlsText = "Controls: WASD or Arrow Keys to Move, Space to Jump";
            var controlsFormatted = new FormattedText(
                controlsText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 18, textBrush, 1.0);
            context.DrawText(controlsFormatted, new Point(960 - controlsFormatted.Width / 2, 550));

            string featuresText = "Features: Double Jump, Wall Slide, Collectibles, Enemies";
            var featuresFormatted = new FormattedText(
                featuresText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"), 16, textBrush, 1.0);
            context.DrawText(featuresFormatted, new Point(960 - featuresFormatted.Width / 2, 600));
        }

        public void Dispose()
        {
            _uiElements.Clear();
            _isInitialized = false;
        }
    }

    public class Camera2D : Component
    {
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float OrthographicSize { get; set; } = 10f;
        public float Zoom { get; set; } = 1f;
    }

    public class ParticleEmitter : Component
    {
        public int MaxParticles { get; set; }
        public int EmissionRate { get; set; }
        public float ParticleLifetime { get; set; }
        public float StartSize { get; set; }
        public float StartSizeVariation { get; set; }
        public float EmissionSpeed { get; set; }
        public float EmissionSpeedVariation { get; set; }
        public Vector2 EmissionAngle { get; set; }
        public float GravityModifier { get; set; }
        public bool IsEmitting { get; set; }

        private List<ParticleData> _activeParticles;
        private float _emitAccumulator;

        public ParticleEmitter()
        {
            _activeParticles = new List<ParticleData>();
            MaxParticles = 100;
            EmissionRate = 10;
            ParticleLifetime = 1f;
            StartSize = 0.1f;
            StartSizeVariation = 0.05f;
            EmissionSpeed = 2f;
            EmissionSpeedVariation = 1f;
            EmissionAngle = new Vector2(-60, 60);
            GravityModifier = 1f;
            IsEmitting = false;
            _emitAccumulator = 0f;
        }

        public void Emit(int count)
        {
            for (int i = 0; i < count; i++)
            {
                if (_activeParticles.Count >= MaxParticles)
                    break;

                var particle = new ParticleData
                {
                    Position = Transform?.Position ?? Vector2.Zero,
                    Velocity = GetRandomVelocity(),
                    Lifetime = ParticleLifetime,
                    Age = 0f,
                    Size = StartSize + (float)(Math.Random.NextDouble() * 2 - 1) * StartSizeVariation,
                    Color = Colors.White
                };

                _activeParticles.Add(particle);
            }
        }

        private Vector2 GetRandomVelocity()
        {
            float angle = (float)(Math.Random.NextDouble() * (EmissionAngle.Y - EmissionAngle.X) + EmissionAngle.X);
            float speed = EmissionSpeed + (float)(Math.Random.NextDouble() * 2 - 1) * EmissionSpeedVariation;
            float radians = angle * (float)Math.PI / 180f;
            return new Vector2((float)Math.Cos(radians) * speed, (float)Math.Sin(radians) * speed);
        }

        public void Update(float deltaTime)
        {
            for (int i = _activeParticles.Count - 1; i >= 0; i--)
            {
                var particle = _activeParticles[i];
                particle.Age += deltaTime;

                if (particle.Age >= particle.Lifetime)
                {
                    _activeParticles.RemoveAt(i);
                    continue;
                }

                particle.Velocity.Y -= 9.81f * GravityModifier * deltaTime;
                particle.Position += particle.Velocity * deltaTime;
            }

            if (IsEmitting && EmissionRate > 0)
            {
                float timeBetweenEmissions = 1f / EmissionRate;
                _emitAccumulator += deltaTime;

                while (_emitAccumulator >= timeBetweenEmissions)
                {
                    _emitAccumulator -= timeBetweenEmissions;
                    Emit(1);
                }
            }
        }
    }

    public class ParticleData
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Lifetime { get; set; }
        public float Age { get; set; }
        public float Size { get; set; }
        public Color Color { get; set; }
        public float NormalizedAge => Lifetime > 0 ? Age / Lifetime : 1f;
    }

    public class PlatformerSettings
    {
        public float MasterVolume { get; set; }
        public float MusicVolume { get; set; }
        public float SfxVolume { get; set; }
        public Difficulty Difficulty { get; set; }
        public bool ShowDebugInfo { get; set; }
        public bool InfiniteLives { get; set; }
        public int StartingLives { get; set; }
        public float StartingHealth { get; set; }
        public bool AllowDoubleJump { get; set; }
        public bool AllowWallJump { get; set; }
        public bool AllowWallSlide { get; set; }
        public float GravityMultiplier { get; set; }
        public float PlayerSpeed { get; set; }
        public float JumpForce { get; set; }
        public float WallSlideSpeed { get; set; }
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard
    }

    public class LevelData
    {
        public string Name { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public Color BackgroundColor { get; set; }
        public string MusicTrack { get; set; }
        public Vector2 PlayerSpawn { get; set; }
        public List<PlatformData> Platforms { get; set; } = new List<PlatformData>();
        public List<CoinData> Coins { get; set; } = new List<CoinData>();
        public List<HazardData> Hazards { get; set; } = new List<HazardData>();
        public List<CheckpointData> Checkpoints { get; set; } = new List<CheckpointData>();
        public List<EnemySpawnData> Enemies { get; set; } = new List<EnemySpawnData>();
        public ExitData ExitDoor { get; set; }
    }

    public class PlatformData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public PlatformType PlatformType { get; set; }
        public float MoveDistance { get; set; }
        public float MoveSpeed { get; set; }
        public Vector2 InitialPosition { get; set; }
        public Vector2 CurrentPosition { get; set; }
        public Vector2 Velocity { get; set; }
        private float _moveTimer;

        public void Update(float deltaTime)
        {
            if (PlatformType != PlatformType.Moving)
                return;

            _moveTimer += deltaTime * MoveSpeed;
            float offset = (float)Math.Sin(_moveTimer) * MoveDistance;
            CurrentPosition = InitialPosition + new Vector2(0, offset);

            Vector2 previousPosition = CurrentPosition - new Vector2(0, (float)Math.Cos(_moveTimer) * MoveDistance * MoveSpeed * deltaTime);
            Velocity = CurrentPosition - previousPosition;
        }
    }

    public enum PlatformType
    {
        Static,
        Moving,
        Ground
    }

    public class CoinData
    {
        public Vector2 Position { get; set; }
        public bool IsCollected { get; set; }
        public GameObject GameObject { get; set; }
    }

    public class HazardData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Width { get; set; }
        public float Height { get; set; }
        public HazardType HazardType { get; set; }
        public Vector2 CenterPosition { get; set; }
    }

    public enum HazardType
    {
        Spikes,
        Lava,
        Saw
    }

    public class CheckpointData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public bool IsActivated { get; set; }
        public Vector2 Position => new Vector2(X + 0.5f, Y + 1f);
    }

    public class EnemySpawnData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public EnemyType EnemyType { get; set; }
        public float PatrolDistance { get; set; }
        public float ChaseRange { get; set; }
        public GameObject GameObject { get; set; }
    }

    public enum EnemyType
    {
        Patrol,
        Chaser,
        Shooter
    }

    public class ExitData
    {
        public float X { get; set; }
        public float Y { get; set; }
        public Vector2 Position => new Vector2(X + 0.75f, Y + 1.5f);
    }

    public static class Colors
    {
        public static Color White => new Color(1f, 1f, 1f);
        public static Color Black => new Color(0f, 0f, 0f);
        public static Color Red => new Color(1f, 0f, 0f);
        public static Color Green => new Color(0f, 1f, 0f);
        public static Color Blue => new Color(0f, 0f, 1f);
        public static Color SkyBlue => new Color(0.53f, 0.81f, 0.92f);
    }
}

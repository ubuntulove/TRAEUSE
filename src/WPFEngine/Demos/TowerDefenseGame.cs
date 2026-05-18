using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WPFEngine.Core;
using WPFEngine.Math;

namespace WPFEngine.Demos
{
    public enum TDGameState
    {
        Menu,
        Building,
        Wave,
        Paused,
        GameOver,
        Victory
    }

    public enum TowerType
    {
        Basic,
        Sniper,
        Cannon,
        Frost,
        Poison,
        Tesla,
        Archer
    }

    public enum EnemyType
    {
        Basic,
        Fast,
        Tank,
        Flying,
        Boss,
        Healer
    }

    public enum TargetingPriority
    {
        First,
        Last,
        Closest,
        Strongest,
        Weakest
    }

    public enum MapLayout
    {
        Simple,
        Loop,
        Cross,
        Maze
    }

    public enum GameDifficulty
    {
        Easy,
        Normal,
        Hard,
        Insane
    }

    public enum AchievementType
    {
        FirstBlood,
        Wave10,
        WaveComplete,
        TowerMaster,
        SpeedDemon,
        NoDamage,
        BossSlayer,
        Millionaire
    }

    public class TowerDefenseGame : GameBase
    {
        private const int GRID_WIDTH = 20;
        private const int GRID_HEIGHT = 15;
        private const float CELL_SIZE = 50f;
        private const float STARTING_GOLD = 200f;
        private const float STARTING_LIVES = 20f;
        private const float WAVE_BONUS = 50f;
        private const float INTEREST_RATE = 0.05f;
        private const float MIN_GOLD_FOR_INTEREST = 100f;

        private TDGameState _gameState;
        private TDGameState _previousState;
        private int _currentWave;
        private float _gold;
        private float _lives;
        private int _score;
        private float _waveCountdown;
        private float _interestTimer;
        private float _gameSpeed;
        private bool _isSpeedUp;
        private MapLayout _currentMap;
        private GameDifficulty _difficulty;
        private TowerType _selectedTowerType;
        private Tower? _selectedTower;
        private Vector2 _mouseGridPos;
        private bool _isMouseOverGrid;
        private TargetingPriority _targetingPriority;
        private bool _showRangeIndicator;
        private Vector2 _mapOffset;
        private Dictionary<AchievementType, bool> _achievements;
        private Dictionary<string, int> _statistics;
        private float _noDamageTimer;
        private int _totalEnemiesKilled;
        private float _fastestWaveTime;
        private bool _isSpecialWave;
        private float _comboMultiplier;
        private float _comboTimer;
        private int _consecutiveKills;

        private MapGrid _mapGrid;
        private List<Tower> _towers;
        private List<Enemy> _enemies;
        private List<Projectile> _projectiles;
        private List<Effect> _effects;
        private List<Wave> _waves;
        private Wave? _currentWaveData;
        private int _enemiesSpawnedThisWave;
        private float _spawnTimer;
        private Random _random;

        private Vector2[] _simplePath = Array.Empty<Vector2>();
        private Vector2[] _loopPath = Array.Empty<Vector2>();
        private Vector2[] _crossPath = Array.Empty<Vector2>();
        private Vector2[] _mazePath = Array.Empty<Vector2>();

        public TDGameState GameState => _gameState;
        public int CurrentWave => _currentWave;
        public float Gold => _gold;
        public float Lives => _lives;
        public int Score => _score;
        public float GameSpeed => _gameSpeed;
        public TowerType SelectedTowerType => _selectedTowerType;
        public Tower? SelectedTower => _selectedTower;
        public TargetingPriority TargetingPriority => _targetingPriority;
        public GameDifficulty Difficulty => _difficulty;
        public MapLayout CurrentMapLayout => _currentMap;
        public IReadOnlyList<Tower> Towers => _towers;
        public IReadOnlyList<Enemy> Enemies => _enemies;
        public MapGrid MapGrid => _mapGrid;
        public Dictionary<AchievementType, bool> Achievements => _achievements;
        public Dictionary<string, int> Statistics => _statistics;

        public event EventHandler<int>? WaveStarted;
        public event EventHandler<int>? WaveCompleted;
        public event EventHandler<int>? GoldChanged;
        public event EventHandler<float>? LivesChanged;
        public event EventHandler<int>? ScoreChanged;
        public event EventHandler? GameOver;
        public event EventHandler? Victory;
        public event EventHandler<Tower>? TowerPlaced;
        public event EventHandler<Tower>? TowerSold;
        public event EventHandler<Tower>? TowerSelected;
        public event EventHandler<AchievementType>? AchievementUnlocked;
        public event EventHandler<string>? AchievementFailed;

        public TowerDefenseGame()
        {
            _gameState = TDGameState.Menu;
            _previousState = TDGameState.Menu;
            _currentWave = 0;
            _gold = STARTING_GOLD;
            _lives = STARTING_LIVES;
            _score = 0;
            _waveCountdown = 10f;
            _interestTimer = 5f;
            _gameSpeed = 1f;
            _isSpeedUp = false;
            _currentMap = MapLayout.Simple;
            _difficulty = GameDifficulty.Normal;
            _selectedTowerType = TowerType.Basic;
            _selectedTower = null;
            _mouseGridPos = Vector2.Zero;
            _isMouseOverGrid = false;
            _targetingPriority = TargetingPriority.First;
            _showRangeIndicator = true;
            _noDamageTimer = 0f;
            _totalEnemiesKilled = 0;
            _fastestWaveTime = float.MaxValue;
            _isSpecialWave = false;
            _comboMultiplier = 1f;
            _comboTimer = 0f;
            _consecutiveKills = 0;

            _achievements = new Dictionary<AchievementType, bool>();
            foreach (AchievementType achievement in Enum.GetValues<AchievementType>())
            {
                _achievements[achievement] = false;
            }

            _statistics = new Dictionary<string, int>
            {
                ["TotalEnemiesKilled"] = 0,
                ["TotalGoldEarned"] = 0,
                ["TotalTowersBuilt"] = 0,
                ["TotalTowersSold"] = 0,
                ["TotalWavesCompleted"] = 0,
                ["HighestWave"] = 0,
                ["HighestScore"] = 0,
                ["PerfectWaves"] = 0,
                ["BossesKilled"] = 0
            };

            _mapGrid = new MapGrid(GRID_WIDTH, GRID_HEIGHT);
            _towers = new List<Tower>();
            _enemies = new List<Enemy>();
            _projectiles = new List<Projectile>();
            _effects = new List<Effect>();
            _waves = new List<Wave>();
            _random = new Random();

            _mapOffset = new Vector2(100, 50);

            InitializePaths();
            InitializeWaves();
        }

        private void InitializePaths()
        {
            _simplePath = new Vector2[]
            {
                new Vector2(-1, 7), new Vector2(4, 7), new Vector2(4, 3), new Vector2(10, 3),
                new Vector2(10, 11), new Vector2(16, 11), new Vector2(16, 7), new Vector2(21, 7)
            };

            _loopPath = new Vector2[]
            {
                new Vector2(-1, 7), new Vector2(5, 7), new Vector2(5, 2), new Vector2(15, 2),
                new Vector2(15, 7), new Vector2(10, 7), new Vector2(10, 12), new Vector2(5, 12),
                new Vector2(5, 7), new Vector2(-1, 7)
            };

            _crossPath = new Vector2[]
            {
                new Vector2(-1, 7), new Vector2(10, 7), new Vector2(21, 7),
                new Vector2(10, 7), new Vector2(10, -1),
                new Vector2(10, 7), new Vector2(10, 15)
            };

            _mazePath = new Vector2[]
            {
                new Vector2(-1, 2), new Vector2(6, 2), new Vector2(6, 5), new Vector2(3, 5),
                new Vector2(3, 8), new Vector2(9, 8), new Vector2(9, 3), new Vector2(14, 3),
                new Vector2(14, 10), new Vector2(6, 10), new Vector2(6, 13), new Vector2(17, 13),
                new Vector2(17, 7), new Vector2(21, 7)
            };
        }

        private void InitializeWaves()
        {
            _waves = new List<Wave>
            {
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Basic, Count = 5, Delay = 1f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Basic, Count = 8, Delay = 0.9f }, new EnemyGroup { Type = EnemyType.Fast, Count = 3, Delay = 1.5f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Basic, Count = 10, Delay = 0.8f }, new EnemyGroup { Type = EnemyType.Fast, Count = 5, Delay = 1.2f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Tank, Count = 2, Delay = 3f }, new EnemyGroup { Type = EnemyType.Basic, Count = 8, Delay = 0.7f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Fast, Count = 10, Delay = 0.5f }, new EnemyGroup { Type = EnemyType.Basic, Count = 5, Delay = 0.8f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Basic, Count = 15, Delay = 0.6f }, new EnemyGroup { Type = EnemyType.Flying, Count = 3, Delay = 2f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Tank, Count = 4, Delay = 2f }, new EnemyGroup { Type = EnemyType.Fast, Count = 8, Delay = 0.6f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Healer, Count = 3, Delay = 2f }, new EnemyGroup { Type = EnemyType.Basic, Count = 10, Delay = 0.5f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Flying, Count = 8, Delay = 1f }, new EnemyGroup { Type = EnemyType.Tank, Count = 3, Delay = 2f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Boss, Count = 1, Delay = 0f }, new EnemyGroup { Type = EnemyType.Basic, Count = 15, Delay = 0.4f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Tank, Count = 6, Delay = 1.5f }, new EnemyGroup { Type = EnemyType.Fast, Count = 12, Delay = 0.4f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Flying, Count = 10, Delay = 0.8f }, new EnemyGroup { Type = EnemyType.Healer, Count = 5, Delay = 1.5f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Basic, Count = 20, Delay = 0.3f }, new EnemyGroup { Type = EnemyType.Fast, Count = 15, Delay = 0.4f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Boss, Count = 2, Delay = 5f }, new EnemyGroup { Type = EnemyType.Tank, Count = 5, Delay = 2f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Healer, Count = 8, Delay = 1f }, new EnemyGroup { Type = EnemyType.Flying, Count = 12, Delay = 0.6f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Tank, Count = 10, Delay = 1f }, new EnemyGroup { Type = EnemyType.Basic, Count = 25, Delay = 0.25f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Fast, Count = 30, Delay = 0.2f }, new EnemyGroup { Type = EnemyType.Boss, Count = 1, Delay = 3f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Flying, Count = 20, Delay = 0.4f }, new EnemyGroup { Type = EnemyType.Healer, Count = 10, Delay = 1f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Boss, Count = 3, Delay = 8f }, new EnemyGroup { Type = EnemyType.Tank, Count = 8, Delay = 1.5f } } },
                new Wave { EnemyGroups = new List<EnemyGroup> { new EnemyGroup { Type = EnemyType.Basic, Count = 50, Delay = 0.15f }, new EnemyGroup { Type = EnemyType.Fast, Count = 30, Delay = 0.2f }, new EnemyGroup { Type = EnemyType.Boss, Count = 2, Delay = 10f } } }
            };
        }

        protected override void Initialize()
        {
            base.Initialize();
            LoadMap(_currentMap);
        }

        private void LoadMap(MapLayout layout)
        {
            _mapGrid.Clear();
            _currentMap = layout;

            Vector2[] path = layout switch
            {
                MapLayout.Loop => _loopPath,
                MapLayout.Cross => _crossPath,
                MapLayout.Maze => _mazePath,
                _ => _simplePath
            };

            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector2 start = path[i];
                Vector2 end = path[i + 1];

                int x1 = (int)Math.Min(start.X, end.X);
                int x2 = (int)Math.Max(start.X, end.X);
                int y1 = (int)Math.Min(start.Y, end.Y);
                int y2 = (int)Math.Max(start.Y, end.Y);

                for (int x = x1; x <= x2; x++)
                {
                    for (int y = y1; y <= y2; y++)
                    {
                        if (x >= 0 && x < GRID_WIDTH && y >= 0 && y < GRID_HEIGHT)
                        {
                            _mapGrid.SetCellType(x, y, CellType.Path);
                        }
                    }
                }
            }

            if (layout == MapLayout.Maze)
            {
                _mapGrid.SetCellType(3, 2, CellType.Blocked);
                _mapGrid.SetCellType(3, 3, CellType.Blocked);
                _mapGrid.SetCellType(6, 8, CellType.Blocked);
                _mapGrid.SetCellType(6, 9, CellType.Blocked);
                _mapGrid.SetCellType(9, 3, CellType.Blocked);
                _mapGrid.SetCellType(9, 4, CellType.Blocked);
                _mapGrid.SetCellType(14, 10, CellType.Blocked);
                _mapGrid.SetCellType(14, 11, CellType.Blocked);
                _mapGrid.SetCellType(17, 7, CellType.Blocked);
                _mapGrid.SetCellType(17, 8, CellType.Blocked);
            }

            if (layout == MapLayout.Cross)
            {
                for (int x = 8; x <= 12; x++)
                {
                    for (int y = 5; y <= 9; y++)
                    {
                        if (x == 10 || y == 7)
                            _mapGrid.SetCellType(x, y, CellType.Path);
                        else
                            _mapGrid.SetCellType(x, y, CellType.Blocked);
                    }
                }
            }

            foreach (var enemy in _enemies)
            {
                enemy.SetPath(path);
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            float adjustedDelta = deltaTime * _gameSpeed;

            UpdateInput();
            UpdateMousePosition();

            if (_gameState == TDGameState.Building || _gameState == TDGameState.Wave)
            {
                UpdateEconomy(adjustedDelta);
                UpdateTowers(adjustedDelta);
                UpdateEnemies(adjustedDelta);
                UpdateProjectiles(adjustedDelta);
                UpdateEffects(adjustedDelta);
                UpdateCombo(adjustedDelta);

                if (_gameState == TDGameState.Wave)
                {
                    UpdateWaveSpawning(adjustedDelta);
                    _noDamageTimer += adjustedDelta;
                }

                CheckWaveComplete();
                CheckGameOver();
                CheckAchievements();
            }

            UpdateGameStateTimers(deltaTime);
        }

        private void UpdateCombo(float deltaTime)
        {
            if (_comboTimer > 0)
            {
                _comboTimer -= deltaTime;
                if (_comboTimer <= 0)
                {
                    _comboMultiplier = 1f;
                    _consecutiveKills = 0;
                }
            }
        }

        private void IncrementCombo()
        {
            _consecutiveKills++;
            _comboTimer = 3f;

            if (_consecutiveKills >= 5)
                _comboMultiplier = 1.25f;
            if (_consecutiveKills >= 10)
                _comboMultiplier = 1.5f;
            if (_consecutiveKills >= 20)
                _comboMultiplier = 2f;
        }

        private void UpdateInput()
        {
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Escape))
            {
                if (_gameState == TDGameState.Building || _gameState == TDGameState.Wave)
                    SetGameState(TDGameState.Paused);
                else if (_gameState == TDGameState.Paused)
                    SetGameState(_previousState);
            }

            if (_gameState == TDGameState.Menu && InputManager.Instance.GetKeyDown(InputManager.KeyCode.Enter))
                StartGame();

            if (_gameState == TDGameState.GameOver && InputManager.Instance.GetKeyDown(InputManager.KeyCode.R))
                RestartGame();

            if (_gameState == TDGameState.Building || _gameState == TDGameState.Wave)
            {
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Space))
                {
                    if (_gameState == TDGameState.Building)
                        StartWave();
                    else
                        SetGameState(TDGameState.Building);
                }

                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.B))
                    _showRangeIndicator = !_showRangeIndicator;

                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D1))
                    _selectedTowerType = TowerType.Basic;
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D2))
                    _selectedTowerType = TowerType.Archer;
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D3))
                    _selectedTowerType = TowerType.Sniper;
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D4))
                    _selectedTowerType = TowerType.Cannon;
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D5))
                    _selectedTowerType = TowerType.Frost;
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D6))
                    _selectedTowerType = TowerType.Poison;
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D7))
                    _selectedTowerType = TowerType.Tesla;

                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Tab))
                    CycleTargetingPriority();

                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.G))
                    ToggleGameSpeed();

                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.U))
                {
                    if (_selectedTower != null && _selectedTower.CanUpgrade)
                        UpgradeTower(_selectedTower);
                }

                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.S))
                {
                    if (_selectedTower != null)
                        SellTower(_selectedTower);
                }

                if (InputManager.Instance.GetMouseButtonDown(0))
                    HandleLeftClick();

                if (InputManager.Instance.GetMouseButtonDown(1))
                    HandleRightClick();
            }
        }

        private void UpdateMousePosition()
        {
            Vector2 mousePos = InputManager.Instance.MousePosition;
            _mouseGridPos = new Vector2(
                (int)((mousePos.X - _mapOffset.X) / CELL_SIZE),
                (int)((mousePos.Y - _mapOffset.Y) / CELL_SIZE)
            );

            _isMouseOverGrid = _mouseGridPos.X >= 0 && _mouseGridPos.X < GRID_WIDTH &&
                              _mouseGridPos.Y >= 0 && _mouseGridPos.Y < GRID_HEIGHT;
        }

        private void UpdateEconomy(float deltaTime)
        {
            _interestTimer -= deltaTime;
            if (_interestTimer <= 0)
            {
                _interestTimer = 5f;
                if (_gold >= MIN_GOLD_FOR_INTEREST)
                {
                    float interest = _gold * INTEREST_RATE;
                    _gold += interest;
                    GoldChanged?.Invoke(this, (int)_gold);
                }
            }
        }

        private void UpdateTowers(float deltaTime)
        {
            foreach (var tower in _towers)
            {
                tower.Update(deltaTime, _enemies, _projectiles, _targetingPriority);
            }
        }

        private void UpdateEnemies(float deltaTime)
        {
            for (int i = _enemies.Count - 1; i >= 0; i--)
            {
                var enemy = _enemies[i];
                enemy.Update(deltaTime);

                if (enemy.IsDead)
                {
                    if (enemy.IsReachedEnd)
                    {
                        _lives -= enemy.Damage;
                        LivesChanged?.Invoke(this, _lives);
                        _noDamageTimer = 0f;
                    }
                    else
                    {
                        float reward = enemy.GoldReward * _comboMultiplier;
                        _gold += reward;
                        _score += enemy.ScoreValue * (int)_comboMultiplier;
                        ScoreChanged?.Invoke(this, _score);
                        GoldChanged?.Invoke(this, (int)_gold);
                        CreateDeathEffect(enemy.Position, enemy.Type);
                        IncrementCombo();

                        _totalEnemiesKilled++;
                        _statistics["TotalEnemiesKilled"]++;
                        _statistics["TotalGoldEarned"] += (int)reward;

                        if (enemy.Type == EnemyType.Boss)
                        {
                            _statistics["BossesKilled"]++;
                            UnlockAchievement(AchievementType.BossSlayer);
                        }

                        if (_totalEnemiesKilled == 1)
                        {
                            UnlockAchievement(AchievementType.FirstBlood);
                        }
                    }

                    CreateDeathParticles(enemy.Position, enemy.Color);
                    _enemies.RemoveAt(i);
                }
            }

            foreach (var enemy in _enemies)
            {
                if (enemy.Type == EnemyType.Healer)
                {
                    foreach (var other in _enemies)
                    {
                        if (other != enemy && !other.IsDead)
                        {
                            float dist = (enemy.Position - other.Position).Length();
                            if (dist < enemy.HealRadius)
                            {
                                other.Heal(enemy.HealRate * deltaTime);
                            }
                        }
                    }
                }
            }
        }

        private void UpdateProjectiles(float deltaTime)
        {
            for (int i = _projectiles.Count - 1; i >= 0; i--)
            {
                var proj = _projectiles[i];
                proj.Update(deltaTime);

                if (proj.IsExpired)
                {
                    _projectiles.RemoveAt(i);
                    continue;
                }

                if (proj.HasHit)
                {
                    HandleProjectileHit(proj);
                    _projectiles.RemoveAt(i);
                }
            }
        }

        private void HandleProjectileHit(Projectile proj)
        {
            if (proj.AreaDamage > 0)
            {
                CreateExplosionEffect(proj.Position, proj.AreaRadius);
                foreach (var enemy in _enemies)
                {
                    float dist = (enemy.Position - proj.Position).Length();
                    if (dist < proj.AreaRadius)
                    {
                        float damageReduction = 1f - (dist / proj.AreaRadius) * 0.5f;
                        enemy.TakeDamage(proj.Damage * damageReduction, proj.EffectType);
                    }
                }
            }
            else
            {
                if (proj.Target != null && !proj.Target.IsDead)
                {
                    proj.Target.TakeDamage(proj.Damage, proj.EffectType);
                    CreateHitEffect(proj.Position, proj.Color);

                    if (proj.EffectType == EffectType.ChainLightning)
                    {
                        ChainLightning(proj.Target, proj.Damage * 0.5f, proj.ChainCount, proj.Position);
                    }
                }
            }
        }

        private void ChainLightning(Enemy initialTarget, float damage, int remainingChains, Vector2 fromPosition)
        {
            if (remainingChains <= 0) return;

            Enemy? closest = null;
            float closestDist = 100f;

            foreach (var enemy in _enemies)
            {
                if (enemy == initialTarget || enemy.IsDead) continue;
                float dist = (enemy.Position - initialTarget.Position).Length();
                if (dist < closestDist && dist < 150f)
                {
                    closestDist = dist;
                    closest = enemy;
                }
            }

            if (closest != null)
            {
                CreateLightningEffect(fromPosition, closest.Position);
                closest.TakeDamage(damage, EffectType.None);
                ChainLightning(closest, damage * 0.6f, remainingChains - 1, closest.Position);
            }
        }

        private void UpdateEffects(float deltaTime)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var effect = _effects[i];
                effect.Update(deltaTime);
                if (effect.IsExpired)
                {
                    _effects.RemoveAt(i);
                }
            }
        }

        private void UpdateWaveSpawning(float deltaTime)
        {
            if (_currentWaveData == null) return;

            if (_enemiesSpawnedThisWave < _currentWaveData.TotalEnemies)
            {
                _spawnTimer -= deltaTime;
                if (_spawnTimer <= 0)
                {
                    SpawnNextEnemy();
                }
            }
        }

        private void SpawnNextEnemy()
        {
            if (_currentWaveData == null) return;

            int spawnedInGroup = _enemiesSpawnedThisWave;
            int groupIndex = 0;
            int countInPreviousGroups = 0;

            foreach (var group in _currentWaveData.EnemyGroups)
            {
                if (spawnedInGroup < countInPreviousGroups + group.Count)
                {
                    Vector2 spawnPos = GetPathStartPosition();
                    var enemy = CreateEnemy(group.Type, spawnPos, _currentWave);
                    enemy.PathIndex = 0;
                    enemy.PathProgress = 0f;
                    enemy.SetPath(GetCurrentPath());
                    _enemies.Add(enemy);
                    _spawnTimer = group.Delay;
                    _enemiesSpawnedThisWave++;
                    return;
                }
                countInPreviousGroups += group.Count;
                groupIndex++;
            }
        }

        private Vector2 GetPathStartPosition()
        {
            Vector2[] path = GetCurrentPath();
            if (path.Length < 2) return _mapOffset + new Vector2(0, GRID_HEIGHT / 2 * CELL_SIZE);

            Vector2 start = path[0];
            return _mapOffset + new Vector2(start.X * CELL_SIZE + CELL_SIZE / 2, start.Y * CELL_SIZE + CELL_SIZE / 2);
        }

        private Vector2[] GetCurrentPath()
        {
            return _currentMap switch
            {
                MapLayout.Loop => _loopPath,
                MapLayout.Cross => _crossPath,
                MapLayout.Maze => _mazePath,
                _ => _simplePath
            };
        }

        private Enemy CreateEnemy(EnemyType type, Vector2 position, int wave)
        {
            float waveMultiplier = 1f + (wave - 1) * 0.15f;
            float difficultyMultiplier = _difficulty switch
            {
                GameDifficulty.Easy => 0.75f,
                GameDifficulty.Hard => 1.5f,
                GameDifficulty.Insane => 2f,
                _ => 1f
            };

            float baseHealth = 100f;
            float baseSpeed = 50f;
            float baseReward = 10f;

            switch (type)
            {
                case EnemyType.Basic:
                    return new Enemy(position, type, baseHealth * 1f * waveMultiplier * difficultyMultiplier, baseSpeed * 1f, baseReward * 1f, 1, Color.Red);
                case EnemyType.Fast:
                    return new Enemy(position, type, baseHealth * 0.5f * waveMultiplier * difficultyMultiplier, baseSpeed * 2f, baseReward * 0.8f, 1, Color.Green);
                case EnemyType.Tank:
                    return new Enemy(position, type, baseHealth * 5f * waveMultiplier * difficultyMultiplier, baseSpeed * 0.5f, baseReward * 3f, 3, Color.Brown);
                case EnemyType.Flying:
                    return new Enemy(position, type, baseHealth * 0.8f * waveMultiplier * difficultyMultiplier, baseSpeed * 1.5f, baseReward * 2f, 1, Color.Cyan);
                case EnemyType.Boss:
                    return new Enemy(position, type, baseHealth * 20f * waveMultiplier * difficultyMultiplier, baseSpeed * 0.3f, baseReward * 20f, 5, Color.Purple);
                case EnemyType.Healer:
                    return new Enemy(position, type, baseHealth * 2f * waveMultiplier * difficultyMultiplier, baseSpeed * 0.8f, baseReward * 2.5f, 1, Color.Lime);
                default:
                    return new Enemy(position, type, baseHealth * waveMultiplier * difficultyMultiplier, baseSpeed, baseReward, 1, Color.Red);
            }
        }

        private void UpdateGameStateTimers(float deltaTime)
        {
            if (_gameState == TDGameState.Building && _currentWave > 0)
            {
                _waveCountdown -= deltaTime;
                if (_waveCountdown <= 0)
                {
                    StartWave();
                }
            }
        }

        private void CheckWaveComplete()
        {
            if (_gameState != TDGameState.Wave) return;

            if (_currentWaveData != null && _enemiesSpawnedThisWave >= _currentWaveData.TotalEnemies && _enemies.Count == 0)
            {
                _gold += WAVE_BONUS + _currentWave * 10;
                GoldChanged?.Invoke(this, (int)_gold);

                WaveCompleted?.Invoke(this, _currentWave);
                _statistics["TotalWavesCompleted"]++;
                _statistics["HighestWave"] = Math.Max(_statistics["HighestWave"], _currentWave);

                if (_noDamageTimer > 10f)
                {
                    _statistics["PerfectWaves"]++;
                    UnlockAchievement(AchievementType.NoDamage);
                }

                UnlockAchievement(AchievementType.WaveComplete);

                if (_currentWave >= _waves.Count)
                {
                    SetGameState(TDGameState.Victory);
                    Victory?.Invoke(this, EventArgs.Empty);
                    UnlockAchievement(AchievementType.Wave10);
                }
                else
                {
                    _currentWave++;
                    _waveCountdown = 15f;
                    SetGameState(TDGameState.Building);
                }
            }
        }

        private void CheckGameOver()
        {
            if (_lives <= 0)
            {
                SetGameState(TDGameState.GameOver);
                GameOver?.Invoke(this, EventArgs.Empty);
                _statistics["HighestScore"] = Math.Max(_statistics["HighestScore"], _score);
            }
        }

        private void CheckAchievements()
        {
            if (_currentWave >= 10)
                UnlockAchievement(AchievementType.Wave10);

            if (_score >= 10000)
                UnlockAchievement(AchievementType.Millionaire);

            if (_towers.Count >= 10)
                UnlockAchievement(AchievementType.TowerMaster);

            if (_gameSpeed >= 2f)
                UnlockAchievement(AchievementType.SpeedDemon);
        }

        private void UnlockAchievement(AchievementType achievement)
        {
            if (!_achievements[achievement])
            {
                _achievements[achievement] = true;
                AchievementUnlocked?.Invoke(this, achievement);
            }
        }

        private void HandleLeftClick()
        {
            if (!_isMouseOverGrid) return;

            int x = (int)_mouseGridPos.X;
            int y = (int)_mouseGridPos.Y;

            var existingTower = GetTowerAt(x, y);
            if (existingTower != null)
            {
                SelectTower(existingTower);
                return;
            }

            if (CanPlaceTower(x, y))
            {
                TryPlaceTower(x, y);
            }
        }

        private void HandleRightClick()
        {
            if (_selectedTower != null)
            {
                DeselectTower();
            }
            else
            {
                CancelPlacement();
            }
        }

        public bool CanPlaceTower(int gridX, int gridY)
        {
            if (gridX < 0 || gridX >= GRID_WIDTH || gridY < 0 || gridY >= GRID_HEIGHT)
                return false;

            if (_mapGrid.GetCellType(gridX, gridY) != CellType.Buildable)
                return false;

            if (GetTowerAt(gridX, gridY) != null)
                return false;

            return true;
        }

        public bool TryPlaceTower(int gridX, int gridY)
        {
            TowerStats stats = GetTowerStats(_selectedTowerType);

            if (_gold < stats.Cost)
                return false;

            if (!CanPlaceTower(gridX, gridY))
                return false;

            _gold -= stats.Cost;
            GoldChanged?.Invoke(this, (int)_gold);

            var tower = new Tower(_selectedTowerType, gridX, gridY, stats);
            tower.Position = _mapOffset + new Vector2(gridX * CELL_SIZE + CELL_SIZE / 2, gridY * CELL_SIZE + CELL_SIZE / 2);
            _towers.Add(tower);
            _mapGrid.SetCellType(gridX, gridY, CellType.Occupied);

            _statistics["TotalTowersBuilt"]++;
            TowerPlaced?.Invoke(this, tower);
            return true;
        }

        public bool SellTower(Tower tower)
        {
            if (!_towers.Contains(tower))
                return false;

            float sellValue = tower.TotalCost * 0.7f;
            _gold += sellValue;
            GoldChanged?.Invoke(this, (int)_gold);

            _mapGrid.SetCellType(tower.GridX, tower.GridY, CellType.Buildable);
            _towers.Remove(tower);

            _statistics["TotalTowersSold"]++;
            TowerSold?.Invoke(this, tower);
            return true;
        }

        public bool UpgradeTower(Tower tower)
        {
            if (!tower.CanUpgrade)
                return false;

            TowerStats upgradeCost = GetUpgradeStats(tower.Type, tower.Level + 1);
            if (_gold < upgradeCost.Cost)
                return false;

            _gold -= upgradeCost.Cost;
            GoldChanged?.Invoke(this, (int)_gold);

            tower.Upgrade(upgradeCost);
            return true;
        }

        public Tower? GetTowerAt(int gridX, int gridY)
        {
            return _towers.FirstOrDefault(t => t.GridX == gridX && t.GridY == gridY);
        }

        public void SelectTower(Tower tower)
        {
            _selectedTower = tower;
            TowerSelected?.Invoke(this, tower);
        }

        public void DeselectTower()
        {
            _selectedTower = null;
        }

        public void CancelPlacement()
        {
        }

        private void CycleTargetingPriority()
        {
            _targetingPriority = (TargetingPriority)(((int)_targetingPriority + 1) % 5);
        }

        private void ToggleGameSpeed()
        {
            _isSpeedUp = !_isSpeedUp;
            _gameSpeed = _isSpeedUp ? 2f : 1f;
        }

        private void SetGameState(TDGameState newState)
        {
            _previousState = _gameState;
            _gameState = newState;
        }

        public void StartGame()
        {
            _currentWave = 1;
            _gold = STARTING_GOLD;
            _lives = STARTING_LIVES;
            _score = 0;
            _waveCountdown = 10f;
            _interestTimer = 5f;
            _gameSpeed = 1f;
            _isSpeedUp = false;
            _noDamageTimer = 0f;
            _totalEnemiesKilled = 0;
            _comboMultiplier = 1f;
            _comboTimer = 0f;
            _consecutiveKills = 0;

            _towers.Clear();
            _enemies.Clear();
            _projectiles.Clear();
            _effects.Clear();
            _selectedTower = null;

            foreach (AchievementType achievement in Enum.GetValues<AchievementType>())
            {
                _achievements[achievement] = false;
            }

            LoadMap(_currentMap);

            GoldChanged?.Invoke(this, (int)_gold);
            LivesChanged?.Invoke(this, _lives);
            ScoreChanged?.Invoke(this, _score);
            WaveStarted?.Invoke(this, _currentWave);

            SetGameState(TDGameState.Building);
        }

        public void RestartGame()
        {
            StartGame();
        }

        public void StartWave()
        {
            if (_currentWave > _waves.Count)
                return;

            _currentWaveData = _waves[_currentWave - 1];
            _enemiesSpawnedThisWave = 0;
            _spawnTimer = 0f;
            _noDamageTimer = 0f;

            WaveStarted?.Invoke(this, _currentWave);
            SetGameState(TDGameState.Wave);
        }

        public void SetDifficulty(GameDifficulty difficulty)
        {
            _difficulty = difficulty;
        }

        public void SetMap(MapLayout layout)
        {
            if (_gameState == TDGameState.Menu)
            {
                _currentMap = layout;
                LoadMap(layout);
            }
        }

        private TowerStats GetTowerStats(TowerType type)
        {
            float difficultyCostMultiplier = _difficulty switch
            {
                GameDifficulty.Easy => 0.8f,
                GameDifficulty.Hard => 1.25f,
                GameDifficulty.Insane => 1.5f,
                _ => 1f
            };

            return type switch
            {
                TowerType.Basic => new TowerStats { Cost = (int)(50 * difficultyCostMultiplier), Damage = 10, Range = 100, FireRate = 1f, SplashRadius = 0 },
                TowerType.Archer => new TowerStats { Cost = (int)(75 * difficultyCostMultiplier), Damage = 15, Range = 120, FireRate = 0.8f, SplashRadius = 0 },
                TowerType.Sniper => new TowerStats { Cost = (int)(150 * difficultyCostMultiplier), Damage = 50, Range = 250, FireRate = 0.3f, SplashRadius = 0 },
                TowerType.Cannon => new TowerStats { Cost = (int)(125 * difficultyCostMultiplier), Damage = 30, Range = 100, FireRate = 0.5f, SplashRadius = 60 },
                TowerType.Frost => new TowerStats { Cost = (int)(100 * difficultyCostMultiplier), Damage = 5, Range = 110, FireRate = 0.7f, SplashRadius = 0, SlowEffect = 0.5f },
                TowerType.Poison => new TowerStats { Cost = (int)(100 * difficultyCostMultiplier), Damage = 8, Range = 100, FireRate = 0.6f, SplashRadius = 0, PoisonDamage = 5, PoisonDuration = 3f },
                TowerType.Tesla => new TowerStats { Cost = (int)(175 * difficultyCostMultiplier), Damage = 25, Range = 130, FireRate = 0.4f, SplashRadius = 0, ChainCount = 3 },
                _ => new TowerStats { Cost = (int)(50 * difficultyCostMultiplier), Damage = 10, Range = 100, FireRate = 1f, SplashRadius = 0 }
            };
        }

        private TowerStats GetUpgradeStats(TowerType type, int level)
        {
            TowerStats baseStats = GetTowerStats(type);
            float multiplier = 1f + level * 0.5f;

            return new TowerStats
            {
                Cost = (int)(baseStats.Cost * 0.75f * level),
                Damage = baseStats.Damage * multiplier,
                Range = baseStats.Range * (1f + level * 0.1f),
                FireRate = baseStats.FireRate * (1f + level * 0.15f),
                SplashRadius = baseStats.SplashRadius,
                SlowEffect = baseStats.SlowEffect,
                PoisonDamage = baseStats.PoisonDamage * multiplier,
                PoisonDuration = baseStats.PoisonDuration,
                ChainCount = baseStats.ChainCount + level - 1
            };
        }

        private void CreateHitEffect(Vector2 position, Color color)
        {
            _effects.Add(new Effect(position, EffectType.Hit, color, 0.3f));
        }

        private void CreateDeathEffect(Vector2 position, EnemyType type)
        {
            Color color = type switch
            {
                EnemyType.Basic => Color.Red,
                EnemyType.Fast => Color.Green,
                EnemyType.Tank => Color.Brown,
                EnemyType.Flying => Color.Cyan,
                EnemyType.Boss => Color.Purple,
                EnemyType.Healer => Color.Lime,
                _ => Color.Gray
            };

            _effects.Add(new Effect(position, EffectType.Explosion, color, 0.5f));
        }

        private void CreateExplosionEffect(Vector2 position, float radius)
        {
            _effects.Add(new Effect(position, EffectType.Explosion, Color.Orange, 0.4f) { Radius = radius });
            _effects.Add(new Effect(position, EffectType.Shockwave, Color.Yellow, 0.3f) { Radius = radius });
        }

        private void CreateDeathParticles(Vector2 position, Color color)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = (float)_random.NextDouble() * MathHelper.TwoPi;
                float speed = 50f + (float)_random.NextDouble() * 50f;
                Vector2 velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed);
                _effects.Add(new ParticleEffect(position, velocity, color, 0.5f));
            }
        }

        private void CreateLightningEffect(Vector2 from, Vector2 to)
        {
            _effects.Add(new LightningEffect(from, to, Color.Cyan, 0.2f));
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            DrawBackground(context);
            DrawMap(context);
            DrawTowers(context);
            DrawEnemies(context);
            DrawProjectiles(context);
            DrawEffects(context);
            DrawUI(context);

            if (_gameState == TDGameState.Menu)
                DrawMainMenu(context);

            if (_gameState == TDGameState.Paused)
                DrawPauseMenu(context);

            if (_gameState == TDGameState.GameOver)
                DrawGameOverScreen(context);

            if (_gameState == TDGameState.Victory)
                DrawVictoryScreen(context);
        }

        private void DrawBackground(DrawingContext context)
        {
            var bgBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 35, 45));
            context.DrawRectangle(bgBrush, null, new Rect(0, 0, 1920, 1080));
        }

        private void DrawMap(DrawingContext context)
        {
            var buildableBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(50, 55, 65));
            var pathBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 70, 60));
            var blockedBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(35, 40, 50));
            var gridLinePen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(40, 100, 100, 120)), 1);

            for (int x = 0; x < GRID_WIDTH; x++)
            {
                for (int y = 0; y < GRID_HEIGHT; y++)
                {
                    Vector2 cellPos = _mapOffset + new Vector2(x * CELL_SIZE, y * CELL_SIZE);
                    CellType cellType = _mapGrid.GetCellType(x, y);

                    System.Windows.Media.Brush brush = cellType switch
                    {
                        CellType.Path => pathBrush,
                        CellType.Blocked => blockedBrush,
                        _ => buildableBrush
                    };

                    context.DrawRectangle(brush, gridLinePen, new Rect(cellPos.X, cellPos.Y, CELL_SIZE, CELL_SIZE));
                }
            }

            if (_isMouseOverGrid && (_gameState == TDGameState.Building || _gameState == TDGameState.Wave))
            {
                int x = (int)_mouseGridPos.X;
                int y = (int)_mouseGridPos.Y;
                Vector2 cellPos = _mapOffset + new Vector2(x * CELL_SIZE, y * CELL_SIZE);

                if (CanPlaceTower(x, y))
                {
                    var hoverBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 100, 255, 100));
                    context.DrawRectangle(hoverBrush, new Pen(System.Windows.Media.Brushes.Lime, 2), new Rect(cellPos.X, cellPos.Y, CELL_SIZE, CELL_SIZE));

                    if (_showRangeIndicator)
                    {
                        TowerStats stats = GetTowerStats(_selectedTowerType);
                        DrawRangeIndicator(context, cellPos + new Vector2(CELL_SIZE / 2), stats.Range, Color.Green);
                    }
                }
                else
                {
                    var hoverBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 255, 100, 100));
                    context.DrawRectangle(hoverBrush, new Pen(System.Windows.Media.Brushes.Red, 2), new Rect(cellPos.X, cellPos.Y, CELL_SIZE, CELL_SIZE));
                }
            }

            DrawPathArrows(context);
        }

        private void DrawPathArrows(DrawingContext context)
        {
            Vector2[] path = GetCurrentPath();
            var arrowPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 200, 180, 140)), 2);

            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector2 start = path[i];
                Vector2 end = path[i + 1];

                Vector2 startPos = _mapOffset + new Vector2(start.X * CELL_SIZE + CELL_SIZE / 2, start.Y * CELL_SIZE + CELL_SIZE / 2);
                Vector2 endPos = _mapOffset + new Vector2(end.X * CELL_SIZE + CELL_SIZE / 2, end.Y * CELL_SIZE + CELL_SIZE / 2);

                Vector2 direction = (endPos - startPos).Normalized();
                float arrowSize = 8f;

                Vector2 perpendicular = direction.Perpendicular();

                Vector2 midPoint = (startPos + endPos) / 2;
                Vector2 arrowLeft = midPoint + perpendicular * arrowSize / 2;
                Vector2 arrowRight = midPoint - perpendicular * arrowSize / 2;
                Vector2 arrowTip = midPoint + direction * arrowSize;

                context.DrawLine(arrowPen, arrowLeft, arrowTip);
                context.DrawLine(arrowPen, arrowRight, arrowTip);
            }
        }

        private void DrawTowers(DrawingContext context)
        {
            foreach (var tower in _towers)
            {
                bool isSelected = tower == _selectedTower;

                if (_showRangeIndicator || isSelected)
                {
                    DrawRangeIndicator(context, tower.Position, tower.Range, isSelected ? Color.Blue : Color.Gray);
                }

                DrawTowerVisual(context, tower);

                if (tower.Type == TowerType.Frost || tower.Type == TowerType.Tesla || tower.Type == TowerType.Poison)
                {
                    DrawTowerAura(context, tower);
                }
            }
        }

        private void DrawTowerVisual(DrawingContext context, Tower tower)
        {
            Vector2 pos = tower.Position;
            float size = CELL_SIZE * 0.7f;

            Color towerColor = tower.Type switch
            {
                TowerType.Basic => Color.Gray,
                TowerType.Archer => Color.Green,
                TowerType.Sniper => Color.Navy,
                TowerType.Cannon => Color.Brown,
                TowerType.Frost => Color.Cyan,
                TowerType.Poison => Color.Purple,
                TowerType.Tesla => Color.Yellow,
                _ => Color.Gray
            };

            var bodyBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(towerColor.R * 255),
                (byte)(towerColor.G * 255),
                (byte)(towerColor.B * 255)));

            float levelIndicator = tower.Level * 3f;
            context.DrawRectangle(bodyBrush, new Pen(System.Windows.Media.Brushes.White, 1),
                new Rect(pos.X - size / 2, pos.Y - size / 2 + levelIndicator, size, size - levelIndicator));

            double angle = tower.CurrentAngle * Math.PI / 180;
            Vector2 barrelDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
            Vector2 barrelStart = pos + barrelDir * (size * 0.2f);
            Vector2 barrelEnd = pos + barrelDir * (size * 0.5f);

            var barrelPen = new Pen(System.Windows.Media.Brushes.DarkGray, tower.Type == TowerType.Cannon ? 6 : 3);
            context.DrawLine(barrelPen, new Point(barrelStart.X, barrelStart.Y), new Point(barrelEnd.X, barrelEnd.Y));

            if (tower.Type == TowerType.Tesla)
            {
                var glowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 255, 255, 100));
                context.DrawEllipse(glowBrush, null, new Point(pos.X, pos.Y), size * 0.3f, size * 0.3f);
            }

            if (tower == _selectedTower)
            {
                var selectionPen = new Pen(System.Windows.Media.Brushes.Yellow, 2);
                context.DrawRectangle(null, selectionPen, new Rect(pos.X - size / 2 - 3, pos.Y - size / 2 - 3, size + 6, size + 6));
            }
        }

        private void DrawTowerAura(DrawingContext context, Tower tower)
        {
            float pulse = (float)(Math.Sin(_totalTime * 3) * 0.2 + 0.8);
            float auraRadius = tower.Range * 0.3f * pulse;

            Color auraColor = tower.Type switch
            {
                TowerType.Frost => Color.Cyan,
                TowerType.Poison => Color.Purple,
                _ => Color.Yellow
            };

            var auraBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30,
                (byte)(auraColor.R * 255),
                (byte)(auraColor.G * 255),
                (byte)(auraColor.B * 255)));

            context.DrawEllipse(auraBrush, null, new Point(tower.Position.X, tower.Position.Y), auraRadius, auraRadius);
        }

        private void DrawRangeIndicator(DrawingContext context, Vector2 center, float range, Color color)
        {
            var rangeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30,
                (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)));
            var rangePen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(100,
                (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255))), 1);

            context.DrawEllipse(rangeBrush, rangePen, new Point(center.X, center.Y), range, range);
        }

        private void DrawEnemies(DrawingContext context)
        {
            foreach (var enemy in _enemies)
            {
                DrawEnemyVisual(context, enemy);
                DrawEnemyHealthBar(context, enemy);

                if (enemy.SlowEffect > 0)
                {
                    DrawSlowEffect(context, enemy);
                }

                if (enemy.PoisonDamage > 0)
                {
                    DrawPoisonEffect(context, enemy);
                }
            }
        }

        private void DrawEnemyVisual(DrawingContext context, Enemy enemy)
        {
            float size = enemy.Radius * 2;

            var bodyBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(enemy.Color.R * 255),
                (byte)(enemy.Color.G * 255),
                (byte)(enemy.Color.B * 255)));

            if (enemy.Type == EnemyType.Flying)
            {
                float wingAngle = (float)(Math.Sin(_totalTime * 10) * 0.3);
                context.PushTransform(new RotateTransform(wingAngle * 180 / Math.PI, enemy.Position.X, enemy.Position.Y));
                context.DrawEllipse(bodyBrush, new Pen(System.Windows.Media.Brushes.White, 1),
                    new Point(enemy.Position.X, enemy.Position.Y), size * 0.6f, size);
                context.Pop();
            }
            else if (enemy.Type == EnemyType.Boss)
            {
                context.DrawRectangle(bodyBrush, new Pen(System.Windows.Media.Brushes.Purple, 3),
                    new Rect(enemy.Position.X - size, enemy.Position.Y - size, size * 2, size * 2));

                for (int i = 0; i < 4; i++)
                {
                    float angle = (float)i * MathHelper.PiOver2 + _totalTime * 2;
                    Vector2 hornPos = enemy.Position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * size * 0.8f;
                    context.DrawEllipse(System.Windows.Media.Brushes.Purple, null,
                        new Point(hornPos.X, hornPos.Y), 5, 5);
                }
            }
            else if (enemy.Type == EnemyType.Healer)
            {
                context.DrawEllipse(bodyBrush, new Pen(System.Windows.Media.Brushes.Lime, 2),
                    new Point(enemy.Position.X, enemy.Position.Y), size, size);

                var healBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 0, 255, 0));
                context.DrawEllipse(healBrush, null, new Point(enemy.Position.X, enemy.Position.Y), enemy.HealRadius, enemy.HealRadius);
            }
            else
            {
                context.DrawEllipse(bodyBrush, new Pen(System.Windows.Media.Brushes.White, 1),
                    new Point(enemy.Position.X, enemy.Position.Y), size, size);
            }
        }

        private void DrawEnemyHealthBar(DrawingContext context, Enemy enemy)
        {
            float barWidth = 30;
            float barHeight = 4;
            float barY = enemy.Position.Y - enemy.Radius - 10;

            context.DrawRectangle(System.Windows.Media.Brushes.DarkGray, null,
                new Rect(enemy.Position.X - barWidth / 2, barY, barWidth, barHeight));

            float healthPercent = enemy.Health / enemy.MaxHealth;
            Color healthColor = healthPercent > 0.5 ? Color.Green : (healthPercent > 0.25 ? Color.Yellow : Color.Red);

            var healthBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(healthColor.R * 255), (byte)(healthColor.G * 255), (byte)(healthColor.B * 255)));

            context.DrawRectangle(healthBrush, null,
                new Rect(enemy.Position.X - barWidth / 2, barY, barWidth * healthPercent, barHeight));
        }

        private void DrawSlowEffect(DrawingContext context, Enemy enemy)
        {
            float pulse = (float)(Math.Sin(_totalTime * 8) * 0.3 + 0.7);
            var slowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(50 * pulse), 100, 200, 255));

            context.DrawEllipse(slowBrush, null,
                new Point(enemy.Position.X, enemy.Position.Y), enemy.Radius * 1.5f, enemy.Radius * 1.5f);
        }

        private void DrawPoisonEffect(DrawingContext context, Enemy enemy)
        {
            var poisonBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 128, 0, 128));
            for (int i = 0; i < 3; i++)
            {
                float angle = _totalTime * 2 + i * MathHelper.TwoPi / 3;
                Vector2 bubblePos = enemy.Position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * enemy.Radius * 0.8f;
                context.DrawEllipse(poisonBrush, null, new Point(bubblePos.X, bubblePos.Y), 4, 4);
            }
        }

        private void DrawProjectiles(DrawingContext context)
        {
            foreach (var proj in _projectiles)
            {
                DrawProjectileVisual(context, proj);
            }
        }

        private void DrawProjectileVisual(DrawingContext context, Projectile proj)
        {
            Color color = proj.Color;
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)));

            if (proj.Type == ProjectileType.Arrow)
            {
                double angle = Math.Atan2(proj.Velocity.Y, proj.Velocity.X);
                context.PushTransform(new RotateTransform(angle * 180 / Math.PI, proj.Position.X, proj.Position.Y));
                context.DrawLine(new Pen(brush, 2), new Point(proj.Position.X - 8, proj.Position.Y), new Point(proj.Position.X + 8, proj.Position.Y));
                context.Pop();
            }
            else if (proj.Type == ProjectileType.Bullet)
            {
                context.DrawEllipse(brush, null, new Point(proj.Position.X, proj.Position.Y), 4, 4);
            }
            else if (proj.Type == ProjectileType.Cannonball)
            {
                context.DrawEllipse(System.Windows.Media.Brushes.DarkGray, null, new Point(proj.Position.X, proj.Position.Y), 8, 8);
            }
            else if (proj.Type == ProjectileType.Lightning)
            {
                var lightningPen = new Pen(brush, 3);
                Vector2 end = proj.Position + proj.Velocity.Normalized() * 10;
                context.DrawLine(lightningPen, new Point(proj.Position.X, proj.Position.Y), new Point(end.X, end.Y));
            }
            else if (proj.Type == ProjectileType.Frost)
            {
                context.DrawEllipse(brush, null, new Point(proj.Position.X, proj.Position.Y), 5, 5);
                var trailBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 100, 200, 255));
                context.DrawEllipse(trailBrush, null, new Point(proj.Position.X - proj.Velocity.X * 0.05f, proj.Position.Y - proj.Velocity.Y * 0.05f), 3, 3);
            }
            else if (proj.Type == ProjectileType.Poison)
            {
                context.DrawEllipse(brush, null, new Point(proj.Position.X, proj.Position.Y), 6, 6);
            }
        }

        private void DrawEffects(DrawingContext context)
        {
            foreach (var effect in _effects)
            {
                DrawEffect(context, effect);
            }
        }

        private void DrawEffect(DrawingContext context, Effect effect)
        {
            if (effect is ParticleEffect particle)
            {
                DrawParticleEffect(context, particle);
            }
            else if (effect is LightningEffect lightning)
            {
                DrawLightningEffect(context, lightning);
            }
            else
            {
                DrawBasicEffect(context, effect);
            }
        }

        private void DrawParticleEffect(DrawingContext context, ParticleEffect particle)
        {
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(particle.Color.R * 255),
                (byte)(particle.Color.G * 255),
                (byte)(particle.Color.B * 255)));

            float alpha = particle.Lifetime / particle.MaxLifetime;

            brush.Opacity = alpha;
            context.DrawEllipse(brush, null, new Point(particle.Position.X, particle.Position.Y), 3, 3);
        }

        private void DrawLightningEffect(DrawingContext context, LightningEffect lightning)
        {
            var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(lightning.Color.R * 255),
                (byte)(lightning.Color.G * 255),
                (byte)(lightning.Color.B * 255))), 2);

            var points = new List<Point>();
            Vector2 direction = lightning.EndPoint - lightning.StartPoint;
            float distance = direction.Length();
            direction.Normalize();

            int segments = 5;
            for (int i = 0; i <= segments; i++)
            {
                float t = i / (float)segments;
                Vector2 point = lightning.StartPoint + direction * (distance * t);

                if (i > 0 && i < segments)
                {
                    Vector2 perpendicular = direction.Perpendicular();
                    float offset = ((float)_random.NextDouble() - 0.5f) * 20;
                    point = point + perpendicular * offset;
                }

                points.Add(new Point(point.X, point.Y));
            }

            for (int i = 0; i < points.Count - 1; i++)
            {
                context.DrawLine(pen, points[i], points[i + 1]);
            }
        }

        private void DrawBasicEffect(DrawingContext context, Effect effect)
        {
            float progress = 1f - (effect.Lifetime / effect.MaxLifetime);

            if (effect.Type == EffectType.Explosion)
            {
                float radius = effect.Radius * progress;
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(255 * (1 - progress)),
                    (byte)(effect.Color.R * 255), (byte)(effect.Color.G * 255), (byte)(effect.Color.B * 255)));

                context.DrawEllipse(brush, null, new Point(effect.Position.X, effect.Position.Y), radius, radius);
            }
            else if (effect.Type == EffectType.Shockwave)
            {
                float radius = effect.Radius * (0.5f + progress * 0.5f);
                float alpha = 1f - progress;
                var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(255 * alpha),
                    (byte)(effect.Color.R * 255), (byte)(effect.Color.G * 255), (byte)(effect.Color.B * 255))), 2);

                context.DrawEllipse(null, pen, new Point(effect.Position.X, effect.Position.Y), radius, radius);
            }
            else if (effect.Type == EffectType.Hit)
            {
                for (int i = 0; i < 4; i++)
                {
                    float angle = (float)i * MathHelper.PiOver2 + progress * MathHelper.PiOver2;
                    float distance = progress * 20;
                    Vector2 sparkPos = effect.Position + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * distance;

                    var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                        (byte)(effect.Color.R * 255), (byte)(effect.Color.G * 255), (byte)(effect.Color.B * 255)));

                    context.DrawEllipse(brush, null, new Point(sparkPos.X, sparkPos.Y), 3, 3);
                }
            }
        }

        private void DrawUI(DrawingContext context)
        {
            if (_gameState == TDGameState.Menu)
                return;

            DrawHUD(context);
            DrawTowerPanel(context);
            DrawSpeedControls(context);
            DrawComboIndicator(context);

            if (_selectedTower != null)
                DrawTowerInfoPanel(context);

            if (_gameState == TDGameState.Building)
                DrawWaveCountdown(context);
        }

        private void DrawHUD(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 20, 25, 35));
            var textBrush = System.Windows.Media.Brushes.White;

            context.DrawRectangle(panelBrush, null, new Rect(20, 20, 180, 180));

            var goldText = new FormattedText($"Gold: {(int)_gold}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 18, textBrush, 1.0);
            context.DrawText(goldText, new Point(30, 30));

            var livesText = new FormattedText($"Lives: {(int)_lives}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 18,
                _lives <= 5 ? Brushes.Red : textBrush, 1.0);
            context.DrawText(livesText, new Point(30, 55));

            var waveText = new FormattedText($"Wave: {_currentWave}/{_waves.Count}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 18, Brushes.Orange, 1.0);
            context.DrawText(waveText, new Point(30, 80));

            var scoreText = new FormattedText($"Score: {_score}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, textBrush, 1.0);
            context.DrawText(scoreText, new Point(30, 105));

            var enemiesText = new FormattedText($"Enemies: {_enemies.Count}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, textBrush, 1.0);
            context.DrawText(enemiesText, new Point(30, 125));

            var towersText = new FormattedText($"Towers: {_towers.Count}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, textBrush, 1.0);
            context.DrawText(towersText, new Point(30, 145));

            var stateText = _gameState == TDGameState.Building ? "BUILDING" : "WAVE ACTIVE";
            var stateBrush = _gameState == TDGameState.Building ? Brushes.Green : Brushes.Red;
            var stateLabel = new FormattedText(stateText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 12, stateBrush, 1.0);
            context.DrawText(stateLabel, new Point(30, 170));

            var difficultyText = $"Difficulty: {_difficulty}";
            var difficultyLabel = new FormattedText(difficultyText, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10,
                _difficulty == GameDifficulty.Easy ? Brushes.Green :
                _difficulty == GameDifficulty.Hard ? Brushes.Orange :
                _difficulty == GameDifficulty.Insane ? Brushes.Red : Brushes.White, 1.0);
            context.DrawText(difficultyLabel, new Point(30, 185));
        }

        private void DrawComboIndicator(DrawingContext context)
        {
            if (_comboMultiplier > 1f)
            {
                var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 50, 30, 30));
                context.DrawRectangle(panelBrush, null, new Rect(860, 20, 120, 60));

                var comboText = new FormattedText($"COMBO!", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.Orange, 1.0);
                context.DrawText(comboText, new Point(920 - comboText.Width / 2, 25));

                var multiplierText = new FormattedText($"x{_comboMultiplier:F2}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 24,
                    _comboMultiplier >= 2f ? Brushes.Red : Brushes.Yellow, 1.0);
                context.DrawText(multiplierText, new Point(920 - multiplierText.Width / 2, 45));

                var timerText = new FormattedText($"{_comboTimer:F1}s", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, System.Windows.Media.Brushes.Gray, 1.0);
                context.DrawText(timerText, new Point(920 - timerText.Width / 2, 72));
            }
        }

        private void DrawTowerPanel(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 20, 25, 35));
            context.DrawRectangle(panelBrush, null, new Rect(20, 800, 300, 260));

            float yOffset = 810;
            float xOffset = 30;

            foreach (TowerType type in Enum.GetValues<TowerType>())
            {
                TowerStats stats = GetTowerStats(type);
                bool isSelected = type == _selectedTowerType;
                bool canAfford = _gold >= stats.Cost;

                var bgColor = isSelected ? System.Windows.Media.Color.FromArgb(255, 60, 60, 80) :
                             (canAfford ? System.Windows.Media.Color.FromArgb(100, 40, 45, 55) :
                              System.Windows.Media.Color.FromArgb(50, 30, 30, 35));
                var bgBrush = new SolidColorBrush(bgColor);
                var borderBrush = isSelected ? System.Windows.Media.Brushes.Orange : System.Windows.Media.Brushes.Gray;

                context.DrawRectangle(bgBrush, new Pen(borderBrush, isSelected ? 2 : 1), new Rect(xOffset, yOffset, 130, 50));

                Color towerColor = type switch
                {
                    TowerType.Basic => Color.Gray,
                    TowerType.Archer => Color.Green,
                    TowerType.Sniper => Color.Navy,
                    TowerType.Cannon => Color.Brown,
                    TowerType.Frost => Color.Cyan,
                    TowerType.Poison => Color.Purple,
                    TowerType.Tesla => Color.Yellow,
                    _ => Color.Gray
                };

                var towerBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(towerColor.R * 255), (byte)(towerColor.G * 255), (byte)(towerColor.B * 255)));

                context.DrawRectangle(towerBrush, null, new Rect(xOffset + 5, yOffset + 10, 15, 30));

                string name = type.ToString();
                var nameText = new FormattedText(name, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 11,
                    canAfford ? System.Windows.Media.Brushes.White : System.Windows.Media.Brushes.Gray, 1.0);
                context.DrawText(nameText, new Point(xOffset + 25, yOffset + 5));

                var costText = new FormattedText($"${stats.Cost}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10,
                    canAfford ? Brushes.Yellow : Brushes.DarkGray, 1.0);
                context.DrawText(costText, new Point(xOffset + 25, yOffset + 22));

                var statsText = new FormattedText($"DMG:{stats.Damage} RNG:{stats.Range}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 8, System.Windows.Media.Brushes.Gray, 1.0);
                context.DrawText(statsText, new Point(xOffset + 25, yOffset + 36));

                xOffset += 140;
                if (xOffset > 280)
                {
                    xOffset = 30;
                    yOffset += 60;
                }
            }

            var targetingText = new FormattedText($"Targeting: {_targetingPriority}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, System.Windows.Media.Brushes.Gray, 1.0);
            context.DrawText(targetingText, new Point(30, 1010));
        }

        private void DrawSpeedControls(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 20, 25, 35));
            context.DrawRectangle(panelBrush, null, new Rect(1750, 20, 150, 60));

            var speedText = new FormattedText($"Speed: {_gameSpeed}x", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 14,
                _isSpeedUp ? Brushes.Lime : System.Windows.Media.Brushes.White, 1.0);
            context.DrawText(speedText, new Point(1760, 30));

            var hintText = new FormattedText("Press G to toggle", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, System.Windows.Media.Brushes.Gray, 1.0);
            context.DrawText(hintText, new Point(1760, 50));
        }

        private void DrawTowerInfoPanel(DrawingContext context)
        {
            var tower = _selectedTower!;
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 20, 25, 35));
            context.DrawRectangle(panelBrush, null, new Rect(1620, 200, 280, 220));

            var titleText = new FormattedText($"{tower.Type} Tower (Lv.{tower.Level})", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
            context.DrawText(titleText, new Point(1630, 210));

            var statsText = new FormattedText(
                $"Damage: {tower.Damage:F0}\n" +
                $"Range: {tower.Range:F0}\n" +
                $"Fire Rate: {tower.FireRate:F2}/s\n" +
                $"Total Cost: ${tower.TotalCost}",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, System.Windows.Media.Brushes.LightGray, 1.0);
            context.DrawText(statsText, new Point(1630, 240));

            if (tower.Type == TowerType.Frost)
            {
                var effectText = new FormattedText($"Slow: {tower.SlowEffect:P0}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Cyan, 1.0);
                context.DrawText(effectText, new Point(1630, 300));
            }
            else if (tower.Type == TowerType.Poison)
            {
                var effectText = new FormattedText($"Poison: {tower.PoisonDamage:F1}/s for {tower.PoisonDuration:F1}s", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Purple, 1.0);
                context.DrawText(effectText, new Point(1630, 300));
            }
            else if (tower.Type == TowerType.Tesla)
            {
                var effectText = new FormattedText($"Chain: {tower.ChainCount} targets", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Yellow, 1.0);
                context.DrawText(effectText, new Point(1630, 300));
            }
            else if (tower.SplashRadius > 0)
            {
                var effectText = new FormattedText($"Splash: {tower.SplashRadius:F0}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Orange, 1.0);
                context.DrawText(effectText, new Point(1630, 300));
            }

            var sellValue = (int)(tower.TotalCost * 0.7f);
            var sellText = new FormattedText($"Sell: ${sellValue} [S]", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Green, 1.0);
            context.DrawText(sellText, new Point(1630, 325));

            if (tower.CanUpgrade)
            {
                TowerStats upgradeStats = GetUpgradeStats(tower.Type, tower.Level + 1);
                bool canUpgrade = _gold >= upgradeStats.Cost;

                var upgradeText = new FormattedText($"Upgrade: ${upgradeStats.Cost} [U]", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12,
                    canUpgrade ? Brushes.Lime : Brushes.DarkGray, 1.0);
                context.DrawText(upgradeText, new Point(1630, 350));

                var upgradeStatsText = new FormattedText($"+{((upgradeStats.Damage - tower.Damage) / tower.Damage * 100):F0}% DMG", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, System.Windows.Media.Brushes.Gray, 1.0);
                context.DrawText(upgradeStatsText, new Point(1750, 350));
            }
            else
            {
                var maxLevelText = new FormattedText("MAX LEVEL", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 12, Brushes.Orange, 1.0);
                context.DrawText(maxLevelText, new Point(1630, 350));
            }

            var hintText = new FormattedText("RMB: Deselect", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 9, System.Windows.Media.Brushes.Gray, 1.0);
            context.DrawText(hintText, new Point(1630, 400));
        }

        private void DrawWaveCountdown(DrawingContext context)
        {
            if (_waveCountdown > 0)
            {
                var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0));
                context.DrawRectangle(panelBrush, null, new Rect(860, 180, 200, 80));

                var nextWaveText = new FormattedText("Next Wave", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.Orange, 1.0);
                context.DrawText(nextWaveText, new Point(960 - nextWaveText.Width / 2, 190));

                var countdownText = new FormattedText($"{_waveCountdown:F1}s", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 28, Brushes.White, 1.0);
                context.DrawText(countdownText, new Point(960 - countdownText.Width / 2, 215));

                var startNowText = new FormattedText("SPACE to start now", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, System.Windows.Media.Brushes.Gray, 1.0);
                context.DrawText(startNowText, new Point(960 - startNowText.Width / 2, 250));
            }
        }

        private void DrawMainMenu(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 18, 25)), null, new Rect(0, 0, 1920, 1080));

            var titleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 180, 50));
            var textBrush = System.Windows.Media.Brushes.White;
            var subtitleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 170));

            var title = new FormattedText("TOWER DEFENSE", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 72, titleBrush, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 150));

            var subtitle = new FormattedText("Defend Your Kingdom", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, subtitleBrush, 1.0);
            context.DrawText(subtitle, new Point(960 - subtitle.Width / 2, 230));

            DrawMapSelection(context);
            DrawDifficultySelection(context);
            DrawAchievementsPreview(context);

            var startText = new FormattedText("Press ENTER to Start", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 28, textBrush, 1.0);
            context.DrawText(startText, new Point(960 - startText.Width / 2, 700));

            string controls = "Controls:\n" +
                             "1-7: Select Tower | LMB: Place/Select | RMB: Deselect\n" +
                             "SPACE: Start Wave | B: Toggle Range | G: Speed Toggle\n" +
                             "TAB: Cycle Targeting | U: Upgrade | S: Sell\n" +
                             "ESC: Pause";

            var controlsText = new FormattedText(controls, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, subtitleBrush, 1.0);
            context.DrawText(controlsText, new Point(960 - controlsText.Width / 2, 800));

            DrawTowerPreview(context, 300, 450);
        }

        private void DrawMapSelection(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 35, 45));
            context.DrawRectangle(panelBrush, null, new Rect(100, 300, 700, 120));

            var titleText = new FormattedText("Map Selection", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
            context.DrawText(titleText, new Point(120, 310));

            float xOffset = 130;
            foreach (MapLayout map in Enum.GetValues<MapLayout>())
            {
                bool isSelected = map == _currentMap;
                var mapColor = isSelected ? System.Windows.Media.Color.FromArgb(255, 100, 80, 50) : System.Windows.Media.Color.FromArgb(100, 50, 55, 65);
                var mapBrush = new SolidColorBrush(mapColor);
                var mapBorder = isSelected ? new Pen(Brushes.Orange, 2) : new Pen(Brushes.Gray, 1);

                context.DrawRectangle(mapBrush, mapBorder, new Rect(xOffset, 340, 150, 60));

                string mapName = map.ToString();
                var nameText = new FormattedText(mapName, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 14,
                    isSelected ? Brushes.White : Brushes.Gray, 1.0);
                context.DrawText(nameText, new Point(xOffset + 75 - nameText.Width / 2, 355));

                string mapDesc = map switch
                {
                    MapLayout.Simple => "Classic path",
                    MapLayout.Loop => "Enemies loop",
                    MapLayout.Cross => "Intersections",
                    MapLayout.Maze => "Winding paths",
                    _ => ""
                };
                var descText = new FormattedText(mapDesc, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, System.Windows.Media.Brushes.Gray, 1.0);
                context.DrawText(descText, new Point(xOffset + 75 - descText.Width / 2, 375));

                xOffset += 165;
            }
        }

        private void DrawDifficultySelection(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 35, 45));
            context.DrawRectangle(panelBrush, null, new Rect(100, 440, 700, 100));

            var titleText = new FormattedText("Difficulty", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
            context.DrawText(titleText, new Point(120, 450));

            float xOffset = 130;
            foreach (GameDifficulty diff in Enum.GetValues<GameDifficulty>())
            {
                bool isSelected = diff == _difficulty;
                Color diffColor = diff switch
                {
                    GameDifficulty.Easy => Color.Green,
                    GameDifficulty.Normal => Color.Yellow,
                    GameDifficulty.Hard => Color.Orange,
                    GameDifficulty.Insane => Color.Red,
                    _ => Color.White
                };

                var diffBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(diffColor.R * 255), (byte)(diffColor.G * 255), (byte)(diffColor.B * 255)));

                context.DrawRectangle(diffBrush, null, new Rect(xOffset, 480, 25, 25));

                var diffName = new FormattedText(diff.ToString(), System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 12,
                    isSelected ? Brushes.White : Brushes.Gray, 1.0);
                context.DrawText(diffName, new Point(xOffset + 35, 485));

                xOffset += 170;
            }
        }

        private void DrawAchievementsPreview(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 35, 45));
            context.DrawRectangle(panelBrush, null, new Rect(1120, 300, 280, 400));

            var titleText = new FormattedText("Achievements", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
            context.DrawText(titleText, new Point(1140, 310));

            int yOffset = 340;
            foreach (var kvp in _achievements)
            {
                string name = kvp.Key.ToString();
                bool unlocked = kvp.Value;

                var achievementBrush = unlocked ?
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 100, 200, 100)) :
                    new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 60, 60, 70));

                context.DrawRectangle(achievementBrush, null, new Rect(1140, yOffset, 240, 30));

                var textBrush = unlocked ? Brushes.White : System.Windows.Media.Brushes.Gray;
                var achievementText = new FormattedText(name, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, textBrush, 1.0);
                context.DrawText(achievementText, new Point(1150, yOffset + 8));

                yOffset += 35;
            }
        }

        private void DrawTowerPreview(DrawingContext context, float x, float y)
        {
            var previewPanel = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 35, 45));
            context.DrawRectangle(previewPanel, null, new Rect(x - 20, y - 20, 440, 200));

            float xOffset = x;
            float yOffset = y;

            foreach (TowerType type in Enum.GetValues<TowerType>())
            {
                TowerStats stats = GetTowerStats(type);

                Color towerColor = type switch
                {
                    TowerType.Basic => Color.Gray,
                    TowerType.Archer => Color.Green,
                    TowerType.Sniper => Color.Navy,
                    TowerType.Cannon => Color.Brown,
                    TowerType.Frost => Color.Cyan,
                    TowerType.Poison => Color.Purple,
                    TowerType.Tesla => Color.Yellow,
                    _ => Color.Gray
                };

                var towerBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(towerColor.R * 255), (byte)(towerColor.G * 255), (byte)(towerColor.B * 255)));

                context.DrawRectangle(towerBrush, null, new Rect(xOffset, yOffset, 25, 25));

                var nameText = new FormattedText(type.ToString(), System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 10, System.Windows.Media.Brushes.White, 1.0);
                context.DrawText(nameText, new Point(xOffset + 30, yOffset));

                var costText = new FormattedText($"${stats.Cost}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 9, Brushes.Yellow, 1.0);
                context.DrawText(costText, new Point(xOffset + 30, yOffset + 14));

                xOffset += 110;
                if (xOffset > x + 380)
                {
                    xOffset = x;
                    yOffset += 50;
                }
            }
        }

        private void DrawPauseMenu(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)), null, new Rect(0, 0, 1920, 1080));

            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 45, 55));
            context.DrawRectangle(panelBrush, null, new Rect(760, 284, 400, 412));

            var title = new FormattedText("PAUSED", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 48, Brushes.White, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 320));

            var resumeText = new FormattedText("Press ESC to Resume", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, System.Windows.Media.Brushes.LightGray, 1.0);
            context.DrawText(resumeText, new Point(960 - resumeText.Width / 2, 400));

            DrawStatistics(context, 800, 450);

            var quitText = new FormattedText("Press R to Restart", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 20, System.Windows.Media.Brushes.Gray, 1.0);
            context.DrawText(quitText, new Point(960 - quitText.Width / 2, 640));
        }

        private void DrawStatistics(DrawingContext context, float x, float y)
        {
            var statsTitle = new FormattedText("Statistics", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 18, Brushes.Orange, 1.0);
            context.DrawText(statsTitle, new Point(960 - statsTitle.Width / 2, y));

            y += 30;
            foreach (var kvp in _statistics)
            {
                var statText = new FormattedText($"{kvp.Key}: {kvp.Value}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, System.Windows.Media.Brushes.LightGray, 1.0);
                context.DrawText(statText, new Point(800, y));
                y += 22;
            }
        }

        private void DrawGameOverScreen(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 30, 0, 0)), null, new Rect(0, 0, 1920, 1080));

            var title = new FormattedText("GAME OVER", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 72, Brushes.Red, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 200));

            var scoreText = new FormattedText($"Final Score: {_score}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 32, Brushes.White, 1.0);
            context.DrawText(scoreText, new Point(960 - scoreText.Width / 2, 300));

            var waveText = new FormattedText($"Waves Survived: {_currentWave - 1}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, Brushes.LightGray, 1.0);
            context.DrawText(waveText, new Point(960 - waveText.Width / 2, 350));

            DrawStatistics(context, 760, 420);

            var restartText = new FormattedText("Press R to Restart", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 28, Brushes.LightBlue, 1.0);
            context.DrawText(restartText, new Point(960 - restartText.Width / 2, 700));
        }

        private void DrawVictoryScreen(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 0, 30, 0)), null, new Rect(0, 0, 1920, 1080));

            var title = new FormattedText("VICTORY!", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 72, Brushes.Gold, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 200));

            var scoreText = new FormattedText($"Final Score: {_score}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 32, Brushes.White, 1.0);
            context.DrawText(scoreText, new Point(960 - scoreText.Width / 2, 300));

            var goldText = new FormattedText($"Gold Remaining: {(int)_gold}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, Brushes.Yellow, 1.0);
            context.DrawText(goldText, new Point(960 - goldText.Width / 2, 350));

            int unlockedCount = _achievements.Count(a => a.Value);
            var achievementsText = new FormattedText($"Achievements: {unlockedCount}/{_achievements.Count}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 18, Brushes.Orange, 1.0);
            context.DrawText(achievementsText, new Point(960 - achievementsText.Width / 2, 390));

            DrawStatistics(context, 760, 450);

            var restartText = new FormattedText("Press R to Play Again", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 28, Brushes.LightBlue, 1.0);
            context.DrawText(restartText, new Point(960 - restartText.Width / 2, 750));
        }

        public override void Dispose()
        {
            _towers.Clear();
            _enemies.Clear();
            _projectiles.Clear();
            _effects.Clear();
            base.Dispose();
        }
    }

    public class MapGrid
    {
        private CellType[,] _cells;
        public int Width { get; }
        public int Height { get; }

        public MapGrid(int width, int height)
        {
            Width = width;
            Height = height;
            _cells = new CellType[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    _cells[x, y] = CellType.Buildable;
                }
            }
        }

        public CellType GetCellType(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return CellType.Blocked;
            return _cells[x, y];
        }

        public void SetCellType(int x, int y, CellType type)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
                _cells[x, y] = type;
        }

        public void Clear()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    _cells[x, y] = CellType.Buildable;
                }
            }
        }
    }

    public enum CellType
    {
        Buildable,
        Path,
        Blocked,
        Occupied
    }

    public class TowerStats
    {
        public int Cost { get; set; }
        public float Damage { get; set; }
        public float Range { get; set; }
        public float FireRate { get; set; }
        public float SplashRadius { get; set; }
        public float SlowEffect { get; set; }
        public float PoisonDamage { get; set; }
        public float PoisonDuration { get; set; }
        public int ChainCount { get; set; }
    }

    public class Tower
    {
        public TowerType Type { get; }
        public int GridX { get; }
        public int GridY { get; }
        public Vector2 Position { get; set; }
        public float Damage { get; private set; }
        public float Range { get; private set; }
        public float FireRate { get; private set; }
        public float SplashRadius { get; }
        public float SlowEffect { get; }
        public float PoisonDamage { get; }
        public float PoisonDuration { get; }
        public int ChainCount { get; }
        public float CurrentAngle { get; private set; }
        public int Level { get; private set; }
        public int TotalCost { get; private set; }
        public bool CanUpgrade => Level < 3;

        private float _fireCooldown;
        private Enemy? _target;

        public Tower(TowerType type, int gridX, int gridY, TowerStats stats)
        {
            Type = type;
            GridX = gridX;
            GridY = gridY;
            Damage = stats.Damage;
            Range = stats.Range;
            FireRate = stats.FireRate;
            SplashRadius = stats.SplashRadius;
            SlowEffect = stats.SlowEffect;
            PoisonDamage = stats.PoisonDamage;
            PoisonDuration = stats.PoisonDuration;
            ChainCount = stats.ChainCount;
            Level = 1;
            TotalCost = stats.Cost;
            _fireCooldown = 0;
            CurrentAngle = 0;
        }

        public void Update(float deltaTime, List<Enemy> enemies, List<Projectile> projectiles, TargetingPriority priority)
        {
            if (_fireCooldown > 0)
                _fireCooldown -= deltaTime;

            _target = SelectTarget(enemies, priority);

            if (_target != null)
            {
                Vector2 direction = _target.Position - Position;
                float targetAngle = (float)(Math.Atan2(direction.Y, direction.X) * 180 / Math.PI);
                float angleDiff = targetAngle - CurrentAngle;
                while (angleDiff > 180) angleDiff -= 360;
                while (angleDiff < -180) angleDiff += 360;

                CurrentAngle += angleDiff * Math.Min(1f, deltaTime * 10);

                if (_fireCooldown <= 0 && Math.Abs(angleDiff) < 45)
                {
                    Fire(projectiles);
                    _fireCooldown = 1f / FireRate;
                }
            }
        }

        private Enemy? SelectTarget(List<Enemy> enemies, TargetingPriority priority)
        {
            if (enemies.Count == 0) return null;

            Enemy? bestTarget = null;
            float bestValue = float.MinValue;

            foreach (var enemy in enemies)
            {
                float dist = (enemy.Position - Position).Length();
                if (dist > Range) continue;

                float value = priority switch
                {
                    TargetingPriority.First => enemy.PathProgress,
                    TargetingPriority.Last => -enemy.PathProgress,
                    TargetingPriority.Closest => -dist,
                    TargetingPriority.Strongest => -enemy.Health,
                    TargetingPriority.Weakest => enemy.Health,
                    _ => -dist
                };

                if (Type == TowerType.Frost)
                    value -= dist * 0.1f;

                if (value > bestValue)
                {
                    bestValue = value;
                    bestTarget = enemy;
                }
            }

            return bestTarget;
        }

        private void Fire(List<Projectile> projectiles)
        {
            if (_target == null) return;

            Vector2 direction = (_target.Position - Position).Normalized();
            Vector2 velocity = direction * 400;

            ProjectileType projType = Type switch
            {
                TowerType.Archer => ProjectileType.Arrow,
                TowerType.Sniper => ProjectileType.Bullet,
                TowerType.Cannon => ProjectileType.Cannonball,
                TowerType.Frost => ProjectileType.Frost,
                TowerType.Poison => ProjectileType.Poison,
                TowerType.Tesla => ProjectileType.Lightning,
                _ => ProjectileType.Bullet
            };

            Color projColor = Type switch
            {
                TowerType.Frost => Color.Cyan,
                TowerType.Poison => Color.Purple,
                TowerType.Tesla => Color.Yellow,
                _ => Color.Red
            };

            EffectType effectType = EffectType.None;
            if (Type == TowerType.Frost)
                effectType = EffectType.Slow;
            else if (Type == TowerType.Poison)
                effectType = EffectType.Poison;
            else if (Type == TowerType.Tesla)
                effectType = EffectType.ChainLightning;

            var projectile = new Projectile(Position, velocity, Damage, _target, projType, projColor, effectType)
            {
                AreaRadius = SplashRadius,
                AreaDamage = SplashRadius > 0 ? Damage : 0,
                ChainCount = ChainCount
            };

            projectiles.Add(projectile);
        }

        public void Upgrade(TowerStats upgradeStats)
        {
            Level++;
            Damage = upgradeStats.Damage;
            Range = upgradeStats.Range;
            FireRate = upgradeStats.FireRate;
            TotalCost += (int)(upgradeStats.Cost * 0.75f);
        }
    }

    public enum ProjectileType
    {
        Bullet,
        Arrow,
        Cannonball,
        Frost,
        Poison,
        Lightning
    }

    public class Projectile
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Damage { get; set; }
        public Enemy? Target { get; set; }
        public ProjectileType Type { get; set; }
        public Color Color { get; set; }
        public EffectType EffectType { get; set; }
        public float AreaRadius { get; set; }
        public float AreaDamage { get; set; }
        public int ChainCount { get; set; }
        public bool IsExpired { get; set; }
        public bool HasHit { get; private set; }
        public float Lifetime { get; private set; }
        public const float MaxLifetime = 3f;

        public Projectile(Vector2 position, Vector2 velocity, float damage, Enemy? target, ProjectileType type, Color color, EffectType effectType)
        {
            Position = position;
            Velocity = velocity;
            Damage = damage;
            Target = target;
            Type = type;
            Color = color;
            EffectType = effectType;
            IsExpired = false;
            HasHit = false;
            Lifetime = 0;
        }

        public void Update(float deltaTime)
        {
            Lifetime += deltaTime;

            if (Lifetime > MaxLifetime)
            {
                IsExpired = true;
                return;
            }

            if (Target != null && !Target.IsDead)
            {
                Vector2 toTarget = Target.Position - Position;
                float dist = toTarget.Length();

                if (dist < 10)
                {
                    HasHit = true;
                    return;
                }

                Vector2 desiredVelocity = toTarget.Normalized() * Velocity.Length();
                Velocity = Vector2.Lerp(Velocity, desiredVelocity, deltaTime * 5);
            }

            Position = Position + Velocity * deltaTime;

            if (Position.X < 0 || Position.X > 1920 || Position.Y < 0 || Position.Y > 1080)
            {
                IsExpired = true;
            }
        }
    }

    public enum EffectType
    {
        None,
        Explosion,
        Shockwave,
        Hit,
        Slow,
        Poison,
        ChainLightning
    }

    public class Effect
    {
        public Vector2 Position { get; set; }
        public EffectType Type { get; set; }
        public Color Color { get; set; }
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }
        public float Radius { get; set; }
        public bool IsExpired => Lifetime <= 0;

        public Effect(Vector2 position, EffectType type, Color color, float duration)
        {
            Position = position;
            Type = type;
            Color = color;
            Lifetime = duration;
            MaxLifetime = duration;
            Radius = 30;
        }

        public void Update(float deltaTime)
        {
            Lifetime -= deltaTime;
        }
    }

    public class ParticleEffect : Effect
    {
        public Vector2 Velocity { get; set; }

        public ParticleEffect(Vector2 position, Vector2 velocity, Color color, float duration)
            : base(position, EffectType.Hit, color, duration)
        {
            Velocity = velocity;
        }

        public new void Update(float deltaTime)
        {
            Lifetime -= deltaTime;
            Position = Position + Velocity * deltaTime;
            Velocity = Velocity * 0.95f;
        }
    }

    public class LightningEffect : Effect
    {
        public Vector2 StartPoint { get; }
        public Vector2 EndPoint { get; }

        public LightningEffect(Vector2 start, Vector2 end, Color color, float duration)
            : base(start, EffectType.ChainLightning, color, duration)
        {
            StartPoint = start;
            EndPoint = end;
        }
    }

    public class Enemy
    {
        public EnemyType Type { get; }
        public Vector2 Position { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; }
        public float Speed { get; set; }
        public float BaseSpeed { get; }
        public float GoldReward { get; }
        public int ScoreValue { get; }
        public float Damage { get; }
        public float Radius { get; set; }
        public Color Color { get; }
        public bool IsDead => Health <= 0;
        public bool IsReachedEnd { get; private set; }
        public int PathIndex { get; set; }
        public float PathProgress { get; set; }
        public float HealRate { get; }
        public float HealRadius { get; }
        public float SlowEffect { get; private set; }
        public float SlowDuration { get; private set; }
        public float PoisonDamage { get; private set; }
        public float PoisonDuration { get; private set; }

        private float _slowTimer;
        private float _poisonTimer;
        private Vector2[] _path;
        private Random _random;

        public Enemy(Vector2 position, EnemyType type, float health, float speed, float goldReward, int scoreValue, Color color)
        {
            Type = type;
            Position = position;
            Health = health;
            MaxHealth = health;
            Speed = speed;
            BaseSpeed = speed;
            GoldReward = goldReward;
            ScoreValue = scoreValue;
            Damage = type == EnemyType.Boss ? 5 : 1;
            Radius = type switch
            {
                EnemyType.Tank => 18,
                EnemyType.Boss => 25,
                EnemyType.Flying => 12,
                _ => 14
            };
            Color = color;
            IsReachedEnd = false;
            PathIndex = 0;
            PathProgress = 0;
            _slowTimer = 0;
            _slowEffect = 0;
            _poisonTimer = 0;
            _poisonDamage = 0;
            HealRate = type == EnemyType.Healer ? 5f : 0;
            HealRadius = type == EnemyType.Healer ? 100f : 0;
            _path = Array.Empty<Vector2>();
            _random = new Random();
        }

        public void Update(float deltaTime)
        {
            if (IsDead) return;

            if (_slowTimer > 0)
            {
                _slowTimer -= deltaTime;
                Speed = BaseSpeed * (1f - _slowEffect);
            }
            else
            {
                Speed = BaseSpeed;
                _slowEffect = 0;
            }

            if (_poisonTimer > 0)
            {
                _poisonTimer -= deltaTime;
                TakeDamage(_poisonDamage * deltaTime, EffectType.None);
            }

            MoveAlongPath(deltaTime);
        }

        private void MoveAlongPath(float deltaTime)
        {
            if (_path.Length < 2 || PathIndex >= _path.Length - 1)
            {
                IsReachedEnd = true;
                return;
            }

            Vector2 current = _path[PathIndex];
            Vector2 next = _path[PathIndex + 1];

            Vector2 start = new Vector2(current.X * CELL_SIZE + CELL_SIZE / 2, current.Y * CELL_SIZE + CELL_SIZE / 2);
            Vector2 end = new Vector2(next.X * CELL_SIZE + CELL_SIZE / 2, next.Y * CELL_SIZE + CELL_SIZE / 2);

            float segmentLength = (end - start).Length();
            float progressInSegment = PathProgress * segmentLength;

            Position = Vector2.Lerp(start, end, Math.Clamp(progressInSegment / segmentLength, 0, 1));
            Position = new Vector2(Position.X + 100, Position.Y + 50);

            progressInSegment += Speed * deltaTime;
            PathProgress = progressInSegment / segmentLength;

            if (PathProgress >= 1)
            {
                PathProgress = 0;
                PathIndex++;
            }
        }

        public void SetPath(Vector2[] path)
        {
            _path = path;
            PathIndex = 0;
            PathProgress = 0;
        }

        public void TakeDamage(float damage, EffectType effectType)
        {
            Health -= damage;

            if (effectType == EffectType.Slow && damage > 0)
            {
                _slowEffect = 0.5f;
                _slowTimer = 1f;
            }
            else if (effectType == EffectType.Poison && damage > 0)
            {
                _poisonDamage = 5f;
                _poisonTimer = 3f;
            }
        }

        public void Heal(float amount)
        {
            Health = Math.Min(MaxHealth, Health + amount);
        }
    }

    public class Wave
    {
        public List<EnemyGroup> EnemyGroups { get; set; } = new List<EnemyGroup>();
        public int TotalEnemies => EnemyGroups.Sum(g => g.Count);
    }

    public class EnemyGroup
    {
        public EnemyType Type { get; set; }
        public int Count { get; set; }
        public float Delay { get; set; }
    }

    public static class MathHelper
    {
        public const float PiOver2 = (float)(Math.PI / 2);
        public const float TwoPi = (float)(Math.PI * 2);
    }
}

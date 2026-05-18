using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WPFEngine.Core;
using WPFEngine.Math;

namespace WPFEngine.Demos
{
    public enum PuzzleGameState
    {
        Menu,
        Playing,
        Paused,
        LevelComplete,
        GameOver
    }

    public enum TileType
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange,
        Cyan,
        Rainbow,
        StripedHorizontal,
        StripedVertical,
        Bomb
    }

    public enum TileColor
    {
        Red,
        Blue,
        Green,
        Yellow,
        Purple,
        Orange,
        Cyan,
        Rainbow
    }

    public enum ObjectiveType
    {
        ScoreTarget,
        CollectTileType,
        ClearObstacles
    }

    public enum ObstacleType
    {
        None,
        Locked,
        Ice,
        Mud
    }

    public class PuzzleGame : GameBase
    {
        private PuzzleGameState _gameState;
        private PuzzleGameState _previousState;
        private GridManager _gridManager;
        private PuzzleUI _ui;
        private LevelData _currentLevel;
        private int _currentLevelIndex;
        private List<LevelData> _levels;
        private int _score;
        private int _displayedScore;
        private int _movesRemaining;
        private int _comboCount;
        private float _comboTimer;
        private bool _isProcessing;
        private float _hintTimer;
        private Vector2Int _selectedTile;
        private Vector2Int _hintPosition;
        private float _scoreAnimationTimer;
        private int _animatedScoreStart;
        private int _animatedScoreEnd;
        private List<ScorePopup> _scorePopups;
        private Dictionary<TileColor, int> _tilesCollected;
        private Random _random;
        private float _swapAnimationTime;
        private float _matchAnimationTime;
        private float _fallAnimationTime;
        private bool _isAnimating;
        private TileType _swapTile1Type;
        private TileType _swapTile2Type;
        private Vector2Int _swapPos1;
        private Vector2Int _swapPos2;

        public PuzzleGameState GameState => _gameState;
        public int Score => _score;
        public int MovesRemaining => _movesRemaining;
        public int CurrentLevelIndex => _currentLevelIndex;
        public GridManager GridManager => _gridManager;

        public event EventHandler<PuzzleGameState>? GameStateChanged;
        public event EventHandler<int>? ScoreChanged;
        public event EventHandler<int>? MovesChanged;
        public event EventHandler? ComboTriggered;
        public event EventHandler? LevelCompleted;
        public event EventHandler? GameOver;

        public PuzzleGame()
        {
            _gameState = PuzzleGameState.Menu;
            _previousState = PuzzleGameState.Menu;
            _gridManager = new GridManager();
            _levels = new List<LevelData>();
            _currentLevelIndex = 0;
            _score = 0;
            _displayedScore = 0;
            _movesRemaining = 0;
            _comboCount = 0;
            _comboTimer = 0f;
            _isProcessing = false;
            _hintTimer = 0f;
            _selectedTile = Vector2Int.Zero;
            _hintPosition = Vector2Int.Zero;
            _scoreAnimationTimer = 0f;
            _scorePopups = new List<ScorePopup>();
            _tilesCollected = new Dictionary<TileColor, int>();
            _random = new Random();
            _swapAnimationTime = 0.2f;
            _matchAnimationTime = 0.3f;
            _fallAnimationTime = 0.15f;
            _isAnimating = false;

            foreach (TileColor color in Enum.GetValues(typeof(TileColor)))
            {
                _tilesCollected[color] = 0;
            }
        }

        protected override void Initialize()
        {
            base.Initialize();
            CreateLevels();
            _ui = new PuzzleUI();
            _ui.Initialize(this);
            _gameState = PuzzleGameState.Menu;
            GameStateChanged?.Invoke(this, _gameState);
        }

        private void CreateLevels()
        {
            _levels.Add(CreateLevel1());
            _levels.Add(CreateLevel2());
            _levels.Add(CreateLevel3());
            _levels.Add(CreateLevel4());
            _levels.Add(CreateLevel5());
            _levels.Add(CreateLevel6());
            _levels.Add(CreateLevel7());
            _levels.Add(CreateLevel8());
            _levels.Add(CreateLevel9());
            _levels.Add(CreateLevel10());
        }

        private LevelData CreateLevel1()
        {
            return new LevelData
            {
                LevelNumber = 1,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 30,
                ScoreTarget = 5000,
                BackgroundColor = new Color(0.3f, 0.5f, 0.8f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.ScoreTarget, TargetValue = 5000 }
                },
                Obstacles = new List<ObstacleData>()
            };
        }

        private LevelData CreateLevel2()
        {
            return new LevelData
            {
                LevelNumber = 2,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 25,
                ScoreTarget = 8000,
                BackgroundColor = new Color(0.4f, 0.6f, 0.3f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.CollectTileType, TileType = TileColor.Red, TargetValue = 10 }
                },
                Obstacles = new List<ObstacleData>()
            };
        }

        private LevelData CreateLevel3()
        {
            return new LevelData
            {
                LevelNumber = 3,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 25,
                ScoreTarget = 10000,
                BackgroundColor = new Color(0.6f, 0.4f, 0.5f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.ScoreTarget, TargetValue = 10000 },
                    new Objective { Type = ObjectiveType.CollectTileType, TileType = TileColor.Blue, TargetValue = 15 }
                },
                Obstacles = new List<ObstacleData>()
            };
        }

        private LevelData CreateLevel4()
        {
            return new LevelData
            {
                LevelNumber = 4,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 30,
                ScoreTarget = 12000,
                BackgroundColor = new Color(0.5f, 0.4f, 0.6f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.CollectTileType, TileType = TileColor.Green, TargetValue = 20 }
                },
                Obstacles = new List<ObstacleData>
                {
                    new ObstacleData { X = 0, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 7, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 3, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 4, Y = 7, Type = ObstacleType.Locked }
                }
            };
        }

        private LevelData CreateLevel5()
        {
            return new LevelData
            {
                LevelNumber = 5,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 20,
                ScoreTarget = 15000,
                BackgroundColor = new Color(0.4f, 0.5f, 0.6f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.ScoreTarget, TargetValue = 15000 }
                },
                Obstacles = new List<ObstacleData>
                {
                    new ObstacleData { X = 2, Y = 2, Type = ObstacleType.Ice },
                    new ObstacleData { X = 3, Y = 2, Type = ObstacleType.Ice },
                    new ObstacleData { X = 4, Y = 2, Type = ObstacleType.Ice },
                    new ObstacleData { X = 5, Y = 2, Type = ObstacleType.Ice },
                    new ObstacleData { X = 2, Y = 5, Type = ObstacleType.Ice },
                    new ObstacleData { X = 3, Y = 5, Type = ObstacleType.Ice },
                    new ObstacleData { X = 4, Y = 5, Type = ObstacleType.Ice },
                    new ObstacleData { X = 5, Y = 5, Type = ObstacleType.Ice }
                }
            };
        }

        private LevelData CreateLevel6()
        {
            return new LevelData
            {
                LevelNumber = 6,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 25,
                ScoreTarget = 18000,
                BackgroundColor = new Color(0.5f, 0.6f, 0.4f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.CollectTileType, TileType = TileColor.Purple, TargetValue = 25 }
                },
                Obstacles = new List<ObstacleData>
                {
                    new ObstacleData { X = 0, Y = 0, Type = ObstacleType.Mud },
                    new ObstacleData { X = 1, Y = 0, Type = ObstacleType.Mud },
                    new ObstacleData { X = 0, Y = 1, Type = ObstacleType.Mud },
                    new ObstacleData { X = 7, Y = 7, Type = ObstacleType.Mud },
                    new ObstacleData { X = 6, Y = 7, Type = ObstacleType.Mud },
                    new ObstacleData { X = 7, Y = 6, Type = ObstacleType.Mud }
                }
            };
        }

        private LevelData CreateLevel7()
        {
            return new LevelData
            {
                LevelNumber = 7,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 20,
                ScoreTarget = 20000,
                BackgroundColor = new Color(0.6f, 0.5f, 0.4f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.ScoreTarget, TargetValue = 20000 },
                    new Objective { Type = ObjectiveType.CollectTileType, TileType = TileColor.Orange, TargetValue = 15 }
                },
                Obstacles = new List<ObstacleData>
                {
                    new ObstacleData { X = 3, Y = 3, Type = ObstacleType.Locked },
                    new ObstacleData { X = 4, Y = 3, Type = ObstacleType.Locked },
                    new ObstacleData { X = 3, Y = 4, Type = ObstacleType.Locked },
                    new ObstacleData { X = 4, Y = 4, Type = ObstacleType.Locked }
                }
            };
        }

        private LevelData CreateLevel8()
        {
            return new LevelData
            {
                LevelNumber = 8,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 30,
                ScoreTarget = 25000,
                BackgroundColor = new Color(0.4f, 0.4f, 0.5f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.CollectTileType, TileType = TileColor.Cyan, TargetValue = 30 }
                },
                Obstacles = new List<ObstacleData>
                {
                    new ObstacleData { X = 1, Y = 1, Type = ObstacleType.Ice },
                    new ObstacleData { X = 6, Y = 1, Type = ObstacleType.Ice },
                    new ObstacleData { X = 1, Y = 6, Type = ObstacleType.Ice },
                    new ObstacleData { X = 6, Y = 6, Type = ObstacleType.Ice },
                    new ObstacleData { X = 0, Y = 4, Type = ObstacleType.Mud },
                    new ObstacleData { X = 7, Y = 3, Type = ObstacleType.Mud }
                }
            };
        }

        private LevelData CreateLevel9()
        {
            return new LevelData
            {
                LevelNumber = 9,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 25,
                ScoreTarget = 30000,
                BackgroundColor = new Color(0.5f, 0.3f, 0.5f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.ScoreTarget, TargetValue = 30000 }
                },
                Obstacles = new List<ObstacleData>
                {
                    new ObstacleData { X = 0, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 1, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 2, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 5, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 6, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 7, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 3, Y = 6, Type = ObstacleType.Ice },
                    new ObstacleData { X = 4, Y = 6, Type = ObstacleType.Ice }
                }
            };
        }

        private LevelData CreateLevel10()
        {
            return new LevelData
            {
                LevelNumber = 10,
                GridWidth = 8,
                GridHeight = 8,
                Moves = 35,
                ScoreTarget = 50000,
                BackgroundColor = new Color(0.3f, 0.4f, 0.4f),
                Objectives = new List<Objective>
                {
                    new Objective { Type = ObjectiveType.ScoreTarget, TargetValue = 50000 },
                    new Objective { Type = ObjectiveType.CollectTileType, TileType = TileColor.Yellow, TargetValue = 25 }
                },
                Obstacles = new List<ObstacleData>
                {
                    new ObstacleData { X = 0, Y = 0, Type = ObstacleType.Locked },
                    new ObstacleData { X = 7, Y = 0, Type = ObstacleType.Locked },
                    new ObstacleData { X = 0, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 7, Y = 7, Type = ObstacleType.Locked },
                    new ObstacleData { X = 3, Y = 3, Type = ObstacleType.Mud },
                    new ObstacleData { X = 4, Y = 3, Type = ObstacleType.Mud },
                    new ObstacleData { X = 3, Y = 4, Type = ObstacleType.Mud },
                    new ObstacleData { X = 4, Y = 4, Type = ObstacleType.Mud },
                    new ObstacleData { X = 2, Y = 2, Type = ObstacleType.Ice },
                    new ObstacleData { X = 5, Y = 2, Type = ObstacleType.Ice },
                    new ObstacleData { X = 2, Y = 5, Type = ObstacleType.Ice },
                    new ObstacleData { X = 5, Y = 5, Type = ObstacleType.Ice }
                }
            };
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            UpdateInput();
            UpdateAnimations(deltaTime);
            UpdateScoreAnimation(deltaTime);
            UpdateComboTimer(deltaTime);
            UpdateHintTimer(deltaTime);

            if (_gameState == PuzzleGameState.Playing && !_isProcessing)
            {
                CheckLevelObjectives();
            }

            _ui.Update(deltaTime);
        }

        private void UpdateInput()
        {
            if (_gameState == PuzzleGameState.Menu)
            {
                if (InputManager?.Instance.GetKeyDown(InputManager.KeyCode.Enter) == true)
                {
                    StartGame();
                }
            }
            else if (_gameState == PuzzleGameState.Playing)
            {
                if (InputManager?.Instance.GetKeyDown(InputManager.KeyCode.Escape) == true)
                {
                    SetGameState(PuzzleGameState.Paused);
                }

                if (InputManager?.Instance.GetKeyDown(InputManager.KeyCode.H) == true)
                {
                    ShowHint();
                }
            }
            else if (_gameState == PuzzleGameState.Paused)
            {
                if (InputManager?.Instance.GetKeyDown(InputManager.KeyCode.Escape) == true)
                {
                    SetGameState(PuzzleGameState.Playing);
                }

                if (InputManager?.Instance.GetKeyDown(InputManager.KeyCode.Q) == true)
                {
                    SetGameState(PuzzleGameState.Menu);
                }
            }
            else if (_gameState == PuzzleGameState.GameOver || _gameState == PuzzleGameState.LevelComplete)
            {
                if (InputManager?.Instance.GetKeyDown(InputManager.KeyCode.Enter) == true)
                {
                    if (_gameState == PuzzleGameState.LevelComplete)
                    {
                        NextLevel();
                    }
                    else
                    {
                        RestartGame();
                    }
                }

                if (InputManager?.Instance.GetKeyDown(InputManager.KeyCode.Escape) == true)
                {
                    SetGameState(PuzzleGameState.Menu);
                }
            }
        }

        private void UpdateAnimations(float deltaTime)
        {
            if (_isAnimating)
            {
                _gridManager.UpdateAnimations(deltaTime);
            }

            for (int i = _scorePopups.Count - 1; i >= 0; i--)
            {
                _scorePopups[i].Update(deltaTime);
                if (_scorePopups[i].IsExpired)
                {
                    _scorePopups.RemoveAt(i);
                }
            }
        }

        private void UpdateScoreAnimation(float deltaTime)
        {
            if (_scoreAnimationTimer > 0)
            {
                _scoreAnimationTimer -= deltaTime;
                float progress = 1f - (_scoreAnimationTimer / 0.5f);
                _displayedScore = _animatedScoreStart + (int)((_animatedScoreEnd - _animatedScoreStart) * progress);
            }
            else if (_displayedScore != _score)
            {
                _displayedScore = _score;
            }
        }

        private void UpdateComboTimer(float deltaTime)
        {
            if (_comboTimer > 0)
            {
                _comboTimer -= deltaTime;
                if (_comboTimer <= 0)
                {
                    _comboCount = 0;
                }
            }
        }

        private void UpdateHintTimer(float deltaTime)
        {
            if (_hintTimer > 0)
            {
                _hintTimer -= deltaTime;
            }
        }

        public void StartGame()
        {
            _score = 0;
            _displayedScore = 0;
            _currentLevelIndex = 0;
            LoadLevel(_currentLevelIndex);
            SetGameState(PuzzleGameState.Playing);
        }

        public void LoadLevel(int levelIndex)
        {
            if (levelIndex < 0 || levelIndex >= _levels.Count)
                return;

            _currentLevel = _levels[levelIndex];
            _currentLevelIndex = levelIndex;
            _movesRemaining = _currentLevel.Moves;
            _score = 0;
            _displayedScore = 0;
            _comboCount = 0;
            _comboTimer = 0f;
            _isProcessing = false;
            _hintTimer = 0f;
            _selectedTile = Vector2Int.Zero;
            _scorePopups.Clear();

            foreach (TileColor color in Enum.GetValues(typeof(TileColor)))
            {
                _tilesCollected[color] = 0;
            }

            _gridManager.Initialize(_currentLevel.GridWidth, _currentLevel.GridHeight);
            _gridManager.SetObstacles(_currentLevel.Obstacles);

            for (int y = 0; y < _currentLevel.GridHeight; y++)
            {
                for (int x = 0; x < _currentLevel.GridWidth; x++)
                {
                    if (!_gridManager.HasObstacle(x, y))
                    {
                        _gridManager.SetTile(x, y, GenerateRandomTile());
                    }
                }
            }

            while (_gridManager.HasMatches())
            {
                var matches = _gridManager.GetAllMatches();
                foreach (var match in matches)
                {
                    _gridManager.SetTile(match.Position.X, match.Position.Y, GenerateRandomTile());
                }
            }

            _ui.LoadLevel(_currentLevel);
            ScoreChanged?.Invoke(this, _score);
            MovesChanged?.Invoke(this, _movesRemaining);
        }

        private TileType GenerateRandomTile()
        {
            int baseType = _random.Next(7);
            return (TileType)baseType;
        }

        public void SelectTile(int x, int y)
        {
            if (_isProcessing || _isAnimating || _gameState != PuzzleGameState.Playing)
                return;

            if (!_gridManager.IsValidPosition(x, y) || _gridManager.HasObstacle(x, y))
                return;

            if (_selectedTile == Vector2Int.Zero)
            {
                _selectedTile = new Vector2Int(x, y);
                _hintTimer = 0f;
            }
            else
            {
                Vector2Int diff = new Vector2Int(x - _selectedTile.X, y - _selectedTile.Y);

                if (Math.Abs(diff.X) + Math.Abs(diff.Y) == 1)
                {
                    TrySwap(_selectedTile.X, _selectedTile.Y, x, y);
                }

                _selectedTile = Vector2Int.Zero;
            }
        }

        private void TrySwap(int x1, int y1, int x2, int y2)
        {
            if (!_gridManager.IsValidPosition(x1, y1) || !_gridManager.IsValidPosition(x2, y2))
                return;

            if (_gridManager.HasObstacle(x1, y1) || _gridManager.HasObstacle(x2, y2))
                return;

            _gridManager.SwapTiles(x1, y1, x2, y2);

            if (_gridManager.HasMatches())
            {
                _isProcessing = true;
                _movesRemaining--;
                MovesChanged?.Invoke(this, _movesRemaining);

                ProcessMatchesAsync();
            }
            else
            {
                _gridManager.SwapTiles(x1, y1, x2, y2);
            }
        }

        private async void ProcessMatchesAsync()
        {
            while (_gridManager.HasMatches())
            {
                var matches = _gridManager.GetAllMatches();

                foreach (var match in matches)
                {
                    if (match.Tile != null)
                    {
                        _tilesCollected[GetTileColor(match.Tile.Type)]++;
                    }
                }

                _comboCount++;
                _comboTimer = 1.5f;

                if (_comboCount > 1)
                {
                    ComboTriggered?.Invoke(this, EventArgs.Empty);
                }

                int matchScore = CalculateMatchScore(matches, _comboCount);

                AnimateScorePopup(matches, matchScore);

                _score += matchScore;
                _animatedScoreStart = _displayedScore;
                _animatedScoreEnd = _score;
                _scoreAnimationTimer = 0.5f;

                ScoreChanged?.Invoke(this, _score);

                _gridManager.ProcessMatches(matches);

                await System.Threading.Tasks.Task.Delay((int)(_matchAnimationTime * 1000));

                _gridManager.FillGaps();

                await System.Threading.Tasks.Task.Delay((int)(_fallAnimationTime * 1000));

                while (_gridManager.HasMatches())
                {
                    var cascadeMatches = _gridManager.GetAllMatches();

                    _comboCount++;
                    _comboTimer = 1.5f;

                    if (_comboCount > 1)
                    {
                        ComboTriggered?.Invoke(this, EventArgs.Empty);
                    }

                    int cascadeScore = CalculateMatchScore(cascadeMatches, _comboCount);
                    AnimateScorePopup(cascadeMatches, cascadeScore);
                    _score += cascadeScore;
                    _animatedScoreStart = _displayedScore;
                    _animatedScoreEnd = _score;
                    _scoreAnimationTimer = 0.5f;

                    ScoreChanged?.Invoke(this, _score);

                    _gridManager.ProcessMatches(cascadeMatches);

                    await System.Threading.Tasks.Task.Delay((int)(_matchAnimationTime * 1000));

                    _gridManager.FillGaps();

                    await System.Threading.Tasks.Task.Delay((int)(_fallAnimationTime * 1000));
                }
            }

            _isProcessing = false;

            CheckGameOver();
        }

        private int CalculateMatchScore(List<MatchInfo> matches, int comboMultiplier)
        {
            int baseScore = 0;

            foreach (var match in matches)
            {
                int matchLength = match.Tiles.Count;
                int matchBonus = 0;

                switch (matchLength)
                {
                    case 3:
                        matchBonus = 50;
                        break;
                    case 4:
                        matchBonus = 100;
                        if (match.Shape == MatchShape.Line)
                        {
                            CreateStripedTile(match);
                        }
                        break;
                    case 5:
                        matchBonus = 200;
                        if (match.Shape == MatchShape.Line)
                        {
                            CreateRainbowTile(match);
                        }
                        else if (match.Shape == MatchShape.L || match.Shape == MatchShape.T)
                        {
                            CreateBombTile(match);
                        }
                        break;
                    default:
                        if (matchLength > 5)
                        {
                            matchBonus = 300;
                            CreateRainbowTile(match);
                        }
                        else
                        {
                            matchBonus = 50 * matchLength;
                        }
                        break;
                }

                if (match.Tile != null && IsSpecialTile(match.Tile.Type))
                {
                    matchBonus *= 2;
                }

                baseScore += matchBonus;
            }

            int comboBonus = comboMultiplier > 1 ? (comboMultiplier - 1) * 50 : 0;

            return baseScore * comboMultiplier + comboBonus;
        }

        private void CreateStripedTile(MatchInfo match)
        {
            Vector2Int pos = match.Tiles[match.Tiles.Count / 2].Position;
            TileType colorType = GetBaseTileType(match.Tiles[0].Tile?.Type ?? TileType.Red);

            if (match.IsHorizontal)
            {
                _gridManager.SetTile(pos.X, pos.Y, TileType.StripedHorizontal);
            }
            else
            {
                _gridManager.SetTile(pos.X, pos.Y, TileType.StripedVertical);
            }
        }

        private void CreateBombTile(MatchInfo match)
        {
            Vector2Int pos = match.Tiles[match.Tiles.Count / 2].Position;
            _gridManager.SetTile(pos.X, pos.Y, TileType.Bomb);
        }

        private void CreateRainbowTile(MatchInfo match)
        {
            Vector2Int pos = match.Tiles[match.Tiles.Count / 2].Position;
            _gridManager.SetTile(pos.X, pos.Y, TileType.Rainbow);
        }

        private TileType GetBaseTileType(TileType type)
        {
            return type switch
            {
                TileType.StripedHorizontal or TileType.StripedVertical or TileType.Bomb or TileType.Rainbow => TileType.Red,
                _ => type
            };
        }

        private TileColor GetTileColor(TileType type)
        {
            return type switch
            {
                TileType.Red => TileColor.Red,
                TileType.Blue => TileColor.Blue,
                TileType.Green => TileColor.Green,
                TileType.Yellow => TileColor.Yellow,
                TileType.Purple => TileColor.Purple,
                TileType.Orange => TileColor.Orange,
                TileType.Cyan => TileColor.Cyan,
                TileType.Rainbow => TileColor.Rainbow,
                TileType.StripedHorizontal or TileType.StripedVertical => TileColor.Rainbow,
                TileType.Bomb => TileColor.Rainbow,
                _ => TileColor.Red
            };
        }

        private bool IsSpecialTile(TileType type)
        {
            return type == TileType.StripedHorizontal || type == TileType.StripedVertical ||
                   type == TileType.Bomb || type == TileType.Rainbow;
        }

        private void AnimateScorePopup(List<MatchInfo> matches, int score)
        {
            Vector2 centerPos = Vector2.Zero;
            int count = 0;

            foreach (var match in matches)
            {
                foreach (var tile in match.Tiles)
                {
                    centerPos += new Vector2(tile.Position.X, tile.Position.Y);
                    count++;
                }
            }

            if (count > 0)
            {
                centerPos /= count;
                _scorePopups.Add(new ScorePopup(centerPos, score, _comboCount));
            }
        }

        private void ShowHint()
        {
            if (_isProcessing || _hintTimer > 0)
                return;

            var hint = _gridManager.FindHint();
            if (hint.HasValue)
            {
                _hintPosition = hint.Value;
                _hintTimer = 2f;
            }
        }

        private void CheckLevelObjectives()
        {
            if (_currentLevel == null)
                return;

            bool allObjectivesMet = true;

            foreach (var objective in _currentLevel.Objectives)
            {
                switch (objective.Type)
                {
                    case ObjectiveType.ScoreTarget:
                        if (_score < objective.TargetValue)
                            allObjectivesMet = false;
                        break;

                    case ObjectiveType.CollectTileType:
                        if (_tilesCollected[objective.TileType] < objective.TargetValue)
                            allObjectivesMet = false;
                        break;

                    case ObjectiveType.ClearObstacles:
                        if (_gridManager.GetRemainingObstacles() > 0)
                            allObjectivesMet = false;
                        break;
                }
            }

            if (allObjectivesMet)
            {
                SetGameState(PuzzleGameState.LevelComplete);
                LevelCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        private void CheckGameOver()
        {
            if (_movesRemaining <= 0 && !_gridManager.HasMatches())
            {
                bool allObjectivesMet = true;

                foreach (var objective in _currentLevel.Objectives)
                {
                    switch (objective.Type)
                    {
                        case ObjectiveType.ScoreTarget:
                            if (_score < objective.TargetValue)
                                allObjectivesMet = false;
                            break;

                        case ObjectiveType.CollectTileType:
                            if (_tilesCollected[objective.TileType] < objective.TargetValue)
                                allObjectivesMet = false;
                            break;

                        case ObjectiveType.ClearObstacles:
                            if (_gridManager.GetRemainingObstacles() > 0)
                                allObjectivesMet = false;
                            break;
                    }
                }

                if (!allObjectivesMet)
                {
                    SetGameState(PuzzleGameState.GameOver);
                    GameOver?.Invoke(this, EventArgs.Empty);
                }
                else
                {
                    SetGameState(PuzzleGameState.LevelComplete);
                    LevelCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        public void RestartGame()
        {
            _score = 0;
            _displayedScore = 0;
            _currentLevelIndex = 0;
            LoadLevel(_currentLevelIndex);
            SetGameState(PuzzleGameState.Playing);
        }

        public void RestartLevel()
        {
            LoadLevel(_currentLevelIndex);
            SetGameState(PuzzleGameState.Playing);
        }

        public void NextLevel()
        {
            _currentLevelIndex++;

            if (_currentLevelIndex >= _levels.Count)
            {
                SetGameState(PuzzleGameState.Menu);
            }
            else
            {
                LoadLevel(_currentLevelIndex);
                SetGameState(PuzzleGameState.Playing);
            }
        }

        private void SetGameState(PuzzleGameState newState)
        {
            _previousState = _gameState;
            _gameState = newState;

            switch (_gameState)
            {
                case PuzzleGameState.Playing:
                    _ui.HideAllMenus();
                    _ui.ShowHUD();
                    break;

                case PuzzleGameState.Paused:
                    _ui.ShowPauseMenu();
                    break;

                case PuzzleGameState.GameOver:
                    _ui.ShowGameOverScreen();
                    break;

                case PuzzleGameState.LevelComplete:
                    _ui.ShowLevelComplete();
                    break;

                case PuzzleGameState.Menu:
                    _ui.ShowMainMenu();
                    _ui.HideHUD();
                    break;
            }

            GameStateChanged?.Invoke(this, _gameState);
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            DrawBackground(context);

            if (_gameState == PuzzleGameState.Playing || _gameState == PuzzleGameState.Paused)
            {
                DrawGameBoard(context);
                DrawScorePopups(context);
            }

            DrawUI(context);
        }

        private void DrawBackground(DrawingContext context)
        {
            Color bgColor = _currentLevel?.BackgroundColor ?? new Color(0.2f, 0.2f, 0.3f);

            var brush = new SolidColorBrush(
                System.Windows.Media.Color.FromRgb(
                    (byte)(bgColor.R * 255),
                    (byte)(bgColor.G * 255),
                    (byte)(bgColor.B * 255)
                )
            );

            context.DrawRectangle(brush, null, new Rect(0, 0, 1920, 1080));
        }

        private void DrawGameBoard(DrawingContext context)
        {
            int gridWidth = _gridManager.Width;
            int gridHeight = _gridManager.Height;

            float cellSize = 80f;
            float boardWidth = gridWidth * cellSize;
            float boardHeight = gridHeight * cellSize;
            float boardX = (1920 - boardWidth) / 2;
            float boardY = (1080 - boardHeight) / 2 - 50;

            var boardBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 30, 50));
            var boardPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 60, 100)), 3);
            context.DrawRectangle(boardBrush, boardPen, new Rect(boardX - 10, boardY - 10, boardWidth + 20, boardHeight + 20));

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    float cellX = boardX + x * cellSize;
                    float cellY = boardY + (gridHeight - 1 - y) * cellSize;

                    var obstacle = _gridManager.GetObstacle(x, y);
                    if (obstacle != null)
                    {
                        DrawObstacle(context, cellX, cellY, cellSize, obstacle);
                        continue;
                    }

                    var tile = _gridManager.GetTile(x, y);
                    if (tile != null)
                    {
                        DrawTile(context, cellX, cellY, cellSize, tile, x, y);
                    }

                    if (_selectedTile.X == x && _selectedTile.Y == y)
                    {
                        var selectPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 255, 255, 255)), 3);
                        context.DrawRectangle(null, selectPen, new Rect(cellX, cellY, cellSize, cellSize));
                    }

                    if (_hintPosition.X == x && _hintPosition.Y == y && _hintTimer > 0)
                    {
                        float alpha = (byte)(Math.Sin(_hintTimer * 10) * 127 + 128);
                        var hintPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)alpha, 255, 255, 100)), 3);
                        context.DrawRectangle(null, hintPen, new Rect(cellX, cellY, cellSize, cellSize));
                    }
                }
            }
        }

        private void DrawTile(DrawingContext context, float x, float y, float size, TileType tileType, int gridX, int gridY)
        {
            float padding = 4f;
            float tileSize = size - padding * 2;

            Vector2 offset = _gridManager.GetTileOffset(gridX, gridY);
            float offsetX = offset.X * tileSize;
            float offsetY = offset.Y * tileSize;

            (Color baseColor, Color highlightColor, Color shadowColor) = GetTileColors(tileType);

            float scale = 1f + _gridManager.GetTileScale(gridX, gridY) * 0.1f;
            float scaledSize = tileSize * scale;
            float scaledPadding = (size - scaledSize) / 2;

            var baseBrush = new SolidColorBrush(
                System.Windows.Media.Color.FromRgb((byte)(baseColor.R * 255), (byte)(baseColor.G * 255), (byte)(baseColor.B * 255)));
            var highlightBrush = new SolidColorBrush(
                System.Windows.Media.Color.FromRgb((byte)(highlightColor.R * 255), (byte)(highlightColor.G * 255), (byte)(highlightColor.B * 255)));
            var shadowBrush = new SolidColorBrush(
                System.Windows.Media.Color.FromRgb((byte)(shadowColor.R * 255), (byte)(shadowColor.G * 255), (byte)(shadowColor.B * 255)));

            float rx = x + scaledPadding + offsetX;
            float ry = y + scaledPadding - offsetY;

            context.DrawRectangle(shadowBrush, null, new Rect(rx + 3, ry + 3, scaledSize, scaledSize));
            context.DrawRectangle(baseBrush, null, new Rect(rx, ry, scaledSize, scaledSize));

            context.DrawRectangle(highlightBrush, null, new Rect(rx, ry, scaledSize, 8));
            context.DrawRectangle(highlightBrush, null, new Rect(rx, ry, 8, scaledSize));

            DrawTileIcon(context, rx + scaledSize / 2, ry + scaledSize / 2, scaledSize * 0.6f, tileType);
        }

        private (Color baseColor, Color highlightColor, Color shadowColor) GetTileColors(TileType type)
        {
            return type switch
            {
                TileType.Red => (new Color(0.9f, 0.3f, 0.3f), new Color(1.0f, 0.5f, 0.5f), new Color(0.7f, 0.2f, 0.2f)),
                TileType.Blue => (new Color(0.3f, 0.5f, 0.9f), new Color(0.5f, 0.7f, 1.0f), new Color(0.2f, 0.3f, 0.7f)),
                TileType.Green => (new Color(0.3f, 0.8f, 0.3f), new Color(0.5f, 1.0f, 0.5f), new Color(0.2f, 0.6f, 0.2f)),
                TileType.Yellow => (new Color(0.9f, 0.9f, 0.2f), new Color(1.0f, 1.0f, 0.5f), new Color(0.7f, 0.7f, 0.1f)),
                TileType.Purple => (new Color(0.7f, 0.3f, 0.8f), new Color(0.9f, 0.5f, 1.0f), new Color(0.5f, 0.2f, 0.6f)),
                TileType.Orange => (new Color(0.9f, 0.6f, 0.2f), new Color(1.0f, 0.8f, 0.4f), new Color(0.7f, 0.4f, 0.1f)),
                TileType.Cyan => (new Color(0.2f, 0.8f, 0.8f), new Color(0.4f, 1.0f, 1.0f), new Color(0.1f, 0.6f, 0.6f)),
                TileType.Rainbow => (new Color(0.8f, 0.8f, 0.8f), new Color(1.0f, 1.0f, 1.0f), new Color(0.6f, 0.6f, 0.6f)),
                _ => (new Color(0.5f, 0.5f, 0.5f), new Color(0.7f, 0.7f, 0.7f), new Color(0.3f, 0.3f, 0.3f))
            };
        }

        private void DrawTileIcon(DrawingContext context, float cx, float cy, float size, TileType type)
        {
            float halfSize = size / 2;

            switch (type)
            {
                case TileType.Red:
                    DrawCircle(context, cx, cy, halfSize * 0.8f, System.Windows.Media.Color.FromRgb(255, 100, 100));
                    break;

                case TileType.Blue:
                    DrawDiamond(context, cx, cy, halfSize * 0.8f, System.Windows.Media.Color.FromRgb(100, 150, 255));
                    break;

                case TileType.Green:
                    DrawTriangle(context, cx, cy, halfSize * 0.9f, System.Windows.Media.Color.FromRgb(100, 200, 100));
                    break;

                case TileType.Yellow:
                    DrawStar(context, cx, cy, halfSize * 0.8f, System.Windows.Media.Color.FromRgb(255, 255, 100));
                    break;

                case TileType.Purple:
                    DrawHexagon(context, cx, cy, halfSize * 0.8f, System.Windows.Media.Color.FromRgb(200, 100, 220));
                    break;

                case TileType.Orange:
                    DrawSquare(context, cx, cy, halfSize * 0.7f, System.Windows.Media.Color.FromRgb(255, 150, 80));
                    break;

                case TileType.Cyan:
                    DrawPentagon(context, cx, cy, halfSize * 0.8f, System.Windows.Media.Color.FromRgb(80, 200, 200));
                    break;

                case TileType.StripedHorizontal:
                    DrawStripedH(context, cx, cy, halfSize, System.Windows.Media.Color.FromRgb(255, 255, 255));
                    break;

                case TileType.StripedVertical:
                    DrawStripedV(context, cx, cy, halfSize, System.Windows.Media.Color.FromRgb(255, 255, 255));
                    break;

                case TileType.Bomb:
                    DrawBomb(context, cx, cy, halfSize * 1.2f, System.Windows.Media.Color.FromRgb(50, 50, 50));
                    break;

                case TileType.Rainbow:
                    DrawRainbow(context, cx, cy, halfSize * 0.9f);
                    break;
            }
        }

        private void DrawCircle(DrawingContext context, float cx, float cy, float radius, System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            context.DrawEllipse(brush, null, new Point(cx, cy), radius, radius);
        }

        private void DrawDiamond(DrawingContext context, float cx, float cy, float size, System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(new Point(cx, cy - size), true, true);
                gc.LineTo(new Point(cx + size, cy), true, false);
                gc.LineTo(new Point(cx, cy + size), true, false);
                gc.LineTo(new Point(cx - size, cy), true, false);
            }
            context.DrawGeometry(brush, null, geometry);
        }

        private void DrawTriangle(DrawingContext context, float cx, float cy, float size, System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(new Point(cx, cy - size), true, true);
                gc.LineTo(new Point(cx + size, cy + size * 0.8f), true, false);
                gc.LineTo(new Point(cx - size, cy + size * 0.8f), true, false);
            }
            context.DrawGeometry(brush, null, geometry);
        }

        private void DrawStar(DrawingContext context, float cx, float cy, float size, System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(new Point(cx, cy - size), true, true);
                gc.LineTo(new Point(cx + size * 0.3f, cy - size * 0.3f), true, false);
                gc.LineTo(new Point(cx + size, cy - size * 0.2f), true, false);
                gc.LineTo(new Point(cx + size * 0.4f, cy + size * 0.2f), true, false);
                gc.LineTo(new Point(cx + size * 0.6f, cy + size), true, false);
                gc.LineTo(new Point(cx, cy + size * 0.5f), true, false);
                gc.LineTo(new Point(cx - size * 0.6f, cy + size), true, false);
                gc.LineTo(new Point(cx - size * 0.4f, cy + size * 0.2f), true, false);
                gc.LineTo(new Point(cx - size, cy - size * 0.2f), true, false);
                gc.LineTo(new Point(cx - size * 0.3f, cy - size * 0.3f), true, false);
            }
            context.DrawGeometry(brush, null, geometry);
        }

        private void DrawHexagon(DrawingContext context, float cx, float cy, float size, System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(new Point(cx + size, cy), true, true);
                gc.LineTo(new Point(cx + size * 0.5f, cy + size * 0.866f), true, false);
                gc.LineTo(new Point(cx - size * 0.5f, cy + size * 0.866f), true, false);
                gc.LineTo(new Point(cx - size, cy), true, false);
                gc.LineTo(new Point(cx - size * 0.5f, cy - size * 0.866f), true, false);
                gc.LineTo(new Point(cx + size * 0.5f, cy - size * 0.866f), true, false);
            }
            context.DrawGeometry(brush, null, geometry);
        }

        private void DrawSquare(DrawingContext context, float cx, float cy, float halfSize, System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            context.DrawRectangle(brush, null, new Rect(cx - halfSize, cy - halfSize, halfSize * 2, halfSize * 2));
        }

        private void DrawPentagon(DrawingContext context, float cx, float cy, float size, System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            var geometry = new StreamGeometry();
            using (var gc = geometry.Open())
            {
                gc.BeginFigure(new Point(cx, cy - size), true, true);
                gc.LineTo(new Point(cx + size * 0.951f, cy - size * 0.309f), true, false);
                gc.LineTo(new Point(cx + size * 0.588f, cy + size * 0.809f), true, false);
                gc.LineTo(new Point(cx - size * 0.588f, cy + size * 0.809f), true, false);
                gc.LineTo(new Point(cx - size * 0.951f, cy - size * 0.309f), true, false);
            }
            context.DrawGeometry(brush, null, geometry);
        }

        private void DrawStripedH(DrawingContext context, float cx, float cy, float size, System.Windows.Media.Color color)
        {
            var pen = new Pen(new SolidColorBrush(color), 3);
            for (float i = -size * 0.8f; i <= size * 0.8f; i += size * 0.4f)
            {
                context.DrawLine(pen, new Point(cx + i, cy - size), new Point(cx + i, cy + size));
            }
        }

        private void DrawStripedV(DrawingContext context, float cx, float cy, float size, System.Windows.Media.Color color)
        {
            var pen = new Pen(new SolidColorBrush(color), 3);
            for (float i = -size * 0.8f; i <= size * 0.8f; i += size * 0.4f)
            {
                context.DrawLine(pen, new Point(cx - size, cy + i), new Point(cx + size, cy + i));
            }
        }

        private void DrawBomb(DrawingContext context, float cx, float cy, float size, System.Windows.Media.Color color)
        {
            var brush = new SolidColorBrush(color);
            context.DrawEllipse(brush, null, new Point(cx, cy + size * 0.1f), size * 0.6f, size * 0.6f);

            var fusePen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 90, 43)), 3);
            context.DrawLine(fusePen, new Point(cx, cy - size * 0.5f), new Point(cx + size * 0.3f, cy - size * 0.8f));

            var sparkBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 50));
            context.DrawEllipse(sparkBrush, null, new Point(cx + size * 0.3f, cy - size * 0.8f), size * 0.2f, size * 0.2f);
        }

        private void DrawRainbow(DrawingContext context, float cx, float cy, float size)
        {
            System.Windows.Media.Color[] colors = {
                System.Windows.Media.Color.FromRgb(255, 100, 100),
                System.Windows.Media.Color.FromRgb(100, 150, 255),
                System.Windows.Media.Color.FromRgb(100, 200, 100),
                System.Windows.Media.Color.FromRgb(255, 255, 100),
                System.Windows.Media.Color.FromRgb(200, 100, 220),
                System.Windows.Media.Color.FromRgb(255, 150, 80),
                System.Windows.Media.Color.FromRgb(80, 200, 200)
            };

            float angleStep = (float)(Math.PI * 2 / colors.Length);
            float radius = size * 0.6f;

            for (int i = 0; i < colors.Length; i++)
            {
                float angle1 = i * angleStep - (float)Math.PI / 2;
                float angle2 = (i + 1) * angleStep - (float)Math.PI / 2;

                var geometry = new StreamGeometry();
                using (var gc = geometry.Open())
                {
                    gc.BeginFigure(new Point(cx, cy), true, true);
                    gc.LineTo(new Point(cx + (float)Math.Cos(angle1) * radius, cy + (float)Math.Sin(angle1) * radius), true, false);
                    gc.LineTo(new Point(cx + (float)Math.Cos(angle2) * radius, cy + (float)Math.Sin(angle2) * radius), true, false);
                }

                var brush = new SolidColorBrush(colors[i]);
                context.DrawGeometry(brush, null, geometry);
            }
        }

        private void DrawObstacle(DrawingContext context, float x, float y, float size, ObstacleType type)
        {
            float padding = 2f;

            switch (type)
            {
                case ObstacleType.Locked:
                    var lockedBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 60, 40));
                    var lockedPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 40, 20)), 2);
                    context.DrawRectangle(lockedBrush, lockedPen, new Rect(x + padding, y + padding, size - padding * 2, size - padding * 2));

                    var lockBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(200, 180, 100));
                    context.DrawEllipse(lockBrush, null, new Point(x + size / 2, y + size / 2), size * 0.2f, size * 0.2f);
                    break;

                case ObstacleType.Ice:
                    var iceBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 200, 230, 255));
                    var icePen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 200, 230)), 2);
                    context.DrawRectangle(iceBrush, icePen, new Rect(x + padding, y + padding, size - padding * 2, size - padding * 2));
                    break;

                case ObstacleType.Mud:
                    var mudBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(60, 40, 30));
                    var mudPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 25, 15)), 2);
                    context.DrawRectangle(mudBrush, mudPen, new Rect(x + padding, y + padding, size - padding * 2, size - padding * 2));
                    break;
            }
        }

        private void DrawScorePopups(DrawingContext context)
        {
            foreach (var popup in _scorePopups)
            {
                float x = 960 + popup.Position.X * 80;
                float y = 540 - popup.Position.Y * 80 - popup.HeightOffset;

                var textBrush = popup.Combo > 1
                    ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 50))
                    : new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 255));

                string text = $"+{popup.Score}";
                if (popup.Combo > 1)
                {
                    text += $" x{popup.Combo}";
                }

                var formatted = new FormattedText(
                    text,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new System.Windows.Media.Typeface("Arial Bold"),
                    popup.Combo > 1 ? 24 : 18,
                    textBrush,
                    1.0);

                float alpha = popup.GetAlpha();
                if (alpha < 1f)
                {
                    textBrush.Opacity = alpha;
                }

                context.DrawText(formatted, new Point(x - formatted.Width / 2, y - formatted.Height / 2));
            }
        }

        private void DrawUI(DrawingContext context)
        {
            _ui.Render(context);
        }

        public override void Dispose()
        {
            _ui?.Dispose();
            base.Dispose();
        }
    }

    public class GridManager
    {
        private TileType[,] _grid;
        private ObstacleType[,] _obstacles;
        private int _width;
        private int _height;
        private Dictionary<Vector2Int, float> _tileOffsets;
        private Dictionary<Vector2Int, float> _tileScales;
        private Random _random;

        public int Width => _width;
        public int Height => _height;

        public GridManager()
        {
            _random = new Random();
            _tileOffsets = new Dictionary<Vector2Int, float>();
            _tileScales = new Dictionary<Vector2Int, float>();
        }

        public void Initialize(int width, int height)
        {
            _width = width;
            _height = height;
            _grid = new TileType[width, height];
            _obstacles = new ObstacleType[width, height];
            _tileOffsets.Clear();
            _tileScales.Clear();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    _grid[x, y] = TileType.Red;
                    _obstacles[x, y] = ObstacleType.None;
                }
            }
        }

        public void SetObstacles(List<ObstacleData> obstacles)
        {
            foreach (var obstacle in obstacles)
            {
                if (IsValidPosition(obstacle.X, obstacle.Y))
                {
                    _obstacles[obstacle.X, obstacle.Y] = obstacle.Type;
                }
            }
        }

        public void SetTile(int x, int y, TileType type)
        {
            if (!IsValidPosition(x, y))
                return;

            _grid[x, y] = type;
            _tileOffsets[new Vector2Int(x, y)] = 0f;
            _tileScales[new Vector2Int(x, y)] = 0f;
        }

        public TileType? GetTile(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return null;

            return _grid[x, y];
        }

        public bool HasObstacle(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return true;

            return _obstacles[x, y] != ObstacleType.None;
        }

        public ObstacleType? GetObstacle(int x, int y)
        {
            if (!IsValidPosition(x, y))
                return ObstacleType.None;

            return _obstacles[x, y];
        }

        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }

        public void SwapTiles(int x1, int y1, int x2, int y2)
        {
            if (!IsValidPosition(x1, y1) || !IsValidPosition(x2, y2))
                return;

            if (HasObstacle(x1, y1) || HasObstacle(x2, y2))
                return;

            (_grid[x1, y1], _grid[x2, y2]) = (_grid[x2, y2], _grid[x1, y1]);
        }

        public bool HasMatches()
        {
            return GetAllMatches().Count > 0;
        }

        public List<MatchInfo> GetAllMatches()
        {
            var matches = new List<MatchInfo>();

            matches.AddRange(FindHorizontalMatches());
            matches.AddRange(FindVerticalMatches());

            return MergeOverlappingMatches(matches);
        }

        private List<MatchInfo> FindHorizontalMatches()
        {
            var matches = new List<MatchInfo>();

            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width - 2; x++)
                {
                    if (HasObstacle(x, y))
                        continue;

                    var tile = _grid[x, y];
                    if (tile == TileType.Rainbow || IsSpecialTile(tile))
                        continue;

                    int matchLength = 1;
                    while (x + matchLength < _width &&
                           !HasObstacle(x + matchLength, y) &&
                           CanMatch(_grid[x + matchLength, y], tile))
                    {
                        matchLength++;
                    }

                    if (matchLength >= 3)
                    {
                        var matchTiles = new List<TileInfo>();
                        for (int i = 0; i < matchLength; i++)
                        {
                            matchTiles.Add(new TileInfo
                            {
                                Position = new Vector2Int(x + i, y),
                                Tile = _grid[x + i, y]
                            });
                        }

                        matches.Add(new MatchInfo
                        {
                            Tiles = matchTiles,
                            Shape = matchLength >= 5 ? MatchShape.Line : (matchLength == 4 ? MatchShape.Line : MatchShape.Line),
                            Direction = MatchDirection.Horizontal,
                            IsHorizontal = true
                        });
                    }

                    x += matchLength - 1;
                }
            }

            return matches;
        }

        private List<MatchInfo> FindVerticalMatches()
        {
            var matches = new List<MatchInfo>();

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height - 2; y++)
                {
                    if (HasObstacle(x, y))
                        continue;

                    var tile = _grid[x, y];
                    if (tile == TileType.Rainbow || IsSpecialTile(tile))
                        continue;

                    int matchLength = 1;
                    while (y + matchLength < _height &&
                           !HasObstacle(x, y + matchLength) &&
                           CanMatch(_grid[x, y + matchLength], tile))
                    {
                        matchLength++;
                    }

                    if (matchLength >= 3)
                    {
                        var matchTiles = new List<TileInfo>();
                        for (int i = 0; i < matchLength; i++)
                        {
                            matchTiles.Add(new TileInfo
                            {
                                Position = new Vector2Int(x, y + i),
                                Tile = _grid[x, y + i]
                            });
                        }

                        matches.Add(new MatchInfo
                        {
                            Tiles = matchTiles,
                            Shape = matchLength >= 5 ? MatchShape.Line : (matchLength == 4 ? MatchShape.Line : MatchShape.Line),
                            Direction = MatchDirection.Vertical,
                            IsHorizontal = false
                        });
                    }

                    y += matchLength - 1;
                }
            }

            return matches;
        }

        private bool CanMatch(TileType tile1, TileType tile2)
        {
            if (tile1 == tile2)
                return true;

            if (tile1 == TileType.Rainbow || tile2 == TileType.Rainbow)
                return true;

            return false;
        }

        private bool IsSpecialTile(TileType type)
        {
            return type == TileType.StripedHorizontal ||
                   type == TileType.StripedVertical ||
                   type == TileType.Bomb ||
                   type == TileType.Rainbow;
        }

        private List<MatchInfo> MergeOverlappingMatches(List<MatchInfo> matches)
        {
            var merged = new List<MatchInfo>();
            var processed = new HashSet<Vector2Int>();

            foreach (var match in matches)
            {
                var positions = match.Tiles.Select(t => t.Position).ToHashSet();

                bool shouldMerge = false;
                foreach (var other in matches)
                {
                    if (other == match)
                        continue;

                    var otherPositions = other.Tiles.Select(t => t.Position).ToHashSet();

                    if (positions.Overlaps(otherPositions))
                    {
                        shouldMerge = true;
                        break;
                    }
                }

                if (!shouldMerge)
                {
                    merged.Add(match);
                }
            }

            return merged.Count > 0 ? matches : merged;
        }

        public void ProcessMatches(List<MatchInfo> matches)
        {
            foreach (var match in matches)
            {
                foreach (var tile in match.Tiles)
                {
                    if (IsValidPosition(tile.Position.X, tile.Position.Y))
                    {
                        _grid[tile.Position.X, tile.Position.Y] = TileType.Red;
                    }
                }
            }
        }

        public void FillGaps()
        {
            for (int x = 0; x < _width; x++)
            {
                int emptySpaces = 0;

                for (int y = 0; y < _height; y++)
                {
                    if (HasObstacle(x, y))
                        continue;

                    if (_grid[x, y] == TileType.Red &&
                        (_tileOffsets.TryGetValue(new Vector2Int(x, y), out float offset) && offset == 0))
                    {
                        bool isEmpty = true;
                        for (int checkY = 0; checkY < _height; checkY++)
                        {
                            if (checkY != y)
                            {
                                var checkPos = new Vector2Int(x, checkY);
                                if (_tileOffsets.TryGetValue(checkPos, out float checkOffset) && checkOffset == 0)
                                {
                                    isEmpty = false;
                                    break;
                                }
                            }
                        }

                        if (isEmpty)
                        {
                            _grid[x, y] = (TileType)_random.Next(7);
                        }
                    }
                }

                for (int y = _height - 1; y >= 0; y--)
                {
                    if (HasObstacle(x, y))
                        continue;

                    if (_grid[x, y] == TileType.Red)
                    {
                        int targetY = y;
                        while (targetY > 0)
                        {
                            targetY--;
                            if (HasObstacle(x, targetY))
                            {
                                targetY++;
                                break;
                            }
                        }

                        if (targetY >= 0 && targetY < y)
                        {
                            _grid[x, y] = _grid[x, targetY];
                            _grid[x, targetY] = TileType.Red;
                        }
                    }
                }

                for (int y = _height - 1; y >= 0; y--)
                {
                    if (HasObstacle(x, y))
                        continue;

                    if (_grid[x, y] == TileType.Red)
                    {
                        _grid[x, y] = (TileType)_random.Next(7);
                        _tileOffsets[new Vector2Int(x, y)] = _height - y;
                    }
                }
            }
        }

        public Vector2Int? FindHint()
        {
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (HasObstacle(x, y))
                        continue;

                    if (x < _width - 1 && !HasObstacle(x + 1, y))
                    {
                        SwapTiles(x, y, x + 1, y);
                        bool hasMatch = HasMatches();
                        SwapTiles(x, y, x + 1, y);

                        if (hasMatch)
                            return new Vector2Int(x, y);
                    }

                    if (y < _height - 1 && !HasObstacle(x, y + 1))
                    {
                        SwapTiles(x, y, x, y + 1);
                        bool hasMatch = HasMatches();
                        SwapTiles(x, y, x, y + 1);

                        if (hasMatch)
                            return new Vector2Int(x, y);
                    }
                }
            }

            return null;
        }

        public int GetRemainingObstacles()
        {
            int count = 0;
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    if (HasObstacle(x, y))
                        count++;
                }
            }
            return count;
        }

        public Vector2 GetTileOffset(int x, int y)
        {
            if (_tileOffsets.TryGetValue(new Vector2Int(x, y), out float offset))
            {
                return new Vector2(0, offset);
            }
            return Vector2.Zero;
        }

        public float GetTileScale(int x, int y)
        {
            if (_tileScales.TryGetValue(new Vector2Int(x, y), out float scale))
            {
                return scale;
            }
            return 0f;
        }

        public void UpdateAnimations(float deltaTime)
        {
            var keysToRemove = new List<Vector2Int>();

            foreach (var kvp in _tileOffsets)
            {
                if (kvp.Value > 0)
                {
                    _tileOffsets[kvp.Key] = Math.Max(0, kvp.Value - deltaTime * 10);
                }
                else
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _tileOffsets.Remove(key);
            }

            keysToRemove.Clear();

            foreach (var kvp in _tileScales)
            {
                if (kvp.Value > 0)
                {
                    _tileScales[kvp.Key] = Math.Max(0, kvp.Value - deltaTime * 5);
                }
                else
                {
                    keysToRemove.Add(kvp.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                _tileScales.Remove(key);
            }
        }
    }

    public struct TileInfo
    {
        public Vector2Int Position;
        public TileType Tile;
    }

    public class MatchInfo
    {
        public List<TileInfo> Tiles { get; set; } = new List<TileInfo>();
        public MatchShape Shape { get; set; }
        public MatchDirection Direction { get; set; }
        public bool IsHorizontal { get; set; }
        public TileType? Tile => Tiles.Count > 0 ? Tiles[0].Tile : null;
        public Vector2Int Position => Tiles.Count > 0 ? Tiles[Tiles.Count / 2].Position : Vector2Int.Zero;
    }

    public enum MatchShape
    {
        Line,
        L,
        T
    }

    public enum MatchDirection
    {
        Horizontal,
        Vertical
    }

    public class LevelData
    {
        public int LevelNumber { get; set; }
        public int GridWidth { get; set; }
        public int GridHeight { get; set; }
        public int Moves { get; set; }
        public int ScoreTarget { get; set; }
        public Color BackgroundColor { get; set; }
        public List<Objective> Objectives { get; set; } = new List<Objective>();
        public List<ObstacleData> Obstacles { get; set; } = new List<ObstacleData>();
    }

    public class Objective
    {
        public ObjectiveType Type { get; set; }
        public TileColor TileType { get; set; }
        public int TargetValue { get; set; }
    }

    public class ObstacleData
    {
        public int X { get; set; }
        public int Y { get; set; }
        public ObstacleType Type { get; set; }
    }

    public class ScorePopup
    {
        public Vector2 Position { get; set; }
        public int Score { get; set; }
        public int Combo { get; set; }
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }
        public float HeightOffset { get; set; }

        public bool IsExpired => Lifetime >= MaxLifetime;

        public ScorePopup(Vector2 position, int score, int combo)
        {
            Position = position;
            Score = score;
            Combo = combo;
            Lifetime = 0f;
            MaxLifetime = 1.5f;
            HeightOffset = 0f;
        }

        public float GetAlpha()
        {
            return Math.Max(0, 1f - (Lifetime / MaxLifetime));
        }

        public void Update(float deltaTime)
        {
            Lifetime += deltaTime;
            HeightOffset += deltaTime * 80;
        }
    }

    public class PuzzleUI : IDisposable
    {
        private PuzzleGame? _game;
        private bool _isInitialized;
        private bool _hudVisible;
        private bool _pauseMenuVisible;
        private bool _gameOverVisible;
        private bool _levelCompleteVisible;
        private bool _mainMenuVisible;
        private int _displayedScore;
        private int _displayedMoves;
        private float _scoreUpdateTimer;

        public PuzzleUI()
        {
            _isInitialized = false;
            _hudVisible = false;
            _pauseMenuVisible = false;
            _gameOverVisible = false;
            _levelCompleteVisible = false;
            _mainMenuVisible = false;
            _displayedScore = 0;
            _displayedMoves = 0;
            _scoreUpdateTimer = 0f;
        }

        public void Initialize(PuzzleGame game)
        {
            _game = game;
            _isInitialized = true;
        }

        public void LoadLevel(LevelData level)
        {
            _displayedScore = 0;
            _displayedMoves = level.Moves;
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

        public void ShowLevelComplete()
        {
            HideAllMenus();
            _levelCompleteVisible = true;
        }

        public void HideAllMenus()
        {
            _mainMenuVisible = false;
            _hudVisible = false;
            _pauseMenuVisible = false;
            _gameOverVisible = false;
            _levelCompleteVisible = false;
        }

        public void Update(float deltaTime)
        {
            if (_game != null)
            {
                if (_displayedScore < _game.Score)
                {
                    int diff = _game.Score - _displayedScore;
                    _displayedScore += (int)(diff * deltaTime * 10);
                    if (_displayedScore > _game.Score)
                        _displayedScore = _game.Score;
                }

                _displayedMoves = _game.MovesRemaining;
            }
        }

        public void Render(DrawingContext context)
        {
            if (_mainMenuVisible)
            {
                RenderMainMenu(context);
            }

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
        }

        private void RenderHUD(DrawingContext context)
        {
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 30, 30, 40));
            var textBrush = new SolidColorBrush(Colors.White);
            var goldBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0));

            context.DrawRectangle(backgroundBrush, null, new Rect(20, 20, 250, 150));

            var levelText = new FormattedText(
                $"Level {_game?.CurrentLevelIndex + 1}",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial Bold"),
                24,
                goldBrush,
                1.0);
            context.DrawText(levelText, new Point(30, 30));

            var scoreLabel = new FormattedText(
                "SCORE",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                14,
                textBrush,
                1.0);
            context.DrawText(scoreLabel, new Point(30, 65));

            var scoreText = new FormattedText(
                $"{_displayedScore:N0}",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial Bold"),
                28,
                textBrush,
                1.0);
            context.DrawText(scoreText, new Point(30, 85));

            var movesLabel = new FormattedText(
                "MOVES",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                14,
                textBrush,
                1.0);
            context.DrawText(movesLabel, new Point(30, 125));

            var movesText = new FormattedText(
                $"{_displayedMoves}",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial Bold"),
                28,
                _displayedMoves <= 5 ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 80, 80)) : textBrush,
                1.0);
            context.DrawText(movesText, new Point(30, 145));

            RenderObjectives(context);
        }

        private void RenderObjectives(DrawingContext context)
        {
            if (_game?._currentLevel == null)
                return;

            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 30, 30, 40));
            context.DrawRectangle(backgroundBrush, null, new Rect(20, 190, 250, 100));

            var textBrush = new SolidColorBrush(Colors.White);

            var objectivesText = new FormattedText(
                "OBJECTIVES",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial Bold"),
                16,
                textBrush,
                1.0);
            context.DrawText(objectivesText, new Point(30, 200));

            int yOffset = 225;
            foreach (var objective in _game._currentLevel.Objectives)
            {
                string objectiveStr = "";
                bool completed = false;

                switch (objective.Type)
                {
                    case ObjectiveType.ScoreTarget:
                        objectiveStr = $"Score: {_game.Score}/{objective.TargetValue}";
                        completed = _game.Score >= objective.TargetValue;
                        break;

                    case ObjectiveType.CollectTileType:
                        int collected = _game._tilesCollected[objective.TileType];
                        objectiveStr = $"{objective.TileType}: {collected}/{objective.TargetValue}";
                        completed = collected >= objective.TargetValue;
                        break;
                }

                var objBrush = completed
                    ? new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 255, 100))
                    : textBrush;

                var objText = new FormattedText(
                    objectiveStr,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new System.Windows.Media.Typeface("Arial"),
                    14,
                    objBrush,
                    1.0);
                context.DrawText(objText, new Point(30, yOffset));
                yOffset += 20;
            }
        }

        private void RenderPauseMenu(DrawingContext context)
        {
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0));
            context.DrawRectangle(overlayBrush, null, new Rect(0, 0, 1920, 1080));

            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 40, 50));
            var panelPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromRgb(80, 80, 100)), 2);
            context.DrawRectangle(panelBrush, panelPen, new Rect(760, 384, 400, 312));

            var textBrush = new SolidColorBrush(Colors.White);
            var goldBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0));

            var titleText = new FormattedText(
                "PAUSED",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial Bold"),
                48,
                goldBrush,
                1.0);
            context.DrawText(titleText, new Point(960 - titleText.Width / 2, 420));

            var resumeText = new FormattedText(
                "Press ESC to Resume",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                24,
                textBrush,
                1.0);
            context.DrawText(resumeText, new Point(960 - resumeText.Width / 2, 520));

            var quitText = new FormattedText(
                "Press Q to Quit",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                20,
                textBrush,
                1.0);
            context.DrawText(quitText, new Point(960 - quitText.Width / 2, 570));
        }

        private void RenderGameOver(DrawingContext context)
        {
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 0, 0, 0));
            context.DrawRectangle(overlayBrush, null, new Rect(0, 0, 1920, 1080));

            var textBrush = new SolidColorBrush(Colors.White);
            var redBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 80, 80));

            var titleText = new FormattedText(
                "GAME OVER",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial Bold"),
                64,
                redBrush,
                1.0);
            context.DrawText(titleText, new Point(960 - titleText.Width / 2, 350));

            var scoreText = new FormattedText(
                $"Final Score: {_displayedScore:N0}",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                32,
                textBrush,
                1.0);
            context.DrawText(scoreText, new Point(960 - scoreText.Width / 2, 450));

            var restartText = new FormattedText(
                "Press ENTER to Restart",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                24,
                textBrush,
                1.0);
            context.DrawText(restartText, new Point(960 - restartText.Width / 2, 530));

            var menuText = new FormattedText(
                "Press ESC for Main Menu",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                18,
                textBrush,
                1.0);
            context.DrawText(menuText, new Point(960 - menuText.Width / 2, 580));
        }

        private void RenderLevelComplete(DrawingContext context)
        {
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0));
            context.DrawRectangle(overlayBrush, null, new Rect(0, 0, 1920, 1080));

            var textBrush = new SolidColorBrush(Colors.White);
            var goldBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0));
            var greenBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(100, 255, 100));

            var titleText = new FormattedText(
                "LEVEL COMPLETE!",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial Bold"),
                56,
                goldBrush,
                1.0);
            context.DrawText(titleText, new Point(960 - titleText.Width / 2, 320));

            var levelText = new FormattedText(
                $"Level {_game?.CurrentLevelIndex + 1} Completed",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                28,
                greenBrush,
                1.0);
            context.DrawText(levelText, new Point(960 - levelText.Width / 2, 400));

            var scoreText = new FormattedText(
                $"Score: {_displayedScore:N0}",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                32,
                textBrush,
                1.0);
            context.DrawText(scoreText, new Point(960 - scoreText.Width / 2, 460));

            var movesText = new FormattedText(
                $"Moves Remaining: {_displayedMoves}",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                24,
                textBrush,
                1.0);
            context.DrawText(movesText, new Point(960 - movesText.Width / 2, 510));

            var continueText = new FormattedText(
                "Press ENTER to Continue",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                24,
                textBrush,
                1.0);
            context.DrawText(continueText, new Point(960 - continueText.Width / 2, 600));
        }

        private void RenderMainMenu(DrawingContext context)
        {
            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(20, 30, 50));
            context.DrawRectangle(backgroundBrush, null, new Rect(0, 0, 1920, 1080));

            var titleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0));
            var textBrush = new SolidColorBrush(Colors.White);
            var subtitleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(180, 180, 180));

            var titleText = new FormattedText(
                "MATCH-3 PUZZLE",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial Bold"),
                72,
                titleBrush,
                1.0);
            context.DrawText(titleText, new Point(960 - titleText.Width / 2, 280));

            var startText = new FormattedText(
                "Press ENTER to Start",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                32,
                textBrush,
                1.0);
            context.DrawText(startText, new Point(960 - startText.Width / 2, 420));

            var controlsText = new FormattedText(
                "Click to swap tiles - Match 3 or more",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                20,
                subtitleBrush,
                1.0);
            context.DrawText(controlsText, new Point(960 - controlsText.Width / 2, 520));

            var hintText = new FormattedText(
                "H = Show Hint",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new System.Windows.Media.Typeface("Arial"),
                18,
                subtitleBrush,
                1.0);
            context.DrawText(hintText, new Point(960 - hintText.Width / 2, 560));

            DrawMenuDecorations(context);
        }

        private void DrawMenuDecorations(DrawingContext context)
        {
            System.Windows.Media.Color[] colors = {
                System.Windows.Media.Color.FromRgb(255, 100, 100),
                System.Windows.Media.Color.FromRgb(100, 150, 255),
                System.Windows.Media.Color.FromRgb(100, 200, 100),
                System.Windows.Media.Color.FromRgb(255, 255, 100),
                System.Windows.Media.Color.FromRgb(200, 100, 220),
                System.Windows.Media.Color.FromRgb(255, 150, 80),
                System.Windows.Media.Color.FromRgb(80, 200, 200)
            };

            float time = (float)(DateTime.Now.TimeOfDay.TotalSeconds);

            for (int i = 0; i < colors.Length; i++)
            {
                float angle = time + i * (float)(Math.PI * 2 / colors.Length);
                float radius = 300 + (float)Math.Sin(time * 2 + i) * 30;
                float x = 960 + (float)Math.Cos(angle) * radius;
                float y = 540 + (float)Math.Sin(angle) * radius * 0.6f;

                var brush = new SolidColorBrush(colors[i]);
                float size = 15 + (float)Math.Sin(time * 3 + i) * 5;
                context.DrawEllipse(brush, null, new Point(x, y), size, size);
            }
        }

        public void Dispose()
        {
            _isInitialized = false;
        }
    }

    public class Vector2Int : IEquatable<Vector2Int>
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static readonly Vector2Int Zero = new Vector2Int(0, 0);
        public static readonly Vector2Int One = new Vector2Int(1, 1);
        public static readonly Vector2Int Up = new Vector2Int(0, 1);
        public static readonly Vector2Int Down = new Vector2Int(0, -1);
        public static readonly Vector2Int Left = new Vector2Int(-1, 0);
        public static readonly Vector2Int Right = new Vector2Int(1, 0);

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public static Vector2Int operator +(Vector2Int a, Vector2Int b) => new Vector2Int(a.X + b.X, a.Y + b.Y);
        public static Vector2Int operator -(Vector2Int a, Vector2Int b) => new Vector2Int(a.X - b.X, a.Y - b.Y);
        public static bool operator ==(Vector2Int a, Vector2Int b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Vector2Int a, Vector2Int b) => !(a == b);

        public override bool Equals(object? obj) => obj is Vector2Int other && Equals(other);
        public bool Equals(Vector2Int other) => X == other.X && Y == other.Y;
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X}, {Y})";
    }
}

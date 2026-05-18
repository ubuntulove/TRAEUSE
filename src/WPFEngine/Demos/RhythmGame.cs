using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WPFEngine.Core;
using WPFEngine.Input;
using WPFEngine.Audio;

namespace WPFEngine.Demos
{
    public enum RhythmGameState
    {
        Menu,
        SongSelect,
        Playing,
        Paused,
        Results,
        Settings
    }

    public enum NoteType
    {
        Tap,
        Hold,
        Slide,
        Spin
    }

    public enum HitRating
    {
        Perfect,
        Great,
        Good,
        Bad,
        Miss
    }

    public enum Difficulty
    {
        Easy,
        Normal,
        Hard,
        Expert
    }

    public enum SlideDirection
    {
        None,
        Left,
        Right,
        Up,
        Down
    }

    public enum PracticeMode
    {
        Off,
        MetronomeOnly,
        SlowMusic,
        SectionRepeat
    }

    public class RhythmGame : GameBase
    {
        private RhythmGameState _gameState;
        private RhythmGameState _previousState;
        private RhythmUI _ui;
        private NoteManager _noteManager;
        private List<Beatmap> _beatmaps;
        private Beatmap _currentBeatmap;
        private int _selectedSongIndex;
        private int _selectedDifficulty;
        private float _gameTime;
        private float _songTime;
        private float _audioOffset;
        private float _noteSpeed;
        private bool _isMetronomeEnabled;
        private int _laneCount;
        private Dictionary<int, InputManager.KeyCode> _keyLaneMapping;
        private Dictionary<int, bool> _keyPressedStates;
        private Dictionary<int, Vector2> _previousKeyPositions;
        private Random _random;
        private Lane[] _lanes;

        private int _score;
        private int _combo;
        private int _maxCombo;
        private float _accuracy;
        private int _perfectCount;
        private int _greatCount;
        private int _goodCount;
        private int _badCount;
        private int _missCount;
        private int _totalNotes;
        private List<HitResult> _hitResults;

        private float _backgroundPulse;
        private float _pulseTimer;
        private List<VisualEffect> _visualEffects;
        private List<ParticleEffect> _particles;
        private float[] _laneFlash;
        private float[] _laneApproach;

        private float _currentBPM;
        private float _beatInterval;
        private float _beatTimer;
        private int _currentBeat;
        private float _metronomeVolume;
        private bool _musicStarted;
        private int _timeSignatureNumerator;
        private int _timeSignatureDenominator;
        private List<BPMChange> _bpmChanges;
        private int _currentBPMChangeIndex;
        private float _sectionStartTime;
        private float _sectionRepeatEnd;

        private PracticeMode _practiceMode;
        private float _slowMotionFactor;
        private bool _isSlowMotionActive;
        private int _sectionRepeatCount;
        private float _sectionStartMarker;
        private float _sectionEndMarker;

        private RhythmSettings _settings;
        private AudioCalibration _audioCalibration;

        private SlideTracker[] _slideTrackers;
        private SpinTracker[] _spinTrackers;

        public RhythmGameState GameState => _gameState;
        public int Score => _score;
        public int Combo => _combo;
        public int MaxCombo => _maxCombo;
        public float Accuracy => _accuracy;
        public float Progress => _totalNotes > 0 ? (float)(_perfectCount + _greatCount + _goodCount + _badCount + _missCount) / _totalNotes : 0;
        public RhythmSettings Settings => _settings;
        public Lane[] Lanes => _lanes;

        public RhythmGame()
        {
            _gameState = RhythmGameState.Menu;
            _previousState = RhythmGameState.Menu;
            _selectedSongIndex = 0;
            _selectedDifficulty = 1;
            _gameTime = 0;
            _songTime = 0;
            _audioOffset = 0;
            _noteSpeed = 400f;
            _isMetronomeEnabled = false;
            _laneCount = 4;
            _keyLaneMapping = new Dictionary<int, InputManager.KeyCode>();
            _keyPressedStates = new Dictionary<int, bool>();
            _previousKeyPositions = new Dictionary<int, Vector2>();
            _random = new Random();

            _score = 0;
            _combo = 0;
            _maxCombo = 0;
            _accuracy = 100f;
            _perfectCount = 0;
            _greatCount = 0;
            _goodCount = 0;
            _badCount = 0;
            _missCount = 0;
            _totalNotes = 0;
            _hitResults = new List<HitResult>();

            _backgroundPulse = 0f;
            _pulseTimer = 0f;
            _visualEffects = new List<VisualEffect>();
            _particles = new List<ParticleEffect>();
            _laneFlash = new float[8];
            _laneApproach = new float[8];

            _currentBPM = 120f;
            _beatInterval = 0.5f;
            _beatTimer = 0f;
            _currentBeat = 0;
            _metronomeVolume = 0.3f;
            _musicStarted = false;
            _timeSignatureNumerator = 4;
            _timeSignatureDenominator = 4;
            _bpmChanges = new List<BPMChange>();
            _currentBPMChangeIndex = 0;
            _sectionStartTime = 0;
            _sectionRepeatEnd = 0;

            _practiceMode = PracticeMode.Off;
            _slowMotionFactor = 0.5f;
            _isSlowMotionActive = false;
            _sectionRepeatCount = 0;
            _sectionStartMarker = -1;
            _sectionEndMarker = -1;

            _settings = new RhythmSettings();
            _audioCalibration = new AudioCalibration();

            _lanes = new Lane[8];
            _slideTrackers = new SlideTracker[8];
            _spinTrackers = new SpinTracker[8];

            InitializeKeyMapping();
            InitializeLanes();
            InitializeTrackers();
        }

        private void InitializeKeyMapping()
        {
            _keyLaneMapping[0] = InputManager.KeyCode.D1;
            _keyLaneMapping[1] = InputManager.KeyCode.D2;
            _keyLaneMapping[2] = InputManager.KeyCode.D3;
            _keyLaneMapping[3] = InputManager.KeyCode.D4;
            _keyLaneMapping[4] = InputManager.KeyCode.D5;
            _keyLaneMapping[5] = InputManager.KeyCode.D6;
            _keyLaneMapping[6] = InputManager.KeyCode.D7;
            _keyLaneMapping[7] = InputManager.KeyCode.D8;

            for (int i = 0; i < 8; i++)
            {
                _keyPressedStates[i] = false;
                _previousKeyPositions[i] = Vector2.Zero;
            }
        }

        private void InitializeLanes()
        {
            for (int i = 0; i < 8; i++)
            {
                _lanes[i] = new Lane
                {
                    Index = i,
                    Position = Vector2.Zero,
                    IsActive = false,
                    FlashIntensity = 0f,
                    ApproachIntensity = 0f,
                    HitCount = 0,
                    MissCount = 0,
                    Color = GetLaneColor(i)
                };
            }
        }

        private void InitializeTrackers()
        {
            for (int i = 0; i < 8; i++)
            {
                _slideTrackers[i] = new SlideTracker();
                _spinTrackers[i] = new SpinTracker();
            }
        }

        private Color GetLaneColor(int laneIndex)
        {
            Color[] laneColors = {
                Color.FromRgb(255, 100, 100),
                Color.FromRgb(100, 255, 100),
                Color.FromRgb(100, 100, 255),
                Color.FromRgb(255, 255, 100),
                Color.FromRgb(255, 100, 255),
                Color.FromRgb(100, 255, 255),
                Color.FromRgb(255, 150, 100),
                Color.FromRgb(150, 100, 255)
            };
            return laneColors[laneIndex % laneColors.Length];
        }

        protected override void Initialize()
        {
            base.Initialize();

            AudioManager.Instance.Initialize();
            AudioManager.Instance.MasterVolume = _settings.MasterVolume;
            AudioManager.Instance.MusicVolume = _settings.MusicVolume;
            AudioManager.Instance.SfxVolume = _settings.SfxVolume;

            _ui = new RhythmUI();
            _ui.Initialize(this);

            _noteManager = new NoteManager(_laneCount);

            CreateSampleBeatmaps();

            _gameState = RhythmGameState.Menu;
        }

        private void CreateSampleBeatmaps()
        {
            _beatmaps = new List<Beatmap>();

            _beatmaps.Add(CreateDemoBeatmap("Neon Pulse", "Synthwave", Difficulty.Easy, 100f, 0.8f));
            _beatmaps.Add(CreateDemoBeatmap("Electric Dreams", "Digital Artist", Difficulty.Normal, 120f, 0.6f));
            _beatmaps.Add(CreateDemoBeatmap("Cyber Rush", "Future Beat", Difficulty.Hard, 140f, 0.45f));
            _beatmaps.Add(CreateDemoBeatmap("Hyperdrive", "Speed Master", Difficulty.Expert, 160f, 0.35f));
            _beatmaps.Add(CreateBPMChangeBeatmap());
            _beatmaps.Add(CreateComplexPatternBeatmap());

            _currentBeatmap = _beatmaps[0];
        }

        private Beatmap CreateDemoBeatmap(string name, string artist, Difficulty difficulty, float bpm, float noteInterval)
        {
            var beatmap = new Beatmap
            {
                Name = name,
                Artist = artist,
                Difficulty = difficulty,
                BPM = bpm,
                Duration = 60f,
                Offset = 0f,
                Level = difficulty
            };

            float duration = beatmap.Duration;
            int maxCombo = (int)(duration / noteInterval);

            for (int i = 0; i < maxCombo; i++)
            {
                float time = i * noteInterval + 2f;
                int lane = GetNoteLane(difficulty, i, maxCombo);

                var noteType = DetermineNoteType(difficulty, i, maxCombo);
                float noteDuration = noteType == NoteType.Hold ? 0.3f + (_random.NextDouble() * 0.3) : 0;

                var note = new NoteData
                {
                    Timing = time,
                    Lane = lane,
                    Type = noteType,
                    Duration = (float)noteDuration,
                    Spawned = false
                };

                beatmap.Notes.Add(note);
            }

            if (difficulty >= Difficulty.Normal)
            {
                AddRhythmPatterns(beatmap, difficulty, noteInterval);
            }

            if (difficulty >= Difficulty.Hard)
            {
                AddChordPatterns(beatmap, difficulty);
                AddStreamPatterns(beatmap, difficulty);
            }

            return beatmap;
        }

        private Beatmap CreateBPMChangeBeatmap()
        {
            var beatmap = new Beatmap
            {
                Name = "BPM Adventure",
                Artist = "Dynamic Composer",
                Difficulty = Difficulty.Hard,
                BPM = 120f,
                Duration = 90f,
                Offset = 0f,
                Level = Difficulty.Hard
            };

            beatmap.BPMChanges.Add(new BPMChange { Time = 0, BPM = 100 });
            beatmap.BPMChanges.Add(new BPMChange { Time = 20, BPM = 130 });
            beatmap.BPMChanges.Add(new BPMChange { Time = 40, BPM = 100 });
            beatmap.BPMChanges.Add(new BPMChange { Time = 50, BPM = 150 });
            beatmap.BPMChanges.Add(new BPMChange { Time = 70, BPM = 120 });

            float currentTime = 2f;
            float sectionBPM = 100f;
            int sectionIndex = 0;

            while (currentTime < beatmap.Duration)
            {
                if (sectionIndex < beatmap.BPMChanges.Count && currentTime >= beatmap.BPMChanges[sectionIndex].Time)
                {
                    sectionBPM = beatmap.BPMChanges[sectionIndex].BPM;
                    sectionIndex++;
                }

                float beatDuration = 60f / sectionBPM;
                float timeUntilNextNote = beatDuration * GetNoteDensity(sectionIndex);

                var note = new NoteData
                {
                    Timing = currentTime,
                    Lane = _random.Next(_laneCount),
                    Type = NoteType.Tap,
                    Duration = 0,
                    Spawned = false
                };

                beatmap.Notes.Add(note);
                currentTime += timeUntilNextNote;
            }

            return beatmap;
        }

        private float GetNoteDensity(int sectionIndex)
        {
            float[] densities = { 1f, 0.5f, 0.25f, 0.5f, 0.25f };
            return densities[sectionIndex % densities.Length];
        }

        private Beatmap CreateComplexPatternBeatmap()
        {
            var beatmap = new Beatmap
            {
                Name = "Pattern Master",
                Artist = "Rhythm Expert",
                Difficulty = Difficulty.Expert,
                BPM = 180f,
                Duration = 45f,
                Offset = 0f,
                Level = Difficulty.Expert
            };

            float beatDuration = 60f / beatmap.BPM;
            int beatsPerMeasure = 8;
            int measureCount = (int)(beatmap.Duration / (beatDuration * beatsPerMeasure));

            for (int measure = 0; measure < measureCount; measure++)
            {
                float measureStart = measure * beatsPerMeasure * beatDuration + 2f;

                for (int beat = 0; beat < beatsPerMeasure; beat++)
                {
                    float time = measureStart + beat * beatDuration;

                    int patternType = (measure + beat) % 6;

                    switch (patternType)
                    {
                        case 0:
                            beatmap.Notes.Add(CreateTapNote(time, beat % _laneCount));
                            break;
                        case 1:
                            beatmap.Notes.Add(CreateTapNote(time, 0));
                            beatmap.Notes.Add(CreateTapNote(time + beatDuration * 0.5f, _laneCount - 1));
                            break;
                        case 2:
                            beatmap.Notes.Add(CreateTapNote(time, 0));
                            beatmap.Notes.Add(CreateTapNote(time, 1));
                            beatmap.Notes.Add(CreateTapNote(time, 2));
                            beatmap.Notes.Add(CreateTapNote(time, 3));
                            break;
                        case 3:
                            for (int i = 0; i < 4; i++)
                            {
                                beatmap.Notes.Add(CreateTapNote(time + i * beatDuration * 0.25f, i % _laneCount));
                            }
                            break;
                        case 4:
                            beatmap.Notes.Add(CreateHoldNote(time, 0, 0.2f));
                            beatmap.Notes.Add(CreateHoldNote(time + beatDuration * 0.25f, 3, 0.2f));
                            break;
                        case 5:
                            for (int i = 0; i < 8; i++)
                            {
                                beatmap.Notes.Add(CreateTapNote(time + i * beatDuration * 0.125f, (i * 2) % _laneCount));
                            }
                            break;
                    }
                }
            }

            return beatmap;
        }

        private NoteData CreateTapNote(float time, int lane)
        {
            return new NoteData
            {
                Timing = time,
                Lane = lane,
                Type = NoteType.Tap,
                Duration = 0,
                Spawned = false
            };
        }

        private NoteData CreateHoldNote(float time, int lane, float duration)
        {
            return new NoteData
            {
                Timing = time,
                Lane = lane,
                Type = NoteType.Hold,
                Duration = duration,
                Spawned = false
            };
        }

        private int GetNoteLane(Difficulty difficulty, int index, int maxCombo)
        {
            if (difficulty == Difficulty.Easy)
            {
                return index % 2;
            }
            else if (difficulty == Difficulty.Normal)
            {
                int pattern = index % 8;
                return pattern switch
                {
                    0 => 0,
                    1 => 1,
                    2 => 2,
                    3 => 3,
                    4 => 2,
                    5 => 1,
                    6 => 0,
                    7 => 3,
                    _ => index % _laneCount
                };
            }
            else if (difficulty == Difficulty.Hard)
            {
                return index % _laneCount;
            }
            else
            {
                int[] patterns = { 0, 1, 2, 3, 2, 1, 3, 0, 1, 3, 2, 0 };
                return patterns[index % patterns.Length];
            }
        }

        private NoteType DetermineNoteType(Difficulty difficulty, int index, int maxCombo)
        {
            if (difficulty == Difficulty.Easy)
            {
                return NoteType.Tap;
            }

            if (difficulty >= Difficulty.Normal && index % 12 == 0)
            {
                return NoteType.Hold;
            }

            if (difficulty >= Difficulty.Hard && index % 20 == 0)
            {
                return NoteType.Slide;
            }

            if (difficulty >= Difficulty.Expert && index % 28 == 0)
            {
                return NoteType.Spin;
            }

            return NoteType.Tap;
        }

        private void AddRhythmPatterns(Beatmap beatmap, Difficulty difficulty, float noteInterval)
        {
            int patternStart = (int)(10 / noteInterval);
            int patternInterval = difficulty >= Difficulty.Expert ? 8 : 12;

            for (int i = patternStart; i < beatmap.Notes.Count; i += patternInterval)
            {
                float baseTime = beatmap.Notes[i].Timing;
                float halfBeat = noteInterval / 2;
                float quarterBeat = noteInterval / 4;

                beatmap.Notes[i].Timing = baseTime;
                if (i + 1 < beatmap.Notes.Count)
                {
                    beatmap.Notes[i + 1].Timing = baseTime + halfBeat;
                }
                if (i + 2 < beatmap.Notes.Count)
                {
                    beatmap.Notes[i + 2].Timing = baseTime + halfBeat + quarterBeat;
                }
                if (i + 3 < beatmap.Notes.Count)
                {
                    beatmap.Notes[i + 3].Timing = baseTime + halfBeat * 2;
                }
            }
        }

        private void AddChordPatterns(Beatmap beatmap, Difficulty difficulty)
        {
            int chordInterval = difficulty >= Difficulty.Expert ? 16 : 24;
            int chordSize = difficulty >= Difficulty.Expert ? 4 : 2;

            for (int i = 0; i < beatmap.Notes.Count; i += chordInterval)
            {
                if (i + chordSize >= beatmap.Notes.Count)
                    break;

                var chordTime = beatmap.Notes[i].Timing;
                var usedLanes = new HashSet<int>();

                for (int j = 1; j < chordSize; j++)
                {
                    int newLane;
                    do
                    {
                        newLane = _random.Next(_laneCount);
                    } while (usedLanes.Contains(newLane));

                    usedLanes.Add(newLane);
                    beatmap.Notes[i + j].Timing = chordTime + 0.02f;
                    beatmap.Notes[i + j].Lane = newLane;
                }
            }
        }

        private void AddStreamPatterns(Beatmap beatmap, Difficulty difficulty)
        {
            int streamStart = (int)(beatmap.Notes.Count * 0.6);
            int streamLength = difficulty >= Difficulty.Expert ? 16 : 8;

            for (int i = streamStart; i < beatmap.Notes.Count && i < streamStart + streamLength; i++)
            {
                int lane = (i - streamStart) % _laneCount;
                beatmap.Notes[i].Lane = lane;
                beatmap.Notes[i].Type = NoteType.Tap;
                beatmap.Notes[i].Duration = 0;
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            UpdateInput();
            UpdateGameState(deltaTime);
            UpdateBeatSystem(deltaTime);
            UpdateVisualEffects(deltaTime);
            UpdateNoteManager(deltaTime);
            UpdateParticles(deltaTime);
            UpdateTrackers(deltaTime);

            if (_gameState == RhythmGameState.Playing)
            {
                UpdatePlaying(deltaTime);
            }

            _ui.Update(deltaTime);
        }

        private void UpdateInput()
        {
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Escape))
            {
                HandleEscapeKey();
            }

            if (_gameState == RhythmGameState.Menu)
            {
                HandleMenuInput();
            }

            if (_gameState == RhythmGameState.SongSelect)
            {
                HandleSongSelectInput();
            }

            if (_gameState == RhythmGameState.Results)
            {
                HandleResultsInput();
            }

            if (_gameState == RhythmGameState.Settings)
            {
                HandleSettingsInput();
            }

            if (_gameState == RhythmGameState.Playing || _gameState == RhythmGameState.Paused)
            {
                HandleGameplayInput();
            }
        }

        private void HandleEscapeKey()
        {
            switch (_gameState)
            {
                case RhythmGameState.Playing:
                    SetGameState(RhythmGameState.Paused);
                    break;
                case RhythmGameState.Paused:
                    SetGameState(RhythmGameState.Playing);
                    break;
                case RhythmGameState.SongSelect:
                    SetGameState(RhythmGameState.Menu);
                    break;
                case RhythmGameState.Results:
                    SetGameState(RhythmGameState.Menu);
                    break;
                case RhythmGameState.Settings:
                    SetGameState(RhythmGameState.Menu);
                    break;
            }
        }

        private void HandleMenuInput()
        {
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.UpArrow))
            {
                _selectedDifficulty = Math.Max(0, _selectedDifficulty - 1);
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.DownArrow))
            {
                _selectedDifficulty = Math.Min(3, _selectedDifficulty + 1);
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Enter))
            {
                SetGameState(RhythmGameState.SongSelect);
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.S))
            {
                SetGameState(RhythmGameState.Settings);
            }
        }

        private void HandleSongSelectInput()
        {
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Enter))
            {
                StartGame();
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.LeftArrow))
            {
                _selectedSongIndex = Math.Max(0, _selectedSongIndex - 1);
                UpdateSelectedBeatmap();
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.RightArrow))
            {
                _selectedSongIndex = Math.Min(_beatmaps.Count - 1, _selectedSongIndex + 1);
                UpdateSelectedBeatmap();
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.UpArrow))
            {
                _selectedDifficulty = Math.Max(0, _selectedDifficulty - 1);
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.DownArrow))
            {
                _selectedDifficulty = Math.Min(3, _selectedDifficulty + 1);
            }
        }

        private void UpdateSelectedBeatmap()
        {
            if (_selectedSongIndex >= 0 && _selectedSongIndex < _beatmaps.Count)
            {
                _currentBeatmap = _beatmaps[_selectedSongIndex];
            }
        }

        private void HandleResultsInput()
        {
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Enter))
            {
                SetGameState(RhythmGameState.SongSelect);
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.R))
            {
                StartGame();
            }
        }

        private void HandleSettingsInput()
        {
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.UpArrow))
            {
                _settings.SelectedSettingIndex = Math.Max(0, _settings.SelectedSettingIndex - 1);
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.DownArrow))
            {
                _settings.SelectedSettingIndex = Math.Min(_settings.SettingCount - 1, _settings.SelectedSettingIndex + 1);
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.LeftArrow))
            {
                AdjustSetting(-1);
            }
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.RightArrow))
            {
                AdjustSetting(1);
            }
        }

        private void AdjustSetting(int direction)
        {
            switch (_settings.SelectedSettingIndex)
            {
                case 0:
                    _noteSpeed = Math.Clamp(_noteSpeed + direction * 50, 200, 800);
                    break;
                case 1:
                    _audioOffset = Math.Clamp(_audioOffset + direction * 10, -200, 200);
                    break;
                case 2:
                    _settings.MasterVolume = Math.Clamp(_settings.MasterVolume + direction * 0.1f, 0, 1);
                    AudioManager.Instance.MasterVolume = _settings.MasterVolume;
                    break;
                case 3:
                    _settings.MusicVolume = Math.Clamp(_settings.MusicVolume + direction * 0.1f, 0, 1);
                    AudioManager.Instance.MusicVolume = _settings.MusicVolume;
                    break;
                case 4:
                    _settings.SfxVolume = Math.Clamp(_settings.SfxVolume + direction * 0.1f, 0, 1);
                    AudioManager.Instance.SfxVolume = _settings.SfxVolume;
                    break;
                case 5:
                    _isMetronomeEnabled = !_isMetronomeEnabled;
                    break;
                case 6:
                    _settings.AutoNoteSpeed = !_settings.AutoNoteSpeed;
                    break;
            }
        }

        private void HandleGameplayInput()
        {
            for (int lane = 0; lane < _laneCount; lane++)
            {
                bool isPressed = InputManager.Instance.GetKey(_keyLaneMapping[lane]);
                bool wasPressed = _keyPressedStates[lane];

                if (isPressed && !wasPressed)
                {
                    OnLanePressed(lane);
                }
                else if (!isPressed && wasPressed)
                {
                    OnLaneReleased(lane);
                }

                _keyPressedStates[lane] = isPressed;
            }

            if (_practiceMode != PracticeMode.Off)
            {
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.M))
                {
                    _isMetronomeEnabled = !_isMetronomeEnabled;
                }
                if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.P))
                {
                    ToggleSlowMotion();
                }
            }
        }

        private void ToggleSlowMotion()
        {
            _isSlowMotionActive = !_isSlowMotionActive;
        }

        private void OnLanePressed(int lane)
        {
            if (_gameState != RhythmGameState.Playing)
                return;

            _lanes[lane].IsActive = true;
            _lanes[lane].FlashIntensity = 1.0f;
            _lanes[lane].ApproachIntensity = 1.0f;

            float hitWindow = GetTimingWindow(lane);
            Note closestNote = null;
            float closestDistance = float.MaxValue;

            foreach (var note in _noteManager.ActiveNotes)
            {
                if (note.Lane != lane || note.IsHit || note.Missed)
                    continue;

                if (note.Type == NoteType.Spin)
                {
                    HandleSpinInput(lane);
                    continue;
                }

                if (note.Type == NoteType.Slide)
                {
                    HandleSlideInput(lane);
                    continue;
                }

                float distance = Math.Abs(note.Timing - _songTime);
                if (distance < closestDistance && distance < hitWindow)
                {
                    closestDistance = distance;
                    closestNote = note;
                }
            }

            if (closestNote != null)
            {
                ProcessHit(closestNote, lane);
            }
        }

        private void OnLaneReleased(int lane)
        {
            if (_gameState != RhythmGameState.Playing)
                return;

            _lanes[lane].IsActive = false;

            foreach (var note in _noteManager.ActiveNotes)
            {
                if (note.Lane != lane || note.Type != NoteType.Hold || note.IsHit || note.IsHolding)
                    continue;

                float distance = Math.Abs(note.Timing + note.Duration - _songTime);
                if (distance < 0.15f)
                {
                    ProcessHoldRelease(note, lane);
                }
            }

            _slideTrackers[lane].Reset();
        }

        private void HandleSpinInput(int lane)
        {
            _spinTrackers[lane].IsTracking = true;
            _spinTrackers[lane].StartTime = _songTime;
            _spinTrackers[lane].RequiredRotations = 1;
            _spinTrackers[lane].CurrentProgress = 0;
        }

        private void HandleSlideInput(int lane)
        {
            _slideTrackers[lane].StartLane = lane;
            _slideTrackers[lane].StartTime = _songTime;
            _slideTrackers[lane].Direction = SlideDirection.None;
        }

        private void ProcessHit(Note note, int lane)
        {
            float timingDiff = Math.Abs(note.Timing - _songTime);
            HitRating rating = GetHitRating(timingDiff, lane);

            note.IsHit = true;
            _hitResults.Add(new HitResult { Rating = rating, Timing = timingDiff, Lane = lane });

            _lanes[lane].HitCount++;

            switch (rating)
            {
                case HitRating.Perfect:
                    _perfectCount++;
                    _score += CalculateScore(1000, _combo);
                    _combo++;
                    SpawnHitEffect(lane, "PERFECT", Color.FromRgb(255, 255, 100), true);
                    SpawnParticles(lane, 20, Color.FromRgb(255, 255, 200));
                    break;
                case HitRating.Great:
                    _greatCount++;
                    _score += CalculateScore(500, _combo);
                    _combo++;
                    SpawnHitEffect(lane, "GREAT", Color.FromRgb(100, 255, 100), true);
                    SpawnParticles(lane, 10, Color.FromRgb(150, 255, 150));
                    break;
                case HitRating.Good:
                    _goodCount++;
                    _score += CalculateScore(200, _combo);
                    _combo++;
                    SpawnHitEffect(lane, "GOOD", Color.FromRgb(100, 200, 255), true);
                    break;
                case HitRating.Bad:
                    _badCount++;
                    _combo = 0;
                    SpawnHitEffect(lane, "BAD", Color.FromRgb(255, 150, 100), false);
                    break;
                case HitRating.Miss:
                    _missCount++;
                    _combo = 0;
                    _lanes[lane].MissCount++;
                    SpawnHitEffect(lane, "MISS", Color.FromRgb(255, 100, 100), false);
                    break;
            }

            _maxCombo = Math.Max(_maxCombo, _combo);
            UpdateAccuracy();
            PlayHitSound(rating);
        }

        private void ProcessHoldRelease(Note note, int lane)
        {
            float startDiff = Math.Abs(note.Timing - _songTime);
            float endDiff = Math.Abs((note.Timing + note.Duration) - _songTime);

            float avgDiff = (startDiff + endDiff) / 2;
            HitRating rating = avgDiff < 0.1f ? HitRating.Great : HitRating.Good;

            note.IsHit = true;
            _hitResults.Add(new HitResult { Rating = rating, Timing = avgDiff, Lane = lane });

            if (rating == HitRating.Great)
            {
                _greatCount++;
                _score += CalculateScore(500, _combo);
                _combo++;
                SpawnHitEffect(lane, "GREAT", Color.FromRgb(100, 255, 100), true);
                SpawnParticles(lane, 10, Color.FromRgb(150, 255, 150));
            }
            else
            {
                _goodCount++;
                _score += CalculateScore(200, _combo);
                _combo++;
                SpawnHitEffect(lane, "GOOD", Color.FromRgb(100, 200, 255), true);
            }

            _maxCombo = Math.Max(_maxCombo, _combo);
            UpdateAccuracy();
        }

        private int CalculateScore(int baseScore, int combo)
        {
            float multiplier = 1.0f + (combo / 100.0f);
            multiplier = Math.Min(multiplier, 3.0f);
            return (int)(baseScore * multiplier);
        }

        private HitRating GetHitRating(float timingDiff, int lane)
        {
            float windowMultiplier = _settings.TimingDifficulty == TimingDifficulty.Hard ? 0.7f :
                                     _settings.TimingDifficulty == TimingDifficulty.Easy ? 1.3f : 1.0f;

            if (timingDiff <= 0.025f * windowMultiplier) return HitRating.Perfect;
            if (timingDiff <= 0.050f * windowMultiplier) return HitRating.Great;
            if (timingDiff <= 0.085f * windowMultiplier) return HitRating.Good;
            if (timingDiff <= 0.120f * windowMultiplier) return HitRating.Bad;
            return HitRating.Miss;
        }

        private float GetTimingWindow(int lane)
        {
            return 0.15f;
        }

        private void UpdateAccuracy()
        {
            int totalHit = _perfectCount + _greatCount + _goodCount + _badCount + _missCount;
            if (totalHit == 0)
            {
                _accuracy = 100f;
                return;
            }

            float accuracy = 0f;
            accuracy += _perfectCount * 100f;
            accuracy += _greatCount * 80f;
            accuracy += _goodCount * 50f;
            accuracy += _badCount * 20f;
            accuracy /= totalHit * 100f;
            accuracy *= 100f;

            _accuracy = accuracy;
        }

        private void SpawnHitEffect(int lane, string text, Color color, bool positive)
        {
            float xPosition = GetLaneScreenPosition(lane);

            _visualEffects.Add(new VisualEffect
            {
                Type = EffectType.HitText,
                Text = text,
                Color = color,
                Position = new Vector2(xPosition, 450),
                LifeTime = positive ? 1.0f : 0.7f,
                Age = 0f,
                Scale = positive ? 1.5f : 1.2f,
                VelocityY = -100f,
                Alpha = 1.0f
            });

            if (_combo > 0 && _combo % 10 == 0)
            {
                _visualEffects.Add(new VisualEffect
                {
                    Type = EffectType.Combo,
                    Text = $"{_combo} COMBO!",
                    Color = Color.FromRgb(255, 255, 100),
                    Position = new Vector2(960, 300),
                    LifeTime = 1.5f,
                    Age = 0f,
                    Scale = 2.0f,
                    VelocityY = -30f,
                    Alpha = 1.0f
                });
            }
        }

        private float GetLaneScreenPosition(int lane)
        {
            float laneWidth = 100f;
            float totalWidth = _laneCount * laneWidth;
            float startX = 960 - totalWidth / 2;
            return startX + lane * laneWidth + laneWidth / 2;
        }

        private void SpawnParticles(int lane, int count, Color color)
        {
            float xPosition = GetLaneScreenPosition(lane);
            float yPosition = 650f;

            for (int i = 0; i < count; i++)
            {
                _particles.Add(new ParticleEffect
                {
                    Position = new Vector2(xPosition, yPosition),
                    Velocity = new Vector2((float)(_random.NextDouble() * 200 - 100), (float)(_random.NextDouble() * -200 - 50)),
                    Color = color,
                    Size = 3 + (float)_random.NextDouble() * 5,
                    LifeTime = 0.5f + (float)_random.NextDouble() * 0.5f,
                    Age = 0f
                });
            }
        }

        private void PlayHitSound(HitRating rating)
        {
            string soundName = rating switch
            {
                HitRating.Perfect => "hit_perfect",
                HitRating.Great => "hit_great",
                HitRating.Good => "hit_good",
                HitRating.Bad => "hit_bad",
                HitRating.Miss => "hit_miss",
                _ => "hit_good"
            };

            float volume = rating >= HitRating.Good ? _settings.SfxVolume * 0.8f : _settings.SfxVolume * 0.5f;
            AudioManager.Instance.PlayUI(soundName, volume);
        }

        private void UpdateGameState(float deltaTime)
        {
            for (int i = 0; i < _laneCount; i++)
            {
                _lanes[i].FlashIntensity = Math.Max(0, _lanes[i].FlashIntensity - deltaTime * 5f);
                _lanes[i].ApproachIntensity = Math.Max(0, _lanes[i].ApproachIntensity - deltaTime * 3f);
            }
        }

        private void UpdateBeatSystem(float deltaTime)
        {
            if (_gameState != RhythmGameState.Playing)
                return;

            if (_practiceMode == PracticeMode.SlowMusic && _isSlowMotionActive)
            {
                deltaTime *= _slowMotionFactor;
            }

            ProcessBPMChanges();

            _beatInterval = 60f / _currentBPM;
            _beatTimer += deltaTime;

            int beatsPerMeasure = _timeSignatureNumerator;
            while (_beatTimer >= _beatInterval)
            {
                _beatTimer -= _beatInterval;
                _currentBeat++;

                if (_currentBeat % beatsPerMeasure == 0)
                {
                    OnMeasureComplete();
                }

                OnBeat();
            }

            if (_isMetronomeEnabled)
            {
                if (_beatTimer < deltaTime * 3)
                {
                    PlayMetronomeClick(_currentBeat % beatsPerMeasure == 0);
                }
            }
        }

        private void ProcessBPMChanges()
        {
            if (_currentBeatmap == null || _currentBeatmap.BPMChanges.Count == 0)
                return;

            while (_currentBPMChangeIndex < _currentBeatmap.BPMChanges.Count &&
                   _songTime >= _currentBeatmap.BPMChanges[_currentBPMChangeIndex].Time)
            {
                _currentBPM = _currentBeatmap.BPMChanges[_currentBPMChangeIndex].BPM;
                _currentBPMChangeIndex++;
            }
        }

        private void OnBeat()
        {
            _backgroundPulse = 1.0f;
            _pulseTimer = 0.1f;
        }

        private void OnMeasureComplete()
        {
            if (_practiceMode == PracticeMode.SectionRepeat && _sectionEndMarker > 0)
            {
                if (_songTime >= _sectionEndMarker)
                {
                    _songTime = _sectionStartMarker;
                    _sectionRepeatCount++;
                    SpawnHitEffect(0, $"Loop #{_sectionRepeatCount}", Color.FromRgb(200, 200, 255), true);
                }
            }
        }

        private void PlayMetronomeClick(bool isDownbeat)
        {
            string sound = isDownbeat ? "metronome_down" : "metronome_up";
            AudioManager.Instance.PlayUI(sound, _metronomeVolume * 0.5f);
        }

        private void UpdateVisualEffects(float deltaTime)
        {
            for (int i = _visualEffects.Count - 1; i >= 0; i--)
            {
                var effect = _visualEffects[i];
                effect.Age += deltaTime;

                effect.Position.Y += effect.VelocityY * deltaTime;

                if (effect.Type == EffectType.HitText)
                {
                    effect.Scale = effect.LifeTime > 0 ? 1.5f - (effect.Age / effect.LifeTime) * 0.5f : 1.0f;
                    effect.Alpha = 1.0f - (effect.Age / effect.LifeTime);
                }
                else if (effect.Type == EffectType.Combo)
                {
                    effect.Scale = 2.0f + effect.Age * 0.2f;
                    effect.Alpha = 1.0f - (effect.Age / effect.LifeTime) * 0.5f;
                }

                if (effect.Age >= effect.LifeTime)
                {
                    _visualEffects.RemoveAt(i);
                }
            }

            _backgroundPulse = Math.Max(0, _backgroundPulse - deltaTime * 3f);
            _pulseTimer = Math.Max(0, _pulseTimer - deltaTime);
        }

        private void UpdateParticles(float deltaTime)
        {
            for (int i = _particles.Count - 1; i >= 0; i--)
            {
                var particle = _particles[i];
                particle.Age += deltaTime;

                particle.Position += particle.Velocity * deltaTime;
                particle.Velocity.Y += 500f * deltaTime;

                particle.Alpha = 1.0f - (particle.Age / particle.LifeTime);

                if (particle.Age >= particle.LifeTime)
                {
                    _particles.RemoveAt(i);
                }
            }
        }

        private void UpdateTrackers(float deltaTime)
        {
            for (int i = 0; i < _laneCount; i++)
            {
                if (_spinTrackers[i].IsTracking)
                {
                    float spinDuration = _songTime - _spinTrackers[i].StartTime;
                    _spinTrackers[i].CurrentProgress = Math.Min(1.0f, spinDuration / 2.0f);

                    if (_spinTrackers[i].CurrentProgress >= 1.0f)
                    {
                        _spinTrackers[i].IsTracking = false;
                        ProcessHit(new Note { Type = NoteType.Spin, Lane = i }, i);
                    }
                }
            }
        }

        private void UpdateNoteManager(float deltaTime)
        {
            _noteManager.Update(deltaTime, _songTime);
        }

        private void UpdatePlaying(float deltaTime)
        {
            float effectiveDelta = deltaTime;
            if (_practiceMode == PracticeMode.SlowMusic && _isSlowMotionActive)
            {
                effectiveDelta *= _slowMotionFactor;
            }

            _gameTime += deltaTime;
            _songTime = _gameTime - 2f + (_audioOffset / 1000f);

            if (_currentBeatmap != null)
            {
                SpawnUpcomingNotes();
                CheckMissedNotes();
                CheckSongEnd();
            }
        }

        private void SpawnUpcomingNotes()
        {
            float spawnTime = _noteSpeed / 200f;

            foreach (var noteData in _currentBeatmap.Notes)
            {
                if (noteData.Spawned)
                    continue;

                if (noteData.Timing - _songTime <= spawnTime && noteData.Timing - _songTime > -0.5f)
                {
                    var note = new Note
                    {
                        Timing = noteData.Timing,
                        Lane = noteData.Lane,
                        Type = noteData.Type,
                        Duration = noteData.Duration,
                        Spawned = true
                    };

                    _noteManager.SpawnNote(note);
                    noteData.Spawned = true;
                }
            }
        }

        private void CheckMissedNotes()
        {
            foreach (var note in _noteManager.ActiveNotes)
            {
                if (note.IsHit || note.Missed)
                    continue;

                if (_songTime - note.Timing > GetTimingWindow(note.Lane))
                {
                    note.Missed = true;
                    _missCount++;
                    _combo = 0;
                    _lanes[note.Lane].MissCount++;
                    _hitResults.Add(new HitResult { Rating = HitRating.Miss, Timing = _songTime - note.Timing, Lane = note.Lane });
                    SpawnHitEffect(note.Lane, "MISS", Color.FromRgb(255, 100, 100), false);
                    UpdateAccuracy();
                }
            }

            _noteManager.CleanupNotes(_songTime);
        }

        private void CheckSongEnd()
        {
            if (_currentBeatmap != null)
            {
                float endTime = _currentBeatmap.Duration + 2f;
                if (_songTime >= endTime)
                {
                    EndGame();
                }
            }
        }

        public void StartGame()
        {
            ResetGameState();

            if (_currentBeatmap != null)
            {
                foreach (var note in _currentBeatmap.Notes)
                {
                    note.Spawned = false;
                }
                _currentBPM = _currentBeatmap.BPM;
                _bpmChanges = new List<BPMChange>(_currentBeatmap.BPMChanges);
                _currentBPMChangeIndex = 0;
                _totalNotes = _currentBeatmap.Notes.Count;
            }

            SetGameState(RhythmGameState.Playing);
            _musicStarted = true;
        }

        private void ResetGameState()
        {
            _score = 0;
            _combo = 0;
            _maxCombo = 0;
            _accuracy = 100f;
            _perfectCount = 0;
            _greatCount = 0;
            _goodCount = 0;
            _badCount = 0;
            _missCount = 0;
            _hitResults.Clear();
            _visualEffects.Clear();
            _particles.Clear();
            _gameTime = 0;
            _songTime = 0;
            _currentBeat = 0;
            _beatTimer = 0;
            _sectionRepeatCount = 0;

            _noteManager.Clear();

            for (int i = 0; i < _laneCount; i++)
            {
                _lanes[i].HitCount = 0;
                _lanes[i].MissCount = 0;
                _lanes[i].FlashIntensity = 0;
                _lanes[i].ApproachIntensity = 0;
            }
        }

        private void EndGame()
        {
            SetGameState(RhythmGameState.Results);
        }

        private void SetGameState(RhythmGameState newState)
        {
            _previousState = _gameState;
            _gameState = newState;

            switch (_gameState)
            {
                case RhythmGameState.Playing:
                    _ui.HideAllOverlays();
                    break;
                case RhythmGameState.Paused:
                    _ui.ShowPauseOverlay();
                    break;
                case RhythmGameState.Results:
                    ShowResults();
                    break;
            }
        }

        private void ShowResults()
        {
            int totalHits = _perfectCount + _greatCount + _goodCount + _badCount + _missCount;
            if (totalHits > 0)
            {
                UpdateAccuracy();
            }
        }

        public string GetGrade()
        {
            if (_accuracy >= 95f && _missCount == 0) return "S";
            if (_accuracy >= 90f) return "A";
            if (_accuracy >= 80f) return "B";
            if (_accuracy >= 70f) return "C";
            if (_accuracy >= 60f) return "D";
            return "F";
        }

        public string GetDetailedGrade()
        {
            if (_accuracy >= 98f && _missCount == 0 && _perfectCount == _totalNotes) return "SS";
            if (_accuracy >= 95f && _missCount == 0) return "S";
            if (_accuracy >= 90f) return "A";
            if (_accuracy >= 80f) return "B";
            if (_accuracy >= 70f) return "C";
            if (_accuracy >= 60f) return "D";
            return "F";
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            DrawBackground(context);

            switch (_gameState)
            {
                case RhythmGameState.Menu:
                    DrawMenu(context);
                    break;
                case RhythmGameState.SongSelect:
                    DrawSongSelect(context);
                    break;
                case RhythmGameState.Playing:
                case RhythmGameState.Paused:
                    DrawGameplay(context);
                    if (_gameState == RhythmGameState.Paused)
                    {
                        DrawPauseOverlay(context);
                    }
                    break;
                case RhythmGameState.Results:
                    DrawResults(context);
                    break;
                case RhythmGameState.Settings:
                    DrawSettings(context);
                    break;
            }
        }

        private void DrawBackground(DrawingContext context)
        {
            float pulseAmount = _backgroundPulse * 0.15f;
            byte baseR = 25;
            byte baseG = 15;
            byte baseB = 45;

            byte r = (byte)(baseR + pulseAmount * 60);
            byte g = (byte)(baseG + pulseAmount * 40);
            byte b = (byte)(baseB + pulseAmount * 100);

            var backgroundBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(r, g, b));
            context.DrawRectangle(backgroundBrush, null, new Rect(0, 0, 1920, 1080));

            for (int i = 0; i < 30; i++)
            {
                float x = (i * 70 + _gameTime * 30) % 1920;
                float y = 50 + (float)Math.Sin(i * 0.4 + _gameTime * 2) * 40;

                byte lineAlpha = (byte)(20 + _backgroundPulse * 30);
                var lineBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(lineAlpha, 80, 120, 180));
                var linePen = new Pen(lineBrush, 1 + _backgroundPulse);

                context.DrawLine(linePen, new Point(x, 0), new Point(x, 1080));
                context.DrawLine(linePen, new Point(0, y), new Point(1920, y));
            }

            for (int i = 0; i < 10; i++)
            {
                float x = 100 + i * 200;
                float y = 200 + (float)Math.Sin(i * 0.3 + _gameTime) * 100;

                byte circleAlpha = (byte)(15 + _backgroundPulse * 20);
                var circleBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(circleAlpha, 100, 150, 200));
                context.DrawEllipse(null, new Pen(circleBrush, 1), new Point(x, y), 50, 50);
            }
        }

        private void DrawMenu(DrawingContext context)
        {
            var titleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 80));
            var textBrush = new SolidColorBrush(Colors.White);
            var selectedBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 255, 100));
            var subtitleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 200));

            var titleFormatted = new FormattedText(
                "RHYTHM GAME",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                80, titleBrush, 1.0);
            context.DrawText(titleFormatted, new Point(960 - titleFormatted.Width / 2, 120));

            var subtitleFormatted = new FormattedText(
                "Demo Edition",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                24, subtitleBrush, 1.0);
            context.DrawText(subtitleFormatted, new Point(960 - subtitleFormatted.Width / 2, 200));

            string[] difficulties = { "EASY", "NORMAL", "HARD", "EXPERT" };
            Color[] colors = {
                Color.FromRgb(100, 255, 100),
                Color.FromRgb(100, 200, 255),
                Color.FromRgb(255, 150, 100),
                Color.FromRgb(255, 100, 255)
            };

            for (int i = 0; i < difficulties.Length; i++)
            {
                var brush = i == _selectedDifficulty ? selectedBrush : textBrush;
                var diffBrush = new SolidColorBrush(i == _selectedDifficulty ? colors[i] : Color.FromRgb(100, 100, 100));

                var bgRect = new Rect(760, 300 + i * 60, 400, 50);
                if (i == _selectedDifficulty)
                {
                    var bgBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, colors[i].R, colors[i].G, colors[i].B));
                    context.DrawRectangle(bgBrush, null, bgRect);
                }

                var diffFormatted = new FormattedText(
                    $">{difficulties[i]}<",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    32, brush, 1.0);
                context.DrawText(diffFormatted, new Point(960 - diffFormatted.Width / 2, 305 + i * 60));
            }

            var startFormatted = new FormattedText(
                "Press ENTER to Select Song",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                28, textBrush, 1.0);
            context.DrawText(startFormatted, new Point(960 - startFormatted.Width / 2, 600));

            var settingsFormatted = new FormattedText(
                "Press S for Settings",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                20, new SolidColorBrush(Color.FromRgb(150, 150, 150)), 1.0);
            context.DrawText(settingsFormatted, new Point(960 - settingsFormatted.Width / 2, 650));

            DrawMenuControls(context);
        }

        private void DrawMenuControls(DrawingContext context)
        {
            var controlBrush = new SolidColorBrush(Color.FromRgb(80, 80, 100));
            context.DrawRectangle(controlBrush, null, new Rect(760, 750, 400, 200));

            var textBrush = new SolidColorBrush(Color.FromRgb(180, 180, 200));

            var controlsTitle = new FormattedText(
                "CONTROLS",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                18, textBrush, 1.0);
            context.DrawText(controlsTitle, new Point(960 - controlsTitle.Width / 2, 760));

            string[] controls = {
                "1-8 Keys: Hit notes in lanes",
                "ESC: Pause / Resume / Back",
                "Arrow Keys: Navigate menus",
                "Enter: Confirm selection"
            };

            for (int i = 0; i < controls.Length; i++)
            {
                var controlFormatted = new FormattedText(
                    controls[i],
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    16, textBrush, 1.0);
                context.DrawText(controlFormatted, new Point(960 - 150, 790 + i * 35));
            }
        }

        private void DrawSongSelect(DrawingContext context)
        {
            var titleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 200, 80));
            var textBrush = new SolidColorBrush(Colors.White);

            var titleFormatted = new FormattedText(
                "SONG SELECT",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                48, titleBrush, 1.0);
            context.DrawText(titleFormatted, new Point(960 - titleFormatted.Width / 2, 50));

            DrawSongList(context);

            if (_currentBeatmap != null)
            {
                DrawSongInfo(context, _currentBeatmap);
            }

            var startFormatted = new FormattedText(
                "Press ENTER to Start | ESC to Go Back",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                20, textBrush, 1.0);
            context.DrawText(startFormatted, new Point(960 - startFormatted.Width / 2, 950));
        }

        private void DrawSongList(DrawingContext context)
        {
            int visibleSongs = 5;
            int startIndex = Math.Max(0, Math.Min(_selectedSongIndex - 2, _beatmaps.Count - visibleSongs));
            int endIndex = Math.Min(_beatmaps.Count, startIndex + visibleSongs);

            for (int i = startIndex; i < endIndex; i++)
            {
                int displayIndex = i - startIndex;
                float y = 150 + displayIndex * 80;

                bool isSelected = i == _selectedSongIndex;
                var beatmap = _beatmaps[i];

                if (isSelected)
                {
                    var selectedBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 100, 150, 200));
                    context.DrawRectangle(selectedBrush, null, new Rect(260, y - 5, 700, 70));

                    var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(100, 200, 255)), 2);
                    context.DrawRectangle(null, borderPen, new Rect(260, y - 5, 700, 70));
                }

                var songBrush = new SolidColorBrush(isSelected ? Color.FromRgb(255, 255, 255) : Color.FromRgb(150, 150, 170));
                var songFormatted = new FormattedText(
                    beatmap.Name,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial Bold"),
                    22, songBrush, 1.0);
                context.DrawText(songFormatted, new Point(280, y));

                var artistFormatted = new FormattedText(
                    beatmap.Artist,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    16, new SolidColorBrush(Color.FromRgb(120, 120, 140)), 1.0);
                context.DrawText(artistFormatted, new Point(280, y + 28));

                var diffFormatted = new FormattedText(
                    $"{beatmap.Level}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    16, new SolidColorBrush(GetDifficultyColor(beatmap.Level)), 1.0);
                context.DrawText(diffFormatted, new Point(900, y + 10));

                var bpmFormatted = new FormattedText(
                    $"{beatmap.BPM:F0} BPM",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    14, new SolidColorBrush(Color.FromRgb(100, 100, 120)), 1.0);
                context.DrawText(bpmFormatted, new Point(900, y + 32));
            }
        }

        private Color GetDifficultyColor(Difficulty difficulty)
        {
            return difficulty switch
            {
                Difficulty.Easy => Color.FromRgb(100, 255, 100),
                Difficulty.Normal => Color.FromRgb(100, 200, 255),
                Difficulty.Hard => Color.FromRgb(255, 150, 100),
                Difficulty.Expert => Color.FromRgb(255, 100, 255),
                _ => Color.FromRgb(255, 255, 255)
            };
        }

        private void DrawSongInfo(DrawingContext context, Beatmap beatmap)
        {
            var infoBgBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 30, 50));
            context.DrawRectangle(infoBgBrush, null, new Rect(1000, 150, 400, 350));

            var textBrush = new SolidColorBrush(Colors.White);

            var infoTitle = new FormattedText(
                "SONG INFO",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                20, textBrush, 1.0);
            context.DrawText(infoTitle, new Point(1200 - infoTitle.Width / 2, 170));

            DrawInfoLine(context, "Title:", beatmap.Name, 210);
            DrawInfoLine(context, "Artist:", beatmap.Artist, 250);
            DrawInfoLine(context, "Difficulty:", beatmap.Level.ToString(), 290);
            DrawInfoLine(context, "BPM:", $"{beatmap.BPM:F0}", 330);
            DrawInfoLine(context, "Duration:", $"{beatmap.Duration:F0}s", 370);
            DrawInfoLine(context, "Notes:", $"{beatmap.Notes.Count}", 410);

            DrawDifficultySelector(context, beatmap);
        }

        private void DrawInfoLine(DrawingContext context, string label, string value, float y)
        {
            var labelBrush = new SolidColorBrush(Color.FromRgb(150, 150, 170));
            var valueBrush = new SolidColorBrush(Colors.White);

            var labelFormatted = new FormattedText(
                label,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                18, labelBrush, 1.0);
            context.DrawText(labelFormatted, new Point(1030, y));

            var valueFormatted = new FormattedText(
                value,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                18, valueBrush, 1.0);
            context.DrawText(valueFormatted, new Point(1150, y));
        }

        private void DrawDifficultySelector(DrawingContext context, Beatmap beatmap)
        {
            var selectorBgBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 40, 40, 60));
            context.DrawRectangle(selectorBgBrush, null, new Rect(1000, 450, 400, 50));

            string[] difficulties = { "EASY", "NORMAL", "HARD", "EXPERT" };

            for (int i = 0; i < difficulties.Length; i++)
            {
                bool isSelected = i == _selectedDifficulty;
                var color = GetDifficultyColor((Difficulty)i);
                var brush = new SolidColorBrush(isSelected ? color : Color.FromRgb(80, 80, 100));

                var diffFormatted = new FormattedText(
                    difficulties[i],
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    16, brush, 1.0);
                context.DrawText(diffFormatted, new Point(1050 + i * 90, 460));

                if (isSelected)
                {
                    context.DrawLine(new Pen(brush, 2), new Point(1050 + i * 90, 490), new Point(1130 + i * 90, 490));
                }
            }
        }

        private void DrawGameplay(DrawingContext context)
        {
            float laneWidth = 100f;
            float totalWidth = _laneCount * laneWidth;
            float startX = 960 - totalWidth / 2;
            float laneHeight = 600f;
            float targetY = 650f;
            float approachY = 100f;

            DrawLanes(context, startX, laneWidth, laneHeight, targetY, approachY);
            DrawNotes(context, startX, laneWidth, targetY, approachY);
            DrawEffects(context);
            DrawParticles(context);
            DrawHUD(context);
            DrawPracticeOverlay(context);
        }

        private void DrawLanes(DrawingContext context, float startX, float laneWidth, float laneHeight, float targetY, float approachY)
        {
            for (int i = 0; i < _laneCount; i++)
            {
                float laneX = startX + i * laneWidth;
                Lane lane = _lanes[i];
                float flashIntensity = lane.FlashIntensity;

                byte r = (byte)(lane.Color.R * 0.3 + flashIntensity * lane.Color.R);
                byte g = (byte)(lane.Color.G * 0.3 + flashIntensity * lane.Color.G);
                byte b = (byte)(lane.Color.B * 0.3 + flashIntensity * lane.Color.B);
                var laneBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(120, r, g, b));

                context.DrawRectangle(laneBrush, null, new Rect(laneX, targetY - laneHeight, laneWidth - 2, laneHeight));

                var separatorPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, 80, 80, 80)), 1);
                context.DrawLine(separatorPen, new Point(laneX, targetY - laneHeight), new Point(laneX, targetY));

                float approachScale = 1.0f + lane.ApproachIntensity * 0.3f;
                byte approachR = (byte)(lane.Color.R * approachScale);
                byte approachG = (byte)(lane.Color.G * approachScale);
                byte approachB = (byte)(lane.Color.B * approachScale);
                var approachBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(100 * lane.ApproachIntensity), approachR, approachG, approachB));
                context.DrawRectangle(approachBrush, null, new Rect(laneX + 2, targetY - 5, laneWidth - 4, 5));

                var targetBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, (byte)(lane.Color.R * 0.8), (byte)(lane.Color.G * 0.8), (byte)(lane.Color.B * 0.8)));
                context.DrawRectangle(targetBrush, null, new Rect(laneX + 10, targetY - 8, laneWidth - 20, 8));

                if (lane.IsActive)
                {
                    var activeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, lane.Color.R, lane.Color.G, lane.Color.B));
                    context.DrawRectangle(activeBrush, null, new Rect(laneX, targetY - laneHeight, laneWidth - 2, laneHeight));
                }

                DrawLaneKeyIndicator(context, laneX, targetY, laneWidth, i);
            }
        }

        private void DrawLaneKeyIndicator(DrawingContext context, float laneX, float targetY, float laneWidth, int lane)
        {
            if (!_keyLaneMapping.ContainsKey(lane))
                return;

            string keyName = ((int)_keyLaneMapping[lane] - 48).ToString();

            var keyBgBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 60, 60, 80));
            context.DrawRectangle(keyBgBrush, null, new Rect(laneX + 30, targetY + 15, 40, 35));

            var keyTextFormatted = new FormattedText(
                keyName,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                20, new SolidColorBrush(Colors.White), 1.0);
            context.DrawText(keyTextFormatted, new Point(laneX + 50 - keyTextFormatted.Width / 2, targetY + 22));
        }

        private void DrawNotes(DrawingContext context, float startX, float laneWidth, float targetY, float approachY)
        {
            foreach (var note in _noteManager.ActiveNotes)
            {
                float noteX = startX + note.Lane * laneWidth + laneWidth / 2;
                float timeUntilHit = note.Timing - _songTime;
                float approachRatio = timeUntilHit / (_noteSpeed / 200f);
                float noteY = targetY - (1 - approachRatio) * approachY;

                if (noteY < targetY - 650 || noteY > targetY + 50)
                    continue;

                Color noteColor = GetNoteColor(note.Type);
                var noteBrush = new SolidColorBrush(noteColor);

                if (note.IsHit)
                    continue;

                switch (note.Type)
                {
                    case NoteType.Tap:
                        DrawTapNote(context, noteX, noteY, noteBrush, noteColor);
                        break;
                    case NoteType.Hold:
                        DrawHoldNote(context, noteX, noteY, noteBrush, noteColor, note.Duration);
                        break;
                    case NoteType.Slide:
                        DrawSlideNote(context, noteX, noteY, noteBrush, noteColor);
                        break;
                    case NoteType.Spin:
                        DrawSpinNote(context, noteX, noteY, noteBrush, noteColor);
                        break;
                }
            }
        }

        private void DrawTapNote(DrawingContext context, float x, float y, SolidColorBrush brush, Color color)
        {
            float glowIntensity = 0.5f + (float)Math.Sin(_gameTime * 10) * 0.2f;
            var glowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(glowIntensity * 100), color.R, color.G, color.B));
            context.DrawEllipse(glowBrush, null, new Point(x, y), 40, 40);

            context.DrawEllipse(brush, new Pen(new SolidColorBrush(Colors.White), 2), new Point(x, y), 28, 28);

            var innerBrush = new SolidColorBrush(Colors.White);
            context.DrawEllipse(innerBrush, null, new Point(x, y), 10, 10);
        }

        private void DrawHoldNote(DrawingContext context, float x, float y, SolidColorBrush brush, Color color, float duration)
        {
            float holdLength = duration * 200f;
            float holdY = y - holdLength;

            var holdGradient = new LinearGradientBrush(
                System.Windows.Media.Color.FromArgb(180, color.R, color.G, color.B),
                System.Windows.Media.Color.FromArgb(80, color.R, color.G, color.B),
                90, 0);
            context.DrawRectangle(holdGradient, null, new Rect(x - 20, holdY, 40, holdLength));

            var holdOutline = new Pen(brush, 2);
            context.DrawRectangle(null, holdOutline, new Rect(x - 20, holdY, 40, holdLength));

            var tailBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, color.R, color.G, color.B));
            context.DrawEllipse(tailBrush, new Pen(new SolidColorBrush(Colors.White), 2), new Point(x, y), 28, 28);
        }

        private void DrawSlideNote(DrawingContext context, float x, float y, SolidColorBrush brush, Color color)
        {
            var triangle = new StreamGeometry();
            using (var ctx = triangle.Open())
            {
                ctx.BeginFigure(new Point(x, y - 35), true, true);
                ctx.LineTo(new Point(x - 30, y + 25), true, false);
                ctx.LineTo(new Point(x + 30, y + 25), true, false);
            }

            var glowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, color.R, color.G, color.B));
            context.DrawGeometry(glowBrush, null, triangle);

            context.DrawGeometry(brush, new Pen(new SolidColorBrush(Colors.White), 2), triangle);

            var arrowBrush = new SolidColorBrush(Colors.White);
            context.DrawEllipse(arrowBrush, null, new Point(x, y), 8, 8);
        }

        private void DrawSpinNote(DrawingContext context, float x, float y, SolidColorBrush brush, Color color)
        {
            float rotation = _gameTime * 180;

            var transform = new System.Windows.Media.RotateTransform(rotation, x, y);

            var outerRing = new StreamGeometry();
            using (var ctx = outerRing.Open())
            {
                for (int i = 0; i < 6; i++)
                {
                    float angle1 = (i * 60) * Math.PI / 180;
                    float angle2 = ((i + 1) * 60) * Math.PI / 180;

                    if (i == 0)
                    {
                        ctx.BeginFigure(new Point(x + (float)Math.Cos(angle1) * 35, y + (float)Math.Sin(angle1) * 35), true, true);
                    }
                    else
                    {
                        ctx.LineTo(new Point(x + (float)Math.Cos(angle1) * 35, y + (float)Math.Sin(angle1) * 35), true, false);
                    }
                    ctx.LineTo(new Point(x + (float)Math.Cos(angle2) * 35, y + (float)Math.Sin(angle2) * 35), true, false);
                }
            }

            var glowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(80, color.R, color.G, color.B));
            context.DrawGeometry(glowBrush, null, outerRing);

            var outlinePen = new Pen(brush, 3);
            context.DrawGeometry(null, outlinePen, outerRing);

            context.DrawEllipse(null, new Pen(brush, 2), new Point(x, y), 35, 35);
            context.DrawEllipse(brush, null, new Point(x, y), 12, 12);
        }

        private void DrawEffects(DrawingContext context)
        {
            foreach (var effect in _visualEffects)
            {
                if (effect.Type == EffectType.HitText || effect.Type == EffectType.Combo)
                {
                    byte alpha = (byte)(effect.Alpha * 255);
                    var effectBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, effect.Color.R, effect.Color.G, effect.Color.B));

                    var effectFormatted = new FormattedText(
                        effect.Text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight,
                        new Typeface("Arial Bold"),
                        (int)(24 * effect.Scale), effectBrush, 1.0);
                    context.DrawText(effectFormatted, new Point(effect.Position.X - effectFormatted.Width / 2, effect.Position.Y));
                }
            }
        }

        private void DrawParticles(DrawingContext context)
        {
            foreach (var particle in _particles)
            {
                byte alpha = (byte)(particle.Alpha * 255);
                var particleBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(alpha, particle.Color.R, particle.Color.G, particle.Color.B));

                float size = particle.Size * particle.Alpha;
                context.DrawEllipse(particleBrush, null, new Point(particle.Position.X, particle.Position.Y), size, size);
            }
        }

        private void DrawHUD(DrawingContext context)
        {
            DrawScorePanel(context);
            DrawStatsPanel(context);
            DrawProgressBar(context);
            DrawComboDisplay(context);
        }

        private void DrawScorePanel(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 20, 20, 40));
            context.DrawRectangle(panelBrush, null, new Rect(20, 20, 320, 180));

            var titleBrush = new SolidColorBrush(Color.FromRgb(100, 100, 120));
            var textBrush = new SolidColorBrush(Colors.White);
            var accentBrush = new SolidColorBrush(Color.FromRgb(255, 200, 100));

            var scoreTitle = new FormattedText(
                "SCORE",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                16, titleBrush, 1.0);
            context.DrawText(scoreTitle, new Point(40, 30));

            var scoreFormatted = new FormattedText(
                $"{_score:N0}",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                36, accentBrush, 1.0);
            context.DrawText(scoreFormatted, new Point(40, 50));

            var accuracyTitle = new FormattedText(
                "ACCURACY",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                14, titleBrush, 1.0);
            context.DrawText(accuracyTitle, new Point(40, 100));

            Color accuracyColor = _accuracy >= 90 ? Color.FromRgb(100, 255, 100) :
                                 _accuracy >= 70 ? Color.FromRgb(255, 200, 100) :
                                 Color.FromRgb(255, 100, 100);
            var accuracyFormatted = new FormattedText(
                $"{_accuracy:F2}%",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                24, new SolidColorBrush(accuracyColor), 1.0);
            context.DrawText(accuracyFormatted, new Point(40, 120));

            var gradeFormatted = new FormattedText(
                GetGrade(),
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                40, new SolidColorBrush(GetGradeColor()), 1.0);
            context.DrawText(gradeFormatted, new Point(250, 45));
        }

        private Color GetGradeColor()
        {
            string grade = GetGrade();
            return grade switch
            {
                "SS" or "S" => Color.FromRgb(255, 255, 100),
                "A" => Color.FromRgb(100, 255, 100),
                "B" => Color.FromRgb(100, 200, 255),
                "C" => Color.FromRgb(255, 200, 100),
                "D" => Color.FromRgb(255, 150, 100),
                _ => Color.FromRgb(255, 100, 100)
            };
        }

        private void DrawStatsPanel(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 20, 20, 40));
            context.DrawRectangle(panelBrush, null, new Rect(1920 - 200, 20, 180, 150));

            var stats = new (string Label, int Count, Color Color)[]
            {
                ("PERFECT", _perfectCount, Color.FromRgb(255, 255, 100)),
                ("GREAT", _greatCount, Color.FromRgb(100, 255, 100)),
                ("GOOD", _goodCount, Color.FromRgb(100, 200, 255)),
                ("BAD", _badCount, Color.FromRgb(255, 150, 100)),
                ("MISS", _missCount, Color.FromRgb(255, 100, 100))
            };

            for (int i = 0; i < stats.Length; i++)
            {
                var statBrush = new SolidColorBrush(stats[i].Color);
                var statFormatted = new FormattedText(
                    $"{stats[i].Label}: {stats[i].Count}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    14, statBrush, 1.0);
                context.DrawText(statFormatted, new Point(1930, 30 + i * 25));
            }
        }

        private void DrawProgressBar(DrawingContext context)
        {
            var barBgBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 50, 50, 70));
            context.DrawRectangle(barBgBrush, null, new Rect(20, 210, 320, 12));

            float progress = Progress;
            var progressBrush = new SolidColorBrush(Color.FromRgb(100, 200, 255));
            context.DrawRectangle(progressBrush, null, new Rect(20, 210, 320 * progress, 12));

            var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(80, 80, 100)), 1);
            context.DrawRectangle(null, borderPen, new Rect(20, 210, 320, 12));

            if (_currentBeatmap != null)
            {
                float timeRemaining = Math.Max(0, _currentBeatmap.Duration - _songTime);
                var timeBrush = new SolidColorBrush(Color.FromRgb(150, 150, 170));
                var timeFormatted = new FormattedText(
                    $"{(int)timeRemaining / 60}:{((int)timeRemaining % 60):D2}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    14, timeBrush, 1.0);
                context.DrawText(timeFormatted, new Point(280, 228));
            }
        }

        private void DrawComboDisplay(DrawingContext context)
        {
            if (_combo <= 0)
                return;

            float comboScale = 1.0f + Math.Min(_combo * 0.02f, 1.0f);

            if (_combo >= 10)
            {
                var comboGlowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 255, 255, 100));
                context.DrawEllipse(comboGlowBrush, null, new Point(960, 250), 100 * comboScale, 60 * comboScale);
            }

            var comboText = $"{_combo}x";
            var comboColor = _combo >= 50 ? Color.FromRgb(255, 255, 100) :
                            _combo >= 20 ? Color.FromRgb(255, 200, 100) :
                            Colors.White;

            var comboFormatted = new FormattedText(
                comboText,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                (int)(48 * comboScale), new SolidColorBrush(comboColor), 1.0);
            context.DrawText(comboFormatted, new Point(960 - comboFormatted.Width / 2, 200));

            var comboLabel = new FormattedText(
                "COMBO",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                16, new SolidColorBrush(Color.FromRgb(150, 150, 170)), 1.0);
            context.DrawText(comboLabel, new Point(960 - comboLabel.Width / 2, 200 + 50 * comboScale));
        }

        private void DrawPracticeOverlay(DrawingContext context)
        {
            if (_practiceMode == PracticeMode.Off)
                return;

            var practiceBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 60, 90));
            context.DrawRectangle(practiceBrush, null, new Rect(20, 920, 300, 120));

            var textBrush = new SolidColorBrush(Colors.White);

            var practiceTitle = new FormattedText(
                "PRACTICE MODE",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                16, textBrush, 1.0);
            context.DrawText(practiceTitle, new Point(40, 930));

            string[] practiceInfo = {
                $"M: Metronome {( _isMetronomeEnabled ? "ON" : "OFF")}",
                $"P: Slow Motion {( _isSlowMotionActive ? "ON" : "OFF")}",
                $"BPM: {_currentBPM:F0}"
            };

            for (int i = 0; i < practiceInfo.Length; i++)
            {
                var infoFormatted = new FormattedText(
                    practiceInfo[i],
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    14, textBrush, 1.0);
                context.DrawText(infoFormatted, new Point(40, 960 + i * 25));
            }
        }

        private void DrawPauseOverlay(DrawingContext context)
        {
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 0, 0, 20));
            context.DrawRectangle(overlayBrush, null, new Rect(0, 0, 1920, 1080));

            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(30, 30, 50));
            context.DrawRectangle(panelBrush, null, new Rect(710, 334, 500, 412));

            var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(100, 150, 200)), 2);
            context.DrawRectangle(null, borderPen, new Rect(710, 334, 500, 412));

            var titleBrush = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            var textBrush = new SolidColorBrush(Colors.White);
            var hintBrush = new SolidColorBrush(Color.FromRgb(120, 120, 140));

            var titleFormatted = new FormattedText(
                "PAUSED",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                56, titleBrush, 1.0);
            context.DrawText(titleFormatted, new Point(960 - titleFormatted.Width / 2, 370));

            var statsBgBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 20, 20, 40));
            context.DrawRectangle(statsBgBrush, null, new Rect(760, 450, 400, 150));

            DrawPauseStat(context, "Score", $"{_score:N0}", 470);
            DrawPauseStat(context, "Combo", $"{_combo}x", 510);
            DrawPauseStat(context, "Accuracy", $"{_accuracy:F2}%", 550);

            var resumeFormatted = new FormattedText(
                "Press ESC to Resume",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                24, textBrush, 1.0);
            context.DrawText(resumeFormatted, new Point(960 - resumeFormatted.Width / 2, 640));

            var quitFormatted = new FormattedText(
                "Press Q to Quit to Menu",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                18, hintBrush, 1.0);
            context.DrawText(quitFormatted, new Point(960 - quitFormatted.Width / 2, 700));
        }

        private void DrawPauseStat(DrawingContext context, string label, string value, float y)
        {
            var labelBrush = new SolidColorBrush(Color.FromRgb(150, 150, 170));
            var valueBrush = new SolidColorBrush(Colors.White);

            var labelFormatted = new FormattedText(
                label + ":",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                20, labelBrush, 1.0);
            context.DrawText(labelFormatted, new Point(800, y));

            var valueFormatted = new FormattedText(
                value,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                24, valueBrush, 1.0);
            context.DrawText(valueFormatted, new Point(1050, y));
        }

        private void DrawResults(DrawingContext context)
        {
            var overlayBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(230, 5, 5, 20));
            context.DrawRectangle(overlayBrush, null, new Rect(0, 0, 1920, 1080));

            string grade = GetDetailedGrade();
            Color gradeColor = GetGradeColor();

            var gradeBrush = new SolidColorBrush(gradeColor);
            var titleBrush = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            var textBrush = new SolidColorBrush(Colors.White);
            var subtitleBrush = new SolidColorBrush(Color.FromRgb(150, 150, 180));

            var gradeFormatted = new FormattedText(
                grade,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                140, gradeBrush, 1.0);
            context.DrawText(gradeFormatted, new Point(960 - gradeFormatted.Width / 2, 50));

            if (_currentBeatmap != null)
            {
                var songFormatted = new FormattedText(
                    _currentBeatmap.Name,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial Bold"),
                    32, titleBrush, 1.0);
                context.DrawText(songFormatted, new Point(960 - songFormatted.Width / 2, 220));
            }

            DrawResultsPanel(context);

            var continueFormatted = new FormattedText(
                "Press ENTER to Continue | R to Retry",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                22, subtitleBrush, 1.0);
            context.DrawText(continueFormatted, new Point(960 - continueFormatted.Width / 2, 900));

            var menuFormatted = new FormattedText(
                "Press ESC for Main Menu",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                18, subtitleBrush, 1.0);
            context.DrawText(menuFormatted, new Point(960 - menuFormatted.Width / 2, 950));
        }

        private void DrawResultsPanel(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 30, 50));
            context.DrawRectangle(panelBrush, null, new Rect(560, 280, 800, 550));

            var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(80, 120, 160)), 2);
            context.DrawRectangle(null, borderPen, new Rect(560, 280, 800, 550));

            int y = 310;
            int lineHeight = 50;

            DrawResultStat(context, "SCORE", $"{_score:N0}", Colors.White, ref y, lineHeight, 32, true);

            Color accuracyColor = _accuracy >= 90 ? Color.FromRgb(100, 255, 100) :
                                 _accuracy >= 70 ? Color.FromRgb(255, 200, 100) :
                                 Color.FromRgb(255, 100, 100);
            DrawResultStat(context, "ACCURACY", $"{_accuracy:F2}%", accuracyColor, ref y, lineHeight, 28);

            DrawResultStat(context, "MAX COMBO", $"{_maxCombo}x", Color.FromRgb(255, 255, 100), ref y, lineHeight, 24);

            y += 20;

            DrawResultStat(context, "PERFECT", $"{_perfectCount}", Color.FromRgb(255, 255, 100), ref y, lineHeight, 22);
            DrawResultStat(context, "GREAT", $"{_greatCount}", Color.FromRgb(100, 255, 100), ref y, lineHeight, 22);
            DrawResultStat(context, "GOOD", $"{_goodCount}", Color.FromRgb(100, 200, 255), ref y, lineHeight, 22);
            DrawResultStat(context, "BAD", $"{_badCount}", Color.FromRgb(255, 150, 100), ref y, lineHeight, 22);
            DrawResultStat(context, "MISS", $"{_missCount}", Color.FromRgb(255, 100, 100), ref y, lineHeight, 22);

            if (_totalNotes > 0)
            {
                y += 10;
                float hitRate = (float)(_perfectCount + _greatCount + _goodCount) / _totalNotes * 100;
                var hitRateBrush = new SolidColorBrush(Color.FromRgb(150, 150, 180));
                var hitRateFormatted = new FormattedText(
                    $"Hit Rate: {hitRate:F1}%",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    20, hitRateBrush, 1.0);
                context.DrawText(hitRateFormatted, new Point(960 - hitRateFormatted.Width / 2, y));
            }
        }

        private void DrawResultStat(DrawingContext context, string label, string value, Color valueColor, ref int y, int lineHeight, int fontSize, bool isLarge = false)
        {
            var labelBrush = new SolidColorBrush(Color.FromRgb(150, 150, 180));
            var valueBrush = new SolidColorBrush(valueColor);

            var labelFormatted = new FormattedText(
                label,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                fontSize, labelBrush, 1.0);
            context.DrawText(labelFormatted, new Point(620, y));

            var valueFormatted = new FormattedText(
                value,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface(isLarge ? "Arial Bold" : "Arial"),
                fontSize + 4, valueBrush, 1.0);
            context.DrawText(valueFormatted, new Point(1100, y));

            y += lineHeight;
        }

        private void DrawSettings(DrawingContext context)
        {
            var bgBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 15, 30));
            context.DrawRectangle(bgBrush, null, new Rect(0, 0, 1920, 1080));

            var titleBrush = new SolidColorBrush(Color.FromRgb(255, 200, 100));
            var textBrush = new SolidColorBrush(Colors.White);
            var hintBrush = new SolidColorBrush(Color.FromRgb(120, 120, 140));

            var titleFormatted = new FormattedText(
                "SETTINGS",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial Bold"),
                56, titleBrush, 1.0);
            context.DrawText(titleFormatted, new Point(960 - titleFormatted.Width / 2, 80));

            DrawSettingItems(context);

            var backFormatted = new FormattedText(
                "Press ESC to Go Back",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                new Typeface("Arial"),
                20, hintBrush, 1.0);
            context.DrawText(backFormatted, new Point(960 - backFormatted.Width / 2, 950));
        }

        private void DrawSettingItems(DrawingContext context)
        {
            var settings = _settings.GetSettingNames();
            int y = 200;

            for (int i = 0; i < settings.Length; i++)
            {
                bool isSelected = i == _settings.SelectedSettingIndex;

                if (isSelected)
                {
                    var selectedBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 100, 150, 200));
                    context.DrawRectangle(selectedBrush, null, new Rect(560, y - 5, 800, 60));
                }

                var labelBrush = new SolidColorBrush(isSelected ? Color.FromRgb(255, 255, 255) : Color.FromRgb(150, 150, 170));
                var valueBrush = new SolidColorBrush(isSelected ? Color.FromRgb(100, 200, 255) : Color.FromRgb(150, 150, 170));

                var labelFormatted = new FormattedText(
                    settings[i],
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"),
                    22, labelBrush, 1.0);
                context.DrawText(labelFormatted, new Point(600, y + 5));

                string value = GetSettingValue(i);
                var valueFormatted = new FormattedText(
                    value,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial Bold"),
                    22, valueBrush, 1.0);
                context.DrawText(valueFormatted, new Point(1200, y + 5));

                if (isSelected)
                {
                    var arrowBrush = new SolidColorBrush(Color.FromRgb(100, 200, 255));
                    context.DrawText(new FormattedText("<", System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 24, arrowBrush, 1.0), new Point(1180, y + 3));
                    context.DrawText(new FormattedText(">", System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 24, arrowBrush, 1.0), new Point(1320, y + 3));
                }

                y += 70;
            }
        }

        private string GetSettingValue(int index)
        {
            return index switch
            {
                0 => $"{_noteSpeed:F0}",
                1 => $"{_audioOffset:F0}ms",
                2 => $"{(_settings.MasterVolume * 100):F0}%",
                3 => $"{(_settings.MusicVolume * 100):F0}%",
                4 => $"{(_settings.SfxVolume * 100):F0}%",
                5 => _isMetronomeEnabled ? "ON" : "OFF",
                6 => _settings.AutoNoteSpeed ? "AUTO" : "MANUAL",
                _ => ""
            };
        }

        private Color GetNoteColor(NoteType type)
        {
            return type switch
            {
                NoteType.Tap => Color.FromRgb(255, 200, 100),
                NoteType.Hold => Color.FromRgb(100, 200, 255),
                NoteType.Slide => Color.FromRgb(255, 100, 200),
                NoteType.Spin => Color.FromRgb(200, 100, 255),
                _ => Color.FromRgb(255, 255, 255)
            };
        }

        public override void Dispose()
        {
            _ui?.Dispose();
            _noteManager?.Clear();
            _visualEffects.Clear();
            _particles.Clear();
            base.Dispose();
        }
    }

    public class Lane
    {
        public int Index { get; set; }
        public Vector2 Position { get; set; }
        public bool IsActive { get; set; }
        public float FlashIntensity { get; set; }
        public float ApproachIntensity { get; set; }
        public int HitCount { get; set; }
        public int MissCount { get; set; }
        public Color Color { get; set; }
    }

    public class Note
    {
        public float Timing { get; set; }
        public int Lane { get; set; }
        public NoteType Type { get; set; }
        public float Duration { get; set; }
        public bool IsHit { get; set; }
        public bool IsHolding { get; set; }
        public bool Missed { get; set; }
        public bool Spawned { get; set; }
    }

    public class NoteData
    {
        public float Timing { get; set; }
        public int Lane { get; set; }
        public NoteType Type { get; set; }
        public float Duration { get; set; }
        public bool Spawned { get; set; }
    }

    public class NoteManager
    {
        private List<Note> _activeNotes;
        private int _laneCount;

        public List<Note> ActiveNotes => _activeNotes;

        public NoteManager(int laneCount)
        {
            _laneCount = laneCount;
            _activeNotes = new List<Note>();
        }

        public void SpawnNote(Note note)
        {
            _activeNotes.Add(note);
        }

        public void Update(float deltaTime, float currentTime)
        {
        }

        public void CleanupNotes(float currentTime)
        {
            _activeNotes.RemoveAll(n => n.IsHit || (currentTime - n.Timing > 1.0f) || n.Missed);
        }

        public void Clear()
        {
            _activeNotes.Clear();
        }
    }

    public class Beatmap
    {
        public string Name { get; set; } = "";
        public string Artist { get; set; } = "";
        public Difficulty Difficulty { get; set; }
        public Difficulty Level { get; set; }
        public float BPM { get; set; }
        public float Duration { get; set; }
        public float Offset { get; set; }
        public List<NoteData> Notes { get; set; } = new List<NoteData>();
        public List<BPMChange> BPMChanges { get; set; } = new List<BPMChange>();
    }

    public class BPMChange
    {
        public float Time { get; set; }
        public float BPM { get; set; }
    }

    public class HitResult
    {
        public HitRating Rating { get; set; }
        public float Timing { get; set; }
        public int Lane { get; set; }
    }

    public enum EffectType
    {
        HitText,
        Combo,
        Particle,
        LaneFlash
    }

    public class VisualEffect
    {
        public EffectType Type { get; set; }
        public string Text { get; set; } = "";
        public Color Color { get; set; }
        public Vector2 Position { get; set; }
        public float LifeTime { get; set; }
        public float Age { get; set; }
        public float Scale { get; set; }
        public float VelocityY { get; set; }
        public float Alpha { get; set; }
    }

    public class ParticleEffect
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Color Color { get; set; }
        public float Size { get; set; }
        public float LifeTime { get; set; }
        public float Age { get; set; }
        public float Alpha { get; set; }
    }

    public class SlideTracker
    {
        public int StartLane { get; set; }
        public float StartTime { get; set; }
        public SlideDirection Direction { get; set; }
    }

    public class SpinTracker
    {
        public bool IsTracking { get; set; }
        public float StartTime { get; set; }
        public int RequiredRotations { get; set; }
        public float CurrentProgress { get; set; }
    }

    public class RhythmSettings
    {
        public float MasterVolume { get; set; } = 1.0f;
        public float MusicVolume { get; set; } = 0.8f;
        public float SfxVolume { get; set; } = 1.0f;
        public TimingDifficulty TimingDifficulty { get; set; } = TimingDifficulty.Normal;
        public bool AutoNoteSpeed { get; set; } = false;
        public int SelectedSettingIndex { get; set; } = 0;
        public int SettingCount => 7;

        public string[] GetSettingNames()
        {
            return new string[]
            {
                "Note Speed",
                "Audio Offset",
                "Master Volume",
                "Music Volume",
                "SFX Volume",
                "Metronome",
                "Auto Speed"
            };
        }
    }

    public enum TimingDifficulty
    {
        Easy,
        Normal,
        Hard
    }

    public class AudioCalibration
    {
        public int Offset { get; set; } = 0;
        public float LatencyCompensation { get; set; } = 0;
        public bool UseSystemLatency { get; set; } = true;
    }

    public class RhythmUI
    {
        private RhythmGame _game;
        private bool _isInitialized;
        private bool _pauseOverlayVisible;
        private float _transitionProgress;
        private float _comboDisplayTime;
        private int _displayedCombo;

        public RhythmUI()
        {
            _isInitialized = false;
            _pauseOverlayVisible = false;
            _transitionProgress = 0f;
            _comboDisplayTime = 0f;
            _displayedCombo = 0;
        }

        public void Initialize(RhythmGame game)
        {
            _game = game;
            _isInitialized = true;
        }

        public void Update(float deltaTime)
        {
            if (_game != null && _game.Combo > _displayedCombo && _game.Combo > 0)
            {
                _comboDisplayTime = 1.0f;
            }

            if (_game != null)
            {
                _displayedCombo = _game.Combo;
            }

            if (_comboDisplayTime > 0)
            {
                _comboDisplayTime -= deltaTime;
            }

            _transitionProgress = Math.Max(0, _transitionProgress - deltaTime * 2);
        }

        public void ShowPauseOverlay()
        {
            _pauseOverlayVisible = true;
        }

        public void HidePauseOverlay()
        {
            _pauseOverlayVisible = false;
        }

        public void HideAllOverlays()
        {
            _pauseOverlayVisible = false;
        }

        public void Dispose()
        {
            _isInitialized = false;
        }
    }

    public static class Colors
    {
        public static Color White => Color.FromRgb(255, 255, 255);
        public static Color Black => Color.FromRgb(0, 0, 0);
        public static Color Red => Color.FromRgb(255, 0, 0);
        public static Color Green => Color.FromRgb(0, 255, 0);
        public static Color Blue => Color.FromRgb(0, 0, 255);
        public static Color Yellow => Color.FromRgb(255, 255, 0);
        public static Color Cyan => Color.FromRgb(0, 255, 255);
        public static Color Magenta => Color.FromRgb(255, 0, 255);
    }
}

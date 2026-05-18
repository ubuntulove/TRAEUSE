using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using WPFEngine.Math;
using WPFEngine.Scene;
using WPFEngine.Rendering;

namespace WPFEngine.Debug
{
    public class DebugConsole
    {
        private static DebugConsole? _instance;
        public static DebugConsole Instance => _instance ??= new DebugConsole();

        private readonly List<DebugMessage> _messages;
        private readonly Dictionary<string, ConsoleCommand> _commands;
        private bool _isOpen;
        private int _maxMessages;
        private LogLevel _minLevel;

        public bool IsOpen
        {
            get => _isOpen;
            set => _isOpen = value;
        }

        public int MaxMessages
        {
            get => _maxMessages;
            set => _maxMessages = Math.Max(1, value);
        }

        public LogLevel MinLevel
        {
            get => _minLevel;
            set => _minLevel = value;
        }

        public event EventHandler<string>? CommandExecuted;
        public event EventHandler<DebugMessage>? MessageAdded;

        private DebugConsole()
        {
            _messages = new List<DebugMessage>();
            _commands = new Dictionary<string, ConsoleCommand>();
            _isOpen = false;
            _maxMessages = 100;
            _minLevel = LogLevel.Debug;

            RegisterDefaultCommands();
        }

        private void RegisterDefaultCommands()
        {
            RegisterCommand("help", "Shows available commands", () => GetHelp());
            RegisterCommand("clear", "Clears console messages", Clear);
            RegisterCommand("fps", "Shows current FPS", () => $"FPS: {Core.Game.GameTime.Instance?.FramesPerSecond ?? 0}");
            RegisterCommand("objects", "Shows object count", () => $"Objects: {SceneManager.Instance.CurrentScene?.GameObjects.Count ?? 0}");
            RegisterCommand("time", "Shows game time", () => $"Time: {Core.Game.GameTime.Instance?.TotalTimeSeconds ?? 0:F2}s");
        }

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (level < _minLevel)
                return;

            var debugMessage = new DebugMessage
            {
                Message = message,
                Level = level,
                Timestamp = DateTime.Now
            };

            _messages.Add(debugMessage);

            while (_messages.Count > _maxMessages)
            {
                _messages.RemoveAt(0);
            }

            MessageAdded?.Invoke(this, debugMessage);
        }

        public void LogInfo(string message) => Log(message, LogLevel.Info);
        public void LogWarning(string message) => Log(message, LogLevel.Warning);
        public void LogError(string message) => Log(message, LogLevel.Error);
        public void LogDebug(string message) => Log(message, LogLevel.Debug);

        public void RegisterCommand(string name, string description, Func<string> action)
        {
            _commands[name.ToLower()] = new ConsoleCommand
            {
                Name = name,
                Description = description,
                Action = action
            };
        }

        public void UnregisterCommand(string name)
        {
            _commands.Remove(name.ToLower());
        }

        public bool ExecuteCommand(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return false;

            string[] parts = input.Split(' ', 2);
            string commandName = parts[0].ToLower();
            string? args = parts.Length > 1 ? parts[1] : null;

            if (_commands.TryGetValue(commandName, out var command))
            {
                string result = command.Action() ?? "";
                if (!string.IsNullOrEmpty(result))
                {
                    Log($"> {result}", LogLevel.Info);
                }
                CommandExecuted?.Invoke(this, input);
                return true;
            }

            Log($"Unknown command: {commandName}", LogLevel.Warning);
            return false;
        }

        private string GetHelp()
        {
            var help = "Available commands:\n";
            foreach (var cmd in _commands.Values)
            {
                help += $"  {cmd.Name} - {cmd.Description}\n";
            }
            return help.TrimEnd('\n');
        }

        public void Clear()
        {
            _messages.Clear();
        }

        public List<DebugMessage> GetMessages(LogLevel? level = null)
        {
            if (level.HasValue)
            {
                return _messages.Where(m => m.Level == level.Value).ToList();
            }
            return new List<DebugMessage>(_messages);
        }

        public List<DebugMessage> GetMessagesSince(DateTime since)
        {
            return _messages.Where(m => m.Timestamp >= since).ToList();
        }
    }

    public class DebugMessage
    {
        public string Message { get; set; } = "";
        public LogLevel Level { get; set; }
        public DateTime Timestamp { get; set; }

        public string FormattedMessage => $"[{Timestamp:HH:mm:ss}] [{Level}] {Message}";
    }

    public class ConsoleCommand
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Func<string?> Action { get; set; } = () => null;
    }

    public class DebugDrawer
    {
        private readonly List<DebugLine> _lines;
        private readonly List<DebugCircle> _circles;
        private readonly List<DebugRect> _rects;
        private readonly List<DebugText> _texts;
        private float _duration;
        private bool _persistent;

        public DebugDrawer()
        {
            _lines = new List<DebugLine>();
            _circles = new List<DebugCircle>();
            _rects = new List<DebugRect>();
            _texts = new List<DebugText>();
            _duration = 0f;
            _persistent = false;
        }

        public void SetDuration(float duration, bool persistent = false)
        {
            _duration = duration;
            _persistent = persistent;
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            _lines.Add(new DebugLine { Start = start, End = end, Color = color, Thickness = thickness });
        }

        public void DrawCircle(Vector2 center, float radius, Color color, bool filled = false)
        {
            _circles.Add(new DebugCircle { Center = center, Radius = radius, Color = color, Filled = filled });
        }

        public void DrawRect(Vector2 position, Vector2 size, Color color, bool filled = false, float rotation = 0f)
        {
            _rects.Add(new DebugRect { Position = position, Size = size, Color = color, Filled = filled, Rotation = rotation });
        }

        public void DrawText(string text, Vector2 position, Color color, float fontSize = 12f)
        {
            _texts.Add(new DebugText { Text = text, Position = position, Color = color, FontSize = fontSize });
        }

        public void DrawWireCube(Vector2 center, Vector2 size, Color color)
        {
            float halfWidth = size.X / 2;
            float halfHeight = size.Y / 2;

            DrawLine(new Vector2(center.X - halfWidth, center.Y - halfHeight), new Vector2(center.X + halfWidth, center.Y - halfHeight), color);
            DrawLine(new Vector2(center.X + halfWidth, center.Y - halfHeight), new Vector2(center.X + halfWidth, center.Y + halfHeight), color);
            DrawLine(new Vector2(center.X + halfWidth, center.Y + halfHeight), new Vector2(center.X - halfWidth, center.Y + halfHeight), color);
            DrawLine(new Vector2(center.X - halfWidth, center.Y + halfHeight), new Vector2(center.X - halfWidth, center.Y - halfHeight), color);
        }

        public void DrawArrow(Vector2 start, Vector2 end, Color color, float headSize = 0.2f)
        {
            DrawLine(start, end, color);

            Vector2 direction = (end - start).Normalized();
            Vector2 perpendicular = new Vector2(-direction.Y, direction.X);

            Vector2 left = end - direction * headSize + perpendicular * headSize;
            Vector2 right = end - direction * headSize - perpendicular * headSize;

            DrawLine(end, left, color);
            DrawLine(end, right, color);
        }

        public void DrawPath(List<Vector2> path, Color color)
        {
            if (path.Count < 2)
                return;

            for (int i = 0; i < path.Count - 1; i++)
            {
                DrawLine(path[i], path[i + 1], color);
            }
        }

        public void Clear()
        {
            if (!_persistent)
            {
                _lines.Clear();
                _circles.Clear();
                _rects.Clear();
                _texts.Clear();
            }
        }

        public void Update(float deltaTime)
        {
            if (_persistent)
                return;

            _duration -= deltaTime;
            if (_duration <= 0)
            {
                Clear();
            }
        }

        public void Render(DrawingContext context)
        {
            foreach (var line in _lines)
            {
                DrawLineToContext(context, line);
            }

            foreach (var circle in _circles)
            {
                DrawCircleToContext(context, circle);
            }

            foreach (var rect in _rects)
            {
                DrawRectToContext(context, rect);
            }

            foreach (var text in _texts)
            {
                DrawTextToContext(context, text);
            }
        }

        private void DrawLineToContext(DrawingContext context, DebugLine line)
        {
            var pen = new Pen(new SolidColorBrush(Color.FromArgb(
                line.Color.GetAByte(), line.Color.GetRByte(), line.Color.GetGByte(), line.Color.GetBByte())),
                line.Thickness);

            context.DrawLine(pen, new Point(line.Start.X, line.Start.Y), new Point(line.End.X, line.End.Y));
        }

        private void DrawCircleToContext(DrawingContext context, DebugCircle circle)
        {
            var brush = circle.Filled
                ? new SolidColorBrush(Color.FromArgb(
                    circle.Color.GetAByte(), circle.Color.GetRByte(), circle.Color.GetGByte(), circle.Color.GetBByte()))
                : null;

            var pen = new Pen(new SolidColorBrush(Color.FromArgb(
                circle.Color.GetAByte(), circle.Color.GetRByte(), circle.Color.GetGByte(), circle.Color.GetBByte())),
                1);

            context.DrawEllipse(brush, pen,
                new Point(circle.Center.X, circle.Center.Y),
                circle.Radius, circle.Radius);
        }

        private void DrawRectToContext(DrawingContext context, DebugRect rect)
        {
            var brush = rect.Filled
                ? new SolidColorBrush(Color.FromArgb(
                    rect.Color.GetAByte(), rect.Color.GetRByte(), rect.Color.GetGByte(), rect.Color.GetBByte()))
                : null;

            var pen = new Pen(new SolidColorBrush(Color.FromArgb(
                rect.Color.GetAByte(), rect.Color.GetRByte(), rect.Color.GetGByte(), rect.Color.GetBByte())),
                1);

            context.DrawRectangle(brush, pen, new Rect(rect.Position.X, rect.Position.Y, rect.Size.X, rect.Size.Y));
        }

        private void DrawTextToContext(DrawingContext context, DebugText text)
        {
            var typeface = new Typeface("Consolas");
            var brush = new SolidColorBrush(Color.FromArgb(
                text.Color.GetAByte(), text.Color.GetRByte(), text.Color.GetGByte(), text.Color.GetBByte()));

            var formattedText = new FormattedText(
                text.Text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                text.FontSize,
                brush,
                1.0);

            context.DrawText(formattedText, new Point(text.Position.X, text.Position.Y));
        }

        private struct DebugLine
        {
            public Vector2 Start { get; set; }
            public Vector2 End { get; set; }
            public Color Color { get; set; }
            public float Thickness { get; set; }
        }

        private struct DebugCircle
        {
            public Vector2 Center { get; set; }
            public float Radius { get; set; }
            public Color Color { get; set; }
            public bool Filled { get; set; }
        }

        private struct DebugRect
        {
            public Vector2 Position { get; set; }
            public Vector2 Size { get; set; }
            public Color Color { get; set; }
            public bool Filled { get; set; }
            public float Rotation { get; set; }
        }

        private struct DebugText
        {
            public string Text { get; set; }
            public Vector2 Position { get; set; }
            public Color Color { get; set; }
            public float FontSize { get; set; }
        }
    }

    public class DebugPanel
    {
        private readonly Dictionary<string, DebugInfo> _infos;
        private bool _isVisible;
        private Vector2 _position;
        private Vector2 _size;
        private int _maxDisplayedItems;
        private readonly object _lock = new object();

        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        public Vector2 Size
        {
            get => _size;
            set => _size = value;
        }

        public int MaxDisplayedItems
        {
            get => _maxDisplayedItems;
            set => _maxDisplayedItems = Math.Max(1, value);
        }

        public event EventHandler? PanelUpdated;

        public DebugPanel()
        {
            _infos = new Dictionary<string, DebugInfo>();
            _isVisible = true;
            _position = new Vector2(10, 10);
            _size = new Vector2(300, 400);
            _maxDisplayedItems = 20;
        }

        public void SetInfo(string key, string value)
        {
            lock (_lock)
            {
                if (_infos.TryGetValue(key, out var info))
                {
                    info.Value = value;
                    info.LastUpdate = DateTime.Now;
                }
                else
                {
                    _infos[key] = new DebugInfo
                    {
                        Key = key,
                        Value = value,
                        LastUpdate = DateTime.Now
                    };
                }
            }
            PanelUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void SetInfo(string key, object value)
        {
            SetInfo(key, value.ToString() ?? "");
        }

        public void Increment(string key)
        {
            lock (_lock)
            {
                if (_infos.TryGetValue(key, out var info) && int.TryParse(info.Value, out int current))
                {
                    info.Value = (current + 1).ToString();
                    info.LastUpdate = DateTime.Now;
                }
                else
                {
                    SetInfo(key, 1);
                }
            }
            PanelUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Decrement(string key)
        {
            lock (_lock)
            {
                if (_infos.TryGetValue(key, out var info) && int.TryParse(info.Value, out int current))
                {
                    info.Value = (current - 1).ToString();
                    info.LastUpdate = DateTime.Now;
                }
            }
            PanelUpdated?.Invoke(this, EventArgs.Empty);
        }

        public string? GetInfo(string key)
        {
            lock (_lock)
            {
                return _infos.TryGetValue(key, out var info) ? info.Value : null;
            }
        }

        public void RemoveInfo(string key)
        {
            lock (_lock)
            {
                _infos.Remove(key);
            }
            PanelUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            lock (_lock)
            {
                _infos.Clear();
            }
            PanelUpdated?.Invoke(this, EventArgs.Empty);
        }

        public List<DebugInfo> GetAllInfo()
        {
            lock (_lock)
            {
                return _infos.Values.ToList();
            }
        }

        public void Render(DrawingContext context)
        {
            if (!_isVisible)
                return;

            var infos = GetAllInfo();
            if (infos.Count == 0)
                return;

            var backgroundBrush = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0));
            var textBrush = new SolidColorBrush(Colors.White);
            var headerBrush = new SolidColorBrush(Color.FromArgb(255, 100, 200, 255));
            var typeface = new Typeface("Consolas");

            context.DrawRectangle(backgroundBrush, null,
                new Rect(_position.X, _position.Y, _size.X, _size.Y));

            float y = _position.Y + 10;
            float x = _position.X + 10;

            var headerText = new FormattedText(
                "DEBUG PANEL",
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                14,
                headerBrush,
                1.0);

            context.DrawText(headerText, new Point(x, y));
            y += 25;

            int displayed = 0;
            foreach (var info in infos.Take(_maxDisplayedItems))
            {
                var text = new FormattedText(
                    $"{info.Key}: {info.Value}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    11,
                    textBrush,
                    1.0);

                context.DrawText(text, new Point(x, y));
                y += 16;
                displayed++;

                if (displayed >= _maxDisplayedItems)
                    break;
            }
        }
    }

    public class DebugInfo
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
        public DateTime LastUpdate { get; set; }
    }

    public class Profiler
    {
        private static Profiler? _instance;
        public static Profiler Instance => _instance ??= new Profiler();

        private readonly Dictionary<string, ProfilerSample> _samples;
        private readonly Stack<ProfilerSample> _sampleStack;
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        private Profiler()
        {
            _samples = new Dictionary<string, ProfilerSample>();
            _sampleStack = new Stack<ProfilerSample>();
            _isEnabled = true;
        }

        public void BeginSample(string name)
        {
            if (!_isEnabled)
                return;

            var sample = GetOrCreateSample(name);
            sample.CallCount++;
            sample.StartTime = DateTime.Now;

            _sampleStack.Push(sample);
        }

        public void EndSample()
        {
            if (!_isEnabled || _sampleStack.Count == 0)
                return;

            var sample = _sampleStack.Pop();
            sample.TotalTime += (DateTime.Now - sample.StartTime).TotalMilliseconds;

            if (sample.Parent != null)
            {
                sample.Parent.TotalChildrenTime += sample.TotalTime;
            }
        }

        public ProfilerSample? GetSample(string name)
        {
            return _samples.TryGetValue(name, out var sample) ? sample : null;
        }

        public List<ProfilerSample> GetAllSamples()
        {
            return _samples.Values.OrderByDescending(s => s.TotalTime).ToList();
        }

        public void Reset()
        {
            foreach (var sample in _samples.Values)
            {
                sample.Reset();
            }
        }

        public string GetReport()
        {
            var report = "=== Profiler Report ===\n";
            var sortedSamples = GetAllSamples();

            foreach (var sample in sortedSamples.Take(20))
            {
                float avgTime = sample.CallCount > 0 ? (float)(sample.TotalTime / sample.CallCount) : 0;
                report += $"{sample.Name}: {sample.TotalTime:F2}ms ({sample.CallCount} calls, avg: {avgTime:F3}ms)\n";
            }

            return report;
        }

        private ProfilerSample GetOrCreateSample(string name)
        {
            if (_samples.TryGetValue(name, out var sample))
            {
                return sample;
            }

            sample = new ProfilerSample
            {
                Name = name,
                Parent = _sampleStack.Count > 0 ? _sampleStack.Peek() : null
            };

            _samples[name] = sample;
            return sample;
        }
    }

    public class ProfilerSample
    {
        public string Name { get; set; } = "";
        public ProfilerSample? Parent { get; set; }
        public DateTime StartTime { get; set; }
        public double TotalTime { get; set; }
        public double TotalChildrenTime { get; set; }
        public int CallCount { get; set; }

        public double SelfTime => TotalTime - TotalChildrenTime;

        public void Reset()
        {
            TotalTime = 0;
            TotalChildrenTime = 0;
            CallCount = 0;
        }
    }

    public class PerformanceMonitor
    {
        private static PerformanceMonitor? _instance;
        public static PerformanceMonitor Instance => _instance ??= new PerformanceMonitor();

        private readonly CyclicArray<float> _fpsHistory;
        private readonly CyclicArray<float> _frameTimeHistory;
        private readonly CyclicArray<long> _memoryHistory;
        private DateTime _lastUpdate;
        private float _currentFps;
        private float _averageFps;
        private float _minFps;
        private float _maxFps;
        private long _currentMemory;
        private long _peakMemory;
        private int _sampleCount;

        public float CurrentFps => _currentFps;
        public float AverageFps => _averageFps;
        public float MinFps => _minFps;
        public float MaxFps => _maxFps;
        public long CurrentMemory => _currentMemory;
        public long PeakMemory => _peakMemory;
        public int SampleCount => _sampleCount;

        private PerformanceMonitor()
        {
            const int HistorySize = 60;
            _fpsHistory = new CyclicArray<float>(HistorySize);
            _frameTimeHistory = new CyclicArray<float>(HistorySize);
            _memoryHistory = new CyclicArray<long>(HistorySize);
            _lastUpdate = DateTime.Now;
            _currentFps = 0;
            _averageFps = 0;
            _minFps = float.MaxValue;
            _maxFps = 0;
            _currentMemory = 0;
            _peakMemory = 0;
            _sampleCount = 0;
        }

        public void Update(float deltaTime, float fps)
        {
            _fpsHistory.Add(fps);
            _frameTimeHistory.Add(deltaTime * 1000);
            _currentFps = fps;

            long memory = GC.GetTotalMemory(false);
            _memoryHistory.Add(memory);
            _currentMemory = memory;

            if (memory > _peakMemory)
                _peakMemory = memory;

            if (_fpsHistory.Count > 0)
            {
                _averageFps = 0;
                foreach (var f in _fpsHistory.GetEnumerable())
                {
                    _averageFps += f;
                }
                _averageFps /= _fpsHistory.Count;
            }

            _minFps = float.MaxValue;
            _maxFps = 0;
            foreach (var f in _fpsHistory.GetEnumerable())
            {
                if (f < _minFps) _minFps = f;
                if (f > _maxFps) _maxFps = f;
            }

            _sampleCount++;

            _lastUpdate = DateTime.Now;
        }

        public float GetAverageFrameTime()
        {
            float total = 0;
            int count = 0;
            foreach (var t in _frameTimeHistory.GetEnumerable())
            {
                total += t;
                count++;
            }
            return count > 0 ? total / count : 0;
        }

        public string GetReport()
        {
            return $"FPS: {_currentFps:F1} (avg: {_averageFps:F1}, min: {_minFps:F1}, max: {_maxFps:F1})\n" +
                   $"Frame Time: {GetAverageFrameTime():F2}ms\n" +
                   $"Memory: {_currentMemory / 1024 / 1024}MB (peak: {_peakMemory / 1024 / 1024}MB)";
        }
    }

    public class StatsDisplay
    {
        private readonly DebugPanel _panel;
        private readonly Profiler _profiler;
        private readonly PerformanceMonitor _monitor;
        private DateTime _lastUpdate;
        private float _updateInterval;
        private bool _showFps;
        private bool _showMemory;
        private bool _showObjects;
        private bool _showPhysics;
        private bool _showProfiler;

        public bool IsVisible
        {
            get => _panel.IsVisible;
            set => _panel.IsVisible = value;
        }

        public bool ShowFps
        {
            get => _showFps;
            set => _showFps = value;
        }

        public bool ShowMemory
        {
            get => _showMemory;
            set => _showMemory = value;
        }

        public bool ShowObjects
        {
            get => _showObjects;
            set => _showObjects = value;
        }

        public bool ShowPhysics
        {
            get => _showPhysics;
            set => _showPhysics = value;
        }

        public bool ShowProfiler
        {
            get => _showProfiler;
            set => _showProfiler = value;
        }

        public StatsDisplay()
        {
            _panel = new DebugPanel();
            _profiler = Profiler.Instance;
            _monitor = PerformanceMonitor.Instance;
            _lastUpdate = DateTime.Now;
            _updateInterval = 0.5f;
            _showFps = true;
            _showMemory = true;
            _showObjects = true;
            _showPhysics = true;
            _showProfiler = false;
        }

        public void Update(float deltaTime)
        {
            var now = DateTime.Now;
            if ((now - _lastUpdate).TotalSeconds < _updateInterval)
                return;

            _lastUpdate = now;

            if (_showFps)
            {
                _panel.SetInfo("FPS", $"{_monitor.CurrentFps:F1}");
                _panel.SetInfo("Avg FPS", $"{_monitor.AverageFps:F1}");
                _panel.SetInfo("Min FPS", $"{_monitor.MinFps:F1}");
                _panel.SetInfo("Max FPS", $"{_monitor.MaxFps:F1}");
                _panel.SetInfo("Frame Time", $"{deltaTime * 1000:F2}ms");
            }

            if (_showMemory)
            {
                _panel.SetInfo("Memory", $"{_monitor.CurrentMemory / 1024 / 1024}MB");
                _panel.SetInfo("Peak Memory", $"{_monitor.PeakMemory / 1024 / 1024}MB");
            }

            if (_showObjects)
            {
                var scene = SceneManager.Instance.CurrentScene;
                if (scene != null)
                {
                    _panel.SetInfo("Objects", scene.GameObjects.Count);
                }
            }

            if (_showPhysics)
            {
                _panel.SetInfo("Physics Bodies", Physics.PhysicsWorld.Instance.Bodies.Count);
                _panel.SetInfo("Physics Colliders", Physics.PhysicsWorld.Instance.Colliders.Count);
            }
        }

        public void Render(DrawingContext context)
        {
            _panel.Render(context);
        }

        public DebugPanel Panel => _panel;
    }
}

namespace WPFEngine.Editor
{
    public class SceneHierarchyWindow
    {
        private Scene? _scene;
        private GameObject? _selectedObject;
        private bool _isExpanded = true;
        private readonly List<string> _searchFilters;

        public event EventHandler<GameObject>? ObjectSelected;
        public event EventHandler<GameObject>? ObjectDeselected;

        public SceneHierarchyWindow()
        {
            _searchFilters = new List<string>();
        }

        public Scene? Scene
        {
            get => _scene;
            set
            {
                _scene = value;
                _selectedObject = null;
            }
        }

        public GameObject? SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject != value)
                {
                    if (_selectedObject != null)
                    {
                        ObjectDeselected?.Invoke(this, _selectedObject);
                    }

                    _selectedObject = value;

                    if (_selectedObject != null)
                    {
                        ObjectSelected?.Invoke(this, _selectedObject);
                    }
                }
            }
        }

        public void SelectObject(GameObject obj)
        {
            SelectedObject = obj;
        }

        public void DeselectObject()
        {
            SelectedObject = null;
        }

        public List<GameObject> GetFilteredObjects()
        {
            if (_scene == null)
                return new List<GameObject>();

            var objects = _scene.GameObjects.ToList();

            if (_searchFilters.Count > 0)
            {
                objects = objects.Where(o =>
                    _searchFilters.Any(f => o.Name.Contains(f, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            return objects;
        }

        public void AddSearchFilter(string filter)
        {
            if (!string.IsNullOrWhiteSpace(filter) && !_searchFilters.Contains(filter))
            {
                _searchFilters.Add(filter);
            }
        }

        public void RemoveSearchFilter(string filter)
        {
            _searchFilters.Remove(filter);
        }

        public void ClearSearchFilters()
        {
            _searchFilters.Clear();
        }
    }

    public class InspectorWindow
    {
        private GameObject? _target;
        private readonly Dictionary<string, object> _properties;

        public event EventHandler? TargetChanged;

        public InspectorWindow()
        {
            _properties = new Dictionary<string, object>();
        }

        public GameObject? Target
        {
            get => _target;
            set
            {
                _target = value;
                Refresh();
                TargetChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Refresh()
        {
            _properties.Clear();

            if (_target == null)
                return;

            _properties["Name"] = _target.Name;
            _properties["Active"] = _target.IsActive;
            _properties["Tag"] = _target.Tag ?? "";
            _properties["Layer"] = _target.Layer ?? "";

            if (_target.Transform != null)
            {
                _properties["Position"] = _target.Transform.Position;
                _properties["Rotation"] = _target.Transform.Rotation;
                _properties["Scale"] = _target.Transform.Scale;
            }

            foreach (var component in _target.Components)
            {
                _properties[$"Component: {component.GetType().Name}"] = component;
            }
        }

        public T? GetProperty<T>(string name)
        {
            if (_properties.TryGetValue(name, out var value) && value is T typed)
            {
                return typed;
            }
            return default;
        }

        public void SetProperty<T>(string name, T value)
        {
            if (_target == null)
                return;

            switch (name)
            {
                case "Name":
                    _target.Name = value?.ToString() ?? "";
                    break;
                case "Active":
                    if (value is bool active)
                        _target.IsActive = active;
                    break;
                case "Tag":
                    _target.Tag = value?.ToString() ?? "";
                    break;
                case "Position":
                    if (_target.Transform != null && value is Math.Vector2 pos)
                        _target.Transform.Position = pos;
                    break;
                case "Rotation":
                    if (_target.Transform != null && value is float rot)
                        _target.Transform.Rotation = rot;
                    break;
                case "Scale":
                    if (_target.Transform != null && value is Math.Vector2 scale)
                        _target.Transform.Scale = scale;
                    break;
            }

            _properties[name] = value!;
        }
    }

    public class EditorCamera
    {
        private Vector2 _position;
        private float _rotation;
        private float _zoom;
        private float _moveSpeed;
        private float _rotateSpeed;
        private float _zoomSpeed;
        private bool _isPanning;
        private bool _isRotating;
        private Vector2 _lastMousePosition;

        public Vector2 Position
        {
            get => _position;
            set => _position = value;
        }

        public float Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        public float Zoom
        {
            get => _zoom;
            set => _zoom = Math.Clamp(value, 0.1f, 10f);
        }

        public float MoveSpeed
        {
            get => _moveSpeed;
            set => _moveSpeed = value;
        }

        public float RotateSpeed
        {
            get => _rotateSpeed;
            set => _rotateSpeed = value;
        }

        public float ZoomSpeed
        {
            get => _zoomSpeed;
            set => _zoomSpeed = value;
        }

        public bool IsPanning
        {
            get => _isPanning;
            set => _isPanning = value;
        }

        public bool IsRotating
        {
            get => _isRotating;
            set => _isRotating = value;
        }

        public EditorCamera()
        {
            _position = Vector2.Zero;
            _rotation = 0f;
            _zoom = 1f;
            _moveSpeed = 10f;
            _rotateSpeed = 1f;
            _zoomSpeed = 0.1f;
            _isPanning = false;
            _isRotating = false;
        }

        public void Move(Vector2 direction, float deltaTime)
        {
            Vector2 rotatedDirection = direction.Rotate(_rotation);
            _position += rotatedDirection * _moveSpeed * _zoom * deltaTime;
        }

        public void Rotate(float angle)
        {
            _rotation += angle * _rotateSpeed;
        }

        public void ZoomIn()
        {
            Zoom /= (1f + _zoomSpeed);
        }

        public void ZoomOut()
        {
            Zoom *= (1f + _zoomSpeed);
        }

        public void StartPan(Vector2 mousePosition)
        {
            _isPanning = true;
            _lastMousePosition = mousePosition;
        }

        public void UpdatePan(Vector2 mousePosition)
        {
            if (!_isPanning)
                return;

            Vector2 delta = mousePosition - _lastMousePosition;
            _position -= delta / _zoom;
            _lastMousePosition = mousePosition;
        }

        public void EndPan()
        {
            _isPanning = false;
        }

        public void StartRotate(Vector2 mousePosition)
        {
            _isRotating = true;
            _lastMousePosition = mousePosition;
        }

        public void UpdateRotate(Vector2 mousePosition)
        {
            if (!_isRotating)
                return;

            float deltaX = mousePosition.X - _lastMousePosition.X;
            Rotate(deltaX * 0.5f);
            _lastMousePosition = mousePosition;
        }

        public void EndRotate()
        {
            _isRotating = false;
        }

        public Matrix3x3 GetViewMatrix()
        {
            return Matrix3x3.Translation(-_position) *
                   Matrix3x3.Rotation(-_rotation) *
                   Matrix3x3.Scale(new Vector2(_zoom));
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return GetViewMatrix().Inverse().Transform(screenPosition);
        }

        public Vector2 WorldToScreen(Vector2 worldPosition)
        {
            return GetViewMatrix().Transform(worldPosition);
        }
    }

    public class EditorGizmos
    {
        private readonly DebugDrawer _drawer;
        private bool _showPositionGizmo = true;
        private bool _showRotationGizmo = true;
        private bool _showScaleGizmo = true;
        private bool _showColliderGizmos = true;
        private float _gizmoSize = 1f;
        private Color _positionColor = Color.Red;
        private Color _rotationColor = Color.Green;
        private Color _scaleColor = Color.Blue;

        public bool ShowPositionGizmo
        {
            get => _showPositionGizmo;
            set => _showPositionGizmo = value;
        }

        public bool ShowRotationGizmo
        {
            get => _showRotationGizmo;
            set => _showRotationGizmo = value;
        }

        public bool ShowScaleGizmo
        {
            get => _showScaleGizmo;
            set => _showScaleGizmo = value;
        }

        public bool ShowColliderGizmos
        {
            get => _showColliderGizmos;
            set => _showColliderGizmos = value;
        }

        public float GizmoSize
        {
            get => _gizmoSize;
            set => _gizmoSize = Math.Max(0.1f, value);
        }

        public EditorGizmos()
        {
            _drawer = new DebugDrawer();
        }

        public void DrawPositionGizmo(Vector2 position, float rotation)
        {
            if (!_showPositionGizmo)
                return;

            float size = _gizmoSize;

            Vector2 forward = new Vector2(MathHelper.Cos(rotation), MathHelper.Sin(rotation)) * size;
            Vector2 right = forward.Perpendicular();

            _drawer.DrawArrow(position, position + forward * size, _positionColor);
            _drawer.DrawArrow(position, position + right * size, _rotationColor);
        }

        public void DrawRotationGizmo(Vector2 position, float rotation)
        {
            if (!_showRotationGizmo)
                return;

            _drawer.DrawCircle(position, _gizmoSize * 1.5f, _rotationColor, false);
            _drawer.DrawLine(
                position,
                position + new Vector2(MathHelper.Cos(rotation), MathHelper.Sin(rotation)) * _gizmoSize * 1.5f,
                _rotationColor
            );
        }

        public void DrawScaleGizmo(Vector2 position, Vector2 scale)
        {
            if (!_showScaleGizmo)
                return;

            Vector2 scaledSize = new Vector2(scale.X * _gizmoSize, scale.Y * _gizmoSize);
            _drawer.DrawRect(position - scaledSize / 2, scaledSize, _scaleColor, false);
        }

        public void DrawColliderBox(Vector2 position, Vector2 size, float rotation, Color color)
        {
            if (!_showColliderGizmos)
                return;

            Vector2 halfSize = size / 2;
            Vector2[] corners = new Vector2[]
            {
                new Vector2(-halfSize.X, -halfSize.Y),
                new Vector2(halfSize.X, -halfSize.Y),
                new Vector2(halfSize.X, halfSize.Y),
                new Vector2(-halfSize.X, halfSize.Y)
            };

            for (int i = 0; i < 4; i++)
            {
                corners[i] = position + corners[i].Rotate(rotation);
            }

            _drawer.DrawLine(corners[0], corners[1], color);
            _drawer.DrawLine(corners[1], corners[2], color);
            _drawer.DrawLine(corners[2], corners[3], color);
            _drawer.DrawLine(corners[3], corners[0], color);
        }

        public void DrawColliderCircle(Vector2 center, float radius, Color color)
        {
            if (!_showColliderGizmos)
                return;

            _drawer.DrawCircle(center, radius, color, false);
        }

        public DebugDrawer GetDrawer()
        {
            return _drawer;
        }

        public void Clear()
        {
            _drawer.Clear();
        }
    }

    public class EditorSelection
    {
        private readonly List<GameObject> _selectedObjects;
        private readonly List<GameObject> _cachedSelection;
        private bool _isMultiSelect;

        public IReadOnlyList<GameObject> SelectedObjects => _selectedObjects.AsReadOnly();
        public GameObject? PrimarySelection => _selectedObjects.Count > 0 ? _selectedObjects[0] : null;
        public int SelectionCount => _selectedObjects.Count;
        public bool IsMultiSelect => _isMultiSelect;

        public event EventHandler? SelectionChanged;

        public EditorSelection()
        {
            _selectedObjects = new List<GameObject>();
            _cachedSelection = new List<GameObject>();
            _isMultiSelect = false;
        }

        public void Select(GameObject obj)
        {
            if (!_selectedObjects.Contains(obj))
            {
                _selectedObjects.Clear();
                _selectedObjects.Add(obj);
                _isMultiSelect = false;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void SelectMultiple(IEnumerable<GameObject> objects)
        {
            _selectedObjects.Clear();
            _selectedObjects.AddRange(objects);
            _isMultiSelect = _selectedObjects.Count > 1;
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddToSelection(GameObject obj)
        {
            if (!_selectedObjects.Contains(obj))
            {
                _selectedObjects.Add(obj);
                _isMultiSelect = _selectedObjects.Count > 1;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void RemoveFromSelection(GameObject obj)
        {
            if (_selectedObjects.Remove(obj))
            {
                _isMultiSelect = _selectedObjects.Count > 1;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void ClearSelection()
        {
            if (_selectedObjects.Count > 0)
            {
                _selectedObjects.Clear();
                _isMultiSelect = false;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public void StoreSelection()
        {
            _cachedSelection.Clear();
            _cachedSelection.AddRange(_selectedObjects);
        }

        public void RestoreSelection()
        {
            _selectedObjects.Clear();
            _selectedObjects.AddRange(_cachedSelection.Where(o => o != null));
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool IsSelected(GameObject obj)
        {
            return _selectedObjects.Contains(obj);
        }

        public Vector2 GetSelectionCenter()
        {
            if (_selectedObjects.Count == 0)
                return Vector2.Zero;

            Vector2 center = Vector2.Zero;
            foreach (var obj in _selectedObjects)
            {
                if (obj.Transform != null)
                {
                    center += obj.Transform.Position;
                }
            }
            return center / _selectedObjects.Count;
        }
    }

    public class EditorTools
    {
        public enum ToolType
        {
            Select,
            Move,
            Rotate,
            Scale,
            Pan,
            Zoom
        }

        private ToolType _currentTool;
        private readonly EditorGizmos _gizmos;
        private Vector2 _toolStartPosition;
        private Vector2 _originalPosition;
        private float _originalRotation;
        private Vector2 _originalScale;

        public ToolType CurrentTool
        {
            get => _currentTool;
            set => _currentTool = value;
        }

        public EditorGizmos Gizmos => _gizmos;

        public event EventHandler<ToolType>? ToolChanged;
        public event EventHandler? ToolStarted;
        public event EventHandler? ToolEnded;

        public EditorTools()
        {
            _currentTool = ToolType.Select;
            _gizmos = new EditorGizmos();
            _toolStartPosition = Vector2.Zero;
            _originalPosition = Vector2.Zero;
            _originalRotation = 0f;
            _originalScale = Vector2.One;
        }

        public void SelectTool(ToolType tool)
        {
            if (_currentTool != tool)
            {
                _currentTool = tool;
                ToolChanged?.Invoke(this, tool);
            }
        }

        public void StartTool(Vector2 position)
        {
            _toolStartPosition = position;
            ToolStarted?.Invoke(this, EventArgs.Empty);
        }

        public void UpdateTool(Vector2 position, Vector2 delta)
        {
        }

        public void EndTool()
        {
            ToolEnded?.Invoke(this, EventArgs.Empty);
        }

        public void ApplyMove(GameObject obj, Vector2 delta)
        {
            if (obj.Transform != null)
            {
                obj.Transform.Position += delta;
            }
        }

        public void ApplyRotate(GameObject obj, float angle)
        {
            if (obj.Transform != null)
            {
                obj.Transform.Rotation += angle;
            }
        }

        public void ApplyScale(GameObject obj, Vector2 scale)
        {
            if (obj.Transform != null)
            {
                obj.Transform.Scale *= scale;
            }
        }
    }

    public class EditorWindow
    {
        private SceneHierarchyWindow _hierarchy;
        private InspectorWindow _inspector;
        private EditorCamera _camera;
        private EditorGizmos _gizmos;
        private EditorSelection _selection;
        private EditorTools _tools;
        private StatsDisplay _stats;
        private bool _isVisible;
        private bool _showGrid;
        private bool _showGizmos;
        private Color _gridColor;

        public SceneHierarchyWindow Hierarchy => _hierarchy;
        public InspectorWindow Inspector => _inspector;
        public EditorCamera Camera => _camera;
        public EditorGizmos Gizmos => _gizmos;
        public EditorSelection Selection => _selection;
        public EditorTools Tools => _tools;
        public StatsDisplay Stats => _stats;

        public bool IsVisible
        {
            get => _isVisible;
            set => _isVisible = value;
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set => _showGrid = value;
        }

        public bool ShowGizmos
        {
            get => _showGizmos;
            set => _showGizmos = value;
        }

        public Color GridColor
        {
            get => _gridColor;
            set => _gridColor = value;
        }

        public EditorWindow()
        {
            _hierarchy = new SceneHierarchyWindow();
            _inspector = new InspectorWindow();
            _camera = new EditorCamera();
            _gizmos = new EditorGizmos();
            _selection = new EditorSelection();
            _tools = new EditorTools();
            _stats = new StatsDisplay();
            _isVisible = true;
            _showGrid = true;
            _showGizmos = true;
            _gridColor = new Color(0.3f, 0.3f, 0.3f, 1f);

            _selection.SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object? sender, EventArgs e)
        {
            _inspector.Target = _selection.PrimarySelection;
        }

        public void Update(float deltaTime)
        {
            _stats.Update(deltaTime);
            _tools.Gizmos.GetDrawer().Update(deltaTime);
        }

        public void Render(DrawingContext context)
        {
            if (!_isVisible)
                return;

            if (_showGrid)
            {
                RenderGrid(context);
            }

            if (_showGizmos && _selection.PrimarySelection != null)
            {
                var obj = _selection.PrimarySelection;
                if (obj.Transform != null)
                {
                    _gizmos.DrawPositionGizmo(obj.Transform.Position, obj.Transform.Rotation);
                }
            }

            _stats.Render(context);
        }

        private void RenderGrid(DrawingContext context)
        {
            float gridSize = 1f;
            float gridExtent = 50f;
            float alpha = 0.3f;

            var gridBrush = new SolidColorBrush(Color.FromArgb(
                (byte)(alpha * 255),
                (byte)(_gridColor.R * 255),
                (byte)(_gridColor.G * 255),
                (byte)(_gridColor.B * 255)));

            var pen = new Pen(gridBrush, 1);

            for (float x = -gridExtent; x <= gridExtent; x += gridSize)
            {
                var worldStart = new Vector2(x, -gridExtent);
                var worldEnd = new Vector2(x, gridExtent);
                var screenStart = _camera.WorldToScreen(worldStart);
                var screenEnd = _camera.WorldToScreen(worldEnd);

                context.DrawLine(pen,
                    new Point(screenStart.X, screenStart.Y),
                    new Point(screenEnd.X, screenEnd.Y));
            }

            for (float y = -gridExtent; y <= gridExtent; y += gridSize)
            {
                var worldStart = new Vector2(-gridExtent, y);
                var worldEnd = new Vector2(gridExtent, y);
                var screenStart = _camera.WorldToScreen(worldStart);
                var screenEnd = _camera.WorldToScreen(worldEnd);

                context.DrawLine(pen,
                    new Point(screenStart.X, screenStart.Y),
                    new Point(screenEnd.X, screenEnd.Y));
            }
        }
    }
}

namespace WPFEngine.Core
{
    public class GameTime
    {
        private static GameTime? _instance;
        public static GameTime Instance => _instance ??= new GameTime();

        private TimeSpan _totalTime;
        private TimeSpan _deltaTime;
        private TimeSpan _unscaledDeltaTime;
        private double _timeScale;
        private bool _isPaused;
        private int _frameCount;

        public TimeSpan TotalTime => _totalTime;
        public TimeSpan DeltaTime => _deltaTime;
        public TimeSpan UnscaledDeltaTime => _unscaledDeltaTime;
        public double TimeScale
        {
            get => _timeScale;
            set => _timeScale = Math.Clamp(value, 0.0, 10.0);
        }
        public bool IsPaused => _isPaused;
        public float DeltaTimeSeconds => (float)_deltaTime.TotalSeconds;
        public float UnscaledDeltaTimeSeconds => (float)_unscaledDeltaTime.TotalSeconds;
        public float TotalTimeSeconds => (float)_totalTime.TotalSeconds;
        public float FramesPerSecond { get; private set; }
        public int FrameCount => _frameCount;

        private GameTime()
        {
            _timeScale = 1.0;
            _isPaused = false;
            _deltaTime = TimeSpan.Zero;
            _unscaledDeltaTime = TimeSpan.Zero;
            _totalTime = TimeSpan.Zero;
            FramesPerSecond = 0;
            _frameCount = 0;
        }

        public void Update(TimeSpan elapsed, TimeSpan unscaledElapsed)
        {
            if (_isPaused)
            {
                _deltaTime = TimeSpan.Zero;
                _unscaledDeltaTime = TimeSpan.Zero;
            }
            else
            {
                _unscaledDeltaTime = unscaledElapsed;
                _deltaTime = TimeSpan.FromTicks((long)(unscaledElapsed.Ticks * _timeScale));
                _totalTime += _deltaTime;
            }

            _frameCount++;
            if (_deltaTime.TotalSeconds > 0)
            {
                FramesPerSecond = (float)(1.0 / _deltaTime.TotalSeconds);
            }
        }

        public void Pause() => _isPaused = true;
        public void Resume() => _isPaused = false;
        public void TogglePause() => _isPaused = !_isPaused;

        public void Reset()
        {
            _totalTime = TimeSpan.Zero;
            _deltaTime = TimeSpan.Zero;
            _unscaledDeltaTime = TimeSpan.Zero;
            _frameCount = 0;
            FramesPerSecond = 0;
            _isPaused = false;
            _timeScale = 1.0;
        }
    }
}

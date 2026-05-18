using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WPFEngine.Math;

namespace WPFEngine.Utils
{
    public interface IEvent { }

    public class EventBus
    {
        private static EventBus? _instance;
        public static EventBus Instance => _instance ??= new EventBus();

        private readonly Dictionary<Type, List<WeakReference>> _subscribers;
        private readonly Dictionary<Type, List<Delegate>> _handlers;
        private readonly object _lock = new object();
        private bool _isProcessing;

        public bool IsProcessing => _isProcessing;

        private EventBus()
        {
            _subscribers = new Dictionary<Type, List<WeakReference>>();
            _handlers = new Dictionary<Type, List<Delegate>>();
        }

        public void Subscribe<T>(Action<T> handler, int priority = 0) where T : class, IEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_handlers.ContainsKey(type))
                {
                    _handlers[type] = new List<Delegate>();
                }
                _handlers[type].Add(handler);
                _handlers[type] = _handlers[type].OrderByDescending(h => GetPriority(h)).ToList();
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class, IEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_handlers.TryGetValue(type, out var handlers))
                {
                    handlers.Remove(handler);
                }
            }
        }

        public void Publish<T>(T eventData) where T : class, IEvent
        {
            List<Delegate>? handlers = null;

            lock (_lock)
            {
                if (_handlers.TryGetValue(typeof(T), out var h))
                {
                    handlers = new List<Delegate>(h);
                }
            }

            if (handlers != null)
            {
                _isProcessing = true;
                try
                {
                    foreach (var handler in handlers)
                    {
                        try
                        {
                            ((Action<T>)handler)(eventData);
                        }
                        catch (Exception ex)
                        {
                            System.Diagnostics.Debug.WriteLine($"Event handler exception: {ex.Message}");
                        }
                    }
                }
                finally
                {
                    _isProcessing = false;
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _handlers.Clear();
            }
        }

        public void Clear<T>() where T : class, IEvent
        {
            lock (_lock)
            {
                _handlers.Remove(typeof(T));
            }
        }

        private int GetPriority(Delegate d)
        {
            return 0;
        }

        public int GetSubscriberCount<T>() where T : class, IEvent
        {
            lock (_lock)
            {
                return _handlers.TryGetValue(typeof(T), out var h) ? h.Count : 0;
            }
        }

        public List<Type> GetEventTypes()
        {
            lock (_lock)
            {
                return new List<Type>(_handlers.Keys);
            }
        }
    }

    public class EventListener<T> where T : class, IEvent
    {
        private readonly Action<T> _handler;
        private readonly int _priority;
        private bool _isActive;

        public int Priority => _priority;
        public bool IsActive => _isActive;

        public EventListener(Action<T> handler, int priority = 0)
        {
            _handler = handler;
            _priority = priority;
            _isActive = true;
        }

        public void Start()
        {
            if (!_isActive)
            {
                EventBus.Instance.Subscribe<T>(_handler, _priority);
                _isActive = true;
            }
        }

        public void Stop()
        {
            if (_isActive)
            {
                EventBus.Instance.Unsubscribe(_handler);
                _isActive = false;
            }
        }

        public void Handle(T eventData)
        {
            if (_isActive)
            {
                _handler(eventData);
            }
        }
    }

    public class EventDispatcher
    {
        private readonly Dictionary<Type, List<Delegate>> _typedHandlers;
        private readonly object _lock = new object();

        public EventDispatcher()
        {
            _typedHandlers = new Dictionary<Type, List<Delegate>>();
        }

        public void RegisterHandler<T>(Action<T> handler) where T : IEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_typedHandlers.ContainsKey(type))
                {
                    _typedHandlers[type] = new List<Delegate>();
                }
                _typedHandlers[type].Add(handler);
            }
        }

        public void UnregisterHandler<T>(Action<T> handler) where T : IEvent
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_typedHandlers.TryGetValue(type, out var handlers))
                {
                    handlers.Remove(handler);
                }
            }
        }

        public void Dispatch<T>(T eventData) where T : IEvent
        {
            List<Delegate>? handlers = null;

            lock (_lock)
            {
                if (_typedHandlers.TryGetValue(typeof(T), out var h))
                {
                    handlers = new List<Delegate>(h);
                }
            }

            handlers?.ForEach(h => ((Action<T>)h)(eventData));
        }

        public void Clear()
        {
            lock (_lock)
            {
                _typedHandlers.Clear();
            }
        }
    }

    public class EventQueue
    {
        private readonly Queue<IEvent> _queue;
        private readonly Queue<IEvent> _processingQueue;
        private readonly object _lock = new object();
        private bool _isProcessing;

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public bool IsProcessing => _isProcessing;

        public EventQueue()
        {
            _queue = new Queue<IEvent>();
            _processingQueue = new Queue<IEvent>();
        }

        public void Enqueue<T>(T eventData) where T : class, IEvent
        {
            lock (_lock)
            {
                _queue.Enqueue(eventData);
            }
        }

        public bool TryDequeue<T>(out T? eventData) where T : class, IEvent
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    var evt = _queue.Dequeue();
                    if (evt is T typed)
                    {
                        eventData = typed;
                        return true;
                    }
                }
            }
            eventData = default;
            return false;
        }

        public void Process()
        {
            if (_isProcessing) return;

            lock (_lock)
            {
                while (_queue.Count > 0)
                {
                    _processingQueue.Enqueue(_queue.Dequeue());
                }
            }

            _isProcessing = true;
            try
            {
                while (_processingQueue.Count > 0)
                {
                    var evt = _processingQueue.Dequeue();
                    EventBus.Instance.Publish(evt);
                }
            }
            finally
            {
                _isProcessing = false;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
                _processingQueue.Clear();
            }
        }
    }

    public class EventDebugInfo
    {
        public Type EventType { get; set; } = typeof(IEvent);
        public string Name { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public int SubscriberCount { get; set; }
        public long ExecutionTimeMs { get; set; }
    }

    public class EventDebugVisualizer
    {
        private readonly List<EventDebugInfo> _eventLog;
        private readonly Dictionary<Type, EventDebugInfo> _eventStats;
        private readonly object _lock = new object();
        private bool _isEnabled;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public IReadOnlyList<EventDebugInfo> EventLog
        {
            get
            {
                lock (_lock)
                {
                    return _eventLog.ToList();
                }
            }
        }

        public IReadOnlyDictionary<Type, EventDebugInfo> EventStats
        {
            get
            {
                lock (_lock)
                {
                    return new Dictionary<Type, EventDebugInfo>(_eventStats);
                }
            }
        }

        public EventDebugVisualizer(int maxLogSize = 1000)
        {
            _eventLog = new List<EventDebugInfo>();
            _eventStats = new Dictionary<Type, EventDebugInfo>();
            _isEnabled = false;
        }

        public void LogEvent<T>(long executionTimeMs) where T : IEvent
        {
            if (!_isEnabled) return;

            var info = new EventDebugInfo
            {
                EventType = typeof(T),
                Name = typeof(T).Name,
                Timestamp = DateTime.Now,
                SubscriberCount = EventBus.Instance.GetSubscriberCount<T>(),
                ExecutionTimeMs = executionTimeMs
            };

            lock (_lock)
            {
                _eventLog.Add(info);
                if (_eventStats.TryGetValue(typeof(T), out var stats))
                {
                    stats.SubscriberCount = info.SubscriberCount;
                    stats.ExecutionTimeMs = (stats.ExecutionTimeMs + executionTimeMs) / 2;
                }
                else
                {
                    _eventStats[typeof(T)] = info;
                }
            }
        }

        public string GetSummary()
        {
            lock (_lock)
            {
                var lines = new List<string>
                {
                    "=== Event Debug Summary ===",
                    $"Total Events Logged: {_eventLog.Count}",
                    $"Unique Event Types: {_eventStats.Count}",
                    ""
                };

                foreach (var kvp in _eventStats.OrderByDescending(k => k.Value.ExecutionTimeMs))
                {
                    lines.Add($"{kvp.Value.Name}: Avg {kvp.Value.ExecutionTimeMs}ms, Subscribers: {kvp.Value.SubscriberCount}");
                }

                return string.Join(Environment.NewLine, lines);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _eventLog.Clear();
            }
        }
    }

    public interface ICommand
    {
        void Execute();
        void Undo();
        string Name { get; }
        bool CanExecute { get; }
    }

    public abstract class CommandBase : ICommand
    {
        public abstract void Execute();
        public abstract void Undo();
        public abstract string Name { get; }
        public virtual bool CanExecute => true;
    }

    public class CommandQueue
    {
        private readonly Stack<ICommand> _undoStack;
        private readonly Stack<ICommand> _redoStack;
        private readonly int _maxHistory;
        private bool _isExecuting;

        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;
        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public bool IsExecuting => _isExecuting;

        public event Action<ICommand>? CommandExecuted;
        public event Action<ICommand>? CommandUndone;
        public event Action<ICommand>? CommandRedone;

        public CommandQueue(int maxHistory = 100)
        {
            _undoStack = new Stack<ICommand>();
            _redoStack = new Stack<ICommand>();
            _maxHistory = maxHistory;
            _isExecuting = false;
        }

        public void Execute(ICommand command)
        {
            if (!command.CanExecute) return;

            _isExecuting = true;
            try
            {
                command.Execute();
                _undoStack.Push(command);
                _redoStack.Clear();

                while (_undoStack.Count > _maxHistory)
                {
                    var items = _undoStack.ToArray();
                    _undoStack.Clear();
                    for (int i = items.Length - 1; i > _maxHistory / 2; i--)
                    {
                        _undoStack.Push(items[i]);
                    }
                    break;
                }

                CommandExecuted?.Invoke(command);
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public bool Undo()
        {
            if (!CanUndo) return false;

            _isExecuting = true;
            try
            {
                var command = _undoStack.Pop();
                command.Undo();
                _redoStack.Push(command);
                CommandUndone?.Invoke(command);
                return true;
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public bool Redo()
        {
            if (!CanRedo) return false;

            _isExecuting = true;
            try
            {
                var command = _redoStack.Pop();
                command.Execute();
                _undoStack.Push(command);
                CommandRedone?.Invoke(command);
                return true;
            }
            finally
            {
                _isExecuting = false;
            }
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }

        public IReadOnlyList<ICommand> GetUndoHistory()
        {
            return _undoStack.ToArray().Reverse().ToList();
        }

        public IReadOnlyList<ICommand> GetRedoHistory()
        {
            return _redoStack.ToArray().Reverse().ToList();
        }
    }

    public class CommandInvoker
    {
        private readonly Dictionary<string, ICommand> _namedCommands;
        private readonly Queue<ICommand> _macroQueue;
        private bool _isRecording;
        private MacroCommand? _currentMacro;

        public bool IsRecording => _isRecording;
        public bool HasActiveMacro => _currentMacro != null;

        public CommandInvoker()
        {
            _namedCommands = new Dictionary<string, ICommand>();
            _macroQueue = new Queue<ICommand>();
            _isRecording = false;
        }

        public void RegisterCommand(string name, ICommand command)
        {
            _namedCommands[name] = command;
        }

        public void UnregisterCommand(string name)
        {
            _namedCommands.Remove(name);
        }

        public bool Execute(string name)
        {
            if (_namedCommands.TryGetValue(name, out var command))
            {
                if (command.CanExecute)
                {
                    command.Execute();
                    return true;
                }
            }
            return false;
        }

        public void Execute(ICommand command)
        {
            if (command.CanExecute)
            {
                command.Execute();
                if (_isRecording && _currentMacro != null)
                {
                    _currentMacro.AddCommand(command);
                }
            }
        }

        public void StartRecording()
        {
            _isRecording = true;
            _currentMacro = new MacroCommand("Recorded Macro");
        }

        public ICommand? StopRecording()
        {
            _isRecording = false;
            var macro = _currentMacro;
            _currentMacro = null;
            return macro?.Commands.Count > 0 ? macro : null;
        }

        public void CancelRecording()
        {
            _isRecording = false;
            _currentMacro = null;
        }

        public void ExecuteMacro(MacroCommand macro)
        {
            foreach (var command in macro.Commands)
            {
                if (command.CanExecute)
                {
                    command.Execute();
                }
            }
        }
    }

    public class MacroCommand : ICommand
    {
        public string Name { get; }
        public List<ICommand> Commands { get; }
        public bool CanExecute => Commands.Count > 0 && Commands.All(c => c.CanExecute);

        public MacroCommand(string name)
        {
            Name = name;
            Commands = new List<ICommand>();
        }

        public void AddCommand(ICommand command)
        {
            Commands.Add(command);
        }

        public void Execute()
        {
            foreach (var command in Commands)
            {
                if (command.CanExecute)
                {
                    command.Execute();
                }
            }
        }

        public void Undo()
        {
            for (int i = Commands.Count - 1; i >= 0; i--)
            {
                Commands[i].Undo();
            }
        }
    }

    public class MoveCommand : CommandBase
    {
        private readonly Action<Vector2> _setPosition;
        private readonly Vector2 _startPosition;
        private readonly Vector2 _endPosition;

        public override string Name => "Move";

        public MoveCommand(Action<Vector2> setPosition, Vector2 startPosition, Vector2 endPosition)
        {
            _setPosition = setPosition;
            _startPosition = startPosition;
            _endPosition = endPosition;
        }

        public override void Execute()
        {
            _setPosition(_endPosition);
        }

        public override void Undo()
        {
            _setPosition(_startPosition);
        }

        public override bool CanExecute => _startPosition != _endPosition;
    }

    public class RotateCommand : CommandBase
    {
        private readonly Action<float> _setRotation;
        private readonly float _startRotation;
        private readonly float _endRotation;

        public override string Name => "Rotate";

        public RotateCommand(Action<float> setRotation, float startRotation, float endRotation)
        {
            _setRotation = setRotation;
            _startRotation = startRotation;
            _endRotation = endRotation;
        }

        public override void Execute()
        {
            _setRotation(_endRotation);
        }

        public override void Undo()
        {
            _setRotation(_startRotation);
        }

        public override bool CanExecute => Math.Abs(_startRotation - _endRotation) > 0.001f;
    }

    public class ScaleCommand : CommandBase
    {
        private readonly Action<Vector2> _setScale;
        private readonly Vector2 _startScale;
        private readonly Vector2 _endScale;

        public override string Name => "Scale";

        public ScaleCommand(Action<Vector2> setScale, Vector2 startScale, Vector2 endScale)
        {
            _setScale = setScale;
            _startScale = startScale;
            _endScale = endScale;
        }

        public override void Execute()
        {
            _setScale(_endScale);
        }

        public override void Undo()
        {
            _setScale(_startScale);
        }

        public override bool CanExecute => _startScale != _endScale;
    }

    public class DeleteCommand : CommandBase
    {
        private readonly Action _deleteAction;
        private readonly Action _restoreAction;
        private bool _isDeleted;

        public override string Name => "Delete";

        public DeleteCommand(Action deleteAction, Action restoreAction)
        {
            _deleteAction = deleteAction;
            _restoreAction = restoreAction;
            _isDeleted = false;
        }

        public override void Execute()
        {
            if (!_isDeleted)
            {
                _deleteAction();
                _isDeleted = true;
            }
        }

        public override void Undo()
        {
            if (_isDeleted)
            {
                _restoreAction();
                _isDeleted = false;
            }
        }

        public override bool CanExecute => !_isDeleted;
    }

    public interface IState { }

    public class Transition<T> where T : IState
    {
        public T FromState { get; }
        public T ToState { get; }
        public Func<bool> Condition { get; }
        public Action? OnTransition { get; }

        public Transition(T from, T to, Func<bool> condition, Action? onTransition = null)
        {
            FromState = from;
            ToState = to;
            Condition = condition;
            OnTransition = onTransition;
        }
    }

    public class FiniteStateMachine<T> where T : IState
    {
        private readonly Dictionary<T, StateInfo> _states;
        private readonly List<Transition<T>> _transitions;
        private T? _currentState;
        private bool _isActive;

        public T? CurrentState => _currentState;
        public bool IsActive => _isActive;
        public IReadOnlyDictionary<T, StateInfo> States => _states.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        public event Action<T, T>? StateChanged;
        public event Action<T>? StateEntered;
        public event Action<T>? StateExited;

        public FiniteStateMachine()
        {
            _states = new Dictionary<T, StateInfo>();
            _transitions = new List<Transition<T>>();
            _isActive = false;
        }

        public void AddState(T state, Action? onEnter = null, Action? onUpdate = null, Action? onExit = null)
        {
            _states[state] = new StateInfo
            {
                State = state,
                OnEnter = onEnter,
                OnUpdate = onUpdate,
                OnExit = onExit
            };
        }

        public void AddTransition(T from, T to, Func<bool> condition, Action? onTransition = null)
        {
            _transitions.Add(new Transition<T>(from, to, condition, onTransition));
        }

        public void AddTransition(Transition<T> transition)
        {
            _transitions.Add(transition);
        }

        public void SetState(T state)
        {
            if (!_states.ContainsKey(state))
                throw new InvalidOperationException($"State {state} has not been registered");

            var previousState = _currentState;

            if (previousState != null && _states.TryGetValue(previousState, out var prevInfo))
            {
                prevInfo.OnExit?.Invoke();
                StateExited?.Invoke(previousState);
            }

            _currentState = state;

            if (_states.TryGetValue(state, out var info))
            {
                info.OnEnter?.Invoke();
                StateEntered?.Invoke(state);
            }

            StateChanged?.Invoke(previousState!, state);
        }

        public void Start(T initialState)
        {
            if (!_states.ContainsKey(initialState))
                throw new InvalidOperationException($"State {initialState} has not been registered");

            _isActive = true;
            SetState(initialState);
        }

        public void Stop()
        {
            _isActive = false;
            _currentState = default;
        }

        public void Update()
        {
            if (!_isActive || _currentState == null) return;

            if (_states.TryGetValue(_currentState, out var info))
            {
                info.OnUpdate?.Invoke();
            }

            foreach (var transition in _transitions)
            {
                if (EqualityComparer<T>.Default.Equals(transition.FromState, _currentState))
                {
                    if (transition.Condition())
                    {
                        transition.OnTransition?.Invoke();
                        SetState(transition.ToState);
                        break;
                    }
                }
            }
        }

        public bool CanTransitionTo(T state)
        {
            return _transitions.Any(t => EqualityComparer<T>.Default.Equals(t.FromState, _currentState) &&
                                         EqualityComparer<T>.Default.Equals(t.ToState, state) &&
                                         t.Condition());
        }

        public List<T> GetAvailableTransitions()
        {
            return _transitions
                .Where(t => EqualityComparer<T>.Default.Equals(t.FromState, _currentState) && t.Condition())
                .Select(t => t.ToState)
                .ToList();
        }

        private class StateInfo
        {
            public T State { get; set; } = default!;
            public Action? OnEnter { get; set; }
            public Action? OnUpdate { get; set; }
            public Action? OnExit { get; set; }
        }
    }

    public class StateMachineHistory<T> where T : IState
    {
        private readonly List<(T State, DateTime Timestamp)> _history;
        private readonly int _maxSize;

        public int Count => _history.Count;
        public IReadOnlyList<(T State, DateTime Timestamp)> History => _history;

        public StateMachineHistory(int maxSize = 100)
        {
            _history = new List<(T, DateTime)>();
            _maxSize = maxSize;
        }

        public void Record(T state)
        {
            _history.Add((state, DateTime.Now));
            if (_history.Count > _maxSize)
            {
                _history.RemoveAt(0);
            }
        }

        public T? GetPreviousState(int stepsBack = 1)
        {
            if (_history.Count < stepsBack) return default;
            return _history[_history.Count - stepsBack].State;
        }

        public List<T> GetRecentStates(int count)
        {
            return _history.TakeLast(count).Select(h => h.State).ToList();
        }

        public void Clear()
        {
            _history.Clear();
        }

        public TimeSpan TimeInState(T state)
        {
            var entries = _history.Where(h => EqualityComparer<T>.Default.Equals(h.State, state)).ToList();
            if (entries.Count < 2) return TimeSpan.Zero;

            TimeSpan total = TimeSpan.Zero;
            for (int i = 0; i < entries.Count - 1; i++)
            {
                total += entries[i + 1].Timestamp - entries[i].Timestamp;
            }
            return total;
        }
    }

    public class StateDebugInfo
    {
        public IState? CurrentState { get; set; }
        public List<IState> AvailableTransitions { get; set; } = new List<IState>();
        public TimeSpan TimeInCurrentState { get; set; }
        public int TransitionCount { get; set; }
        public Dictionary<IState, int> StateVisitCounts { get; set; } = new Dictionary<IState, int>();
    }

    public interface IPoolable
    {
        void OnPooled();
        void OnUnpooled();
    }

    public class PooledObject<T> where T : class
    {
        private readonly T _object;
        private readonly Action<T>? _resetAction;
        private bool _isPooled;

        public T Object => _object;
        public bool IsPooled => _isPooled;

        public PooledObject(T obj, Action<T>? resetAction)
        {
            _object = obj;
            _resetAction = resetAction;
            _isPooled = true;
        }

        public void Dispose()
        {
            if (!_isPooled)
            {
                _resetAction?.Invoke(_object);
                if (_object is IPoolable poolable)
                {
                    poolable.OnPooled();
                }
                _isPooled = true;
            }
        }
    }

    public class ObjectPool<T> where T : class
    {
        private readonly Stack<T> _available;
        private readonly Func<T> _factory;
        private readonly Action<T>? _resetAction;
        private readonly int _preloadCount;
        private int _totalCreated;
        private int _totalRented;
        private int _totalReturned;
        private readonly object _lock = new object();

        public int AvailableCount
        {
            get
            {
                lock (_lock)
                {
                    return _available.Count;
                }
            }
        }

        public int BorrowedCount => _totalRented - _totalReturned;
        public int TotalCreated => _totalCreated;
        public int TotalRented => _totalRented;
        public int TotalReturned => _totalReturned;

        public ObjectPool(Func<T> factory, Action<T>? resetAction = null, int preloadCount = 0)
        {
            _factory = factory ?? throw new ArgumentNullException(nameof(factory));
            _resetAction = resetAction;
            _available = new Stack<T>();

            for (int i = 0; i < preloadCount; i++)
            {
                var obj = _factory();
                _available.Push(obj);
                _totalCreated++;
            }
            _preloadCount = preloadCount;
        }

        public T Rent()
        {
            lock (_lock)
            {
                T obj;
                if (_available.Count > 0)
                {
                    obj = _available.Pop();
                }
                else
                {
                    obj = _factory();
                    _totalCreated++;
                }
                _totalRented++;

                if (obj is IPoolable poolable)
                {
                    poolable.OnUnpooled();
                }

                return obj;
            }
        }

        public PooledObject<T> RentWithTracking()
        {
            return new PooledObject<T>(Rent(), Return);
        }

        public void Return(T obj)
        {
            if (obj == null) return;

            lock (_lock)
            {
                _resetAction?.Invoke(obj);

                if (obj is IPoolable poolable)
                {
                    poolable.OnPooled();
                }

                _available.Push(obj);
                _totalReturned++;
            }
        }

        public List<T> RentMultiple(int count)
        {
            var result = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                result.Add(Rent());
            }
            return result;
        }

        public void ReturnMultiple(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                Return(obj);
            }
        }

        public void Preload(int count)
        {
            lock (_lock)
            {
                for (int i = 0; i < count; i++)
                {
                    var obj = _factory();
                    _available.Push(obj);
                    _totalCreated++;
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _available.Clear();
            }
        }

        public float GetUtilizationRate()
        {
            if (_totalCreated == 0) return 0;
            return (float)BorrowedCount / _totalCreated;
        }
    }

    public class PoolManager
    {
        private static PoolManager? _instance;
        public static PoolManager Instance => _instance ??= new PoolManager();

        private readonly Dictionary<Type, object> _pools;
        private readonly object _lock = new object();

        private PoolManager()
        {
            _pools = new Dictionary<Type, object>();
        }

        public ObjectPool<T> GetPool<T>(Func<T> factory, Action<T>? resetAction = null, int preloadCount = 0) where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_pools.TryGetValue(type, out var poolObj))
                {
                    poolObj = new ObjectPool<T>(factory, resetAction, preloadCount);
                    _pools[type] = poolObj;
                }
                return (ObjectPool<T>)poolObj;
            }
        }

        public T Rent<T>() where T : class
        {
            return GetPool(() => Activator.CreateInstance<T>()).Rent();
        }

        public void Return<T>(T obj) where T : class
        {
            GetPool(() => Activator.CreateInstance<T>()).Return(obj);
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pools.Clear();
            }
        }

        public void Clear<T>() where T : class
        {
            lock (_lock)
            {
                _pools.Remove(typeof(T));
            }
        }

        public int GetPoolCount()
        {
            lock (_lock)
            {
                return _pools.Count;
            }
        }

        public Dictionary<Type, int> GetPoolStats()
        {
            lock (_lock)
            {
                var stats = new Dictionary<Type, int>();
                foreach (var kvp in _pools)
                {
                    var poolType = typeof(ObjectPool<>).MakeGenericType(kvp.Key);
                    var countProp = poolType.GetProperty("AvailableCount");
                    stats[kvp.Key] = (int)(countProp?.GetValue(kvp.Value) ?? 0);
                }
                return stats;
            }
        }
    }

    public class GameTimer : IDisposable
    {
        private readonly Action _callback;
        private readonly float _interval;
        private readonly bool _isRepeating;
        private readonly object _lock = new object();
        private float _elapsed;
        private bool _isRunning;
        private bool _isDisposed;

        public float Interval => _interval;
        public float Elapsed => _elapsed;
        public bool IsRunning => _isRunning;
        public float Progress => _interval > 0 ? _elapsed / _interval : 0;

        public event Action? TimerTick;
        public event Action? TimerCompleted;

        public GameTimer(float interval, Action callback, bool isRepeating = false)
        {
            _interval = interval;
            _callback = callback;
            _isRepeating = isRepeating;
            _elapsed = 0;
            _isRunning = false;
            _isDisposed = false;
        }

        public void Start()
        {
            lock (_lock)
            {
                if (!_isDisposed)
                {
                    _isRunning = true;
                }
            }
        }

        public void Stop()
        {
            lock (_lock)
            {
                _isRunning = false;
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _elapsed = 0;
            }
        }

        public void Pause()
        {
            Stop();
        }

        public void Resume()
        {
            Start();
        }

        public void Update(float deltaTime)
        {
            if (!_isRunning || _isDisposed) return;

            lock (_lock)
            {
                _elapsed += deltaTime;

                if (_elapsed >= _interval)
                {
                    if (_isRepeating)
                    {
                        while (_elapsed >= _interval)
                        {
                            _elapsed -= _interval;
                            _callback();
                            TimerTick?.Invoke();
                        }
                    }
                    else
                    {
                        _callback();
                        TimerTick?.Invoke();
                        _isRunning = false;
                        TimerCompleted?.Invoke();
                    }
                }
            }
        }

        public void Dispose()
        {
            lock (_lock)
            {
                _isDisposed = true;
                _isRunning = false;
            }
        }
    }

    public class TimerManager
    {
        private static TimerManager? _instance;
        public static TimerManager Instance => _instance ??= new TimerManager();

        private readonly List<GameTimer> _timers;
        private readonly object _lock = new object();

        public int TimerCount
        {
            get
            {
                lock (_lock)
                {
                    return _timers.Count;
                }
            }
        }

        private TimerManager()
        {
            _timers = new List<GameTimer>();
        }

        public GameTimer CreateTimer(float interval, Action callback, bool isRepeating = false)
        {
            var timer = new GameTimer(interval, callback, isRepeating);
            lock (_lock)
            {
                _timers.Add(timer);
            }
            return timer;
        }

        public void RemoveTimer(GameTimer timer)
        {
            lock (_lock)
            {
                timer.Dispose();
                _timers.Remove(timer);
            }
        }

        public void Update(float deltaTime)
        {
            List<GameTimer> toRemove = null!;

            lock (_lock)
            {
                foreach (var timer in _timers)
                {
                    timer.Update(deltaTime);
                }

                toRemove = _timers.Where(t => !t.IsRunning).ToList();
                foreach (var timer in toRemove)
                {
                    timer.Dispose();
                    _timers.Remove(timer);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                foreach (var timer in _timers)
                {
                    timer.Dispose();
                }
                _timers.Clear();
            }
        }

        public List<GameTimer> GetActiveTimers()
        {
            lock (_lock)
            {
                return _timers.Where(t => t.IsRunning).ToList();
            }
        }
    }

    public class Cooldown
    {
        private float _currentTime;
        private float _maxTime;
        private bool _isActive;
        private bool _isPaused;

        public float CurrentTime => _currentTime;
        public float MaxTime => _maxTime;
        public float Progress => _maxTime > 0 ? _currentTime / _maxTime : 0;
        public float RemainingTime => _maxTime - _currentTime;
        public bool IsReady => _currentTime >= _maxTime;
        public bool IsActive => _isActive;
        public bool IsPaused => _isPaused;

        public Cooldown(float duration)
        {
            _maxTime = duration;
            _currentTime = duration;
            _isActive = false;
            _isPaused = false;
        }

        public void Start()
        {
            _isActive = true;
            _isPaused = false;
        }

        public void Stop()
        {
            _isActive = false;
            _currentTime = 0;
        }

        public void Reset()
        {
            _currentTime = 0;
        }

        public void SetDuration(float duration)
        {
            _maxTime = duration;
        }

        public void Pause()
        {
            _isPaused = true;
        }

        public void Resume()
        {
            _isPaused = false;
        }

        public bool TryUse()
        {
            if (IsReady && _isActive)
            {
                _currentTime = 0;
                return true;
            }
            return false;
        }

        public void Update(float deltaTime)
        {
            if (!_isActive || _isPaused || IsReady) return;

            _currentTime += deltaTime;
            if (_currentTime > _maxTime)
            {
                _currentTime = _maxTime;
            }
        }
    }

    public class StopwatchWrapper
    {
        private readonly System.Diagnostics.Stopwatch _stopwatch;
        private readonly List<long> _lapTimes;

        public TimeSpan Elapsed => _stopwatch.Elapsed;
        public long ElapsedMilliseconds => _stopwatch.ElapsedMilliseconds;
        public long ElapsedTicks => _stopwatch.ElapsedTicks;
        public bool IsRunning => _stopwatch.IsRunning;

        public StopwatchWrapper()
        {
            _stopwatch = new System.Diagnostics.Stopwatch();
            _lapTimes = new List<long>();
        }

        public void Start()
        {
            _stopwatch.Start();
        }

        public void Stop()
        {
            _stopwatch.Stop();
        }

        public void Reset()
        {
            _stopwatch.Reset();
            _lapTimes.Clear();
        }

        public void Restart()
        {
            _stopwatch.Restart();
            _lapTimes.Clear();
        }

        public long Lap()
        {
            var elapsed = _stopwatch.ElapsedTicks;
            _lapTimes.Add(elapsed);
            return elapsed;
        }

        public TimeSpan GetLapTime(int lapIndex)
        {
            if (lapIndex < 0 || lapIndex >= _lapTimes.Count) return TimeSpan.Zero;
            return TimeSpan.FromTicks(_lapTimes[lapIndex]);
        }

        public TimeSpan GetLapDuration(int lapIndex)
        {
            if (lapIndex < 0 || lapIndex >= _lapTimes.Count) return TimeSpan.Zero;
            var current = _lapTimes[lapIndex];
            var previous = lapIndex > 0 ? _lapTimes[lapIndex - 1] : 0;
            return TimeSpan.FromTicks(current - previous);
        }

        public IReadOnlyList<long> GetLapTimes()
        {
            return _lapTimes;
        }

        public void ClearLapTimes()
        {
            _lapTimes.Clear();
        }
    }

    public class CircularBuffer<T>
    {
        private readonly T[] _buffer;
        private readonly int _capacity;
        private int _head;
        private int _tail;
        private int _count;

        public int Count => _count;
        public int Capacity => _capacity;
        public bool IsEmpty => _count == 0;
        public bool IsFull => _count == _capacity;

        public CircularBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentException("Capacity must be positive", nameof(capacity));

            _capacity = capacity;
            _buffer = new T[capacity];
            _head = 0;
            _tail = 0;
            _count = 0;
        }

        public void Enqueue(T item)
        {
            _buffer[_tail] = item;
            _tail = (_tail + 1) % _capacity;

            if (_count < _capacity)
            {
                _count++;
            }
            else
            {
                _head = (_head + 1) % _capacity;
            }
        }

        public T Dequeue()
        {
            if (_count == 0)
                throw new InvalidOperationException("Buffer is empty");

            T item = _buffer[_head];
            _buffer[_head] = default!;
            _head = (_head + 1) % _capacity;
            _count--;
            return item;
        }

        public bool TryDequeue(out T? item)
        {
            if (_count == 0)
            {
                item = default;
                return false;
            }
            item = Dequeue();
            return true;
        }

        public T Peek()
        {
            if (_count == 0)
                throw new InvalidOperationException("Buffer is empty");
            return _buffer[_head];
        }

        public bool TryPeek(out T? item)
        {
            if (_count == 0)
            {
                item = default;
                return false;
            }
            item = Peek();
            return true;
        }

        public void Clear()
        {
            _head = 0;
            _tail = 0;
            _count = 0;
            Array.Clear(_buffer, 0, _capacity);
        }

        public T[] ToArray()
        {
            var result = new T[_count];
            for (int i = 0; i < _count; i++)
            {
                result[i] = _buffer[(_head + i) % _capacity];
            }
            return result;
        }

        public T this[int index]
        {
            get
            {
                if (index < 0 || index >= _count)
                    throw new IndexOutOfRangeException();
                return _buffer[(_head + index) % _capacity];
            }
        }
    }

    public class BidirectionalDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, TValue> _forward;
        private readonly Dictionary<TValue, TKey> _reverse;
        private readonly object _lock = new object();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _forward.Count;
                }
            }
        }

        public BidirectionalDictionary()
        {
            _forward = new Dictionary<TKey, TValue>();
            _reverse = new Dictionary<TValue, TKey>();
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                _forward[key] = value;
                _reverse[value] = key;
            }
        }

        public bool TryGetForward(TKey key, out TValue? value)
        {
            lock (_lock)
            {
                return _forward.TryGetValue(key, out value);
            }
        }

        public bool TryGetReverse(TValue value, out TKey? key)
        {
            lock (_lock)
            {
                return _reverse.TryGetValue(value, out key);
            }
        }

        public TValue? GetForward(TKey key)
        {
            TryGetForward(key, out var value);
            return value;
        }

        public TKey? GetReverse(TValue value)
        {
            TryGetReverse(value, out var key);
            return key;
        }

        public bool RemoveByKey(TKey key)
        {
            lock (_lock)
            {
                if (_forward.TryGetValue(key, out var value))
                {
                    _forward.Remove(key);
                    _reverse.Remove(value);
                    return true;
                }
                return false;
            }
        }

        public bool RemoveByValue(TValue value)
        {
            lock (_lock)
            {
                if (_reverse.TryGetValue(value, out var key))
                {
                    _reverse.Remove(value);
                    _forward.Remove(key);
                    return true;
                }
                return false;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _forward.Clear();
                _reverse.Clear();
            }
        }

        public IReadOnlyDictionary<TKey, TValue> Forward => _forward;
        public IReadOnlyDictionary<TValue, TKey> Reverse => _reverse;
    }

    public class MultiDictionary<TKey, TValue>
    {
        private readonly Dictionary<TKey, HashSet<TValue>> _dictionary;
        private readonly object _lock = new object();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary.Count;
                }
            }
        }

        public int TotalValues
        {
            get
            {
                lock (_lock)
                {
                    return _dictionary.Values.Sum(s => s.Count);
                }
            }
        }

        public MultiDictionary()
        {
            _dictionary = new Dictionary<TKey, HashSet<TValue>>();
        }

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (!_dictionary.TryGetValue(key, out var set))
                {
                    set = new HashSet<TValue>();
                    _dictionary[key] = set;
                }
                set.Add(value);
            }
        }

        public bool Remove(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (_dictionary.TryGetValue(key, out var set))
                {
                    if (set.Remove(value))
                    {
                        if (set.Count == 0)
                        {
                            _dictionary.Remove(key);
                        }
                        return true;
                    }
                }
                return false;
            }
        }

        public bool RemoveAll(TKey key)
        {
            lock (_lock)
            {
                return _dictionary.Remove(key);
            }
        }

        public bool Contains(TKey key, TValue value)
        {
            lock (_lock)
            {
                return _dictionary.TryGetValue(key, out var set) && set.Contains(value);
            }
        }

        public bool ContainsKey(TKey key)
        {
            lock (_lock)
            {
                return _dictionary.ContainsKey(key);
            }
        }

        public HashSet<TValue>? GetValues(TKey key)
        {
            lock (_lock)
            {
                if (_dictionary.TryGetValue(key, out var set))
                {
                    return new HashSet<TValue>(set);
                }
                return null;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _dictionary.Clear();
            }
        }
    }

    public class ReadOnlyList<T> : IReadOnlyList<T>
    {
        private readonly List<T> _innerList;

        public int Count => _innerList.Count;
        public T this[int index] => _innerList[index];

        public ReadOnlyList(IEnumerable<T> collection)
        {
            _innerList = new List<T>(collection);
        }

        public ReadOnlyList(int capacity)
        {
            _innerList = new List<T>(capacity);
        }

        public IEnumerator<T> GetEnumerator() => _innerList.GetEnumerator();
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public class PooledList<T> : IDisposable
    {
        private readonly List<T> _innerList;
        private bool _isDisposed;

        public int Count => _innerList.Count;
        public int Capacity => _innerList.Capacity;

        public PooledList()
        {
            _innerList = new List<T>();
            _isDisposed = false;
        }

        public PooledList(int capacity)
        {
            _innerList = new List<T>(capacity);
            _isDisposed = false;
        }

        public void Add(T item)
        {
            _innerList.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            _innerList.AddRange(items);
        }

        public void Clear()
        {
            _innerList.Clear();
        }

        public T this[int index]
        {
            get => _innerList[index];
            set => _innerList[index] = value;
        }

        public List<T> ToList()
        {
            return new List<T>(_innerList);
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _innerList.Clear();
                _isDisposed = true;
            }
        }
    }

    public class SparseSet<T> where T : class
    {
        private readonly Dictionary<int, T> _sparse;
        private readonly List<int> _dense;
        private readonly Dictionary<T, int> _reverseDense;
        private readonly object _lock = new object();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _dense.Count;
                }
            }
        }

        public SparseSet()
        {
            _sparse = new Dictionary<int, T>();
            _dense = new List<int>();
            _reverseDense = new Dictionary<T, int>();
        }

        public void Add(int id, T value)
        {
            lock (_lock)
            {
                if (Contains(id)) return;

                _sparse[id] = value;
                _dense.Add(id);
                _reverseDense[value] = id;
            }
        }

        public bool Remove(int id)
        {
            lock (_lock)
            {
                if (!_sparse.TryGetValue(id, out var value))
                    return false;

                _sparse.Remove(id);
                _reverseDense.Remove(value);

                int index = _dense.IndexOf(id);
                if (index >= 0)
                {
                    _dense.RemoveAt(index);
                }

                return true;
            }
        }

        public bool Contains(int id)
        {
            lock (_lock)
            {
                return _sparse.ContainsKey(id);
            }
        }

        public bool TryGet(int id, out T? value)
        {
            lock (_lock)
            {
                return _sparse.TryGetValue(id, out value);
            }
        }

        public T? Get(int id)
        {
            TryGet(id, out var value);
            return value;
        }

        public void Clear()
        {
            lock (_lock)
            {
                _sparse.Clear();
                _dense.Clear();
                _reverseDense.Clear();
            }
        }

        public int? GetId(T value)
        {
            lock (_lock)
            {
                return _reverseDense.TryGetValue(value, out var id) ? id : null;
            }
        }

        public IEnumerable<int> GetIds()
        {
            lock (_lock)
            {
                return _dense.ToList();
            }
        }

        public IEnumerable<T> GetValues()
        {
            lock (_lock)
            {
                return _sparse.Values.ToList();
            }
        }
    }

    public class ThreadPoolHelper
    {
        private static readonly object _lock = new object();
        private static int _maxWorkerThreads;
        private static int _maxIOThreads;

        static ThreadPoolHelper()
        {
            UpdateLimits();
        }

        public static void UpdateLimits()
        {
            ThreadPool.GetMaxThreads(out _maxWorkerThreads, out _maxIOThreads);
        }

        public static bool QueueWorkItem(Action callback)
        {
            return ThreadPool.QueueUserWorkItem(_ =>
            {
                callback();
            });
        }

        public static bool QueueWorkItem<T>(Action<T> callback, T state)
        {
            return ThreadPool.QueueUserWorkItem(_ =>
            {
                callback(state);
            });
        }

        public static void QueueWorkItemAndWait(Action callback)
        {
            using var evt = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(_ =>
            {
                callback();
                evt.Set();
            });
            evt.WaitOne();
        }

        public static int GetAvailableWorkerThreads()
        {
            ThreadPool.GetAvailableThreads(out int workers, out _);
            return workers;
        }

        public static int GetAvailableIOThreads()
        {
            ThreadPool.GetAvailableThreads(out _, out int io);
            return io;
        }

        public static void SetMinThreads(int workerThreads, int ioThreads)
        {
            ThreadPool.SetMinThreads(workerThreads, ioThreads);
        }

        public static void SetMaxThreads(int workerThreads, int ioThreads)
        {
            ThreadPool.SetMaxThreads(workerThreads, ioThreads);
        }
    }

    public class TaskRunner
    {
        private readonly Queue<Func<Task>> _taskQueue;
        private readonly List<Task> _runningTasks;
        private readonly int _maxConcurrency;
        private readonly object _lock = new object();
        private readonly CancellationTokenSource _cts;
        private bool _isRunning;

        public int MaxConcurrency => _maxConcurrency;
        public int RunningTaskCount => _runningTasks.Count;
        public int QueuedTaskCount => _taskQueue.Count;
        public bool IsRunning => _isRunning;

        public TaskRunner(int maxConcurrency = 4)
        {
            _maxConcurrency = maxConcurrency;
            _taskQueue = new Queue<Func<Task>>();
            _runningTasks = new List<Task>();
            _cts = new CancellationTokenSource();
            _isRunning = false;
        }

        public void Start()
        {
            _isRunning = true;
            ProcessQueue();
        }

        public void Stop()
        {
            _isRunning = false;
            _cts.Cancel();
        }

        public void Enqueue(Func<Task> taskFactory)
        {
            lock (_lock)
            {
                _taskQueue.Enqueue(taskFactory);
            }

            if (_isRunning)
            {
                ProcessQueue();
            }
        }

        public void Enqueue(Action action)
        {
            Enqueue(() =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        private void ProcessQueue()
        {
            while (_isRunning)
            {
                Func<Task>? taskFactory = null;

                lock (_lock)
                {
                    if (_runningTasks.Count < _maxConcurrency && _taskQueue.Count > 0)
                    {
                        taskFactory = _taskQueue.Dequeue();
                    }
                }

                if (taskFactory != null)
                {
                    var task = taskFactory();
                    lock (_lock)
                    {
                        _runningTasks.Add(task);
                    }

                    task.ContinueWith(t =>
                    {
                        lock (_lock)
                        {
                            _runningTasks.Remove(t);
                        }
                        ProcessQueue();
                    }, _cts.Token);
                }
                else
                {
                    break;
                }
            }
        }

        public async Task WaitForAllAsync()
        {
            List<Task> tasks;
            lock (_lock)
            {
                tasks = new List<Task>(_runningTasks);
            }

            await Task.WhenAll(tasks);
        }

        public void Clear()
        {
            lock (_lock)
            {
                _taskQueue.Clear();
            }
        }
    }

    public class AsyncOperation<T>
    {
        private readonly Task<T> _task;
        private readonly DateTime _startTime;
        private DateTime? _endTime;

        public Task<T> Task => _task;
        public bool IsCompleted => _task.IsCompleted;
        public bool IsCanceled => _task.IsCanceled;
        public bool IsFaulted => _task.IsFaulted;
        public TimeSpan ElapsedTime => (_endTime ?? DateTime.Now) - _startTime;
        public T? Result => _task.IsCompletedSuccessfully ? _task.Result : default;
        public Exception? Exception => _task.Exception;

        public AsyncOperation(Task<T> task)
        {
            _task = task;
            _startTime = DateTime.Now;

            _task.ContinueWith(_ =>
            {
                _endTime = DateTime.Now;
            });
        }

        public async Task<T> WaitAsync()
        {
            var result = await _task;
            _endTime ??= DateTime.Now;
            return result;
        }

        public void Cancel()
        {
            if (_task is ICancelable cancelable)
            {
            }
        }
    }

    public interface ICancelable
    {
        void Cancel();
    }

    public class ThreadSafeQueue<T>
    {
        private readonly Queue<T> _queue;
        private readonly object _lock = new object();

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _queue.Count;
                }
            }
        }

        public ThreadSafeQueue()
        {
            _queue = new Queue<T>();
        }

        public void Enqueue(T item)
        {
            lock (_lock)
            {
                _queue.Enqueue(item);
            }
        }

        public bool TryDequeue(out T? item)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    item = _queue.Dequeue();
                    return true;
                }
            }
            item = default;
            return false;
        }

        public bool TryPeek(out T? item)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    item = _queue.Peek();
                    return true;
                }
            }
            item = default;
            return false;
        }

        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }
    }

    public class ThreadSafeDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _dictionary;
        private readonly ReaderWriterLockSlim _lock;

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _dictionary.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public ThreadSafeDictionary()
        {
            _dictionary = new Dictionary<TKey, TValue>();
            _lock = new ReaderWriterLockSlim();
        }

        public void Add(TKey key, TValue value)
        {
            _lock.EnterWriteLock();
            try
            {
                _dictionary[key] = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGet(TKey key, out TValue? value)
        {
            _lock.EnterReadLock();
            try
            {
                return _dictionary.TryGetValue(key, out value);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public bool Remove(TKey key)
        {
            _lock.EnterWriteLock();
            try
            {
                return _dictionary.Remove(key);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _dictionary.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<TKey> Keys
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _dictionary.Keys.ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public IEnumerable<TValue> Values
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _dictionary.Values.ToList();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }
    }

    public static class Easing
    {
        public enum Type
        {
            Linear,
            QuadIn, QuadOut, QuadInOut,
            CubicIn, CubicOut, CubicInOut,
            QuartIn, QuartOut, QuartInOut,
            QuintIn, QuintOut, QuintInOut,
            SineIn, SineOut, SineInOut,
            ExpoIn, ExpoOut, ExpoInOut,
            CircIn, CircOut, CircInOut,
            ElasticIn, ElasticOut, ElasticInOut,
            BackIn, BackOut, BackInOut,
            BounceIn, BounceOut, BounceInOut
        }

        public static float Evaluate(float t, Type type)
        {
            return type switch
            {
                Type.Linear => t,
                Type.QuadIn => QuadIn(t),
                Type.QuadOut => QuadOut(t),
                Type.QuadInOut => QuadInOut(t),
                Type.CubicIn => CubicIn(t),
                Type.CubicOut => CubicOut(t),
                Type.CubicInOut => CubicInOut(t),
                Type.QuartIn => QuartIn(t),
                Type.QuartOut => QuartOut(t),
                Type.QuartInOut => QuartInOut(t),
                Type.QuintIn => QuintIn(t),
                Type.QuintOut => QuintOut(t),
                Type.QuintInOut => QuintInOut(t),
                Type.SineIn => SineIn(t),
                Type.SineOut => SineOut(t),
                Type.SineInOut => SineInOut(t),
                Type.ExpoIn => ExpoIn(t),
                Type.ExpoOut => ExpoOut(t),
                Type.ExpoInOut => ExpoInOut(t),
                Type.CircIn => CircIn(t),
                Type.CircOut => CircOut(t),
                Type.CircInOut => CircInOut(t),
                Type.ElasticIn => ElasticIn(t),
                Type.ElasticOut => ElasticOut(t),
                Type.ElasticInOut => ElasticInOut(t),
                Type.BackIn => BackIn(t),
                Type.BackOut => BackOut(t),
                Type.BackInOut => BackInOut(t),
                Type.BounceIn => BounceIn(t),
                Type.BounceOut => BounceOut(t),
                Type.BounceInOut => BounceInOut(t),
                _ => t
            };
        }

        public static float QuadIn(float t) => t * t;
        public static float QuadOut(float t) => t * (2 - t);
        public static float QuadInOut(float t) => t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;

        public static float CubicIn(float t) => t * t * t;
        public static float CubicOut(float t) => (--t) * t * t + 1;
        public static float CubicInOut(float t) => t < 0.5f ? 4 * t * t * t : (t - 1) * (2 * t - 2) * (2 * t - 2) + 1;

        public static float QuartIn(float t) => t * t * t * t;
        public static float QuartOut(float t) => 1 - (--t) * t * t * t;
        public static float QuartInOut(float t) => t < 0.5f ? 8 * t * t * t * t : 1 - 8 * (--t) * t * t * t;

        public static float QuintIn(float t) => t * t * t * t * t;
        public static float QuintOut(float t) => 1 + (--t) * t * t * t * t;
        public static float QuintInOut(float t) => t < 0.5f ? 16 * t * t * t * t * t : 1 + 16 * (--t) * t * t * t * t;

        public static float SineIn(float t) => 1 - MathF.Cos(t * MathF.PI / 2);
        public static float SineOut(float t) => MathF.Sin(t * MathF.PI / 2);
        public static float SineInOut(float t) => -(MathF.Cos(MathF.PI * t) - 1) / 2;

        public static float ExpoIn(float t) => t == 0 ? 0 : MathF.Pow(2, 10 * (t - 1));
        public static float ExpoOut(float t) => t == 1 ? 1 : 1 - MathF.Pow(2, -10 * t);
        public static float ExpoInOut(float t) =>
            t == 0 ? 0 : t == 1 ? 1 : t < 0.5f
                ? MathF.Pow(2, 20 * t - 10) / 2
                : (2 - MathF.Pow(2, -20 * t + 10)) / 2;

        public static float CircIn(float t) => 1 - MathF.Sqrt(1 - t * t);
        public static float CircOut(float t) => MathF.Sqrt(1 - (--t) * t);
        public static float CircInOut(float t) =>
            t < 0.5f ? (1 - MathF.Sqrt(1 - 4 * t * t)) / 2
                     : (MathF.Sqrt(1 - 4 * (t - 1) * (t - 1)) + 1) / 2;

        public static float ElasticIn(float t) =>
            t == 0 ? 0 : t == 1 ? 1
            : -MathF.Pow(2, 10 * t - 10) * MathF.Sin((t * 10 - 10.75f) * (2 * MathF.PI / 3));

        public static float ElasticOut(float t) =>
            t == 0 ? 0 : t == 1 ? 1
            : MathF.Pow(2, -10 * t) * MathF.Sin((t * 10 - 0.75f) * (2 * MathF.PI / 3)) + 1;

        public static float ElasticInOut(float t) =>
            t == 0 ? 0 : t == 1 ? 1 : t < 0.5f
                ? -(MathF.Pow(2, 20 * t - 10) * MathF.Sin((20 * t - 11.125f) * (2 * MathF.PI / 4.5f))) / 2
                : (MathF.Pow(2, -20 * t + 10) * MathF.Sin((20 * t - 11.125f) * (2 * MathF.PI / 4.5f))) / 2 + 1;

        public static float BackIn(float t) => (1.70158f + 1) * t * t * t - 1.70158f * t * t;

        public static float BackOut(float t) =>
            1 + (1.70158f + 1) * (--t) * t * t + 1.70158f * t * t;

        public static float BackInOut(float t) =>
            t < 0.5f
                ? (2 * t) * (2 * t) * ((2.5949095f + 1) * 2 * t - 2.5949095f) / 2
                : ((2 * t - 2) * (2 * t - 2) * ((2.5949095f + 1) * (2 * t - 2) + 2.5949095f) + 2) / 2;

        public static float BounceIn(float t) => 1 - BounceOut(1 - t);

        public static float BounceOut(float t) =>
            t < 1 / 2.75f ? 7.5625f * t * t
            : t < 2 / 2.75f ? 7.5625f * (t -= 1.5f / 2.75f) * t + 0.75f
            : t < 2.5f / 2.75f ? 7.5625f * (t -= 2.25f / 2.75f) * t + 0.9375f
            : 7.5625f * (t -= 2.625f / 2.75f) * t + 0.984375f;

        public static float BounceInOut(float t) =>
            t < 0.5f ? (1 - BounceOut(1 - 2 * t)) / 2 : (1 + BounceOut(2 * t - 1)) / 2;
    }

    public static class Lerp
    {
        public static float Float(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        public static Vector2 Vector2(Vector2 a, Vector2 b, float t)
        {
            return a.Lerp(b, t);
        }

        public static Vector3 Vector3(Vector3 a, Vector3 b, float t)
        {
            return a.Lerp(b, t);
        }

        public static Color Color(Color a, Color b, float t)
        {
            return new Color(
                Lerp.Float(a.R, b.R, t),
                Lerp.Float(a.G, b.G, t),
                Lerp.Float(a.B, b.B, t),
                Lerp.Float(a.A, b.A, t)
            );
        }

        public static float InverseLerp(float a, float b, float value)
        {
            if (Math.Abs(a - b) < 0.0001f)
                return 0;
            return Math.Clamp((value - a) / (b - a), 0, 1);
        }

        public static Vector2 InverseLerp(Vector2 a, Vector2 b, Vector2 value)
        {
            return new Vector2(
                InverseLerp(a.X, b.X, value.X),
                InverseLerp(a.Y, b.Y, value.Y)
            );
        }

        public static float Remap(float value, float inMin, float inMax, float outMin, float outMax)
        {
            float t = InverseLerp(inMin, inMax, value);
            return Lerp.Float(outMin, outMax, t);
        }

        public static Vector2 Remap(Vector2 value, Vector2 inMin, Vector2 inMax, Vector2 outMin, Vector2 outMax)
        {
            return new Vector2(
                Remap(value.X, inMin.X, inMax.X, outMin.X, outMax.X),
                Remap(value.Y, inMin.Y, inMax.Y, outMin.Y, outMax.Y)
            );
        }
    }

    public static class Angle
    {
        public const float Deg2Rad = MathF.PI / 180f;
        public const float Rad2Deg = 180f / MathF.PI;

        public static float ToRadians(float degrees) => degrees * Deg2Rad;
        public static float ToDegrees(float radians) => radians * Rad2Deg;

        public static Vector2 FromDegrees(float degrees)
        {
            float radians = ToRadians(degrees);
            return new Vector2(MathF.Cos(radians), MathF.Sin(radians));
        }

        public static Vector2 FromRadians(float radians)
        {
            return new Vector2(MathF.Cos(radians), MathF.Sin(radians));
        }

        public static float GetAngle(Vector2 direction)
        {
            return MathF.Atan2(direction.Y, direction.X);
        }

        public static float GetAngleDegrees(Vector2 direction)
        {
            return ToDegrees(GetAngle(direction));
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle < 0) angle += 360f;
            while (angle >= 360f) angle -= 360f;
            return angle;
        }

        public static float NormalizeAngleRadians(float angle)
        {
            while (angle < 0) angle += MathF.PI * 2;
            while (angle >= MathF.PI * 2) angle -= MathF.PI * 2;
            return angle;
        }

        public static float AngleDifference(float angle1, float angle2)
        {
            float diff = NormalizeAngle(angle2 - angle1);
            if (diff > 180f) diff -= 360f;
            return diff;
        }

        public static float AngleDifferenceRadians(float angle1, float angle2)
        {
            float diff = NormalizeAngleRadians(angle2 - angle1);
            if (diff > MathF.PI) diff -= MathF.PI * 2;
            return diff;
        }

        public static float LerpAngle(float a, float b, float t)
        {
            float diff = AngleDifference(a, b);
            return a + diff * t;
        }

        public static float LerpAngleRadians(float a, float b, float t)
        {
            float diff = AngleDifferenceRadians(a, b);
            return a + diff * t;
        }

        public static bool IsAngleInRange(float angle, float min, float max)
        {
            angle = NormalizeAngle(angle);
            min = NormalizeAngle(min);
            max = NormalizeAngle(max);

            if (min <= max)
            {
                return angle >= min && angle <= max;
            }
            else
            {
                return angle >= min || angle <= max;
            }
        }

        public static Vector2 RotateVector(Vector2 vector, float angleRadians)
        {
            float cos = MathF.Cos(angleRadians);
            float sin = MathF.Sin(angleRadians);
            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }
    }

    public static class Interpolation
    {
        public static float SmoothStep(float from, float to, float t)
        {
            t = Math.Clamp(t, 0, 1);
            t = t * t * (3 - 2 * t);
            return from + (to - from) * t;
        }

        public static Vector2 SmoothStep(Vector2 from, Vector2 to, float t)
        {
            return new Vector2(
                SmoothStep(from.X, to.X, t),
                SmoothStep(from.Y, to.Y, t)
            );
        }

        public static float SmootherStep(float from, float to, float t)
        {
            t = Math.Clamp(t, 0, 1);
            t = t * t * t * (t * (t * 6 - 15) + 10);
            return from + (to - from) * t;
        }

        public static float Spring(float t)
        {
            return 1 - MathF.Cos(t * MathF.PI * 4) * MathF.Exp(-t * 8);
        }

        public static float Parabolic(float t)
        {
            return 4 * t * (1 - t);
        }

        public static float DampedWave(float t)
        {
            return MathF.Sin(t * MathF.PI * 3) * MathF.Pow(2, -10 * t);
        }

        public static float DoubleExponential(float t)
        {
            if (t < 0.5f)
            {
                return MathF.Pow(2, 20 * t - 10) / 2;
            }
            return (2 - MathF.Pow(2, -20 * t + 10)) / 2;
        }

        public static float DoubleExponentialFade(float t)
        {
            if (t < 0.5f)
            {
                return MathF.Pow(2, 10 * t - 10);
            }
            return 1 - MathF.Pow(2, -10 * t + 10);
        }

        public static Vector2 Bezier(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;
            float mt = 1 - t;
            float mt2 = mt * mt;
            float mt3 = mt2 * mt;

            return mt3 * p0 + 3 * mt2 * t * p1 + 3 * mt * t2 * p2 + t3 * p3;
        }

        public static float CatmullRom(float p0, float p1, float p2, float p3, float t)
        {
            float t2 = t * t;
            float t3 = t2 * t;

            return 0.5f * (
                (2 * p1) +
                (-p0 + p2) * t +
                (2 * p0 - 5 * p1 + 4 * p2 - p3) * t2 +
                (-p0 + 3 * p1 - 3 * p2 + p3) * t3
            );
        }

        public static Vector2 CatmullRom(Vector2 p0, Vector2 p1, Vector2 p2, Vector2 p3, float t)
        {
            return new Vector2(
                CatmullRom(p0.X, p1.X, p2.X, p3.X, t),
                CatmullRom(p0.Y, p1.Y, p2.Y, p3.Y, t)
            );
        }
    }

    public static class Validation
    {
        public static bool IsInRange(float value, float min, float max)
        {
            return value >= min && value <= max;
        }

        public static bool IsInRange(int value, int min, int max)
        {
            return value >= min && value <= max;
        }

        public static bool IsInRange(Vector2 value, Vector2 min, Vector2 max)
        {
            return IsInRange(value.X, min.X, max.X) && IsInRange(value.Y, min.Y, max.Y);
        }

        public static bool IsApproximatelyEqual(float a, float b, float epsilon = 0.0001f)
        {
            return Math.Abs(a - b) < epsilon;
        }

        public static bool IsApproximatelyZero(float value, float epsilon = 0.0001f)
        {
            return Math.Abs(value) < epsilon;
        }

        public static void RequireNonNull(object? obj, string paramName = "value")
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }

        public static void RequireNonNull<T>(T? obj, string paramName = "value") where T : class
        {
            if (obj == null)
                throw new ArgumentNullException(paramName);
        }

        public static void RequireInRange(float value, float min, float max, string paramName = "value")
        {
            if (!IsInRange(value, min, max))
                throw new ArgumentOutOfRangeException(paramName, $"Value {value} must be between {min} and {max}");
        }

        public static void RequireInRange(int value, int min, int max, string paramName = "value")
        {
            if (!IsInRange(value, min, max))
                throw new ArgumentOutOfRangeException(paramName, $"Value {value} must be between {min} and {max}");
        }

        public static void RequirePositive(float value, string paramName = "value")
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, $"Value {value} must be positive");
        }

        public static void RequirePositive(int value, string paramName = "value")
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(paramName, $"Value {value} must be positive");
        }

        public static void RequireNonNegative(float value, string paramName = "value")
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, $"Value {value} must be non-negative");
        }

        public static void RequireNonNegative(int value, string paramName = "value")
        {
            if (value < 0)
                throw new ArgumentOutOfRangeException(paramName, $"Value {value} must be non-negative");
        }

        public static T EnsureNotNull<T>(T? obj, T defaultValue) where T : class
        {
            return obj ?? defaultValue;
        }

        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static int Clamp(int value, int min, int max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static float Clamp01(float value)
        {
            return Clamp(value, 0, 1);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace OfficeSuite.Common.Infrastructure
{
    public class CommandManager
    {
        private readonly Stack<ICommand> _undoStack;
        private readonly Stack<ICommand> _redoStack;
        private readonly Dictionary<string, ICommand> _registeredCommands;
        private readonly Dictionary<Type, Func<object, ICommand>> _commandFactories;
        private int _maxUndoLevels;

        public CommandManager()
        {
            _undoStack = new Stack<ICommand>();
            _redoStack = new Stack<ICommand>();
            _registeredCommands = new Dictionary<string, ICommand>();
            _commandFactories = new Dictionary<Type, Func<object, ICommand>>();
            _maxUndoLevels = 100;
        }

        public int MaxUndoLevels
        {
            get => _maxUndoLevels;
            set => _maxUndoLevels = Math.Max(1, value);
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public event EventHandler CommandExecuted;
        public event EventHandler CommandUndone;
        public event EventHandler CommandRedone;
        public event EventHandler HistoryChanged;

        public void ExecuteCommand(ICommand command, object parameter = null)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            if (command.CanExecute(parameter))
            {
                command.Execute(parameter);
                _undoStack.Push(command);
                _redoStack.Clear();

                TrimUndoStack();
                OnCommandExecuted();
                OnHistoryChanged();
            }
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var command = _undoStack.Pop();
            if (command is IUndoableCommand undoable)
            {
                undoable.Undo();
            }
            else if (command is IUndoCommand simpleUndo)
            {
                simpleUndo.Undo();
            }

            _redoStack.Push(command);
            OnCommandUndone();
            OnHistoryChanged();
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var command = _redoStack.Pop();
            if (command is IUndoableCommand undoable)
            {
                undoable.Redo();
            }
            else
            {
                command.Execute(null);
            }

            _undoStack.Push(command);
            OnCommandRedone();
            OnHistoryChanged();
        }

        public void ClearHistory()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            OnHistoryChanged();
        }

        public void RegisterCommand(string name, ICommand command)
        {
            _registeredCommands[name] = command;
        }

        public ICommand GetRegisteredCommand(string name)
        {
            return _registeredCommands.TryGetValue(name, out var command) ? command : null;
        }

        public void RegisterCommandFactory(Type commandType, Func<object, ICommand> factory)
        {
            _commandFactories[commandType] = factory;
        }

        public ICommand CreateCommand(Type commandType, object parameter)
        {
            if (_commandFactories.TryGetValue(commandType, out var factory))
            {
                return factory(parameter);
            }
            return null;
        }

        public IEnumerable<ICommand> GetUndoHistory()
        {
            return _undoStack.ToArray().Reverse();
        }

        public IEnumerable<ICommand> GetRedoHistory()
        {
            return _redoStack.ToArray().Reverse();
        }

        private void TrimUndoStack()
        {
            while (_undoStack.Count > _maxUndoLevels)
            {
                var items = _undoStack.ToArray();
                _undoStack.Clear();
                for (int i = 0; i < items.Length - 1; i++)
                {
                    _undoStack.Push(items[items.Length - 1 - i]);
                }
            }
        }

        protected virtual void OnCommandExecuted()
        {
            CommandExecuted?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCommandUndone()
        {
            CommandUndone?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnCommandRedone()
        {
            CommandRedone?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnHistoryChanged()
        {
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public interface IUndoableCommand : ICommand
    {
        void Undo();
        void Redo();
    }

    public interface IUndoCommand : ICommand
    {
        void Undo();
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public RelayCommand(Action execute, Func<bool> canExecute = null)
            : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T> _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            if (parameter == null && typeof(T).IsValueType)
                return _canExecute == null;

            return _canExecute == null || _canExecute((T)parameter);
        }

        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class AsyncRelayCommand : ICommand
    {
        private readonly Func<object, System.Threading.Tasks.Task> _execute;
        private readonly Predicate<object> _canExecute;
        private System.Threading.Tasks.Task _executingTask;
        private bool _isExecuting;

        public AsyncRelayCommand(Func<object, System.Threading.Tasks.Task> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public AsyncRelayCommand(Func<System.Threading.Tasks.Task> execute, Func<bool> canExecute = null)
            : this(_ => execute(), canExecute != null ? _ => canExecute() : null)
        {
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute == null || _canExecute(parameter));
        }

        public async void Execute(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    RaiseCanExecuteChanged();
                    _executingTask = _execute(parameter);
                    await _executingTask;
                }
                finally
                {
                    _isExecuting = false;
                    RaiseCanExecuteChanged();
                }
            }
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }

        public bool IsExecuting => _isExecuting;
    }

    public class UndoableCommand : IUndoableCommand
    {
        private readonly Action<object> _execute;
        private readonly Action<object> _undo;
        private readonly Predicate<object> _canExecute;
        private readonly object _parameter;

        public UndoableCommand(Action<object> execute, Action<object> undo, Predicate<object> canExecute = null, object parameter = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _undo = undo ?? throw new ArgumentNullException(nameof(undo));
            _canExecute = canExecute;
            _parameter = parameter;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter ?? _parameter);
        }

        public void Undo()
        {
            _undo(_parameter);
        }

        public void Redo()
        {
            Execute(_parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }

    public class CompositeCommand : ICommand
    {
        private readonly List<ICommand> _commands;
        private readonly bool _stopOnFailure;

        public CompositeCommand(bool stopOnFailure = false)
        {
            _commands = new List<ICommand>();
            _stopOnFailure = stopOnFailure;
        }

        public IReadOnlyList<ICommand> Commands => _commands.AsReadOnly();

        public void AddCommand(ICommand command)
        {
            if (command != null)
                _commands.Add(command);
        }

        public void RemoveCommand(ICommand command)
        {
            _commands.Remove(command);
        }

        public void ClearCommands()
        {
            _commands.Clear();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _commands.All(c => c.CanExecute(parameter));
        }

        public void Execute(object parameter)
        {
            foreach (var command in _commands)
            {
                if (_stopOnFailure && !command.CanExecute(parameter))
                    break;

                if (command.CanExecute(parameter))
                    command.Execute(parameter);
            }
        }
    }

    public class UndoableCompositeCommand : CompositeCommand, IUndoableCommand
    {
        private readonly Stack<List<ICommand>> _executedCommands;

        public UndoableCompositeCommand(bool stopOnFailure = false)
            : base(stopOnFailure)
        {
            _executedCommands = new Stack<List<ICommand>>();
        }

        public new void Execute(object parameter)
        {
            var executed = new List<ICommand>();
            _executedCommands.Push(executed);

            foreach (var command in Commands)
            {
                if (StopOnFailure && !command.CanExecute(parameter))
                    break;

                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                    executed.Add(command);
                }
            }
        }

        public void Undo()
        {
            if (_executedCommands.Count == 0) return;

            var commands = _executedCommands.Pop();
            for (int i = commands.Count - 1; i >= 0; i--)
            {
                if (commands[i] is IUndoableCommand undoable)
                    undoable.Undo();
                else if (commands[i] is IUndoCommand simpleUndo)
                    simpleUndo.Undo();
            }
        }

        public void Redo()
        {
            if (_executedCommands.Count == 0) return;

            var commands = _executedCommands.Peek();
            foreach (var command in commands)
            {
                if (command is IUndoableCommand undoable)
                    undoable.Redo();
                else
                    command.Execute(null);
            }
        }

        public bool StopOnFailure => _stopOnFailure;

        public bool CanUndo => _executedCommands.Count > 0;
        public bool CanRedo => _executedCommands.Count > 0;
    }

    public class CommandHistory
    {
        private readonly List<CommandHistoryEntry> _history;
        private int _currentIndex;
        private readonly int _maxEntries;

        public CommandHistory(int maxEntries = 100)
        {
            _history = new List<CommandHistoryEntry>();
            _currentIndex = -1;
            _maxEntries = maxEntries;
        }

        public int Count => _history.Count;
        public int CurrentIndex => _currentIndex;
        public bool CanUndo => _currentIndex >= 0;
        public bool CanRedo => _currentIndex < _history.Count - 1;

        public event EventHandler HistoryChanged;

        public void AddCommand(ICommand command, string description, object parameter = null)
        {
            if (_currentIndex < _history.Count - 1)
            {
                _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
            }

            var entry = new CommandHistoryEntry
            {
                Command = command,
                Description = description,
                Parameter = parameter,
                ExecutedAt = DateTime.Now
            };

            _history.Add(entry);
            _currentIndex = _history.Count - 1;

            TrimHistory();
            OnHistoryChanged();
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var entry = _history[_currentIndex];
            if (entry.Command is IUndoableCommand undoable)
                undoable.Undo();
            else if (entry.Command is IUndoCommand simpleUndo)
                simpleUndo.Undo();

            _currentIndex--;
            OnHistoryChanged();
        }

        public void Redo()
        {
            if (!CanRedo) return;

            _currentIndex++;
            var entry = _history[_currentIndex];
            if (entry.Command is IUndoableCommand undoable)
                undoable.Redo();
            else
                entry.Command.Execute(entry.Parameter);

            OnHistoryChanged();
        }

        public CommandHistoryEntry GetEntry(int index)
        {
            return index >= 0 && index < _history.Count ? _history[index] : null;
        }

        public IEnumerable<CommandHistoryEntry> GetHistory()
        {
            return _history;
        }

        public IEnumerable<CommandHistoryEntry> GetFutureHistory()
        {
            return _history.Skip(_currentIndex + 1);
        }

        public void Clear()
        {
            _history.Clear();
            _currentIndex = -1;
            OnHistoryChanged();
        }

        private void TrimHistory()
        {
            while (_history.Count > _maxEntries)
            {
                _history.RemoveAt(0);
                _currentIndex--;
            }
        }

        protected virtual void OnHistoryChanged()
        {
            HistoryChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    public class CommandHistoryEntry
    {
        public ICommand Command { get; set; }
        public string Description { get; set; }
        public object Parameter { get; set; }
        public DateTime ExecutedAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public CommandHistoryEntry()
        {
            Metadata = new Dictionary<string, object>();
        }
    }

    public class CommandBuilder
    {
        private Action<object> _execute;
        private Predicate<object> _canExecute;
        private string _name;
        private string _description;
        private string _category;
        private string _inputGestureText;
        private object _iconSource;

        public CommandBuilder WithExecute(Action<object> execute)
        {
            _execute = execute;
            return this;
        }

        public CommandBuilder WithExecute(Action execute)
        {
            _execute = _ => execute();
            return this;
        }

        public CommandBuilder WithCanExecute(Predicate<object> canExecute)
        {
            _canExecute = canExecute;
            return this;
        }

        public CommandBuilder WithCanExecute(Func<bool> canExecute)
        {
            _canExecute = _ => canExecute();
            return this;
        }

        public CommandBuilder WithName(string name)
        {
            _name = name;
            return this;
        }

        public CommandBuilder WithDescription(string description)
        {
            _description = description;
            return this;
        }

        public CommandBuilder WithCategory(string category)
        {
            _category = category;
            return this;
        }

        public CommandBuilder WithInputGesture(string gestureText)
        {
            _inputGestureText = gestureText;
            return this;
        }

        public CommandBuilder WithIcon(object iconSource)
        {
            _iconSource = iconSource;
            return this;
        }

        public RelayCommand Build()
        {
            return new RelayCommand(_execute, _canExecute);
        }

        public string Name => _name;
        public string Description => _description;
        public string Category => _category;
        public string InputGestureText => _inputGestureText;
        public object IconSource => _iconSource;
    }

    public class UndoableCommandBuilder : CommandBuilder
    {
        private Action<object> _undo;
        private Action<object> _redo;

        public new UndoableCommandBuilder WithExecute(Action<object> execute)
        {
            base.WithExecute(execute);
            return this;
        }

        public UndoableCommandBuilder WithUndo(Action<object> undo)
        {
            _undo = undo;
            return this;
        }

        public UndoableCommandBuilder WithRedo(Action<object> redo)
        {
            _redo = redo;
            return this;
        }

        public new UndoableCommand Build()
        {
            return new UndoableCommand(
                base.Build().Execute,
                _undo ?? (_ => { }),
                base.Build().CanExecute);
        }
    }

    public class CommandAggregator : ICommand
    {
        private readonly List<ICommand> _commands;
        private readonly Predicate<object> _canExecute;

        public CommandAggregator(Predicate<object> canExecute = null)
        {
            _commands = new List<ICommand>();
            _canExecute = canExecute ?? (_ => true);
        }

        public void AddCommand(ICommand command)
        {
            if (command != null)
                _commands.Add(command);
        }

        public void RemoveCommand(ICommand command)
        {
            _commands.Remove(command);
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute(parameter) && _commands.All(c => c.CanExecute(parameter));
        }

        public void Execute(object parameter)
        {
            foreach (var command in _commands)
            {
                if (command.CanExecute(parameter))
                    command.Execute(parameter);
            }
        }
    }
}

namespace OfficeSuite.Common.Events
{
    public class EventAggregator
    {
        private readonly Dictionary<Type, List<object>> _subscribers;
        private readonly Dictionary<Type, List<WeakReference>> _weakSubscribers;
        private readonly object _lock = new object();
        private bool _useWeakReferences;

        public EventAggregator()
        {
            _subscribers = new Dictionary<Type, List<object>>();
            _weakSubscribers = new Dictionary<Type, List<WeakReference>>();
            _useWeakReferences = false;
        }

        public bool UseWeakReferences
        {
            get => _useWeakReferences;
            set => _useWeakReferences = value;
        }

        public void Subscribe<TEvent>(Action<TEvent> handler)
        {
            lock (_lock)
            {
                var eventType = typeof(TEvent);

                if (_useWeakReferences)
                {
                    if (!_weakSubscribers.ContainsKey(eventType))
                        _weakSubscribers[eventType] = new List<WeakReference>();

                    _weakSubscribers[eventType].Add(new WeakReference(handler));
                }
                else
                {
                    if (!_subscribers.ContainsKey(eventType))
                        _subscribers[eventType] = new List<object>();

                    _subscribers[eventType].Add(handler);
                }
            }
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler)
        {
            lock (_lock)
            {
                var eventType = typeof(TEvent);

                if (_useWeakReferences)
                {
                    if (_weakSubscribers.ContainsKey(eventType))
                    {
                        _weakSubscribers[eventType].RemoveAll(wr =>
                        {
                            var target = wr.Target;
                            return target == null || target.Equals(handler);
                        });
                    }
                }
                else
                {
                    if (_subscribers.ContainsKey(eventType))
                    {
                        _subscribers[eventType].Remove(handler);
                    }
                }
            }
        }

        public void Publish<TEvent>(TEvent eventData)
        {
            List<object> handlers = null;

            lock (_lock)
            {
                var eventType = typeof(TEvent);

                if (_useWeakReferences)
                {
                    if (_weakSubscribers.ContainsKey(eventType))
                    {
                        handlers = new List<object>();
                        var deadRefs = new List<WeakReference>();

                        foreach (var wr in _weakSubscribers[eventType])
                        {
                            if (wr.IsAlive)
                                handlers.Add(wr.Target);
                            else
                                deadRefs.Add(wr);
                        }

                        foreach (var deadRef in deadRefs)
                            _weakSubscribers[eventType].Remove(deadRef);
                    }
                }
                else
                {
                    if (_subscribers.ContainsKey(eventType))
                        handlers = new List<object>(_subscribers[eventType]);
                }
            }

            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    try
                    {
                        ((Action<TEvent>)handler)(eventData);
                    }
                    catch (Exception ex)
                    {
                        HandleException(ex, eventData);
                    }
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _subscribers.Clear();
                _weakSubscribers.Clear();
            }
        }

        public void ClearEvent<TEvent>()
        {
            lock (_lock)
            {
                var eventType = typeof(TEvent);
                _subscribers.Remove(eventType);
                _weakSubscribers.Remove(eventType);
            }
        }

        protected virtual void HandleException<TEvent>(Exception ex, TEvent eventData)
        {
        }

        public int GetSubscriberCount<TEvent>()
        {
            lock (_lock)
            {
                var eventType = typeof(TEvent);

                if (_useWeakReferences)
                {
                    if (_weakSubscribers.ContainsKey(eventType))
                        return _weakSubscribers[eventType].Count(wr => wr.IsAlive);
                    return 0;
                }

                return _subscribers.ContainsKey(eventType) ? _subscribers[eventType].Count : 0;
            }
        }
    }

    public class TypedEventArgs<T> : EventArgs
    {
        public T EventData { get; }

        public TypedEventArgs(T eventData)
        {
            EventData = eventData;
        }
    }

    public class EventSubscription<TEvent> : IDisposable
    {
        private readonly EventAggregator _aggregator;
        private readonly Action<TEvent> _handler;
        private bool _isDisposed;

        public EventSubscription(EventAggregator aggregator, Action<TEvent> handler)
        {
            _aggregator = aggregator;
            _handler = handler;
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _aggregator.Unsubscribe(_handler);
                _isDisposed = true;
            }
        }
    }

    public static class EventAggregatorExtensions
    {
        public static EventSubscription<TEvent> Subscribe<TEvent>(this EventAggregator aggregator, Action<TEvent> handler)
        {
            return new EventSubscription<TEvent>(aggregator, handler);
        }

        public static void PublishEvent<TEvent>(this EventAggregator aggregator, TEvent eventData = default)
        {
            aggregator.Publish(eventData);
        }
    }

    public class MessageBus : EventAggregator
    {
        private static readonly Lazy<MessageBus> _instance = new Lazy<MessageBus>(() => new MessageBus());

        public static MessageBus Instance => _instance.Value;

        private MessageBus() { }
    }

    public class EventAggregatorOptions
    {
        public bool UseWeakReferences { get; set; } = false;
        public bool HandleExceptions { get; set; } = true;
        public bool SynchronizeExecution { get; set; } = false;
    }
}

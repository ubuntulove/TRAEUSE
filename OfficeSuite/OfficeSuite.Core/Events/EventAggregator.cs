using System;
using System.Collections.Generic;

namespace OfficeSuite.Core.Events
{
    public class DocumentChangedEvent
    {
        public string DocumentId { get; set; }
        public string DocumentName { get; set; }
        public bool IsNew { get; set; }
        public DateTime Timestamp { get; set; }

        public DocumentChangedEvent()
        {
            Timestamp = DateTime.Now;
        }
    }

    public class DocumentSavedEvent
    {
        public string DocumentId { get; set; }
        public string FilePath { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }

        public DocumentSavedEvent()
        {
            Timestamp = DateTime.Now;
        }
    }

    public class SelectionChangedEvent
    {
        public object Selection { get; set; }
        public Type SelectionType { get; set; }
    }

    public class ApplicationMessageEvent
    {
        public string Message { get; set; }
        public MessageType Type { get; set; }
        public DateTime Timestamp { get; set; }

        public ApplicationMessageEvent()
        {
            Timestamp = DateTime.Now;
            Type = MessageType.Info;
        }
    }

    public enum MessageType
    {
        Info,
        Warning,
        Error,
        Success
    }

    public class EventAggregator
    {
        private static EventAggregator _instance;
        private readonly Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        public static EventAggregator Instance => _instance ??= new EventAggregator();

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);
            if (!_subscribers.ContainsKey(type))
                _subscribers[type] = new List<Delegate>();
            _subscribers[type].Add(handler);
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
                _subscribers[type].Remove(handler);
        }

        public void Publish<T>(T message) where T : class
        {
            var type = typeof(T);
            if (_subscribers.ContainsKey(type))
            {
                foreach (var handler in _subscribers[type])
                {
                    ((Action<T>)handler)(message);
                }
            }
        }

        public void Clear()
        {
            _subscribers.Clear();
        }
    }

    public class DocumentClosedEvent
    {
        public string DocumentId { get; set; }
        public bool HasUnsavedChanges { get; set; }
        public DateTime Timestamp { get; set; }

        public DocumentClosedEvent()
        {
            Timestamp = DateTime.Now;
        }
    }

    public class ModuleActivatedEvent
    {
        public string ModuleName { get; set; }
        public string DocumentId { get; set; }
    }

    public class StatusBarUpdateEvent
    {
        public string Text { get; set; }
        public StatusBarUpdateType UpdateType { get; set; }
    }

    public enum StatusBarUpdateType
    {
        Text,
        Progress,
        Indicator
    }
}

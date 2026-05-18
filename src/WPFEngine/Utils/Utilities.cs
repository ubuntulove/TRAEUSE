using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml.Linq;
using WPFEngine.Math;

namespace WPFEngine.Serialization
{
    public interface ISerializable
    {
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }

    public class GameDataSerializer
    {
        private readonly BinaryFormatter _binaryFormatter;
        private readonly JsonSerializerOptions _jsonOptions;

        public GameDataSerializer()
        {
            _binaryFormatter = new BinaryFormatter();
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public void SaveToBinary<T>(T obj, string filePath) where T : class
        {
            using var stream = File.Create(filePath);
            _binaryFormatter.Serialize(stream, obj);
        }

        public T? LoadFromBinary<T>(string filePath) where T : class
        {
            using var stream = File.OpenRead(filePath);
            return _binaryFormatter.Deserialize(stream) as T;
        }

        public void SaveToJson<T>(T obj, string filePath) where T : class
        {
            string json = System.Text.Json.JsonSerializer.Serialize(obj, _jsonOptions);
            File.WriteAllText(filePath, json);
        }

        public T? LoadFromJson<T>(string filePath) where T : class
        {
            string json = File.ReadAllText(filePath);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }

        public void SaveToXml<T>(T obj, string filePath) where T : class
        {
            var doc = new XDocument();
            SerializeToXml(obj, doc.Root!);
            doc.Save(filePath);
        }

        public T? LoadFromXml<T>(string filePath) where T : class
        {
            var doc = XDocument.Load(filePath);
            return DeserializeFromXml<T>(doc.Root!);
        }

        private void SerializeToXml<T>(T obj, XElement element)
        {
            var type = typeof(T);
            var properties = type.GetProperties();

            foreach (var prop in properties)
            {
                if (!prop.CanRead)
                    continue;

                var value = prop.GetValue(obj);
                if (value == null)
                    continue;

                var propElement = new XElement(prop.Name);

                if (value is string str)
                {
                    propElement.Value = str;
                }
                else if (value is int i)
                {
                    propElement.Value = i.ToString();
                }
                else if (value is float f)
                {
                    propElement.Value = f.ToString();
                }
                else if (value is bool b)
                {
                    propElement.Value = b.ToString();
                }
                else if (value is IEnumerable<object> list)
                {
                    foreach (var item in list)
                    {
                        var itemElement = new XElement("Item");
                        SerializeToXml(item, itemElement);
                        propElement.Add(itemElement);
                    }
                }
                else
                {
                    SerializeToXml(value, propElement);
                }

                element.Add(propElement);
            }
        }

        private T DeserializeFromXml<T>(XElement element) where T : class
        {
            var type = typeof(T);
            var constructor = type.GetConstructors().FirstOrDefault();
            object? obj = constructor?.Invoke(null);

            if (obj == null)
                obj = Activator.CreateInstance<T>();

            var properties = type.GetProperties();

            foreach (var prop in properties)
            {
                if (!prop.CanWrite)
                    continue;

                var propElement = element.Element(prop.Name);
                if (propElement == null)
                    continue;

                object? value = null;

                if (prop.PropertyType == typeof(string))
                {
                    value = propElement.Value;
                }
                else if (prop.PropertyType == typeof(int))
                {
                    value = int.Parse(propElement.Value);
                }
                else if (prop.PropertyType == typeof(float))
                {
                    value = float.Parse(propElement.Value);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    value = bool.Parse(propElement.Value);
                }
                else if (prop.PropertyType.IsClass)
                {
                    value = DeserializeFromXml(prop.PropertyType, propElement);
                }

                if (value != null)
                    prop.SetValue(obj, value);
            }

            return (T)obj!;
        }

        private object DeserializeFromXml(Type type, XElement element)
        {
            var constructor = type.GetConstructors().FirstOrDefault();
            object? obj = constructor?.Invoke(null);

            if (obj == null)
                obj = Activator.CreateInstance(type);

            var properties = type.GetProperties();

            foreach (var prop in properties)
            {
                if (!prop.CanWrite)
                    continue;

                var propElement = element.Element(prop.Name);
                if (propElement == null)
                    continue;

                object? value = null;

                if (prop.PropertyType == typeof(string))
                {
                    value = propElement.Value;
                }
                else if (prop.PropertyType == typeof(int))
                {
                    value = int.Parse(propElement.Value);
                }
                else if (prop.PropertyType == typeof(float))
                {
                    value = float.Parse(propElement.Value);
                }
                else if (prop.PropertyType == typeof(bool))
                {
                    value = bool.Parse(propElement.Value);
                }
                else if (prop.PropertyType.IsClass)
                {
                    value = DeserializeFromXml(prop.PropertyType, propElement);
                }

                if (value != null)
                    prop.SetValue(obj, value);
            }

            return obj!;
        }
    }

    public class SaveGameData : ISerializable
    {
        public string Version { get; set; } = "1.0";
        public DateTime SaveTime { get; set; }
        public string SceneName { get; set; } = "";
        public float PlayTime { get; set; }
        public Dictionary<string, object> PlayerData { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, object> GameState { get; set; } = new Dictionary<string, object>();

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(Version);
            writer.Write(SaveTime.Ticks);
            writer.Write(SceneName);
            writer.Write(PlayTime);
            writer.Write(PlayerData.Count);
            foreach (var kvp in PlayerData)
            {
                writer.Write(kvp.Key);
                WriteObject(writer, kvp.Value);
            }
            writer.Write(GameState.Count);
            foreach (var kvp in GameState)
            {
                writer.Write(kvp.Key);
                WriteObject(writer, kvp.Value);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            Version = reader.ReadString();
            SaveTime = new DateTime(reader.ReadInt64());
            SceneName = reader.ReadString();
            PlayTime = reader.ReadSingle();
            int playerCount = reader.ReadInt32();
            PlayerData.Clear();
            for (int i = 0; i < playerCount; i++)
            {
                string key = reader.ReadString();
                object? value = ReadObject(reader);
                PlayerData[key] = value!;
            }
            int stateCount = reader.ReadInt32();
            GameState.Clear();
            for (int i = 0; i < stateCount; i++)
            {
                string key = reader.ReadString();
                object? value = ReadObject(reader);
                GameState[key] = value!;
            }
        }

        private void WriteObject(BinaryWriter writer, object value)
        {
            if (value is int i)
            {
                writer.Write((byte)0);
                writer.Write(i);
            }
            else if (value is float f)
            {
                writer.Write((byte)1);
                writer.Write(f);
            }
            else if (value is string s)
            {
                writer.Write((byte)2);
                writer.Write(s);
            }
            else if (value is bool b)
            {
                writer.Write((byte)3);
                writer.Write(b);
            }
            else if (value is Vector2 v2)
            {
                writer.Write((byte)4);
                writer.Write(v2.X);
                writer.Write(v2.Y);
            }
            else if (value is Vector3 v3)
            {
                writer.Write((byte)5);
                writer.Write(v3.X);
                writer.Write(v3.Y);
                writer.Write(v3.Z);
            }
        }

        private object? ReadObject(BinaryReader reader)
        {
            byte type = reader.ReadByte();
            return type switch
            {
                0 => reader.ReadInt32(),
                1 => reader.ReadSingle(),
                2 => reader.ReadString(),
                3 => reader.ReadBoolean(),
                4 => new Vector2(reader.ReadSingle(), reader.ReadSingle()),
                5 => new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle()),
                _ => null
            };
        }
    }

    public class HighScoreEntry : ISerializable
    {
        public string PlayerName { get; set; } = "";
        public int Score { get; set; }
        public int Level { get; set; }
        public DateTime Date { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerName);
            writer.Write(Score);
            writer.Write(Level);
            writer.Write(Date.Ticks);
        }

        public void Deserialize(BinaryReader reader)
        {
            PlayerName = reader.ReadString();
            Score = reader.ReadInt32();
            Level = reader.ReadInt32();
            Date = new DateTime(reader.ReadInt64());
        }
    }

    public class SettingsData : ISerializable
    {
        public float MasterVolume { get; set; } = 1f;
        public float MusicVolume { get; set; } = 0.8f;
        public float SfxVolume { get; set; } = 1f;
        public int ScreenWidth { get; set; } = 1920;
        public int ScreenHeight { get; set; } = 1080;
        public bool Fullscreen { get; set; } = false;
        public bool VSync { get; set; } = true;
        public int QualityLevel { get; set; } = 2;
        public float MouseSensitivity { get; set; } = 1f;
        public bool ShowFPS { get; set; } = true;
        public string Language { get; set; } = "en";

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(MasterVolume);
            writer.Write(MusicVolume);
            writer.Write(SfxVolume);
            writer.Write(ScreenWidth);
            writer.Write(ScreenHeight);
            writer.Write(Fullscreen);
            writer.Write(VSync);
            writer.Write(QualityLevel);
            writer.Write(MouseSensitivity);
            writer.Write(ShowFPS);
            writer.Write(Language);
        }

        public void Deserialize(BinaryReader reader)
        {
            MasterVolume = reader.ReadSingle();
            MusicVolume = reader.ReadSingle();
            SfxVolume = reader.ReadSingle();
            ScreenWidth = reader.ReadInt32();
            ScreenHeight = reader.ReadInt32();
            Fullscreen = reader.ReadBoolean();
            VSync = reader.ReadBoolean();
            QualityLevel = reader.ReadInt32();
            MouseSensitivity = reader.ReadSingle();
            ShowFPS = reader.ReadBoolean();
            Language = reader.ReadString();
        }
    }
}

namespace WPFEngine.Storage
{
    public class StorageManager
    {
        private static StorageManager? _instance;
        public static StorageManager Instance => _instance ??= new StorageManager();

        private readonly string _basePath;
        private readonly Dictionary<string, object> _cache;
        private readonly object _cacheLock = new object();

        public string BasePath => _basePath;

        private StorageManager()
        {
            _basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WPFGameEngine"
            );

            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }

            _cache = new Dictionary<string, object>();
        }

        public T? Load<T>(string key) where T : class
        {
            lock (_cacheLock)
            {
                if (_cache.TryGetValue(key, out var cached))
                {
                    return cached as T;
                }
            }

            string filePath = GetFilePath(key);
            if (!File.Exists(filePath))
                return null;

            try
            {
                var serializer = new Serialization.GameDataSerializer();
                var data = serializer.LoadFromBinary<T>(filePath);
                if (data != null)
                {
                    lock (_cacheLock)
                    {
                        _cache[key] = data;
                    }
                }
                return data;
            }
            catch
            {
                return null;
            }
        }

        public void Save<T>(string key, T data) where T : class
        {
            lock (_cacheLock)
            {
                _cache[key] = data;
            }

            string filePath = GetFilePath(key);
            try
            {
                var serializer = new Serialization.GameDataSerializer();
                serializer.SaveToBinary(data, filePath);
            }
            catch
            {
            }
        }

        public void Delete(string key)
        {
            lock (_cacheLock)
            {
                _cache.Remove(key);
            }

            string filePath = GetFilePath(key);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public bool Exists(string key)
        {
            lock (_cacheLock)
            {
                if (_cache.ContainsKey(key))
                    return true;
            }

            return File.Exists(GetFilePath(key));
        }

        public List<string> GetAllKeys(string prefix = "")
        {
            var keys = new List<string>();
            lock (_cacheLock)
            {
                keys.AddRange(_cache.Keys.Where(k => string.IsNullOrEmpty(prefix) || k.StartsWith(prefix)));
            }

            if (Directory.Exists(_basePath))
            {
                var files = Directory.GetFiles(_basePath, "*.dat");
                foreach (var file in files)
                {
                    string key = Path.GetFileNameWithoutExtension(file);
                    if (!keys.Contains(key) && (string.IsNullOrEmpty(prefix) || key.StartsWith(prefix)))
                    {
                        keys.Add(key);
                    }
                }
            }

            return keys;
        }

        public void ClearCache()
        {
            lock (_cacheLock)
            {
                _cache.Clear();
            }
        }

        public void ClearAll()
        {
            ClearCache();

            if (Directory.Exists(_basePath))
            {
                foreach (var file in Directory.GetFiles(_basePath, "*.dat"))
                {
                    File.Delete(file);
                }
            }
        }

        private string GetFilePath(string key)
        {
            string sanitizedKey = key.Replace("/", "_").Replace("\\", "_").Replace(":", "_");
            return Path.Combine(_basePath, $"{sanitizedKey}.dat");
        }

        public long GetTotalStorageSize()
        {
            if (!Directory.Exists(_basePath))
                return 0;

            long totalSize = 0;
            foreach (var file in Directory.GetFiles(_basePath, "*.dat"))
            {
                var info = new FileInfo(file);
                totalSize += info.Length;
            }

            return totalSize;
        }

        public void ExportToJson(string exportPath)
        {
            var allData = new Dictionary<string, object>();
            var keys = GetAllKeys();

            foreach (var key in keys)
            {
                var data = Load<object>(key);
                if (data != null)
                {
                    allData[key] = data;
                }
            }

            var serializer = new Serialization.GameDataSerializer();
            serializer.SaveToJson(allData, exportPath);
        }

        public void ImportFromJson(string importPath)
        {
            if (!File.Exists(importPath))
                return;

            var serializer = new Serialization.GameDataSerializer();
            var data = serializer.LoadFromJson<Dictionary<string, object>>(importPath);

            if (data != null)
            {
                foreach (var kvp in data)
                {
                    Save(kvp.Key, kvp.Value);
                }
            }
        }
    }
}

namespace WPFEngine.Utils
{
    public class ObjectPool<T> where T : class, new()
    {
        private readonly Stack<T> _availableObjects;
        private readonly Func<T>? _objectFactory;
        private readonly Action<T>? _resetAction;
        private int _totalCreated;
        private int _totalBorrowed;

        public int AvailableCount => _availableObjects.Count;
        public int BorrowedCount => _totalBorrowed - _availableObjects.Count;
        public int TotalCreated => _totalCreated;

        public ObjectPool(int initialCapacity = 0, Func<T>? factory = null, Action<T>? resetAction = null)
        {
            _availableObjects = new Stack<T>();
            _objectFactory = factory ?? (() => new T());
            _resetAction = resetAction;
            _totalCreated = 0;
            _totalBorrowed = 0;

            for (int i = 0; i < initialCapacity; i++)
            {
                Return(_objectFactory());
            }
        }

        public T Rent()
        {
            T obj;
            if (_availableObjects.Count > 0)
            {
                obj = _availableObjects.Pop();
            }
            else
            {
                obj = _objectFactory();
                _totalCreated++;
            }
            _totalBorrowed++;
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null)
                return;

            _resetAction?.Invoke(obj);
            _availableObjects.Push(obj);
        }

        public void Clear()
        {
            _availableObjects.Clear();
        }

        public List<T> RentMultiple(int count)
        {
            var objects = new List<T>(count);
            for (int i = 0; i < count; i++)
            {
                objects.Add(Rent());
            }
            return objects;
        }

        public void ReturnMultiple(IEnumerable<T> objects)
        {
            foreach (var obj in objects)
            {
                Return(obj);
            }
        }
    }

    public class PriorityQueue<T>
    {
        private readonly List<(T Item, float Priority)> _heap;
        private readonly bool _isMinHeap;

        public int Count => _heap.Count;

        public PriorityQueue(bool isMinHeap = true)
        {
            _heap = new List<(T, float)>();
            _isMinHeap = isMinHeap;
        }

        public void Enqueue(T item, float priority)
        {
            _heap.Add((item, priority));
            HeapifyUp(_heap.Count - 1);
        }

        public T Dequeue()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");

            T result = _heap[0].Item;
            _heap[0] = _heap[_heap.Count - 1];
            _heap.RemoveAt(_heap.Count - 1);
            HeapifyDown(0);
            return result;
        }

        public T Peek()
        {
            if (_heap.Count == 0)
                throw new InvalidOperationException("Queue is empty");
            return _heap[0].Item;
        }

        public bool TryDequeue(out T? item, out float priority)
        {
            if (_heap.Count == 0)
            {
                item = default;
                priority = 0;
                return false;
            }

            item = Dequeue();
            priority = 0;
            return true;
        }

        private void HeapifyUp(int index)
        {
            while (index > 0)
            {
                int parentIndex = (index - 1) / 2;
                if (Compare(_heap[index].Priority, _heap[parentIndex].Priority) >= 0)
                    break;

                (_heap[index], _heap[parentIndex]) = (_heap[parentIndex], _heap[index]);
                index = parentIndex;
            }
        }

        private void HeapifyDown(int index)
        {
            while (true)
            {
                int leftChild = 2 * index + 1;
                int rightChild = 2 * index + 2;
                int smallest = index;

                if (leftChild < _heap.Count && Compare(_heap[leftChild].Priority, _heap[smallest].Priority) < 0)
                    smallest = leftChild;

                if (rightChild < _heap.Count && Compare(_heap[rightChild].Priority, _heap[smallest].Priority) < 0)
                    smallest = rightChild;

                if (smallest == index)
                    break;

                (_heap[index], _heap[smallest]) = (_heap[smallest], _heap[index]);
                index = smallest;
            }
        }

        private int Compare(float a, float b)
        {
            return _isMinHeap ? a.CompareTo(b) : b.CompareTo(a);
        }

        public void Clear()
        {
            _heap.Clear();
        }

        public bool Contains(T item)
        {
            return _heap.Any(x => EqualityComparer<T>.Default.Equals(x.Item, item));
        }
    }

    public class RingBuffer<T>
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

        public RingBuffer(int capacity)
        {
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

        public IEnumerable<T> GetEnumerable()
        {
            for (int i = 0; i < _count; i++)
            {
                yield return _buffer[(_head + i) % _capacity];
            }
        }
    }

    public class EventBus
    {
        private static EventBus? _instance;
        public static EventBus Instance => _instance ??= new EventBus();

        private readonly Dictionary<Type, List<Delegate>> _subscribers;
        private readonly object _lock = new object();

        private EventBus()
        {
            _subscribers = new Dictionary<Type, List<Delegate>>();
        }

        public void Subscribe<T>(Action<T> handler) where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (!_subscribers.ContainsKey(type))
                {
                    _subscribers[type] = new List<Delegate>();
                }
                _subscribers[type].Add(handler);
            }
        }

        public void Unsubscribe<T>(Action<T> handler) where T : class
        {
            lock (_lock)
            {
                var type = typeof(T);
                if (_subscribers.TryGetValue(type, out var handlers))
                {
                    handlers.Remove(handler);
                }
            }
        }

        public void Publish<T>(T eventData) where T : class
        {
            List<Delegate>? handlers = null;

            lock (_lock)
            {
                if (_subscribers.TryGetValue(typeof(T), out var h))
                {
                    handlers = new List<Delegate>(h);
                }
            }

            if (handlers != null)
            {
                foreach (var handler in handlers)
                {
                    ((Action<T>)handler)(eventData);
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _subscribers.Clear();
            }
        }
    }

    public class GameEvent
    {
        public string Name { get; set; } = "";
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();

        public T? GetData<T>(string key)
        {
            if (Data.TryGetValue(key, out var value) && value is T typed)
            {
                return typed;
            }
            return default;
        }
    }

    public class EventTracker
    {
        private readonly Dictionary<string, List<GameEvent>> _trackedEvents;
        private readonly object _lock = new object();
        private DateTime _sessionStart;

        public EventTracker()
        {
            _trackedEvents = new Dictionary<string, List<GameEvent>>();
            _sessionStart = DateTime.Now;
        }

        public void Track(string eventName, Dictionary<string, object>? data = null)
        {
            var gameEvent = new GameEvent
            {
                Name = eventName,
                Timestamp = DateTime.Now,
                Data = data ?? new Dictionary<string, object>()
            };

            lock (_lock)
            {
                if (!_trackedEvents.ContainsKey(eventName))
                {
                    _trackedEvents[eventName] = new List<GameEvent>();
                }
                _trackedEvents[eventName].Add(gameEvent);
            }

            EventBus.Instance.Publish(gameEvent);
        }

        public int GetEventCount(string eventName)
        {
            lock (_lock)
            {
                return _trackedEvents.TryGetValue(eventName, out var events)
                    ? events.Count
                    : 0;
            }
        }

        public List<GameEvent> GetEvents(string eventName)
        {
            lock (_lock)
            {
                return _trackedEvents.TryGetValue(eventName, out var events)
                    ? new List<GameEvent>(events)
                    : new List<GameEvent>();
            }
        }

        public Dictionary<string, int> GetAllEventCounts()
        {
            lock (_lock)
            {
                return _trackedEvents.ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Count
                );
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _trackedEvents.Clear();
            }
        }
    }

    public class UniqueIdGenerator
    {
        private static readonly object _lock = new object();
        private static long _lastId;

        public static long Generate()
        {
            lock (_lock)
            {
                long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() << 16;
                long id = timestamp | (++_lastId & 0xFFFF);
                return id;
            }
        }
    }

    public class CyclicArray<T>
    {
        private readonly T[] _array;
        private int _index;
        private int _count;

        public int Count => _count;
        public int Capacity => _array.Length;

        public CyclicArray(int capacity)
        {
            _array = new T[capacity];
            _index = 0;
            _count = 0;
        }

        public void Add(T item)
        {
            _array[_index] = item;
            _index = (_index + 1) % _array.Length;
            if (_count < _array.Length)
                _count++;
        }

        public T this[int i]
        {
            get
            {
                if (i < 0 || i >= _count)
                    throw new IndexOutOfRangeException();

                int actualIndex = (_index - _count + i + _array.Length) % _array.Length;
                return _array[actualIndex];
            }
        }

        public T GetLatest()
        {
            if (_count == 0)
                throw new InvalidOperationException("Array is empty");

            int latestIndex = (_index - 1 + _array.Length) % _array.Length;
            return _array[latestIndex];
        }

        public void Clear()
        {
            Array.Clear(_array, 0, _array.Length);
            _index = 0;
            _count = 0;
        }
    }

    public class BitArray
    {
        private readonly ulong[] _data;
        private readonly int _size;

        public int Size => _size;

        public BitArray(int size)
        {
            _size = size;
            _data = new ulong[(size + 63) / 64];
        }

        public void Set(int index, bool value)
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException();

            int arrayIndex = index / 64;
            int bitIndex = index % 64;

            if (value)
                _data[arrayIndex] |= (1UL << bitIndex);
            else
                _data[arrayIndex] &= ~(1UL << bitIndex);
        }

        public bool Get(int index)
        {
            if (index < 0 || index >= _size)
                throw new IndexOutOfRangeException();

            int arrayIndex = index / 64;
            int bitIndex = index % 64;

            return (_data[arrayIndex] & (1UL << bitIndex)) != 0;
        }

        public void Clear()
        {
            Array.Clear(_data, 0, _data.Length);
        }

        public int CountSetBits()
        {
            int count = 0;
            foreach (var value in _data)
            {
                count += CountBits(value);
            }
            return count;
        }

        private static int CountBits(ulong value)
        {
            int count = 0;
            while (value != 0)
            {
                count++;
                value &= value - 1;
            }
            return count;
        }
    }

    public class LRU<TKey, TValue> where TKey : notnull
    {
        private readonly int _capacity;
        private readonly Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>> _cache;
        private readonly LinkedList<(TKey Key, TValue Value)> _order;

        public int Count => _cache.Count;

        public LRU(int capacity)
        {
            _capacity = capacity;
            _cache = new Dictionary<TKey, LinkedListNode<(TKey, TValue)>>(capacity);
            _order = new LinkedList<(TKey, TValue)>();
        }

        public TValue? Get(TKey key)
        {
            if (!_cache.TryGetValue(key, out var node))
                return default;

            _order.Remove(node);
            _order.AddFirst(node);
            return node.Value.Value;
        }

        public void Add(TKey key, TValue value)
        {
            if (_cache.TryGetValue(key, out var existingNode))
            {
                existingNode.Value = (key, value);
                _order.Remove(existingNode);
                _order.AddFirst(existingNode);
                return;
            }

            if (_cache.Count >= _capacity)
            {
                var last = _order.Last;
                if (last != null)
                {
                    _order.RemoveLast();
                    _cache.Remove(last.Value.Key);
                }
            }

            var newNode = _order.AddFirst((key, value));
            _cache[key] = newNode;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            if (_cache.TryGetValue(key, out var node))
            {
                value = node.Value.Value;
                _order.Remove(node);
                _order.AddFirst(node);
                return true;
            }

            value = default!;
            return false;
        }

        public bool Remove(TKey key)
        {
            if (_cache.TryGetValue(key, out var node))
            {
                _order.Remove(node);
                _cache.Remove(key);
                return true;
            }
            return false;
        }

        public void Clear()
        {
            _order.Clear();
            _cache.Clear();
        }
    }

    public class GaussianRandom
    {
        private bool _hasSpare;
        private double _spare;
        private readonly Random _random;

        public GaussianRandom(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            _hasSpare = false;
        }

        public double Next()
        {
            if (_hasSpare)
            {
                _hasSpare = false;
                return _spare;
            }

            double u, v, s;
            do
            {
                u = _random.NextDouble() * 2 - 1;
                v = _random.NextDouble() * 2 - 1;
                s = u * u + v * v;
            } while (s >= 1 || s == 0);

            double normal = Math.Sqrt(-2 * Math.Log(s) / s);
            _spare = v * normal;
            _hasSpare = true;
            return u * normal;
        }

        public float NextFloat()
        {
            return (float)Next();
        }
    }

    public class NoiseGenerator
    {
        private readonly int[] _permutation;
        private readonly int _seed;
        private readonly GaussianRandom _gaussian;

        public NoiseGenerator(int seed = 0)
        {
            _seed = seed;
            _gaussian = new GaussianRandom(seed);

            var random = new Random(seed);
            _permutation = new int[512];

            for (int i = 0; i < 256; i++)
            {
                _permutation[i] = i;
            }

            for (int i = 255; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (_permutation[i], _permutation[j]) = (_permutation[j], _permutation[i]);
            }

            for (int i = 0; i < 256; i++)
            {
                _permutation[i + 256] = _permutation[i];
            }
        }

        public float Perlin(float x, float y)
        {
            int xi = (int)Math.Floor(x) & 255;
            int yi = (int)Math.Floor(y) & 255;

            float xf = x - (float)Math.Floor(x);
            float yf = y - (float)Math.Floor(y);

            float u = Fade(xf);
            float v = Fade(yf);

            int aa = _permutation[_permutation[xi] + yi];
            int ab = _permutation[_permutation[xi] + yi + 1];
            int ba = _permutation[_permutation[xi + 1] + yi];
            int bb = _permutation[_permutation[xi + 1] + yi + 1];

            float x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
            float x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

            return (Lerp(x1, x2, v) + 1) / 2;
        }

        public float Simplex(float xin, float yin)
        {
            float F2 = 0.5f * ((float)Math.Sqrt(3.0f) - 1.0f);
            float s = (xin + yin) * F2;
            int i = (int)Math.Floor(xin + s);
            int j = (int)Math.Floor(yin + s);
            float G2 = (3.0f - (float)Math.Sqrt(3.0f)) / 6.0f;
            float t = (i + j) * G2;
            float X0 = i - t;
            float Y0 = j - t;
            float x0 = xin - X0;
            float y0 = yin - Y0;

            int i1, j1;
            if (x0 > y0) { i1 = 1; j1 = 0; }
            else { i1 = 0; j1 = 1; }

            float x1 = x0 - i1 + G2;
            float y1 = y0 - j1 + G2;
            float x2 = x0 - 1.0f + 2.0f * G2;
            float y2 = y0 - 1.0f + 2.0f * G2;

            int ii = i & 255;
            int jj = j & 255;

            float n0, n1, n2;

            float t0 = 0.5f - x0 * x0 - y0 * y0;
            if (t0 < 0) n0 = 0;
            else { t0 *= t0; n0 = t0 * t0 * Grad(_permutation[ii + _permutation[jj]], x0, y0); }

            float t1 = 0.5f - x1 * x1 - y1 * y1;
            if (t1 < 0) n1 = 0;
            else { t1 *= t1; n1 = t1 * t1 * Grad(_permutation[ii + i1 + _permutation[jj + j1]], x1, y1); }

            float t2 = 0.5f - x2 * x2 - y2 * y2;
            if (t2 < 0) n2 = 0;
            else { t2 *= t2; n2 = t2 * t2 * Grad(_permutation[ii + 1 + _permutation[jj + 1]], x2, y2); }

            return (70.0f * (n0 + n1 + n2) + 1) / 2;
        }

        public float Fractal(int octaves, float x, float y, float persistence = 0.5f)
        {
            float total = 0;
            float frequency = 1;
            float amplitude = 1;
            float maxValue = 0;

            for (int i = 0; i < octaves; i++)
            {
                total += Perlin(x * frequency, y * frequency) * amplitude;
                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= 2;
            }

            return total / maxValue;
        }

        private static float Fade(float t)
        {
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        private static float Grad(int hash, float x, float y)
        {
            int h = hash & 7;
            float u = h < 4 ? x : y;
            float v = h < 4 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }
    }
}

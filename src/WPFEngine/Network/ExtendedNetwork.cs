using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using WPFEngine.Math;

namespace WPFEngine.Network
{
    #region Connection Management

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Handshaking,
        Connected,
        Authenticating,
        Ready,
        Disconnecting
    }

    public enum NetworkTopology
    {
        PeerToPeer,
        ClientServer
    }

    public class ConnectionConfig
    {
        public int TcpPort { get; set; } = 7777;
        public int UdpPort { get; set; } = 7778;
        public int MaxConnections { get; set; } = 32;
        public int TimeoutMs { get; set; } = 10000;
        public int HeartbeatIntervalMs { get; set; } = 1000;
        public int MaxPacketSize { get; set; } = 65535;
        public bool EnableNagle { get; set; } = false;
        public float SendRate { get; set; } = 20f;
        public NetworkTopology Topology { get; set; } = NetworkTopology.ClientServer;
        public int DisconnectTimeoutMs { get; set; } = 30000;
        public int SceneMigrationTimeoutMs { get; set; } = 15000;
    }

    public class NetworkClient
    {
        private int _id = -1;
        private string _name = "Player";
        private ConnectionState _state = ConnectionState.Disconnected;
        private IPEndPoint? _endpoint;
        private long _lastHeartbeat;
        private long _connectTime;
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();
        private byte[]? _sessionToken;

        public int Id => _id;
        public string Name => _name;
        public ConnectionState State => _state;
        public IPEndPoint? EndPoint => _endpoint;
        public long LastHeartbeat => _lastHeartbeat;
        public long ConnectTime => _connectTime;
        public bool IsConnected => _state == ConnectionState.Connected || _state == ConnectionState.Ready;
        public byte[]? SessionToken => _sessionToken;

        public event EventHandler<ConnectionState>? StateChanged;
        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        internal void SetId(int id) => _id = id;
        internal void SetEndpoint(IPEndPoint? endpoint) => _endpoint = endpoint;
        internal void SetSessionToken(byte[]? token) => _sessionToken = token;

        public void SetName(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public void SetState(ConnectionState state)
        {
            if (_state != state)
            {
                _state = state;
                StateChanged?.Invoke(this, state);
            }
        }

        public void UpdateHeartbeat()
        {
            _lastHeartbeat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public void SetProperty(string key, object value)
        {
            _properties[key] = value;
        }

        public T? GetProperty<T>(string key)
        {
            return _properties.TryGetValue(key, out var value) && value is T typed ? typed : default;
        }

        public bool HasProperty(string key) => _properties.ContainsKey(key);
    }

    public class NetworkServer
    {
        private readonly Dictionary<int, NetworkClient> _clients = new Dictionary<int, NetworkClient>();
        private readonly object _clientLock = new object();
        private int _nextClientId = 1;
        private int _maxClients;
        private bool _isRunning;

        public int ClientCount
        {
            get
            {
                lock (_clientLock)
                {
                    return _clients.Count;
                }
            }
        }

        public int MaxClients => _maxClients;
        public bool IsRunning => _isRunning;

        public event EventHandler<NetworkClient>? ClientConnected;
        public event EventHandler<NetworkClient>? ClientDisconnected;
        public event EventHandler<NetworkClient>? ClientReady;
        public event EventHandler<NetworkClient, ConnectionState>? ClientStateChanged;

        public NetworkServer(int maxClients = 32)
        {
            _maxClients = maxClients;
        }

        public void Start()
        {
            _isRunning = true;
        }

        public void Stop()
        {
            _isRunning = false;
            lock (_clientLock)
            {
                _clients.Clear();
            }
        }

        public NetworkClient AddClient(IPEndPoint? endpoint)
        {
            lock (_clientLock)
            {
                if (_clients.Count >= _maxClients)
                    throw new InvalidOperationException("Maximum client count reached");

                var client = new NetworkClient
                {
                    _id = _nextClientId++
                };
                client.SetEndpoint(endpoint);
                client.SetState(ConnectionState.Connected);
                client.UpdateHeartbeat();
                _clients[client.Id] = client;
                ClientConnected?.Invoke(this, client);
                return client;
            }
        }

        public void RemoveClient(int clientId)
        {
            lock (_clientLock)
            {
                if (_clients.TryGetValue(clientId, out var client))
                {
                    client.SetState(ConnectionState.Disconnected);
                    _clients.Remove(clientId);
                    ClientDisconnected?.Invoke(this, client);
                }
            }
        }

        public NetworkClient? GetClient(int clientId)
        {
            lock (_clientLock)
            {
                return _clients.TryGetValue(clientId, out var client) ? client : null;
            }
        }

        public IEnumerable<NetworkClient> GetAllClients()
        {
            lock (_clientLock)
            {
                return _clients.Values.ToList();
            }
        }

        public IEnumerable<NetworkClient> GetReadyClients()
        {
            lock (_clientLock)
            {
                return _clients.Values.Where(c => c.State == ConnectionState.Ready).ToList();
            }
        }

        public void SetClientReady(int clientId, bool ready)
        {
            lock (_clientLock)
            {
                if (_clients.TryGetValue(clientId, out var client))
                {
                    client.SetState(ready ? ConnectionState.Ready : ConnectionState.Connected);
                    if (ready) ClientReady?.Invoke(this, client);
                }
            }
        }

        public void UpdateHeartbeat(int clientId)
        {
            lock (_clientLock)
            {
                if (_clients.TryGetValue(clientId, out var client))
                {
                    client.UpdateHeartbeat();
                }
            }
        }

        public void DisconnectClient(int clientId, string reason = "")
        {
            RemoveClient(clientId);
        }

        public bool SendToClient(int clientId, byte[] data)
        {
            return true;
        }

        public void Broadcast(byte[] data, int? excludeClientId = null)
        {
        }
    }

    #endregion

    #region Message System

    public enum MessageChannel : byte
    {
        Reliable = 0,
        Unreliable = 1,
        Ordered = 2,
        ReliableOrdered = 3,
        Voice = 4,
        Raw = 5
    }

    public enum MessageReliability
    {
        Unreliable,
        UnreliableSequenced,
        Reliable,
        ReliableOrdered,
        ReliableSequenced,
        ReliableAcked,
        Broadcast
    }

    public interface INetworkMessage
    {
        ushort TypeId { get; }
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }

    public struct NetworkMessageBuffer : INetworkMessage
    {
        public ushort TypeId { get; set; }
        public byte Channel { get; set; }
        public byte Reliability { get; set; }
        public uint SequenceNumber { get; set; }
        public int SenderId { get; set; }
        public long Timestamp { get; set; }
        public byte[]? Payload { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(TypeId);
            writer.Write(Channel);
            writer.Write(Reliability);
            writer.Write(SequenceNumber);
            writer.Write(SenderId);
            writer.Write(Timestamp);
            writer.Write(Payload?.Length ?? 0);
            if (Payload != null && Payload.Length > 0)
                writer.Write(Payload);
        }

        public void Deserialize(BinaryReader reader)
        {
            TypeId = reader.ReadUInt16();
            Channel = reader.ReadByte();
            Reliability = reader.ReadByte();
            SequenceNumber = reader.ReadUInt32();
            SenderId = reader.ReadInt32();
            Timestamp = reader.ReadInt64();
            int length = reader.ReadInt32();
            if (length > 0)
                Payload = reader.ReadBytes(length);
        }
    }

    public class MessageSerializer
    {
        private static readonly Dictionary<ushort, Type> _messageTypes = new Dictionary<ushort, Type>();
        private static readonly Dictionary<Type, ushort> _typeToId = new Dictionary<Type, ushort>();
        private static ushort _nextTypeId = 256;

        public static void RegisterMessage<T>(ushort typeId) where T : INetworkMessage, new()
        {
            var type = typeof(T);
            _messageTypes[typeId] = type;
            _typeToId[type] = typeId;
        }

        public static ushort GetTypeId<T>() where T : INetworkMessage
        {
            return _typeToId.TryGetValue(typeof(T), out var id) ? id : 0;
        }

        public static Type? GetType(ushort typeId)
        {
            return _messageTypes.TryGetValue(typeId, out var type) ? type : null;
        }

        public static INetworkMessage? Deserialize(ushort typeId, BinaryReader reader)
        {
            if (!_messageTypes.TryGetValue(typeId, out var type))
                return null;

            var message = (INetworkMessage?)Activator.CreateInstance(type);
            message?.Deserialize(reader);
            return message;
        }

        public static byte[] Serialize<T>(T message) where T : INetworkMessage
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(GetTypeId<T>());
            message.Serialize(writer);
            return stream.ToArray();
        }

        public static INetworkMessage? Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            ushort typeId = reader.ReadUInt16();
            return Deserialize(typeId, reader);
        }

        public static void SerializeValue<T>(BinaryWriter writer, T value)
        {
            if (value == null)
            {
                writer.Write((byte)0);
                return;
            }

            writer.Write((byte)1);

            if (typeof(T) == typeof(int))
                writer.Write((int)(object)value!);
            else if (typeof(T) == typeof(uint))
                writer.Write((uint)(object)value!);
            else if (typeof(T) == typeof(float))
                writer.Write((float)(object)value!);
            else if (typeof(T) == typeof(double))
                writer.Write((double)(object)value!);
            else if (typeof(T) == typeof(long))
                writer.Write((long)(object)value!);
            else if (typeof(T) == typeof(ulong))
                writer.Write((ulong)(object)value!);
            else if (typeof(T) == typeof(short))
                writer.Write((short)(object)value!);
            else if (typeof(T) == typeof(ushort))
                writer.Write((ushort)(object)value!);
            else if (typeof(T) == typeof(byte))
                writer.Write((byte)(object)value!);
            else if (typeof(T) == typeof(sbyte))
                writer.Write((sbyte)(object)value!);
            else if (typeof(T) == typeof(bool))
                writer.Write((bool)(object)value!);
            else if (typeof(T) == typeof(string))
                writer.Write((string)(object)value!);
            else if (typeof(T) == typeof(Vector2))
            {
                var v = (Vector2)(object)value!;
                writer.Write(v.X);
                writer.Write(v.Y);
            }
            else if (typeof(T) == typeof(Vector3))
            {
                var v = (Vector3)(object)value!;
                writer.Write(v.X);
                writer.Write(v.Y);
                writer.Write(v.Z);
            }
            else if (typeof(T) == typeof(Color))
            {
                var c = (Color)(object)value!;
                writer.Write(c.R);
                writer.Write(c.G);
                writer.Write(c.B);
                writer.Write(c.A);
            }
            else
                throw new NotSupportedException($"Type {typeof(T)} is not supported for serialization");
        }

        public static T DeserializeValue<T>(BinaryReader reader)
        {
            bool hasValue = reader.ReadByte() == 1;
            if (!hasValue)
                return default!;

            if (typeof(T) == typeof(int))
                return (T)(object)reader.ReadInt32();
            if (typeof(T) == typeof(uint))
                return (T)(object)reader.ReadUInt32();
            if (typeof(T) == typeof(float))
                return (T)(object)reader.ReadSingle();
            if (typeof(T) == typeof(double))
                return (T)(object)reader.ReadDouble();
            if (typeof(T) == typeof(long))
                return (T)(object)reader.ReadInt64();
            if (typeof(T) == typeof(ulong))
                return (T)(object)reader.ReadUInt64();
            if (typeof(T) == typeof(short))
                return (T)(object)reader.ReadInt16();
            if (typeof(T) == typeof(ushort))
                return (T)(object)reader.ReadUInt16();
            if (typeof(T) == typeof(byte))
                return (T)(object)reader.ReadByte();
            if (typeof(T) == typeof(sbyte))
                return (T)(object)reader.ReadSByte();
            if (typeof(T) == typeof(bool))
                return (T)(object)reader.ReadBoolean();
            if (typeof(T) == typeof(string))
                return (T)(object)reader.ReadString();
            if (typeof(T) == typeof(Vector2))
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                return (T)(object)new Vector2(x, y);
            }
            if (typeof(T) == typeof(Vector3))
            {
                float x = reader.ReadSingle();
                float y = reader.ReadSingle();
                float z = reader.ReadSingle();
                return (T)(object)new Vector3(x, y, z);
            }
            if (typeof(T) == typeof(Color))
            {
                float r = reader.ReadSingle();
                float g = reader.ReadSingle();
                float b = reader.ReadSingle();
                float a = reader.ReadSingle();
                return (T)(object)new Color(r, g, b, a);
            }

            throw new NotSupportedException($"Type {typeof(T)} is not supported for deserialization");
        }
    }

    public class MessageQueue
    {
        private readonly Queue<NetworkMessageBuffer> _queue = new Queue<NetworkMessageBuffer>();
        private readonly object _lock = new object();
        private int _maxSize;

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

        public MessageQueue(int maxSize = 1024)
        {
            _maxSize = maxSize;
        }

        public void Enqueue(NetworkMessageBuffer message)
        {
            lock (_lock)
            {
                if (_queue.Count >= _maxSize)
                    _queue.Dequeue();
                _queue.Enqueue(message);
            }
        }

        public bool TryDequeue(out NetworkMessageBuffer message)
        {
            lock (_lock)
            {
                if (_queue.Count > 0)
                {
                    message = _queue.Dequeue();
                    return true;
                }
            }
            message = default;
            return false;
        }

        public List<NetworkMessageBuffer> DequeueAll()
        {
            lock (_lock)
            {
                var messages = _queue.ToList();
                _queue.Clear();
                return messages;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _queue.Clear();
            }
        }
    }

    #endregion

    #region RPC System

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RpcAttribute : Attribute
    {
        public RpcTarget Target { get; set; } = RpcTarget.All;
        public bool IncludeOwner { get; set; } = true;
        public Channel Channel { get; set; } = Channel.Reliable;
    }

    public enum RpcTarget
    {
        Server,
        Client,
        Others,
        All,
        AllBuffered,
        OthersBuffered,
        MasterClient
    }

    public enum Channel
    {
        Unreliable = 0,
        Reliable = 1,
        ReliableUnordered = 2
    }

    public struct RpcCall
    {
        public string MethodName { get; set; }
        public int TargetId { get; set; }
        public RpcTarget Target { get; set; }
        public byte[]? Arguments { get; set; }
        public int SenderId { get; set; }
        public long Timestamp { get; set; }
        public ushort NetworkBehaviourId { get; set; }
    }

    public class RpcParameter
    {
        public Type Type { get; set; } = null!;
        public string Name { get; set; } = "";
        public object? Value { get; set; }
    }

    public class RpcMessage : INetworkMessage
    {
        public ushort TypeId => 1;
        public ushort NetworkBehaviourId { get; set; }
        public string MethodName { get; set; } = "";
        public RpcTarget Target { get; set; }
        public int SenderId { get; set; }
        public int TargetId { get; set; }
        public byte[]? Arguments { get; set; }
        public long Timestamp { get; set; }
        public Channel Channel { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(NetworkBehaviourId);
            writer.Write(MethodName);
            writer.Write((byte)Target);
            writer.Write(SenderId);
            writer.Write(TargetId);
            writer.Write(Arguments?.Length ?? 0);
            if (Arguments != null && Arguments.Length > 0)
                writer.Write(Arguments);
            writer.Write(Timestamp);
            writer.Write((byte)Channel);
        }

        public void Deserialize(BinaryReader reader)
        {
            NetworkBehaviourId = reader.ReadUInt16();
            MethodName = reader.ReadString();
            Target = (RpcTarget)reader.ReadByte();
            SenderId = reader.ReadInt32();
            TargetId = reader.ReadInt32();
            int length = reader.ReadInt32();
            if (length > 0)
                Arguments = reader.ReadBytes(length);
            Timestamp = reader.ReadInt64();
            Channel = (Channel)reader.ReadByte();
        }
    }

    public class ServerRpcAttribute : Attribute
    {
        public RpcTarget Target { get; set; } = RpcTarget.Server;
        public Channel Channel { get; set; } = Channel.Reliable;
    }

    public class RpcHandler
    {
        private readonly Dictionary<string, MethodInfo> _rpcMethods = new Dictionary<string, MethodInfo>();
        private readonly Dictionary<int, object> _networkBehaviours = new Dictionary<int, object>();
        private readonly object _lock = new object();

        public event EventHandler<RpcMessage>? RpcReceived;

        public void RegisterRpc(string name, MethodInfo method)
        {
            lock (_lock)
            {
                _rpcMethods[name] = method;
            }
        }

        public void RegisterNetworkBehaviour(int id, object behaviour)
        {
            lock (_lock)
            {
                _networkBehaviours[id] = behaviour;
            }
        }

        public void UnregisterNetworkBehaviour(int id)
        {
            lock (_lock)
            {
                _networkBehaviours.Remove(id);
            }
        }

        public void HandleRpc(RpcMessage rpc)
        {
            lock (_lock)
            {
                string key = $"{rpc.NetworkBehaviourId}:{rpc.MethodName}";
                if (_rpcMethods.TryGetValue(key, out var method) &&
                    _networkBehaviours.TryGetValue(rpc.NetworkBehaviourId, out var behaviour))
                {
                    var args = DeserializeArguments(method.GetParameters(), rpc.Arguments);
                    method.Invoke(behaviour, args);
                }
            }
            RpcReceived?.Invoke(this, rpc);
        }

        private object?[] DeserializeArguments(ParameterInfo[] parameters, byte[]? data)
        {
            if (data == null || data.Length == 0)
                return Array.Empty<object>();

            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            var args = new List<object?>();

            foreach (var param in parameters)
            {
                try
                {
                    args.Add(MessageSerializer.DeserializeValue<object>(reader));
                }
                catch
                {
                    args.Add(null);
                }
            }

            return args.ToArray();
        }

        public byte[] SerializeArguments(object?[] args, Type[] types)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            for (int i = 0; i < args.Length && i < types.Length; i++)
            {
                SerializeValue(writer, args[i], types[i]);
            }

            return stream.ToArray();
        }

        private void SerializeValue(BinaryWriter writer, object? value, Type type)
        {
            if (value == null)
            {
                writer.Write((byte)0);
                return;
            }

            writer.Write((byte)1);

            if (type == typeof(int))
                writer.Write((int)value);
            else if (type == typeof(float))
                writer.Write((float)value);
            else if (type == typeof(string))
                writer.Write((string)value ?? "");
            else if (type == typeof(bool))
                writer.Write((bool)value);
            else if (type == typeof(Vector2))
                ((Vector2)value).WriteTo(writer);
            else if (type == typeof(Vector3))
                ((Vector3)value).WriteTo(writer);
        }
    }

    #endregion

    #region Sync System

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class NetworkSyncVarAttribute : Attribute
    {
        public string? HookMethod { get; set; }
    }

    public class NetworkIdentity
    {
        private static ushort _nextNetId = 1;
        private ushort _netId;
        private int _ownerId = -1;
        private bool _isOwned;
        private bool _isLocalPlayer;
        private bool _hasAuthority;
        private readonly Dictionary<ushort, object> _syncVars = new Dictionary<ushort, object>();
        private readonly List<ushort> _dirtySyncVars = new List<ushort>();

        public ushort NetId => _netId;
        public int OwnerId => _ownerId;
        public bool IsOwned => _isOwned;
        public bool IsLocalPlayer => _isLocalPlayer;
        public bool HasAuthority => _hasAuthority;

        public event EventHandler<int>? OwnershipChanged;
        public event EventHandler? LocalPlayerSet;

        public NetworkIdentity()
        {
            _netId = _nextNetId++;
        }

        public void SetOwner(int ownerId)
        {
            _ownerId = ownerId;
            _isOwned = ownerId >= 0;
            OwnershipChanged?.Invoke(this, ownerId);
        }

        public void SetLocalPlayer(int localPlayerId)
        {
            _isLocalPlayer = _ownerId == localPlayerId;
            if (_isLocalPlayer)
                LocalPlayerSet?.Invoke(this, EventArgs.Empty);
        }

        public void SetAuthority(bool hasAuthority)
        {
            _hasAuthority = hasAuthority;
        }

        public void SetSyncVar<T>(ushort index, T value)
        {
            _syncVars[index] = value!;
            if (!_dirtySyncVars.Contains(index))
                _dirtySyncVars.Add(index);
        }

        public T? GetSyncVar<T>(ushort index)
        {
            return _syncVars.TryGetValue(index, out var value) && value is T typed ? typed : default;
        }

        public List<ushort> GetDirtySyncVars()
        {
            return new List<ushort>(_dirtySyncVars);
        }

        public void ClearDirtySyncVars()
        {
            _dirtySyncVars.Clear();
        }

        public byte[] SerializeSyncVars()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(_netId);
            writer.Write(_syncVars.Count);
            foreach (var kvp in _syncVars)
            {
                writer.Write(kvp.Key);
                MessageSerializer.SerializeValue(writer, kvp.Value);
            }

            return stream.ToArray();
        }

        public void DeserializeSyncVars(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            reader.ReadUInt16();
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                ushort index = reader.ReadUInt16();
                var value = MessageSerializer.DeserializeValue<object>(reader);
                _syncVars[index] = value!;
            }
        }
    }

    public abstract class NetworkBehaviour
    {
        protected NetworkIdentity _identity = null!;
        private readonly Dictionary<string, ushort> _syncVarIndices = new Dictionary<string, ushort>();
        private ushort _nextSyncVarIndex;
        private bool _isServer;
        private bool _isClient;
        private bool _isLocalPlayer;

        public NetworkIdentity Identity => _identity;
        public ushort BehaviourId { get; protected set; }
        public bool IsServer => _isServer;
        public bool IsClient => _isClient;
        public bool IsLocalPlayer => _isLocalPlayer;
        public int OwnerId => _identity.OwnerId;

        public event EventHandler? Started;
        public event EventHandler? UpdateReceived;

        protected void Initialize(NetworkIdentity identity, ushort behaviourId)
        {
            _identity = identity;
            BehaviourId = behaviourId;
        }

        protected ushort RegisterSyncVar(string name)
        {
            ushort index = _nextSyncVarIndex++;
            _syncVarIndices[name] = index;
            return index;
        }

        protected T GetSyncVar<T>(string name)
        {
            if (_syncVarIndices.TryGetValue(name, out var index))
            {
                return _identity.GetSyncVar<T>(index);
            }
            return default!;
        }

        protected void SetSyncVar<T>(string name, T value)
        {
            if (_syncVarIndices.TryGetValue(name, out var index))
            {
                _identity.SetSyncVar(index, value);
            }
        }

        public virtual void OnNetworkStart() { }
        public virtual void OnNetworkUpdate() { }
        public virtual void OnNetworkLateUpdate() { }

        internal void SetServer(bool isServer) => _isServer = isServer;
        internal void SetClient(bool isClient) => _isClient = isClient;
        internal void SetLocalPlayer(bool isLocalPlayer) => _isLocalPlayer = isLocalPlayer;

        internal void InvokeStarted() => Started?.Invoke(this, EventArgs.Empty);
        internal void InvokeUpdateReceived() => UpdateReceived?.Invoke(this, EventArgs.Empty);
    }

    public class SyncList<T>
    {
        private readonly List<T> _items = new List<T>();
        private readonly List<SyncListOperation> _operations = new List<SyncListOperation>();
        private readonly object _lock = new object();
        private bool _isDirty;

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }

        public event EventHandler<SyncListEventArgs<T>>? Changed;

        public void Add(T item)
        {
            lock (_lock)
            {
                int index = _items.Count;
                _items.Add(item);
                _operations.Add(new SyncListOperation { Type = SyncListOperationType.Add, Index = (ushort)index });
                _isDirty = true;
                Changed?.Invoke(this, new SyncListEventArgs<T>(SyncListOperationType.Add, index, item));
            }
        }

        public void Remove(T item)
        {
            lock (_lock)
            {
                int index = _items.IndexOf(item);
                if (index >= 0)
                {
                    _items.RemoveAt(index);
                    _operations.Add(new SyncListOperation { Type = SyncListOperationType.Remove, Index = (ushort)index });
                    _isDirty = true;
                    Changed?.Invoke(this, new SyncListEventArgs<T>(SyncListOperationType.Remove, index, item));
                }
            }
        }

        public void Insert(int index, T item)
        {
            lock (_lock)
            {
                if (index >= 0 && index <= _items.Count)
                {
                    _items.Insert(index, item);
                    _operations.Add(new SyncListOperation { Type = SyncListOperationType.Insert, Index = (ushort)index });
                    _isDirty = true;
                    Changed?.Invoke(this, new SyncListEventArgs<T>(SyncListOperationType.Insert, index, item));
                }
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
                _operations.Add(new SyncListOperation { Type = SyncListOperationType.Clear, Index = 0 });
                _isDirty = true;
                Changed?.Invoke(this, new SyncListEventArgs<T>(SyncListOperationType.Clear, 0, default!));
            }
        }

        public T? Get(int index)
        {
            lock (_lock)
            {
                return index >= 0 && index < _items.Count ? _items[index] : default;
            }
        }

        public void Set(int index, T item)
        {
            lock (_lock)
            {
                if (index >= 0 && index < _items.Count)
                {
                    _items[index] = item;
                    _operations.Add(new SyncListOperation { Type = SyncListOperationType.Set, Index = (ushort)index });
                    _isDirty = true;
                    Changed?.Invoke(this, new SyncListEventArgs<T>(SyncListOperationType.Set, index, item));
                }
            }
        }

        public bool IsDirty => _isDirty;

        public List<SyncListOperation> GetOperations()
        {
            lock (_lock)
            {
                return new List<SyncListOperation>(_operations);
            }
        }

        public void ClearOperations()
        {
            lock (_lock)
            {
                _operations.Clear();
                _isDirty = false;
            }
        }

        public byte[] Serialize()
        {
            lock (_lock)
            {
                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                writer.Write(_items.Count);
                foreach (var item in _items)
                {
                    MessageSerializer.SerializeValue(writer, item);
                }
                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            lock (_lock)
            {
                using var stream = new MemoryStream(data);
                using var reader = new BinaryReader(stream);
                int count = reader.ReadInt32();
                _items.Clear();
                for (int i = 0; i < count; i++)
                {
                    var item = MessageSerializer.DeserializeValue<T>(reader);
                    _items.Add(item!);
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_lock)
            {
                return _items.ToList().GetEnumerator();
            }
        }
    }

    public enum SyncListOperationType
    {
        Add,
        Remove,
        Insert,
        Set,
        Clear
    }

    public struct SyncListOperation
    {
        public SyncListOperationType Type { get; set; }
        public ushort Index { get; set; }
    }

    public class SyncListEventArgs<T> : EventArgs
    {
        public SyncListOperationType Operation { get; }
        public int Index { get; }
        public T? Value { get; }

        public SyncListEventArgs(SyncListOperationType operation, int index, T? value)
        {
            Operation = operation;
            Index = index;
            Value = value;
        }
    }

    public class SyncDictionary<TKey, TValue> where TKey : notnull
    {
        private readonly Dictionary<TKey, TValue> _items = new Dictionary<TKey, TValue>();
        private readonly List<SyncDictionaryOperation<TKey, TValue>> _operations = new List<SyncDictionaryOperation<TKey, TValue>>();
        private readonly object _lock = new object();
        private bool _isDirty;

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _items.Count;
                }
            }
        }

        public event EventHandler<SyncDictionaryEventArgs<TKey, TValue>>? Changed;

        public void Add(TKey key, TValue value)
        {
            lock (_lock)
            {
                if (!_items.ContainsKey(key))
                {
                    _items[key] = value;
                    _operations.Add(new SyncDictionaryOperation<TKey, TValue>
                    {
                        Type = SyncDictionaryOperationType.Add,
                        Key = key,
                        Value = value
                    });
                    _isDirty = true;
                    Changed?.Invoke(this, new SyncDictionaryEventArgs<TKey, TValue>(
                        SyncDictionaryOperationType.Add, key, value));
                }
            }
        }

        public bool Remove(TKey key)
        {
            lock (_lock)
            {
                if (_items.TryGetValue(key, out var value))
                {
                    _items.Remove(key);
                    _operations.Add(new SyncDictionaryOperation<TKey, TValue>
                    {
                        Type = SyncDictionaryOperationType.Remove,
                        Key = key,
                        Value = value
                    });
                    _isDirty = true;
                    Changed?.Invoke(this, new SyncDictionaryEventArgs<TKey, TValue>(
                        SyncDictionaryOperationType.Remove, key, value));
                    return true;
                }
            }
            return false;
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            lock (_lock)
            {
                return _items.TryGetValue(key, out value);
            }
        }

        public TValue? Get(TKey key)
        {
            lock (_lock)
            {
                return _items.TryGetValue(key, out var value) ? value : default;
            }
        }

        public void Set(TKey key, TValue value)
        {
            lock (_lock)
            {
                bool isAdd = !_items.ContainsKey(key);
                _items[key] = value;
                _operations.Add(new SyncDictionaryOperation<TKey, TValue>
                {
                    Type = isAdd ? SyncDictionaryOperationType.Add : SyncDictionaryOperationType.Set,
                    Key = key,
                    Value = value
                });
                _isDirty = true;
                Changed?.Invoke(this, new SyncDictionaryEventArgs<TKey, TValue>(
                    isAdd ? SyncDictionaryOperationType.Add : SyncDictionaryOperationType.Set, key, value));
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _items.Clear();
                _operations.Add(new SyncDictionaryOperation<TKey, TValue>
                {
                    Type = SyncDictionaryOperationType.Clear,
                    Key = default!,
                    Value = default!
                });
                _isDirty = true;
            }
        }

        public bool IsDirty => _isDirty;

        public List<SyncDictionaryOperation<TKey, TValue>> GetOperations()
        {
            lock (_lock)
            {
                return new List<SyncDictionaryOperation<TKey, TValue>>(_operations);
            }
        }

        public void ClearOperations()
        {
            lock (_lock)
            {
                _operations.Clear();
                _isDirty = false;
            }
        }
    }

    public enum SyncDictionaryOperationType
    {
        Add,
        Remove,
        Set,
        Clear
    }

    public struct SyncDictionaryOperation<TKey, TValue>
    {
        public SyncDictionaryOperationType Type { get; set; }
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }

    public class SyncDictionaryEventArgs<TKey, TValue> : EventArgs
    {
        public SyncDictionaryOperationType Operation { get; }
        public TKey Key { get; }
        public TValue Value { get; }

        public SyncDictionaryEventArgs(SyncDictionaryOperationType operation, TKey key, TValue value)
        {
            Operation = operation;
            Key = key;
            Value = value;
        }
    }

    public class NetworkTransform
    {
        private Vector3 _position;
        private Vector3 _rotation;
        private Vector3 _scale = Vector3.One;
        private Vector3 _velocity;
        private float _snapThreshold = 0.1f;
        private float _lerpSpeed = 10f;
        private TransformSyncMode _syncMode = TransformSyncMode.SyncPositionAndRotation;
        private bool _isOwner;
        private long _lastUpdateTime;
        private readonly object _lock = new object();

        public Vector3 Position
        {
            get
            {
                lock (_lock)
                {
                    return _position;
                }
            }
            set
            {
                lock (_lock)
                {
                    _position = value;
                }
            }
        }

        public Vector3 Rotation
        {
            get
            {
                lock (_lock)
                {
                    return _rotation;
                }
            }
            set
            {
                lock (_lock)
                {
                    _rotation = value;
                }
            }
        }

        public Vector3 Scale
        {
            get
            {
                lock (_lock)
                {
                    return _scale;
                }
            }
            set
            {
                lock (_lock)
                {
                    _scale = value;
                }
            }
        }

        public Vector3 Velocity
        {
            get
            {
                lock (_lock)
                {
                    return _velocity;
                }
            }
            set
            {
                lock (_lock)
                {
                    _velocity = value;
                }
            }
        }

        public float SnapThreshold
        {
            get => _snapThreshold;
            set => _snapThreshold = value;
        }

        public float LerpSpeed
        {
            get => _lerpSpeed;
            set => _lerpSpeed = value;
        }

        public TransformSyncMode SyncMode
        {
            get => _syncMode;
            set => _syncMode = value;
        }

        public bool IsOwner
        {
            get => _isOwner;
            set => _isOwner = value;
        }

        public NetworkTransform()
        {
            _position = Vector3.Zero;
            _rotation = Vector3.Zero;
            _scale = Vector3.One;
        }

        public void SetFromServer(Vector3 position, Vector3 rotation, Vector3 scale, long timestamp)
        {
            lock (_lock)
            {
                if (timestamp > _lastUpdateTime)
                {
                    _position = position;
                    _rotation = rotation;
                    _scale = scale;
                    _lastUpdateTime = timestamp;
                }
            }
        }

        public Vector3 InterpolatePosition(Vector3 target, float deltaTime)
        {
            lock (_lock)
            {
                return Vector3.Lerp(_position, target, _lerpSpeed * deltaTime);
            }
        }

        public Vector3 InterpolateRotation(Vector3 target, float deltaTime)
        {
            lock (_lock)
            {
                return Vector3.Lerp(_rotation, target, _lerpSpeed * deltaTime);
            }
        }

        public bool NeedsSnap(Vector3 target)
        {
            lock (_lock)
            {
                return Vector3.Distance(_position, target) > _snapThreshold;
            }
        }

        public byte[] Serialize()
        {
            lock (_lock)
            {
                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                _position.WriteTo(writer);
                _rotation.WriteTo(writer);
                _scale.WriteTo(writer);
                writer.Write(_lastUpdateTime);
                return stream.ToArray();
            }
        }

        public void Deserialize(byte[] data)
        {
            lock (_lock)
            {
                using var stream = new MemoryStream(data);
                using var reader = new BinaryReader(stream);
                _position = Vector3.ReadFrom(reader);
                _rotation = Vector3.ReadFrom(reader);
                _scale = Vector3.ReadFrom(reader);
                _lastUpdateTime = reader.ReadInt64();
            }
        }
    }

    public enum TransformSyncMode
    {
        SyncNothing,
        SyncPositionOnly,
        SyncPositionAndRotation,
        SyncPositionRotationAndScale
    }

    #endregion

    #region Object Spawning

    public class NetworkSpawner
    {
        private readonly Dictionary<int, NetworkObject> _spawnedObjects = new Dictionary<int, NetworkObject>();
        private readonly Dictionary<string, Func<NetworkObject>> _prefabRegistry = new Dictionary<string, Func<NetworkObject>>();
        private readonly Dictionary<int, Queue<NetworkObject>> _objectPools = new Dictionary<int, Queue<NetworkObject>>();
        private readonly object _lock = new object();
        private int _nextObjectId = 1;

        public event EventHandler<NetworkObject>? ObjectSpawned;
        public event EventHandler<NetworkObject>? ObjectDestroyed;

        public static NetworkSpawner? Instance { get; set; }

        public int SpawnedObjectCount
        {
            get
            {
                lock (_lock)
                {
                    return _spawnedObjects.Count;
                }
            }
        }

        public void RegisterPrefab(string prefabId, Func<NetworkObject> factory)
        {
            lock (_lock)
            {
                _prefabRegistry[prefabId] = factory;
            }
        }

        public void UnregisterPrefab(string prefabId)
        {
            lock (_lock)
            {
                _prefabRegistry.Remove(prefabId);
            }
        }

        public NetworkObject? Spawn(string prefabId, int ownerId = -1, Vector3? position = null)
        {
            lock (_lock)
            {
                if (!_prefabRegistry.TryGetValue(prefabId, out var factory))
                    return null;

                var obj = factory();
                int objectId = _nextObjectId++;

                obj.Id = objectId;
                obj.OwnerId = ownerId;
                obj.Position = position ?? Vector3.Zero;
                obj.IsOwner = ownerId == NetworkManagerExtended.Instance?.LocalClientId;

                _spawnedObjects[objectId] = obj;
                obj.OnNetworkSpawn();
                ObjectSpawned?.Invoke(this, obj);

                return obj;
            }
        }

        public NetworkObject? SpawnFromPool(string prefabId, int ownerId = -1, Vector3? position = null)
        {
            lock (_lock)
            {
                int prefabHash = prefabId.GetHashCode();
                if (_objectPools.TryGetValue(prefabHash, out var pool) && pool.Count > 0)
                {
                    var obj = pool.Dequeue();
                    int objectId = _nextObjectId++;

                    obj.Id = objectId;
                    obj.OwnerId = ownerId;
                    obj.Position = position ?? Vector3.Zero;
                    obj.IsOwner = ownerId == NetworkManagerExtended.Instance?.LocalClientId;

                    _spawnedObjects[objectId] = obj;
                    obj.OnNetworkSpawn();
                    ObjectSpawned?.Invoke(this, obj);

                    return obj;
                }
                return Spawn(prefabId, ownerId, position);
            }
        }

        public void ReturnToPool(NetworkObject obj)
        {
            lock (_lock)
            {
                if (_spawnedObjects.Remove(obj.Id))
                {
                    int prefabHash = obj.PrefabId?.GetHashCode() ?? 0;
                    if (!_objectPools.ContainsKey(prefabHash))
                        _objectPools[prefabHash] = new Queue<NetworkObject>();
                    _objectPools[prefabHash].Enqueue(obj);
                }
            }
        }

        public void Destroy(NetworkObject obj)
        {
            lock (_lock)
            {
                if (_spawnedObjects.Remove(obj.Id))
                {
                    obj.OnNetworkDestroy();
                    ObjectDestroyed?.Invoke(this, obj);
                }
            }
        }

        public void DestroyById(int objectId)
        {
            lock (_lock)
            {
                if (_spawnedObjects.TryGetValue(objectId, out var obj))
                {
                    _spawnedObjects.Remove(objectId);
                    obj.OnNetworkDestroy();
                    ObjectDestroyed?.Invoke(this, obj);
                }
            }
        }

        public NetworkObject? GetObject(int objectId)
        {
            lock (_lock)
            {
                return _spawnedObjects.TryGetValue(objectId, out var obj) ? obj : null;
            }
        }

        public List<NetworkObject> GetAllObjects()
        {
            lock (_lock)
            {
                return _spawnedObjects.Values.ToList();
            }
        }

        public List<NetworkObject> GetObjectsByOwner(int ownerId)
        {
            lock (_lock)
            {
                return _spawnedObjects.Values.Where(o => o.OwnerId == ownerId).ToList();
            }
        }

        public void ClearAll()
        {
            lock (_lock)
            {
                foreach (var obj in _spawnedObjects.Values)
                {
                    obj.OnNetworkDestroy();
                }
                _spawnedObjects.Clear();
                _objectPools.Clear();
            }
        }
    }

    public class NetworkObject
    {
        private int _id = -1;
        private int _ownerId = -1;
        private bool _isOwner;
        private string? _prefabId;
        private Vector3 _position;
        private Vector3 _rotation = Vector3.Zero;
        private Vector3 _scale = Vector3.One;
        private bool _isSpawned;
        private readonly Dictionary<ushort, NetworkBehaviour> _behaviours = new Dictionary<ushort, NetworkBehaviour>();

        public int Id => _id;
        public int OwnerId => _ownerId;
        public bool IsOwner => _isOwner;
        public string? PrefabId => _prefabId;
        public Vector3 Position
        {
            get => _position;
            set => _position = value;
        }
        public Vector3 Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }
        public Vector3 Scale
        {
            get => _scale;
            set => _scale = value;
        }
        public bool IsSpawned => _isSpawned;

        public NetworkIdentity NetworkIdentity { get; } = new NetworkIdentity();

        public event EventHandler? OnSpawn;
        public event EventHandler? OnDestroy;
        public event EventHandler? OnOwnershipChanged;

        public virtual void OnNetworkSpawn() { _isSpawned = true; OnSpawn?.Invoke(this, EventArgs.Empty); }
        public virtual void OnNetworkDestroy() { _isSpawned = false; OnDestroy?.Invoke(this, EventArgs.Empty); }
        public virtual void OnOwnershipChangedEvent(int newOwnerId) { OnOwnershipChanged?.Invoke(this, EventArgs.Empty); }

        public void SetPrefabId(string prefabId) => _prefabId = prefabId;
        public void SetId(int id) => _id = id;
        public void SetOwner(int ownerId) { _ownerId = ownerId; _isOwner = _ownerId >= 0; OnOwnershipChangedEvent(ownerId); }
        public void SetSpawned(bool spawned) => _isSpawned = spawned;

        public void AddBehaviour(ushort id, NetworkBehaviour behaviour)
        {
            _behaviours[id] = behaviour;
        }

        public T? GetBehaviour<T>() where T : NetworkBehaviour
        {
            return _behaviours.Values.FirstOrDefault(b => b is T) as T;
        }
    }

    public class ClientPrediction
    {
        private readonly Queue<PredictionCommand> _pendingCommands = new Queue<PredictionCommand>();
        private readonly Dictionary<int, object> _predictedStates = new Dictionary<int, object>();
        private readonly object _lock = new object();
        private int _lastProcessedSequence;

        public event EventHandler<PredictionCommand>? CommandExecuted;
        public event EventHandler<int>? StateReconciled;

        public void AddCommand(PredictionCommand command)
        {
            lock (_lock)
            {
                _pendingCommands.Enqueue(command);
            }
        }

        public void ExecuteCommand(PredictionCommand command)
        {
            CommandExecuted?.Invoke(this, command);
        }

        public void ProcessServerUpdate(int serverSequence, object serverState)
        {
            lock (_lock)
            {
                if (serverSequence <= _lastProcessedSequence)
                    return;

                _predictedStates[serverSequence] = serverState;

                while (_pendingCommands.Count > 0)
                {
                    var cmd = _pendingCommands.Peek();
                    if (cmd.Sequence <= serverSequence)
                    {
                        _pendingCommands.Dequeue();
                        _lastProcessedSequence = cmd.Sequence;
                    }
                    else
                    {
                        break;
                    }
                }

                StateReconciled?.Invoke(this, serverSequence);
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pendingCommands.Clear();
                _predictedStates.Clear();
                _lastProcessedSequence = 0;
            }
        }
    }

    public struct PredictionCommand
    {
        public int Sequence { get; set; }
        public int ClientId { get; set; }
        public long Timestamp { get; set; }
        public byte[]? InputData { get; set; }
    }

    #endregion

    #region Latency Simulation

    public class NetworkSimulator
    {
        private readonly Queue<SimulatedPacket> _incomingPackets = new Queue<SimulatedPacket>();
        private readonly Queue<SimulatedPacket> _outgoingPackets = new Queue<SimulatedPacket>();
        private readonly Random _random = new Random();
        private readonly object _lock = new object();
        private float _packetLossPercent;
        private int _minLatencyMs;
        private int _maxLatencyMs;
        private float _jitterMs;
        private int _maxBandwidthBps;
        private long _lastBandwidthCheck;
        private int _bytesSentThisInterval;
        private bool _isEnabled;

        public float PacketLossPercent
        {
            get => _packetLossPercent;
            set => _packetLossPercent = Math.Clamp(value, 0f, 100f);
        }

        public int MinLatencyMs
        {
            get => _minLatencyMs;
            set
            {
                _minLatencyMs = Math.Max(0, value);
                if (_maxLatencyMs < _minLatencyMs) _maxLatencyMs = _minLatencyMs;
            }
        }

        public int MaxLatencyMs
        {
            get => _maxLatencyMs;
            set
            {
                _maxLatencyMs = Math.Max(_minLatencyMs, value);
            }
        }

        public float JitterMs
        {
            get => _jitterMs;
            set => _jitterMs = Math.Max(0, value);
        }

        public int MaxBandwidthBps
        {
            get => _maxBandwidthBps;
            set => _maxBandwidthBps = Math.Max(0, value);
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public event EventHandler<SimulatedPacket>? PacketDropped;
        public event EventHandler<SimulatedPacket>? PacketSent;

        public NetworkSimulator()
        {
            _isEnabled = false;
            _minLatencyMs = 50;
            _maxLatencyMs = 100;
            _jitterMs = 10;
            _maxBandwidthBps = 0;
        }

        public void SimulateOutgoing(byte[] data, int offset, int length, Action<byte[], int, int> sendCallback)
        {
            if (!_isEnabled)
            {
                sendCallback(data, offset, length);
                return;
            }

            if (_random.NextDouble() * 100 < _packetLossPercent)
            {
                var packet = new SimulatedPacket { Data = data, Offset = offset, Length = length };
                PacketDropped?.Invoke(this, packet);
                return;
            }

            int baseLatency = _random.Next(_minLatencyMs, _maxLatencyMs + 1);
            int jitter = (int)((_random.NextDouble() * 2 - 1) * _jitterMs);
            int totalLatency = Math.Max(0, baseLatency + jitter);

            var simulatedPacket = new SimulatedPacket
            {
                Data = data,
                Offset = offset,
                Length = length,
                SendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DeliveryTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + totalLatency
            };

            lock (_lock)
            {
                _outgoingPackets.Enqueue(simulatedPacket);
            }
        }

        public void SimulateIncoming(byte[] data, int offset, int length, Action<byte[], int, int> deliverCallback)
        {
            if (!_isEnabled)
            {
                deliverCallback(data, offset, length);
                return;
            }

            if (_random.NextDouble() * 100 < _packetLossPercent)
            {
                var packet = new SimulatedPacket { Data = data, Offset = offset, Length = length };
                PacketDropped?.Invoke(this, packet);
                return;
            }

            int baseLatency = _random.Next(_minLatencyMs, _maxLatencyMs + 1);
            int jitter = (int)((_random.NextDouble() * 2 - 1) * _jitterMs);
            int totalLatency = Math.Max(0, baseLatency + jitter);

            var simulatedPacket = new SimulatedPacket
            {
                Data = data,
                Offset = offset,
                Length = length,
                SendTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                DeliveryTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + totalLatency
            };

            lock (_lock)
            {
                _incomingPackets.Enqueue(simulatedPacket);
            }
        }

        public void Update(Action<byte[], int, int> deliverCallback)
        {
            if (!_isEnabled)
                return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            lock (_lock)
            {
                while (_incomingPackets.Count > 0 && _incomingPackets.Peek().DeliveryTime <= currentTime)
                {
                    var packet = _incomingPackets.Dequeue();
                    deliverCallback(packet.Data, packet.Offset, packet.Length);
                }
            }
        }

        public void Reset()
        {
            lock (_lock)
            {
                _incomingPackets.Clear();
                _outgoingPackets.Clear();
            }
        }

        public NetworkStatistics GetStatistics()
        {
            lock (_lock)
            {
                return new NetworkStatistics
                {
                    PendingIncomingPackets = _incomingPackets.Count,
                    PendingOutgoingPackets = _outgoingPackets.Count,
                    PacketLossPercent = _packetLossPercent,
                    AverageLatencyMs = (_minLatencyMs + _maxLatencyMs) / 2,
                    JitterMs = _jitterMs
                };
            }
        }
    }

    public class SimulatedPacket
    {
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public int Offset { get; set; }
        public int Length { get; set; }
        public long SendTime { get; set; }
        public long DeliveryTime { get; set; }
    }

    public struct NetworkStatistics
    {
        public int PendingIncomingPackets { get; set; }
        public int PendingOutgoingPackets { get; set; }
        public float PacketLossPercent { get; set; }
        public int AverageLatencyMs { get; set; }
        public float JitterMs { get; set; }
    }

    #endregion

    #region Matchmaking

    public class LobbySystem
    {
        private readonly Dictionary<string, Lobby> _lobbies = new Dictionary<string, Lobby>();
        private readonly object _lock = new object();
        private string? _currentLobbyId;

        public event EventHandler<Lobby>? LobbyCreated;
        public event EventHandler<Lobby>? LobbyJoined;
        public event EventHandler<Lobby>? LobbyLeft;
        public event EventHandler<LobbyMember>? MemberJoined;
        public event EventHandler<LobbyMember>? MemberLeft;
        public event EventHandler<LobbyMember>? MemberReadyChanged;

        public Lobby? CurrentLobby
        {
            get
            {
                lock (_lock)
                {
                    return _currentLobbyId != null && _lobbies.TryGetValue(_currentLobbyId, out var lobby) ? lobby : null;
                }
            }
        }

        public Lobby? CreateLobby(string name, int maxPlayers, bool isPrivate = false)
        {
            lock (_lock)
            {
                var lobby = new Lobby
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = name,
                    MaxPlayers = maxPlayers,
                    IsPrivate = isPrivate,
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                _lobbies[lobby.Id] = lobby;
                _currentLobbyId = lobby.Id;
                LobbyCreated?.Invoke(this, lobby);
                return lobby;
            }
        }

        public Lobby? JoinLobby(string lobbyId, string playerName)
        {
            lock (_lock)
            {
                if (!_lobbies.TryGetValue(lobbyId, out var lobby))
                    return null;

                if (lobby.Members.Count >= lobby.MaxPlayers)
                    return null;

                var member = new LobbyMember
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = playerName,
                    IsReady = false,
                    JoinedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                };

                lobby.Members.Add(member);
                _currentLobbyId = lobbyId;
                LobbyJoined?.Invoke(this, lobby);
                MemberJoined?.Invoke(this, member);
                return lobby;
            }
        }

        public void LeaveLobby()
        {
            lock (_lock)
            {
                if (_currentLobbyId == null)
                    return;

                if (_lobbies.TryGetValue(_currentLobbyId, out var lobby))
                {
                    LobbyLeft?.Invoke(this, lobby);
                }

                _currentLobbyId = null;
            }
        }

        public void SetMemberReady(bool ready)
        {
            lock (_lock)
            {
                if (CurrentLobby == null)
                    return;
            }
        }

        public List<Lobby> GetPublicLobbies()
        {
            lock (_lock)
            {
                return _lobbies.Values.Where(l => !l.IsPrivate).ToList();
            }
        }

        public bool StartGame()
        {
            lock (_lock)
            {
                if (CurrentLobby == null)
                    return false;

                if (CurrentLobby.Members.Any(m => !m.IsReady))
                    return false;

                CurrentLobby.IsStarted = true;
                return true;
            }
        }
    }

    public class Lobby
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public int MaxPlayers { get; set; }
        public bool IsPrivate { get; set; }
        public bool IsStarted { get; set; }
        public long CreatedAt { get; set; }
        public List<LobbyMember> Members { get; } = new List<LobbyMember>();
        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
        public string? HostId { get; set; }
    }

    public class LobbyMember
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public bool IsReady { get; set; }
        public long JoinedAt { get; set; }
        public int LatencyMs { get; set; }
        public Dictionary<string, string> Properties { get; } = new Dictionary<string, string>();
    }

    public interface IMasterServer
    {
        Task<bool> RegisterServerAsync(string address, int port, string gameName, int playerCount);
        Task<bool> UnregisterServerAsync(string serverId);
        Task<List<ServerInfo>> GetServerListAsync();
        Task<ServerInfo?> GetServerAsync(string serverId);
    }

    public class ServerInfo
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public int Port { get; set; }
        public int PlayerCount { get; set; }
        public int MaxPlayers { get; set; }
        public string GameType { get; set; } = "";
        public long LastUpdate { get; set; }
    }

    public class LANDiscovery
    {
        private const int DiscoveryPort = 8888;
        private UdpClient? _udpClient;
        private CancellationTokenSource? _cts;
        private readonly List<DiscoveredServer> _discoveredServers = new List<DiscoveredServer>();
        private readonly object _lock = new object();

        public event EventHandler<DiscoveredServer>? ServerFound;
        public event EventHandler<string>? ServerLost;

        public bool IsRunning { get; private set; }

        public async Task StartAsync()
        {
            if (IsRunning)
                return;

            _cts = new CancellationTokenSource();
            _udpClient = new UdpClient();
            _udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoveryPort));
            _udpClient.EnableBroadcast = true;

            IsRunning = true;
            _ = ListenAsync(_cts.Token);
            _ = BroadcastPresenceAsync(_cts.Token);
        }

        public void Stop()
        {
            if (!IsRunning)
                return;

            _cts?.Cancel();
            _udpClient?.Close();
            _udpClient = null;
            IsRunning = false;

            lock (_lock)
            {
                _discoveredServers.Clear();
            }
        }

        private async Task ListenAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _udpClient != null)
            {
                try
                {
                    var result = await _udpClient.ReceiveAsync(ct);
                    var message = System.Text.Encoding.UTF8.GetString(result.Buffer);

                    if (message.StartsWith("SERVER_ANNOUNCE:"))
                    {
                        var parts = message.Substring("SERVER_ANNOUNCE:".Length).Split(':');
                        if (parts.Length >= 3)
                        {
                            var server = new DiscoveredServer
                            {
                                Id = parts[0],
                                Name = parts[1],
                                Address = result.RemoteEndPoint.Address.ToString(),
                                Port = int.Parse(parts[2])
                            };

                            lock (_lock)
                            {
                                if (!_discoveredServers.Any(s => s.Id == server.Id))
                                {
                                    _discoveredServers.Add(server);
                                    ServerFound?.Invoke(this, server);
                                }
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                }
            }
        }

        private async Task BroadcastPresenceAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested && _udpClient != null)
            {
                try
                {
                    var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, DiscoveryPort);
                    var message = System.Text.Encoding.UTF8.GetBytes("CLIENT_QUERY");
                    await _udpClient.SendAsync(message, message.Length, broadcastEndpoint);
                    await Task.Delay(5000, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                }
            }
        }

        public List<DiscoveredServer> GetDiscoveredServers()
        {
            lock (_lock)
            {
                return _discoveredServers.ToList();
            }
        }
    }

    public class DiscoveredServer
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public string Address { get; set; } = "";
        public int Port { get; set; }
        public long LastSeen { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    }

    #endregion

    #region Chat System

    public enum ChatChannel
    {
        Global,
        Team,
        Whisper,
        Local,
        System
    }

    public class ChatMessage
    {
        public int SenderId { get; set; }
        public string SenderName { get; set; } = "";
        public string Text { get; set; } = "";
        public ChatChannel Channel { get; set; }
        public int? WhisperTargetId { get; set; }
        public long Timestamp { get; set; }
        public bool IsSystem { get; set; }

        public ChatMessage()
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        public ChatMessage(int senderId, string senderName, string text, ChatChannel channel)
        {
            SenderId = senderId;
            SenderName = senderName;
            Text = text;
            Channel = channel;
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }

    public class ChatCommands
    {
        private readonly Dictionary<string, ChatCommand> _commands = new Dictionary<string, ChatCommand>();
        private readonly object _lock = new object();

        public event EventHandler<ChatCommandEventArgs>? CommandExecuted;

        public ChatCommands()
        {
            RegisterDefaultCommands();
        }

        private void RegisterDefaultCommands()
        {
            Register("help", (args, sender) =>
            {
                return "Available commands: /help, /kick, /ban, /mute, /unmute, /whisper, /team, /global";
            });

            Register("whisper", (args, sender) =>
            {
                if (args.Length < 2)
                    return "Usage: /whisper <player> <message>";
                return $"Whispered to {args[0]}: {string.Join(" ", args.Skip(1))}";
            });

            Register("team", (args, sender) =>
            {
                if (args.Length < 1)
                    return "Usage: /team <message>";
                return $"[TEAM] {sender.SenderName}: {string.Join(" ", args)}";
            });
        }

        public void Register(string name, Func<string[], ChatMessage, string> handler)
        {
            lock (_lock)
            {
                _commands[name.ToLower()] = new ChatCommand
                {
                    Name = name,
                    Handler = handler
                };
            }
        }

        public void Unregister(string name)
        {
            lock (_lock)
            {
                _commands.Remove(name.ToLower());
            }
        }

        public string? Execute(string commandLine, ChatMessage sender)
        {
            lock (_lock)
            {
                if (string.IsNullOrEmpty(commandLine))
                    return null;

                if (!commandLine.StartsWith("/"))
                    return null;

                var parts = commandLine.Substring(1).Split(' ');
                var commandName = parts[0].ToLower();
                var args = parts.Skip(1).ToArray();

                if (_commands.TryGetValue(commandName, out var command))
                {
                    var result = command.Handler(args, sender);
                    CommandExecuted?.Invoke(this, new ChatCommandEventArgs(commandName, args, sender, result));
                    return result;
                }
            }

            return $"Unknown command: /{commandName}";
        }

        public bool IsCommand(string text)
        {
            return text.StartsWith("/");
        }

        public List<string> GetCommandNames()
        {
            lock (_lock)
            {
                return _commands.Keys.ToList();
            }
        }
    }

    public class ChatCommand
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Usage { get; set; } = "";
        public Func<string[], ChatMessage, string>? Handler { get; set; }
        public bool RequiresAdmin { get; set; }
    }

    public class ChatCommandEventArgs : EventArgs
    {
        public string CommandName { get; }
        public string[] Args { get; }
        public ChatMessage Sender { get; }
        public string Result { get; }

        public ChatCommandEventArgs(string commandName, string[] args, ChatMessage sender, string result)
        {
            CommandName = commandName;
            Args = args;
            Sender = sender;
            Result = result;
        }
    }

    public class ChatManager
    {
        private static ChatManager? _instance;
        public static ChatManager Instance => _instance ??= new ChatManager();

        private readonly List<ChatMessage> _messageHistory = new List<ChatMessage>();
        private readonly int _maxHistorySize = 100;
        private readonly object _lock = new object();
        private readonly ChatCommands _commands = new ChatCommands();
        private bool _isMuted;

        public event EventHandler<ChatMessage>? MessageReceived;
        public event EventHandler<ChatMessage>? MessageSent;
        public event EventHandler<int>? PlayerMuted;
        public event EventHandler<int>? PlayerUnmuted;

        public ChatCommands Commands => _commands;
        public bool IsMuted
        {
            get => _isMuted;
            set => _isMuted = value;
        }

        private ChatManager()
        {
        }

        public void SendMessage(string text, ChatChannel channel = ChatChannel.Global, int? whisperTargetId = null)
        {
            if (_isMuted)
                return;

            lock (_lock)
            {
                if (_commands.IsCommand(text))
                {
                    var systemMessage = new ChatMessage
                    {
                        SenderId = -1,
                        SenderName = "System",
                        Text = _commands.Execute(text, new ChatMessage { SenderId = -1 }) ?? "",
                        Channel = ChatChannel.System,
                        IsSystem = true
                    };
                    _messageHistory.Add(systemMessage);
                    MessageReceived?.Invoke(this, systemMessage);
                }
                else
                {
                    var message = new ChatMessage
                    {
                        SenderId = NetworkManagerExtended.Instance?.LocalClientId ?? 0,
                        SenderName = "Player",
                        Text = text,
                        Channel = channel,
                        WhisperTargetId = whisperTargetId
                    };

                    _messageHistory.Add(message);
                    MessageSent?.Invoke(this, message);
                    MessageReceived?.Invoke(this, message);
                }

                while (_messageHistory.Count > _maxHistorySize)
                {
                    _messageHistory.RemoveAt(0);
                }
            }
        }

        public List<ChatMessage> GetMessageHistory(int count = 50)
        {
            lock (_lock)
            {
                return _messageHistory.TakeLast(count).ToList();
            }
        }

        public List<ChatMessage> GetMessagesByChannel(ChatChannel channel)
        {
            lock (_lock)
            {
                return _messageHistory.Where(m => m.Channel == channel).ToList();
            }
        }

        public void MutePlayer(int playerId)
        {
            PlayerMuted?.Invoke(this, playerId);
        }

        public void UnmutePlayer(int playerId)
        {
            PlayerUnmuted?.Invoke(this, playerId);
        }

        public void ClearHistory()
        {
            lock (_lock)
            {
                _messageHistory.Clear();
            }
        }
    }

    #endregion

    #region Reliable UDP & Protocols

    public class ReliableUDP
    {
        private readonly Dictionary<uint, PendingPacket> _pendingAcks = new Dictionary<uint, PendingPacket>();
        private readonly Dictionary<uint, ReceivedPacket> _receivedPackets = new Dictionary<uint, ReceivedPacket>();
        private readonly object _lock = new object();
        private uint _sendSequence;
        private uint _receiveSequence;
        private const uint MaxSequence = 0xFFFFFFFF;
        private const int DuplicateWindowSize = 32;

        public event EventHandler<ReliablePacketEventArgs>? PacketDelivered;
        public event EventHandler<ReliablePacketEventArgs>? PacketSent;

        public uint NextSendSequence
        {
            get
            {
                lock (_lock)
                {
                    return _sendSequence++;
                }
            }
        }

        public uint CurrentReceiveSequence
        {
            get
            {
                lock (_lock)
                {
                    return _receiveSequence;
                }
            }
        }

        public bool IsDuplicate(uint sequence)
        {
            lock (_lock)
            {
                uint distance = sequence - _receiveSequence;
                return distance > MaxSequence / 2;
            }
        }

        public bool ProcessIncomingPacket(uint sequence, byte[] data)
        {
            lock (_lock)
            {
                if (_receivedPackets.ContainsKey(sequence))
                    return false;

                uint distance = sequence - _receiveSequence;
                if (distance > MaxSequence / 2)
                    return false;

                _receivedPackets[sequence] = new ReceivedPacket { Sequence = sequence, Data = data, ReceivedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };

                if (sequence > _receiveSequence)
                    _receiveSequence = sequence;

                CleanupOldPackets();

                PacketDelivered?.Invoke(this, new ReliablePacketEventArgs(sequence, data));
                return true;
            }
        }

        public PendingPacket CreateReliablePacket(byte[] data)
        {
            lock (_lock)
            {
                var packet = new PendingPacket
                {
                    Sequence = _sendSequence++,
                    Data = data,
                    SentAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    SendCount = 1
                };
                _pendingAcks[packet.Sequence] = packet;
                return packet;
            }
        }

        public void ProcessAck(uint sequence)
        {
            lock (_lock)
            {
                if (_pendingAcks.Remove(sequence))
                {
                    PacketSent?.Invoke(this, new ReliablePacketEventArgs(sequence, Array.Empty<byte>()));
                }
            }
        }

        public List<PendingPacket> GetUnackedPackets()
        {
            lock (_lock)
            {
                return _pendingAcks.Values.ToList();
            }
        }

        public List<uint> GetPendingAcks()
        {
            lock (_lock)
            {
                return _pendingAcks.Keys.ToList();
            }
        }

        private void CleanupOldPackets()
        {
            long cutoff = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - 30000;
            _receivedPackets.RemoveWhere(kvp => kvp.Value.ReceivedAt < cutoff);
            _pendingAcks.RemoveWhere(kvp => kvp.Value.SentAt < cutoff);
        }
    }

    public class PendingPacket
    {
        public uint Sequence { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public long SentAt { get; set; }
        public int SendCount { get; set; }
        public int MaxSendAttempts { get; set; } = 5;
    }

    public class ReceivedPacket
    {
        public uint Sequence { get; set; }
        public byte[] Data { get; set; } = Array.Empty<byte>();
        public long ReceivedAt { get; set; }
    }

    public class ReliablePacketEventArgs : EventArgs
    {
        public uint Sequence { get; }
        public byte[] Data { get; }

        public ReliablePacketEventArgs(uint sequence, byte[] data)
        {
            Sequence = sequence;
            Data = data;
        }
    }

    public class HeartbeatManager
    {
        private long _lastHeartbeatSent;
        private long _lastHeartbeatReceived;
        private int _heartbeatIntervalMs = 1000;
        private int _missedHeartbeats;
        private int _maxMissedHeartbeats = 5;
        private bool _isConnected;
        private readonly object _lock = new object();

        public event EventHandler? HeartbeatMissed;
        public event EventHandler? ConnectionLost;

        public int HeartbeatInterval
        {
            get => _heartbeatIntervalMs;
            set => _heartbeatIntervalMs = Math.Max(100, value);
        }

        public int MissedHeartbeats => _missedHeartbeats;
        public bool IsConnected => _isConnected;

        public void SetConnected(bool connected)
        {
            lock (_lock)
            {
                _isConnected = connected;
                if (connected)
                {
                    _missedHeartbeats = 0;
                }
            }
        }

        public bool ShouldSendHeartbeat()
        {
            lock (_lock)
            {
                long elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastHeartbeatSent;
                return elapsed >= _heartbeatIntervalMs;
            }
        }

        public void RecordHeartbeatSent()
        {
            lock (_lock)
            {
                _lastHeartbeatSent = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
        }

        public void RecordHeartbeatReceived()
        {
            lock (_lock)
            {
                _lastHeartbeatReceived = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                _missedHeartbeats = 0;
            }
        }

        public void CheckHeartbeatTimeout()
        {
            lock (_lock)
            {
                long elapsed = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastHeartbeatReceived;
                if (elapsed > _heartbeatIntervalMs * _maxMissedHeartbeats)
                {
                    _isConnected = false;
                    ConnectionLost?.Invoke(this, EventArgs.Empty);
                }
                else if (elapsed > _heartbeatIntervalMs)
                {
                    _missedHeartbeats++;
                    HeartbeatMissed?.Invoke(this, EventArgs.Empty);
                }
            }
        }
    }

    public class ConnectionApproval
    {
        private readonly Dictionary<string, ConnectionApprovalToken> _pendingTokens = new Dictionary<string, ConnectionApprovalToken>();
        private readonly object _lock = new object();
        private int _timeoutSeconds = 30;

        public event EventHandler<ConnectionApprovalRequest>? ApprovalRequested;
        public event EventHandler<ConnectionApprovalResult>? ApprovalCompleted;

        public int TimeoutSeconds
        {
            get => _timeoutSeconds;
            set => _timeoutSeconds = Math.Max(5, value);
        }

        public string CreateToken(int clientId, string clientName, Dictionary<string, string>? customData = null)
        {
            lock (_lock)
            {
                var token = new ConnectionApprovalToken
                {
                    TokenId = Guid.NewGuid().ToString(),
                    ClientId = clientId,
                    ClientName = clientName,
                    CustomData = customData ?? new Dictionary<string, string>(),
                    CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                    ExpiresAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + (_timeoutSeconds * 1000)
                };

                _pendingTokens[token.TokenId] = token;
                ApprovalRequested?.Invoke(this, new ConnectionApprovalRequest(clientId, clientName, customData));
                return token.TokenId;
            }
        }

        public bool Approve(string tokenId, string? reason = null)
        {
            lock (_lock)
            {
                if (_pendingTokens.TryGetValue(tokenId, out var token))
                {
                    token.IsApproved = true;
                    token.Reason = reason;
                    token.ProcessedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _pendingTokens.Remove(tokenId);
                    ApprovalCompleted?.Invoke(this, new ConnectionApprovalResult(token.ClientId, true, reason));
                    return true;
                }
            }
            return false;
        }

        public bool Reject(string tokenId, string reason)
        {
            lock (_lock)
            {
                if (_pendingTokens.TryGetValue(tokenId, out var token))
                {
                    token.IsApproved = false;
                    token.Reason = reason;
                    token.ProcessedAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    _pendingTokens.Remove(tokenId);
                    ApprovalCompleted?.Invoke(this, new ConnectionApprovalResult(token.ClientId, false, reason));
                    return true;
                }
            }
            return false;
        }

        public bool ValidateToken(string tokenId)
        {
            lock (_lock)
            {
                if (_pendingTokens.TryGetValue(tokenId, out var token))
                {
                    return token.ExpiresAt > DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
            return false;
        }

        public void CleanupExpiredTokens()
        {
            lock (_lock)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                var expired = _pendingTokens.Where(kvp => kvp.Value.ExpiresAt <= now).Select(kvp => kvp.Key).ToList();
                foreach (var tokenId in expired)
                {
                    _pendingTokens.Remove(tokenId);
                }
            }
        }
    }

    public class ConnectionApprovalToken
    {
        public string TokenId { get; set; } = "";
        public int ClientId { get; set; }
        public string ClientName { get; set; } = "";
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
        public long CreatedAt { get; set; }
        public long ExpiresAt { get; set; }
        public bool IsApproved { get; set; }
        public string? Reason { get; set; }
        public long ProcessedAt { get; set; }
    }

    public class ConnectionApprovalRequest : EventArgs
    {
        public int ClientId { get; }
        public string ClientName { get; }
        public Dictionary<string, string>? CustomData { get; }

        public ConnectionApprovalRequest(int clientId, string clientName, Dictionary<string, string>? customData)
        {
            ClientId = clientId;
            ClientName = clientName;
            CustomData = customData;
        }
    }

    public class ConnectionApprovalResult : EventArgs
    {
        public int ClientId { get; }
        public bool Approved { get; }
        public string? Reason { get; }

        public ConnectionApprovalResult(int clientId, bool approved, string? reason)
        {
            ClientId = clientId;
            Approved = approved;
            Reason = reason;
        }
    }

    #endregion

    #region Main Extended Network Manager

    public class NetworkManagerExtended : IDisposable
    {
        private static NetworkManagerExtended? _instance;
        public static NetworkManagerExtended Instance => _instance ??= new NetworkManagerExtended();

        private ConnectionConfig _config = new ConnectionConfig();
        private ConnectionState _state = ConnectionState.Disconnected;
        private int _localClientId = -1;
        private string _localPlayerName = "Player";
        private NetworkServer _server;
        private NetworkSpawner _spawner;
        private NetworkSimulator _simulator;
        private ReliableUDP _reliableUdp;
        private HeartbeatManager _heartbeat;
        private ConnectionApproval _approval;
        private LobbySystem _lobbySystem;
        private ChatManager _chatManager;
        private RpcHandler _rpcHandler;
        private UdpClient? _udpListener;
        private TcpListener? _tcpListener;
        private TcpClient? _tcpClient;
        private NetworkStream? _tcpStream;
        private CancellationTokenSource? _cts;
        private Task? _listenerTask;
        private Task? _heartbeatTask;
        private readonly object _lock = new object();
        private bool _isDisposed;

        public ConnectionConfig Config => _config;
        public ConnectionState State => _state;
        public int LocalClientId => _localClientId;
        public string LocalPlayerName
        {
            get => _localPlayerName;
            set => _localPlayerName = value ?? "Player";
        }
        public bool IsServer => _config.Topology == NetworkTopology.ClientServer && _state == ConnectionState.Ready;
        public bool IsClient => _config.Topology == NetworkTopology.ClientServer && _state == ConnectionState.Ready && !IsServer;
        public bool IsRunning => _state != ConnectionState.Disconnected && _state != ConnectionState.Disconnecting;

        public NetworkServer Server => _server;
        public NetworkSpawner Spawner => _spawner;
        public NetworkSimulator Simulator => _simulator;
        public LobbySystem LobbySystem => _lobbySystem;
        public ChatManager Chat => _chatManager;
        public RpcHandler Rpc => _rpcHandler;

        public event EventHandler<ConnectionState>? StateChanged;
        public event EventHandler<int>? ClientConnected;
        public event EventHandler<int>? ClientDisconnected;
        public event EventHandler<NetworkMessageBuffer>? MessageReceived;
        public event EventHandler? Connected;
        public event EventHandler<string>? Disconnected;

        private NetworkManagerExtended()
        {
            _server = new NetworkServer(_config.MaxConnections);
            _spawner = new NetworkSpawner();
            _simulator = new NetworkSimulator();
            _reliableUdp = new ReliableUDP();
            _heartbeat = new HeartbeatManager();
            _approval = new ConnectionApproval();
            _lobbySystem = new LobbySystem();
            _chatManager = ChatManager.Instance;
            _rpcHandler = new RpcHandler();
            NetworkSpawner.Instance = _spawner;

            _server.ClientConnected += (s, c) => ClientConnected?.Invoke(this, c.Id);
            _server.ClientDisconnected += (s, c) => ClientDisconnected?.Invoke(this, c.Id);
        }

        public void Configure(ConnectionConfig config)
        {
            _config = config;
            _server = new NetworkServer(config.MaxConnections);
        }

        public async Task StartServerAsync(int port = 7777)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(NetworkManagerExtended));

            SetState(ConnectionState.Connecting);

            try
            {
                _config.TcpPort = port;
                _config.UdpPort = port + 1;

                _tcpListener = new TcpListener(IPAddress.Any, _config.TcpPort);
                _tcpListener.Start();

                _udpListener = new UdpClient(_config.UdpPort);

                _cts = new CancellationTokenSource();
                _listenerTask = Task.Run(() => ServerListenerLoop(_cts.Token), _cts.Token);
                _heartbeatTask = Task.Run(() => HeartbeatLoop(_cts.Token), _cts.Token);

                _localClientId = 0;
                _server.Start();
                SetState(ConnectionState.Ready);
                Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                SetState(ConnectionState.Disconnected);
                throw new InvalidOperationException("Failed to start server", ex);
            }
        }

        public async Task ConnectAsync(string address, int port)
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(NetworkManagerExtended));

            SetState(ConnectionState.Connecting);

            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(address, port);
                _tcpStream = _tcpClient.GetStream();

                _localClientId = 1;
                SetState(ConnectionState.Connected);

                _cts = new CancellationTokenSource();
                _listenerTask = Task.Run(() => ClientListenerLoop(_cts.Token), _cts.Token);
                _heartbeatTask = Task.Run(() => HeartbeatLoop(_cts.Token), _cts.Token);

                await SendHandshakeAsync();
                SetState(ConnectionState.Ready);
                Connected?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                SetState(ConnectionState.Disconnected);
                throw new InvalidOperationException("Failed to connect", ex);
            }
        }

        public void Disconnect(string reason = "")
        {
            if (_isDisposed)
                return;

            SetState(ConnectionState.Disconnecting);

            _cts?.Cancel();
            _tcpListener?.Stop();
            _tcpClient?.Close();
            _udpListener?.Close();

            _listenerTask = null;
            _heartbeatTask = null;

            SetState(ConnectionState.Disconnected);
            Disconnected?.Invoke(this, reason);
        }

        private async Task SendHandshakeAsync()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write((ushort)0);
            writer.Write(_localClientId);
            writer.Write(_localPlayerName);
            writer.Write(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            if (_tcpStream != null)
            {
                await _tcpStream.WriteAsync(stream.ToArray());
            }
        }

        private async Task ServerListenerLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_tcpListener != null && _tcpListener.Pending())
                    {
                        var client = await _tcpListener.AcceptTcpClientAsync(ct);
                        _ = HandleClientConnectionAsync(client, ct);
                    }

                    if (_udpListener != null && _udpListener.Available > 0)
                    {
                        var result = await _udpListener.ReceiveAsync(ct);
                        ProcessUdpMessage(result.Buffer, result.RemoteEndPoint);
                    }

                    await Task.Delay(10, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                }
            }
        }

        private async Task HandleClientConnectionAsync(TcpClient client, CancellationToken ct)
        {
            var stream = client.GetStream();
            var buffer = new byte[65536];

            try
            {
                while (!ct.IsCancellationRequested && client.Connected)
                {
                    if (stream.DataAvailable)
                    {
                        int bytesRead = await stream.ReadAsync(buffer, ct);
                        if (bytesRead > 0)
                        {
                            var data = new byte[bytesRead];
                            Array.Copy(buffer, data, bytesRead);
                            ProcessTcpMessage(data);
                        }
                    }
                    await Task.Delay(10, ct);
                }
            }
            catch
            {
            }
            finally
            {
                client.Close();
            }
        }

        private async Task ClientListenerLoop(CancellationToken ct)
        {
            var buffer = new byte[65536];

            while (!ct.IsCancellationRequested && _tcpClient?.Connected == true)
            {
                try
                {
                    if (_tcpStream != null && _tcpStream.DataAvailable)
                    {
                        int bytesRead = await _tcpStream.ReadAsync(buffer, ct);
                        if (bytesRead > 0)
                        {
                            var data = new byte[bytesRead];
                            Array.Copy(buffer, data, bytesRead);
                            ProcessTcpMessage(data);
                        }
                    }
                    await Task.Delay(10, ct);
                }
                catch
                {
                    break;
                }
            }
        }

        private async Task HeartbeatLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    if (_heartbeat.ShouldSendHeartbeat())
                    {
                        await SendHeartbeatAsync();
                        _heartbeat.RecordHeartbeatSent();
                    }

                    _heartbeat.CheckHeartbeatTimeout();

                    await Task.Delay(_heartbeat.HeartbeatInterval, ct);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch
                {
                }
            }
        }

        private async Task SendHeartbeatAsync()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write((ushort)1);
            writer.Write(_localClientId);
            writer.Write(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());

            if (_tcpStream != null && IsClient)
            {
                await _tcpStream.WriteAsync(stream.ToArray());
            }
        }

        private void ProcessTcpMessage(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            ushort messageType = reader.ReadUInt16();

            switch (messageType)
            {
                case 0:
                    int clientId = reader.ReadInt32();
                    string clientName = reader.ReadString();
                    var client = _server.AddClient(null);
                    client.SetName(clientName);
                    break;
                case 1:
                    _heartbeat.RecordHeartbeatReceived();
                    break;
                default:
                    var msg = MessageSerializer.Deserialize(data);
                    if (msg != null)
                    {
                        MessageReceived?.Invoke(this, new NetworkMessageBuffer
                        {
                            TypeId = messageType,
                            Payload = data
                        });
                    }
                    break;
            }
        }

        private void ProcessUdpMessage(byte[] data, IPEndPoint sender)
        {
            _reliableUdp.ProcessIncomingPacket(BitConverter.ToUInt32(data, 0), data);
        }

        public async Task SendMessageAsync(NetworkMessageBuffer message)
        {
            if (_isDisposed)
                return;

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            message.Serialize(writer);
            var data = stream.ToArray();

            if (_tcpStream != null && IsClient)
            {
                await _tcpStream.WriteAsync(data);
            }
            else if (_tcpListener != null && IsServer)
            {
            }
        }

        public void BroadcastMessage(NetworkMessageBuffer message, int? excludeClientId = null)
        {
            if (_isDisposed || !IsServer)
                return;

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            message.Serialize(writer);
        }

        private void SetState(ConnectionState newState)
        {
            if (_state != newState)
            {
                _state = newState;
                StateChanged?.Invoke(this, newState);
            }
        }

        public void Dispose()
        {
            if (_isDisposed)
                return;

            _isDisposed = true;
            Disconnect();
            _instance = null;
        }
    }

    #endregion
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using WPFEngine.Math;

namespace WPFEngine.Network
{
    public enum NetworkMessageType : byte
    {
        Handshake = 1,
        HandshakeAck = 2,
        Disconnect = 3,
        Heartbeat = 4,
        ObjectSpawn = 10,
        ObjectDestroy = 11,
        ObjectUpdate = 12,
        PlayerInput = 20,
        ChatMessage = 30,
        GameState = 40,
        Custom = 100
    }

    public interface INetworkMessage
    {
        NetworkMessageType Type { get; }
        void Serialize(BinaryWriter writer);
        void Deserialize(BinaryReader reader);
    }

    public struct NetworkMessage : INetworkMessage
    {
        public NetworkMessageType Type { get; set; }
        public byte[]? Payload { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write((byte)Type);
            writer.Write(Payload?.Length ?? 0);
            if (Payload != null && Payload.Length > 0)
            {
                writer.Write(Payload);
            }
        }

        public void Deserialize(BinaryReader reader)
        {
            Type = (NetworkMessageType)reader.ReadByte();
            int length = reader.ReadInt32();
            if (length > 0)
            {
                Payload = reader.ReadBytes(length);
            }
        }
    }

    public class HandshakeMessage : INetworkMessage
    {
        public NetworkMessageType Type => NetworkMessageType.Handshake;
        public string ClientName { get; set; } = "";
        public string GameVersion { get; set; } = "1.0";
        public long Timestamp { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ClientName);
            writer.Write(GameVersion);
            writer.Write(Timestamp);
        }

        public void Deserialize(BinaryReader reader)
        {
            ClientName = reader.ReadString();
            GameVersion = reader.ReadString();
            Timestamp = reader.ReadInt64();
        }
    }

    public class PlayerInputMessage : INetworkMessage
    {
        public NetworkMessageType Type => NetworkMessageType.PlayerInput;
        public int PlayerId { get; set; }
        public float Horizontal { get; set; }
        public float Vertical { get; set; }
        public bool Jump { get; set; }
        public bool Fire { get; set; }
        public long Timestamp { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(PlayerId);
            writer.Write(Horizontal);
            writer.Write(Vertical);
            writer.Write(Jump);
            writer.Write(Fire);
            writer.Write(Timestamp);
        }

        public void Deserialize(BinaryReader reader)
        {
            PlayerId = reader.ReadInt32();
            Horizontal = reader.ReadSingle();
            Vertical = reader.ReadSingle();
            Jump = reader.ReadBoolean();
            Fire = reader.ReadBoolean();
            Timestamp = reader.ReadInt64();
        }
    }

    public class ObjectUpdateMessage : INetworkMessage
    {
        public NetworkMessageType Type => NetworkMessageType.ObjectUpdate;
        public int ObjectId { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public Vector2 Velocity { get; set; }
        public float AngularVelocity { get; set; }
        public byte[]? ExtraData { get; set; }
        public long Timestamp { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ObjectId);
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Rotation);
            writer.Write(Velocity.X);
            writer.Write(Velocity.Y);
            writer.Write(AngularVelocity);
            writer.Write(ExtraData?.Length ?? 0);
            if (ExtraData != null && ExtraData.Length > 0)
            {
                writer.Write(ExtraData);
            }
            writer.Write(Timestamp);
        }

        public void Deserialize(BinaryReader reader)
        {
            ObjectId = reader.ReadInt32();
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Rotation = reader.ReadSingle();
            Velocity = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            AngularVelocity = reader.ReadSingle();
            int extraLength = reader.ReadInt32();
            if (extraLength > 0)
            {
                ExtraData = reader.ReadBytes(extraLength);
            }
            Timestamp = reader.ReadInt64();
        }
    }

    public class NetworkPeer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public IPEndPoint? EndPoint { get; set; }
        public bool IsConnected { get; set; }
        public long LastHeartbeat { get; set; }
        public Vector2 Position { get; set; }
        public Dictionary<string, byte[]> CustomData { get; set; } = new Dictionary<string, byte[]>();
    }

    public abstract class NetworkTransport
    {
        public abstract void Connect(string address, int port);
        public abstract void Disconnect();
        public abstract void Send(byte[] data, int offset, int length);
        public abstract void Broadcast(byte[] data, int offset, int length);
        public abstract bool Receive(out byte[] data, out IPEndPoint? sender);
        public abstract bool IsConnected { get; }
        public abstract IPEndPoint? LocalEndPoint { get; }
    }

    public class UDPTransport : NetworkTransport
    {
        private UdpClient? _client;
        private IPEndPoint? _remoteEndpoint;
        private bool _isConnected;

        public override bool IsConnected => _isConnected;
        public override IPEndPoint? LocalEndPoint => _client?.Client.LocalEndPoint as IPEndPoint;

        public override void Connect(string address, int port)
        {
            _client = new UdpClient();
            _remoteEndpoint = new IPEndPoint(IPAddress.Parse(address), port);
            _isConnected = true;
        }

        public override void Disconnect()
        {
            _isConnected = false;
            _client?.Close();
            _client = null;
        }

        public override void Send(byte[] data, int offset, int length)
        {
            if (_client == null || _remoteEndpoint == null)
                return;

            _client.Send(data, length, _remoteEndpoint);
        }

        public override void Broadcast(byte[] data, int offset, int length)
        {
            Send(data, offset, length);
        }

        public override bool Receive(out byte[] data, out IPEndPoint? sender)
        {
            data = Array.Empty<byte>();
            sender = null;

            if (_client == null)
                return false;

            try
            {
                var result = _client.Receive(ref sender!);
                data = result;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }

    public class TCPTransport : NetworkTransport
    {
        private TcpClient? _client;
        private NetworkStream? _stream;
        private readonly byte[] _receiveBuffer = new byte[65536];
        private int _receiveOffset;
        private int _receiveLength;

        public override bool IsConnected => _client?.Connected ?? false;
        public override IPEndPoint? LocalEndPoint => _client?.Client.LocalEndPoint as IPEndPoint;

        public override void Connect(string address, int port)
        {
            _client = new TcpClient();
            _client.Connect(address, port);
            _stream = _client.GetStream();
            _receiveOffset = 0;
            _receiveLength = 0;
        }

        public override void Disconnect()
        {
            _stream?.Close();
            _client?.Close();
            _stream = null;
            _client = null;
        }

        public override void Send(byte[] data, int offset, int length)
        {
            if (_stream == null)
                return;

            _stream.Write(data, offset, length);
        }

        public override void Broadcast(byte[] data, int offset, int length)
        {
            Send(data, offset, length);
        }

        public override bool Receive(out byte[] data, out IPEndPoint? sender)
        {
            data = Array.Empty<byte>();
            sender = null;

            if (_stream == null)
                return false;

            try
            {
                if (_stream.DataAvailable)
                {
                    int bytesRead = _stream.Read(_receiveBuffer, _receiveOffset, _receiveBuffer.Length - _receiveOffset);
                    if (bytesRead > 0)
                    {
                        _receiveLength += bytesRead;
                        data = _receiveBuffer.Take(_receiveLength).ToArray();
                        return true;
                    }
                }
            }
            catch
            {
            }

            return false;
        }
    }

    public class NetworkManager
    {
        private static NetworkManager? _instance;
        public static NetworkManager Instance => _instance ??= new NetworkManager();

        private NetworkTransport? _transport;
        private readonly Dictionary<int, NetworkPeer> _peers;
        private readonly List<INetworkMessage> _incomingMessages;
        private readonly List<INetworkMessage> _outgoingMessages;
        private readonly object _messageLock = new object();
        private int _localPeerId;
        private string _localPeerName;
        private bool _isServer;
        private bool _isRunning;
        private long _lastHeartbeat;
        private int _heartbeatIntervalMs;
        private int _connectionTimeoutMs;
        private CancellationTokenSource? _runningCts;

        public int LocalPeerId => _localPeerId;
        public string LocalPeerName => _localPeerName;
        public bool IsServer => _isServer;
        public bool IsRunning => _isRunning;
        public IReadOnlyDictionary<int, NetworkPeer> Peers => _peers;
        public int HeartbeatInterval
        {
            get => _heartbeatIntervalMs;
            set => _heartbeatIntervalMs = Math.Max(100, value);
        }
        public int ConnectionTimeout
        {
            get => _connectionTimeoutMs;
            set => _connectionTimeoutMs = Math.Max(1000, value);
        }

        public event EventHandler<int>? PeerConnected;
        public event EventHandler<int>? PeerDisconnected;
        public event EventHandler<INetworkMessage>? MessageReceived;
        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        private NetworkManager()
        {
            _peers = new Dictionary<int, NetworkPeer>();
            _incomingMessages = new List<INetworkMessage>();
            _outgoingMessages = new List<INetworkMessage>();
            _localPeerId = -1;
            _localPeerName = "Player";
            _isServer = false;
            _isRunning = false;
            _heartbeatIntervalMs = 1000;
            _connectionTimeoutMs = 5000;
        }

        public void StartServer(int port = 7777)
        {
            _transport = new UDPTransport();
            _isServer = true;
            _isRunning = true;
            _runningCts = new CancellationTokenSource();

            Task.Run(() => ServerLoop(_runningCts.Token));

            _localPeerId = 0;
            _peers[_localPeerId] = new NetworkPeer
            {
                Id = _localPeerId,
                Name = _localPeerName,
                IsConnected = true,
                LastHeartbeat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };

            Connected?.Invoke(this, EventArgs.Empty);
        }

        public void Connect(string address, int port)
        {
            _transport = new UDPTransport();
            ((UDPTransport)_transport).Connect(address, port);
            _isServer = false;
            _isRunning = true;
            _runningCts = new CancellationTokenSource();

            Task.Run(() => ClientLoop(_runningCts.Token));

            SendMessage(new HandshakeMessage
            {
                ClientName = _localPeerName,
                GameVersion = "1.0",
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            });

            Connected?.Invoke(this, EventArgs.Empty);
        }

        public void Disconnect()
        {
            if (!_isRunning)
                return;

            if (_transport?.IsConnected == true)
            {
                SendMessage(new NetworkMessage { Type = NetworkMessageType.Disconnect });
            }

            _runningCts?.Cancel();
            _transport?.Disconnect();
            _isRunning = false;

            _peers.Clear();
            _incomingMessages.Clear();
            _outgoingMessages.Clear();

            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        public void SendMessage(INetworkMessage message)
        {
            lock (_messageLock)
            {
                _outgoingMessages.Add(message);
            }
        }

        public void SendToPeer(int peerId, INetworkMessage message)
        {
            SendMessage(message);
        }

        public void BroadcastMessage(INetworkMessage message)
        {
            SendMessage(message);
        }

        public List<INetworkMessage> GetIncomingMessages()
        {
            lock (_messageLock)
            {
                var messages = new List<INetworkMessage>(_incomingMessages);
                _incomingMessages.Clear();
                return messages;
            }
        }

        private async Task ServerLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                while (_transport?.Receive(out var data, out var sender) == true)
                {
                    ProcessIncomingData(data, sender);
                }

                SendQueuedMessages();

                CheckHeartbeats();

                await Task.Delay(10);
            }
        }

        private async Task ClientLoop(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                while (_transport?.Receive(out var data, out _) == true)
                {
                    ProcessIncomingData(data, null);
                }

                SendQueuedMessages();

                if (DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - _lastHeartbeat > _heartbeatIntervalMs)
                {
                    SendHeartbeat();
                }

                await Task.Delay(10);
            }
        }

        private void ProcessIncomingData(byte[] data, IPEndPoint? sender)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            var message = new NetworkMessage();
            message.Deserialize(reader);

            switch (message.Type)
            {
                case NetworkMessageType.Handshake:
                    var handshake = new HandshakeMessage();
                    if (message.Payload != null && message.Payload.Length > 0)
                    {
                        using var payloadStream = new MemoryStream(message.Payload);
                        handshake.Deserialize(new BinaryReader(payloadStream));
                    }

                    int newPeerId = _peers.Count;
                    _peers[newPeerId] = new NetworkPeer
                    {
                        Id = newPeerId,
                        Name = handshake.ClientName,
                        EndPoint = sender,
                        IsConnected = true,
                        LastHeartbeat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };

                    SendToPeer(newPeerId, new NetworkMessage { Type = NetworkMessageType.HandshakeAck });
                    PeerConnected?.Invoke(this, newPeerId);
                    break;

                case NetworkMessageType.HandshakeAck:
                    _localPeerId = 0;
                    _isRunning = true;
                    break;

                case NetworkMessageType.Heartbeat:
                    if (sender != null)
                    {
                        var peer = _peers.Values.FirstOrDefault(p => p.EndPoint?.Equals(sender) == true);
                        if (peer != null)
                        {
                            peer.LastHeartbeat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                        }
                    }
                    break;

                case NetworkMessageType.Disconnect:
                    if (sender != null)
                    {
                        var peer = _peers.Values.FirstOrDefault(p => p.EndPoint?.Equals(sender) == true);
                        if (peer != null)
                        {
                            _peers.Remove(peer.Id);
                            PeerDisconnected?.Invoke(this, peer.Id);
                        }
                    }
                    break;

                default:
                    lock (_messageLock)
                    {
                        _incomingMessages.Add(message);
                    }
                    MessageReceived?.Invoke(this, message);
                    break;
            }
        }

        private void SendQueuedMessages()
        {
            lock (_messageLock)
            {
                foreach (var message in _outgoingMessages)
                {
                    using var stream = new MemoryStream();
                    using var writer = new BinaryWriter(stream);

                    var netMessage = new NetworkMessage { Type = message.Type };
                    using var payloadStream = new MemoryStream();
                    using var payloadWriter = new BinaryWriter(payloadStream);
                    message.Serialize(payloadWriter);
                    netMessage.Payload = payloadStream.ToArray();

                    netMessage.Serialize(writer);

                    _transport?.Send(stream.ToArray(), 0, (int)stream.Length);
                }

                _outgoingMessages.Clear();
            }
        }

        private void SendHeartbeat()
        {
            SendMessage(new NetworkMessage { Type = NetworkMessageType.Heartbeat });
            _lastHeartbeat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }

        private void CheckHeartbeats()
        {
            if (!_isServer)
                return;

            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var disconnectedPeers = _peers.Values
                .Where(p => currentTime - p.LastHeartbeat > _connectionTimeoutMs)
                .ToList();

            foreach (var peer in disconnectedPeers)
            {
                _peers.Remove(peer.Id);
                PeerDisconnected?.Invoke(this, peer.Id);
            }
        }

        public NetworkPeer? GetPeer(int id)
        {
            return _peers.TryGetValue(id, out var peer) ? peer : null;
        }

        public List<NetworkPeer> GetAllPeers()
        {
            return _peers.Values.ToList();
        }
    }

    public class RPCManager
    {
        private static RPCManager? _instance;
        public static RPCManager Instance => _instance ??= new RPCManager();

        private readonly Dictionary<string, MethodInfo> _registeredMethods;
        private readonly object _lock = new object();

        private RPCManager()
        {
            _registeredMethods = new Dictionary<string, MethodInfo>();
        }

        public void RegisterMethod(string name, MethodInfo method)
        {
            lock (_lock)
            {
                _registeredMethods[name] = method;
            }
        }

        public void UnregisterMethod(string name)
        {
            lock (_lock)
            {
                _registeredMethods.Remove(name);
            }
        }

        public void CallRPC(string methodName, params object[] args)
        {
            lock (_lock)
            {
                if (_registeredMethods.TryGetValue(methodName, out var method))
                {
                    method.Invoke(null, args);
                }
            }

            var message = new RPCMessage
            {
                MethodName = methodName,
                Arguments = SerializeArguments(args)
            };

            NetworkManager.Instance.SendMessage(message);
        }

        public void HandleRPCMessage(RPCMessage message)
        {
            lock (_lock)
            {
                if (_registeredMethods.TryGetValue(message.MethodName, out var method))
                {
                    var args = DeserializeArguments(message.Arguments);
                    method.Invoke(null, args);
                }
            }
        }

        private byte[] SerializeArguments(object[] args)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(args.Length);
            foreach (var arg in args)
            {
                if (arg is int i)
                    writer.Write(i);
                else if (arg is float f)
                    writer.Write(f);
                else if (arg is string s)
                    writer.Write(s);
                else if (arg is bool b)
                    writer.Write(b);
                else if (arg is Vector2 v2)
                {
                    writer.Write((byte)1);
                    writer.Write(v2.X);
                    writer.Write(v2.Y);
                }
                else if (arg is Vector3 v3)
                {
                    writer.Write((byte)2);
                    writer.Write(v3.X);
                    writer.Write(v3.Y);
                    writer.Write(v3.Z);
                }
            }

            return stream.ToArray();
        }

        private object[] DeserializeArguments(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            int count = reader.ReadInt32();
            var args = new object[count];

            for (int i = 0; i < count; i++)
            {
                byte type = reader.ReadByte();
                switch (type)
                {
                    case 0:
                        args[i] = reader.ReadInt32();
                        break;
                    case 1:
                        args[i] = reader.ReadSingle();
                        break;
                    case 2:
                        args[i] = reader.ReadString();
                        break;
                    case 3:
                        args[i] = reader.ReadBoolean();
                        break;
                    case 4:
                        args[i] = new Vector2(reader.ReadSingle(), reader.ReadSingle());
                        break;
                    case 5:
                        args[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                        break;
                }
            }

            return args;
        }
    }

    public class RPCMessage : INetworkMessage
    {
        public NetworkMessageType Type => NetworkMessageType.Custom;
        public string MethodName { get; set; } = "";
        public byte[] Arguments { get; set; } = Array.Empty<byte>();

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(MethodName);
            writer.Write(Arguments.Length);
            writer.Write(Arguments);
        }

        public void Deserialize(BinaryReader reader)
        {
            MethodName = reader.ReadString();
            int length = reader.ReadInt32();
            Arguments = reader.ReadBytes(length);
        }
    }

    public class SyncVar<T>
    {
        private T _value;
        private T _previousValue;
        private bool _isDirty;
        private readonly string _name;
        private readonly int _objectId;

        public T Value
        {
            get => _value;
            set
            {
                if (!Equals(_value, value))
                {
                    _previousValue = _value;
                    _value = value;
                    _isDirty = true;
                }
            }
        }

        public SyncVar(string name, int objectId, T initialValue = default!)
        {
            _name = name;
            _objectId = objectId;
            _value = initialValue;
            _previousValue = initialValue;
            _isDirty = false;
        }

        public bool IsDirty => _isDirty;

        public void ClearDirty()
        {
            _isDirty = false;
            _previousValue = _value;
        }

        public byte[] Serialize()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(_objectId);
            writer.Write(_name);
            writer.Write(_previousValue?.ToString() ?? "");
            writer.Write(_value?.ToString() ?? "");

            return stream.ToArray();
        }

        public void Deserialize(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            reader.ReadInt32();
            reader.ReadString();
            reader.ReadString();
            string newValue = reader.ReadString();
        }
    }

    public class NetworkObject
    {
        public int Id { get; set; }
        public int OwnerId { get; set; }
        public bool IsOwner { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }
        public List<SyncVar<object>> SyncVars { get; } = new List<SyncVar<object>>();

        public event EventHandler? OnSpawn;
        public event EventHandler? OnDestroy;
        public event EventHandler? OnOwnershipChanged;

        public virtual void OnNetworkSpawn()
        {
            OnSpawn?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnNetworkDestroy()
        {
            OnDestroy?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnOwnershipChange(int newOwnerId)
        {
            OwnerId = newOwnerId;
            IsOwner = newOwnerId == NetworkManager.Instance.LocalPeerId;
            OnOwnershipChanged?.Invoke(this, EventArgs.Empty);
        }

        public void AddSyncVar(string name, object initialValue)
        {
            var syncVar = new SyncVar<object>(name, Id, initialValue);
            SyncVars.Add(syncVar);
        }

        public SyncVar<T> GetSyncVar<T>(string name)
        {
            return SyncVars.FirstOrDefault(sv => sv is SyncVar<T>) as SyncVar<T>;
        }
    }

    public class NetworkManager_Extensions
    {
        private readonly Dictionary<int, NetworkObject> _networkObjects = new Dictionary<int, NetworkObject>();

        public event EventHandler<NetworkObject>? ObjectSpawned;
        public event EventHandler<NetworkObject>? ObjectDestroyed;

        public void SpawnObject(NetworkObject obj, int ownerId = -1)
        {
            obj.Id = _networkObjects.Count;
            obj.OwnerId = ownerId;
            obj.IsOwner = ownerId == NetworkManager.Instance.LocalPeerId;

            _networkObjects[obj.Id] = obj;
            obj.OnNetworkSpawn();

            ObjectSpawned?.Invoke(this, obj);

            if (NetworkManager.Instance.IsServer)
            {
                var message = new ObjectSpawnMessage
                {
                    ObjectId = obj.Id,
                    OwnerId = ownerId,
                    Position = obj.Position,
                    Rotation = obj.Rotation
                };
                NetworkManager.Instance.BroadcastMessage(message);
            }
        }

        public void DestroyObject(int objectId)
        {
            if (_networkObjects.TryGetValue(objectId, out var obj))
            {
                obj.OnNetworkDestroy();
                _networkObjects.Remove(objectId);
                ObjectDestroyed?.Invoke(this, obj);

                if (NetworkManager.Instance.IsServer)
                {
                    var message = new ObjectDestroyMessage { ObjectId = objectId };
                    NetworkManager.Instance.BroadcastMessage(message);
                }
            }
        }

        public NetworkObject? GetObject(int objectId)
        {
            return _networkObjects.TryGetValue(objectId, out var obj) ? obj : null;
        }

        public List<NetworkObject> GetAllObjects()
        {
            return _networkObjects.Values.ToList();
        }
    }

    public class ObjectSpawnMessage : INetworkMessage
    {
        public NetworkMessageType Type => NetworkMessageType.ObjectSpawn;
        public int ObjectId { get; set; }
        public int OwnerId { get; set; }
        public Vector2 Position { get; set; }
        public float Rotation { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ObjectId);
            writer.Write(OwnerId);
            writer.Write(Position.X);
            writer.Write(Position.Y);
            writer.Write(Rotation);
        }

        public void Deserialize(BinaryReader reader)
        {
            ObjectId = reader.ReadInt32();
            OwnerId = reader.ReadInt32();
            Position = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            Rotation = reader.ReadSingle();
        }
    }

    public class ObjectDestroyMessage : INetworkMessage
    {
        public NetworkMessageType Type => NetworkMessageType.ObjectDestroy;
        public int ObjectId { get; set; }

        public void Serialize(BinaryWriter writer)
        {
            writer.Write(ObjectId);
        }

        public void Deserialize(BinaryReader reader)
        {
            ObjectId = reader.ReadInt32();
        }
    }

    using System.Reflection;
}

namespace WPFEngine.Audio
{
    public enum SoundCategory
    {
        Master,
        Music,
        SFX,
        Voice,
        Ambient,
        UI
    }

    public class AudioSource
    {
        private NAudio.Wave.WaveOut? _waveOut;
        private NAudio.Wave.AudioFileReader? _audioFile;
        private bool _isPlaying;
        private bool _isLooping;
        private float _volume;
        private float _pitch;
        private float _pan;
        private float _fadeInDuration;
        private float _fadeOutDuration;
        private float _fadeTimer;
        private bool _isFadingIn;
        private bool _isFadingOut;

        public bool IsPlaying => _isPlaying;
        public bool IsLooping
        {
            get => _isLooping;
            set
            {
                _isLooping = value;
                if (_waveOut != null)
                {
                    _waveOut.loop = value;
                }
            }
        }

        public float Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0f, 1f);
                if (_audioFile != null)
                {
                    _audioFile.Volume = _volume;
                }
            }
        }

        public float Pitch
        {
            get => _pitch;
            set => _pitch = Math.Clamp(value, 0.5f, 2f);
        }

        public float Pan
        {
            get => _pan;
            set => _pan = Math.Clamp(value, -1f, 1f);
        }

        public AudioSource()
        {
            _isPlaying = false;
            _isLooping = false;
            _volume = 1f;
            _pitch = 1f;
            _pan = 0f;
            _fadeInDuration = 0f;
            _fadeOutDuration = 0f;
            _fadeTimer = 0f;
            _isFadingIn = false;
            _isFadingOut = false;
        }

        public bool Load(string filePath)
        {
            try
            {
                Stop();
                _audioFile = new NAudio.Wave.AudioFileReader(filePath);
                _waveOut = new NAudio.Wave.WaveOut();
                _waveOut.Init(_audioFile);
                _waveOut.Volume = _volume;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void Play()
        {
            if (_waveOut == null || _audioFile == null)
                return;

            _audioFile.Position = 0;
            _waveOut.Play();
            _isPlaying = true;

            if (_fadeInDuration > 0)
            {
                _isFadingIn = true;
                _fadeTimer = 0f;
            }
        }

        public void Pause()
        {
            _waveOut?.Pause();
            _isPlaying = false;
        }

        public void Stop()
        {
            _waveOut?.Stop();
            _isPlaying = false;
            if (_audioFile != null)
            {
                _audioFile.Position = 0;
            }
        }

        public void Update(float deltaTime)
        {
            if (_isFadingIn)
            {
                _fadeTimer += deltaTime;
                float t = Math.Min(_fadeTimer / _fadeInDuration, 1f);
                _audioFile!.Volume = _volume * t;

                if (_fadeTimer >= _fadeInDuration)
                {
                    _isFadingIn = false;
                }
            }

            if (_isFadingOut)
            {
                _fadeTimer += deltaTime;
                float t = Math.Max(1f - _fadeTimer / _fadeOutDuration, 0f);
                _audioFile!.Volume = _volume * t;

                if (_fadeTimer >= _fadeOutDuration)
                {
                    _isFadingOut = false;
                    Stop();
                }
            }
        }

        public void FadeIn(float duration)
        {
            _fadeInDuration = duration;
            _fadeTimer = 0f;
            _isFadingIn = true;
            _isFadingOut = false;
        }

        public void FadeOut(float duration)
        {
            if (_isPlaying)
            {
                _fadeOutDuration = duration;
                _fadeTimer = 0f;
                _isFadingOut = true;
                _isFadingIn = false;
            }
        }

        public void SetPosition(float position)
        {
            if (_audioFile != null)
            {
                _audioFile.CurrentTime = TimeSpan.FromSeconds(position);
            }
        }

        public float GetPosition()
        {
            return (float)(_audioFile?.CurrentTime.TotalSeconds ?? 0);
        }

        public float GetDuration()
        {
            return (float)(_audioFile?.TotalTime.TotalSeconds ?? 0);
        }

        public void Dispose()
        {
            Stop();
            _waveOut?.Dispose();
            _audioFile?.Dispose();
            _waveOut = null;
            _audioFile = null;
        }
    }

    public class AudioMixer
    {
        private readonly Dictionary<SoundCategory, float> _categoryVolumes;
        private float _masterVolume;
        private bool _muted;

        public float MasterVolume
        {
            get => _masterVolume;
            set
            {
                _masterVolume = Math.Clamp(value, 0f, 1f);
                UpdateAllVolumes();
            }
        }

        public bool Muted
        {
            get => _muted;
            set
            {
                _muted = value;
                UpdateAllVolumes();
            }
        }

        public AudioMixer()
        {
            _categoryVolumes = new Dictionary<SoundCategory, float>();
            foreach (SoundCategory category in Enum.GetValues(typeof(SoundCategory)))
            {
                _categoryVolumes[category] = 1f;
            }
            _masterVolume = 1f;
            _muted = false;
        }

        public float GetCategoryVolume(SoundCategory category)
        {
            return _categoryVolumes.TryGetValue(category, out var volume) ? volume : 1f;
        }

        public void SetCategoryVolume(SoundCategory category, float volume)
        {
            _categoryVolumes[category] = Math.Clamp(volume, 0f, 1f);
        }

        public float GetEffectiveVolume(SoundCategory category)
        {
            if (_muted)
                return 0f;

            float categoryVol = GetCategoryVolume(category);
            return _masterVolume * categoryVol;
        }

        private void UpdateAllVolumes()
        {
            AudioManager.Instance.UpdateMixerVolumes();
        }
    }

    public class AudioManager
    {
        private static AudioManager? _instance;
        public static AudioManager Instance => _instance ??= new AudioManager();

        private readonly Dictionary<string, AudioSource> _sources;
        private readonly Dictionary<string, AudioClip> _audioClips;
        private readonly AudioMixer _mixer;
        private readonly ObjectPool<AudioSource> _sourcePool;
        private const int MaxPooledSources = 32;

        public AudioMixer Mixer => _mixer;

        public event EventHandler<string>? ClipLoaded;
        public event EventHandler<string>? ClipUnloaded;

        private AudioManager()
        {
            _sources = new Dictionary<string, AudioSource>();
            _audioClips = new Dictionary<string, AudioClip>();
            _mixer = new AudioMixer();
            _sourcePool = new ObjectPool<AudioSource>(MaxPooledSources, () => new AudioSource());
        }

        public AudioSource? LoadClip(string name, string filePath)
        {
            if (_audioClips.TryGetValue(name, out var existingClip))
            {
                return existingClip.Source;
            }

            var source = new AudioSource();
            if (source.Load(filePath))
            {
                _audioClips[name] = new AudioClip { Name = name, FilePath = filePath, Source = source };
                ClipLoaded?.Invoke(this, name);
                return source;
            }

            source.Dispose();
            return null;
        }

        public AudioSource? GetClip(string name)
        {
            return _audioClips.TryGetValue(name, out var clip) ? clip.Source : null;
        }

        public void UnloadClip(string name)
        {
            if (_audioClips.TryGetValue(name, out var clip))
            {
                clip.Source.Dispose();
                _audioClips.Remove(name);
                ClipUnloaded?.Invoke(this, name);
            }
        }

        public void UnloadAll()
        {
            foreach (var clip in _audioClips.Values)
            {
                clip.Source.Dispose();
            }
            _audioClips.Clear();
        }

        public AudioSource? Play(string clipName, SoundCategory category = SoundCategory.SFX, bool loop = false)
        {
            var source = GetClip(clipName);
            if (source == null)
                return null;

            source.IsLooping = loop;
            source.Volume = _mixer.GetEffectiveVolume(category);
            source.Play();
            return source;
        }

        public AudioSource? PlayOneShot(string clipName, SoundCategory category = SoundCategory.SFX)
        {
            var source = _sourcePool.Rent();
            string path = _audioClips.TryGetValue(clipName, out var clip) ? clip.FilePath : clipName;

            if (source.Load(path))
            {
                source.Volume = _mixer.GetEffectiveVolume(category);
                source.Play();
                return source;
            }

            _sourcePool.Return(source);
            return null;
        }

        public void Stop(string clipName)
        {
            var source = GetClip(clipName);
            source?.Stop();
        }

        public void StopAll()
        {
            foreach (var clip in _audioClips.Values)
            {
                clip.Source.Stop();
            }

            var pooledSources = _sourcePool.RentMultiple(MaxPooledSources);
            foreach (var source in pooledSources)
            {
                source.Stop();
            }
            _sourcePool.ReturnMultiple(pooledSources);
        }

        public void FadeIn(string clipName, float duration)
        {
            var source = GetClip(clipName);
            source?.FadeIn(duration);
        }

        public void FadeOut(string clipName, float duration)
        {
            var source = GetClip(clipName);
            source?.FadeOut(duration);
        }

        public void Update(float deltaTime)
        {
            foreach (var clip in _audioClips.Values)
            {
                clip.Source.Update(deltaTime);
            }
        }

        public void UpdateMixerVolumes()
        {
            foreach (var clip in _audioClips.Values)
            {
            }
        }

        public int GetLoadedClipCount()
        {
            return _audioClips.Count;
        }

        public List<string> GetLoadedClipNames()
        {
            return _audioClips.Keys.ToList();
        }
    }

    public class AudioClip
    {
        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";
        public AudioSource? Source { get; set; }
        public float Duration { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
    }

    public class MusicPlayer
    {
        private readonly AudioSource _source;
        private readonly AudioSource? _secondarySource;
        private bool _crossfadeEnabled;
        private float _crossfadeDuration;
        private float _crossfadeTimer;
        private bool _isCrossfading;
        private string? _currentTrack;
        private string? _nextTrack;

        public string? CurrentTrack => _currentTrack;
        public bool IsPlaying => _source.IsPlaying;
        public bool CrossfadeEnabled
        {
            get => _crossfadeEnabled;
            set => _crossfadeEnabled = value;
        }

        public MusicPlayer()
        {
            _source = new AudioSource();
            _secondarySource = new AudioSource();
            _crossfadeEnabled = false;
            _crossfadeDuration = 2f;
            _isCrossfading = false;
            _currentTrack = null;
            _nextTrack = null;
        }

        public bool Load(string filePath)
        {
            return _source.Load(filePath);
        }

        public void Play()
        {
            _source.IsLooping = true;
            _source.Volume = AudioManager.Instance.Mixer.GetEffectiveVolume(SoundCategory.Music);
            _source.Play();
        }

        public void Play(string trackName)
        {
            if (_crossfadeEnabled && _source.IsPlaying)
            {
                CrossfadeTo(trackName);
            }
            else
            {
                _source.Stop();
                if (_source.Load(trackName))
                {
                    _currentTrack = trackName;
                    _nextTrack = null;
                    Play();
                }
            }
        }

        public void Stop()
        {
            _source.Stop();
            _currentTrack = null;
        }

        public void Pause()
        {
            _source.Pause();
        }

        public void Resume()
        {
            _source.Play();
        }

        public void CrossfadeTo(string trackName)
        {
            if (!_crossfadeEnabled || _secondarySource == null)
            {
                Play(trackName);
                return;
            }

            _nextTrack = trackName;
            _crossfadeTimer = 0f;
            _isCrossfading = true;

            if (_secondarySource.Load(trackName))
            {
                _secondarySource.Volume = 0;
                _secondarySource.IsLooping = true;
                _secondarySource.Play();
            }
        }

        public void Update(float deltaTime)
        {
            if (_isCrossfading && _secondarySource != null)
            {
                _crossfadeTimer += deltaTime;
                float t = Math.Min(_crossfadeTimer / _crossfadeDuration, 1f);

                _source.Volume = AudioManager.Instance.Mixer.GetEffectiveVolume(SoundCategory.Music) * (1f - t);
                _secondarySource.Volume = AudioManager.Instance.Mixer.GetEffectiveVolume(SoundCategory.Music) * t;

                if (t >= 1f)
                {
                    _source.Stop();
                    _isCrossfading = false;
                    _currentTrack = _nextTrack;
                    _nextTrack = null;

                    var tempSource = _source;
                    _source = _secondarySource;
                    _secondarySource = tempSource;
                }
            }
        }
    }
}

namespace WPFEngine.Rendering
{
    public class RenderTexture
    {
        private System.Windows.Media.Imaging.WriteableBitmap? _bitmap;
        private byte[]? _pixels;
        private int _width;
        private int _height;
        private bool _isDirty;

        public int Width => _width;
        public int Height => _height;
        public System.Windows.Media.Imaging.WriteableBitmap? Bitmap => _bitmap;

        public RenderTexture(int width, int height)
        {
            _width = width;
            _height = height;
            Initialize();
        }

        private void Initialize()
        {
            _bitmap = new System.Windows.Media.Imaging.WriteableBitmap(
                _width, _height, 96, 96,
                System.Windows.Media.PixelFormats.Bgra32, null);

            _pixels = new byte[_width * _height * 4];
            _isDirty = false;
        }

        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return;

            int index = (y * _width + x) * 4;
            _pixels[index] = color.GetBByte();
            _pixels[index + 1] = color.GetGByte();
            _pixels[index + 2] = color.GetRByte();
            _pixels[index + 3] = color.GetAByte();
            _isDirty = true;
        }

        public Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return Color.Black;

            int index = (y * _width + x) * 4;
            return new Color(_pixels[index + 2], _pixels[index + 1], _pixels[index], _pixels[index + 3]);
        }

        public void Fill(Color color)
        {
            if (_pixels == null)
                return;

            byte b = color.GetBByte();
            byte g = color.GetGByte();
            byte r = color.GetRByte();
            byte a = color.GetAByte();

            for (int i = 0; i < _pixels.Length; i += 4)
            {
                _pixels[i] = b;
                _pixels[i + 1] = g;
                _pixels[i + 2] = r;
                _pixels[i + 3] = a;
            }

            _isDirty = true;
        }

        public void Clear()
        {
            Fill(Color.Black);
        }

        public void Apply()
        {
            if (_bitmap == null || _pixels == null || !_isDirty)
                return;

            _bitmap.WritePixels(
                new System.Windows.Int32Rect(0, 0, _width, _height),
                _pixels, _width * 4, 0);

            _isDirty = false;
        }

        public byte[]? GetPixels()
        {
            return _pixels?.Clone() as byte[];
        }

        public void SetPixels(byte[] pixels)
        {
            if (pixels.Length != _pixels?.Length)
                return;

            Array.Copy(pixels, _pixels, pixels.Length);
            _isDirty = true;
        }
    }

    public class TextureAtlas
    {
        private readonly Dictionary<string, AtlasRegion> _regions;
        private RenderTexture? _texture;
        private int _width;
        private int _height;

        public RenderTexture? Texture => _texture;
        public int Width => _width;
        public int Height => _height;

        public TextureAtlas(int width, int height)
        {
            _width = width;
            _height = height;
            _regions = new Dictionary<string, AtlasRegion>();
        }

        public void AddRegion(string name, int x, int y, int width, int height)
        {
            _regions[name] = new AtlasRegion
            {
                X = x,
                Y = y,
                Width = width,
                Height = height
            };
        }

        public AtlasRegion? GetRegion(string name)
        {
            return _regions.TryGetValue(name, out var region) ? region : null;
        }

        public bool HasRegion(string name)
        {
            return _regions.ContainsKey(name);
        }

        public List<string> GetRegionNames()
        {
            return _regions.Keys.ToList();
        }

        public void Clear()
        {
            _regions.Clear();
        }
    }

    public struct AtlasRegion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public Vector2 UV0 => new Vector2(X, Y);
        public Vector2 UV1 => new Vector2(X + Width, Y + Height);
    }

    public class Sprite
    {
        public RenderTexture? Texture { get; set; }
        public Rectangle SourceRect { get; set; }
        public Vector2 Pivot { get; set; } = new Vector2(0.5f, 0.5f);
        public float PixelsPerUnit { get; set; } = 100f;
        public Vector2 BoundsMin { get; set; }
        public Vector2 BoundsMax { get; set; }

        public Sprite()
        {
            SourceRect = new Rectangle(0, 0, 0, 0);
        }

        public Sprite(RenderTexture texture)
        {
            Texture = texture;
            SourceRect = new Rectangle(0, 0, texture.Width, texture.Height);
            CalculateBounds();
        }

        public Sprite(RenderTexture texture, Rectangle rect)
        {
            Texture = texture;
            SourceRect = rect;
            CalculateBounds();
        }

        private void CalculateBounds()
        {
            float width = SourceRect.Width / PixelsPerUnit;
            float height = SourceRect.Height / PixelsPerUnit;
            BoundsMin = new Vector2(-width * Pivot.X, -height * Pivot.Y);
            BoundsMax = new Vector2(width * (1 - Pivot.X), height * (1 - Pivot.Y));
        }

        public static Sprite Create(RenderTexture texture, int x, int y, int width, int height)
        {
            return new Sprite(texture, new Rectangle(x, y, width, height));
        }
    }

    public class Material
    {
        public string? MainTexturePath { get; set; }
        public Color TintColor { get; set; } = Color.White;
        public BlendMode BlendMode { get; set; } = BlendMode.Alpha;
        public float Opacity { get; set; } = 1f;
        public bool IsInstanced { get; set; } = false;
    }

    public enum BlendMode
    {
        Alpha,
        Additive,
        Multiply,
        Screen,
        Opaque
    }

    public class Shader
    {
        public string Name { get; set; } = "";
        public string? VertexShader { get; set; }
        public string? FragmentShader { get; set; }
        public Dictionary<string, object> Properties { get; } = new Dictionary<string, object>();

        public void SetProperty(string name, object value)
        {
            Properties[name] = value;
        }

        public T? GetProperty<T>(string name)
        {
            if (Properties.TryGetValue(name, out var value) && value is T typed)
            {
                return typed;
            }
            return default;
        }
    }

    public class PostProcessingEffect
    {
        public string Name { get; set; } = "";
        public bool IsEnabled { get; set; } = true;
        public int Order { get; set; }

        public virtual void Apply(RenderTexture source, RenderTexture destination)
        {
        }

        public virtual void Dispose()
        {
        }
    }

    public class GrayscaleEffect : PostProcessingEffect
    {
        public float Intensity { get; set; } = 1f;

        public override void Apply(RenderTexture source, RenderTexture destination)
        {
            if (!IsEnabled)
                return;

            byte[]? sourcePixels = source.GetPixels();
            byte[]? destPixels = new byte[sourcePixels!.Length];

            for (int i = 0; i < sourcePixels.Length; i += 4)
            {
                float r = sourcePixels[i + 2] / 255f;
                float g = sourcePixels[i + 1] / 255f;
                float b = sourcePixels[i] / 255f;

                float gray = 0.299f * r + 0.587f * g + 0.114f * b;

                float finalR = r + (gray - r) * Intensity;
                float finalG = g + (gray - g) * Intensity;
                float finalB = b + (gray - b) * Intensity;

                destPixels[i] = (byte)(finalB * 255);
                destPixels[i + 1] = (byte)(finalG * 255);
                destPixels[i + 2] = (byte)(finalR * 255);
                destPixels[i + 3] = sourcePixels[i + 3];
            }

            destination.SetPixels(destPixels);
            destination.Apply();
        }
    }

    public class BlurEffect : PostProcessingEffect
    {
        public int Radius { get; set; } = 2;
        public float Spread { get; set; } = 1f;

        public override void Apply(RenderTexture source, RenderTexture destination)
        {
            if (!IsEnabled)
                return;

            byte[]? sourcePixels = source.GetPixels();
            byte[]? destPixels = new byte[sourcePixels!.Length];

            int width = source.Width;
            int height = source.Height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float r = 0, g = 0, b = 0, a = 0;
                    int samples = 0;

                    for (int dy = -Radius; dy <= Radius; dy++)
                    {
                        for (int dx = -Radius; dx <= Radius; dx++)
                        {
                            int sx = Math.Clamp(x + dx, 0, width - 1);
                            int sy = Math.Clamp(y + dy, 0, height - 1);
                            int index = (sy * width + sx) * 4;

                            float weight = 1f / (1f + (dx * dx + dy * dy) * Spread);
                            r += sourcePixels[index + 2] * weight;
                            g += sourcePixels[index + 1] * weight;
                            b += sourcePixels[index] * weight;
                            a += sourcePixels[index + 3] * weight;
                            samples += (int)weight;
                        }
                    }

                    int i = (y * width + x) * 4;
                    destPixels[i] = (byte)(b / samples);
                    destPixels[i + 1] = (byte)(g / samples);
                    destPixels[i + 2] = (byte)(r / samples);
                    destPixels[i + 3] = (byte)(a / samples);
                }
            }

            destination.SetPixels(destPixels);
            destination.Apply();
        }
    }

    public class BloomEffect : PostProcessingEffect
    {
        public float Threshold { get; set; } = 0.5f;
        public float Intensity { get; set; } = 1f;
        public int Radius { get; set; } = 4;

        public override void Apply(RenderTexture source, RenderTexture destination)
        {
            if (!IsEnabled)
                return;

            byte[]? sourcePixels = source.GetPixels();
            byte[]? destPixels = new byte[sourcePixels!.Length];

            for (int i = 0; i < sourcePixels.Length; i += 4)
            {
                float r = sourcePixels[i + 2] / 255f;
                float g = sourcePixels[i + 1] / 255f;
                float b = sourcePixels[i] / 255f;

                float brightness = 0.299f * r + 0.587f * g + 0.114f * b;

                if (brightness > Threshold)
                {
                    float factor = (brightness - Threshold) / (1f - Threshold) * Intensity;
                    destPixels[i] = (byte)Math.Min(b + b * factor, 1f) * 255;
                    destPixels[i + 1] = (byte)Math.Min(g + g * factor, 1f) * 255;
                    destPixels[i + 2] = (byte)Math.Min(r + r * factor, 1f) * 255;
                    destPixels[i + 3] = sourcePixels[i + 3];
                }
                else
                {
                    destPixels[i] = sourcePixels[i];
                    destPixels[i + 1] = sourcePixels[i + 1];
                    destPixels[i + 2] = sourcePixels[i + 2];
                    destPixels[i + 3] = sourcePixels[i + 3];
                }
            }

            destination.SetPixels(destPixels);
            destination.Apply();
        }
    }

    public class PostProcessingManager
    {
        private static PostProcessingManager? _instance;
        public static PostProcessingManager Instance => _instance ??= new PostProcessingManager();

        private readonly List<PostProcessingEffect> _effects = new List<PostProcessingEffect>();
        private RenderTexture? _temporaryTexture;

        public IReadOnlyList<PostProcessingEffect> Effects => _effects;

        private PostProcessingManager()
        {
        }

        public void AddEffect(PostProcessingEffect effect)
        {
            _effects.Add(effect);
            _effects.Sort((a, b) => a.Order.CompareTo(b.Order));
        }

        public void RemoveEffect(PostProcessingEffect effect)
        {
            _effects.Remove(effect);
        }

        public void RemoveEffect(string name)
        {
            _effects.RemoveAll(e => e.Name == name);
        }

        public PostProcessingEffect? GetEffect(string name)
        {
            return _effects.FirstOrDefault(e => e.Name == name);
        }

        public void ClearEffects()
        {
            foreach (var effect in _effects)
            {
                effect.Dispose();
            }
            _effects.Clear();
        }

        public void Apply(RenderTexture source, RenderTexture destination)
        {
            if (_effects.Count == 0)
                return;

            _temporaryTexture ??= new RenderTexture(source.Width, source.Height);

            RenderTexture currentSource = source;
            RenderTexture currentDest = _temporaryTexture;

            for (int i = 0; i < _effects.Count; i++)
            {
                bool isLast = i == _effects.Count - 1;
                PostProcessingEffect effect = _effects[i];

                if (isLast)
                {
                    effect.Apply(currentSource, destination);
                }
                else
                {
                    effect.Apply(currentSource, currentDest);

                    var temp = currentSource;
                    currentSource = currentDest;
                    currentDest = temp;
                }
            }
        }
    }
}

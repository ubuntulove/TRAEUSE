using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;

namespace OfficeSuite.Common.Serialization
{
    public class DocumentSerializer
    {
        private readonly Dictionary<string, ISerializer> _serializers;
        private ISerializer _defaultSerializer;

        public DocumentSerializer()
        {
            _serializers = new Dictionary<string, ISerializer>();
            InitializeDefaultSerializers();
        }

        private void InitializeDefaultSerializers()
        {
            RegisterSerializer(".json", new JsonDocumentSerializer());
            RegisterSerializer(".xml", new XmlDocumentSerializer());
            RegisterSerializer(".bin", new BinaryDocumentSerializer());
            _defaultSerializer = _serializers[".json"];
        }

        public void RegisterSerializer(string fileExtension, ISerializer serializer)
        {
            _serializers[fileExtension.ToLowerInvariant()] = serializer;
        }

        public ISerializer GetSerializer(string fileExtension)
        {
            return _serializers.TryGetValue(fileExtension.ToLowerInvariant(), out var serializer) 
                ? serializer 
                : _defaultSerializer;
        }

        public void Serialize<T>(T document, string filePath)
        {
            var extension = Path.GetExtension(filePath);
            var serializer = GetSerializer(extension);
            serializer.Serialize(document, filePath);
        }

        public T Deserialize<T>(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            var serializer = GetSerializer(extension);
            return serializer.Deserialize<T>(filePath);
        }

        public string SerializeToString<T>(T document, string format = ".json")
        {
            var serializer = GetSerializer(format);
            return serializer.SerializeToString(document);
        }

        public T DeserializeFromString<T>(string data, string format = ".json")
        {
            var serializer = GetSerializer(format);
            return serializer.DeserializeFromString<T>(data);
        }
    }

    public interface ISerializer
    {
        void Serialize<T>(T obj, string filePath);
        T Deserialize<T>(string filePath);
        string SerializeToString<T>(T obj);
        T DeserializeFromString<T>(string data);
        string[] SupportedExtensions { get; }
    }

    public class JsonDocumentSerializer : ISerializer
    {
        public string[] SupportedExtensions => new[] { ".json" };

        public void Serialize<T>(T obj, string filePath)
        {
            var json = SerializeToString(obj);
            File.WriteAllText(filePath, json, Encoding.UTF8);
        }

        public T Deserialize<T>(string filePath)
        {
            var json = File.ReadAllText(filePath, Encoding.UTF8);
            return DeserializeFromString<T>(json);
        }

        public string SerializeToString<T>(T obj)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Serialize(obj, options);
        }

        public T DeserializeFromString<T>(string data)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            return JsonSerializer.Deserialize<T>(data, options);
        }
    }

    public class XmlDocumentSerializer : ISerializer
    {
        public string[] SupportedExtensions => new[] { ".xml" };

        public void Serialize<T>(T obj, string filePath)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using var writer = new FileStream(filePath, FileMode.Create);
            serializer.Serialize(writer, obj);
        }

        public T Deserialize<T>(string filePath)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using var reader = new FileStream(filePath, FileMode.Open);
            return (T)serializer.Deserialize(reader);
        }

        public string SerializeToString<T>(T obj)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using var writer = new StringWriter();
            serializer.Serialize(writer, obj);
            return writer.ToString();
        }

        public T DeserializeFromString<T>(string data)
        {
            var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
            using var reader = new StringReader(data);
            return (T)serializer.Deserialize(reader);
        }
    }

    public class BinaryDocumentSerializer : ISerializer
    {
        public string[] SupportedExtensions => new[] { ".bin" };

        public void Serialize<T>(T obj, string filePath)
        {
            var formatter = new BinaryFormatter();
            using var stream = new FileStream(filePath, FileMode.Create);
            formatter.Serialize(stream, obj);
        }

        public T Deserialize<T>(string filePath)
        {
            var formatter = new BinaryFormatter();
            using var stream = new FileStream(filePath, FileMode.Open);
            return (T)formatter.Deserialize(stream);
        }

        public string SerializeToString<T>(T obj)
        {
            using var stream = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            return Convert.ToBase64String(stream.ToArray());
        }

        public T DeserializeFromString<T>(string data)
        {
            var bytes = Convert.FromBase64String(data);
            using var stream = new MemoryStream(bytes);
            var formatter = new BinaryFormatter();
            return (T)formatter.Deserialize(stream);
        }
    }

    public class SettingsSerializer
    {
        private readonly string _settingsPath;
        private readonly DocumentSerializer _documentSerializer;

        public SettingsSerializer(string settingsPath)
        {
            _settingsPath = settingsPath;
            _documentSerializer = new DocumentSerializer();
        }

        public void Save<T>(T settings) where T : new()
        {
            _documentSerializer.Serialize(settings, _settingsPath);
        }

        public T Load<T>() where T : new()
        {
            if (File.Exists(_settingsPath))
            {
                return _documentSerializer.Deserialize<T>(_settingsPath);
            }
            return new T();
        }

        public T LoadOrDefault<T>() where T : new()
        {
            try
            {
                return Load<T>();
            }
            catch
            {
                return new T();
            }
        }

        public void Delete()
        {
            if (File.Exists(_settingsPath))
            {
                File.Delete(_settingsPath);
            }
        }

        public bool Exists()
        {
            return File.Exists(_settingsPath);
        }

        public DateTime LastModified()
        {
            return File.Exists(_settingsPath) ? File.GetLastWriteTime(_settingsPath) : DateTime.MinValue;
        }
    }

    public class CompressedSerializer : ISerializer
    {
        private readonly ISerializer _innerSerializer;

        public CompressedSerializer(ISerializer innerSerializer)
        {
            _innerSerializer = innerSerializer;
        }

        public string[] SupportedExtensions => _innerSerializer.SupportedExtensions;

        public void Serialize<T>(T obj, string filePath)
        {
            var json = _innerSerializer.SerializeToString(obj);
            var compressed = Compress(Encoding.UTF8.GetBytes(json));
            File.WriteAllBytes(filePath, compressed);
        }

        public T Deserialize<T>(string filePath)
        {
            var compressed = File.ReadAllBytes(filePath);
            var json = Encoding.UTF8.GetString(Decompress(compressed));
            return _innerSerializer.DeserializeFromString<T>(json);
        }

        public string SerializeToString<T>(T obj)
        {
            var json = _innerSerializer.SerializeToString(obj);
            return Convert.ToBase64String(Compress(Encoding.UTF8.GetBytes(json)));
        }

        public T DeserializeFromString<T>(string data)
        {
            var bytes = Decompress(Convert.FromBase64String(data));
            var json = Encoding.UTF8.GetString(bytes);
            return _innerSerializer.DeserializeFromString<T>(json);
        }

        private byte[] Compress(byte[] data)
        {
            using var output = new MemoryStream();
            using (var gzip = new System.IO.Compression.GZipStream(output, System.IO.Compression.CompressionMode.Compress))
            {
                gzip.Write(data, 0, data.Length);
            }
            return output.ToArray();
        }

        private byte[] Decompress(byte[] data)
        {
            using var input = new MemoryStream(data);
            using var gzip = new System.IO.Compression.GZipStream(input, System.IO.Compression.CompressionMode.Decompress);
            using var output = new MemoryStream();
            gzip.CopyTo(output);
            return output.ToArray();
        }
    }

    public class EncryptedSerializer : ISerializer
    {
        private readonly ISerializer _innerSerializer;
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public EncryptedSerializer(ISerializer innerSerializer, string password)
        {
            _innerSerializer = innerSerializer;

            using var deriveBytes = new Rfc2898DeriveBytes(password, 16);
            _key = deriveBytes.GetBytes(32);
            _iv = deriveBytes.GetBytes(16);
        }

        public string[] SupportedExtensions => _innerSerializer.SupportedExtensions;

        public void Serialize<T>(T obj, string filePath)
        {
            var json = _innerSerializer.SerializeToString(obj);
            var encrypted = Encrypt(Encoding.UTF8.GetBytes(json));
            File.WriteAllBytes(filePath, encrypted);
        }

        public T Deserialize<T>(string filePath)
        {
            var encrypted = File.ReadAllBytes(filePath);
            var json = Encoding.UTF8.GetString(Decrypt(encrypted));
            return _innerSerializer.DeserializeFromString<T>(json);
        }

        public string SerializeToString<T>(T obj)
        {
            var json = _innerSerializer.SerializeToString(obj);
            return Convert.ToBase64String(Encrypt(Encoding.UTF8.GetBytes(json)));
        }

        public T DeserializeFromString<T>(string data)
        {
            var bytes = Decrypt(Convert.FromBase64String(data));
            var json = Encoding.UTF8.GetString(bytes);
            return _innerSerializer.DeserializeFromString<T>(json);
        }

        private byte[] Encrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var encryptor = aes.CreateEncryptor();
            using var output = new MemoryStream();
            using var cryptoStream = new CryptoStream(output, encryptor, CryptoStreamMode.Write);
            cryptoStream.Write(data, 0, data.Length);
            cryptoStream.FlushFinalBlock();
            return output.ToArray();
        }

        private byte[] Decrypt(byte[] data)
        {
            using var aes = Aes.Create();
            aes.Key = _key;
            aes.IV = _iv;
            using var decryptor = aes.CreateDecryptor();
            using var input = new MemoryStream(data);
            using var cryptoStream = new CryptoStream(input, decryptor, CryptoStreamMode.Read);
            using var output = new MemoryStream();
            cryptoStream.CopyTo(output);
            return output.ToArray();
        }
    }

    public class VersionedSerializer<T> : ISerializer where T : IVersionable, new()
    {
        private readonly ISerializer _innerSerializer;
        private readonly int _currentVersion;

        public VersionedSerializer(ISerializer innerSerializer, int currentVersion)
        {
            _innerSerializer = innerSerializer;
            _currentVersion = currentVersion;
        }

        public string[] SupportedExtensions => _innerSerializer.SupportedExtensions;

        public void Serialize<T>(T obj, string filePath)
        {
            _innerSerializer.Serialize(obj, filePath);
        }

        public T Deserialize<T>(string filePath)
        {
            var obj = _innerSerializer.Deserialize<T>(filePath);
            return MigrateIfNeeded(obj);
        }

        public string SerializeToString<T>(T obj)
        {
            return _innerSerializer.SerializeToString(obj);
        }

        public T DeserializeFromString<T>(string data)
        {
            var obj = _innerSerializer.DeserializeFromString<T>(data);
            return MigrateIfNeeded(obj);
        }

        private T MigrateIfNeeded(T obj)
        {
            if (obj is IVersionable versionable)
            {
                if (versionable.Version < _currentVersion)
                {
                    return (T)Migrate(versionable);
                }
            }
            return obj;
        }

        private IVersionable Migrate(IVersionable obj)
        {
            while (obj.Version < _currentVersion)
            {
                obj = MigrateToNextVersion(obj);
                obj.Version++;
            }
            return obj;
        }

        private IVersionable MigrateToNextVersion(IVersionable obj)
        {
            return obj;
        }
    }

    public interface IVersionable
    {
        int Version { get; set; }
    }

    public class IncrementalBackupSerializer : ISerializer
    {
        private readonly ISerializer _innerSerializer;
        private readonly string _backupDirectory;
        private readonly int _maxBackups;

        public IncrementalBackupSerializer(ISerializer innerSerializer, string backupDirectory, int maxBackups = 10)
        {
            _innerSerializer = innerSerializer;
            _backupDirectory = backupDirectory;
            _maxBackups = maxBackups;

            if (!Directory.Exists(_backupDirectory))
                Directory.CreateDirectory(_backupDirectory);
        }

        public string[] SupportedExtensions => _innerSerializer.SupportedExtensions;

        public void Serialize<T>(T obj, string filePath)
        {
            CreateBackup(filePath);
            _innerSerializer.Serialize(obj, filePath);
        }

        public T Deserialize<T>(string filePath)
        {
            return _innerSerializer.Deserialize<T>(filePath);
        }

        public string SerializeToString<T>(T obj)
        {
            return _innerSerializer.SerializeToString(obj);
        }

        public T DeserializeFromString<T>(string data)
        {
            return _innerSerializer.DeserializeFromString<T>(data);
        }

        private void CreateBackup(string filePath)
        {
            if (!File.Exists(filePath)) return;

            var backupName = $"{Path.GetFileNameWithoutExtension(filePath)}_{DateTime.Now:yyyyMMddHHmmss}{Path.GetExtension(filePath)}";
            var backupPath = Path.Combine(_backupDirectory, backupName);
            File.Copy(filePath, backupPath, true);

            CleanupOldBackups(Path.GetFileNameWithoutExtension(filePath));
        }

        private void CleanupOldBackups(string baseFileName)
        {
            var backups = Directory.GetFiles(_backupDirectory, $"{baseFileName}_*{Path.GetExtension(baseFileName)}")
                .OrderByDescending(f => File.GetCreationTime(f))
                .Skip(_maxBackups)
                .ToList();

            foreach (var backup in backups)
            {
                File.Delete(backup);
            }
        }

        public IEnumerable<string> GetBackups(string baseFileName)
        {
            return Directory.GetFiles(_backupDirectory, $"{baseFileName}_*")
                .OrderByDescending(f => File.GetCreationTime(f));
        }

        public T RestoreBackup<T>(string backupPath)
        {
            return _innerSerializer.Deserialize<T>(backupPath);
        }
    }

    public class SchemaValidationSerializer : ISerializer
    {
        private readonly ISerializer _innerSerializer;
        private readonly Dictionary<Type, object> _schemas;

        public SchemaValidationSerializer(ISerializer innerSerializer)
        {
            _innerSerializer = innerSerializer;
            _schemas = new Dictionary<Type, object>();
        }

        public string[] SupportedExtensions => _innerSerializer.SupportedExtensions;

        public void Serialize<T>(T obj, string filePath)
        {
            Validate(obj);
            _innerSerializer.Serialize(obj, filePath);
        }

        public T Deserialize<T>(string filePath)
        {
            var obj = _innerSerializer.Deserialize<T>(filePath);
            Validate(obj);
            return obj;
        }

        public string SerializeToString<T>(T obj)
        {
            Validate(obj);
            return _innerSerializer.SerializeToString(obj);
        }

        public T DeserializeFromString<T>(string data)
        {
            var obj = _innerSerializer.DeserializeFromString<T>(data);
            Validate(obj);
            return obj;
        }

        public void RegisterSchema<T>(object schema)
        {
            _schemas[typeof(T)] = schema;
        }

        private void Validate<T>(T obj)
        {
            if (_schemas.TryGetValue(typeof(T), out var schema))
            {
                // Validation logic would go here
            }
        }
    }

    public class StreamingSerializer<T> : ISerializer where T : class
    {
        private readonly ISerializer _innerSerializer;
        private readonly int _chunkSize;

        public StreamingSerializer(ISerializer innerSerializer, int chunkSize = 1024 * 1024)
        {
            _innerSerializer = innerSerializer;
            _chunkSize = chunkSize;
        }

        public string[] SupportedExtensions => _innerSerializer.SupportedExtensions;

        public void Serialize<T>(T obj, string filePath)
        {
            _innerSerializer.Serialize(obj, filePath);
        }

        public T Deserialize<T>(string filePath)
        {
            return _innerSerializer.Deserialize<T>(filePath);
        }

        public string SerializeToString<T>(T obj)
        {
            return _innerSerializer.SerializeToString(obj);
        }

        public T DeserializeFromString<T>(string data)
        {
            return _innerSerializer.DeserializeFromString<T>(data);
        }

        public async Task SerializeAsync<T>(T obj, string filePath)
        {
            var data = SerializeToString(obj);
            await File.WriteAllTextAsync(filePath, data);
        }

        public async Task<T> DeserializeAsync<T>(string filePath)
        {
            var data = await File.ReadAllTextAsync(filePath);
            return DeserializeFromString<T>(data);
        }

        public async Task SerializeInChunksAsync(string data, string filePath)
        {
            await using var stream = new FileStream(filePath, FileMode.Create);
            var bytes = Encoding.UTF8.GetBytes(data);
            var position = 0;

            while (position < bytes.Length)
            {
                var count = Math.Min(_chunkSize, bytes.Length - position);
                await stream.WriteAsync(bytes, position, count);
                position += count;
            }
        }

        public async Task<string> DeserializeInChunksAsync(string filePath)
        {
            await using var stream = new FileStream(filePath, FileMode.Open);
            using var reader = new StreamReader(stream);
            return await reader.ReadToEndAsync();
        }
    }

    public class SerializerFactory
    {
        private static readonly Dictionary<string, Func<ISerializer>> _serializerFactories;

        static SerializerFactory()
        {
            _serializerFactories = new Dictionary<string, Func<ISerializer>>
            {
                ["json"] = () => new JsonDocumentSerializer(),
                ["xml"] = () => new XmlDocumentSerializer(),
                ["bin"] = () => new BinaryDocumentSerializer(),
                ["compressed-json"] = () => new CompressedSerializer(new JsonDocumentSerializer()),
                ["encrypted-json"] = password => new EncryptedSerializer(new JsonDocumentSerializer(), password)
            };
        }

        public static ISerializer Create(string type, string password = null)
        {
            if (_serializerFactories.TryGetValue(type.ToLowerInvariant(), out var factory))
            {
                if (type.Contains("encrypted") && password != null)
                {
                    return new EncryptedSerializer(new JsonDocumentSerializer(), password);
                }
                return factory();
            }
            return new JsonDocumentSerializer();
        }

        public static ISerializer CreateFromExtension(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".json" => new JsonDocumentSerializer(),
                ".xml" => new XmlDocumentSerializer(),
                ".bin" => new BinaryDocumentSerializer(),
                _ => new JsonDocumentSerializer()
            };
        }

        public static void RegisterFactory(string type, Func<ISerializer> factory)
        {
            _serializerFactories[type.ToLowerInvariant()] = factory;
        }
    }
}

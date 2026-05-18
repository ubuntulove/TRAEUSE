using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace WPFEngine.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
    public sealed class SerializableAttribute : Attribute
    {
        public string Alias { get; set; }
        public int Version { get; set; } = 1;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializeFieldAttribute : Attribute
    {
        public string Name { get; set; }
        public int Priority { get; set; } = 0;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class HideInInspectorAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializePrivateFieldAttribute : Attribute
    {
        public bool IncludeBackingField { get; set; } = false;
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class GUIDAttribute : Attribute
    {
        public string Prefix { get; set; }
        public GUIDAttribute(string prefix = "")
        {
            Prefix = prefix;
        }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SceneSaveAttribute : Attribute
    {
        public string FileExtension { get; set; } = ".scene";
        public bool AutoSave { get; set; } = false;
        public int MaxAutoSaves { get; set; } = 5;
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public sealed class TypeAliasAttribute : Attribute
    {
        public string Alias { get; }
        public TypeAliasAttribute(string alias)
        {
            Alias = alias;
        }
    }

    public interface ISerializable
    {
        void Serialize(ISerializer serializer);
        void Deserialize(IDeserializer deserializer);
    }

    public interface ISerializer
    {
        void WriteBool(string name, bool value);
        void WriteInt(string name, int value);
        void WriteFloat(string name, float value);
        void WriteDouble(string name, double value);
        void WriteString(string name, string value);
        void WriteGuid(string name, Guid value);
        void WriteBytes(string name, byte[] value);
        void WriteVector2(string name, float x, float y);
        void WriteVector3(string name, float x, float y, float z);
        void WriteColor(string name, byte r, byte g, byte b, byte a);
        void WriteObjectBegin(string name, Type type);
        void WriteObjectEnd();
        void WriteArrayBegin(string name, int count);
        void WriteArrayEnd();
        void WriteDictionaryBegin(string name, int count);
        void WriteDictionaryEnd();
        void WriteNull(string name);
        bool IsWriting { get; }
    }

    public interface IDeserializer
    {
        bool ReadBool(string name, bool defaultValue = default);
        int ReadInt(string name, int defaultValue = default);
        float ReadFloat(string name, float defaultValue = default);
        double ReadDouble(string name, double defaultValue = default);
        string ReadString(string name, string defaultValue = default);
        Guid ReadGuid(string name, Guid defaultValue = default);
        byte[] ReadBytes(string name, byte[] defaultValue = null);
        (float X, float Y) ReadVector2(string name);
        (float X, float Y, float Z) ReadVector3(string name);
        (byte R, byte G, byte B, byte A) ReadColor(string name);
        bool ReadObjectBegin(string name, out Type type);
        void ReadObjectEnd();
        bool ReadArrayBegin(string name, out int count);
        void ReadArrayEnd();
        bool ReadDictionaryBegin(string name, out int count);
        void ReadDictionaryEnd();
        bool IsNull(string name);
        bool HasProperty(string name);
        bool IsReading { get; }
    }

    public enum SerializerFormat
    {
        Binary,
        JSON,
        XML,
        CustomBinary
    }

    public enum CompressionLevel
    {
        None,
        Fast,
        Optimal,
        Maximum
    }

    public enum EncryptionType
    {
        None,
        XOR,
        AES128,
        AES256
    }

    public abstract class FormatterBase
    {
        public CompressionLevel Compression { get; set; } = CompressionLevel.Optimal;
        public EncryptionType Encryption { get; set; } = EncryptionType.None;
        public string EncryptionKey { get; set; } = string.Empty;

        protected virtual byte[] Compress(byte[] data, CompressionLevel level)
        {
            if (level == CompressionLevel.None || data == null || data.Length == 0)
                return data;

            using var output = new MemoryStream();
            var compressor = level switch
            {
                CompressionLevel.Fast => (DeflateStream)new DeflateStream(output, CompressionMode.Compress, leaveOpen: true),
                CompressionLevel.Optimal => new DeflateStream(output, CompressionLevel.Compress, leaveOpen: true),
                CompressionLevel.Maximum => new GZipStream(output, CompressionMode.Compress, leaveOpen: true),
                _ => new DeflateStream(output, CompressionMode.Compress, leaveOpen: true)
            };

            compressor.Write(data, 0, data.Length);
            compressor.Close();
            return output.ToArray();
        }

        protected virtual byte[] Decompress(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            if (data[0] == 0x1F && data[1] == 0x8B)
            {
                using var input = new MemoryStream(data);
                using var gzip = new GZipStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                gzip.CopyTo(output);
                return output.ToArray();
            }
            else
            {
                using var input = new MemoryStream(data);
                using var deflate = new DeflateStream(input, CompressionMode.Decompress);
                using var output = new MemoryStream();
                deflate.CopyTo(output);
                return output.ToArray();
            }
        }

        protected virtual byte[] Encrypt(byte[] data, EncryptionType type, string key)
        {
            if (type == EncryptionType.None || data == null)
                return data;

            return type switch
            {
                EncryptionType.XOR => EncryptXOR(data, key),
                EncryptionType.AES128 or EncryptionType.AES256 => EncryptAES(data, key, type),
                _ => data
            };
        }

        protected virtual byte[] Decrypt(byte[] data, EncryptionType type, string key)
        {
            if (type == EncryptionType.None || data == null)
                return data;

            return type switch
            {
                EncryptionType.XOR => DecryptXOR(data, key),
                EncryptionType.AES128 or EncryptionType.AES256 => DecryptAES(data, key, type),
                _ => data
            };
        }

        private byte[] EncryptXOR(byte[] data, string key)
        {
            if (string.IsNullOrEmpty(key))
                return data;

            var keyBytes = Encoding.UTF8.GetBytes(key);
            var result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                result[i] = (byte)(data[i] ^ keyBytes[i % keyBytes.Length]);
            }
            return result;
        }

        private byte[] DecryptXOR(byte[] data, string key)
        {
            return EncryptXOR(data, key);
        }

        private byte[] EncryptAES(byte[] data, string key, EncryptionType type)
        {
            int keySize = type == EncryptionType.AES128 ? 16 : 32;
            using var aes = Aes.Create();
            aes.KeySize = type == EncryptionType.AES128 ? 128 : 256;
            aes.GenerateIV();

            using var deriveBytes = new Rfc2898DeriveBytes(key, aes.IV, 1000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(keySize);

            using var encryptor = aes.CreateEncryptor();
            var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
            var result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            return result;
        }

        private byte[] DecryptAES(byte[] data, string key, EncryptionType type)
        {
            int keySize = type == EncryptionType.AES128 ? 16 : 32;
            using var aes = Aes.Create();
            aes.KeySize = type == EncryptionType.AES128 ? 128 : 256;

            var iv = new byte[16];
            Buffer.BlockCopy(data, 0, iv, 0, 16);
            aes.IV = iv;

            using var deriveBytes = new Rfc2898DeriveBytes(key, iv, 1000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(keySize);

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 16, data.Length - 16);
        }

        public abstract byte[] Serialize<T>(T obj) where T : class;
        public abstract T Deserialize<T>(byte[] data) where T : class;
    }

    public sealed class BinaryFormatter : FormatterBase
    {
        private static readonly BinaryFormatter Default = new BinaryFormatter();
        private readonly TypeCache _typeCache = TypeCache.Instance;

        public static BinaryFormatter Create() => new BinaryFormatter();

        public override byte[] Serialize<T>(T obj)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: true);
            SerializeValue(writer, obj, typeof(T));
            writer.Flush();
            var data = stream.ToArray();
            data = Compress(data, Compression);
            data = Encrypt(data, Encryption, EncryptionKey);
            return data;
        }

        public override T Deserialize<T>(byte[] data)
        {
            data = Decrypt(data, Encryption, EncryptionKey);
            data = Decompress(data);
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream, Encoding.UTF8);
            return (T)DeserializeValue(reader, typeof(T));
        }

        private void SerializeValue(BinaryWriter writer, object value, Type type)
        {
            if (value == null)
            {
                writer.Write((byte)0);
                return;
            }

            writer.Write((byte)1);

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                SerializePrimitive(writer, value, type);
            }
            else if (type.IsEnum)
            {
                writer.Write((byte)50);
                writer.Write(value.ToString());
            }
            else if (type.IsArray)
            {
                SerializeArray(writer, value as Array, type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                SerializeList(writer, value as IList, type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                SerializeDictionary(writer, value as IDictionary, type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                SerializeValue(writer, value, Nullable.GetUnderlyingType(type));
            }
            else if (value is ISerializable serializable)
            {
                writer.Write((byte)100);
                writer.Write(_typeCache.GetTypeId(type));
                serializable.Serialize(new BinarySerializerAdapter(writer));
            }
            else
            {
                SerializeObject(writer, value, type);
            }
        }

        private void SerializePrimitive(BinaryWriter writer, object value, Type type)
        {
            if (type == typeof(bool)) { writer.Write((byte)10); writer.Write((bool)value); }
            else if (type == typeof(byte)) { writer.Write((byte)11); writer.Write((byte)value); }
            else if (type == typeof(sbyte)) { writer.Write((byte)12); writer.Write((sbyte)value); }
            else if (type == typeof(short)) { writer.Write((byte)13); writer.Write((short)value); }
            else if (type == typeof(ushort)) { writer.Write((byte)14); writer.Write((ushort)value); }
            else if (type == typeof(int)) { writer.Write((byte)15); writer.Write((int)value); }
            else if (type == typeof(uint)) { writer.Write((byte)16); writer.Write((uint)value); }
            else if (type == typeof(long)) { writer.Write((byte)17); writer.Write((long)value); }
            else if (type == typeof(ulong)) { writer.Write((byte)18); writer.Write((ulong)value); }
            else if (type == typeof(float)) { writer.Write((byte)19); writer.Write((float)value); }
            else if (type == typeof(double)) { writer.Write((byte)20); writer.Write((double)value); }
            else if (type == typeof(decimal)) { writer.Write((byte)21); writer.Write((decimal)value); }
            else if (type == typeof(char)) { writer.Write((byte)22); writer.Write((char)value); }
            else if (type == typeof(string)) { writer.Write((byte)23); writer.Write((string)value ?? string.Empty); }
            else if (type == typeof(Guid)) { writer.Write((byte)24); writer.Write(((Guid)value).ToByteArray()); }
            else if (type == typeof(DateTime)) { writer.Write((byte)25); writer.Write(((DateTime)value).Ticks); }
            else if (type == typeof(TimeSpan)) { writer.Write((byte)26); writer.Write(((TimeSpan)value).Ticks); }
        }

        private void SerializeArray(BinaryWriter writer, Array array, Type type)
        {
            writer.Write((byte)60);
            var elementType = type.GetElementType();
            writer.Write(_typeCache.GetTypeId(elementType));
            writer.Write(array.Length);
            foreach (var item in array)
            {
                SerializeValue(writer, item, elementType);
            }
        }

        private void SerializeList(BinaryWriter writer, IList list, Type type)
        {
            writer.Write((byte)70);
            var elementType = type.GetGenericArguments()[0];
            writer.Write(_typeCache.GetTypeId(elementType));
            writer.Write(list.Count);
            foreach (var item in list)
            {
                SerializeValue(writer, item, elementType);
            }
        }

        private void SerializeDictionary(BinaryWriter writer, IDictionary dict, Type type)
        {
            writer.Write((byte)80);
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            writer.Write(_typeCache.GetTypeId(keyType));
            writer.Write(_typeCache.GetTypeId(valueType));
            writer.Write(dict.Count);
            foreach (DictionaryEntry entry in dict)
            {
                SerializeValue(writer, entry.Key, keyType);
                SerializeValue(writer, entry.Value, valueType);
            }
        }

        private void SerializeObject(BinaryWriter writer, object obj, Type type)
        {
            writer.Write((byte)90);
            writer.Write(_typeCache.GetTypeId(type));
            var fields = GetSerializableFields(type);
            writer.Write(fields.Count);
            foreach (var field in fields)
            {
                writer.Write(field.Name);
                var value = field.GetValue(obj);
                SerializeValue(writer, value, field.FieldType);
            }
        }

        private object DeserializeValue(BinaryReader reader, Type type)
        {
            if (reader.ReadByte() == 0)
                return null;

            return ReadByTypeMarker(reader, type);
        }

        private object ReadByTypeMarker(BinaryReader reader, Type expectedType)
        {
            byte marker = reader.ReadByte();

            return marker switch
            {
                10 => reader.ReadBoolean(),
                11 => reader.ReadByte(),
                12 => reader.ReadSByte(),
                13 => reader.ReadInt16(),
                14 => reader.ReadUInt16(),
                15 => reader.ReadInt32(),
                16 => reader.ReadUInt32(),
                17 => reader.ReadInt64(),
                18 => reader.ReadUInt64(),
                19 => reader.ReadSingle(),
                20 => reader.ReadDouble(),
                21 => reader.ReadDecimal(),
                22 => reader.ReadChar(),
                23 => reader.ReadString(),
                24 => new Guid(reader.ReadBytes(16)),
                25 => new DateTime(reader.ReadInt64()),
                26 => new TimeSpan(reader.ReadInt64()),
                50 => Enum.Parse(expectedType, reader.ReadString()),
                60 => ReadArray(reader, expectedType),
                70 => ReadList(reader, expectedType),
                80 => ReadDictionary(reader, expectedType),
                90 => ReadObject(reader, expectedType),
                100 => ReadSerializable(reader, expectedType),
                _ => throw new InvalidOperationException($"Unknown type marker: {marker}")
            };
        }

        private Array ReadArray(BinaryReader reader, Type arrayType)
        {
            var elementType = _typeCache.GetType(reader.ReadInt32());
            int length = reader.ReadInt32();
            var array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
            {
                array.SetValue(DeserializeValue(reader, elementType), i);
            }
            return array;
        }

        private IList ReadList(BinaryReader reader, Type listType)
        {
            var elementType = _typeCache.GetType(reader.ReadInt32());
            int count = reader.ReadInt32();
            var listType2 = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType2);
            for (int i = 0; i < count; i++)
            {
                list.Add(DeserializeValue(reader, elementType));
            }
            return list;
        }

        private IDictionary ReadDictionary(BinaryReader reader, Type dictType)
        {
            var keyType = _typeCache.GetType(reader.ReadInt32());
            var valueType = _typeCache.GetType(reader.ReadInt32());
            int count = reader.ReadInt32();
            var dictType2 = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            var dict = (IDictionary)Activator.CreateInstance(dictType2);
            for (int i = 0; i < count; i++)
            {
                var key = DeserializeValue(reader, keyType);
                var value = DeserializeValue(reader, valueType);
                dict.Add(key, value);
            }
            return dict;
        }

        private object ReadObject(BinaryReader reader, Type type)
        {
            var typeId = reader.ReadInt32();
            var actualType = _typeCache.GetType(typeId);
            var obj = Activator.CreateInstance(actualType);
            int fieldCount = reader.ReadInt32();
            for (int i = 0; i < fieldCount; i++)
            {
                var fieldName = reader.ReadString();
                var field = actualType.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    var value = DeserializeValue(reader, field.FieldType);
                    field.SetValue(obj, value);
                }
            }
            return obj;
        }

        private object ReadSerializable(BinaryReader reader, Type type)
        {
            var typeId = reader.ReadInt32();
            var actualType = _typeCache.GetType(typeId);
            var obj = Activator.CreateInstance(actualType);
            if (obj is ISerializable serializable)
            {
                serializable.Deserialize(new BinaryDeserializerAdapter(reader));
            }
            return obj;
        }

        private List<FieldInfo> GetSerializableFields(Type type)
        {
            var fields = new List<FieldInfo>();
            var visited = new HashSet<Type>();

            while (type != null && type != typeof(object))
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (visited.Contains(field))
                        continue;

                    if (field.GetCustomAttribute<HideInInspectorAttribute>() != null)
                        continue;

                    if (field.IsPrivate && field.GetCustomAttribute<SerializePrivateFieldAttribute>() == null
                        && field.GetCustomAttribute<SerializeFieldAttribute>() == null)
                        continue;

                    fields.Add(field);
                    visited.Add(field);
                }
                type = type.BaseType;
            }

            return fields;
        }

        private class BinarySerializerAdapter : ISerializer
        {
            private readonly BinaryWriter _writer;

            public bool IsWriting => true;

            public BinarySerializerAdapter(BinaryWriter writer)
            {
                _writer = writer;
            }

            public void WriteBool(string name, bool value) => _writer.Write(value);
            public void WriteInt(string name, int value) => _writer.Write(value);
            public void WriteFloat(string name, float value) => _writer.Write(value);
            public void WriteDouble(string name, double value) => _writer.Write(value);
            public void WriteString(string name, string value) => _writer.Write(value ?? string.Empty);
            public void WriteGuid(string name, Guid value) => _writer.Write(value.ToByteArray());
            public void WriteBytes(string name, byte[] value) { _writer.Write(value.Length); _writer.Write(value); }
            public void WriteVector2(string name, float x, float y) { _writer.Write(x); _writer.Write(y); }
            public void WriteVector3(string name, float x, float y, float z) { _writer.Write(x); _writer.Write(y); _writer.Write(z); }
            public void WriteColor(string name, byte r, byte g, byte b, byte a) { _writer.Write(r); _writer.Write(g); _writer.Write(b); _writer.Write(a); }
            public void WriteObjectBegin(string name, Type type) { _writer.Write(_typeCache.GetTypeId(type)); }
            public void WriteObjectEnd() { }
            public void WriteArrayBegin(string name, int count) => _writer.Write(count);
            public void WriteArrayEnd() { }
            public void WriteDictionaryBegin(string name, int count) => _writer.Write(count);
            public void WriteDictionaryEnd() { }
            public void WriteNull(string name) => _writer.Write(false);
        }

        private class BinaryDeserializerAdapter : IDeserializer
        {
            private readonly BinaryReader _reader;

            public bool IsReading => true;

            public BinaryDeserializerAdapter(BinaryReader reader)
            {
                _reader = reader;
            }

            public bool ReadBool(string name, bool defaultValue = default) => _reader.ReadBoolean();
            public int ReadInt(string name, int defaultValue = default) => _reader.ReadInt32();
            public float ReadFloat(string name, float defaultValue = default) => _reader.ReadSingle();
            public double ReadDouble(string name, double defaultValue = default) => _reader.ReadDouble();
            public string ReadString(string name, string defaultValue = default) => _reader.ReadString();
            public Guid ReadGuid(string name, Guid defaultValue = default) => new Guid(_reader.ReadBytes(16));
            public byte[] ReadBytes(string name, byte[] defaultValue = null) { var len = _reader.ReadInt32(); return _reader.ReadBytes(len); }
            public (float X, float Y) ReadVector2(string name) => (_reader.ReadSingle(), _reader.ReadSingle());
            public (float X, float Y, float Z) ReadVector3(string name) => (_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
            public (byte R, byte G, byte B, byte A) ReadColor(string name) => (_reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte());
            public bool ReadObjectBegin(string name, out Type type) { type = _typeCache.GetType(_reader.ReadInt32()); return true; }
            public void ReadObjectEnd() { }
            public bool ReadArrayBegin(string name, out int count) { count = _reader.ReadInt32(); return true; }
            public void ReadArrayEnd() { }
            public bool ReadDictionaryBegin(string name, out int count) { count = _reader.ReadInt32(); return true; }
            public void ReadDictionaryEnd() { }
            public bool IsNull(string name) => _reader.ReadByte() == 0;
            public bool HasProperty(string name) => true;
        }
    }

    public sealed class JSONFormatter : FormatterBase
    {
        private readonly TypeCache _typeCache = TypeCache.Instance;
        private readonly JsonSerializerOptions _options;

        public JSONFormatter()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };
        }

        public override byte[] Serialize<T>(T obj)
        {
            var json = JsonSerializer.Serialize(obj, obj.GetType(), _options);
            var data = Encoding.UTF8.GetBytes(json);
            data = Compress(data, Compression);
            data = Encrypt(data, Encryption, EncryptionKey);
            return data;
        }

        public override T Deserialize<T>(byte[] data)
        {
            data = Decrypt(data, Encryption, EncryptionKey);
            data = Decompress(data);
            var json = Encoding.UTF8.GetString(data);
            return JsonSerializer.Deserialize<T>(json, _options);
        }

        public string SerializeToString<T>(T obj) where T : class
        {
            return JsonSerializer.Serialize(obj, obj.GetType(), _options);
        }

        public T DeserializeFromString<T>(string json) where T : class
        {
            return JsonSerializer.Deserialize<T>(json, _options);
        }
    }

    public sealed class XMLFormatter : FormatterBase
    {
        private readonly TypeCache _typeCache = TypeCache.Instance;
        private readonly XmlWriterSettings _settings;
        private readonly XmlSerializerNamespaces _namespaces;

        public XMLFormatter()
        {
            _settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = Environment.NewLine,
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = false
            };
            _namespaces = new XmlSerializerNamespaces();
            _namespaces.Add("", "");
        }

        public override byte[] Serialize<T>(T obj)
        {
            using var stream = new MemoryStream();
            using var writer = XmlWriter.Create(stream, _settings);
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, obj, _namespaces);
            writer.Flush();
            var data = stream.ToArray();
            data = Compress(data, Compression);
            data = Encrypt(data, Encryption, EncryptionKey);
            return data;
        }

        public override T Deserialize<T>(byte[] data)
        {
            data = Decrypt(data, Encryption, EncryptionKey);
            data = Decompress(data);
            using var stream = new MemoryStream(data);
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(stream);
        }

        public string SerializeToString<T>(T obj) where T : class
        {
            using var writer = new StringWriter();
            var serializer = new XmlSerializer(typeof(T));
            serializer.Serialize(writer, obj, _namespaces);
            return writer.ToString();
        }

        public T DeserializeFromString<T>(string xml) where T : class
        {
            using var reader = new StringReader(xml);
            var serializer = new XmlSerializer(typeof(T));
            return (T)serializer.Deserialize(reader);
        }
    }

    public sealed class CustomBinaryFormatter : FormatterBase
    {
        private readonly TypeCache _typeCache = TypeCache.Instance;
        private const int MagicNumber = 0x53415645;
        private const int Version = 1;

        public override byte[] Serialize<T>(T obj)
        {
            using var stream = new MemoryStream();
            using var writer = new CustomBinaryWriter(stream);
            writer.Write(MagicNumber);
            writer.Write(Version);
            writer.Write(_typeCache.GetTypeId(typeof(T)));
            SerializeValue(writer, obj, typeof(T));
            var data = stream.ToArray();
            var checksum = ComputeChecksum(data);
            using var finalStream = new MemoryStream();
            using var finalWriter = new BinaryWriter(finalStream);
            finalWriter.Write(data.Length);
            finalWriter.Write(data);
            finalWriter.Write(checksum);
            data = finalStream.ToArray();
            data = Compress(data, Compression);
            data = Encrypt(data, Encryption, EncryptionKey);
            return data;
        }

        public override T Deserialize<T>(byte[] data)
        {
            data = Decrypt(data, Encryption, EncryptionKey);
            data = Decompress(data);
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);
            var length = reader.ReadInt32();
            var payload = reader.ReadBytes(length);
            var checksum = reader.ReadUInt32();
            if (ComputeChecksum(payload) != checksum)
                throw new InvalidOperationException("Checksum mismatch - data may be corrupted");
            using var payloadStream = new MemoryStream(payload);
            using var customReader = new CustomBinaryReader(payloadStream);
            var magic = customReader.ReadInt32();
            if (magic != MagicNumber)
                throw new InvalidOperationException("Invalid magic number");
            var ver = customReader.ReadInt32();
            var typeId = customReader.ReadInt32();
            return (T)DeserializeValue(customReader, _typeCache.GetType(typeId));
        }

        private uint ComputeChecksum(byte[] data)
        {
            uint hash = 2166136261;
            foreach (var b in data)
            {
                hash ^= b;
                hash *= 16777619;
            }
            return hash;
        }

        private void SerializeValue(CustomBinaryWriter writer, object value, Type type)
        {
            if (value == null)
            {
                writer.Write((byte)0);
                return;
            }
            writer.Write((byte)1);

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                SerializePrimitive(writer, value, type);
            }
            else if (type.IsEnum)
            {
                writer.Write((byte)5);
                writer.Write(Convert.ToInt64(value));
            }
            else if (type.IsArray)
            {
                SerializeArray(writer, value as Array, type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                SerializeList(writer, value as IList, type);
            }
            else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                SerializeDictionary(writer, value as IDictionary, type);
            }
            else if (value is Guid g)
            {
                writer.Write((byte)10);
                writer.Write(g.ToByteArray());
            }
            else if (value is ISerializable serializable)
            {
                writer.Write((byte)20);
                writer.Write(_typeCache.GetTypeId(type));
                serializable.Serialize(new BinarySerializerAdapter(writer));
            }
            else
            {
                SerializeObject(writer, value, type);
            }
        }

        private void SerializePrimitive(CustomBinaryWriter writer, object value, Type type)
        {
            writer.Write(type);
            if (type == typeof(bool)) writer.Write((bool)value);
            else if (type == typeof(int)) writer.Write7BitEncodedInt((int)value);
            else if (type == typeof(float)) writer.Write((float)value);
            else if (type == typeof(double)) writer.Write((double)value);
            else if (type == typeof(string)) writer.Write((string)value);
            else if (type == typeof(byte)) writer.Write((byte)value);
            else if (type == typeof(long)) writer.Write((long)value);
            else if (type == typeof(short)) writer.Write((short)value);
            else writer.Write(value?.ToString() ?? string.Empty);
        }

        private void SerializeArray(CustomBinaryWriter writer, Array array, Type type)
        {
            writer.Write((byte)30);
            writer.Write7BitEncodedInt(array.Length);
            var elementType = type.GetElementType();
            foreach (var item in array)
            {
                SerializeValue(writer, item, elementType);
            }
        }

        private void SerializeList(CustomBinaryWriter writer, IList list, Type type)
        {
            writer.Write((byte)31);
            writer.Write7BitEncodedInt(list.Count);
            var elementType = type.GetGenericArguments()[0];
            foreach (var item in list)
            {
                SerializeValue(writer, item, elementType);
            }
        }

        private void SerializeDictionary(CustomBinaryWriter writer, IDictionary dict, Type type)
        {
            writer.Write((byte)32);
            writer.Write7BitEncodedInt(dict.Count);
            var keyType = type.GetGenericArguments()[0];
            var valueType = type.GetGenericArguments()[1];
            foreach (DictionaryEntry entry in dict)
            {
                SerializeValue(writer, entry.Key, keyType);
                SerializeValue(writer, entry.Value, valueType);
            }
        }

        private void SerializeObject(CustomBinaryWriter writer, object obj, Type type)
        {
            writer.Write((byte)40);
            writer.Write(_typeCache.GetTypeId(type));
            var fields = GetSerializableFields(type);
            writer.Write7BitEncodedInt(fields.Count);
            foreach (var field in fields)
            {
                writer.Write(field.Name);
                SerializeValue(writer, field.GetValue(obj), field.FieldType);
            }
        }

        private object DeserializeValue(CustomBinaryReader reader, Type expectedType)
        {
            if (reader.ReadByte() == 0)
                return null;

            return ReadByMarker(reader, expectedType);
        }

        private object ReadByMarker(CustomBinaryReader reader, Type expectedType)
        {
            byte marker = reader.PeekByte();
            switch (marker)
            {
                case 0: reader.ReadByte(); return null;
                case 1: reader.ReadByte(); return ReadPrimitive(reader, expectedType);
                case 5: reader.ReadByte(); return Enum.ToObject(expectedType, reader.ReadInt64());
                case 10: reader.ReadByte(); return new Guid(reader.ReadBytes(16));
                case 20: reader.ReadByte(); return ReadSerializable(reader);
                case 30: reader.ReadByte(); return ReadArray(reader, expectedType);
                case 31: reader.ReadByte(); return ReadList(reader, expectedType);
                case 32: reader.ReadByte(); return ReadDictionary(reader, expectedType);
                case 40: reader.ReadByte(); return ReadObject(reader);
                default: reader.ReadByte(); return ReadPrimitive(reader, expectedType);
            }
        }

        private object ReadPrimitive(CustomBinaryReader reader, Type type)
        {
            if (type == typeof(bool)) return reader.ReadBoolean();
            if (type == typeof(int)) return reader.Read7BitEncodedInt();
            if (type == typeof(float)) return reader.ReadSingle();
            if (type == typeof(double)) return reader.ReadDouble();
            if (type == typeof(string)) return reader.ReadString();
            if (type == typeof(byte)) return reader.ReadByte();
            if (type == typeof(long)) return reader.ReadInt64();
            if (type == typeof(short)) return reader.ReadInt16();
            return null;
        }

        private Array ReadArray(CustomBinaryReader reader, Type arrayType)
        {
            int length = reader.Read7BitEncodedInt();
            var elementType = arrayType.GetElementType();
            var array = Array.CreateInstance(elementType, length);
            for (int i = 0; i < length; i++)
            {
                array.SetValue(DeserializeValue(reader, elementType), i);
            }
            return array;
        }

        private IList ReadList(CustomBinaryReader reader, Type listType)
        {
            int count = reader.Read7BitEncodedInt();
            var elementType = listType.GetGenericArguments()[0];
            var list = (IList)Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType));
            for (int i = 0; i < count; i++)
            {
                list.Add(DeserializeValue(reader, elementType));
            }
            return list;
        }

        private IDictionary ReadDictionary(CustomBinaryReader reader, Type dictType)
        {
            int count = reader.Read7BitEncodedInt();
            var keyType = dictType.GetGenericArguments()[0];
            var valueType = dictType.GetGenericArguments()[1];
            var dict = (IDictionary)Activator.CreateInstance(typeof(Dictionary<,>).MakeGenericType(keyType, valueType));
            for (int i = 0; i < count; i++)
            {
                var key = DeserializeValue(reader, keyType);
                var value = DeserializeValue(reader, valueType);
                dict.Add(key, value);
            }
            return dict;
        }

        private object ReadObject(CustomBinaryReader reader)
        {
            var typeId = reader.Read7BitEncodedInt();
            var type = _typeCache.GetType(typeId);
            var obj = Activator.CreateInstance(type);
            int fieldCount = reader.Read7BitEncodedInt();
            for (int i = 0; i < fieldCount; i++)
            {
                var fieldName = reader.ReadString();
                var field = type.GetField(fieldName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                if (field != null)
                {
                    field.SetValue(obj, DeserializeValue(reader, field.FieldType));
                }
            }
            return obj;
        }

        private object ReadSerializable(CustomBinaryReader reader)
        {
            var typeId = reader.Read7BitEncodedInt();
            var type = _typeCache.GetType(typeId);
            var obj = Activator.CreateInstance(type);
            if (obj is ISerializable serializable)
            {
                serializable.Deserialize(new BinarySerializerAdapter(reader));
            }
            return obj;
        }

        private List<FieldInfo> GetSerializableFields(Type type)
        {
            var fields = new List<FieldInfo>();
            while (type != null && type != typeof(object))
            {
                foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (field.GetCustomAttribute<HideInInspectorAttribute>() != null)
                        continue;
                    if (field.IsPrivate && field.GetCustomAttribute<SerializePrivateFieldAttribute>() == null
                        && field.GetCustomAttribute<SerializeFieldAttribute>() == null)
                        continue;
                    fields.Add(field);
                }
                type = type.BaseType;
            }
            return fields;
        }

        private class BinarySerializerAdapter : ISerializer
        {
            private readonly CustomBinaryWriter _writer;
            public bool IsWriting => true;
            public BinarySerializerAdapter(CustomBinaryWriter writer) => _writer = writer;
            public void WriteBool(string name, bool value) => _writer.Write(value);
            public void WriteInt(string name, int value) => _writer.Write7BitEncodedInt(value);
            public void WriteFloat(string name, float value) => _writer.Write(value);
            public void WriteDouble(string name, double value) => _writer.Write(value);
            public void WriteString(string name, string value) => _writer.Write(value);
            public void WriteGuid(string name, Guid value) => _writer.Write(value.ToByteArray());
            public void WriteBytes(string name, byte[] value) { _writer.Write7BitEncodedInt(value.Length); _writer.Write(value); }
            public void WriteVector2(string name, float x, float y) { _writer.Write(x); _writer.Write(y); }
            public void WriteVector3(string name, float x, float y, float z) { _writer.Write(x); _writer.Write(y); _writer.Write(z); }
            public void WriteColor(string name, byte r, byte g, byte b, byte a) { _writer.Write(r); _writer.Write(g); _writer.Write(b); _writer.Write(a); }
            public void WriteObjectBegin(string name, Type type) => _writer.Write7BitEncodedInt(TypeCache.Instance.GetTypeId(type));
            public void WriteObjectEnd() { }
            public void WriteArrayBegin(string name, int count) => _writer.Write7BitEncodedInt(count);
            public void WriteArrayEnd() { }
            public void WriteDictionaryBegin(string name, int count) => _writer.Write7BitEncodedInt(count);
            public void WriteDictionaryEnd() { }
            public void WriteNull(string name) => _writer.Write(false);
        }

        private class BinaryDeserializerAdapter : IDeserializer
        {
            private readonly CustomBinaryReader _reader;
            public bool IsReading => true;
            public BinaryDeserializerAdapter(CustomBinaryReader reader) => _reader = reader;
            public bool ReadBool(string name, bool defaultValue = default) => _reader.ReadBoolean();
            public int ReadInt(string name, int defaultValue = default) => _reader.Read7BitEncodedInt();
            public float ReadFloat(string name, float defaultValue = default) => _reader.ReadSingle();
            public double ReadDouble(string name, double defaultValue = default) => _reader.ReadDouble();
            public string ReadString(string name, string defaultValue = default) => _reader.ReadString();
            public Guid ReadGuid(string name, Guid defaultValue = default) => new Guid(_reader.ReadBytes(16));
            public byte[] ReadBytes(string name, byte[] defaultValue = null) { var len = _reader.Read7BitEncodedInt(); return _reader.ReadBytes(len); }
            public (float X, float Y) ReadVector2(string name) => (_reader.ReadSingle(), _reader.ReadSingle());
            public (float X, float Y, float Z) ReadVector3(string name) => (_reader.ReadSingle(), _reader.ReadSingle(), _reader.ReadSingle());
            public (byte R, byte G, byte B, byte A) ReadColor(string name) => (_reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte(), _reader.ReadByte());
            public bool ReadObjectBegin(string name, out Type type) { type = TypeCache.Instance.GetType(_reader.Read7BitEncodedInt()); return true; }
            public void ReadObjectEnd() { }
            public bool ReadArrayBegin(string name, out int count) { count = _reader.Read7BitEncodedInt(); return true; }
            public void ReadArrayEnd() { }
            public bool ReadDictionaryBegin(string name, out int count) { count = _reader.Read7BitEncodedInt(); return true; }
            public void ReadDictionaryEnd() { }
            public bool IsNull(string name) => _reader.ReadByte() == 0;
            public bool HasProperty(string name) => true;
        }
    }

    public sealed class CustomBinaryWriter : BinaryWriter
    {
        public CustomBinaryWriter(Stream output) : base(output, Encoding.UTF8, leaveOpen: true) { }
        public CustomBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen) { }

        public void Write7BitEncodedInt(int value)
        {
            uint uvalue = (uint)value;
            while (uvalue >= 0x80)
            {
                Write((byte)(uvalue | 0x80));
                uvalue >>= 7;
            }
            Write((byte)uvalue);
        }

        public byte PeekByte()
        {
            var pos = BaseStream.Position;
            var b = ReadByte();
            BaseStream.Position = pos;
            return (byte)b;
        }
    }

    public sealed class CustomBinaryReader : BinaryReader
    {
        public CustomBinaryReader(Stream input) : base(input, Encoding.UTF8, leaveOpen: true) { }
        public CustomBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen) { }

        public int Read7BitEncodedInt()
        {
            int value = 0;
            int shift = 0;
            byte b;
            do
            {
                b = ReadByte();
                value |= (b & 0x7F) << shift;
                shift += 7;
            } while ((b & 0x80) != 0);
            return value;
        }
    }

    public sealed class TypeCache
    {
        private static readonly Lazy<TypeCache> _instance = new Lazy<TypeCache>(() => new TypeCache());
        public static TypeCache Instance => _instance.Value;

        private readonly Dictionary<Type, int> _typeToId = new Dictionary<Type, int>();
        private readonly Dictionary<int, Type> _idToType = new Dictionary<int, Type>();
        private readonly Dictionary<string, Type> _typeAliases = new Dictionary<string, Type>();
        private readonly Dictionary<Type, ISerializer> _customSerializers = new Dictionary<Type, ISerializer>();
        private int _nextId = 1;
        private readonly object _lock = new object();

        private TypeCache()
        {
            RegisterPrimitives();
        }

        private void RegisterPrimitives()
        {
            Register(typeof(bool));
            Register(typeof(byte));
            Register(typeof(sbyte));
            Register(typeof(short));
            Register(typeof(ushort));
            Register(typeof(int));
            Register(typeof(uint));
            Register(typeof(long));
            Register(typeof(ulong));
            Register(typeof(float));
            Register(typeof(double));
            Register(typeof(decimal));
            Register(typeof(char));
            Register(typeof(string));
            Register(typeof(object));
            Register(typeof(Guid));
            Register(typeof(DateTime));
            Register(typeof(TimeSpan));
            Register(typeof(Vector2));
            Register(typeof(Vector3));
            Register(typeof(Color));
        }

        public void Register(Type type)
        {
            lock (_lock)
            {
                if (!_typeToId.ContainsKey(type))
                {
                    _typeToId[type] = _nextId;
                    _idToType[_nextId] = type;
                    _nextId++;
                }
            }
        }

        public int GetTypeId(Type type)
        {
            lock (_lock)
            {
                if (_typeToId.TryGetValue(type, out var id))
                    return id;
                Register(type);
                return _typeToId[type];
            }
        }

        public Type GetType(int id)
        {
            lock (_lock)
            {
                if (_idToType.TryGetValue(id, out var type))
                    return type;
                throw new InvalidOperationException($"Type with ID {id} not found");
            }
        }

        public void RegisterAlias(string alias, Type type)
        {
            lock (_lock)
            {
                _typeAliases[alias] = type;
            }
        }

        public Type GetByAlias(string alias)
        {
            lock (_lock)
            {
                if (_typeAliases.TryGetValue(alias, out var type))
                    return type;
                throw new InvalidOperationException($"Type alias '{alias}' not found");
            }
        }

        public void RegisterCustomSerializer(Type type, ISerializer serializer)
        {
            lock (_lock)
            {
                _customSerializers[type] = serializer;
            }
        }

        public bool HasCustomSerializer(Type type)
        {
            lock (_lock)
            {
                return _customSerializers.ContainsKey(type);
            }
        }

        public ISerializer GetCustomSerializer(Type type)
        {
            lock (_lock)
            {
                if (_customSerializers.TryGetValue(type, out var serializer))
                    return serializer;
                return null;
            }
        }
    }

    public sealed class TypeRegistry
    {
        private static readonly Lazy<TypeRegistry> _instance = new Lazy<TypeRegistry>(() => new TypeRegistry());
        public static TypeRegistry Instance => _instance.Value;

        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        private TypeRegistry()
        {
            ScanAssemblies();
        }

        private void ScanAssemblies()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        var alias = type.GetCustomAttribute<TypeAliasAttribute>();
                        if (alias != null)
                        {
                            Register(alias.Alias, type);
                        }

                        if (type.GetCustomAttribute<SerializableAttribute>() != null)
                        {
                            TypeCache.Instance.Register(type);
                        }
                    }
                }
                catch { }
            }
        }

        public void Register(string name, Type type)
        {
            _types[name] = type;
        }

        public Type Get(string name)
        {
            if (_types.TryGetValue(name, out var type))
                return type;
            return Type.GetType(name);
        }

        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            _factories[typeof(T)] = () => factory();
        }

        public T Create<T>() where T : class
        {
            if (_factories.TryGetValue(typeof(T), out var factory))
                return factory() as T;
            return Activator.CreateInstance<T>();
        }

        public object Create(Type type)
        {
            if (_factories.TryGetValue(type, out var factory))
                return factory();
            return Activator.CreateInstance(type);
        }
    }

    [Serializable]
    public sealed class SceneSave
    {
        public string SceneName { get; set; } = string.Empty;
        public int BuildIndex { get; set; }
        public DateTime SavedAt { get; set; }
        public int Version { get; set; } = 1;
        public List<GameObjectSave> GameObjects { get; set; } = new List<GameObjectSave>();
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public static SceneSave Create(string sceneName, int buildIndex = -1)
        {
            return new SceneSave
            {
                SceneName = sceneName,
                BuildIndex = buildIndex,
                SavedAt = DateTime.Now,
                Version = 1
            };
        }
    }

    [Serializable]
    public sealed class GameObjectSave
    {
        public Guid InstanceId { get; set; }
        public Guid? PrefabId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Tag { get; set; } = "Untagged";
        public bool IsActive { get; set; } = true;
        public Guid? ParentId { get; set; }
        public List<ComponentSave> Components { get; set; } = new List<ComponentSave>();
        public TransformSave Transform { get; set; } = new TransformSave();
    }

    [Serializable]
    public sealed class ComponentSave
    {
        public Guid InstanceId { get; set; }
        public string TypeName { get; set; } = string.Empty;
        public string AssemblyName { get; set; } = string.Empty;
        public bool IsEnabled { get; set; } = true;
        public Dictionary<string, byte[]> SerializedData { get; set; } = new Dictionary<string, byte[]>();
    }

    [Serializable]
    public sealed class TransformSave
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float Rotation { get; set; }
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public int HierarchyOrder { get; set; }
    }

    [Serializable]
    public sealed class SaveMetadata
    {
        public string SaveId { get; set; } = Guid.NewGuid().ToString();
        public string DisplayName { get; set; } = string.Empty;
        public DateTime SavedAt { get; set; }
        public TimeSpan PlayTime { get; set; }
        public string SceneName { get; set; } = string.Empty;
        public int BuildIndex { get; set; }
        public string ThumbnailPath { get; set; } = string.Empty;
        public byte[] ThumbnailData { get; set; }
        public long FileSize { get; set; }
        public int Version { get; set; } = 1;
        public string Checksum { get; set; } = string.Empty;
        public Dictionary<string, string> CustomData { get; set; } = new Dictionary<string, string>();
    }

    [Serializable]
    public sealed class GamePreferences
    {
        public string Category { get; set; } = "General";
        public Dictionary<string, PreferenceValue> Values { get; set; } = new Dictionary<string, PreferenceValue>();
    }

    [Serializable]
    public class PreferenceValue
    {
        public object Value { get; set; }
        public string TypeName { get; set; } = string.Empty;

        public PreferenceValue() { }

        public PreferenceValue(object value)
        {
            Value = value;
            TypeName = value?.GetType().FullName ?? "System.String";
        }
    }

    public sealed class SaveSlot
    {
        public int SlotIndex { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public DateTime LastSaved { get; set; }
        public TimeSpan PlayTime { get; set; }
        public string SceneName { get; set; } = string.Empty;
        public byte[] Thumbnail { get; set; }
        public bool IsCorrupted { get; set; }
        public bool IsEmpty => string.IsNullOrEmpty(DisplayName);
    }

    public interface IStorageBackend
    {
        Task<bool> ExistsAsync(string path);
        Task<byte[]> ReadAsync(string path);
        Task WriteAsync(string path, byte[] data);
        Task DeleteAsync(string path);
        Task<string[]> ListFilesAsync(string directory);
        Task EnsureDirectoryExistsAsync(string path);
    }

    public sealed class FileStorage : IStorageBackend
    {
        private readonly string _basePath;

        public FileStorage(string basePath)
        {
            _basePath = Path.GetFullPath(basePath);
        }

        public async Task<bool> ExistsAsync(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            return await Task.FromResult(File.Exists(fullPath));
        }

        public async Task<byte[]> ReadAsync(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            return await File.ReadAllBytesAsync(fullPath);
        }

        public async Task WriteAsync(string path, byte[] data)
        {
            var fullPath = Path.Combine(_basePath, path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            await File.WriteAllBytesAsync(fullPath, data);
        }

        public async Task DeleteAsync(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
            await Task.CompletedTask;
        }

        public async Task<string[]> ListFilesAsync(string directory)
        {
            var fullPath = Path.Combine(_basePath, directory);
            if (!Directory.Exists(fullPath))
                return Array.Empty<string>();
            var files = Directory.GetFiles(fullPath, "*.*", SearchOption.AllDirectories);
            return await Task.FromResult(files.Select(f => f.Substring(_basePath.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)).ToArray());
        }

        public async Task EnsureDirectoryExistsAsync(string path)
        {
            var fullPath = Path.Combine(_basePath, path);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            await Task.CompletedTask;
        }
    }

    public sealed class PlayerPrefsStorage : IStorageBackend
    {
        private readonly Dictionary<string, byte[]> _cache = new Dictionary<string, byte[]>();
        private readonly string _keyPrefix;

        public PlayerPrefsStorage(string keyPrefix = "WPFEngine_")
        {
            _keyPrefix = keyPrefix;
        }

        public Task<bool> ExistsAsync(string path)
        {
            var key = _keyPrefix + path;
            return Task.FromResult(_cache.ContainsKey(key) || Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"Software\{path}") != null);
        }

        public async Task<byte[]> ReadAsync(string path)
        {
            var key = _keyPrefix + path;
            if (_cache.TryGetValue(key, out var data))
                return data;
            try
            {
                using var regKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey($@"Software\{path}");
                if (regKey != null)
                {
                    var base64 = regKey.GetValue("Data") as string;
                    if (!string.IsNullOrEmpty(base64))
                        return Convert.FromBase64String(base64);
                }
            }
            catch { }
            return await Task.FromResult(Array.Empty<byte>());
        }

        public async Task WriteAsync(string path, byte[] data)
        {
            var key = _keyPrefix + path;
            _cache[key] = data;
            try
            {
                using var regKey = Microsoft.Win32.Registry.CurrentUser.CreateSubKey($@"Software\{path}");
                regKey?.SetValue("Data", Convert.ToBase64String(data));
            }
            catch { }
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string path)
        {
            var key = _keyPrefix + path;
            _cache.Remove(key);
            try
            {
                Microsoft.Win32.Registry.CurrentUser.DeleteSubKeyTree($@"Software\{path}", false);
            }
            catch { }
            await Task.CompletedTask;
        }

        public Task<string[]> ListFilesAsync(string directory)
        {
            return Task.FromResult(_cache.Keys.Where(k => k.StartsWith(_keyPrefix + directory)).Select(k => k.Substring(_keyPrefix.Length)).ToArray());
        }

        public Task EnsureDirectoryExistsAsync(string path)
        {
            return Task.CompletedTask;
        }
    }

    public interface ICloudStorage
    {
        Task<bool> IsAvailableAsync();
        Task<byte[]> DownloadAsync(string remotePath);
        Task UploadAsync(string remotePath, byte[] data);
        Task DeleteAsync(string remotePath);
        Task<string[]> ListFilesAsync(string directory);
    }

    public sealed class CloudStorage : ICloudStorage
    {
        private readonly string _apiEndpoint;
        private readonly string _apiKey;

        public CloudStorage(string apiEndpoint, string apiKey)
        {
            _apiEndpoint = apiEndpoint;
            _apiKey = apiKey;
        }

        public Task<bool> IsAvailableAsync()
        {
            return Task.FromResult(false);
        }

        public Task<byte[]> DownloadAsync(string remotePath)
        {
            throw new NotImplementedException("Cloud storage not implemented");
        }

        public Task UploadAsync(string remotePath, byte[] data)
        {
            throw new NotImplementedException("Cloud storage not implemented");
        }

        public Task DeleteAsync(string remotePath)
        {
            throw new NotImplementedException("Cloud storage not implemented");
        }

        public Task<string[]> ListFilesAsync(string directory)
        {
            return Task.FromResult(Array.Empty<string>());
        }
    }

    public sealed class SecureStorage : IStorageBackend
    {
        private readonly IStorageBackend _inner;
        private readonly string _encryptionKey;

        public SecureStorage(IStorageBackend inner, string encryptionKey)
        {
            _inner = inner;
            _encryptionKey = encryptionKey;
        }

        public async Task<bool> ExistsAsync(string path)
        {
            return await _inner.ExistsAsync(path + ".enc");
        }

        public async Task<byte[]> ReadAsync(string path)
        {
            var data = await _inner.ReadAsync(path + ".enc");
            return Decrypt(data);
        }

        public async Task WriteAsync(string path, byte[] data)
        {
            var encrypted = Encrypt(data);
            await _inner.WriteAsync(path + ".enc", encrypted);
        }

        public async Task DeleteAsync(string path)
        {
            await _inner.DeleteAsync(path + ".enc");
        }

        public async Task<string[]> ListFilesAsync(string directory)
        {
            var files = await _inner.ListFilesAsync(directory);
            return files.Where(f => f.EndsWith(".enc")).Select(f => f.Substring(0, f.Length - 4)).ToArray();
        }

        public Task EnsureDirectoryExistsAsync(string path)
        {
            return _inner.EnsureDirectoryExistsAsync(path);
        }

        private byte[] Encrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            using var aes = Aes.Create();
            aes.KeySize = 256;
            aes.GenerateIV();
            using var deriveBytes = new Rfc2898DeriveBytes(_encryptionKey, aes.IV, 1000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32);

            using var encryptor = aes.CreateEncryptor();
            var encrypted = encryptor.TransformFinalBlock(data, 0, data.Length);
            var result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);
            return result;
        }

        private byte[] Decrypt(byte[] data)
        {
            if (data == null || data.Length == 0)
                return data;

            using var aes = Aes.Create();
            aes.KeySize = 256;
            var iv = new byte[16];
            Buffer.BlockCopy(data, 0, iv, 0, 16);
            aes.IV = iv;
            using var deriveBytes = new Rfc2898DeriveBytes(_encryptionKey, iv, 1000, HashAlgorithmName.SHA256);
            aes.Key = deriveBytes.GetBytes(32);

            using var decryptor = aes.CreateDecryptor();
            return decryptor.TransformFinalBlock(data, 16, data.Length - 16);
        }
    }

    public sealed class HMACSignature
    {
        private readonly byte[] _key;

        public HMACSignature(string key)
        {
            _key = Encoding.UTF8.GetBytes(key);
        }

        public byte[] ComputeSignature(byte[] data)
        {
            using var hmac = new HMACSHA256(_key);
            return hmac.ComputeHash(data);
        }

        public bool VerifySignature(byte[] data, byte[] signature)
        {
            var computed = ComputeSignature(data);
            if (computed.Length != signature.Length)
                return false;

            var result = 0;
            for (int i = 0; i < computed.Length; i++)
            {
                result |= computed[i] ^ signature[i];
            }
            return result == 0;
        }

        public string ComputeChecksum(byte[] data)
        {
            var hash = ComputeSignature(data);
            return Convert.ToBase64String(hash);
        }
    }

    public sealed class AntiCheatChecksum
    {
        private readonly List<Func<byte[], bool>> _validators = new List<Func<byte[], bool>>();

        public void RegisterValidator(Func<byte[], bool> validator)
        {
            _validators.Add(validator);
        }

        public bool Validate(byte[] data)
        {
            foreach (var validator in _validators)
            {
                if (!validator(data))
                    return false;
            }
            return true;
        }

        public string GenerateHash(byte[] data)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(data);
            return Convert.ToBase64String(hash);
        }

        public bool VerifyHash(byte[] data, string expectedHash)
        {
            var actualHash = GenerateHash(data);
            return actualHash == expectedHash;
        }
    }

    public sealed class SaveManager
    {
        private static readonly Lazy<SaveManager> _instance = new Lazy<SaveManager>(() => new SaveManager());
        public static SaveManager Instance => _instance.Value;

        private IStorageBackend _storage;
        private FormatterBase _formatter;
        private HMACSignature _hmac;
        private AntiCheatChecksum _antiCheat;
        private readonly TypeCache _typeCache = TypeCache.Instance;
        private readonly Dictionary<Guid, object> _objectCache = new Dictionary<Guid, object>();
        private readonly Dictionary<object, Guid> _referenceMap = new Dictionary<object, Guid>();
        private readonly List<Guid> _pendingReferences = new List<Guid>();
        private TimeSpan _autoSaveInterval;
        private DateTime _lastAutoSave;
        private bool _isAutoSaveEnabled;
        private int _maxSaveSlots = 10;
        private bool _useCompression = true;
        private bool _useEncryption = false;
        private string _encryptionKey = string.Empty;

        public event EventHandler<SaveProgressEventArgs> SaveProgress;
        public event EventHandler<LoadProgressEventArgs> LoadProgress;
        public event EventHandler<AutoSaveEventArgs> AutoSaveTriggered;

        public IStorageBackend Storage
        {
            get => _storage;
            set => _storage = value ?? throw new ArgumentNullException(nameof(value));
        }

        public FormatterBase Formatter
        {
            get => _formatter;
            set => _formatter = value ?? new BinaryFormatter();
        }

        public bool UseCompression
        {
            get => _useCompression;
            set => _useCompression = value;
        }

        public bool UseEncryption
        {
            get => _useEncryption;
            set => _useEncryption = value;
        }

        public string EncryptionKey
        {
            get => _encryptionKey;
            set => _encryptionKey = value ?? string.Empty;
        }

        public int MaxSaveSlots
        {
            get => _maxSaveSlots;
            set => _maxSaveSlots = Math.Max(1, value);
        }

        public TimeSpan AutoSaveInterval
        {
            get => _autoSaveInterval;
            set => _autoSaveInterval = value;
        }

        public bool IsAutoSaveEnabled
        {
            get => _isAutoSaveEnabled;
            set => _isAutoSaveEnabled = value;
        }

        private SaveManager()
        {
            _storage = new FileStorage(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WPFEngine", "Saves"));
            _formatter = new BinaryFormatter { Compression = CompressionLevel.Optimal };
            _hmac = new HMACSignature(GetDefaultKey());
            _antiCheat = new AntiCheatChecksum();
            _autoSaveInterval = TimeSpan.FromMinutes(5);
            _lastAutoSave = DateTime.MinValue;
        }

        private string GetDefaultKey()
        {
            var machineId = Environment.MachineName + Environment.UserName;
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(machineId));
            return Convert.ToBase64String(hash);
        }

        public async Task<bool> SaveAsync(string path, object obj, SaveMetadata metadata = null)
        {
            try
            {
                SaveProgress?.Invoke(this, new SaveProgressEventArgs(0, "Starting save..."));
                await _storage.EnsureDirectoryExistsAsync(Path.GetDirectoryName(path) ?? string.Empty);

                var data = SerializeObject(obj);
                SaveProgress?.Invoke(this, new SaveProgressEventArgs(30, "Serializing data..."));

                if (_useCompression)
                {
                    _formatter.Compression = CompressionLevel.Optimal;
                }

                if (_useEncryption && !string.IsNullOrEmpty(_encryptionKey))
                {
                    _formatter.Encryption = EncryptionType.AES256;
                    _formatter.EncryptionKey = _encryptionKey;
                }

                var saveData = _formatter.Serialize(obj);
                SaveProgress?.Invoke(this, new SaveProgressEventArgs(60, "Compressing data..."));

                var signature = _hmac.ComputeSignature(saveData);
                SaveProgress?.Invoke(this, new SaveProgressEventArgs(80, "Signing save..."));

                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                writer.Write(metadata?.SaveId ?? Guid.NewGuid().ToString());
                writer.Write(metadata != null ? _formatter.Serialize(metadata) : Array.Empty<byte>());
                writer.Write(saveData);
                writer.Write(signature);
                await _storage.WriteAsync(path, stream.ToArray());
                SaveProgress?.Invoke(this, new SaveProgressEventArgs(100, "Save complete"));
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Save failed: {ex.Message}");
                return false;
            }
        }

        public async Task<(T Object, SaveMetadata Metadata)> LoadAsync<T>(string path) where T : class
        {
            return await LoadAsync(path, typeof(T));
        }

        public async Task<(object Object, SaveMetadata Metadata)> LoadAsync(string path, Type targetType)
        {
            try
            {
                LoadProgress?.Invoke(this, new LoadProgressEventArgs(0, "Loading save file..."));

                if (!await _storage.ExistsAsync(path))
                    throw new FileNotFoundException("Save file not found", path);

                var data = await _storage.ReadAsync(path);
                LoadProgress?.Invoke(this, new LoadProgressEventArgs(10, "Reading file..."));

                using var stream = new MemoryStream(data);
                using var reader = new BinaryReader(stream);

                var saveId = reader.ReadString();
                var metadataBytes = reader.ReadBytes(reader.ReadInt32());
                var saveData = reader.ReadBytes(reader.ReadInt32());
                var signature = reader.ReadBytes(reader.ReadInt32());

                LoadProgress?.Invoke(this, new LoadProgressEventArgs(20, "Verifying integrity..."));

                if (!_hmac.VerifySignature(saveData, signature))
                {
                    if (_antiCheat.Validate(saveData))
                    {
                        throw new InvalidOperationException("Save file signature verification failed");
                    }
                    else
                    {
                        throw new InvalidOperationException("Save file appears to be corrupted or modified");
                    }
                }

                LoadProgress?.Invoke(this, new LoadProgressEventArgs(30, "Loading metadata..."));

                SaveMetadata metadata = null;
                if (metadataBytes.Length > 0)
                {
                    metadata = _formatter.Deserialize<SaveMetadata>(metadataBytes);
                }

                if (_useEncryption && !string.IsNullOrEmpty(_encryptionKey))
                {
                    _formatter.Encryption = EncryptionType.AES256;
                    _formatter.EncryptionKey = _encryptionKey;
                }

                LoadProgress?.Invoke(this, new LoadProgressEventArgs(50, "Deserializing data..."));
                var obj = _formatter.Deserialize(targetType, saveData);
                LoadProgress?.Invoke(this, new LoadProgressEventArgs(100, "Load complete"));
                return (obj, metadata);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Load failed: {ex.Message}");
                throw;
            }
        }

        public async Task<bool> SaveToSlotAsync(int slotIndex, object obj, string displayName, byte[] thumbnail = null)
        {
            if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
                throw new ArgumentOutOfRangeException(nameof(slotIndex));

            var metadata = new SaveMetadata
            {
                DisplayName = displayName,
                SavedAt = DateTime.Now,
                ThumbnailData = thumbnail
            };

            var path = $"slot_{slotIndex}.save";
            return await SaveAsync(path, obj, metadata);
        }

        public async Task<(object Object, SaveMetadata Metadata)> LoadFromSlotAsync(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
                throw new ArgumentOutOfRangeException(nameof(slotIndex));

            var path = $"slot_{slotIndex}.save";
            return await LoadAsync(path, typeof(object));
        }

        public async Task<SaveMetadata> GetSlotMetadataAsync(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
                return null;

            try
            {
                var path = $"slot_{slotIndex}.save";
                if (!await _storage.ExistsAsync(path))
                    return null;

                var data = await _storage.ReadAsync(path);
                using var stream = new MemoryStream(data);
                using var reader = new BinaryReader(stream);
                reader.ReadString();
                var metadataBytes = reader.ReadBytes(reader.ReadInt32());
                if (metadataBytes.Length > 0)
                    return _formatter.Deserialize<SaveMetadata>(metadataBytes);
                return null;
            }
            catch
            {
                return null;
            }
        }

        public async Task<List<SaveSlot>> GetAllSlotsAsync()
        {
            var slots = new List<SaveSlot>();
            for (int i = 0; i < _maxSaveSlots; i++)
            {
                var slot = new SaveSlot { SlotIndex = i };
                var metadata = await GetSlotMetadataAsync(i);
                if (metadata != null)
                {
                    slot.DisplayName = metadata.DisplayName;
                    slot.LastSaved = metadata.SavedAt;
                    slot.PlayTime = metadata.PlayTime;
                    slot.SceneName = metadata.SceneName;
                    slot.Thumbnail = metadata.ThumbnailData;
                }
                slots.Add(slot);
            }
            return slots;
        }

        public async Task DeleteSlotAsync(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _maxSaveSlots)
                return;
            var path = $"slot_{slotIndex}.save";
            await _storage.DeleteAsync(path);
        }

        public async Task<bool> RecoverCorruptedSaveAsync(string path)
        {
            try
            {
                if (!await _storage.ExistsAsync(path))
                    return false;

                var data = await _storage.ReadAsync(path);
                if (_hmac.VerifySignature(data, Array.Empty<byte>()))
                    return false;

                var signature = _hmac.ComputeSignature(data);
                using var stream = new MemoryStream();
                using var writer = new BinaryWriter(stream);
                writer.Write(Guid.NewGuid().ToString());
                writer.Write(Array.Empty<byte>());
                writer.Write(data);
                writer.Write(signature);
                await _storage.WriteAsync(path + ".recovered", stream.ToArray());
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<byte[]> SerializeObject(object obj)
        {
            return await Task.Run(() => _formatter.Serialize(obj));
        }

        public async Task<T> DeserializeObject<T>(byte[] data) where T : class
        {
            return await Task.Run(() => _formatter.Deserialize<T>(data));
        }

        public async Task<SceneSave> SerializeScene(WPFEngine.Scene.Scene scene)
        {
            return await Task.Run(() =>
            {
                var sceneSave = SceneSave.Create(scene.Name, scene.BuildIndex);

                foreach (var gameObject in scene.GetRootGameObjects())
                {
                    var goSave = SerializeGameObject(gameObject);
                    sceneSave.GameObjects.Add(goSave);
                }

                return sceneSave;
            });
        }

        public GameObjectSave SerializeGameObject(ECS.GameObject gameObject)
        {
            var save = new GameObjectSave
            {
                InstanceId = gameObject.InstanceId,
                Name = gameObject.Name,
                Tag = gameObject.Tag,
                IsActive = gameObject.IsActive,
                ParentId = gameObject.Parent?.InstanceId,
                Transform = new TransformSave
                {
                    PositionX = gameObject.Transform?.LocalPosition?.X ?? 0,
                    PositionY = gameObject.Transform?.LocalPosition?.Y ?? 0,
                    Rotation = gameObject.Transform?.LocalRotation ?? 0,
                    ScaleX = gameObject.Transform?.LocalScale?.X ?? 1,
                    ScaleY = gameObject.Transform?.LocalScale?.Y ?? 1
                }
            };

            foreach (var component in gameObject.GetComponents<ECS.Component>())
            {
                save.Components.Add(SerializeComponent(component));
            }

            foreach (var child in gameObject.Children)
            {
                var childSave = SerializeGameObject(child);
                sceneSave.GameObjects.Add(childSave);
            }

            return save;
        }

        private SceneSave sceneSave;

        public ComponentSave SerializeComponent(ECS.Component component)
        {
            var save = new ComponentSave
            {
                InstanceId = component.Id,
                TypeName = component.GetType().FullName,
                AssemblyName = component.GetType().Assembly.GetName().Name,
                IsEnabled = component.IsEnabled
            };

            if (component is ISerializable serializable)
            {
                using var stream = new MemoryStream();
                using var writer = new CustomBinaryWriter(stream);
                serializable.Serialize(new BinarySerializerAdapter(writer));
                save.SerializedData["ISerializable"] = stream.ToArray();
            }

            var fields = component.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (field.GetCustomAttribute<HideInInspectorAttribute>() != null)
                    continue;

                var shouldSerialize = field.GetCustomAttribute<SerializeFieldAttribute>() != null
                    || field.GetCustomAttribute<SerializePrivateFieldAttribute>() != null
                    || (field.IsPublic && !field.GetCustomAttribute<HideInInspectorAttribute>()?.GetType().Name.Contains("Hide") == true);

                if (shouldSerialize)
                {
                    try
                    {
                        var value = field.GetValue(component);
                        var fieldData = _formatter.Serialize(value ?? new object());
                        save.SerializedData[field.Name] = fieldData;
                    }
                    catch { }
                }
            }

            return save;
        }

        public async Task<WPFEngine.Scene.Scene> DeserializeScene(SceneSave sceneSave)
        {
            return await Task.Run(() =>
            {
                var scene = new WPFEngine.Scene.Scene(sceneSave.SceneName);
                var objectMap = new Dictionary<Guid, ECS.GameObject>();

                foreach (var goSave in sceneSave.GameObjects.Where(g => g.ParentId == null))
                {
                    var gameObject = DeserializeGameObject(goSave, scene, objectMap);
                    scene.AddGameObject(gameObject);
                }

                foreach (var kvp in objectMap)
                {
                    if (kvp.Value.Parent == null && kvp.Value.Transform != null)
                    {
                    }
                }

                return scene;
            });
        }

        public ECS.GameObject DeserializeGameObject(GameObjectSave save, WPFEngine.Scene.Scene scene, Dictionary<Guid, ECS.GameObject> objectMap)
        {
            var gameObject = new ECS.GameObject(save.Name)
            {
                Tag = save.Tag,
                IsActive = save.IsActive
            };

            objectMap[save.InstanceId] = gameObject;

            if (gameObject.Transform != null)
            {
                gameObject.Transform.LocalPosition = new Vector2(save.Transform.PositionX, save.Transform.PositionY);
                gameObject.Transform.LocalRotation = save.Transform.Rotation;
                if (save.Transform.ScaleX != 1 || save.Transform.ScaleY != 1)
                {
                }
            }

            foreach (var componentSave in save.Components)
            {
                var component = DeserializeComponent(componentSave, gameObject);
                if (component != null)
                {
                    gameObject.AddComponent(component);
                }
            }

            return gameObject;
        }

        public ECS.Component DeserializeComponent(ComponentSave save, ECS.GameObject gameObject)
        {
            var type = Type.GetType($"{save.TypeName}, {save.AssemblyName}");
            if (type == null)
                return null;

            ECS.Component component;
            try
            {
                component = Activator.CreateInstance(type) as ECS.Component;
                if (component == null)
                    return null;
            }
            catch
            {
                return null;
            }

            component.IsEnabled = save.IsEnabled;

            if (component is ISerializable serializable && save.SerializedData.TryGetValue("ISerializable", out var data))
            {
                using var stream = new MemoryStream(data);
                using var reader = new CustomBinaryReader(stream);
                serializable.Deserialize(new BinaryDeserializerAdapter(reader));
            }

            var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            foreach (var field in fields)
            {
                if (save.SerializedData.TryGetValue(field.Name, out var fieldData))
                {
                    try
                    {
                        var value = _formatter.Deserialize(field.FieldType, fieldData);
                        field.SetValue(component, value);
                    }
                    catch { }
                }
            }

            return component;
        }

        public async Task<GamePreferences> SerializePreferencesAsync(string category)
        {
            return await Task.Run(() => new GamePreferences { Category = category });
        }

        public async Task SavePreferencesAsync(string path, GamePreferences preferences)
        {
            await SaveAsync(path, preferences);
        }

        public async Task<GamePreferences> LoadPreferencesAsync(string path)
        {
            try
            {
                var result = await LoadAsync<GamePreferences>(path);
                return result.Object;
            }
            catch
            {
                return new GamePreferences();
            }
        }

        public void RegisterObject(Guid id, object obj)
        {
            _objectCache[id] = obj;
            _referenceMap[obj] = id;
        }

        public object GetObject(Guid id)
        {
            return _objectCache.TryGetValue(id, out var obj) ? obj : null;
        }

        public Guid GetReference(object obj)
        {
            return _referenceMap.TryGetValue(obj, out var id) ? id : Guid.Empty;
        }

        public void ClearCache()
        {
            _objectCache.Clear();
            _referenceMap.Clear();
            _pendingReferences.Clear();
        }

        public void CheckAutoSave()
        {
            if (!_isAutoSaveEnabled)
                return;

            if (DateTime.Now - _lastAutoSave >= _autoSaveInterval)
            {
                AutoSaveTriggered?.Invoke(this, new AutoSaveEventArgs(_lastAutoSave));
                _lastAutoSave = DateTime.Now;
            }
        }
    }

    public class SaveProgressEventArgs : EventArgs
    {
        public int Progress { get; }
        public string Status { get; }

        public SaveProgressEventArgs(int progress, string status)
        {
            Progress = progress;
            Status = status;
        }
    }

    public class LoadProgressEventArgs : EventArgs
    {
        public int Progress { get; }
        public string Status { get; }

        public LoadProgressEventArgs(int progress, string status)
        {
            Progress = progress;
            Status = status;
        }
    }

    public class AutoSaveEventArgs : EventArgs
    {
        public DateTime LastSaveTime { get; }

        public AutoSaveEventArgs(DateTime lastSaveTime)
        {
            LastSaveTime = lastSaveTime;
        }
    }

    public static class SerializationExtensions
    {
        public static byte[] ToBytes<T>(this T obj) where T : class
        {
            var formatter = new BinaryFormatter { Compression = CompressionLevel.Optimal };
            return formatter.Serialize(obj);
        }

        public static T FromBytes<T>(this byte[] data) where T : class
        {
            var formatter = new BinaryFormatter { Compression = CompressionLevel.Optimal };
            return formatter.Deserialize<T>(data);
        }

        public static string ToJson<T>(this T obj) where T : class
        {
            var formatter = new JSONFormatter();
            return formatter.SerializeToString(obj);
        }

        public static T FromJson<T>(this string json) where T : class
        {
            var formatter = new JSONFormatter();
            return formatter.DeserializeFromString<T>(json);
        }

        public static string ToXml<T>(this T obj) where T : class
        {
            var formatter = new XMLFormatter();
            return formatter.SerializeToString(obj);
        }

        public static T FromXml<T>(this string xml) where T : class
        {
            var formatter = new XMLFormatter();
            return formatter.DeserializeFromString<T>(xml);
        }
    }

    public class ReferenceResolver
    {
        private readonly Dictionary<Guid, object> _references = new Dictionary<Guid, object>();

        public void RegisterReference(Guid id, object obj)
        {
            _references[id] = obj;
        }

        public object ResolveReference(Guid id)
        {
            return _references.TryGetValue(id, out var obj) ? obj : null;
        }

        public bool HasReference(Guid id)
        {
            return _references.ContainsKey(id);
        }

        public void Clear()
        {
            _references.Clear();
        }

        public IEnumerable<Guid> GetUnresolvedReferences()
        {
            return _references.Keys;
        }
    }

    public class VersionMigration
    {
        private readonly Dictionary<Type, Dictionary<int, Action<object>>> _migrations = new Dictionary<Type, Dictionary<int, Action<object>>>();

        public void RegisterMigration<T>(int fromVersion, int toVersion, Action<T> migrate) where T : class
        {
            var type = typeof(T);
            if (!_migrations.ContainsKey(type))
                _migrations[type] = new Dictionary<int, Action<object>>();

            _migrations[type][fromVersion] = obj => migrate((T)obj);
        }

        public object Migrate(object obj, int currentVersion, int targetVersion)
        {
            var type = obj.GetType();
            if (!_migrations.TryGetValue(type, out var versionMigrations))
                return obj;

            for (int v = currentVersion; v < targetVersion; v++)
            {
                if (versionMigrations.TryGetValue(v, out var migration))
                {
                    migration(obj);
                }
            }

            return obj;
        }
    }

    public class LazyInstantiator
    {
        private readonly Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();
        private readonly Dictionary<object, bool> _instantiated = new Dictionary<object, bool>();

        public void RegisterFactory<T>(Func<T> factory) where T : class
        {
            _factories[typeof(T)] = () => factory();
        }

        public T Instantiate<T>() where T : class
        {
            var type = typeof(T);
            if (_factories.TryGetValue(type, out var factory))
            {
                var instance = (T)factory();
                _instantiated[instance] = true;
                return instance;
            }
            var obj = Activator.CreateInstance<T>();
            _instantiated[obj] = true;
            return obj;
        }

        public bool IsInstantiated(object obj)
        {
            return _instantiated.TryGetValue(obj, out var value) && value;
        }

        public void MarkInstantiated(object obj)
        {
            _instantiated[obj] = true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml.Linq;

namespace OfficeSuite.Common.Serialization
{
    public enum SerializationFormat
    {
        Json,
        Xml,
        Binary,
        Text
    }

    public interface ISerializer
    {
        byte[] Serialize<T>(T obj);
        T Deserialize<T>(byte[] data);
        SerializationFormat Format { get; }
    }

    public class JsonSerializer<T> : ISerializer
    {
        private readonly JsonSerializerOptions _options;

        public JsonSerializer()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public SerializationFormat Format => SerializationFormat.Json;

        public byte[] Serialize<T>(T obj)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(obj, _options);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] data)
        {
            var json = Encoding.UTF8.GetString(data);
            return System.Text.Json.JsonSerializer.Deserialize<T>(json, _options);
        }
    }

    public class XmlSerializer<T> : ISerializer
    {
        private readonly XNamespace _defaultNamespace;

        public XmlSerializer(string defaultNamespace = "http://schemas.officesuite.com/2010/data")
        {
            _defaultNamespace = defaultNamespace;
        }

        public SerializationFormat Format => SerializationFormat.Xml;

        public byte[] Serialize<T>(T obj)
        {
            var doc = new XDocument();
            var root = new XElement(_defaultNamespace + "root",
                new XAttribute("type", typeof(T).Name));

            SerializeObject(obj, root, _defaultNamespace);
            doc.Add(root);

            return Encoding.UTF8.GetBytes(doc.ToString());
        }

        private void SerializeObject(object obj, XElement parent, XNamespace ns)
        {
            if (obj == null)
                return;

            var type = obj.GetType();

            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                parent.Value = obj.ToString();
                return;
            }

            if (obj is System.Collections.IEnumerable enumerable)
            {
                foreach (var item in enumerable)
                {
                    var itemElement = new XElement(ns + "item");
                    SerializeObject(item, itemElement, ns);
                    parent.Add(itemElement);
                }
                return;
            }

            var properties = type.GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(obj);
                if (value == null)
                    continue;

                var propElement = new XElement(ns + prop.Name);
                SerializeObject(value, propElement, ns);
                parent.Add(propElement);
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            var xml = Encoding.UTF8.GetString(data);
            var doc = XDocument.Parse(xml);
            var root = doc.Root;

            return (T)DeserializeObject(root, typeof(T), _defaultNamespace);
        }

        private object DeserializeObject(XElement element, Type type, XNamespace ns)
        {
            if (type.IsPrimitive || type == typeof(string) || type == typeof(decimal))
            {
                return Convert.ChangeType(element.Value, type);
            }

            var instance = Activator.CreateInstance(type);
            var properties = type.GetProperties();

            foreach (var prop in properties)
            {
                var propName = prop.Name;
                var propElement = element.Element(ns + propName);
                if (propElement == null)
                    continue;

                var value = DeserializeObject(propElement, prop.PropertyType, ns);
                prop.SetValue(instance, value);
            }

            return instance;
        }
    }

    public class BinarySerializer<T> : ISerializer
    {
        private readonly List<Type> _knownTypes = new List<Type>();

        public SerializationFormat Format => SerializationFormat.Binary;

        public void RegisterType(Type type)
        {
            if (!_knownTypes.Contains(type))
                _knownTypes.Add(type);
        }

        public byte[] Serialize<T>(T obj)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            SerializeValue(writer, obj);

            return stream.ToArray();
        }

        private void SerializeValue(BinaryWriter writer, object value)
        {
            if (value == null)
            {
                writer.Write((byte)0);
                return;
            }

            var type = value.GetType();
            var typeCode = GetTypeCode(type);

            writer.Write((byte)(typeCode + 1));

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    writer.Write((bool)value);
                    break;
                case TypeCode.Byte:
                    writer.Write((byte)value);
                    break;
                case TypeCode.Int16:
                    writer.Write((short)value);
                    break;
                case TypeCode.Int32:
                    writer.Write((int)value);
                    break;
                case TypeCode.Int64:
                    writer.Write((long)value);
                    break;
                case TypeCode.Single:
                    writer.Write((float)value);
                    break;
                case TypeCode.Double:
                    writer.Write((double)value);
                    break;
                case TypeCode.Decimal:
                    writer.Write((decimal)value);
                    break;
                case TypeCode.String:
                    writer.Write((string)value);
                    break;
                case TypeCode.DateTime:
                    writer.Write(((DateTime)value).Ticks);
                    break;
                case TypeCode.Char:
                    writer.Write((char)value);
                    break;
                default:
                    if (value is System.Collections.IEnumerable enumerable)
                    {
                        var list = enumerable.Cast<object>().ToList();
                        writer.Write(list.Count);
                        foreach (var item in list)
                        {
                            SerializeValue(writer, item);
                        }
                    }
                    else
                    {
                        SerializeComplexObject(writer, value);
                    }
                    break;
            }
        }

        private void SerializeComplexObject(BinaryWriter writer, object value)
        {
            var properties = value.GetType().GetProperties();
            writer.Write(properties.Length);

            foreach (var prop in properties)
            {
                writer.Write(prop.Name);
                var propValue = prop.GetValue(value);
                SerializeValue(writer, propValue);
            }
        }

        public T Deserialize<T>(byte[] data)
        {
            using var stream = new MemoryStream(data);
            using var reader = new BinaryReader(stream);

            return (T)DeserializeValue(reader, typeof(T));
        }

        private object DeserializeValue(BinaryReader reader, Type expectedType)
        {
            var typeCode = (byte)(reader.ReadByte() - 1);

            if (typeCode == 255)
                return null;

            switch (typeCode)
            {
                case TypeCode.Boolean:
                    return reader.ReadBoolean();
                case TypeCode.Byte:
                    return reader.ReadByte();
                case TypeCode.Int16:
                    return reader.ReadInt16();
                case TypeCode.Int32:
                    return reader.ReadInt32();
                case TypeCode.Int64:
                    return reader.ReadInt64();
                case TypeCode.Single:
                    return reader.ReadSingle();
                case TypeCode.Double:
                    return reader.ReadDouble();
                case TypeCode.Decimal:
                    return reader.ReadDecimal();
                case TypeCode.String:
                    return reader.ReadString();
                case TypeCode.DateTime:
                    return new DateTime(reader.ReadInt64());
                case TypeCode.Char:
                    return reader.ReadChar();
                default:
                    if (expectedType.GetInterfaces().Contains(typeof(System.Collections.IEnumerable)))
                    {
                        var count = reader.ReadInt32();
                        var listType = typeof(List<>).MakeGenericType(expectedType.GetGenericArguments());
                        var list = (System.Collections.IList)Activator.CreateInstance(listType);
                        for (int i = 0; i < count; i++)
                        {
                            list.Add(DeserializeValue(reader, expectedType.GetGenericArguments()[0]));
                        }
                        return list;
                    }
                    else
                    {
                        return DeserializeComplexObject(reader, expectedType);
                    }
            }
        }

        private object DeserializeComplexObject(BinaryReader reader, Type type)
        {
            var instance = Activator.CreateInstance(type);
            var propertyCount = reader.ReadInt32();

            var properties = type.GetProperties();
            var propertyDict = properties.ToDictionary(p => p.Name);

            for (int i = 0; i < propertyCount; i++)
            {
                var propName = reader.ReadString();
                if (propertyDict.TryGetValue(propName, out var prop))
                {
                    var value = DeserializeValue(reader, prop.PropertyType);
                    prop.SetValue(instance, value);
                }
            }

            return instance;
        }

        private TypeCode GetTypeCode(Type type)
        {
            return Type.GetTypeCode(type);
        }
    }

    public class DocumentSerializer
    {
        private readonly Dictionary<SerializationFormat, ISerializer> _serializers = new Dictionary<SerializationFormat, ISerializer>();

        public DocumentSerializer()
        {
            _serializers[SerializationFormat.Json] = new JsonSerializer<Document>();
            _serializers[SerializationFormat.Xml] = new XmlSerializer<Document>();
        }

        public byte[] SerializeDocument(Document document, SerializationFormat format)
        {
            if (_serializers.TryGetValue(format, out var serializer))
            {
                return serializer.Serialize(document);
            }

            throw new NotSupportedException($"Format {format} is not supported");
        }

        public Document DeserializeDocument(byte[] data, SerializationFormat format)
        {
            if (_serializers.TryGetValue(format, out var serializer))
            {
                return serializer.Deserialize<Document>(data);
            }

            throw new NotSupportedException($"Format {format} is not supported");
        }
    }

    public class Document
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DocumentType Type { get; set; }
        public DocumentMetadata Metadata { get; set; } = new DocumentMetadata();
        public byte[] Content { get; set; }
        public DocumentVersion CurrentVersion { get; set; }
        public List<DocumentVersion> VersionHistory { get; set; } = new List<DocumentVersion>();
    }

    public enum DocumentType
    {
        Unknown,
        Word,
        Excel,
        PowerPoint
    }

    public class DocumentMetadata
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Subject { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime? LastAccessed { get; set; }
        public int Revision { get; set; }
        public string Category { get; set; }
        public string ContentStatus { get; set; }
        public string Identifier { get; set; }
        public string Language { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
    }

    public class DocumentVersion
    {
        public int VersionNumber { get; set; }
        public string Author { get; set; }
        public DateTime Timestamp { get; set; }
        public string Comment { get; set; }
        public byte[] Content { get; set; }
        public long Size { get; set; }
    }

    public class SettingsSerializer
    {
        private readonly JsonSerializer<ApplicationSettings> _jsonSerializer;

        public SettingsSerializer()
        {
            _jsonSerializer = new JsonSerializer<ApplicationSettings>();
        }

        public byte[] SerializeSettings(ApplicationSettings settings)
        {
            return _jsonSerializer.Serialize(settings);
        }

        public ApplicationSettings DeserializeSettings(byte[] data)
        {
            return _jsonSerializer.Deserialize(data);
        }

        public void SaveSettings(string filePath, ApplicationSettings settings)
        {
            var data = SerializeSettings(settings);
            File.WriteAllBytes(filePath, data);
        }

        public ApplicationSettings LoadSettings(string filePath)
        {
            if (!File.Exists(filePath))
                return new ApplicationSettings();

            var data = File.ReadAllBytes(filePath);
            return DeserializeSettings(data);
        }
    }

    public class ApplicationSettings
    {
        public GeneralSettings General { get; set; } = new GeneralSettings();
        public EditorSettings Editor { get; set; } = new EditorSettings();
        public ViewSettings View { get; set; } = new ViewSettings();
        public SpellingSettings Spelling { get; set; } = new SpellingSettings();
        public PrintSettings Print { get; set; } = new PrintSettings();
        public SecuritySettings Security { get; set; } = new SecuritySettings();
        public BackupSettings Backup { get; set; } = new BackupSettings();
        public Dictionary<string, string> CustomSettings { get; set; } = new Dictionary<string, string>();
    }

    public class GeneralSettings
    {
        public string DefaultAuthor { get; set; }
        public string DefaultCompany { get; set; }
        public string DefaultEmail { get; set; }
        public int AutoSaveInterval { get; set; } = 300;
        public bool AutoSaveEnabled { get; set; } = true;
        public string DefaultFileFormat { get; set; } = "docx";
        public string DefaultSaveLocation { get; set; }
        public string DefaultTemplateLocation { get; set; }
        public bool CheckForUpdates { get; set; } = true;
        public bool SendFeedback { get; set; } = false;
        public string Language { get; set; } = "en-US";
        public string Region { get; set; } = "US";
    }

    public class EditorSettings
    {
        public bool EnableSpellCheck { get; set; } = true;
        public bool EnableGrammarCheck { get; set; } = true;
        public bool EnableAutoCorrect { get; set; } = true;
        public bool EnableSmartQuotes { get; set; } = true;
        public bool EnableAutoFormat { get; set; } = true;
        public int UndoHistorySize { get; set; } = 100;
        public bool ShowRuler { get; set; } = true;
        public bool ShowStatusBar { get; set; } = true;
        public bool ShowMiniMap { get; set; } = false;
        public string TabSize { get; set; } = "4";
        public bool InsertSpaces { get; set; } = true;
        public bool WordWrap { get; set; } = true;
    }

    public class ViewSettings
    {
        public string Theme { get; set; } = "Light";
        public string AccentColor { get; set; } = "Blue";
        public double FontSize { get; set; } = 12;
        public string FontFamily { get; set; } = "Segoe UI";
        public double ZoomLevel { get; set; } = 100;
        public bool ShowWhitespace { get; set; } = false;
        public bool ShowEndOfLine { get; set; } = false;
        public bool ShowLineNumbers { get; set; } = true;
        public bool ShowBreadcrumbs { get; set; } = true;
        public bool MinimizeRibbon { get; set; } = false;
        public bool UseCompactMode { get; set; } = false;
    }

    public class SpellingSettings
    {
        public string DefaultLanguage { get; set; } = "en-US";
        public bool IgnoreAllCaps { get; set; } = true;
        public bool IgnoreWordsWithNumbers { get; set; } = true;
        public bool IgnoreFileNames { get; set; } = true;
        public bool CheckGrammar { get; set; } = true;
        public bool CheckSpelling { get; set; } = true;
        public List<string> CustomDictionary { get; set; } = new List<string>();
        public List<string> IgnoreList { get; set; } = new List<string>();
    }

    public class PrintSettings
    {
        public string DefaultPrinter { get; set; }
        public PaperSize DefaultPaperSize { get; set; } = PaperSize.Letter;
        public PaperOrientation DefaultOrientation { get; set; } = PaperOrientation.Portrait;
        public int Copies { get; set; } = 1;
        public bool Collate { get; set; } = true;
        public bool ColorPrint { get; set; } = true;
        public bool PrintBackground { get; set; } = true;
        public bool PrintComments { get; set; } = false;
        public string PrintRange { get; set; } = "All";
        public int StartPage { get; set; } = 1;
        public int EndPage { get; set; } = 1;
    }

    public enum PaperSize
    {
        Letter,
        Legal,
        A4,
        A5,
        B5,
        Executive,
        Envelope10,
        EnvelopeDL,
        EnvelopeMonarch
    }

    public enum PaperOrientation
    {
        Portrait,
        Landscape
    }

    public class SecuritySettings
    {
        public bool RequirePassword { get; set; } = false;
        public string EncryptedPassword { get; set; }
        public EncryptionStrength EncryptionStrength { get; set; } = EncryptionStrength.Strong;
        public bool MarkAsFinal { get; set; } = false;
        public bool RestrictEditing { get; set; } = false;
        public string EditingRestriction { get; set; } = "none";
        public bool DigitalSignature { get; set; } = false;
        public List<string> AllowedMacros { get; set; } = new List<string>();
    }

    public enum EncryptionStrength
    {
        None,
        Weak,
        Standard,
        Strong
    }

    public class BackupSettings
    {
        public bool AutoBackup { get; set; } = true;
        public int BackupIntervalMinutes { get; set; } = 10;
        public string BackupLocation { get; set; }
        public int MaxBackupCopies { get; set; } = 10;
        public bool KeepBackupOnClose { get; set; } = false;
        public bool BackupOnSave { get; set; } = true;
        public string BackupFileNamePattern { get; set; } = "{filename}_backup_{timestamp}";
    }

    public class ProjectSerializer
    {
        public byte[] SerializeProject(OfficeProject project)
        {
            var serializer = new JsonSerializer<OfficeProject>();
            return serializer.Serialize(project);
        }

        public OfficeProject DeserializeProject(byte[] data)
        {
            var serializer = new JsonSerializer<OfficeProject>();
            return serializer.Deserialize(data);
        }

        public void SaveProject(string filePath, OfficeProject project)
        {
            var data = SerializeProject(project);
            File.WriteAllBytes(filePath, data);
        }

        public OfficeProject LoadProject(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var data = File.ReadAllBytes(filePath);
            return DeserializeProject(data);
        }
    }

    public class OfficeProject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public ProjectSettings Settings { get; set; } = new ProjectSettings();
        public List<ProjectDocument> Documents { get; set; } = new List<ProjectDocument>();
        public List<ProjectReference> References { get; set; } = new List<ProjectReference>();
        public List<ProjectResource> Resources { get; set; } = new List<ProjectResource>();
    }

    public class ProjectSettings
    {
        public string DefaultDocumentType { get; set; }
        public Dictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();
    }

    public class ProjectDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public string DocumentType { get; set; }
        public int Order { get; set; }
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }

    public class ProjectReference
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Path { get; set; }
    }

    public class ProjectResource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public byte[] Data { get; set; }
    }
}

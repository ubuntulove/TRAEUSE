using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.ObjectModel;

namespace WPFEngine.Resources
{
    #region Asset Database

    public interface IGUID
    {
        string Value { get; }
    }

    public struct GUID : IGUID, IEquatable<GUID>, IComparable<GUID>
    {
        private readonly string _value;

        public string Value => _value;

        public GUID(string value)
        {
            _value = value ?? GenerateNew();
        }

        public static GUID New()
        {
            return new GUID(GenerateNew());
        }

        private static string GenerateNew()
        {
            return Guid.NewGuid().ToString("N");
        }

        public bool Equals(GUID other)
        {
            return _value == other._value;
        }

        public override bool Equals(object? obj)
        {
            return obj is GUID other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _value?.GetHashCode() ?? 0;
        }

        public int CompareTo(GUID other)
        {
            return string.Compare(_value, other._value, StringComparison.Ordinal);
        }

        public override string ToString()
        {
            return _value;
        }

        public static bool operator ==(GUID left, GUID right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(GUID left, GUID right)
        {
            return !left.Equals(right);
        }
    }

    public class AssetInfo
    {
        public GUID Guid { get; set; }
        public string AssetPath { get; set; }
        public string SourcePath { get; set; }
        public Type AssetType { get; set; }
        public long FileSize { get; set; }
        public DateTime LastModified { get; set; }
        public DateTime ImportTime { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
        public List<GUID> Dependencies { get; set; }
        public bool IsDirty { get; set; }

        public AssetInfo()
        {
            Guid = GUID.New();
            AssetPath = string.Empty;
            SourcePath = string.Empty;
            Metadata = new Dictionary<string, object>();
            Dependencies = new List<GUID>();
            ImportTime = DateTime.Now;
            LastModified = DateTime.Now;
            IsDirty = true;
        }

        public AssetInfo(GUID guid, string assetPath, Type assetType) : this()
        {
            Guid = guid;
            AssetPath = assetPath;
            AssetType = assetType;
        }
    }

    public class AssetDatabase
    {
        private static AssetDatabase? _instance;
        private readonly ConcurrentDictionary<GUID, AssetInfo> _assetsByGuid;
        private readonly ConcurrentDictionary<string, AssetInfo> _assetsByPath;
        private readonly ConcurrentDictionary<Type, List<GUID>> _assetsByType;
        private readonly ReaderWriterLockSlim _lock;
        private readonly string _databasePath;
        private readonly AssetPostprocessorHandler _postprocessorHandler;
        private readonly AssetChangeTracker _changeTracker;

        public static AssetDatabase Instance => _instance ??= new AssetDatabase();

        public event EventHandler<AssetInfo>? AssetAdded;
        public event EventHandler<AssetInfo>? AssetRemoved;
        public event EventHandler<AssetInfo>? AssetModified;

        private AssetDatabase()
        {
            _assetsByGuid = new ConcurrentDictionary<GUID, AssetInfo>();
            _assetsByPath = new ConcurrentDictionary<string, AssetInfo>();
            _assetsByType = new ConcurrentDictionary<Type, List<GUID>>();
            _lock = new ReaderWriterLockSlim();
            _databasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "WPFEngine", "AssetDatabase");
            _postprocessorHandler = new AssetPostprocessorHandler();
            _changeTracker = new AssetChangeTracker();
        }

        public AssetInfo? FindAssetByPath(string path)
        {
            _lock.EnterReadLock();
            try
            {
                _assetsByPath.TryGetValue(path, out var asset);
                return asset;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public AssetInfo? FindAssetByGUID(GUID guid)
        {
            _lock.EnterReadLock();
            try
            {
                _assetsByGuid.TryGetValue(guid, out var asset);
                return asset;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<AssetInfo> FindAssetsOfType(Type type)
        {
            _lock.EnterReadLock();
            try
            {
                var results = new List<AssetInfo>();
                if (_assetsByType.TryGetValue(type, out var guids))
                {
                    foreach (var guid in guids)
                    {
                        if (_assetsByGuid.TryGetValue(guid, out var asset))
                        {
                            results.Add(asset);
                        }
                    }
                }
                return results;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<AssetInfo> FindAssets(string searchPattern)
        {
            _lock.EnterReadLock();
            try
            {
                return _assetsByPath.Values
                    .Where(a => a.AssetPath.Contains(searchPattern, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public GUID RegisterAsset(string assetPath, Type assetType, string? sourcePath = null)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_assetsByPath.TryGetValue(assetPath, out var existing))
                {
                    return existing.Guid;
                }

                var guid = GUID.New();
                var assetInfo = new AssetInfo(guid, assetPath, assetType)
                {
                    SourcePath = sourcePath ?? assetPath,
                    FileSize = File.Exists(assetPath) ? new FileInfo(assetPath).Length : 0,
                    LastModified = File.Exists(assetPath) ? File.GetLastWriteTime(assetPath) : DateTime.Now
                };

                _assetsByGuid[guid] = assetInfo;
                _assetsByPath[assetPath] = assetInfo;

                if (!_assetsByType.ContainsKey(assetType))
                {
                    _assetsByType[assetType] = new List<GUID>();
                }
                _assetsByType[assetType].Add(guid);

                _changeTracker.TrackAddition(guid);
                AssetAdded?.Invoke(this, assetInfo);

                return guid;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void UnregisterAsset(GUID guid)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_assetsByGuid.TryRemove(guid, out var asset))
                {
                    _assetsByPath.TryRemove(asset.AssetPath, out _);
                    
                    if (_assetsByType.TryGetValue(asset.AssetType, out var guids))
                    {
                        guids.Remove(guid);
                    }

                    _changeTracker.TrackRemoval(guid);
                    AssetRemoved?.Invoke(this, asset);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void UpdateAssetMetadata(GUID guid, Dictionary<string, object> metadata)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_assetsByGuid.TryGetValue(guid, out var asset))
                {
                    foreach (var kvp in metadata)
                    {
                        asset.Metadata[kvp.Key] = kvp.Value;
                    }
                    asset.IsDirty = true;
                    _changeTracker.TrackModification(guid);
                    AssetModified?.Invoke(this, asset);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddDependency(GUID parent, GUID child)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_assetsByGuid.TryGetValue(parent, out var parentAsset))
                {
                    if (!parentAsset.Dependencies.Contains(child))
                    {
                        parentAsset.Dependencies.Add(child);
                        parentAsset.IsDirty = true;
                        _changeTracker.TrackDependency(parent, child);
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public List<GUID> GetDependencies(GUID guid)
        {
            _lock.EnterReadLock();
            try
            {
                if (_assetsByGuid.TryGetValue(guid, out var asset))
                {
                    return new List<GUID>(asset.Dependencies);
                }
                return new List<GUID>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<GUID> GetDependents(GUID guid)
        {
            _lock.EnterReadLock();
            try
            {
                var dependents = new List<GUID>();
                foreach (var kvp in _assetsByGuid)
                {
                    if (kvp.Value.Dependencies.Contains(guid))
                    {
                        dependents.Add(kvp.Key);
                    }
                }
                return dependents;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void ImportAsset(string sourcePath, AssetImportContext context)
        {
            var assetPath = context.GetAssetPath(sourcePath);
            var assetType = context.GetAssetType(sourcePath);
            
            var guid = RegisterAsset(assetPath, assetType, sourcePath);
            var asset = FindAssetByGUID(guid);

            if (asset != null)
            {
                context.AssetInfo = asset;
                _postprocessorHandler.InvokePostprocessAsset(asset, sourcePath, context);

                if (File.Exists(sourcePath))
                {
                    asset.FileSize = new FileInfo(sourcePath).Length;
                    asset.LastModified = File.GetLastWriteTime(sourcePath);
                    asset.ImportTime = DateTime.Now;
                    asset.IsDirty = false;
                }
            }
        }

        public void RegisterPostprocessor(AssetPostprocessor postprocessor)
        {
            _postprocessorHandler.Register(postprocessor);
        }

        public async Task SaveDatabaseAsync()
        {
            Directory.CreateDirectory(_databasePath);
            var dbFile = Path.Combine(_databasePath, "assets.db");

            await Task.Run(() =>
            {
                _lock.EnterWriteLock();
                try
                {
                    var lines = new List<string>();
                    foreach (var kvp in _assetsByGuid)
                    {
                        var asset = kvp.Value;
                        var deps = string.Join(";", asset.Dependencies.Select(d => d.Value));
                        var metadata = string.Join("|", asset.Metadata.Select(m => $"{m.Key}={m.Value}"));
                        lines.Add($"{asset.Guid.Value}|{asset.AssetPath}|{asset.SourcePath}|{asset.AssetType.FullName}|{asset.FileSize}|{asset.LastModified:O}|{asset.ImportTime:O}|{deps}|{metadata}");
                    }
                    File.WriteAllLines(dbFile, lines);
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            });
        }

        public int AssetCount => _assetsByGuid.Count;
    }

    public class AssetImportContext
    {
        private readonly Dictionary<string, object> _settings;
        private readonly string _projectPath;
        private AssetInfo? _assetInfo;

        public AssetInfo? AssetInfo
        {
            get => _assetInfo;
            set => _assetInfo = value;
        }

        public string ProjectPath => _projectPath;

        public AssetImportContext(string projectPath)
        {
            _projectPath = projectPath;
            _settings = new Dictionary<string, object>();
        }

        public void SetSetting(string key, object value)
        {
            _settings[key] = value;
        }

        public T? GetSetting<T>(string key)
        {
            if (_settings.TryGetValue(key, out var value))
            {
                return (T)value;
            }
            return default;
        }

        public string GetAssetPath(string sourcePath)
        {
            var relativePath = Path.GetRelativePath(_projectPath, sourcePath);
            return Path.Combine("Assets", relativePath).Replace("\\", "/");
        }

        public Type GetAssetType(string sourcePath)
        {
            var extension = Path.GetExtension(sourcePath).ToLowerInvariant();
            return extension switch
            {
                ".png" or ".jpg" or ".jpeg" or ".bmp" or ".gif" or ".tga" => typeof(System.Windows.Media.Imaging.BitmapSource),
                ".wav" or ".mp3" or ".ogg" => typeof(NAudio.Wave.WaveOut),
                ".fbx" or ".obj" or ".gltf" => typeof(object),
                ".mat" => typeof(object),
                ".cs" => typeof(object),
                _ => typeof(object)
            };
        }

        public string GetOutputPath(string sourcePath, string suffix = "")
        {
            var assetPath = GetAssetPath(sourcePath);
            var ext = Path.GetExtension(assetPath);
            var name = Path.GetFileNameWithoutExtension(assetPath);
            var dir = Path.GetDirectoryName(assetPath) ?? "";
            return Path.Combine(dir, $"{name}{suffix}{ext}").Replace("\\", "/");
        }
    }

    public abstract class AssetPostprocessor
    {
        public virtual int order => 0;

        public virtual void OnPreprocessAsset(AssetInfo asset, string sourcePath, AssetImportContext context) { }

        public virtual void OnPostprocessAsset(AssetInfo asset, string sourcePath, AssetImportContext context) { }

        public virtual void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths) { }
    }

    public class TexturePostprocessor : AssetPostprocessor
    {
        public override int order => -100;

        public override void OnPostprocessAsset(AssetInfo asset, string sourcePath, AssetImportContext context)
        {
            if (asset.AssetType == typeof(System.Windows.Media.Imaging.BitmapSource))
            {
                context.SetSetting("GenerateMipmaps", true);
                context.SetSetting("CompressionQuality", 85);
            }
        }
    }

    public class AudioPostprocessor : AssetPostprocessor
    {
        public override int order => -50;

        public override void OnPostprocessAsset(AssetInfo asset, string sourcePath, AssetImportContext context)
        {
            if (asset.AssetType == typeof(NAudio.Wave.WaveOut))
            {
                context.SetSetting("LoadInBackground", true);
                context.SetSetting("PreloadAudioData", false);
            }
        }
    }

    internal class AssetPostprocessorHandler
    {
        private readonly List<AssetPostprocessor> _postprocessors;

        public AssetPostprocessorHandler()
        {
            _postprocessors = new List<AssetPostprocessor>();
        }

        public void Register(AssetPostprocessor postprocessor)
        {
            _postprocessors.Add(postprocessor);
            _postprocessors.Sort((a, b) => a.order.CompareTo(b.order));
        }

        public void InvokePreprocessAsset(AssetInfo asset, string sourcePath, AssetImportContext context)
        {
            foreach (var processor in _postprocessors)
            {
                processor.OnPreprocessAsset(asset, sourcePath, context);
            }
        }

        public void InvokePostprocessAsset(AssetInfo asset, string sourcePath, AssetImportContext context)
        {
            foreach (var processor in _postprocessors)
            {
                processor.OnPostprocessAsset(asset, sourcePath, context);
            }
        }

        public void InvokePostprocessAllAssets(string[] imported, string[] deleted, string[] moved, string[] movedFrom)
        {
            foreach (var processor in _postprocessors)
            {
                processor.OnPostprocessAllAssets(imported, deleted, moved, movedFrom);
            }
        }
    }

    public class AssetChangeTracker
    {
        private readonly List<AssetChange> _changes;
        private readonly Dictionary<GUID, DateTime> _lastAccessTimes;
        private readonly ReaderWriterLockSlim _lock;

        public AssetChangeTracker()
        {
            _changes = new List<AssetChange>();
            _lastAccessTimes = new Dictionary<GUID, DateTime>();
            _lock = new ReaderWriterLockSlim();
        }

        public void TrackAddition(GUID guid)
        {
            AddChange(AssetChangeType.Added, guid);
        }

        public void TrackRemoval(GUID guid)
        {
            AddChange(AssetChangeType.Removed, guid);
        }

        public void TrackModification(GUID guid)
        {
            AddChange(AssetChangeType.Modified, guid);
        }

        public void TrackDependency(GUID parent, GUID child)
        {
            _lock.EnterWriteLock();
            try
            {
                _changes.Add(new AssetChange
                {
                    ChangeType = AssetChangeType.DependencyChanged,
                    Guid = parent,
                    RelatedGuid = child,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void TrackAccess(GUID guid)
        {
            _lock.EnterWriteLock();
            try
            {
                _lastAccessTimes[guid] = DateTime.Now;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void AddChange(AssetChangeType type, GUID guid)
        {
            _lock.EnterWriteLock();
            try
            {
                _changes.Add(new AssetChange
                {
                    ChangeType = type,
                    Guid = guid,
                    Timestamp = DateTime.Now
                });
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<AssetChange> GetChangesSince(DateTime since)
        {
            _lock.EnterReadLock();
            try
            {
                return _changes.Where(c => c.Timestamp >= since).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public DateTime? GetLastAccessTime(GUID guid)
        {
            _lock.EnterReadLock();
            try
            {
                return _lastAccessTimes.TryGetValue(guid, out var time) ? time : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void ClearChanges()
        {
            _lock.EnterWriteLock();
            try
            {
                _changes.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public enum AssetChangeType
    {
        Added,
        Removed,
        Modified,
        DependencyChanged
    }

    public class AssetChange
    {
        public AssetChangeType ChangeType { get; set; }
        public GUID Guid { get; set; }
        public GUID? RelatedGuid { get; set; }
        public DateTime Timestamp { get; set; }
    }

    #endregion

    #region Asset Importers

    public enum TextureFormat
    {
        Auto,
        RGBA32,
        RGB24,
        Gray8,
        DXT1,
        DXT5,
        BC7
    }

    public enum TextureCompression
    {
        None,
        Low,
        Medium,
        High,
        Ultra
    }

    public class TextureImporterSettings
    {
        public TextureFormat Format { get; set; } = TextureFormat.Auto;
        public TextureCompression Compression { get; set; } = TextureCompression.Medium;
        public bool GenerateMipmaps { get; set; } = true;
        public bool SRGB { get; set; } = false;
        public bool StreamingMipmaps { get; set; } = false;
        public int MaxSize { get; set; } = 2048;
        public int AnisoLevel { get; set; } = 1;
        public FilterMode FilterMode { get; set; } = FilterMode.Bilinear;
        public TextureWrapMode WrapMode { get; set; } = TextureWrapMode.Repeat;
    }

    public enum FilterMode
    {
        Point,
        Bilinear,
        Trilinear,
        Anisotropic
    }

    public enum TextureWrapMode
    {
        Repeat,
        Clamp,
        Mirror,
        Border
    }

    public class TextureImporter
    {
        private readonly TextureImporterSettings _settings;

        public TextureImporterSettings Settings => _settings;

        public TextureImporter() : this(new TextureImporterSettings()) { }

        public TextureImporter(TextureImporterSettings settings)
        {
            _settings = settings;
        }

        public System.Windows.Media.Imaging.BitmapSource Import(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture not found: {path}");

            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;

            if (_settings.MaxSize > 0)
            {
                bitmap.DecodePixelWidth = _settings.MaxSize;
            }

            bitmap.EndInit();
            bitmap.Freeze();

            return bitmap;
        }

        public async Task<System.Windows.Media.Imaging.BitmapSource> ImportAsync(string path, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Import(path), cancellationToken);
        }

        public byte[] CompressTexture(System.Windows.Media.Imaging.BitmapSource source)
        {
            using var stream = new MemoryStream();
            var encoder = new System.Windows.Media.Imaging.PngBitmapEncoder();
            encoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(source));
            encoder.Save(stream);
            return stream.ToArray();
        }

        public System.Windows.Media.Imaging.BitmapSource? GenerateMipmaps(System.Windows.Media.Imaging.BitmapSource source)
        {
            if (!_settings.GenerateMipmaps)
                return source;

            int width = Math.Max(1, (int)source.Width / 2);
            int height = Math.Max(1, (int)source.Height / 2);

            var mipmap = new System.Windows.Media.Imaging.WriteableBitmap(width, height, 96, 96, source.Format, null);
            var scaleX = (double)source.PixelWidth / width;
            var scaleY = (double)source.PixelHeight / height;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int srcX = (int)(x * scaleX);
                    int srcY = (int)(y * scaleY);
                    mipmap.SetPixel(x, y, GetPixelAveraged(source, srcX, srcY, (int)scaleX, (int)scaleY));
                }
            }

            return mipmap;
        }

        private System.Windows.Media.Color GetPixelAveraged(System.Windows.Media.Imaging.BitmapSource source, int x, int y, int scaleX, int scaleY)
        {
            int r = 0, g = 0, b = 0, a = 0, count = 0;
            for (int dy = 0; dy < scaleY && y + dy < source.PixelHeight; dy++)
            {
                for (int dx = 0; dx < scaleX && x + dx < source.PixelWidth; dx++)
                {
                    var color = GetPixel(source, x + dx, y + dy);
                    r += color.R;
                    g += color.G;
                    b += color.B;
                    a += color.A;
                    count++;
                }
            }
            return System.Windows.Media.Color.FromArgb(
                (byte)(a / count),
                (byte)(r / count),
                (byte)(g / count),
                (byte)(b / count));
        }

        private System.Windows.Media.Color GetPixel(System.Windows.Media.Imaging.BitmapSource source, int x, int y)
        {
            if (source.Format == System.Windows.Media.PixelFormats.Bgra32)
            {
                var pixels = new byte[4];
                var stride = source.PixelWidth * 4;
                var rect = new System.Windows.Int32Rect(x, y, 1, 1);
                source.CopyPixels(rect, pixels, stride, 0);
                return System.Windows.Media.Color.FromArgb(pixels[3], pixels[2], pixels[1], pixels[0]);
            }
            return System.Windows.Media.Colors.White;
        }
    }

    public enum AudioFormat
    {
        Auto,
        PCM,
        MP3,
        Vorbis,
        AAC
    }

    public enum AudioQuality
    {
        Low,
        Medium,
        High,
        Best
    }

    public class AudioImporterSettings
    {
        public AudioFormat Format { get; set; } = AudioFormat.Auto;
        public AudioQuality Quality { get; set; } = AudioQuality.High;
        public bool LoadInBackground { get; set; } = false;
        public bool PreloadAudioData { get; set; } = true;
        public bool Ambisonic { get; set; } = false;
        public bool ForceToMono { get; set; } = false;
        public float Volume { get; set; } = 1.0f;
        public float Pitch { get; set; } = 1.0f;
        public int SampleRateSetting { get; set; } = 0;
    }

    public class AudioImporter
    {
        private readonly AudioImporterSettings _settings;

        public AudioImporterSettings Settings => _settings;

        public AudioImporter() : this(new AudioImporterSettings()) { }

        public AudioImporter(AudioImporterSettings settings)
        {
            _settings = settings;
        }

        public byte[] Import(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Audio file not found: {path}");

            return File.ReadAllBytes(path);
        }

        public async Task<byte[]> ImportAsync(string path, CancellationToken cancellationToken = default)
        {
            return await Task.Run(() => Import(path), cancellationToken);
        }

        public AudioMetadata GetMetadata(string path)
        {
            var info = new FileInfo(path);
            return new AudioMetadata
            {
                FilePath = path,
                FileSize = info.Length,
                Format = DetectFormat(path),
                Duration = GetDuration(path),
                SampleRate = 44100,
                Channels = 2,
                BitDepth = 16
            };
        }

        private AudioFormat DetectFormat(string path)
        {
            var ext = Path.GetExtension(path).ToLowerInvariant();
            return ext switch
            {
                ".wav" => AudioFormat.PCM,
                ".mp3" => AudioFormat.MP3,
                ".ogg" => AudioFormat.Vorbis,
                ".aac" or ".m4a" => AudioFormat.AAC,
                _ => AudioFormat.Auto
            };
        }

        private TimeSpan GetDuration(string path)
        {
            try
            {
                using var reader = new NAudio.Wave.AudioFileReader(path);
                return reader.TotalTime;
            }
            catch
            {
                return TimeSpan.Zero;
            }
        }

        public byte[] ConvertFormat(byte[] audioData, AudioFormat targetFormat)
        {
            return audioData;
        }

        public byte[] Resample(byte[] audioData, int targetSampleRate)
        {
            return audioData;
        }
    }

    public class AudioMetadata
    {
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public AudioFormat Format { get; set; }
        public TimeSpan Duration { get; set; }
        public int SampleRate { get; set; }
        public int Channels { get; set; }
        public int BitDepth { get; set; }
    }

    public class ModelImporterSettings
    {
        public bool GenerateColliders { get; set; } = false;
        public bool OptimizeMesh { get; set; } = true;
        public bool ImportMaterials { get; set; } = true;
        public float ScaleFactor { get; set; } = 1.0f;
        public bool ImportAnimation { get; set; } = true;
    }

    public class ModelImporter
    {
        private readonly ModelImporterSettings _settings;

        public ModelImporterSettings Settings => _settings;

        public ModelImporter() : this(new ModelImporterSettings()) { }

        public ModelImporter(ModelImporterSettings settings)
        {
            _settings = settings;
        }

        public ModelAsset Import(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Model not found: {path}");

            var model = new ModelAsset
            {
                Path = path,
                Name = Path.GetFileNameWithoutExtension(path),
                Meshes = new List<MeshData>(),
                Materials = new List<MaterialData>(),
                Animations = new List<AnimationData>()
            };

            var ext = Path.GetExtension(path).ToLowerInvariant();
            switch (ext)
            {
                case ".obj":
                    ImportOBJ(model, path);
                    break;
                case ".fbx":
                    ImportFBX(model, path);
                    break;
                case ".gltf":
                case ".glb":
                    ImportGLTF(model, path);
                    break;
            }

            return model;
        }

        private void ImportOBJ(ModelAsset model, string path)
        {
            var mesh = new MeshData
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Vertices = new List<Vector3>(),
                Normals = new List<Vector3>(),
                UVs = new List<Vector2>(),
                Triangles = new List<int>()
            };

            var vertices = new List<Vector3>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();
            var faces = new List<(int v, int n, int u)>();

            foreach (var line in File.ReadAllLines(path))
            {
                var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0) continue;

                switch (parts[0])
                {
                    case "v":
                        if (parts.Length >= 4)
                        {
                            vertices.Add(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])));
                        }
                        break;
                    case "vn":
                        if (parts.Length >= 4)
                        {
                            normals.Add(new Vector3(
                                float.Parse(parts[1]),
                                float.Parse(parts[2]),
                                float.Parse(parts[3])));
                        }
                        break;
                    case "vt":
                        if (parts.Length >= 3)
                        {
                            uvs.Add(new Vector2(
                                float.Parse(parts[1]),
                                float.Parse(parts[2])));
                        }
                        break;
                    case "f":
                        if (parts.Length >= 4)
                        {
                            var indices = ParseFace(parts[1], vertices.Count, normals.Count, uvs.Count);
                            faces.Add(indices);
                            if (parts.Length >= 5)
                            {
                                faces.Add(ParseFace(parts[2], vertices.Count, normals.Count, uvs.Count));
                                faces.Add(ParseFace(parts[4], vertices.Count, normals.Count, uvs.Count));
                            }
                        }
                        break;
                }
            }

            mesh.Vertices = vertices;
            mesh.Normals = normals;
            mesh.UVs = uvs;
            mesh.Triangles = faces.Select(f => f.v).ToList();
            model.Meshes.Add(mesh);
        }

        private (int v, int n, int u) ParseFace(string face, int vCount, int nCount, int uCount)
        {
            var parts = face.Split('/');
            int v = parts.Length > 0 ? int.Parse(parts[0]) - 1 : 0;
            int u = parts.Length > 1 && !string.IsNullOrEmpty(parts[1]) ? int.Parse(parts[1]) - 1 : 0;
            int n = parts.Length > 2 && !string.IsNullOrEmpty(parts[2]) ? int.Parse(parts[2]) - 1 : 0;
            return (v, n, u);
        }

        private void ImportFBX(ModelAsset model, string path)
        {
            var mesh = new MeshData
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Vertices = new List<Vector3> { new Vector3(0, 0, 0) },
                Triangles = new List<int> { 0 }
            };
            model.Meshes.Add(mesh);
        }

        private void ImportGLTF(ModelAsset model, string path)
        {
            var mesh = new MeshData
            {
                Name = Path.GetFileNameWithoutExtension(path),
                Vertices = new List<Vector3> { new Vector3(0, 0, 0) },
                Triangles = new List<int> { 0 }
            };
            model.Meshes.Add(mesh);
        }
    }

    public class ModelAsset
    {
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public List<MeshData> Meshes { get; set; } = new();
        public List<MaterialData> Materials { get; set; } = new();
        public List<AnimationData> Animations { get; set; } = new();
    }

    public class MeshData
    {
        public string Name { get; set; } = string.Empty;
        public List<Vector3> Vertices { get; set; } = new();
        public List<Vector3> Normals { get; set; } = new();
        public List<Vector2> UVs { get; set; } = new();
        public List<int> Triangles { get; set; } = new();
    }

    public class MaterialData
    {
        public string Name { get; set; } = string.Empty;
        public System.Windows.Media.Color BaseColor { get; set; } = System.Windows.Media.Colors.White;
        public float Metallic { get; set; } = 0;
        public float Smoothness { get; set; } = 0.5f;
    }

    public class AnimationData
    {
        public string Name { get; set; } = string.Empty;
        public float Duration { get; set; }
        public int FrameRate { get; set; } = 30;
    }

    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Vector3() { }

        public Vector3(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }
    }

    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public Vector2() { }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }
    }

    public class MaterialImporterSettings
    {
        public string ShaderName { get; set; } = "Standard";
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    public class MaterialImporter
    {
        private readonly MaterialImporterSettings _settings;

        public MaterialImporterSettings Settings => _settings;

        public MaterialImporter() : this(new MaterialImporterSettings()) { }

        public MaterialImporter(MaterialImporterSettings settings)
        {
            _settings = settings;
        }

        public MaterialData Import(string path)
        {
            var material = new MaterialData
            {
                Name = Path.GetFileNameWithoutExtension(path)
            };

            if (File.Exists(path))
            {
                foreach (var line in File.ReadAllLines(path))
                {
                    if (line.StartsWith("#"))
                    {
                        continue;
                    }
                }
            }

            return material;
        }

        public void Export(MaterialData material, string path)
        {
            var lines = new List<string>
            {
                "# WPF Engine Material File",
                $"Name: {material.Name}",
                $"BaseColor: {material.BaseColor.A},{material.BaseColor.R},{material.BaseColor.G},{material.BaseColor.B}",
                $"Metallic: {material.Metallic}",
                $"Smoothness: {material.Smoothness}"
            };

            File.WriteAllLines(path, lines);
        }
    }

    #endregion

    #region Addressables

    public interface IResourceLocation
    {
        string ProviderId { get; }
        string InternalId { get; }
        string? AssetPath { get; }
        Type AssetType { get; }
        bool IsPlaceholder { get; }
        IReadOnlyList<IResourceLocation> Dependencies { get; }
    }

    public class ResourceLocation : IResourceLocation
    {
        public string ProviderId { get; set; } = string.Empty;
        public string InternalId { get; set; } = string.Empty;
        public string? AssetPath { get; set; }
        public Type AssetType { get; set; } = typeof(object);
        public bool IsPlaceholder { get; set; }
        public List<IResourceLocation> DependenciesInternal { get; set; } = new();

        public IReadOnlyList<IResourceLocation> Dependencies => DependenciesInternal;
    }

    public class AddressablesAsset
    {
        public string Address { get; set; } = string.Empty;
        public GUID Guid { get; set; }
        public string? AssetPath { get; set; }
        public Type AssetType { get; set; } = typeof(object);
        public string GroupName { get; set; } = "Default";
        public bool LoadAsync { get; set; } = true;
        public Dictionary<string, object> Settings { get; set; } = new();

        public AddressablesAsset() { }

        public AddressablesAsset(string address, GUID guid, Type assetType)
        {
            Address = address;
            Guid = guid;
            AssetType = assetType;
        }
    }

    public class AddressablesGroup
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public AddressablesGroupType GroupType { get; set; } = AddressablesGroupType.Static;
        public List<AddressablesAsset> Assets { get; set; } = new();
        public Dictionary<string, object> Settings { get; set; } = new();

        public event EventHandler<AddressablesAsset>? AssetAdded;
        public event EventHandler<AddressablesAsset>? AssetRemoved;
    }

    public enum AddressablesGroupType
    {
        Static,
        Packed,
        PlayableScripts,
        Remote
    }

    public class ContentCatalog
    {
        private readonly Dictionary<string, AddressablesAsset> _addressablesByAddress;
        private readonly Dictionary<GUID, AddressablesAsset> _addressablesByGuid;
        private readonly Dictionary<string, AddressablesGroup> _groups;
        private readonly List<ResourceLocation> _locations;
        private readonly ReaderWriterLockSlim _lock;

        public ContentCatalog()
        {
            _addressablesByAddress = new Dictionary<string, AddressablesAsset>();
            _addressablesByGuid = new Dictionary<GUID, AddressablesAsset>();
            _groups = new Dictionary<string, AddressablesGroup>();
            _locations = new List<ResourceLocation>();
            _lock = new ReaderWriterLockSlim();
        }

        public void AddAsset(AddressablesAsset asset)
        {
            _lock.EnterWriteLock();
            try
            {
                _addressablesByAddress[asset.Address] = asset;
                _addressablesByGuid[asset.Guid] = asset;

                if (!_groups.ContainsKey(asset.GroupName))
                {
                    _groups[asset.GroupName] = new AddressablesGroup { Name = asset.GroupName };
                }
                _groups[asset.GroupName].Assets.Add(asset);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public AddressablesAsset? GetAssetByAddress(string address)
        {
            _lock.EnterReadLock();
            try
            {
                return _addressablesByAddress.TryGetValue(address, out var asset) ? asset : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public AddressablesAsset? GetAssetByGuid(GUID guid)
        {
            _lock.EnterReadLock();
            try
            {
                return _addressablesByGuid.TryGetValue(guid, out var asset) ? asset : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<AddressablesGroup> GetAllGroups()
        {
            _lock.EnterReadLock();
            try
            {
                return _groups.Values.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public AddressablesGroup? GetGroup(string name)
        {
            _lock.EnterReadLock();
            try
            {
                return _groups.TryGetValue(name, out var group) ? group : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void AddLocation(ResourceLocation location)
        {
            _lock.EnterWriteLock();
            try
            {
                _locations.Add(location);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<ResourceLocation> GetLocations(Type assetType)
        {
            _lock.EnterReadLock();
            try
            {
                return _locations.Where(l => l.AssetType == assetType || assetType.IsAssignableFrom(l.AssetType)).ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<byte[]> SerializeAsync()
        {
            return await Task.Run(() =>
            {
                _lock.EnterReadLock();
                try
                {
                    using var stream = new MemoryStream();
                    using var writer = new BinaryWriter(stream);
                    writer.Write(_addressablesByAddress.Count);
                    foreach (var kvp in _addressablesByAddress)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value.Guid.Value);
                        writer.Write(kvp.Value.AssetPath ?? "");
                        writer.Write(kvp.Value.AssetType.AssemblyQualifiedName ?? "");
                        writer.Write(kvp.Value.GroupName);
                    }
                    return stream.ToArray();
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            });
        }
    }

    public class AddressablesManager
    {
        private static AddressablesManager? _instance;
        private readonly ContentCatalog _catalog;
        private readonly ResourceManager _resourceManager;
        private readonly Dictionary<string, object> _loadedAssets;
        private readonly ReaderWriterLockSlim _lock;

        public static AddressablesManager Instance => _instance ??= new AddressablesManager();

        public ContentCatalog Catalog => _catalog;

        public event EventHandler<string>? AssetLoaded;
        public event EventHandler<string>? AssetUnloaded;

        private AddressablesManager()
        {
            _catalog = new ContentCatalog();
            _resourceManager = ResourceManager.Instance;
            _loadedAssets = new Dictionary<string, object>();
            _lock = new ReaderWriterLockSlim();
        }

        public async Task<T?> LoadAssetAsync<T>(string address, CancellationToken cancellationToken = default) where T : class
        {
            var asset = _catalog.GetAssetByAddress(address);
            if (asset == null)
            {
                throw new KeyNotFoundException($"Addressable asset not found: {address}");
            }

            _lock.EnterReadLock();
            if (_loadedAssets.TryGetValue(address, out var existing))
            {
                _lock.ExitReadLock();
                return existing as T;
            }
            _lock.ExitReadLock();

            if (!string.IsNullOrEmpty(asset.AssetPath))
            {
                var loaded = await _resourceManager.LoadAsync<T>(asset.AssetPath, null, cancellationToken);
                if (loaded != null)
                {
                    _lock.EnterWriteLock();
                    _loadedAssets[address] = loaded;
                    _lock.ExitWriteLock();
                    AssetLoaded?.Invoke(this, address);
                }
                return loaded;
            }

            return null;
        }

        public void ReleaseAsset(string address)
        {
            _lock.EnterWriteLock();
            if (_loadedAssets.Remove(address))
            {
                _lock.ExitWriteLock();
                AssetUnloaded?.Invoke(this, address);
            }
            else
            {
                _lock.ExitWriteLock();
            }
        }

        public void ReleaseAllAssets()
        {
            _lock.EnterWriteLock();
            _loadedAssets.Clear();
            _lock.ExitWriteLock();
        }
    }

    #endregion

    #region Bundles

    public enum BundleCompression
    {
        Uncompressed,
        LZMA,
        LZ4
    }

    public class AssetBundle
    {
        private readonly string _name;
        private readonly BundleCompression _compression;
        private readonly Dictionary<string, BundleEntry> _entries;
        private readonly Dictionary<GUID, List<GUID>> _dependencies;
        private readonly ReaderWriterLockSlim _lock;
        private byte[]? _data;

        public string Name => _name;
        public BundleCompression Compression => _compression;
        public int AssetCount => _entries.Count;
        public bool IsLoaded => _data != null;

        public AssetBundle(string name, BundleCompression compression = BundleCompression.LZ4)
        {
            _name = name;
            _compression = compression;
            _entries = new Dictionary<string, BundleEntry>();
            _dependencies = new Dictionary<GUID, List<GUID>>();
            _lock = new ReaderWriterLockSlim();
        }

        public void AddAsset(string assetPath, byte[] data, GUID guid)
        {
            _lock.EnterWriteLock();
            try
            {
                var entry = new BundleEntry
                {
                    AssetPath = assetPath,
                    Offset = 0,
                    CompressedSize = data.Length,
                    DecompressedSize = data.Length,
                    GUID = guid
                };
                _entries[assetPath] = entry;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddDependency(GUID asset, GUID dependency)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_dependencies.ContainsKey(asset))
                {
                    _dependencies[asset] = new List<GUID>();
                }
                _dependencies[asset].Add(dependency);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public List<GUID> GetDependencies(GUID asset)
        {
            _lock.EnterReadLock();
            try
            {
                return _dependencies.TryGetValue(asset, out var deps) ? new List<GUID>(deps) : new List<GUID>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public byte[]? GetAsset(string assetPath)
        {
            _lock.EnterReadLock();
            if (_data == null)
            {
                _lock.ExitReadLock();
                return null;
            }
            _lock.ExitReadLock();

            _lock.EnterReadLock();
            if (_entries.TryGetValue(assetPath, out var entry))
            {
                var result = new byte[entry.DecompressedSize];
                Array.Copy(_data, entry.Offset, result, 0, result.Length);
                _lock.ExitReadLock();
                return result;
            }
            _lock.ExitReadLock();

            return null;
        }

        public async Task<byte[]> CreateBundleDataAsync()
        {
            return await Task.Run(() =>
            {
                _lock.EnterWriteLock();
                try
                {
                    using var stream = new MemoryStream();
                    using var writer = new BinaryWriter(stream);

                    writer.Write(_name);
                    writer.Write((int)_compression);
                    writer.Write(_entries.Count);

                    long dataOffset = stream.Position + 4 + _entries.Count * 128;
                    int offset = (int)dataOffset;

                    foreach (var kvp in _entries)
                    {
                        writer.Write(kvp.Key);
                        writer.Write(kvp.Value.GUID.Value);
                        writer.Write(offset);
                        writer.Write(kvp.Value.CompressedSize);
                        writer.Write(kvp.Value.DecompressedSize);
                        offset += kvp.Value.CompressedSize;
                    }

                    return stream.ToArray();
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            });
        }

        public void LoadBundle(byte[] data)
        {
            _lock.EnterWriteLock();
            _data = Decompress(data);
            _lock.ExitWriteLock();
        }

        private byte[] Decompress(byte[] data)
        {
            return _compression switch
            {
                BundleCompression.LZMA => DecompressLZMA(data),
                BundleCompression.LZ4 => DecompressLZ4(data),
                _ => data
            };
        }

        private byte[] DecompressLZMA(byte[] data)
        {
            try
            {
                using var input = new MemoryStream(data);
                using var lzma = new LZMA.LzmaStream(input, System.IO.Compression.CompressionMode.Decompress);
                using var output = new MemoryStream();
                lzma.CopyTo(output);
                return output.ToArray();
            }
            catch
            {
                return data;
            }
        }

        private byte[] DecompressLZ4(byte[] data)
        {
            return data;
        }

        public void Unload()
        {
            _lock.EnterWriteLock();
            _data = null;
            _lock.ExitWriteLock();
        }
    }

    public class BundleEntry
    {
        public string AssetPath { get; set; } = string.Empty;
        public int Offset { get; set; }
        public int CompressedSize { get; set; }
        public int DecompressedSize { get; set; }
        public GUID GUID { get; set; }
    }

    public class BundleRequest
    {
        private readonly AssetBundle _bundle;
        private readonly List<BundleRequest> _dependencyRequests;
        private Task<byte[]>? _loadingTask;
        private bool _isDone;
        private float _progress;
        private string? _error;

        public AssetBundle Bundle => _bundle;
        public bool IsDone => _isDone;
        public float Progress => _progress;
        public string? Error => _error;

        public event EventHandler<BundleRequest>? Completed;
        public event EventHandler<float>? ProgressChanged;

        public BundleRequest(AssetBundle bundle)
        {
            _bundle = bundle;
            _dependencyRequests = new List<BundleRequest>();
            _isDone = false;
            _progress = 0f;
        }

        public BundleRequest(AssetBundle bundle, IEnumerable<BundleRequest> dependencies)
        {
            _bundle = bundle;
            _dependencyRequests = dependencies.ToList();
            _isDone = false;
            _progress = 0f;
        }

        public async Task<AssetBundle?> LoadAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                foreach (var dep in _dependencyRequests)
                {
                    await dep.LoadAsync(cancellationToken);
                    UpdateProgress();
                }

                if (_bundle.IsLoaded)
                {
                    Complete();
                    return _bundle;
                }

                _loadingTask = Task.Run(() => new byte[0], cancellationToken);
                var data = await _loadingTask;
                _bundle.LoadBundle(data);
                Complete();
                return _bundle;
            }
            catch (Exception ex)
            {
                _error = ex.Message;
                _isDone = true;
                return null;
            }
        }

        private void UpdateProgress()
        {
            int depsLoaded = _dependencyRequests.Count(d => d.IsDone);
            _progress = depsLoaded / (float)(_dependencyRequests.Count + 1);
            ProgressChanged?.Invoke(this, _progress);
        }

        private void Complete()
        {
            _isDone = true;
            _progress = 1f;
            Completed?.Invoke(this, this);
        }
    }

    public class BundleDependencies
    {
        private readonly Dictionary<GUID, List<GUID>> _dependencies;
        private readonly Dictionary<GUID, List<GUID>> _dependents;
        private readonly ReaderWriterLockSlim _lock;

        public BundleDependencies()
        {
            _dependencies = new Dictionary<GUID, List<GUID>>();
            _dependents = new Dictionary<GUID, List<GUID>>();
            _lock = new ReaderWriterLockSlim();
        }

        public void AddDependency(GUID asset, GUID dependency)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_dependencies.ContainsKey(asset))
                {
                    _dependencies[asset] = new List<GUID>();
                }
                _dependencies[asset].Add(dependency);

                if (!_dependents.ContainsKey(dependency))
                {
                    _dependents[dependency] = new List<GUID>();
                }
                _dependents[dependency].Add(asset);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public List<GUID> GetDependencies(GUID asset)
        {
            _lock.EnterReadLock();
            try
            {
                return _dependencies.TryGetValue(asset, out var deps) ? new List<GUID>(deps) : new List<GUID>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<GUID> GetDependents(GUID asset)
        {
            _lock.EnterReadLock();
            try
            {
                return _dependents.TryGetValue(asset, out var deps) ? new List<GUID>(deps) : new List<GUID>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public List<GUID> GetAllDependencies(GUID asset)
        {
            var result = new List<GUID>();
            var toProcess = new Queue<GUID>();
            toProcess.Enqueue(asset);

            while (toProcess.Count > 0)
            {
                var current = toProcess.Dequeue();
                var deps = GetDependencies(current);
                foreach (var dep in deps)
                {
                    if (!result.Contains(dep))
                    {
                        result.Add(dep);
                        toProcess.Enqueue(dep);
                    }
                }
            }

            return result;
        }

        public bool HasCircularDependency(GUID asset)
        {
            var visited = new HashSet<GUID>();
            var path = new HashSet<GUID>();
            return DfsDetectCycle(asset, visited, path);
        }

        private bool DfsDetectCycle(GUID current, HashSet<GUID> visited, HashSet<GUID> path)
        {
            if (path.Contains(current)) return true;
            if (visited.Contains(current)) return false;

            visited.Add(current);
            path.Add(current);

            foreach (var dep in GetDependencies(current))
            {
                if (DfsDetectCycle(dep, visited, path))
                    return true;
            }

            path.Remove(current);
            return false;
        }
    }

    public class BundlePacking
    {
        private readonly List<PackedAsset> _packedAssets;
        private readonly long _maxBundleSize;
        private readonly ReaderWriterLockSlim _lock;

        public BundlePacking(long maxBundleSize = 50 * 1024 * 1024)
        {
            _packedAssets = new List<PackedAsset>();
            _maxBundleSize = maxBundleSize;
            _lock = new ReaderWriterLockSlim();
        }

        public List<AssetBundle> Pack(IEnumerable<AssetInfo> assets)
        {
            var bundles = new List<AssetBundle>();
            var currentBundle = new AssetBundle($"bundle_{bundles.Count}");
            long currentSize = 0;

            foreach (var asset in assets.OrderBy(a => a.Dependencies.Count))
            {
                if (currentSize + asset.FileSize > _maxBundleSize && currentBundle.AssetCount > 0)
                {
                    bundles.Add(currentBundle);
                    currentBundle = new AssetBundle($"bundle_{bundles.Count}");
                    currentSize = 0;
                }

                if (File.Exists(asset.SourcePath))
                {
                    var data = File.ReadAllBytes(asset.SourcePath);
                    currentBundle.AddAsset(asset.AssetPath, data, asset.Guid);
                    currentSize += asset.FileSize;
                }
            }

            if (currentBundle.AssetCount > 0)
            {
                bundles.Add(currentBundle);
            }

            return bundles;
        }

        public void RecordPackedAsset(string assetPath, string bundleName, int offset)
        {
            _lock.EnterWriteLock();
            try
            {
                _packedAssets.Add(new PackedAsset
                {
                    AssetPath = assetPath,
                    BundleName = bundleName,
                    Offset = offset
                });
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public string? GetBundleName(string assetPath)
        {
            _lock.EnterReadLock();
            try
            {
                return _packedAssets.FirstOrDefault(p => p.AssetPath == assetPath)?.BundleName;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public class PackedAsset
    {
        public string AssetPath { get; set; } = string.Empty;
        public string BundleName { get; set; } = string.Empty;
        public int Offset { get; set; }
    }

    #endregion

    #region Caching

    public class ResourceCache
    {
        private readonly Dictionary<string, CacheEntry> _cache;
        private readonly LinkedList<string> _accessOrder;
        private readonly ReaderWriterLockSlim _lock;
        private readonly ICachePolicy _policy;
        private long _currentSize;
        private long _maxSize;

        public long CurrentSize => _currentSize;
        public long MaxSize => _maxSize;
        public int EntryCount => _cache.Count;

        public event EventHandler<string>? CacheEvicted;
        public event EventHandler<string>? CacheHit;
        public event EventHandler<string>? CacheMiss;

        public ResourceCache(ICachePolicy policy, long maxSize = 100 * 1024 * 1024)
        {
            _cache = new Dictionary<string, CacheEntry>();
            _accessOrder = new LinkedList<string>();
            _lock = new ReaderWriterLockSlim();
            _policy = policy;
            _maxSize = maxSize;
            _currentSize = 0;
        }

        public void Set(string key, object value, long size)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_cache.TryGetValue(key, out var existing))
                {
                    _currentSize -= existing.Size;
                    _accessOrder.Remove(existing.AccessNode);
                }

                while (_currentSize + size > _maxSize && _accessOrder.Count > 0)
                {
                    EvictLRU();
                }

                var node = _accessOrder.AddLast(key);
                _cache[key] = new CacheEntry
                {
                    Value = value,
                    Size = size,
                    AccessNode = node,
                    CreatedAt = DateTime.Now
                };
                _currentSize += size;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TryGet(string key, out object? value)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    if (_policy.ShouldEvict(entry, DateTime.Now))
                    {
                        _cache.Remove(key);
                        _currentSize -= entry.Size;
                        _accessOrder.Remove(entry.AccessNode);
                        value = null;
                        CacheEvicted?.Invoke(this, key);
                        return false;
                    }

                    _accessOrder.Remove(entry.AccessNode);
                    entry.AccessNode = _accessOrder.AddLast(key);
                    entry.LastAccess = DateTime.Now;
                    value = entry.Value;
                    CacheHit?.Invoke(this, key);
                    return true;
                }

                value = null;
                CacheMiss?.Invoke(this, key);
                return false;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Contains(string key)
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.ContainsKey(key);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Remove(string key)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_cache.TryGetValue(key, out var entry))
                {
                    _currentSize -= entry.Size;
                    _accessOrder.Remove(entry.AccessNode);
                    _cache.Remove(key);
                }
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
                _cache.Clear();
                _accessOrder.Clear();
                _currentSize = 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private void EvictLRU()
        {
            if (_accessOrder.Count == 0) return;

            var lruKey = _accessOrder.First!.Value;
            var entry = _cache[lruKey];

            _cache.Remove(lruKey);
            _accessOrder.RemoveFirst();
            _currentSize -= entry.Size;

            CacheEvicted?.Invoke(this, lruKey);
        }

        public Dictionary<string, CacheStats> GetStats()
        {
            _lock.EnterReadLock();
            try
            {
                return _cache.ToDictionary(
                    kvp => kvp.Key,
                    kvp => new CacheStats
                    {
                        Size = kvp.Value.Size,
                        CreatedAt = kvp.Value.CreatedAt,
                        LastAccess = kvp.Value.LastAccess,
                        AccessCount = kvp.Value.AccessCount
                    });
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    public class CacheEntry
    {
        public object Value { get; set; } = null!;
        public long Size { get; set; }
        public LinkedListNode<string>? AccessNode { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccess { get; set; }
        public int AccessCount { get; set; }
    }

    public interface ICachePolicy
    {
        bool ShouldEvict(CacheEntry entry, DateTime now);
    }

    public class MemoryCachePolicy : ICachePolicy
    {
        private readonly long _maxMemoryMB;

        public MemoryCachePolicy(long maxMemoryMB = 256)
        {
            _maxMemoryMB = maxMemoryMB;
        }

        public bool ShouldEvict(CacheEntry entry, DateTime now)
        {
            return false;
        }
    }

    public class TimeBasedCachePolicy : ICachePolicy
    {
        private readonly TimeSpan _maxAge;

        public TimeBasedCachePolicy(TimeSpan maxAge)
        {
            _maxAge = maxAge;
        }

        public bool ShouldEvict(CacheEntry entry, DateTime now)
        {
            return (now - entry.CreatedAt) > _maxAge;
        }
    }

    public class LRUCachePolicy : ICachePolicy
    {
        private readonly TimeSpan _unusedThreshold;

        public LRUCachePolicy(TimeSpan unusedThreshold)
        {
            _unusedThreshold = unusedThreshold;
        }

        public bool ShouldEvict(CacheEntry entry, DateTime now)
        {
            return (now - entry.LastAccess) > _unusedThreshold;
        }
    }

    public class CacheStats
    {
        public long Size { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime LastAccess { get; set; }
        public int AccessCount { get; set; }
    }

    public class CachePolicy
    {
        public TimeSpan? MaxAge { get; set; }
        public long MaxMemoryBytes { get; set; } = 256 * 1024 * 1024;
        public int MaxEntryCount { get; set; } = 1000;
        public bool EnableDiskCache { get; set; } = false;
        public string? DiskCachePath { get; set; }

        public static CachePolicy Default => new CachePolicy
        {
            MaxAge = TimeSpan.FromMinutes(30),
            MaxMemoryBytes = 256 * 1024 * 1024,
            MaxEntryCount = 1000
        };

        public static CachePolicy Aggressive => new CachePolicy
        {
            MaxAge = TimeSpan.FromHours(2),
            MaxMemoryBytes = 512 * 1024 * 1024,
            MaxEntryCount = 5000,
            EnableDiskCache = true
        };

        public static CachePolicy Minimal => new CachePolicy
        {
            MaxAge = TimeSpan.FromMinutes(5),
            MaxMemoryBytes = 64 * 1024 * 1024,
            MaxEntryCount = 100
        };
    }

    public class PreloadQueue
    {
        private readonly PriorityQueue<PreloadItem, int> _queue;
        private readonly HashSet<string> _queued;
        private readonly ReaderWriterLockSlim _lock;
        private readonly ResourceManager _resourceManager;

        public PreloadQueue(ResourceManager resourceManager)
        {
            _queue = new PriorityQueue<PreloadItem, int>();
            _queued = new HashSet<string>();
            _lock = new ReaderWriterLockSlim();
            _resourceManager = resourceManager;
        }

        public void Enqueue(string assetPath, int priority = 0)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_queued.Contains(assetPath))
                {
                    _queue.Enqueue(new PreloadItem { AssetPath = assetPath }, -priority);
                    _queued.Add(assetPath);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void EnqueueRange(IEnumerable<string> assetPaths, int basePriority = 0)
        {
            int priority = basePriority;
            _lock.EnterWriteLock();
            try
            {
                foreach (var path in assetPaths)
                {
                    if (!_queued.Contains(path))
                    {
                        _queue.Enqueue(new PreloadItem { AssetPath = path }, -priority);
                        _queued.Add(path);
                    }
                    priority++;
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public async Task LoadNextAsync(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
        {
            PreloadItem item;
            _lock.EnterWriteLock();
            try
            {
                if (_queue.Count == 0)
                {
                    return;
                }
                _queue.TryDequeue(out item, out _);
                _queued.Remove(item.AssetPath);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

            await _resourceManager.LoadAsync<object>(item.AssetPath, null, cancellationToken);
            progress?.Report(1f);
        }

        public async Task LoadAllAsync(int maxConcurrent = 4, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
        {
            int total = Count;
            int loaded = 0;

            var tasks = new List<Task>();
            for (int i = 0; i < maxConcurrent && Count > 0; i++)
            {
                tasks.Add(Task.Run(async () =>
                {
                    while (true)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!await LoadNextAsync(null, cancellationToken))
                        {
                            break;
                        }
                        int count = Interlocked.Increment(ref loaded);
                        progress?.Report(count / (float)total);
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }

        public int Count
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _queue.Count;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _queue.Clear();
                _queued.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public class PreloadItem
    {
        public string AssetPath { get; set; } = string.Empty;
        public DateTime EnqueuedAt { get; set; } = DateTime.Now;
    }

    #endregion

    #region Loading

    public class AsyncOperation
    {
        private volatile bool _isDone;
        private volatile float _progress;
        private volatile bool _canceled;
        private string? _error;
        private readonly object _lock = new object();

        public bool IsDone
        {
            get => _isDone;
            protected set => _isDone = value;
        }

        public float Progress
        {
            get => _progress;
            protected set
            {
                _progress = Math.Clamp(value, 0f, 1f);
                ProgressChanged?.Invoke(this, _progress);
            }
        }

        public bool Canceled => _canceled;
        public string? Error => _error;

        public event EventHandler<float>? ProgressChanged;
        public event EventHandler<AsyncOperation>? Completed;

        public virtual void Cancel()
        {
            lock (_lock)
            {
                _canceled = true;
            }
        }

        protected void Complete()
        {
            _isDone = true;
            _progress = 1f;
            Completed?.Invoke(this, this);
        }

        protected void Fail(string error)
        {
            _error = error;
            _isDone = true;
            Progress = 1f;
            Completed?.Invoke(this, this);
        }
    }

    public class ResourceRequestAsync : AsyncOperation
    {
        private readonly ResourceManager _resourceManager;
        private readonly string _path;
        private readonly Type _assetType;
        private Task<object?>? _loadingTask;

        public object? Result { get; private set; }

        public event EventHandler<object?>? ResourceLoaded;

        public ResourceRequestAsync(ResourceManager resourceManager, string path, Type assetType)
        {
            _resourceManager = resourceManager;
            _path = path;
            _assetType = assetType;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            _loadingTask = _resourceManager.LoadAsync<object>(_path, new Progress<float>(p => Progress = p), cancellationToken);

            try
            {
                Result = await _loadingTask;
                if (Result != null)
                {
                    ResourceLoaded?.Invoke(this, Result);
                }
                Complete();
            }
            catch (Exception ex)
            {
                Fail(ex.Message);
            }
        }

        public override void Cancel()
        {
            base.Cancel();
            _loadingTask?.ContinueWith(t => { });
        }
    }

    public class BackgroundLoader
    {
        private readonly BlockingCollection<LoadTask> _taskQueue;
        private readonly Thread[] _workerThreads;
        private readonly CancellationTokenSource _cts;
        private readonly ResourceManager _resourceManager;
        private readonly int _threadCount;

        public BackgroundLoader(ResourceManager resourceManager, int threadCount = 2)
        {
            _resourceManager = resourceManager;
            _threadCount = threadCount;
            _taskQueue = new BlockingCollection<LoadTask>();
            _cts = new CancellationTokenSource();
            _workerThreads = new Thread[_threadCount];

            for (int i = 0; i < _threadCount; i++)
            {
                _workerThreads[i] = new Thread(WorkerLoop)
                {
                    IsBackground = true,
                    Name = $"BackgroundLoader_{i}"
                };
            }
        }

        public void Start()
        {
            foreach (var thread in _workerThreads)
            {
                thread.Start();
            }
        }

        public void Stop()
        {
            _cts.Cancel();
            _taskQueue.CompleteAdding();

            foreach (var thread in _workerThreads)
            {
                if (thread.Join(TimeSpan.FromSeconds(5)))
                {
                    thread.Interrupt();
                }
            }
        }

        public LoadTask Enqueue(string path, Type assetType, int priority = 0)
        {
            var task = new LoadTask
            {
                Path = path,
                AssetType = assetType,
                Priority = priority,
                CreatedAt = DateTime.Now
            };

            _taskQueue.Add(task);
            return task;
        }

        private void WorkerLoop()
        {
            try
            {
                while (!_taskQueue.IsAddingCompleted)
                {
                    try
                    {
                        if (_taskQueue.TryTake(out var task, Timeout.Infinite, _cts.Token))
                        {
                            ProcessTask(task);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private async void ProcessTask(LoadTask task)
        {
            try
            {
                task.Status = LoadTaskStatus.Loading;
                var result = await _resourceManager.LoadAsync<object>(task.Path, new Progress<float>(p => task.Progress = p), _cts.Token);
                task.Result = result;
                task.Status = LoadTaskStatus.Completed;
                task.Completed?.Invoke(task);
            }
            catch (Exception ex)
            {
                task.Status = LoadTaskStatus.Failed;
                task.Error = ex.Message;
                task.Failed?.Invoke(task);
            }
        }
    }

    public class LoadTask
    {
        public string Path { get; set; } = string.Empty;
        public Type AssetType { get; set; } = typeof(object);
        public int Priority { get; set; }
        public DateTime CreatedAt { get; set; }
        public float Progress { get; set; }
        public object? Result { get; private set; }
        public LoadTaskStatus Status { get; set; }
        public string? Error { get; private set; }

        public event EventHandler<LoadTask>? Completed;
        public event EventHandler<LoadTask>? Failed;
    }

    public enum LoadTaskStatus
    {
        Pending,
        Loading,
        Completed,
        Failed,
        Canceled
    }

    public class LoadingScreen
    {
        private readonly List<AsyncOperation> _trackedOperations;
        private readonly object _lock = new object();

        public event EventHandler<float>? GlobalProgressChanged;
        public event EventHandler<string>? StatusChanged;
        public event EventHandler? LoadingComplete;

        public string Status { get; private set; } = string.Empty;
        public float GlobalProgress { get; private set; }
        public bool IsLoading => _trackedOperations.Any(o => !o.IsDone);

        public LoadingScreen()
        {
            _trackedOperations = new List<AsyncOperation>();
        }

        public void TrackOperation(AsyncOperation operation)
        {
            lock (_lock)
            {
                _trackedOperations.Add(operation);
                operation.ProgressChanged += OnProgressChanged;
                operation.Completed += OnCompleted;
            }
        }

        public void UntrackOperation(AsyncOperation operation)
        {
            lock (_lock)
            {
                _trackedOperations.Remove(operation);
                operation.ProgressChanged -= OnProgressChanged;
                operation.Completed -= OnCompleted;
            }
        }

        private void OnProgressChanged(object? sender, float progress)
        {
            UpdateGlobalProgress();
        }

        private void OnCompleted(object? sender, AsyncOperation operation)
        {
            UpdateGlobalProgress();
            if (!IsLoading)
            {
                LoadingComplete?.Invoke(this, EventArgs.Empty);
            }
        }

        private void UpdateGlobalProgress()
        {
            lock (_lock)
            {
                if (_trackedOperations.Count == 0)
                {
                    GlobalProgress = 1f;
                    return;
                }

                float totalProgress = _trackedOperations.Sum(o => o.Progress);
                GlobalProgress = totalProgress / _trackedOperations.Count;
                GlobalProgressChanged?.Invoke(this, GlobalProgress);
            }
        }

        public void SetStatus(string status)
        {
            Status = status;
            StatusChanged?.Invoke(this, status);
        }
    }

    #endregion

    #region Dependencies

    public class DependencyGraph
    {
        private readonly Dictionary<GUID, HashSet<GUID>> _dependencies;
        private readonly Dictionary<GUID, HashSet<GUID>> _dependents;
        private readonly ReaderWriterLockSlim _lock;

        public DependencyGraph()
        {
            _dependencies = new Dictionary<GUID, HashSet<GUID>>();
            _dependents = new Dictionary<GUID, HashSet<GUID>>();
            _lock = new ReaderWriterLockSlim();
        }

        public void AddNode(GUID guid)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_dependencies.ContainsKey(guid))
                {
                    _dependencies[guid] = new HashSet<GUID>();
                }
                if (!_dependents.ContainsKey(guid))
                {
                    _dependents[guid] = new HashSet<GUID>();
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void AddDependency(GUID from, GUID to)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_dependencies.ContainsKey(from))
                {
                    _dependencies[from] = new HashSet<GUID>();
                }
                _dependencies[from].Add(to);

                if (!_dependents.ContainsKey(to))
                {
                    _dependents[to] = new HashSet<GUID>();
                }
                _dependents[to].Add(from);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RemoveDependency(GUID from, GUID to)
        {
            _lock.EnterWriteLock();
            try
            {
                _dependencies[from]?.Remove(to);
                _dependents[to]?.Remove(from);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<GUID> GetDependencies(GUID guid)
        {
            _lock.EnterReadLock();
            try
            {
                return _dependencies.TryGetValue(guid, out var deps) ? deps.ToList() : Enumerable.Empty<GUID>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<GUID> GetDependents(GUID guid)
        {
            _lock.EnterReadLock();
            try
            {
                return _dependents.TryGetValue(guid, out var deps) ? deps.ToList() : Enumerable.Empty<GUID>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<GUID> GetAllDependencies(GUID guid)
        {
            var result = new List<GUID>();
            var visited = new HashSet<GUID>();
            DfsCollect(guid, visited, result, _dependencies);
            return result;
        }

        public IEnumerable<GUID> GetAllDependents(GUID guid)
        {
            var result = new List<GUID>();
            var visited = new HashSet<GUID>();
            DfsCollect(guid, visited, result, _dependents);
            return result;
        }

        private void DfsCollect(GUID start, HashSet<GUID> visited, List<GUID> result, Dictionary<GUID, HashSet<GUID>> graph)
        {
            var stack = new Stack<GUID>();
            stack.Push(start);

            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (visited.Contains(current)) continue;

                visited.Add(current);
                if (!current.Equals(start))
                {
                    result.Add(current);
                }

                if (graph.TryGetValue(current, out var neighbors))
                {
                    foreach (var neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            stack.Push(neighbor);
                        }
                    }
                }
            }
        }

        public List<List<GUID>> FindCircularDependencies()
        {
            var cycles = new List<List<GUID>>();
            var visited = new HashSet<GUID>();
            var recursionStack = new HashSet<GUID>();
            var path = new List<GUID>();

            foreach (var node in _dependencies.Keys)
            {
                if (!visited.Contains(node))
                {
                    FindCyclesDFS(node, visited, recursionStack, path, cycles);
                }
            }

            return cycles;
        }

        private void FindCyclesDFS(GUID current, HashSet<GUID> visited, HashSet<GUID> recursionStack, List<GUID> path, List<List<GUID>> cycles)
        {
            visited.Add(current);
            recursionStack.Add(current);
            path.Add(current);

            if (_dependencies.TryGetValue(current, out var deps))
            {
                foreach (var dep in deps)
                {
                    if (!visited.Contains(dep))
                    {
                        FindCyclesDFS(dep, visited, recursionStack, path, cycles);
                    }
                    else if (recursionStack.Contains(dep))
                    {
                        var cycleStart = path.IndexOf(dep);
                        var cycle = path.Skip(cycleStart).Take(path.Count - cycleStart).ToList();
                        cycle.Add(dep);
                        cycles.Add(cycle);
                    }
                }
            }

            path.RemoveAt(path.Count - 1);
            recursionStack.Remove(current);
        }
    }

    public class CircularDependencyChecker
    {
        private readonly DependencyGraph _graph;

        public CircularDependencyChecker(DependencyGraph graph)
        {
            _graph = graph;
        }

        public bool HasCircularDependencies()
        {
            return _graph.FindCircularDependencies().Count > 0;
        }

        public bool WouldCreateCycle(GUID from, GUID to)
        {
            var deps = _graph.GetAllDependents(to);
            return deps.Contains(from);
        }

        public List<List<GUID>> GetAllCycles()
        {
            return _graph.FindCircularDependencies();
        }

        public string FormatCycle(List<GUID> cycle)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < cycle.Count; i++)
            {
                sb.Append(cycle[i].Value);
                if (i < cycle.Count - 1)
                {
                    sb.Append(" -> ");
                }
            }
            return sb.ToString();
        }
    }

    public class LazyDependencyResolver
    {
        private readonly DependencyGraph _graph;
        private readonly Dictionary<GUID, Lazy<object?>> _lazyInstances;
        private readonly Func<GUID, object?> _factory;
        private readonly ReaderWriterLockSlim _lock;

        public LazyDependencyResolver(DependencyGraph graph, Func<GUID, object?> factory)
        {
            _graph = graph;
            _factory = factory;
            _lazyInstances = new Dictionary<GUID, Lazy<object?>>();
            _lock = new ReaderWriterLockSlim();
        }

        public object? GetOrCreate(GUID guid)
        {
            _lock.EnterReadLock();
            if (_lazyInstances.TryGetValue(guid, out var lazy))
            {
                _lock.ExitReadLock();
                return lazy.Value;
            }
            _lock.ExitReadLock();

            _lock.EnterWriteLock();
            try
            {
                if (_lazyInstances.TryGetValue(guid, out lazy))
                {
                    return lazy.Value;
                }

                lazy = new Lazy<object?>(() => CreateWithDependencies(guid));
                _lazyInstances[guid] = lazy;
                return lazy.Value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private object? CreateWithDependencies(GUID guid)
        {
            var dependencies = _graph.GetDependencies(guid);
            foreach (var dep in dependencies)
            {
                GetOrCreate(dep);
            }

            return _factory(guid);
        }

        public void Preload(GUID guid)
        {
            GetOrCreate(guid);
        }

        public void PreloadAll()
        {
            foreach (var guid in _graph.FindCircularDependencies().SelectMany(c => c).Distinct())
            {
                GetOrCreate(guid);
            }
        }

        public bool IsLoaded(GUID guid)
        {
            _lock.EnterReadLock();
            try
            {
                return _lazyInstances.TryGetValue(guid, out var lazy) && lazy.IsValueCreated;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }

    #endregion

    #region Profiling

    public class ResourceProfiler
    {
        private static ResourceProfiler? _instance;
        private readonly Dictionary<string, MemoryRecord> _memoryRecords;
        private readonly Dictionary<string, LoadRecord> _loadRecords;
        private readonly ReaderWriterLockSlim _lock;
        private long _totalMemoryUsed;
        private int _totalAssetsLoaded;
        private long _totalBytesRead;
        private readonly Stopwatch _timer;

        public static ResourceProfiler Instance => _instance ??= new ResourceProfiler();

        public long TotalMemoryUsed => _totalMemoryUsed;
        public int TotalAssetsLoaded => _totalAssetsLoaded;
        public long TotalBytesRead => _totalBytesRead;

        public event EventHandler<MemoryRecord>? MemoryAllocated;
        public event EventHandler<MemoryRecord>? MemoryFreed;

        private ResourceProfiler()
        {
            _memoryRecords = new Dictionary<string, MemoryRecord>();
            _loadRecords = new Dictionary<string, LoadRecord>();
            _lock = new ReaderWriterLockSlim();
            _timer = Stopwatch.StartNew();
        }

        public void RecordAllocation(string path, long size, Type type)
        {
            _lock.EnterWriteLock();
            try
            {
                var record = new MemoryRecord
                {
                    Path = path,
                    Type = type,
                    Size = size,
                    AllocatedAt = DateTime.Now,
                    LoadTime = _timer.Elapsed.TotalSeconds
                };

                _memoryRecords[path] = record;
                _totalMemoryUsed += size;
                _totalAssetsLoaded++;

                MemoryAllocated?.Invoke(this, record);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RecordDeallocation(string path)
        {
            _lock.EnterWriteLock();
            try
            {
                if (_memoryRecords.TryGetValue(path, out var record))
                {
                    _totalMemoryUsed -= record.Size;
                    record.FreedAt = DateTime.Now;
                    MemoryFreed?.Invoke(this, record);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void RecordLoadTime(string path, double timeMs)
        {
            _lock.EnterWriteLock();
            try
            {
                var record = new LoadRecord
                {
                    Path = path,
                    LoadTimeMs = timeMs,
                    LoadedAt = DateTime.Now
                };
                _loadRecords[path] = record;
                _totalBytesRead += record.LoadTimeMs > 0 ? (long)record.LoadTimeMs * 1000 : 0;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public MemoryRecord? GetMemoryRecord(string path)
        {
            _lock.EnterReadLock();
            try
            {
                return _memoryRecords.TryGetValue(path, out var record) ? record : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<MemoryRecord> GetTopMemoryUsers(int count = 10)
        {
            _lock.EnterReadLock();
            try
            {
                return _memoryRecords.Values
                    .OrderByDescending(r => r.Size)
                    .Take(count)
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Dictionary<Type, long> GetMemoryByType()
        {
            _lock.EnterReadLock();
            try
            {
                return _memoryRecords.Values
                    .GroupBy(r => r.Type)
                    .ToDictionary(g => g.Key, g => g.Sum(r => r.Size));
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<LoadRecord> GetSlowestLoads(int count = 10)
        {
            _lock.EnterReadLock();
            try
            {
                return _loadRecords.Values
                    .OrderByDescending(r => r.LoadTimeMs)
                    .Take(count)
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public ProfilerSnapshot CreateSnapshot()
        {
            _lock.EnterReadLock();
            try
            {
                return new ProfilerSnapshot
                {
                    Timestamp = DateTime.Now,
                    TotalMemoryUsed = _totalMemoryUsed,
                    TotalAssetsLoaded = _totalAssetsLoaded,
                    MemoryRecords = _memoryRecords.Values.ToList(),
                    LoadRecords = _loadRecords.Values.ToList(),
                    MemoryByType = GetMemoryByType()
                };
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Reset()
        {
            _lock.EnterWriteLock();
            try
            {
                _memoryRecords.Clear();
                _loadRecords.Clear();
                _totalMemoryUsed = 0;
                _totalAssetsLoaded = 0;
                _totalBytesRead = 0;
                _timer.Restart();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public class MemoryRecord
    {
        public string Path { get; set; } = string.Empty;
        public Type Type { get; set; } = typeof(object);
        public long Size { get; set; }
        public DateTime AllocatedAt { get; set; }
        public DateTime? FreedAt { get; set; }
        public double LoadTime { get; set; }
    }

    public class LoadRecord
    {
        public string Path { get; set; } = string.Empty;
        public double LoadTimeMs { get; set; }
        public DateTime LoadedAt { get; set; }
    }

    public class ProfilerSnapshot
    {
        public DateTime Timestamp { get; set; }
        public long TotalMemoryUsed { get; set; }
        public int TotalAssetsLoaded { get; set; }
        public List<MemoryRecord> MemoryRecords { get; set; } = new();
        public List<LoadRecord> LoadRecords { get; set; } = new();
        public Dictionary<Type, long> MemoryByType { get; set; } = new();
    }

    public class AssetUsageTracker
    {
        private readonly Dictionary<string, UsageInfo> _usageData;
        private readonly ReaderWriterLockSlim _lock;

        public AssetUsageTracker()
        {
            _usageData = new Dictionary<string, UsageInfo>();
            _lock = new ReaderWriterLockSlim();
        }

        public void TrackUsage(string assetPath)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_usageData.TryGetValue(assetPath, out var info))
                {
                    info = new UsageInfo { Path = assetPath };
                    _usageData[assetPath] = info;
                }

                info.UseCount++;
                info.LastUsed = DateTime.Now;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void TrackReference(string fromPath, string toPath)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_usageData.TryGetValue(toPath, out var info))
                {
                    info = new UsageInfo { Path = toPath };
                    _usageData[toPath] = info;
                }

                if (!info.ReferencedBy.Contains(fromPath))
                {
                    info.ReferencedBy.Add(fromPath);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public UsageInfo? GetUsageInfo(string assetPath)
        {
            _lock.EnterReadLock();
            try
            {
                return _usageData.TryGetValue(assetPath, out var info) ? info : null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<UsageInfo> GetMostUsed(int count = 10)
        {
            _lock.EnterReadLock();
            try
            {
                return _usageData.Values
                    .OrderByDescending(u => u.UseCount)
                    .Take(count)
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<UsageInfo> GetRecentlyUsed(TimeSpan since)
        {
            _lock.EnterReadLock();
            try
            {
                var cutoff = DateTime.Now - since;
                return _usageData.Values
                    .Where(u => u.LastUsed >= cutoff)
                    .OrderByDescending(u => u.LastUsed)
                    .ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public void Reset()
        {
            _lock.EnterWriteLock();
            try
            {
                _usageData.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public class UsageInfo
    {
        public string Path { get; set; } = string.Empty;
        public int UseCount { get; set; }
        public DateTime LastUsed { get; set; }
        public DateTime FirstUsed { get; set; } = DateTime.Now;
        public List<string> ReferencedBy { get; set; } = new();
    }

    public class UnusedAssetFinder
    {
        private readonly AssetDatabase _database;
        private readonly AssetUsageTracker _usageTracker;
        private readonly TimeSpan _unusedThreshold;

        public UnusedAssetFinder(AssetDatabase database, AssetUsageTracker usageTracker, TimeSpan? unusedThreshold = null)
        {
            _database = database;
            _usageTracker = usageTracker;
            _unusedThreshold = unusedThreshold ?? TimeSpan.FromDays(7);
        }

        public IEnumerable<AssetInfo> FindUnusedAssets()
        {
            var cutoff = DateTime.Now - _unusedThreshold;
            var allAssets = _database.FindAssetsOfType(typeof(object));
            var unused = new List<AssetInfo>();

            foreach (var asset in allAssets)
            {
                var usage = _usageTracker.GetUsageInfo(asset.AssetPath);
                if (usage == null || usage.LastUsed < cutoff)
                {
                    var dependents = _database.GetDependents(asset.Guid);
                    if (dependents.Count == 0)
                    {
                        unused.Add(asset);
                    }
                }
            }

            return unused;
        }

        public IEnumerable<AssetInfo> FindDuplicateAssets()
        {
            var duplicates = new Dictionary<string, List<AssetInfo>>();

            foreach (var asset in _database.FindAssetsOfType(typeof(object)))
            {
                var fileName = Path.GetFileName(asset.SourcePath);
                if (!duplicates.ContainsKey(fileName))
                {
                    duplicates[fileName] = new List<AssetInfo>();
                }
                duplicates[fileName].Add(asset);
            }

            return duplicates.Values
                .Where(list => list.Count > 1)
                .SelectMany(list => list.Skip(1));
        }

        public IEnumerable<AssetInfo> FindLargeUnusedAssets(long minSizeBytes = 1024 * 1024)
        {
            return FindUnusedAssets()
                .Where(a => a.FileSize >= minSizeBytes)
                .OrderByDescending(a => a.FileSize);
        }
    }

    #endregion

    #region Virtual Filesystem

    public interface IVirtualFile
    {
        string Path { get; }
        string Name { get; }
        bool Exists { get; }
        long Size { get; }
        DateTime LastModified { get; }
        Stream OpenRead();
        Stream OpenWrite();
        void Delete();
    }

    public class VirtualFile : IVirtualFile
    {
        private readonly string _physicalPath;
        private readonly IMountPoint _mountPoint;

        public string Path { get; }
        public string Name => System.IO.Path.GetFileName(Path);
        public bool Exists => File.Exists(_physicalPath);
        public long Size => Exists ? new FileInfo(_physicalPath).Length : 0;
        public DateTime LastModified => Exists ? File.GetLastWriteTime(_physicalPath) : DateTime.MinValue;

        public VirtualFile(string virtualPath, string physicalPath, IMountPoint mountPoint)
        {
            Path = virtualPath;
            _physicalPath = physicalPath;
            _mountPoint = mountPoint;
        }

        public Stream OpenRead()
        {
            if (!Exists)
                throw new FileNotFoundException($"Virtual file not found: {Path}");
            return File.OpenRead(_physicalPath);
        }

        public Stream OpenWrite()
        {
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(_physicalPath) ?? "");
            return File.Create(_physicalPath);
        }

        public void Delete()
        {
            if (Exists)
            {
                File.Delete(_physicalPath);
            }
        }
    }

    public interface IMountPoint
    {
        string Name { get; }
        string PhysicalPath { get; }
        int Priority { get; }
        bool HandlesPath(string virtualPath);
        IVirtualFile? GetFile(string virtualPath);
        IEnumerable<IVirtualFile> GetFiles(string searchPattern);
    }

    public class MountPoint : IMountPoint
    {
        private readonly string _name;
        private readonly string _physicalPath;
        private readonly int _priority;
        private readonly string _virtualPrefix;

        public string Name => _name;
        public string PhysicalPath => _physicalPath;
        public int Priority => _priority;

        public MountPoint(string name, string physicalPath, string virtualPrefix = "", int priority = 0)
        {
            _name = name;
            _physicalPath = physicalPath;
            _virtualPrefix = virtualPrefix;
            _priority = priority;
        }

        public bool HandlesPath(string virtualPath)
        {
            if (string.IsNullOrEmpty(_virtualPrefix))
            {
                return true;
            }
            return virtualPath.StartsWith(_virtualPrefix, StringComparison.OrdinalIgnoreCase);
        }

        public IVirtualFile? GetFile(string virtualPath)
        {
            if (!HandlesPath(virtualPath))
            {
                return null;
            }

            string relativePath = virtualPath;
            if (!string.IsNullOrEmpty(_virtualPrefix))
            {
                relativePath = virtualPath.Substring(_virtualPrefix.Length).TrimStart('/');
            }

            var physicalPath = System.IO.Path.Combine(_physicalPath, relativePath.Replace("/", "\\"));
            return new VirtualFile(virtualPath, physicalPath, this);
        }

        public IEnumerable<IVirtualFile> GetFiles(string searchPattern)
        {
            if (!Directory.Exists(_physicalPath))
            {
                yield break;
            }

            var files = Directory.GetFiles(_physicalPath, searchPattern, SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var relativePath = System.IO.Path.GetRelativePath(_physicalPath, file);
                var virtualPath = string.IsNullOrEmpty(_virtualPrefix)
                    ? relativePath.Replace("\\", "/")
                    : $"{_virtualPrefix}/{relativePath.Replace("\\", "/")}";

                yield return new VirtualFile(virtualPath, file, this);
            }
        }
    }

    public class VirtualFilesystem
    {
        private static VirtualFilesystem? _instance;
        private readonly List<MountPoint> _mountPoints;
        private readonly ReaderWriterLockSlim _lock;

        public static VirtualFilesystem Instance => _instance ??= new VirtualFilesystem();

        public event EventHandler<IVirtualFile>? FileCreated;
        public event EventHandler<IVirtualFile>? FileDeleted;
        public event EventHandler<IVirtualFile>? FileModified;

        private VirtualFilesystem()
        {
            _mountPoints = new List<MountPoint>();
            _lock = new ReaderWriterLockSlim();
        }

        public void Mount(IMountPoint mountPoint)
        {
            _lock.EnterWriteLock();
            try
            {
                _mountPoints.Add((MountPoint)mountPoint);
                _mountPoints.Sort((a, b) => b.Priority.CompareTo(a.Priority));
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void Unmount(string name)
        {
            _lock.EnterWriteLock();
            try
            {
                _mountPoints.RemoveAll(m => m.Name == name);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IVirtualFile? GetFile(string virtualPath)
        {
            _lock.EnterReadLock();
            try
            {
                foreach (var mountPoint in _mountPoints)
                {
                    var file = mountPoint.GetFile(virtualPath);
                    if (file != null && file.Exists)
                    {
                        return file;
                    }
                }
                return null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<IVirtualFile> GetFiles(string searchPattern)
        {
            _lock.EnterReadLock();
            try
            {
                var seen = new HashSet<string>();
                var files = new List<IVirtualFile>();

                foreach (var mountPoint in _mountPoints)
                {
                    foreach (var file in mountPoint.GetFiles(searchPattern))
                    {
                        if (!seen.Contains(file.Path))
                        {
                            seen.Add(file.Path);
                            files.Add(file);
                        }
                    }
                }

                return files;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public IEnumerable<string> GetAllPaths()
        {
            _lock.EnterReadLock();
            try
            {
                var paths = new List<string>();
                foreach (var mountPoint in _mountPoints)
                {
                    foreach (var file in mountPoint.GetFiles("*"))
                    {
                        paths.Add(file.Path);
                    }
                }
                return paths.Distinct();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Stream OpenRead(string virtualPath)
        {
            var file = GetFile(virtualPath);
            if (file == null)
            {
                throw new FileNotFoundException($"Virtual file not found: {virtualPath}");
            }
            return file.OpenRead();
        }

        public Stream OpenWrite(string virtualPath)
        {
            var file = GetFile(virtualPath);
            if (file != null)
            {
                FileDeleted?.Invoke(this, file);
                var stream = file.OpenWrite();
                FileCreated?.Invoke(this, file);
                return stream;
            }

            foreach (var mountPoint in _mountPoints)
            {
                if (mountPoint.HandlesPath(virtualPath))
                {
                    var newFile = mountPoint.GetFile(virtualPath);
                    if (newFile != null)
                    {
                        FileCreated?.Invoke(this, newFile);
                        return newFile.OpenWrite();
                    }
                }
            }

            throw new InvalidOperationException($"No mount point handles path: {virtualPath}");
        }

        public bool Exists(string virtualPath)
        {
            return GetFile(virtualPath) != null;
        }

        public void Delete(string virtualPath)
        {
            var file = GetFile(virtualPath);
            if (file != null)
            {
                file.Delete();
                FileDeleted?.Invoke(this, file);
            }
        }
    }

    public class OverlapResolution
    {
        private readonly Dictionary<string, List<OverlapEntry>> _overlaps;
        private readonly ReaderWriterLockSlim _lock;

        public OverlapResolution()
        {
            _overlaps = new Dictionary<string, List<OverlapEntry>>();
            _lock = new ReaderWriterLockSlim();
        }

        public void RegisterOverlap(string virtualPath, string physicalPath, string mountPointName)
        {
            _lock.EnterWriteLock();
            try
            {
                if (!_overlaps.ContainsKey(virtualPath))
                {
                    _overlaps[virtualPath] = new List<OverlapEntry>();
                }
                _overlaps[virtualPath].Add(new OverlapEntry
                {
                    VirtualPath = virtualPath,
                    PhysicalPath = physicalPath,
                    MountPointName = mountPointName
                });
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IEnumerable<OverlapEntry> GetOverlaps(string virtualPath)
        {
            _lock.EnterReadLock();
            try
            {
                return _overlaps.TryGetValue(virtualPath, out var entries) ? entries.ToList() : Enumerable.Empty<OverlapEntry>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public OverlapEntry? ResolveWithPriority(string virtualPath)
        {
            var overlaps = GetOverlaps(virtualPath).OrderByDescending(o => o.Priority).ToList();
            return overlaps.FirstOrDefault();
        }

        public OverlapEntry? ResolveWithMountPoint(string virtualPath, string preferredMountPoint)
        {
            var overlaps = GetOverlaps(virtualPath);
            return overlaps.FirstOrDefault(o => o.MountPointName == preferredMountPoint)
                ?? overlaps.FirstOrDefault();
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _overlaps.Clear();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public class OverlapEntry
    {
        public string VirtualPath { get; set; } = string.Empty;
        public string PhysicalPath { get; set; } = string.Empty;
        public string MountPointName { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    #endregion
}

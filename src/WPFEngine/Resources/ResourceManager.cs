using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace WPFEngine.Resources
{
    public enum ResourceState
    {
        Unloaded,
        Loading,
        Loaded,
        Failed
    }

    public interface IResource
    {
        string Path { get; }
        ResourceState State { get; }
        T? GetData<T>() where T : class;
        void Unload();
    }

    public class Resource : IResource
    {
        private readonly string _path;
        private readonly Func<string, object> _loader;
        private readonly Action<object> _unloader;
        private object? _data;
        private ResourceState _state;
        private int _refCount;
        private readonly object _lock = new object();
        private string? _error;

        public string Path => _path;
        public ResourceState State => _state;
        public string? Error => _error;
        public int RefCount => _refCount;

        public Resource(string path, Func<string, object> loader, Action<object>? unloader = null)
        {
            _path = path;
            _loader = loader;
            _unloader = unloader ?? (_ => { });
            _state = ResourceState.Unloaded;
            _refCount = 0;
            _data = null;
            _error = null;
        }

        public async Task<bool> LoadAsync(CancellationToken cancellationToken = default)
        {
            lock (_lock)
            {
                if (_state == ResourceState.Loaded || _state == ResourceState.Loading)
                    return _state == ResourceState.Loaded;
                _state = ResourceState.Loading;
            }

            try
            {
                _data = await Task.Run(() => _loader(_path), cancellationToken);
                lock (_lock)
                {
                    _state = ResourceState.Loaded;
                    _error = null;
                }
                return true;
            }
            catch (Exception ex)
            {
                lock (_lock)
                {
                    _state = ResourceState.Failed;
                    _error = ex.Message;
                }
                return false;
            }
        }

        public void Load()
        {
            LoadAsync().Wait();
        }

        public T? GetData<T>() where T : class
        {
            if (_state != ResourceState.Loaded)
                return null;
            return _data as T;
        }

        public void AddRef()
        {
            Interlocked.Increment(ref _refCount);
        }

        public void Release()
        {
            if (Interlocked.Decrement(ref _refCount) <= 0)
            {
                Unload();
            }
        }

        public void Unload()
        {
            lock (_lock)
            {
                if (_state == ResourceState.Unloaded)
                    return;

                if (_data != null)
                {
                    _unloader(_data);
                    _data = null;
                }

                _state = ResourceState.Unloaded;
                _error = null;
            }
        }
    }

    public class ResourceManager
    {
        private static ResourceManager? _instance;
        private readonly Dictionary<string, Resource> _resources;
        private readonly Dictionary<string, List<string>> _bundles;
        private readonly Dictionary<Type, IResourceLoader> _loaders;
        private readonly object _lock = new object();
        private CancellationTokenSource? _globalCts;
        private Task? _preloadTask;
        private int _maxConcurrentLoads;
        private SemaphoreSlim? _loadSemaphore;

        public static ResourceManager Instance => _instance ??= new ResourceManager();

        public int MaxConcurrentLoads
        {
            get => _maxConcurrentLoads;
            set
            {
                _maxConcurrentLoads = Math.Max(1, value);
                _loadSemaphore?.Dispose();
                _loadSemaphore = new SemaphoreSlim(_maxConcurrentLoads);
            }
        }

        public int LoadedResourceCount
        {
            get
            {
                lock (_lock)
                {
                    return _resources.Values.Count(r => r.State == ResourceState.Loaded);
                }
            }
        }

        public int TotalResourceCount
        {
            get
            {
                lock (_lock)
                {
                    return _resources.Count;
                }
            }
        }

        public int LoadingResourceCount
        {
            get
            {
                lock (_lock)
                {
                    return _resources.Values.Count(r => r.State == ResourceState.Loading);
                }
            }
        }

        public event EventHandler<string>? ResourceLoaded;
        public event EventHandler<string>? ResourceUnloaded;
        public event EventHandler<(string Path, string Error)>? ResourceFailed;
        public event EventHandler<float>? LoadingProgressChanged;

        private ResourceManager()
        {
            _resources = new Dictionary<string, Resource>();
            _bundles = new Dictionary<string, List<string>>();
            _loaders = new Dictionary<Type, IResourceLoader>();
            _maxConcurrentLoads = 4;
            _loadSemaphore = new SemaphoreSlim(_maxConcurrentLoads);
            _globalCts = new CancellationTokenSource();

            RegisterDefaultLoaders();
        }

        private void RegisterDefaultLoaders()
        {
            RegisterLoader(new TextureLoader());
            RegisterLoader(new AudioClipLoader());
            RegisterLoader(new TextAssetLoader());
            RegisterLoader(new BinaryAssetLoader());
        }

        public void RegisterLoader<T>(IResourceLoader<T> loader) where T : class
        {
            lock (_lock)
            {
                _loaders[typeof(T)] = loader;
            }
        }

        public T? Load<T>(string path) where T : class
        {
            var resource = GetOrCreateResource(path, typeof(T));
            if (resource == null) return null;

            if (resource.State != ResourceState.Loaded)
            {
                resource.Load();
            }

            return resource.GetData<T>();
        }

        public async Task<T?> LoadAsync<T>(string path, IProgress<float>? progress = null, CancellationToken cancellationToken = default) where T : class
        {
            var resource = GetOrCreateResource(path, typeof(T));
            if (resource == null) return null;

            if (resource.State != ResourceState.Loaded)
            {
                await LoadResourceAsync(resource, progress, cancellationToken);
            }

            return resource.GetData<T>();
        }

        private async Task LoadResourceAsync(Resource resource, IProgress<float>? progress, CancellationToken cancellationToken)
        {
            await _loadSemaphore!.WaitAsync(cancellationToken);
            try
            {
                await resource.LoadAsync(cancellationToken);

                if (resource.State == ResourceState.Loaded)
                {
                    ResourceLoaded?.Invoke(this, resource.Path);
                }
                else if (resource.State == ResourceState.Failed)
                {
                    ResourceFailed?.Invoke(this, (resource.Path, resource.Error ?? "Unknown error"));
                }

                UpdateLoadingProgress();
            }
            finally
            {
                _loadSemaphore.Release();
            }
        }

        public async Task LoadAllAsync(IProgress<float>? progress = null, CancellationToken cancellationToken = default)
        {
            List<Resource> unloadedResources;
            lock (_lock)
            {
                unloadedResources = _resources.Values
                    .Where(r => r.State != ResourceState.Loaded)
                    .ToList();
            }

            int total = unloadedResources.Count;
            int loaded = 0;

            var tasks = unloadedResources.Select(async r =>
            {
                await LoadResourceAsync(r, null, cancellationToken);
                int current = Interlocked.Increment(ref loaded);
                progress?.Report((float)current / total);
            });

            await Task.WhenAll(tasks);
        }

        public void UnloadUnused()
        {
            List<string> keysToRemove;
            lock (_lock)
            {
                keysToRemove = _resources
                    .Where(kvp => kvp.Value.RefCount <= 0 && kvp.Value.State == ResourceState.Loaded)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _resources[key].Unload();
                }
            }

            foreach (var key in keysToRemove)
            {
                ResourceUnloaded?.Invoke(this, key);
            }
        }

        public void UnloadAll()
        {
            lock (_lock)
            {
                foreach (var resource in _resources.Values)
                {
                    resource.Unload();
                }
            }

            _resources.Clear();
        }

        private Resource? GetOrCreateResource(string path, Type type)
        {
            lock (_lock)
            {
                if (_resources.TryGetValue(path, out var existing))
                {
                    return existing;
                }

                if (!_loaders.TryGetValue(type, out var loader))
                {
                    return null;
                }

                var resource = new Resource(
                    path,
                    p => loader.Load(p),
                    d => loader.Unload(d)
                );

                _resources[path] = resource;
                return resource;
            }
        }

        public bool HasResource(string path)
        {
            lock (_lock)
            {
                return _resources.ContainsKey(path);
            }
        }

        public ResourceState GetResourceState(string path)
        {
            lock (_lock)
            {
                return _resources.TryGetValue(path, out var resource) ? resource.State : ResourceState.Unloaded;
            }
        }

        public void CreateBundle(string bundleName, IEnumerable<string> paths)
        {
            lock (_lock)
            {
                _bundles[bundleName] = paths.ToList();
            }
        }

        public async Task LoadBundleAsync(string bundleName, IProgress<float>? progress = null, CancellationToken cancellationToken = default)
        {
            List<string> paths;
            lock (_lock)
            {
                if (!_bundles.TryGetValue(bundleName, out paths))
                    return;
            }

            int total = paths.Count;
            int loaded = 0;

            var tasks = paths.Select(async path =>
            {
                await LoadAsync<object>(path, null, cancellationToken);
                int current = Interlocked.Increment(ref loaded);
                progress?.Report((float)current / total);
            });

            await Task.WhenAll(tasks);
        }

        public void UnloadBundle(string bundleName)
        {
            List<string> paths;
            lock (_lock)
            {
                if (!_bundles.TryGetValue(bundleName, out paths))
                    return;
            }

            foreach (var path in paths)
            {
                if (_resources.TryGetValue(path, out var resource))
                {
                    resource.Release();
                }
            }

            lock (_lock)
            {
                _bundles.Remove(bundleName);
            }
        }

        private void UpdateLoadingProgress()
        {
            int loaded = LoadedResourceCount;
            int total = TotalResourceCount;
            if (total > 0)
            {
                LoadingProgressChanged?.Invoke(this, (float)loaded / total);
            }
        }

        public Dictionary<string, float> GetMemoryUsage()
        {
            var usage = new Dictionary<string, float>();
            lock (_lock)
            {
                foreach (var kvp in _resources)
                {
                    if (kvp.Value.State == ResourceState.Loaded && kvp.Value.GetData<object>() != null)
                    {
                        var data = kvp.Value.GetData<object>();
                        string type = data?.GetType().Name ?? "Unknown";
                        if (!usage.ContainsKey(type))
                            usage[type] = 0;

                        if (data is byte[] bytes)
                            usage[type] += bytes.Length / (1024f * 1024f);
                        else if (data is Array arr)
                            usage[type] += arr.Length * 4 / (1024f * 1024f);
                    }
                }
            }
            return usage;
        }
    }

    public interface IResourceLoader
    {
        object Load(string path);
        void Unload(object data);
    }

    public interface IResourceLoader<T> : IResourceLoader where T : class
    {
        new T Load(string path);
    }

    public class TextureLoader : IResourceLoader<System.Windows.Media.Imaging.BitmapSource>
    {
        public System.Windows.Media.Imaging.BitmapSource Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Texture not found: {path}");

            var bitmap = new System.Windows.Media.Imaging.BitmapImage();
            bitmap.BeginInit();
            bitmap.UriSource = new Uri(path, UriKind.Absolute);
            bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
            bitmap.EndInit();
            bitmap.Freeze();
            return bitmap;
        }

        public void Unload(object data)
        {
        }

        object IResourceLoader.Load(string path) => Load(path);
    }

    public class AudioClipLoader : IResourceLoader<NAudio.Wave.WaveOut>
    {
        public NAudio.Wave.WaveOut Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Audio file not found: {path}");

            var audioFile = new NAudio.Wave.AudioFileReader(path);
            var waveOut = new NAudio.Wave.WaveOut();
            waveOut.Init(audioFile);
            return waveOut;
        }

        public void Unload(object data)
        {
            if (data is NAudio.Wave.WaveOut waveOut)
            {
                waveOut.Stop();
                waveOut.Dispose();
            }
        }

        object IResourceLoader.Load(string path) => Load(path);
    }

    public class TextAssetLoader : IResourceLoader<string>
    {
        public string Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Text file not found: {path}");

            return File.ReadAllText(path);
        }

        public void Unload(object data)
        {
        }

        object IResourceLoader.Load(string path) => Load(path);
    }

    public class BinaryAssetLoader : IResourceLoader<byte[]>
    {
        public byte[] Load(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"Binary file not found: {path}");

            return File.ReadAllBytes(path);
        }

        public void Unload(object data)
        {
        }

        object IResourceLoader.Load(string path) => Load(path);
    }
}

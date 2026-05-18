using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Threading;
using WPFEngine.ECS;

namespace WPFEngine.Audio
{
    public enum AudioSourceType
    {
        SoundEffect,
        Music,
        Voice,
        Ambient,
        UI
    }

    public enum AudioState
    {
        Stopped,
        Playing,
        Paused,
        FadingIn,
        FadingOut
    }

    public enum AudioOutputDevice
    {
        Default,
        Speaker,
        Headphones,
        Surround,
        HDMI
    }

    public enum AudioFormat
    {
        Unknown,
        WAV,
        MP3,
        OGG,
        FLAC,
        AIFF
    }

    public class AudioManager : IDisposable
    {
        private static AudioManager instance;
        private Dictionary<string, AudioClip> loadedClips;
        private Dictionary<string, List<AudioSource>> activeSources;
        private Dictionary<string, AudioSource> musicSources;
        private Dictionary<string, AudioSource> sfxSources;
        private Dictionary<int, AudioSource> audioSourcePool;
        private float masterVolume;
        private float musicVolume;
        private float sfxVolume;
        private float voiceVolume;
        private float ambientVolume;
        private float uiVolume;
        private bool isMuted;
        private bool musicMuted;
        private bool sfxMuted;
        private bool voiceMuted;
        private bool ambientMuted;
        private bool uiMuted;
        private AudioOutputDevice outputDevice;
        private bool isInitialized;
        private int poolSize;
        private int maxSimultaneousSounds;
        private AudioSource prioritySource;
        private AudioSource currentMusic;
        private AudioSource previousMusic;
        private float crossfadeDuration;
        private float defaultMusicFadeTime;
        private float defaultSfxFadeTime;
        private List<AudioMixer> mixers;
        private Dictionary<string, AudioEffect> effects;
        private bool enableAudioEffects;
        private bool enableReverb;
        private bool enableCompressor;
        private float dopplerFactor;
        private float speedOfSound;
        private AudioRolloffMode rolloffMode;
        private float minDistance;
        private float maxDistance;
        private float panLevel;
        private Dictionary<string, AudioCategory> categories;
        private Dictionary<string, AudioBank> audioBanks;
        private bool preloadAudio;
        private int preloadThreads;
        private DispatcherTimer updateTimer;
        private List<AudioSource> toRemove;
        private bool isPaused;
        private bool isSuspended;

        public event EventHandler<AudioEventArgs> AudioLoaded;
        public event EventHandler<AudioEventArgs> AudioUnloaded;
        public event EventHandler<AudioEventArgs> MusicStarted;
        public event EventHandler<AudioEventArgs> MusicStopped;
        public event EventHandler<AudioEventArgs> SoundPlayed;
        public event EventHandler<AudioEventArgs> SoundCompleted;
        public event EventHandler<AudioEventArgs> VolumeChanged;
        public event EventHandler<AudioEventArgs> AudioSourceCreated;
        public event EventHandler<AudioEventArgs> AudioSourceDestroyed;

        public static AudioManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new AudioManager();
                return instance;
            }
        }

        public float MasterVolume
        {
            get => masterVolume;
            set
            {
                masterVolume = Math.Max(0f, Math.Min(1f, value));
                UpdateAllVolumes();
                VolumeChanged?.Invoke(this, new AudioEventArgs("MasterVolume"));
            }
        }

        public float MusicVolume
        {
            get => musicVolume;
            set
            {
                musicVolume = Math.Max(0f, Math.Min(1f, value));
                UpdateMusicVolumes();
                VolumeChanged?.Invoke(this, new AudioEventArgs("MusicVolume"));
            }
        }

        public float SfxVolume
        {
            get => sfxVolume;
            set
            {
                sfxVolume = Math.Max(0f, Math.Min(1f, value));
                UpdateSfxVolumes();
                VolumeChanged?.Invoke(this, new AudioEventArgs("SfxVolume"));
            }
        }

        public float VoiceVolume
        {
            get => voiceVolume;
            set
            {
                voiceVolume = Math.Max(0f, Math.Min(1f, value));
                UpdateVoiceVolumes();
                VolumeChanged?.Invoke(this, new AudioEventArgs("VoiceVolume"));
            }
        }

        public float AmbientVolume
        {
            get => ambientVolume;
            set
            {
                ambientVolume = Math.Max(0f, Math.Min(1f, value));
                UpdateAmbientVolumes();
                VolumeChanged?.Invoke(this, new AudioEventArgs("AmbientVolume"));
            }
        }

        public float UIVolume
        {
            get => uiVolume;
            set
            {
                uiVolume = Math.Max(0f, Math.Min(1f, value));
                UpdateUIVolumes();
                VolumeChanged?.Invoke(this, new AudioEventArgs("UIVolume"));
            }
        }

        public bool IsMuted
        {
            get => isMuted;
            set
            {
                isMuted = value;
                UpdateAllVolumes();
            }
        }

        public bool MusicMuted
        {
            get => musicMuted;
            set
            {
                musicMuted = value;
                UpdateMusicVolumes();
            }
        }

        public bool SfxMuted
        {
            get => sfxMuted;
            set
            {
                sfxMuted = value;
                UpdateSfxVolumes();
            }
        }

        public bool VoiceMuted
        {
            get => voiceMuted;
            set
            {
                voiceMuted = value;
                UpdateVoiceVolumes();
            }
        }

        public bool AmbientMuted
        {
            get => ambientMuted;
            set
            {
                ambientMuted = value;
                UpdateAmbientVolumes();
            }
        }

        public bool UIMuted
        {
            get => uiMuted;
            set
            {
                uiMuted = value;
                UpdateUIVolumes();
            }
        }

        public AudioOutputDevice OutputDevice
        {
            get => outputDevice;
            set => outputDevice = value;
        }

        public bool IsInitialized => isInitialized;
        public int PoolSize => poolSize;
        public AudioSource CurrentMusic => currentMusic;
        public AudioSource PreviousMusic => previousMusic;
        public float CrossfadeDuration => crossfadeDuration;
        public bool EnableAudioEffects => enableAudioEffects;

        public AudioManager()
        {
            instance = this;
            loadedClips = new Dictionary<string, AudioClip>();
            activeSources = new Dictionary<string, List<AudioSource>>();
            musicSources = new Dictionary<string, AudioSource>();
            sfxSources = new Dictionary<string, List<AudioSource>>();
            audioSourcePool = new Dictionary<int, AudioSource>();
            masterVolume = 1f;
            musicVolume = 0.7f;
            sfxVolume = 1f;
            voiceVolume = 1f;
            ambientVolume = 0.5f;
            uiVolume = 0.8f;
            isMuted = false;
            musicMuted = false;
            sfxMuted = false;
            voiceMuted = false;
            ambientMuted = false;
            uiMuted = false;
            outputDevice = AudioOutputDevice.Default;
            isInitialized = false;
            poolSize = 32;
            maxSimultaneousSounds = 16;
            crossfadeDuration = 2f;
            defaultMusicFadeTime = 1f;
            defaultSfxFadeTime = 0.1f;
            mixers = new List<AudioMixer>();
            effects = new Dictionary<string, AudioEffect>();
            enableAudioEffects = true;
            enableReverb = false;
            enableCompressor = false;
            dopplerFactor = 1f;
            speedOfSound = 343f;
            rolloffMode = AudioRolloffMode.Inverse;
            minDistance = 1f;
            maxDistance = 500f;
            panLevel = 1f;
            categories = new Dictionary<string, AudioCategory>();
            audioBanks = new Dictionary<string, AudioBank>();
            preloadAudio = false;
            preloadThreads = 2;
            toRemove = new List<AudioSource>();
            isPaused = false;
            isSuspended = false;
        }

        public void Initialize()
        {
            if (isInitialized)
                return;

            InitializeAudioPool();
            InitializeCategories();
            InitializeMixers();
            StartUpdateTimer();

            isInitialized = true;
        }

        private void InitializeAudioPool()
        {
            for (int i = 0; i < poolSize; i++)
            {
                var source = new AudioSource();
                source.SourceId = i;
                audioSourcePool[i] = source;
            }
        }

        private void InitializeCategories()
        {
            categories["Master"] = new AudioCategory { Name = "Master", Volume = masterVolume };
            categories["Music"] = new AudioCategory { Name = "Music", Volume = musicVolume };
            categories["SFX"] = new AudioCategory { Name = "SFX", Volume = sfxVolume };
            categories["Voice"] = new AudioCategory { Name = "Voice", Volume = voiceVolume };
            categories["Ambient"] = new AudioCategory { Name = "Ambient", Volume = ambientVolume };
            categories["UI"] = new AudioCategory { Name = "UI", Volume = uiVolume };
        }

        private void InitializeMixers()
        {
            var masterMixer = new AudioMixer { Name = "Master", OutputLevel = masterVolume };
            var musicMixer = new AudioMixer { Name = "Music", OutputLevel = musicVolume };
            var sfxMixer = new AudioMixer { Name = "SFX", OutputLevel = sfxVolume };

            mixers.Add(masterMixer);
            mixers.Add(musicMixer);
            mixers.Add(sfxMixer);
        }

        private void StartUpdateTimer()
        {
            updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16)
            };
            updateTimer.Tick += OnUpdateTimerTick;
            updateTimer.Start();
        }

        private void OnUpdateTimerTick(object sender, EventArgs e)
        {
            Update();
        }

        public void Update()
        {
            if (isPaused || isSuspended)
                return;

            toRemove.Clear();

            foreach (var kvp in activeSources)
            {
                var sources = kvp.Value;
                for (int i = sources.Count - 1; i >= 0; i--)
                {
                    var source = sources[i];
                    source.Update();

                    if (source.State == AudioState.Stopped && !source.Loop)
                    {
                        SoundCompleted?.Invoke(this, new AudioEventArgs(source.ClipName));
                        toRemove.Add(source);
                    }
                }

                foreach (var source in toRemove)
                {
                    sources.Remove(source);
                }
            }

            foreach (var source in audioSourcePool.Values)
            {
                source.Update();
            }
        }

        public AudioClip LoadClip(string path)
        {
            return LoadClip(path, Path.GetFileNameWithoutExtension(path));
        }

        public AudioClip LoadClip(string path, string key)
        {
            if (loadedClips.ContainsKey(key))
                return loadedClips[key];

            try
            {
                var clip = new AudioClip(path);
                clip.Name = key;
                loadedClips[key] = clip;
                AudioLoaded?.Invoke(this, new AudioEventArgs(key));
                return clip;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load audio clip: {ex.Message}");
                return null;
            }
        }

        public AudioClip LoadClipFromResource(string resourcePath)
        {
            return LoadClipFromResource(resourcePath, Path.GetFileNameWithoutExtension(resourcePath));
        }

        public AudioClip LoadClipFromResource(string resourcePath, string key)
        {
            if (loadedClips.ContainsKey(key))
                return loadedClips[key];

            try
            {
                var uri = new Uri(resourcePath, UriKind.Relative);
                var streamInfo = System.Windows.Application.GetResourceStream(uri);
                if (streamInfo == null)
                    return null;

                var clip = new AudioClip(streamInfo.Stream);
                clip.Name = key;
                loadedClips[key] = clip;
                AudioLoaded?.Invoke(this, new AudioEventArgs(key));
                return clip;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load audio clip from resource: {ex.Message}");
                return null;
            }
        }

        public void UnloadClip(string key)
        {
            if (loadedClips.TryGetValue(key, out var clip))
            {
                clip.Dispose();
                loadedClips.Remove(key);
                AudioUnloaded?.Invoke(this, new AudioEventArgs(key));
            }
        }

        public void UnloadAllClips()
        {
            foreach (var clip in loadedClips.Values)
            {
                clip.Dispose();
            }
            loadedClips.Clear();
        }

        public AudioClip GetClip(string key)
        {
            return loadedClips.TryGetValue(key, out var clip) ? clip : null;
        }

        public AudioSource PlaySound(string clipKey)
        {
            return PlaySound(clipKey, 1f, 1f, false);
        }

        public AudioSource PlaySound(string clipKey, float volume)
        {
            return PlaySound(clipKey, volume, 1f, false);
        }

        public AudioSource PlaySound(string clipKey, float volume, float pitch)
        {
            return PlaySound(clipKey, volume, pitch, false);
        }

        public AudioSource PlaySound(string clipKey, float volume, float pitch, bool loop)
        {
            var clip = GetClip(clipKey);
            if (clip == null)
            {
                clip = LoadClip(clipKey);
                if (clip == null)
                    return null;
            }

            var source = GetSourceFromPool();
            if (source == null)
                return null;

            source.Clip = clip;
            source.Volume = volume;
            source.Pitch = pitch;
            source.Loop = loop;
            source.SourceType = AudioSourceType.SoundEffect;
            source.Play();

            if (!sfxSources.ContainsKey(clipKey))
                sfxSources[clipKey] = new List<AudioSource>();
            sfxSources[clipKey].Add(source);

            SoundPlayed?.Invoke(this, new AudioEventArgs(clipKey));
            return source;
        }

        public AudioSource PlaySound(AudioClip clip, float volume = 1f, float pitch = 1f, bool loop = false)
        {
            if (clip == null)
                return null;

            var source = GetSourceFromPool();
            if (source == null)
                return null;

            source.Clip = clip;
            source.Volume = volume;
            source.Pitch = pitch;
            source.Loop = loop;
            source.SourceType = AudioSourceType.SoundEffect;
            source.Play();

            SoundPlayed?.Invoke(this, new AudioEventArgs(clip.Name));
            return source;
        }

        public AudioSource PlayMusic(string clipKey)
        {
            return PlayMusic(clipKey, 1f, true);
        }

        public AudioSource PlayMusic(string clipKey, float volume)
        {
            return PlayMusic(clipKey, volume, true);
        }

        public AudioSource PlayMusic(string clipKey, float volume, bool loop)
        {
            var clip = GetClip(clipKey);
            if (clip == null)
            {
                clip = LoadClip(clipKey);
                if (clip == null)
                    return null;
            }

            if (currentMusic != null && currentMusic.State == AudioState.Playing)
            {
                previousMusic = currentMusic;
                previousMusic.FadeOut(defaultMusicFadeTime);
            }

            var source = GetSourceFromPool();
            if (source == null)
                return null;

            source.Clip = clip;
            source.Volume = volume;
            source.Pitch = 1f;
            source.Loop = loop;
            source.SourceType = AudioSourceType.Music;
            source.Play();

            currentMusic = source;
            musicSources[clipKey] = source;

            MusicStarted?.Invoke(this, new AudioEventArgs(clipKey));
            return source;
        }

        public AudioSource PlayMusic(AudioClip clip, float volume = 1f, bool loop = true)
        {
            if (clip == null)
                return null;

            if (currentMusic != null)
            {
                previousMusic = currentMusic;
                previousMusic.FadeOut(defaultMusicFadeTime);
            }

            var source = GetSourceFromPool();
            if (source == null)
                return null;

            source.Clip = clip;
            source.Volume = volume;
            source.Pitch = 1f;
            source.Loop = loop;
            source.SourceType = AudioSourceType.Music;
            source.Play();

            currentMusic = source;
            MusicStarted?.Invoke(this, new AudioEventArgs(clip.Name));
            return source;
        }

        public void StopMusic()
        {
            if (currentMusic != null)
            {
                currentMusic.Stop();
                MusicStopped?.Invoke(this, new AudioEventArgs(currentMusic.ClipName));
                currentMusic = null;
            }
        }

        public void StopMusic(float fadeTime)
        {
            if (currentMusic != null)
            {
                currentMusic.FadeOut(fadeTime, () =>
                {
                    MusicStopped?.Invoke(this, new AudioEventArgs(currentMusic.ClipName));
                    currentMusic = null;
                });
            }
        }

        public void PauseMusic()
        {
            if (currentMusic != null)
            {
                currentMusic.Pause();
            }
        }

        public void ResumeMusic()
        {
            if (currentMusic != null)
            {
                currentMusic.Resume();
            }
        }

        public void CrossfadeMusic(string newClipKey, float fadeTime = -1f)
        {
            if (fadeTime < 0)
                fadeTime = crossfadeDuration;

            var newClip = GetClip(newClipKey);
            if (newClip == null)
            {
                newClip = LoadClip(newClipKey);
                if (newClip == null)
                    return;
            }

            if (currentMusic != null)
            {
                previousMusic = currentMusic;
                previousMusic.FadeOut(fadeTime);
            }

            var source = GetSourceFromPool();
            if (source == null)
                return;

            source.Clip = newClip;
            source.Volume = musicVolume;
            source.Pitch = 1f;
            source.Loop = true;
            source.SourceType = AudioSourceType.Music;
            source.FadeIn(fadeTime);
            source.Play();

            currentMusic = source;
            musicSources[newClipKey] = source;
            MusicStarted?.Invoke(this, new AudioEventArgs(newClipKey));
        }

        public AudioSource PlayAmbient(string clipKey, float volume = 1f)
        {
            var clip = GetClip(clipKey);
            if (clip == null)
            {
                clip = LoadClip(clipKey);
                if (clip == null)
                    return null;
            }

            var source = GetSourceFromPool();
            if (source == null)
                return null;

            source.Clip = clip;
            source.Volume = volume;
            source.Pitch = 1f;
            source.Loop = true;
            source.SourceType = AudioSourceType.Ambient;
            source.Play();

            return source;
        }

        public AudioSource PlayVoice(string clipKey, float volume = 1f)
        {
            var clip = GetClip(clipKey);
            if (clip == null)
            {
                clip = LoadClip(clipKey);
                if (clip == null)
                    return null;
            }

            var source = GetSourceFromPool();
            if (source == null)
                return null;

            source.Clip = clip;
            source.Volume = volume;
            source.Pitch = 1f;
            source.Loop = false;
            source.SourceType = AudioSourceType.Voice;
            source.Play();

            return source;
        }

        public AudioSource PlayUI(string clipKey, float volume = 1f)
        {
            var clip = GetClip(clipKey);
            if (clip == null)
            {
                clip = LoadClip(clipKey);
                if (clip == null)
                    return null;
            }

            var source = GetSourceFromPool();
            if (source == null)
                return null;

            source.Clip = clip;
            source.Volume = volume;
            source.Pitch = 1f;
            source.Loop = false;
            source.SourceType = AudioSourceType.UI;
            source.Play();

            return source;
        }

        public void StopAllSounds()
        {
            foreach (var sources in activeSources.Values)
            {
                foreach (var source in sources)
                {
                    source.Stop();
                }
            }

            foreach (var source in audioSourcePool.Values)
            {
                source.Stop();
            }
        }

        public void PauseAllSounds()
        {
            isPaused = true;
            foreach (var sources in activeSources.Values)
            {
                foreach (var source in sources)
                {
                    source.Pause();
                }
            }
        }

        public void ResumeAllSounds()
        {
            isPaused = false;
            foreach (var sources in activeSources.Values)
            {
                foreach (var source in sources)
                {
                    source.Resume();
                }
            }
        }

        public void MuteAll()
        {
            IsMuted = true;
        }

        public void UnmuteAll()
        {
            IsMuted = false;
        }

        private AudioSource GetSourceFromPool()
        {
            foreach (var source in audioSourcePool.Values)
            {
                if (!source.IsPlaying && source.State != AudioState.Playing)
                {
                    return source;
                }
            }

            if (audioSourcePool.Count < poolSize * 2)
            {
                var newId = audioSourcePool.Count;
                var newSource = new AudioSource { SourceId = newId };
                audioSourcePool[newId] = newSource;
                AudioSourceCreated?.Invoke(this, new AudioEventArgs($"Source_{newId}"));
                return newSource;
            }

            return null;
        }

        private void UpdateAllVolumes()
        {
            var effectiveVolume = isMuted ? 0f : masterVolume;

            foreach (var sources in activeSources.Values)
            {
                foreach (var source in sources)
                {
                    source.UpdateVolume(effectiveVolume);
                }
            }

            foreach (var source in audioSourcePool.Values)
            {
                source.UpdateVolume(effectiveVolume);
            }
        }

        private void UpdateMusicVolumes()
        {
            var effectiveVolume = (isMuted || musicMuted) ? 0f : masterVolume * musicVolume;

            foreach (var source in musicSources.Values)
            {
                source.UpdateVolume(effectiveVolume);
            }

            if (currentMusic != null)
            {
                currentMusic.UpdateVolume(effectiveVolume);
            }
        }

        private void UpdateSfxVolumes()
        {
            var effectiveVolume = (isMuted || sfxMuted) ? 0f : masterVolume * sfxVolume;

            foreach (var sources in sfxSources.Values)
            {
                foreach (var source in sources)
                {
                    source.UpdateVolume(effectiveVolume);
                }
            }
        }

        private void UpdateVoiceVolumes()
        {
            var effectiveVolume = (isMuted || voiceMuted) ? 0f : masterVolume * voiceVolume;
        }

        private void UpdateAmbientVolumes()
        {
            var effectiveVolume = (isMuted || ambientMuted) ? 0f : masterVolume * ambientVolume;
        }

        private void UpdateUIVolumes()
        {
            var effectiveVolume = (isMuted || uiMuted) ? 0f : masterVolume * uiVolume;
        }

        public void SetCategoryVolume(string category, float volume)
        {
            if (categories.TryGetValue(category, out var cat))
            {
                cat.Volume = Math.Max(0f, Math.Min(1f, volume));
            }
        }

        public float GetCategoryVolume(string category)
        {
            return categories.TryGetValue(category, out var cat) ? cat.Volume : 1f;
        }

        public void AddAudioBank(string name, AudioBank bank)
        {
            audioBanks[name] = bank;
        }

        public AudioBank GetAudioBank(string name)
        {
            return audioBanks.TryGetValue(name, out var bank) ? bank : null;
        }

        public void Suspend()
        {
            isSuspended = true;
            foreach (var source in audioSourcePool.Values)
            {
                source.Suspend();
            }
        }

        public void Resume()
        {
            isSuspended = false;
            foreach (var source in audioSourcePool.Values)
            {
                source.Resume();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (updateTimer != null)
                {
                    updateTimer.Stop();
                    updateTimer = null;
                }

                foreach (var clip in loadedClips.Values)
                {
                    clip.Dispose();
                }
                loadedClips.Clear();

                foreach (var source in audioSourcePool.Values)
                {
                    source.Dispose();
                }
                audioSourcePool.Clear();

                activeSources.Clear();
                musicSources.Clear();
                sfxSources.Clear();
                categories.Clear();
                mixers.Clear();
                audioBanks.Clear();
            }
        }
    }

    public class AudioClip : IDisposable
    {
        public string Name { get; set; }
        public string FilePath { get; private set; }
        public AudioFormat Format { get; private set; }
        public int SampleRate { get; private set; }
        public int Channels { get; private set; }
        public int BitsPerSample { get; private set; }
        public double Duration { get; private set; }
        public long Samples { get; private set; }
        public byte[] Data { get; private set; }
        public bool IsLoaded { get; private set; }
        public float Volume { get; set; }

        public AudioClip(string path)
        {
            FilePath = path;
            LoadFromFile(path);
        }

        public AudioClip(Stream stream)
        {
            LoadFromStream(stream);
        }

        private void LoadFromFile(string path)
        {
            try
            {
                if (!File.Exists(path))
                    return;

                var extension = Path.GetExtension(path).ToLower();
                Format = extension switch
                {
                    ".wav" => AudioFormat.WAV,
                    ".mp3" => AudioFormat.MP3,
                    ".ogg" => AudioFormat.OGG,
                    ".flac" => AudioFormat.FLAC,
                    ".aiff" or ".aif" => AudioFormat.AIFF,
                    _ => AudioFormat.Unknown
                };

                Data = File.ReadAllBytes(path);
                IsLoaded = true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error loading audio file: {ex.Message}");
            }
        }

        private void LoadFromStream(Stream stream)
        {
            using (var ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                Data = ms.ToArray();
            }
            IsLoaded = true;
        }

        public override string ToString() => $"AudioClip({Name}, Duration={Duration:F2}s)";

        public void Dispose()
        {
            Data = null;
            IsLoaded = false;
        }
    }

    public class AudioSource : IDisposable
    {
        public int SourceId { get; set; }
        public AudioClip Clip { get; set; }
        public string ClipName => Clip?.Name ?? "None";
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool Loop { get; set; }
        public AudioState State { get; private set; }
        public AudioSourceType SourceType { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public float Pan { get; set; }
        public float SpatialBlend { get; set; }
        public float ReverbZoneMix { get; set; }
        public AudioRolloffMode RolloffMode { get; set; }
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public float PanLevel { get; set; }
        public int Priority { get; set; }
        public double CurrentTime { get; private set; }
        public double PlaybackProgress { get; private set; }

        private float currentVolume;
        private float targetVolume;
        private float fadeSpeed;
        private Action fadeCompleteCallback;
        private double startTime;
        private double pauseTime;

        public event EventHandler<AudioSourceEventArgs> PlaybackStarted;
        public event EventHandler<AudioSourceEventArgs> PlaybackPaused;
        public event EventHandler<AudioSourceEventArgs> PlaybackStopped;
        public event EventHandler<AudioSourceEventArgs> PlaybackCompleted;

        public AudioSource()
        {
            Volume = 1f;
            Pitch = 1f;
            Loop = false;
            State = AudioState.Stopped;
            Position = Vector2.Zero;
            Velocity = Vector2.Zero;
            Pan = 0f;
            SpatialBlend = 0f;
            ReverbZoneMix = 0f;
            RolloffMode = AudioRolloffMode.Inverse;
            MinDistance = 1f;
            MaxDistance = 500f;
            PanLevel = 1f;
            Priority = 128;
            CurrentTime = 0;
            PlaybackProgress = 0;
            currentVolume = 1f;
            targetVolume = 1f;
            fadeSpeed = 0f;
        }

        public void Play()
        {
            if (Clip == null || !Clip.IsLoaded)
                return;

            State = AudioState.Playing;
            startTime = DateTime.Now.TimeOfDay.TotalSeconds;
            targetVolume = Volume;
            currentVolume = Volume;
            PlaybackStarted?.Invoke(this, new AudioSourceEventArgs(this));
        }

        public void Play(float delay)
        {
        }

        public void Pause()
        {
            if (State == AudioState.Playing)
            {
                State = AudioState.Paused;
                pauseTime = CurrentTime;
                PlaybackPaused?.Invoke(this, new AudioSourceEventArgs(this));
            }
        }

        public void Resume()
        {
            if (State == AudioState.Paused)
            {
                State = AudioState.Playing;
                PlaybackStarted?.Invoke(this, new AudioSourceEventArgs(this));
            }
        }

        public void Stop()
        {
            State = AudioState.Stopped;
            CurrentTime = 0;
            PlaybackProgress = 0;
            PlaybackStopped?.Invoke(this, new AudioSourceEventArgs(this));
        }

        public void FadeIn(float fadeTime, Action onComplete = null)
        {
            State = AudioState.FadingIn;
            targetVolume = Volume;
            currentVolume = 0f;
            fadeSpeed = Volume / fadeTime;
            fadeCompleteCallback = onComplete;
            Play();
        }

        public void FadeOut(float fadeTime, Action onComplete = null)
        {
            if (State != AudioState.Playing && State != AudioState.FadingIn)
                return;

            State = AudioState.FadingOut;
            targetVolume = 0f;
            fadeSpeed = Volume / fadeTime;
            fadeCompleteCallback = onComplete;
        }

        public void Update()
        {
            if (State == AudioState.Stopped)
                return;

            if (State == AudioState.FadingIn || State == AudioState.FadingOut)
            {
                UpdateFade();
            }

            if (State == AudioState.Playing)
            {
                UpdatePlayback();
            }
        }

        private void UpdateFade()
        {
            if (State == AudioState.FadingIn)
            {
                currentVolume += fadeSpeed * (float)Time.deltaTime;
                if (currentVolume >= targetVolume)
                {
                    currentVolume = targetVolume;
                    State = AudioState.Playing;
                    fadeCompleteCallback?.Invoke();
                    fadeCompleteCallback = null;
                }
            }
            else if (State == AudioState.FadingOut)
            {
                currentVolume -= fadeSpeed * (float)Time.deltaTime;
                if (currentVolume <= 0f)
                {
                    currentVolume = 0f;
                    Stop();
                    fadeCompleteCallback?.Invoke();
                    fadeCompleteCallback = null;
                }
            }
        }

        private void UpdatePlayback()
        {
            if (Clip != null && Clip.Duration > 0)
            {
                CurrentTime = (DateTime.Now.TimeOfDay.TotalSeconds - startTime) * Pitch;
                PlaybackProgress = CurrentTime / Clip.Duration;

                if (CurrentTime >= Clip.Duration && !Loop)
                {
                    Stop();
                    PlaybackCompleted?.Invoke(this, new AudioSourceEventArgs(this));
                }
                else if (Loop)
                {
                    CurrentTime = CurrentTime % Clip.Duration;
                }
            }
        }

        public void UpdateVolume(float masterVolume)
        {
        }

        public void Suspend()
        {
        }

        public void Resume()
        {
        }

        public override string ToString() => $"AudioSource({ClipName}, State={State}, Volume={Volume:F2})";

        public void Dispose()
        {
            Clip = null;
            fadeCompleteCallback = null;
            PlaybackStarted = null;
            PlaybackPaused = null;
            PlaybackStopped = null;
            PlaybackCompleted = null;
        }
    }

    public enum AudioRolloffMode
    {
        Logarithmic,
        Linear,
        Inverse
    }

    public class AudioMixer
    {
        public string Name { get; set; }
        public float OutputLevel { get; set; }
        public List<AudioMixerGroup> Groups { get; set; }
        public AudioEffect[] Effects { get; set; }
    }

    public class AudioMixerGroup
    {
        public string Name { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public AudioMixer Parent { get; set; }
    }

    public class AudioEffect
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; }
        public Dictionary<string, float> Parameters { get; set; }
    }

    public class AudioCategory
    {
        public string Name { get; set; }
        public float Volume { get; set; }
        public bool IsMuted { get; set; }
        public AudioRolloffMode RolloffMode { get; set; }
    }

    public class AudioBank
    {
        public string Name { get; set; }
        public Dictionary<string, AudioClip> Clips { get; set; }
        public string[] Tags { get; set; }

        public AudioBank()
        {
            Clips = new Dictionary<string, AudioClip>();
            Tags = Array.Empty<string>();
        }

        public AudioClip GetClip(string key)
        {
            return Clips.TryGetValue(key, out var clip) ? clip : null;
        }

        public void AddClip(string key, AudioClip clip)
        {
            Clips[key] = clip;
        }
    }

    public class AudioEventArgs : EventArgs
    {
        public string Message { get; }
        public AudioClip Clip { get; }
        public AudioSource Source { get; }

        public AudioEventArgs(string message)
        {
            Message = message;
        }

        public AudioEventArgs(AudioClip clip)
        {
            Clip = clip;
            Message = clip?.Name;
        }

        public AudioEventArgs(AudioSource source)
        {
            Source = source;
            Message = source?.ClipName;
        }
    }

    public class AudioSourceEventArgs : EventArgs
    {
        public AudioSource Source { get; }

        public AudioSourceEventArgs(AudioSource source)
        {
            Source = source;
        }
    }

    public static class Time
    {
        public static float deltaTime { get; internal set; } = 0.016f;
    }
}

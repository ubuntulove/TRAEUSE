using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Core;

namespace WPFEngine.Audio
{
    public enum DistanceAttenuationModel
    {
        Linear,
        Logarithmic,
        Inverse,
        Exponential,
        Custom
    }

    public enum FilterType
    {
        LowPass,
        HighPass,
        BandPass,
        Notch,
        AllPass
    }

    public enum ReverbPreset
    {
        Off,
        Generic,
        Padded,
        Room,
        Bathroom,
        LivingRoom,
        StoneRoom,
        Auditorium,
        ConcertHall,
        Cave,
        Arena,
        Hangar,
        CarpetedHallway,
        Hallway,
        StoneCorridor,
        Alley,
        Forest,
        City,
        Mountains,
        Quarry,
        Plain,
        ParkingLot,
        SewerPipe,
        Underwater,
        Drugged,
        Dizzy,
        Psychotic
    }

    public enum BeatType
    {
        None,
        Quarter,
        Eighth,
        Sixteenth,
        Whole
    }

    public enum MusicState
    {
        Stopped,
        Playing,
        Paused,
        Transitioning
    }

    public enum SpatialBlendMode
    {
        Manual,
        Automatic2D,
        Automatic3D,
        AutomaticSwitching
    }

    public interface IHRTF
    {
        float[] GetHRTFImpulseResponse(float azimuth, float elevation, int sampleRate);
        float ApplyHRTF(float leftInput, float rightInput, float azimuth, float elevation);
    }

    public class SimpleHRTF : IHRTF
    {
        private readonly Dictionary<string, float[]> cachedResponses;

        public SimpleHRTF()
        {
            cachedResponses = new Dictionary<string, float[]>();
        }

        public float[] GetHRTFImpulseResponse(float azimuth, float elevation, int sampleRate)
        {
            var key = $"{azimuth:F0}_{elevation:F0}_{sampleRate}";
            if (cachedResponses.TryGetValue(key, out var cached))
                return cached;

            int length = sampleRate / 10;
            var response = new float[length];
            float azimuthRad = azimuth * (float)(Math.PI / 180.0);

            for (int i = 0; i < length; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = (float)Math.Exp(-t * 15.0);
                float hrtfFactor = (float)Math.Cos(azimuthRad) * 0.5f + 0.5f;
                response[i] = envelope * hrtfFactor * ((i % 2 == 0) ? 1f : -0.3f);
            }

            cachedResponses[key] = response;
            return response;
        }

        public float ApplyHRTF(float leftInput, float rightInput, float azimuth, float elevation)
        {
            float azimuthNorm = ((azimuth % 360.0f) + 360.0f) % 360.0f;
            float leftGain = (float)Math.Cos((azimuthNorm - 30.0) * Math.PI / 180.0) * 0.5f + 0.5f;
            float rightGain = (float)Math.Cos((azimuthNorm + 30.0) * Math.PI / 180.0) * 0.5f + 0.5f;
            return (leftInput * leftGain + rightInput * rightGain) * 0.707f;
        }
    }

    public class AdvancedAudioSource : IDisposable
    {
        public int SourceId { get; set; }
        public string Name { get; set; }
        public List<AudioClip> Clips { get; set; }
        public int CurrentClipIndex { get; set; }
        public AudioClip CurrentClip => CurrentClipIndex >= 0 && CurrentClipIndex < Clips.Count ? Clips[CurrentClipIndex] : null;

        public float Volume { get; set; }
        public float BaseVolume { get; set; }
        public float Pitch { get; set; }
        public float BasePitch { get; set; }
        public bool Loop { get; set; }
        public AudioState State { get; private set; }
        public AudioSourceType SourceType { get; set; }

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 LocalPosition { get; set; }

        public float DopplerLevel { get; set; }
        public DistanceAttenuationModel AttenuationModel { get; set; }
        public float MinDistance { get; set; }
        public float MaxDistance { get; set; }
        public float RolloffFactor { get; set; }
        public float Pan2D { get; set; }
        public float SpatialBlend { get; set; }
        public SpatialBlendMode BlendMode { get; set; }
        public float ReverbZoneMix { get; set; }

        public float CustomAttenuationCurve(float distance);

        public float Spread { get; set; }
        public Vector3 PanPosition { get; set; }

        public float PlaybackSpeed { get; set; }
        public double CurrentTime { get; private set; }
        public double Duration => CurrentClip?.Duration ?? 0.0;
        public double PlaybackProgress => Duration > 0 ? CurrentTime / Duration : 0.0;

        public int Priority { get; set; }
        public bool IgnoreListenerVolume { get; set; }
        public bool IgnoreListenerPause { get; set; }
        public bool BypassEffects { get; set; }
        public bool BypassReverbZones { get; set; }
        public bool BypassListenerVolume { get; set; }

        public AudioMixerGroup MixerGroup { get; set; }
        public List<AudioEffectBase> Effects { get; set; }

        public Vector3 LastPosition { get; private set; }
        public float LastDopplerShift { get; private set; }

        private float currentVolume;
        private float currentPitch;
        private float fadeTargetVolume;
        private float fadeSpeed;
        private Action fadeCompleteCallback;
        private double startTime;
        private double pauseTime;
        private bool isPaused;

        public event EventHandler<AudioSourceEventArgs> PlaybackStarted;
        public event EventHandler<AudioSourceEventArgs> PlaybackPaused;
        public event EventHandler<AudioSourceEventArgs> PlaybackStopped;
        public event EventHandler<AudioSourceEventArgs> PlaybackCompleted;
        public event EventHandler<AudioSourceEventArgs> ClipChanged;

        public AdvancedAudioSource()
        {
            Clips = new List<AudioClip>();
            CurrentClipIndex = -1;
            Volume = 1f;
            BaseVolume = 1f;
            Pitch = 1f;
            BasePitch = 1f;
            Loop = false;
            State = AudioState.Stopped;
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            LocalPosition = Vector3.Zero;
            DopplerLevel = 1f;
            AttenuationModel = DistanceAttenuationModel.Inverse;
            MinDistance = 1f;
            MaxDistance = 500f;
            RolloffFactor = 1f;
            Pan2D = 0f;
            SpatialBlend = 1f;
            BlendMode = SpatialBlendMode.Automatic3D;
            ReverbZoneMix = 1f;
            Spread = 0f;
            PanPosition = Vector3.Zero;
            PlaybackSpeed = 1f;
            Priority = 128;
            IgnoreListenerVolume = false;
            IgnoreListenerPause = false;
            BypassEffects = false;
            BypassReverbZones = false;
            BypassListenerVolume = false;
            Effects = new List<AudioEffectBase>();
            currentVolume = 1f;
            currentPitch = 1f;
            fadeTargetVolume = 1f;
            fadeSpeed = 0f;
            LastDopplerShift = 1f;
        }

        public void AddClip(AudioClip clip)
        {
            if (clip != null && !Clips.Contains(clip))
            {
                Clips.Add(clip);
                if (CurrentClipIndex < 0) CurrentClipIndex = 0;
            }
        }

        public void RemoveClip(AudioClip clip)
        {
            if (clip != null && Clips.Contains(clip))
            {
                int index = Clips.IndexOf(clip);
                Clips.Remove(clip);
                if (CurrentClipIndex >= Clips.Count)
                    CurrentClipIndex = Clips.Count - 1;
            }
        }

        public void SelectClip(int index)
        {
            if (index >= 0 && index < Clips.Count && index != CurrentClipIndex)
            {
                CurrentClipIndex = index;
                ClipChanged?.Invoke(this, new AudioSourceEventArgs(this));
            }
        }

        public void Play()
        {
            if (CurrentClip == null || !CurrentClip.IsLoaded) return;

            State = AudioState.Playing;
            startTime = DateTime.Now.TimeOfDay.TotalSeconds;
            isPaused = false;
            UpdateDopplerShift();
            PlaybackStarted?.Invoke(this, new AudioSourceEventArgs(this));
        }

        public void Play(float delaySeconds)
        {
            System.Threading.Tasks.Task.Run(async () =>
            {
                await System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(delaySeconds));
                Play();
            });
        }

        public void Play(AudioClip clip)
        {
            if (clip == null) return;

            if (!Clips.Contains(clip))
                AddClip(clip);

            CurrentClipIndex = Clips.IndexOf(clip);
            Play();
        }

        public void Pause()
        {
            if (State == AudioState.Playing)
            {
                State = AudioState.Paused;
                pauseTime = CurrentTime;
                isPaused = true;
                PlaybackPaused?.Invoke(this, new AudioSourceEventArgs(this));
            }
        }

        public void Resume()
        {
            if (State == AudioState.Paused)
            {
                State = AudioState.Playing;
                isPaused = false;
                startTime = DateTime.Now.TimeOfDay.TotalSeconds - pauseTime;
                PlaybackStarted?.Invoke(this, new AudioSourceEventArgs(this));
            }
        }

        public void Stop()
        {
            State = AudioState.Stopped;
            CurrentTime = 0;
            pauseTime = 0;
            startTime = 0;
            isPaused = false;
            PlaybackStopped?.Invoke(this, new AudioSourceEventArgs(this));
        }

        public void FadeIn(float fadeTime, Action onComplete = null)
        {
            if (CurrentClip == null) return;

            State = AudioState.FadingIn;
            fadeTargetVolume = Volume;
            currentVolume = 0f;
            fadeSpeed = Volume / fadeTime;
            fadeCompleteCallback = onComplete;
            Play();
        }

        public void FadeOut(float fadeTime, Action onComplete = null)
        {
            if (State != AudioState.Playing && State != AudioState.FadingIn) return;

            State = AudioState.FadingOut;
            fadeTargetVolume = 0f;
            fadeSpeed = Volume / fadeTime;
            fadeCompleteCallback = onComplete;
        }

        public void FadeTo(float targetVolume, float fadeTime, Action onComplete = null)
        {
            if (State != AudioState.Playing && State != AudioState.FadingIn && State != AudioState.FadingOut) return;

            fadeTargetVolume = targetVolume;
            fadeSpeed = (targetVolume - currentVolume) / fadeTime;
            fadeCompleteCallback = onComplete;
        }

        public float CalculateDistanceAttenuation(float distance)
        {
            distance = Math.Max(0f, distance);

            if (distance <= MinDistance)
                return 1f;

            if (distance >= MaxDistance)
                return 0f;

            switch (AttenuationModel)
            {
                case DistanceAttenuationModel.Linear:
                    return 1f - (distance - MinDistance) / (MaxDistance - MinDistance);

                case DistanceAttenuationModel.Logarithmic:
                    return MinDistance / (MinDistance + RolloffFactor * (distance - MinDistance));

                case DistanceAttenuationModel.Inverse:
                    return MinDistance / (MinDistance + RolloffFactor * distance);

                case DistanceAttenuationModel.Exponential:
                    return (float)Math.Exp(-RolloffFactor * (distance - MinDistance) / MaxDistance);

                case DistanceAttenuationModel.Custom:
                    return CustomAttenuationCurve(distance);

                default:
                    return 1f;
            }
        }

        public float CalculatePan(Vector3 listenerPosition, Vector3 listenerForward)
        {
            Vector3 toListener = listenerPosition - Position;
            float distance = toListener.Length();

            if (distance < 0.01f) return 0f;

            Vector3 direction = toListener.Normalized();
            float dot = Vector3.Dot(direction, listenerForward);

            return Math.Clamp(dot, -1f, 1f);
        }

        private float CalculateDopplerShift(Vector3 listenerVelocity)
        {
            Vector3 toListener = AdvancedAudioListener.Instance.Position - Position;
            float distance = toListener.Length();

            if (distance < 0.01f) return 1f;

            Vector3 relativeVelocity = listenerVelocity - Velocity;
            float velocityAlongLine = Vector3.Dot(relativeVelocity, toListener.Normalized());

            float speedOfSound = 343f;
            float frequencyShift = 1f - (velocityAlongLine / speedOfSound) * DopplerLevel * AdvancedAudioListener.Instance.DopplerFactor;

            return Math.Clamp(frequencyShift, 0.5f, 2f);
        }

        public void UpdateDopplerShift()
        {
            LastDopplerShift = CalculateDopplerShift(AdvancedAudioListener.Instance.Velocity);
            currentPitch = BasePitch * LastDopplerShift * PlaybackSpeed;
        }

        public void Update()
        {
            if (State == AudioState.Stopped) return;

            LastPosition = Position;

            if (!isPaused && !IgnoreListenerPause)
            {
                UpdateDopplerShift();
                UpdatePlayback();
            }

            if (State == AudioState.FadingIn || State == AudioState.FadingOut || Math.Abs(fadeSpeed) > 0.001f)
            {
                UpdateFade();
            }
        }

        private void UpdateFade()
        {
            if (State == AudioState.FadingIn)
            {
                currentVolume += fadeSpeed * Time.deltaTime;
                if (currentVolume >= fadeTargetVolume)
                {
                    currentVolume = fadeTargetVolume;
                    State = AudioState.Playing;
                    fadeSpeed = 0f;
                    fadeCompleteCallback?.Invoke();
                    fadeCompleteCallback = null;
                }
            }
            else if (State == AudioState.FadingOut)
            {
                currentVolume -= fadeSpeed * Time.deltaTime;
                if (currentVolume <= 0f)
                {
                    currentVolume = 0f;
                    Stop();
                    fadeSpeed = 0f;
                    fadeCompleteCallback?.Invoke();
                    fadeCompleteCallback = null;
                }
            }
        }

        private void UpdatePlayback()
        {
            if (CurrentClip != null && Duration > 0)
            {
                CurrentTime = (DateTime.Now.TimeOfDay.TotalSeconds - startTime) * currentPitch;

                if (CurrentTime >= Duration && !Loop)
                {
                    if (CurrentClipIndex < Clips.Count - 1)
                    {
                        CurrentClipIndex++;
                        startTime = DateTime.Now.TimeOfDay.TotalSeconds;
                        ClipChanged?.Invoke(this, new AudioSourceEventArgs(this));
                    }
                    else
                    {
                        Stop();
                        PlaybackCompleted?.Invoke(this, new AudioSourceEventArgs(this));
                    }
                }
                else if (Loop && CurrentTime >= Duration)
                {
                    CurrentTime = CurrentTime % Duration;
                    startTime = DateTime.Now.TimeOfDay.TotalSeconds - CurrentTime / currentPitch;
                }
            }
        }

        public float GetEffectiveVolume(float listenerDistance)
        {
            float attenuation = CalculateDistanceAttenuation(listenerDistance);
            return BaseVolume * currentVolume * attenuation;
        }

        public void Dispose()
        {
            Stop();
            Clips.Clear();
            Effects.Clear();
            MixerGroup = null;
            PlaybackStarted = null;
            PlaybackPaused = null;
            PlaybackStopped = null;
            PlaybackCompleted = null;
            ClipChanged = null;
        }
    }

    public class AudioClip
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

        public AudioClip() { }

        public AudioClip(string path)
        {
            FilePath = path;
            LoadFromFile(path);
        }

        private void LoadFromFile(string path)
        {
            try
            {
                if (!System.IO.File.Exists(path)) return;

                var extension = System.IO.Path.GetExtension(path).ToLower();
                Format = extension switch
                {
                    ".wav" => AudioFormat.WAV,
                    ".mp3" => AudioFormat.MP3,
                    ".ogg" => AudioFormat.OGG,
                    ".flac" => AudioFormat.FLAC,
                    ".aiff" or ".aif" => AudioFormat.AIFF,
                    _ => AudioFormat.Unknown
                };

                Data = System.IO.File.ReadAllBytes(path);
                IsLoaded = true;
            }
            catch { }
        }

        public void Dispose()
        {
            Data = null;
            IsLoaded = false;
        }
    }

    public class AdvancedAudioListener : IDisposable
    {
        private static AdvancedAudioListener instance;
        private List<AdvancedAudioListener> additionalListeners;
        private int activeListenerIndex;

        public static AdvancedAudioListener Instance
        {
            get
            {
                if (instance == null)
                    instance = new AdvancedAudioListener();
                return instance;
            }
        }

        public Vector3 Position { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 Forward { get; set; }
        public Vector3 Up { get; set; }
        public float DopplerFactor { get; set; }
        public float SpeedOfSound { get; set; }
        public float Volume { get; set; }
        public bool Enabled { get; set; }
        public float ConeOfAudibilityRadius { get; set; }
        public float ConeOfAudibilityAngle { get; set; }
        public float NearClip { get; set; }
        public float FarClip { get; set; }

        public static int ListenerCount => Instance.additionalListeners.Count + 1;
        public int ActiveListenerIndex
        {
            get => activeListenerIndex;
            set
            {
                if (value >= 0 && value <= additionalListeners.Count)
                    activeListenerIndex = value;
            }
        }

        public event EventHandler ListenerUpdated;

        public AdvancedAudioListener()
        {
            additionalListeners = new List<AdvancedAudioListener>();
            Position = Vector3.Zero;
            Velocity = Vector3.Zero;
            Forward = new Vector3(0, 0, -1);
            Up = new Vector3(0, 1, 0);
            DopplerFactor = 1f;
            SpeedOfSound = 343f;
            Volume = 1f;
            Enabled = true;
            ConeOfAudibilityRadius = 100f;
            ConeOfAudibilityAngle = 360f;
            NearClip = 0.1f;
            FarClip = 1000f;
            activeListenerIndex = 0;
        }

        public void AddListener(AdvancedAudioListener listener)
        {
            if (listener != null && !additionalListeners.Contains(listener))
                additionalListeners.Add(listener);
        }

        public void RemoveListener(AdvancedAudioListener listener)
        {
            if (listener != null)
                additionalListeners.Remove(listener);
        }

        public AdvancedAudioListener GetListener(int index)
        {
            if (index == 0) return this;
            if (index > 0 && index <= additionalListeners.Count)
                return additionalListeners[index - 1];
            return null;
        }

        public bool IsWithinAudibilityCone(Vector3 sourcePosition)
        {
            Vector3 toSource = sourcePosition - Position;
            float distance = toSource.Length();

            if (distance > ConeOfAudibilityRadius)
                return false;

            if (ConeOfAudibilityAngle >= 360f)
                return true;

            Vector3 direction = toSource.Normalized();
            float dot = Vector3.Dot(direction, Forward);
            float angle = (float)Math.Acos(Math.Clamp(dot, -1f, 1f)) * (180f / (float)Math.PI);

            return angle <= ConeOfAudibilityAngle * 0.5f;
        }

        public float GetSourceGain(AdvancedAudioSource source)
        {
            if (!Enabled) return 0f;

            Vector3 toSource = source.Position - Position;
            float distance = toSource.Length();

            if (distance > FarClip) return 0f;
            if (distance < NearClip) return 1f;

            float gain = 1f - (distance - NearClip) / (FarClip - NearClip);
            return Math.Max(0f, gain);
        }

        public void Update()
        {
            ListenerUpdated?.Invoke(this, EventArgs.Empty);
        }

        public void SetOrientation(Vector3 forward, Vector3 up)
        {
            Forward = forward.Normalized();
            Up = up.Normalized();
        }

        public void LookAt(Vector3 target)
        {
            Forward = (target - Position).Normalized();
        }

        public void Dispose()
        {
            additionalListeners.Clear();
            ListenerUpdated = null;
        }
    }

    public abstract class AudioEffectBase
    {
        public string Name { get; set; }
        public bool IsEnabled { get; set; } = true;
        public float Mix { get; set; } = 1f;
        public abstract float Process(float input, float time);
        public abstract void Reset();
        public abstract AudioEffectBase Clone();
    }

    public class ReverbEffect : AudioEffectBase
    {
        public float RoomSize { get; set; }
        public float Damping { get; set; }
        public float WetMix { get; set; }
        public float DryMix { get; set; }
        public float DecayTime { get; set; }
        public float PreDelay { get; set; }
        public float Diffusion { get; set; }
        public float Density { get; set; }
        public bool HighFreqDecay { get; set; }
        public float LowFreqDecay { get; set; }
        public float HighFreqCutoff { get; set; }
        public float LowFreqCutoff { get; set; }

        private float[] delayBuffer;
        private float[] combBuffers;
        private float[] allpassBuffers;
        private int delayIndex;
        private int[] combIndices;
        private int[] allpassIndices;
        private Random random;

        public ReverbEffect()
        {
            Name = "Reverb";
            RoomSize = 0.5f;
            Damping = 0.5f;
            WetMix = 0.5f;
            DryMix = 0.5f;
            DecayTime = 1.5f;
            PreDelay = 0.02f;
            Diffusion = 0.8f;
            Density = 1f;
            HighFreqDecay = true;
            LowFreqDecay = 0.5f;
            HighFreqCutoff = 6000f;
            LowFreqCutoff = 200f;

            InitializeBuffers();
        }

        private void InitializeBuffers()
        {
            int sampleRate = 44100;
            int maxDelay = (int)(sampleRate * 0.1f);
            delayBuffer = new float[maxDelay];
            delayIndex = 0;

            int[] combDelays = { 1557, 1617, 1491, 1422, 1277, 1356, 1188, 1116 };
            combBuffers = new float[combDelays.Max() + 1];
            combIndices = new int[combDelays.Length];
            for (int i = 0; i < combDelays.Length; i++)
                combIndices[i] = 0;

            int[] allpassDelays = { 225, 556, 441, 341 };
            allpassBuffers = new float[allpassDelays.Max() + 1];
            allpassIndices = new int[allpassDelays.Length];
            for (int i = 0; i < allpassDelays.Length; i++)
                allpassIndices[i] = 0;

            random = new Random();
        }

        public override float Process(float input, float time)
        {
            float output = 0f;
            float combFeedback = RoomSize * 0.28f + 0.7f;
            float dampFactor = Damping * 0.4f;

            delayBuffer[delayIndex] = input;
            delayIndex = (delayIndex + 1) % delayBuffer.Length;

            float preDelaySample = delayBuffer[(delayIndex - (int)(PreDelay * 44100) + delayBuffer.Length) % delayBuffer.Length];

            for (int i = 0; i < combBuffers.Length; i++)
            {
                combBuffers[i] *= (1f - dampFactor);
            }

            for (int i = 0; i < combIndices.Length; i++)
            {
                int idx = combIndices[i];
                float sample = combBuffers[idx];
                combBuffers[idx] = preDelaySample + sample * combFeedback;
                output += sample;
                combIndices[i] = (idx + 1) % combBuffers.Length;
            }

            output /= combIndices.Length;

            for (int i = 0; i < allpassIndices.Length; i++)
            {
                int idx = allpassIndices[i];
                float bufOut = allpassBuffers[idx];
                float bufIn = output + bufOut * 0.5f;
                allpassBuffers[idx] = bufIn;
                output = bufOut - bufIn;
                allpassIndices[i] = (idx + 1) % allpassBuffers.Length;
            }

            float wet = output * WetMix;
            float dry = input * DryMix;

            return (wet + dry) * Mix;
        }

        public override void Reset()
        {
            Array.Clear(delayBuffer, 0, delayBuffer.Length);
            Array.Clear(combBuffers, 0, combBuffers.Length);
            Array.Clear(allpassBuffers, 0, allpassBuffers.Length);
        }

        public override AudioEffectBase Clone()
        {
            return new ReverbEffect
            {
                Name = Name,
                IsEnabled = IsEnabled,
                Mix = Mix,
                RoomSize = RoomSize,
                Damping = Damping,
                WetMix = WetMix,
                DryMix = DryMix,
                DecayTime = DecayTime,
                PreDelay = PreDelay,
                Diffusion = Diffusion,
                Density = Density,
                HighFreqDecay = HighFreqDecay,
                LowFreqDecay = LowFreqDecay,
                HighFreqCutoff = HighFreqCutoff,
                LowFreqCutoff = LowFreqCutoff
            };
        }

        public static ReverbEffect FromPreset(ReverbPreset preset)
        {
            return preset switch
            {
                ReverbPreset.Off => new ReverbEffect { RoomSize = 0, DecayTime = 0, WetMix = 0 },
                ReverbPreset.Generic => new ReverbEffect { RoomSize = 0.5f, DecayTime = 1.5f },
                ReverbPreset.Padded => new ReverbEffect { RoomSize = 0.1f, DecayTime = 0.5f, Damping = 0.9f },
                ReverbPreset.Room => new ReverbEffect { RoomSize = 0.3f, DecayTime = 0.8f },
                ReverbPreset.Bathroom => new ReverbEffect { RoomSize = 0.4f, DecayTime = 1.2f, Damping = 0.8f, WetMix = 0.6f },
                ReverbPreset.LivingRoom => new ReverbEffect { RoomSize = 0.2f, DecayTime = 0.6f, Damping = 0.9f },
                ReverbPreset.StoneRoom => new ReverbEffect { RoomSize = 0.6f, DecayTime = 2.5f, Damping = 0.4f },
                ReverbPreset.Auditorium => new ReverbEffect { RoomSize = 0.7f, DecayTime = 3.5f, Damping = 0.3f },
                ReverbPreset.ConcertHall => new ReverbEffect { RoomSize = 0.8f, DecayTime = 4.0f, Damping = 0.25f, WetMix = 0.7f },
                ReverbPreset.Cave => new ReverbEffect { RoomSize = 0.9f, DecayTime = 5.0f, Damping = 0.2f },
                ReverbPreset.Arena => new ReverbEffect { RoomSize = 0.75f, DecayTime = 3.0f, Damping = 0.35f },
                ReverbPreset.Hangar => new ReverbEffect { RoomSize = 0.85f, DecayTime = 4.5f, Damping = 0.3f },
                ReverbPreset.CarpetedHallway => new ReverbEffect { RoomSize = 0.15f, DecayTime = 0.4f, Damping = 0.95f },
                ReverbPreset.Hallway => new ReverbEffect { RoomSize = 0.35f, DecayTime = 1.0f, Damping = 0.7f },
                ReverbPreset.StoneCorridor => new ReverbEffect { RoomSize = 0.55f, DecayTime = 2.0f, Damping = 0.45f },
                ReverbPreset.Alley => new ReverbEffect { RoomSize = 0.45f, DecayTime = 1.5f, Damping = 0.5f },
                ReverbPreset.Forest => new ReverbEffect { RoomSize = 0.4f, DecayTime = 2.5f, Damping = 0.6f, WetMix = 0.4f },
                ReverbPreset.City => new ReverbEffect { RoomSize = 0.3f, DecayTime = 1.2f, Damping = 0.55f },
                ReverbPreset.Mountains => new ReverbEffect { RoomSize = 0.6f, DecayTime = 3.5f, Damping = 0.5f, WetMix = 0.35f },
                ReverbPreset.Quarry => new ReverbEffect { RoomSize = 0.65f, DecayTime = 3.0f, Damping = 0.4f },
                ReverbPreset.Plain => new ReverbEffect { RoomSize = 0.35f, DecayTime = 2.0f, Damping = 0.65f, WetMix = 0.3f },
                ReverbPreset.ParkingLot => new ReverbEffect { RoomSize = 0.5f, DecayTime = 1.5f, Damping = 0.5f },
                ReverbPreset.SewerPipe => new ReverbEffect { RoomSize = 0.4f, DecayTime = 1.8f, Damping = 0.45f },
                ReverbPreset.Underwater => new ReverbEffect { RoomSize = 0.3f, DecayTime = 2.5f, Damping = 0.95f, WetMix = 0.8f, HighFreqCutoff = 2000f },
                ReverbPreset.Drugged => new ReverbEffect { RoomSize = 0.9f, DecayTime = 5.0f, Damping = 0.8f, WetMix = 0.9f },
                ReverbPreset.Dizzy => new ReverbEffect { RoomSize = 0.6f, DecayTime = 2.0f, Damping = 0.7f, WetMix = 0.7f },
                ReverbPreset.Psychotic => new ReverbEffect { RoomSize = 0.8f, DecayTime = 4.0f, Damping = 0.6f, WetMix = 0.85f },
                _ => new ReverbEffect()
            };
        }
    }

    public class ChorusEffect : AudioEffectBase
    {
        public float Rate { get; set; }
        public float Depth { get; set; }
        public float CenterDelay { get; set; }
        public float Feedback { get; set; }
        public float WetMix { get; set; }
        public float DryMix { get; set; }
        public bool IsPhaser { get; set; }

        private float[] delayBuffer;
        private int delayIndex;
        private float phase;
        private float lfoPhase;

        public ChorusEffect()
        {
            Name = "Chorus";
            Rate = 1.5f;
            Depth = 0.002f;
            CenterDelay = 0.007f;
            Feedback = 0.5f;
            WetMix = 0.5f;
            DryMix = 0.5f;
            IsPhaser = false;

            int maxDelaySamples = (int)(44100 * 0.05f);
            delayBuffer = new float[maxDelaySamples];
            delayIndex = 0;
            phase = 0f;
            lfoPhase = 0f;
        }

        public override float Process(float input, float time)
        {
            float delayMs = CenterDelay * 1000f + (float)Math.Sin(lfoPhase * 2.0 * Math.PI) * Depth * 1000f;
            int delaySamples = (int)(delayMs * 44.1f);
            delaySamples = Math.Clamp(delaySamples, 1, delayBuffer.Length - 1);

            float delayed = delayBuffer[(delayIndex - delaySamples + delayBuffer.Length) % delayBuffer.Length];
            float output = delayed;

            delayBuffer[delayIndex] = input + delayed * Feedback;
            delayIndex = (delayIndex + 1) % delayBuffer.Length;

            lfoPhase += Rate / 44100f;
            if (lfoPhase >= 1f) lfoPhase -= 1f;

            if (IsPhaser)
            {
                float phaseShift = (float)Math.Sin(phase) * 0.5f + 0.5f;
                phase += Rate * 0.001f;
                if (phase >= 1f) phase -= 1f;
                output = input * phaseShift + delayed * (1f - phaseShift);
            }

            return (output * WetMix + input * DryMix) * Mix;
        }

        public override void Reset()
        {
            Array.Clear(delayBuffer, 0, delayBuffer.Length);
            phase = 0f;
            lfoPhase = 0f;
        }

        public override AudioEffectBase Clone()
        {
            return new ChorusEffect
            {
                Name = Name,
                IsEnabled = IsEnabled,
                Mix = Mix,
                Rate = Rate,
                Depth = Depth,
                CenterDelay = CenterDelay,
                Feedback = Feedback,
                WetMix = WetMix,
                DryMix = DryMix,
                IsPhaser = IsPhaser
            };
        }
    }

    public class EchoEffect : AudioEffectBase
    {
        public float Delay { get; set; }
        public float Feedback { get; set; }
        public float WetMix { get; set; }
        public float DryMix { get; set; }
        public int MaxTapCount { get; set; }

        private float[] delayBuffer;
        private int delayIndex;
        private int tapIndex;
        private int[] tapDelays;

        public EchoEffect()
        {
            Name = "Echo";
            Delay = 0.3f;
            Feedback = 0.5f;
            WetMix = 0.5f;
            DryMix = 0.5f;
            MaxTapCount = 8;

            int maxDelaySamples = (int)(44100 * 2.0f);
            delayBuffer = new float[maxDelaySamples];
            delayIndex = 0;
            tapIndex = 0;
            tapDelays = new int[MaxTapCount];
        }

        public override float Process(float input, float time)
        {
            int delaySamples = (int)(Delay * 44100f);
            delaySamples = Math.Clamp(delaySamples, 1, delayBuffer.Length - 1);

            float delayed = delayBuffer[(delayIndex - delaySamples + delayBuffer.Length) % delayBuffer.Length];
            float output = delayed;

            delayBuffer[delayIndex] = input + delayed * Feedback;
            delayIndex = (delayIndex + 1) % delayBuffer.Length;

            tapIndex = (tapIndex + 1) % MaxTapCount;

            return (output * WetMix + input * DryMix) * Mix;
        }

        public override void Reset()
        {
            Array.Clear(delayBuffer, 0, delayBuffer.Length);
            tapIndex = 0;
        }

        public override AudioEffectBase Clone()
        {
            return new EchoEffect
            {
                Name = Name,
                IsEnabled = IsEnabled,
                Mix = Mix,
                Delay = Delay,
                Feedback = Feedback,
                WetMix = WetMix,
                DryMix = DryMix,
                MaxTapCount = MaxTapCount
            };
        }
    }

    public class DistortionEffect : AudioEffectBase
    {
        public float Drive { get; set; }
        public float Tone { get; set; }
        public float OutputLevel { get; set; }
        public DistortionCurveType CurveType { get; set; }

        public enum DistortionCurveType
        {
            Soft,
            Hard,
            Fuzz,
            Tube,
            Bitcrush
        }

        private float lowPassState;

        public DistortionEffect()
        {
            Name = "Distortion";
            Drive = 0.5f;
            Tone = 0.5f;
            OutputLevel = 1f;
            CurveType = DistortionCurveType.Soft;
            lowPassState = 0f;
        }

        public override float Process(float input, float time)
        {
            float driven = input * (1f + Drive * 10f);

            float output = CurveType switch
            {
                DistortionCurveType.Soft => SoftClip(driven),
                DistortionCurveType.Hard => HardClip(driven),
                DistortionCurveType.Fuzz => FuzzClip(driven),
                DistortionCurveType.Tube => TubeClip(driven),
                DistortionCurveType.Bitcrush => Bitcrush(driven, 8),
                _ => driven
            };

            lowPassState = lowPassState + Tone * (output - lowPassState);
            output = lowPassState;

            return output * OutputLevel * Mix;
        }

        private float SoftClip(float x)
        {
            return (float)Math.Tanh(x);
        }

        private float HardClip(float x)
        {
            return Math.Clamp(x, -1f, 1f);
        }

        private float FuzzClip(float x)
        {
            return (float)Math.Sign(x) * (1f - (float)Math.Exp(-Math.Abs(x) * 2f));
        }

        private float TubeClip(float x)
        {
            return x / (1f + Math.Abs(x));
        }

        private float Bitcrush(float x, int bits)
        {
            float levels = (float)Math.Pow(2, bits);
            return (float)Math.Round(x * levels) / levels;
        }

        public override void Reset()
        {
            lowPassState = 0f;
        }

        public override AudioEffectBase Clone()
        {
            return new DistortionEffect
            {
                Name = Name,
                IsEnabled = IsEnabled,
                Mix = Mix,
                Drive = Drive,
                Tone = Tone,
                OutputLevel = OutputLevel,
                CurveType = CurveType
            };
        }
    }

    public class FilterEffect : AudioEffectBase
    {
        public FilterType FilterType { get; set; }
        public float CutoffFrequency { get; set; }
        public float Resonance { get; set; }
        public float Bandwidth { get; set; }
        public float Gain { get; set; }

        private float x1, x2, y1, y2;
        private float a0, a1, a2, b0, b1, b2;

        public FilterEffect()
        {
            Name = "Filter";
            FilterType = FilterType.LowPass;
            CutoffFrequency = 1000f;
            Resonance = 0.707f;
            Bandwidth = 1f;
            Gain = 1f;
            UpdateCoefficients();
        }

        public void UpdateCoefficients()
        {
            float w0 = 2f * (float)Math.PI * CutoffFrequency / 44100f;
            float Q = Math.Max(0.1f, Resonance);
            float alpha = (float)Math.Sin(w0) / (2f * Q);
            float A = (float)Math.Pow(10, Gain / 40f);

            switch (FilterType)
            {
                case FilterType.LowPass:
                    b0 = (1f - (float)Math.Cos(w0)) / 2f;
                    b1 = 1f - (float)Math.Cos(w0);
                    b2 = (1f - (float)Math.Cos(w0)) / 2f;
                    a0 = 1f + alpha;
                    a1 = -2f * (float)Math.Cos(w0);
                    a2 = 1f - alpha;
                    break;

                case FilterType.HighPass:
                    b0 = (1f + (float)Math.Cos(w0)) / 2f;
                    b1 = -(1f + (float)Math.Cos(w0));
                    b2 = (1f + (float)Math.Cos(w0)) / 2f;
                    a0 = 1f + alpha;
                    a1 = -2f * (float)Math.Cos(w0);
                    a2 = 1f - alpha;
                    break;

                case FilterType.BandPass:
                    b0 = alpha;
                    b1 = 0;
                    b2 = -alpha;
                    a0 = 1f + alpha;
                    a1 = -2f * (float)Math.Cos(w0);
                    a2 = 1f - alpha;
                    break;

                case FilterType.Notch:
                    b0 = 1f;
                    b1 = -2f * (float)Math.Cos(w0);
                    b2 = 1f;
                    a0 = 1f + alpha;
                    a1 = -2f * (float)Math.Cos(w0);
                    a2 = 1f - alpha;
                    break;

                case FilterType.AllPass:
                    b0 = 1f - alpha;
                    b1 = -2f * (float)Math.Cos(w0);
                    b2 = 1f + alpha;
                    a0 = 1f + alpha;
                    a1 = -2f * (float)Math.Cos(w0);
                    a2 = 1f - alpha;
                    break;
            }

            b0 /= a0;
            b1 /= a0;
            b2 /= a0;
            a1 /= a0;
            a2 /= a0;
        }

        public override float Process(float input, float time)
        {
            float output = b0 * input + b1 * x1 + b2 * x2 - a1 * y1 - a2 * y2;

            x2 = x1;
            x1 = input;
            y2 = y1;
            y1 = output;

            return output * Mix;
        }

        public override void Reset()
        {
            x1 = x2 = y1 = y2 = 0f;
        }

        public override AudioEffectBase Clone()
        {
            return new FilterEffect
            {
                Name = Name,
                IsEnabled = IsEnabled,
                Mix = Mix,
                FilterType = FilterType,
                CutoffFrequency = CutoffFrequency,
                Resonance = Resonance,
                Bandwidth = Bandwidth,
                Gain = Gain
            };
        }

        public void SetFrequency(float frequency)
        {
            CutoffFrequency = Math.Clamp(frequency, 20f, 20000f);
            UpdateCoefficients();
        }

        public void SweepTo(float targetFrequency, float duration, float time)
        {
            float startFreq = CutoffFrequency;
            float t = Math.Clamp(time / duration, 0f, 1f);
            float newFreq = startFreq + (targetFrequency - startFreq) * t;
            SetFrequency(newFreq);
        }
    }

    public class CompressorEffect : AudioEffectBase
    {
        public float Threshold { get; set; }
        public float Ratio { get; set; }
        public float Attack { get; set; }
        public float Release { get; set; }
        public float KneeWidth { get; set; }
        public float MakeupGain { get; set; }
        public bool LimiterMode { get; set; }

        private float envelope;

        public CompressorEffect()
        {
            Name = "Compressor";
            Threshold = -20f;
            Ratio = 4f;
            Attack = 0.005f;
            Release = 0.1f;
            KneeWidth = 6f;
            MakeupGain = 0f;
            LimiterMode = false;
            envelope = 0f;
        }

        public override float Process(float input, float time)
        {
            float inputDb = 20f * (float)Math.Log10(Math.Max(0.0001f, Math.Abs(input)));
            float absInput = Math.Abs(input);

            float kneeStart = Threshold - KneeWidth / 2f;
            float kneeEnd = Threshold + KneeWidth / 2f;
            float gainReduction = 0f;

            if (inputDb < kneeStart)
            {
                gainReduction = 0f;
            }
            else if (inputDb > kneeEnd)
            {
                float overDb = inputDb - Threshold;
                float compressedDb = LimiterMode ? 0f : overDb / Ratio;
                gainReduction = overDb - compressedDb;
            }
            else
            {
                float x = inputDb - kneeStart;
                float y = x * x / (2f * KneeWidth);
                gainReduction = y / Ratio;
            }

            float targetEnvelope = absInput;
            float coeff = targetEnvelope > envelope ? Attack : Release;
            envelope = envelope + coeff * (targetEnvelope - envelope);

            float gainLinear = (float)Math.Pow(10, -gainReduction / 20f);
            float makeupLinear = (float)Math.Pow(10, MakeupGain / 20f);

            return input * gainLinear * makeupLinear * Mix;
        }

        public override void Reset()
        {
            envelope = 0f;
        }

        public override AudioEffectBase Clone()
        {
            return new CompressorEffect
            {
                Name = Name,
                IsEnabled = IsEnabled,
                Mix = Mix,
                Threshold = Threshold,
                Ratio = Ratio,
                Attack = Attack,
                Release = Release,
                KneeWidth = KneeWidth,
                MakeupGain = MakeupGain,
                LimiterMode = LimiterMode
            };
        }

        public float GetGainReduction() => -(float)Math.Log10(Math.Max(0.0001f, envelope)) * 20f / Ratio;
    }

    public class FlangerEffect : AudioEffectBase
    {
        public float Rate { get; set; }
        public float Depth { get; set; }
        public float Feedback { get; set; }
        public float WetMix { get; set; }
        public float DryMix { get; set; }

        private float[] delayBuffer;
        private int delayIndex;
        private float lfoPhase;

        public FlangerEffect()
        {
            Name = "Flanger";
            Rate = 0.5f;
            Depth = 0.003f;
            Feedback = 0.7f;
            WetMix = 0.5f;
            DryMix = 0.5f;

            int maxDelaySamples = (int)(44100 * 0.01f);
            delayBuffer = new float[maxDelaySamples];
            delayIndex = 0;
            lfoPhase = 0f;
        }

        public override float Process(float input, float time)
        {
            float delayMs = 0.5f + (float)Math.Sin(lfoPhase * 2.0 * Math.PI) * Depth * 1000f;
            int delaySamples = Math.Clamp((int)(delayMs * 44.1f), 1, delayBuffer.Length - 1);

            float delayed = delayBuffer[(delayIndex - delaySamples + delayBuffer.Length) % delayBuffer.Length];
            float output = input + delayed;

            delayBuffer[delayIndex] = input + delayed * Feedback;
            delayIndex = (delayIndex + 1) % delayBuffer.Length;

            lfoPhase += Rate / 44100f;
            if (lfoPhase >= 1f) lfoPhase -= 1f;

            return (output * WetMix + input * DryMix) * Mix;
        }

        public override void Reset()
        {
            Array.Clear(delayBuffer, 0, delayBuffer.Length);
            lfoPhase = 0f;
        }

        public override AudioEffectBase Clone()
        {
            return new FlangerEffect
            {
                Name = Name,
                IsEnabled = IsEnabled,
                Mix = Mix,
                Rate = Rate,
                Depth = Depth,
                Feedback = Feedback,
                WetMix = WetMix,
                DryMix = DryMix
            };
        }
    }

    public class PhaserEffect : AudioEffectBase
    {
        public float Rate { get; set; }
        public float Depth { get; set; }
        public float Feedback { get; set; }
        public float WetMix { get; set; }
        public float DryMix { get; set; }
        public float MinFrequency { get; set; }
        public float MaxFrequency { get; set; }

        private float[] x1, x2, y1, y2;
        private float lfoPhase;
        private const int Stages = 4;

        public PhaserEffect()
        {
            Name = "Phaser";
            Rate = 0.5f;
            Depth = 1f;
            Feedback = 0.7f;
            WetMix = 0.5f;
            DryMix = 0.5f;
            MinFrequency = 440f;
            MaxFrequency = 1600f;

            x1 = new float[Stages];
            x2 = new float[Stages];
            y1 = new float[Stages];
            y2 = new float[Stages];
            lfoPhase = 0f;
        }

        public override float Process(float input, float time)
        {
            float lfo = (float)Math.Sin(lfoPhase * 2.0 * Math.PI);
            float freq = MinFrequency + (MaxFrequency - MinFrequency) * (lfo * 0.5f + 0.5f) * Depth;

            float w0 = 2f * (float)Math.PI * freq / 44100f;
            float Q = 1.0f;
            float alpha = (float)Math.Sin(w0) / (2f * Q);

            float output = input;

            for (int i = 0; i < Stages; i++)
            {
                float b0 = alpha;
                float b1 = 0;
                float b2 = -alpha;
                float a0 = 1f + alpha;
                float a1 = -2f * (float)Math.Cos(w0);
                float a2 = 1f - alpha;

                b0 /= a0; b1 /= a0; b2 /= a0; a1 /= a0; a2 /= a0;

                float y = b0 * input + b1 * x1[i] + b2 * x2[i] - a1 * y1[i] - a2 * y2[i];

                x2[i] = x1[i];
                x1[i] = input;
                y2[i] = y1[i];
                y1[i] = y;

                input = y;
            }

            output = input + output * Feedback;

            lfoPhase += Rate / 44100f;
            if (lfoPhase >= 1f) lfoPhase -= 1f;

            return (output * WetMix + input * DryMix) * Mix;
        }

        public override void Reset()
        {
            Array.Clear(x1, 0, Stages);
            Array.Clear(x2, 0, Stages);
            Array.Clear(y1, 0, Stages);
            Array.Clear(y2, 0, Stages);
            lfoPhase = 0f;
        }

        public override AudioEffectBase Clone()
        {
            return new PhaserEffect
            {
                Name = Name,
                IsEnabled = IsEnabled,
                Mix = Mix,
                Rate = Rate,
                Depth = Depth,
                Feedback = Feedback,
                WetMix = WetMix,
                DryMix = DryMix,
                MinFrequency = MinFrequency,
                MaxFrequency = MaxFrequency
            };
        }
    }

    public class AudioMixerAdvanced : IDisposable
    {
        public string Name { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool IsMuted { get; set; }
        public bool IsSolo { get; set; }
        public List<AudioMixerAdvanced> Children { get; set; }
        public AudioMixerAdvanced Parent { get; set; }
        public List<AudioEffectBase> Effects { get; set; }

        private float currentVolume;
        private float targetVolume;
        private float volumeTransitionSpeed;
        private Action volumeTransitionCallback;

        public AudioMixerAdvanced()
        {
            Name = "Mixer";
            Volume = 1f;
            Pitch = 1f;
            IsMuted = false;
            IsSolo = false;
            Children = new List<AudioMixerAdvanced>();
            Effects = new List<AudioEffectBase>();
            currentVolume = 1f;
            targetVolume = 1f;
            volumeTransitionSpeed = 0f;
        }

        public void AddChild(AudioMixerAdvanced child)
        {
            if (child != null && !Children.Contains(child))
            {
                Children.Add(child);
                child.Parent = this;
            }
        }

        public void RemoveChild(AudioMixerAdvanced child)
        {
            if (child != null && Children.Contains(child))
            {
                Children.Remove(child);
                child.Parent = null;
            }
        }

        public float GetOutputVolume()
        {
            if (IsMuted) return 0f;

            float volume = Volume;
            if (Parent != null)
                volume *= Parent.GetOutputVolume();

            return volume;
        }

        public float ProcessSample(float input, float time)
        {
            float output = input;

            foreach (var effect in Effects)
            {
                if (effect.IsEnabled)
                    output = effect.Process(output, time);
            }

            foreach (var child in Children)
            {
                if (child.IsSolo)
                    return output * child.GetOutputVolume();
            }

            return output * getOutputVolumeInternal();
        }

        private float getOutputVolumeInternal()
        {
            if (IsMuted) return 0f;
            float vol = currentVolume * Volume;
            if (Parent != null)
                vol *= Parent.currentVolume;
            return vol;
        }

        public void TransitionTo(float targetVol, float duration, Action callback = null)
        {
            targetVolume = targetVol;
            volumeTransitionSpeed = (targetVol - currentVolume) / duration;
            volumeTransitionCallback = callback;
        }

        public void Update(float deltaTime)
        {
            if (Math.Abs(volumeTransitionSpeed) > 0.0001f)
            {
                currentVolume += volumeTransitionSpeed * deltaTime;

                if ((volumeTransitionSpeed > 0 && currentVolume >= targetVolume) ||
                    (volumeTransitionSpeed < 0 && currentVolume <= targetVolume))
                {
                    currentVolume = targetVolume;
                    volumeTransitionSpeed = 0f;
                    volumeTransitionCallback?.Invoke();
                    volumeTransitionCallback = null;
                }
            }

            foreach (var child in Children)
            {
                child.Update(deltaTime);
            }
        }

        public void AddEffect(AudioEffectBase effect)
        {
            if (effect != null && !Effects.Contains(effect))
                Effects.Add(effect);
        }

        public void RemoveEffect(AudioEffectBase effect)
        {
            if (effect != null)
                Effects.Remove(effect);
        }

        public void Dispose()
        {
            Children.Clear();
            Effects.Clear();
            Parent = null;
        }
    }

    public class MixerSnapshot
    {
        public string Name { get; set; }
        public Dictionary<string, float> MixerVolumes { get; set; }
        public Dictionary<string, float> MixerPitches { get; set; }
        public Dictionary<string, bool> MixerMutes { get; set; }
        public Dictionary<string, Dictionary<string, float>> EffectParameters { get; set; }
        public float TransitionTime { get; set; }

        public MixerSnapshot()
        {
            Name = "Snapshot";
            MixerVolumes = new Dictionary<string, float>();
            MixerPitches = new Dictionary<string, float>();
            MixerMutes = new Dictionary<string, bool>();
            EffectParameters = new Dictionary<string, Dictionary<string, float>>();
            TransitionTime = 1f;
        }

        public void Capture(Dictionary<string, AudioMixerAdvanced> mixers)
        {
            foreach (var kvp in mixers)
            {
                MixerVolumes[kvp.Key] = kvp.Value.Volume;
                MixerPitches[kvp.Key] = kvp.Value.Pitch;
                MixerMutes[kvp.Key] = kvp.Value.IsMuted;

                foreach (var effect in kvp.Value.Effects)
                {
                    if (!EffectParameters.ContainsKey(kvp.Key))
                        EffectParameters[kvp.Key] = new Dictionary<string, float>();
                }
            }
        }

        public void Apply(Dictionary<string, AudioMixerAdvanced> mixers)
        {
            foreach (var kvp in mixers)
            {
                if (MixerVolumes.TryGetValue(kvp.Key, out var vol))
                    kvp.Value.TransitionTo(vol, TransitionTime);

                if (MixerPitches.TryGetValue(kvp.Key, out var pitch))
                    kvp.Value.Pitch = pitch;

                if (MixerMutes.TryGetValue(kvp.Key, out var mute))
                    kvp.Value.IsMuted = mute;
            }
        }
    }

    public class AudioDucker
    {
        public string Name { get; set; }
        public float Threshold { get; set; }
        public float Ratio { get; set; }
        public float AttackTime { get; set; }
        public float ReleaseTime { get; set; }
        public float TargetVolume { get; set; }
        public string[] TriggerSources { get; set; }

        private float envelope;
        private bool isDucking;

        public AudioDucker()
        {
            Name = "Ducker";
            Threshold = -20f;
            Ratio = 4f;
            AttackTime = 0.01f;
            ReleaseTime = 0.1f;
            TargetVolume = 0.2f;
            TriggerSources = Array.Empty<string>();
            envelope = 0f;
            isDucking = false;
        }

        public float Process(float input, float time)
        {
            float inputDb = 20f * (float)Math.Log10(Math.Max(0.0001f, Math.Abs(input)));

            float targetEnvelope = inputDb > Threshold ? Math.Abs(input) : 0f;
            float coeff = targetEnvelope > envelope ? AttackTime : ReleaseTime;
            envelope = envelope + coeff * (targetEnvelope - envelope);

            float gainReduction = (float)Math.Pow(envelope, 1f / Ratio);
            return input * Math.Max(TargetVolume, gainReduction);
        }
    }

    public abstract class ProceduralAudioGenerator
    {
        public bool IsPlaying { get; protected set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public int SampleRate { get; set; }

        protected double phase;
        protected double phaseIncrement;

        public event EventHandler<AudioSourceEventArgs> GenerationCompleted;

        public ProceduralAudioGenerator()
        {
            IsPlaying = false;
            Volume = 1f;
            Pitch = 1f;
            SampleRate = 44100;
            phase = 0f;
            phaseIncrement = 0f;
        }

        public abstract float GenerateSample(double time);

        public virtual float GenerateNextSample()
        {
            if (!IsPlaying) return 0f;
            float sample = GenerateSample(phase) * Volume;
            phaseIncrement = Pitch * 440.0 / SampleRate;
            phase += phaseIncrement;
            if (phase >= 1.0) phase -= 1.0;
            return sample;
        }

        public virtual float[] GenerateBuffer(int sampleCount)
        {
            var buffer = new float[sampleCount];
            for (int i = 0; i < sampleCount; i++)
            {
                buffer[i] = GenerateNextSample();
            }
            return buffer;
        }

        public void Start()
        {
            IsPlaying = true;
            phase = 0f;
        }

        public void Stop()
        {
            IsPlaying = false;
            OnGenerationCompleted();
        }

        public virtual void Reset()
        {
            phase = 0f;
            IsPlaying = false;
        }

        protected virtual void OnGenerationCompleted()
        {
            GenerationCompleted?.Invoke(this, new AudioSourceEventArgs(null));
        }
    }

    public class SynthesizerGenerator : ProceduralAudioGenerator
    {
        public OscillatorType Oscillator { get; set; }
        public float Attack { get; set; }
        public float Decay { get; set; }
        public float Sustain { get; set; }
        public float Release { get; set; }
        public float PulseWidth { get; set; }

        public enum OscillatorType
        {
            Sine,
            Square,
            Sawtooth,
            Triangle,
            Noise,
            Pulse
        }

        public SynthesizerGenerator()
        {
            Oscillator = OscillatorType.Sine;
            Attack = 0.01f;
            Decay = 0.1f;
            Sustain = 0.7f;
            Release = 0.3f;
            PulseWidth = 0.5f;
        }

        public override float GenerateSample(double time)
        {
            float env = CalculateEnvelope(time);
            float osc = GenerateOscillator(phase);

            return osc * env;
        }

        private float CalculateEnvelope(double time)
        {
            double total = Attack + Decay + Release;
            double normalized = time % 1.0;
            double attackEnd = Attack / total;
            double decayEnd = (Attack + Decay) / total;

            if (normalized < attackEnd)
            {
                return (float)(normalized / attackEnd);
            }
            else if (normalized < decayEnd)
            {
                double decayProgress = (normalized - attackEnd) / (decayEnd - attackEnd);
                return (float)(1.0 - (1.0 - Sustain) * decayProgress);
            }
            else
            {
                double releaseProgress = (normalized - decayEnd) / (1.0 - decayEnd);
                return (float)(Sustain * (1.0 - releaseProgress));
            }
        }

        private float GenerateOscillator(double phase)
        {
            return Oscillator switch
            {
                OscillatorType.Sine => (float)Math.Sin(phase * 2.0 * Math.PI),
                OscillatorType.Square => phase < 0.5 ? 1f : -1f,
                OscillatorType.Sawtooth => (float)(2.0 * phase - 1.0),
                OscillatorType.Triangle => (float)(4.0 * Math.Abs(2.0 * phase - 1.0) - 1.0),
                OscillatorType.Pulse => phase < PulseWidth ? 1f : -1f,
                OscillatorType.Noise => (float)(Random.Shared.NextDouble() * 2.0 - 1.0),
                _ => (float)Math.Sin(phase * 2.0 * Math.PI)
            };
        }
    }

    public class NoiseGenerator : ProceduralAudioGenerator
    {
        public NoiseType Type { get; set; }
        private float lastValue;
        private float lastBrownValue;

        public enum NoiseType
        {
            White,
            Pink,
            Brown
        }

        public NoiseGenerator()
        {
            Type = NoiseType.White;
            lastValue = 0f;
            lastBrownValue = 0f;
        }

        public override float GenerateSample(double time)
        {
            return Type switch
            {
                NoiseType.White => GenerateWhiteNoise(),
                NoiseType.Pink => GeneratePinkNoise(),
                NoiseType.Brown => GenerateBrownNoise(),
                _ => 0f
            };
        }

        private float GenerateWhiteNoise()
        {
            return (float)(Random.Shared.NextDouble() * 2.0 - 1.0);
        }

        private float GeneratePinkNoise()
        {
            float white = GenerateWhiteNoise();

            lastValue = (0.99886f * lastValue) + (white * 0.0555179f);
            float pink = lastValue + (white * 0.2969114f) + (white * 0.0741735f);

            lastValue = (0.963f * lastValue) + (white * 0.1921266f);
            pink += lastValue;

            return pink * 0.11f;
        }

        private float GenerateBrownNoise()
        {
            float white = GenerateWhiteNoise();
            lastBrownValue = (lastBrownValue + (0.02f * white)) / 1.02f;
            return lastBrownValue * 3.5f;
        }
    }

    public class DrumSynthesizer : ProceduralAudioGenerator
    {
        public DrumType Type { get; set; }
        public float DecayTime { get; set; }
        public float Pitch { get; set; }
        public float NoiseMix { get; set; }

        public enum DrumType
        {
            Kick,
            Snare,
            HiHat,
            Clap,
            Tom,
            Rim
        }

        private float envelope;
        private double drumPhase;

        public DrumSynthesizer()
        {
            Type = DrumType.Kick;
            DecayTime = 0.5f;
            NoiseMix = 0f;
            envelope = 0f;
            drumPhase = 0f;
        }

        public override float GenerateSample(double time)
        {
            float env = CalculateDrumEnvelope(time);

            float tone = Type switch
            {
                DrumType.Kick => GenerateKick(time),
                DrumType.Snare => GenerateSnare(time),
                DrumType.HiHat => GenerateHiHat(time),
                DrumType.Clap => GenerateClap(time),
                DrumType.Tom => GenerateTom(time),
                DrumType.Rim => GenerateRim(time),
                _ => GenerateKick(time)
            };

            float noise = (float)(Random.Shared.NextDouble() * 2.0 - 1.0);
            return (tone * (1f - NoiseMix) + noise * NoiseMix) * env;
        }

        private float CalculateDrumEnvelope(double time)
        {
            double normalized = time / DecayTime;
            return (float)Math.Exp(-normalized * 5.0);
        }

        private float GenerateKick(double time)
        {
            double freq = 150.0 * Math.Exp(-time * 30.0) + 50.0;
            drumPhase += freq / SampleRate;
            if (drumPhase > 1.0) drumPhase -= 1.0;

            float body = (float)Math.Sin(drumPhase * 2.0 * Math.PI);
            float click = (float)Math.Exp(-time * 500.0);

            return body * 0.8f + click * 0.2f;
        }

        private float GenerateSnare(double time)
        {
            double toneFreq = 200.0 * Math.Exp(-time * 10.0);
            drumPhase += toneFreq / SampleRate;
            if (drumPhase > 1.0) drumPhase -= 1.0;

            float body = (float)Math.Sin(drumPhase * 2.0 * Math.PI);
            float noise = (float)(Random.Shared.NextDouble() * 2.0 - 1.0) * (float)Math.Exp(-time * 20.0);

            return body * 0.4f + noise * 0.6f;
        }

        private float GenerateHiHat(double time)
        {
            float noise = (float)(Random.Shared.NextDouble() * 2.0 - 1.0);
            float envelope = (float)Math.Exp(-time * 50.0);

            float hp = Math.Abs(noise - lastValue);
            lastValue = noise;

            return hp * envelope;
        }

        private float GenerateClap(double time)
        {
            float noise = (float)(Random.Shared.NextDouble() * 2.0 - 1.0);
            float env1 = (float)Math.Exp(-time * 100.0);
            float env2 = (float)Math.Exp(-(time - 0.015) * 50.0);
            float env3 = (float)Math.Exp(-(time - 0.025) * 30.0);

            return noise * (env1 + env2 + env3) * 0.5f;
        }

        private float GenerateTom(double time)
        {
            double freq = 120.0 * Math.Exp(-time * 15.0) + 80.0;
            drumPhase += freq / SampleRate;
            if (drumPhase > 1.0) drumPhase -= 1.0;

            return (float)Math.Sin(drumPhase * 2.0 * Math.PI);
        }

        private float GenerateRim(double time)
        {
            float body = (float)Math.Sin(drumPhase * 2.0 * Math.PI * 1700.0);
            float click = (float)Math.Exp(-time * 1000.0);
            drumPhase += 0.001;

            return body * 0.3f + click * 0.7f;
        }

        public void Trigger()
        {
            Start();
            envelope = 1f;
        }
    }

    public class MelodyGenerator : ProceduralAudioGenerator
    {
        public int[] Scale { get; set; }
        public int BaseOctave { get; set; }
        public float NoteLength { get; set; }
        public float RestLength { get; set; }
        public MelodyStyle Style { get; set; }
        public int RootNote { get; set; }

        public enum MelodyStyle
        {
            Random,
            Arpeggio,
            Scale,
            Chord,
            Sequence
        }

        private int currentNote;
        private double noteTime;
        private Random random;
        private int[] sequence;
        private int sequenceIndex;

        private static readonly int[] MajorScale = { 0, 2, 4, 5, 7, 9, 11 };
        private static readonly int[] MinorScale = { 0, 2, 3, 5, 7, 8, 10 };
        private static readonly int[] PentatonicScale = { 0, 2, 4, 7, 9 };

        public MelodyGenerator()
        {
            Scale = MajorScale;
            BaseOctave = 4;
            NoteLength = 0.25f;
            RestLength = 0.1f;
            Style = MelodyStyle.Random;
            RootNote = 0;
            currentNote = 0;
            noteTime = 0;
            random = new Random();
            sequence = Array.Empty<int>();
            sequenceIndex = 0;
        }

        public void SetScale(bool major)
        {
            Scale = major ? MajorScale : MinorScale;
        }

        public void SetSequence(int[] notes)
        {
            sequence = notes;
            sequenceIndex = 0;
        }

        public override float GenerateSample(double time)
        {
            double noteDuration = NoteLength;
            double noteEnd = noteDuration + RestLength;

            if (noteTime >= noteEnd)
            {
                AdvanceNote();
                noteTime = 0;
            }

            float env = 1f;
            if (noteTime < noteDuration)
            {
                float t = (float)(noteTime / noteDuration);
                if (t < 0.1f)
                    env = t / 0.1f;
                else if (t > 0.8f)
                    env = (1f - t) / 0.2f;
                else
                    env = 1f;
            }
            else
            {
                env = 0f;
            }

            double freq = GetFrequency(currentNote);
            double tone = Math.Sin(phase * 2.0 * Math.PI);
            phase += freq / SampleRate;
            if (phase >= 1.0) phase -= 1.0;

            noteTime += 1.0 / SampleRate;

            return (float)tone * env * Volume;
        }

        private void AdvanceNote()
        {
            switch (Style)
            {
                case MelodyStyle.Random:
                    currentNote = random.Next(Scale.Length);
                    break;

                case MelodyStyle.Arpeggio:
                    currentNote = (currentNote + 1) % Scale.Length;
                    break;

                case MelodyStyle.Scale:
                    currentNote = (currentNote + 1) % Scale.Length;
                    if (currentNote == 0)
                        BaseOctave++;
                    break;

                case MelodyStyle.Chord:
                    currentNote = random.Next(Scale.Length);
                    break;

                case MelodyStyle.Sequence:
                    if (sequence.Length > 0)
                    {
                        currentNote = sequence[sequenceIndex];
                        sequenceIndex = (sequenceIndex + 1) % sequence.Length;
                    }
                    break;
            }
        }

        private double GetFrequency(int scaleIndex)
        {
            int noteInOctave = Scale[scaleIndex % Scale.Length];
            int octave = BaseOctave + (scaleIndex / Scale.Length);
            int midiNote = RootNote + octave * 12 + noteInOctave;
            return 440.0 * Math.Pow(2.0, (midiNote - 69) / 12.0);
        }

        public void PlayNote(int scaleIndex)
        {
            currentNote = scaleIndex;
            noteTime = 0;
        }

        public void PlayChord(int[] scaleIndices)
        {
        }
    }

    public class SpatialAudioProcessor
    {
        public IHRTF Hrtf { get; set; }
        public bool EnableHRTF { get; set; }
        public bool EnableOcclusion { get; set; }
        public bool EnableObstruction { get; set; }
        public float OcclusionAttenuation { get; set; }
        public float ObstructionAttenuation { get; set; }

        public SpatialAudioProcessor()
        {
            Hrtf = new SimpleHRTF();
            EnableHRTF = false;
            EnableOcclusion = true;
            EnableObstruction = true;
            OcclusionAttenuation = 0.5f;
            ObstructionAttenuation = 0.3f;
        }

        public float CalculateOcclusion(Vector3 sourcePosition, Vector3 listenerPosition, List<Vector3> occlusionPoints)
        {
            if (!EnableOcclusion || occlusionPoints.Count == 0)
                return 1f;

            float occlusion = 1f;
            Vector3 direction = (sourcePosition - listenerPosition).Normalized();

            foreach (var point in occlusionPoints)
            {
                float distanceToPoint = (point - listenerPosition).Length();
                Vector3 toPoint = (point - listenerPosition).Normalized();
                float dot = Vector3.Dot(direction, toPoint);

                if (dot > 0)
                {
                    float pointOcclusion = 1f - (dot * OcclusionAttenuation);
                    occlusion *= pointOcclusion;
                }
            }

            return Math.Max(0.1f, occlusion);
        }

        public float CalculateObstruction(Vector3 sourcePosition, Vector3 listenerPosition, Vector3 listenerForward)
        {
            if (!EnableObstruction)
                return 1f;

            Vector3 toSource = (sourcePosition - listenerPosition).Normalized();
            float dot = Vector3.Dot(toSource, listenerForward);

            if (dot < 0)
            {
                float obstruction = 1f + dot * ObstructionAttenuation;
                return Math.Max(0.1f, obstruction);
            }

            return 1f;
        }

        public Vector2 ApplyHRTFProcessing(float monoInput, Vector3 sourcePosition, Vector3 listenerPosition, Vector3 listenerForward)
        {
            if (!EnableHRTF)
            {
                return new Vector2(monoInput, monoInput);
            }

            Vector3 toSource = sourcePosition - listenerPosition;
            float distance = toSource.Length();
            Vector3 direction = toSource.Normalized();

            float azimuth = (float)Math.Acos(Vector3.Dot(direction, listenerForward)) * (180f / (float)Math.PI);
            float elevation = (float)Math.Asin(Vector3.Dot(direction, Vector3.Up)) * (180f / (float)Math.PI);

            float left = Hrtf.ApplyHRTF(monoInput, 0, azimuth - 30, elevation);
            float right = Hrtf.ApplyHRTF(0, monoInput, azimuth + 30, elevation);

            float distanceAttenuation = 1f / (1f + distance * 0.1f);

            return new Vector2(left * distanceAttenuation, right * distanceAttenuation);
        }
    }

    public class RoomGeometry
    {
        public Vector3 Dimensions { get; set; }
        public Vector3 Position { get; set; }
        public SurfaceMaterial[] Surfaces { get; set; }
        public float AbsorptionCoefficient { get; set; }
        public float Diffusion { get; set; }

        public enum SurfaceMaterial
        {
            Concrete,
            Brick,
            Wood,
            Glass,
            Curtain,
            Carpet,
            Metal,
            Plastic,
            Plaster,
            Marble
        }

        public RoomGeometry()
        {
            Dimensions = new Vector3(10, 3, 10);
            Position = Vector3.Zero;
            Surfaces = new SurfaceMaterial[6];
            AbsorptionCoefficient = 0.3f;
            Diffusion = 0.5f;
        }

        public float GetSurfaceAbsorption(SurfaceMaterial material)
        {
            return material switch
            {
                SurfaceMaterial.Concrete => 0.02f,
                SurfaceMaterial.Brick => 0.03f,
                SurfaceMaterial.Wood => 0.10f,
                SurfaceMaterial.Glass => 0.04f,
                SurfaceMaterial.Curtain => 0.50f,
                SurfaceMaterial.Carpet => 0.20f,
                SurfaceMaterial.Metal => 0.01f,
                SurfaceMaterial.Plastic => 0.03f,
                SurfaceMaterial.Plaster => 0.05f,
                SurfaceMaterial.Marble => 0.01f,
                _ => 0.10f
            };
        }

        public float CalculateReverbTime()
        {
            float volume = Dimensions.X * Dimensions.Y * Dimensions.Z;
            float totalAbsorption = 0f;

            for (int i = 0; i < Surfaces.Length; i++)
            {
                float surfaceArea = GetSurfaceArea(i);
                float absorption = GetSurfaceAbsorption(Surfaces[i]);
                totalAbsorption += surfaceArea * absorption;
            }

            if (totalAbsorption <= 0f)
                return 1.5f;

            return (float)(0.161 * volume / totalAbsorption);
        }

        private float GetSurfaceArea(int index)
        {
            switch (index)
            {
                case 0: return Dimensions.X * Dimensions.Z;
                case 1: return Dimensions.X * Dimensions.Z;
                case 2: return Dimensions.X * Dimensions.Y;
                case 3: return Dimensions.X * Dimensions.Y;
                case 4: return Dimensions.Y * Dimensions.Z;
                case 5: return Dimensions.Y * Dimensions.Z;
                default: return 0f;
            }
        }

        public bool IsPointInside(Vector3 point)
        {
            Vector3 halfSize = Dimensions * 0.5f;
            Vector3 localPoint = point - Position;

            return Math.Abs(localPoint.X) <= halfSize.X &&
                   Math.Abs(localPoint.Y) <= halfSize.Y &&
                   Math.Abs(localPoint.Z) <= halfSize.Z;
        }
    }

    public class ReverbZone
    {
        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public ReverbPreset Preset { get; set; }
        public float BlendDistance { get; set; }
        public bool IsActive { get; set; }

        private ReverbEffect cachedEffect;

        public ReverbZone()
        {
            Name = "ReverbZone";
            Position = Vector3.Zero;
            Radius = 10f;
            Preset = ReverbPreset.Generic;
            BlendDistance = 5f;
            IsActive = true;
            cachedEffect = ReverbEffect.FromPreset(Preset);
        }

        public float GetBlendFactor(Vector3 listenerPosition)
        {
            float distance = (listenerPosition - Position).Length();

            if (distance >= Radius)
                return 0f;

            if (distance <= Radius - BlendDistance)
                return 1f;

            return 1f - (distance - (Radius - BlendDistance)) / BlendDistance;
        }

        public ReverbEffect GetEffect()
        {
            return cachedEffect;
        }

        public void SetPreset(ReverbPreset preset)
        {
            Preset = preset;
            cachedEffect = ReverbEffect.FromPreset(preset);
        }
    }

    public class MusicPlayer : IDisposable
    {
        public string Name { get; set; }
        public MusicState State { get; private set; }
        public float Volume { get; set; }
        public float CrossfadeDuration { get; set; }
        public int CurrentLayerIndex { get; set; }
        public List<MusicLayer> Layers { get; set; }
        public BeatInfo CurrentBeat { get; private set; }
        public float BPM { get; set; }

        private float crossfadeProgress;
        private float crossfadeStartVolume;
        private float crossfadeTargetVolume;
        private bool isCrossfading;
        private double beatTime;
        private int beatCount;

        public event EventHandler<BeatInfo> BeatDetected;
        public event EventHandler<MusicLayer> LayerChanged;
        public event EventHandler MusicStarted;
        public event EventHandler MusicStopped;
        public event EventHandler MusicPaused;
        public event EventHandler MusicResumed;

        public MusicPlayer()
        {
            Name = "MusicPlayer";
            State = MusicState.Stopped;
            Volume = 1f;
            CrossfadeDuration = 2f;
            CurrentLayerIndex = 0;
            Layers = new List<MusicLayer>();
            BPM = 120f;
            crossfadeProgress = 0f;
            isCrossfading = false;
            beatTime = 0f;
            beatCount = 0;
            CurrentBeat = new BeatInfo();
        }

        public void AddLayer(MusicLayer layer)
        {
            if (layer != null)
            {
                Layers.Add(layer);
                layer.Player = this;
            }
        }

        public void RemoveLayer(MusicLayer layer)
        {
            if (layer != null)
            {
                Layers.Remove(layer);
                layer.Player = null;
            }
        }

        public void Play()
        {
            foreach (var layer in Layers)
            {
                layer.Play();
            }

            State = MusicState.Playing;
            MusicStarted?.Invoke(this, EventArgs.Empty);
        }

        public void Pause()
        {
            foreach (var layer in Layers)
            {
                layer.Pause();
            }

            State = MusicState.Paused;
            MusicPaused?.Invoke(this, EventArgs.Empty);
        }

        public void Resume()
        {
            foreach (var layer in Layers)
            {
                layer.Resume();
            }

            State = MusicState.Playing;
            MusicResumed?.Invoke(this, EventArgs.Empty);
        }

        public void Stop()
        {
            foreach (var layer in Layers)
            {
                layer.Stop();
            }

            State = MusicState.Stopped;
            MusicStopped?.Invoke(this, EventArgs.Empty);
        }

        public void CrossfadeTo(MusicPlayer newPlayer, float duration)
        {
            if (isCrossfading) return;

            isCrossfading = true;
            crossfadeProgress = 0f;
            crossfadeStartVolume = Volume;
            crossfadeTargetVolume = 0f;
            CrossfadeDuration = duration;

            State = MusicState.Transitioning;
        }

        public void SetLayerVolume(int layerIndex, float volume)
        {
            if (layerIndex >= 0 && layerIndex < Layers.Count)
            {
                Layers[layerIndex].Volume = Math.Clamp(volume, 0f, 1f);
            }
        }

        public void SelectLayer(int index)
        {
            if (index >= 0 && index < Layers.Count && index != CurrentLayerIndex)
            {
                CurrentLayerIndex = index;
                LayerChanged?.Invoke(this, Layers[index]);
            }
        }

        public void TriggerLayer(int layerIndex)
        {
            if (layerIndex >= 0 && layerIndex < Layers.Count)
            {
                Layers[layerIndex].Trigger();
            }
        }

        public void Update(float deltaTime)
        {
            if (State == MusicState.Playing)
            {
                UpdateBeatDetection(deltaTime);

                foreach (var layer in Layers)
                {
                    layer.Update(deltaTime);
                }
            }

            if (isCrossfading)
            {
                UpdateCrossfade(deltaTime);
            }
        }

        private void UpdateBeatDetection(float deltaTime)
        {
            double beatInterval = 60.0 / BPM;
            beatTime += deltaTime;

            if (beatTime >= beatInterval)
            {
                beatTime -= beatInterval;
                beatCount++;

                CurrentBeat = new BeatInfo
                {
                    BeatNumber = beatCount,
                    BeatType = GetBeatType(beatCount),
                    Time = beatTime,
                    BPM = BPM,
                    IsDownbeat = beatCount % 4 == 1
                };

                BeatDetected?.Invoke(this, CurrentBeat);
            }
        }

        private BeatType GetBeatType(int beatNumber)
        {
            if (beatNumber % 4 == 0)
                return BeatType.Whole;
            if (beatNumber % 2 == 0)
                return BeatType.Half;
            return BeatType.Quarter;
        }

        private void UpdateCrossfade(float deltaTime)
        {
            crossfadeProgress += deltaTime / CrossfadeDuration;

            if (crossfadeProgress >= 1f)
            {
                crossfadeProgress = 1f;
                isCrossfading = false;
                Volume = crossfadeTargetVolume;
            }
            else
            {
                float t = EaseInOut(crossfadeProgress);
                Volume = crossfadeStartVolume * (1f - t) + crossfadeTargetVolume * t;
            }
        }

        private float EaseInOut(float t)
        {
            return t < 0.5f ? 2f * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 2f) / 2f;
        }

        public void Dispose()
        {
            Stop();
            Layers.Clear();
            BeatDetected = null;
            LayerChanged = null;
            MusicStarted = null;
            MusicStopped = null;
            MusicPaused = null;
            MusicResumed = null;
        }
    }

    public class MusicLayer : IDisposable
    {
        public string Name { get; set; }
        public AudioClip Clip { get; set; }
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool Loop { get; set; }
        public bool IsPlaying { get; private set; }
        public bool IsTriggered { get; set; }
        public int Priority { get; set; }
        public MusicPlayer Player { get; internal set; }
        public Dictionary<string, float> Tags { get; set; }

        private AdvancedAudioSource source;

        public MusicLayer()
        {
            Name = "Layer";
            Volume = 1f;
            Pitch = 1f;
            Loop = true;
            IsPlaying = false;
            IsTriggered = false;
            Priority = 0;
            Tags = new Dictionary<string, float>();
        }

        public void Play()
        {
            if (Clip == null) return;

            source = new AdvancedAudioSource();
            source.AddClip(Clip);
            source.Volume = Volume;
            source.Pitch = Pitch;
            source.Loop = Loop;
            source.Play();

            IsPlaying = true;
        }

        public void Pause()
        {
            if (source != null)
                source.Pause();
            IsPlaying = false;
        }

        public void Resume()
        {
            if (source != null)
                source.Resume();
            IsPlaying = true;
        }

        public void Stop()
        {
            if (source != null)
            {
                source.Stop();
                source.Dispose();
                source = null;
            }
            IsPlaying = false;
        }

        public void Trigger()
        {
            IsTriggered = true;
            Play();
        }

        public void SetVolume(float volume)
        {
            Volume = Math.Clamp(volume, 0f, 1f);
            if (source != null)
                source.Volume = Volume;
        }

        public void Update(float deltaTime)
        {
            if (source != null)
                source.Update();
        }

        public void Dispose()
        {
            Stop();
            Tags.Clear();
            Player = null;
        }
    }

    public class BeatInfo
    {
        public int BeatNumber { get; set; }
        public BeatType BeatType { get; set; }
        public double Time { get; set; }
        public float BPM { get; set; }
        public bool IsDownbeat { get; set; }
        public double Period => 60.0 / BPM;
    }

    public class DynamicMusicController
    {
        public string CurrentState { get; set; }
        public Dictionary<string, MusicPlayer> StateMusic { get; set; }
        public float TransitionDuration { get; set; }
        public bool IsTransitioning { get; private set; }

        private MusicPlayer activePlayer;
        private MusicPlayer nextPlayer;
        private float transitionProgress;

        public DynamicMusicController()
        {
            CurrentState = "Default";
            StateMusic = new Dictionary<string, MusicPlayer>();
            TransitionDuration = 2f;
            IsTransitioning = false;
            transitionProgress = 0f;
        }

        public void RegisterState(string stateName, MusicPlayer player)
        {
            StateMusic[stateName] = player;
        }

        public void TransitionTo(string stateName)
        {
            if (!StateMusic.TryGetValue(stateName, out var player))
                return;

            if (activePlayer != null && activePlayer != player)
            {
                nextPlayer = player;
                nextPlayer.Volume = 0f;
                nextPlayer.Play();
                IsTransitioning = true;
                transitionProgress = 0f;
            }
            else
            {
                activePlayer = player;
                activePlayer.Play();
            }

            CurrentState = stateName;
        }

        public void Update(float deltaTime)
        {
            if (IsTransitioning && activePlayer != null && nextPlayer != null)
            {
                transitionProgress += deltaTime / TransitionDuration;

                if (transitionProgress >= 1f)
                {
                    activePlayer.Stop();
                    activePlayer = nextPlayer;
                    nextPlayer = null;
                    IsTransitioning = false;
                    transitionProgress = 0f;
                }
                else
                {
                    float t = transitionProgress;
                    activePlayer.Volume = 1f - t;
                    nextPlayer.Volume = t;
                }
            }

            activePlayer?.Update(deltaTime);
            nextPlayer?.Update(deltaTime);
        }

        public void Stop()
        {
            activePlayer?.Stop();
            nextPlayer?.Stop();
            activePlayer = null;
            nextPlayer = null;
            IsTransitioning = false;
        }
    }

    public class AudioPoolAdvanced : IDisposable
    {
        public int MaxSize { get; set; }
        public int ActiveCount => activeSources.Count;
        public int AvailableCount => availableSources.Count;

        private List<AdvancedAudioSource> activeSources;
        private List<AdvancedAudioSource> availableSources;
        private Dictionary<int, AdvancedAudioSource> allSources;

        public AudioPoolAdvanced(int maxSize = 32)
        {
            MaxSize = maxSize;
            activeSources = new List<AdvancedAudioSource>();
            availableSources = new List<AdvancedAudioSource>();
            allSources = new Dictionary<int, AdvancedAudioSource>();

            Prewarm(maxSize);
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var source = new AdvancedAudioSource { SourceId = i };
                allSources[i] = source;
                availableSources.Add(source);
            }
        }

        public AdvancedAudioSource Get()
        {
            AdvancedAudioSource source;

            if (availableSources.Count > 0)
            {
                source = availableSources[availableSources.Count - 1];
                availableSources.RemoveAt(availableSources.Count - 1);
            }
            else if (allSources.Count < MaxSize)
            {
                int newId = allSources.Count;
                source = new AdvancedAudioSource { SourceId = newId };
                allSources[newId] = source;
            }
            else
            {
                AdvancedAudioSource oldest = null;
                float oldestTime = float.MaxValue;

                foreach (var s in activeSources)
                {
                    if (s.CurrentTime < oldestTime && !s.Loop)
                    {
                        oldest = s;
                        oldestTime = (float)s.CurrentTime;
                    }
                }

                if (oldest != null)
                {
                    Return(oldest);
                    source = oldest;
                }
                else
                {
                    return null;
                }
            }

            activeSources.Add(source);
            return source;
        }

        public void Return(AdvancedAudioSource source)
        {
            if (source == null) return;

            source.Stop();
            activeSources.Remove(source);
            availableSources.Add(source);
        }

        public void Update()
        {
            for (int i = activeSources.Count - 1; i >= 0; i--)
            {
                var source = activeSources[i];
                source.Update();

                if (source.State == AudioState.Stopped && !source.Loop)
                {
                    Return(source);
                }
            }
        }

        public void ReleaseAll()
        {
            foreach (var source in activeSources.ToArray())
            {
                Return(source);
            }
        }

        public void Dispose()
        {
            ReleaseAll();
            foreach (var source in allSources.Values)
            {
                source.Dispose();
            }
            allSources.Clear();
            activeSources.Clear();
            availableSources.Clear();
        }
    }

    public class AudioBankAdvanced
    {
        public string Name { get; set; }
        public string[] Tags { get; set; }
        public Dictionary<string, AudioClip> Clips { get; set; }
        public Dictionary<string, List<string>> TagGroups { get; set; }

        public event EventHandler<AudioClip> ClipLoaded;
        public event EventHandler<string> ClipUnloaded;

        public AudioBankAdvanced()
        {
            Name = "Bank";
            Tags = Array.Empty<string>();
            Clips = new Dictionary<string, AudioClip>();
            TagGroups = new Dictionary<string, List<string>>();
        }

        public void AddClip(string key, AudioClip clip)
        {
            if (clip != null && !Clips.ContainsKey(key))
            {
                Clips[key] = clip;
                ClipLoaded?.Invoke(this, clip);
            }
        }

        public void RemoveClip(string key)
        {
            if (Clips.Remove(key))
            {
                ClipUnloaded?.Invoke(this, key);

                foreach (var group in TagGroups.Values)
                {
                    group.Remove(key);
                }
            }
        }

        public AudioClip GetClip(string key)
        {
            return Clips.TryGetValue(key, out var clip) ? clip : null;
        }

        public AudioClip GetRandomClip()
        {
            if (Clips.Count == 0) return null;
            int index = Random.Shared.Next(Clips.Count);
            return Clips.Values.ElementAt(index);
        }

        public AudioClip GetRandomClip(string tag)
        {
            if (!TagGroups.TryGetValue(tag, out var group) || group.Count == 0)
                return null;

            string key = group[Random.Shared.Next(group.Count)];
            return GetClip(key);
        }

        public void AddTag(string tag, params string[] clipKeys)
        {
            if (!TagGroups.ContainsKey(tag))
                TagGroups[tag] = new List<string>();

            foreach (var key in clipKeys)
            {
                if (Clips.ContainsKey(key) && !TagGroups[tag].Contains(key))
                {
                    TagGroups[tag].Add(key);
                }
            }
        }

        public List<string> GetClipsByTag(string tag)
        {
            return TagGroups.TryGetValue(tag, out var group) ? new List<string>(group) : new List<string>();
        }

        public void Clear()
        {
            Clips.Clear();
            TagGroups.Clear();
        }
    }

    public class AudioVisualizerData
    {
        public float[] SpectrumData { get; set; }
        public float[] WaveformData { get; set; }
        public float RMSLevel { get; set; }
        public float PeakLevel { get; set; }
        public float[] ChannelLevels { get; set; }

        public AudioVisualizerData(int spectrumSize, int channelCount = 2)
        {
            SpectrumData = new float[spectrumSize];
            WaveformData = new float[spectrumSize];
            ChannelLevels = new float[channelCount];
        }

        public void Update(float[] samples)
        {
            if (samples == null || samples.Length == 0) return;

            WaveformData = samples.Take(Math.Min(samples.Length, WaveformData.Length)).ToArray();
            RMSLevel = CalculateRMS(samples);
            PeakLevel = CalculatePeak(samples);
        }

        private float CalculateRMS(float[] samples)
        {
            if (samples.Length == 0) return 0f;

            float sum = 0f;
            foreach (var sample in samples)
            {
                sum += sample * sample;
            }

            return (float)Math.Sqrt(sum / samples.Length);
        }

        private float CalculatePeak(float[] samples)
        {
            if (samples.Length == 0) return 0f;

            float peak = 0f;
            foreach (var sample in samples)
            {
                float abs = Math.Abs(sample);
                if (abs > peak)
                    peak = abs;
            }

            return peak;
        }
    }

    public class FFTAnalyzer
    {
        public int FFTSize { get; }
        public float[] Real { get; private set; }
        public float[] Imaginary { get; private set; }
        public float[] Magnitudes { get; private set; }
        public float[] Phases { get; private set; }

        private float[] window;
        private int[] reversedBits;
        private int levels;

        public FFTAnalyzer(int fftSize = 1024)
        {
            FFTSize = fftSize;
            Real = new float[fftSize];
            Imaginary = new float[fftSize];
            Magnitudes = new float[fftSize / 2];
            Phases = new float[fftSize / 2];

            window = new float[fftSize];
            for (int i = 0; i < fftSize; i++)
            {
                window[i] = (float)(0.5 * (1.0 - Math.Cos(2.0 * Math.PI * i / (fftSize - 1))));
            }

            GenerateReversedBits();
        }

        private void GenerateReversedBits()
        {
            int bits = (int)Math.Log2(FFTSize);
            levels = bits;

            reversedBits = new int[FFTSize];
            for (int i = 0; i < FFTSize; i++)
            {
                int reversed = 0;
                for (int j = 0; j < bits; j++)
                {
                    reversed = (reversed << 1) | ((i >> j) & 1);
                }
                reversedBits[i] = reversed;
            }
        }

        public void Compute(float[] samples)
        {
            if (samples.Length < FFTSize)
            {
                Array.Clear(Real, 0, FFTSize);
                Array.Clear(Imaginary, 0, FFTSize);

                for (int i = 0; i < samples.Length; i++)
                {
                    Real[i] = samples[i] * window[i];
                }
            }
            else
            {
                for (int i = 0; i < FFTSize; i++)
                {
                    Real[i] = samples[i] * window[i];
                    Imaginary[i] = 0f;
                }
            }

            CooleyTukeyFFT();
            CalculateMagnitudesAndPhases();
        }

        private void CooleyTukeyFFT()
        {
            for (int i = 0; i < FFTSize; i++)
            {
                int j = reversedBits[i];
                if (j > i)
                {
                    (Real[i], Real[j]) = (Real[j], Real[i]);
                    (Imaginary[i], Imaginary[j]) = (Imaginary[j], Imaginary[i]);
                }
            }

            for (int size = 2; size <= FFTSize; size *= 2)
            {
                float angle = -2f * (float)Math.PI / size;

                for (int i = 0; i < FFTSize; i += size)
                {
                    for (int j = 0; j < size / 2; j++)
                    {
                        float cos = (float)Math.Cos(angle * j);
                        float sin = (float)Math.Sin(angle * j);

                        int evenIdx = i + j;
                        int oddIdx = i + j + size / 2;

                        float evenReal = Real[evenIdx];
                        float evenImag = Imaginary[evenIdx];

                        Real[oddIdx] = Real[oddIdx] * cos - Imaginary[oddIdx] * sin;
                        Imaginary[oddIdx] = Real[oddIdx] * sin + Imaginary[oddIdx] * cos;

                        Real[evenIdx] = evenReal + Real[oddIdx];
                        Imaginary[evenIdx] = evenImag + Imaginary[oddIdx];

                        Real[oddIdx] = evenReal - Real[oddIdx];
                        Imaginary[oddIdx] = evenImag - Imaginary[oddIdx];
                    }
                }
            }
        }

        private void CalculateMagnitudesAndPhases()
        {
            for (int i = 0; i < FFTSize / 2; i++)
            {
                float real = Real[i];
                float imag = Imaginary[i];

                Magnitudes[i] = (float)Math.Sqrt(real * real + imag * imag) / FFTSize;
                Phases[i] = (float)Math.Atan2(imag, real);
            }
        }

        public float GetMagnitudeAtFrequency(float frequency, float sampleRate)
        {
            int bin = (int)(frequency * FFTSize / sampleRate);
            if (bin >= 0 && bin < Magnitudes.Length)
                return Magnitudes[bin];
            return 0f;
        }

        public float GetPeakFrequency(float sampleRate)
        {
            float maxMag = 0f;
            int peakBin = 0;

            for (int i = 0; i < Magnitudes.Length; i++)
            {
                if (Magnitudes[i] > maxMag)
                {
                    maxMag = Magnitudes[i];
                    peakBin = i;
                }
            }

            return peakBin * sampleRate / FFTSize;
        }
    }

    public class BeatDetector
    {
        public float Sensitivity { get; set; }
        public float MinBPM { get; set; }
        public float MaxBPM { get; set; }
        public float ConfidenceThreshold { get; set; }

        private FFTAnalyzer fft;
        private List<float> energyHistory;
        private List<double> beatTimes;
        private int historySize;
        private float lastBeatTime;
        private float averageEnergy;

        public BeatDetector(int historySize = 43)
        {
            Sensitivity = 1.5f;
            MinBPM = 60f;
            MaxBPM = 180f;
            ConfidenceThreshold = 0.3f;

            fft = new FFTAnalyzer(1024);
            energyHistory = new List<float>(historySize);
            beatTimes = new List<double>();
            this.historySize = historySize;
            lastBeatTime = 0f;
            averageEnergy = 0f;
        }

        public BeatDetectionResult DetectBeat(float[] samples, float currentTime)
        {
            fft.Compute(samples);

            float bassEnergy = GetBassEnergy();
            float overallEnergy = GetOverallEnergy();

            while (energyHistory.Count >= historySize)
            {
                energyHistory.RemoveAt(0);
            }
            energyHistory.Add(bassEnergy);

            averageEnergy = energyHistory.Average();

            float variance = CalculateVariance();
            float threshold = averageEnergy + Sensitivity * (float)Math.Sqrt(variance);

            bool isBeat = bassEnergy > threshold && (currentTime - lastBeatTime) > (60.0 / MaxBPM);

            float bpmEstimate = 0f;
            float confidence = 0f;

            if (isBeat)
            {
                beatTimes.Add(currentTime);
                lastBeatTime = currentTime;

                if (beatTimes.Count >= 4)
                {
                    bpmEstimate = EstimateBPM();
                    confidence = CalculateConfidence();

                    if (bpmEstimate < MinBPM)
                        bpmEstimate *= 2f;
                    if (bpmEstimate > MaxBPM)
                        bpmEstimate /= 2f;
                }
            }

            return new BeatDetectionResult
            {
                IsBeat = isBeat,
                BPM = bpmEstimate,
                Confidence = confidence,
                BassEnergy = bassEnergy,
                OverallEnergy = overallEnergy,
                Threshold = threshold
            };
        }

        private float GetBassEnergy()
        {
            float energy = 0f;
            int bassBins = Math.Min(10, fft.Magnitudes.Length);

            for (int i = 0; i < bassBins; i++)
            {
                energy += fft.Magnitudes[i] * fft.Magnitudes[i];
            }

            return (float)Math.Sqrt(energy / bassBins);
        }

        private float GetOverallEnergy()
        {
            float energy = 0f;

            foreach (var mag in fft.Magnitudes)
            {
                energy += mag * mag;
            }

            return (float)Math.Sqrt(energy / fft.Magnitudes.Length);
        }

        private float CalculateVariance()
        {
            if (energyHistory.Count < 2) return 0f;

            float sum = 0f;
            foreach (var e in energyHistory)
            {
                sum += e;
            }
            float mean = sum / energyHistory.Count;

            float variance = 0f;
            foreach (var e in energyHistory)
            {
                float diff = e - mean;
                variance += diff * diff;
            }

            return variance / energyHistory.Count;
        }

        private float EstimateBPM()
        {
            if (beatTimes.Count < 4) return 0f;

            List<double> intervals = new List<double>();
            for (int i = 1; i < beatTimes.Count; i++)
            {
                intervals.Add(beatTimes[i] - beatTimes[i - 1]);
            }

            intervals.Sort();

            double medianInterval = intervals[intervals.Count / 2];
            return (float)(60.0 / medianInterval);
        }

        private float CalculateConfidence()
        {
            if (beatTimes.Count < 4) return 0f;

            float variance = CalculateVariance();
            return Math.Min(1f, (float)(1.0 / (1.0 + variance * 100)));
        }
    }

    public class BeatDetectionResult
    {
        public bool IsBeat { get; set; }
        public float BPM { get; set; }
        public float Confidence { get; set; }
        public float BassEnergy { get; set; }
        public float OverallEnergy { get; set; }
        public float Threshold { get; set; }
    }

    public class VolumeMeter
    {
        public float PeakLevel { get; private set; }
        public float RMSLevel { get; private set; }
        public float AverageLevel { get; private set; }
        public float DynamicRange { get; private set; }
        public bool IsClipping { get; private set; }

        private List<float> peakHistory;
        private int historyLength;
        private float clipThreshold;
        private int clipCount;

        public VolumeMeter(int historyLength = 100, float clipThreshold = 0.99f)
        {
            this.historyLength = historyLength;
            this.clipThreshold = clipThreshold;
            peakHistory = new List<float>(historyLength);
            PeakLevel = 0f;
            RMSLevel = 0f;
            AverageLevel = 0f;
            DynamicRange = 0f;
            IsClipping = false;
            clipCount = 0;
        }

        public void Process(float[] samples)
        {
            if (samples == null || samples.Length == 0) return;

            float maxSample = 0f;
            float sumSquares = 0f;

            foreach (var sample in samples)
            {
                float abs = Math.Abs(sample);
                if (abs > maxSample)
                    maxSample = abs;

                sumSquares += sample * sample;

                if (abs >= clipThreshold)
                {
                    IsClipping = true;
                    clipCount++;
                }
            }

            PeakLevel = maxSample;
            RMSLevel = (float)Math.Sqrt(sumSquares / samples.Length);

            while (peakHistory.Count >= historyLength)
            {
                peakHistory.RemoveAt(0);
            }
            peakHistory.Add(PeakLevel);

            AverageLevel = peakHistory.Average();
            DynamicRange = PeakLevel > 0.001f ? PeakLevel / Math.Max(0.0001f, AverageLevel) : 0f;
        }

        public float GetDecibels(float linearLevel)
        {
            return 20f * (float)Math.Log10(Math.Max(0.0001f, linearLevel));
        }

        public float GetLinear(float decibels)
        {
            return (float)Math.Pow(10, decibels / 20f);
        }

        public void Reset()
        {
            peakHistory.Clear();
            PeakLevel = 0f;
            RMSLevel = 0f;
            AverageLevel = 0f;
            DynamicRange = 0f;
            IsClipping = false;
            clipCount = 0;
        }

        public int GetClipCount() => clipCount;
    }

    public class Vector3
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public static Vector3 Zero => new Vector3(0, 0, 0);
        public static Vector3 One => new Vector3(1, 1, 1);
        public static Vector3 Up => new Vector3(0, 1, 0);
        public static Vector3 Down => new Vector3(0, -1, 0);
        public static Vector3 Forward => new Vector3(0, 0, -1);
        public static Vector3 Backward => new Vector3(0, 0, 1);
        public static Vector3 Left => new Vector3(-1, 0, 0);
        public static Vector3 Right => new Vector3(1, 0, 0);

        public Vector3() { X = 0; Y = 0; Z = 0; }
        public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }

        public float Length() => (float)Math.Sqrt(X * X + Y * Y + Z * Z);
        public float LengthSquared() => X * X + Y * Y + Z * Z;
        public Vector3 Normalized()
        {
            float len = Length();
            if (len < 0.0001f) return Zero;
            return new Vector3(X / len, Y / len, Z / len);
        }

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator *(Vector3 a, float scalar) => new Vector3(a.X * scalar, a.Y * scalar, a.Z * scalar);
        public static Vector3 operator *(float scalar, Vector3 a) => a * scalar;
        public static Vector3 operator /(Vector3 a, float scalar) => new Vector3(a.X / scalar, a.Y / scalar, a.Z / scalar);
        public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }

    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);

        public Vector2() { X = 0; Y = 0; }
        public Vector2(float x, float y) { X = x; Y = y; }

        public float Length() => (float)Math.Sqrt(X * X + Y * Y);

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator *(Vector2 a, float scalar) => new Vector2(a.X * scalar, a.Y * scalar);
    }
}

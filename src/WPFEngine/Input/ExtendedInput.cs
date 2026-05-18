using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WPFEngine.ECS;

namespace WPFEngine.Input
{
    public enum InputDevice
    {
        Keyboard,
        Mouse,
        Gamepad,
        Touch,
        Pen
    }

    public enum DeadZoneType
    {
        None,
        Axial,
        Radial,
        ScaledRadial
    }

    public class ExtendedInputManager
    {
        private static ExtendedInputManager instance;
        private Dictionary<InputDevice, DeviceState> deviceStates;
        private InputDevice primaryDevice;
        private InputDevice secondaryDevice;
        private List<InputDevice> hotSwappableDevices;
        private Dictionary<InputDevice, DateTime> lastDeviceActivity;
        private bool autoDetectDevice;
        private float deviceSwitchCooldown;
        private float deviceSwitchTimer;

        public event EventHandler<DeviceChangedEventArgs> DeviceConnected;
        public event EventHandler<DeviceChangedEventArgs> DeviceDisconnected;
        public event EventHandler<DeviceChangedEventArgs> PrimaryDeviceChanged;

        public static ExtendedInputManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new ExtendedInputManager();
                return instance;
            }
        }

        public InputDevice PrimaryDevice => primaryDevice;
        public InputDevice SecondaryDevice => secondaryDevice;

        public ExtendedInputManager()
        {
            instance = this;
            deviceStates = new Dictionary<InputDevice, DeviceState>();
            hotSwappableDevices = new List<InputDevice>();
            lastDeviceActivity = new Dictionary<InputDevice, DateTime>();
            autoDetectDevice = true;
            deviceSwitchCooldown = 0.5f;
            deviceSwitchTimer = 0f;

            foreach (InputDevice device in Enum.GetValues(typeof(InputDevice)))
            {
                deviceStates[device] = new DeviceState
                {
                    Device = device,
                    IsConnected = false,
                    IsActive = false,
                    LastUpdate = DateTime.MinValue
                };
            }

            hotSwappableDevices.Add(InputDevice.Keyboard);
            hotSwappableDevices.Add(InputDevice.Mouse);
        }

        public void Update(float deltaTime)
        {
            if (deviceSwitchTimer > 0)
            {
                deviceSwitchTimer -= deltaTime;
            }

            if (autoDetectDevice)
            {
                DetectActiveDevice();
            }
        }

        public void SetDeviceConnected(InputDevice device, bool connected)
        {
            if (deviceStates[device].IsConnected != connected)
            {
                deviceStates[device].IsConnected = connected;
                var args = new DeviceChangedEventArgs(device, connected);
                
                if (connected)
                {
                    DeviceConnected?.Invoke(this, args);
                }
                else
                {
                    DeviceDisconnected?.Invoke(this, args);
                    if (primaryDevice == device)
                    {
                        TrySwitchToNextDevice();
                    }
                }
            }
        }

        public void SetDeviceActive(InputDevice device)
        {
            if (deviceSwitchTimer > 0)
                return;

            if (deviceStates[device].IsConnected && device != primaryDevice)
            {
                primaryDevice = device;
                deviceStates[device].IsActive = true;
                deviceStates[device].LastUpdate = DateTime.Now;
                lastDeviceActivity[device] = DateTime.Now;
                deviceSwitchTimer = deviceSwitchCooldown;
                PrimaryDeviceChanged?.Invoke(this, new DeviceChangedEventArgs(device, true));
            }
        }

        public bool IsDeviceConnected(InputDevice device)
        {
            return deviceStates.TryGetValue(device, out var state) && state.IsConnected;
        }

        public bool IsDeviceActive(InputDevice device)
        {
            return deviceStates.TryGetValue(device, out var state) && state.IsActive;
        }

        public void SetAutoDetectEnabled(bool enabled)
        {
            autoDetectDevice = enabled;
        }

        public void ForceDeviceSwitch(InputDevice device)
        {
            if (deviceStates[device].IsConnected)
            {
                primaryDevice = device;
                deviceStates[device].IsActive = true;
                deviceSwitchTimer = 0f;
                PrimaryDeviceChanged?.Invoke(this, new DeviceChangedEventArgs(device, true));
            }
        }

        private void DetectActiveDevice()
        {
            InputDevice mostRecentDevice = primaryDevice;
            DateTime mostRecentTime = DateTime.MinValue;

            foreach (var kvp in lastDeviceActivity)
            {
                if (kvp.Value > mostRecentTime && deviceStates[kvp.Key].IsConnected)
                {
                    mostRecentTime = kvp.Value;
                    mostRecentDevice = kvp.Key;
                }
            }

            if (mostRecentDevice != primaryDevice && deviceSwitchTimer <= 0)
            {
                SetDeviceActive(mostRecentDevice);
            }
        }

        private void TrySwitchToNextDevice()
        {
            foreach (InputDevice device in hotSwappableDevices)
            {
                if (deviceStates[device].IsConnected)
                {
                    SetDeviceActive(device);
                    return;
                }
            }
        }

        public void RecordDeviceActivity(InputDevice device)
        {
            lastDeviceActivity[device] = DateTime.Now;
        }
    }

    public class DeviceState
    {
        public InputDevice Device { get; set; }
        public bool IsConnected { get; set; }
        public bool IsActive { get; set; }
        public DateTime LastUpdate { get; set; }
        public Dictionary<string, float> DeviceMetrics { get; set; }
    }

    public class DeviceChangedEventArgs : EventArgs
    {
        public InputDevice Device { get; }
        public bool IsConnected { get; }

        public DeviceChangedEventArgs(InputDevice device, bool connected)
        {
            Device = device;
            IsConnected = connected;
        }
    }

    public class KeyBinding
    {
        public string ActionName { get; set; }
        public List<KeyCode> Keys { get; set; }
        public InputDevice PreferredDevice { get; set; }
        public bool IsEnabled { get; set; }
        public float Priority { get; set; }

        public KeyBinding()
        {
            Keys = new List<KeyCode>();
            IsEnabled = true;
            Priority = 0f;
            PreferredDevice = InputDevice.Keyboard;
        }

        public KeyBinding(string actionName, params KeyCode[] keys)
        {
            ActionName = actionName;
            Keys = keys.ToList();
            IsEnabled = true;
            Priority = 0f;
            PreferredDevice = InputDevice.Keyboard;
        }

        public bool IsPressed(InputManager input)
        {
            if (!IsEnabled)
                return false;

            return Keys.All(k => input.GetKey(k));
        }

        public bool IsPressedThisFrame(InputManager input)
        {
            if (!IsEnabled)
                return false;

            return Keys.All(k => input.GetKeyDown(k));
        }

        public bool IsReleasedThisFrame(InputManager input)
        {
            if (!IsEnabled)
                return false;

            return Keys.All(k => input.GetKeyUp(k));
        }

        public void AddKey(KeyCode key)
        {
            if (!Keys.Contains(key))
            {
                Keys.Add(key);
            }
        }

        public void RemoveKey(KeyCode key)
        {
            Keys.Remove(key);
        }

        public void ClearKeys()
        {
            Keys.Clear();
        }
    }

    public class KeyCombo
    {
        public string Name { get; set; }
        public List<KeyCode> ModifierKeys { get; set; }
        public KeyCode PrimaryKey { get; set; }
        public bool RequireAllModifiers { get; set; }
        public float Timeout { get; set; }
        private DateTime lastKeyPressTime;
        private int currentModifierIndex;

        public KeyCombo()
        {
            ModifierKeys = new List<KeyCode>();
            RequireAllModifiers = true;
            Timeout = 1.0f;
            currentModifierIndex = 0;
        }

        public KeyCombo(string name, KeyCode primaryKey, params KeyCode[] modifiers)
        {
            Name = name;
            PrimaryKey = primaryKey;
            ModifierKeys = modifiers.ToList();
            RequireAllModifiers = true;
            Timeout = 1.0f;
            currentModifierIndex = 0;
        }

        public bool Check(InputManager input)
        {
            var now = DateTime.Now;
            if ((now - lastKeyPressTime).TotalSeconds > Timeout)
            {
                Reset();
                return false;
            }

            if (RequireAllModifiers)
            {
                bool allModifiersPressed = ModifierKeys.All(m => input.GetKey(m));
                bool primaryPressed = input.GetKeyDown(PrimaryKey);
                return allModifiersPressed && primaryPressed;
            }
            else
            {
                bool anyModifierPressed = ModifierKeys.Any(m => input.GetKey(m));
                bool primaryPressed = input.GetKeyDown(PrimaryKey);
                return anyModifierPressed && primaryPressed;
            }
        }

        public bool CheckSequence(InputManager input)
        {
            bool allModifiersHeld = ModifierKeys.All(m => input.GetKey(m));
            bool primaryJustPressed = input.GetKeyDown(PrimaryKey);
            
            if (primaryJustPressed)
            {
                lastKeyPressTime = DateTime.Now;
            }

            return allModifiersHeld && primaryJustPressed;
        }

        public void Reset()
        {
            currentModifierIndex = 0;
            lastKeyPressTime = DateTime.MinValue;
        }

        public string GetDisplayString()
        {
            var parts = new List<string>();
            foreach (var modifier in ModifierKeys)
            {
                parts.Add(GetKeyName(modifier));
            }
            parts.Add(GetKeyName(PrimaryKey));
            return string.Join("+", parts);
        }

        private string GetKeyName(KeyCode key)
        {
            return key switch
            {
                KeyCode.Ctrl => "Ctrl",
                KeyCode.Shift => "Shift",
                KeyCode.Alt => "Alt",
                _ => key.ToString()
            };
        }
    }

    public class InputBuffer
    {
        private Queue<BufferedInput> buffer;
        private int maxBufferSize;
        private float defaultTimeout;

        public InputBuffer(int maxSize = 32, float timeout = 2.0f)
        {
            maxBufferSize = maxSize;
            defaultTimeout = timeout;
            buffer = new Queue<BufferedInput>(maxSize);
        }

        public void AddInput(KeyCode key, float timestamp)
        {
            if (buffer.Count >= maxBufferSize)
            {
                buffer.Dequeue();
            }

            buffer.Enqueue(new BufferedInput
            {
                Key = key,
                Timestamp = timestamp,
                Device = InputDevice.Keyboard
            });
        }

        public void AddInput(KeyCode key, float timestamp, InputDevice device)
        {
            if (buffer.Count >= maxBufferSize)
            {
                buffer.Dequeue();
            }

            buffer.Enqueue(new BufferedInput
            {
                Key = key,
                Timestamp = timestamp,
                Device = device
            });
        }

        public void AddGamepadInput(GamepadButton button, float timestamp)
        {
            if (buffer.Count >= maxBufferSize)
            {
                buffer.Dequeue();
            }

            buffer.Enqueue(new BufferedInput
            {
                GamepadButton = button,
                Timestamp = timestamp,
                Device = InputDevice.Gamepad
            });
        }

        public bool HasInputs()
        {
            CleanupExpired();
            return buffer.Count > 0;
        }

        public BufferedInput[] GetRecentInputs(int count)
        {
            CleanupExpired();
            return buffer.TakeLast(Math.Min(count, buffer.Count)).ToArray();
        }

        public BufferedInput[] GetInputsSince(float timestamp)
        {
            CleanupExpired();
            return buffer.Where(i => i.Timestamp >= timestamp).ToArray();
        }

        public bool ContainsSequence(KeyCode[] sequence, float maxTimeBetween = 0.5f)
        {
            CleanupExpired();
            var inputs = buffer.ToArray();
            
            if (inputs.Length < sequence.Length)
                return false;

            int sequenceIndex = 0;
            for (int i = 0; i < inputs.Length && sequenceIndex < sequence.Length; i++)
            {
                if (inputs[i].Key == sequence[sequenceIndex])
                {
                    if (sequenceIndex > 0)
                    {
                        var timeDiff = inputs[i].Timestamp - inputs[i - 1].Timestamp;
                        if (timeDiff > maxTimeBetween)
                        {
                            sequenceIndex = 0;
                            continue;
                        }
                    }
                    sequenceIndex++;
                }
            }

            return sequenceIndex == sequence.Length;
        }

        public void Clear()
        {
            buffer.Clear();
        }

        private void CleanupExpired()
        {
            var cutoffTime = Time.realtimeSinceStartup - defaultTimeout;
            while (buffer.Count > 0 && buffer.Peek().Timestamp < cutoffTime)
            {
                buffer.Dequeue();
            }
        }

        public int Count => buffer.Count;
    }

    public class BufferedInput
    {
        public KeyCode Key { get; set; }
        public GamepadButton? GamepadButton { get; set; }
        public float Timestamp { get; set; }
        public InputDevice Device { get; set; }
    }

    public class KeySequence
    {
        public string Name { get; set; }
        public List<KeyCode> Sequence { get; set; }
        public float MaxTimeBetweenInputs { get; set; }
        public float CompletionWindow { get; set; }
        public bool AllowRepeats { get; set; }
        private int currentIndex;
        private float lastInputTime;
        private bool isActive;

        public event EventHandler<KeySequenceEventArgs> SequenceCompleted;
        public event EventHandler<KeySequenceEventArgs> SequenceProgress;

        public KeySequence()
        {
            Sequence = new List<KeyCode>();
            MaxTimeBetweenInputs = 0.5f;
            CompletionWindow = 0.1f;
            AllowRepeats = false;
            currentIndex = 0;
            isActive = false;
        }

        public KeySequence(string name, float maxTimeBetween, params KeyCode[] sequence)
        {
            Name = name;
            Sequence = sequence.ToList();
            MaxTimeBetweenInputs = maxTimeBetween;
            CompletionWindow = 0.1f;
            AllowRepeats = false;
            currentIndex = 0;
            isActive = false;
        }

        public bool Update(InputManager input, float currentTime)
        {
            if (Sequence.Count == 0)
                return false;

            if (currentIndex >= Sequence.Count)
            {
                if (currentTime - lastInputTime < CompletionWindow)
                {
                    return true;
                }
                Reset();
                return false;
            }

            var expectedKey = Sequence[currentIndex];
            if (input.GetKeyDown(expectedKey))
            {
                if (currentIndex > 0)
                {
                    var timeSinceLastInput = currentTime - lastInputTime;
                    if (timeSinceLastInput > MaxTimeBetweenInputs)
                    {
                        Reset();
                        if (expectedKey == Sequence[0])
                        {
                            currentIndex = 1;
                            lastInputTime = currentTime;
                            isActive = true;
                            SequenceProgress?.Invoke(this, new KeySequenceEventArgs(Name, currentIndex, Sequence.Count));
                        }
                        return false;
                    }
                }

                currentIndex++;
                lastInputTime = currentTime;
                isActive = true;
                SequenceProgress?.Invoke(this, new KeySequenceEventArgs(Name, currentIndex, Sequence.Count));

                if (currentIndex >= Sequence.Count)
                {
                    SequenceCompleted?.Invoke(this, new KeySequenceEventArgs(Name, currentIndex, Sequence.Count));
                    return true;
                }
            }
            else if (!AllowRepeats)
            {
                for (int i = currentIndex; i < Sequence.Count; i++)
                {
                    if (input.GetKeyDown(Sequence[i]))
                    {
                        if (i == 0 || i == currentIndex)
                        {
                            if (i != currentIndex)
                            {
                                Reset();
                            }
                            currentIndex = i + 1;
                            lastInputTime = currentTime;
                            isActive = true;
                            SequenceProgress?.Invoke(this, new KeySequenceEventArgs(Name, currentIndex, Sequence.Count));
                            break;
                        }
                    }
                }
            }

            if (isActive && currentTime - lastInputTime > MaxTimeBetweenInputs)
            {
                Reset();
            }

            return false;
        }

        public void Reset()
        {
            currentIndex = 0;
            isActive = false;
        }

        public float GetProgress()
        {
            if (Sequence.Count == 0)
                return 0f;
            return (float)currentIndex / Sequence.Count;
        }

        public bool IsActive => isActive;
    }

    public class KeySequenceEventArgs : EventArgs
    {
        public string SequenceName { get; }
        public int CurrentIndex { get; }
        public int TotalLength { get; }

        public KeySequenceEventArgs(string name, int current, int total)
        {
            SequenceName = name;
            CurrentIndex = current;
            TotalLength = total;
        }
    }

    public class MouseCursor
    {
        private static MouseCursor instance;
        private Cursor currentCursor;
        private Dictionary<string, Cursor> customCursors;
        private Vector2 hotspot;
        private bool isVisible;
        private bool isLocked;
        private float visibilityAlpha;
        private Vector2 targetPosition;
        private Vector2 smoothedPosition;
        private float smoothingFactor;

        public static MouseCursor Instance
        {
            get
            {
                if (instance == null)
                    instance = new MouseCursor();
                return instance;
            }
        }

        public Vector2 Position
        {
            get => smoothedPosition;
            set
            {
                targetPosition = value;
                if (!isLocked)
                {
                    smoothedPosition = value;
                }
            }
        }

        public Vector2 Hotspot
        {
            get => hotspot;
            set => hotspot = value;
        }

        public bool IsVisible
        {
            get => isVisible;
            set => isVisible = value;
        }

        public bool IsLocked
        {
            get => isLocked;
            set => isLocked = value;
        }

        public float VisibilityAlpha
        {
            get => visibilityAlpha;
            set => visibilityAlpha = Math.Clamp(value, 0f, 1f);
        }

        public MouseCursor()
        {
            customCursors = new Dictionary<string, Cursor>();
            isVisible = true;
            isLocked = false;
            visibilityAlpha = 1f;
            smoothingFactor = 1f;
            smoothedPosition = Vector2.Zero;
            targetPosition = Vector2.Zero;
        }

        public void Update(float deltaTime)
        {
            if (!isLocked)
            {
                smoothedPosition = Vector2.Lerp(smoothedPosition, targetPosition, smoothingFactor * deltaTime * 60f);
            }
        }

        public void SetCursor(string cursorName)
        {
            if (customCursors.TryGetValue(cursorName, out var cursor))
            {
                currentCursor = cursor;
            }
        }

        public void RegisterCustomCursor(string name, Cursor cursor)
        {
            customCursors[name] = cursor;
        }

        public Cursor GetCursor(string name)
        {
            return customCursors.TryGetValue(name, out var cursor) ? cursor : null;
        }

        public void SetSmoothing(float factor)
        {
            smoothingFactor = Math.Clamp(factor, 0f, 10f);
        }

        public void ForcePosition(Vector2 position)
        {
            smoothedPosition = position;
            targetPosition = position;
        }
    }

    public class MouseDelta
    {
        public Vector2 RawDelta { get; private set; }
        public Vector2 SmoothedDelta { get; private set; }
        public Vector2 AccumulatedDelta { get; private set; }
        public float SmoothingFactor { get; set; }
        public bool UseSmoothing { get; set; }
        private Vector2 lastPosition;

        public MouseDelta()
        {
            RawDelta = Vector2.Zero;
            SmoothedDelta = Vector2.Zero;
            AccumulatedDelta = Vector2.Zero;
            SmoothingFactor = 0.5f;
            UseSmoothing = true;
            lastPosition = Vector2.Zero;
        }

        public void Update(Vector2 currentPosition, float deltaTime)
        {
            RawDelta = currentPosition - lastPosition;
            lastPosition = currentPosition;

            if (UseSmoothing)
            {
                SmoothedDelta = Vector2.Lerp(SmoothedDelta, RawDelta, SmoothingFactor * deltaTime * 60f);
            }
            else
            {
                SmoothedDelta = RawDelta;
            }

            AccumulatedDelta += SmoothedDelta;
        }

        public void Reset()
        {
            RawDelta = Vector2.Zero;
            SmoothedDelta = Vector2.Zero;
        }

        public void ResetAccumulated()
        {
            AccumulatedDelta = Vector2.Zero;
        }
    }

    public class MouseScrollDelta
    {
        public float X { get; private set; }
        public float Y { get; private set; }
        public float AccumulatedX { get; private set; }
        public float AccumulatedY { get; private set; }
        public float Sensitivity { get; set; }

        public MouseScrollDelta()
        {
            X = 0f;
            Y = 0f;
            AccumulatedX = 0f;
            AccumulatedY = 0f;
            Sensitivity = 1f;
        }

        public void AddScroll(float delta)
        {
            Y = delta * Sensitivity;
            AccumulatedY += Y;
        }

        public void AddHorizontalScroll(float delta)
        {
            X = delta * Sensitivity;
            AccumulatedX += X;
        }

        public void Reset()
        {
            X = 0f;
            Y = 0f;
        }

        public void ResetAccumulated()
        {
            AccumulatedX = 0f;
            AccumulatedY = 0f;
        }

        public float GetTotalScroll()
        {
            return AccumulatedY;
        }
    }

    public class CursorLock
    {
        private bool isLocked;
        private Vector2 lockPosition;
        private Vector2 unlockPosition;
        private bool confineToWindow;
        private IntPtr windowHandle;

        public bool IsLocked => isLocked;
        public Vector2 LockPosition => lockPosition;

        public CursorLock()
        {
            isLocked = false;
            confineToWindow = true;
        }

        public void Lock()
        {
            isLocked = true;
        }

        public void Unlock()
        {
            isLocked = false;
        }

        public void LockToPosition(Vector2 position)
        {
            lockPosition = position;
            isLocked = true;
        }

        public void SetUnlockPosition(Vector2 position)
        {
            unlockPosition = position;
        }

        public void Toggle()
        {
            if (isLocked)
                Unlock();
            else
                Lock();
        }
    }

    public class GamepadManager
    {
        private static GamepadManager instance;
        private GamepadState[] gamepads;
        private int maxGamepads;
        private int activeGamepadIndex;
        private List<int> connectedGamepadIndices;

        public event EventHandler<GamepadConnectionEventArgs> GamepadConnected;
        public event EventHandler<GamepadConnectionEventArgs> GamepadDisconnected;

        public static GamepadManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new GamepadManager();
                return instance;
            }
        }

        public int ActiveGamepadIndex
        {
            get => activeGamepadIndex;
            set => activeGamepadIndex = Math.Clamp(value, 0, maxGamepads - 1);
        }

        public GamepadManager(int maxControllers = 4)
        {
            maxGamepads = maxControllers;
            gamepads = new GamepadState[maxGamepads];
            connectedGamepadIndices = new List<int>();
            activeGamepadIndex = 0;

            for (int i = 0; i < maxGamepads; i++)
            {
                gamepads[i] = new GamepadState(i);
            }
        }

        public void Update(float deltaTime)
        {
            foreach (var gamepad in gamepads)
            {
                if (gamepad.IsConnected)
                {
                    gamepad.Update(deltaTime);
                }
            }
        }

        public void SetGamepadConnected(int index, bool connected)
        {
            if (index >= 0 && index < maxGamepads)
            {
                bool wasConnected = gamepads[index].IsConnected;
                gamepads[index].IsConnected = connected;

                if (connected && !wasConnected)
                {
                    connectedGamepadIndices.Add(index);
                    GamepadConnected?.Invoke(this, new GamepadConnectionEventArgs(index, true));
                }
                else if (!connected && wasConnected)
                {
                    connectedGamepadIndices.Remove(index);
                    GamepadDisconnected?.Invoke(this, new GamepadConnectionEventArgs(index, false));
                    if (activeGamepadIndex == index && connectedGamepadIndices.Count > 0)
                    {
                        activeGamepadIndex = connectedGamepadIndices[0];
                    }
                }
            }
        }

        public GamepadState GetGamepad(int index)
        {
            if (index >= 0 && index < maxGamepads)
                return gamepads[index];
            return null;
        }

        public GamepadState GetActiveGamepad()
        {
            return GetGamepad(activeGamepadIndex);
        }

        public int GetConnectedGamepadCount()
        {
            return connectedGamepadIndices.Count;
        }

        public int[] GetConnectedGamepadIndices()
        {
            return connectedGamepadIndices.ToArray();
        }

        public void SetActiveGamepad(int index)
        {
            if (index >= 0 && index < maxGamepads && gamepads[index].IsConnected)
            {
                activeGamepadIndex = index;
            }
        }

        public void CycleGamepad()
        {
            if (connectedGamepadIndices.Count <= 1)
                return;

            int currentIndex = connectedGamepadIndices.IndexOf(activeGamepadIndex);
            int nextIndex = (currentIndex + 1) % connectedGamepadIndices.Count;
            activeGamepadIndex = connectedGamepadIndices[nextIndex];
        }
    }

    public class GamepadConnectionEventArgs : EventArgs
    {
        public int GamepadIndex { get; }
        public bool IsConnected { get; }

        public GamepadConnectionEventArgs(int index, bool connected)
        {
            GamepadIndex = index;
            IsConnected = connected;
        }
    }

    public class GamepadState
    {
        public int Index { get; }
        public bool IsConnected { get; set; }
        public string Name { get; set; }
        public DeadZoneConfig DeadZone { get; set; }

        public Vector2 LeftStick { get; private set; }
        public Vector2 RightStick { get; private set; }
        public float LeftTrigger { get; private set; }
        public float RightTrigger { get; private set; }

        private Dictionary<GamepadButton, bool> buttons;
        private Dictionary<GamepadButton, bool> buttonsPrevious;
        private Dictionary<GamepadButton, bool> buttonsPressedThisFrame;
        private Dictionary<GamepadButton, bool> buttonsReleasedThisFrame;

        private float vibrationLeft;
        private float vibrationRight;
        private float vibrationDuration;
        private float vibrationTimer;
        private bool isVibrating;

        public GamepadState(int index)
        {
            Index = index;
            DeadZone = new DeadZoneConfig();
            buttons = new Dictionary<GamepadButton, bool>();
            buttonsPrevious = new Dictionary<GamepadButton, bool>();
            buttonsPressedThisFrame = new Dictionary<GamepadButton, bool>();
            buttonsReleasedThisFrame = new Dictionary<GamepadButton, bool>();

            foreach (GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
            {
                buttons[button] = false;
                buttonsPrevious[button] = false;
                buttonsPressedThisFrame[button] = false;
                buttonsReleasedThisFrame[button] = false;
            }

            LeftStick = Vector2.Zero;
            RightStick = Vector2.Zero;
            LeftTrigger = 0f;
            RightTrigger = 0f;
        }

        public void Update(float deltaTime)
        {
            foreach (GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
            {
                buttonsPrevious[button] = buttons[button];
            }

            if (isVibrating)
            {
                vibrationTimer += deltaTime;
                if (vibrationTimer >= vibrationDuration)
                {
                    StopVibration();
                }
            }
        }

        public void SetButtonState(GamepadButton button, bool pressed)
        {
            if (buttons.ContainsKey(button))
            {
                buttons[button] = pressed;
                buttonsPressedThisFrame[button] = pressed && !buttonsPrevious[button];
                buttonsReleasedThisFrame[button] = !pressed && buttonsPrevious[button];
            }
        }

        public void SetLeftStick(Vector2 position)
        {
            LeftStick = ApplyDeadZone(position, DeadZone.LeftStick);
        }

        public void SetRightStick(Vector2 position)
        {
            RightStick = ApplyDeadZone(position, DeadZone.RightStick);
        }

        public void SetLeftTrigger(float value)
        {
            LeftTrigger = ApplyDeadZoneScalar(value, DeadZone.Triggers);
        }

        public void SetRightTrigger(float value)
        {
            RightTrigger = ApplyDeadZoneScalar(value, DeadZone.Triggers);
        }

        private Vector2 ApplyDeadZone(Vector2 input, DeadZoneType type)
        {
            return type switch
            {
                DeadZoneType.Axial => ApplyAxialDeadZone(input),
                DeadZoneType.Radial => ApplyRadialDeadZone(input),
                DeadZoneType.ScaledRadial => ApplyScaledRadialDeadZone(input),
                _ => input
            };
        }

        private Vector2 ApplyAxialDeadZone(Vector2 input)
        {
            float deadZoneValue = DeadZone.Value;
            return new Vector2(
                Math.Abs(input.X) < deadZoneValue ? 0f : input.X,
                Math.Abs(input.Y) < deadZoneValue ? 0f : input.Y
            );
        }

        private Vector2 ApplyRadialDeadZone(Vector2 input)
        {
            float magnitude = input.Magnitude;
            float deadZoneValue = DeadZone.Value;

            if (magnitude < deadZoneValue)
                return Vector2.Zero;

            float normalizedMagnitude = (magnitude - deadZoneValue) / (1f - deadZoneValue);
            return input.Normalized * normalizedMagnitude;
        }

        private Vector2 ApplyScaledRadialDeadZone(Vector2 input)
        {
            float magnitude = input.Magnitude;
            float deadZoneValue = DeadZone.Value;

            if (magnitude < deadZoneValue)
                return Vector2.Zero;

            float scaledMagnitude = (magnitude - deadZoneValue) / (1f - deadZoneValue);
            float scaleFactor = scaledMagnitude * (1f + DeadZone.ScaleFactor * (1f - scaledMagnitude));
            return input.Normalized * Math.Min(scaleFactor, 1f);
        }

        private float ApplyDeadZoneScalar(float input, DeadZoneType type)
        {
            float deadZoneValue = DeadZone.Value;
            
            if (type == DeadZoneType.Radial || type == DeadZoneType.ScaledRadial)
            {
                if (Math.Abs(input) < deadZoneValue)
                    return 0f;
                return (input - Math.Sign(input) * deadZoneValue) / (1f - deadZoneValue);
            }
            else if (type == DeadZoneType.Axial)
            {
                return Math.Abs(input) < deadZoneValue ? 0f : input;
            }
            
            return input;
        }

        public bool GetButton(GamepadButton button)
        {
            return buttons.TryGetValue(button, out var pressed) && pressed;
        }

        public bool GetButtonDown(GamepadButton button)
        {
            return buttonsPressedThisFrame.TryGetValue(button, out var pressed) && pressed;
        }

        public bool GetButtonUp(GamepadButton button)
        {
            return buttonsReleasedThisFrame.TryGetValue(button, out var released) && released;
        }

        public void StartVibration(float left, float right, float duration)
        {
            vibrationLeft = Math.Clamp(left, 0f, 1f);
            vibrationRight = Math.Clamp(right, 0f, 1f);
            vibrationDuration = duration;
            vibrationTimer = 0f;
            isVibrating = true;
        }

        public void StopVibration()
        {
            vibrationLeft = 0f;
            vibrationRight = 0f;
            vibrationTimer = 0f;
            isVibrating = false;
        }

        public float GetLeftVibration() => vibrationLeft;
        public float GetRightVibration() => vibrationRight;
        public bool IsVibrating => isVibrating;
    }

    public class DeadZoneConfig
    {
        public DeadZoneType LeftStick { get; set; }
        public DeadZoneType RightStick { get; set; }
        public DeadZoneType Triggers { get; set; }
        public float Value { get; set; }
        public float ScaleFactor { get; set; }

        public DeadZoneConfig()
        {
            LeftStick = DeadZoneType.Radial;
            RightStick = DeadZoneType.Radial;
            Triggers = DeadZoneType.Radial;
            Value = 0.125f;
            ScaleFactor = 0.1f;
        }

        public void SetAllTypes(DeadZoneType type)
        {
            LeftStick = type;
            RightStick = type;
            Triggers = type;
        }

        public void SetAllValues(float value)
        {
            Value = Math.Clamp(value, 0f, 1f);
        }
    }

    public class TouchManager
    {
        private static TouchManager instance;
        private Dictionary<int, TouchInfo> activeTouches;
        private int maxTouches;
        private GestureRecognizer gestureRecognizer;
        private List<TouchEvent> touchEvents;

        public event EventHandler<TouchEvent> TouchBegan;
        public event EventHandler<TouchEvent> TouchMoved;
        public event EventHandler<TouchEvent> TouchEnded;
        public event EventHandler<TouchEvent> TouchCanceled;

        public static TouchManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new TouchManager();
                return instance;
            }
        }

        public int ActiveTouchCount => activeTouches.Count;
        public int MaxTouches => maxTouches;
        public GestureRecognizer GestureRecognizer => gestureRecognizer;

        public TouchManager(int maxTouchPoints = 10)
        {
            maxTouches = maxTouchPoints;
            activeTouches = new Dictionary<int, TouchInfo>();
            gestureRecognizer = new GestureRecognizer();
            touchEvents = new List<TouchEvent>();
        }

        public void Update(float deltaTime)
        {
            gestureRecognizer.Update(deltaTime);
            
            foreach (var touch in activeTouches.Values)
            {
                touch.Update(deltaTime);
            }

            ProcessTouchEvents();
        }

        public void BeginTouch(int touchId, Vector2 position)
        {
            if (activeTouches.Count >= maxTouches)
                return;

            var touch = new TouchInfo
            {
                TouchId = touchId,
                Position = position,
                StartPosition = position,
                Delta = Vector2.Zero,
                PreviousPosition = position,
                Phase = TouchPhase.Began,
                StartTime = Time.realtimeSinceStartup
            };

            activeTouches[touchId] = touch;
            gestureRecognizer.OnTouchBegan(touch);

            touchEvents.Add(new TouchEvent(touchId, TouchPhase.Began, position));
            TouchBegan?.Invoke(this, touchEvents.Last());
        }

        public void UpdateTouch(int touchId, Vector2 position)
        {
            if (!activeTouches.TryGetValue(touchId, out var touch))
                return;

            touch.PreviousPosition = touch.Position;
            touch.Delta = position - touch.Position;
            touch.Position = position;
            touch.TotalDelta += touch.Delta;
            touch.Phase = TouchPhase.Moved;

            gestureRecognizer.OnTouchMoved(touch);

            touchEvents.Add(new TouchEvent(touchId, TouchPhase.Moved, position) { Delta = touch.Delta });
            TouchMoved?.Invoke(this, touchEvents.Last());
        }

        public void EndTouch(int touchId)
        {
            if (!activeTouches.TryGetValue(touchId, out var touch))
                return;

            touch.Phase = TouchPhase.Ended;
            gestureRecognizer.OnTouchEnded(touch);

            touchEvents.Add(new TouchEvent(touchId, TouchPhase.Ended, touch.Position));
            TouchEnded?.Invoke(this, touchEvents.Last());

            activeTouches.Remove(touchId);
        }

        public void CancelTouch(int touchId)
        {
            if (!activeTouches.TryGetValue(touchId, out var touch))
                return;

            touch.Phase = TouchPhase.Canceled;
            gestureRecognizer.OnTouchCanceled(touch);

            touchEvents.Add(new TouchEvent(touchId, TouchPhase.Canceled, touch.Position));
            TouchCanceled?.Invoke(this, touchEvents.Last());

            activeTouches.Remove(touchId);
        }

        public void CancelAllTouches()
        {
            foreach (var touchId in activeTouches.Keys.ToList())
            {
                CancelTouch(touchId);
            }
        }

        public TouchInfo GetTouch(int touchId)
        {
            return activeTouches.TryGetValue(touchId, out var touch) ? touch : null;
        }

        public TouchInfo[] GetAllTouches()
        {
            return activeTouches.Values.ToArray();
        }

        public TouchInfo GetTouchByIndex(int index)
        {
            if (index >= 0 && index < activeTouches.Count)
                return activeTouches.Values.ElementAt(index);
            return null;
        }

        private void ProcessTouchEvents()
        {
            touchEvents.Clear();
        }

        public Vector2 GetAverageTouchPosition()
        {
            if (activeTouches.Count == 0)
                return Vector2.Zero;

            Vector2 sum = Vector2.Zero;
            foreach (var touch in activeTouches.Values)
            {
                sum += touch.Position;
            }
            return sum / activeTouches.Count;
        }

        public Vector2 GetTouchCentroid()
        {
            return GetAverageTouchPosition();
        }
    }

    public class TouchInfo
    {
        public int TouchId { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 PreviousPosition { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 Delta { get; set; }
        public Vector2 TotalDelta { get; set; }
        public TouchPhase Phase { get; set; }
        public float StartTime { get; set; }
        public float Duration => Time.realtimeSinceStartup - StartTime;
        public float PressedDuration { get; set; }

        public TouchInfo()
        {
            TotalDelta = Vector2.Zero;
        }

        public void Update(float deltaTime)
        {
            if (Phase == TouchPhase.Moved || Phase == TouchPhase.Stationary)
            {
                PressedDuration += deltaTime;
            }
        }

        public float GetDistanceFromStart()
        {
            return (Position - StartPosition).Magnitude;
        }

        public float GetDistance()
        {
            return Delta.Magnitude;
        }

        public Vector2 GetDirection()
        {
            return Delta.Normalized;
        }

        public Vector2 GetPositionRelativeToStart()
        {
            return Position - StartPosition;
        }
    }

    public class TouchEvent : EventArgs
    {
        public int TouchId { get; }
        public TouchPhase Phase { get; }
        public Vector2 Position { get; }
        public Vector2 Delta { get; set; }

        public TouchEvent(int touchId, TouchPhase phase, Vector2 position)
        {
            TouchId = touchId;
            Phase = phase;
            Position = position;
        }
    }

    public class GestureRecognizer
    {
        private List<RecognizedGesture> recognizedGestures;
        private float tapTimeThreshold;
        private float doubleTapTimeThreshold;
        private float holdTimeThreshold;
        private float swipeThresholdDistance;
        private float pinchThreshold;
        private float panThreshold;
        private float tapPositionThreshold;
        private int tapCount;
        private float lastTapTime;

        public event EventHandler<TapGestureEventArgs> TapRecognized;
        public event EventHandler<DoubleTapGestureEventArgs> DoubleTapRecognized;
        public event EventHandler<HoldGestureEventArgs> HoldRecognized;
        public event EventHandler<SwipeGestureEventArgs> SwipeRecognized;
        public event EventHandler<PinchGestureEventArgs> PinchRecognized;
        public event EventHandler<PanGestureEventArgs> PanRecognized;

        public GestureRecognizer()
        {
            recognizedGestures = new List<RecognizedGesture>();
            tapTimeThreshold = 0.2f;
            doubleTapTimeThreshold = 0.3f;
            holdTimeThreshold = 0.5f;
            swipeThresholdDistance = 50f;
            pinchThreshold = 0.1f;
            panThreshold = 10f;
            tapPositionThreshold = 20f;
        }

        public void Update(float deltaTime)
        {
            for (int i = recognizedGestures.Count - 1; i >= 0; i--)
            {
                var gesture = recognizedGestures[i];
                gesture.Duration += deltaTime;

                if (gesture.Type == GestureType.Hold && gesture.Duration >= holdTimeThreshold)
                {
                    HoldRecognized?.Invoke(this, new HoldGestureEventArgs(gesture.Center, gesture.Duration));
                }
            }
        }

        public void OnTouchBegan(TouchInfo touch)
        {
            recognizedGestures.Add(new RecognizedGesture
            {
                Type = GestureType.None,
                TouchId = touch.TouchId,
                StartPosition = touch.Position,
                Center = touch.Position,
                StartTime = touch.StartTime,
                Duration = 0f
            });
        }

        public void OnTouchMoved(TouchInfo touch)
        {
            var gesture = recognizedGestures.FirstOrDefault(g => g.TouchId == touch.TouchId);
            if (gesture == null)
                return;

            var delta = touch.Position - gesture.StartPosition;
            float distance = delta.Magnitude;

            if (distance > tapPositionThreshold)
            {
                gesture.Type = GestureType.Pan;
                PanRecognized?.Invoke(this, new PanGestureEventArgs(touch.Position, touch.Delta, gesture.Center));
            }
        }

        public void OnTouchEnded(TouchInfo touch)
        {
            var gesture = recognizedGestures.FirstOrDefault(g => g.TouchId == touch.TouchId);
            if (gesture == null)
                return;

            float duration = touch.StartTime > 0 ? Time.realtimeSinceStartup - gesture.StartTime : 0f;
            var delta = touch.Position - gesture.StartPosition;
            float distance = delta.Magnitude;

            if (distance < tapPositionThreshold && duration < tapTimeThreshold)
            {
                var now = Time.realtimeSinceStartup;
                if (now - lastTapTime < doubleTapTimeThreshold)
                {
                    tapCount++;
                    DoubleTapRecognized?.Invoke(this, new DoubleTapGestureEventArgs(touch.Position, tapCount));
                }
                else
                {
                    tapCount = 1;
                    TapRecognized?.Invoke(this, new TapGestureEventArgs(touch.Position, tapCount));
                }
                lastTapTime = now;
            }
            else if (distance >= swipeThresholdDistance)
            {
                var direction = GetSwipeDirection(delta);
                SwipeRecognized?.Invoke(this, new SwipeGestureEventArgs(gesture.StartPosition, direction, distance));
            }
            else if (gesture.Type == GestureType.Hold || duration >= holdTimeThreshold)
            {
                HoldRecognized?.Invoke(this, new HoldGestureEventArgs(touch.Position, duration));
            }

            recognizedGestures.Remove(gesture);
        }

        public void OnTouchCanceled(TouchInfo touch)
        {
            recognizedGestures.RemoveAll(g => g.TouchId == touch.TouchId);
        }

        public void OnMultiTouch(List<TouchInfo> touches)
        {
            if (touches.Count < 2)
                return;

            var touch1 = touches[0];
            var touch2 = touches[1];
            var currentDistance = (touch2.Position - touch1.Position).Magnitude;
            var previousDistance = (touch2.PreviousPosition - touch1.PreviousPosition).Magnitude;
            var scale = currentDistance / previousDistance;

            if (Math.Abs(scale - 1f) > pinchThreshold)
            {
                var center = (touch1.Position + touch2.Position) / 2f;
                PinchRecognized?.Invoke(this, new PinchGestureEventArgs(center, scale));
            }
        }

        private SwipeDirection GetSwipeDirection(Vector2 delta)
        {
            float angle = MathF.Atan2(delta.Y, delta.X) * 180f / MathF.PI;
            
            if (angle >= -45f && angle < 45f)
                return SwipeDirection.Right;
            if (angle >= 45f && angle < 135f)
                return SwipeDirection.Up;
            if (angle >= -135f && angle < -45f)
                return SwipeDirection.Down;
            return SwipeDirection.Left;
        }

        public void SetTapTimeThreshold(float seconds)
        {
            tapTimeThreshold = seconds;
        }

        public void SetDoubleTapTimeThreshold(float seconds)
        {
            doubleTapTimeThreshold = seconds;
        }

        public void SetHoldTimeThreshold(float seconds)
        {
            holdTimeThreshold = seconds;
        }

        public void SetSwipeThreshold(float distance)
        {
            swipeThresholdDistance = distance;
        }
    }

    public enum GestureType
    {
        None,
        Tap,
        DoubleTap,
        Hold,
        Swipe,
        Pinch,
        Pan
    }

    public enum SwipeDirection
    {
        Left,
        Right,
        Up,
        Down
    }

    public class RecognizedGesture
    {
        public GestureType Type { get; set; }
        public int TouchId { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 Center { get; set; }
        public float StartTime { get; set; }
        public float Duration { get; set; }
        public float Scale { get; set; }
    }

    public class TapGestureEventArgs : EventArgs
    {
        public Vector2 Position { get; }
        public int TapCount { get; }

        public TapGestureEventArgs(Vector2 position, int tapCount)
        {
            Position = position;
            TapCount = tapCount;
        }
    }

    public class DoubleTapGestureEventArgs : EventArgs
    {
        public Vector2 Position { get; }
        public int TapCount { get; }

        public DoubleTapGestureEventArgs(Vector2 position, int tapCount)
        {
            Position = position;
            TapCount = tapCount;
        }
    }

    public class HoldGestureEventArgs : EventArgs
    {
        public Vector2 Position { get; }
        public float Duration { get; }

        public HoldGestureEventArgs(Vector2 position, float duration)
        {
            Position = position;
            Duration = duration;
        }
    }

    public class SwipeGestureEventArgs : EventArgs
    {
        public Vector2 StartPosition { get; }
        public SwipeDirection Direction { get; }
        public float Distance { get; }

        public SwipeGestureEventArgs(Vector2 start, SwipeDirection direction, float distance)
        {
            StartPosition = start;
            Direction = direction;
            Distance = distance;
        }
    }

    public class PinchGestureEventArgs : EventArgs
    {
        public Vector2 Center { get; }
        public float Scale { get; }

        public PinchGestureEventArgs(Vector2 center, float scale)
        {
            Center = center;
            Scale = scale;
        }
    }

    public class PanGestureEventArgs : EventArgs
    {
        public Vector2 Position { get; }
        public Vector2 Delta { get; }
        public Vector2 TotalDelta { get; }

        public PanGestureEventArgs(Vector2 position, Vector2 delta, Vector2 total)
        {
            Position = position;
            Delta = delta;
            TotalDelta = total;
        }
    }

    public class InputEvent
    {
        public string ActionName { get; set; }
        public InputDevice Device { get; set; }
        public KeyCode Key { get; set; }
        public GamepadButton? GamepadButton { get; set; }
        public Vector2 MousePosition { get; set; }
        public Vector2 Delta { get; set; }
        public float Value { get; set; }
        public InputEventType Type { get; set; }
        public float Timestamp { get; set; }

        public InputEvent()
        {
            Timestamp = Time.realtimeSinceStartup;
        }

        public InputEvent(string actionName, InputEventType type)
        {
            ActionName = actionName;
            Type = type;
            Timestamp = Time.realtimeSinceStartup;
        }

        public bool IsButtonEvent()
        {
            return Type == InputEventType.ButtonDown || Type == InputEventType.ButtonUp;
        }

        public bool IsAxisEvent()
        {
            return Type == InputEventType.AxisChanged;
        }

        public bool IsMouseEvent()
        {
            return Device == InputDevice.Mouse;
        }

        public bool IsKeyboardEvent()
        {
            return Device == InputDevice.Keyboard;
        }

        public bool IsGamepadEvent()
        {
            return Device == InputDevice.Gamepad;
        }
    }

    public enum InputEventType
    {
        ButtonDown,
        ButtonUp,
        AxisChanged,
        MouseMoved,
        MouseDown,
        MouseUp,
        TouchBegan,
        TouchMoved,
        TouchEnded,
        GestureRecognized
    }

    public class InputEventQueue
    {
        private Queue<InputEvent> eventQueue;
        private int maxQueueSize;
        private Dictionary<string, List<Action<InputEvent>>> eventHandlers;
        private bool isProcessing;

        public int Count => eventQueue.Count;
        public bool IsProcessing => isProcessing;

        public InputEventQueue(int maxSize = 256)
        {
            maxQueueSize = maxSize;
            eventQueue = new Queue<InputEvent>(maxSize);
            eventHandlers = new Dictionary<string, List<Action<InputEvent>>>();
            isProcessing = false;
        }

        public void Enqueue(InputEvent inputEvent)
        {
            if (eventQueue.Count >= maxQueueSize)
            {
                eventQueue.Dequeue();
            }
            eventQueue.Enqueue(inputEvent);

            if (eventHandlers.TryGetValue(inputEvent.ActionName, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler(inputEvent);
                }
            }
        }

        public InputEvent Dequeue()
        {
            return eventQueue.Dequeue();
        }

        public InputEvent Peek()
        {
            return eventQueue.Peek();
        }

        public void ProcessEvents()
        {
            if (isProcessing)
                return;

            isProcessing = true;
            while (eventQueue.Count > 0)
            {
                var inputEvent = eventQueue.Dequeue();
                ProcessEvent(inputEvent);
            }
            isProcessing = false;
        }

        private void ProcessEvent(InputEvent inputEvent)
        {
        }

        public void RegisterHandler(string actionName, Action<InputEvent> handler)
        {
            if (!eventHandlers.ContainsKey(actionName))
            {
                eventHandlers[actionName] = new List<Action<InputEvent>>();
            }
            eventHandlers[actionName].Add(handler);
        }

        public void UnregisterHandler(string actionName, Action<InputEvent> handler)
        {
            if (eventHandlers.TryGetValue(actionName, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public void Clear()
        {
            eventQueue.Clear();
        }

        public InputEvent[] GetEventsByType(InputEventType type)
        {
            return eventQueue.Where(e => e.Type == type).ToArray();
        }

        public InputEvent[] GetEventsByAction(string actionName)
        {
            return eventQueue.Where(e => e.ActionName == actionName).ToArray();
        }
    }

    public class InputActionSystem
    {
        private Dictionary<string, InputActionBinding> actions;
        private List<string> activeActions;

        public event EventHandler<ActionEventArgs> ActionStarted;
        public event EventHandler<ActionEventArgs> ActionPerformed;
        public event EventHandler<ActionEventArgs> ActionCanceled;

        public InputActionSystem()
        {
            actions = new Dictionary<string, InputActionBinding>();
            activeActions = new List<string>();
        }

        public void RegisterAction(InputActionBinding binding)
        {
            actions[binding.Name] = binding;
            binding.OnStarted += HandleActionStarted;
            binding.OnCanceled += HandleActionCanceled;
        }

        public void UnregisterAction(string name)
        {
            if (actions.TryGetValue(name, out var binding))
            {
                binding.OnStarted -= HandleActionStarted;
                binding.OnCanceled -= HandleActionCanceled;
                actions.Remove(name);
            }
        }

        public InputActionBinding GetAction(string name)
        {
            return actions.TryGetValue(name, out var binding) ? binding : null;
        }

        public void Update(InputManager input)
        {
            foreach (var action in actions.Values)
            {
                action.Update(input);
            }

            for (int i = activeActions.Count - 1; i >= 0; i--)
            {
                var actionName = activeActions[i];
                if (!actions.TryGetValue(actionName, out var action) || !action.IsPressed)
                {
                    activeActions.RemoveAt(i);
                    ActionCanceled?.Invoke(this, new ActionEventArgs(actionName));
                }
            }
        }

        private void HandleActionStarted(object sender, string actionName)
        {
            if (!activeActions.Contains(actionName))
            {
                activeActions.Add(actionName);
                ActionStarted?.Invoke(this, new ActionEventArgs(actionName));
                ActionPerformed?.Invoke(this, new ActionEventArgs(actionName));
            }
        }

        private void HandleActionCanceled(object sender, string actionName)
        {
            activeActions.Remove(actionName);
            ActionCanceled?.Invoke(this, new ActionEventArgs(actionName));
        }

        public bool IsActionActive(string name)
        {
            return activeActions.Contains(name);
        }

        public string[] GetActiveActions()
        {
            return activeActions.ToArray();
        }
    }

    public class InputActionBinding
    {
        public string Name { get; set; }
        public List<KeyBinding> KeyBindings { get; set; }
        public GamepadButton? GamepadButton { get; set; }
        public InputAxis Axis { get; set; }
        public bool RequiresHold { get; set; }
        public float HoldTime { get; set; }
        public float RepeatDelay { get; set; }
        public bool IsPressed { get; private set; }
        public float PressedTime { get; private set; }

        public event EventHandler<string> OnStarted;
        public event EventHandler<string> OnCanceled;
        public event EventHandler<string> OnUpdated;

        public InputActionBinding()
        {
            KeyBindings = new List<KeyBinding>();
            RequiresHold = false;
            HoldTime = 0f;
            RepeatDelay = 0f;
        }

        public void Update(InputManager input)
        {
            bool anyPressed = false;

            foreach (var binding in KeyBindings)
            {
                if (binding.IsPressed(input))
                {
                    anyPressed = true;
                    break;
                }
            }

            if (GamepadButton.HasValue)
            {
                anyPressed = anyPressed || input.GetGamepadButton(GamepadButton.Value);
            }

            if (Axis != null)
            {
                anyPressed = anyPressed || Math.Abs(input.GetAxis(Axis.Type)) > 0.1f;
            }

            bool wasPressed = IsPressed;
            IsPressed = anyPressed;

            if (IsPressed && !wasPressed)
            {
                PressedTime = 0f;
                OnStarted?.Invoke(this, Name);
            }
            else if (IsPressed)
            {
                PressedTime += Time.deltaTime;
                OnUpdated?.Invoke(this, Name);
            }
            else if (wasPressed)
            {
                OnCanceled?.Invoke(this, Name);
                PressedTime = 0f;
            }
        }

        public void AddKeyBinding(KeyCode key)
        {
            KeyBindings.Add(new KeyBinding(Name, key));
        }

        public void AddKeyBinding(params KeyCode[] keys)
        {
            KeyBindings.Add(new KeyBinding(Name, keys));
        }

        public void AddGamepadButton(GamepadButton button)
        {
            GamepadButton = button;
        }
    }

    public class ActionEventArgs : EventArgs
    {
        public string ActionName { get; }

        public ActionEventArgs(string name)
        {
            ActionName = name;
        }
    }

    public class AxisInput
    {
        public string Name { get; set; }
        public InputAxis Type { get; set; }
        public float Sensitivity { get; set; }
        public float DeadZone { get; set; }
        public float Gravity { get; set; }
        public bool Snap { get; set; }
        public float CurrentValue { get; private set; }
        public float RawValue { get; private set; }
        public bool UseKeyboard { get; set; }
        public bool UseGamepad { get; set; }
        public KeyCode PositiveKey { get; set; }
        public KeyCode NegativeKey { get; set; }
        public GamepadButton? PositiveGamepadButton { get; set; }
        public GamepadButton? NegativeGamepadButton { get; set; }

        private float keyboardInput;

        public AxisInput(string name, InputAxis type)
        {
            Name = name;
            Type = type;
            Sensitivity = 1f;
            DeadZone = 0f;
            Gravity = 3f;
            Snap = false;
            UseKeyboard = true;
            UseGamepad = true;
        }

        public void Update(InputManager input)
        {
            RawValue = CalculateRawValue(input);
            
            if (Math.Abs(RawValue) > DeadZone)
            {
                CurrentValue = Mathf.MoveTowards(CurrentValue, RawValue, Sensitivity * Time.deltaTime);
            }
            else
            {
                if (Snap && Math.Abs(CurrentValue) < 0.1f)
                {
                    CurrentValue = 0f;
                }
                else
                {
                    CurrentValue = Mathf.MoveTowards(CurrentValue, 0f, Gravity * Time.deltaTime);
                }
            }
        }

        private float CalculateRawValue(InputManager input)
        {
            float value = 0f;

            if (UseKeyboard)
            {
                if (PositiveKey != 0 && input.GetKey(PositiveKey))
                    value += 1f;
                if (NegativeKey != 0 && input.GetKey(NegativeKey))
                    value -= 1f;
            }

            if (UseGamepad && Type != null)
            {
                float gamepadValue = input.GetAxis(Type);
                if (Math.Abs(gamepadValue) > Math.Abs(value))
                    value = gamepadValue;
            }

            if (PositiveGamepadButton.HasValue && input.GetGamepadButton(PositiveGamepadButton.Value))
                value += 1f;
            if (NegativeGamepadButton.HasValue && input.GetGamepadButton(NegativeGamepadButton.Value))
                value -= 1f;

            return Math.Clamp(value, -1f, 1f);
        }

        public float GetValue()
        {
            return CurrentValue;
        }

        public float GetRawValue()
        {
            return RawValue;
        }
    }

    public class VirtualJoystick
    {
        public string Name { get; set; }
        public Vector2 Position { get; private set; }
        public Vector2 RawPosition { get; private set; }
        public float DeadZone { get; set; }
        public float Sensitivity { get; set; }
        public bool UseKeyboard { get; set; }
        public bool UseGamepad { get; set; }
        public int GamepadIndex { get; set; }
        public bool InvertX { get; set; }
        public bool InvertY { get; set; }

        public VirtualJoystick(string name)
        {
            Name = name;
            DeadZone = 0.1f;
            Sensitivity = 1f;
            UseKeyboard = true;
            UseGamepad = true;
            GamepadIndex = 0;
            InvertX = false;
            InvertY = false;
        }

        public void Update(InputManager input)
        {
            float x = 0f;
            float y = 0f;

            if (UseKeyboard)
            {
                if (input.GetKey(KeyCode.D) || input.GetKey(KeyCode.RightArrow))
                    x += 1f;
                if (input.GetKey(KeyCode.A) || input.GetKey(KeyCode.LeftArrow))
                    x -= 1f;
                if (input.GetKey(KeyCode.W) || input.GetKey(KeyCode.UpArrow))
                    y += 1f;
                if (input.GetKey(KeyCode.S) || input.GetKey(KeyCode.DownArrow))
                    y -= 1f;
            }

            if (UseGamepad)
            {
                var gamepad = GamepadManager.Instance.GetGamepad(GamepadIndex);
                if (gamepad != null && gamepad.IsConnected)
                {
                    x = gamepad.LeftStick.X;
                    y = gamepad.LeftStick.Y;
                }
            }

            RawPosition = new Vector2(InvertX ? -x : x, InvertY ? -y : y);

            float magnitude = RawPosition.Magnitude;
            if (magnitude < DeadZone)
            {
                Position = Vector2.Zero;
            }
            else
            {
                float adjustedMagnitude = (magnitude - DeadZone) / (1f - DeadZone);
                Position = RawPosition.Normalized * adjustedMagnitude * Sensitivity;
            }
        }

        public float GetHorizontal()
        {
            return Position.X;
        }

        public float GetVertical()
        {
            return Position.Y;
        }

        public Vector2 GetDirection()
        {
            return Position.Magnitude > 0f ? Position.Normalized : Vector2.Zero;
        }

        public float GetMagnitude()
        {
            return Position.Magnitude;
        }

        public bool IsActive()
        {
            return Position.Magnitude > 0f;
        }
    }

    public class InputHintsDisplay
    {
        private static InputHintsDisplay instance;
        private Dictionary<string, string> hints;
        private InputDevice currentInputMethod;
        private bool showHints;
        private float opacity;
        private Vector2 position;
        private float scale;

        public static InputHintsDisplay Instance
        {
            get
            {
                if (instance == null)
                    instance = new InputHintsDisplay();
                return instance;
            }
        }

        public InputDevice CurrentInputMethod
        {
            get => currentInputMethod;
            set
            {
                if (currentInputMethod != value)
                {
                    currentInputMethod = value;
                    UpdateHintsForInputMethod();
                }
            }
        }

        public bool ShowHints
        {
            get => showHints;
            set => showHints = value;
        }

        public float Opacity
        {
            get => opacity;
            set => opacity = Math.Clamp(value, 0f, 1f);
        }

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public float Scale
        {
            get => scale;
            set => scale = Math.Max(0.1f, value);
        }

        public InputHintsDisplay()
        {
            hints = new Dictionary<string, string>();
            currentInputMethod = InputDevice.Keyboard;
            showHints = true;
            opacity = 1f;
            position = new Vector2(20f, 20f);
            scale = 1f;
            InitializeDefaultHints();
        }

        private void InitializeDefaultHints()
        {
            hints["Keyboard_Jump"] = "Space";
            hints["Keyboard_MoveLeft"] = "A / Left Arrow";
            hints["Keyboard_MoveRight"] = "D / Right Arrow";
            hints["Keyboard_Fire"] = "F / Left Mouse";
            hints["Keyboard_Reload"] = "R";
            hints["Keyboard_Pause"] = "Escape";

            hints["Gamepad_Jump"] = "A Button";
            hints["Gamepad_MoveLeft"] = "Left Stick";
            hints["Gamepad_MoveRight"] = "Left Stick";
            hints["Gamepad_Fire"] = "Right Trigger";
            hints["Gamepad_Reload"] = "X Button";
            hints["Gamepad_Pause"] = "Start";

            hints["Touch_Jump"] = "Jump Button";
            hints["Touch_MoveLeft"] = "Left Side";
            hints["Touch_MoveRight"] = "Left Side";
            hints["Touch_Fire"] = "Right Side / Tap";
            hints["Touch_Reload"] = "Reload Button";
            hints["Touch_Pause"] = "Menu Button";
        }

        public void SetHint(string actionName, string hint)
        {
            hints[$"{currentInputMethod}_{actionName}"] = hint;
        }

        public string GetHint(string actionName)
        {
            string key = $"{currentInputMethod}_{actionName}";
            if (hints.TryGetValue(key, out var hint))
                return hint;

            return hints.TryGetValue($"Keyboard_{actionName}", out var fallback) ? fallback : actionName;
        }

        public void SetHintForInputMethod(InputDevice method, string actionName, string hint)
        {
            hints[$"{method}_{actionName}"] = hint;
        }

        private void UpdateHintsForInputMethod()
        {
        }

        public void AutoDetectInputMethod(InputManager input)
        {
            if (input.GetKeyDown(KeyCode.Space) || input.GetKeyDown(KeyCode.A) || 
                input.GetKeyDown(KeyCode.D) || input.GetKeyDown(KeyCode.W) || input.GetKeyDown(KeyCode.S))
            {
                CurrentInputMethod = InputDevice.Keyboard;
            }
            else if (input.GetMouseButton(0) || input.GetMouseButton(1) || input.GetMouseButton(2))
            {
                CurrentInputMethod = InputDevice.Mouse;
            }
            else
            {
                var gamepad = GamepadManager.Instance.GetActiveGamepad();
                if (gamepad != null && gamepad.IsConnected)
                {
                    CurrentInputMethod = InputDevice.Gamepad;
                }
            }
        }

        public void Update(InputManager input)
        {
            if (input.GetKeyDown(KeyCode.F1))
            {
                showHints = !showHints;
            }
        }

        public string[] GetAllHints()
        {
            var result = new List<string>();
            foreach (var kvp in hints)
            {
                if (kvp.Key.StartsWith(currentInputMethod.ToString()))
                {
                    result.Add($"{kvp.Key.Replace($"{currentInputMethod}_", "")}: {kvp.Value}");
                }
            }
            return result.ToArray();
        }
    }

    public class InputRemapping
    {
        private Dictionary<string, InputRemapBinding> remappedBindings;
        private List<RemapPreset> presets;
        private int activePresetIndex;
        private RemapPreset activePreset;

        public event EventHandler<RemapChangedEventArgs> BindingChanged;
        public event EventHandler<PresetChangedEventArgs> PresetChanged;

        public InputRemapping()
        {
            remappedBindings = new Dictionary<string, InputRemapBinding>();
            presets = new List<RemapPreset>();
            activePresetIndex = -1;
        }

        public void SetBinding(string actionName, InputRemapBinding binding)
        {
            remappedBindings[actionName] = binding;
            BindingChanged?.Invoke(this, new RemapChangedEventArgs(actionName, binding));
        }

        public InputRemapBinding GetBinding(string actionName)
        {
            return remappedBindings.TryGetValue(actionName, out var binding) ? binding : null;
        }

        public void AddPreset(RemapPreset preset)
        {
            presets.Add(preset);
        }

        public void RemovePreset(string presetName)
        {
            presets.RemoveAll(p => p.Name == presetName);
        }

        public void ApplyPreset(string presetName)
        {
            var preset = presets.FirstOrDefault(p => p.Name == presetName);
            if (preset != null)
            {
                ApplyPreset(preset);
            }
        }

        public void ApplyPreset(RemapPreset preset)
        {
            activePreset = preset;
            activePresetIndex = presets.IndexOf(preset);
            remappedBindings.Clear();

            foreach (var binding in preset.Bindings)
            {
                remappedBindings[binding.ActionName] = binding;
            }

            PresetChanged?.Invoke(this, new PresetChangedEventArgs(preset.Name));
        }

        public RemapPreset CreatePresetFromCurrent(string name)
        {
            var preset = new RemapPreset
            {
                Name = name,
                Bindings = remappedBindings.Values.ToList()
            };
            presets.Add(preset);
            return preset;
        }

        public void ResetToDefaults()
        {
            remappedBindings.Clear();
            PresetChanged?.Invoke(this, new PresetChangedEventArgs("Default"));
        }

        public RemapPreset[] GetPresets()
        {
            return presets.ToArray();
        }

        public int ActivePresetIndex => activePresetIndex;
        public string ActivePresetName => activePreset?.Name;
    }

    public class InputRemapBinding
    {
        public string ActionName { get; set; }
        public InputDevice Device { get; set; }
        public KeyCode PrimaryKey { get; set; }
        public KeyCode SecondaryKey { get; set; }
        public GamepadButton? PrimaryGamepadButton { get; set; }
        public GamepadButton? SecondaryGamepadButton { get; set; }
        public int KeyCodeIndex { get; set; }

        public InputRemapBinding()
        {
            KeyCodeIndex = 0;
        }

        public InputRemapBinding(string actionName, KeyCode key)
        {
            ActionName = actionName;
            PrimaryKey = key;
            KeyCodeIndex = 0;
            Device = InputDevice.Keyboard;
        }

        public InputRemapBinding(string actionName, GamepadButton button)
        {
            ActionName = actionName;
            PrimaryGamepadButton = button;
            KeyCodeIndex = 0;
            Device = InputDevice.Gamepad;
        }

        public bool IsPressed(InputManager input)
        {
            if (Device == InputDevice.Keyboard)
            {
                if (PrimaryKey != 0 && input.GetKey(PrimaryKey))
                    return true;
                if (SecondaryKey != 0 && input.GetKey(SecondaryKey))
                    return true;
            }
            else if (Device == InputDevice.Gamepad)
            {
                if (PrimaryGamepadButton.HasValue && input.GetGamepadButton(PrimaryGamepadButton.Value))
                    return true;
                if (SecondaryGamepadButton.HasValue && input.GetGamepadButton(SecondaryGamepadButton.Value))
                    return true;
            }
            return false;
        }

        public bool IsPressedThisFrame(InputManager input)
        {
            if (Device == InputDevice.Keyboard)
            {
                if (PrimaryKey != 0 && input.GetKeyDown(PrimaryKey))
                    return true;
                if (SecondaryKey != 0 && input.GetKeyDown(SecondaryKey))
                    return true;
            }
            else if (Device == InputDevice.Gamepad)
            {
                if (PrimaryGamepadButton.HasValue && input.GetGamepadButtonDown(PrimaryGamepadButton.Value))
                    return true;
                if (SecondaryGamepadButton.HasValue && input.GetGamepadButtonDown(SecondaryGamepadButton.Value))
                    return true;
            }
            return false;
        }

        public string GetDisplayString()
        {
            if (Device == InputDevice.Keyboard)
            {
                if (SecondaryKey != 0)
                    return $"{PrimaryKey} / {SecondaryKey}";
                return PrimaryKey.ToString();
            }
            else if (Device == InputDevice.Gamepad)
            {
                if (SecondaryGamepadButton.HasValue)
                    return $"{PrimaryGamepadButton} / {SecondaryGamepadButton}";
                return PrimaryGamepadButton?.ToString() ?? "None";
            }
            return "Unbound";
        }

        public void SetPrimaryKey(KeyCode key)
        {
            PrimaryKey = key;
            Device = InputDevice.Keyboard;
        }

        public void SetSecondaryKey(KeyCode key)
        {
            SecondaryKey = key;
        }

        public void SetPrimaryGamepadButton(GamepadButton button)
        {
            PrimaryGamepadButton = button;
            Device = InputDevice.Gamepad;
        }

        public void SetSecondaryGamepadButton(GamepadButton button)
        {
            SecondaryGamepadButton = button;
        }
    }

    public class RemapPreset
    {
        public string Name { get; set; }
        public List<InputRemapBinding> Bindings { get; set; }
        public string Description { get; set; }
        public bool IsBuiltIn { get; set; }

        public RemapPreset()
        {
            Bindings = new List<InputRemapBinding>();
            IsBuiltIn = false;
        }

        public RemapPreset(string name)
        {
            Name = name;
            Bindings = new List<InputRemapBinding>();
            IsBuiltIn = false;
        }
    }

    public class RemapPresetFactory
    {
        public static RemapPreset CreateFPSPreset()
        {
            var preset = new RemapPreset("FPS")
            {
                Description = "First Person Shooter controls",
                IsBuiltIn = true
            };

            preset.Bindings.Add(new InputRemapBinding("MoveForward", KeyCode.W));
            preset.Bindings.Add(new InputRemapBinding("MoveForward", GamepadButton.LeftStick));

            preset.Bindings.Add(new InputRemapBinding("MoveBackward", KeyCode.S));
            preset.Bindings.Add(new InputRemapBinding("MoveBackward", GamepadButton.LeftStick));

            preset.Bindings.Add(new InputRemapBinding("StrafeLeft", KeyCode.A));
            preset.Bindings.Add(new InputRemapBinding("StrafeRight", KeyCode.D));

            preset.Bindings.Add(new InputRemapBinding("Jump", KeyCode.Space));
            preset.Bindings.Add(new InputRemapBinding("Jump", GamepadButton.A));

            preset.Bindings.Add(new InputRemapBinding("Fire", KeyCode.MouseLeftButton));
            preset.Bindings.Add(new InputRemapBinding("Fire", GamepadButton.RightTrigger));

            preset.Bindings.Add(new InputRemapBinding("Aim", KeyCode.MouseRightButton));
            preset.Bindings.Add(new InputRemapBinding("Aim", GamepadButton.LeftTrigger));

            preset.Bindings.Add(new InputRemapBinding("Reload", KeyCode.R));
            preset.Bindings.Add(new InputRemapBinding("Reload", GamepadButton.X));

            preset.Bindings.Add(new InputRemapBinding("Sprint", KeyCode.LeftShift));
            preset.Bindings.Add(new InputRemapBinding("Sprint", GamepadButton.LeftBumper));

            return preset;
        }

        public static RemapPreset CreatePlatformerPreset()
        {
            var preset = new RemapPreset("Platformer")
            {
                Description = "2D Platformer controls",
                IsBuiltIn = true
            };

            preset.Bindings.Add(new InputRemapBinding("MoveLeft", KeyCode.A));
            preset.Bindings.Add(new InputRemapBinding("MoveLeft", GamepadButton.DPadLeft));

            preset.Bindings.Add(new InputRemapBinding("MoveRight", KeyCode.D));
            preset.Bindings.Add(new InputRemapBinding("MoveRight", GamepadButton.DPadRight));

            preset.Bindings.Add(new InputRemapBinding("Jump", KeyCode.Space));
            preset.Bindings.Add(new InputRemapBinding("Jump", GamepadButton.A));

            preset.Bindings.Add(new InputRemapBinding("DoubleJump", KeyCode.Space));
            preset.Bindings.Add(new InputRemapBinding("DoubleJump", GamepadButton.B));

            preset.Bindings.Add(new InputRemapBinding("Crouch", KeyCode.S));
            preset.Bindings.Add(new InputRemapBinding("Crouch", GamepadButton.DPadDown));

            preset.Bindings.Add(new InputRemapBinding("Attack", KeyCode.K));
            preset.Bindings.Add(new InputRemapBinding("Attack", GamepadButton.X));

            return preset;
        }

        public static RemapPreset CreateRacingPreset()
        {
            var preset = new RemapPreset("Racing")
            {
                Description = "Racing game controls",
                IsBuiltIn = true
            };

            preset.Bindings.Add(new InputRemapBinding("Accelerate", KeyCode.W));
            preset.Bindings.Add(new InputRemapBinding("Accelerate", GamepadButton.RightTrigger));

            preset.Bindings.Add(new InputRemapBinding("Brake", KeyCode.S));
            preset.Bindings.Add(new InputRemapBinding("Brake", GamepadButton.LeftTrigger));

            preset.Bindings.Add(new InputRemapBinding("SteerLeft", KeyCode.A));
            preset.Bindings.Add(new InputRemapBinding("SteerLeft", GamepadButton.DPadLeft));

            preset.Bindings.Add(new InputRemapBinding("SteerRight", KeyCode.D));
            preset.Bindings.Add(new InputRemapBinding("SteerRight", GamepadButton.DPadRight));

            preset.Bindings.Add(new InputRemapBinding("Handbrake", KeyCode.Space));
            preset.Bindings.Add(new InputRemapBinding("Handbrake", GamepadButton.A));

            preset.Bindings.Add(new InputRemapBinding("LookBack", KeyCode.Q));
            preset.Bindings.Add(new InputRemapBinding("LookBack", GamepadButton.LeftBumper));

            return preset;
        }

        public static RemapPreset CreateMenuPreset()
        {
            var preset = new RemapPreset("Menu")
            {
                Description = "Menu navigation controls",
                IsBuiltIn = true
            };

            preset.Bindings.Add(new InputRemapBinding("NavigateUp", KeyCode.UpArrow));
            preset.Bindings.Add(new InputRemapBinding("NavigateUp", GamepadButton.DPadUp));

            preset.Bindings.Add(new InputRemapBinding("NavigateDown", KeyCode.DownArrow));
            preset.Bindings.Add(new InputRemapBinding("NavigateDown", GamepadButton.DPadDown));

            preset.Bindings.Add(new InputRemapBinding("NavigateLeft", KeyCode.LeftArrow));
            preset.Bindings.Add(new InputRemapBinding("NavigateLeft", GamepadButton.DPadLeft));

            preset.Bindings.Add(new InputRemapBinding("NavigateRight", KeyCode.RightArrow));
            preset.Bindings.Add(new InputRemapBinding("NavigateRight", GamepadButton.DPadRight));

            preset.Bindings.Add(new InputRemapBinding("Confirm", KeyCode.Enter));
            preset.Bindings.Add(new InputRemapBinding("Confirm", GamepadButton.A));

            preset.Bindings.Add(new InputRemapBinding("Cancel", KeyCode.Escape));
            preset.Bindings.Add(new InputRemapBinding("Cancel", GamepadButton.B));

            preset.Bindings.Add(new InputRemapBinding("Options", KeyCode.O));
            preset.Bindings.Add(new InputRemapBinding("Options", GamepadButton.Start));

            return preset;
        }
    }

    public class RemapChangedEventArgs : EventArgs
    {
        public string ActionName { get; }
        public InputRemapBinding Binding { get; }

        public RemapChangedEventArgs(string actionName, InputRemapBinding binding)
        {
            ActionName = actionName;
            Binding = binding;
        }
    }

    public class PresetChangedEventArgs : EventArgs
    {
        public string PresetName { get; }

        public PresetChangedEventArgs(string presetName)
        {
            PresetName = presetName;
        }
    }

    public class InputProfiles
    {
        private Dictionary<string, InputProfile> profiles;
        private string activeProfileName;
        private InputProfile activeProfile;

        public event EventHandler<ProfileChangedEventArgs> ProfileChanged;

        public InputProfiles()
        {
            profiles = new Dictionary<string, InputProfile>();
            activeProfileName = "Default";
            activeProfile = new InputProfile("Default");
            profiles["Default"] = activeProfile;
        }

        public string ActiveProfileName => activeProfileName;
        public InputProfile ActiveProfile => activeProfile;

        public void CreateProfile(string name)
        {
            if (!profiles.ContainsKey(name))
            {
                var profile = new InputProfile(name);
                profiles[name] = profile;
            }
        }

        public void DeleteProfile(string name)
        {
            if (name != "Default" && profiles.ContainsKey(name))
            {
                profiles.Remove(name);
                if (activeProfileName == name)
                {
                    SetActiveProfile("Default");
                }
            }
        }

        public void SetActiveProfile(string name)
        {
            if (profiles.TryGetValue(name, out var profile))
            {
                activeProfileName = name;
                activeProfile = profile;
                ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(name));
            }
        }

        public InputProfile GetProfile(string name)
        {
            return profiles.TryGetValue(name, out var profile) ? profile : null;
        }

        public string[] GetProfileNames()
        {
            return profiles.Keys.ToArray();
        }

        public void DuplicateProfile(string sourceName, string newName)
        {
            if (profiles.TryGetValue(sourceName, out var source))
            {
                var duplicate = source.Clone(newName);
                profiles[newName] = duplicate;
            }
        }

        public void CopyBindingsFromProfile(string sourceName)
        {
            if (profiles.TryGetValue(sourceName, out var source))
            {
                activeProfile.CopyFrom(source);
                ProfileChanged?.Invoke(this, new ProfileChangedEventArgs(activeProfileName));
            }
        }
    }

    public class InputProfile
    {
        public string Name { get; set; }
        public InputRemapping Remapping { get; set; }
        public Dictionary<string, float> Settings { get; set; }
        public float Sensitivity { get; set; }
        public float GamepadSensitivity { get; set; }
        public float MouseSensitivity { get; set; }
        public bool InvertY { get; set; }
        public bool InvertGamepadY { get; set; }
        public float VibrationIntensity { get; set; }
        public DeadZoneConfig DeadZone { get; set; }

        public InputProfile()
        {
            Remapping = new InputRemapping();
            Settings = new Dictionary<string, float>();
            Sensitivity = 1f;
            GamepadSensitivity = 1f;
            MouseSensitivity = 1f;
            InvertY = false;
            InvertGamepadY = false;
            VibrationIntensity = 1f;
            DeadZone = new DeadZoneConfig();
        }

        public InputProfile(string name) : this()
        {
            Name = name;
        }

        public void SetSetting(string key, float value)
        {
            Settings[key] = value;
        }

        public float GetSetting(string key, float defaultValue = 0f)
        {
            return Settings.TryGetValue(key, out var value) ? value : defaultValue;
        }

        public InputProfile Clone(string newName)
        {
            var clone = new InputProfile(newName)
            {
                Sensitivity = Sensitivity,
                GamepadSensitivity = GamepadSensitivity,
                MouseSensitivity = MouseSensitivity,
                InvertY = InvertY,
                InvertGamepadY = InvertGamepadY,
                VibrationIntensity = VibrationIntensity
            };

            foreach (var kvp in Settings)
            {
                clone.Settings[kvp.Key] = kvp.Value;
            }

            return clone;
        }

        public void CopyFrom(InputProfile source)
        {
            Sensitivity = source.Sensitivity;
            GamepadSensitivity = source.GamepadSensitivity;
            MouseSensitivity = source.MouseSensitivity;
            InvertY = source.InvertY;
            InvertGamepadY = source.InvertGamepadY;
            VibrationIntensity = source.VibrationIntensity;
            
            Settings.Clear();
            foreach (var kvp in source.Settings)
            {
                Settings[kvp.Key] = kvp.Value;
            }
        }
    }

    public class ProfileChangedEventArgs : EventArgs
    {
        public string ProfileName { get; }

        public ProfileChangedEventArgs(string name)
        {
            ProfileName = name;
        }
    }

    public class InputRecorder
    {
        private List<RecordedInput> recordedInputs;
        private int maxRecordings;
        private bool isRecording;
        private bool isPlaying;
        private int playbackIndex;
        private float recordingStartTime;
        private float playbackSpeed;

        public event EventHandler RecordingStarted;
        public event EventHandler RecordingStopped;
        public event EventHandler PlaybackStarted;
        public event EventHandler PlaybackStopped;
        public event EventHandler<PlaybackProgressEventArgs> PlaybackProgress;

        public bool IsRecording => isRecording;
        public bool IsPlaying => isPlaying;
        public int RecordingCount => recordedInputs.Count;
        public float PlaybackSpeed
        {
            get => playbackSpeed;
            set => playbackSpeed = Math.Max(0.1f, Math.Min(10f, value));
        }

        public InputRecorder(int max = 10000)
        {
            maxRecordings = max;
            recordedInputs = new List<RecordedInput>();
            playbackSpeed = 1f;
        }

        public void StartRecording()
        {
            recordedInputs.Clear();
            isRecording = true;
            recordingStartTime = Time.realtimeSinceStartup;
            RecordingStarted?.Invoke(this, EventArgs.Empty);
        }

        public void StopRecording()
        {
            isRecording = false;
            RecordingStopped?.Invoke(this, EventArgs.Empty);
        }

        public void RecordInput(KeyCode key, float value = 1f)
        {
            if (isRecording && recordedInputs.Count < maxRecordings)
            {
                recordedInputs.Add(new RecordedInput
                {
                    Type = InputType.Keyboard,
                    KeyCode = key,
                    Value = value,
                    Timestamp = Time.realtimeSinceStartup - recordingStartTime,
                    Device = InputDevice.Keyboard
                });
            }
        }

        public void RecordMouseInput(Vector2 position, Vector2 delta, MouseButton button)
        {
            if (isRecording && recordedInputs.Count < maxRecordings)
            {
                recordedInputs.Add(new RecordedInput
                {
                    Type = InputType.Mouse,
                    MousePosition = position,
                    MouseDelta = delta,
                    MouseButton = button,
                    Timestamp = Time.realtimeSinceStartup - recordingStartTime,
                    Device = InputDevice.Mouse
                });
            }
        }

        public void RecordGamepadInput(GamepadButton button, float value = 1f)
        {
            if (isRecording && recordedInputs.Count < maxRecordings)
            {
                recordedInputs.Add(new RecordedInput
                {
                    Type = InputType.Gamepad,
                    GamepadButton = button,
                    Value = value,
                    Timestamp = Time.realtimeSinceStartup - recordingStartTime,
                    Device = InputDevice.Gamepad
                });
            }
        }

        public void StartPlayback()
        {
            if (recordedInputs.Count == 0)
                return;

            isPlaying = true;
            playbackIndex = 0;
            PlaybackStarted?.Invoke(this, EventArgs.Empty);
        }

        public void StopPlayback()
        {
            isPlaying = false;
            PlaybackStopped?.Invoke(this, EventArgs.Empty);
        }

        public void PausePlayback()
        {
            isPlaying = false;
        }

        public void ResumePlayback()
        {
            if (recordedInputs.Count > 0)
                isPlaying = true;
        }

        public RecordedInput Update(float deltaTime)
        {
            if (!isPlaying || playbackIndex >= recordedInputs.Count)
            {
                if (isPlaying)
                {
                    StopPlayback();
                }
                return null;
            }

            var currentTime = GetPlaybackTime();
            var recorded = recordedInputs[playbackIndex];

            if (recorded.Timestamp <= currentTime)
            {
                playbackIndex++;
                PlaybackProgress?.Invoke(this, new PlaybackProgressEventArgs(playbackIndex, recordedInputs.Count, GetPlaybackProgress()));
                return recorded;
            }

            return null;
        }

        public float GetPlaybackTime()
        {
            return (Time.realtimeSinceStartup * playbackSpeed) - recordingStartTime;
        }

        public float GetPlaybackProgress()
        {
            if (recordedInputs.Count == 0)
                return 0f;
            return (float)playbackIndex / recordedInputs.Count;
        }

        public void SetPlaybackPosition(float timestamp)
        {
            playbackIndex = 0;
            for (int i = 0; i < recordedInputs.Count; i++)
            {
                if (recordedInputs[i].Timestamp <= timestamp)
                    playbackIndex = i + 1;
            }
        }

        public void SaveToFile(string filePath)
        {
        }

        public void LoadFromFile(string filePath)
        {
        }

        public RecordedInput[] GetAllRecordedInputs()
        {
            return recordedInputs.ToArray();
        }

        public void Clear()
        {
            recordedInputs.Clear();
            playbackIndex = 0;
        }
    }

    public enum InputType
    {
        Keyboard,
        Mouse,
        Gamepad
    }

    public class RecordedInput
    {
        public InputType Type { get; set; }
        public KeyCode KeyCode { get; set; }
        public GamepadButton? GamepadButton { get; set; }
        public Vector2 MousePosition { get; set; }
        public Vector2 MouseDelta { get; set; }
        public MouseButton? MouseButton { get; set; }
        public float Value { get; set; }
        public float Timestamp { get; set; }
        public InputDevice Device { get; set; }
    }

    public class PlaybackProgressEventArgs : EventArgs
    {
        public int CurrentIndex { get; }
        public int TotalCount { get; }
        public float Progress { get; }

        public PlaybackProgressEventArgs(int current, int total, float progress)
        {
            CurrentIndex = current;
            TotalCount = total;
            Progress = progress;
        }
    }

    public class InputSimulator
    {
        private InputManager targetInput;

        public InputSimulator(InputManager input)
        {
            targetInput = input;
        }

        public void SimulateKeyPress(KeyCode key)
        {
            targetInput.SetKeyState(key, true);
        }

        public void SimulateKeyRelease(KeyCode key)
        {
            targetInput.SetKeyState(key, false);
        }

        public void SimulateKeyCombo(params KeyCode[] keys)
        {
            foreach (var key in keys)
            {
                targetInput.SetKeyState(key, true);
            }
        }

        public void SimulateMouseMove(Vector2 position)
        {
            targetInput.SetMousePosition(position);
        }

        public void SimulateMouseDelta(Vector2 delta)
        {
            targetInput.SetMousePosition(targetInput.MousePosition + delta);
        }

        public void SimulateMouseButton(int button, bool pressed)
        {
            var key = GetMouseButtonKey(button);
            targetInput.SetKeyState(key, pressed);
        }

        public void SimulateMouseClick(int button)
        {
            SimulateMouseButton(button, true);
            SimulateMouseButton(button, false);
        }

        public void SimulateGamepadButton(GamepadButton button, bool pressed)
        {
            targetInput.SetGamepadButtonState(button, pressed);
        }

        public void SimulateGamepadStick(int stickIndex, Vector2 position)
        {
            targetInput.SetJoystickPosition(stickIndex, position);
        }

        public void SimulateGamepadTrigger(GamepadButton trigger, float value)
        {
            int index = trigger == GamepadButton.LeftTrigger ? 0 : 1;
            targetInput.SetTriggerValue(index, value);
        }

        public void SimulateScroll(float delta)
        {
            targetInput.AddMouseScrollDelta(delta);
        }

        public void SimulateTextInput(string text)
        {
        }

        public void SimulateDragStart(Vector2 startPosition)
        {
            targetInput.SetMousePosition(startPosition);
            targetInput.BeginMouseDrag();
        }

        public void SimulateDragUpdate(Vector2 currentPosition)
        {
            targetInput.SetMousePosition(currentPosition);
        }

        public void SimulateDragEnd()
        {
            targetInput.EndMouseDrag();
        }

        private KeyCode GetMouseButtonKey(int button)
        {
            return button switch
            {
                0 => KeyCode.LeftArrow,
                1 => KeyCode.MiddleMouse,
                2 => KeyCode.RightMouse,
                3 => KeyCode.XButton1,
                4 => KeyCode.XButton2,
                _ => KeyCode.None
            };
        }
    }

    public class ExtendedInputBuffer : InputBuffer
    {
        private Dictionary<string, KeySequence> sequences;
        private float defaultTimeout;

        public ExtendedInputBuffer(int maxSize = 64, float timeout = 2.0f, float sequenceTimeout = 0.5f) 
            : base(maxSize, timeout)
        {
            defaultTimeout = timeout;
            sequences = new Dictionary<string, KeySequence>();
        }

        public void RegisterSequence(KeySequence sequence)
        {
            sequences[sequence.Name] = sequence;
        }

        public void UnregisterSequence(string name)
        {
            sequences.Remove(name);
        }

        public KeySequence GetSequence(string name)
        {
            return sequences.TryGetValue(name, out var sequence) ? sequence : null;
        }

        public void UpdateSequences(InputManager input, float currentTime)
        {
            foreach (var sequence in sequences.Values)
            {
                sequence.Update(input, currentTime);
            }
        }

        public bool CheckSequence(string name, KeyCode[] sequence, float maxTimeBetween = 0.5f)
        {
            return ContainsSequence(sequence, maxTimeBetween);
        }

        public string DetectCompletedSequence()
        {
            foreach (var sequence in sequences.Values)
            {
                if (sequence.GetProgress() >= 1.0f)
                {
                    return sequence.Name;
                }
            }
            return null;
        }

        public KeySequence[] GetActiveSequences()
        {
            return sequences.Values.Where(s => s.IsActive).ToArray();
        }

        public void ClearAllSequences()
        {
            foreach (var sequence in sequences.Values)
            {
                sequence.Reset();
            }
        }

        public float GetSequenceProgress(string name)
        {
            if (sequences.TryGetValue(name, out var sequence))
            {
                return sequence.GetProgress();
            }
            return 0f;
        }
    }
}

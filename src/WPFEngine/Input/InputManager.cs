using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WPFEngine.ECS;

namespace WPFEngine.Input
{
    public enum InputAxis
    {
        Horizontal,
        Vertical,
        MouseX,
        MouseY,
        ScrollWheel
    }

    public enum GamepadButton
    {
        A,
        B,
        X,
        Y,
        LeftBumper,
        RightBumper,
        LeftTrigger,
        RightTrigger,
        DPadUp,
        DPadDown,
        DPadLeft,
        DPadRight,
        Start,
        Select,
        LeftStick,
        RightStick
    }

    public enum KeyCode
    {
        None = 0,
        Backspace = 8,
        Tab = 9,
        Enter = 13,
        Shift = 16,
        Ctrl = 17,
        Alt = 18,
        Pause = 19,
        CapsLock = 20,
        Escape = 27,
        Space = 32,
        PageUp = 33,
        PageDown = 34,
        End = 35,
        Home = 36,
        LeftArrow = 37,
        UpArrow = 38,
        RightArrow = 39,
        DownArrow = 40,
        Insert = 45,
        Delete = 46,
        D0 = 48,
        D1 = 49,
        D2 = 50,
        D3 = 51,
        D4 = 52,
        D5 = 53,
        D6 = 54,
        D7 = 55,
        D8 = 56,
        D9 = 57,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        LeftWindows = 91,
        RightWindows = 92,
        NumPad0 = 96,
        NumPad1 = 97,
        NumPad2 = 98,
        NumPad3 = 99,
        NumPad4 = 100,
        NumPad5 = 101,
        NumPad6 = 102,
        NumPad7 = 103,
        NumPad8 = 104,
        NumPad9 = 105,
        Multiply = 106,
        Add = 107,
        Subtract = 109,
        Decimal = 110,
        Divide = 111,
        F1 = 112,
        F2 = 113,
        F3 = 114,
        F4 = 115,
        F5 = 116,
        F6 = 117,
        F7 = 118,
        F8 = 119,
        F9 = 120,
        F10 = 121,
        F11 = 122,
        F12 = 123,
        NumLock = 144,
        ScrollLock = 145,
        SemiColon = 186,
        Plus = 187,
        Comma = 188,
        Minus = 189,
        Period = 190,
        QuestionMark = 191,
        Tilde = 192,
        OpenBracket = 219,
        Pipe = 220,
        CloseBracket = 221,
        Quotes = 222
    }

    public class InputManager : IDisposable
    {
        private static InputManager instance;
        private Dictionary<KeyCode, KeyState> keyStates;
        private Dictionary<GamepadButton, GamepadButtonState> gamepadStates;
        private Vector2 mousePosition;
        private Vector2 mouseDelta;
        private float mouseScrollDelta;
        private Vector2 mouseDragDelta;
        private Vector2 mouseDragStart;
        private bool isMouseDragging;
        private float mouseDragThreshold;
        private int activeGamepadIndex;
        private Vector2 lastMousePosition;
        private Vector2[] joystickPositions;
        private float[] triggerValues;
        private float vibrationLeft;
        private float vibrationRight;
        private float vibrationDuration;
        private float vibrationTimer;
        private bool isVibrating;
        private Dictionary<string, InputAction> inputActions;
        private Dictionary<string, InputAxisConfig> axisConfigs;
        private bool isEnabled;
        private bool isLocked;
        private Vector2 lockPosition;
        private float deadZone;
        private float sensitivity;
        private bool invertY;
        private bool useInputSystem;
        private List<InputEventHandler> eventHandlers;
        private Dictionary<string, List<InputCallback>> actionCallbacks;
        private Dictionary<InputAxis, List<AxisCallback>> axisCallbacks;
        private Vector2 virtualJoystickPosition;
        private bool useVirtualJoystick;
        private bool simulateMouseWithTouch;
        private float touchSensitivity;
        private Dictionary<int, TouchData> activeTouches;
        private int maximumTouches;

        public event EventHandler<InputEventArgs> KeyDown;
        public event EventHandler<InputEventArgs> KeyUp;
        public event EventHandler<InputEventArgs> KeyPressed;
        public event EventHandler<MouseInputEventArgs> MouseMoved;
        public event EventHandler<MouseInputEventArgs> MouseDown;
        public event EventHandler<MouseInputEventArgs> MouseUp;
        public event EventHandler<MouseInputEventArgs> MouseClicked;
        public event EventHandler<MouseInputEventArgs> MouseDragStarted;
        public event EventHandler<MouseInputEventArgs> MouseDragEnded;
        public event EventHandler<MouseInputEventArgs> MouseWheel;
        public event EventHandler<GamepadInputEventArgs> GamepadConnected;
        public event EventHandler<GamepadInputEventArgs> GamepadDisconnected;
        public event EventHandler<GamepadInputEventArgs> GamepadButtonDown;
        public event EventHandler<GamepadInputEventArgs> GamepadButtonUp;

        public static InputManager Instance
        {
            get
            {
                if (instance == null)
                    instance = new InputManager();
                return instance;
            }
        }

        public Vector2 MousePosition
        {
            get => mousePosition;
            internal set
            {
                if (mousePosition != value)
                {
                    lastMousePosition = mousePosition;
                    mousePosition = value;
                    mouseDelta = mousePosition - lastMousePosition;
                    MouseMoved?.Invoke(this, new MouseInputEventArgs(mousePosition, mouseDelta, MouseButton.Left));
                }
            }
        }

        public Vector2 MouseDelta => mouseDelta;
        public float MouseScrollDelta => mouseScrollDelta;
        public Vector2 MouseDragDelta => mouseDragDelta;
        public Vector2 MouseDragStart => mouseDragStart;
        public bool IsMouseDragging => isMouseDragging;
        public float MouseDragThreshold => mouseDragThreshold;
        public int ActiveGamepadIndex => activeGamepadIndex;
        public bool IsEnabled => isEnabled;
        public bool IsLocked => isLocked;
        public Vector2 LockPosition => lockPosition;
        public float DeadZone => deadZone;
        public float Sensitivity => sensitivity;
        public bool InvertY => invertY;
        public Vector2 VirtualJoystickPosition => virtualJoystickPosition;
        public bool UseVirtualJoystick => useVirtualJoystick;

        public InputManager()
        {
            instance = this;
            keyStates = new Dictionary<KeyCode, KeyState>();
            gamepadStates = new Dictionary<GamepadButton, GamepadButtonState>();
            mousePosition = Vector2.Zero;
            mouseDelta = Vector2.Zero;
            mouseScrollDelta = 0f;
            mouseDragDelta = Vector2.Zero;
            mouseDragStart = Vector2.Zero;
            isMouseDragging = false;
            mouseDragThreshold = 5f;
            activeGamepadIndex = 0;
            joystickPositions = new Vector2[4];
            triggerValues = new float[4];
            vibrationLeft = 0f;
            vibrationRight = 0f;
            vibrationDuration = 0f;
            vibrationTimer = 0f;
            isVibrating = false;
            inputActions = new Dictionary<string, InputAction>();
            axisConfigs = new Dictionary<string, InputAxisConfig>();
            isEnabled = true;
            isLocked = false;
            lockPosition = Vector2.Zero;
            deadZone = 0.1f;
            sensitivity = 1f;
            invertY = false;
            useInputSystem = true;
            eventHandlers = new List<InputEventHandler>();
            actionCallbacks = new Dictionary<string, List<InputCallback>>();
            axisCallbacks = new Dictionary<InputAxis, List<AxisCallback>>();
            virtualJoystickPosition = Vector2.Zero;
            useVirtualJoystick = false;
            simulateMouseWithTouch = false;
            touchSensitivity = 1f;
            activeTouches = new Dictionary<int, TouchData>();
            maximumTouches = 5;

            InitializeKeyStates();
            InitializeGamepadStates();
            SetupDefaultInputActions();
        }

        private void InitializeKeyStates()
        {
            foreach (KeyCode key in Enum.GetValues(typeof(KeyCode)))
            {
                keyStates[key] = new KeyState { KeyCode = key, IsPressed = false, PressedThisFrame = false, ReleasedThisFrame = false };
            }
        }

        private void InitializeGamepadStates()
        {
            foreach (GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
            {
                gamepadStates[button] = new GamepadButtonState { Button = button, IsPressed = false, PressedThisFrame = false, ReleasedThisFrame = false };
            }
        }

        private void SetupDefaultInputActions()
        {
            RegisterAction("Jump", new InputAction { Name = "Jump", KeyCode = KeyCode.Space });
            RegisterAction("Fire", new InputAction { Name = "Fire", KeyCode = KeyCode.F });
            RegisterAction("Reload", new InputAction { Name = "Reload", KeyCode = KeyCode.R });
            RegisterAction("Sprint", new InputAction { Name = "Sprint", KeyCode = KeyCode.LeftShift });
            RegisterAction("Crouch", new InputAction { Name = "Crouch", KeyCode = KeyCode.C });
            RegisterAction("Interact", new InputAction { Name = "Interact", KeyCode = KeyCode.E });
            RegisterAction("Inventory", new InputAction { Name = "Inventory", KeyCode = KeyCode.I });
            RegisterAction("Pause", new InputAction { Name = "Pause", KeyCode = KeyCode.Escape });

            RegisterAxis("Horizontal", new InputAxisConfig
            {
                Name = "Horizontal",
                PositiveButton = KeyCode.D,
                NegativeButton = KeyCode.A,
                Gravity = 3f,
                Sensitivity = 3f
            });

            RegisterAxis("Vertical", new InputAxisConfig
            {
                Name = "Vertical",
                PositiveButton = KeyCode.W,
                NegativeButton = KeyCode.S,
                Gravity = 3f,
                Sensitivity = 3f
            });
        }

        public void Update(float deltaTime)
        {
            if (!isEnabled)
                return;

            UpdateKeyStates();
            UpdateGamepadStates();
            UpdateMouseStates();
            UpdateVirtualJoystick();
            UpdateVibration(deltaTime);
            ProcessInputActions();
            ProcessAxisInputs();
            mouseScrollDelta = 0f;
        }

        private void UpdateKeyStates()
        {
            foreach (var kvp in keyStates)
            {
                var state = kvp.Value;
                state.PressedThisFrame = false;
                state.ReleasedThisFrame = false;
            }
        }

        private void UpdateGamepadStates()
        {
            foreach (var kvp in gamepadStates)
            {
                var state = kvp.Value;
                state.PressedThisFrame = false;
                state.ReleasedThisFrame = false;
            }
        }

        private void UpdateMouseStates()
        {
            if (isMouseDragging)
            {
                mouseDragDelta = mousePosition - mouseDragStart;
            }
        }

        private void UpdateVirtualJoystick()
        {
            if (useVirtualJoystick)
            {
                var horizontal = GetAxisRaw(InputAxis.Horizontal);
                var vertical = GetAxisRaw(InputAxis.Vertical);
                virtualJoystickPosition = new Vector2(horizontal, vertical);
            }
        }

        private void UpdateVibration(float deltaTime)
        {
            if (!isVibrating)
                return;

            vibrationTimer += deltaTime;
            if (vibrationTimer >= vibrationDuration)
            {
                StopVibration();
            }
        }

        private void ProcessInputActions()
        {
            foreach (var action in inputActions.Values)
            {
                action.Update(this);
            }
        }

        private void ProcessAxisInputs()
        {
            foreach (var config in axisConfigs.Values)
            {
                config.Update(this);
            }
        }

        public void SetKeyState(KeyCode key, bool isPressed)
        {
            if (!keyStates.ContainsKey(key))
                return;

            var state = keyStates[key];
            var wasPressed = state.IsPressed;
            state.IsPressed = isPressed;

            if (isPressed && !wasPressed)
            {
                state.PressedThisFrame = true;
                KeyDown?.Invoke(this, new InputEventArgs(key));
                KeyPressed?.Invoke(this, new InputEventArgs(key));
            }
            else if (!isPressed && wasPressed)
            {
                state.ReleasedThisFrame = true;
                KeyUp?.Invoke(this, new InputEventArgs(key));
            }
        }

        public void SetGamepadButtonState(GamepadButton button, bool isPressed)
        {
            if (!gamepadStates.ContainsKey(button))
                return;

            var state = gamepadStates[button];
            var wasPressed = state.IsPressed;
            state.IsPressed = isPressed;

            if (isPressed && !wasPressed)
            {
                state.PressedThisFrame = true;
                GamepadButtonDown?.Invoke(this, new GamepadInputEventArgs(button, activeGamepadIndex));
            }
            else if (!isPressed && wasPressed)
            {
                state.ReleasedThisFrame = true;
                GamepadButtonUp?.Invoke(this, new GamepadInputEventArgs(button, activeGamepadIndex));
            }
        }

        public bool GetKey(KeyCode key)
        {
            return keyStates.TryGetValue(key, out var state) && state.IsPressed;
        }

        public bool GetKeyDown(KeyCode key)
        {
            return keyStates.TryGetValue(key, out var state) && state.PressedThisFrame;
        }

        public bool GetKeyUp(KeyCode key)
        {
            return keyStates.TryGetValue(key, out var state) && state.ReleasedThisFrame;
        }

        public bool GetMouseButton(int button)
        {
            var key = GetMouseButtonKey(button);
            return GetKey(key);
        }

        public bool GetMouseButtonDown(int button)
        {
            var key = GetMouseButtonKey(button);
            return GetKeyDown(key);
        }

        public bool GetMouseButtonUp(int button)
        {
            var key = GetMouseButtonKey(button);
            return GetKeyUp(key);
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

        public bool GetGamepadButton(GamepadButton button)
        {
            return gamepadStates.TryGetValue(button, out var state) && state.IsPressed;
        }

        public bool GetGamepadButtonDown(GamepadButton button)
        {
            return gamepadStates.TryGetValue(button, out var state) && state.PressedThisFrame;
        }

        public bool GetGamepadButtonUp(GamepadButton button)
        {
            return gamepadStates.TryGetValue(button, out var state) && state.ReleasedThisFrame;
        }

        public float GetAxis(InputAxis axis)
        {
            return axis switch
            {
                InputAxis.Horizontal => GetHorizontalAxis(),
                InputAxis.Vertical => GetVerticalAxis(),
                InputAxis.MouseX => mouseDelta.X * sensitivity,
                InputAxis.MouseY => mouseDelta.Y * sensitivity * (invertY ? -1 : 1),
                InputAxis.ScrollWheel => mouseScrollDelta,
                _ => 0f
            };
        }

        public float GetAxisRaw(InputAxis axis)
        {
            return axis switch
            {
                InputAxis.Horizontal => GetRawHorizontalAxis(),
                InputAxis.Vertical => GetRawVerticalAxis(),
                InputAxis.MouseX => mouseDelta.X * sensitivity,
                InputAxis.MouseY => mouseDelta.Y * sensitivity * (invertY ? -1 : 1),
                InputAxis.ScrollWheel => mouseScrollDelta,
                _ => 0f
            };
        }

        private float GetHorizontalAxis()
        {
            var value = 0f;
            
            if (GetKey(KeyCode.A) || GetKey(KeyCode.LeftArrow))
                value -= 1f;
            if (GetKey(KeyCode.D) || GetKey(KeyCode.RightArrow))
                value += 1f;

            var gamepadX = GetGamepadLeftStickX();
            if (Math.Abs(gamepadX) > Math.Abs(value))
                value = gamepadX;

            if (useVirtualJoystick)
                value = virtualJoystickPosition.X;

            return Mathf.Clamp(value, -1f, 1f);
        }

        private float GetVerticalAxis()
        {
            var value = 0f;
            
            if (GetKey(KeyCode.S) || GetKey(KeyCode.DownArrow))
                value -= 1f;
            if (GetKey(KeyCode.W) || GetKey(KeyCode.UpArrow))
                value += 1f;

            var gamepadY = GetGamepadLeftStickY();
            if (Math.Abs(gamepadY) > Math.Abs(value))
                value = gamepadY;

            if (useVirtualJoystick)
                value = virtualJoystickPosition.Y;

            return Mathf.Clamp(value, -1f, 1f);
        }

        private float GetRawHorizontalAxis()
        {
            var value = 0f;
            
            if (GetKey(KeyCode.A) || GetKey(KeyCode.LeftArrow))
                value -= 1f;
            if (GetKey(KeyCode.D) || GetKey(KeyCode.RightArrow))
                value += 1f;

            return value;
        }

        private float GetRawVerticalAxis()
        {
            var value = 0f;
            
            if (GetKey(KeyCode.S) || GetKey(KeyCode.DownArrow))
                value -= 1f;
            if (GetKey(KeyCode.W) || GetKey(KeyCode.UpArrow))
                value += 1f;

            return value;
        }

        public float GetGamepadLeftStickX()
        {
            return joystickPositions[activeGamepadIndex].X;
        }

        public float GetGamepadLeftStickY()
        {
            return joystickPositions[activeGamepadIndex].Y;
        }

        public float GetGamepadRightStickX()
        {
            return joystickPositions[activeGamepadIndex + 2].X;
        }

        public float GetGamepadRightStickY()
        {
            return joystickPositions[activeGamepadIndex + 2].Y;
        }

        public float GetGamepadTrigger(GamepadButton trigger)
        {
            return trigger switch
            {
                GamepadButton.LeftTrigger => triggerValues[activeGamepadIndex * 2],
                GamepadButton.RightTrigger => triggerValues[activeGamepadIndex * 2 + 1],
                _ => 0f
            };
        }

        public Vector2 GetJoystickPosition(int joystickIndex = 0)
        {
            return joystickPositions[joystickIndex];
        }

        public void SetJoystickPosition(int index, Vector2 position)
        {
            if (index >= 0 && index < joystickPositions.Length)
            {
                joystickPositions[index] = ApplyDeadZone(position);
            }
        }

        public void SetTriggerValue(int index, float value)
        {
            if (index >= 0 && index < triggerValues.Length)
            {
                triggerValues[index] = Math.Max(0f, Math.Min(1f, value));
            }
        }

        private Vector2 ApplyDeadZone(Vector2 input)
        {
            var magnitude = input.Magnitude;
            if (magnitude < deadZone)
                return Vector2.Zero;

            var normalized = input / magnitude;
            var adjustedMagnitude = (magnitude - deadZone) / (1f - deadZone);
            return normalized * adjustedMagnitude;
        }

        public void SetMousePosition(Vector2 position)
        {
            MousePosition = position;
        }

        public void AddMouseScrollDelta(float delta)
        {
            mouseScrollDelta += delta;
            MouseWheel?.Invoke(this, new MouseInputEventArgs(mousePosition, Vector2.Zero, MouseButton.Middle) { ScrollDelta = delta });
        }

        public void BeginMouseDrag()
        {
            if (!isMouseDragging)
            {
                isMouseDragging = true;
                mouseDragStart = mousePosition;
                mouseDragDelta = Vector2.Zero;
                MouseDragStarted?.Invoke(this, new MouseInputEventArgs(mousePosition, Vector2.Zero, MouseButton.Left));
            }
        }

        public void EndMouseDrag()
        {
            if (isMouseDragging)
            {
                isMouseDragging = false;
                MouseDragEnded?.Invoke(this, new MouseInputEventArgs(mousePosition, mouseDragDelta, MouseButton.Left));
            }
        }

        public bool IsMouseOverThreshold()
        {
            return mouseDragDelta.Magnitude >= mouseDragThreshold;
        }

        public void RegisterAction(string name, InputAction action)
        {
            action.Name = name;
            inputActions[name] = action;
        }

        public void UnregisterAction(string name)
        {
            inputActions.Remove(name);
        }

        public InputAction GetAction(string name)
        {
            return inputActions.TryGetValue(name, out var action) ? action : null;
        }

        public bool GetActionPressed(string actionName)
        {
            return inputActions.TryGetValue(actionName, out var action) && action.IsPressed;
        }

        public bool GetActionDown(string actionName)
        {
            return inputActions.TryGetValue(actionName, out var action) && action.IsPressedThisFrame;
        }

        public bool GetActionUp(string actionName)
        {
            return inputActions.TryGetValue(actionName, out var action) && action.ReleasedThisFrame;
        }

        public void RegisterAxis(string name, InputAxisConfig config)
        {
            config.Name = name;
            axisConfigs[name] = config;
        }

        public void UnregisterAxis(string name)
        {
            axisConfigs.Remove(name);
        }

        public float GetAxis(string axisName)
        {
            return axisConfigs.TryGetValue(axisName, out var config) ? config.Value : 0f;
        }

        public float GetAxisRaw(string axisName)
        {
            return axisConfigs.TryGetValue(axisName, out var config) ? config.RawValue : 0f;
        }

        public void RegisterCallback(string actionName, InputCallback callback)
        {
            if (!actionCallbacks.ContainsKey(actionName))
                actionCallbacks[actionName] = new List<InputCallback>();
            actionCallbacks[actionName].Add(callback);
        }

        public void UnregisterCallback(string actionName, InputCallback callback)
        {
            if (actionCallbacks.TryGetValue(actionName, out var callbacks))
            {
                callbacks.Remove(callback);
            }
        }

        public void RegisterAxisCallback(InputAxis axis, AxisCallback callback)
        {
            if (!axisCallbacks.ContainsKey(axis))
                axisCallbacks[axis] = new List<AxisCallback>();
            axisCallbacks[axis].Add(callback);
        }

        public void UnregisterAxisCallback(InputAxis axis, AxisCallback callback)
        {
            if (axisCallbacks.TryGetValue(axis, out var callbacks))
            {
                callbacks.Remove(callback);
            }
        }

        public void StartVibration(float leftMotor, float rightMotor, float duration)
        {
            vibrationLeft = Math.Max(0f, Math.Min(1f, leftMotor));
            vibrationRight = Math.Max(0f, Math.Min(1f, rightMotor));
            vibrationDuration = duration;
            vibrationTimer = 0f;
            isVibrating = true;
        }

        public void StopVibration()
        {
            vibrationLeft = 0f;
            vibrationRight = 0f;
            vibrationDuration = 0f;
            vibrationTimer = 0f;
            isVibrating = false;
        }

        public void SetActiveGamepad(int index)
        {
            activeGamepadIndex = Math.Max(0, Math.Min(3, index));
        }

        public void SetDeadZone(float deadzone)
        {
            deadZone = Math.Max(0f, Math.Min(1f, deadzone));
        }

        public void SetSensitivity(float sens)
        {
            sensitivity = Math.Max(0.1f, sens);
        }

        public void SetInvertY(bool invert)
        {
            invertY = invert;
        }

        public void LockInput()
        {
            isLocked = true;
        }

        public void UnlockInput()
        {
            isLocked = false;
        }

        public void LockMouseToPosition(Vector2 position)
        {
            lockPosition = position;
            isLocked = true;
        }

        public void Enable()
        {
            isEnabled = true;
        }

        public void Disable()
        {
            isEnabled = false;
        }

        public void AddTouch(int touchId, Vector2 position)
        {
            if (activeTouches.Count < maximumTouches)
            {
                activeTouches[touchId] = new TouchData
                {
                    TouchId = touchId,
                    Position = position,
                    StartPosition = position,
                    Delta = Vector2.Zero,
                    Phase = TouchPhase.Began
                };
            }
        }

        public void UpdateTouch(int touchId, Vector2 position)
        {
            if (activeTouches.TryGetValue(touchId, out var touch))
            {
                touch.Delta = position - touch.Position;
                touch.Position = position;
                touch.Phase = TouchPhase.Moved;
            }
        }

        public void RemoveTouch(int touchId)
        {
            if (activeTouches.TryGetValue(touchId, out var touch))
            {
                touch.Phase = TouchPhase.Ended;
            }
        }

        public TouchData GetTouch(int touchId)
        {
            return activeTouches.TryGetValue(touchId, out var touch) ? touch : null;
        }

        public TouchData[] GetAllTouches()
        {
            return activeTouches.Values.ToArray();
        }

        public void ClearAllTouches()
        {
            activeTouches.Clear();
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
                keyStates.Clear();
                gamepadStates.Clear();
                inputActions.Clear();
                axisConfigs.Clear();
                actionCallbacks.Clear();
                axisCallbacks.Clear();
                activeTouches.Clear();
            }
        }
    }

    public class KeyState
    {
        public KeyCode KeyCode { get; set; }
        public bool IsPressed { get; set; }
        public bool PressedThisFrame { get; set; }
        public bool ReleasedThisFrame { get; set; }
    }

    public class GamepadButtonState
    {
        public GamepadButton Button { get; set; }
        public bool IsPressed { get; set; }
        public bool PressedThisFrame { get; set; }
        public bool ReleasedThisFrame { get; set; }
    }

    public class InputAction
    {
        public string Name { get; set; }
        public KeyCode KeyCode { get; set; }
        public GamepadButton GamepadButton { get; set; }
        public bool UseGamepad { get; set; }
        public bool IsPressed { get; private set; }
        public bool IsPressedThisFrame { get; private set; }
        public bool ReleasedThisFrame { get; private set; }

        public void Update(InputManager input)
        {
            IsPressed = input.GetKey(KeyCode);
            if (UseGamepad)
                IsPressed = IsPressed || input.GetGamepadButton(GamepadButton);
            
            IsPressedThisFrame = input.GetKeyDown(KeyCode);
            if (UseGamepad)
                IsPressedThisFrame = IsPressedThisFrame || input.GetGamepadButtonDown(GamepadButton);
            
            ReleasedThisFrame = input.GetKeyUp(KeyCode);
            if (UseGamepad)
                ReleasedThisFrame = ReleasedThisFrame || input.GetGamepadButtonUp(GamepadButton);
        }
    }

    public class InputAxisConfig
    {
        public string Name { get; set; }
        public KeyCode PositiveButton { get; set; }
        public KeyCode NegativeButton { get; set; }
        public KeyCode AltPositiveButton { get; set; }
        public KeyCode AltNegativeButton { get; set; }
        public float Gravity { get; set; }
        public float Sensitivity { get; set; }
        public float DeadZone { get; set; }
        public float Value { get; private set; }
        public float RawValue { get; private set; }
        private float currentValue;

        public InputAxisConfig()
        {
            Gravity = 1f;
            Sensitivity = 1f;
            DeadZone = 0f;
            Value = 0f;
            RawValue = 0f;
        }

        public void Update(InputManager input)
        {
            var inputValue = 0f;

            if (input.GetKey(PositiveButton))
                inputValue += 1f;
            if (input.GetKey(NegativeButton))
                inputValue -= 1f;
            if (AltPositiveButton != 0 && input.GetKey(AltPositiveButton))
                inputValue += 1f;
            if (AltNegativeButton != 0 && input.GetKey(AltNegativeButton))
                inputValue -= 1f;

            if (inputValue != 0)
            {
                currentValue = Mathf.MoveTowards(currentValue, inputValue, Sensitivity * Time.deltaTime);
            }
            else
            {
                currentValue = Mathf.MoveTowards(currentValue, 0f, Gravity * Time.deltaTime);
            }

            if (Math.Abs(currentValue) < DeadZone)
                currentValue = 0f;

            RawValue = inputValue;
            Value = currentValue;
        }
    }

    public class InputEventArgs : EventArgs
    {
        public KeyCode Key { get; }
        public GamepadButton? GamepadButton { get; }

        public InputEventArgs(KeyCode key)
        {
            Key = key;
        }

        public InputEventArgs(GamepadButton button)
        {
            GamepadButton = button;
        }
    }

    public class MouseInputEventArgs : EventArgs
    {
        public Vector2 Position { get; }
        public Vector2 Delta { get; }
        public MouseButton Button { get; }
        public float ScrollDelta { get; set; }

        public MouseInputEventArgs(Vector2 position, Vector2 delta, MouseButton button)
        {
            Position = position;
            Delta = delta;
            Button = button;
        }
    }

    public class GamepadInputEventArgs : EventArgs
    {
        public GamepadButton Button { get; }
        public int GamepadIndex { get; }

        public GamepadInputEventArgs(GamepadButton button, int gamepadIndex)
        {
            Button = button;
            GamepadIndex = gamepadIndex;
        }
    }

    public class TouchData
    {
        public int TouchId { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 StartPosition { get; set; }
        public Vector2 Delta { get; set; }
        public TouchPhase Phase { get; set; }
        public float TapCount { get; set; }
        public float DeltaTime { get; set; }
    }

    public enum TouchPhase
    {
        None,
        Began,
        Moved,
        Stationary,
        Ended,
        Canceled
    }

    public enum MouseButton
    {
        Left,
        Middle,
        Right,
        XButton1,
        XButton2
    }

    public class InputEventHandler
    {
        public string Name { get; set; }
        public int Priority { get; set; }
        public bool IsEnabled { get; set; }

        public InputEventHandler()
        {
            IsEnabled = true;
        }

        public virtual void OnKeyDown(KeyCode key) { }
        public virtual void OnKeyUp(KeyCode key) { }
        public virtual void OnMouseMoved(Vector2 position, Vector2 delta) { }
        public virtual void OnMouseDown(Vector2 position, MouseButton button) { }
        public virtual void OnMouseUp(Vector2 position, MouseButton button) { }
    }

    public delegate void InputCallback(InputAction action);
    public delegate void AxisCallback(InputAxis axis, float value);

    public static class Mathf
    {
        public static float Clamp(float value, float min, float max)
        {
            return Math.Max(min, Math.Min(max, value));
        }

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
                return target;
            return current + Math.Sign(target - current) * maxDelta;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp(t, 0f, 1f);
        }
    }
}

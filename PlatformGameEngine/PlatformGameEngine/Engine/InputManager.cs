using System.Collections.Generic;
using System.Windows.Input;

namespace PlatformGameEngine.Engine
{
    public static class InputManager
    {
        private static HashSet<Key> _pressedKeys = new HashSet<Key>();
        private static HashSet<Key> _downKeys = new HashSet<Key>();
        private static HashSet<Key> _releasedKeys = new HashSet<Key>();

        public static void Update()
        {
            _downKeys.Clear();
            _releasedKeys.Clear();
        }

        public static void KeyDown(Key key)
        {
            if (!_pressedKeys.Contains(key))
            {
                _pressedKeys.Add(key);
                _downKeys.Add(key);
            }
        }

        public static void KeyUp(Key key)
        {
            if (_pressedKeys.Contains(key))
            {
                _pressedKeys.Remove(key);
                _releasedKeys.Add(key);
            }
        }

        public static bool IsKeyPressed(Key key) => _pressedKeys.Contains(key);
        public static bool IsKeyDown(Key key) => _downKeys.Contains(key);
        public static bool IsKeyReleased(Key key) => _releasedKeys.Contains(key);
    }
}
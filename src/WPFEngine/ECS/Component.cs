using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Core.Game;

namespace WPFEngine.ECS
{
    public abstract class Component : IComparable<Component>
    {
        private bool _isEnabled = true;
        private GameObject? _gameObject;
        private int _sortOrder = 0;

        public bool IsEnabled
        {
            get => _isEnabled && (_gameObject == null || _gameObject.IsActive);
            set => _isEnabled = value;
        }

        public GameObject? GameObject
        {
            get => _gameObject;
            internal set => _gameObject = value;
        }

        public Scene? Scene => _gameObject?.Scene;
        public Transform? Transform => _gameObject?.Transform;
        public Guid Id { get; } = Guid.NewGuid();
        public int SortOrder
        {
            get => _sortOrder;
            set => _sortOrder = value;
        }

        public virtual void Initialize() { }
        public virtual void Update(float deltaTime) { }
        public virtual void FixedUpdate(float fixedDeltaTime) { }
        public virtual void LateUpdate(float deltaTime) { }
        public virtual void OnEnable() { }
        public virtual void OnDisable() { }
        public virtual void OnDestroy() { }

        public virtual int CompareTo(Component? other)
        {
            if (other == null) return 1;
            return _sortOrder.CompareTo(other._sortOrder);
        }

        public T GetComponent<T>() where T : Component
        {
            return _gameObject?.GetComponent<T>()!;
        }

        public T? GetComponentInParent<T>() where T : Component
        {
            return _gameObject?.GetComponentInParent<T>();
        }

        public T? GetComponentInChildren<T>() where T : Component
        {
            return _gameObject?.GetComponentInChildren<T>();
        }

        public List<T> GetComponents<T>() where T : Component
        {
            return _gameObject?.GetComponents<T>() ?? new List<T>();
        }
    }

    public abstract class Component<T> : Component where T : Component<T>
    {
    }
}

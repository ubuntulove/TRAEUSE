using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Scene;
using WPFEngine.Rendering;

namespace WPFEngine.ECS
{
    public class GameObject : IDisposable
    {
        private string name;
        private string tag;
        private bool isActive;
        private bool isDirty;
        private List<GameObject> children;
        private GameObject parent;
        private SceneObject currentScene;
        private Guid instanceId;

        public event EventHandler<GameObjectEventArgs> ComponentAdded;
        public event EventHandler<GameObjectEventArgs> ComponentRemoved;
        public event EventHandler<GameObjectEventArgs> ChildAdded;
        public event EventHandler<GameObjectEventArgs> ChildRemoved;
        public event EventHandler<GameObjectEventArgs> Destroyed;
        public event EventHandler<GameObjectEventArgs> ActiveChanged;

        public string Name
        {
            get => name;
            set
            {
                if (name != value)
                {
                    name = value;
                    OnPropertyChanged(nameof(Name));
                }
            }
        }

        public string Tag
        {
            get => tag;
            set => tag = value ?? string.Empty;
        }

        public bool IsActive
        {
            get => isActive;
            set
            {
                if (isActive != value)
                {
                    isActive = value;
                    UpdateActiveState();
                    ActiveChanged?.Invoke(this, new GameObjectEventArgs(this));
                }
            }
        }

        public bool IsActiveInHierarchy => IsActive && (Parent == null || Parent.IsActiveInHierarchy);

        public Guid InstanceId => instanceId;

        public Transform Transform { get; private set; }

        public GameObject Parent
        {
            get => parent;
            set
            {
                if (parent != value)
                {
                    var oldParent = parent;
                    parent?.RemoveChild(this);
                    parent = value;
                    parent?.AddChild(this);
                    OnParentChanged(oldParent, value);
                }
            }
        }

        public IReadOnlyList<GameObject> Children => children.AsReadOnly();

        public SceneObject CurrentScene => currentScene;

        public int ChildCount => children.Count;

        private Dictionary<Type, Component> componentCache;
        private List<Component> components;

        public GameObject() : this("GameObject")
        {
        }

        public GameObject(string name)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            tag = "Untagged";
            isActive = true;
            isDirty = true;
            children = new List<GameObject>();
            components = new List<Component>();
            componentCache = new Dictionary<Type, Component>();
            instanceId = Guid.NewGuid();
            Transform = AddComponent<Transform>();
        }

        public T AddComponent<T>() where T : Component, new()
        {
            return AddComponent(typeof(T)) as T;
        }

        public Component AddComponent(Type componentType)
        {
            if (!typeof(Component).IsAssignableFrom(componentType))
            {
                throw new ArgumentException($"Type {componentType.Name} must inherit from Component", nameof(componentType));
            }

            var component = Activator.CreateInstance(componentType) as Component;
            if (component != null)
            {
                AddComponentInternal(component);
            }
            return component;
        }

        public T AddComponent<T>(T component) where T : Component
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            if (component.GameObject != null)
            {
                throw new InvalidOperationException("Component already belongs to a GameObject");
            }

            AddComponentInternal(component);
            return component;
        }

        private void AddComponentInternal(Component component)
        {
            var componentType = component.GetType();
            if (componentCache.ContainsKey(componentType))
            {
                throw new InvalidOperationException($"GameObject already has a component of type {componentType.Name}");
            }

            components.Add(component);
            componentCache[componentType] = component;
            component.Initialize(this);
            component.Enabled = IsActiveInHierarchy;
            ComponentAdded?.Invoke(this, new GameObjectEventArgs(this));
        }

        public T GetComponent<T>() where T : Component
        {
            return GetComponent(typeof(T)) as T;
        }

        public Component GetComponent(Type componentType)
        {
            if (componentCache.TryGetValue(componentType, out var component))
            {
                return component;
            }

            foreach (var kvp in componentCache)
            {
                if (componentType.IsAssignableFrom(kvp.Key))
                {
                    return kvp.Value;
                }
            }

            return null;
        }

        public T GetComponentInChildren<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component != null)
                return component;

            foreach (var child in children)
            {
                component = child.GetComponentInChildren<T>();
                if (component != null)
                    return component;
            }

            return null;
        }

        public T GetComponentInParent<T>() where T : Component
        {
            var component = GetComponent<T>();
            if (component != null)
                return component;

            return Parent?.GetComponentInParent<T>();
        }

        public T[] GetComponents<T>() where T : Component
        {
            return GetComponents(typeof(T)).Cast<T>().ToArray();
        }

        public Component[] GetComponents(Type componentType)
        {
            var results = new List<Component>();
            foreach (var kvp in componentCache)
            {
                if (componentType.IsAssignableFrom(kvp.Key))
                {
                    results.Add(kvp.Value);
                }
            }
            return results.ToArray();
        }

        public bool HasComponent<T>() where T : Component
        {
            return HasComponent(typeof(T));
        }

        public bool HasComponent(Type componentType)
        {
            return componentCache.ContainsKey(componentType);
        }

        public bool RemoveComponent<T>() where T : Component
        {
            return RemoveComponent(typeof(T));
        }

        public bool RemoveComponent(Type componentType)
        {
            if (componentCache.TryGetValue(componentType, out var component))
            {
                component.OnComponentRemoved();
                components.Remove(component);
                componentCache.Remove(componentType);
                component.GameObject = null;
                component.Enabled = false;
                ComponentRemoved?.Invoke(this, new GameObjectEventArgs(this));
                return true;
            }
            return false;
        }

        public void RemoveComponent(Component component)
        {
            if (component == null)
                return;

            var componentType = component.GetType();
            if (componentCache.TryGetValue(componentType, out var cachedComponent) && cachedComponent == component)
            {
                RemoveComponent(componentType);
            }
        }

        public GameObject Find(string name)
        {
            if (Name == name)
                return this;

            foreach (var child in children)
            {
                var found = child.Find(name);
                if (found != null)
                    return found;
            }

            return null;
        }

        public GameObject FindGameObjectWithTag(string tag)
        {
            if (Tag == tag)
                return this;

            foreach (var child in children)
            {
                var found = child.FindGameObjectWithTag(tag);
                if (found != null)
                    return found;
            }

            return null;
        }

        public GameObject[] FindGameObjectsWithTag(string tag)
        {
            var results = new List<GameObject>();
            CollectWithTag(tag, results);
            return results.ToArray();
        }

        private void CollectWithTag(string tag, List<GameObject> results)
        {
            if (Tag == tag)
                results.Add(this);

            foreach (var child in children)
            {
                child.CollectWithTag(tag, results);
            }
        }

        public GameObject Instantiate()
        {
            return Instantiate(Vector2.Zero, 0f);
        }

        public GameObject Instantiate(Vector2 position)
        {
            return Instantiate(position, 0f);
        }

        public GameObject Instantiate(Vector2 position, float rotation)
        {
            var clone = Clone();
            if (clone.Transform != null)
            {
                clone.Transform.LocalPosition = position;
                clone.Transform.LocalRotation = rotation;
            }
            return clone;
        }

        public GameObject Clone()
        {
            var clone = new GameObject(Name + "(Clone)");
            clone.Tag = Tag;
            clone.IsActive = IsActive;

            foreach (var child in children)
            {
                var childClone = child.Clone();
                childClone.Parent = clone;
            }

            return clone;
        }

        public void Destroy()
        {
            Destroy(0f);
        }

        public void Destroy(float delay)
        {
            if (delay <= 0f)
            {
                DestroyImmediate();
            }
            else
            {
                ScheduleDestroy(delay);
            }
        }

        private void ScheduleDestroy(float delay)
        {
            System.Threading.Tasks.Task.Delay(TimeSpan.FromSeconds(delay)).ContinueWith(_ =>
            {
                System.Windows.Application.Current?.Dispatcher.Invoke(() =>
                {
                    DestroyImmediate();
                });
            });
        }

        public void DestroyImmediate()
        {
            if (currentScene != null)
            {
                currentScene.RemoveGameObject(this);
            }

            DestroyChildren();
            DestroyComponents();
            Destroyed?.Invoke(this, new GameObjectEventArgs(this));
            ClearReferences();
        }

        private void DestroyChildren()
        {
            var childrenToDestroy = children.ToList();
            foreach (var child in childrenToDestroy)
            {
                child.DestroyImmediate();
            }
        }

        private void DestroyComponents()
        {
            foreach (var component in components.ToList())
            {
                component.OnComponentRemoved();
            }
            components.Clear();
            componentCache.Clear();
        }

        private void ClearReferences()
        {
            parent?.RemoveChild(this);
            parent = null;
            currentScene = null;
        }

        public T InstantiatePrefab<T>(T prefab) where T : GameObject
        {
            if (prefab == null)
                throw new ArgumentNullException(nameof(prefab));

            var clone = prefab.Clone();
            if (currentScene != null)
            {
                currentScene.AddGameObject(clone);
            }
            return clone as T;
        }

        internal void SetScene(SceneObject scene)
        {
            currentScene = scene;
            foreach (var child in children)
            {
                child.SetScene(scene);
            }
        }

        internal void MarkDirty()
        {
            isDirty = true;
            foreach (var child in children)
            {
                child.MarkDirty();
            }
        }

        internal void UpdateActiveState()
        {
            var newActiveState = IsActiveInHierarchy;
            foreach (var component in components)
            {
                component.Enabled = newActiveState;
            }
        }

        private void AddChild(GameObject child)
        {
            if (!children.Contains(child))
            {
                children.Add(child);
                ChildAdded?.Invoke(this, new GameObjectEventArgs(child));
            }
        }

        private void RemoveChild(GameObject child)
        {
            if (children.Remove(child))
            {
                ChildRemoved?.Invoke(this, new GameObjectEventArgs(child));
            }
        }

        protected virtual void OnParentChanged(GameObject oldParent, GameObject newParent)
        {
            MarkDirty();
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
        }

        public override string ToString()
        {
            return $"GameObject({Name}, InstanceId={InstanceId}, Active={IsActive})";
        }

        public override bool Equals(object obj)
        {
            if (obj is GameObject other)
            {
                return instanceId == other.instanceId;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return instanceId.GetHashCode();
        }

        public static bool operator ==(GameObject left, GameObject right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(GameObject left, GameObject right)
        {
            return !(left == right);
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
                DestroyComponents();
                children.Clear();
                componentCache.Clear();
            }
        }
    }

    public class GameObjectEventArgs : EventArgs
    {
        public GameObject GameObject { get; }
        public Component Component { get; }

        public GameObjectEventArgs(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public GameObjectEventArgs(Component component)
        {
            Component = component;
            GameObject = component?.GameObject;
        }
    }

    public class Vector2
    {
        public float X { get; set; }
        public float Y { get; set; }

        public static Vector2 Zero => new Vector2(0, 0);
        public static Vector2 One => new Vector2(1, 1);
        public static Vector2 Up => new Vector2(0, 1);
        public static Vector2 Down => new Vector2(0, -1);
        public static Vector2 Left => new Vector2(-1, 0);
        public static Vector2 Right => new Vector2(1, 0);

        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);
        public float SqrMagnitude => X * X + Y * Y;
        public Vector2 Normalized
        {
            get
            {
                var mag = Magnitude;
                return mag > 0 ? this / mag : Zero;
            }
        }

        public Vector2() : this(0, 0)
        {
        }

        public Vector2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float Dot(Vector2 other) => X * other.X + Y * other.Y;
        public float Cross(Vector2 other) => X * other.Y - Y * other.X;
        public float Distance(Vector2 other) => (this - other).Magnitude;
        public float SqrDistance(Vector2 other) => (this - other).SqrMagnitude;
        public Vector2 Reflect(Vector2 normal) => this - 2 * Dot(normal) * normal;
        public Vector2 Lerp(Vector2 other, float t) => this + (other - this) * t;
        public Vector2 MoveTowards(Vector2 target, float maxDistanceDelta)
        {
            var diff = target - this;
            var dist = diff.Magnitude;
            if (dist <= maxDistanceDelta || dist < float.Epsilon)
                return target;
            return this + diff / dist * maxDistanceDelta;
        }

        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.X + b.X, a.Y + b.Y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.X - b.X, a.Y - b.Y);
        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.X * d, a.Y * d);
        public static Vector2 operator *(float d, Vector2 a) => new Vector2(a.X * d, a.Y * d);
        public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.X / d, a.Y / d);
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.X, -a.Y);
        public static bool operator ==(Vector2 lhs, Vector2 rhs) => lhs.X == rhs.X && lhs.Y == rhs.Y;
        public static bool operator !=(Vector2 lhs, Vector2 rhs) => !(lhs == rhs);

        public override bool Equals(object obj)
        {
            if (obj is Vector2 other)
                return this == other;
            return false;
        }

        public override int GetHashCode() => HashCode.Combine(X, Y);
        public override string ToString() => $"({X:F2}, {Y:F2})";
    }
}

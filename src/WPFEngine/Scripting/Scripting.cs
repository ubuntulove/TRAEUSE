using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using WPFEngine.ECS;
using WPFEngine.Scene;
using WPFEngine.Input;
using WPFEngine.Core.Game;
using WPFEngine.Math;
using WPFEngine.Physics;

namespace WPFEngine.Scripting
{
    public abstract class Script : Component
    {
        private bool _hasAwakened;
        private bool _hasStarted;
        private CoroutineRunner _coroutineRunner;
        private List<InvokeInfo> _pendingInvokes;
        private bool _isDestroyed;

        public bool IsDestroyed => _isDestroyed;

        public Script()
        {
            _hasAwakened = false;
            _hasStarted = false;
            _coroutineRunner = new CoroutineRunner();
            _pendingInvokes = new List<InvokeInfo>();
            _isDestroyed = false;
        }

        public override void Initialize(GameObject? gameObject)
        {
            base.Initialize(gameObject);
            if (GameObject != null)
            {
                GameObject.Destroyed += OnGameObjectDestroyed;
            }
        }

        private void OnGameObjectDestroyed(object? sender, GameObjectEventArgs e)
        {
            _isDestroyed = true;
            OnDestroy();
        }

        public void InvokeAwake()
        {
            if (_hasAwakened) return;
            _hasAwakened = true;
            Awake();
        }

        public void InvokeStart()
        {
            if (_hasStarted) return;
            if (!_hasAwakened)
            {
                InvokeAwake();
            }
            _hasStarted = true;
            Start();
        }

        public override void Update(float deltaTime)
        {
            if (!_hasAwakened)
            {
                InvokeAwake();
            }

            if (!_hasStarted)
            {
                InvokeStart();
            }

            _coroutineRunner.Update(deltaTime);
            ProcessInvokes(deltaTime);
            UpdateScript(deltaTime);
        }

        public override void FixedUpdate(float fixedDeltaTime)
        {
            FixedUpdateScript(fixedDeltaTime);
        }

        public override void LateUpdate(float deltaTime)
        {
            LateUpdateScript(deltaTime);
        }

        public override void OnEnable()
        {
            base.OnEnable();
            OnEnableScript();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            OnDisableScript();
        }

        public override void OnDestroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            if (GameObject != null)
            {
                GameObject.Destroyed -= OnGameObjectDestroyed;
            }

            CancelAllInvokes();
            StopAllCoroutines();
            OnDestroyScript();
        }

        protected virtual void Awake() { }
        protected virtual void Start() { }
        protected virtual void UpdateScript(float deltaTime) { }
        protected virtual void FixedUpdateScript(float fixedDeltaTime) { }
        protected virtual void LateUpdateScript(float deltaTime) { }
        protected virtual void OnEnableScript() { }
        protected virtual void OnDisableScript() { }
        protected virtual void OnDestroyScript() { }

        public CoroutineHandle StartCoroutine(IEnumerator coroutine)
        {
            return _coroutineRunner.StartCoroutine(coroutine);
        }

        public void StopCoroutine(CoroutineHandle handle)
        {
            _coroutineRunner.StopCoroutine(handle);
        }

        public void StopCoroutine(IEnumerator coroutine)
        {
            _coroutineRunner.StopCoroutine(coroutine);
        }

        public void StopAllCoroutines()
        {
            _coroutineRunner.StopAllCoroutines();
        }

        public CoroutineHandle StartCoroutine(string methodName)
        {
            var method = GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (method == null || method.ReturnType != typeof(IEnumerator))
            {
                throw new InvalidOperationException($"Method '{methodName}' not found or does not return IEnumerator");
            }

            var coroutine = (IEnumerator)method.Invoke(this, null);
            return StartCoroutine(coroutine);
        }

        public void StopCoroutine(string methodName)
        {
            _coroutineRunner.StopCoroutine(methodName);
        }

        public void Invoke(string methodName, float time = 0f)
        {
            _pendingInvokes.Add(new InvokeInfo
            {
                MethodName = methodName,
                TimeRemaining = time,
                IsRepeating = false,
                RepeatInterval = 0f
            });
        }

        public void InvokeRepeating(string methodName, float time, float repeatInterval)
        {
            _pendingInvokes.Add(new InvokeInfo
            {
                MethodName = methodName,
                TimeRemaining = time,
                IsRepeating = true,
                RepeatInterval = repeatInterval
            });
        }

        public void CancelInvoke(string methodName)
        {
            _pendingInvokes.RemoveAll(invoke => invoke.MethodName == methodName);
        }

        public void CancelAllInvokes()
        {
            _pendingInvokes.Clear();
        }

        public bool IsInvoking(string methodName)
        {
            return _pendingInvokes.Any(invoke => invoke.MethodName == methodName);
        }

        public bool IsInvoking()
        {
            return _pendingInvokes.Count > 0;
        }

        private void ProcessInvokes(float deltaTime)
        {
            var toRemove = new List<InvokeInfo>();

            foreach (var invoke in _pendingInvokes)
            {
                invoke.TimeRemaining -= deltaTime;
                if (invoke.TimeRemaining <= 0f)
                {
                    var method = GetType().GetMethod(invoke.MethodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    if (method != null && method.GetParameters().Length == 0)
                    {
                        method.Invoke(this, null);
                    }

                    if (invoke.IsRepeating)
                    {
                        invoke.TimeRemaining = invoke.RepeatInterval;
                    }
                    else
                    {
                        toRemove.Add(invoke);
                    }
                }
            }

            foreach (var invoke in toRemove)
            {
                _pendingInvokes.Remove(invoke);
            }
        }

        private class InvokeInfo
        {
            public string MethodName { get; set; } = string.Empty;
            public float TimeRemaining { get; set; }
            public bool IsRepeating { get; set; }
            public float RepeatInterval { get; set; }
        }
    }

    public abstract class ScriptableObject : IDisposable
    {
        private static readonly Dictionary<Type, List<ScriptableObject>> _loadedAssets = new Dictionary<Type, List<ScriptableObject>>();
        private bool _isDestroyed;

        public bool IsDestroyed => _isDestroyed;

        protected ScriptableObject()
        {
            _isDestroyed = false;
        }

        public static T CreateInstance<T>() where T : ScriptableObject, new()
        {
            var instance = new T();
            instance.OnCreate();
            RegisterInstance(instance);
            return instance;
        }

        public static ScriptableObject CreateInstance(Type type)
        {
            if (!typeof(ScriptableObject).IsAssignableFrom(type))
            {
                throw new ArgumentException($"Type {type.Name} must inherit from ScriptableObject");
            }

            var instance = Activator.CreateInstance(type) as ScriptableObject;
            if (instance == null)
            {
                throw new InvalidOperationException($"Failed to create instance of {type.Name}");
            }

            instance.OnCreate();
            RegisterInstance(instance);
            return instance;
        }

        private static void RegisterInstance(ScriptableObject instance)
        {
            var type = instance.GetType();
            if (!_loadedAssets.ContainsKey(type))
            {
                _loadedAssets[type] = new List<ScriptableObject>();
            }
            _loadedAssets[type].Add(instance);
        }

        public static T[] FindObjectsOfType<T>() where T : ScriptableObject
        {
            var type = typeof(T);
            if (_loadedAssets.TryGetValue(type, out var list))
            {
                return list.OfType<T>().ToArray();
            }
            return Array.Empty<T>();
        }

        public static T? FindObjectOfType<T>() where T : ScriptableObject
        {
            return FindObjectsOfType<T>().FirstOrDefault();
        }

        public static void Destroy(ScriptableObject? obj)
        {
            if (obj == null || obj._isDestroyed) return;

            obj._isDestroyed = true;
            var type = obj.GetType();
            if (_loadedAssets.TryGetValue(type, out var list))
            {
                list.Remove(obj);
            }
            obj.OnDestroy();
        }

        protected virtual void OnCreate() { }
        protected virtual void OnDestroy() { }

        public virtual void Dispose()
        {
            Destroy(this);
            GC.SuppressFinalize(this);
        }
    }

    public class ScriptContext
    {
        private static ScriptContext? _instance;
        private readonly Dictionary<string, object> _globalVariables;
        private ScriptCompiler? _compiler;
        private ScriptHotReload? _hotReloader;
        private CoroutineRunner _coroutineRunner;
        private Scene? _activeScene;

        public static ScriptContext Instance => _instance ??= new ScriptContext();

        public Scene? ActiveScene
        {
            get => _activeScene;
            set => _activeScene = value;
        }

        public Dictionary<string, object> GlobalVariables => _globalVariables;

        public ScriptCompiler Compiler => _compiler ??= new ScriptCompiler();
        public ScriptHotReload HotReloader => _hotReloader ??= new ScriptHotReload();

        private ScriptContext()
        {
            _globalVariables = new Dictionary<string, object>();
            _coroutineRunner = new CoroutineRunner();
        }

        public void Update(float deltaTime)
        {
            _coroutineRunner.Update(deltaTime);
        }

        public CoroutineHandle StartGlobalCoroutine(IEnumerator coroutine)
        {
            return _coroutineRunner.StartCoroutine(coroutine);
        }

        public void StopGlobalCoroutine(CoroutineHandle handle)
        {
            _coroutineRunner.StopCoroutine(handle);
        }

        public void SetGlobalVariable(string name, object value)
        {
            _globalVariables[name] = value;
        }

        public T? GetGlobalVariable<T>(string name)
        {
            if (_globalVariables.TryGetValue(name, out var value) && value is T typed)
            {
                return typed;
            }
            return default;
        }

        public bool TryGetGlobalVariable<T>(string name, out T? value)
        {
            if (_globalVariables.TryGetValue(name, out var obj) && obj is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        public void ClearGlobalVariables()
        {
            _globalVariables.Clear();
        }
    }

    public struct CoroutineHandle : IEquatable<CoroutineHandle>
    {
        private static int _nextId = 0;
        private readonly int _id;
        private readonly string? _methodName;

        public int Id => _id;
        public string? MethodName => _methodName;

        public CoroutineHandle(int id, string? methodName = null)
        {
            _id = id;
            _methodName = methodName;
        }

        public static CoroutineHandle Invalid => new CoroutineHandle(-1);

        public bool IsValid => _id >= 0;

        public bool Equals(CoroutineHandle other)
        {
            return _id == other._id;
        }

        public override bool Equals(object? obj)
        {
            return obj is CoroutineHandle other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public static bool operator ==(CoroutineHandle left, CoroutineHandle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(CoroutineHandle left, CoroutineHandle right)
        {
            return !left.Equals(right);
        }
    }

    public class CoroutineRunner
    {
        private readonly List<ActiveCoroutine> _activeCoroutines;
        private readonly Queue<ActiveCoroutine> _toStart;
        private readonly Queue<ActiveCoroutine> _toStop;
        private int _nextHandleId;

        public CoroutineRunner()
        {
            _activeCoroutines = new List<ActiveCoroutine>();
            _toStart = new Queue<ActiveCoroutine>();
            _toStop = new Queue<ActiveCoroutine>();
            _nextHandleId = 1;
        }

        public CoroutineHandle StartCoroutine(IEnumerator coroutine, string? methodName = null)
        {
            if (coroutine == null)
            {
                return CoroutineHandle.Invalid;
            }

            var handle = new CoroutineHandle(_nextHandleId++, methodName);
            var activeCoroutine = new ActiveCoroutine(handle, coroutine);
            _toStart.Enqueue(activeCoroutine);
            return handle;
        }

        public void StopCoroutine(CoroutineHandle handle)
        {
            var coroutine = FindCoroutine(handle);
            if (coroutine != null)
            {
                _toStop.Enqueue(coroutine);
            }
        }

        public void StopCoroutine(IEnumerator coroutine)
        {
            var active = _activeCoroutines.FirstOrDefault(c => c.Enumerator == coroutine);
            if (active != null)
            {
                _toStop.Enqueue(active);
            }
        }

        public void StopCoroutine(string methodName)
        {
            var toStop = _activeCoroutines.Where(c => c.Handle.MethodName == methodName).ToList();
            foreach (var coroutine in toStop)
            {
                _toStop.Enqueue(coroutine);
            }
        }

        public void StopAllCoroutines()
        {
            foreach (var coroutine in _activeCoroutines)
            {
                _toStop.Enqueue(coroutine);
            }
        }

        public void Update(float deltaTime)
        {
            while (_toStart.Count > 0)
            {
                var coroutine = _toStart.Dequeue();
                _activeCoroutines.Add(coroutine);
                MoveNext(coroutine, deltaTime);
            }

            while (_toStop.Count > 0)
            {
                var coroutine = _toStop.Dequeue();
                _activeCoroutines.Remove(coroutine);
            }

            foreach (var coroutine in _activeCoroutines.ToList())
            {
                MoveNext(coroutine, deltaTime);
            }
        }

        private void MoveNext(ActiveCoroutine coroutine, float deltaTime)
        {
            if (!coroutine.Enumerator.MoveNext())
            {
                _toStop.Enqueue(coroutine);
                return;
            }

            var current = coroutine.Enumerator.Current;

            if (current is WaitForSeconds wait)
            {
                coroutine.CurrentWait = wait;
                wait.Initialize(this, coroutine, deltaTime);
            }
            else if (current is WaitForFixedUpdate)
            {
                coroutine.WaitForFixedUpdate = true;
            }
            else if (current is WaitForEndOfFrame)
            {
                coroutine.WaitForEndOfFrame = true;
            }
            else if (current is WaitUntil waitUntil)
            {
                coroutine.CurrentWaitUntil = waitUntil;
            }
            else if (current is WaitWhile waitWhile)
            {
                coroutine.CurrentWaitWhile = waitWhile;
            }
            else if (current is CoroutineHandle handle)
            {
                var nested = FindCoroutine(handle);
                if (nested != null)
                {
                    coroutine.NestedCoroutines.Add(nested);
                }
            }
        }

        private ActiveCoroutine? FindCoroutine(CoroutineHandle handle)
        {
            return _activeCoroutines.FirstOrDefault(c => c.Handle == handle);
        }

        public bool IsRunning(CoroutineHandle handle)
        {
            return FindCoroutine(handle) != null;
        }

        private class ActiveCoroutine
        {
            public CoroutineHandle Handle { get; }
            public IEnumerator Enumerator { get; }
            public WaitForSeconds? CurrentWait { get; set; }
            public WaitUntil? CurrentWaitUntil { get; set; }
            public WaitWhile? CurrentWaitWhile { get; set; }
            public bool WaitForFixedUpdate { get; set; }
            public bool WaitForEndOfFrame { get; set; }
            public List<ActiveCoroutine> NestedCoroutines { get; }

            public ActiveCoroutine(CoroutineHandle handle, IEnumerator enumerator)
            {
                Handle = handle;
                Enumerator = enumerator;
                NestedCoroutines = new List<ActiveCoroutine>();
            }
        }
    }

    public class WaitForSeconds : CustomYieldInstruction
    {
        private readonly float _seconds;
        private float _remainingTime;
        private bool _isDone;
        private CoroutineRunner? _runner;
        private object? _coroutine;

        public float Seconds => _seconds;
        public bool IsDone => _isDone;

        public override bool KeepWaiting => !_isDone;

        public WaitForSeconds(float seconds)
        {
            _seconds = Mathf.Max(0f, seconds);
            _remainingTime = _seconds;
            _isDone = false;
        }

        internal void Initialize(CoroutineRunner runner, object coroutine, float deltaTime)
        {
            _runner = runner;
            _coroutine = coroutine;
            _remainingTime -= deltaTime;
            if (_remainingTime <= 0f)
            {
                _isDone = true;
            }
        }

        public void Update(float deltaTime)
        {
            if (!_isDone && _runner != null)
            {
                _remainingTime -= deltaTime;
                if (_remainingTime <= 0f)
                {
                    _isDone = true;
                }
            }
        }
    }

    public class WaitForFixedUpdate : CustomYieldInstruction
    {
        public override bool KeepWaiting => false;
    }

    public class WaitForEndOfFrame : CustomYieldInstruction
    {
        public override bool KeepWaiting => false;
    }

    public class WaitUntil : CustomYieldInstruction
    {
        private readonly Func<bool> _predicate;

        public override bool KeepWaiting => !_predicate();

        public WaitUntil(Func<bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }
    }

    public class WaitWhile : CustomYieldInstruction
    {
        private readonly Func<bool> _predicate;

        public override bool KeepWaiting => _predicate();

        public WaitWhile(Func<bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }
    }

    public abstract class CustomYieldInstruction
    {
        public abstract bool KeepWaiting { get; }
    }

    public static class GameObject
    {
        private static Scene? _currentScene;

        public static void SetCurrentScene(Scene? scene)
        {
            _currentScene = scene;
        }

        public static WPFEngine.ECS.GameObject? Find(string name)
        {
            if (_currentScene == null) return null;

            foreach (var go in _currentScene.GameObjects)
            {
                if (go.Name == name)
                    return go;

                var found = FindInChildren(go, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static WPFEngine.ECS.GameObject? FindInChildren(WPFEngine.ECS.GameObject parent, string name)
        {
            foreach (var child in parent.Children)
            {
                if (child.Name == name)
                    return child;

                var found = FindInChildren(child, name);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static WPFEngine.ECS.GameObject? FindWithTag(string tag)
        {
            if (_currentScene == null) return null;

            foreach (var go in _currentScene.GameObjects)
            {
                if (go.Tag == tag)
                    return go;

                var found = FindWithTagInChildren(go, tag);
                if (found != null)
                    return found;
            }
            return null;
        }

        private static WPFEngine.ECS.GameObject? FindWithTagInChildren(WPFEngine.ECS.GameObject parent, string tag)
        {
            foreach (var child in parent.Children)
            {
                if (child.Tag == tag)
                    return child;

                var found = FindWithTagInChildren(child, tag);
                if (found != null)
                    return found;
            }
            return null;
        }

        public static WPFEngine.ECS.GameObject[] FindGameObjectsWithTag(string tag)
        {
            if (_currentScene == null) return Array.Empty<WPFEngine.ECS.GameObject>();

            var results = new List<WPFEngine.ECS.GameObject>();
            foreach (var go in _currentScene.GameObjects)
            {
                CollectWithTag(go, tag, results);
            }
            return results.ToArray();
        }

        private static void CollectWithTag(WPFEngine.ECS.GameObject parent, string tag, List<WPFEngine.ECS.GameObject> results)
        {
            if (parent.Tag == tag)
                results.Add(parent);

            foreach (var child in parent.Children)
            {
                CollectWithTag(child, tag, results);
            }
        }

        public static WPFEngine.ECS.GameObject[] FindObjectsOfType<T>() where T : class
        {
            return _currentScene?.FindObjectsOfType<T>().ToArray() ?? Array.Empty<WPFEngine.ECS.GameObject>().OfType<T>().ToArray();
        }

        public static T? FindObjectOfType<T>() where T : class
        {
            return _currentScene?.FindObjectOfType<T>();
        }

        public static WPFEngine.ECS.GameObject CreatePrimitive(PrimitiveType type)
        {
            var go = new WPFEngine.ECS.GameObject($"Primitive_{type}");
            return go;
        }

        public static WPFEngine.ECS.GameObject Instantiate(WPFEngine.ECS.GameObject original)
        {
            if (original == null) return null!;
            return original.Clone();
        }

        public static WPFEngine.ECS.GameObject Instantiate(WPFEngine.ECS.GameObject original, Vector2 position, float rotation)
        {
            if (original == null) return null!;
            var clone = original.Clone();
            clone.Transform.LocalPosition = position;
            clone.Transform.LocalRotation = rotation;
            return clone;
        }

        public static WPFEngine.ECS.GameObject Instantiate(WPFEngine.ECS.GameObject original, Vector2 position, float rotation, WPFEngine.ECS.GameObject? parent)
        {
            if (original == null) return null!;
            var clone = original.Clone();
            clone.Transform.LocalPosition = position;
            clone.Transform.LocalRotation = rotation;
            if (parent != null)
            {
                clone.Parent = parent;
            }
            return clone;
        }
    }

    public static class Transform
    {
        public static void SetParent(Transform? child, Transform? parent)
        {
            if (child != null)
            {
                child.Parent = parent;
            }
        }

        public static void SetParent(Transform? child, Transform? parent, bool worldPositionStays)
        {
            if (child == null) return;

            if (worldPositionStays)
            {
                var worldPos = child.WorldPosition;
                var worldRot = child.WorldRotation;
                var worldScale = child.WorldScale;

                child.Parent = parent;

                child.SetWorldPosition(worldPos);
                child.SetWorldRotation(worldRot);
                child.SetWorldScale(worldScale);
            }
            else
            {
                child.Parent = parent;
            }
        }

        public static Vector2 GetPosition(Transform transform)
        {
            return transform.WorldPosition;
        }

        public static void SetPosition(Transform transform, Vector2 position)
        {
            transform.Position = position;
        }

        public static Vector2 GetLocalPosition(Transform transform)
        {
            return transform.LocalPosition;
        }

        public static void SetLocalPosition(Transform transform, Vector2 position)
        {
            transform.LocalPosition = position;
        }

        public static float GetRotation(Transform transform)
        {
            return transform.WorldRotation;
        }

        public static void SetRotation(Transform transform, float rotation)
        {
            transform.Rotation = rotation;
        }

        public static float GetLocalRotation(Transform transform)
        {
            return transform.LocalRotation;
        }

        public static void SetLocalRotation(Transform transform, float rotation)
        {
            transform.LocalRotation = rotation;
        }

        public static Vector2 GetScale(Transform transform)
        {
            return transform.WorldScale;
        }

        public static void SetScale(Transform transform, Vector2 scale)
        {
            transform.Scale = scale;
        }

        public static Vector2 GetLocalScale(Transform transform)
        {
            return transform.LocalScale;
        }

        public static void SetLocalScale(Transform transform, Vector2 scale)
        {
            transform.LocalScale = scale;
        }

        public static void Translate(Transform transform, Vector2 translation)
        {
            transform.Translate(translation);
        }

        public static void Translate(Transform transform, Vector2 translation, Space relativeTo)
        {
            if (relativeTo == Space.World)
            {
                transform.Position += translation;
            }
            else
            {
                transform.LocalPosition += translation;
            }
        }

        public static void Rotate(Transform transform, float angle)
        {
            transform.Rotate(angle);
        }

        public static void Rotate(Transform transform, float angle, Space relativeTo)
        {
            if (relativeTo == Space.World)
            {
                transform.Rotation += angle;
            }
            else
            {
                transform.LocalRotation += angle;
            }
        }

        public static void LookAt(Transform transform, Vector2 target)
        {
            transform.LookAt(target);
        }

        public static Vector2 TransformPoint(Transform transform, Vector2 point)
        {
            return transform.TransformPoint(point);
        }

        public static Vector2 InverseTransformPoint(Transform transform, Vector2 point)
        {
            return transform.InverseTransformPoint(point);
        }

        public static Vector2 TransformDirection(Transform transform, Vector2 direction)
        {
            return transform.TransformDirection(direction);
        }

        public static Vector2 InverseTransformDirection(Transform transform, Vector2 direction)
        {
            return transform.InverseTransformDirection(direction);
        }

        public static float GetAngle(Transform transform, Vector2 direction)
        {
            return transform.GetAngle(direction);
        }

        public static float GetSignedAngle(Transform transform, Vector2 to)
        {
            return transform.GetSignedAngle(to);
        }

        public static Transform? FindChild(Transform parent, string name)
        {
            return parent.Find(name)?.Transform;
        }

        public static void DetachChildren(Transform parent)
        {
            parent.DetachChildren();
        }

        public static Transform? GetChild(Transform parent, int index)
        {
            return parent.GetChild(index);
        }

        public static int GetChildCount(Transform parent)
        {
            return parent.ChildCount;
        }

        public static Transform? GetParent(Transform transform)
        {
            return transform.Parent;
        }
    }

    public enum Space
    {
        Self,
        World
    }

    public enum PrimitiveType
    {
        Sphere,
        Capsule,
        Cylinder,
        Cube,
        Plane,
        Quad
    }

    public static class SceneManagement
    {
        public static void LoadScene(string sceneName, bool additive = false)
        {
            SceneManager.Instance.LoadScene(sceneName, additive);
        }

        public static void LoadScene(int sceneIndex, bool additive = false)
        {
            var scenes = SceneManager.Instance.Scenes;
            if (sceneIndex >= 0 && sceneIndex < scenes.Count)
            {
                var scene = scenes.ElementAt(sceneIndex);
                SceneManager.Instance.LoadScene(scene.Key, additive);
            }
        }

        public static Scene? GetActiveScene()
        {
            return SceneManager.Instance.CurrentScene;
        }

        public static string GetActiveSceneName()
        {
            return SceneManager.Instance.CurrentScene?.Name ?? string.Empty;
        }

        public static Guid GetActiveSceneHandle()
        {
            return SceneManager.Instance.CurrentScene?.InstanceId ?? Guid.Empty;
        }

        public static void UnloadScene(string sceneName)
        {
            SceneManager.Instance.UnloadScene(sceneName);
        }

        public static AsyncOperation LoadSceneAsync(string sceneName, bool additive = false)
        {
            return new AsyncOperation();
        }

        public static AsyncOperation UnloadSceneAsync(string sceneName)
        {
            return new AsyncOperation();
        }

        public static void SetActiveScene(Scene scene)
        {
            ScriptContext.Instance.ActiveScene = scene;
        }
    }

    public class AsyncOperation
    {
        private bool _isDone;
        private float _progress;
        private Action? _callback;

        public bool IsDone => _isDone;
        public float Progress => _progress;
        public float Priority { get; set; }

        public event Action<AsyncOperation>? Completed;

        public AsyncOperation()
        {
            _isDone = false;
            _progress = 0f;
            Priority = 0f;
        }

        public void Update(float deltaTime)
        {
            if (_isDone) return;

            _progress += deltaTime * 0.5f;
            if (_progress >= 1f)
            {
                _progress = 1f;
                _isDone = true;
                Completed?.Invoke(this);
                _callback?.Invoke();
            }
        }

        public AsyncOperation OnCompleted(Action callback)
        {
            _callback = callback;
            if (_isDone)
            {
                callback?.Invoke();
            }
            return this;
        }
    }

    public static class Time
    {
        private static GameTime? _gameTime;

        public static void SetGameTime(GameTime gameTime)
        {
            _gameTime = gameTime;
        }

        public static float deltaTime => _gameTime?.DeltaTimeSeconds ?? 0f;
        public static float unscaledDeltaTime => _gameTime?.UnscaledDeltaTimeSeconds ?? 0f;
        public static float time => _gameTime?.TotalTimeSeconds ?? 0f;
        public static float timeScale
        {
            get => (float)(_gameTime?.TimeScale ?? 1.0);
            set
            {
                if (_gameTime != null)
                    _gameTime.TimeScale = value;
            }
        }
        public static int frameCount => _gameTime?.FrameCount ?? 0;
        public static float framesPerSecond => _gameTime?.FramesPerSecond ?? 0f;
        public static float fixedDeltaTime => 0.02f;
        public static float maximumDeltaTime => 0.333333f;
        public static float smoothDeltaTime => deltaTime;
        public static float realtimeSinceStartup => (float)DateTime.Now.TimeOfDay.TotalSeconds;
        public static float fixedTime => time;
        public static float timeSinceLevelLoad => time;
        public static float captureFramerate
        {
            get => 0;
            set { }
        }

        public static bool inFixedTimeStep => false;

        public static void SetTimeScale(float scale)
        {
            if (_gameTime != null)
                _gameTime.TimeScale = scale;
        }

        public static void Pause()
        {
            _gameTime?.Pause();
        }

        public static void Unpause()
        {
            _gameTime?.Resume();
        }
    }

    public static class Input
    {
        private static InputManager? _inputManager;

        public static void SetInputManager(InputManager inputManager)
        {
            _inputManager = inputManager;
        }

        public static bool GetKey(KeyCode key)
        {
            return _inputManager?.GetKey(key) ?? false;
        }

        public static bool GetKeyDown(KeyCode key)
        {
            return _inputManager?.GetKeyDown(key) ?? false;
        }

        public static bool GetKeyUp(KeyCode key)
        {
            return _inputManager?.GetKeyUp(key) ?? false;
        }

        public static bool GetMouseButton(int button)
        {
            return _inputManager?.GetMouseButton(button) ?? false;
        }

        public static bool GetMouseButtonDown(int button)
        {
            return _inputManager?.GetMouseButtonDown(button) ?? false;
        }

        public static bool GetMouseButtonUp(int button)
        {
            return _inputManager?.GetMouseButtonUp(button) ?? false;
        }

        public static Vector2 mousePosition => _inputManager?.MousePosition ?? Vector2.Zero;
        public static Vector2 mouseScrollDelta => new Vector2(0, _inputManager?.MouseScrollDelta ?? 0f);

        public static bool GetJoystickButton(int button)
        {
            return false;
        }

        public static bool GetJoystickButtonDown(int button)
        {
            return false;
        }

        public static bool GetJoystickButtonUp(int button)
        {
            return false;
        }

        public static float GetAxis(string axisName)
        {
            if (axisName == "Horizontal")
                return _inputManager?.GetAxis(InputAxis.Horizontal) ?? 0f;
            if (axisName == "Vertical")
                return _inputManager?.GetAxis(InputAxis.Vertical) ?? 0f;
            if (axisName == "Mouse X")
                return _inputManager?.GetAxis(InputAxis.MouseX) ?? 0f;
            if (axisName == "Mouse Y")
                return _inputManager?.GetAxis(InputAxis.MouseY) ?? 0f;
            if (axisName == "Mouse ScrollWheel")
                return _inputManager?.GetAxis(InputAxis.ScrollWheel) ?? 0f;
            return 0f;
        }

        public static float GetAxisRaw(string axisName)
        {
            if (axisName == "Horizontal")
                return _inputManager?.GetAxisRaw(InputAxis.Horizontal) ?? 0f;
            if (axisName == "Vertical")
                return _inputManager?.GetAxisRaw(InputAxis.Vertical) ?? 0f;
            return 0f;
        }

        public static bool GetButton(string buttonName)
        {
            return _inputManager?.GetActionPressed(buttonName) ?? false;
        }

        public static bool GetButtonDown(string buttonName)
        {
            return _inputManager?.GetActionDown(buttonName) ?? false;
        }

        public static bool GetButtonUp(string buttonName)
        {
            return _inputManager?.GetActionUp(buttonName) ?? false;
        }

        public static void ResetInputAxes()
        {
        }

        public static bool IsJoystickConnected(int joystickId)
        {
            return false;
        }

        public static string GetJoystickNames()
        {
            return string.Empty;
        }
    }

    public static class Debug
    {
        public static void Log(object? message)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG] {message}");
        }

        public static void Log(object? message, UnityEngine.Object? context)
        {
            System.Diagnostics.Debug.WriteLine($"[LOG] {message} (Context: {context})");
        }

        public static void LogWarning(object? message)
        {
            System.Diagnostics.Debug.WriteLine($"[WARNING] {message}");
        }

        public static void LogWarning(object? message, UnityEngine.Object? context)
        {
            System.Diagnostics.Debug.WriteLine($"[WARNING] {message} (Context: {context})");
        }

        public static void LogError(object? message)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] {message}");
        }

        public static void LogError(object? message, UnityEngine.Object? context)
        {
            System.Diagnostics.Debug.WriteLine($"[ERROR] {message} (Context: {context})");
        }

        public static void LogException(Exception exception)
        {
            System.Diagnostics.Debug.WriteLine($"[EXCEPTION] {exception}");
        }

        public static void LogException(Exception exception, UnityEngine.Object? context)
        {
            System.Diagnostics.Debug.WriteLine($"[EXCEPTION] {exception} (Context: {context})");
        }

        public static void DrawLine(Vector2 start, Vector2 end)
        {
            DrawLine(start, end, Color.white, 0f, false);
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color)
        {
            DrawLine(start, end, color, 0f, false);
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float duration)
        {
            DrawLine(start, end, color, duration, false);
        }

        public static void DrawLine(Vector2 start, Vector2 end, Color color, float duration, bool depthTest)
        {
            System.Diagnostics.Debug.WriteLine($"[DEBUG] Line: {start} -> {end}, Color: {color}");
        }

        public static void DrawRay(Vector2 start, Vector2 direction)
        {
            DrawRay(start, direction, Color.white, 0f, false);
        }

        public static void DrawRay(Vector2 start, Vector2 direction, Color color)
        {
            DrawRay(start, direction, color, 0f, false);
        }

        public static void DrawRay(Vector2 start, Vector2 direction, Color color, float duration)
        {
            DrawRay(start, direction, color, duration, false);
        }

        public static void DrawRay(Vector2 start, Vector2 direction, Color color, float duration, bool depthTest)
        {
            var end = start + direction;
            DrawLine(start, end, color, duration, depthTest);
        }

        public static void ClearDeveloperConsole()
        {
        }

        public static bool isDebugBuild => true;
    }

    public class Color
    {
        public float r, g, b, a;

        public static Color white => new Color(1f, 1f, 1f, 1f);
        public static Color black => new Color(0f, 0f, 0f, 1f);
        public static Color red => new Color(1f, 0f, 0f, 1f);
        public static Color green => new Color(0f, 1f, 0f, 1f);
        public static Color blue => new Color(0f, 0f, 1f, 1f);
        public static Color yellow => new Color(1f, 0.92f, 0.23f, 1f);
        public static Color cyan => new Color(0f, 1f, 1f, 1f);
        public static Color magenta => new Color(1f, 0f, 1f, 1f);
        public static Color gray => new Color(0.5f, 0.5f, 0.5f, 1f);
        public static Color clear => new Color(0f, 0f, 0f, 0f);

        public float this[int index]
        {
            get
            {
                return index switch
                {
                    0 => r,
                    1 => g,
                    2 => b,
                    3 => a,
                    _ => throw new IndexOutOfRangeException()
                };
            }
            set
            {
                switch (index)
                {
                    case 0: r = value; break;
                    case 1: g = value; break;
                    case 2: b = value; break;
                    case 3: a = value; break;
                    default: throw new IndexOutOfRangeException();
                }
            }
        }

        public Color(float r, float g, float b, float a = 1f)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public static Color Lerp(Color a, Color b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color(
                a.r + (b.r - a.r) * t,
                a.g + (b.g - a.g) * t,
                a.b + (b.b - a.b) * t,
                a.a + (b.a - a.a) * t
            );
        }

        public static Color operator +(Color a, Color b)
        {
            return new Color(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
        }

        public static Color operator -(Color a, Color b)
        {
            return new Color(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
        }

        public static Color operator *(Color a, float d)
        {
            return new Color(a.r * d, a.g * d, a.b * d, a.a * d);
        }

        public static Color operator *(float d, Color a)
        {
            return a * d;
        }

        public static bool operator ==(Color lhs, Color rhs)
        {
            return Mathf.Approximately(lhs.r, rhs.r) &&
                   Mathf.Approximately(lhs.g, rhs.g) &&
                   Mathf.Approximately(lhs.b, rhs.b) &&
                   Mathf.Approximately(lhs.a, rhs.a);
        }

        public static bool operator !=(Color lhs, Color rhs)
        {
            return !(lhs == rhs);
        }

        public override bool Equals(object? obj)
        {
            return obj is Color other && this == other;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(r, g, b, a);
        }

        public override string ToString()
        {
            return $"({r:F2}, {g:F2}, {b:F2}, {a:F2})";
        }

        public string ToString(string format)
        {
            return $"({r.ToString(format)}, {g.ToString(format)}, {b.ToString(format)}, {a.ToString(format)})";
        }

        public Color(Rendering.Color color)
        {
            r = color.R / 255f;
            g = color.G / 255f;
            b = color.B / 255f;
            a = color.A / 255f;
        }

        public Rendering.Color ToWPFColor()
        {
            return new Rendering.Color(
                (byte)(r * 255),
                (byte)(g * 255),
                (byte)(b * 255),
                (byte)(a * 255)
            );
        }
    }

    public static class Mathf
    {
        public static float PI = (float)Math.PI;
        public static float TwoPI = (float)(Math.PI * 2);
        public static float Infinity = float.PositiveInfinity;
        public static float NegativeInfinity = float.NegativeInfinity;
        public static float Deg2Rad = (float)(Math.PI / 180.0);
        public static float Rad2Deg = (float)(180.0 / Math.PI);
        public static float Epsilon = float.Epsilon;

        public static float Abs(float f) => Math.Abs(f);
        public static int Abs(int value) => Math.Abs(value);
        public static float Acos(float f) => (float)Math.Acos(f);
        public static float Asin(float f) => (float)Math.Asin(f);
        public static float Atan(float f) => (float)Math.Atan(f);
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Ceil(float f) => (float)Math.Ceiling(f);
        public static int CeilToInt(float f) => (int)Math.Ceiling(f);
        public static float Clamp(float value, float min, float max) => Math.Max(min, Math.Min(max, value));
        public static int Clamp(int value, int min, int max) => Math.Max(min, Math.Min(max, value));
        public static float Clamp01(float value) => Clamp(value, 0f, 1f);
        public static float Cos(float f) => (float)Math.Cos(f);
        public static float DeltaAngle(float current, float target) => Repeat(target - current, 360f);
        public static float Floor(float f) => (float)Math.Floor(f);
        public static int FloorToInt(float f) => (int)Math.Floor(f);
        public static float InverseLerp(Vector2 from, Vector2 to, Vector2 value)
        {
            return Clamp01(Vector2.Dot(to - from, value - from) / Vector2.Dot(to - from, to - from));
        }

        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float LerpUnclamped(float a, float b, float t) => a + (b - a) * t;
        public static float Log(float f, float p) => (float)Math.Log(f, p);
        public static float Log(float f) => (float)Math.Log(f);
        public static float Log10(float f) => (float)Math.Log10(f);
        public static float Max(float a, float b) => Math.Max(a, b);
        public static float Max(params float[] values) => values.Length > 0 ? values.Max() : 0f;
        public static int Max(int a, int b) => Math.Max(a, b);
        public static int Max(params int[] values) => values.Length > 0 ? values.Max() : 0;
        public static float Min(float a, float b) => Math.Min(a, b);
        public static float Min(params float[] values) => values.Length > 0 ? values.Min() : 0f;
        public static int Min(int a, int b) => Math.Min(a, b);
        public static int Min(params int[] values) => values.Length > 0 ? values.Min() : 0;
        public static float MoveTowards(float current, float target, float maxDelta) => Mathf.Abs(target - current) <= maxDelta ? target : current + Math.Sign(target - current) * maxDelta;
        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float deltaAngle = DeltaAngle(current, target);
            return Mathf.Abs(deltaAngle) <= maxDelta ? target : current + Math.Sign(deltaAngle) * maxDelta;
        }

        public static float PerlinNoise(float x, float y) => (float)(new Random().NextDouble() * 0.5 + 0.25);
        public static float Pow(float f, float p) => (float)Math.Pow(f, p);
        public static float Repeat(float t, float length) => Clamp(t - Floor(t / length) * length, 0f, length);
        public static float Round(float f) => (float)Math.Round(f);
        public static int RoundToInt(float f) => (int)Math.Round(f);
        public static float Sign(float f) => f >= 0f ? 1f : -1f;
        public static float Sin(float f) => (float)Math.Sin(f);
        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
        {
            smoothTime = Mathf.Max(0.0001f, smoothTime);
            float omega = 2f / smoothTime;
            float x = omega * deltaTime;
            float exp = 1f / (1f + x + 0.48f * x * x + 0.235f * x * x * x);
            float change = current - target;
            float maxChange = maxSpeed * smoothTime;
            change = Clamp(change, -maxChange, maxChange);
            float newTarget = current - change;
            float temp = (currentVelocity + omega * change) * deltaTime;
            currentVelocity = (currentVelocity - omega * temp) * exp;
            float output = newTarget + (change + temp) * exp;
            if (newTarget - current > 0.0f == output > newTarget)
            {
                output = newTarget;
                currentVelocity = (output - newTarget) / deltaTime;
            }
            return output;
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime)
        {
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, deltaTime, float.PositiveInfinity);
        }

        public static float SmoothDamp(float current, float target, ref float currentVelocity, float smoothTime)
        {
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, Time.deltaTime, float.PositiveInfinity);
        }

        public static float SmoothStep(float from, float to, float t)
        {
            t = Clamp01(t);
            t = -2f * t * t * t + 3f * t * t;
            return to * t + from * (1f - t);
        }

        public static float Tan(float f) => (float)Math.Tan(f);
        public static float SmoothDampAngle(float current, float target, ref float currentVelocity, float smoothTime, float deltaTime, float maxSpeed = float.PositiveInfinity)
        {
            target = current + DeltaAngle(current, target);
            return SmoothDamp(current, target, ref currentVelocity, smoothTime, deltaTime, maxSpeed);
        }

        public static bool Approximately(float a, float b)
        {
            return Math.Abs(b - a) < Math.Max(0.000001f * Math.Max(Math.Abs(a), Math.Abs(b)), float.Epsilon * 8);
        }

        public static float ClosestPowerOfTwo(float value) => (float)Math.Pow(2, Math.Round(Math.Log(value) / Math.Log(2)));
        public static float CorrelatedColorTemperatureToRGB(float kelvin)
        {
            float temp = kelvin / 100f;
            float red, green, blue;

            if (temp <= 66f)
            {
                red = 255f;
                green = (float)(99.4708025861 * Math.Log(temp) - 161.1195681661);
                blue = temp <= 19f ? 0f : (float)(138.5177312231 * Math.Log(temp - 10) - 305.0447927307);
            }
            else
            {
                red = (float)(329.698727446 * Math.Pow(temp - 60, -0.1332047592));
                green = (float)(288.1221695283 * Math.Pow(temp - 60, -0.0755148492));
                blue = 255f;
            }

            return Clamp(red, 0f, 255f) * 0.00392156863f + Clamp(green, 0f, 255f) * 0.00392156863f * 256 + Clamp(blue, 0f, 255f) * 0.00392156863f * 65536;
        }

        public static float GammaToLinearSpace(float value) => (float)Math.Pow(value, 2.2);
        public static float LinearToGammaSpace(float value) => (float)Math.Pow(value, 1f / 2.2);
        public static int NextPowerOfTwo(float value) => (int)Math.Pow(2, Math.Ceiling(Math.Log(value, 2)));
        public static float InverseLerp(float a, float b, float value) => Clamp01((value - a) / (b - a));
        public static float LerpAngle(float a, float b, float t)
        {
            float delta = DeltaAngle(a, b);
            return a + delta * Clamp01(t);
        }
    }

    namespace UnityEngine
    {
        public class Object
        {
            public string name { get; set; } = string.Empty;
            public static void Destroy(Object? obj) { }
            public static void DestroyImmediate(Object? obj) { }
            public static void DestroyObject(Object? obj) { }
            public static T[] FindObjectsOfType<T>() where T : Object => Array.Empty<T>();
            public static T? FindObjectOfType<T>() where T : Object => null;
            public static void DontDestroyOnLoad(Object? obj) { }
        }
    }

    public class ScriptCompiler
    {
        private readonly Dictionary<string, Type> _compiledTypes;
        private readonly List<ScriptCompilationError> _errors;
        private bool _roslynAvailable;

        public ScriptCompiler()
        {
            _compiledTypes = new Dictionary<string, Type>();
            _errors = new List<ScriptCompilationError>();
            _roslynAvailable = CheckRoslynAvailability();
        }

        private bool CheckRoslynAvailability()
        {
            try
            {
                var roslynType = Type.GetType("Microsoft.CodeAnalysis.CSharp.CSharpCompilation, Microsoft.CodeAnalysis");
                return roslynType != null;
            }
            catch
            {
                return false;
            }
        }

        public bool IsRoslynAvailable => _roslynAvailable;

        public IReadOnlyList<ScriptCompilationError> Errors => _errors;

        public CompiledScript Compile(string sourceCode, string className)
        {
            _errors.Clear();

            if (string.IsNullOrWhiteSpace(sourceCode))
            {
                _errors.Add(new ScriptCompilationError
                {
                    ErrorText = "Source code cannot be empty",
                    Line = 0,
                    Column = 0
                });
                return new CompiledScript(null, _errors.ToList());
            }

            if (_roslynAvailable)
            {
                return CompileWithRoslyn(sourceCode, className);
            }
            else
            {
                return CompileWithLightweightParser(sourceCode, className);
            }
        }

        private CompiledScript CompileWithRoslyn(string sourceCode, string className)
        {
            try
            {
                var syntaxTree = Microsoft.CodeAnalysis.CSharp.CSharpSyntaxTree.ParseText(sourceCode);
                var references = new List<Microsoft.CodeAnalysis.MetadataReference>
                {
                    Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
                    Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Console).Assembly.Location),
                    Microsoft.CodeAnalysis.MetadataReference.CreateFromFile(typeof(Script).Assembly.Location)
                };

                var compilation = Microsoft.CodeAnalysis.CSharp.CSharpCompilation.Create(
                    $"Dynamic_{className}_{Guid.NewGuid():N}",
                    new[] { syntaxTree },
                    references,
                    new Microsoft.CodeAnalysis.CSharp.CSharpCompilationOptions(Microsoft.CodeAnalysis.OutputKind.DynamicallyLinkedLibrary)
                );

                using var ms = new System.IO.MemoryStream();
                var result = compilation.Emit(ms);

                if (result.Success)
                {
                    ms.Seek(0, System.IO.SeekOrigin.Begin);
                    var assembly = System.Reflection.Assembly.Load(ms.ToArray());
                    var type = assembly.GetType(className);
                    return new CompiledScript(type, _errors.ToList());
                }
                else
                {
                    foreach (var diagnostic in result.Diagnostics.Where(d => d.Severity == Microsoft.CodeAnalysis.DiagnosticSeverity.Error))
                    {
                        _errors.Add(new ScriptCompilationError
                        {
                            ErrorText = diagnostic.GetMessage(),
                            Line = (int)diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1,
                            Column = (int)diagnostic.Location.GetLineSpan().StartLinePosition.Character + 1
                        });
                    }
                    return new CompiledScript(null, _errors.ToList());
                }
            }
            catch (Exception ex)
            {
                _errors.Add(new ScriptCompilationError
                {
                    ErrorText = ex.Message,
                    Line = 0,
                    Column = 0
                });
                return new CompiledScript(null, _errors.ToList());
            }
        }

        private CompiledScript CompileWithLightweightParser(string sourceCode, string className)
        {
            var lines = sourceCode.Split('\n');
            var errors = new List<string>();

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.Contains("class " + className))
                {
                    continue;
                }

                if (trimmed.StartsWith("using ") || trimmed.StartsWith("namespace ") ||
                    trimmed.StartsWith("{") || trimmed.StartsWith("}") ||
                    trimmed.StartsWith("public ") || trimmed.StartsWith("private ") ||
                    trimmed.StartsWith("protected ") || trimmed.StartsWith("internal ") ||
                    trimmed == "" || trimmed.StartsWith("//"))
                {
                    continue;
                }

                if (trimmed.Contains("(") && trimmed.Contains(")"))
                {
                    continue;
                }
            }

            _errors.Add(new ScriptCompilationError
            {
                ErrorText = "Lightweight parser: Unable to compile complex C# code. Roslyn is recommended.",
                Line = 0,
                Column = 0
            });

            return new CompiledScript(null, _errors.ToList());
        }

        public Type? GetCompiledType(string name)
        {
            return _compiledTypes.TryGetValue(name, out var type) ? type : null;
        }

        public void ClearCompiledTypes()
        {
            _compiledTypes.Clear();
        }

        public bool ValidateSyntax(string sourceCode)
        {
            if (string.IsNullOrWhiteSpace(sourceCode))
                return false;

            var openBraces = sourceCode.Count(c => c == '{');
            var closeBraces = sourceCode.Count(c => c == '}');
            var openParens = sourceCode.Count(c => c == '(');
            var closeParens = sourceCode.Count(c => c == ')');

            return openBraces == closeBraces && openParens == closeParens;
        }
    }

    public class ScriptCompilationError
    {
        public string ErrorText { get; set; } = string.Empty;
        public int Line { get; set; }
        public int Column { get; set; }
        public string FileName { get; set; } = string.Empty;

        public override string ToString()
        {
            return string.IsNullOrEmpty(FileName)
                ? $"Line {Line}, Column {Column}: {ErrorText}"
                : $"{FileName}({Line},{Column}): {ErrorText}";
        }
    }

    public class CompiledScript
    {
        public Type? Type { get; }
        public IReadOnlyList<ScriptCompilationError> Errors { get; }
        public bool IsValid => Type != null && Errors.Count == 0;

        public CompiledScript(Type? type, List<ScriptCompilationError> errors)
        {
            Type = type;
            Errors = errors.AsReadOnly();
        }

        public Script? CreateInstance()
        {
            if (Type == null || !typeof(Script).IsAssignableFrom(Type))
                return null;

            return Activator.CreateInstance(Type) as Script;
        }
    }

    public class ScriptHotReload
    {
        private readonly Dictionary<Type, ScriptReloadInfo> _reloadInfos;
        private readonly ScriptCompiler _compiler;
        private bool _isReloading;

        public ScriptHotReload()
        {
            _reloadInfos = new Dictionary<Type, ScriptReloadInfo>();
            _compiler = new ScriptCompiler();
            _isReloading = false;
        }

        public bool IsReloading => _isReloading;

        public void RegisterScript(Script script)
        {
            var type = script.GetType();
            var info = new ScriptReloadInfo
            {
                OriginalScript = script,
                ScriptType = type,
                ScriptName = type.Name,
                StateSnapshot = CaptureState(script),
                RegistrationTime = DateTime.Now
            };

            _reloadInfos[type] = info;
        }

        public void UnregisterScript(Script script)
        {
            var type = script.GetType();
            _reloadInfos.Remove(type);
        }

        public HotReloadResult Reload(string sourceCode, string className)
        {
            _isReloading = true;
            var result = new HotReloadResult();

            try
            {
                var compiled = _compiler.Compile(sourceCode, className);
                result.Success = compiled.IsValid;
                result.Errors = compiled.Errors.ToList();

                if (!compiled.IsValid)
                {
                    return result;
                }

                if (_reloadInfos.TryGetValue(compiled.Type!, out var info))
                {
                    var newScript = compiled.CreateInstance();
                    if (newScript != null)
                    {
                        RestoreState(newScript, info.StateSnapshot);
                        info.OriginalScript = newScript;
                        info.ScriptType = compiled.Type!;
                        info.LastReloadTime = DateTime.Now;
                        result.NewScript = newScript;
                    }
                }

                return result;
            }
            finally
            {
                _isReloading = false;
            }
        }

        public HotReloadResult ReloadType(Type scriptType)
        {
            if (!_reloadInfos.TryGetValue(scriptType, out var info))
            {
                return new HotReloadResult
                {
                    Success = false,
                    Errors = new List<ScriptCompilationError>
                    {
                        new ScriptCompilationError
                        {
                            ErrorText = $"Type {scriptType.Name} is not registered for hot reload"
                        }
                    }
                };
            }

            return new HotReloadResult { Success = true };
        }

        private Dictionary<string, object?> CaptureState(Script script)
        {
            var state = new Dictionary<string, object?>();
            var type = script.GetType();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!field.IsLiteral && !field.IsStatic)
                {
                    try
                    {
                        state[field.Name] = field.GetValue(script);
                    }
                    catch
                    {
                        state[field.Name] = null;
                    }
                }
            }

            return state;
        }

        private void RestoreState(Script script, Dictionary<string, object?> state)
        {
            var type = script.GetType();

            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var field in fields)
            {
                if (!field.IsLiteral && !field.IsStatic && state.TryGetValue(field.Name, out var value))
                {
                    try
                    {
                        if (value == null || field.FieldType.IsAssignableFrom(value.GetType()))
                        {
                            field.SetValue(script, value);
                        }
                    }
                    catch
                    {
                    }
                }
            }
        }

        public void ClearAll()
        {
            _reloadInfos.Clear();
        }

        private class ScriptReloadInfo
        {
            public Script OriginalScript { get; set; } = null!;
            public Type ScriptType { get; set; } = null!;
            public string ScriptName { get; set; } = string.Empty;
            public Dictionary<string, object?> StateSnapshot { get; set; } = new Dictionary<string, object?>();
            public DateTime RegistrationTime { get; set; }
            public DateTime LastReloadTime { get; set; }
        }
    }

    public class HotReloadResult
    {
        public bool Success { get; set; }
        public Script? NewScript { get; set; }
        public List<ScriptCompilationError> Errors { get; set; } = new List<ScriptCompilationError>();
    }

    public static class ScriptReflection
    {
        public static object? GetField(object target, string fieldName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (target == null) return null;

            var type = target.GetType();
            var field = type.GetField(fieldName, flags);

            return field?.GetValue(target);
        }

        public static bool SetField(object target, string fieldName, object? value, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (target == null) return false;

            var type = target.GetType();
            var field = type.GetField(fieldName, flags);

            if (field == null) return false;

            try
            {
                field.SetValue(target, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static object? GetProperty(object target, string propertyName, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (target == null) return null;

            var type = target.GetType();
            var property = type.GetProperty(propertyName, flags);

            return property?.GetValue(target);
        }

        public static bool SetProperty(object target, string propertyName, object? value, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (target == null) return false;

            var type = target.GetType();
            var property = type.GetProperty(propertyName, flags);

            if (property == null || !property.CanWrite) return false;

            try
            {
                property.SetValue(target, value);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static object? CallMethod(object target, string methodName, object?[]? args = null, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (target == null) return null;

            var type = target.GetType();
            var method = type.GetMethod(methodName, flags);

            if (method == null) return null;

            try
            {
                return method.Invoke(target, args);
            }
            catch
            {
                return null;
            }
        }

        public static T? CallMethod<T>(object target, string methodName, object?[]? args = null, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var result = CallMethod(target, methodName, args, flags);
            return result is T typed ? typed : default;
        }

        public static Type[] GetTypesWithAttribute<T>(Assembly? assembly = null) where T : Attribute
        {
            assembly ??= Assembly.GetCallingAssembly();
            return assembly.GetTypes()
                .Where(t => t.GetCustomAttribute<T>() != null)
                .ToArray();
        }

        public static Type[] GetTypesInheritingFrom<T>(Assembly? assembly = null)
        {
            assembly ??= Assembly.GetCallingAssembly();
            var baseType = typeof(T);
            return assembly.GetTypes()
                .Where(t => t != baseType && baseType.IsAssignableFrom(t))
                .ToArray();
        }

        public static FieldInfo[] GetAllFields(object target, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (target == null) return Array.Empty<FieldInfo>();

            var type = target.GetType();
            var fields = new List<FieldInfo>();
            var visitedTypes = new HashSet<Type>();

            while (type != null && type != typeof(object))
            {
                if (visitedTypes.Add(type))
                {
                    fields.AddRange(type.GetFields(flags));
                }
                type = type.BaseType!;
            }

            return fields.ToArray();
        }

        public static PropertyInfo[] GetAllProperties(object target, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (target == null) return Array.Empty<PropertyInfo>();

            var type = target.GetType();
            var properties = new List<PropertyInfo>();
            var visitedTypes = new HashSet<Type>();

            while (type != null && type != typeof(object))
            {
                if (visitedTypes.Add(type))
                {
                    properties.AddRange(type.GetProperties(flags));
                }
                type = type.BaseType!;
            }

            return properties.ToArray();
        }

        public static MethodInfo[] GetAllMethods(object target, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            if (target == null) return Array.Empty<MethodInfo>();

            var type = target.GetType();
            var methods = new List<MethodInfo>();
            var visitedTypes = new HashSet<Type>();

            while (type != null && type != typeof(object))
            {
                if (visitedTypes.Add(type))
                {
                    methods.AddRange(type.GetMethods(flags));
                }
                type = type.BaseType!;
            }

            return methods.ToArray();
        }

        public static Dictionary<string, object?> GetAllMembers(object target, BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
        {
            var result = new Dictionary<string, object?>();

            foreach (var field in GetAllFields(target, flags))
            {
                try
                {
                    result[field.Name] = field.GetValue(target);
                }
                catch { }
            }

            foreach (var property in GetAllProperties(target, flags))
            {
                try
                {
                    if (property.CanRead)
                    {
                        result[property.Name] = property.GetValue(target);
                    }
                }
                catch { }
            }

            return result;
        }
    }

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class ScriptableObjectTypeAttribute : Attribute
    {
        public string MenuName { get; }
        public int Order { get; }

        public ScriptableObjectTypeAttribute(string menuName, int order = 0)
        {
            MenuName = menuName;
            Order = order;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class ScriptInvokeAttribute : Attribute
    {
        public float Delay { get; }

        public ScriptInvokeAttribute(float delay = 0f)
        {
            Delay = delay;
        }
    }

    public class PlayerController : Script
    {
        public float MoveSpeed = 5f;
        public float JumpForce = 10f;
        public bool CanJump = true;
        private RigidBody2D? _rigidBody;
        private Vector2 _velocity;

        protected override void Awake()
        {
            base.Awake();
            _rigidBody = GetComponent<RigidBody2D>();
        }

        protected override void UpdateScript(float deltaTime)
        {
            HandleMovement();
            HandleJump();
        }

        private void HandleMovement()
        {
            var horizontal = Input.GetAxis("Horizontal");
            _velocity.x = horizontal * MoveSpeed;

            if (_rigidBody != null)
            {
                _rigidBody.Velocity = _velocity;
            }
            else if (Transform != null)
            {
                Transform.Translate(_velocity * Time.deltaTime);
            }
        }

        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && CanJump)
            {
                _velocity.y = JumpForce;
                if (_rigidBody != null)
                {
                    _rigidBody.Velocity = _velocity;
                }
                CanJump = false;
                Invoke(nameof(ResetJump), 0.5f);
            }
        }

        private void ResetJump()
        {
            CanJump = true;
        }
    }

    public class Rotate : Script
    {
        public Vector3 Axis = Vector3.Forward;
        public float Speed = 90f;
        public Space RotationSpace = Space.Self;

        protected override void UpdateScript(float deltaTime)
        {
            if (Transform != null)
            {
                var rotation = Axis * Speed * deltaTime;
                Transform.Rotate(rotation);
            }
        }
    }

    public class FollowTarget : Script
    {
        public Transform? Target;
        public Vector3 Offset = Vector3.Zero;
        public float SmoothSpeed = 5f;
        public bool LookAtTarget = true;

        protected override void UpdateScript(float deltaTime)
        {
            if (Target == null || Transform == null) return;

            var desiredPosition = Target.Position + Offset;
            var smoothedPosition = Vector2.Lerp(Transform.Position, desiredPosition, SmoothSpeed * deltaTime);
            Transform.Position = smoothedPosition;

            if (LookAtTarget)
            {
                Transform.LookAt(Target.Position);
            }
        }
    }

    public class DestroyOnCollision : Script
    {
        public string TagToDestroy = "";
        public float Delay = 0f;

        public void OnCollisionEnter(CollisionInfo collision)
        {
            if (collision.ColliderA != null && collision.ColliderB != null)
            {
                var other = collision.ColliderA.GameObject == GameObject ? collision.ColliderB.GameObject : collision.ColliderA.GameObject;
                if (other != null && (string.IsNullOrEmpty(TagToDestroy) || other.Tag == TagToDestroy))
                {
                    other.Destroy(Delay);
                }
            }
        }
    }

    public class SpawnOnClick : Script
    {
        public WPFEngine.ECS.GameObject? PrefabToSpawn;
        public Transform? SpawnPoint;
        public int MaxSpawns = 10;
        private int _spawnCount;

        protected override void UpdateScript(float deltaTime)
        {
            if (Input.GetMouseButtonDown(0) && _spawnCount < MaxSpawns)
            {
                Spawn();
            }
        }

        private void Spawn()
        {
            if (PrefabToSpawn == null) return;

            var position = SpawnPoint?.WorldPosition ?? Transform?.WorldPosition ?? Vector2.Zero;
            var rotation = SpawnPoint?.WorldRotation ?? Transform?.WorldRotation ?? 0f;

            var instance = GameObject.Instantiate(PrefabToSpawn);
            if (instance != null)
            {
                instance.Transform.Position = position;
                instance.Transform.Rotation = rotation;
            }

            _spawnCount++;
        }
    }

    public class Pulsate : Script
    {
        public float MinScale = 0.8f;
        public float MaxScale = 1.2f;
        public float Speed = 2f;
        private float _time;

        protected override void UpdateScript(float deltaTime)
        {
            if (Transform == null) return;

            _time += deltaTime * Speed;
            var t = (Mathf.Sin(_time * Mathf.TwoPI) + 1f) / 2f;
            var scale = Mathf.Lerp(MinScale, MaxScale, t);
            Transform.LocalScale = new Vector2(scale, scale);
        }
    }

    public class Timer : ScriptableObject
    {
        public float Duration { get; set; }
        public bool IsRepeating { get; set; }
        public float ElapsedTime { get; private set; }

        public event Action? OnTimerComplete;
        public event Action<float>? OnTimerTick;

        protected override void OnCreate()
        {
            ElapsedTime = 0f;
        }

        public void Update(float deltaTime)
        {
            ElapsedTime += deltaTime;
            OnTimerTick?.Invoke(ElapsedTime / Duration);

            if (ElapsedTime >= Duration)
            {
                OnTimerComplete?.Invoke();

                if (IsRepeating)
                {
                    ElapsedTime = 0f;
                }
                else
                {
                    Destroy(this);
                }
            }
        }

        public float Progress => Duration > 0f ? Mathf.Clamp01(ElapsedTime / Duration) : 0f;
        public float RemainingTime => Mathf.Max(0f, Duration - ElapsedTime);
        public bool IsComplete => ElapsedTime >= Duration;
    }

    public class ObjectPool : ScriptableObject
    {
        private readonly Queue<WPFEngine.ECS.GameObject> _availableObjects;
        private readonly List<WPFEngine.ECS.GameObject> _allObjects;
        private WPFEngine.ECS.GameObject? _prefab;
        private int _poolSize;
        private int _activeCount;

        public int PoolSize => _poolSize;
        public int ActiveCount => _activeCount;
        public int AvailableCount => _availableObjects.Count;

        protected override void OnCreate()
        {
            _availableObjects = new Queue<WPFEngine.ECS.GameObject>();
            _allObjects = new List<WPFEngine.ECS.GameObject>();
            _poolSize = 10;
            _activeCount = 0;
        }

        public void Initialize(WPFEngine.ECS.GameObject prefab, int initialSize)
        {
            _prefab = prefab;
            _poolSize = initialSize;

            for (int i = 0; i < initialSize; i++)
            {
                var obj = _prefab.Clone();
                obj.IsActive = false;
                _availableObjects.Enqueue(obj);
                _allObjects.Add(obj);
            }
        }

        public WPFEngine.ECS.GameObject? Get()
        {
            if (_availableObjects.Count > 0)
            {
                var obj = _availableObjects.Dequeue();
                obj.IsActive = true;
                _activeCount++;
                return obj;
            }

            if (_prefab != null)
            {
                var newObj = _prefab.Clone();
                _allObjects.Add(newObj);
                _poolSize++;
                _activeCount++;
                return newObj;
            }

            return null;
        }

        public void Return(WPFEngine.ECS.GameObject obj)
        {
            obj.IsActive = false;
            _availableObjects.Enqueue(obj);
            _activeCount--;
        }

        public void ReturnAll()
        {
            foreach (var obj in _allObjects)
            {
                if (obj.IsActive)
                {
                    obj.IsActive = false;
                    _availableObjects.Enqueue(obj);
                }
            }
            _activeCount = 0;
        }

        public void Clear()
        {
            foreach (var obj in _allObjects)
            {
                obj.Destroy();
            }
            _availableObjects.Clear();
            _allObjects.Clear();
            _activeCount = 0;
            _poolSize = 0;
        }
    }

    public static class ScriptTemplates
    {
        public const string EmptyScript = @"
using WPFEngine.Scripting;

namespace MyGame.Scripts
{
    public class NewScript : Script
    {
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
        }

        protected override void UpdateScript(float deltaTime)
        {
        }

        protected override void OnDestroyScript()
        {
        }
    }
}";

        public const string PlayerControllerTemplate = @"
using WPFEngine.Scripting;
using WPFEngine.Input;
using WPFEngine.ECS;

namespace MyGame.Controllers
{
    public class PlayerController : Script
    {
        public float MoveSpeed = 5f;
        public float JumpForce = 10f;

        protected override void Start()
        {
            base.Start();
            Debug.Log(""Player controller initialized"");
        }

        protected override void UpdateScript(float deltaTime)
        {
            var horizontal = Input.GetAxis(""Horizontal"");
            var vertical = Input.GetAxis(""Vertical"");

            Transform?.Translate(new Vector2(horizontal, vertical) * MoveSpeed * deltaTime);

            if (Input.GetButtonDown(""Jump""))
            {
                Jump();
            }
        }

        private void Jump()
        {
            Transform?.Translate(Vector2.Up * JumpForce);
        }
    }
}";

        public const string AIControllerTemplate = @"
using WPFEngine.Scripting;
using WPFEngine.ECS;

namespace MyGame.AI
{
    public class AIController : Script
    {
        public Transform? Target;
        public float ChaseSpeed = 3f;
        public float StopDistance = 2f;

        protected override void UpdateScript(float deltaTime)
        {
            if (Target == null || Transform == null) return;

            var direction = (Target.Position - Transform.Position).Normalized;
            var distance = (Target.Position - Transform.Position).Magnitude;

            if (distance > StopDistance)
            {
                Transform.Translate(direction * ChaseSpeed * deltaTime);
            }
        }
    }
}";

        public const string StateMachineTemplate = @"
using WPFEngine.Scripting;
using System.Collections.Generic;

namespace MyGame.FSM
{
    public abstract class State
    {
        public virtual void Enter() { }
        public virtual void Exit() { }
        public virtual void Update(float deltaTime) { }
        public virtual void FixedUpdate(float deltaTime) { }
    }

    public class StateMachine : Script
    {
        private Dictionary<string, State> _states;
        private State? _currentState;

        protected override void Awake()
        {
            base.Awake();
            _states = new Dictionary<string, State>();
        }

        public void AddState(string name, State state)
        {
            _states[name] = state;
        }

        public void ChangeState(string name)
        {
            if (_states.TryGetValue(name, out var newState))
            {
                _currentState?.Exit();
                _currentState = newState;
                _currentState.Enter();
            }
        }

        protected override void UpdateScript(float deltaTime)
        {
            _currentState?.Update(deltaTime);
        }

        protected override void FixedUpdateScript(float deltaTime)
        {
            _currentState?.FixedUpdate(deltaTime);
        }
    }
}";
    }
}

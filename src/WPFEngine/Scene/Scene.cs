using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WPFEngine.Core.Game;

namespace WPFEngine.Scene
{
    public class Scene : IUpdatable, IDrawable
    {
        private readonly string _name;
        private readonly List<GameObject> _gameObjects;
        private readonly Dictionary<Type, List<GameObject>> _gameObjectsByComponent;
        private bool _isActive;
        private bool _isLoaded;

        public string Name => _name;
        public bool IsActive
        {
            get => _isActive;
            set => _isActive = value;
        }
        public bool IsLoaded => _isLoaded;
        public IReadOnlyList<GameObject> GameObjects => _gameObjects.AsReadOnly();
        public Camera? MainCamera { get; set; }

        public event EventHandler<GameObject>? GameObjectAdded;
        public event EventHandler<GameObject>? GameObjectRemoved;

        public Scene(string name)
        {
            _name = name;
            _gameObjects = new List<GameObject>();
            _gameObjectsByComponent = new Dictionary<Type, List<GameObject>>();
            _isActive = true;
            _isLoaded = false;
        }

        public virtual void OnLoad()
        {
            _isLoaded = true;
            foreach (var gameObject in _gameObjects)
            {
                gameObject.OnSceneLoad();
            }
        }

        public virtual void OnEnter()
        {
            foreach (var gameObject in _gameObjects)
            {
                gameObject.OnSceneEnter();
            }
        }

        public virtual void OnExit()
        {
            foreach (var gameObject in _gameObjects)
            {
                gameObject.OnSceneExit();
            }
        }

        public void Update(float deltaTime)
        {
            if (!_isActive) return;

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject.IsActive)
                {
                    gameObject.Update(deltaTime);
                }
            }
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            if (!_isActive) return;

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject.IsActive)
                {
                    gameObject.FixedUpdate(fixedDeltaTime);
                }
            }
        }

        public void Draw(float deltaTime)
        {
            if (!_isActive || MainCamera == null) return;

            foreach (var gameObject in _gameObjects)
            {
                if (gameObject.IsActive && gameObject.IsVisible)
                {
                    gameObject.Draw(deltaTime, MainCamera);
                }
            }
        }

        public T AddGameObject<T>(T gameObject) where T : GameObject
        {
            if (gameObject == null)
                throw new ArgumentNullException(nameof(gameObject));

            gameObject.Scene = this;
            _gameObjects.Add(gameObject);

            RegisterGameObjectWithComponents(gameObject);
            gameObject.OnAddToScene();

            GameObjectAdded?.Invoke(this, gameObject);
            return gameObject;
        }

        public GameObject AddGameObject(string name = "GameObject")
        {
            var gameObject = new GameObject(name);
            return AddGameObject(gameObject);
        }

        public void RemoveGameObject(GameObject gameObject)
        {
            if (gameObject == null || !_gameObjects.Contains(gameObject))
                return;

            UnregisterGameObjectWithComponents(gameObject);
            gameObject.OnRemoveFromScene();
            gameObject.Scene = null;
            _gameObjects.Remove(gameObject);

            GameObjectRemoved?.Invoke(this, gameObject);
        }

        public bool DestroyGameObject(GameObject gameObject)
        {
            if (gameObject == null)
                return false;

            gameObject.Destroy();
            RemoveGameObject(gameObject);
            return true;
        }

        public GameObject? FindGameObject(string name)
        {
            foreach (var gameObject in _gameObjects)
            {
                if (gameObject.Name == name)
                    return gameObject;
            }
            return null;
        }

        public List<GameObject> FindGameObjectsWithTag(string tag)
        {
            var result = new List<GameObject>();
            foreach (var gameObject in _gameObjects)
            {
                if (gameObject.Tag == tag)
                    result.Add(gameObject);
            }
            return result;
        }

        public GameObject? FindGameObjectById(Guid id)
        {
            foreach (var gameObject in _gameObjects)
            {
                if (gameObject.Id == id)
                    return gameObject;
            }
            return null;
        }

        public List<T> FindObjectsOfType<T>() where T : class
        {
            var result = new List<T>();
            if (_gameObjectsByComponent.TryGetValue(typeof(T), out var objects))
            {
                foreach (var obj in objects)
                {
                    if (obj is T typed)
                        result.Add(typed);
                }
            }
            return result;
        }

        public T? FindObjectOfType<T>() where T : class
        {
            return FindObjectsOfType<T>().FirstOrDefault();
        }

        private void RegisterGameObjectWithComponents(GameObject gameObject)
        {
            foreach (var component in gameObject.Components)
            {
                var componentType = component.GetType();
                if (!_gameObjectsByComponent.ContainsKey(componentType))
                {
                    _gameObjectsByComponent[componentType] = new List<GameObject>();
                }
                _gameObjectsByComponent[componentType].Add(gameObject);
            }
        }

        private void UnregisterGameObjectWithComponents(GameObject gameObject)
        {
            foreach (var component in gameObject.Components)
            {
                var componentType = component.GetType();
                if (_gameObjectsByComponent.TryGetValue(componentType, out var objects))
                {
                    objects.Remove(gameObject);
                }
            }
        }

        public void Clear()
        {
            var gameObjectsCopy = new List<GameObject>(_gameObjects);
            foreach (var gameObject in gameObjectsCopy)
            {
                DestroyGameObject(gameObject);
            }
            _gameObjects.Clear();
            _gameObjectsByComponent.Clear();
        }
    }
}

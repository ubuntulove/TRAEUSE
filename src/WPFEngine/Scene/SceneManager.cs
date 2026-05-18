using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Core.Game;

namespace WPFEngine.Scene
{
    public class SceneManager
    {
        private static SceneManager? _instance;
        private Scene? _currentScene;
        private Scene? _nextScene;
        private readonly Dictionary<string, Scene> _scenes;
        private bool _isTransitioning;
        private float _transitionProgress;
        private float _transitionDuration;

        public static SceneManager Instance => _instance ??= new SceneManager();
        public Scene? CurrentScene => _currentScene;
        public bool IsTransitioning => _isTransitioning;
        public float TransitionProgress => _transitionProgress;
        public IReadOnlyDictionary<string, Scene> Scenes => _scenes;

        public event EventHandler<Scene>? SceneChanged;
        public event EventHandler<Scene>? SceneLoading;
        public event EventHandler<Scene>? SceneLoaded;

        private SceneManager()
        {
            _scenes = new Dictionary<string, Scene>();
            _isTransitioning = false;
            _transitionProgress = 0f;
            _transitionDuration = 0.5f;
        }

        public Scene CreateScene(string name)
        {
            if (_scenes.ContainsKey(name))
            {
                throw new ArgumentException($"Scene with name '{name}' already exists.");
            }

            var scene = new Scene(name);
            _scenes[name] = scene;
            return scene;
        }

        public void AddScene(string name, Scene scene)
        {
            if (_scenes.ContainsKey(name))
            {
                throw new ArgumentException($"Scene with name '{name}' already exists.");
            }

            _scenes[name] = scene;
        }

        public bool RemoveScene(string name)
        {
            if (_scenes.TryGetValue(name, out var scene))
            {
                if (_currentScene == scene)
                {
                    _currentScene = null;
                }
                return _scenes.Remove(name);
            }
            return false;
        }

        public Scene? GetScene(string name)
        {
            return _scenes.TryGetValue(name, out var scene) ? scene : null;
        }

        public bool HasScene(string name)
        {
            return _scenes.ContainsKey(name);
        }

        public void LoadScene(string name, bool additive = false)
        {
            if (!_scenes.TryGetValue(name, out var scene))
            {
                throw new ArgumentException($"Scene '{name}' not found.");
            }

            LoadSceneInternal(scene, additive);
        }

        public void LoadScene(Scene scene, bool additive = false)
        {
            LoadSceneInternal(scene, additive);
        }

        private void LoadSceneInternal(Scene scene, bool additive = false)
        {
            SceneLoading?.Invoke(this, scene);

            if (!additive && _currentScene != null)
            {
                _currentScene.OnExit();
            }

            _nextScene = scene;

            if (!additive)
            {
                _currentScene = _nextScene;
                _nextScene = null;
            }

            _currentScene?.OnEnter();
            SceneChanged?.Invoke(this, _currentScene);
            SceneLoaded?.Invoke(this, _currentScene);
        }

        public void UnloadScene(string name)
        {
            if (!_scenes.TryGetValue(name, out var scene))
            {
                return;
            }

            if (_currentScene == scene)
            {
                throw new InvalidOperationException("Cannot unload the currently active scene.");
            }

            scene.OnExit();
            _scenes.Remove(name);
        }

        public void Update(float deltaTime)
        {
            if (_isTransitioning)
            {
                _transitionProgress += deltaTime / _transitionDuration;
                if (_transitionProgress >= 1f)
                {
                    _transitionProgress = 1f;
                    _isTransitioning = false;
                }
            }

            _currentScene?.Update(deltaTime);
        }

        public void FixedUpdate(float fixedDeltaTime)
        {
            _currentScene?.FixedUpdate(fixedDeltaTime);
        }

        public void Draw(float deltaTime)
        {
            _currentScene?.Draw(deltaTime);
        }

        public void ClearAllScenes()
        {
            if (_currentScene != null)
            {
                _currentScene.OnExit();
                _currentScene = null;
            }

            foreach (var scene in _scenes.Values)
            {
                scene.OnExit();
            }

            _scenes.Clear();
        }

        public void TransitionTo(string sceneName, float duration = 0.5f)
        {
            if (!_scenes.TryGetValue(sceneName, out var scene))
            {
                throw new ArgumentException($"Scene '{sceneName}' not found.");
            }

            _transitionDuration = duration;
            _transitionProgress = 0f;
            _isTransitioning = true;
            _nextScene = scene;
        }

        public void CompleteTransition()
        {
            if (_nextScene == null) return;

            if (_currentScene != null)
            {
                _currentScene.OnExit();
            }

            _currentScene = _nextScene;
            _nextScene = null;
            _isTransitioning = false;
            _transitionProgress = 0f;

            _currentScene?.OnEnter();
            SceneChanged?.Invoke(this, _currentScene);
            SceneLoaded?.Invoke(this, _currentScene);
        }
    }
}

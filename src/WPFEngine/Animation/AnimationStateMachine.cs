using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.Animation
{
    public enum ParameterType
    {
        Float,
        Int,
        Bool,
        Trigger
    }

    public class StateMachineParameter
    {
        public string Name { get; set; }
        public ParameterType Type { get; set; }
        public float FloatDefaultValue { get; set; }
        public int IntDefaultValue { get; set; }
        public bool BoolDefaultValue { get; set; }

        public StateMachineParameter(string name, ParameterType type)
        {
            Name = name;
            Type = type;
        }

        public StateMachineParameter(string name, float defaultValue)
        {
            Name = name;
            Type = ParameterType.Float;
            FloatDefaultValue = defaultValue;
        }

        public StateMachineParameter(string name, int defaultValue)
        {
            Name = name;
            Type = ParameterType.Int;
            IntDefaultValue = defaultValue;
        }

        public StateMachineParameter(string name, bool defaultValue)
        {
            Name = name;
            Type = ParameterType.Bool;
            BoolDefaultValue = defaultValue;
        }
    }

    public abstract class State
    {
        public string Name { get; set; }
        public StateMachine? StateMachine { get; set; }
        public List<Transition> Transitions { get; } = new List<Transition>();
        public float StateWeight { get; set; } = 1.0f;
        public float Speed { get; set; } = 1.0f;
        public float CycleOffset { get; set; } = 0.0f;
        public bool CycleOffsetActive { get; set; } = false;

        public event Action<State>? OnStateEnter;
        public event Action<State>? OnStateUpdate;
        public event Action<State>? OnStateExit;

        protected State(string name)
        {
            Name = name;
        }

        public virtual void Enter()
        {
            OnStateEnter?.Invoke(this);
        }

        public virtual void Update(float deltaTime)
        {
            OnStateUpdate?.Invoke(this);
        }

        public virtual void Exit()
        {
            OnStateExit?.Invoke(this);
        }

        public void AddTransition(Transition transition)
        {
            transition.SourceState = this;
            Transitions.Add(transition);
        }

        public virtual float GetBlendWeight()
        {
            return StateWeight;
        }

        public virtual float GetCurrentTime()
        {
            return 0f;
        }

        public virtual float GetAnimationLength()
        {
            return 0f;
        }

        public virtual bool IsInTransition()
        {
            return false;
        }
    }

    public class Transition
    {
        public State? SourceState { get; set; }
        public State? DestinationState { get; set; }
        public bool HasExitTime { get; set; } = false;
        public float ExitTime { get; set; } = 0f;
        public float TransitionDuration { get; set; } = 0f;
        public float TransitionOffset { get; set; } = 0f;
        public List<TransitionCondition> Conditions { get; } = new List<TransitionCondition>();
        public bool CanTransitionToSelf { get; set; } = false;
        public int OrderedIndex { get; set; } = -1;

        private float _blendProgress = 1f;
        private bool _isTransitioning = false;

        public float BlendProgress => _blendProgress;
        public bool IsTransitioning => _isTransitioning;

        public event Action<Transition>? OnTransitionBegin;
        public event Action<Transition, float>? OnTransitionUpdate;
        public event Action<Transition>? OnTransitionEnd;

        public Transition()
        {
        }

        public Transition(State destination)
        {
            DestinationState = destination;
        }

        public Transition SetExitTime(float exitTime)
        {
            ExitTime = exitTime;
            HasExitTime = true;
            return this;
        }

        public Transition SetDuration(float duration)
        {
            TransitionDuration = duration;
            return this;
        }

        public Transition AddCondition(Func<bool> condition)
        {
            Conditions.Add(new TransitionCondition { ConditionFunction = condition });
            return this;
        }

        public Transition AddCondition(string parameterName, ConditionMode mode, float threshold)
        {
            Conditions.Add(new TransitionCondition
            {
                ParameterName = parameterName,
                Mode = mode,
                Threshold = threshold
            });
            return this;
        }

        public bool CanTransition()
        {
            if (SourceState == null || DestinationState == null)
                return false;

            if (!CanTransitionToSelf && SourceState == DestinationState)
                return false;

            if (HasExitTime)
            {
                float normalizedTime = SourceState.GetCurrentTime() / SourceState.GetAnimationLength();
                if (normalizedTime < ExitTime)
                    return false;
            }

            foreach (var condition in Conditions)
            {
                if (!condition.Evaluate())
                    return false;
            }

            return true;
        }

        public void BeginTransition()
        {
            _isTransitioning = true;
            _blendProgress = 0f;
            OnTransitionBegin?.Invoke(this);
        }

        public void UpdateTransition(float deltaTime)
        {
            if (!_isTransitioning)
                return;

            if (TransitionDuration > 0)
            {
                _blendProgress += deltaTime / TransitionDuration;
                _blendProgress = Math.Min(_blendProgress, 1f);
            }
            else
            {
                _blendProgress = 1f;
            }

            OnTransitionUpdate?.Invoke(this, _blendProgress);

            if (_blendProgress >= 1f)
            {
                _isTransitioning = false;
                OnTransitionEnd?.Invoke(this);
            }
        }

        public float GetBlendFactor()
        {
            if (!_isTransitioning)
                return 1f;

            if (TransitionDuration <= 0)
                return 1f;

            return MathHelper.Clamp(_blendProgress, 0f, 1f);
        }
    }

    public enum ConditionMode
    {
        If,
        IfNot,
        Greater,
        Less,
        Equals,
        NotEquals,
        GreaterEquals,
        LessEquals
    }

    public class TransitionCondition
    {
        public string? ParameterName { get; set; }
        public ConditionMode Mode { get; set; } = ConditionMode.If;
        public float Threshold { get; set; } = 0f;
        public Func<bool>? ConditionFunction { get; set; }
        public string? SourceStateName { get; set; }

        public bool Evaluate()
        {
            if (ConditionFunction != null)
                return ConditionFunction();

            return true;
        }
    }

    public abstract class StateMachineLayer
    {
        public string Name { get; set; }
        public float Weight { get; set; } = 1.0f;
        public int Index { get; set; }
        public bool IsAdditive { get; set; } = false;
        public bool IKPass { get; set; } = false;
        public AvatarMask? AvatarMask { get; set; }
        public abstract State? CurrentState { get; }
        public abstract void Update(float deltaTime);
        public abstract void Evaluate();
    }

    public class AvatarMask
    {
        public string Name { get; set; }
        public Dictionary<string, float> BodyPartWeights { get; } = new Dictionary<string, float>();
        public bool ApplyFootIK { get; set; } = true;
        public bool ApplyIKGoal { get; set; } = true;

        public AvatarMask(string name)
        {
            Name = name;
            SetDefaultWeights();
        }

        private void SetDefaultWeights()
        {
            BodyPartWeights["Root"] = 1.0f;
            BodyPartWeights["Body"] = 1.0f;
            BodyPartWeights["Head"] = 1.0f;
            BodyPartWeights["LeftArm"] = 1.0f;
            BodyPartWeights["RightArm"] = 1.0f;
            BodyPartWeights["LeftHand"] = 1.0f;
            BodyPartWeights["RightHand"] = 1.0f;
            BodyPartWeights["LeftLeg"] = 1.0f;
            BodyPartWeights["RightLeg"] = 1.0f;
            BodyPartWeights["LeftFoot"] = 1.0f;
            BodyPartWeights["RightFoot"] = 1.0f;
        }

        public void SetBodyPartWeight(string bodyPart, float weight)
        {
            BodyPartWeights[bodyPart] = MathHelper.Clamp(weight, 0f, 1f);
        }

        public float GetBodyPartWeight(string bodyPart)
        {
            return BodyPartWeights.TryGetValue(bodyPart, out float weight) ? weight : 0f;
        }
    }

    public class AnimationState : State
    {
        public AnimationClip? Clip { get; set; }
        private float _currentTime = 0f;
        private bool _isPlaying = false;
        private bool _inTransition = false;
        private float _transitionBlend = 1f;

        public event Action<AnimationState, float>? OnAnimationUpdate;
        public event Action<AnimationState>? OnAnimationComplete;

        public AnimationState(string name) : base(name)
        {
        }

        public AnimationState(string name, AnimationClip clip) : base(name)
        {
            Clip = clip;
        }

        public override void Enter()
        {
            base.Enter();
            _isPlaying = true;
            if (Clip != null && Clip.IsLooping)
            {
                _currentTime = CycleOffsetActive ? CycleOffset * Clip.Duration : 0f;
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!_isPlaying || Clip == null)
                return;

            float adjustedDelta = deltaTime * Speed;
            _currentTime += adjustedDelta;

            float clipDuration = Clip.Duration;
            if (Clip.IsLooping)
            {
                while (_currentTime >= clipDuration)
                {
                    _currentTime -= clipDuration;
                }
                while (_currentTime < 0)
                {
                    _currentTime += clipDuration;
                }
            }
            else
            {
                if (_currentTime >= clipDuration)
                {
                    _currentTime = clipDuration;
                    _isPlaying = false;
                    OnAnimationComplete?.Invoke(this);
                }
            }

            OnAnimationUpdate?.Invoke(this, _currentTime);
        }

        public override void Exit()
        {
            base.Exit();
            _isPlaying = false;
        }

        public override float GetCurrentTime()
        {
            return _currentTime;
        }

        public override float GetAnimationLength()
        {
            return Clip?.Duration ?? 0f;
        }

        public float GetNormalizedTime()
        {
            if (Clip == null || Clip.Duration <= 0)
                return 0f;
            return _currentTime / Clip.Duration;
        }

        public void SetTime(float time)
        {
            _currentTime = time;
        }

        public void SetTransitionBlend(float blend)
        {
            _transitionBlend = MathHelper.Clamp(blend, 0f, 1f);
            _inTransition = blend < 1f;
        }

        public override float GetBlendWeight()
        {
            return StateWeight * _transitionBlend;
        }

        public override bool IsInTransition()
        {
            return _inTransition;
        }

        public AnimationClip? GetCurrentClip()
        {
            return Clip;
        }

        public void SampleAnimation(float time)
        {
            if (Clip == null)
                return;

            _currentTime = time;
        }

        public Dictionary<string, object?> GetPoseAtTime(float time)
        {
            var pose = new Dictionary<string, object?>();

            if (Clip == null)
                return pose;

            foreach (var track in Clip.Tracks)
            {
                pose[track.TargetPath] = track.GetValueAtTime(time);
            }

            return pose;
        }
    }

    public class BlendState : State
    {
        public BlendTree? BlendTree { get; set; }
        public float BlendParameter { get; set; } = 0f;
        public float BlendParameter2 { get; set; } = 0f;
        private AnimationState? _currentAnimationState;

        public BlendState(string name) : base(name)
        {
        }

        public BlendState(string name, BlendTree blendTree) : base(name)
        {
            BlendTree = blendTree;
        }

        public override void Enter()
        {
            base.Enter();
            BlendTree?.Reset();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (BlendTree == null)
                return;

            BlendTree.SetBlendParameter(BlendParameter);
            BlendTree.SetBlendParameter2(BlendParameter2);
            BlendTree.Update(deltaTime);

            foreach (var child in BlendTree.Children)
            {
                child.Update(deltaTime);
            }
        }

        public override float GetAnimationLength()
        {
            if (BlendTree == null)
                return 0f;

            float maxLength = 0f;
            foreach (var child in BlendTree.Children)
            {
                if (child.Clip != null && child.Clip.Duration > maxLength)
                {
                    maxLength = child.Clip.Duration;
                }
            }
            return maxLength;
        }

        public float GetBlendWeight()
        {
            return StateWeight * (BlendTree?.GetBlendWeight() ?? 1f);
        }

        public void UpdateBlendParameters(float param1, float param2)
        {
            BlendParameter = param1;
            BlendParameter2 = param2;
        }
    }

    public class OneShotState : State
    {
        public AnimationClip? Clip { get; set; }
        public State? ReturnState { get; set; }
        private float _currentTime = 0f;
        private bool _isPlaying = false;
        private bool _hasCompleted = false;

        public event Action<OneShotState>? OnOneShotComplete;

        public OneShotState(string name) : base(name)
        {
        }

        public OneShotState(string name, AnimationClip clip) : base(name)
        {
            Clip = clip;
        }

        public override void Enter()
        {
            base.Enter();
            _isPlaying = true;
            _hasCompleted = false;
            _currentTime = 0f;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!_isPlaying || Clip == null || _hasCompleted)
                return;

            _currentTime += deltaTime * Speed;

            if (_currentTime >= Clip.Duration)
            {
                _currentTime = Clip.Duration;
                _isPlaying = false;
                _hasCompleted = true;
                OnOneShotComplete?.Invoke(this);

                if (StateMachine != null && ReturnState != null)
                {
                    StateMachine.TransitionTo(ReturnState.Name);
                }
            }
        }

        public override float GetCurrentTime()
        {
            return _currentTime;
        }

        public override float GetAnimationLength()
        {
            return Clip?.Duration ?? 0f;
        }

        public float GetNormalizedTime()
        {
            if (Clip == null || Clip.Duration <= 0)
                return 0f;
            return _currentTime / Clip.Duration;
        }

        public void Reset()
        {
            _hasCompleted = false;
            _currentTime = 0f;
        }
    }

    public class SubStateMachine : State
    {
        public StateMachine? ChildStateMachine { get; set; }
        public string DefaultChildState { get; set; } = "";

        public SubStateMachine(string name) : base(name)
        {
            ChildStateMachine = new StateMachine(name + "_Child");
        }

        public override void Enter()
        {
            base.Enter();

            if (ChildStateMachine != null && !string.IsNullOrEmpty(DefaultChildState))
            {
                ChildStateMachine.TransitionTo(DefaultChildState);
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            ChildStateMachine?.Update(deltaTime);
        }

        public override void Exit()
        {
            base.Exit();
            ChildStateMachine?.ForceExit();
        }

        public override float GetCurrentTime()
        {
            return ChildStateMachine?.CurrentStateTime ?? 0f;
        }

        public override float GetAnimationLength()
        {
            return ChildStateMachine?.CurrentStateLength ?? 0f;
        }

        public StateMachine? GetChildStateMachine()
        {
            return ChildStateMachine;
        }

        public State? GetCurrentChildState()
        {
            return ChildStateMachine?.CurrentState;
        }
    }

    public enum BlendMode
    {
        Free,
        Direct,
        OneDimensional,
        TwoDimensionalSimple,
        TwoDimensionalFreeCartesian,
        TwoDimensionalFreeDirectional
    }

    public class BlendTree
    {
        public string Name { get; set; }
        public BlendMode BlendMode { get; set; } = BlendMode.OneDimensional;
        public List<BlendTreeChild> Children { get; } = new List<BlendTreeChild>();
        public BlendTreeChild? BlendTreeChild { get; set; }
        private float _blendParameter = 0f;
        private float _blendParameter2 = 0f;
        private bool _isBlendDirty = true;
        private float[] _blendWeights = Array.Empty<float>();

        public BlendTree(string name)
        {
            Name = name;
        }

        public void AddChild(BlendTreeChild child)
        {
            child.Index = Children.Count;
            Children.Add(child);
            _isBlendDirty = true;
        }

        public void RemoveChild(BlendTreeChild child)
        {
            Children.Remove(child);
            for (int i = 0; i < Children.Count; i++)
            {
                Children[i].Index = i;
            }
            _isBlendDirty = true;
        }

        public void SetBlendParameter(float value)
        {
            if (Math.Abs(_blendParameter - value) > 0.0001f)
            {
                _blendParameter = value;
                _isBlendDirty = true;
            }
        }

        public void SetBlendParameter2(float value)
        {
            if (Math.Abs(_blendParameter2 - value) > 0.0001f)
            {
                _blendParameter2 = value;
                _isBlendDirty = true;
            }
        }

        public void Reset()
        {
            foreach (var child in Children)
            {
                child.Reset();
            }
        }

        public void Update(float deltaTime)
        {
            if (_isBlendDirty)
            {
                CalculateBlendWeights();
                _isBlendDirty = false;
            }
        }

        private void CalculateBlendWeights()
        {
            if (Children.Count == 0)
                return;

            switch (BlendMode)
            {
                case BlendMode.Free:
                case BlendMode.Direct:
                    CalculateDirectBlendWeights();
                    break;

                case BlendMode.OneDimensional:
                    Calculate1DBlendWeights();
                    break;

                case BlendMode.TwoDimensionalSimple:
                case BlendMode.TwoDimensionalFreeCartesian:
                case BlendMode.TwoDimensionalFreeDirectional:
                    Calculate2DBlendWeights();
                    break;
            }
        }

        private void CalculateDirectBlendWeights()
        {
            Array.Resize(ref _blendWeights, Children.Count);
            float totalWeight = 0f;

            for (int i = 0; i < Children.Count; i++)
            {
                _blendWeights[i] = Children[i].DirectBlendParameter;
                totalWeight += _blendWeights[i];
            }

            if (totalWeight > 0)
            {
                for (int i = 0; i < _blendWeights.Length; i++)
                {
                    _blendWeights[i] /= totalWeight;
                }
            }
        }

        private void Calculate1DBlendWeights()
        {
            Array.Resize(ref _blendWeights, Children.Count);

            if (Children.Count == 1)
            {
                _blendWeights[0] = 1f;
                return;
            }

            Children.Sort((a, b) => a.Threshold.CompareTo(b.Threshold));

            if (_blendParameter <= Children[0].Threshold)
            {
                _blendWeights[0] = 1f;
                return;
            }

            if (_blendParameter >= Children[Children.Count - 1].Threshold)
            {
                _blendWeights[Children.Count - 1] = 1f;
                return;
            }

            for (int i = 0; i < Children.Count - 1; i++)
            {
                float threshold1 = Children[i].Threshold;
                float threshold2 = Children[i + 1].Threshold;

                if (_blendParameter >= threshold1 && _blendParameter <= threshold2)
                {
                    float t = (_blendParameter - threshold1) / (threshold2 - threshold1);
                    t = MathHelper.Clamp(t, 0f, 1f);

                    float scale1 = Children[i].Scale;
                    float scale2 = Children[i + 1].Scale;

                    _blendWeights[i] = (1f - t) * scale1;
                    _blendWeights[i + 1] = t * scale2;
                }
            }

            NormalizeWeights();
        }

        private void Calculate2DBlendWeights()
        {
            Array.Resize(ref _blendWeights, Children.Count);

            if (Children.Count == 1)
            {
                _blendWeights[0] = 1f;
                return;
            }

            if (Children.Count == 2)
            {
                Calculate2PointBlend();
                return;
            }

            CalculateNearestNeighborBlend();
        }

        private void Calculate2PointBlend()
        {
            Vector2 blendPos = new Vector2(_blendParameter, _blendParameter2);

            float dist1 = Vector2.Distance(blendPos, new Vector2(Children[0].Position.X, Children[0].Position.Y));
            float dist2 = Vector2.Distance(blendPos, new Vector2(Children[1].Position.X, Children[1].Position.Y));

            float totalDist = dist1 + dist2;
            if (totalDist > 0.0001f)
            {
                _blendWeights[0] = (1f - (dist1 / totalDist)) * Children[0].Scale;
                _blendWeights[1] = (1f - (dist2 / totalDist)) * Children[1].Scale;
            }
            else
            {
                _blendWeights[0] = 0.5f * Children[0].Scale;
                _blendWeights[1] = 0.5f * Children[1].Scale;
            }

            NormalizeWeights();
        }

        private void CalculateNearestNeighborBlend()
        {
            float minDist = float.MaxValue;
            int nearestIndex = 0;

            for (int i = 0; i < Children.Count; i++)
            {
                float dist = CalculateDistance2D(i);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearestIndex = i;
                }
            }

            Array.Clear(_blendWeights, 0, _blendWeights.Length);
            _blendWeights[nearestIndex] = 1f;
        }

        private float CalculateDistance2D(int childIndex)
        {
            var child = Children[childIndex];
            float dx = _blendParameter - child.Position.X;
            float dy = _blendParameter2 - child.Position.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void NormalizeWeights()
        {
            float totalWeight = 0f;
            for (int i = 0; i < _blendWeights.Length; i++)
            {
                totalWeight += _blendWeights[i];
            }

            if (totalWeight > 0)
            {
                for (int i = 0; i < _blendWeights.Length; i++)
                {
                    _blendWeights[i] /= totalWeight;
                }
            }
        }

        public float GetBlendWeight(int childIndex)
        {
            if (childIndex >= 0 && childIndex < _blendWeights.Length)
            {
                return _blendWeights[childIndex];
            }
            return 0f;
        }

        public float GetBlendWeight()
        {
            return 1f;
        }

        public Dictionary<string, object?> GetBlendedPose(float time)
        {
            var blendedPose = new Dictionary<string, object?>();

            if (Children.Count == 0)
                return blendedPose;

            for (int i = 0; i < Children.Count; i++)
            {
                var child = Children[i];
                float weight = GetBlendWeight(i);

                if (weight <= 0f || child.Clip == null)
                    continue;

                var childPose = child.GetPoseAtTime(time);

                foreach (var kvp in childPose)
                {
                    if (blendedPose.TryGetValue(kvp.Key, out var existingValue) && existingValue != null)
                    {
                        blendedPose[kvp.Key] = BlendValues(existingValue, kvp.Value, weight);
                    }
                    else
                    {
                        blendedPose[kvp.Key] = kvp.Value;
                    }
                }
            }

            return blendedPose;
        }

        private object? BlendValues(object value1, object value2, float weight)
        {
            if (value1 is float f1 && value2 is float f2)
            {
                return f1 * (1f - weight) + f2 * weight;
            }
            if (value1 is Vector2 v2_1 && value2 is Vector2 v2_2)
            {
                return v2_1 * (1f - weight) + v2_2 * weight;
            }
            if (value1 is Vector3 v3_1 && value2 is Vector3 v3_2)
            {
                return v3_1 * (1f - weight) + v3_2 * weight;
            }
            if (value1 is Color c1 && value2 is Color c2)
            {
                return c1.Lerp(c2, weight);
            }

            if (weight >= 0.5f)
                return value2;
            return value1;
        }
    }

    public class BlendTreeChild
    {
        public AnimationClip? Clip { get; set; }
        public float Threshold { get; set; } = 0f;
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float Scale { get; set; } = 1f;
        public float DirectBlendParameter { get; set; } = 1f;
        public int Index { get; set; } = -1;
        private float _currentTime = 0f;

        public BlendTreeChild()
        {
        }

        public BlendTreeChild(AnimationClip clip)
        {
            Clip = clip;
        }

        public BlendTreeChild(AnimationClip clip, float threshold)
        {
            Clip = clip;
            Threshold = threshold;
        }

        public void Reset()
        {
            _currentTime = 0f;
        }

        public void Update(float deltaTime)
        {
            if (Clip == null)
                return;

            _currentTime += deltaTime;
            while (_currentTime >= Clip.Duration)
            {
                _currentTime -= Clip.Duration;
            }
        }

        public float GetCurrentTime()
        {
            return _currentTime;
        }

        public Dictionary<string, object?> GetPoseAtTime(float time)
        {
            var pose = new Dictionary<string, object?>();

            if (Clip == null)
                return pose;

            foreach (var track in Clip.Tracks)
            {
                pose[track.TargetPath] = track.GetValueAtTime(time);
            }

            return pose;
        }
    }

    public class StateMachine
    {
        private readonly Dictionary<string, State> _states;
        private readonly Dictionary<string, StateMachineParameter> _parameters;
        private readonly Dictionary<string, object> _parameterValues;
        private readonly List<Transition> _anyStateTransitions;
        private State? _currentState;
        private State? _previousState;
        private Transition? _activeTransition;
        private readonly List<StateMachineLayer> _layers;
        private int _defaultLayerIndex;
        private bool _forceResetAnyState;
        private string _name;

        public string Name => _name;
        public State? CurrentState => _currentState;
        public State? PreviousState => _previousState;
        public float CurrentStateTime => _currentState?.GetCurrentTime() ?? 0f;
        public float CurrentStateLength => _currentState?.GetAnimationLength() ?? 0f;
        public IReadOnlyDictionary<string, State> States => _states;
        public IReadOnlyDictionary<string, StateMachineParameter> Parameters => _parameters;
        public IReadOnlyList<StateMachineLayer> Layers => _layers;

        public event Action<State, State>? OnStateChanged;
        public event Action<StateMachine, string, float>? OnParameterChanged;
        public event Action<Transition>? OnTransitionBegan;
        public event Action<Transition, float>? OnTransitionUpdated;
        public event Action<Transition>? OnTransitionCompleted;

        public StateMachine(string name = "StateMachine")
        {
            _name = name;
            _states = new Dictionary<string, State>();
            _parameters = new Dictionary<string, StateMachineParameter>();
            _parameterValues = new Dictionary<string, object>();
            _anyStateTransitions = new List<Transition>();
            _layers = new List<StateMachineLayer>();
            _defaultLayerIndex = 0;
            _forceResetAnyState = false;

            CreateLayer("Base Layer");
        }

        public void AddState(string name, State state)
        {
            state.StateMachine = this;
            _states[name] = state;
        }

        public State? GetState(string name)
        {
            return _states.TryGetValue(name, out var state) ? state : null;
        }

        public bool RemoveState(string name)
        {
            if (_currentState?.Name == name)
            {
                return false;
            }

            return _states.Remove(name);
        }

        public AnimationState CreateAnimationState(string name, AnimationClip? clip = null)
        {
            var state = new AnimationState(name) { Clip = clip };
            AddState(name, state);
            return state;
        }

        public BlendState CreateBlendState(string name, BlendTree? blendTree = null)
        {
            var state = new BlendState(name) { BlendTree = blendTree };
            AddState(name, state);
            return state;
        }

        public OneShotState CreateOneShotState(string name, AnimationClip? clip = null)
        {
            var state = new OneShotState(name) { Clip = clip };
            AddState(name, state);
            return state;
        }

        public SubStateMachine CreateSubStateMachine(string name, string defaultChildState = "")
        {
            var state = new SubStateMachine(name) { DefaultChildState = defaultChildState };
            AddState(name, state);
            return state;
        }

        public void AddAnyStateTransition(State destination, Action<Transition>? setup = null)
        {
            var transition = new Transition(destination);
            setup?.Invoke(transition);
            transition.SourceStateName = "AnyState";
            _anyStateTransitions.Add(transition);
        }

        public void AddAnyStateTransition(string destinationStateName, Action<Transition>? setup = null)
        {
            if (!_states.TryGetValue(destinationStateName, out var destination))
                return;

            AddAnyStateTransition(destination, setup);
        }

        public void CreateParameter(string name, ParameterType type)
        {
            var parameter = new StateMachineParameter(name, type);
            _parameters[name] = parameter;

            switch (type)
            {
                case ParameterType.Float:
                    _parameterValues[name] = 0f;
                    break;
                case ParameterType.Int:
                    _parameterValues[name] = 0;
                    break;
                case ParameterType.Bool:
                    _parameterValues[name] = false;
                    break;
                case ParameterType.Trigger:
                    _parameterValues[name] = false;
                    break;
            }
        }

        public void CreateFloatParameter(string name, float defaultValue = 0f)
        {
            CreateParameter(name, ParameterType.Float);
            _parameterValues[name] = defaultValue;
        }

        public void CreateIntParameter(string name, int defaultValue = 0)
        {
            CreateParameter(name, ParameterType.Int);
            _parameterValues[name] = defaultValue;
        }

        public void CreateBoolParameter(string name, bool defaultValue = false)
        {
            CreateParameter(name, ParameterType.Bool);
            _parameterValues[name] = defaultValue;
        }

        public void CreateTriggerParameter(string name)
        {
            CreateParameter(name, ParameterType.Trigger);
            _parameterValues[name] = false;
        }

        public StateMachineParameter? GetParameter(string name)
        {
            return _parameters.TryGetValue(name, out var param) ? param : null;
        }

        public void SetFloat(string name, float value)
        {
            if (_parameters.TryGetValue(name, out var param) && param.Type == ParameterType.Float)
            {
                _parameterValues[name] = value;
                OnParameterChanged?.Invoke(this, name, value);
            }
        }

        public float GetFloat(string name)
        {
            if (_parameterValues.TryGetValue(name, out var value))
            {
                return Convert.ToSingle(value);
            }
            return 0f;
        }

        public void SetInt(string name, int value)
        {
            if (_parameters.TryGetValue(name, out var param) && param.Type == ParameterType.Int)
            {
                _parameterValues[name] = value;
            }
        }

        public int GetInt(string name)
        {
            if (_parameterValues.TryGetValue(name, out var value))
            {
                return Convert.ToInt32(value);
            }
            return 0;
        }

        public void SetBool(string name, bool value)
        {
            if (_parameters.TryGetValue(name, out var param) &&
                (param.Type == ParameterType.Bool || param.Type == ParameterType.Trigger))
            {
                _parameterValues[name] = value;
            }
        }

        public bool GetBool(string name)
        {
            if (_parameterValues.TryGetValue(name, out var value))
            {
                if (value is bool b)
                    return b;
                if (value is float f)
                    return f > 0.5f;
                if (value is int i)
                    return i != 0;
            }
            return false;
        }

        public void SetTrigger(string name)
        {
            if (_parameters.TryGetValue(name, out var param) && param.Type == ParameterType.Trigger)
            {
                _parameterValues[name] = true;
            }
        }

        public void ResetTrigger(string name)
        {
            if (_parameters.TryGetValue(name, out var param) && param.Type == ParameterType.Trigger)
            {
                _parameterValues[name] = false;
            }
        }

        public bool GetTrigger(string name)
        {
            return GetBool(name);
        }

        public void TransitionTo(string stateName)
        {
            if (!_states.TryGetValue(stateName, out var destination))
                return;

            if (_currentState != null)
            {
                var transition = _currentState.Transitions.FirstOrDefault(t =>
                    t.DestinationState == destination && t.CanTransition());

                if (transition != null)
                {
                    BeginTransition(transition);
                    return;
                }
            }

            ForceTransitionTo(destination);
        }

        private void BeginTransition(Transition transition)
        {
            var oldState = _currentState;
            _activeTransition = transition;
            transition.BeginTransition();

            OnTransitionBegan?.Invoke(transition);
        }

        private void ForceTransitionTo(State destination)
        {
            var oldState = _currentState;
            _previousState = oldState;

            oldState?.Exit();
            _currentState = destination;
            _currentState?.Enter();

            _activeTransition = null;

            OnStateChanged?.Invoke(oldState!, _currentState!);
        }

        public void Update(float deltaTime)
        {
            UpdateTriggers();

            if (_activeTransition != null)
            {
                UpdateTransition(deltaTime);
            }
            else
            {
                UpdateCurrentState(deltaTime);
                CheckTransitions();
            }

            foreach (var layer in _layers)
            {
                layer.Update(deltaTime);
            }
        }

        private void UpdateTriggers()
        {
            foreach (var param in _parameters)
            {
                if (param.Value.Type == ParameterType.Trigger)
                {
                    if (_parameterValues.TryGetValue(param.Key, out var value) && value is bool b && b)
                    {
                        _parameterValues[param.Key] = false;
                    }
                }
            }
        }

        private void UpdateTransition(float deltaTime)
        {
            if (_activeTransition == null)
                return;

            _activeTransition.UpdateTransition(deltaTime);
            OnTransitionUpdated?.Invoke(_activeTransition, _activeTransition.BlendProgress);

            if (!_activeTransition.IsTransitioning && _activeTransition.DestinationState != null)
            {
                var oldState = _currentState;
                _previousState = oldState;

                oldState?.Exit();
                _currentState = _activeTransition.DestinationState;
                _currentState?.Enter();

                OnTransitionCompleted?.Invoke(_activeTransition);
                _activeTransition = null;

                OnStateChanged?.Invoke(oldState!, _currentState!);
            }
        }

        private void UpdateCurrentState(float deltaTime)
        {
            _currentState?.Update(deltaTime);
        }

        private void CheckTransitions()
        {
            foreach (var transition in _anyStateTransitions)
            {
                transition.SourceState = _currentState;
                if (transition.CanTransition())
                {
                    BeginTransition(transition);
                    return;
                }
            }

            if (_currentState != null)
            {
                foreach (var transition in _currentState.Transitions.OrderBy(t => t.OrderedIndex))
                {
                    if (transition.CanTransition())
                    {
                        BeginTransition(transition);
                        return;
                    }
                }
            }
        }

        public void ForceExit()
        {
            _currentState?.Exit();
            _currentState = null;
            _activeTransition = null;
        }

        public StateMachineLayer CreateLayer(string name, AvatarMask? mask = null)
        {
            var layer = new StateMachineLayerImpl(name, this);
            layer.Index = _layers.Count;
            layer.AvatarMask = mask;
            _layers.Add(layer);
            return layer;
        }

        public StateMachineLayer? GetLayer(int index)
        {
            if (index >= 0 && index < _layers.Count)
                return _layers[index];
            return null;
        }

        public StateMachineLayer? GetLayer(string name)
        {
            return _layers.FirstOrDefault(l => l.Name == name);
        }

        public int GetLayerCount()
        {
            return _layers.Count;
        }

        public float GetTransitionBlend()
        {
            return _activeTransition?.GetBlendFactor() ?? 1f;
        }

        public bool IsInTransition()
        {
            return _activeTransition != null && _activeTransition.IsTransitioning;
        }

        public Transition? GetActiveTransition()
        {
            return _activeTransition;
        }

        public void Reset()
        {
            _currentState?.Exit();
            _currentState = null;
            _previousState = null;
            _activeTransition = null;

            foreach (var param in _parameters)
            {
                switch (param.Value.Type)
                {
                    case ParameterType.Float:
                        _parameterValues[param.Key] = param.Value.FloatDefaultValue;
                        break;
                    case ParameterType.Int:
                        _parameterValues[param.Key] = param.Value.IntDefaultValue;
                        break;
                    case ParameterType.Bool:
                        _parameterValues[param.Key] = param.Value.BoolDefaultValue;
                        break;
                    case ParameterType.Trigger:
                        _parameterValues[param.Key] = false;
                        break;
                }
            }
        }

        public Dictionary<string, object?> GetCurrentPose()
        {
            if (_currentState is AnimationState animState)
            {
                return animState.GetPoseAtTime(animState.GetCurrentTime());
            }
            else if (_currentState is BlendState blendState && blendState.BlendTree != null)
            {
                return blendState.BlendTree.GetBlendedPose(blendState.BlendTree.Children.FirstOrDefault()?.GetCurrentTime() ?? 0f);
            }
            else if (_currentState is OneShotState oneShotState)
            {
                if (oneShotState.Clip != null)
                {
                    var pose = new Dictionary<string, object?>();
                    foreach (var track in oneShotState.Clip.Tracks)
                    {
                        pose[track.TargetPath] = track.GetValueAtTime(oneShotState.GetCurrentTime());
                    }
                    return pose;
                }
            }
            else if (_currentState is SubStateMachine subStateMachine)
            {
                return subStateMachine.GetChildStateMachine()?.GetCurrentPose() ?? new Dictionary<string, object?>();
            }

            return new Dictionary<string, object?>();
        }
    }

    internal class StateMachineLayerImpl : StateMachineLayer
    {
        private readonly StateMachine _stateMachine;
        private State? _currentState;
        private State? _previousState;
        private Transition? _activeTransition;

        public override State? CurrentState => _currentState;

        public StateMachineLayerImpl(string name, StateMachine stateMachine)
        {
            Name = name;
            _stateMachine = stateMachine;
        }

        public override void Update(float deltaTime)
        {
            _currentState?.Update(deltaTime);
        }

        public override void Evaluate()
        {
        }
    }

    public enum IKGoal
    {
        LeftFoot,
        RightFoot,
        LeftHand,
        RightHand
    }

    public enum IKHint
    {
        LeftElbow,
        RightElbow,
        LeftKnee,
        RightKnee
    }

    public class IKSettings
    {
        public bool IsEnabled { get; set; } = false;
        public float Weight { get; set; } = 1.0f;
        public Vector2 TargetPosition { get; set; } = Vector2.Zero;
        public float TargetRotation { get; set; } = 0f;
        public IKGoal Goal { get; set; }

        public IKSettings()
        {
        }

        public IKSettings(IKGoal goal)
        {
            Goal = goal;
        }
    }

    public class IKLimbSolver
    {
        private Vector2 _jointPosition;
        private Vector2 _targetPosition;
        private float _jointLength;
        private float _totalLength;
        private float _baseRotation;
        private float _midRotation;
        private float _endRotation;
        private float _weight;

        public Vector2 JointPosition => _jointPosition;
        public float BaseRotation => _baseRotation;
        public float MidRotation => _midRotation;
        public float EndRotation => _endRotation;

        public IKLimbSolver()
        {
        }

        public void Solve(Vector2 rootPos, Vector2 targetPos, float upperLimbLength, float lowerLimbLength, float weight = 1.0f)
        {
            _weight = weight;
            _targetPosition = targetPos;
            _jointLength = lowerLimbLength;
            _totalLength = upperLimbLength + lowerLimbLength;

            Vector2 toTarget = targetPos - rootPos;
            float distance = toTarget.Length();

            if (distance > _totalLength)
            {
                float scale = _totalLength / distance;
                _targetPosition = rootPos + toTarget * scale;
                distance = _totalLength;
            }

            if (distance < Math.Abs(upperLimbLength - lowerLimbLength))
            {
                float scale = Math.Abs(upperLimbLength - lowerLimbLength) / (distance + 0.0001f);
                _targetPosition = rootPos + (targetPos - rootPos) * scale;
                distance = Math.Abs(upperLimbLength - lowerLimbLength);
            }

            float cosAngleA = (upperLimbLength * upperLimbLength + distance * distance - lowerLimbLength * lowerLimbLength) /
                              (2f * upperLimbLength * distance);
            cosAngleA = MathHelper.Clamp(cosAngleA, -1f, 1f);
            float angleA = (float)Math.Acos(cosAngleA);

            float cosAngleB = (upperLimbLength * upperLimbLength + lowerLimbLength * lowerLimbLength - distance * distance) /
                              (2f * upperLimbLength * lowerLimbLength);
            cosAngleB = MathHelper.Clamp(cosAngleB, -1f, 1f);
            float angleB = (float)Math.Acos(cosAngleB);

            Vector2 toTargetDir = (_targetPosition - rootPos).Normalized();
            float rootAngle = (float)Math.Atan2(toTargetDir.Y, toTargetDir.X);

            _baseRotation = rootAngle - angleA;
            _midRotation = MathHelper.Pi - angleB;

            _jointPosition = rootPos + new Vector2((float)Math.Cos(_baseRotation), (float)Math.Sin(_baseRotation)) * upperLimbLength;

            Vector2 toEndDir = (_targetPosition - _jointPosition).Normalized();
            _endRotation = (float)Math.Atan2(toEndDir.Y, toEndDir.X);

            if (_weight < 1.0f)
            {
                _jointPosition = Vector2.Lerp(rootPos + new Vector2(upperLimbLength, 0f), _jointPosition, _weight);
            }
        }

        public static Vector2 RotateAround(Vector2 point, Vector2 pivot, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);

            float x = point.X - pivot.X;
            float y = point.Y - pivot.Y;

            return new Vector2(
                x * cos - y * sin + pivot.X,
                x * sin + y * cos + pivot.Y
            );
        }
    }

    public class IKAimSolver
    {
        private Vector2 _rootPosition;
        private Vector2 _aimPosition;
        private Vector2 _targetPosition;
        private float _weight;
        private float _aimAngle;
        private float _rollAngle;
        private Vector2 _polePosition;

        public Vector2 RootPosition => _rootPosition;
        public Vector2 AimPosition => _aimPosition;
        public float AimAngle => _aimAngle;

        public IKAimSolver()
        {
        }

        public void Solve(
            Vector2 rootPos,
            Vector2 targetPos,
            Vector2 pole,
            float weight = 1.0f)
        {
            _rootPosition = rootPos;
            _targetPosition = targetPos;
            _weight = weight;

            Vector2 toTarget = _targetPosition - _rootPosition;
            _aimAngle = (float)Math.Atan2(toTarget.Y, toTarget.X);

            _aimPosition = _rootPosition + toTarget.Normalized() * 1f;

            if (pole != Vector2.Zero)
            {
                _polePosition = pole;
                float poleInfluence = CalculatePoleInfluence();
                Vector2 poleDir = (_polePosition - _rootPosition).Normalized();
                Vector2 aimDir = toTarget.Normalized();

                Vector2 bendDir = Vector2.Lerp(aimDir, poleDir, poleInfluence);
                _aimPosition = _rootPosition + bendDir.Normalized() * toTarget.Length();
            }
        }

        private float CalculatePoleInfluence()
        {
            if (_weight <= 0f)
                return 0f;

            return MathHelper.Clamp(_weight, 0f, 1f);
        }

        public Vector2 GetPositionAlongBone(float normalizedPosition)
        {
            return Vector2.Lerp(_rootPosition, _aimPosition, normalizedPosition);
        }
    }

    public class IKHeadSolver
    {
        private Vector2 _headPosition;
        private Vector2 _lookAtPosition;
        private float _weight;
        private float _clampWeight;
        private float _yawAngle;
        private float _pitchAngle;

        public Vector2 HeadPosition => _headPosition;
        public Vector2 LookAtPosition => _lookAtPosition;
        public float YawAngle => _yawAngle;
        public float PitchAngle => _pitchAngle;

        public IKHeadSolver()
        {
            _clampWeight = 0.3f;
        }

        public void Solve(
            Vector2 headPos,
            Vector2 targetPos,
            Vector2 forward,
            float weight = 1.0f,
            float clampWeight = 0.3f)
        {
            _headPosition = headPos;
            _weight = weight;
            _clampWeight = clampWeight;

            Vector2 toTarget = targetPos - _headPosition;
            float distance = toTarget.Length();

            if (distance < 0.0001f)
            {
                _lookAtPosition = _headPosition;
                return;
            }

            Vector2 lookDir = toTarget.Normalized();
            Vector2 forwardDir = forward.Normalized();

            float dot = Vector2.Dot(lookDir, forwardDir);
            dot = MathHelper.Clamp(dot, -1f + _clampWeight, 1f - _clampWeight);

            _yawAngle = (float)Math.Acos(dot);

            if (dot < 0)
            {
                _yawAngle = MathHelper.Pi - _yawAngle * (1f / (1f - (-1f + _clampWeight)));
            }
            else
            {
                _yawAngle = MathHelper.Pi - _yawAngle * (1f / (1f - (1f - _clampWeight)));
            }

            Vector2 cross = new Vector2(
                lookDir.X * forwardDir.Y - lookDir.Y * forwardDir.X,
                -(lookDir.X * forwardDir.X + lookDir.Y * forwardDir.Y) + 1f
            );

            if (cross.X < 0)
                _yawAngle = -_yawAngle;

            _yawAngle *= _weight;

            Vector2 clampedLookDir = new Vector2(
                (float)(lookDir.X * Math.Cos(-_yawAngle) - lookDir.Y * Math.Sin(-_yawAngle)),
                (float)(lookDir.X * Math.Sin(-_yawAngle) + lookDir.Y * Math.Cos(-_yawAngle))
            );

            _lookAtPosition = _headPosition + clampedLookDir * distance;
        }
    }

    public class IKSystem
    {
        private readonly Dictionary<IKGoal, IKSettings> _goalSettings;
        private readonly Dictionary<IKHint, IKSettings> _hintSettings;
        private readonly IKLimbSolver _limbSolver;
        private readonly IKAimSolver _aimSolver;
        private readonly IKHeadSolver _headSolver;
        private bool _isEnabled;
        private float _globalWeight;

        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public float GlobalWeight
        {
            get => _globalWeight;
            set => _globalWeight = MathHelper.Clamp(value, 0f, 1f);
        }

        public IKSystem()
        {
            _goalSettings = new Dictionary<IKGoal, IKSettings>();
            _hintSettings = new Dictionary<IKHint, IKSettings>();
            _limbSolver = new IKLimbSolver();
            _aimSolver = new IKAimSolver();
            _headSolver = new IKHeadSolver();
            _isEnabled = true;
            _globalWeight = 1.0f;

            foreach (IKGoal goal in Enum.GetValues(typeof(IKGoal)))
            {
                _goalSettings[goal] = new IKSettings(goal);
            }

            foreach (IKHint hint in Enum.GetValues(typeof(IKHint)))
            {
                _hintSettings[hint] = new IKSettings();
            }
        }

        public IKSettings GetGoalSettings(IKGoal goal)
        {
            return _goalSettings[goal];
        }

        public void SetGoalTarget(IKGoal goal, Vector2 position, float rotation = 0f)
        {
            var settings = _goalSettings[goal];
            settings.TargetPosition = position;
            settings.TargetRotation = rotation;
            settings.IsEnabled = true;
        }

        public void SetGoalWeight(IKGoal goal, float weight)
        {
            _goalSettings[goal].Weight = MathHelper.Clamp(weight, 0f, 1f);
        }

        public void DisableGoal(IKGoal goal)
        {
            _goalSettings[goal].IsEnabled = false;
        }

        public void Update()
        {
            if (!_isEnabled)
                return;

            foreach (var goal in _goalSettings)
            {
                if (goal.Value.IsEnabled)
                {
                    SolveGoal(goal.Key, goal.Value);
                }
            }
        }

        private void SolveGoal(IKGoal goal, IKSettings settings)
        {
            float weight = settings.Weight * _globalWeight;

            switch (goal)
            {
                case IKGoal.LeftFoot:
                case IKGoal.RightFoot:
                    SolveFoot(goal, settings, weight);
                    break;

                case IKGoal.LeftHand:
                case IKGoal.RightHand:
                    SolveHand(goal, settings, weight);
                    break;
            }
        }

        private void SolveFoot(IKGoal goal, IKSettings settings, float weight)
        {
            if (weight <= 0f)
                return;

            Vector2 rootPos = Vector2.Zero;
            float upperLength = 0.5f;
            float lowerLength = 0.5f;

            _limbSolver.Solve(rootPos, settings.TargetPosition, upperLength, lowerLength, weight);
        }

        private void SolveHand(IKGoal goal, IKSettings settings, float weight)
        {
            if (weight <= 0f)
                return;

            Vector2 rootPos = Vector2.Zero;
            float upperLength = 0.4f;
            float lowerLength = 0.4f;

            _limbSolver.Solve(rootPos, settings.TargetPosition, upperLength, lowerLength, weight);
        }

        public IKLimbSolver GetLimbSolver()
        {
            return _limbSolver;
        }

        public IKAimSolver GetAimSolver()
        {
            return _aimSolver;
        }

        public IKHeadSolver GetHeadSolver()
        {
            return _headSolver;
        }

        public void SolveLookAt(Vector2 headPosition, Vector2 targetPosition, Vector2 forward, float weight = 1.0f)
        {
            _headSolver.Solve(headPosition, targetPosition, forward, weight * _globalWeight);
        }

        public void SolveAimAt(Vector2 rootPosition, Vector2 targetPosition, Vector2 pole, float weight = 1.0f)
        {
            _aimSolver.Solve(rootPosition, targetPosition, pole, weight * _globalWeight);
        }
    }

    public class AnimationEvent
    {
        public string FunctionName { get; set; } = string.Empty;
        public float FloatParameter { get; set; }
        public int IntParameter { get; set; }
        public string StringParameter { get; set; } = string.Empty;
        public object? ObjectParameter { get; set; }
        public float Time { get; set; }
        public bool HasFired { get; set; } = false;
        public Action<AnimationEvent>? Callback { get; set; }

        public AnimationEvent()
        {
        }

        public AnimationEvent(string functionName, float time)
        {
            FunctionName = functionName;
            Time = time;
        }

        public void Invoke()
        {
            if (!HasFired)
            {
                HasFired = true;
                Callback?.Invoke(this);
            }
        }

        public void Reset()
        {
            HasFired = false;
        }
    }

    public class AnimationEventHandler
    {
        private readonly Dictionary<string, List<Action<AnimationEvent>>> _eventHandlers;
        private readonly List<AnimationEvent> _activeEvents;

        public AnimationEventHandler()
        {
            _eventHandlers = new Dictionary<string, List<Action<AnimationEvent>>>();
            _activeEvents = new List<AnimationEvent>();
        }

        public void RegisterEventHandler(string functionName, Action<AnimationEvent> handler)
        {
            if (!_eventHandlers.TryGetValue(functionName, out var handlers))
            {
                handlers = new List<Action<AnimationEvent>>();
                _eventHandlers[functionName] = handlers;
            }

            handlers.Add(handler);
        }

        public void UnregisterEventHandler(string functionName, Action<AnimationEvent> handler)
        {
            if (_eventHandlers.TryGetValue(functionName, out var handlers))
            {
                handlers.Remove(handler);
            }
        }

        public void TriggerEvent(string functionName, AnimationEvent evt)
        {
            if (_eventHandlers.TryGetValue(functionName, out var handlers))
            {
                foreach (var handler in handlers)
                {
                    handler(evt);
                }
            }
        }

        public void ProcessEvents(List<AnimationEvent> events, float currentTime, float previousTime)
        {
            foreach (var evt in events)
            {
                if (evt.HasFired)
                    continue;

                if (evt.Time > previousTime && evt.Time <= currentTime)
                {
                    evt.Invoke();
                    TriggerEvent(evt.FunctionName, evt);
                    _activeEvents.Add(evt);
                }
            }
        }

        public void ResetEvents(List<AnimationEvent> events)
        {
            foreach (var evt in events)
            {
                evt.Reset();
            }
            _activeEvents.Clear();
        }
    }

    public class RootMotionData
    {
        public Vector2 PositionDelta { get; set; }
        public float RotationDelta { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 AngularVelocity { get; set; }

        public RootMotionData()
        {
            PositionDelta = Vector2.Zero;
            RotationDelta = 0f;
            Velocity = Vector2.Zero;
            AngularVelocity = Vector2.Zero;
        }

        public void Apply(GameObject target, float deltaTime)
        {
            if (target == null)
                return;

            var transform = target.Transform;
            if (transform != null)
            {
                transform.Position += PositionDelta;
                transform.Rotation += RotationDelta;
            }
        }

        public void Clear()
        {
            PositionDelta = Vector2.Zero;
            RotationDelta = 0f;
            Velocity = Vector2.Zero;
            AngularVelocity = Vector2.Zero;
        }
    }

    public class RootMotionExtractor
    {
        private readonly RootMotionData _currentMotion;
        private readonly RootMotionData _previousMotion;
        private bool _isEnabled;

        public RootMotionData CurrentMotion => _currentMotion;
        public bool IsEnabled
        {
            get => _isEnabled;
            set => _isEnabled = value;
        }

        public RootMotionExtractor()
        {
            _currentMotion = new RootMotionData();
            _previousMotion = new RootMotionData();
            _isEnabled = true;
        }

        public void ExtractRootMotion(
            Vector2 startPosition,
            float startRotation,
            Vector2 endPosition,
            float endRotation,
            float deltaTime)
        {
            if (!_isEnabled)
                return;

            _previousMotion.PositionDelta = _currentMotion.PositionDelta;
            _previousMotion.RotationDelta = _currentMotion.RotationDelta;

            _currentMotion.PositionDelta = endPosition - startPosition;
            _currentMotion.RotationDelta = NormalizeAngle(endRotation - startRotation);

            if (deltaTime > 0)
            {
                _currentMotion.Velocity = _currentMotion.PositionDelta / deltaTime;
                _currentMotion.AngularVelocity = new Vector2(_currentMotion.RotationDelta / deltaTime, 0);
            }
        }

        private float NormalizeAngle(float angle)
        {
            while (angle > MathHelper.Pi) angle -= MathHelper.TwoPi;
            while (angle < -MathHelper.Pi) angle += MathHelper.TwoPi;
            return angle;
        }

        public void ApplyMotion(GameObject target, float deltaTime)
        {
            _currentMotion.Apply(target, deltaTime);
        }

        public Vector2 GetPositionDelta()
        {
            return _currentMotion.PositionDelta;
        }

        public float GetRotationDelta()
        {
            return _currentMotion.RotationDelta;
        }

        public void Clear()
        {
            _currentMotion.Clear();
            _previousMotion.Clear();
        }
    }

    public class SkeletonAnimation
    {
        private readonly Skeleton _skeleton;
        private StateMachine? _stateMachine;
        private readonly RootMotionExtractor _rootMotion;
        private readonly AnimationEventHandler _eventHandler;
        private readonly IKSystem _ikSystem;
        private readonly Dictionary<string, object?> _currentPose;
        private GameObject? _gameObject;
        private bool _isPlaying;

        public Skeleton Skeleton => _skeleton;
        public StateMachine? StateMachine => _stateMachine;
        public RootMotionExtractor RootMotion => _rootMotion;
        public AnimationEventHandler EventHandler => _eventHandler;
        public IKSystem IKSystem => _ikSystem;
        public bool IsPlaying => _isPlaying;

        public SkeletonAnimation()
        {
            _skeleton = new Skeleton();
            _rootMotion = new RootMotionExtractor();
            _eventHandler = new AnimationEventHandler();
            _ikSystem = new IKSystem();
            _currentPose = new Dictionary<string, object?>();
            _isPlaying = false;
        }

        public void SetGameObject(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public void CreateStateMachine()
        {
            _stateMachine = new StateMachine("SkeletonStateMachine");
            _isPlaying = true;
        }

        public StateMachine? GetStateMachine()
        {
            return _stateMachine;
        }

        public void Update(float deltaTime)
        {
            if (_stateMachine != null)
            {
                _stateMachine.Update(deltaTime);
                UpdatePose();
            }

            _ikSystem.Update();

            if (_rootMotion.IsEnabled && _stateMachine != null)
            {
                var pose = _stateMachine.GetCurrentPose();
            }
        }

        private void UpdatePose()
        {
            if (_stateMachine == null)
                return;

            _currentPose.Clear();
            var pose = _stateMachine.GetCurrentPose();

            foreach (var kvp in pose)
            {
                _currentPose[kvp.Key] = kvp.Value;
            }
        }

        public Dictionary<string, object?> GetCurrentPose()
        {
            return new Dictionary<string, object?>(_currentPose);
        }

        public object? GetBoneTransform(string boneName)
        {
            var bone = _skeleton.GetBone(boneName);
            if (bone != null)
            {
                return bone.LocalTransform;
            }
            return null;
        }

        public void ApplyPose(Dictionary<string, object?> pose)
        {
            foreach (var kvp in pose)
            {
                string[] parts = kvp.Key.Split('/');
                if (parts.Length == 0)
                    continue;

                var bone = _skeleton.GetBone(parts[parts.Length - 1]);
                if (bone != null)
                {
                    if (kvp.Value is Vector2 pos)
                    {
                        bone.Position = pos;
                    }
                }

                _currentPose[kvp.Key] = kvp.Value;
            }
        }

        public void SetFloat(string name, float value)
        {
            _stateMachine?.SetFloat(name, value);
        }

        public void SetBool(string name, bool value)
        {
            _stateMachine?.SetBool(name, value);
        }

        public void SetInt(string name, int value)
        {
            _stateMachine?.SetInt(name, value);
        }

        public void SetTrigger(string name)
        {
            _stateMachine?.SetTrigger(name);
        }

        public void ResetTrigger(string name)
        {
            _stateMachine?.ResetTrigger(name);
        }

        public float GetFloat(string name)
        {
            return _stateMachine?.GetFloat(name) ?? 0f;
        }

        public bool GetBool(string name)
        {
            return _stateMachine?.GetBool(name) ?? false;
        }

        public int GetInt(string name)
        {
            return _stateMachine?.GetInt(name) ?? 0;
        }

        public AnimationState CreateAnimationState(string stateName, AnimationClip? clip)
        {
            if (_stateMachine == null)
            {
                CreateStateMachine();
            }

            return _stateMachine!.CreateAnimationState(stateName, clip);
        }

        public BlendState CreateBlendState(string stateName, BlendTree? blendTree)
        {
            if (_stateMachine == null)
            {
                CreateStateMachine();
            }

            return _stateMachine!.CreateBlendState(stateName, blendTree);
        }

        public OneShotState CreateOneShotState(string stateName, AnimationClip? clip)
        {
            if (_stateMachine == null)
            {
                CreateStateMachine();
            }

            return _stateMachine!.CreateOneShotState(stateName, clip);
        }

        public void TransitionTo(string stateName)
        {
            _stateMachine?.TransitionTo(stateName);
        }

        public State? GetCurrentState()
        {
            return _stateMachine?.CurrentState;
        }

        public void ApplyRootMotion(bool apply)
        {
            _rootMotion.IsEnabled = apply;
        }

        public void ApplyIK(IKGoal goal, Vector2 targetPosition, float weight = 1.0f)
        {
            _ikSystem.SetGoalTarget(goal, targetPosition);
            _ikSystem.SetGoalWeight(goal, weight);
        }

        public void DisableIK(IKGoal goal)
        {
            _ikSystem.DisableGoal(goal);
        }
    }

    public class SpriteAnimationStateMachine
    {
        private readonly Dictionary<string, SpriteAnimation> _animations;
        private readonly StateMachine _stateMachine;
        private SpriteAnimation? _currentAnimation;
        private string? _currentAnimationName;
        private bool _isPlaying;

        public IReadOnlyDictionary<string, SpriteAnimation> Animations => _animations;
        public SpriteAnimation? CurrentAnimation => _currentAnimation;
        public string? CurrentAnimationName => _currentAnimationName;
        public bool IsPlaying => _isPlaying;
        public StateMachine StateMachine => _stateMachine;

        public event Action<string, string>? OnAnimationChanged;
        public event Action? OnAnimationFinished;

        public SpriteAnimationStateMachine()
        {
            _animations = new Dictionary<string, SpriteAnimation>();
            _stateMachine = new StateMachine("SpriteAnimationStateMachine");
            _isPlaying = false;

            SetupStateMachine();
        }

        private void SetupStateMachine()
        {
            _stateMachine.OnStateChanged += (from, to) =>
            {
                if (to != null)
                {
                    PlayAnimationByState(to.Name);
                }
            };
        }

        public void AddAnimation(string name, SpriteAnimation animation)
        {
            _animations[name] = animation;
        }

        public bool RemoveAnimation(string name)
        {
            return _animations.Remove(name);
        }

        public SpriteAnimation? GetAnimation(string name)
        {
            return _animations.TryGetValue(name, out var anim) ? anim : null;
        }

        public void CreateStatesFromAnimations()
        {
            foreach (var kvp in _animations)
            {
                var state = _stateMachine.CreateAnimationState(kvp.Key, CreateClipFromSpriteAnimation(kvp.Value));
            }
        }

        private AnimationClip? CreateClipFromSpriteAnimation(SpriteAnimation spriteAnim)
        {
            var clip = new AnimationClip(spriteAnim.CurrentAnimation?.ToString() ?? "GeneratedClip");

            foreach (var frame in spriteAnim.Frames)
            {
            }

            return clip;
        }

        public void Play(string name, bool forceRestart = false)
        {
            if (!_animations.TryGetValue(name, out var animation))
                return;

            if (_currentAnimationName == name && !forceRestart)
                return;

            var previousName = _currentAnimationName;
            _currentAnimation?.Stop();
            _currentAnimation = animation;
            _currentAnimationName = name;
            _currentAnimation.Play();
            _isPlaying = true;

            _stateMachine.TransitionTo(name);

            OnAnimationChanged?.Invoke(previousName ?? string.Empty, name);
        }

        private void PlayAnimationByState(string stateName)
        {
            if (_animations.TryGetValue(stateName, out var animation))
            {
                _currentAnimation = animation;
                _currentAnimationName = stateName;
                _currentAnimation.Play();
            }
        }

        public void Stop()
        {
            _currentAnimation?.Stop();
            _isPlaying = false;
        }

        public void Pause()
        {
            _isPlaying = false;
        }

        public void Resume()
        {
            if (_currentAnimation != null)
                _isPlaying = true;
        }

        public void Update(float deltaTime)
        {
            if (!_isPlaying || _currentAnimation == null)
                return;

            _stateMachine.Update(deltaTime);

            bool wasPlaying = _currentAnimation.IsPlaying;
            _currentAnimation.Update(deltaTime);

            if (wasPlaying && !_currentAnimation.IsPlaying)
            {
                OnAnimationFinished?.Invoke();
            }
        }

        public SpriteFrame? GetCurrentFrame()
        {
            return _currentAnimation?.GetCurrentFrame();
        }

        public void SetFloat(string name, float value)
        {
            _stateMachine.SetFloat(name, value);
        }

        public void SetBool(string name, bool value)
        {
            _stateMachine.SetBool(name, value);
        }

        public void SetInt(string name, int value)
        {
            _stateMachine.SetInt(name, value);
        }

        public void SetTrigger(string name)
        {
            _stateMachine.SetTrigger(name);
        }

        public float GetFloat(string name)
        {
            return _stateMachine.GetFloat(name);
        }

        public bool GetBool(string name)
        {
            return _stateMachine.GetBool(name);
        }

        public int GetInt(string name)
        {
            return _stateMachine.GetInt(name);
        }
    }

    public class AnimationStateMachineFactory
    {
        public static StateMachine CreateLocomotionStateMachine()
        {
            var stateMachine = new StateMachine("Locomotion");

            stateMachine.CreateFloatParameter("Speed", 0f);
            stateMachine.CreateFloatParameter("Direction", 0f);
            stateMachine.CreateBoolParameter("IsGrounded", true);
            stateMachine.CreateBoolParameter("IsMoving", false);
            stateMachine.CreateTriggerParameter("Jump");
            stateMachine.CreateTriggerParameter("Land");

            stateMachine.CreateParameter("VelocityX", ParameterType.Float);
            stateMachine.CreateParameter("VelocityY", ParameterType.Float);

            return stateMachine;
        }

        public static StateMachine CreateCharacterStateMachine()
        {
            var stateMachine = new StateMachine("Character");

            stateMachine.CreateFloatParameter("Horizontal", 0f);
            stateMachine.CreateFloatParameter("Vertical", 0f);
            stateMachine.CreateBoolParameter("IsRunning", false);
            stateMachine.CreateBoolParameter("IsCrouching", false);
            stateMachine.CreateBoolParameter("IsJumping", false);
            stateMachine.CreateBoolParameter("IsGrounded", true);
            stateMachine.CreateTriggerParameter("Attack");
            stateMachine.CreateTriggerParameter("Interact");

            var idle = stateMachine.CreateAnimationState("Idle");
            var walk = stateMachine.CreateAnimationState("Walk");
            var run = stateMachine.CreateAnimationState("Run");
            var jump = stateMachine.CreateOneShotState("Jump");
            var fall = stateMachine.CreateAnimationState("Fall");
            var crouch = stateMachine.CreateAnimationState("Crouch");

            walk.AddTransition(new Transition(idle)
                .AddCondition("IsMoving", ConditionMode.IfNot, 0.1f)
                .SetDuration(0.1f));

            idle.AddTransition(new Transition(walk)
                .AddCondition("IsMoving", ConditionMode.If, 0.1f)
                .SetDuration(0.1f));

            walk.AddTransition(new Transition(run)
                .AddCondition("IsRunning", ConditionMode.If, 1f)
                .SetDuration(0.15f));

            run.AddTransition(new Transition(walk)
                .AddCondition("IsRunning", ConditionMode.IfNot, 0f)
                .SetDuration(0.15f));

            idle.AddTransition(new Transition(jump)
                .AddCondition("IsJumping", ConditionMode.If, 1f)
                .SetDuration(0.05f));

            walk.AddTransition(new Transition(jump)
                .AddCondition("IsJumping", ConditionMode.If, 1f)
                .SetDuration(0.05f));

            run.AddTransition(new Transition(jump)
                .AddCondition("IsJumping", ConditionMode.If, 1f)
                .SetDuration(0.05f));

            jump.AddTransition(new Transition(fall)
                .SetExitTime(0.8f)
                .SetDuration(0.1f));

            fall.AddTransition(new Transition(idle)
                .AddCondition("IsGrounded", ConditionMode.If, 1f)
                .SetExitTime(0.3f)
                .SetDuration(0.1f));

            idle.AddTransition(new Transition(crouch)
                .AddCondition("IsCrouching", ConditionMode.If, 1f)
                .SetDuration(0.1f));

            crouch.AddTransition(new Transition(idle)
                .AddCondition("IsCrouching", ConditionMode.IfNot, 0f)
                .SetDuration(0.1f));

            return stateMachine;
        }

        public static BlendTree CreateLocomotionBlendTree()
        {
            var blendTree = new BlendTree("LocomotionBlend");
            blendTree.BlendMode = BlendMode.OneDimensional;

            return blendTree;
        }

        public static BlendTree CreateDirectionalBlendTree()
        {
            var blendTree = new BlendTree("DirectionalBlend");
            blendTree.BlendMode = BlendMode.TwoDimensionalFreeDirectional;

            return blendTree;
        }
    }
}

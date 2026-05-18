using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WPFEngine.Math;

namespace WPFEngine.AI
{
    #region GOAP System

    public interface IGOAPState
    {
        bool GetCondition(string key);
        void SetCondition(string key, bool value);
        HashSet<string> GetAllConditions();
        IGOAPState Clone();
        float GetNumericValue(string key);
        void SetNumericValue(string key, float value);
    }

    public class GOAPWorldState : IGOAPState
    {
        private readonly Dictionary<string, bool> _boolConditions = new Dictionary<string, bool>();
        private readonly Dictionary<string, float> _numericConditions = new Dictionary<string, float>();

        public bool GetCondition(string key)
        {
            return _boolConditions.TryGetValue(key, out var value) && value;
        }

        public void SetCondition(string key, bool value)
        {
            _boolConditions[key] = value;
        }

        public HashSet<string> GetAllConditions()
        {
            return new HashSet<string>(_boolConditions.Keys);
        }

        public IGOAPState Clone()
        {
            var clone = new GOAPWorldState();
            foreach (var kvp in _boolConditions)
            {
                clone._boolConditions[kvp.Key] = kvp.Value;
            }
            foreach (var kvp in _numericConditions)
            {
                clone._numericConditions[kvp.Key] = kvp.Value;
            }
            return clone;
        }

        public float GetNumericValue(string key)
        {
            return _numericConditions.TryGetValue(key, out var value) ? value : 0f;
        }

        public void SetNumericValue(string key, float value)
        {
            _numericConditions[key] = value;
        }
    }

    public class GOAPGoal
    {
        public string Name { get; set; } = string.Empty;
        public float Priority { get; set; } = 1f;
        public Func<IGOAPState, float>? SatisfactionCalculator { get; set; }

        public float CalculateSatisfaction(IGOAPState state)
        {
            return SatisfactionCalculator?.Invoke(state) ?? 0f;
        }

        public bool IsValid(IGOAPState state)
        {
            return CalculateSatisfaction(state) >= 0.99f;
        }

        public virtual bool IsAchieved(IGOAPState state)
        {
            return IsValid(state);
        }
    }

    public class GOAPAction
    {
        public string Name { get; set; } = string.Empty;
        public float Cost { get; set; } = 1f;
        public Dictionary<string, bool> Preconditions { get; set; } = new Dictionary<string, bool>();
        public Dictionary<string, bool> Effects { get; set; } = new Dictionary<string, bool>();
        public Func<IGOAPState, bool>? PreconditionValidator { get; set; }
        public Action<IGOAPState>? EffectApplier { get; set; }
        public Func<IGOAPState, float>? CostModifier { get; set; }
        public float RemainingDuration { get; set; }
        public float TotalDuration { get; set; } = 1f;
        public bool IsInterruptible { get; set; } = true;
        public int MinAgentLevel { get; set; } = 0;

        public virtual bool CheckPreconditions(IGOAPState state)
        {
            foreach (var precondition in Preconditions)
            {
                if (state.GetCondition(precondition.Key) != precondition.Value)
                {
                    return false;
                }
            }

            return PreconditionValidator?.Invoke(state) ?? true;
        }

        public virtual void ApplyEffects(IGOAPState state)
        {
            foreach (var effect in Effects)
            {
                state.SetCondition(effect.Key, effect.Value);
            }

            EffectApplier?.Invoke(state);
        }

        public virtual float GetCost(IGOAPState state)
        {
            float modifiedCost = CostModifier?.Invoke(state) ?? Cost;
            return Math.Max(0.1f, modifiedCost);
        }

        public virtual bool CanExecute(IGOAPState state)
        {
            return CheckPreconditions(state) && RemainingDuration <= 0;
        }

        public virtual void Execute(IGOAPState state, float deltaTime)
        {
            if (RemainingDuration > 0)
            {
                RemainingDuration -= deltaTime;
                if (RemainingDuration < 0)
                    RemainingDuration = 0;
            }
        }

        public virtual bool IsExecuting()
        {
            return RemainingDuration > 0;
        }

        public void Reset()
        {
            RemainingDuration = TotalDuration;
        }

        public GOAPAction AddPrecondition(string key, bool value)
        {
            Preconditions[key] = value;
            return this;
        }

        public GOAPAction AddEffect(string key, bool value)
        {
            Effects[key] = value;
            return this;
        }
    }

    public class GOAPPlanner
    {
        private readonly int _maxIterations;
        private readonly float _maxPlanningTime;
        private readonly List<GOAPAction> _availableActions;
        private readonly HashSet<string> _processedGoals = new HashSet<string>();

        public int NodesGenerated { get; private set; }
        public int MaxDepthReached { get; private set; }

        public GOAPPlanner(List<GOAPAction> availableActions, int maxIterations = 1000, float maxPlanningTime = 0.1f)
        {
            _availableActions = availableActions ?? throw new ArgumentNullException(nameof(availableActions));
            _maxIterations = maxIterations;
            _maxPlanningTime = maxPlanningTime;
        }

        public List<GOAPAction>? Plan(IGOAPState startState, GOAPGoal goal, CancellationToken cancellationToken = default)
        {
            if (goal == null)
                throw new ArgumentNullException(nameof(goal));

            if (goal.IsAchieved(startState))
                return new List<GOAPAction>();

            var goalState = CreateGoalState(startState, goal);
            var plan = AStarSearch(startState, goalState, cancellationToken);

            _processedGoals.Add(goal.Name);
            return plan;
        }

        private IGOAPState CreateGoalState(IGOAPState currentState, GOAPGoal goal)
        {
            var goalState = currentState.Clone();
            return goalState;
        }

        private List<GOAPAction>? AStarSearch(IGOAPState startState, IGOAPState goalState, CancellationToken cancellationToken)
        {
            NodesGenerated = 0;
            MaxDepthReached = 0;

            var openSet = new SortedSet<GOAPNode>(new GOAPNodeComparer());
            var closedSet = new HashSet<string>();
            var nodeMap = new Dictionary<string, GOAPNode>();

            var startNode = new GOAPNode
            {
                State = startState.Clone(),
                Actions = new List<GOAPAction>(),
                CostSoFar = 0,
                HeuristicScore = CalculateHeuristic(startState, goalState),
                Depth = 0
            };

            string startKey = GetStateKey(startNode.State);
            openSet.Add(startNode);
            nodeMap[startKey] = startNode;

            int iterations = 0;
            DateTime startTime = DateTime.UtcNow;

            while (openSet.Count > 0 && iterations < _maxIterations)
            {
                if (cancellationToken.IsCancellationRequested)
                    return null;

                if ((DateTime.UtcNow - startTime).TotalSeconds > _maxPlanningTime)
                    return null;

                var current = openSet.Min;
                openSet.Remove(current);

                if (IsGoalState(current.State, goalState))
                {
                    return current.Actions;
                }

                string currentKey = GetStateKey(current.State);
                if (closedSet.Contains(currentKey))
                    continue;

                closedSet.Add(currentKey);
                nodesGenerated++;

                if (current.Depth > MaxDepthReached)
                    MaxDepthReached = current.Depth;

                foreach (var action in _availableActions)
                {
                    if (!action.CheckPreconditions(current.State))
                        continue;

                    var newState = current.State.Clone();
                    action.ApplyEffects(newState);

                    string newKey = GetStateKey(newState);
                    if (closedSet.Contains(newKey))
                        continue;

                    float actionCost = action.GetCost(current.State);
                    float newCostSoFar = current.CostSoFar + actionCost;
                    float newHeuristic = CalculateHeuristic(newState, goalState);
                    float newTotalScore = newCostSoFar + newHeuristic;

                    if (!nodeMap.TryGetValue(newKey, out var existingNode) || existingNode.CostSoFar > newCostSoFar)
                    {
                        var newNode = new GOAPNode
                        {
                            State = newState,
                            Actions = new List<GOAPAction>(current.Actions) { action },
                            CostSoFar = newCostSoFar,
                            HeuristicScore = newTotalScore,
                            Depth = current.Depth + 1,
                            Parent = current
                        };

                        openSet.Add(newNode);
                        nodeMap[newKey] = newNode;
                    }
                }

                iterations++;
            }

            return null;
        }

        private float CalculateHeuristic(IGOAPState state, IGOAPState goalState)
        {
            float heuristic = 0f;

            foreach (var condition in goalState.GetAllConditions())
            {
                bool goalValue = goalState.GetCondition(condition);
                bool currentValue = state.GetCondition(condition);

                if (goalValue != currentValue)
                {
                    heuristic += 1f;
                }
            }

            return heuristic;
        }

        private bool IsGoalState(IGOAPState state, IGOAPState goalState)
        {
            foreach (var condition in goalState.GetAllConditions())
            {
                if (state.GetCondition(condition) != goalState.GetCondition(condition))
                    return false;
            }
            return true;
        }

        private string GetStateKey(IGOAPState state)
        {
            var conditions = state.GetAllConditions().OrderBy(c => c);
            return string.Join(";", conditions.Select(c => $"{c}:{state.GetCondition(c)}"));
        }

        private class GOAPNode
        {
            public IGOAPState State { get; set; } = null!;
            public List<GOAPAction> Actions { get; set; } = null!;
            public float CostSoFar { get; set; }
            public float HeuristicScore { get; set; }
            public int Depth { get; set; }
            public GOAPNode? Parent { get; set; }
        }

        private class GOAPNodeComparer : IComparer<GOAPNode>
        {
            public int Compare(GOAPNode? x, GOAPNode? y)
            {
                if (x == null || y == null)
                    return 0;

                int scoreCompare = x.HeuristicScore.CompareTo(y.HeuristicScore);
                if (scoreCompare != 0)
                    return scoreCompare;

                int costCompare = x.CostSoFar.CompareTo(y.CostSoFar);
                if (costCompare != 0)
                    return costCompare;

                return x.Depth.CompareTo(y.Depth);
            }
        }
    }

    public class BeliefSystem
    {
        private readonly Dictionary<string, float> _beliefs = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _beliefDecay = new Dictionary<string, float>();
        private readonly Dictionary<string, float> _beliefGrowth = new Dictionary<string, float>();

        public float GetBelief(string key)
        {
            return _beliefs.TryGetValue(key, out var value) ? value : 0f;
        }

        public void SetBelief(string key, float value)
        {
            _beliefs[key] = Math.Clamp(value, 0f, 1f);
        }

        public void AddBelief(string key, float value, float decay = 0f, float growth = 0f)
        {
            _beliefs[key] = Math.Clamp(value, 0f, 1f);
            _beliefDecay[key] = decay;
            _beliefGrowth[key] = growth;
        }

        public void RemoveBelief(string key)
        {
            _beliefs.Remove(key);
            _beliefDecay.Remove(key);
            _beliefGrowth.Remove(key);
        }

        public void Update(float deltaTime)
        {
            var keysToUpdate = _beliefs.Keys.ToList();

            foreach (var key in keysToUpdate)
            {
                float decay = _beliefDecay.TryGetValue(key, out var d) ? d : 0f;
                float growth = _beliefGrowth.TryGetValue(key, out var g) ? g : 0f;

                _beliefs[key] += (growth - decay) * deltaTime;
                _beliefs[key] = Math.Clamp(_beliefs[key], 0f, 1f);

                if (_beliefs[key] <= 0f || _beliefs[key] >= 1f)
                {
                    if (_beliefs[key] <= 0f)
                        _beliefs[key] = 0f;
                    else
                        _beliefs[key] = 1f;
                }
            }
        }

        public bool IsConfident(string key, float threshold = 0.7f)
        {
            return GetBelief(key) >= threshold;
        }

        public void Clear()
        {
            _beliefs.Clear();
            _beliefDecay.Clear();
            _beliefGrowth.Clear();
        }

        public IReadOnlyDictionary<string, float> GetAllBeliefs()
        {
            return _beliefs;
        }
    }

    public class ActionQueue
    {
        private readonly Queue<GOAPAction> _actionQueue = new Queue<GOAPAction>();
        private GOAPAction? _currentAction;
        private bool _isExecuting;

        public event EventHandler<GOAPAction>? ActionStarted;
        public event EventHandler<GOAPAction>? ActionCompleted;
        public event EventHandler<GOAPAction>? ActionFailed;

        public GOAPAction? CurrentAction => _currentAction;
        public bool IsExecuting => _isExecuting;
        public int QueueLength => _actionQueue.Count;

        public void EnqueueAction(GOAPAction action)
        {
            _actionQueue.Enqueue(action);
        }

        public void EnqueueActions(IEnumerable<GOAPAction> actions)
        {
            foreach (var action in actions)
            {
                EnqueueAction(action);
            }
        }

        public void ClearQueue()
        {
            _actionQueue.Clear();
            if (_currentAction != null)
            {
                _currentAction.Reset();
                _currentAction = null;
            }
            _isExecuting = false;
        }

        public void Update(IGOAPState state, float deltaTime)
        {
            if (!_isExecuting && _actionQueue.Count > 0)
            {
                _currentAction = _actionQueue.Dequeue();
                _currentAction.Reset();
                _isExecuting = true;
                ActionStarted?.Invoke(this, _currentAction);
            }

            if (_currentAction != null)
            {
                _currentAction.Execute(state, deltaTime);

                if (!_currentAction.IsExecuting())
                {
                    _currentAction.ApplyEffects(state);
                    ActionCompleted?.Invoke(this, _currentAction);
                    _currentAction = null;
                    _isExecuting = false;
                }
            }
        }

        public void Interrupt()
        {
            if (_currentAction != null && _currentAction.IsInterruptible)
            {
                _currentAction.Reset();
                _currentAction = null;
                _isExecuting = false;
            }
        }

        public bool HasActions()
        {
            return _actionQueue.Count > 0 || _isExecuting;
        }

        public void SkipCurrentAction()
        {
            if (_currentAction != null)
            {
                _currentAction.RemainingDuration = 0;
            }
        }
    }

    public class GOAPAgent
    {
        private readonly BeliefSystem _beliefs;
        private readonly GOAPWorldState _currentState;
        private readonly List<GOAPGoal> _goals;
        private readonly List<GOAPAction> _actions;
        private readonly ActionQueue _actionQueue;
        private GOAPPlanner _planner;
        private GOAPGoal? _currentGoal;
        private float _planningInterval;
        private float _timeSinceLastPlanning;

        public BeliefSystem Beliefs => _beliefs;
        public GOAPWorldState CurrentState => _currentState;
        public IReadOnlyList<GOAPGoal> Goals => _goals;
        public IReadOnlyList<GOAPAction> Actions => _actions;
        public ActionQueue ActionQueue => _actionQueue;
        public GOAPGoal? CurrentGoal => _currentGoal;
        public int AgentLevel { get; set; } = 1;

        public event EventHandler<GOAPGoal>? GoalChanged;
        public event EventHandler<GOAPAction>? PlanGenerated;

        public GOAPAgent(float planningInterval = 1f)
        {
            _beliefs = new BeliefSystem();
            _currentState = new GOAPWorldState();
            _goals = new List<GOAPGoal>();
            _actions = new List<GOAPAction>();
            _actionQueue = new ActionQueue();
            _planningInterval = planningInterval;
            _timeSinceLastPlanning = 0f;
            _planner = new GOAPPlanner(_actions);
        }

        public void AddGoal(GOAPGoal goal)
        {
            _goals.Add(goal);
        }

        public void AddAction(GOAPAction action)
        {
            if (action.MinAgentLevel <= AgentLevel)
            {
                _actions.Add(action);
                _planner = new GOAPPlanner(_actions);
            }
        }

        public void RemoveGoal(GOAPGoal goal)
        {
            _goals.Remove(goal);
            if (_currentGoal == goal)
            {
                _currentGoal = null;
            }
        }

        public void RemoveAction(GOAPAction action)
        {
            _actions.Remove(action);
            _planner = new GOAPPlanner(_actions);
        }

        public virtual void Update(float deltaTime)
        {
            _beliefs.Update(deltaTime);
            _actionQueue.Update(_currentState, deltaTime);

            _timeSinceLastPlanning += deltaTime;

            if (_timeSinceLastPlanning >= _planningInterval && !_actionQueue.IsExecuting)
            {
                Plan();
                _timeSinceLastPlanning = 0f;
            }
        }

        public virtual void Plan()
        {
            if (_goals.Count == 0)
                return;

            var validGoals = _goals
                .Where(g => !g.IsAchieved(_currentState))
                .OrderByDescending(g => g.Priority * g.CalculateSatisfaction(_currentState))
                .ToList();

            if (validGoals.Count == 0)
                return;

            foreach (var goal in validGoals)
            {
                var plan = _planner.Plan(_currentState, goal);

                if (plan != null && plan.Count > 0)
                {
                    _currentGoal = goal;
                    _actionQueue.ClearQueue();
                    _actionQueue.EnqueueActions(plan);
                    GoalChanged?.Invoke(this, goal);
                    PlanGenerated?.Invoke(this, plan.First());
                    break;
                }
            }
        }

        public void UpdateState(string key, bool value)
        {
            _currentState.SetCondition(key, value);
        }

        public void UpdateBelief(string key, float value)
        {
            _beliefs.SetBelief(key, value);
        }

        public void ForceReplan()
        {
            _timeSinceLastPlanning = _planningInterval;
        }
    }

    #endregion

    #region Utility AI System

    public enum ConsiderationCurveType
    {
        Linear,
        Exponential,
        Logarithmic,
        Sigmoid,
        Step,
        Triangle,
        Trapezoid,
        Bell
    }

    public class Consideration
    {
        public string Name { get; set; } = string.Empty;
        public ConsiderationCurveType CurveType { get; set; } = ConsiderationCurveType.Linear;
        public float InputMin { get; set; } = 0f;
        public float InputMax { get; set; } = 1f;
        public float OutputMin { get; set; } = 0f;
        public float OutputMax { get; set; } = 1f;
        public float Steepness { get; set; } = 1f;
        public float Midpoint { get; set; } = 0.5f;
        public Func<float>? InputProvider { get; set; }
        public float Weight { get; set; } = 1f;
        public bool IsEnabled { get; set; } = true;

        public float Evaluate(float input)
        {
            if (!IsEnabled)
                return 0f;

            float normalizedInput = Math.Clamp((input - InputMin) / (InputMax - InputMin), 0f, 1f);

            return CurveType switch
            {
                ConsiderationCurveType.Linear => EvaluateLinear(normalizedInput),
                ConsiderationCurveType.Exponential => EvaluateExponential(normalizedInput),
                ConsiderationCurveType.Logarithmic => EvaluateLogarithmic(normalizedInput),
                ConsiderationCurveType.Sigmoid => EvaluateSigmoid(normalizedInput),
                ConsiderationCurveType.Step => EvaluateStep(normalizedInput),
                ConsiderationCurveType.Triangle => EvaluateTriangle(normalizedInput),
                ConsiderationCurveType.Trapezoid => EvaluateTrapezoid(normalizedInput),
                ConsiderationCurveType.Bell => EvaluateBell(normalizedInput),
                _ => normalizedInput
            };
        }

        private float EvaluateLinear(float x)
        {
            return OutputMin + (OutputMax - OutputMin) * x;
        }

        private float EvaluateExponential(float x)
        {
            float exponential = (float)Math.Pow(x, Steepness);
            return OutputMin + (OutputMax - OutputMin) * exponential;
        }

        private float EvaluateLogarithmic(float x)
        {
            if (x <= 0)
                return OutputMin;
            float logValue = (float)Math.Log(1 + x * (Math.E - 1));
            return OutputMin + (OutputMax - OutputMin) * logValue;
        }

        private float EvaluateSigmoid(float x)
        {
            float exponent = Steepness * (Midpoint - x) * 10;
            float sigmoid = 1f / (1f + (float)Math.Exp(exponent));
            return OutputMin + (OutputMax - OutputMin) * sigmoid;
        }

        private float EvaluateStep(float x)
        {
            return x >= Midpoint ? OutputMax : OutputMin;
        }

        private float EvaluateTriangle(float x)
        {
            float center = Midpoint;
            float width = Steepness;

            if (x < center - width || x > center + width)
                return OutputMin;

            if (x <= center)
                return OutputMin + (OutputMax - OutputMin) * (x - (center - width)) / width;

            return OutputMin + (OutputMax - OutputMin) * ((center + width) - x) / width;
        }

        private float EvaluateTrapezoid(float x)
        {
            float left = Midpoint - Steepness;
            float right = Midpoint + Steepness;

            if (x < left || x > right)
                return OutputMin;

            if (x >= left && x < Midpoint)
                return OutputMin + (OutputMax - OutputMin) * (x - left) / (Midpoint - left);

            if (x >= Midpoint && x <= right)
                return OutputMin + (OutputMax - OutputMin) * (right - x) / (right - Midpoint);

            return OutputMin;
        }

        private float EvaluateBell(float x)
        {
            float center = Midpoint;
            float width = Steepness;
            float exponent = -Math.Pow(x - center, 2) / (2 * Math.Pow(width, 2));
            float bell = (float)Math.Exp(exponent);
            return OutputMin + (OutputMax - OutputMin) * bell;
        }

        public float GetScore()
        {
            if (InputProvider == null)
                return 0f;

            float input = InputProvider();
            return Evaluate(input) * Weight;
        }

        public Consideration SetInput(Func<float> provider)
        {
            InputProvider = provider;
            return this;
        }
    }

    public class UtilityAction
    {
        public string Name { get; set; } = string.Empty;
        public List<Consideration> Considerations { get; set; } = new List<Consideration>();
        public float BaseScore { get; set; } = 0f;
        public Action? ExecuteAction { get; set; }
        public Func<bool>? CanExecutePredicate { get; set; }
        public float Score { get; private set; }

        public void AddConsideration(Consideration consideration)
        {
            Considerations.Add(consideration);
        }

        public float CalculateScore()
        {
            if (!CanExecutePredicate?.Invoke() ?? false)
            {
                Score = 0f;
                return Score;
            }

            float totalScore = BaseScore;

            foreach (var consideration in Considerations)
            {
                totalScore += consideration.GetScore();
            }

            Score = Math.Clamp(totalScore, 0f, 1f);
            return Score;
        }

        public void Execute()
        {
            ExecuteAction?.Invoke();
        }
    }

    public enum SelectionStrategy
    {
        HighestScore,
        EpsilonGreedy,
        Softmax
    }

    public class UtilityScoreCalculator
    {
        private readonly List<UtilityAction> _actions;
        private float _epsilon = 0.1f;
        private float _temperature = 1f;
        private readonly Random _random;
        private SelectionStrategy _strategy;

        public float Epsilon
        {
            get => _epsilon;
            set => _epsilon = Math.Clamp(value, 0f, 1f);
        }

        public float Temperature
        {
            get => _temperature;
            set => _temperature = Math.Max(0.01f, value);
        }

        public SelectionStrategy Strategy
        {
            get => _strategy;
            set => _strategy = value;
        }

        public IReadOnlyList<UtilityAction> Actions => _actions;

        public UtilityScoreCalculator(SelectionStrategy strategy = SelectionStrategy.HighestScore, int? seed = null)
        {
            _actions = new List<UtilityAction>();
            _strategy = strategy;
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
        }

        public void AddAction(UtilityAction action)
        {
            _actions.Add(action);
        }

        public void RemoveAction(UtilityAction action)
        {
            _actions.Remove(action);
        }

        public void ClearActions()
        {
            _actions.Clear();
        }

        public List<(UtilityAction Action, float Score)> GetAllScores()
        {
            var scores = new List<(UtilityAction, float)>();

            foreach (var action in _actions)
            {
                float score = action.CalculateScore();
                scores.Add((action, score));
            }

            return scores.OrderByDescending(s => s.Item2).ToList();
        }

        public UtilityAction? SelectAction()
        {
            var validActions = _actions.Where(a => a.CanExecutePredicate?.Invoke() ?? true).ToList();

            if (validActions.Count == 0)
                return null;

            foreach (var action in validActions)
            {
                action.CalculateScore();
            }

            return _strategy switch
            {
                SelectionStrategy.HighestScore => SelectHighestScore(validActions),
                SelectionStrategy.EpsilonGreedy => SelectEpsilonGreedy(validActions),
                SelectionStrategy.Softmax => SelectSoftmax(validActions),
                _ => SelectHighestScore(validActions)
            };
        }

        private UtilityAction SelectHighestScore(List<UtilityAction> actions)
        {
            return actions.OrderByDescending(a => a.Score).FirstOrDefault();
        }

        private UtilityAction SelectEpsilonGreedy(List<UtilityAction> actions)
        {
            if (_random.NextDouble() < _epsilon)
            {
                return actions[_random.Next(actions.Count)];
            }

            return actions.OrderByDescending(a => a.Score).FirstOrDefault();
        }

        private UtilityAction SelectSoftmax(List<UtilityAction> actions)
        {
            if (actions.Count == 0)
                return null!;

            if (actions.Count == 1)
                return actions[0];

            var scores = actions.Select(a => a.Score).ToList();
            float maxScore = scores.Max();

            var expScores = scores.Select(s => (float)Math.Exp((s - maxScore) / _temperature)).ToList();
            float sumExpScores = expScores.Sum();

            if (sumExpScores <= 0)
                return actions[0];

            float randomValue = (float)_random.NextDouble() * sumExpScores;
            float cumulative = 0f;

            for (int i = 0; i < actions.Count; i++)
            {
                cumulative += expScores[i];
                if (randomValue <= cumulative)
                    return actions[i];
            }

            return actions.Last();
        }
    }

    public class UtilityAI
    {
        private readonly UtilityScoreCalculator _calculator;
        private UtilityAction? _currentAction;
        private bool _isRunning;

        public UtilityScoreCalculator Calculator => _calculator;
        public UtilityAction? CurrentAction => _currentAction;
        public bool IsRunning => _isRunning;

        public event EventHandler<UtilityAction>? ActionSelected;
        public event EventHandler? UpdateTick;

        public UtilityAI(SelectionStrategy strategy = SelectionStrategy.HighestScore)
        {
            _calculator = new UtilityScoreCalculator(strategy);
            _isRunning = false;
        }

        public void Update(float deltaTime)
        {
            var selectedAction = _calculator.SelectAction();

            if (selectedAction != null && selectedAction != _currentAction)
            {
                _currentAction = selectedAction;
                _currentAction.Execute();
                ActionSelected?.Invoke(this, _currentAction);
            }

            UpdateTick?.Invoke(this, EventArgs.Empty);
        }

        public void AddAction(UtilityAction action)
        {
            _calculator.AddAction(action);
        }

        public void RemoveAction(UtilityAction action)
        {
            _calculator.RemoveAction(action);
            if (_currentAction == action)
            {
                _currentAction = null;
            }
        }

        public UtilityAction? EvaluateAll()
        {
            return _calculator.SelectAction();
        }

        public List<(UtilityAction Action, float Score)> GetRankedActions()
        {
            return _calculator.GetAllScores();
        }
    }

    #endregion

    #region Fuzzy Logic System

    public enum MembershipFunctionType
    {
        Triangle,
        Trapezoid,
        Gaussian,
        Sigmoid,
        Singleton,
        Rectangular
    }

    public class FuzzySet
    {
        public string Name { get; set; } = string.Empty;
        public MembershipFunctionType FunctionType { get; set; } = MembershipFunctionType.Triangle;
        public float Min { get; set; }
        public float Max { get; set; }
        public float Peak { get; set; }
        public float LeftSlope { get; set; } = 1f;
        public float RightSlope { get; set; } = 1f;

        public FuzzySet() { }

        public FuzzySet(string name, MembershipFunctionType type, float min, float max, float peak)
        {
            Name = name;
            FunctionType = type;
            Min = min;
            Max = max;
            Peak = peak;
        }

        public virtual float GetMembership(float value)
        {
            if (value < Min || value > Max)
                return 0f;

            return FunctionType switch
            {
                MembershipFunctionType.Triangle => CalculateTriangle(value),
                MembershipFunctionType.Trapezoid => CalculateTrapezoid(value),
                MembershipFunctionType.Gaussian => CalculateGaussian(value),
                MembershipFunctionType.Sigmoid => CalculateSigmoid(value),
                MembershipFunctionType.Singleton => CalculateSingleton(value),
                MembershipFunctionType.Rectangular => CalculateRectangular(value),
                _ => CalculateTriangle(value)
            };
        }

        private float CalculateTriangle(float value)
        {
            if (value <= Min || value >= Max)
                return 0f;

            float center = (Min + Max) / 2f;

            if (value < center)
                return (value - Min) / (center - Min);

            return (Max - value) / (Max - center);
        }

        private float CalculateTrapezoid(float value)
        {
            if (value < Min || value > Max)
                return 0f;

            float left = Min + LeftSlope * (Peak - Min);
            float right = Peak + RightSlope * (Max - Peak);

            if (value >= left && value <= right)
                return 1f;

            if (value < left)
                return (value - Min) / (left - Min);

            return (Max - value) / (Max - right);
        }

        private float CalculateGaussian(float value)
        {
            float center = (Min + Max) / 2f;
            float sigma = (Max - Min) / 3f;
            float exponent = -Math.Pow(value - center, 2) / (2 * Math.Pow(sigma, 2));
            return (float)Math.Exp(exponent);
        }

        private float CalculateSigmoid(float value)
        {
            float center = (Min + Max) / 2f;
            float steepness = LeftSlope * 10f;
            return 1f / (1f + (float)Math.Exp(-steepness * (value - center)));
        }

        private float CalculateSingleton(float value)
        {
            return Math.Abs(value - Peak) < 0.001f ? 1f : 0f;
        }

        private float CalculateRectangular(float value)
        {
            return value >= Min && value <= Max ? 1f : 0f;
        }

        public void SetTriangle(float min, float max)
        {
            FunctionType = MembershipFunctionType.Triangle;
            Min = min;
            Max = max;
            Peak = (min + max) / 2f;
        }

        public void SetTrapezoid(float min, float max, float left, float right)
        {
            FunctionType = MembershipFunctionType.Trapezoid;
            Min = min;
            Max = max;
            Peak = left;
            LeftSlope = 1f;
            RightSlope = 1f;
        }

        public void SetGaussian(float min, float max)
        {
            FunctionType = MembershipFunctionType.Gaussian;
            Min = min;
            Max = max;
            Peak = (min + max) / 2f;
        }
    }

    public class LinguisticVariable
    {
        public string Name { get; set; } = string.Empty;
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public Dictionary<string, FuzzySet> Terms { get; } = new Dictionary<string, FuzzySet>();

        public LinguisticVariable() { }

        public LinguisticVariable(string name, float min, float max)
        {
            Name = name;
            MinValue = min;
            MaxValue = max;
        }

        public void AddTerm(string termName, FuzzySet set)
        {
            Terms[termName] = set;
        }

        public void AddTerm(string termName, MembershipFunctionType type, float min, float max, float peak)
        {
            Terms[termName] = new FuzzySet(termName, type, min, max, peak);
        }

        public Dictionary<string, float> Fuzzify(float value)
        {
            var memberships = new Dictionary<string, float>();

            foreach (var term in Terms)
            {
                memberships[term.Key] = term.Value.GetMembership(value);
            }

            return memberships;
        }

        public float GetMembership(string termName, float value)
        {
            if (Terms.TryGetValue(termName, out var set))
            {
                return set.GetMembership(value);
            }

            return 0f;
        }

        public string? GetDominantTerm(float value)
        {
            string? dominantTerm = null;
            float maxMembership = 0f;

            foreach (var term in Terms)
            {
                float membership = term.Value.GetMembership(value);
                if (membership > maxMembership)
                {
                    maxMembership = membership;
                    dominantTerm = term.Key;
                }
            }

            return dominantTerm;
        }
    }

    public class FuzzyRule
    {
        public string Name { get; set; } = string.Empty;
        public List<FuzzyCondition> Antecedents { get; set; } = new List<FuzzyCondition>();
        public List<string> Consequents { get; set; } = new List<string>();
        public float Weight { get; set; } = 1f;
        public bool IsEnabled { get; set; } = true;

        public FuzzyRule() { }

        public FuzzyRule(string name)
        {
            Name = name;
        }

        public void AddAntecedent(string variable, string term)
        {
            Antecedents.Add(new FuzzyCondition { Variable = variable, Term = term });
        }

        public void AddConsequent(string variable, string term)
        {
            Consequents.Add($"{variable}:{term}");
        }

        public float Evaluate(Dictionary<string, Dictionary<string, float>> fuzzifiedValues)
        {
            if (!IsEnabled || Antecedents.Count == 0)
                return 0f;

            float minMembership = 1f;

            foreach (var antecedent in Antecedents)
            {
                if (fuzzifiedValues.TryGetValue(antecedent.Variable, out var termMemberships))
                {
                    if (termMemberships.TryGetValue(antecedent.Term, out var membership))
                    {
                        minMembership = Math.Min(minMembership, membership);
                    }
                    else
                    {
                        return 0f;
                    }
                }
                else
                {
                    return 0f;
                }
            }

            return minMembership * Weight;
        }

        public Dictionary<string, float> GetConsequents()
        {
            var consequents = new Dictionary<string, float>();

            foreach (var consequent in Consequents)
            {
                var parts = consequent.Split(':');
                if (parts.Length == 2)
                {
                    consequents[parts[0]] = 0f;
                }
            }

            return consequents;
        }
    }

    public class FuzzyCondition
    {
        public string Variable { get; set; } = string.Empty;
        public string Term { get; set; } = string.Empty;
        public float Hedge { get; set; } = 1f;
    }

    public enum DefuzzificationMethod
    {
        Centroid,
        MaxMembership,
        MeanOfMaximum,
        WeightedAverage
    }

    public class FuzzyRuleSet
    {
        public List<FuzzyRule> Rules { get; } = new List<FuzzyRule>();
        public Dictionary<string, LinguisticVariable> InputVariables { get; } = new Dictionary<string, LinguisticVariable>();
        public Dictionary<string, LinguisticVariable> OutputVariables { get; } = new Dictionary<string, LinguisticVariable>();
        public DefuzzificationMethod DefuzzificationMethod { get; set; } = DefuzzificationMethod.Centroid;

        public void AddRule(FuzzyRule rule)
        {
            Rules.Add(rule);
        }

        public void RemoveRule(FuzzyRule rule)
        {
            Rules.Remove(rule);
        }

        public void AddInputVariable(LinguisticVariable variable)
        {
            InputVariables[variable.Name] = variable;
        }

        public void AddOutputVariable(LinguisticVariable variable)
        {
            OutputVariables[variable.Name] = variable;
        }

        public void AddRule(string ruleName, Action<FuzzyRule> configure)
        {
            var rule = new FuzzyRule(ruleName);
            configure(rule);
            Rules.Add(rule);
        }
    }

    public class FuzzyLogicEngine
    {
        private readonly FuzzyRuleSet _ruleSet;
        private readonly Dictionary<string, Dictionary<string, float>> _fuzzifiedInputs = new Dictionary<string, Dictionary<string, float>>();
        private readonly Dictionary<string, float> _aggregatedOutputs = new Dictionary<string, float>();

        public FuzzyRuleSet RuleSet => _ruleSet;

        public FuzzyLogicEngine(FuzzyRuleSet ruleSet)
        {
            _ruleSet = ruleSet ?? throw new ArgumentNullException(nameof(ruleSet));
        }

        public void SetInput(string variableName, float value)
        {
            if (_ruleSet.InputVariables.TryGetValue(variableName, out var variable))
            {
                _fuzzifiedInputs[variableName] = variable.Fuzzify(value);
            }
        }

        public void SetInputs(Dictionary<string, float> inputs)
        {
            foreach (var input in inputs)
            {
                SetInput(input.Key, input.Value);
            }
        }

        public Dictionary<string, float> Infer()
        {
            var ruleActivations = new List<(FuzzyRule Rule, float Activation)>();
            var outputTerms = new Dictionary<string, List<(string Term, float Activation)>>();

            foreach (var rule in _ruleSet.Rules)
            {
                float activation = rule.Evaluate(_fuzzifiedInputs);

                if (activation > 0)
                {
                    ruleActivations.Add((rule, activation));

                    foreach (var consequent in rule.Consequents)
                    {
                        var parts = consequent.Split(':');
                        if (parts.Length == 2)
                        {
                            string variableName = parts[0];
                            string termName = parts[1];

                            if (!outputTerms.ContainsKey(variableName))
                            {
                                outputTerms[variableName] = new List<(string, float)>();
                            }

                            outputTerms[variableName].Add((termName, activation));
                        }
                    }
                }
            }

            _aggregatedOutputs.Clear();

            foreach (var output in outputTerms)
            {
                if (_ruleSet.OutputVariables.TryGetValue(output.Key, out var variable))
                {
                    float defuzzifiedValue = Defuzzify(variable, output.Value);
                    _aggregatedOutputs[output.Key] = defuzzifiedValue;
                }
            }

            return new Dictionary<string, float>(_aggregatedOutputs);
        }

        public float GetOutput(string variableName)
        {
            return _aggregatedOutputs.TryGetValue(variableName, out var value) ? value : 0f;
        }

        public Dictionary<string, float> GetAllOutputs()
        {
            return new Dictionary<string, float>(_aggregatedOutputs);
        }

        private float Defuzzify(LinguisticVariable variable, List<(string Term, float Activation)> activatedTerms)
        {
            if (activatedTerms.Count == 0)
                return 0f;

            return _ruleSet.DefuzzificationMethod switch
            {
                DefuzzificationMethod.Centroid => DefuzzifyCentroid(variable, activatedTerms),
                DefuzzificationMethod.MaxMembership => DefuzzifyMaxMembership(variable, activatedTerms),
                DefuzzificationMethod.MeanOfMaximum => DefuzzifyMeanOfMaximum(variable, activatedTerms),
                DefuzzificationMethod.WeightedAverage => DefuzzifyWeightedAverage(variable, activatedTerms),
                _ => DefuzzifyCentroid(variable, activatedTerms)
            };
        }

        private float DefuzzifyCentroid(LinguisticVariable variable, List<(string Term, float Activation)> activatedTerms)
        {
            float numerator = 0f;
            float denominator = 0f;
            float step = (variable.MaxValue - variable.MinValue) / 100f;

            for (float x = variable.MinValue; x <= variable.MaxValue; x += step)
            {
                float clippedMembership = 0f;

                foreach (var (term, activation) in activatedTerms)
                {
                    if (variable.Terms.TryGetValue(term, out var fuzzySet))
                    {
                        float termMembership = fuzzySet.GetMembership(x);
                        clippedMembership = Math.Max(clippedMembership, Math.Min(termMembership, activation));
                    }
                }

                numerator += x * clippedMembership;
                denominator += clippedMembership;
            }

            return denominator > 0 ? numerator / denominator : (variable.MinValue + variable.MaxValue) / 2f;
        }

        private float DefuzzifyMaxMembership(LinguisticVariable variable, List<(string Term, float Activation)> activatedTerms)
        {
            float maxActivation = 0f;
            float bestX = variable.MinValue;

            for (float x = variable.MinValue; x <= variable.MaxValue; x += 0.01f)
            {
                float clippedMembership = 0f;

                foreach (var (term, activation) in activatedTerms)
                {
                    if (variable.Terms.TryGetValue(term, out var fuzzySet))
                    {
                        float termMembership = fuzzySet.GetMembership(x);
                        clippedMembership = Math.Max(clippedMembership, Math.Min(termMembership, activation));
                    }
                }

                if (clippedMembership > maxActivation)
                {
                    maxActivation = clippedMembership;
                    bestX = x;
                }
            }

            return bestX;
        }

        private float DefuzzifyMeanOfMaximum(LinguisticVariable variable, List<(string Term, float Activation)> activatedTerms)
        {
            float maxActivation = 0f;
            var xValues = new List<float>();

            for (float x = variable.MinValue; x <= variable.MaxValue; x += 0.01f)
            {
                float clippedMembership = 0f;

                foreach (var (term, activation) in activatedTerms)
                {
                    if (variable.Terms.TryGetValue(term, out var fuzzySet))
                    {
                        float termMembership = fuzzySet.GetMembership(x);
                        clippedMembership = Math.Max(clippedMembership, Math.Min(termMembership, activation));
                    }
                }

                if (clippedMembership > maxActivation)
                {
                    maxActivation = clippedMembership;
                    xValues.Clear();
                    xValues.Add(x);
                }
                else if (Math.Abs(clippedMembership - maxActivation) < 0.001f)
                {
                    xValues.Add(x);
                }
            }

            return xValues.Count > 0 ? xValues.Average() : (variable.MinValue + variable.MaxValue) / 2f;
        }

        private float DefuzzifyWeightedAverage(LinguisticVariable variable, List<(string Term, float Activation)> activatedTerms)
        {
            float numerator = 0f;
            float denominator = 0f;

            foreach (var (term, activation) in activatedTerms)
            {
                if (variable.Terms.TryGetValue(term, out var fuzzySet))
                {
                    float crispValue = fuzzySet.Peak;
                    numerator += crispValue * activation;
                    denominator += activation;
                }
            }

            return denominator > 0 ? numerator / denominator : (variable.MinValue + variable.MaxValue) / 2f;
        }

        public void Clear()
        {
            _fuzzifiedInputs.Clear();
            _aggregatedOutputs.Clear();
        }
    }

    #endregion

    #region Hierarchical Task Network (HTN)

    public enum TaskType
    {
        Primitive,
        Compound
    }

    public interface IHTNTask
    {
        string Name { get; set; }
        TaskType Type { get; }
        bool IsValid(IGOAPState state);
        IEnumerable<IHTNTask> Expand(IGOAPState state);
        bool Execute(IGOAPState state, float deltaTime);
        void Reset();
    }

    public class PrimitiveTask : IHTNTask
    {
        public string Name { get; set; } = string.Empty;
        public TaskType Type => TaskType.Primitive;
        public Func<IGOAPState, bool>? Precondition { get; set; }
        public Action<IGOAPState, float>? ExecuteAction { get; set; }
        public Action<IGOAPState>? EffectAction { get; set; }
        public float Duration { get; set; } = 1f;
        public float RemainingTime { get; private set; }
        public bool IsInterruptible { get; set; } = true;

        private bool _isExecuting;

        public PrimitiveTask() { }

        public PrimitiveTask(string name)
        {
            Name = name;
        }

        public bool IsValid(IGOAPState state)
        {
            return Precondition?.Invoke(state) ?? true;
        }

        public IEnumerable<IHTNTask> Expand(IGOAPState state)
        {
            yield break;
        }

        public bool Execute(IGOAPState state, float deltaTime)
        {
            if (!_isExecuting)
            {
                _isExecuting = true;
                RemainingTime = Duration;
            }

            if (RemainingTime > 0)
            {
                ExecuteAction?.Invoke(state, deltaTime);
                RemainingTime -= deltaTime;
                return false;
            }

            EffectAction?.Invoke(state);
            _isExecuting = false;
            return true;
        }

        public void Reset()
        {
            _isExecuting = false;
            RemainingTime = Duration;
        }
    }

    public class CompoundTask : IHTNTask
    {
        public string Name { get; set; } = string.Empty;
        public TaskType Type => TaskType.Compound;
        public List<HTNMethod> Methods { get; } = new List<HTNMethod>();
        public Func<IGOAPState, bool>? Precondition { get; set; }

        public CompoundTask() { }

        public CompoundTask(string name)
        {
            Name = name;
        }

        public void AddMethod(HTNMethod method)
        {
            Methods.Add(method);
        }

        public bool IsValid(IGOAPState state)
        {
            return Precondition?.Invoke(state) ?? true;
        }

        public IEnumerable<IHTNTask> Expand(IGOAPState state)
        {
            foreach (var method in Methods)
            {
                if (method.IsApplicable(state))
                {
                    return method.GetTasks(state);
                }
            }

            return Enumerable.Empty<IHTNTask>();
        }

        public bool Execute(IGOAPState state, float deltaTime)
        {
            return true;
        }

        public void Reset()
        {
        }
    }

    public class HTNMethod
    {
        public string Name { get; set; } = string.Empty;
        public Func<IGOAPState, bool>? ApplicabilityCondition { get; set; }
        public List<IHTNTask> Tasks { get; } = new List<IHTNTask>();

        public HTNMethod() { }

        public HTNMethod(string name)
        {
            Name = name;
        }

        public HTNMethod(string name, Func<IGOAPState, bool> condition)
        {
            Name = name;
            ApplicabilityCondition = condition;
        }

        public bool IsApplicable(IGOAPState state)
        {
            return ApplicabilityCondition?.Invoke(state) ?? true;
        }

        public IEnumerable<IHTNTask> GetTasks(IGOAPState state)
        {
            return Tasks;
        }

        public HTNMethod AddTask(IHTNTask task)
        {
            Tasks.Add(task);
            return this;
        }

        public HTNMethod AddPrimitive(string name, Action<IGOAPState, float> execute, Action<IGOAPState>? effect = null)
        {
            var task = new PrimitiveTask(name)
            {
                ExecuteAction = execute,
                EffectAction = effect
            };
            Tasks.Add(task);
            return this;
        }

        public HTNMethod AddCompound(string name, Action<CompoundTask> configure)
        {
            var task = new CompoundTask(name);
            configure(task);
            Tasks.Add(task);
            return this;
        }
    }

    public class TaskNetwork
    {
        private readonly List<IHTNTask> _tasks = new List<IHTNTask>();
        private int _currentIndex;
        private bool _isExecuting;

        public IReadOnlyList<IHTNTask> Tasks => _tasks;
        public int CurrentIndex => _currentIndex;
        public bool IsComplete => _currentIndex >= _tasks.Count;
        public bool IsExecuting => _isExecuting;

        public void AddTask(IHTNTask task)
        {
            _tasks.Add(task);
        }

        public void ClearTasks()
        {
            _tasks.Clear();
            _currentIndex = 0;
            _isExecuting = false;
        }

        public bool Execute(IGOAPState state, float deltaTime)
        {
            if (IsComplete)
                return true;

            _isExecuting = true;

            while (_currentIndex < _tasks.Count)
            {
                var currentTask = _tasks[_currentIndex];

                if (!currentTask.IsValid(state))
                {
                    _currentIndex++;
                    continue;
                }

                bool taskComplete = currentTask.Execute(state, deltaTime);

                if (!taskComplete && currentTask.Type == TaskType.Primitive)
                {
                    return false;
                }

                if (taskComplete)
                {
                    _currentIndex++;
                }
            }

            _isExecuting = false;
            return true;
        }

        public void Reset()
        {
            _currentIndex = 0;
            _isExecuting = false;
            foreach (var task in _tasks)
            {
                task.Reset();
            }
        }

        public void Interrupt()
        {
            if (_currentIndex < _tasks.Count)
            {
                var currentTask = _tasks[_currentIndex];
                if (currentTask is PrimitiveTask primitive && primitive.IsInterruptible)
                {
                    _currentIndex++;
                }
            }
        }
    }

    public class HTNPlanner
    {
        private readonly Dictionary<string, CompoundTask> _compoundTasks = new Dictionary<string, CompoundTask>();
        private readonly Dictionary<string, PrimitiveTask> _primitiveTasks = new Dictionary<string, PrimitiveTask>();
        private readonly Stack<IHTNTask> _taskStack = new Stack<IHTNTask>();

        public event EventHandler<IHTNTask>? TaskExpanded;
        public event EventHandler<IHTNTask>? TaskExecuted;

        public HTNPlanner()
        {
        }

        public void RegisterCompoundTask(CompoundTask task)
        {
            _compoundTasks[task.Name] = task;
        }

        public void RegisterPrimitiveTask(PrimitiveTask task)
        {
            _primitiveTasks[task.Name] = task;
        }

        public CompoundTask? GetCompoundTask(string name)
        {
            return _compoundTasks.TryGetValue(name, out var task) ? task : null;
        }

        public PrimitiveTask? GetPrimitiveTask(string name)
        {
            return _primitiveTasks.TryGetValue(name, out var task) ? task : null;
        }

        public TaskNetwork? CreatePlan(IGOAPState initialState, string rootTaskName)
        {
            _taskStack.Clear();

            if (!_compoundTasks.TryGetValue(rootTaskName, out var rootTask))
                return null;

            var network = new TaskNetwork();
            BuildNetwork(initialState, rootTask, network);

            return network;
        }

        private void BuildNetwork(IGOAPState state, CompoundTask compoundTask, TaskNetwork network)
        {
            foreach (var method in compoundTask.Methods)
            {
                if (!method.IsApplicable(state))
                    continue;

                foreach (var task in method.Tasks)
                {
                    if (task is CompoundTask compound)
                    {
                        BuildNetwork(state, compound, network);
                    }
                    else
                    {
                        network.AddTask(task);
                        TaskExpanded?.Invoke(this, task);
                    }
                }

                break;
            }
        }

        public TaskNetwork? CreatePlanWithDecomposition(IGOAPState state, IHTNTask rootTask, int maxDepth = 10)
        {
            var network = new TaskNetwork();
            var expanded = new HashSet<string>();

            if (!DecomposeTask(state, rootTask, network, expanded, 0, maxDepth))
                return null;

            return network;
        }

        private bool DecomposeTask(IGOAPState state, IHTNTask task, TaskNetwork network, 
            HashSet<string> expanded, int currentDepth, int maxDepth)
        {
            if (currentDepth >= maxDepth)
                return false;

            if (task is CompoundTask compound)
            {
                foreach (var method in compound.Methods)
                {
                    if (!method.IsApplicable(state))
                        continue;

                    foreach (var subTask in method.Tasks)
                    {
                        if (subTask is CompoundTask subCompound)
                        {
                            if (!DecomposeTask(state, subCompound, network, expanded, currentDepth + 1, maxDepth))
                                return false;
                        }
                        else
                        {
                            network.AddTask(subTask);
                            TaskExpanded?.Invoke(this, subTask);
                        }
                    }

                    return true;
                }

                return false;
            }
            else if (task is PrimitiveTask primitive)
            {
                network.AddTask(primitive);
                TaskExpanded?.Invoke(this, primitive);
                return true;
            }

            return true;
        }

        public void Clear()
        {
            _compoundTasks.Clear();
            _primitiveTasks.Clear();
            _taskStack.Clear();
        }
    }

    public class HTNAgent
    {
        private readonly HTNPlanner _planner;
        private readonly IGOAPState _worldState;
        private TaskNetwork? _currentPlan;
        private string _rootTaskName = string.Empty;
        private bool _isPlanning;

        public HTNPlanner Planner => _planner;
        public IGOAPState WorldState => _worldState;
        public TaskNetwork? CurrentPlan => _currentPlan;
        public bool IsPlanning => _isPlanning;
        public bool IsExecuting => _currentPlan?.IsExecuting ?? false;
        public string RootTaskName
        {
            get => _rootTaskName;
            set => _rootTaskName = value;
        }

        public event EventHandler<TaskNetwork>? PlanCreated;
        public event EventHandler? PlanCompleted;
        public event EventHandler? PlanFailed;

        public HTNAgent()
        {
            _planner = new HTNPlanner();
            _worldState = new GOAPWorldState();
        }

        public void CreatePlan()
        {
            if (string.IsNullOrEmpty(_rootTaskName))
                return;

            _isPlanning = true;
            _currentPlan = _planner.CreatePlan(_worldState, _rootTaskName);

            if (_currentPlan != null && _currentPlan.Tasks.Count > 0)
            {
                PlanCreated?.Invoke(this, _currentPlan);
            }
            else
            {
                PlanFailed?.Invoke(this, EventArgs.Empty);
            }

            _isPlanning = false;
        }

        public void Update(float deltaTime)
        {
            if (_currentPlan == null || _currentPlan.IsComplete)
            {
                if (_currentPlan?.IsComplete ?? false)
                {
                    PlanCompleted?.Invoke(this, EventArgs.Empty);
                }
                return;
            }

            bool complete = _currentPlan.Execute(_worldState, deltaTime);

            if (_currentPlan.IsComplete)
            {
                PlanCompleted?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Interrupt()
        {
            _currentPlan?.Interrupt();
        }

        public void Reset()
        {
            _currentPlan?.Reset();
        }

        public void SetWorldState(string key, bool value)
        {
            _worldState.SetCondition(key, value);
        }

        public void RegisterCompoundTask(string name, Action<CompoundTask> configure)
        {
            var task = new CompoundTask(name);
            configure(task);
            _planner.RegisterCompoundTask(task);
        }

        public void RegisterPrimitiveTask(string name, Action<IGOAPState, float> execute, 
            Action<IGOAPState>? effect = null, Func<IGOAPState, bool>? precondition = null)
        {
            var task = new PrimitiveTask(name)
            {
                ExecuteAction = execute,
                EffectAction = effect,
                Precondition = precondition
            };
            _planner.RegisterPrimitiveTask(task);
        }
    }

    #endregion

    #region AI Integration and Utilities

    public static class AIFactory
    {
        public static GOAPAgent CreateGOAPAgent(float planningInterval = 1f)
        {
            return new GOAPAgent(planningInterval);
        }

        public static UtilityAI CreateUtilityAI(SelectionStrategy strategy = SelectionStrategy.HighestScore)
        {
            return new UtilityAI(strategy);
        }

        public static FuzzyLogicEngine CreateFuzzyEngine()
        {
            var ruleSet = new FuzzyRuleSet();
            return new FuzzyLogicEngine(ruleSet);
        }

        public static HTNAgent CreateHTNAgent()
        {
            return new HTNAgent();
        }

        public static GOAPWorldState CreateDefaultWorldState()
        {
            return new GOAPWorldState();
        }

        public static FuzzyRuleSet CreateFuzzyRuleSet(DefuzzificationMethod method = DefuzzificationMethod.Centroid)
        {
            var ruleSet = new FuzzyRuleSet
            {
                DefuzzificationMethod = method
            };
            return ruleSet;
        }
    }

    public class AIManager
    {
        private readonly List<GOAPAgent> _goapAgents = new List<GOAPAgent>();
        private readonly List<UtilityAI> _utilityAgents = new List<UtilityAI>();
        private readonly List<HTNAgent> _htnAgents = new List<HTNAgent>();
        private readonly List<FuzzyLogicEngine> _fuzzyEngines = new List<FuzzyLogicEngine>();

        private static AIManager? _instance;
        public static AIManager Instance => _instance ??= new AIManager();

        public IReadOnlyList<GOAPAgent> GOAPAgents => _goapAgents;
        public IReadOnlyList<UtilityAI> UtilityAgents => _utilityAgents;
        public IReadOnlyList<HTNAgent> HTNAgents => _htnAgents;
        public IReadOnlyList<FuzzyLogicEngine> FuzzyEngines => _fuzzyEngines;

        public void RegisterGOAPAgent(GOAPAgent agent)
        {
            if (!_goapAgents.Contains(agent))
            {
                _goapAgents.Add(agent);
            }
        }

        public void UnregisterGOAPAgent(GOAPAgent agent)
        {
            _goapAgents.Remove(agent);
        }

        public void RegisterUtilityAI(UtilityAI ai)
        {
            if (!_utilityAgents.Contains(ai))
            {
                _utilityAgents.Add(ai);
            }
        }

        public void UnregisterUtilityAI(UtilityAI ai)
        {
            _utilityAgents.Remove(ai);
        }

        public void RegisterHTNAgent(HTNAgent agent)
        {
            if (!_htnAgents.Contains(agent))
            {
                _htnAgents.Add(agent);
            }
        }

        public void UnregisterHTNAgent(HTNAgent agent)
        {
            _htnAgents.Remove(agent);
        }

        public void RegisterFuzzyEngine(FuzzyLogicEngine engine)
        {
            if (!_fuzzyEngines.Contains(engine))
            {
                _fuzzyEngines.Add(engine);
            }
        }

        public void UnregisterFuzzyEngine(FuzzyLogicEngine engine)
        {
            _fuzzyEngines.Remove(engine);
        }

        public void Update(float deltaTime)
        {
            foreach (var agent in _goapAgents)
            {
                agent.Update(deltaTime);
            }

            foreach (var ai in _utilityAgents)
            {
                ai.Update(deltaTime);
            }

            foreach (var agent in _htnAgents)
            {
                agent.Update(deltaTime);
            }
        }

        public void Clear()
        {
            _goapAgents.Clear();
            _utilityAgents.Clear();
            _htnAgents.Clear();
            _fuzzyEngines.Clear();
        }
    }

    #endregion
}

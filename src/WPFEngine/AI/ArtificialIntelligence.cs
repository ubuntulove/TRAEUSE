using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WPFEngine.Math;

namespace WPFEngine.AI.Pathfinding
{
    public interface IPathNode
    {
        Vector2Int GridPosition { get; }
        float MovementCost { get; set; }
        float Heuristic { get; set; }
        float TotalCost { get; }
        bool IsWalkable { get; set; }
        IPathNode? Parent { get; set; }
    }

    public class PathNode<T> : IPathNode where T : IPathNode
    {
        public Vector2Int GridPosition { get; set; }
        public float MovementCost { get; set; }
        public float Heuristic { get; set; }
        public float TotalCost => MovementCost + Heuristic;
        public bool IsWalkable { get; set; } = true;
        public IPathNode? Parent { get; set; }
        public T? UserData { get; set; }

        public PathNode(Vector2Int position)
        {
            GridPosition = position;
            MovementCost = 0;
            Heuristic = 0;
        }
    }

    public class Pathfinder
    {
        private readonly int _width;
        private readonly int _height;
        private readonly Dictionary<Vector2Int, IPathNode> _nodes;
        private readonly IPathfindingHeuristic _heuristic;
        private readonly bool _allowDiagonalMovement;

        public int Width => _width;
        public int Height => _height;
        public IReadOnlyDictionary<Vector2Int, IPathNode> Nodes => _nodes;

        public Pathfinder(int width, int height, IPathfindingHeuristic? heuristic = null, bool allowDiagonalMovement = true)
        {
            _width = width;
            _height = height;
            _nodes = new Dictionary<Vector2Int, IPathNode>();
            _heuristic = heuristic ?? new ManhattanDistance();
            _allowDiagonalMovement = allowDiagonalMovement;

            InitializeNodes();
        }

        private void InitializeNodes()
        {
            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var position = new Vector2Int(x, y);
                    _nodes[position] = new PathNode<PathNode<object>>(position);
                }
            }
        }

        public IPathNode? GetNode(Vector2Int position)
        {
            return _nodes.TryGetValue(position, out var node) ? node : null;
        }

        public void SetWalkable(Vector2Int position, bool walkable)
        {
            if (_nodes.TryGetValue(position, out var node))
            {
                node.IsWalkable = walkable;
            }
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            return FindPath(start, end, out _);
        }

        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end, out float totalCost)
        {
            totalCost = 0;
            var path = new List<Vector2Int>();

            if (!_nodes.TryGetValue(start, out var startNode) || !_nodes.TryGetValue(end, out var endNode))
                return path;

            if (!startNode.IsWalkable || !endNode.IsWalkable)
                return path;

            var openSet = new SortedSet<PathNodeWrapper>(new PathNodeComparer());
            var openDict = new Dictionary<Vector2Int, PathNodeWrapper>();
            var closedSet = new HashSet<Vector2Int>();

            foreach (var node in _nodes.Values)
            {
                node.MovementCost = float.MaxValue;
                node.Parent = null;
            }

            startNode.MovementCost = 0;
            startNode.Heuristic = _heuristic.Calculate(start, end);

            var startWrapper = new PathNodeWrapper { Node = startNode, Position = start };
            openSet.Add(startWrapper);
            openDict[start] = startWrapper;

            Vector2Int[] directions;
            if (_allowDiagonalMovement)
            {
                directions = new Vector2Int[]
                {
                    new Vector2Int(-1, 0), new Vector2Int(1, 0),
                    new Vector2Int(0, -1), new Vector2Int(0, 1),
                    new Vector2Int(-1, -1), new Vector2Int(1, -1),
                    new Vector2Int(-1, 1), new Vector2Int(1, 1)
                };
            }
            else
            {
                directions = new Vector2Int[]
                {
                    new Vector2Int(-1, 0), new Vector2Int(1, 0),
                    new Vector2Int(0, -1), new Vector2Int(0, 1)
                };
            }

            while (openSet.Count > 0)
            {
                var current = openSet.Min;
                openSet.Remove(current);
                openDict.Remove(current.Position);

                if (current.Position == end)
                {
                    totalCost = current.Node.TotalCost;
                    return ReconstructPath(current.Node);
                }

                closedSet.Add(current.Position);

                foreach (var direction in directions)
                {
                    var neighborPos = current.Position + direction;

                    if (!_nodes.TryGetValue(neighborPos, out var neighbor))
                        continue;

                    if (!neighbor.IsWalkable || closedSet.Contains(neighborPos))
                        continue;

                    bool isDiagonal = direction.X != 0 && direction.Y != 0;
                    float moveCost = isDiagonal ? 1.414f : 1f;
                    float newMovementCost = current.Node.MovementCost + moveCost;

                    if (newMovementCost < neighbor.MovementCost)
                    {
                        neighbor.Parent = current.Node;
                        neighbor.MovementCost = newMovementCost;
                        neighbor.Heuristic = _heuristic.Calculate(neighborPos, end);

                        if (!openDict.TryGetValue(neighborPos, out var neighborWrapper))
                        {
                            neighborWrapper = new PathNodeWrapper { Node = neighbor, Position = neighborPos };
                            openSet.Add(neighborWrapper);
                            openDict[neighborPos] = neighborWrapper;
                        }
                        else
                        {
                            openSet.Remove(neighborWrapper);
                            openSet.Add(neighborWrapper);
                        }
                    }
                }
            }

            return path;
        }

        private List<Vector2Int> ReconstructPath(IPathNode endNode)
        {
            var path = new List<Vector2Int>();
            var current = endNode;

            while (current != null)
            {
                path.Add(current.GridPosition);
                current = current.Parent;
            }

            path.Reverse();
            return path;
        }

        public void Clear()
        {
            foreach (var node in _nodes.Values)
            {
                node.MovementCost = 0;
                node.Heuristic = 0;
                node.Parent = null;
            }
        }

        private struct PathNodeWrapper
        {
            public IPathNode Node;
            public Vector2Int Position;
        }

        private class PathNodeComparer : IComparer<PathNodeWrapper>
        {
            public int Compare(PathNodeWrapper x, PathNodeWrapper y)
            {
                int costCompare = x.Node.TotalCost.CompareTo(y.Node.TotalCost);
                if (costCompare != 0) return costCompare;

                int xCompare = x.Position.X.CompareTo(y.Position.X);
                if (xCompare != 0) return xCompare;

                return x.Position.Y.CompareTo(y.Position.Y);
            }
        }
    }

    public interface IPathfindingHeuristic
    {
        float Calculate(Vector2Int from, Vector2Int to);
    }

    public class ManhattanDistance : IPathfindingHeuristic
    {
        public float Calculate(Vector2Int from, Vector2Int to)
        {
            return Math.Abs(to.X - from.X) + Math.Abs(to.Y - from.Y);
        }
    }

    public class EuclideanDistance : IPathfindingHeuristic
    {
        public float Calculate(Vector2Int from, Vector2Int to)
        {
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }
    }

    public class ChebyshevDistance : IPathfindingHeuristic
    {
        public float Calculate(Vector2Int from, Vector2Int to)
        {
            return Math.Max(Math.Abs(to.X - from.X), Math.Abs(to.Y - from.Y));
        }
    }

    public class OctileDistance : IPathfindingHeuristic
    {
        public float Calculate(Vector2Int from, Vector2Int to)
        {
            int dx = Math.Abs(to.X - from.X);
            int dy = Math.Abs(to.Y - from.Y);
            float D = 1f;
            float D2 = 1.414f;
            return D * Math.Max(dx, dy) + (D2 - D) * Math.Min(dx, dy);
        }
    }

    public class FlowField
    {
        private readonly int _width;
        private readonly int _height;
        private readonly Vector2Int _target;
        private readonly Dictionary<Vector2Int, Vector2> _flowField;
        private readonly Pathfinder _pathfinder;

        public int Width => _width;
        public int Height => _height;
        public Vector2Int Target => _target;
        public IReadOnlyDictionary<Vector2Int, Vector2> FlowFieldData => _flowField;

        public FlowField(int width, int height, Vector2Int target, Pathfinder pathfinder)
        {
            _width = width;
            _height = height;
            _target = target;
            _pathfinder = pathfinder;
            _flowField = new Dictionary<Vector2Int, Vector2>();
        }

        public void Generate()
        {
            _flowField.Clear();

            var path = _pathfinder.FindPath(_target, new Vector2Int(_target.X + 100, _target.Y));

            for (int x = 0; x < _width; x++)
            {
                for (int y = 0; y < _height; y++)
                {
                    var pos = new Vector2Int(x, y);
                    var node = _pathfinder.GetNode(pos);

                    if (node == null || !node.IsWalkable)
                        continue;

                    var bestDirection = Vector2.Zero;
                    float bestCost = float.MaxValue;

                    Vector2Int[] directions = new Vector2Int[]
                    {
                        new Vector2Int(-1, 0), new Vector2Int(1, 0),
                        new Vector2Int(0, -1), new Vector2Int(0, 1),
                        new Vector2Int(-1, -1), new Vector2Int(1, -1),
                        new Vector2Int(-1, 1), new Vector2Int(1, 1)
                    };

                    foreach (var dir in directions)
                    {
                        var neighborPos = pos + dir;
                        var neighbor = _pathfinder.GetNode(neighborPos);

                        if (neighbor != null && neighbor.IsWalkable && neighbor.TotalCost < bestCost)
                        {
                            bestCost = neighbor.TotalCost;
                            bool isDiagonal = dir.X != 0 && dir.Y != 0;
                            bestDirection = new Vector2(dir.X, dir.Y).Normalized();
                        }
                    }

                    if (pos == _target)
                    {
                        bestDirection = Vector2.Zero;
                    }

                    _flowField[pos] = bestDirection;
                }
            }
        }

        public Vector2 GetFlowDirection(Vector2Int position)
        {
            return _flowField.TryGetValue(position, out var dir) ? dir : Vector2.Zero;
        }
    }

    public class NavigationMesh
    {
        private readonly List<NavMeshPolygon> _polygons;
        private readonly Dictionary<Vector2Int, List<int>> _spatialIndex;

        public IReadOnlyList<NavMeshPolygon> Polygons => _polygons;

        public NavigationMesh()
        {
            _polygons = new List<NavMeshPolygon>();
            _spatialIndex = new Dictionary<Vector2Int, List<int>>();
        }

        public void AddPolygon(NavMeshPolygon polygon)
        {
            int index = _polygons.Count;
            _polygons.Add(polygon);

            AABB bounds = polygon.GetBounds();
            Vector2Int minCell = WorldToCell(bounds.Min);
            Vector2Int maxCell = WorldToCell(bounds.Max);

            for (int x = minCell.X; x <= maxCell.X; x++)
            {
                for (int y = minCell.Y; y <= maxCell.Y; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (!_spatialIndex.TryGetValue(cell, out var list))
                    {
                        list = new List<int>();
                        _spatialIndex[cell] = list;
                    }
                    list.Add(index);
                }
            }
        }

        public NavMeshPolygon? GetPolygonAt(Vector2 point)
        {
            Vector2Int cell = WorldToCell(point);
            if (_spatialIndex.TryGetValue(cell, out var indices))
            {
                foreach (int index in indices)
                {
                    if (_polygons[index].ContainsPoint(point))
                        return _polygons[index];
                }
            }
            return null;
        }

        public List<NavMeshPolygon> GetPolygonsInRadius(Vector2 center, float radius)
        {
            var result = new List<NavMeshPolygon>();
            Vector2Int minCell = WorldToCell(center - new Vector2(radius));
            Vector2Int maxCell = WorldToCell(center + new Vector2(radius));

            for (int x = minCell.X; x <= maxCell.X; x++)
            {
                for (int y = minCell.Y; y <= maxCell.Y; y++)
                {
                    var cell = new Vector2Int(x, y);
                    if (_spatialIndex.TryGetValue(cell, out var indices))
                    {
                        foreach (int index in indices)
                        {
                            if (!result.Contains(_polygons[index]))
                            {
                                result.Add(_polygons[index]);
                            }
                        }
                    }
                }
            }

            return result;
        }

        public List<Vector2> FindPath(Vector2 start, Vector2 end)
        {
            var path = new List<Vector2>();
            var startPoly = GetPolygonAt(start);
            var endPoly = GetPolygonAt(end);

            if (startPoly == null || endPoly == null)
                return path;

            if (startPoly == endPoly)
            {
                path.Add(start);
                path.Add(end);
                return path;
            }

            path.Add(start);

            Vector2? currentPoint = start;
            var openSet = new SortedSet<PolygonNode>(new PolygonNodeComparer());
            var visited = new HashSet<int>();
            var nodeMap = new Dictionary<int, PolygonNode>();

            var startNode = new PolygonNode { Polygon = startPoly, Cost = 0, Heuristic = startPoly.GetBounds().Center.Distance(end) };
            openSet.Add(startNode);
            nodeMap[startPoly.Index] = startNode;

            while (openSet.Count > 0)
            {
                var current = openSet.Min;
                openSet.Remove(current);

                if (current.Polygon == endPoly)
                {
                    ReconstructPath(nodeMap, endPoly.Index, path);
                    path.Add(end);
                    return path;
                }

                visited.Add(current.Polygon.Index);

                foreach (var neighbor in current.Polygon.Connections)
                {
                    if (visited.Contains(neighbor))
                        continue;

                    Vector2 entryPoint = current.Polygon.GetNearestEdgePoint(end);
                    float moveCost = entryPoint.Distance(current.Polygon.GetBounds().Center);

                    if (!nodeMap.TryGetValue(neighbor, out var neighborNode))
                    {
                        neighborNode = new PolygonNode { Polygon = _polygons[neighbor], Cost = float.MaxValue };
                        nodeMap[neighbor] = neighborNode;
                    }

                    float newCost = current.Cost + moveCost;
                    if (newCost < neighborNode.Cost)
                    {
                        neighborNode.ParentIndex = current.Polygon.Index;
                        neighborNode.Cost = newCost;
                        neighborNode.Heuristic = _polygons[neighbor].GetBounds().Center.Distance(end);
                        openSet.Add(neighborNode);
                    }
                }
            }

            path.Add(end);
            return path;
        }

        private void ReconstructPath(Dictionary<int, PolygonNode> nodeMap, int endIndex, List<Vector2> path)
        {
            var reversed = new List<int>();
            int current = endIndex;

            while (nodeMap.TryGetValue(current, out var node) && node.ParentIndex >= 0)
            {
                reversed.Add(current);
                current = node.ParentIndex;
            }

            reversed.Reverse();
            foreach (int index in reversed)
            {
                path.Add(_polygons[index].GetBounds().Center);
            }
        }

        private Vector2Int WorldToCell(Vector2 point)
        {
            return new Vector2Int((int)Math.Floor(point.X), (int)Math.Floor(point.Y));
        }

        private class PolygonNode
        {
            public NavMeshPolygon? Polygon { get; set; }
            public int Index => Polygon?.Index ?? -1;
            public float Cost { get; set; }
            public float Heuristic { get; set; }
            public int ParentIndex { get; set; } = -1;
        }

        private class PolygonNodeComparer : IComparer<PolygonNode>
        {
            public int Compare(PolygonNode x, PolygonNode y)
            {
                float xTotal = x.Cost + x.Heuristic;
                float yTotal = y.Cost + y.Heuristic;
                return xTotal.CompareTo(yTotal);
            }
        }
    }

    public class NavMeshPolygon
    {
        private readonly List<Vector2> _vertices;
        private readonly List<int> _connections;
        private int _index;
        private AABB? _cachedBounds;

        public int Index
        {
            get => _index;
            set => _index = value;
        }

        public IReadOnlyList<Vector2> Vertices => _vertices;
        public IReadOnlyList<int> Connections => _connections;

        public NavMeshPolygon(IEnumerable<Vector2> vertices)
        {
            _vertices = vertices.ToList();
            _connections = new List<int>();
        }

        public bool ContainsPoint(Vector2 point)
        {
            if (_vertices.Count < 3)
                return false;

            bool inside = false;
            int j = _vertices.Count - 1;

            for (int i = 0; i < _vertices.Count; i++)
            {
                if ((_vertices[i].Y > point.Y) != (_vertices[j].Y > point.Y) &&
                    point.X < (_vertices[j].X - _vertices[i].X) * (point.Y - _vertices[i].Y) / (_vertices[j].Y - _vertices[i].Y) + _vertices[i].X)
                {
                    inside = !inside;
                }
                j = i;
            }

            return inside;
        }

        public AABB GetBounds()
        {
            if (_cachedBounds.HasValue)
                return _cachedBounds.Value;

            if (_vertices.Count == 0)
                return new AABB(Vector2.Zero, Vector2.Zero);

            Vector2 min = _vertices[0];
            Vector2 max = _vertices[0];

            for (int i = 1; i < _vertices.Count; i++)
            {
                min = Vector2.Min(min, _vertices[i]);
                max = Vector2.Max(max, _vertices[i]);
            }

            _cachedBounds = new AABB(min, max);
            return _cachedBounds.Value;
        }

        public Vector2 GetCenter()
        {
            return GetBounds().Center;
        }

        public Vector2 GetNearestEdgePoint(Vector2 target)
        {
            if (_vertices.Count < 2)
                return _vertices.Count > 0 ? _vertices[0] : Vector2.Zero;

            Vector2 nearest = _vertices[0];
            float minDist = float.MaxValue;

            for (int i = 0; i < _vertices.Count; i++)
            {
                Vector2 a = _vertices[i];
                Vector2 b = _vertices[(i + 1) % _vertices.Count];
                Vector2 closest = ClosestPointOnSegment(target, a, b);
                float dist = closest.Distance(target);

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = closest;
                }
            }

            return nearest;
        }

        private Vector2 ClosestPointOnSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            Vector2 ab = b - a;
            float t = (point - a).Dot(ab) / ab.Dot(ab);
            t = Math.Clamp(t, 0f, 1f);
            return a + ab * t;
        }

        public void AddConnection(int polygonIndex)
        {
            if (!_connections.Contains(polygonIndex))
            {
                _connections.Add(polygonIndex);
            }
        }
    }
}

namespace WPFEngine.AI.BehaviorTree
{
    public enum BehaviorStatus
    {
        Running,
        Success,
        Failure,
        Aborted
    }

    public abstract class BehaviorNode
    {
        public BehaviorStatus Status { get; protected set; } = BehaviorStatus.Running;
        public bool IsStarted { get; protected set; }
        public string? Name { get; set; }

        public virtual BehaviorStatus Tick()
        {
            if (!IsStarted)
            {
                OnStart();
                IsStarted = true;
            }

            Status = OnUpdate();

            if (Status != BehaviorStatus.Running)
            {
                OnEnd();
                IsStarted = false;
            }

            return Status;
        }

        public virtual void Reset()
        {
            Status = BehaviorStatus.Running;
            IsStarted = false;
        }

        public virtual void Abort()
        {
            if (IsStarted)
            {
                OnAbort();
                OnEnd();
            }
            Status = BehaviorStatus.Aborted;
            IsStarted = false;
        }

        protected virtual void OnStart() { }
        protected virtual BehaviorStatus OnUpdate() => BehaviorStatus.Success;
        protected virtual void OnEnd() { }
        protected virtual void OnAbort() { }
    }

    public class CompositeNode : BehaviorNode
    {
        protected readonly List<BehaviorNode> _children = new List<BehaviorNode>();
        public IReadOnlyList<BehaviorNode> Children => _children;

        public CompositeNode AddChild(BehaviorNode child)
        {
            _children.Add(child);
            return this;
        }

        public CompositeNode RemoveChild(BehaviorNode child)
        {
            _children.Remove(child);
            return this;
        }

        public CompositeNode ClearChildren()
        {
            _children.Clear();
            return this;
        }

        protected override void OnStart()
        {
            foreach (var child in _children)
            {
                child.Reset();
            }
        }
    }

    public class SequenceNode : CompositeNode
    {
        private int _currentChildIndex;

        protected override void OnStart()
        {
            base.OnStart();
            _currentChildIndex = 0;
        }

        protected override BehaviorStatus OnUpdate()
        {
            while (true)
            {
                if (_currentChildIndex >= _children.Count)
                    return BehaviorStatus.Success;

                var child = _children[_currentChildIndex];
                var childStatus = child.Tick();

                if (childStatus != BehaviorStatus.Success)
                    return childStatus;

                _currentChildIndex++;
            }
        }

        protected override void OnEnd()
        {
            _currentChildIndex = 0;
            base.OnEnd();
        }
    }

    public class SelectorNode : CompositeNode
    {
        private int _currentChildIndex;

        protected override void OnStart()
        {
            base.OnStart();
            _currentChildIndex = 0;
        }

        protected override BehaviorStatus OnUpdate()
        {
            while (true)
            {
                if (_currentChildIndex >= _children.Count)
                    return BehaviorStatus.Failure;

                var child = _children[_currentChildIndex];
                var childStatus = child.Tick();

                if (childStatus != BehaviorStatus.Failure)
                    return childStatus;

                _currentChildIndex++;
            }
        }

        protected override void OnEnd()
        {
            _currentChildIndex = 0;
            base.OnEnd();
        }
    }

    public class RandomSelectorNode : CompositeNode
    {
        private int _currentChildIndex;
        private readonly Random _random;
        private List<int> _shuffledIndices;

        public RandomSelectorNode(int? seed = null)
        {
            _random = seed.HasValue ? new Random(seed.Value) : new Random();
            _shuffledIndices = new List<int>();
        }

        protected override void OnStart()
        {
            base.OnStart();
            _shuffledIndices = Enumerable.Range(0, _children.Count).OrderBy(_ => _random.Next()).ToList();
            _currentChildIndex = 0;
        }

        protected override BehaviorStatus OnUpdate()
        {
            while (_currentChildIndex < _shuffledIndices.Count)
            {
                var child = _children[_shuffledIndices[_currentChildIndex]];
                var childStatus = child.Tick();

                if (childStatus != BehaviorStatus.Failure)
                    return childStatus;

                _currentChildIndex++;
            }

            return BehaviorStatus.Failure;
        }

        protected override void OnEnd()
        {
            _currentChildIndex = 0;
            base.OnEnd();
        }
    }

    public class ParallelNode : CompositeNode
    {
        private readonly int _requiredSuccessCount;
        private readonly int _requiredFailureCount;
        private int _successCount;
        private int _failureCount;

        public ParallelNode(int requiredSuccessCount = -1, int requiredFailureCount = -1)
        {
            _requiredSuccessCount = requiredSuccessCount < 0 ? _children.Count : requiredSuccessCount;
            _requiredFailureCount = requiredFailureCount < 0 ? _children.Count : requiredFailureCount;
        }

        protected override void OnStart()
        {
            base.OnStart();
            _successCount = 0;
            _failureCount = 0;
        }

        protected override BehaviorStatus OnUpdate()
        {
            int successCount = 0;
            int failureCount = 0;

            foreach (var child in _children)
            {
                if (child.Status == BehaviorStatus.Running)
                {
                    child.Tick();
                }

                if (child.Status == BehaviorStatus.Success)
                    successCount++;
                else if (child.Status == BehaviorStatus.Failure)
                    failureCount++;
            }

            if (successCount >= _requiredSuccessCount)
                return BehaviorStatus.Success;

            if (failureCount >= _requiredFailureCount)
                return BehaviorStatus.Failure;

            return BehaviorStatus.Running;
        }
    }

    public class SequenceWithMemoryNode : CompositeNode
    {
        private int _currentChildIndex;

        protected override void OnStart()
        {
            base.OnStart();
            _currentChildIndex = 0;
        }

        protected override BehaviorStatus OnUpdate()
        {
            if (_currentChildIndex >= _children.Count)
                return BehaviorStatus.Success;

            var child = _children[_currentChildIndex];

            if (child.Status == BehaviorStatus.Running)
            {
                var status = child.Tick();
                return status;
            }

            if (child.Status == BehaviorStatus.Success)
            {
                _currentChildIndex++;
                if (_currentChildIndex >= _children.Count)
                    return BehaviorStatus.Success;
                return BehaviorStatus.Running;
            }

            return child.Status;
        }

        protected override void OnEnd()
        {
            _currentChildIndex = 0;
            base.OnEnd();
        }
    }

    public class DecoratorNode : BehaviorNode
    {
        protected BehaviorNode? _child;

        public DecoratorNode SetChild(BehaviorNode child)
        {
            _child = child;
            return this;
        }

        public override void Reset()
        {
            base.Reset();
            _child?.Reset();
        }

        public override void Abort()
        {
            _child?.Abort();
            base.Abort();
        }

        protected override void OnStart()
        {
            _child?.Reset();
        }

        protected override void OnEnd()
        {
            _child?.Abort();
        }
    }

    public class InverterNode : DecoratorNode
    {
        protected override BehaviorStatus OnUpdate()
        {
            if (_child == null)
                return BehaviorStatus.Failure;

            var childStatus = _child.Tick();

            if (childStatus == BehaviorStatus.Running)
                return BehaviorStatus.Running;

            return childStatus == BehaviorStatus.Success
                ? BehaviorStatus.Failure
                : BehaviorStatus.Success;
        }
    }

    public class RepeaterNode : DecoratorNode
    {
        public int LoopCount { get; set; } = -1;
        private int _currentLoop;

        public RepeaterNode(int loopCount = -1)
        {
            LoopCount = loopCount;
        }

        protected override void OnStart()
        {
            base.OnStart();
            _currentLoop = 0;
        }

        protected override BehaviorStatus OnUpdate()
        {
            if (_child == null)
                return BehaviorStatus.Success;

            var childStatus = _child.Tick();

            if (childStatus == BehaviorStatus.Running)
                return BehaviorStatus.Running;

            if (childStatus == BehaviorStatus.Failure)
                return BehaviorStatus.Failure;

            _currentLoop++;

            if (LoopCount > 0 && _currentLoop >= LoopCount)
                return BehaviorStatus.Success;

            _child.Reset();
            return BehaviorStatus.Running;
        }

        protected override void OnEnd()
        {
            _currentLoop = 0;
            base.OnEnd();
        }
    }

    public class RepeatUntilSuccessNode : DecoratorNode
    {
        protected override BehaviorStatus OnUpdate()
        {
            if (_child == null)
                return BehaviorStatus.Failure;

            var childStatus = _child.Tick();

            if (childStatus == BehaviorStatus.Running)
                return BehaviorStatus.Running;

            if (childStatus == BehaviorStatus.Success)
                return BehaviorStatus.Success;

            _child.Reset();
            return BehaviorStatus.Running;
        }
    }

    public class RepeatUntilFailureNode : DecoratorNode
    {
        protected override BehaviorStatus OnUpdate()
        {
            if (_child == null)
                return BehaviorStatus.Failure;

            var childStatus = _child.Tick();

            if (childStatus == BehaviorStatus.Running)
                return BehaviorStatus.Running;

            if (childStatus == BehaviorStatus.Failure)
                return BehaviorStatus.Success;

            _child.Reset();
            return BehaviorStatus.Running;
        }
    }

    public class ConditionNode : BehaviorNode
    {
        private readonly Func<bool> _condition;

        public ConditionNode(Func<bool> condition)
        {
            _condition = condition;
        }

        protected override BehaviorStatus OnUpdate()
        {
            return _condition() ? BehaviorStatus.Success : BehaviorStatus.Failure;
        }
    }

    public class ActionNode : BehaviorNode
    {
        private readonly Func<BehaviorStatus> _action;

        public ActionNode(Func<BehaviorStatus> action)
        {
            _action = action;
        }

        protected override BehaviorStatus OnUpdate()
        {
            return _action();
        }
    }

    public class WaitNode : BehaviorNode
    {
        private float _waitTime;
        private float _elapsedTime;

        public WaitNode(float waitTime)
        {
            _waitTime = waitTime;
        }

        protected override void OnStart()
        {
            base.OnStart();
            _elapsedTime = 0;
        }

        protected override BehaviorStatus OnUpdate()
        {
            _elapsedTime += Time.deltaTime;
            return _elapsedTime >= _waitTime
                ? BehaviorStatus.Success
                : BehaviorStatus.Running;
        }
    }

    public class BehaviorTree
    {
        private readonly BehaviorNode _rootNode;
        private readonly object _context;
        private bool _isRunning;

        public BehaviorNode RootNode => _rootNode;
        public object Context => _context;
        public bool IsRunning => _isRunning;

        public event EventHandler<BehaviorStatus>? StatusChanged;

        public BehaviorTree(BehaviorNode rootNode, object context)
        {
            _rootNode = rootNode;
            _context = context;
            _isRunning = false;
        }

        public void Tick()
        {
            if (!_isRunning)
            {
                _isRunning = true;
            }

            var previousStatus = _rootNode.Status;
            _rootNode.Tick();

            if (_rootNode.Status != previousStatus)
            {
                StatusChanged?.Invoke(this, _rootNode.Status);
            }
        }

        public void Abort()
        {
            _rootNode.Abort();
            _isRunning = false;
        }

        public void Reset()
        {
            _rootNode.Reset();
            _isRunning = false;
        }
    }

    internal static class Time
    {
        internal static float deltaTime = 1f / 60f;
    }
}

namespace WPFEngine.AI.Steering
{
    public class SteeringAgent
    {
        public Vector2 Position { get; set; }
        public Vector2 Velocity { get; set; }
        public Vector2 Acceleration { get; set; }
        public float Rotation { get; set; }
        public float AngularVelocity { get; set; }
        public float MaxSpeed { get; set; } = 5f;
        public float MaxForce { get; set; } = 10f;
        public float MaxAngularSpeed { get; set; } = 5f;
        public float Mass { get; set; } = 1f;
        public float ArrivalRadius { get; set; } = 1f;
        public float WanderAngle { get; set; }

        public SteeringAgent(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
            Acceleration = Vector2.Zero;
            Rotation = 0f;
        }

        public void Update(float deltaTime)
        {
            Velocity += Acceleration * deltaTime;

            float speed = Velocity.Length();
            if (speed > MaxSpeed)
            {
                Velocity = Velocity.Normalized() * MaxSpeed;
            }

            Position += Velocity * deltaTime;

            Acceleration = Vector2.Zero;
        }

        public void ApplyForce(Vector2 force)
        {
            Acceleration += force / Mass;
        }

        public Vector2 ForwardVector => new Vector2(MathHelper.Cos(Rotation), MathHelper.Sin(Rotation));

        public Vector2 GetSteeringForce(ISteeringBehavior behavior)
        {
            return behavior.CalculateSteering(this);
        }
    }

    public interface ISteeringBehavior
    {
        Vector2 CalculateSteering(SteeringAgent agent);
    }

    public class SeekBehavior : ISteeringBehavior
    {
        public Vector2 TargetPosition { get; set; }
        public float ArrivalRadius { get; set; } = 1f;
        public float SlowingRadius { get; set; } = 5f;

        public SeekBehavior(Vector2 targetPosition)
        {
            TargetPosition = targetPosition;
        }

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            Vector2 desired = TargetPosition - agent.Position;
            float distance = desired.Length();

            if (distance < ArrivalRadius)
            {
                return Vector2.Zero;
            }

            float speed;
            if (distance < SlowingRadius)
            {
                speed = agent.MaxSpeed * (distance / SlowingRadius);
            }
            else
            {
                speed = agent.MaxSpeed;
            }

            desired = desired.Normalized() * speed;
            Vector2 steering = desired - agent.Velocity;
            return steering;
        }
    }

    public class FleeBehavior : ISteeringBehavior
    {
        public Vector2 TargetPosition { get; set; }
        public float PanicRadius { get; set; } = 10f;

        public FleeBehavior(Vector2 targetPosition)
        {
            TargetPosition = targetPosition;
        }

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            Vector2 desired = agent.Position - TargetPosition;
            float distance = desired.Length();

            if (distance > PanicRadius)
            {
                return Vector2.Zero;
            }

            desired = desired.Normalized() * agent.MaxSpeed;
            return desired - agent.Velocity;
        }
    }

    public class ArriveBehavior : ISteeringBehavior
    {
        public Vector2 TargetPosition { get; set; }
        public float SlowingRadius { get; set; } = 5f;
        public float ArrivalRadius { get; set; } = 1f;

        public ArriveBehavior(Vector2 targetPosition)
        {
            TargetPosition = targetPosition;
        }

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            Vector2 desired = TargetPosition - agent.Position;
            float distance = desired.Length();

            if (distance < ArrivalRadius)
            {
                agent.Velocity = Vector2.Zero;
                return Vector2.Zero;
            }

            float speed;
            if (distance < SlowingRadius)
            {
                speed = agent.MaxSpeed * (distance / SlowingRadius);
            }
            else
            {
                speed = agent.MaxSpeed;
            }

            desired = desired.Normalized() * speed;
            return desired - agent.Velocity;
        }
    }

    public class WanderBehavior : ISteeringBehavior
    {
        public float WanderRadius { get; set; } = 1.5f;
        public float WanderDistance { get; set; } = 2f;
        public float WanderAngleChange { get; set; } = 0.5f;
        private float _wanderAngle;

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            _wanderAngle += (float)(new Random().NextDouble() * 2 - 1) * WanderAngleChange;

            Vector2 circleCenter = agent.ForwardVector * WanderDistance;
            Vector2 displacement = new Vector2(
                MathHelper.Cos(_wanderAngle),
                MathHelper.Sin(_wanderAngle)
            ) * WanderRadius;

            Vector2 wanderForce = circleCenter + displacement;
            return wanderForce;
        }
    }

    public class PursueBehavior : ISteeringBehavior
    {
        public SteeringAgent Target { get; set; }
        public float MaxPredictionTime { get; set; } = 2f;

        public PursueBehavior(SteeringAgent target)
        {
            Target = target;
        }

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            Vector2 direction = Target.Position - agent.Position;
            float distance = direction.Length();
            float speed = agent.Velocity.Length();

            float prediction;
            if (speed <= distance / MaxPredictionTime)
            {
                prediction = MaxPredictionTime;
            }
            else
            {
                prediction = distance / speed;
            }

            Vector2 futurePosition = Target.Position + Target.Velocity * prediction;
            return new SeekBehavior(futurePosition).CalculateSteering(agent);
        }
    }

    public class EvadeBehavior : ISteeringBehavior
    {
        public SteeringAgent Target { get; set; }
        public float MaxPredictionTime { get; set; } = 2f;

        public EvadeBehavior(SteeringAgent target)
        {
            Target = target;
        }

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            Vector2 direction = Target.Position - agent.Position;
            float distance = direction.Length();
            float speed = agent.Velocity.Length();

            float prediction;
            if (speed <= distance / MaxPredictionTime)
            {
                prediction = MaxPredictionTime;
            }
            else
            {
                prediction = distance / speed;
            }

            Vector2 futurePosition = Target.Position + Target.Velocity * prediction;
            return new FleeBehavior(futurePosition).CalculateSteering(agent);
        }
    }

    public class FollowPathBehavior : ISteeringBehavior
    {
        public List<Vector2> Path { get; set; } = new List<Vector2>();
        public int CurrentWaypointIndex { get; set; }
        public float WaypointRadius { get; set; } = 1f;

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            if (Path.Count == 0)
                return Vector2.Zero;

            if (CurrentWaypointIndex >= Path.Count)
                return Vector2.Zero;

            Vector2 target = Path[CurrentWaypointIndex];
            float distance = (target - agent.Position).Length();

            if (distance < WaypointRadius)
            {
                CurrentWaypointIndex++;
                if (CurrentWaypointIndex >= Path.Count)
                    return Vector2.Zero;
            }

            return new SeekBehavior(target).CalculateSteering(agent);
        }

        public void Reset()
        {
            CurrentWaypointIndex = 0;
        }
    }

    public class ObstacleAvoidanceBehavior : ISteeringBehavior
    {
        public float DetectionBoxLength { get; set; } = 5f;
        public float DetectionBoxWidth { get; set; } = 2f;
        public List<Vector2> Obstacles { get; set; } = new List<Vector2>();
        public float AvoidanceStrength { get; set; } = 10f;

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            Vector2 avoidanceForce = Vector2.Zero;
            Vector2 ahead = agent.Position + agent.Velocity.Normalized() * DetectionBoxLength;
            Vector2 ahead2 = agent.Position + agent.Velocity.Normalized() * DetectionBoxLength * 0.5f;

            foreach (var obstacle in Obstacles)
            {
                float distance = (agent.Position - obstacle).Length();
                float obstacleRadius = 1f;

                if (distance < DetectionBoxLength + obstacleRadius)
                {
                    Vector2 avoidance = ahead - obstacle;
                    if (avoidance.Length() < DetectionBoxWidth)
                    {
                        avoidanceForce += avoidance.Normalized() * AvoidanceStrength;
                    }

                    avoidance = ahead2 - obstacle;
                    if (avoidance.Length() < DetectionBoxWidth)
                    {
                        avoidanceForce += avoidance.Normalized() * AvoidanceStrength * 0.5f;
                    }
                }
            }

            return avoidanceForce;
        }
    }

    public class AlignmentBehavior : ISteeringBehavior
    {
        public List<SteeringAgent> Neighbors { get; set; } = new List<SteeringAgent>();

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            if (Neighbors.Count == 0)
                return Vector2.Zero;

            Vector2 averageVelocity = Vector2.Zero;
            foreach (var neighbor in Neighbors)
            {
                averageVelocity += neighbor.Velocity;
            }

            averageVelocity /= Neighbors.Count;
            return averageVelocity - agent.Velocity;
        }
    }

    public class CohesionBehavior : ISteeringBehavior
    {
        public List<SteeringAgent> Neighbors { get; set; } = new List<SteeringAgent>();

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            if (Neighbors.Count == 0)
                return Vector2.Zero;

            Vector2 centerOfMass = Vector2.Zero;
            foreach (var neighbor in Neighbors)
            {
                centerOfMass += neighbor.Position;
            }

            centerOfMass /= Neighbors.Count;
            return new SeekBehavior(centerOfMass).CalculateSteering(agent);
        }
    }

    public class SeparationBehavior : ISteeringBehavior
    {
        public List<SteeringAgent> Neighbors { get; set; } = new List<SteeringAgent>();
        public float SeparationRadius { get; set; } = 2f;

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            Vector2 separationForce = Vector2.Zero;
            int count = 0;

            foreach (var neighbor in Neighbors)
            {
                float distance = (agent.Position - neighbor.Position).Length();
                if (distance < SeparationRadius && distance > 0)
                {
                    Vector2 awayFromNeighbor = (agent.Position - neighbor.Position).Normalized() / distance;
                    separationForce += awayFromNeighbor;
                    count++;
                }
            }

            if (count > 0)
            {
                separationForce /= count;
            }

            return separationForce;
        }
    }

    public class FlockingBehavior : ISteeringBehavior
    {
        public AlignmentBehavior Alignment { get; } = new AlignmentBehavior();
        public CohesionBehavior Cohesion { get; } = new CohesionBehavior();
        public SeparationBehavior Separation { get; } = new SeparationBehavior();

        public float AlignmentWeight { get; set; } = 1f;
        public float CohesionWeight { get; set; } = 1f;
        public float SeparationWeight { get; set; } = 1.5f;

        public Vector2 CalculateSteering(SteeringAgent agent)
        {
            Vector2 alignment = Alignment.CalculateSteering(agent) * AlignmentWeight;
            Vector2 cohesion = Cohesion.CalculateSteering(agent) * CohesionWeight;
            Vector2 separation = Separation.CalculateSteering(agent) * SeparationWeight;

            return alignment + cohesion + separation;
        }
    }
}

namespace WPFEngine.AI.StateMachine
{
    public interface IState
    {
        void OnEnter();
        void OnUpdate(float deltaTime);
        void OnExit();
        void OnHandleInput();
    }

    public class StateMachine<T> where T : class
    {
        private readonly Dictionary<Type, IState> _states = new Dictionary<Type, IState>();
        private IState? _currentState;
        private IState? _previousState;
        private T? _context;

        public IState? CurrentState => _currentState;
        public IState? PreviousState => _previousState;
        public T? Context => _context;

        public event EventHandler<IState>? StateChanged;

        public void SetContext(T context)
        {
            _context = context;
        }

        public void AddState<S>(S state) where S : class, IState
        {
            _states[typeof(S)] = state;
        }

        public void SetState<S>() where S : class, IState
        {
            if (!_states.TryGetValue(typeof(S), out var state))
                return;

            if (_currentState == state)
                return;

            _previousState = _currentState;
            _currentState?.OnExit();
            _currentState = state;
            _currentState.OnEnter();
            StateChanged?.Invoke(this, _currentState);
        }

        public void Update(float deltaTime)
        {
            _currentState?.OnUpdate(deltaTime);
        }

        public void HandleInput()
        {
            _currentState?.OnHandleInput();
        }

        public void RevertToPreviousState()
        {
            if (_previousState != null)
            {
                var temp = _currentState;
                _currentState = _previousState;
                _previousState = temp;
                _currentState?.OnEnter();
                StateChanged?.Invoke(this, _currentState);
            }
        }

        public bool IsInState<S>() where S : class, IState
        {
            return _currentState is S;
        }
    }

    public abstract class StateBase : IState
    {
        protected StateMachine<object>? _stateMachine;

        public virtual void SetStateMachine(StateMachine<object> stateMachine)
        {
            _stateMachine = stateMachine;
        }

        public virtual void OnEnter() { }
        public virtual void OnUpdate(float deltaTime) { }
        public virtual void OnExit() { }
        public virtual void OnHandleInput() { }

        protected void ChangeState<T>() where T : class, IState
        {
            _stateMachine?.SetState<T>();
        }
    }
}

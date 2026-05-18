using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using WPFEngine.Core;
using WPFEngine.Math;

namespace WPFEngine.Demos
{
    public enum RTSGameState
    {
        Menu,
        Playing,
        Paused,
        GameOver,
        Victory
    }

    public enum UnitType
    {
        Worker,
        Infantry,
        Archer,
        Knight,
        Mage,
        Siege
    }

    public enum UnitState
    {
        Idle,
        Moving,
        Attacking,
        Gathering,
        Building,
        Patrolling,
        HoldingPosition
    }

    public enum BuildingType
    {
        TownCenter,
        Barracks,
        Farm,
        Mine,
        LumberMill,
        Tower,
        Castle
    }

    public enum ResourceType
    {
        Gold,
        Wood,
        Food
    }

    public enum UnitCommand
    {
        Move,
        Attack,
        Gather,
        Build,
        Patrol,
        HoldPosition
    }

    public class UnitStats
    {
        public float Health;
        public float Attack;
        public float Defense;
        public float Speed;
        public float AttackRange;
        public float AttackSpeed;
        public float GatherRate;
        public int Cost;
        public int PopulationCost;
        public string Name;
    }

    public class BuildingStats
    {
        public float Health;
        public float Defense;
        public float BuildTime;
        public int GoldCost;
        public int WoodCost;
        public int FoodCost;
        public bool ProducesUnits;
        public List<UnitType> CanProduce;
    }

    public class Unit
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public UnitType Type { get; set; }
        public UnitState State { get; set; }
        public Vector2 Position { get; set; }
        public Vector2 TargetPosition { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Attack { get; set; }
        public float Defense { get; set; }
        public float Speed { get; set; }
        public float AttackRange { get; set; }
        public float AttackCooldown { get; set; }
        public float GatherRate { get; set; }
        public int Cost { get; set; }
        public int PopulationCost { get; set; }
        public float Size { get; set; }
        public bool IsSelected { get; set; }
        public bool IsOwnedByPlayer { get; set; }
        public Unit? AttackTarget { get; set; }
        public Building? GatherTarget { get; set; }
        public Building? BuildTarget { get; set; }
        public Vector2 PatrolPoint { get; set; }
        public Vector2 RallyPoint { get; set; }
        public float AttackAngle { get; set; }
        public bool IsCarryingResource { get; set; }
        public ResourceType CarriedResource { get; set; }
        public float CarriedAmount { get; set; }
        public float AnimationTimer { get; set; }
        public UnitStats BaseStats { get; set; } = new UnitStats();
        public event EventHandler? UnitDied;
        public event EventHandler? ResourceGathered;

        public bool IsDead => Health <= 0;
        public bool CanAttack => Type != UnitType.Worker;
        public bool CanGather => Type == UnitType.Worker;

        public Unit(UnitType type, Vector2 position, bool ownedByPlayer)
        {
            Type = type;
            Position = position;
            IsOwnedByPlayer = ownedByPlayer;
            State = UnitState.Idle;
            IsSelected = false;
            IsCarryingResource = false;
            CarriedAmount = 0;
            RallyPoint = position;
            AttackAngle = 0;

            var stats = GetStatsForType(type);
            Health = stats.Health;
            MaxHealth = stats.Health;
            Attack = stats.Attack;
            Defense = stats.Defense;
            Speed = stats.Speed;
            AttackRange = stats.AttackRange;
            GatherRate = stats.GatherRate;
            Cost = stats.Cost;
            PopulationCost = stats.PopulationCost;
            BaseStats = stats;
            Size = GetSizeForType(type);
            Name = stats.Name;
        }

        private static UnitStats GetStatsForType(UnitType type)
        {
            return type switch
            {
                UnitType.Worker => new UnitStats { Health = 40, Attack = 5, Defense = 1, Speed = 80, AttackRange = 10, AttackSpeed = 1f, GatherRate = 10, Cost = 50, PopulationCost = 1, Name = "Worker" },
                UnitType.Infantry => new UnitStats { Health = 60, Attack = 12, Defense = 3, Speed = 60, AttackRange = 25, AttackSpeed = 1f, GatherRate = 0, Cost = 60, PopulationCost = 1, Name = "Infantry" },
                UnitType.Archer => new UnitStats { Health = 40, Attack = 15, Defense = 1, Speed = 70, AttackRange = 150, AttackSpeed = 0.8f, GatherRate = 0, Cost = 80, PopulationCost = 1, Name = "Archer" },
                UnitType.Knight => new UnitStats { Health = 120, Attack = 25, Defense = 8, Speed = 50, AttackRange = 30, AttackSpeed = 1.2f, GatherRate = 0, Cost = 150, PopulationCost = 2, Name = "Knight" },
                UnitType.Mage => new UnitStats { Health = 35, Attack = 30, Defense = 1, Speed = 55, AttackRange = 120, AttackSpeed = 0.5f, GatherRate = 0, Cost = 200, PopulationCost = 2, Name = "Mage" },
                UnitType.Siege => new UnitStats { Health = 80, Attack = 50, Defense = 5, Speed = 30, AttackRange = 100, AttackSpeed = 0.3f, GatherRate = 0, Cost = 250, PopulationCost = 3, Name = "Siege Engine" },
                _ => new UnitStats { Health = 50, Attack = 10, Defense = 2, Speed = 50, AttackRange = 30, AttackSpeed = 1f, GatherRate = 0, Cost = 100, PopulationCost = 1, Name = "Unit" }
            };
        }

        private static float GetSizeForType(UnitType type)
        {
            return type switch
            {
                UnitType.Worker => 12,
                UnitType.Infantry => 14,
                UnitType.Archer => 12,
                UnitType.Knight => 18,
                UnitType.Mage => 12,
                UnitType.Siege => 22,
                _ => 14
            };
        }

        public void Update(float deltaTime, List<Unit> allUnits, List<Building> allBuildings)
        {
            if (IsDead) return;

            AnimationTimer += deltaTime;
            AttackCooldown = Math.Max(0, AttackCooldown - deltaTime);

            switch (State)
            {
                case UnitState.Idle:
                    UpdateIdle(deltaTime, allUnits);
                    break;
                case UnitState.Moving:
                    UpdateMoving(deltaTime);
                    break;
                case UnitState.Attacking:
                    UpdateAttacking(deltaTime, allUnits);
                    break;
                case UnitState.Gathering:
                    UpdateGathering(deltaTime, allBuildings);
                    break;
                case UnitState.Building:
                    UpdateBuilding(deltaTime);
                    break;
                case UnitState.Patrolling:
                    UpdatePatrolling(deltaTime);
                    break;
                case UnitState.HoldingPosition:
                    UpdateHoldingPosition(deltaTime, allUnits);
                    break;
            }
        }

        private void UpdateIdle(float deltaTime, List<Unit> allUnits)
        {
            if (CanAttack && AttackTarget != null && !AttackTarget.IsDead)
            {
                float dist = (Position - AttackTarget.Position).Length();
                if (dist <= AttackRange * 1.5f)
                {
                    State = UnitState.Attacking;
                }
            }
        }

        private void UpdateMoving(float deltaTime)
        {
            Vector2 direction = (TargetPosition - Position).Normalized();
            float distance = (TargetPosition - Position).Length();

            if (distance < 5)
            {
                State = UnitState.Idle;
                Position = TargetPosition;
                return;
            }

            Position = Position + direction * Speed * deltaTime;
            AttackAngle = (float)Math.Atan2(direction.Y, direction.X);
        }

        private void UpdateAttacking(float deltaTime, List<Unit> allUnits)
        {
            if (AttackTarget == null || AttackTarget.IsDead)
            {
                State = UnitState.Idle;
                AttackTarget = null;
                return;
            }

            float dist = (Position - AttackTarget.Position).Length();

            if (dist > AttackRange * 2)
            {
                TargetPosition = AttackTarget.Position;
                State = UnitState.Moving;
                return;
            }

            if (dist <= AttackRange)
            {
                Vector2 dir = (AttackTarget.Position - Position).Normalized();
                AttackAngle = (float)Math.Atan2(dir.Y, dir.X);

                if (AttackCooldown <= 0)
                {
                    float damage = Math.Max(1, Attack - AttackTarget.Defense * 0.5f);
                    AttackTarget.Health -= damage;
                    AttackCooldown = 1f / Attack.AttackSpeed;

                    if (AttackTarget.IsDead)
                    {
                        AttackTarget.UnitDied?.Invoke(this, EventArgs.Empty);
                        State = UnitState.Idle;
                        AttackTarget = null;
                    }
                }
            }
        }

        private void UpdateGathering(float deltaTime, List<Building> allBuildings)
        {
            if (GatherTarget == null || GatherTarget.IsDead || GatherTarget.ResourcesRemaining <= 0)
            {
                if (IsCarryingResource)
                {
                    ReturnResources(allBuildings);
                }
                else
                {
                    State = UnitState.Idle;
                    GatherTarget = null;
                }
                return;
            }

            float dist = (Position - GatherTarget.Position).Length();

            if (dist > 50)
            {
                TargetPosition = GatherTarget.Position;
                State = UnitState.Moving;
                return;
            }

            CarriedAmount += GatherRate * deltaTime;
            GatherTarget.ResourcesRemaining -= GatherRate * deltaTime;

            if (CarriedAmount >= 10)
            {
                ReturnResources(allBuildings);
            }
        }

        private void ReturnResources(List<Building> allBuildings)
        {
            Building? townCenter = allBuildings.FirstOrDefault(b => b.Type == BuildingType.TownCenter && b.IsOwnedByPlayer);
            if (townCenter != null)
            {
                TargetPosition = townCenter.Position;
                State = UnitState.Moving;
            }
            else
            {
                IsCarryingResource = false;
                CarriedAmount = 0;
                State = UnitState.Idle;
            }
        }

        private void UpdateBuilding(float deltaTime)
        {
            if (BuildTarget == null || BuildTarget.ConstructionProgress >= 1f)
            {
                State = UnitState.Idle;
                BuildTarget = null;
                return;
            }

            float dist = (Position - BuildTarget.Position).Length();
            if (dist > 60)
            {
                TargetPosition = BuildTarget.Position;
                State = UnitState.Moving;
                return;
            }

            BuildTarget.ConstructionProgress += deltaTime * 0.5f;
        }

        private void UpdatePatrolling(float deltaTime)
        {
            Vector2 direction = (PatrolPoint - Position).Normalized();
            float distance = (Position - PatrolPoint).Length();

            if (distance < 10)
            {
                PatrolPoint = RallyPoint + (RallyPoint - PatrolPoint);
            }

            Position = Position + direction * Speed * 0.7f * deltaTime;
            AttackAngle = (float)Math.Atan2(direction.Y, direction.X);
        }

        private void UpdateHoldingPosition(float deltaTime, List<Unit> allUnits)
        {
            var enemies = allUnits.Where(u => u.IsOwnedByPlayer != IsOwnedByPlayer && !u.IsDead).ToList();
            Unit? nearest = null;
            float nearestDist = float.MaxValue;

            foreach (var enemy in enemies)
            {
                float dist = (Position - enemy.Position).Length();
                if (dist < nearestDist && dist <= AttackRange * 2)
                {
                    nearestDist = dist;
                    nearest = enemy;
                }
            }

            if (nearest != null && AttackCooldown <= 0)
            {
                AttackTarget = nearest;
                State = UnitState.Attacking;
            }
        }

        public void MoveTo(Vector2 position)
        {
            TargetPosition = position;
            State = UnitState.Moving;
        }

        public void AttackUnit(Unit target)
        {
            AttackTarget = target;
            State = UnitState.Attacking;
        }

        public void GatherFrom(Building building)
        {
            GatherTarget = building;
            State = UnitState.Gathering;
        }

        public void Build(Building building)
        {
            BuildTarget = building;
            State = UnitState.Building;
        }

        public void Patrol(Vector2 point)
        {
            PatrolPoint = point;
            RallyPoint = Position;
            State = UnitState.Patrolling;
        }

        public void HoldPosition()
        {
            State = UnitState.HoldingPosition;
        }

        public void TakeDamage(float damage)
        {
            Health -= Math.Max(1, damage - Defense * 0.3f);
            if (IsDead)
            {
                UnitDied?.Invoke(this, EventArgs.Empty);
            }
        }

        public Color GetUnitColor()
        {
            return Type switch
            {
                UnitType.Worker => IsOwnedByPlayer ? new Color(0.9f, 0.7f, 0.3f) : new Color(0.7f, 0.5f, 0.2f),
                UnitType.Infantry => IsOwnedByPlayer ? new Color(0.3f, 0.5f, 0.9f) : new Color(0.5f, 0.3f, 0.7f),
                UnitType.Archer => IsOwnedByPlayer ? new Color(0.3f, 0.8f, 0.4f) : new Color(0.4f, 0.6f, 0.3f),
                UnitType.Knight => IsOwnedByPlayer ? new Color(0.8f, 0.8f, 0.9f) : new Color(0.6f, 0.6f, 0.7f),
                UnitType.Mage => IsOwnedByPlayer ? new Color(0.7f, 0.4f, 0.9f) : new Color(0.5f, 0.3f, 0.7f),
                UnitType.Siege => IsOwnedByPlayer ? new Color(0.6f, 0.5f, 0.3f) : new Color(0.4f, 0.35f, 0.2f),
                _ => IsOwnedByPlayer ? Color.Blue : Color.Red
            };
        }
    }

    public class Building
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public BuildingType Type { get; set; }
        public Vector2 Position { get; set; }
        public float Health { get; set; }
        public float MaxHealth { get; set; }
        public float Defense { get; set; }
        public float ConstructionProgress { get; set; }
        public float Size { get; set; }
        public bool IsSelected { get; set; }
        public bool IsOwnedByPlayer { get; set; }
        public bool IsConstructed => ConstructionProgress >= 1f;
        public float ResourcesRemaining { get; set; }
        public ResourceType ResourceType { get; set; }
        public List<UnitType> CanProduce { get; set; }
        public float TrainProgress { get; set; }
        public UnitType? TrainingUnit { get; set; }
        public Queue<UnitType> TrainingQueue { get; set; } = new Queue<UnitType>();
        public BuildingStats BaseStats { get; set; }
        public event EventHandler? BuildingDestroyed;
        public event EventHandler? UnitTrained;

        public bool IsDead => Health <= 0;

        public Building(BuildingType type, Vector2 position, bool ownedByPlayer)
        {
            Type = type;
            Position = position;
            IsOwnedByPlayer = ownedByPlayer;
            ConstructionProgress = type == BuildingType.TownCenter ? 1f : 0f;
            IsSelected = false;

            var stats = GetStatsForType(type);
            Health = stats.Health;
            MaxHealth = stats.Health;
            Defense = stats.Defense;
            BaseStats = stats;
            Size = GetSizeForType(type);
            Name = GetNameForType(type);
            CanProduce = stats.CanProduce;

            if (type == BuildingType.Mine)
            {
                ResourcesRemaining = 1000;
                ResourceType = ResourceType.Gold;
            }
            else if (type == BuildingType.Farm)
            {
                ResourcesRemaining = 500;
                ResourceType = ResourceType.Food;
            }
            else if (type == BuildingType.LumberMill)
            {
                ResourcesRemaining = 800;
                ResourceType = ResourceType.Wood;
            }
        }

        private static BuildingStats GetStatsForType(BuildingType type)
        {
            return type switch
            {
                BuildingType.TownCenter => new BuildingStats { Health = 500, Defense = 5, BuildTime = 0, GoldCost = 0, WoodCost = 0, FoodCost = 0, ProducesUnits = true, CanProduce = new List<UnitType> { UnitType.Worker } },
                BuildingType.Barracks => new BuildingStats { Health = 300, Defense = 4, BuildTime = 60, GoldCost = 150, WoodCost = 100, FoodCost = 0, ProducesUnits = true, CanProduce = new List<UnitType> { UnitType.Infantry, UnitType.Archer } },
                BuildingType.Farm => new BuildingStats { Health = 100, Defense = 1, BuildTime = 30, GoldCost = 50, WoodCost = 30, FoodCost = 0, ProducesUnits = false, CanProduce = new List<UnitType>() },
                BuildingType.Mine => new BuildingStats { Health = 150, Defense = 2, BuildTime = 40, GoldCost = 80, WoodCost = 50, FoodCost = 0, ProducesUnits = false, CanProduce = new List<UnitType>() },
                BuildingType.LumberMill => new BuildingStats { Health = 120, Defense = 2, BuildTime = 35, GoldCost = 70, WoodCost = 0, FoodCost = 0, ProducesUnits = false, CanProduce = new List<UnitType>() },
                BuildingType.Tower => new BuildingStats { Health = 200, Defense = 6, BuildTime = 50, GoldCost = 100, WoodCost = 80, FoodCost = 0, ProducesUnits = false, CanProduce = new List<UnitType>() },
                BuildingType.Castle => new BuildingStats { Health = 800, Defense = 10, BuildTime = 120, GoldCost = 500, WoodCost = 300, FoodCost = 100, ProducesUnits = true, CanProduce = new List<UnitType> { UnitType.Knight, UnitType.Mage, UnitType.Siege } },
                _ => new BuildingStats { Health = 200, Defense = 3, BuildTime = 60, GoldCost = 100, WoodCost = 100, FoodCost = 0, ProducesUnits = false, CanProduce = new List<UnitType>() }
            };
        }

        private static float GetSizeForType(BuildingType type)
        {
            return type switch
            {
                BuildingType.TownCenter => 60,
                BuildingType.Barracks => 50,
                BuildingType.Farm => 40,
                BuildingType.Mine => 35,
                BuildingType.LumberMill => 45,
                BuildingType.Tower => 30,
                BuildingType.Castle => 70,
                _ => 45
            };
        }

        private static string GetNameForType(BuildingType type)
        {
            return type switch
            {
                BuildingType.TownCenter => "Town Center",
                BuildingType.Barracks => "Barracks",
                BuildingType.Farm => "Farm",
                BuildingType.Mine => "Gold Mine",
                BuildingType.LumberMill => "Lumber Mill",
                BuildingType.Tower => "Guard Tower",
                BuildingType.Castle => "Castle",
                _ => "Building"
            };
        }

        public void Update(float deltaTime, List<Unit> allUnits)
        {
            if (IsDead) return;

            if (TrainingQueue.Count > 0 && IsConstructed)
            {
                TrainProgress += deltaTime / 30f;
                if (TrainProgress >= 1f)
                {
                    UnitTrained?.Invoke(this, EventArgs.Empty);
                    TrainProgress = 0;
                    TrainingQueue.Dequeue();
                    if (TrainingQueue.Count > 0)
                    {
                        TrainingUnit = TrainingQueue.Peek();
                    }
                    else
                    {
                        TrainingUnit = null;
                    }
                }
            }
        }

        public bool CanAffordTraining(int gold, int wood, int food)
        {
            return gold >= 0 && wood >= 0 && food >= 0;
        }

        public void StartTraining(UnitType unitType)
        {
            if (!CanProduce.Contains(unitType) || !IsConstructed) return;

            if (TrainingQueue.Count < 5)
            {
                TrainingQueue.Enqueue(unitType);
                if (TrainingQueue.Count == 1)
                {
                    TrainingUnit = unitType;
                }
            }
        }

        public Unit? CompleteTraining(Vector2 spawnPosition)
        {
            if (TrainingQueue.Count == 0 || TrainProgress < 1f) return null;

            var unitType = TrainingQueue.Dequeue();
            var unit = new Unit(unitType, spawnPosition, IsOwnedByPlayer);

            TrainProgress = 0;
            if (TrainingQueue.Count > 0)
            {
                TrainingUnit = TrainingQueue.Peek();
            }
            else
            {
                TrainingUnit = null;
            }

            return unit;
        }

        public void TakeDamage(float damage)
        {
            Health -= Math.Max(1, damage - Defense * 0.5f);
            if (IsDead)
            {
                BuildingDestroyed?.Invoke(this, EventArgs.Empty);
            }
        }

        public Color GetBuildingColor()
        {
            if (!IsConstructed)
                return new Color(0.4f, 0.35f, 0.3f);

            return Type switch
            {
                BuildingType.TownCenter => IsOwnedByPlayer ? new Color(0.8f, 0.6f, 0.3f) : new Color(0.6f, 0.4f, 0.2f),
                BuildingType.Barracks => IsOwnedByPlayer ? new Color(0.5f, 0.5f, 0.6f) : new Color(0.4f, 0.4f, 0.5f),
                BuildingType.Farm => new Color(0.4f, 0.7f, 0.3f),
                BuildingType.Mine => new Color(1f, 0.8f, 0.2f),
                BuildingType.LumberMill => new Color(0.6f, 0.4f, 0.2f),
                BuildingType.Tower => IsOwnedByPlayer ? new Color(0.6f, 0.6f, 0.7f) : new Color(0.5f, 0.5f, 0.6f),
                BuildingType.Castle => IsOwnedByPlayer ? new Color(0.7f, 0.7f, 0.8f) : new Color(0.55f, 0.55f, 0.65f),
                _ => Color.Gray
            };
        }
    }

    public class RTSCamera
    {
        public Vector2 Position { get; set; }
        public float Zoom { get; set; } = 1f;
        public float Rotation { get; set; } = 0f;
        public float PanSpeed { get; set; } = 500f;
        public float ZoomSpeed { get; set; } = 0.2f;
        public float MinZoom { get; set; } = 0.5f;
        public float MaxZoom { get; set; } = 2f;
        public Rect Bounds { get; set; } = new Rect(-500, -500, 3000, 3000);
        public bool IsDragging { get; set; }
        public Vector2 DragStart { get; set; }
        public Vector2 CameraStart { get; set; }

        public RTSCamera()
        {
            Position = new Vector2(500, 500);
        }

        public void Update(float deltaTime, Vector2 mousePos, Vector2 screenSize, bool mouseDown, bool[] keys)
        {
            if (IsDragging && mouseDown)
            {
                Vector2 delta = mousePos - DragStart;
                Position = CameraStart - delta / Zoom;
            }

            if (keys[0]) Position.Y -= PanSpeed * deltaTime;
            if (keys[1]) Position.Y += PanSpeed * deltaTime;
            if (keys[2]) Position.X -= PanSpeed * deltaTime;
            if (keys[3]) Position.X += PanSpeed * deltaTime;

            float edgeSize = 30f;
            if (mousePos.X < edgeSize) Position.X -= PanSpeed * deltaTime * (1 - mousePos.X / edgeSize);
            if (mousePos.X > screenSize.X - edgeSize) Position.X += PanSpeed * deltaTime * (1 - (screenSize.X - mousePos.X) / edgeSize);
            if (mousePos.Y < edgeSize) Position.Y -= PanSpeed * deltaTime * (1 - mousePos.Y / edgeSize);
            if (mousePos.Y > screenSize.Y - edgeSize) Position.Y += PanSpeed * deltaTime * (1 - (screenSize.Y - mousePos.Y) / edgeSize);

            Position.X = Math.Clamp(Position.X, Bounds.X, Bounds.X + Bounds.Width);
            Position.Y = Math.Clamp(Position.Y, Bounds.Y, Bounds.Y + Bounds.Height);
        }

        public void StartDrag(Vector2 mousePos)
        {
            IsDragging = true;
            DragStart = mousePos;
            CameraStart = Position;
        }

        public void EndDrag()
        {
            IsDragging = false;
        }

        public void ZoomAt(float delta, Vector2 point)
        {
            float oldZoom = Zoom;
            Zoom = Math.Clamp(Zoom + delta * ZoomSpeed, MinZoom, MaxZoom);

            if (Math.Abs(Zoom - oldZoom) > 0.001f)
            {
                Vector2 worldBefore = ScreenToWorld(point);
                Zoom = Math.Clamp(Zoom + delta * ZoomSpeed, MinZoom, MaxZoom);
                Vector2 worldAfter = ScreenToWorld(point);
                Position = Position + (worldBefore - worldAfter);
            }
        }

        public Vector2 WorldToScreen(Vector2 worldPos)
        {
            return (worldPos - Position) * Zoom + new Vector2(960, 540);
        }

        public Vector2 ScreenToWorld(Vector2 screenPos)
        {
            return (screenPos - new Vector2(960, 540)) / Zoom + Position;
        }
    }

    public class TechNode
    {
        public string Name { get; set; } = "";
        public int GoldCost { get; set; }
        public int WoodCost { get; set; }
        public float ResearchTime { get; set; }
        public List<string> Requires { get; set; } = new List<string>();
        public bool IsResearched { get; set; }
        public bool IsResearching { get; set; }
        public float ResearchProgress { get; set; }
    }

    public class RTSGame : GameBase
    {
        private const float MAP_WIDTH = 2000f;
        private const float MAP_HEIGHT = 1500f;
        private const int STARTING_GOLD = 500;
        private const int STARTING_WOOD = 300;
        private const int STARTING_FOOD = 200;
        private const int MAX_POPULATION = 100;

        private RTSGameState _gameState;
        private RTSGameState _previousState;

        private int _gold;
        private int _wood;
        private int _food;
        private int _currentPopulation;
        private int _maxPopulation;
        private int _score;
        private float _gameTime;
        private float _resourceTimer;

        private RTSCamera _camera;
        private List<Unit> _playerUnits;
        private List<Unit> _enemyUnits;
        private List<Building> _playerBuildings;
        private List<Building> _enemyBuildings;
        private List<Building> _resourceNodes;
        private List<VisualEffect> _effects;
        private Dictionary<string, TechNode> _techTree;
        private string? _selectedTech;

        private List<Unit> _selectedUnits;
        private Building? _selectedBuilding;
        private Vector2 _selectionStart;
        private Vector2 _selectionEnd;
        private bool _isSelecting;
        private bool _isDraggingCamera;

        private BuildingType? _placementMode;
        private Vector2 _placementPreview;
        private bool _canPlaceBuilding;

        private Vector2 _mousePos;
        private bool[] _movementKeys;
        private bool _leftMouseDown;
        private bool _rightMouseDown;
        private Vector2 _lastRightClickPos;

        private float _aiTimer;
        private int _aiDifficulty;
        private Random _random;

        private Vector2 _mapOffset;
        private float _minimapScale;
        private Rect _minimapRect;

        public RTSGameState GameState => _gameState;
        public int Gold => _gold;
        public int Wood => _wood;
        public int Food => _food;
        public int CurrentPopulation => _currentPopulation;
        public int MaxPopulation => _maxPopulation;
        public RTSCamera Camera => _camera;
        public IReadOnlyList<Unit> PlayerUnits => _playerUnits;
        public IReadOnlyList<Unit> EnemyUnits => _enemyUnits;
        public IReadOnlyList<Building> PlayerBuildings => _playerBuildings;
        public IReadOnlyList<Building> ResourceNodes => _resourceNodes;
        public IReadOnlyList<Unit> SelectedUnits => _selectedUnits;
        public Building? SelectedBuilding => _selectedBuilding;

        public event EventHandler? ResourcesChanged;
        public event EventHandler? PopulationChanged;
        public event EventHandler? SelectionChanged;
        public event EventHandler? BuildingPlaced;
        public event EventHandler? UnitCreated;
        public event EventHandler? TechResearched;

        public RTSGame()
        {
            _gameState = RTSGameState.Menu;
            _previousState = RTSGameState.Menu;
            _gold = STARTING_GOLD;
            _wood = STARTING_WOOD;
            _food = STARTING_FOOD;
            _currentPopulation = 0;
            _maxPopulation = MAX_POPULATION;
            _score = 0;
            _gameTime = 0;
            _resourceTimer = 0;

            _camera = new RTSCamera();
            _playerUnits = new List<Unit>();
            _enemyUnits = new List<Unit>();
            _playerBuildings = new List<Building>();
            _enemyBuildings = new List<Building>();
            _resourceNodes = new List<Building>();
            _effects = new List<VisualEffect>();
            _techTree = new Dictionary<string, TechNode>();

            _selectedUnits = new List<Unit>();
            _selectedBuilding = null;
            _isSelecting = false;
            _isDraggingCamera = false;

            _placementMode = null;
            _placementPreview = Vector2.Zero;

            _movementKeys = new bool[4];
            _aiTimer = 0;
            _aiDifficulty = 1;
            _random = new Random();

            _mapOffset = new Vector2(50, 50);
            _minimapScale = 0.1f;
            _minimapRect = new Rect(20, 20, 200, 150);

            InitializeTechTree();
        }

        private void InitializeTechTree()
        {
            _techTree["BetterWeapons"] = new TechNode { Name = "Better Weapons", GoldCost = 100, WoodCost = 50, ResearchTime = 60 };
            _techTree["BetterArmor"] = new TechNode { Name = "Better Armor", GoldCost = 100, WoodCost = 50, ResearchTime = 60 };
            _techTree["HorseBreeding"] = new TechNode { Name = "Horse Breeding", GoldCost = 150, WoodCost = 80, ResearchTime = 90, Requires = new List<string> { "BetterWeapons" } };
            _techTree["Ballistics"] = new TechNode { Name = "Ballistics", GoldCost = 150, WoodCost = 100, ResearchTime = 90, Requires = new List<string> { "BetterWeapons" } };
            _techTree["Wizardry"] = new TechNode { Name = "Wizardry", GoldCost = 200, WoodCost = 100, ResearchTime = 120, Requires = new List<string> { "HorseBreeding" } };
            _techTree["SiegeEngineering"] = new TechNode { Name = "Siege Engineering", GoldCost = 250, WoodCost = 150, ResearchTime = 150, Requires = new List<string> { "Ballistics" } };
            _techTree["CastleUpgrade"] = new TechNode { Name = "Castle Upgrade", GoldCost = 400, WoodCost = 300, ResearchTime = 180, Requires = new List<string> { "Wizardry", "SiegeEngineering" } };
        }

        protected override void Initialize()
        {
            base.Initialize();
            GenerateMap();
        }

        private void GenerateMap()
        {
            var townCenter = new Building(BuildingType.TownCenter, new Vector2(300, 400), true);
            _playerBuildings.Add(townCenter);

            for (int i = 0; i < 3; i++)
            {
                var worker = new Unit(UnitType.Worker, new Vector2(280 + i * 40, 480), true);
                _playerUnits.Add(worker);
                _currentPopulation += worker.PopulationCost;
            }

            var enemyTownCenter = new Building(BuildingType.TownCenter, new Vector2(1700, 1000), false);
            _enemyBuildings.Add(enemyTownCenter);

            for (int i = 0; i < 2; i++)
            {
                var enemyWorker = new Unit(UnitType.Worker, new Vector2(1680 + i * 40, 1080), false);
                _enemyUnits.Add(enemyWorker);
            }

            int[] goldPositions = { 500, 700, 1200, 1400 };
            foreach (var x in goldPositions)
            {
                var mine = new Building(BuildingType.Mine, new Vector2(x, 300 + _random.Next(100, 400)), true);
                _resourceNodes.Add(mine);
            }

            int[] enemyGoldPositions = { 100, 1600 };
            foreach (var x in enemyGoldPositions)
            {
                var enemyMine = new Building(BuildingType.Mine, new Vector2(x, 900 + _random.Next(100, 200)), false);
                _resourceNodes.Add(enemyMine);
            }

            int[] woodPositions = { 200, 400, 600, 1500, 1700 };
            foreach (var x in woodPositions)
            {
                var lumberMill = new Building(BuildingType.LumberMill, new Vector2(x, 200 + _random.Next(50, 150)), true);
                _resourceNodes.Add(lumberMill);
            }

            int[] farmPositions = { 250, 350, 450, 550, 650, 750, 850 };
            foreach (var x in farmPositions)
            {
                var farm = new Building(BuildingType.Farm, new Vector2(x, 700 + _random.Next(50, 100)), true);
                _resourceNodes.Add(farm);
            }

            _camera.Position = new Vector2(400, 500);
        }

        protected override void OnUpdate(float deltaTime)
        {
            base.OnUpdate(deltaTime);

            if (_gameState != RTSGameState.Playing) return;

            _gameTime += deltaTime;
            _resourceTimer += deltaTime;

            UpdateInput(deltaTime);
            _camera.Update(deltaTime, _mousePos, new Vector2(1920, 1080), _isDraggingCamera, _movementKeys);

            UpdateUnits(deltaTime);
            UpdateBuildings(deltaTime);
            UpdateEffects(deltaTime);
            UpdateResourceCollection(deltaTime);
            UpdateAI(deltaTime);
            UpdateTechResearch(deltaTime);

            if (_resourceTimer >= 5f)
            {
                _resourceTimer = 0;
                UpdateResourceGeneration();
            }

            CheckWinCondition();
        }

        private void UpdateInput(float deltaTime)
        {
            _movementKeys[0] = InputManager.Instance.GetKey(InputManager.KeyCode.W);
            _movementKeys[1] = InputManager.Instance.GetKey(InputManager.KeyCode.S);
            _movementKeys[2] = InputManager.Instance.GetKey(InputManager.KeyCode.A);
            _movementKeys[3] = InputManager.Instance.GetKey(InputManager.KeyCode.D);

            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Escape))
            {
                if (_placementMode.HasValue)
                {
                    _placementMode = null;
                }
                else if (_gameState == RTSGameState.Playing)
                {
                    SetGameState(RTSGameState.Paused);
                }
                else if (_gameState == RTSGameState.Paused)
                {
                    SetGameState(RTSGameState.Playing);
                }
            }

            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.Enter) && _gameState == RTSGameState.Menu)
            {
                StartGame();
            }

            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.R) && _gameState == RTSGameState.GameOver)
            {
                RestartGame();
            }

            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.F1))
                _placementMode = BuildingType.Barracks;
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.F2))
                _placementMode = BuildingType.Farm;
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.F3))
                _placementMode = BuildingType.Tower;
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.F4))
                _placementMode = BuildingType.Mine;
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.F5))
                _placementMode = BuildingType.LumberMill;

            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D1))
                CreateUnit(UnitType.Infantry);
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D2))
                CreateUnit(UnitType.Archer);
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D3))
                CreateUnit(UnitType.Knight);
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D4))
                CreateUnit(UnitType.Mage);
            if (InputManager.Instance.GetKeyDown(InputManager.KeyCode.D5))
                CreateUnit(UnitType.Siege);

            if (InputManager.Instance.GetMouseButtonDown(0))
            {
                HandleLeftClick();
            }

            if (InputManager.Instance.GetMouseButtonUp(0))
            {
                if (_isSelecting)
                {
                    FinalizeSelection();
                }
                _isSelecting = false;
                _isDraggingCamera = false;
            }

            if (InputManager.Instance.GetMouseButtonDown(1))
            {
                HandleRightClick();
            }

            float scrollDelta = InputManager.Instance.MouseScrollDelta;
            if (Math.Abs(scrollDelta) > 0.1f)
            {
                _camera.ZoomAt(scrollDelta > 0 ? 1 : -1, _mousePos);
            }
        }

        private void HandleLeftClick()
        {
            Vector2 worldPos = _camera.ScreenToWorld(_mousePos);

            if (_placementMode.HasValue)
            {
                TryPlaceBuilding(_placementMode.Value, worldPos);
                return;
            }

            var clickedBuilding = GetBuildingAt(worldPos);
            if (clickedBuilding != null && clickedBuilding.IsOwnedByPlayer)
            {
                _selectedBuilding = clickedBuilding;
                clickedBuilding.IsSelected = true;
                ClearUnitSelection();
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            var clickedUnit = GetUnitAt(worldPos, _playerUnits);
            if (clickedUnit != null)
            {
                if (InputManager.Instance.GetKey(InputManager.KeyCode.ShiftLeft))
                {
                    AddToSelection(clickedUnit);
                }
                else
                {
                    ClearSelection();
                    AddToSelection(clickedUnit);
                }
                _selectedBuilding = null;
                SelectionChanged?.Invoke(this, EventArgs.Empty);
                return;
            }

            _isSelecting = true;
            _selectionStart = worldPos;
            _selectionEnd = worldPos;

            if (!InputManager.Instance.GetKey(InputManager.KeyCode.ShiftLeft))
            {
                ClearSelection();
                _selectedBuilding = null;
            }
        }

        private void HandleRightClick()
        {
            if (_selectedUnits.Count == 0) return;

            Vector2 worldPos = _camera.ScreenToWorld(_mousePos);
            _lastRightClickPos = worldPos;

            var clickedEnemy = GetUnitAt(worldPos, _enemyUnits);
            if (clickedEnemy != null)
            {
                foreach (var unit in _selectedUnits.Where(u => u.CanAttack))
                {
                    unit.AttackUnit(clickedEnemy);
                }
                return;
            }

            var clickedResource = GetResourceBuildingAt(worldPos);
            if (clickedResource != null)
            {
                foreach (var unit in _selectedUnits.Where(u => u.CanGather))
                {
                    unit.GatherFrom(clickedResource);
                }
                return;
            }

            foreach (var unit in _selectedUnits)
            {
                unit.MoveTo(worldPos);
            }

            CreateMoveIndicator(worldPos);
        }

        private void UpdateSelectionBox()
        {
            if (!_isSelecting) return;

            Vector2 worldPos = _camera.ScreenToWorld(_mousePos);
            _selectionEnd = worldPos;
        }

        private void FinalizeSelection()
        {
            float minX = Math.Min(_selectionStart.X, _selectionEnd.X);
            float maxX = Math.Max(_selectionStart.X, _selectionEnd.X);
            float minY = Math.Min(_selectionStart.Y, _selectionEnd.Y);
            float maxY = Math.Max(_selectionStart.Y, _selectionEnd.Y);

            if (maxX - minX < 10 && maxY - minY < 10) return;

            foreach (var unit in _playerUnits.Where(u => !u.IsDead))
            {
                if (unit.Position.X >= minX && unit.Position.X <= maxX &&
                    unit.Position.Y >= minY && unit.Position.Y <= maxY)
                {
                    AddToSelection(unit);
                }
            }

            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }

        private void AddToSelection(Unit unit)
        {
            if (_selectedUnits.Count >= 20) return;
            if (!_selectedUnits.Contains(unit))
            {
                unit.IsSelected = true;
                _selectedUnits.Add(unit);
            }
        }

        private void ClearUnitSelection()
        {
            foreach (var unit in _selectedUnits)
            {
                unit.IsSelected = false;
            }
            _selectedUnits.Clear();
        }

        private void ClearSelection()
        {
            ClearUnitSelection();
            if (_selectedBuilding != null)
            {
                _selectedBuilding.IsSelected = false;
                _selectedBuilding = null;
            }
        }

        private Unit? GetUnitAt(Vector2 position, List<Unit> units)
        {
            foreach (var unit in units.Where(u => !u.IsDead))
            {
                float dist = (unit.Position - position).Length();
                if (dist <= unit.Size + 10)
                {
                    return unit;
                }
            }
            return null;
        }

        private Building? GetBuildingAt(Vector2 position)
        {
            var allBuildings = _playerBuildings.Concat(_enemyBuildings).Concat(_resourceNodes);
            foreach (var building in allBuildings.Where(b => !b.IsDead))
            {
                float dist = (building.Position - position).Length();
                if (dist <= building.Size)
                {
                    return building;
                }
            }
            return null;
        }

        private Building? GetResourceBuildingAt(Vector2 position)
        {
            foreach (var resource in _resourceNodes.Where(r => !r.IsDead && r.ResourcesRemaining > 0))
            {
                float dist = (resource.Position - position).Length();
                if (dist <= resource.Size + 20)
                {
                    return resource;
                }
            }
            return null;
        }

        private void UpdateUnits(float deltaTime)
        {
            var allUnits = _playerUnits.Concat(_enemyUnits).ToList();
            var allBuildings = _playerBuildings.Concat(_enemyBuildings).Concat(_resourceNodes).ToList();

            for (int i = _playerUnits.Count - 1; i >= 0; i--)
            {
                var unit = _playerUnits[i];
                unit.Update(deltaTime, allUnits, allBuildings);

                if (unit.IsDead)
                {
                    _currentPopulation -= unit.PopulationCost;
                    CreateDeathEffect(unit.Position, unit.GetUnitColor());
                    _playerUnits.RemoveAt(i);
                    _selectedUnits.Remove(unit);
                }
            }

            for (int i = _enemyUnits.Count - 1; i >= 0; i--)
            {
                var unit = _enemyUnits[i];
                unit.Update(deltaTime, allUnits, allBuildings);

                if (unit.IsDead)
                {
                    _score += unit.Cost;
                    CreateDeathEffect(unit.Position, unit.GetUnitColor());
                    _enemyUnits.RemoveAt(i);
                }
            }

            _currentPopulation = _playerUnits.Sum(u => u.PopulationCost);
            PopulationChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateBuildings(float deltaTime)
        {
            var allUnits = _playerUnits.Concat(_enemyUnits).ToList();

            for (int i = _playerBuildings.Count - 1; i >= 0; i--)
            {
                var building = _playerBuildings[i];
                building.Update(deltaTime, allUnits);

                if (building.IsDead)
                {
                    CreateDeathEffect(building.Position, building.GetBuildingColor());
                    _playerBuildings.RemoveAt(i);
                    if (_selectedBuilding == building)
                    {
                        _selectedBuilding = null;
                    }
                }
            }

            for (int i = _enemyBuildings.Count - 1; i >= 0; i--)
            {
                var building = _enemyBuildings[i];
                building.Update(deltaTime, allUnits);

                if (building.IsDead)
                {
                    _score += building.BaseStats.GoldCost;
                    CreateDeathEffect(building.Position, building.GetBuildingColor());
                    _enemyBuildings.RemoveAt(i);
                }
            }

            for (int i = _resourceNodes.Count - 1; i >= 0; i--)
            {
                var resource = _resourceNodes[i];
                if (resource.ResourcesRemaining <= 0 && resource.Type != BuildingType.Farm)
                {
                    _resourceNodes.RemoveAt(i);
                }
            }
        }

        private void UpdateEffects(float deltaTime)
        {
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                _effects[i].Update(deltaTime);
                if (_effects[i].IsExpired)
                {
                    _effects.RemoveAt(i);
                }
            }
        }

        private void UpdateResourceCollection(float deltaTime)
        {
            foreach (var unit in _playerUnits.Where(u => u.IsCarryingResource && u.State == UnitState.Moving))
            {
                Building? townCenter = _playerBuildings.FirstOrDefault(b => b.Type == BuildingType.TownCenter);
                if (townCenter != null)
                {
                    float dist = (unit.Position - townCenter.Position).Length();
                    if (dist < townCenter.Size)
                    {
                        switch (unit.CarriedResource)
                        {
                            case ResourceType.Gold:
                                _gold += (int)unit.CarriedAmount;
                                break;
                            case ResourceType.Wood:
                                _wood += (int)unit.CarriedAmount;
                                break;
                            case ResourceType.Food:
                                _food += (int)unit.CarriedAmount;
                                break;
                        }
                        unit.IsCarryingResource = false;
                        unit.CarriedAmount = 0;
                        unit.State = UnitState.Idle;
                        ResourcesChanged?.Invoke(this, EventArgs.Empty);

                        if (unit.GatherTarget != null && unit.GatherTarget.ResourcesRemaining > 0)
                        {
                            unit.State = UnitState.Gathering;
                        }
                        else
                        {
                            unit.GatherTarget = null;
                        }
                    }
                }
            }
        }

        private void UpdateResourceGeneration()
        {
            int foodFromFarms = _playerBuildings.Count(b => b.Type == BuildingType.Farm && b.ResourcesRemaining > 0);
            _food += foodFromFarms;

            _maxPopulation = 50 + _playerBuildings.Count(b => b.Type == BuildingType.TownCenter) * 25 +
                             _playerBuildings.Count(b => b.Type == BuildingType.Castle) * 50;

            ResourcesChanged?.Invoke(this, EventArgs.Empty);
        }

        private void UpdateAI(float deltaTime)
        {
            _aiTimer += deltaTime;

            if (_aiTimer >= 10f)
            {
                _aiTimer = 0;

                foreach (var worker in _enemyUnits.Where(u => u.Type == UnitType.Worker && u.State == UnitState.Idle))
                {
                    var nearestResource = _resourceNodes
                        .Where(r => !r.IsDead && r.ResourcesRemaining > 0 && !r.IsOwnedByPlayer)
                        .OrderBy(r => (r.Position - worker.Position).Length())
                        .FirstOrDefault();

                    if (nearestResource != null)
                    {
                        worker.GatherFrom(nearestResource);
                    }
                }

                foreach (var worker in _enemyUnits.Where(u => u.Type == UnitType.Worker && u.State == UnitState.Idle))
                {
                    var enemyTownCenter = _enemyBuildings.FirstOrDefault(b => b.Type == BuildingType.TownCenter);
                    if (enemyTownCenter != null)
                    {
                        float dist = (worker.Position - enemyTownCenter.Position).Length();
                        if (dist < 50 && _enemyUnits.Count < 30)
                        {
                            var newWorker = new Unit(UnitType.Worker, enemyTownCenter.Position + new Vector2(_random.Next(-30, 30), _random.Next(-30, 30)), false);
                            _enemyUnits.Add(newWorker);
                        }
                    }
                }

                var enemyBarracks = _enemyBuildings.FirstOrDefault(b => b.Type == BuildingType.Barracks);
                if (enemyBarracks != null && _enemyUnits.Count < 40)
                {
                    if (_random.NextDouble() > 0.5)
                    {
                        var newUnit = new Unit(UnitType.Infantry, enemyBarracks.Position + new Vector2(_random.Next(-40, 40), _random.Next(-40, 40)), false);
                        _enemyUnits.Add(newUnit);
                    }
                    else
                    {
                        var newUnit = new Unit(UnitType.Archer, enemyBarracks.Position + new Vector2(_random.Next(-40, 40), _random.Next(-40, 40)), false);
                        _enemyUnits.Add(newUnit);
                    }
                }

                foreach (var enemy in _enemyUnits.Where(u => u.CanAttack && u.State == UnitState.Idle))
                {
                    var nearestPlayer = _playerUnits
                        .Where(p => !p.IsDead)
                        .OrderBy(p => (p.Position - enemy.Position).Length())
                        .FirstOrDefault();

                    if (nearestPlayer != null)
                    {
                        float dist = (enemy.Position - nearestPlayer.Position).Length();
                        if (dist < 300)
                        {
                            enemy.AttackUnit(nearestPlayer);
                        }
                    }
                }

                if (_enemyBuildings.Count(b => b.Type == BuildingType.Barracks) == 0 && _enemyUnits.Count >= 10)
                {
                    var enemyTownCenter = _enemyBuildings.FirstOrDefault(b => b.Type == BuildingType.TownCenter);
                    if (enemyTownCenter != null)
                    {
                        var barracks = new Building(BuildingType.Barracks, enemyTownCenter.Position + new Vector2(_random.Next(80, 150), _random.Next(-100, 100)), false);
                        _enemyBuildings.Add(barracks);
                    }
                }
            }

            foreach (var enemy in _enemyUnits.Where(u => u.Type == UnitType.Worker && u.State == UnitState.Gathering))
            {
                if (enemy.GatherTarget != null && enemy.GatherTarget.ResourcesRemaining <= 0)
                {
                    var newResource = _resourceNodes
                        .Where(r => !r.IsDead && r.ResourcesRemaining > 0 && !r.IsOwnedByPlayer)
                        .OrderBy(r => (r.Position - enemy.Position).Length())
                        .FirstOrDefault();

                    if (newResource != null)
                    {
                        enemy.GatherFrom(newResource);
                    }
                }
            }
        }

        private void UpdateTechResearch(float deltaTime)
        {
            foreach (var tech in _techTree.Values.Where(t => t.IsResearching && !t.IsResearched))
            {
                tech.ResearchProgress += deltaTime / tech.ResearchTime;
                if (tech.ResearchProgress >= 1f)
                {
                    tech.IsResearched = true;
                    tech.IsResearching = false;
                    TechResearched?.Invoke(this, EventArgs.Empty);
                }
            }
        }

        private void CheckWinCondition()
        {
            bool playerHasTownCenter = _playerBuildings.Any(b => b.Type == BuildingType.TownCenter);
            bool enemyHasTownCenter = _enemyBuildings.Any(b => b.Type == BuildingType.TownCenter);

            if (!playerHasTownCenter)
            {
                SetGameState(RTSGameState.GameOver);
            }
            else if (!enemyHasTownCenter && _enemyUnits.Count == 0)
            {
                SetGameState(RTSGameState.Victory);
            }
        }

        private void SetGameState(RTSGameState newState)
        {
            _previousState = _gameState;
            _gameState = newState;
        }

        public void StartGame()
        {
            _gold = STARTING_GOLD;
            _wood = STARTING_WOOD;
            _food = STARTING_FOOD;
            _currentPopulation = 0;
            _maxPopulation = MAX_POPULATION;
            _score = 0;
            _gameTime = 0;
            _resourceTimer = 0;

            _playerUnits.Clear();
            _enemyUnits.Clear();
            _playerBuildings.Clear();
            _enemyBuildings.Clear();
            _resourceNodes.Clear();
            _effects.Clear();
            _selectedUnits.Clear();
            _selectedBuilding = null;
            _placementMode = null;

            foreach (var tech in _techTree.Values)
            {
                tech.IsResearched = false;
                tech.IsResearching = false;
                tech.ResearchProgress = 0;
            }

            GenerateMap();
            SetGameState(RTSGameState.Playing);

            ResourcesChanged?.Invoke(this, EventArgs.Empty);
            PopulationChanged?.Invoke(this, EventArgs.Empty);
        }

        public void RestartGame()
        {
            StartGame();
        }

        private bool TryPlaceBuilding(BuildingType type, Vector2 position)
        {
            var stats = GetBuildingStats(type);

            if (_gold < stats.GoldCost || _wood < stats.WoodCost || _food < stats.FoodCost)
                return false;

            if (_currentPopulation + stats.PopulationCost > _maxPopulation)
                return false;

            bool canPlace = CanPlaceAt(position, type);
            if (!canPlace) return false;

            _gold -= stats.GoldCost;
            _wood -= stats.WoodCost;
            _food -= stats.FoodCost;

            var building = new Building(type, position, true);

            var nearestWorker = _selectedUnits.FirstOrDefault(u => u.Type == UnitType.Worker);
            if (nearestWorker != null && type != BuildingType.TownCenter)
            {
                nearestWorker.Build(building);
            }

            _playerBuildings.Add(building);
            _placementMode = null;

            BuildingPlaced?.Invoke(this, EventArgs.Empty);
            ResourcesChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private bool CanPlaceAt(Vector2 position, BuildingType type)
        {
            float size = GetBuildingSize(type);
            var allBuildings = _playerBuildings.Concat(_enemyBuildings).Concat(_resourceNodes);

            foreach (var building in allBuildings.Where(b => !b.IsDead))
            {
                float dist = (building.Position - position).Length();
                if (dist < (size + building.Size) * 0.6f)
                    return false;
            }

            if (position.X < _mapOffset.X + size || position.X > MAP_WIDTH - size ||
                position.Y < _mapOffset.Y + size || position.Y > MAP_HEIGHT - size)
                return false;

            return true;
        }

        private bool CreateUnit(UnitType type)
        {
            if (_selectedBuilding == null || !_selectedBuilding.IsConstructed)
                return false;

            if (!_selectedBuilding.CanProduce.Contains(type))
                return false;

            var unitStats = GetUnitStats(type);
            if (_gold < unitStats.Cost || _currentPopulation + unitStats.PopulationCost > _maxPopulation)
                return false;

            _gold -= unitStats.Cost;
            _selectedBuilding.StartTraining(type);

            ResourcesChanged?.Invoke(this, EventArgs.Empty);
            return true;
        }

        private void CreateMoveIndicator(Vector2 position)
        {
            _effects.Add(new MoveIndicatorEffect(position, 0.5f));
        }

        private void CreateDeathEffect(Vector2 position, Color color)
        {
            for (int i = 0; i < 8; i++)
            {
                float angle = (float)_random.NextDouble() * MathHelper.TwoPi;
                float speed = 50f + (float)_random.NextDouble() * 100f;
                Vector2 velocity = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed);
                _effects.Add(new ParticleEffect(position, velocity, color, 0.5f));
            }
        }

        public void CreateDamageNumber(Vector2 position, float damage)
        {
            _effects.Add(new DamageNumber(position, damage));
        }

        private static BuildingStats GetBuildingStats(BuildingType type)
        {
            return type switch
            {
                BuildingType.TownCenter => new BuildingStats { GoldCost = 0, WoodCost = 0, FoodCost = 0, PopulationCost = 0 },
                BuildingType.Barracks => new BuildingStats { GoldCost = 150, WoodCost = 100, FoodCost = 0, PopulationCost = 0 },
                BuildingType.Farm => new BuildingStats { GoldCost = 50, WoodCost = 30, FoodCost = 0, PopulationCost = 0 },
                BuildingType.Mine => new BuildingStats { GoldCost = 80, WoodCost = 50, FoodCost = 0, PopulationCost = 0 },
                BuildingType.LumberMill => new BuildingStats { GoldCost = 70, WoodCost = 0, FoodCost = 0, PopulationCost = 0 },
                BuildingType.Tower => new BuildingStats { GoldCost = 100, WoodCost = 80, FoodCost = 0, PopulationCost = 0 },
                BuildingType.Castle => new BuildingStats { GoldCost = 500, WoodCost = 300, FoodCost = 100, PopulationCost = 0 },
                _ => new BuildingStats { GoldCost = 100, WoodCost = 100, FoodCost = 0, PopulationCost = 0 }
            };
        }

        private static UnitStats GetUnitStats(UnitType type)
        {
            return type switch
            {
                UnitType.Worker => new UnitStats { Cost = 50, PopulationCost = 1 },
                UnitType.Infantry => new UnitStats { Cost = 60, PopulationCost = 1 },
                UnitType.Archer => new UnitStats { Cost = 80, PopulationCost = 1 },
                UnitType.Knight => new UnitStats { Cost = 150, PopulationCost = 2 },
                UnitType.Mage => new UnitStats { Cost = 200, PopulationCost = 2 },
                UnitType.Siege => new UnitStats { Cost = 250, PopulationCost = 3 },
                _ => new UnitStats { Cost = 100, PopulationCost = 1 }
            };
        }

        private static float GetBuildingSize(BuildingType type)
        {
            return type switch
            {
                BuildingType.TownCenter => 60,
                BuildingType.Barracks => 50,
                BuildingType.Farm => 40,
                BuildingType.Mine => 35,
                BuildingType.LumberMill => 45,
                BuildingType.Tower => 30,
                BuildingType.Castle => 70,
                _ => 45
            };
        }

        protected override void OnRender(DrawingContext context)
        {
            base.OnRender(context);

            DrawBackground(context);
            DrawResourceNodes(context);
            DrawBuildings(context);
            DrawUnits(context);
            DrawEffects(context);
            DrawSelectionBox(context);
            DrawPlacementPreview(context);
            DrawUI(context);

            if (_gameState == RTSGameState.Menu)
                DrawMainMenu(context);

            if (_gameState == RTSGameState.Paused)
                DrawPauseMenu(context);

            if (_gameState == RTSGameState.GameOver)
                DrawGameOverScreen(context);

            if (_gameState == RTSGameState.Victory)
                DrawVictoryScreen(context);
        }

        private void DrawBackground(DrawingContext context)
        {
            var bgBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(34, 85, 34));
            context.DrawRectangle(bgBrush, null, new Rect(0, 0, 1920, 1080));

            var gridPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(20, 100, 140, 100)), 1);
            for (float x = 0; x < MAP_WIDTH; x += 100)
            {
                Vector2 start = _camera.WorldToScreen(new Vector2(x, 0));
                Vector2 end = _camera.WorldToScreen(new Vector2(x, MAP_HEIGHT));
                if (start.X >= 0 && start.X <= 1920)
                    context.DrawLine(gridPen, new Point(start.X, Math.Max(0, start.Y)), new Point(end.X, Math.Min(1080, end.Y)));
            }
            for (float y = 0; y < MAP_HEIGHT; y += 100)
            {
                Vector2 start = _camera.WorldToScreen(new Vector2(0, y));
                Vector2 end = _camera.WorldToScreen(new Vector2(MAP_WIDTH, y));
                if (start.Y >= 0 && start.Y <= 1080)
                    context.DrawLine(gridPen, new Point(Math.Max(0, start.X), start.Y), new Point(Math.Min(1920, end.X), end.Y));
            }

            var borderPen = new Pen(System.Windows.Media.Brushes.DarkGray, 3);
            Vector2 tl = _camera.WorldToScreen(_mapOffset);
            Vector2 br = _camera.WorldToScreen(new Vector2(MAP_WIDTH, MAP_HEIGHT));
            context.DrawRectangle(null, borderPen, new Rect(tl.X, tl.Y, br.X - tl.X, br.Y - tl.Y));
        }

        private void DrawResourceNodes(DrawingContext context)
        {
            foreach (var resource in _resourceNodes.Where(r => !r.IsDead))
            {
                Vector2 screenPos = _camera.WorldToScreen(resource.Position);
                float size = resource.Size * _camera.Zoom;

                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(resource.GetBuildingColor().R * 255),
                    (byte)(resource.GetBuildingColor().G * 255),
                    (byte)(resource.GetBuildingColor().B * 255)));

                if (resource.Type == BuildingType.Farm)
                {
                    context.DrawRectangle(brush, null, new Rect(screenPos.X - size / 2, screenPos.Y - size / 2, size, size));
                    DrawProgressBar(context, screenPos.X, screenPos.Y - size / 2 - 10, size, resource.ResourcesRemaining / 500f, Color.Green);
                }
                else if (resource.Type == BuildingType.Mine)
                {
                    var points = new PointCollection();
                    for (int i = 0; i < 6; i++)
                    {
                        float angle = i * MathHelper.PiOver2 - MathHelper.PiOver4;
                        points.Add(new Point(screenPos.X + (float)Math.Cos(angle) * size / 2, screenPos.Y + (float)Math.Sin(angle) * size / 2));
                    }
                    context.DrawPolygon(brush, null, points);
                }
                else if (resource.Type == BuildingType.LumberMill)
                {
                    context.DrawEllipse(brush, null, new Point(screenPos.X, screenPos.Y), size / 2, size / 2);
                }

                if (resource.ResourcesRemaining < 100)
                {
                    float alpha = (float)Math.Sin(_gameTime * 5) * 0.3f + 0.5f;
                    var depletedBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(alpha * 255), 200, 200, 200));
                    context.DrawRectangle(depletedBrush, null, new Rect(screenPos.X - size / 2, screenPos.Y - size / 2, size, size));
                }
            }
        }

        private void DrawBuildings(DrawingContext context)
        {
            var allBuildings = _playerBuildings.Concat(_enemyBuildings);
            foreach (var building in allBuildings.Where(b => !b.IsDead))
            {
                Vector2 screenPos = _camera.WorldToScreen(building.Position);
                float size = building.Size * _camera.Zoom;

                Color buildingColor = building.GetBuildingColor();
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(buildingColor.R * 255),
                    (byte)(buildingColor.G * 255),
                    (byte)(buildingColor.B * 255)));

                if (!building.IsConstructed)
                {
                    float progress = building.ConstructionProgress;
                    context.DrawRectangle(brush, null, new Rect(screenPos.X - size / 2, screenPos.Y - size / 2, size * progress, size * progress));
                    DrawProgressBar(context, screenPos.X, screenPos.Y - size / 2 - 8, size, progress, Color.Yellow);
                }
                else
                {
                    context.DrawRectangle(brush, null, new Rect(screenPos.X - size / 2, screenPos.Y - size / 2, size, size));

                    var borderPen = new Pen(System.Windows.Media.Brushes.Black, 2);
                    context.DrawRectangle(null, borderPen, new Rect(screenPos.X - size / 2, screenPos.Y - size / 2, size, size));

                    if (building.IsSelected)
                    {
                        var selectionPen = new Pen(Brushes.Yellow, 3);
                        context.DrawRectangle(null, selectionPen, new Rect(screenPos.X - size / 2 - 3, screenPos.Y - size / 2 - 3, size + 6, size + 6));
                    }
                }

                if (building.IsConstructed && building.Health < building.MaxHealth)
                {
                    DrawHealthBar(context, screenPos.X, screenPos.Y - size / 2 - 12, size, building.Health / building.MaxHealth);
                }

                if (building.TrainingQueue.Count > 0 && building.TrainProgress > 0)
                {
                    DrawProgressBar(context, screenPos.X - size / 2, screenPos.Y + size / 2 + 5, size, building.TrainProgress, Color.Blue);
                }
            }
        }

        private void DrawUnits(DrawingContext context)
        {
            var allUnits = _playerUnits.Concat(_enemyUnits);
            foreach (var unit in allUnits.Where(u => !u.IsDead))
            {
                Vector2 screenPos = _camera.WorldToScreen(unit.Position);
                float size = unit.Size * _camera.Zoom;

                Color unitColor = unit.GetUnitColor();
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(unitColor.R * 255),
                    (byte)(unitColor.G * 255),
                    (byte)(unitColor.B * 255)));

                double angle = unit.AttackAngle * 180 / Math.PI;
                context.PushTransform(new RotateTransform(angle, screenPos.X, screenPos.Y));

                if (unit.Type == UnitType.Siege)
                {
                    context.DrawRectangle(brush, new Pen(System.Windows.Media.Brushes.DarkGray, 2), new Rect(screenPos.X - size / 2, screenPos.Y - size / 2, size, size * 0.7f));
                    var barrelPen = new Pen(System.Windows.Media.Brushes.DarkGray, size * 0.15f);
                    context.DrawLine(barrelPen, new Point(screenPos.X + size / 4, screenPos.Y), new Point(screenPos.X + size, screenPos.Y));
                }
                else if (unit.Type == UnitType.Knight)
                {
                    context.DrawEllipse(brush, new Pen(System.Windows.Media.Brushes.White, 2), new Point(screenPos.X, screenPos.Y), size / 2, size / 2);
                    context.DrawEllipse(System.Windows.Media.Brushes.LightGray, null, new Point(screenPos.X + size / 3, screenPos.Y), size / 4, size / 4);
                }
                else if (unit.Type == UnitType.Archer)
                {
                    context.DrawEllipse(brush, new Pen(System.Windows.Media.Brushes.Brown, 2), new Point(screenPos.X, screenPos.Y), size / 2, size / 2);
                    var bowPen = new Pen(System.Windows.Media.Brushes.Brown, 2);
                    context.DrawArc(bowPen, new Rect(screenPos.X + size / 3, screenPos.Y - size / 3, size / 3, size * 0.66f), 90, 180);
                }
                else if (unit.Type == UnitType.Mage)
                {
                    context.DrawEllipse(brush, new Pen(System.Windows.Media.Brushes.Purple, 2), new Point(screenPos.X, screenPos.Y), size / 2, size / 2);
                    var glowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 200, 100, 255));
                    context.DrawEllipse(glowBrush, null, new Point(screenPos.X + size / 3, screenPos.Y), size / 3, size / 3);
                }
                else
                {
                    context.DrawEllipse(brush, new Pen(System.Windows.Media.Brushes.Black, 1), new Point(screenPos.X, screenPos.Y), size / 2, size / 2);
                }

                context.Pop();

                if (unit.IsSelected)
                {
                    var selectionPen = new Pen(Brushes.Yellow, 2);
                    context.DrawEllipse(null, selectionPen, new Point(screenPos.X, screenPos.Y), size / 2 + 4, size / 2 + 4);
                }

                if (unit.Health < unit.MaxHealth)
                {
                    DrawHealthBar(context, screenPos.X, screenPos.Y - size / 2 - 8, size, unit.Health / unit.MaxHealth);
                }

                if (unit.State == UnitState.Attacking)
                {
                    float pulse = (float)Math.Sin(_gameTime * 10) * 0.3f + 0.7f;
                    var attackBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(pulse * 100), 255, 100, 0));
                    context.DrawEllipse(attackBrush, null, new Point(screenPos.X, screenPos.Y), size * 1.2f, size * 1.2f);
                }

                if (unit.IsCarryingResource)
                {
                    var resourceBrush = unit.CarriedResource switch
                    {
                        ResourceType.Gold => new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0)),
                        ResourceType.Wood => new SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 90, 43)),
                        ResourceType.Food => new SolidColorBrush(System.Windows.Media.Color.FromRgb(144, 238, 144)),
                        _ => System.Windows.Media.Brushes.White
                    };
                    context.DrawEllipse(resourceBrush, null, new Point(screenPos.X, screenPos.Y - size / 2 - 5), 4, 4);
                }
            }
        }

        private void DrawEffects(DrawingContext context)
        {
            foreach (var effect in _effects)
            {
                effect.Render(context, _camera);
            }
        }

        private void DrawSelectionBox(DrawingContext context)
        {
            if (!_isSelecting) return;

            Vector2 start = _camera.WorldToScreen(_selectionStart);
            Vector2 end = _camera.WorldToScreen(_selectionEnd);

            float x = Math.Min(start.X, end.X);
            float y = Math.Min(start.Y, end.Y);
            float width = Math.Abs(end.X - start.X);
            float height = Math.Abs(end.Y - start.Y);

            var fillBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(30, 100, 150, 255));
            var borderPen = new Pen(System.Windows.Media.Brushes.LightBlue, 1);

            context.DrawRectangle(fillBrush, borderPen, new Rect(x, y, width, height));
        }

        private void DrawPlacementPreview(DrawingContext context)
        {
            if (!_placementMode.HasValue) return;

            Vector2 worldPos = _camera.ScreenToWorld(_mousePos);
            Vector2 screenPos = _camera.WorldToScreen(worldPos);
            float size = GetBuildingSize(_placementMode.Value) * _camera.Zoom;

            bool canPlace = CanPlaceAt(worldPos, _placementMode.Value);
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150,
                canPlace ? (byte)100 : (byte)255,
                canPlace ? (byte)255 : (byte)100,
                canPlace ? (byte)100 : (byte)100));

            context.DrawRectangle(brush, new Pen(canPlace ? System.Windows.Media.Brushes.Green : System.Windows.Media.Brushes.Red, 2), new Rect(screenPos.X - size / 2, screenPos.Y - size / 2, size, size));
        }

        private void DrawHealthBar(DrawingContext context, float x, float y, float width, float percentage)
        {
            float barHeight = 4;
            float healthPercent = Math.Max(0, percentage);

            context.DrawRectangle(System.Windows.Media.Brushes.DarkGray, null, new Rect(x - width / 2, y, width, barHeight));

            Color healthColor = healthPercent > 0.5 ? Color.Green : (healthPercent > 0.25 ? Color.Yellow : Color.Red);
            var healthBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(healthColor.R * 255), (byte)(healthColor.G * 255), (byte)(healthColor.B * 255)));

            context.DrawRectangle(healthBrush, null, new Rect(x - width / 2, y, width * healthPercent, barHeight));
        }

        private void DrawProgressBar(DrawingContext context, float x, float y, float width, float percentage, Color color)
        {
            float barHeight = 3;
            float progress = Math.Max(0, Math.Min(1, percentage));

            context.DrawRectangle(System.Windows.Media.Brushes.DarkGray, null, new Rect(x - width / 2, y, width, barHeight));

            var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)));

            context.DrawRectangle(brush, null, new Rect(x - width / 2, y, width * progress, barHeight));
        }

        private void DrawUI(DrawingContext context)
        {
            if (_gameState == RTSGameState.Menu) return;

            DrawResourceBar(context);
            DrawMinimap(context);
            DrawUnitInfoPanel(context);
            DrawBuildingPanel(context);
            DrawTechPanel(context);
            DrawControls(context);
        }

        private void DrawResourceBar(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 20, 25, 35));
            context.DrawRectangle(panelBrush, null, new Rect(0, 0, 1920, 50));

            var goldBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 215, 0));
            var woodBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(139, 90, 43));
            var foodBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(144, 238, 144));

            DrawResourceIcon(context, 20, 12, goldBrush, "Gold");
            DrawResourceIcon(context, 170, 12, woodBrush, "Wood");
            DrawResourceIcon(context, 320, 12, foodBrush, "Food");

            var popText = new FormattedText($"Pop: {_currentPopulation}/{_maxPopulation}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16,
                _currentPopulation >= _maxPopulation ? Brushes.Red : Brushes.White, 1.0);
            context.DrawText(popText, new Point(470, 14));

            var scoreText = new FormattedText($"Score: {_score}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, Brushes.LightGray, 1.0);
            context.DrawText(scoreText, new Point(620, 16));

            var timeText = new FormattedText($"Time: {FormatTime(_gameTime)}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, Brushes.LightGray, 1.0);
            context.DrawText(timeText, new Point(750, 16));

            if (_placementMode.HasValue)
            {
                var placementText = new FormattedText($"Placing: {_placementMode.Value} [ESC to cancel]", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 14, Brushes.Yellow, 1.0);
                context.DrawText(placementText, new Point(870, 14));
            }
        }

        private void DrawResourceIcon(DrawingContext context, float x, float y, SolidColorBrush brush, string resource)
        {
            context.DrawEllipse(brush, null, new Point(x + 10, y + 12), 12, 12);

            int value = resource switch
            {
                "Gold" => _gold,
                "Wood" => _wood,
                "Food" => _food,
                _ => 0
            };

            var text = new FormattedText($"{value}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
            context.DrawText(text, new Point(x + 30, 12));
        }

        private void DrawMinimap(DrawingContext context)
        {
            var minimapBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 30, 35, 45));
            context.DrawRectangle(minimapBrush, new Pen(System.Windows.Media.Brushes.DarkGray, 2), _minimapRect);

            float scaleX = _minimapRect.Width / MAP_WIDTH;
            float scaleY = _minimapRect.Height / MAP_HEIGHT;

            foreach (var unit in _playerUnits.Where(u => !u.IsDead))
            {
                float x = _minimapRect.X + unit.Position.X * scaleX;
                float y = _minimapRect.Y + unit.Position.Y * scaleY;
                context.DrawEllipse(Brushes.Blue, null, new Point(x, y), 2, 2);
            }

            foreach (var unit in _enemyUnits.Where(u => !u.IsDead))
            {
                float x = _minimapRect.X + unit.Position.X * scaleX;
                float y = _minimapRect.Y + unit.Position.Y * scaleY;
                context.DrawEllipse(Brushes.Red, null, new Point(x, y), 2, 2);
            }

            foreach (var building in _playerBuildings.Where(b => !b.IsDead))
            {
                float x = _minimapRect.X + building.Position.X * scaleX;
                float y = _minimapRect.Y + building.Position.Y * scaleY;
                context.DrawRectangle(Brushes.Blue, null, new Rect(x - 3, y - 3, 6, 6));
            }

            foreach (var building in _enemyBuildings.Where(b => !b.IsDead))
            {
                float x = _minimapRect.X + building.Position.X * scaleX;
                float y = _minimapRect.Y + building.Position.Y * scaleY;
                context.DrawRectangle(Brushes.Red, null, new Rect(x - 3, y - 3, 6, 6));
            }

            float camX = _minimapRect.X + _camera.Position.X * scaleX;
            float camY = _minimapRect.Y + _camera.Position.Y * scaleY;
            var camPen = new Pen(Brushes.White, 1);
            context.DrawRectangle(null, camPen, new Rect(camX - 10, camY - 7, 20, 14));

            if (_mousePos.X >= _minimapRect.X && _mousePos.X <= _minimapRect.X + _minimapRect.Width &&
                _mousePos.Y >= _minimapRect.Y && _mousePos.Y <= _minimapRect.Y + _minimapRect.Height)
            {
                var hoverX = (_mousePos.X - _minimapRect.X) / scaleX;
                var hoverY = (_mousePos.Y - _minimapRect.Y) / scaleY;
                _camera.Position = new Vector2(hoverX, hoverY);
            }
        }

        private void DrawUnitInfoPanel(DrawingContext context)
        {
            if (_selectedUnits.Count == 0 && _selectedBuilding == null) return;

            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 20, 25, 35));
            context.DrawRectangle(panelBrush, null, new Rect(20, 60, 300, 280));

            if (_selectedUnits.Count > 0)
            {
                var unit = _selectedUnits[0];
                var nameText = new FormattedText(_selectedUnits.Count > 1 ? $"{_selectedUnits.Count} Units Selected" : unit.Name, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
                context.DrawText(nameText, new Point(30, 70));

                var healthText = new FormattedText($"Health: {(int)unit.Health}/{unit.MaxHealth}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.LightGray, 1.0);
                context.DrawText(healthText, new Point(30, 100));

                DrawProgressBar(context, 30, 120, 280, unit.Health / unit.MaxHealth, Color.Green);

                var statsText = new FormattedText(
                    $"Attack: {unit.Attack:F0}  Defense: {unit.Defense:F0}\n" +
                    $"Speed: {unit.Speed:F0}  Range: {unit.AttackRange:F0}",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.LightGray, 1.0);
                context.DrawText(statsText, new Point(30, 135));

                var stateText = new FormattedText($"State: {unit.State}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Yellow, 1.0);
                context.DrawText(stateText, new Point(30, 180));

                if (_selectedUnits.Count > 1)
                {
                    var typesText = new FormattedText("Unit Types:", System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, Brushes.Gray, 1.0);
                    context.DrawText(typesText, new Point(30, 210));

                    int y = 225;
                    foreach (var group in _selectedUnits.GroupBy(u => u.Type))
                    {
                        var groupText = new FormattedText($"{group.Key}: {group.Count()}", System.Globalization.CultureInfo.CurrentCulture,
                            System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, Brushes.White, 1.0);
                        context.DrawText(groupText, new Point(40, y));
                        y += 15;
                    }
                }

                if (unit.CanGather)
                {
                    var gatherHint = new FormattedText("Right-click resources to gather", System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, Brushes.Cyan, 1.0);
                    context.DrawText(gatherHint, new Point(30, 310));
                }
            }

            if (_selectedBuilding != null)
            {
                var building = _selectedBuilding;
                var nameText = new FormattedText(building.Name, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
                context.DrawText(nameText, new Point(30, 70));

                if (!building.IsConstructed)
                {
                    var constructText = new FormattedText($"Constructing: {building.ConstructionProgress * 100:F0}%", System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.Yellow, 1.0);
                    context.DrawText(constructText, new Point(30, 100));

                    DrawProgressBar(context, 30, 120, 280, building.ConstructionProgress, Color.Yellow);
                }
                else
                {
                    var healthText = new FormattedText($"Health: {(int)building.Health}/{building.MaxHealth}", System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 12, Brushes.LightGray, 1.0);
                    context.DrawText(healthText, new Point(30, 100));

                    DrawProgressBar(context, 30, 120, 280, building.Health / building.MaxHealth, Color.Green);

                    if (building.CanProduce.Count > 0)
                    {
                        var produceText = new FormattedText("Can produce:", System.Globalization.CultureInfo.CurrentCulture,
                            System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.White, 1.0);
                        context.DrawText(produceText, new Point(30, 140));

                        int y = 160;
                        foreach (var unitType in building.CanProduce)
                        {
                            var unitStats = GetUnitStats(unitType);
                            var unitText = new FormattedText($"  {unitType}: {unitStats.Cost}g", System.Globalization.CultureInfo.CurrentCulture,
                                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10,
                                _gold >= unitStats.Cost ? Brushes.LightGray : Brushes.DarkGray, 1.0);
                            context.DrawText(unitText, new Point(40, y));
                            y += 15;
                        }

                        if (building.TrainingQueue.Count > 0)
                        {
                            var trainText = new FormattedText($"Training: {building.TrainingUnit} ({building.TrainProgress * 100:F0}%)", System.Globalization.CultureInfo.CurrentCulture,
                                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 11, Brushes.Cyan, 1.0);
                            context.DrawText(trainText, new Point(30, y + 10));
                        }
                    }
                }
            }
        }

        private void DrawBuildingPanel(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 20, 25, 35));
            context.DrawRectangle(panelBrush, null, new Rect(1700, 60, 200, 300));

            var titleText = new FormattedText("Build", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 14, Brushes.White, 1.0);
            context.DrawText(titleText, new Point(1720, 70));

            var buildings = new[] { BuildingType.Barracks, BuildingType.Farm, BuildingType.Tower, BuildingType.Mine, BuildingType.LumberMill };
            int y = 95;

            foreach (var building in buildings)
            {
                var stats = GetBuildingStats(building);
                bool canAfford = _gold >= stats.GoldCost && _wood >= stats.WoodCost && _food >= stats.FoodCost;
                bool isSelected = _placementMode == building;

                var bgColor = isSelected ? System.Windows.Media.Color.FromArgb(255, 60, 60, 80) :
                             (canAfford ? System.Windows.Media.Color.FromArgb(100, 40, 45, 55) :
                              System.Windows.Media.Color.FromArgb(50, 30, 30, 35));
                var bgBrush = new SolidColorBrush(bgColor);
                var borderBrush = isSelected ? System.Windows.Media.Brushes.Orange : System.Windows.Media.Brushes.Gray;

                context.DrawRectangle(bgBrush, new Pen(borderBrush, isSelected ? 2 : 1), new Rect(1710, y, 180, 40));

                var nameText = new FormattedText(building.ToString(), System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 11,
                    canAfford ? Brushes.White : Brushes.Gray, 1.0);
                context.DrawText(nameText, new Point(1720, y + 3));

                var costText = new FormattedText($"F{Array.IndexOf(buildings, building) + 1}: {stats.GoldCost}g {stats.WoodCost}w", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 9,
                    canAfford ? Brushes.Yellow : Brushes.DarkGray, 1.0);
                context.DrawText(costText, new Point(1720, y + 22));

                y += 45;
            }
        }

        private void DrawTechPanel(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(200, 20, 25, 35));
            context.DrawRectangle(panelBrush, null, new Rect(1700, 370, 200, 200));

            var titleText = new FormattedText("Tech Tree", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 14, Brushes.White, 1.0);
            context.DrawText(titleText, new Point(1720, 380));

            int y = 405;
            foreach (var tech in _techTree.Values.Take(6))
            {
                Color techColor = tech.IsResearched ? Color.Green : (tech.IsResearching ? Color.Yellow : Color.Gray);
                var techBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(
                    (byte)(techColor.R * 255), (byte)(techColor.G * 255), (byte)(techColor.B * 255)));

                string status = tech.IsResearched ? "[X]" : (tech.IsResearching ? $"[{tech.ResearchProgress * 100:F0}%]" : "[ ]");
                var techText = new FormattedText($"{status} {tech.Name}", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 9, Brushes.LightGray, 1.0);
                context.DrawText(techText, new Point(1710, y));
                y += 18;
            }
        }

        private void DrawControls(DrawingContext context)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 20, 25, 35));
            context.DrawRectangle(panelBrush, null, new Rect(350, 60, 400, 80));

            var controlsText = new FormattedText(
                "WASD: Move Camera | Mouse: Select | Right-Click: Command\n" +
                "F1-F5: Build | 1-5: Train Units | Scroll: Zoom | Shift: Multi-select",
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 10, Brushes.LightGray, 1.0);
            context.DrawText(controlsText, new Point(360, 70));
        }

        private void DrawMainMenu(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromRgb(15, 18, 25)), null, new Rect(0, 0, 1920, 1080));

            var titleBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 180, 50));
            var textBrush = System.Windows.Media.Brushes.White;

            var title = new FormattedText("REAL-TIME STRATEGY", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 64, titleBrush, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 150));

            var subtitle = new FormattedText("Command Your Forces to Victory", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 170)), 1.0);
            context.DrawText(subtitle, new Point(960 - subtitle.Width / 2, 230));

            DrawUnitTypePreview(context, 200, 400);
            DrawBuildingTypePreview(context, 900, 400);

            var startText = new FormattedText("Press ENTER to Start", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 28, textBrush, 1.0);
            context.DrawText(startText, new Point(960 - startText.Width / 2, 700));

            var controls = "Controls:\n" +
                          "WASD: Pan Camera | Mouse Wheel: Zoom\n" +
                          "Left Click: Select | Right Click: Command Units\n" +
                          "F1-F5: Build Buildings | 1-5: Train Units\n" +
                          "ESC: Pause | Shift+Click: Add to Selection";

            var controlsText = new FormattedText(controls, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, new SolidColorBrush(System.Windows.Media.Color.FromRgb(150, 150, 170)), 1.0);
            context.DrawText(controlsText, new Point(960 - controlsText.Width / 2, 780));
        }

        private void DrawUnitTypePreview(DrawingContext context, float x, float y)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 35, 45));
            context.DrawRectangle(panelBrush, null, new Rect(x - 20, y - 20, 380, 280));

            var title = new FormattedText("Units", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
            context.DrawText(title, new Point(x, y));

            var units = new[] { UnitType.Worker, UnitType.Infantry, UnitType.Archer, UnitType.Knight, UnitType.Mage, UnitType.Siege };
            int row = 0;
            int col = 0;

            foreach (var unitType in units)
            {
                float ux = x + col * 130;
                float uy = y + 30 + row * 80;

                var stats = GetUnitStats(unitType);
                var previewUnit = new Unit(unitType, new Vector2(ux + 60, uy + 20), true);
                Color color = previewUnit.GetUnitColor();
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)));

                context.DrawEllipse(brush, new Pen(System.Windows.Media.Brushes.Black, 1), new Point(ux + 60, uy + 20), 15, 15);

                var nameText = new FormattedText(unitType.ToString(), System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 10, Brushes.White, 1.0);
                context.DrawText(nameText, new Point(ux + 5, uy + 40));

                var costText = new FormattedText($"{stats.Cost}g", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 9, Brushes.Yellow, 1.0);
                context.DrawText(costText, new Point(ux + 5, uy + 55));

                col++;
                if (col >= 3)
                {
                    col = 0;
                    row++;
                }
            }
        }

        private void DrawBuildingTypePreview(DrawingContext context, float x, float y)
        {
            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 30, 35, 45));
            context.DrawRectangle(panelBrush, null, new Rect(x - 20, y - 20, 380, 280));

            var title = new FormattedText("Buildings", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 16, Brushes.White, 1.0);
            context.DrawText(title, new Point(x, y));

            var buildings = new[] { BuildingType.TownCenter, BuildingType.Barracks, BuildingType.Tower, BuildingType.Farm, BuildingType.Mine, BuildingType.LumberMill };
            int row = 0;
            int col = 0;

            foreach (var buildingType in buildings)
            {
                float ux = x + col * 130;
                float uy = y + 30 + row * 80;

                var stats = GetBuildingStats(buildingType);
                var previewBuilding = new Building(buildingType, new Vector2(ux + 60, uy + 20), true);
                Color color = previewBuilding.GetBuildingColor();
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromRgb((byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)));

                context.DrawRectangle(brush, new Pen(System.Windows.Media.Brushes.Black, 1), new Rect(ux + 40, uy + 5, 40, 35));

                var nameText = new FormattedText(buildingType.ToString(), System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 9, Brushes.White, 1.0);
                context.DrawText(nameText, new Point(ux + 5, uy + 45));

                var costText = new FormattedText($"{stats.GoldCost}g", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 8, Brushes.Yellow, 1.0);
                context.DrawText(costText, new Point(ux + 5, uy + 58));

                col++;
                if (col >= 3)
                {
                    col = 0;
                    row++;
                }
            }
        }

        private void DrawPauseMenu(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(180, 0, 0, 0)), null, new Rect(0, 0, 1920, 1080));

            var panelBrush = new SolidColorBrush(System.Windows.Media.Color.FromRgb(40, 45, 55));
            context.DrawRectangle(panelBrush, null, new Rect(760, 284, 400, 312));

            var title = new FormattedText("PAUSED", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 48, Brushes.White, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 320));

            var resumeText = new FormattedText("Press ESC to Resume", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, System.Windows.Media.Brushes.LightGray, 1.0);
            context.DrawText(resumeText, new Point(960 - resumeText.Width / 2, 400));

            DrawGameStats(context, 800, 450);

            var quitText = new FormattedText("Press R to Restart", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 20, System.Windows.Media.Brushes.Gray, 1.0);
            context.DrawText(quitText, new Point(960 - quitText.Width / 2, 550));
        }

        private void DrawGameOverScreen(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 30, 0, 0)), null, new Rect(0, 0, 1920, 1080));

            var title = new FormattedText("DEFEAT", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 72, Brushes.Red, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 200));

            var scoreText = new FormattedText($"Final Score: {_score}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 32, Brushes.White, 1.0);
            context.DrawText(scoreText, new Point(960 - scoreText.Width / 2, 300));

            var timeText = new FormattedText($"Time Survived: {FormatTime(_gameTime)}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, Brushes.LightGray, 1.0);
            context.DrawText(timeText, new Point(960 - timeText.Width / 2, 350));

            DrawGameStats(context, 760, 420);

            var restartText = new FormattedText("Press R to Restart", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 28, Brushes.LightBlue, 1.0);
            context.DrawText(restartText, new Point(960 - restartText.Width / 2, 700));
        }

        private void DrawVictoryScreen(DrawingContext context)
        {
            context.DrawRectangle(new SolidColorBrush(System.Windows.Media.Color.FromArgb(220, 0, 30, 0)), null, new Rect(0, 0, 1920, 1080));

            var title = new FormattedText("VICTORY!", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 72, Brushes.Gold, 1.0);
            context.DrawText(title, new Point(960 - title.Width / 2, 200));

            var scoreText = new FormattedText($"Final Score: {_score}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 32, Brushes.White, 1.0);
            context.DrawText(scoreText, new Point(960 - scoreText.Width / 2, 300));

            var timeText = new FormattedText($"Time: {FormatTime(_gameTime)}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 24, Brushes.LightGray, 1.0);
            context.DrawText(timeText, new Point(960 - timeText.Width / 2, 350));

            var unitsText = new FormattedText($"Units Trained: {_playerUnits.Count}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 18, Brushes.LightBlue, 1.0);
            context.DrawText(unitsText, new Point(960 - unitsText.Width / 2, 390));

            DrawGameStats(context, 760, 450);

            var restartText = new FormattedText("Press R to Play Again", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 28, Brushes.LightBlue, 1.0);
            context.DrawText(restartText, new Point(960 - restartText.Width / 2, 750));
        }

        private void DrawGameStats(DrawingContext context, float x, float y)
        {
            var statsTitle = new FormattedText("Statistics", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 18, Brushes.Orange, 1.0);
            context.DrawText(statsTitle, new Point(960 - statsTitle.Width / 2, y));

            y += 30;
            var stats = new[]
            {
                $"Player Units: {_playerUnits.Count}",
                $"Enemy Units: {_enemyUnits.Count}",
                $"Player Buildings: {_playerBuildings.Count}",
                $"Enemy Buildings: {_enemyBuildings.Count}",
                $"Gold: {_gold}",
                $"Wood: {_wood}",
                $"Food: {_food}"
            };

            foreach (var stat in stats)
            {
                var statText = new FormattedText(stat, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, new Typeface("Arial"), 14, System.Windows.Media.Brushes.LightGray, 1.0);
                context.DrawText(statText, new Point(x, y));
                y += 22;
            }
        }

        private string FormatTime(float seconds)
        {
            int mins = (int)(seconds / 60);
            int secs = (int)(seconds % 60);
            return $"{mins:D2}:{secs:D2}";
        }

        public override void Dispose()
        {
            _playerUnits.Clear();
            _enemyUnits.Clear();
            _playerBuildings.Clear();
            _enemyBuildings.Clear();
            _resourceNodes.Clear();
            _effects.Clear();
            _selectedUnits.Clear();
            base.Dispose();
        }
    }

    public abstract class VisualEffect
    {
        public Vector2 Position { get; set; }
        public float Lifetime { get; set; }
        public float MaxLifetime { get; set; }
        public bool IsExpired => Lifetime <= 0;

        protected VisualEffect(Vector2 position, float duration)
        {
            Position = position;
            Lifetime = duration;
            MaxLifetime = duration;
        }

        public virtual void Update(float deltaTime)
        {
            Lifetime -= deltaTime;
        }

        public abstract void Render(DrawingContext context, RTSCamera camera);
    }

    public class ParticleEffect : VisualEffect
    {
        public Vector2 Velocity { get; set; }
        public Color Color { get; set; }

        public ParticleEffect(Vector2 position, Vector2 velocity, Color color, float duration)
            : base(position, duration)
        {
            Velocity = velocity;
            Color = color;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            Position = Position + Velocity * deltaTime;
            Velocity = Velocity * 0.95f;
        }

        public override void Render(DrawingContext context, RTSCamera camera)
        {
            float alpha = Lifetime / MaxLifetime;
            Vector2 screenPos = camera.WorldToScreen(Position);

            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(alpha * 255),
                (byte)(Color.R * 255), (byte)(Color.G * 255), (byte)(Color.B * 255)));

            context.DrawEllipse(brush, null, new Point(screenPos.X, screenPos.Y), 3 * camera.Zoom, 3 * camera.Zoom);
        }
    }

    public class MoveIndicatorEffect : VisualEffect
    {
        public MoveIndicatorEffect(Vector2 position, float duration)
            : base(position, duration)
        {
        }

        public override void Render(DrawingContext context, RTSCamera camera)
        {
            float progress = 1f - Lifetime / MaxLifetime;
            float alpha = 1f - progress;
            Vector2 screenPos = camera.WorldToScreen(Position);

            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(alpha * 200), 100, 255, 100));
            float size = 15 * camera.Zoom * progress;

            context.DrawEllipse(brush, null, new Point(screenPos.X, screenPos.Y), size, size);

            if (progress < 0.5f)
            {
                var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(alpha * 255), 100, 255, 100)), 2);
                float innerSize = size * 0.5f;
                context.DrawLine(pen, new Point(screenPos.X - innerSize, screenPos.Y), new Point(screenPos.X + innerSize, screenPos.Y));
                context.DrawLine(pen, new Point(screenPos.X, screenPos.Y - innerSize), new Point(screenPos.X, screenPos.Y + innerSize));
            }
        }
    }

    public class DamageNumber : VisualEffect
    {
        public float Damage { get; }
        private float _offset;

        public DamageNumber(Vector2 position, float damage)
            : base(position, 1f)
        {
            Damage = damage;
            _offset = 0;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            _offset += deltaTime * 30;
            Position = Position + new Vector2(0, -_offset);
        }

        public override void Render(DrawingContext context, RTSCamera camera)
        {
            float alpha = Lifetime / MaxLifetime;
            Vector2 screenPos = camera.WorldToScreen(Position);

            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(alpha * 255), 255, 100, 100));
            var text = new FormattedText($"{(int)Damage}", System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, new Typeface("Arial Bold"), 14 * camera.Zoom, brush, 1.0);
            context.DrawText(text, new Point(screenPos.X - text.Width / 2, screenPos.Y));
        }
    }

    public class AttackEffect : VisualEffect
    {
        public Vector2 Target { get; set; }
        public Color Color { get; set; }

        public AttackEffect(Vector2 from, Vector2 to, Color color, float duration)
            : base(from, duration)
        {
            Target = to;
            Color = color;
        }

        public override void Render(DrawingContext context, RTSCamera camera)
        {
            float progress = 1f - Lifetime / MaxLifetime;
            float alpha = 1f - progress;

            Vector2 start = camera.WorldToScreen(Position);
            Vector2 end = camera.WorldToScreen(Target);
            Vector2 current = Vector2.Lerp(start, end, progress);

            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(alpha * 255),
                (byte)(Color.R * 255), (byte)(Color.G * 255), (byte)(Color.B * 255)));

            context.DrawEllipse(brush, null, new Point(current.X, current.Y), 5 * camera.Zoom * (1 - progress * 0.5f), 5 * camera.Zoom * (1 - progress * 0.5f));
        }
    }

    public static class MathHelper
    {
        public const float PiOver2 = (float)(Math.PI / 2);
        public const float TwoPi = (float)(Math.PI * 2);
    }
}

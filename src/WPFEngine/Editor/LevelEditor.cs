using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WPFEngine.ECS;
using WPFEngine.Math;
using WPFEngine.Scene;

namespace WPFEngine.Editor
{
    public enum EditorTool
    {
        Select,
        Move,
        Rotate,
        Scale,
        TileBrush,
        TerrainBrush,
        EntityPlace,
        WaterBrush,
        DecorationBrush,
        SpawnPoint,
        TriggerZone,
        PathNode,
        LightPlace,
        Eraser,
        Fill,
        RectangleSelect
    }

    public enum TileLayer
    {
        Background,
        Collision,
        Interactive,
        Decoration
    }

    public enum TileBrushType
    {
        Single,
        Fill,
        Rectangle,
        Random
    }

    public class TileInfo
    {
        public int TileId { get; set; }
        public string TileName { get; set; }
        public Brush TileBrush { get; set; }
        public string TexturePath { get; set; }
    }

    public class TileData
    {
        public int TileId { get; set; }
        public TileLayer Layer { get; set; }
        public int GridX { get; set; }
        public int GridY { get; set; }
    }

    public class WorldBuilderMarker
    {
        public string Id { get; set; }
        public string MarkerType { get; set; }
        public Vector2 Position { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public GameObject LinkedGameObject { get; set; }

        public WorldBuilderMarker(string type, Vector2 position)
        {
            Id = Guid.NewGuid().ToString();
            MarkerType = type;
            Position = position;
            Properties = new Dictionary<string, object>();
        }
    }

    public class EditorAction
    {
        public string Description { get; set; }
        public Action Undo { get; set; }
        public Action Redo { get; set; }
    }

    public class UndoRedoSystem
    {
        private readonly Stack<EditorAction> undoStack = new();
        private readonly Stack<EditorAction> redoStack = new();
        private const int MaxHistorySize = 100;

        public event EventHandler<bool> UndoRedoStateChanged;

        public bool CanUndo => undoStack.Count > 0;
        public bool CanRedo => redoStack.Count > 0;

        public void Execute(EditorAction action)
        {
            action.Redo?.Invoke();
            undoStack.Push(action);
            redoStack.Clear();

            if (undoStack.Count > MaxHistorySize)
            {
                var tempStack = new Stack<EditorAction>();
                for (int i = 0; i < MaxHistorySize; i++)
                {
                    tempStack.Push(undoStack.Pop());
                }
                undoStack.Clear();
                while (tempStack.Count > 0)
                {
                    undoStack.Push(tempStack.Pop());
                }
            }

            UndoRedoStateChanged?.Invoke(this, CanUndo);
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var action = undoStack.Pop();
            action.Undo?.Invoke();
            redoStack.Push(action);
            UndoRedoStateChanged?.Invoke(this, CanUndo);
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var action = redoStack.Pop();
            action.Redo?.Invoke();
            undoStack.Push(action);
            UndoRedoStateChanged?.Invoke(this, CanUndo);
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
            UndoRedoStateChanged?.Invoke(this, false);
        }
    }

    public class EditorSettings
    {
        public float GridSize { get; set; } = 32f;
        public float SnapThreshold { get; set; } = 8f;
        public bool SnapToGrid { get; set; } = true;
        public bool ShowGrid { get; set; } = true;
        public bool ShowCollisionLayer { get; set; } = true;
        public float MinZoom { get; set; } = 0.1f;
        public float MaxZoom { get; set; } = 10f;
        public float DefaultZoom { get; set; } = 1f;
        public Color GridColor { get; set; } = Color.FromArgb(40, 255, 255, 255);
        public Color SelectionColor { get; set; } = Color.FromRgb(30, 130, 230);
        public int UndoHistorySize { get; set; } = 100;
        public bool AutoSaveEnabled { get; set; } = true;
        public int AutoSaveIntervalSeconds { get; set; } = 300;
        public string DefaultTilePalette { get; set; } = "Default";
    }

    public class EditorCamera
    {
        private Vector2 position = Vector2.Zero;
        private float zoom = 1f;
        private float rotation = 0f;
        private float minZoom = 0.1f;
        private float maxZoom = 10f;

        public Vector2 Position
        {
            get => position;
            set => position = value;
        }

        public float Zoom
        {
            get => zoom;
            set => zoom = Math.Clamp(value, minZoom, maxZoom);
        }

        public float Rotation
        {
            get => rotation;
            set => rotation = value;
        }

        public float MinZoom
        {
            get => minZoom;
            set => minZoom = value;
        }

        public float MaxZoom
        {
            get => maxZoom;
            set => maxZoom = value;
        }

        public void Pan(Vector2 delta)
        {
            position += delta / zoom;
        }

        public void ZoomAt(Vector2 worldPoint, float delta)
        {
            var oldZoom = zoom;
            zoom = Math.Clamp(zoom * (1 + delta * 0.1f), minZoom, maxZoom);

            var zoomRatio = zoom / oldZoom;
            position = worldPoint - (worldPoint - position) * zoomRatio;
        }

        public Vector2 ScreenToWorld(Vector2 screenPoint)
        {
            return (screenPoint / zoom) + position;
        }

        public Vector2 WorldToScreen(Vector2 worldPoint)
        {
            return (worldPoint - position) * zoom;
        }

        public void Reset()
        {
            position = Vector2.Zero;
            zoom = 1f;
            rotation = 0f;
        }

        public void FitToBounds(Bounds bounds, Vector2 viewportSize)
        {
            if (bounds.Size.X <= 0 || bounds.Size.Y <= 0) return;

            var scaleX = viewportSize.X / bounds.Size.X;
            var scaleY = viewportSize.Y / bounds.Size.Y;
            zoom = Math.Min(scaleX, scaleY) * 0.9f;
            zoom = Math.Clamp(zoom, minZoom, maxZoom);

            position = bounds.Center - (viewportSize / (2 * zoom));
        }
    }

    public class SelectionSystem
    {
        private readonly List<GameObject> selectedObjects = new();
        private readonly List<GameObject> clipboard = new();
        private Rect marqueeRect;
        private bool isMarqueeActive;
        private Vector2 marqueeStart;

        public event EventHandler<List<GameObject>> SelectionChanged;
        public event EventHandler<Rect> MarqueeChanged;

        public IReadOnlyList<GameObject> SelectedObjects => selectedObjects.AsReadOnly();
        public bool IsMarqueeActive => isMarqueeActive;
        public Rect MarqueeRect => marqueeRect;

        public void Select(GameObject obj)
        {
            if (obj == null) return;
            ClearSelection();
            selectedObjects.Add(obj);
            SelectionChanged?.Invoke(this, selectedObjects);
        }

        public void AddToSelection(GameObject obj)
        {
            if (obj == null || selectedObjects.Contains(obj)) return;
            selectedObjects.Add(obj);
            SelectionChanged?.Invoke(this, selectedObjects);
        }

        public void RemoveFromSelection(GameObject obj)
        {
            if (obj == null || !selectedObjects.Contains(obj)) return;
            selectedObjects.Remove(obj);
            SelectionChanged?.Invoke(this, selectedObjects);
        }

        public void ToggleSelection(GameObject obj)
        {
            if (selectedObjects.Contains(obj))
                RemoveFromSelection(obj);
            else
                AddToSelection(obj);
        }

        public void ClearSelection()
        {
            selectedObjects.Clear();
            SelectionChanged?.Invoke(this, selectedObjects);
        }

        public void StartMarquee(Vector2 start)
        {
            isMarqueeActive = true;
            marqueeStart = start;
            marqueeRect = new Rect(start.X, start.Y, 0, 0);
            MarqueeChanged?.Invoke(this, marqueeRect);
        }

        public void UpdateMarquee(Vector2 current)
        {
            if (!isMarqueeActive) return;

            var x = Math.Min(marqueeStart.X, current.X);
            var y = Math.Min(marqueeStart.Y, current.Y);
            var width = Math.Abs(current.X - marqueeStart.X);
            var height = Math.Abs(current.Y - marqueeStart.Y);

            marqueeRect = new Rect(x, y, width, height);
            MarqueeChanged?.Invoke(this, marqueeRect);
        }

        public void EndMarquee(List<GameObject> allObjects)
        {
            if (!isMarqueeActive) return;

            isMarqueeActive = false;
            selectedObjects.Clear();

            foreach (var obj in allObjects)
            {
                if (obj.Transform != null)
                {
                    var objBounds = obj.Transform.GetWorldBounds();
                    if (marqueeRect.Contains(new Point(objBounds.Center.X, objBounds.Center.Y)))
                    {
                        selectedObjects.Add(obj);
                    }
                }
            }

            SelectionChanged?.Invoke(this, selectedObjects);
        }

        public void Copy()
        {
            clipboard.Clear();
            foreach (var obj in selectedObjects)
            {
                clipboard.Add(obj.Clone());
            }
        }

        public List<GameObject> Paste(Vector2 position)
        {
            var pasted = new List<GameObject>();
            foreach (var obj in clipboard)
            {
                var clone = obj.Clone();
                clone.Transform.LocalPosition += position;
                pasted.Add(clone);
            }
            return pasted;
        }

        public List<GameObject> Duplicate(Vector2 offset)
        {
            var duplicated = new List<GameObject>();
            foreach (var obj in selectedObjects)
            {
                var clone = obj.Clone();
                clone.Transform.LocalPosition += offset;
                duplicated.Add(clone);
            }
            return duplicated;
        }

        public bool HasClipboard => clipboard.Count > 0;
    }

    public class TileMapEditor
    {
        private readonly Dictionary<TileLayer, Dictionary<Vector2Int, TileData>> tileMaps = new();
        private readonly ObservableCollection<TileInfo> tilePalette = new();
        private TileInfo selectedTile;
        private TileLayer currentLayer = TileLayer.Background;
        private TileBrushType brushType = TileBrushType.Single;
        private readonly Random random = new();

        public ObservableCollection<TileInfo> TilePalette => tilePalette;
        public TileInfo SelectedTile
        {
            get => selectedTile;
            set => selectedTile = value;
        }

        public TileLayer CurrentLayer
        {
            get => currentLayer;
            set => currentLayer = value;
        }

        public TileBrushType BrushType
        {
            get => brushType;
            set => brushType = value;
        }

        public TileMapEditor()
        {
            foreach (TileLayer layer in Enum.GetValues(typeof(TileLayer)))
            {
                tileMaps[layer] = new Dictionary<Vector2Int, TileData>();
            }
        }

        public void AddTile(int gridX, int gridY)
        {
            if (selectedTile == null) return;

            var key = new Vector2Int(gridX, gridY);
            var tile = new TileData
            {
                TileId = selectedTile.TileId,
                Layer = currentLayer,
                GridX = gridX,
                GridY = gridY
            };

            tileMaps[currentLayer][key] = tile;
        }

        public void RemoveTile(int gridX, int gridY)
        {
            var key = new Vector2Int(gridX, gridY);
            tileMaps[currentLayer].Remove(key);
        }

        public TileData GetTile(int gridX, int gridY, TileLayer layer)
        {
            var key = new Vector2Int(gridX, gridY);
            tileMaps.TryGetValue(layer, out var layerTiles);
            layerTiles?.TryGetValue(key, out var tile);
            return tile;
        }

        public Dictionary<Vector2Int, TileData> GetTilesForLayer(TileLayer layer)
        {
            return tileMaps.TryGetValue(layer, out var tiles) ? tiles : new Dictionary<Vector2Int, TileData>();
        }

        public void Fill(int startX, int startY)
        {
            if (selectedTile == null) return;

            var targetTile = GetTile(startX, startY, currentLayer);
            var targetId = targetTile?.TileId ?? -1;

            if (targetId == selectedTile.TileId) return;

            var stack = new Stack<Vector2Int>();
            var visited = new HashSet<Vector2Int>();
            stack.Push(new Vector2Int(startX, startY));

            while (stack.Count > 0)
            {
                var pos = stack.Pop();
                if (visited.Contains(pos)) continue;

                var current = GetTile(pos.X, pos.Y, currentLayer);
                var currentId = current?.TileId ?? -1;

                if (currentId != targetId) continue;

                visited.Add(pos);
                AddTile(pos.X, pos.Y);

                stack.Push(new Vector2Int(pos.X + 1, pos.Y));
                stack.Push(new Vector2Int(pos.X - 1, pos.Y));
                stack.Push(new Vector2Int(pos.X, pos.Y + 1));
                stack.Push(new Vector2Int(pos.X, pos.Y - 1));
            }
        }

        public void FillRectangle(int x1, int y1, int x2, int y2)
        {
            if (selectedTile == null) return;

            var minX = Math.Min(x1, x2);
            var maxX = Math.Max(x1, x2);
            var minY = Math.Min(y1, y2);
            var maxY = Math.Max(y1, y2);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    AddTile(x, y);
                }
            }
        }

        public void PlaceRandom(int gridX, int gridY, int width, int height)
        {
            if (selectedTile == null) return;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (random.NextDouble() > 0.5)
                    {
                        AddTile(gridX + x, gridY + y);
                    }
                }
            }
        }

        public void AddTileToPalette(TileInfo tile)
        {
            tilePalette.Add(tile);
        }

        public void Clear()
        {
            foreach (var layer in tileMaps.Values)
            {
                layer.Clear();
            }
        }
    }

    public class EntityPlacer
    {
        private readonly ObservableCollection<GameObject> prefabs = new();
        private GameObject selectedPrefab;
        private Vector2 placementOffset = Vector2.Zero;

        public ObservableCollection<GameObject> Prefabs => prefabs;
        public GameObject SelectedPrefab
        {
            get => selectedPrefab;
            set => selectedPrefab = value;
        }

        public Vector2 PlacementOffset
        {
            get => placementOffset;
            set => placementOffset = value;
        }

        public event EventHandler<GameObject> EntityPlaced;

        public GameObject PlaceEntity(Vector2 position, SceneObject scene)
        {
            if (selectedPrefab == null) return null;

            var entity = selectedPrefab.Clone();
            entity.Transform.LocalPosition = position + placementOffset;
            scene.AddGameObject(entity);
            EntityPlaced?.Invoke(this, entity);
            return entity;
        }

        public void AddPrefab(GameObject prefab)
        {
            if (!prefabs.Contains(prefab))
            {
                prefabs.Add(prefab);
            }
        }

        public void RemovePrefab(GameObject prefab)
        {
            prefabs.Remove(prefab);
            if (selectedPrefab == prefab)
            {
                selectedPrefab = null;
            }
        }
    }

    public class WorldBuilderSystem
    {
        private readonly ObservableCollection<WorldBuilderMarker> markers = new();
        private WorldBuilderMarker selectedMarker;
        private readonly Dictionary<string, List<WorldBuilderMarker>> markersByType = new();

        public ObservableCollection<WorldBuilderMarker> Markers => markers;
        public WorldBuilderMarker SelectedMarker
        {
            get => selectedMarker;
            set => selectedMarker = value;
        }

        public WorldBuilderMarker AddMarker(string type, Vector2 position)
        {
            var marker = new WorldBuilderMarker(type, position);
            markers.Add(marker);

            if (!markersByType.ContainsKey(type))
            {
                markersByType[type] = new List<WorldBuilderMarker>();
            }
            markersByType[type].Add(marker);

            return marker;
        }

        public void RemoveMarker(WorldBuilderMarker marker)
        {
            markers.Remove(marker);
            markersByType[marker.MarkerType]?.Remove(marker);
            if (selectedMarker == marker)
            {
                selectedMarker = null;
            }
        }

        public List<WorldBuilderMarker> GetMarkersByType(string type)
        {
            return markersByType.TryGetValue(type, out var list) ? list : new List<WorldBuilderMarker>();
        }

        public void ClearMarkers()
        {
            markers.Clear();
            markersByType.Clear();
            selectedMarker = null;
        }
    }

    public class EditorConsole
    {
        private readonly ObservableCollection<string> logMessages = new();
        private readonly List<string> commandHistory = new();
        private int historyIndex = -1;
        private const int MaxMessages = 1000;

        public ObservableCollection<string> LogMessages => logMessages;

        public event EventHandler<string> CommandExecuted;
        public event EventHandler<string> MessageLogged;

        public void Log(string message)
        {
            AddMessage($"[LOG] {message}");
        }

        public void LogWarning(string message)
        {
            AddMessage($"[WARN] {message}");
        }

        public void LogError(string message)
        {
            AddMessage($"[ERROR] {message}");
        }

        public void LogInfo(string message)
        {
            AddMessage($"[INFO] {message}");
        }

        public void LogDebug(string message)
        {
            AddMessage($"[DEBUG] {message}");
        }

        private void AddMessage(string message)
        {
            if (logMessages.Count >= MaxMessages)
            {
                logMessages.RemoveAt(0);
            }
            logMessages.Add(message);
            MessageLogged?.Invoke(this, message);
        }

        public bool ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return false;

            commandHistory.Add(command);
            historyIndex = commandHistory.Count;
            CommandExecuted?.Invoke(this, command);
            return true;
        }

        public string GetPreviousCommand()
        {
            if (historyIndex > 0)
            {
                historyIndex--;
                return commandHistory[historyIndex];
            }
            return "";
        }

        public string GetNextCommand()
        {
            if (historyIndex < commandHistory.Count - 1)
            {
                historyIndex++;
                return commandHistory[historyIndex];
            }
            historyIndex = commandHistory.Count;
            return "";
        }

        public void Clear()
        {
            logMessages.Clear();
        }
    }

    public class EditorHierarchy : TreeView
    {
        private readonly Dictionary<GameObject, TreeViewItem> itemMap = new();
        private GameObject rootObject;

        public event EventHandler<GameObject> GameObjectSelected;

        public GameObject RootObject
        {
            get => rootObject;
            set
            {
                rootObject = value;
                Refresh();
            }
        }

        public EditorHierarchy()
        {
            SelectionChanged += OnSelectionChanged;
        }

        private void OnSelectionChanged(object sender, RoutedEventArgs e)
        {
            if (SelectedItem is TreeViewItem item && item.Tag is GameObject obj)
            {
                GameObjectSelected?.Invoke(this, obj);
            }
        }

        public void Refresh()
        {
            Items.Clear();
            itemMap.Clear();

            if (rootObject == null) return;

            var rootItem = CreateItem(rootObject);
            Items.Add(rootItem);
            rootItem.IsExpanded = true;
        }

        private TreeViewItem CreateItem(GameObject gameObject)
        {
            var item = new TreeViewItem
            {
                Header = CreateHeader(gameObject),
                Tag = gameObject,
                IsExpanded = true
            };

            itemMap[gameObject] = item;

            foreach (var child in gameObject.Children)
            {
                var childItem = CreateItem(child);
                item.Items.Add(childItem);
            }

            gameObject.ChildAdded += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (itemMap.TryGetValue(gameObject, out var parentItem))
                    {
                        var newChildItem = CreateItem(e.GameObject);
                        parentItem.Items.Add(newChildItem);
                    }
                });
            };

            gameObject.ChildRemoved += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (itemMap.TryGetValue(e.GameObject, out var childItem))
                    {
                        if (itemMap.TryGetValue(gameObject, out var parentItem))
                        {
                            parentItem.Items.Remove(childItem);
                        }
                        itemMap.Remove(e.GameObject);
                    }
                });
            };

            return item;
        }

        private StackPanel CreateHeader(GameObject gameObject)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var icon = new Ellipse
            {
                Width = 8,
                Height = 8,
                Fill = gameObject.IsActive ? Brushes.LightGreen : Brushes.Gray,
                Margin = new Thickness(0, 0, 4, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var name = new TextBlock
            {
                Text = gameObject.Name,
                Foreground = gameObject.IsActive ? Brushes.White : Brushes.Gray
            };

            panel.Children.Add(icon);
            panel.Children.Add(name);

            gameObject.ActiveChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    icon.Fill = gameObject.IsActive ? Brushes.LightGreen : Brushes.Gray;
                    name.Foreground = gameObject.IsActive ? Brushes.White : Brushes.Gray;
                });
            };

            return panel;
        }

        public void SelectGameObject(GameObject gameObject)
        {
            if (itemMap.TryGetValue(gameObject, out var item))
            {
                item.IsSelected = true;
            }
        }
    }

    public class EditorInspector : StackPanel
    {
        private GameObject selectedObject;
        private readonly StackPanel propertiesPanel;
        private readonly TextBlock headerLabel;

        public event EventHandler<GameObject> ObjectModified;

        public GameObject SelectedObject
        {
            get => selectedObject;
            set
            {
                selectedObject = value;
                Refresh();
            }
        }

        public EditorInspector()
        {
            Orientation = Orientation.Vertical;
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));

            headerLabel = new TextBlock
            {
                Text = "Inspector",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(10, 10, 10, 5)
            };
            Children.Add(headerLabel);

            var separator = new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Margin = new Thickness(0, 5, 0, 10)
            };
            Children.Add(separator);

            propertiesPanel = new StackPanel { Orientation = Orientation.Vertical };
            Children.Add(propertiesPanel);
        }

        public void Refresh()
        {
            propertiesPanel.Children.Clear();

            if (selectedObject == null)
            {
                headerLabel.Text = "Inspector";
                return;
            }

            headerLabel.Text = selectedObject.Name;

            AddTransformSection();
            AddComponentSection();
        }

        private void AddTransformSection()
        {
            if (selectedObject.Transform == null) return;

            var sectionHeader = CreateSectionHeader("Transform");
            propertiesPanel.Children.Add(sectionHeader);

            var transform = selectedObject.Transform;

            AddPropertyRow("Position X", transform.LocalPosition.X.ToString("F2"), (s, e) =>
            {
                if (float.TryParse((s as TextBox)?.Text, out var val))
                    transform.LocalPosition = new Vector2(val, transform.LocalPosition.Y);
            });

            AddPropertyRow("Position Y", transform.LocalPosition.Y.ToString("F2"), (s, e) =>
            {
                if (float.TryParse((s as TextBox)?.Text, out var val))
                    transform.LocalPosition = new Vector2(transform.LocalPosition.X, val);
            });

            AddPropertyRow("Rotation", transform.LocalRotation.ToString("F1"), (s, e) =>
            {
                if (float.TryParse((s as TextBox)?.Text, out var val))
                    transform.LocalRotation = val;
            });

            AddPropertyRow("Scale X", transform.LocalScale.X.ToString("F2"), (s, e) =>
            {
                if (float.TryParse((s as TextBox)?.Text, out var val))
                    transform.LocalScale = new Vector2(val, transform.LocalScale.Y);
            });

            AddPropertyRow("Scale Y", transform.LocalScale.Y.ToString("F2"), (s, e) =>
            {
                if (float.TryParse((s as TextBox)?.Text, out var val))
                    transform.LocalScale = new Vector2(transform.LocalScale.X, val);
            });
        }

        private void AddComponentSection()
        {
            var sectionHeader = CreateSectionHeader("Components");
            propertiesPanel.Children.Add(sectionHeader);

            foreach (var component in selectedObject.GetComponents<Component>())
            {
                if (component is Transform) continue;

                var componentLabel = new TextBlock
                {
                    Text = component.GetType().Name,
                    Foreground = Brushes.LightBlue,
                    Margin = new Thickness(10, 5, 10, 2),
                    FontSize = 11
                };
                propertiesPanel.Children.Add(componentLabel);
            }
        }

        private TextBlock CreateSectionHeader(string title)
        {
            return new TextBlock
            {
                Text = title,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(10, 10, 10, 5)
            };
        }

        private void AddPropertyRow(string label, string value, EventHandler<RoutedEventArgs> valueChanged)
        {
            var row = new Grid { Margin = new Thickness(10, 2, 10, 2) };
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(80) });
            row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var labelBlock = new TextBlock
            {
                Text = label,
                Foreground = Brushes.Gray,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(labelBlock, 0);

            var textBox = new TextBox
            {
                Text = value,
                Background = new SolidColorBrush(Color.FromRgb(55, 55, 58)),
                Foreground = Brushes.White,
                BorderBrush = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                Padding = new Thickness(4, 2, 4, 2),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            textBox.TextChanged += (s, e) =>
            {
                valueChanged?.Invoke(s, e);
                ObjectModified?.Invoke(this, selectedObject);
            };
            Grid.SetColumn(textBox, 1);

            row.Children.Add(labelBlock);
            row.Children.Add(textBox);
            propertiesPanel.Children.Add(row);
        }
    }

    public class EditorToolbar : StackPanel
    {
        private readonly Dictionary<EditorTool, ToggleButton> toolButtons = new();
        private EditorTool currentTool = EditorTool.Select;

        public event EventHandler<EditorTool> ToolChanged;

        public EditorTool CurrentTool
        {
            get => currentTool;
            set
            {
                if (currentTool != value)
                {
                    currentTool = value;
                    UpdateButtonStates();
                    ToolChanged?.Invoke(this, value);
                }
            }
        }

        public EditorToolbar()
        {
            Orientation = Orientation.Horizontal;
            Background = new SolidColorBrush(Color.FromRgb(55, 55, 58));

            AddTool(EditorTool.Select, "Select", "S");
            AddTool(EditorTool.Move, "Move", "V");
            AddTool(EditorTool.Rotate, "Rotate", "R");
            AddTool(EditorTool.Scale, "Scale", "E");
            AddSeparator();
            AddTool(EditorTool.TileBrush, "Tile Brush", "T");
            AddTool(EditorTool.TerrainBrush, "Terrain Brush", "G");
            AddTool(EditorTool.WaterBrush, "Water Brush", "W");
            AddTool(EditorTool.DecorationBrush, "Decoration", "D");
            AddSeparator();
            AddTool(EditorTool.EntityPlace, "Place Entity", "P");
            AddTool(EditorTool.SpawnPoint, "Spawn Point", "M");
            AddTool(EditorTool.TriggerZone, "Trigger Zone", "Z");
            AddTool(EditorTool.PathNode, "Path Node", "N");
            AddTool(EditorTool.LightPlace, "Place Light", "L");
            AddSeparator();
            AddTool(EditorTool.Eraser, "Eraser", "X");
            AddTool(EditorTool.Fill, "Fill", "F");
            AddTool(EditorTool.RectangleSelect, "Marquee", "Q");
        }

        private void AddTool(EditorTool tool, string tooltip, string shortcut)
        {
            var button = new ToggleButton
            {
                Content = GetToolIcon(tool),
                ToolTip = $"{tooltip} ({shortcut})",
                Width = 32,
                Height = 32,
                Margin = new Thickness(2),
                Tag = tool
            };

            button.Click += (s, e) => CurrentTool = tool;
            toolButtons[tool] = button;
            Children.Add(button);
        }

        private void AddSeparator()
        {
            var separator = new Rectangle
            {
                Width = 1,
                Height = 24,
                Fill = new SolidColorBrush(Color.FromRgb(80, 80, 80)),
                Margin = new Thickness(5, 0, 5, 0),
                VerticalAlignment = VerticalAlignment.Center
            };
            Children.Add(separator);
        }

        private FrameworkElement GetToolIcon(EditorTool tool)
        {
            switch (tool)
            {
                case EditorTool.Select:
                    return CreateIcon("\u2190");
                case EditorTool.Move:
                    return CreateIcon("\u2194");
                case EditorTool.Rotate:
                    return CreateIcon("\u21BB");
                case EditorTool.Scale:
                    return CreateIcon("\u2195");
                case EditorTool.TileBrush:
                    return CreateIcon("\u25A1");
                case EditorTool.TerrainBrush:
                    return CreateIcon("\u25CE");
                case EditorTool.EntityPlace:
                    return CreateIcon("\u25A3");
                case EditorTool.WaterBrush:
                    return CreateIcon("\u2248");
                case EditorTool.DecorationBrush:
                    return CreateIcon("\u2666");
                case EditorTool.SpawnPoint:
                    return CreateIcon("\u2605");
                case EditorTool.TriggerZone:
                    return CreateIcon("\u25AD");
                case EditorTool.PathNode:
                    return CreateIcon("\u25CF");
                case EditorTool.LightPlace:
                    return CreateIcon("\u2600");
                case EditorTool.Eraser:
                    return CreateIcon("\u2715");
                case EditorTool.Fill:
                    return CreateIcon("\u224B");
                case EditorTool.RectangleSelect:
                    return CreateIcon("\u25A9");
                default:
                    return CreateIcon("?");
            }
        }

        private FrameworkElement CreateIcon(string symbol)
        {
            return new TextBlock
            {
                Text = symbol,
                FontSize = 16,
                Foreground = Brushes.White,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        private void UpdateButtonStates()
        {
            foreach (var kvp in toolButtons)
            {
                kvp.Value.IsChecked = kvp.Key == currentTool;
            }
        }
    }

    public class TilePalettePanel : StackPanel
    {
        private readonly ListBox tileList;
        private readonly ObservableCollection<TileInfo> tiles = new();
        private TileInfo selectedTile;

        public event EventHandler<TileInfo> TileSelected;

        public TileInfo SelectedTile
        {
            get => selectedTile;
            private set
            {
                selectedTile = value;
                TileSelected?.Invoke(this, value);
            }
        }

        public TilePalettePanel()
        {
            Orientation = Orientation.Vertical;
            Background = new SolidColorBrush(Color.FromRgb(45, 45, 48));

            var header = new TextBlock
            {
                Text = "Tile Palette",
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(10, 10, 10, 5)
            };
            Children.Add(header);

            var separator = new Rectangle
            {
                Height = 1,
                Fill = new SolidColorBrush(Color.FromRgb(60, 60, 60)),
                Margin = new Thickness(0, 5, 0, 10)
            };
            Children.Add(separator);

            tileList = new ListBox
            {
                Background = Brushes.Transparent,
                BorderThickness = new Thickness(0),
                ItemsSource = tiles
            };

            tileList.SelectionChanged += (s, e) =>
            {
                if (tileList.SelectedItem is TileInfo tile)
                {
                    SelectedTile = tile;
                }
            };

            var itemTemplate = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(StackPanel));
            factory.SetValue(OrientationProperty, Orientation.Horizontal);

            var rectFactory = new FrameworkElementFactory(typeof(Rectangle));
            rectFactory.SetValue(WidthProperty, 32.0);
            rectFactory.SetValue(HeightProperty, 32.0);
            rectFactory.SetValue(MarginProperty, new Thickness(4));
            rectFactory.SetBinding(WidthProperty, new System.Windows.Data.Binding("TileId")
            {
                Converter = new TileIdToSizeConverter(),
                FallbackValue = 32.0
            });

            var textFactory = new FrameworkElementFactory(typeof(TextBlock));
            textFactory.SetBinding(TextBlock.TextProperty, new System.Windows.Data.Binding("TileName"));
            textFactory.SetValue(ForegroundProperty, Brushes.White);

            factory.AppendChild(rectFactory);
            factory.AppendChild(textFactory);
            itemTemplate.VisualTree = factory;
            tileList.ItemTemplate = itemTemplate;

            Children.Add(tileList);
        }

        public void AddTile(TileInfo tile)
        {
            tiles.Add(tile);
        }

        public void RemoveTile(TileInfo tile)
        {
            tiles.Remove(tile);
        }

        public void ClearTiles()
        {
            tiles.Clear();
        }
    }

    public class TileIdToSizeConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 32.0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return 0;
        }
    }

    public class EditorMenu : Menu
    {
        public event EventHandler<string> MenuItemClicked;

        public EditorMenu()
        {
            Background = new SolidColorBrush(Color.FromRgb(50, 50, 52));

            var fileMenu = CreateFileMenu();
            var editMenu = CreateEditMenu();
            var viewMenu = CreateViewMenu();
            var gameObjectMenu = CreateGameObjectMenu();
            var windowMenu = CreateWindowMenu();

            Items.Add(fileMenu);
            Items.Add(editMenu);
            Items.Add(viewMenu);
            Items.Add(gameObjectMenu);
            Items.Add(windowMenu);
        }

        private MenuItem CreateFileMenu()
        {
            var fileItem = new MenuItem { Header = "_File" };

            fileItem.Items.Add(CreateMenuItem("_New Scene", "Ctrl+N", "NewScene"));
            fileItem.Items.Add(CreateMenuItem("_Open Scene...", "Ctrl+O", "OpenScene"));
            fileItem.Items.Add(CreateSeparator());
            fileItem.Items.Add(CreateMenuItem("_Save", "Ctrl+S", "Save"));
            fileItem.Items.Add(CreateMenuItem("Save _As...", "Ctrl+Shift+S", "SaveAs"));
            fileItem.Items.Add(CreateSeparator());
            fileItem.Items.Add(CreateMenuItem("_Export...", "Ctrl+E", "Export"));
            fileItem.Items.Add(CreateMenuItem("_Import...", "Ctrl+I", "Import"));
            fileItem.Items.Add(CreateSeparator());
            fileItem.Items.Add(CreateMenuItem("E_xit", "Alt+F4", "Exit"));

            return fileItem;
        }

        private MenuItem CreateEditMenu()
        {
            var editItem = new MenuItem { Header = "_Edit" };

            editItem.Items.Add(CreateMenuItem("_Undo", "Ctrl+Z", "Undo"));
            editItem.Items.Add(CreateMenuItem("_Redo", "Ctrl+Y", "Redo"));
            editItem.Items.Add(CreateSeparator());
            editItem.Items.Add(CreateMenuItem("Cu_t", "Ctrl+X", "Cut"));
            editItem.Items.Add(CreateMenuItem("_Copy", "Ctrl+C", "Copy"));
            editItem.Items.Add(CreateMenuItem("_Paste", "Ctrl+V", "Paste"));
            editItem.Items.Add(CreateMenuItem("_Duplicate", "Ctrl+D", "Duplicate"));
            editItem.Items.Add(CreateMenuItem("_Delete", "Del", "Delete"));
            editItem.Items.Add(CreateSeparator());
            editItem.Items.Add(CreateMenuItem("Select _All", "Ctrl+A", "SelectAll"));
            editItem.Items.Add(CreateMenuItem("Select _None", "Ctrl+Shift+A", "SelectNone"));

            return editItem;
        }

        private MenuItem CreateViewMenu()
        {
            var viewItem = new MenuItem { Header = "_View" };

            viewItem.Items.Add(CreateMenuItem("Toggle _Grid", "Ctrl+G", "ToggleGrid"));
            viewItem.Items.Add(CreateMenuItem("Toggle _Snap", "Ctrl+Shift+G", "ToggleSnap"));
            viewItem.Items.Add(CreateSeparator());
            viewItem.Items.Add(CreateMenuItem("Zoom _In", "Ctrl++", "ZoomIn"));
            viewItem.Items.Add(CreateMenuItem("Zoom _Out", "Ctrl+-", "ZoomOut"));
            viewItem.Items.Add(CreateMenuItem("_Reset Zoom", "Ctrl+0", "ResetZoom"));
            viewItem.Items.Add(CreateMenuItem("_Fit to Window", "Ctrl+F", "FitToWindow"));
            viewItem.Items.Add(CreateSeparator());
            viewItem.Items.Add(CreateMenuItem("Show _All Layers", "", "ShowAllLayers"));
            viewItem.Items.Add(CreateMenuItem("Hide _All Layers", "", "HideAllLayers"));

            return viewItem;
        }

        private MenuItem CreateGameObjectMenu()
        {
            var goItem = new MenuItem { Header = "_GameObject" };

            goItem.Items.Add(CreateMenuItem("_Create Empty", "Ctrl+Shift+N", "CreateEmpty"));
            goItem.Items.Add(CreateSeparator());
            goItem.Items.Add(CreateMenuItem("Create _2D Object", "", "Create2DObject"));
            goItem.Items.Add(CreateMenuItem("Create _Sprite", "", "CreateSprite"));
            goItem.Items.Add(CreateMenuItem("Create _Light", "", "CreateLight"));
            goItem.Items.Add(CreateSeparator());
            goItem.Items.Add(CreateMenuItem("Create _Spawn Point", "", "CreateSpawnPoint"));
            goItem.Items.Add(CreateMenuItem("Create _Trigger Zone", "", "CreateTriggerZone"));
            goItem.Items.Add(CreateMenuItem("Create _Path Node", "", "CreatePathNode"));

            return goItem;
        }

        private MenuItem CreateWindowMenu()
        {
            var windowItem = new MenuItem { Header = "_Window" };

            windowItem.Items.Add(CreateMenuItem("Show _Hierarchy", "Ctrl+1", "ShowHierarchy"));
            windowItem.Items.Add(CreateMenuItem("Show _Inspector", "Ctrl+2", "ShowInspector"));
            windowItem.Items.Add(CreateMenuItem("Show _Console", "Ctrl+3", "ShowConsole"));
            windowItem.Items.Add(CreateMenuItem("Show _Tile Palette", "Ctrl+4", "ShowTilePalette"));
            windowItem.Items.Add(CreateSeparator());
            windowItem.Items.Add(CreateMenuItem("Show _All", "Ctrl+Shift+1", "ShowAll"));
            windowItem.Items.Add(CreateMenuItem("_Reset Layout", "", "ResetLayout"));

            return windowItem;
        }

        private MenuItem CreateMenuItem(string header, string inputGestureText, string command)
        {
            var item = new MenuItem
            {
                Header = header,
                InputGestureText = inputGestureText,
                Tag = command
            };

            item.Click += (s, e) =>
            {
                if (s is MenuItem mi && mi.Tag is string cmd)
                {
                    MenuItemClicked?.Invoke(this, cmd);
                }
            };

            return item;
        }

        private Separator CreateSeparator()
        {
            return new Separator();
        }
    }

    public class EditorConsolePanel : Grid
    {
        private readonly ListBox consoleList;
        private readonly TextBox inputBox;
        private readonly ObservableCollection<string> messages = new();

        public event EventHandler<string> CommandEntered;

        public EditorConsolePanel()
        {
            RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            consoleList = new ListBox
            {
                ItemsSource = messages,
                Background = new SolidColorBrush(Color.FromRgb(30, 30, 30)),
                Foreground = Brushes.LightGray,
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                BorderThickness = new Thickness(0)
            };
            Grid.SetRow(consoleList, 0);
            Children.Add(consoleList);

            var inputPanel = new DockPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)),
                LastChildFill = true
            };
            Grid.SetRow(inputPanel, 1);

            var promptLabel = new TextBlock
            {
                Text = ">",
                Foreground = Brushes.LightBlue,
                FontFamily = new FontFamily("Consolas"),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 5, 0)
            };
            DockPanel.SetDock(promptLabel, Dock.Left);
            inputPanel.Children.Add(promptLabel);

            inputBox = new TextBox
            {
                Background = Brushes.Transparent,
                Foreground = Brushes.White,
                BorderThickness = new Thickness(0),
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                Padding = new Thickness(0, 5, 10, 5),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            inputBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    var command = inputBox.Text;
                    inputBox.Clear();
                    if (!string.IsNullOrWhiteSpace(command))
                    {
                        CommandEntered?.Invoke(this, command);
                    }
                }
            };
            inputPanel.Children.Add(inputBox);

            Children.Add(inputPanel);
        }

        public void AddMessage(string message, Brush foreground = null)
        {
            messages.Add(message);
            consoleList.ScrollIntoView(consoleList.Items[consoleList.Items.Count - 1]);
        }

        public void Clear()
        {
            messages.Clear();
        }
    }

    public class EditorCanvas : Canvas
    {
        private readonly EditorCamera camera = new();
        private readonly TileMapEditor tileMap;
        private readonly WorldBuilderSystem worldBuilder;
        private EditorTool currentTool = EditorTool.Select;
        private TileLayer currentLayer = TileLayer.Background;
        private bool isDrawing;
        private bool isPanning;
        private Vector2 lastMousePosition;
        private Vector2Int drawStartTile;
        private Vector2Int currentTile;
        private Point dragStartPoint;
        private bool isDragging;

        private readonly Rectangle gridRect;
        private readonly Rectangle marqueeRect;
        private readonly Polyline pathPreviewLine;

        public event EventHandler<Vector2> WorldPositionChanged;
        public event EventHandler<GameObject> ObjectClicked;

        public EditorCamera Camera => camera;
        public TileMapEditor TileMap => tileMap;
        public WorldBuilderSystem WorldBuilder => worldBuilder;
        public EditorTool CurrentTool
        {
            get => currentTool;
            set => currentTool = value;
        }

        public TileLayer CurrentLayer
        {
            get => currentLayer;
            set => currentLayer = value;
        }

        public EditorCanvas()
        {
            Background = new SolidColorBrush(Color.FromRgb(40, 40, 40));
            ClipToBounds = true;
            Focusable = true;

            tileMap = new TileMapEditor();
            worldBuilder = new WorldBuilderSystem();

            gridRect = new Rectangle
            {
                Stroke = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255)),
                StrokeThickness = 1,
                Fill = Brushes.Transparent
            };
            Children.Add(gridRect);

            marqueeRect = new Rectangle
            {
                Stroke = new SolidColorBrush(Color.FromRgb(30, 130, 230)),
                StrokeThickness = 2,
                Fill = new SolidColorBrush(Color.FromArgb(30, 30, 130, 230)),
                Visibility = Visibility.Collapsed
            };
            Children.Add(marqueeRect);

            pathPreviewLine = new Polyline
            {
                Stroke = Brushes.Yellow,
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection { 4, 2 },
                Visibility = Visibility.Collapsed
            };
            Children.Add(pathPreviewLine);

            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
            MouseRightButtonDown += OnMouseRightButtonDown;
            MouseRightButtonUp += OnMouseRightButtonUp;
            MouseMove += OnMouseMove;
            MouseWheel += OnMouseWheel;
            KeyDown += OnKeyDown;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Delete:
                    break;
                case Key.Escape:
                    currentTool = EditorTool.Select;
                    break;
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            Focus();
            CaptureMouse();
            lastMousePosition = GetWorldPosition(e.GetPosition(this));
            dragStartPoint = e.GetPosition(this);
            isDragging = true;

            if (currentTool == EditorTool.TileBrush)
            {
                isDrawing = true;
                var tilePos = ScreenToTile(lastMousePosition);
                drawStartTile = tilePos;
                currentTile = tilePos;
                tileMap.AddTile(tilePos.X, tilePos.Y);
                InvalidateVisual();
            }
            else if (currentTool == EditorTool.Fill)
            {
                var tilePos = ScreenToTile(lastMousePosition);
                tileMap.Fill(tilePos.X, tilePos.Y);
                InvalidateVisual();
            }
            else if (currentTool == EditorTool.RectangleSelect)
            {
                marqueeRect.Visibility = Visibility.Visible;
                UpdateMarquee(e.GetPosition(this));
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();

            if (isDrawing && currentTool == EditorTool.TileBrush)
            {
                var currentPos = GetWorldPosition(e.GetPosition(this));
                var currentTilePos = ScreenToTile(currentPos);

                if (currentTilePos != drawStartTile)
                {
                    tileMap.FillRectangle(drawStartTile.X, drawStartTile.Y, currentTilePos.X, currentTilePos.Y);
                }
            }

            isDrawing = false;
            isDragging = false;
            marqueeRect.Visibility = Visibility.Collapsed;
            InvalidateVisual();
        }

        private void OnMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            CaptureMouse();
            isPanning = true;
            lastMousePosition = e.GetPosition(this);
        }

        private void OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            ReleaseMouseCapture();
            isPanning = false;
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            var screenPos = e.GetPosition(this);
            var worldPos = GetWorldPosition(screenPos);

            WorldPositionChanged?.Invoke(this, worldPos);

            if (isPanning)
            {
                var delta = screenPos - lastMousePosition;
                camera.Pan(new Vector2((float)-delta.X, (float)-delta.Y) / (float)camera.Zoom);
                lastMousePosition = screenPos;
                InvalidateVisual();
            }
            else if (isDrawing && currentTool == EditorTool.TileBrush)
            {
                var tilePos = ScreenToTile(worldPos);
                if (tilePos != currentTile)
                {
                    currentTile = tilePos;
                    tileMap.AddTile(tilePos.X, tilePos.Y);
                    InvalidateVisual();
                }
            }
            else if (currentTool == EditorTool.RectangleSelect && isDragging)
            {
                UpdateMarquee(screenPos);
            }
        }

        private void OnMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var screenPos = e.GetPosition(this);
            var worldPosBefore = ScreenToWorld(screenPos);

            float zoomDelta = e.Delta > 0 ? 1.1f : 0.9f;
            camera.ZoomAt(new Vector2((float)worldPosBefore.X, (float)worldPosBefore.Y), zoomDelta > 1 ? 0.1f : -0.1f);

            InvalidateVisual();
        }

        private void UpdateMarquee(Point screenPos)
        {
            var x = Math.Min(dragStartPoint.X, screenPos.X);
            var y = Math.Min(dragStartPoint.Y, screenPos.Y);
            var width = Math.Abs(screenPos.X - dragStartPoint.X);
            var height = Math.Abs(screenPos.Y - dragStartPoint.Y);

            Canvas.SetLeft(marqueeRect, x);
            Canvas.SetTop(marqueeRect, y);
            marqueeRect.Width = width;
            marqueeRect.Height = height;
        }

        public Vector2 GetWorldPosition(Point screenPoint)
        {
            return camera.ScreenToWorld(new Vector2((float)screenPoint.X, (float)screenPoint.Y));
        }

        public Vector2 ScreenToWorld(Vector2 screenPoint)
        {
            return camera.ScreenToWorld(screenPoint);
        }

        public Vector2Int ScreenToTile(Vector2 worldPosition)
        {
            return new Vector2Int(
                (int)Math.Floor(worldPosition.X / 32f),
                (int)Math.Floor(worldPosition.Y / 32f)
            );
        }

        public Vector2 SnapToGrid(Vector2 worldPosition, float gridSize)
        {
            return new Vector2(
                (float)Math.Round(worldPosition.X / gridSize) * gridSize,
                (float)Math.Round(worldPosition.Y / gridSize) * gridSize
            );
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            dc.PushTransform(new TranslateTransform(-camera.Position.X * camera.Zoom + ActualWidth / 2,
                                                     -camera.Position.Y * camera.Zoom + ActualHeight / 2));
            dc.PushTransform(new ScaleTransform(camera.Zoom, camera.Zoom));

            DrawGrid(dc);
            DrawTileMap(dc);
            DrawWorldBuilderMarkers(dc);

            dc.Pop();
            dc.Pop();
        }

        private void DrawGrid(DrawingContext dc)
        {
            var gridSize = 32f;
            var gridBrush = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255));

            var startX = (float)Math.Floor(camera.Position.X - ActualWidth / camera.Zoom / 2 / gridSize) * gridSize;
            var startY = (float)Math.Floor(camera.Position.Y - ActualHeight / camera.Zoom / 2 / gridSize) * gridSize;
            var endX = startX + (float)ActualWidth / camera.Zoom + gridSize * 2;
            var endY = startY + (float)ActualHeight / camera.Zoom + gridSize * 2;

            for (float x = startX; x < endX; x += gridSize)
            {
                dc.DrawLine(new Pen(gridBrush, 0.5f), new Point(x, startY), new Point(x, endY));
            }

            for (float y = startY; y < endY; y += gridSize)
            {
                dc.DrawLine(new Pen(gridBrush, 0.5f), new Point(startX, y), new Point(endX, y));
            }
        }

        private void DrawTileMap(DrawingContext dc)
        {
            var tiles = tileMap.GetTilesForLayer(currentLayer);
            var tileBrush = new SolidColorBrush(Color.FromRgb(100, 150, 200));

            foreach (var kvp in tiles)
            {
                var tile = kvp.Value;
                var rect = new Rect(tile.GridX * 32, tile.GridY * 32, 32, 32);
                dc.DrawRectangle(tileBrush, null, rect);
            }
        }

        private void DrawWorldBuilderMarkers(DrawingContext dc)
        {
            foreach (var marker in worldBuilder.Markers)
            {
                var screenPos = marker.Position;
                var brush = GetMarkerBrush(marker.MarkerType);

                switch (marker.MarkerType)
                {
                    case "SpawnPoint":
                        dc.DrawEllipse(brush, null, new Point(screenPos.X, screenPos.Y), 12, 12);
                        break;
                    case "TriggerZone":
                        dc.DrawRectangle(null, new Pen(brush, 2), new Rect(screenPos.X - 25, screenPos.Y - 25, 50, 50));
                        break;
                    case "PathNode":
                        dc.DrawEllipse(brush, null, new Point(screenPos.X, screenPos.Y), 6, 6);
                        break;
                    case "Light":
                        dc.DrawEllipse(brush, null, new Point(screenPos.X, screenPos.Y), 15, 15);
                        break;
                }
            }
        }

        private Brush GetMarkerBrush(string markerType)
        {
            return markerType switch
            {
                "SpawnPoint" => Brushes.Green,
                "TriggerZone" => Brushes.Orange,
                "PathNode" => Brushes.Yellow,
                "Light" => Brushes.LightYellow,
                "Decoration" => Brushes.Purple,
                "Water" => Brushes.DodgerBlue,
                _ => Brushes.White
            };
        }
    }

    public class LevelEditorWindow : Window
    {
        private readonly EditorCanvas canvas;
        private readonly EditorHierarchy hierarchy;
        private readonly EditorInspector inspector;
        private readonly EditorConsolePanel consolePanel;
        private readonly TilePalettePanel tilePalettePanel;
        private readonly EditorToolbar toolbar;
        private readonly EditorMenu menu;
        private readonly EditorSettings settings;
        private readonly UndoRedoSystem undoRedo;
        private readonly SelectionSystem selection;
        private readonly EditorConsole console;

        private SceneObject currentScene;
        private Grid mainGrid;

        public LevelEditorWindow()
        {
            Title = "WPF Engine Level Editor";
            Width = 1280;
            Height = 800;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(35, 35, 38));

            settings = new EditorSettings();
            console = new EditorConsole();
            undoRedo = new UndoRedoSystem();
            selection = new SelectionSystem();
            canvas = new EditorCanvas();
            hierarchy = new EditorHierarchy();
            inspector = new EditorInspector();
            consolePanel = new EditorConsolePanel();
            tilePalettePanel = new TilePalettePanel();
            toolbar = new EditorToolbar();
            menu = new EditorMenu();

            InitializeUI();
            SetupEventHandlers();
            LoadDefaultTiles();

            console.Log("Level Editor initialized");
        }

        private void InitializeUI()
        {
            mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            menu.Height = 24;
            Grid.SetRow(menu, 0);
            Grid.SetColumnSpan(menu, 3);
            mainGrid.Children.Add(menu);

            toolbar.Height = 40;
            Grid.SetRow(toolbar, 1);
            Grid.SetColumnSpan(toolbar, 3);
            mainGrid.Children.Add(toolbar);

            var leftPanel = new Grid { Width = 200, Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)) };
            leftPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            leftPanel.RowDefinitions.Add(new RowDefinition { Height = new GridDefinition { Height = new GridLength(1, GridUnitType.Star) } });

            hierarchy.Width = 200;
            Grid.SetRow(hierarchy, 0);
            leftPanel.Children.Add(hierarchy);

            tilePalettePanel.Width = 200;
            Grid.SetRow(tilePalettePanel, 1);
            leftPanel.Children.Add(tilePalettePanel);

            Grid.SetRow(leftPanel, 2);
            mainGrid.Children.Add(leftPanel);

            var centerPanel = new Border { BorderBrush = new SolidColorBrush(Color.FromRgb(60, 60, 60)), BorderThickness = new Thickness(1) };
            centerPanel.Child = canvas;
            Grid.SetRow(centerPanel, 2);
            Grid.SetColumn(centerPanel, 1);
            mainGrid.Children.Add(centerPanel);

            var rightPanel = new Grid { Width = 250, Background = new SolidColorBrush(Color.FromRgb(45, 45, 48)) };
            rightPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            rightPanel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(150) });

            inspector.Width = 250;
            Grid.SetRow(inspector, 0);
            rightPanel.Children.Add(inspector);

            consolePanel.Width = 250;
            Grid.SetRow(consolePanel, 1);
            rightPanel.Children.Add(consolePanel);

            Grid.SetRow(rightPanel, 2);
            Grid.SetColumn(rightPanel, 2);
            mainGrid.Children.Add(rightPanel);

            var statusBar = CreateStatusBar();
            Grid.SetRow(statusBar, 3);
            Grid.SetColumnSpan(statusBar, 3);
            mainGrid.Children.Add(statusBar);

            Content = mainGrid;
        }

        private Grid CreateStatusBar()
        {
            var statusBar = new Grid { Height = 24, Background = new SolidColorBrush(Color.FromRgb(50, 50, 52)) };
            statusBar.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            statusBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            statusBar.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var positionLabel = new TextBlock
            {
                Text = "X: 0.00, Y: 0.00",
                Foreground = Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 0, 0),
                FontFamily = new FontFamily("Consolas")
            };
            positionLabel.Tag = "PositionLabel";
            Grid.SetColumn(positionLabel, 0);
            statusBar.Children.Add(positionLabel);

            var zoomLabel = new TextBlock
            {
                Text = "Zoom: 100%",
                Foreground = Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0),
                FontFamily = new FontFamily("Consolas")
            };
            zoomLabel.Tag = "ZoomLabel";
            Grid.SetColumn(zoomLabel, 1);
            statusBar.Children.Add(zoomLabel);

            var toolLabel = new TextBlock
            {
                Text = "Tool: Select",
                Foreground = Brushes.LightGray,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(10, 0, 10, 0),
                FontFamily = new FontFamily("Consolas")
            };
            toolLabel.Tag = "ToolLabel";
            Grid.SetColumn(toolLabel, 2);
            statusBar.Children.Add(toolLabel);

            canvas.WorldPositionChanged += (s, pos) =>
            {
                positionLabel.Text = $"X: {pos.X:F2}, Y: {pos.Y:F2}";
            };

            return statusBar;
        }

        private void SetupEventHandlers()
        {
            toolbar.ToolChanged += (s, tool) =>
            {
                canvas.CurrentTool = tool;
                UpdateToolLabel(tool);
            };

            menu.MenuItemClicked += HandleMenuCommand;
            consolePanel.CommandEntered += HandleConsoleCommand;

            selection.SelectionChanged += (s, objects) =>
            {
                if (objects.Count > 0)
                {
                    inspector.SelectedObject = objects[0];
                }
                else
                {
                    inspector.SelectedObject = null;
                }
            };

            canvas.WorldPositionChanged += (s, pos) =>
            {
                console.LogDebug($"World position: {pos.X:F2}, {pos.Y:F2}");
            };

            KeyDown += (s, e) =>
            {
                if (Keyboard.Modifiers == ModifierKeys.Control)
                {
                    switch (e.Key)
                    {
                        case Key.Z:
                            undoRedo.Undo();
                            console.Log("Undo");
                            break;
                        case Key.Y:
                            undoRedo.Redo();
                            console.Log("Redo");
                            break;
                        case Key.S:
                            SaveScene();
                            break;
                        case Key.C:
                            selection.Copy();
                            console.Log("Copy");
                            break;
                        case Key.V:
                            if (selection.HasClipboard)
                            {
                                var pasted = selection.Paste(canvas.Camera.Position);
                                foreach (var obj in pasted)
                                {
                                    currentScene?.AddGameObject(obj);
                                }
                                console.Log($"Pasted {pasted.Count} objects");
                            }
                            break;
                        case Key.D:
                            if (selection.SelectedObjects.Count > 0)
                            {
                                var offset = new Vector2(50, 50);
                                var duplicated = selection.Duplicate(offset);
                                foreach (var obj in duplicated)
                                {
                                    currentScene?.AddGameObject(obj);
                                }
                                console.Log($"Duplicated {duplicated.Count} objects");
                            }
                            break;
                        case Key.G:
                            settings.ShowGrid = !settings.ShowGrid;
                            console.Log($"Grid visibility: {settings.ShowGrid}");
                            break;
                    }
                }
                else if (Keyboard.Modifiers == ModifierKeys.Shift)
                {
                    if (e.Key == Key.G)
                    {
                        settings.SnapToGrid = !settings.SnapToGrid;
                        console.Log($"Snap to grid: {settings.SnapToGrid}");
                    }
                }
                else
                {
                    switch (e.Key)
                    {
                        case Key.V:
                            toolbar.CurrentTool = EditorTool.Move;
                            break;
                        case Key.E:
                            toolbar.CurrentTool = EditorTool.Scale;
                            break;
                        case Key.R:
                            toolbar.CurrentTool = EditorTool.Rotate;
                            break;
                        case Key.T:
                            toolbar.CurrentTool = EditorTool.TileBrush;
                            break;
                        case Key.X:
                            toolbar.CurrentTool = EditorTool.Eraser;
                            break;
                        case Key.F:
                            toolbar.CurrentTool = EditorTool.Fill;
                            break;
                        case Key.Delete:
                            DeleteSelectedObjects();
                            break;
                        case Key.Escape:
                            selection.ClearSelection();
                            break;
                    }
                }
            };
        }

        private void UpdateToolLabel(EditorTool tool)
        {
            var statusBar = mainGrid.Children.OfType<Grid>().LastOrDefault();
            if (statusBar != null)
            {
                var toolLabel = statusBar.Children.OfType<TextBlock>().FirstOrDefault(t => t.Tag?.ToString() == "ToolLabel");
                if (toolLabel != null)
                {
                    toolLabel.Text = $"Tool: {tool}";
                }
            }
        }

        private void HandleMenuCommand(object sender, string command)
        {
            console.Log($"Command: {command}");

            switch (command)
            {
                case "NewScene":
                    NewScene();
                    break;
                case "OpenScene":
                    OpenScene();
                    break;
                case "Save":
                    SaveScene();
                    break;
                case "SaveAs":
                    SaveSceneAs();
                    break;
                case "Undo":
                    undoRedo.Undo();
                    break;
                case "Redo":
                    undoRedo.Redo();
                    break;
                case "Cut":
                    selection.Copy();
                    DeleteSelectedObjects();
                    break;
                case "Copy":
                    selection.Copy();
                    break;
                case "Paste":
                    PasteFromClipboard();
                    break;
                case "Duplicate":
                    DuplicateSelected();
                    break;
                case "Delete":
                    DeleteSelectedObjects();
                    break;
                case "SelectAll":
                    SelectAllObjects();
                    break;
                case "ToggleGrid":
                    settings.ShowGrid = !settings.ShowGrid;
                    break;
                case "ToggleSnap":
                    settings.SnapToGrid = !settings.SnapToGrid;
                    break;
                case "ZoomIn":
                    canvas.Camera.Zoom *= 1.2f;
                    break;
                case "ZoomOut":
                    canvas.Camera.Zoom *= 0.8f;
                    break;
                case "ResetZoom":
                    canvas.Camera.Zoom = 1f;
                    break;
                case "CreateEmpty":
                    CreateEmptyObject();
                    break;
                case "CreateSpawnPoint":
                    toolbar.CurrentTool = EditorTool.SpawnPoint;
                    break;
                case "CreateTriggerZone":
                    toolbar.CurrentTool = EditorTool.TriggerZone;
                    break;
                case "CreatePathNode":
                    toolbar.CurrentTool = EditorTool.PathNode;
                    break;
                case "ShowHierarchy":
                case "ShowInspector":
                case "ShowConsole":
                case "ShowTilePalette":
                    break;
            }
        }

        private void HandleConsoleCommand(object sender, string command)
        {
            console.ExecuteCommand(command);
            consolePanel.AddMessage($"$ {command}", Brushes.LightBlue);

            var parts = command.Split(' ', 2);
            var cmd = parts[0].ToLower();
            var args = parts.Length > 1 ? parts[1] : "";

            switch (cmd)
            {
                case "help":
                    consolePanel.AddMessage("Available commands: help, clear, save, load, setgrid, info", Brushes.LightGreen);
                    break;
                case "clear":
                    consolePanel.Clear();
                    break;
                case "save":
                    SaveScene();
                    break;
                case "load":
                    OpenScene();
                    break;
                case "setgrid":
                    if (float.TryParse(args, out var gridSize))
                    {
                        settings.GridSize = gridSize;
                        consolePanel.AddMessage($"Grid size set to {gridSize}", Brushes.LightGreen);
                    }
                    break;
                case "info":
                    ShowSceneInfo();
                    break;
                case "reset":
                    canvas.Camera.Reset();
                    consolePanel.AddMessage("Camera reset", Brushes.LightGreen);
                    break;
                default:
                    consolePanel.AddMessage($"Unknown command: {cmd}", Brushes.OrangeRed);
                    break;
            }
        }

        private void LoadDefaultTiles()
        {
            var colors = new[] {
                (Color.FromRgb(100, 100, 100), "Gray Stone"),
                (Color.FromRgb(139, 90, 43), "Brown Dirt"),
                (Color.FromRgb(34, 139, 34), "Green Grass"),
                (Color.FromRgb(70, 130, 180), "Blue Water"),
                (Color.FromRgb(205, 133, 63), "Tan Sand"),
                (Color.FromRgb(128, 128, 128), "Silver Metal"),
                (Color.FromRgb(139, 69, 19), "Dark Wood"),
                (Color.FromRgb(255, 215, 0), "Gold")
            };

            for (int i = 0; i < colors.Length; i++)
            {
                var tile = new TileInfo
                {
                    TileId = i,
                    TileName = colors[i].Item2,
                    TileBrush = new SolidColorBrush(colors[i].Item1)
                };
                tilePalettePanel.AddTile(tile);
            }

            console.Log($"Loaded {colors.Length} default tiles");
        }

        private void NewScene()
        {
            if (currentScene != null)
            {
                var result = MessageBox.Show("Save current scene before creating new?", "New Scene",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel) return;
                if (result == MessageBoxResult.Yes) SaveScene();
            }

            currentScene = new SceneObject("New Scene");
            hierarchy.RootObject = currentScene.RootGameObject;
            canvas.TileMap.Clear();
            canvas.WorldBuilder.ClearMarkers();
            selection.ClearSelection();
            undoRedo.Clear();

            console.Log("New scene created");
        }

        private void OpenScene()
        {
            console.LogWarning("Open scene functionality not yet implemented");
        }

        private void SaveScene()
        {
            console.LogWarning("Save scene functionality not yet implemented");
        }

        private void SaveSceneAs()
        {
            console.LogWarning("Save scene as functionality not yet implemented");
        }

        private void DeleteSelectedObjects()
        {
            var toDelete = selection.SelectedObjects.ToList();
            foreach (var obj in toDelete)
            {
                obj.Destroy();
            }
            selection.ClearSelection();
            console.Log($"Deleted {toDelete.Count} objects");
        }

        private void PasteFromClipboard()
        {
            if (!selection.HasClipboard) return;

            var pasted = selection.Paste(canvas.Camera.Position);
            foreach (var obj in pasted)
            {
                currentScene?.AddGameObject(obj);
            }
            console.Log($"Pasted {pasted.Count} objects");
        }

        private void DuplicateSelected()
        {
            if (selection.SelectedObjects.Count == 0) return;

            var duplicated = selection.Duplicate(new Vector2(50, 50));
            foreach (var obj in duplicated)
            {
                currentScene?.AddGameObject(obj);
            }
            console.Log($"Duplicated {duplicated.Count} objects");
        }

        private void SelectAllObjects()
        {
            if (currentScene == null) return;

            var allObjects = new List<GameObject>();
            CollectAllObjects(currentScene.RootGameObject, allObjects);

            selection.ClearSelection();
            foreach (var obj in allObjects)
            {
                selection.AddToSelection(obj);
            }

            console.Log($"Selected {allObjects.Count} objects");
        }

        private void CollectAllObjects(GameObject parent, List<GameObject> list)
        {
            if (parent == null) return;
            list.Add(parent);
            foreach (var child in parent.Children)
            {
                CollectAllObjects(child, list);
            }
        }

        private void ShowSceneInfo()
        {
            if (currentScene == null)
            {
                consolePanel.AddMessage("No scene loaded", Brushes.Orange);
                return;
            }

            var objectCount = 0;
            CollectAllObjects(currentScene.RootGameObject, new List<GameObject>());

            consolePanel.AddMessage($"Scene: {currentScene.Name}", Brushes.LightGreen);
            consolePanel.AddMessage($"Objects: {objectCount}", Brushes.LightGreen);
            consolePanel.AddMessage($"Tiles: {canvas.TileMap.GetTilesForLayer(TileLayer.Background).Count}", Brushes.LightGreen);
            consolePanel.AddMessage($"Markers: {canvas.WorldBuilder.Markers.Count}", Brushes.LightGreen);
        }

        private void CreateEmptyObject()
        {
            if (currentScene == null)
            {
                NewScene();
            }

            var obj = new GameObject("GameObject");
            obj.Transform.LocalPosition = canvas.Camera.Position;
            currentScene.AddGameObject(obj);
            selection.Select(obj);
            console.Log($"Created: {obj.Name}");
        }

        public void LoadScene(SceneObject scene)
        {
            currentScene = scene;
            hierarchy.RootObject = scene.RootGameObject;
            console.Log($"Loaded scene: {scene.Name}");
        }

        public SceneObject GetCurrentScene() => currentScene;
        public EditorCanvas GetCanvas() => canvas;
        public EditorHierarchy GetHierarchy() => hierarchy;
        public EditorInspector GetInspector() => inspector;
        public EditorConsole GetConsole() => console;
        public EditorSettings GetSettings() => settings;
        public UndoRedoSystem GetUndoRedo() => undoRedo;
        public SelectionSystem GetSelection() => selection;
    }
}

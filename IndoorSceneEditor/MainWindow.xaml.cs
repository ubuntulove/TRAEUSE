using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Microsoft.Win32;
using IndoorSceneEditor.Models;
using IndoorSceneEditor.Services;

namespace IndoorSceneEditor
{
    public partial class MainWindow : Window
    {
        private SceneDocument _doc = new();
        private readonly Model3DGroup _itemsGroup = new();
        private readonly Dictionary<string, GeometryModel3D> _itemModels = new();
        private readonly Dictionary<Model3D, string> _modelToId = new();
        private string? _selectedId;

        // 相机轨道参数
        private double _camAzimuth = 35;    // 度
        private double _camElevation = 25;  // 度
        private double _camDistance = 12;
        private readonly Point3D _camTarget = new(0, 0.5, 0);

        private Point _lastMouse;
        private bool _orbiting;

        private string? _currentFilePath;

        public MainWindow()
        {
            InitializeComponent();
            InitializeTypesCombo();
            Initialize3D();
            NewScene();
            LoadSample();
        }

        private void InitializeTypesCombo()
        {
            PropType.ItemsSource = Enum.GetValues(typeof(ItemType)).Cast<ItemType>().ToList();
        }

        private void Initialize3D()
        {
            // 相机
            var camera = new PerspectiveCamera
            {
                FieldOfView = 45,
                NearPlaneDistance = 0.1,
                FarPlaneDistance = 1000
            };
            Viewport.Camera = camera;
            UpdateCamera();

            // 灯光
            var lights = new Model3DGroup();
            lights.Children.Add(new AmbientLight(Color.FromRgb(80, 80, 80)));
            var dir = new DirectionalLight(Color.FromRgb(220, 220, 220), new Vector3D(-0.5, -1, -0.3));
            lights.Children.Add(dir);
            var dir2 = new DirectionalLight(Color.FromRgb(120, 120, 140), new Vector3D(0.4, -0.6, 0.7));
            lights.Children.Add(dir2);

            // 地面
            var ground = new GeometryModel3D
            {
                Geometry = MeshFactory.Create(ItemType.Rug),
                Material = new DiffuseMaterial(new SolidColorBrush(Color.FromRgb(60, 60, 65)))
            };
            // 放大地面
            var groundTransform = new ScaleTransform3D(5, 1, 5);
            ground.Transform = groundTransform;

            // 组装到视口
            var root = new Model3DGroup();
            root.Children.Add(lights);
            root.Children.Add(ground);
            root.Children.Add(_itemsGroup);

            var visual = new ModelVisual3D { Content = root };
            Viewport.Children.Add(visual);
        }

        private void UpdateCamera()
        {
            double az = _camAzimuth * Math.PI / 180;
            double el = _camElevation * Math.PI / 180;
            double x = _camDistance * Math.Cos(el) * Math.Sin(az);
            double y = _camDistance * Math.Sin(el);
            double z = _camDistance * Math.Cos(el) * Math.Cos(az);
            var cam = (PerspectiveCamera)Viewport.Camera;
            cam.Position = new Point3D(_camTarget.X + x, _camTarget.Y + y, _camTarget.Z + z);
            cam.LookDirection = _camTarget - cam.Position;
            cam.UpDirection = new Vector3D(0, 1, 0);
        }

        // ========== 场景文档操作 ==========

        private void NewScene()
        {
            _doc = new SceneDocument { Name = "未命名场景" };
            _selectedId = null;
            SceneNameBox.Text = _doc.Name;
            RebuildAllModels();
            RefreshItemList();
            UpdatePropsPanel();
            _currentFilePath = null;
            Status("新建场景");
        }

        private void RebuildAllModels()
        {
            _itemsGroup.Children.Clear();
            _itemModels.Clear();
            _modelToId.Clear();
            foreach (var item in _doc.Items)
            {
                AddItemModel(item);
            }
        }

        private void AddItemModel(SceneItem item)
        {
            var mesh = MeshFactory.Create(item.Type);
            var material = new DiffuseMaterial(new SolidColorBrush(ParseColor(item.Color)));
            var model = new GeometryModel3D(mesh, material);
            model.Transform = BuildTransform(item);
            _itemsGroup.Children.Add(model);
            _itemModels[item.Id] = model;
            _modelToId[model] = item.Id;
        }

        private void RemoveItemModel(string id)
        {
            if (_itemModels.TryGetValue(id, out var model))
            {
                _itemsGroup.Children.Remove(model);
                _modelToId.Remove(model);
                _itemModels.Remove(id);
            }
        }

        private Transform3D BuildTransform(SceneItem item)
        {
            var group = new Transform3DGroup();
            group.Children.Add(new ScaleTransform3D(item.Scale, item.Scale, item.Scale));
            group.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 1, 0), item.RotationY)));
            group.Children.Add(new TranslateTransform3D(item.Position.X, item.Position.Y, item.Position.Z));
            return group;
        }

        private void RefreshItemList()
        {
            ItemList.Items.Clear();
            foreach (var item in _doc.Items)
            {
                ItemList.Items.Add(new ItemListEntry { Id = item.Id, Label = $"{item.Name} [{item.Type}]" });
            }
            if (_selectedId != null)
            {
                foreach (ItemListEntry e in ItemList.Items)
                {
                    if (e.Id == _selectedId) { ItemList.SelectedItem = e; break; }
                }
            }
        }

        private class ItemListEntry
        {
            public string Id { get; set; } = "";
            public string Label { get; set; } = "";
            public override string ToString() => Label;
        }

        // ========== 物品增删/交换 ==========

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string tag && Enum.TryParse<ItemType>(tag, out var type))
            {
                var item = new SceneItem
                {
                    Name = DefaultName(type),
                    Type = type,
                    Position = new Vec3(0, 0, 0),
                    Color = DefaultColor(type)
                };
                _doc.Items.Add(item);
                AddItemModel(item);
                _selectedId = item.Id;
                RefreshItemList();
                UpdatePropsPanel();
                Status($"已添加 {type}");
            }
        }

        private void DeleteSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == null) return;
            var item = _doc.Items.FirstOrDefault(i => i.Id == _selectedId);
            if (item == null) return;
            _doc.Items.Remove(item);
            RemoveItemModel(_selectedId);
            _selectedId = null;
            RefreshItemList();
            UpdatePropsPanel();
            Status("已删除选中物品");
        }

        private void DuplicateSelected_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedId == null) return;
            var item = _doc.Items.FirstOrDefault(i => i.Id == _selectedId);
            if (item == null) return;
            var clone = item.Clone();
            clone.Id = Guid.NewGuid().ToString("N");
            clone.Name = item.Name + " 副本";
            clone.Position = new Vec3(item.Position.X + 1, item.Position.Y, item.Position.Z + 1);
            _doc.Items.Add(clone);
            AddItemModel(clone);
            _selectedId = clone.Id;
            RefreshItemList();
            UpdatePropsPanel();
            Status("已复制选中物品");
        }

        /// <summary>交换物品类型：替换网格，保留位置/旋转/缩放/颜色。</summary>
        private void SwapItemType(SceneItem item, ItemType newType)
        {
            item.Type = newType;
            RemoveItemModel(item.Id);
            AddItemModel(item);
            RefreshItemList();
            Status($"已交换为 {newType}");
        }

        // ========== 选择与属性 ==========

        private void ItemList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (ItemList.SelectedItem is ItemListEntry entry)
            {
                _selectedId = entry.Id;
                UpdatePropsPanel();
            }
        }

        private void UpdatePropsPanel()
        {
            var item = _doc.Items.FirstOrDefault(i => i.Id == _selectedId);
            bool has = item != null;
            PropName.IsEnabled = has;
            PropType.IsEnabled = has;
            PropColor.IsEnabled = has;
            PropX.IsEnabled = has;
            PropZ.IsEnabled = has;
            PropRot.IsEnabled = has;
            PropScale.IsEnabled = has;

            if (item == null)
            {
                PropName.Text = "";
                PropType.SelectedItem = null;
                PropColor.Text = "";
                PropX.Text = "";
                PropZ.Text = "";
                PropRot.Text = "";
                PropScale.Text = "";
                return;
            }
            PropName.Text = item.Name;
            PropType.SelectedItem = item.Type;
            PropColor.Text = item.Color;
            PropX.Text = item.Position.X.ToString("F2");
            PropZ.Text = item.Position.Z.ToString("F2");
            PropRot.Text = item.RotationY.ToString("F1");
            PropScale.Text = item.Scale.ToString("F2");
        }

        private void PropType_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // 由「应用」按钮统一提交，避免中途误触发；此处仅记录。
        }

        private void ApplyProps_Click(object sender, RoutedEventArgs e)
        {
            var item = _doc.Items.FirstOrDefault(i => i.Id == _selectedId);
            if (item == null) return;

            item.Name = string.IsNullOrWhiteSpace(PropName.Text) ? item.Name : PropName.Text.Trim();
            item.Color = SanitizeColor(PropColor.Text, item.Color);
            if (PropType.SelectedItem is ItemType t)
            {
                if (t != item.Type) SwapItemType(item, t);
            }
            if (double.TryParse(PropX.Text, out var x)) item.Position.X = x;
            if (double.TryParse(PropZ.Text, out var z)) item.Position.Z = z;
            if (double.TryParse(PropRot.Text, out var r)) item.RotationY = r;
            if (double.TryParse(PropScale.Text, out var s) && s > 0) item.Scale = s;

            // 更新对应模型
            if (_itemModels.TryGetValue(item.Id, out var model))
            {
                model.Transform = BuildTransform(item);
                ((DiffuseMaterial)model.Material).Brush = new SolidColorBrush(ParseColor(item.Color));
            }
            RefreshItemList();
            Status("属性已应用");
        }

        private void PickColor_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.ColorDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var c = dlg.Color;
                PropColor.Text = $"#{c.R:X2}{c.G:X2}{c.B:X2}";
            }
        }

        // ========== 3D 鼠标交互（相机轨道 + 点选）==========

        private void Viewport_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _lastMouse = e.GetPosition(Viewport);
            if (e.ChangedButton == MouseButton.Right ||
                (e.ChangedButton == MouseButton.Left && Keyboard.Modifiers == ModifierKeys.Alt))
            {
                _orbiting = true;
                Viewport.CaptureMouse();
            }
            else if (e.ChangedButton == MouseButton.Left)
            {
                // 点选物品
                SelectAt(e.GetPosition(Viewport));
            }
        }

        private void Viewport_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_orbiting) return;
            var p = e.GetPosition(Viewport);
            double dx = p.X - _lastMouse.X;
            double dy = p.Y - _lastMouse.Y;
            _lastMouse = p;
            _camAzimuth -= dx * 0.5;
            _camElevation += dy * 0.5;
            if (_camElevation > 85) _camElevation = 85;
            if (_camElevation < 5) _camElevation = 5;
            UpdateCamera();
        }

        private void Viewport_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _orbiting = false;
            Viewport.ReleaseMouseCapture();
        }

        private void Viewport_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _camDistance *= e.Delta > 0 ? 0.9 : 1.1;
            if (_camDistance < 3) _camDistance = 3;
            if (_camDistance > 80) _camDistance = 80;
            UpdateCamera();
        }

        private void SelectAt(Point pt)
        {
            var hitParams = new PointHitTestParameters(pt);
            Model3D? hitModel = null;
            VisualTreeHelper.HitTest(Viewport, null, result =>
            {
                if (result is RayHitTestResult ray && ray.ModelHit is GeometryModel3D gm)
                {
                    hitModel = gm;
                    return HitTestResultBehavior.Stop;
                }
                return HitTestResultBehavior.Continue;
            }, hitParams);

            if (hitModel != null && _modelToId.TryGetValue(hitModel, out var id))
            {
                _selectedId = id;
                RefreshItemList();
                UpdatePropsPanel();
            }
            else
            {
                _selectedId = null;
                RefreshItemList();
                UpdatePropsPanel();
            }
        }

        // ========== 文件操作 ==========

        private void NewScene_Click(object sender, RoutedEventArgs e) => NewScene();

        private void OpenScene_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Filter = "室内场景文件 (*.indoor.json)|*.indoor.json|JSON 文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                DefaultExt = ".indoor.json"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var doc = SceneSerializer.Load(dlg.FileName);
                    if (doc == null) { Status("打开失败：文件内容无法解析"); return; }
                    _doc = doc;
                    _currentFilePath = dlg.FileName;
                    SceneNameBox.Text = _doc.Name;
                    _selectedId = null;
                    RebuildAllModels();
                    RefreshItemList();
                    UpdatePropsPanel();
                    Status($"已打开 {Path.GetFileName(dlg.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("打开失败：" + ex.Message);
                }
            }
        }

        private void SaveScene_Click(object sender, RoutedEventArgs e)
        {
            _doc.Name = SceneNameBox.Text;
            if (_currentFilePath == null)
            {
                SaveAsScene_Click(sender, e);
                return;
            }
            try
            {
                SceneSerializer.Save(_doc, _currentFilePath);
                Status($"已保存 {Path.GetFileName(_currentFilePath)}");
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存失败：" + ex.Message);
            }
        }

        private void SaveAsScene_Click(object sender, RoutedEventArgs e)
        {
            _doc.Name = SceneNameBox.Text;
            var dlg = new SaveFileDialog
            {
                Filter = "室内场景文件 (*.indoor.json)|*.indoor.json|JSON 文件 (*.json)|*.json",
                DefaultExt = ".indoor.json",
                FileName = (string.IsNullOrWhiteSpace(_doc.Name) ? "scene" : _doc.Name) + ".indoor.json"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    SceneSerializer.Save(_doc, dlg.FileName);
                    _currentFilePath = dlg.FileName;
                    Status($"已另存为 {Path.GetFileName(dlg.FileName)}");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("保存失败：" + ex.Message);
                }
            }
        }

        // ========== 示例场景 ==========

        private void LoadSample_Click(object sender, RoutedEventArgs e) => LoadSample();

        private void LoadSample()
        {
            _doc = new SceneDocument { Name = "示例客厅" };
            _doc.Items.Add(new SceneItem { Name = "沙发", Type = ItemType.Sofa, Position = new Vec3(0, 0, 2), Color = "#8B4513" });
            _doc.Items.Add(new SceneItem { Name = "茶几", Type = ItemType.Table, Position = new Vec3(0, 0, 0.2), Color = "#3E2723", Scale = 0.7 });
            _doc.Items.Add(new SceneItem { Name = "电视", Type = ItemType.TV, Position = new Vec3(0, 0, -2.5), RotationY = 180, Color = "#222222" });
            _doc.Items.Add(new SceneItem { Name = "电视柜", Type = ItemType.Cabinet, Position = new Vec3(0, 0, -2.2), Color = "#5D4037", Scale = 1.2 });
            _doc.Items.Add(new SceneItem { Name = "落地灯", Type = ItemType.Lamp, Position = new Vec3(-2.5, 0, 2), Color = "#FFD54F" });
            _doc.Items.Add(new SceneItem { Name = "盆栽", Type = ItemType.Plant, Position = new Vec3(2.5, 0, -2), Color = "#388E3C" });
            _doc.Items.Add(new SceneItem { Name = "单人椅", Type = ItemType.Chair, Position = new Vec3(-2, 0, 0), RotationY = 30, Color = "#6D4C41" });
            _doc.Items.Add(new SceneItem { Name = "地毯", Type = ItemType.Rug, Position = new Vec3(0, 0, 0.5), Color = "#A1887F", Scale = 1.3 });
            _selectedId = null;
            SceneNameBox.Text = _doc.Name;
            RebuildAllModels();
            RefreshItemList();
            UpdatePropsPanel();
            Status("已加载示例客厅");
        }

        // ========== 工具方法 ==========

        private static string DefaultName(ItemType type) => type switch
        {
            ItemType.Sofa => "沙发",
            ItemType.Table => "桌子",
            ItemType.Chair => "椅子",
            ItemType.Bed => "床",
            ItemType.Cabinet => "柜子",
            ItemType.Lamp => "落地灯",
            ItemType.Plant => "盆栽",
            ItemType.TV => "电视",
            ItemType.Shelf => "书架",
            ItemType.Rug => "地毯",
            _ => "物品"
        };

        private static string DefaultColor(ItemType type) => type switch
        {
            ItemType.Sofa => "#8B4513",
            ItemType.Table => "#5D4037",
            ItemType.Chair => "#6D4C41",
            ItemType.Bed => "#E0E0E0",
            ItemType.Cabinet => "#5D4037",
            ItemType.Lamp => "#FFD54F",
            ItemType.Plant => "#388E3C",
            ItemType.TV => "#222222",
            ItemType.Shelf => "#8D6E63",
            ItemType.Rug => "#A1887F",
            _ => "#888888"
        };

        private static Color ParseColor(string hex)
        {
            try
            {
                if (hex.StartsWith("#") && hex.Length >= 7)
                {
                    byte r = Convert.ToByte(hex.Substring(1, 2), 16);
                    byte g = Convert.ToByte(hex.Substring(3, 2), 16);
                    byte b = Convert.ToByte(hex.Substring(5, 2), 16);
                    return Color.FromRgb(r, g, b);
                }
            }
            catch { }
            return Colors.Gray;
        }

        private static string SanitizeColor(string input, string fallback)
        {
            if (string.IsNullOrWhiteSpace(input)) return fallback;
            var c = input.Trim();
            if (!c.StartsWith("#")) c = "#" + c;
            if (c.Length == 7 && c.Skip(1).All(ch => "0123456789abcdefABCDEF".Contains(ch)))
                return c.ToUpper();
            return fallback;
        }

        private void Status(string msg) => StatusText.Text = msg;
    }
}

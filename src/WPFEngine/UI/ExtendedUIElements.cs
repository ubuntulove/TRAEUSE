using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using WPFEngine.Math;

namespace WPFEngine.UI
{
    public class UIAbstractContainer : UIElement
    {
        protected Vector2 _containerPadding;
        protected Vector2 _contentSize;
        protected bool _fitToContent;
        protected bool _clipChildren;
        protected Alignment _childAlignment;
        protected float _spacing;

        public Vector2 ContainerPadding
        {
            get => _containerPadding;
            set { _containerPadding = value; OnLayoutChanged(); }
        }

        public Vector2 ContentSize => _contentSize;

        public bool FitToContent
        {
            get => _fitToContent;
            set { _fitToContent = value; }
        }

        public bool ClipChildren
        {
            get => _clipChildren;
            set { _clipChildren = value; }
        }

        public Alignment ChildAlignment
        {
            get => _childAlignment;
            set { _childAlignment = value; OnLayoutChanged(); }
        }

        public float Spacing
        {
            get => _spacing;
            set { _spacing = value; OnLayoutChanged(); }
        }

        public UIAbstractContainer()
        {
            _containerPadding = new Vector2(10);
            _contentSize = Vector2.Zero;
            _fitToContent = false;
            _clipChildren = false;
            _childAlignment = Alignment.Start;
            _spacing = 5;
            _backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        }

        protected override void OnLayoutChanged()
        {
            base.OnLayoutChanged();
            LayoutChildren();
        }

        protected virtual void LayoutChildren()
        {
            if (_children.Count == 0)
            {
                _contentSize = Vector2.Zero;
                return;
            }

            float totalWidth = 0;
            float totalHeight = 0;
            float maxWidth = 0;
            float maxHeight = 0;

            foreach (var child in _children)
            {
                maxWidth = Math.Max(maxWidth, child.Size.X);
                maxHeight = Math.Max(maxHeight, child.Size.Y);
            }

            _contentSize = new Vector2(maxWidth, maxHeight);

            if (_fitToContent)
            {
                CalculateFitSize();
            }
        }

        protected virtual void CalculateFitSize()
        {
            if (_children.Count == 0) return;

            float width = _containerPadding.X * 2;
            float height = _containerPadding.Y * 2;

            foreach (var child in _children)
            {
                width = Math.Max(width, child.Position.X - _position.X + child.Size.X + _containerPadding.X);
                height = Math.Max(height, child.Position.Y - _position.Y + child.Size.Y + _containerPadding.Y);
            }

            _size = new Vector2(width, height);
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            if (_clipChildren)
            {
                var clipRect = new RectangleGeometry(
                    new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y));
                context.PushClip(clipRect);

                foreach (var child in _children.OrderBy(c => c.SortingOrder))
                {
                    child.Draw(context);
                }

                context.Pop();
            }
        }
    }

    public class UIVerticalLayout : UIAbstractContainer
    {
        private bool _reverseOrder;
        private float _childForceExpandWidth;

        public bool ReverseOrder
        {
            get => _reverseOrder;
            set { _reverseOrder = value; OnLayoutChanged(); }
        }

        public float ChildForceExpandWidth
        {
            get => _childForceExpandWidth;
            set { _childForceExpandWidth = value; OnLayoutChanged(); }
        }

        public UIVerticalLayout()
        {
            _reverseOrder = false;
            _childForceExpandWidth = 0;
            _spacing = 5;
        }

        protected override void LayoutChildren()
        {
            if (_children.Count == 0)
            {
                _contentSize = Vector2.Zero;
                return;
            }

            var orderedChildren = _reverseOrder ? _children.Reverse().ToList() : _children.ToList();

            float totalHeight = 0;
            float maxWidth = 0;

            foreach (var child in orderedChildren)
            {
                maxWidth = Math.Max(maxWidth, child.Size.X);
                totalHeight += child.Size.Y;
            }

            totalHeight += (_children.Count - 1) * _spacing;

            _contentSize = new Vector2(maxWidth, totalHeight);

            float currentY = _position.Y + _containerPadding.Y;

            if (_childAlignment == Alignment.End)
            {
                currentY = _position.Y + _size.Y - totalHeight - _containerPadding.Y;
            }
            else if (_childAlignment == Alignment.Center)
            {
                currentY = _position.Y + (_size.Y - totalHeight) / 2;
            }

            foreach (var child in orderedChildren)
            {
                float childX = _position.X + _containerPadding.X;

                if (_childForceExpandWidth > 0)
                {
                    child.Size = new Vector2(_childForceExpandWidth, child.Size.Y);
                }

                if (_childAlignment == Alignment.Center)
                {
                    childX = _position.X + (_size.X - child.Size.X) / 2;
                }
                else if (_childAlignment == Alignment.End)
                {
                    childX = _position.X + _size.X - child.Size.X - _containerPadding.X;
                }

                child.Position = new Vector2(childX, currentY);
                currentY += child.Size.Y + _spacing;
            }

            if (_fitToContent)
            {
                _size = new Vector2(
                    Math.Max(_size.X, maxWidth + _containerPadding.X * 2),
                    Math.Max(_size.Y, totalHeight + _containerPadding.Y * 2));
            }
        }
    }

    public class UIHorizontalLayout : UIAbstractContainer
    {
        private bool _reverseOrder;
        private float _childForceExpandHeight;
        private float _gravity;

        public bool ReverseOrder
        {
            get => _reverseOrder;
            set { _reverseOrder = value; OnLayoutChanged(); }
        }

        public float ChildForceExpandHeight
        {
            get => _childForceExpandHeight;
            set { _childForceExpandHeight = value; OnLayoutChanged(); }
        }

        public float Gravity
        {
            get => _gravity;
            set { _gravity = value; OnLayoutChanged(); }
        }

        public UIHorizontalLayout()
        {
            _reverseOrder = false;
            _childForceExpandHeight = 0;
            _gravity = 0.5f;
            _spacing = 5;
        }

        protected override void LayoutChildren()
        {
            if (_children.Count == 0)
            {
                _contentSize = Vector2.Zero;
                return;
            }

            var orderedChildren = _reverseOrder ? _children.Reverse().ToList() : _children.ToList();

            float totalWidth = 0;
            float maxHeight = 0;

            foreach (var child in orderedChildren)
            {
                maxHeight = Math.Max(maxHeight, child.Size.Y);
                totalWidth += child.Size.X;
            }

            totalWidth += (_children.Count - 1) * _spacing;

            _contentSize = new Vector2(totalWidth, maxHeight);

            float currentX = _position.X + _containerPadding.X;

            if (_childAlignment == Alignment.End)
            {
                currentX = _position.X + _size.X - totalWidth - _containerPadding.X;
            }
            else if (_childAlignment == Alignment.Center)
            {
                currentX = _position.X + (_size.X - totalWidth) / 2;
            }

            foreach (var child in orderedChildren)
            {
                float childY = _position.Y + _containerPadding.Y;

                if (_childForceExpandHeight > 0)
                {
                    child.Size = new Vector2(child.Size.X, _childForceExpandHeight);
                }

                float alignOffset = (_size.Y - _containerPadding.Y * 2 - child.Size.Y) * _gravity;
                childY = _position.Y + _containerPadding.Y + alignOffset;

                child.Position = new Vector2(currentX, childY);
                currentX += child.Size.X + _spacing;
            }

            if (_fitToContent)
            {
                _size = new Vector2(
                    Math.Max(_size.X, totalWidth + _containerPadding.X * 2),
                    Math.Max(_size.Y, maxHeight + _containerPadding.Y * 2));
            }
        }
    }

    public class UIGridLayout : UIAbstractContainer
    {
        private int _columns;
        private int _rows;
        private Vector2 _cellSize;
        private Vector2 _spacing;
        private int _constraintColumns;
        private int _constraintRows;
        private bool _autoFitColumns;
        private bool _autoFitRows;

        public int Columns
        {
            get => _columns;
            set { _columns = Math.Max(1, value); OnLayoutChanged(); }
        }

        public int Rows
        {
            get => _rows;
            set { _rows = Math.Max(1, value); OnLayoutChanged(); }
        }

        public Vector2 CellSize
        {
            get => _cellSize;
            set { _cellSize = value; OnLayoutChanged(); }
        }

        public Vector2 Spacing
        {
            get => _spacing;
            set { _spacing = value; OnLayoutChanged(); }
        }

        public int ConstraintColumns
        {
            get => _constraintColumns;
            set { _constraintColumns = Math.Max(1, value); }
        }

        public int ConstraintRows
        {
            get => _constraintRows;
            set { _constraintRows = Math.Max(1, value); }
        }

        public bool AutoFitColumns
        {
            get => _autoFitColumns;
            set { _autoFitColumns = value; OnLayoutChanged(); }
        }

        public bool AutoFitRows
        {
            get => _autoFitRows;
            set { _autoFitRows = value; OnLayoutChanged(); }
        }

        public UIGridLayout()
        {
            _columns = 4;
            _rows = 0;
            _cellSize = new Vector2(100, 50);
            _spacing = new Vector2(5);
            _constraintColumns = 1;
            _constraintRows = 1;
            _autoFitColumns = false;
            _autoFitRows = true;
        }

        protected override void LayoutChildren()
        {
            if (_children.Count == 0)
            {
                _contentSize = Vector2.Zero;
                return;
            }

            int actualRows = _autoFitRows ? (int)Math.Ceiling((float)_children.Count / _columns) : _rows;
            if (actualRows == 0) actualRows = 1;

            float totalWidth = _columns * _cellSize.X + (_columns - 1) * _spacing.X;
            float totalHeight = actualRows * _cellSize.Y + (actualRows - 1) * _spacing.Y;

            _contentSize = new Vector2(totalWidth, totalHeight);

            for (int i = 0; i < _children.Count; i++)
            {
                int col = i % _columns;
                int row = i / _columns;

                var child = _children[i];
                child.Size = _cellSize;

                float posX = _position.X + _containerPadding.X + col * (_cellSize.X + _spacing.X);
                float posY = _position.Y + _containerPadding.Y + row * (_cellSize.Y + _spacing.Y);

                child.Position = new Vector2(posX, posY);
            }

            if (_fitToContent)
            {
                _size = new Vector2(
                    Math.Max(_size.X, totalWidth + _containerPadding.X * 2),
                    Math.Max(_size.Y, totalHeight + _containerPadding.Y * 2));
            }
        }
    }

    public class UICollapsiblePanel : UIAbstractContainer
    {
        private bool _isExpanded;
        private float _animationSpeed;
        private float _currentHeight;
        private float _targetHeight;
        private float _headerHeight;
        private UILabel _headerLabel;
        private UIButton _toggleButton;
        private bool _isAnimating;
        private Color _headerColor;
        private Color _expandedHeaderColor;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                _targetHeight = _isExpanded ? CalculateContentHeight() : 0;
                _isAnimating = true;
                CollapsedChanged?.Invoke(this, _isExpanded);
            }
        }

        public float AnimationSpeed
        {
            get => _animationSpeed;
            set { _animationSpeed = Math.Max(0.1f, value); }
        }

        public float HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = value; }
        }

        public UILabel HeaderLabel => _headerLabel;

        public UIButton ToggleButton => _toggleButton;

        public Color HeaderColor
        {
            get => _headerColor;
            set { _headerColor = value; }
        }

        public Color ExpandedHeaderColor
        {
            get => _expandedHeaderColor;
            set { _expandedHeaderColor = value; }
        }

        public event EventHandler<bool>? CollapsedChanged;

        public UICollapsiblePanel()
        {
            _isExpanded = true;
            _animationSpeed = 5f;
            _currentHeight = 0;
            _targetHeight = 0;
            _headerHeight = 30;
            _isAnimating = false;
            _headerColor = new Color(0.3f, 0.3f, 0.3f);
            _expandedHeaderColor = new Color(0.4f, 0.4f, 0.4f);

            _headerLabel = new UILabel("Panel Header");
            _headerLabel.Size = new Vector2(200, _headerHeight);
            _headerLabel.BackgroundColor = _headerColor;
            AddChild(_headerLabel);

            _toggleButton = new UIButton();
            _toggleButton.Size = new Vector2(30, _headerHeight);
            _toggleButton.Text = _isExpanded ? "-" : "+";
            _toggleButton.Click += (s, e) => IsExpanded = !IsExpanded;
            AddChild(_toggleButton);

            _size = new Vector2(200, 100);
            _backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        }

        private float CalculateContentHeight()
        {
            float contentHeight = 0;
            foreach (var child in _children)
            {
                if (child != _headerLabel && child != _toggleButton)
                {
                    contentHeight += child.Size.Y + _spacing;
                }
            }
            return contentHeight;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _headerLabel.Size = new Vector2(_size.X - _headerHeight - 5, _headerHeight);
            _toggleButton.Position = new Vector2(_position.X + _size.X - _headerHeight, _position.Y);
            _toggleButton.Size = new Vector2(_headerHeight, _headerHeight);

            if (_isAnimating)
            {
                float heightDiff = _targetHeight - _currentHeight;
                if (Math.Abs(heightDiff) < 0.1f)
                {
                    _currentHeight = _targetHeight;
                    _isAnimating = false;
                }
                else
                {
                    _currentHeight += heightDiff * _animationSpeed * deltaTime;
                }

                float newHeight = _headerHeight + _currentHeight;
                if (Math.Abs(_size.Y - newHeight) > 0.1f)
                {
                    _size = new Vector2(_size.X, newHeight);
                }
            }
        }

        protected override void LayoutChildren()
        {
            if (_children.Count == 0) return;

            float contentY = _position.Y + _headerHeight + _spacing;

            foreach (var child in _children)
            {
                if (child == _headerLabel || child == _toggleButton) continue;

                child.Position = new Vector2(
                    _position.X + _containerPadding.X,
                    contentY);
                contentY += child.Size.Y + _spacing;
            }
        }
    }

    public class UITabControl : UIElement
    {
        private List<UITabPage> _pages;
        private int _selectedIndex;
        private UIPanel _tabHeaderContainer;
        private UIPanel _tabContentContainer;
        private float _tabHeight;
        private float _tabWidth;
        private Color _activeTabColor;
        private Color _inactiveTabColor;
        private Orientation _tabOrientation;

        public List<UITabPage> Pages => _pages;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= 0 && value < _pages.Count)
                {
                    _selectedIndex = value;
                    UpdateTabVisibility();
                    SelectionChanged?.Invoke(this, _selectedIndex);
                }
            }
        }

        public UITabPage? SelectedPage => _selectedIndex >= 0 && _selectedIndex < _pages.Count
            ? _pages[_selectedIndex] : null;

        public float TabHeight
        {
            get => _tabHeight;
            set { _tabHeight = value; }
        }

        public float TabWidth
        {
            get => _tabWidth;
            set { _tabWidth = value; }
        }

        public Color ActiveTabColor
        {
            get => _activeTabColor;
            set { _activeTabColor = value; }
        }

        public Color InactiveTabColor
        {
            get => _inactiveTabColor;
            set { _inactiveTabColor = value; }
        }

        public Orientation TabOrientation
        {
            get => _tabOrientation;
            set { _tabOrientation = value; OnLayoutChanged(); }
        }

        public event EventHandler<int>? SelectionChanged;

        public UITabControl()
        {
            _pages = new List<UITabPage>();
            _selectedIndex = -1;
            _tabHeight = 30;
            _tabWidth = 100;
            _activeTabColor = new Color(0.4f, 0.4f, 0.4f);
            _inactiveTabColor = new Color(0.25f, 0.25f, 0.25f);
            _tabOrientation = Orientation.Horizontal;

            _tabHeaderContainer = new UIPanel();
            _tabHeaderContainer.BackgroundColor = new Color(0.15f, 0.15f, 0.15f);

            _tabContentContainer = new UIPanel();
            _tabContentContainer.BackgroundColor = new Color(0.2f, 0.2f, 0.2f);
            _tabContentContainer.Position = new Vector2(0, _tabHeight);

            _size = new Vector2(300, 200);
        }

        public UITabPage AddTab(string title)
        {
            var page = new UITabPage(title);
            _pages.Add(page);

            var tabButton = new UIButton(title);
            tabButton.Size = new Vector2(_tabWidth, _tabHeight);
            tabButton.BackgroundColor = _inactiveTabColor;
            int index = _pages.Count - 1;
            tabButton.Click += (s, e) => SelectedIndex = index;
            page.TabButton = tabButton;

            if (_selectedIndex == -1)
            {
                _selectedIndex = 0;
            }

            UpdateTabVisibility();
            return page;
        }

        public void RemoveTab(int index)
        {
            if (index < 0 || index >= _pages.Count) return;

            _pages.RemoveAt(index);

            if (_selectedIndex >= _pages.Count)
            {
                _selectedIndex = _pages.Count - 1;
            }

            UpdateTabVisibility();
        }

        private void UpdateTabVisibility()
        {
            for (int i = 0; i < _pages.Count; i++)
            {
                var page = _pages[i];
                if (page.TabButton != null)
                {
                    page.TabButton.BackgroundColor = i == _selectedIndex ? _activeTabColor : _inactiveTabColor;
                }
                page.IsVisible = i == _selectedIndex;
            }
        }

        protected override void LayoutChildren()
        {
            base.LayoutChildren();

            _tabHeaderContainer.Size = new Vector2(_size.X, _tabHeight);

            for (int i = 0; i < _pages.Count; i++)
            {
                if (_pages[i].TabButton != null)
                {
                    if (_tabOrientation == Orientation.Horizontal)
                    {
                        _pages[i].TabButton!.Position = new Vector2(
                            _position.X + i * (_tabWidth + 2),
                            _position.Y);
                    }
                    else
                    {
                        _pages[i].TabButton!.Position = new Vector2(
                            _position.X,
                            _position.Y + i * _tabHeight);
                    }
                }
            }

            _tabContentContainer.Size = new Vector2(_size.X, _size.Y - _tabHeight);
            _tabContentContainer.Position = new Vector2(
                _position.X,
                _position.Y + _tabHeight);
        }
    }

    public class UITabPage : UIPanel
    {
        private string _title;
        private UIButton? _tabButton;
        private bool _canClose;

        public string Title
        {
            get => _title;
            set { _title = value; if (_tabButton != null) _tabButton.Text = value; }
        }

        public UIButton? TabButton
        {
            get => _tabButton;
            set => _tabButton = value;
        }

        public bool CanClose
        {
            get => _canClose;
            set => _canClose = value;
        }

        public UITabPage()
        {
            _title = "Tab";
            _canClose = false;
        }

        public UITabPage(string title)
        {
            _title = title;
            _canClose = false;
        }
    }

    public class UITreeView : UIElement
    {
        private List<UITreeViewNode> _rootNodes;
        private UITreeViewNode? _selectedNode;
        private float _indentWidth;
        private float _rowHeight;
        private float _contentHeight;
        private UIPanel _contentPanel;
        private bool _showRootLines;
        private bool _showExpandIcons;

        public List<UITreeViewNode> RootNodes => _rootNodes;
        public UITreeViewNode? SelectedNode => _selectedNode;
        public float IndentWidth
        {
            get => _indentWidth;
            set { _indentWidth = value; }
        }
        public float RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = value; }
        }
        public bool ShowRootLines
        {
            get => _showRootLines;
            set { _showRootLines = value; }
        }
        public bool ShowExpandIcons
        {
            get => _showExpandIcons;
            set { _showExpandIcons = value; }
        }

        public event EventHandler<UITreeViewNode>? SelectionChanged;
        public event EventHandler<UITreeViewNode>? NodeExpanded;
        public event EventHandler<UITreeViewNode>? NodeCollapsed;

        public UITreeView()
        {
            _rootNodes = new List<UITreeViewNode>();
            _indentWidth = 20;
            _rowHeight = 25;
            _contentHeight = 0;
            _showRootLines = true;
            _showExpandIcons = true;
            _backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            _size = new Vector2(200, 300);
        }

        public UITreeViewNode AddNode(string text, UITreeViewNode? parent = null)
        {
            var node = new UITreeViewNode(text);

            if (parent != null)
            {
                parent.AddChild(node);
            }
            else
            {
                _rootNodes.Add(node);
            }

            CalculateContentHeight();
            return node;
        }

        public void RemoveNode(UITreeViewNode node)
        {
            if (node.Parent != null)
            {
                node.Parent.RemoveChild(node);
            }
            else
            {
                _rootNodes.Remove(node);
            }

            if (_selectedNode == node)
            {
                _selectedNode = null;
            }

            CalculateContentHeight();
        }

        public void SelectNode(UITreeViewNode node)
        {
            _selectedNode = node;
            SelectionChanged?.Invoke(this, node);
        }

        public void ExpandAll()
        {
            foreach (var root in _rootNodes)
            {
                root.ExpandAll();
            }
            CalculateContentHeight();
        }

        public void CollapseAll()
        {
            foreach (var root in _rootNodes)
            {
                root.CollapseAll();
            }
            CalculateContentHeight();
        }

        private void CalculateContentHeight()
        {
            _contentHeight = 0;
            CalculateNodeHeights(_rootNodes, 0);
        }

        private void CalculateNodeHeights(List<UITreeViewNode> nodes, int depth)
        {
            foreach (var node in nodes)
            {
                _contentHeight += _rowHeight;
                if (node.IsExpanded)
                {
                    CalculateNodeHeights(node.Children, depth + 1);
                }
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            CalculateContentHeight();
        }
    }

    public class UITreeViewNode
    {
        private string _text;
        private List<UITreeViewNode> _children;
        private UITreeViewNode? _parent;
        private bool _isExpanded;
        private bool _isSelected;
        private bool _isVisible;
        private UIButton? _expandButton;
        private UILabel? _label;
        private object? _tag;

        public string Text
        {
            get => _text;
            set { _text = value; if (_label != null) _label.Text = value; }
        }

        public List<UITreeViewNode> Children => _children;
        public UITreeViewNode? Parent => _parent;
        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; }
        }
        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; }
        }
        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; }
        }
        public UIButton? ExpandButton => _expandButton;
        public UILabel? Label => _label;
        public object? Tag
        {
            get => _tag;
            set => _tag = value;
        }

        public UITreeViewNode(string text)
        {
            _text = text;
            _children = new List<UITreeViewNode>();
            _isExpanded = false;
            _isSelected = false;
            _isVisible = true;
        }

        public void AddChild(UITreeViewNode child)
        {
            child._parent = this;
            _children.Add(child);
        }

        public void RemoveChild(UITreeViewNode child)
        {
            if (_children.Remove(child))
            {
                child._parent = null;
            }
        }

        public void ExpandAll()
        {
            _isExpanded = true;
            foreach (var child in _children)
            {
                child.ExpandAll();
            }
        }

        public void CollapseAll()
        {
            _isExpanded = false;
            foreach (var child in _children)
            {
                child.CollapseAll();
            }
        }

        public int GetDepth()
        {
            int depth = 0;
            var current = _parent;
            while (current != null)
            {
                depth++;
                current = current.Parent;
            }
            return depth;
        }
    }

    public class UIListView : UIAbstractContainer
    {
        private List<UIListItem> _items;
        private int _selectedIndex;
        private UIListItem? _selectedItem;
        private List<UIListItem> _selectedItems;
        private bool _allowMultiSelect;
        private float _itemHeight;
        private float _contentHeight;
        private UIScrollBar? _scrollBar;

        public List<UIListItem> Items => _items;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= -1 && value < _items.Count)
                {
                    _selectedIndex = value;
                    _selectedItem = value >= 0 ? _items[value] : null;
                    SelectionChanged?.Invoke(this, value);
                }
            }
        }

        public UIListItem? SelectedItem => _selectedItem;
        public List<UIListItem> SelectedItems => _selectedItems;
        public bool AllowMultiSelect
        {
            get => _allowMultiSelect;
            set { _allowMultiSelect = value; }
        }

        public float ItemHeight
        {
            get => _itemHeight;
            set { _itemHeight = value; OnLayoutChanged(); }
        }

        public event EventHandler<int>? SelectionChanged;

        public UIListView()
        {
            _items = new List<UIListItem>();
            _selectedIndex = -1;
            _selectedItem = null;
            _selectedItems = new List<UIListItem>();
            _allowMultiSelect = false;
            _itemHeight = 30;
            _contentHeight = 0;
            _backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            _size = new Vector2(200, 200);
        }

        public UIListItem AddItem(string text, object? value = null)
        {
            var item = new UIListItem(text, value);
            _items.Add(item);
            OnLayoutChanged();
            return item;
        }

        public void RemoveItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                _items.RemoveAt(index);
                if (_selectedIndex >= index)
                {
                    _selectedIndex--;
                }
                OnLayoutChanged();
            }
        }

        public void ClearItems()
        {
            _items.Clear();
            _selectedIndex = -1;
            _selectedItem = null;
            _selectedItems.Clear();
            OnLayoutChanged();
        }

        protected override void LayoutChildren()
        {
            _contentHeight = _items.Count * _itemHeight;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                item.Position = new Vector2(_position.X + _containerPadding.X, _position.Y + _containerPadding.Y + i * _itemHeight);
                item.Size = new Vector2(_size.X - _containerPadding.X * 2, _itemHeight);
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_scrollBar != null)
            {
                float maxScroll = Math.Max(0, _contentHeight - _size.Y);
                _scrollBar!.ScrollPosition = Math.Clamp(_scrollBar!.ScrollPosition, 0, maxScroll);
            }
        }
    }

    public class UIListItem : UIElement
    {
        private string _text;
        private object? _value;
        private bool _isSelected;
        private Color _selectedColor;
        private Color _hoverColor;
        private bool _isHovered;

        public string Text
        {
            get => _text;
            set { _text = value; }
        }

        public object? Value
        {
            get => _value;
            set { _value = value; }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set { _isSelected = value; UpdateAppearance(); }
        }

        public Color SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; }
        }

        public Color HoverColor
        {
            get => _hoverColor;
            set { _hoverColor = value; }
        }

        public UIListItem()
        {
            _text = "";
            _selectedColor = new Color(0.3f, 0.4f, 0.6f);
            _hoverColor = new Color(0.3f, 0.3f, 0.3f);
            _isHovered = false;
            _backgroundColor = Color.Transparent;
            _size = new Vector2(200, 30);
        }

        public UIListItem(string text, object? value = null)
        {
            _text = text;
            _value = value;
            _selectedColor = new Color(0.3f, 0.4f, 0.6f);
            _hoverColor = new Color(0.3f, 0.3f, 0.3f);
            _isHovered = false;
            _backgroundColor = Color.Transparent;
            _size = new Vector2(200, 30);
        }

        public override void OnPointerEnter()
        {
            _isHovered = true;
            UpdateAppearance();
        }

        public override void OnPointerExit()
        {
            _isHovered = false;
            UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            if (_isSelected)
            {
                _backgroundColor = _selectedColor;
            }
            else if (_isHovered)
            {
                _backgroundColor = _hoverColor;
            }
            else
            {
                _backgroundColor = Color.Transparent;
            }
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            var typeface = new Typeface("Segoe UI");
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
            var text = new FormattedText(_text, System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight, typeface, 12, brush, 1.0);

            context.DrawText(text, new System.Windows.Point(_position.X + 10, _position.Y + (_size.Y - text.Height) / 2));
        }
    }

    public class UICarousel : UIElement
    {
        private List<UIElement> _items;
        private int _currentIndex;
        private float _transitionDuration;
        private float _currentTransition;
        private bool _isTransitioning;
        private int _targetIndex;
        private float _itemWidth;

        public List<UIElement> Items => _items;
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (value >= 0 && value < _items.Count)
                {
                    _currentIndex = value;
                    IndexChanged?.Invoke(this, _currentIndex);
                }
            }
        }

        public UIElement? CurrentItem => _currentIndex >= 0 && _currentIndex < _items.Count
            ? _items[_currentIndex] : null;

        public float TransitionDuration
        {
            get => _transitionDuration;
            set { _transitionDuration = value; }
        }

        public float ItemWidth
        {
            get => _itemWidth;
            set { _itemWidth = value; }
        }

        public event EventHandler<int>? IndexChanged;

        public UICarousel()
        {
            _items = new List<UIElement>();
            _currentIndex = 0;
            _transitionDuration = 0.3f;
            _currentTransition = 0;
            _isTransitioning = false;
            _targetIndex = 0;
            _itemWidth = 200;
            _size = new Vector2(600, 400);
        }

        public void AddItem(UIElement item)
        {
            _items.Add(item);
            UpdateItemPositions();
        }

        public void RemoveItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                _items.RemoveAt(index);
                if (_currentIndex >= _items.Count)
                {
                    _currentIndex = Math.Max(0, _items.Count - 1);
                }
                UpdateItemPositions();
            }
        }

        public void Next()
        {
            if (_currentIndex < _items.Count - 1)
            {
                _targetIndex = _currentIndex + 1;
                StartTransition();
            }
        }

        public void Previous()
        {
            if (_currentIndex > 0)
            {
                _targetIndex = _currentIndex - 1;
                StartTransition();
            }
        }

        public void GoTo(int index)
        {
            if (index >= 0 && index < _items.Count && index != _currentIndex)
            {
                _targetIndex = index;
                StartTransition();
            }
        }

        private void StartTransition()
        {
            _isTransitioning = true;
            _currentTransition = 0;
        }

        private void UpdateItemPositions()
        {
            float offsetX = _position.X;

            for (int i = 0; i < _items.Count; i++)
            {
                var item = _items[i];
                float itemOffset = (i - _currentIndex) * _itemWidth;

                if (_isTransitioning)
                {
                    float targetOffset = (i - _targetIndex) * _itemWidth;
                    itemOffset = Math.Lerp(itemOffset, targetOffset, _currentTransition);
                }

                item.Position = new Vector2(offsetX + itemOffset, _position.Y);
                item.Size = new Vector2(_itemWidth, _size.Y);
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_isTransitioning)
            {
                _currentTransition += deltaTime / _transitionDuration;

                if (_currentTransition >= 1.0f)
                {
                    _currentTransition = 1.0f;
                    _currentIndex = _targetIndex;
                    _isTransitioning = false;
                }

                UpdateItemPositions();
            }
        }
    }

    public class UITextArea : UIElement
    {
        private string _text;
        private string _placeholder;
        private int _maxLength;
        private bool _readOnly;
        private int _caretPosition;
        private Color _caretColor;
        private string? _fontFamily;
        private float _fontSize;
        private float _lineHeight;

        public string Text
        {
            get => _text;
            set { _text = value.Length > _maxLength ? value.Substring(0, _maxLength) : value; }
        }

        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value; }
        }

        public int MaxLength
        {
            get => _maxLength;
            set { _maxLength = Math.Max(1, value); }
        }

        public bool ReadOnly
        {
            get => _readOnly;
            set { _readOnly = value; }
        }

        public int CaretPosition
        {
            get => _caretPosition;
            set { _caretPosition = Math.Clamp(value, 0, _text.Length); }
        }

        public Color CaretColor
        {
            get => _caretColor;
            set { _caretColor = value; }
        }

        public string? FontFamily
        {
            get => _fontFamily;
            set { _fontFamily = value; }
        }

        public float FontSize
        {
            get => _fontSize;
            set { _fontSize = value; }
        }

        public float LineHeight
        {
            get => _lineHeight;
            set { _lineHeight = value; }
        }

        public event EventHandler<string>? TextChanged;

        public UITextArea()
        {
            _text = "";
            _placeholder = "Enter text...";
            _maxLength = 10000;
            _readOnly = false;
            _caretPosition = 0;
            _caretColor = Color.White;
            _fontFamily = "Segoe UI";
            _fontSize = 14;
            _lineHeight = 20;
            _backgroundColor = new Color(0.15f, 0.15f, 0.15f);
            _borderColor = new Color(0.4f, 0.4f, 0.4f);
            _borderWidth = 1;
            _size = new Vector2(300, 200);
        }

        public void AppendText(string text)
        {
            if (_readOnly) return;

            int spaceAvailable = _maxLength - _text.Length;
            int charsToAdd = Math.Min(text.Length, spaceAvailable);

            if (charsToAdd > 0)
            {
                _text += text.Substring(0, charsToAdd);
                _caretPosition = _text.Length;
                TextChanged?.Invoke(this, _text);
            }
        }

        public void Clear()
        {
            _text = "";
            _caretPosition = 0;
            TextChanged?.Invoke(this, _text);
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            var typeface = new Typeface(_fontFamily ?? "Segoe UI");
            var textColor = string.IsNullOrEmpty(_text) ? new Color(0.5f, 0.5f, 0.5f) : Color.White;
            var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                (byte)(textColor.R * 255), (byte)(textColor.G * 255), (byte)(textColor.B * 255)));

            string[] lines = _text.Split('\n');
            float yOffset = _position.Y + 10;

            foreach (var line in lines)
            {
                var formattedText = new FormattedText(line, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, typeface, _fontSize, brush, 1.0);

                context.DrawText(formattedText, new System.Windows.Point(_position.X + 10, yOffset));
                yOffset += _lineHeight;

                if (yOffset > _position.Y + _size.Y - _lineHeight)
                    break;
            }

            if (string.IsNullOrEmpty(_text) && !string.IsNullOrEmpty(_placeholder))
            {
                var placeholderBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(128, 128, 128, 128));
                var placeholderText = new FormattedText(_placeholder, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, typeface, _fontSize, placeholderBrush, 1.0);

                context.DrawText(placeholderText, new System.Windows.Point(_position.X + 10, _position.Y + 10));
            }
        }
    }

    public class UIColorPicker : UIElement
    {
        private Color _selectedColor;
        private float _hue;
        private float _saturation;
        private float _value;
        private UIPanel _colorPreview;
        private UISlider _hueSlider;
        private UISlider _saturationSlider;
        private UISlider _valueSlider;
        private UILabel _hexLabel;
        private bool _showAlpha;

        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                RGBToHSV(value, out _hue, out _saturation, out _value);
                ColorChanged?.Invoke(this, _selectedColor);
            }
        }

        public float Hue
        {
            get => _hue;
            set { _hue = value; UpdateColorFromHSV(); }
        }

        public float Saturation
        {
            get => _saturation;
            set { _saturation = value; UpdateColorFromHSV(); }
        }

        public float Value
        {
            get => _value;
            set { _value = value; UpdateColorFromHSV(); }
        }

        public bool ShowAlpha
        {
            get => _showAlpha;
            set { _showAlpha = value; }
        }

        public event EventHandler<Color>? ColorChanged;

        public UIColorPicker()
        {
            _selectedColor = Color.White;
            _hue = 0;
            _saturation = 1;
            _value = 1;
            _showAlpha = false;
            _size = new Vector2(250, 300);
            _backgroundColor = new Color(0.2f, 0.2f, 0.2f);

            _colorPreview = new UIPanel();
            _colorPreview.Size = new Vector2(_size.X - 20, 60);
            _colorPreview.BackgroundColor = _selectedColor;

            _hueSlider = new UISlider();
            _hueSlider.Size = new Vector2(_size.X - 20, 20);
            _hueSlider.MinValue = 0;
            _hueSlider.MaxValue = 360;
            _hueSlider.Value = 0;

            _saturationSlider = new UISlider();
            _saturationSlider.Size = new Vector2(_size.X - 20, 20);
            _saturationSlider.MinValue = 0;
            _saturationSlider.MaxValue = 1;
            _saturationSlider.Value = 1;

            _valueSlider = new UISlider();
            _valueSlider.Size = new Vector2(_size.X - 20, 20);
            _valueSlider.MinValue = 0;
            _valueSlider.MaxValue = 1;
            _valueSlider.Value = 1;

            _hexLabel = new UILabel("#FFFFFF");
            _hexLabel.Size = new Vector2(_size.X - 20, 25);
        }

        private void UpdateColorFromHSV()
        {
            _selectedColor = HSVToRGB(_hue, _saturation, _value);
            ColorChanged?.Invoke(this, _selectedColor);
        }

        private void RGBToHSV(Color color, out float hue, out float saturation, out float value)
        {
            float r = color.R;
            float g = color.G;
            float b = color.B;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            value = max;

            if (delta == 0)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                saturation = delta / max;

                if (max == r)
                    hue = 60 * (((g - b) / delta) % 6);
                else if (max == g)
                    hue = 60 * (((b - r) / delta) + 2);
                else
                    hue = 60 * (((r - g) / delta) + 4);

                if (hue < 0) hue += 360;
            }
        }

        private Color HSVToRGB(float hue, float saturation, float value)
        {
            float c = value * saturation;
            float x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = value - c;

            float r, g, b;

            if (hue < 60) { r = c; g = x; b = 0; }
            else if (hue < 120) { r = x; g = c; b = 0; }
            else if (hue < 180) { r = 0; g = c; b = x; }
            else if (hue < 240) { r = 0; g = x; b = c; }
            else if (hue < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return new Color(r + m, g + m, b + m);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _colorPreview.BackgroundColor = _selectedColor;
            _hexLabel.Text = $"#{ToHex(_selectedColor)}";
        }

        private string ToHex(Color color)
        {
            int r = (int)(color.R * 255);
            int g = (int)(color.G * 255);
            int b = (int)(color.B * 255);
            return $"{r:X2}{g:X2}{b:X2}";
        }
    }

    public class UICircularProgress : UIElement
    {
        private float _value;
        private float _minValue;
        private float _maxValue;
        private float _startAngle;
        private float _thickness;
        private Color _trackColor;
        private Color _progressColor;
        private bool _showPercentage;
        private UILabel? _percentageLabel;

        public float Value
        {
            get => _value;
            set { _value = Math.Clamp(value, _minValue, _maxValue); }
        }

        public float MinValue
        {
            get => _minValue;
            set { _minValue = value; }
        }

        public float MaxValue
        {
            get => _maxValue;
            set { _maxValue = value; }
        }

        public float StartAngle
        {
            get => _startAngle;
            set { _startAngle = value; }
        }

        public float Thickness
        {
            get => _thickness;
            set { _thickness = value; }
        }

        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; }
        }

        public Color ProgressColor
        {
            get => _progressColor;
            set { _progressColor = value; }
        }

        public bool ShowPercentage
        {
            get => _showPercentage;
            set { _showPercentage = value; }
        }

        public float Percentage => (_value - _minValue) / (_maxValue - _minValue) * 100;

        public UICircularProgress()
        {
            _value = 50;
            _minValue = 0;
            _maxValue = 100;
            _startAngle = -90;
            _thickness = 10;
            _trackColor = new Color(0.3f, 0.3f, 0.3f);
            _progressColor = new Color(0.2f, 0.6f, 0.9f);
            _showPercentage = true;
            _size = new Vector2(100, 100);
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            float centerX = _position.X + _size.X / 2;
            float centerY = _position.Y + _size.Y / 2;
            float radius = Math.Min(_size.X, _size.Y) / 2 - _thickness / 2;

            var trackPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                _trackColor.GetAByte(), _trackColor.GetRByte(), _trackColor.GetGByte(), _trackColor.GetBByte())), _thickness);

            context.DrawEllipse(null, trackPen, new System.Windows.Point(centerX, centerY), radius, radius);

            float percentage = Percentage / 100f;
            float sweepAngle = percentage * 360;

            if (sweepAngle > 0)
            {
                var progressPen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    _progressColor.GetAByte(), _progressColor.GetRByte(), _progressColor.GetGByte(), _progressColor.GetBByte())), _thickness);

                var startAngleRad = _startAngle * Math.PI / 180;
                var startPoint = new System.Windows.Point(
                    centerX + radius * (float)Math.Cos(startAngleRad),
                    centerY + radius * (float)Math.Sin(startAngleRad));

                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(new System.Windows.Point(centerX, centerY), true, true);
                    ctx.LineTo(startPoint, false, false);
                }

                context.DrawGeometry(null, progressPen, geometry);
            }

            if (_showPercentage)
            {
                var typeface = new Typeface("Segoe UI");
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                var text = new FormattedText($"{Percentage:F0}%", System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, typeface, _size.X * 0.2f, brush, 1.0);

                context.DrawText(text, new System.Windows.Point(
                    centerX - text.Width / 2,
                    centerY - text.Height / 2));
            }
        }
    }

    public class UIScrollBar : UIElement
    {
        private float _scrollPosition;
        private float _viewportSize;
        private float _contentSize;
        private float _scrollRange;
        private Orientation _orientation;
        private float _thumbSize;
        private float _thumbPosition;
        private bool _isDragging;
        private float _dragOffset;
        private Color _thumbColor;
        private Color _trackColor;

        public float ScrollPosition
        {
            get => _scrollPosition;
            set
            {
                _scrollPosition = Math.Clamp(value, 0, Math.Max(0, _scrollRange));
                PositionChanged?.Invoke(this, _scrollPosition);
            }
        }

        public float ViewportSize
        {
            get => _viewportSize;
            set { _viewportSize = value; UpdateThumb(); }
        }

        public float ContentSize
        {
            get => _contentSize;
            set
            {
                _contentSize = value;
                _scrollRange = Math.Max(0, _contentSize - _viewportSize);
                UpdateThumb();
            }
        }

        public Orientation Orientation
        {
            get => _orientation;
            set { _orientation = value; }
        }

        public Color ThumbColor
        {
            get => _thumbColor;
            set { _thumbColor = value; }
        }

        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; }
        }

        public event EventHandler<float>? PositionChanged;

        public UIScrollBar()
        {
            _scrollPosition = 0;
            _viewportSize = 100;
            _contentSize = 1000;
            _scrollRange = 900;
            _orientation = Orientation.Vertical;
            _thumbSize = 20;
            _thumbPosition = 0;
            _isDragging = false;
            _dragOffset = 0;
            _thumbColor = new Color(0.5f, 0.5f, 0.5f);
            _trackColor = new Color(0.2f, 0.2f, 0.2f);
            _size = new Vector2(15, 200);
        }

        private void UpdateThumb()
        {
            if (_scrollRange <= 0)
            {
                _thumbSize = _orientation == Orientation.Vertical ? _size.Y : _size.X;
            }
            else
            {
                float ratio = _viewportSize / _contentSize;
                float trackLength = (_orientation == Orientation.Vertical ? _size.Y : _size.X) - 10;
                _thumbSize = Math.Max(20, trackLength * ratio);
            }

            float trackLength2 = (_orientation == Orientation.Vertical ? _size.Y : _size.X) - _thumbSize - 5;
            float thumbRatio = _scrollRange > 0 ? _scrollPosition / _scrollRange : 0;
            _thumbPosition = 5 + thumbRatio * trackLength2;
        }

        public void ScrollTo(float position)
        {
            ScrollPosition = position;
        }

        public void ScrollBy(float delta)
        {
            ScrollPosition += delta;
        }

        public void ScrollToTop()
        {
            ScrollPosition = 0;
        }

        public void ScrollToBottom()
        {
            ScrollPosition = _scrollRange;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdateThumb();
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            var trackBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                _trackColor.GetAByte(), _trackColor.GetRByte(), _trackColor.GetGByte(), _trackColor.GetBByte()));

            if (_orientation == Orientation.Vertical)
            {
                context.DrawRectangle(trackBrush, null,
                    new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y));

                var thumbBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    _thumbColor.GetAByte(), _thumbColor.GetRByte(), _thumbColor.GetGByte(), _thumbColor.GetBByte()));

                context.DrawRectangle(thumbBrush, null,
                    new System.Windows.Rect(_position.X + 2, _position.Y + _thumbPosition, _size.X - 4, _thumbSize));
            }
            else
            {
                context.DrawRectangle(trackBrush, null,
                    new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y));

                var thumbBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    _thumbColor.GetAByte(), _thumbColor.GetRByte(), _thumbColor.GetGByte(), _thumbColor.GetBByte()));

                context.DrawRectangle(thumbBrush, null,
                    new System.Windows.Rect(_position.X + _thumbPosition, _position.Y + 2, _thumbSize, _size.Y - 4));
            }
        }
    }

    public class UISearchBox : UIElement
    {
        private string _searchText;
        private string _placeholder;
        private UITextField _inputField;
        private UIButton _searchButton;
        private UIButton _clearButton;
        private List<string> _suggestions;
        private UIPanel _suggestionPanel;
        private bool _showSuggestions;
        private float _maxSuggestions;

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                _inputField.Text = value;
                SearchTextChanged?.Invoke(this, _searchText);
            }
        }

        public string Placeholder
        {
            get => _placeholder;
            set { _placeholder = value; _inputField.Placeholder = value; }
        }

        public List<string> Suggestions => _suggestions;

        public bool ShowSuggestions
        {
            get => _showSuggestions;
            set { _showSuggestions = value; _suggestionPanel.IsVisible = _showSuggestions && _suggestions.Count > 0; }
        }

        public float MaxSuggestions
        {
            get => _maxSuggestions;
            set { _maxSuggestions = value; }
        }

        public event EventHandler<string>? SearchTextChanged;
        public event EventHandler<string>? SearchSubmitted;

        public UISearchBox()
        {
            _searchText = "";
            _placeholder = "Search...";
            _suggestions = new List<string>();
            _showSuggestions = false;
            _maxSuggestions = 5;
            _size = new Vector2(250, 35);

            _inputField = new UITextField();
            _inputField.Size = new Vector2(_size.X - 70, _size.Y);
            _inputField.Placeholder = _placeholder;
            _inputField.TextChanged += (s, e) =>
            {
                _searchText = _inputField.Text;
                FilterSuggestions();
            };

            _searchButton = new UIButton();
            _searchButton.Size = new Vector2(30, _size.Y);
            _searchButton.Text = "🔍";
            _searchButton.Click += (s, e) => SearchSubmitted?.Invoke(this, _searchText);

            _clearButton = new UIButton();
            _clearButton.Size = new Vector2(30, _size.Y);
            _clearButton.Text = "✕";
            _clearButton.IsVisible = false;
            _clearButton.Click += (s, e) =>
            {
                _searchText = "";
                _inputField.Text = "";
                _clearButton.IsVisible = false;
                SearchSubmitted?.Invoke(this, "");
            };

            _suggestionPanel = new UIPanel();
            _suggestionPanel.Size = new Vector2(_size.X, 150);
            _suggestionPanel.Position = new Vector2(0, _size.Y);
            _suggestionPanel.IsVisible = false;
            _suggestionPanel.BackgroundColor = new Color(0.3f, 0.3f, 0.3f);
        }

        private void FilterSuggestions()
        {
            _clearButton.IsVisible = !string.IsNullOrEmpty(_searchText);

            if (string.IsNullOrEmpty(_searchText))
            {
                _suggestionPanel.IsVisible = false;
                return;
            }

            _suggestions.Clear();
            foreach (var suggestion in _suggestions)
            {
                if (suggestion.ToLower().Contains(_searchText.ToLower()))
                {
                    _suggestions.Add(suggestion);
                    if (_suggestions.Count >= _maxSuggestions) break;
                }
            }

            _suggestionPanel.IsVisible = _suggestions.Count > 0;
        }

        public void AddSuggestion(string suggestion)
        {
            _suggestions.Add(suggestion);
        }

        public void ClearSuggestions()
        {
            _suggestions.Clear();
            _suggestionPanel.IsVisible = false;
        }
    }

    public class UINumericInput : UIElement
    {
        private float _value;
        private float _minValue;
        private float _maxValue;
        private float _step;
        private UITextField _inputField;
        private UIButton _incrementButton;
        private UIButton _decrementButton;
        private string _format;

        public float Value
        {
            get => _value;
            set
            {
                _value = Math.Clamp(value, _minValue, _maxValue);
                if (_step > 0)
                {
                    _value = (float)Math.Round(_value / _step) * _step;
                }
                _inputField.Text = _value.ToString(_format);
                ValueChanged?.Invoke(this, _value);
            }
        }

        public float MinValue
        {
            get => _minValue;
            set { _minValue = value; }
        }

        public float MaxValue
        {
            get => _maxValue;
            set { _maxValue = value; }
        }

        public float Step
        {
            get => _step;
            set { _step = value; }
        }

        public string Format
        {
            get => _format;
            set { _format = value; }
        }

        public event EventHandler<float>? ValueChanged;

        public UINumericInput()
        {
            _value = 0;
            _minValue = 0;
            _maxValue = 100;
            _step = 1;
            _format = "F0";
            _size = new Vector2(120, 30);

            _inputField = new UITextField();
            _inputField.Size = new Vector2(_size.X - 50, _size.Y);
            _inputField.Text = _value.ToString(_format);
            _inputField.TextChanged += (s, e) =>
            {
                if (float.TryParse(_inputField.Text, out float parsed))
                {
                    Value = parsed;
                }
            };

            _incrementButton = new UIButton();
            _incrementButton.Size = new Vector2(25, _size.Y);
            _incrementButton.Text = "+";
            _incrementButton.Position = new Vector2(_size.X - 50, 0);
            _incrementButton.Click += (s, e) => Value += _step;

            _decrementButton = new UIButton();
            _decrementButton.Size = new Vector2(25, _size.Y);
            _decrementButton.Text = "-";
            _decrementButton.Position = new Vector2(_size.X - 25, 0);
            _decrementButton.Click += (s, e) => Value -= _step;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _inputField.Size = new Vector2(_size.X - 50, _size.Y);
            _incrementButton.Position = new Vector2(_position.X + _size.X - 50, _position.Y);
            _decrementButton.Position = new Vector2(_position.X + _size.X - 25, _position.Y);
        }
    }

    public class UIGridView : UIElement
    {
        private List<List<string>> _rows;
        private List<float> _columnWidths;
        private float _rowHeight;
        private int _frozenColumns;
        private int _frozenRows;
        private UIScrollBar _horizontalScroll;
        private UIScrollBar _verticalScroll;
        private Vector2 _scrollPosition;
        private Color _headerColor;
        private Color _alternatingRowColor;

        public List<List<string>> Rows => _rows;
        public List<float> ColumnWidths => _columnWidths;
        public float RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = value; }
        }
        public int FrozenColumns
        {
            get => _frozenColumns;
            set { _frozenColumns = value; }
        }
        public Color HeaderColor
        {
            get => _headerColor;
            set { _headerColor = value; }
        }
        public Color AlternatingRowColor
        {
            get => _alternatingRowColor;
            set { _alternatingRowColor = value; }
        }

        public event EventHandler<int>? RowSelected;
        public event EventHandler<(int Row, int Column)>? CellSelected;

        public UIGridView()
        {
            _rows = new List<List<string>>();
            _columnWidths = new List<float>();
            _rowHeight = 30;
            _frozenColumns = 0;
            _frozenRows = 0;
            _scrollPosition = Vector2.Zero;
            _headerColor = new Color(0.25f, 0.25f, 0.25f);
            _alternatingRowColor = new Color(0.22f, 0.22f, 0.22f);
            _size = new Vector2(400, 300);

            _horizontalScroll = new UIScrollBar();
            _horizontalScroll.Orientation = Orientation.Horizontal;
            _horizontalScroll.Size = new Vector2(_size.X, 15);

            _verticalScroll = new UIScrollBar();
            _verticalScroll.Orientation = Orientation.Vertical;
            _verticalScroll.Size = new Vector2(15, _size.Y);
        }

        public void AddColumn(string header, float width = 100)
        {
            _columnWidths.Add(width);

            foreach (var row in _rows)
            {
                row.Add("");
            }
        }

        public void AddRow(params string[] cells)
        {
            var row = new List<string>(cells);
            while (row.Count < _columnWidths.Count)
            {
                row.Add("");
            }
            _rows.Add(row);
        }

        public void SetCell(int row, int column, string value)
        {
            if (row >= 0 && row < _rows.Count && column >= 0 && column < _columnWidths.Count)
            {
                _rows[row][column] = value;
            }
        }

        public string GetCell(int row, int column)
        {
            if (row >= 0 && row < _rows.Count && column >= 0 && column < _rows[row].Count)
            {
                return _rows[row][column];
            }
            return "";
        }

        public void Clear()
        {
            _rows.Clear();
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            float totalWidth = 0;
            foreach (var width in _columnWidths)
            {
                totalWidth += width;
            }

            for (int i = 0; i < _rows.Count; i++)
            {
                float rowY = _position.Y + i * _rowHeight - _scrollPosition.Y;

                if (rowY + _rowHeight < _position.Y || rowY > _position.Y + _size.Y)
                    continue;

                Color rowColor = (i % 2 == 0) ? _alternatingRowColor : _backgroundColor;
                var rowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    rowColor.GetAByte(), rowColor.GetRByte(), rowColor.GetGByte(), rowColor.GetBByte()));

                context.DrawRectangle(rowBrush, null,
                    new System.Windows.Rect(_position.X, rowY, _size.X, _rowHeight));

                float cellX = _position.X - _scrollPosition.X;
                for (int j = 0; j < _rows[i].Count && j < _columnWidths.Count; j++)
                {
                    var typeface = new Typeface("Segoe UI");
                    var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                    var text = new FormattedText(_rows[i][j], System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight, typeface, 12, brush, 1.0);

                    context.DrawText(text, new System.Windows.Point(cellX + 5, rowY + (_rowHeight - text.Height) / 2));
                    cellX += _columnWidths[j];
                }
            }
        }
    }

    public class UICheckbox : UIElement
    {
        private bool _isChecked;
        private string _labelText;
        private UILabel? _label;
        private float _boxSize;
        private Color _checkColor;
        private Color _uncheckedColor;
        private Color _hoverColor;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    CheckedChanged?.Invoke(this, _isChecked);
                }
            }
        }

        public string LabelText
        {
            get => _labelText;
            set
            {
                _labelText = value;
                if (_label != null)
                {
                    _label.Text = value;
                }
            }
        }

        public float BoxSize
        {
            get => _boxSize;
            set { _boxSize = value; }
        }

        public Color CheckColor
        {
            get => _checkColor;
            set { _checkColor = value; }
        }

        public Color UncheckedColor
        {
            get => _uncheckedColor;
            set { _uncheckedColor = value; }
        }

        public event EventHandler<bool>? CheckedChanged;

        public UICheckbox()
        {
            _isChecked = false;
            _labelText = "Checkbox";
            _boxSize = 20;
            _checkColor = new Color(0.2f, 0.6f, 0.9f);
            _uncheckedColor = new Color(0.4f, 0.4f, 0.4f);
            _hoverColor = new Color(0.5f, 0.5f, 0.5f);
            _size = new Vector2(_boxSize + 100, _boxSize);

            _label = new UILabel(_labelText);
            _label.Position = new Vector2(_boxSize + 5, 0);
            _label.Size = new Vector2(100, _boxSize);
        }

        public override void OnPointerClick(Vector2 position)
        {
            IsChecked = !_isChecked;
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            var boxBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (_isChecked ? _checkColor : _uncheckedColor).GetAByte(),
                (_isChecked ? _checkColor : _uncheckedColor).GetRByte(),
                (_isChecked ? _checkColor : _uncheckedColor).GetGByte(),
                (_isChecked ? _checkColor : _uncheckedColor).GetBByte()));

            context.DrawRectangle(boxBrush, null,
                new System.Windows.Rect(_position.X, _position.Y, _boxSize, _boxSize));

            if (_isChecked)
            {
                var checkBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                var pen = new Pen(checkBrush, 2);

                context.DrawLine(pen,
                    new System.Windows.Point(_position.X + 4, _position.Y + _boxSize / 2),
                    new System.Windows.Point(_position.X + _boxSize / 2, _position.Y + _boxSize - 4));

                context.DrawLine(pen,
                    new System.Windows.Point(_position.X + _boxSize / 2, _position.Y + _boxSize - 4),
                    new System.Windows.Point(_position.X + _boxSize - 4, _position.Y + 4));
            }

            if (_label != null)
            {
                _label.Draw(context);
            }
        }
    }

    public class UIRadioButton : UIElement
    {
        private bool _isSelected;
        private string _labelText;
        private UILabel? _label;
        private string _groupName;
        private float _circleSize;
        private Color _selectedColor;
        private Color _unselectedColor;

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    SelectionChanged?.Invoke(this, _isSelected);
                }
            }
        }

        public string LabelText
        {
            get => _labelText;
            set
            {
                _labelText = value;
                if (_label != null) _label.Text = value;
            }
        }

        public string GroupName
        {
            get => _groupName;
            set { _groupName = value; }
        }

        public float CircleSize
        {
            get => _circleSize;
            set { _circleSize = value; }
        }

        public Color SelectedColor
        {
            get => _selectedColor;
            set { _selectedColor = value; }
        }

        public Color UnselectedColor
        {
            get => _unselectedColor;
            set { _unselectedColor = value; }
        }

        public event EventHandler<bool>? SelectionChanged;

        public UIRadioButton()
        {
            _isSelected = false;
            _labelText = "Radio Button";
            _groupName = "DefaultGroup";
            _circleSize = 18;
            _selectedColor = new Color(0.2f, 0.6f, 0.9f);
            _unselectedColor = new Color(0.4f, 0.4f, 0.4f);
            _size = new Vector2(_circleSize + 100, _circleSize);

            _label = new UILabel(_labelText);
            _label.Position = new Vector2(_circleSize + 5, 0);
            _label.Size = new Vector2(100, _circleSize);
        }

        public override void OnPointerClick(Vector2 position)
        {
            IsSelected = true;
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            var outerBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                (_isSelected ? _selectedColor : _unselectedColor).GetAByte(),
                (_isSelected ? _selectedColor : _unselectedColor).GetRByte(),
                (_isSelected ? _selectedColor : _unselectedColor).GetGByte(),
                (_isSelected ? _selectedColor : _unselectedColor).GetBByte()));

            context.DrawEllipse(null, new Pen(outerBrush, 2),
                new System.Windows.Point(_position.X + _circleSize / 2, _position.Y + _circleSize / 2),
                _circleSize / 2 - 1, _circleSize / 2 - 1);

            if (_isSelected)
            {
                var innerBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                context.DrawEllipse(innerBrush, null,
                    new System.Windows.Point(_position.X + _circleSize / 2, _position.Y + _circleSize / 2),
                    _circleSize / 4, _circleSize / 4);
            }

            if (_label != null)
            {
                _label.Draw(context);
            }
        }
    }

    public class UIModal : UIElement
    {
        private bool _isOpen;
        private UIPanel _backdrop;
        private UIPanel _contentPanel;
        private UILabel _titleLabel;
        private UIButton _closeButton;
        private float _animationDuration;
        private float _animationProgress;
        private bool _isAnimating;
        private float _targetOpacity;
        private float _currentOpacity;

        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                if (_isOpen)
                {
                    Show();
                }
                else
                {
                    Hide();
                }
            }
        }

        public UIPanel ContentPanel => _contentPanel;
        public UILabel TitleLabel => _titleLabel;
        public UIButton CloseButton => _closeButton;

        public event EventHandler? ModalOpened;
        public event EventHandler? ModalClosed;

        public UIModal()
        {
            _isOpen = false;
            _animationDuration = 0.3f;
            _animationProgress = 0;
            _isAnimating = false;
            _targetOpacity = 0;
            _currentOpacity = 0;

            _backdrop = new UIPanel();
            _backdrop.BackgroundColor = new Color(0, 0, 0, 0.7f);
            _backdrop.IsVisible = false;

            _titleLabel = new UILabel("Modal Title");
            _titleLabel.Size = new Vector2(200, 30);
            _titleLabel.BackgroundColor = new Color(0.25f, 0.25f, 0.25f);

            _closeButton = new UIButton();
            _closeButton.Size = new Vector2(30, 30);
            _closeButton.Text = "✕";
            _closeButton.Click += (s, e) => IsOpen = false;

            _contentPanel = new UIPanel();
            _contentPanel.BackgroundColor = new Color(0.25f, 0.25f, 0.25f);
            _contentPanel.BorderColor = new Color(0.4f, 0.4f, 0.4f);
            _contentPanel.BorderWidth = 1;
            _contentPanel.Size = new Vector2(400, 300);

            _size = new Vector2(400, 300);
        }

        public void Show()
        {
            _isOpen = true;
            _backdrop.IsVisible = true;
            _isAnimating = true;
            _animationProgress = 0;
            _targetOpacity = 1;
            ModalOpened?.Invoke(this, EventArgs.Empty);
        }

        public void Hide()
        {
            _isAnimating = true;
            _animationProgress = 0;
            _targetOpacity = 0;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_isAnimating)
            {
                _animationProgress += deltaTime / _animationDuration;

                if (_animationProgress >= 1.0f)
                {
                    _animationProgress = 1.0f;
                    _isAnimating = false;

                    if (_targetOpacity == 0)
                    {
                        _isOpen = false;
                        _backdrop.IsVisible = false;
                        ModalClosed?.Invoke(this, EventArgs.Empty);
                    }
                }

                float startOpacity = _currentOpacity;
                _currentOpacity = Math.Lerp(startOpacity, _targetOpacity, _animationProgress);
                _backdrop.Opacity = _currentOpacity;
            }
        }

        public float Opacity
        {
            get => _currentOpacity;
            set { _currentOpacity = value; }
        }
    }

    public class UIPopup : UIElement
    {
        private UIPanel _popupPanel;
        private Vector2 _offset;
        private Vector2 _anchorPoint;
        private bool _showArrow;
        private float _arrowSize;
        private float _animationDuration;
        private float _animationProgress;
        private bool _isAnimating;

        public UIPanel PopupPanel => _popupPanel;
        public Vector2 Offset
        {
            get => _offset;
            set { _offset = value; }
        }
        public Vector2 AnchorPoint
        {
            get => _anchorPoint;
            set { _anchorPoint = value; }
        }
        public bool ShowArrow
        {
            get => _showArrow;
            set { _showArrow = value; }
        }
        public float ArrowSize
        {
            get => _arrowSize;
            set { _arrowSize = value; }
        }

        public UIPopup()
        {
            _offset = new Vector2(0, 10);
            _anchorPoint = new Vector2(0.5f, 0);
            _showArrow = true;
            _arrowSize = 10;
            _animationDuration = 0.2f;
            _animationProgress = 0;
            _isAnimating = false;
            _isVisible = false;

            _popupPanel = new UIPanel();
            _popupPanel.BackgroundColor = new Color(0.3f, 0.3f, 0.3f);
            _popupPanel.BorderColor = new Color(0.5f, 0.5f, 0.5f);
            _popupPanel.BorderWidth = 1;
            _size = new Vector2(200, 100);
        }

        public void ShowAt(Vector2 position)
        {
            _isVisible = true;
            _isAnimating = true;
            _animationProgress = 0;

            Position = new Vector2(
                position.X + _offset.X - _size.X * _anchorPoint.X,
                position.Y + _offset.Y - _size.Y * _anchorPoint.Y);
        }

        public void Hide()
        {
            _isVisible = false;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_isAnimating)
            {
                _animationProgress += deltaTime / _animationDuration;

                if (_animationProgress >= 1.0f)
                {
                    _animationProgress = 1.0f;
                    _isAnimating = false;
                }
            }
        }
    }

    public class UITooltip : UIPopup
    {
        private string _tooltipText;
        private float _delay;
        private float _elapsedTime;
        private bool _isShowing;
        private UILabel _textLabel;

        public string TooltipText
        {
            get => _tooltipText;
            set { _tooltipText = value; _textLabel.Text = value; }
        }

        public float Delay
        {
            get => _delay;
            set { _delay = value; }
        }

        public UITooltip()
        {
            _tooltipText = "";
            _delay = 0.5f;
            _elapsedTime = 0;
            _isShowing = false;
            _showArrow = false;
            _animationDuration = 0.15f;

            _textLabel = new UILabel(_tooltipText);
            _textLabel.BackgroundColor = Color.Transparent;
            _textLabel.TextColor = Color.White;
            _textLabel.Size = new Vector2(150, 30);

            _popupPanel.BackgroundColor = new Color(0.15f, 0.15f, 0.15f);
            _size = new Vector2(160, 40);
        }

        public void StartDelay()
        {
            _elapsedTime = 0;
            _isShowing = false;
        }

        public void UpdateDelay(float deltaTime)
        {
            if (!_isShowing && _elapsedTime < _delay)
            {
                _elapsedTime += deltaTime;

                if (_elapsedTime >= _delay)
                {
                    _isShowing = true;
                    IsVisible = true;
                }
            }
        }
    }

    public class UINotification : UIPanel
    {
        private string _message;
        private UINotificationType _notificationType;
        private UILabel _messageLabel;
        private UIButton _closeButton;
        private float _displayDuration;
        private float _animationDuration;
        private float _animationProgress;
        private bool _isAnimatingOut;

        public string Message
        {
            get => _message;
            set { _message = value; _messageLabel.Text = value; }
        }

        public UINotificationType NotificationType
        {
            get => _notificationType;
            set { _notificationType = value; UpdateAppearance(); }
        }

        public float DisplayDuration
        {
            get => _displayDuration;
            set { _displayDuration = value; }
        }

        public float AnimationDuration
        {
            get => _animationDuration;
            set { _animationDuration = value; }
        }

        public event EventHandler? NotificationClosed;

        public UINotification()
        {
            _message = "";
            _notificationType = UINotificationType.Info;
            _displayDuration = 3.0f;
            _animationDuration = 0.3f;
            _animationProgress = 0;
            _isAnimatingOut = false;

            _messageLabel = new UILabel(_message);
            _messageLabel.Size = new Vector2(250, 40);
            _messageLabel.HorizontalTextAlignment = Alignment.Center;
            _messageLabel.VerticalTextAlignment = Alignment.Center;

            _closeButton = new UIButton();
            _closeButton.Size = new Vector2(30, 30);
            _closeButton.Text = "✕";
            _closeButton.Click += (s, e) => StartDismiss();

            _size = new Vector2(300, 50);
            UpdateAppearance();
        }

        private void UpdateAppearance()
        {
            switch (_notificationType)
            {
                case UINotificationType.Success:
                    _backgroundColor = new Color(0.2f, 0.4f, 0.2f);
                    break;
                case UINotificationType.Error:
                    _backgroundColor = new Color(0.4f, 0.2f, 0.2f);
                    break;
                case UINotificationType.Warning:
                    _backgroundColor = new Color(0.4f, 0.4f, 0.2f);
                    break;
                case UINotificationType.Info:
                default:
                    _backgroundColor = new Color(0.2f, 0.3f, 0.4f);
                    break;
            }
        }

        public void StartDismiss()
        {
            _isAnimatingOut = true;
            _animationProgress = 0;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_isAnimatingOut)
            {
                _animationProgress += deltaTime / _animationDuration;

                if (_animationProgress >= 1.0f)
                {
                    IsVisible = false;
                    NotificationClosed?.Invoke(this, EventArgs.Empty);
                }
            }
            else if (IsVisible && _displayDuration > 0)
            {
                _displayDuration -= deltaTime;

                if (_displayDuration <= 0)
                {
                    StartDismiss();
                }
            }
        }
    }

    public enum UINotificationType
    {
        Info,
        Success,
        Warning,
        Error
    }

    public class UIContextMenu : UIPanel
    {
        private List<UIMenuItem> _menuItems;
        private float _itemHeight;
        private float _separatorHeight;

        public List<UIMenuItem> MenuItems => _menuItems;
        public float ItemHeight
        {
            get => _itemHeight;
            set { _itemHeight = value; }
        }

        public event EventHandler<UIMenuItem>? ItemClicked;

        public UIContextMenu()
        {
            _menuItems = new List<UIMenuItem>();
            _itemHeight = 30;
            _separatorHeight = 5;
            _size = new Vector2(180, 200);
            _backgroundColor = new Color(0.25f, 0.25f, 0.25f);
            _borderColor = new Color(0.4f, 0.4f, 0.4f);
            _borderWidth = 1;
            IsVisible = false;
        }

        public UIMenuItem AddItem(string text, string? shortcut = null)
        {
            var item = new UIMenuItem(text, shortcut);
            _menuItems.Add(item);
            UpdateLayout();
            return item;
        }

        public void AddSeparator()
        {
            var separator = new UIMenuItem();
            separator.IsSeparator = true;
            _menuItems.Add(separator);
            UpdateLayout();
        }

        private void UpdateLayout()
        {
            float y = _position.Y + 5;

            foreach (var item in _menuItems)
            {
                item.Position = new Vector2(_position.X + 5, y);
                item.Size = new Vector2(_size.X - 10, item.IsSeparator ? _separatorHeight : _itemHeight);
                y += item.Size.Y;
            }

            _size = new Vector2(_size.X, y - _position.Y + 5);
        }

        public void ShowAt(Vector2 position)
        {
            Position = position;
            IsVisible = true;
            UpdateLayout();
        }

        public void Hide()
        {
            IsVisible = false;
        }
    }

    public class UIMenuItem : UIElement
    {
        private string _text;
        private string? _shortcut;
        private bool _isSeparator;
        private bool _isEnabled;
        private Color _hoverColor;

        public string Text
        {
            get => _text;
            set { _text = value; }
        }

        public string? Shortcut
        {
            get => _shortcut;
            set { _shortcut = value; }
        }

        public bool IsSeparator
        {
            get => _isSeparator;
            set { _isSeparator = value; }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; }
        }

        public Color HoverColor
        {
            get => _hoverColor;
            set { _hoverColor = value; }
        }

        public event EventHandler? Clicked;

        public UIMenuItem()
        {
            _text = "";
            _isSeparator = false;
            _isEnabled = true;
            _hoverColor = new Color(0.35f, 0.35f, 0.35f);
        }

        public UIMenuItem(string text, string? shortcut = null)
        {
            _text = text;
            _shortcut = shortcut;
            _isSeparator = false;
            _isEnabled = true;
            _hoverColor = new Color(0.35f, 0.35f, 0.35f);
        }

        public override void OnPointerClick(Vector2 position)
        {
            if (!_isSeparator && _isEnabled)
            {
                Clicked?.Invoke(this, EventArgs.Empty);
            }
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            if (_isSeparator)
            {
                var pen = new Pen(new SolidColorBrush(System.Windows.Media.Color.FromArgb(100, 128, 128, 128)), 1);
                context.DrawLine(pen,
                    new System.Windows.Point(_position.X, _position.Y + _size.Y / 2),
                    new System.Windows.Point(_position.X + _size.X, _position.Y + _size.Y / 2));
            }
            else
            {
                var typeface = new Typeface("Segoe UI");
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                var text = new FormattedText(_text, System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight, typeface, 12, brush, 1.0);

                context.DrawText(text, new System.Windows.Point(_position.X + 10, _position.Y + (_size.Y - text.Height) / 2));

                if (!string.IsNullOrEmpty(_shortcut))
                {
                    var shortcutBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(150, 200, 200, 200));
                    var shortcutText = new FormattedText(_shortcut, System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight, typeface, 10, shortcutBrush, 1.0);

                    context.DrawText(shortcutText, new System.Windows.Point(_position.X + _size.X - shortcutText.Width - 10,
                        _position.Y + (_size.Y - shortcutText.Height) / 2));
                }
            }
        }
    }

    public class UIMenuBar : UIElement
    {
        private List<UIMenuBarItem> _menus;
        private float _menuWidth;
        private float _menuHeight;

        public List<UIMenuBarItem> Menus => _menus;
        public float MenuWidth
        {
            get => _menuWidth;
            set { _menuWidth = value; }
        }

        public UIMenuBar()
        {
            _menus = new List<UIMenuBarItem>();
            _menuWidth = 80;
            _menuHeight = 30;
            _size = new Vector2(600, _menuHeight);
            _backgroundColor = new Color(0.2f, 0.2f, 0.2f);
        }

        public UIMenuBarItem AddMenu(string text)
        {
            var menu = new UIMenuBarItem(text);
            menu.Size = new Vector2(_menuWidth, _menuHeight);
            menu.Position = new Vector2(_position.X + _menus.Count * (_menuWidth + 2), _position.Y);
            _menus.Add(menu);
            return menu;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            for (int i = 0; i < _menus.Count; i++)
            {
                _menus[i].Position = new Vector2(_position.X + i * (_menuWidth + 2), _position.Y);
            }
        }
    }

    public class UIMenuBarItem : UIButton
    {
        private UIPanel _dropdownPanel;
        private List<UIMenuItem> _items;

        public List<UIMenuItem> Items => _items;

        public UIMenuBarItem()
        {
            _items = new List<UIMenuItem>();

            _dropdownPanel = new UIPanel();
            _dropdownPanel.BackgroundColor = new Color(0.25f, 0.25f, 0.25f);
            _dropdownPanel.BorderColor = new Color(0.4f, 0.4f, 0.4f);
            _dropdownPanel.BorderWidth = 1;
            _dropdownPanel.IsVisible = false;
        }

        public UIMenuItem AddItem(string text, string? shortcut = null)
        {
            var item = new UIMenuItem(text, shortcut);
            _items.Add(item);
            return item;
        }

        public void ShowDropdown()
        {
            _dropdownPanel.IsVisible = true;
        }

        public void HideDropdown()
        {
            _dropdownPanel.IsVisible = false;
        }
    }

    public class UIDataGrid : UIElement
    {
        private List<List<object>> _data;
        private List<string> _columnHeaders;
        private List<float> _columnWidths;
        private float _rowHeight;
        private float _headerHeight;
        private int _sortColumnIndex;
        private bool _sortAscending;
        private bool _showHeader;
        private UIScrollBar _horizontalScroll;
        private UIScrollBar _verticalScroll;
        private Vector2 _scrollPosition;
        private Color _headerColor;
        private Color _alternatingRowColor;
        private Color _selectedRowColor;
        private int _selectedRowIndex;

        public List<List<object>> Data => _data;
        public List<string> ColumnHeaders => _columnHeaders;
        public List<float> ColumnWidths => _columnWidths;
        public float RowHeight
        {
            get => _rowHeight;
            set { _rowHeight = value; }
        }
        public float HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = value; }
        }
        public int SortColumnIndex
        {
            get => _sortColumnIndex;
            set { _sortColumnIndex = value; }
        }
        public bool SortAscending
        {
            get => _sortAscending;
            set { _sortAscending = value; }
        }
        public Color HeaderColor
        {
            get => _headerColor;
            set { _headerColor = value; }
        }
        public Color AlternatingRowColor
        {
            get => _alternatingRowColor;
            set { _alternatingRowColor = value; }
        }
        public Color SelectedRowColor
        {
            get => _selectedRowColor;
            set { _selectedRowColor = value; }
        }
        public int SelectedRowIndex
        {
            get => _selectedRowIndex;
            set
            {
                _selectedRowIndex = value;
                RowSelected?.Invoke(this, _selectedRowIndex);
            }
        }

        public event EventHandler<int>? RowSelected;
        public event EventHandler<int>? ColumnSorted;

        public UIDataGrid()
        {
            _data = new List<List<object>>();
            _columnHeaders = new List<string>();
            _columnWidths = new List<float>();
            _rowHeight = 28;
            _headerHeight = 32;
            _sortColumnIndex = -1;
            _sortAscending = true;
            _showHeader = true;
            _scrollPosition = Vector2.Zero;
            _headerColor = new Color(0.25f, 0.25f, 0.25f);
            _alternatingRowColor = new Color(0.22f, 0.22f, 0.22f);
            _selectedRowColor = new Color(0.2f, 0.4f, 0.6f);
            _selectedRowIndex = -1;
            _size = new Vector2(500, 350);
        }

        public void AddColumn(string header, float width = 100)
        {
            _columnHeaders.Add(header);
            _columnWidths.Add(width);
        }

        public void AddRow(params object[] values)
        {
            var row = new List<object>(values);
            while (row.Count < _columnWidths.Count)
            {
                row.Add("");
            }
            _data.Add(row);
        }

        public void InsertRow(int index, params object[] values)
        {
            var row = new List<object>(values);
            while (row.Count < _columnWidths.Count)
            {
                row.Add("");
            }
            _data.Insert(index, row);
        }

        public void RemoveRow(int index)
        {
            if (index >= 0 && index < _data.Count)
            {
                _data.RemoveAt(index);
                if (_selectedRowIndex == index)
                {
                    _selectedRowIndex = -1;
                }
            }
        }

        public void SortBy(int columnIndex, bool ascending = true)
        {
            if (columnIndex < 0 || columnIndex >= _columnWidths.Count) return;

            _sortColumnIndex = columnIndex;
            _sortAscending = ascending;

            _data.Sort((a, b) =>
            {
                object valA = columnIndex < a.Count ? a[columnIndex] : null;
                object valB = columnIndex < b.Count ? b[columnIndex] : null;

                int result = Comparer<object>.Default.Compare(valA, valB);
                return ascending ? result : -result;
            });

            ColumnSorted?.Invoke(this, columnIndex);
        }

        public void Clear()
        {
            _data.Clear();
            _selectedRowIndex = -1;
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            if (_showHeader)
            {
                float headerY = _position.Y - _scrollPosition.Y;
                var headerBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    _headerColor.GetAByte(), _headerColor.GetRByte(), _headerColor.GetGByte(), _headerColor.GetBByte()));

                context.DrawRectangle(headerBrush, null,
                    new System.Windows.Rect(_position.X, headerY, _size.X, _headerHeight));

                float cellX = _position.X - _scrollPosition.X;
                for (int i = 0; i < _columnHeaders.Count && i < _columnWidths.Count; i++)
                {
                    var typeface = new Typeface("Segoe UI");
                    var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 255, 255, 255));
                    var text = new FormattedText(_columnHeaders[i], System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight, typeface, 12, brush, 1.0);

                    context.DrawText(text, new System.Windows.Point(cellX + 5, headerY + (_headerHeight - text.Height) / 2));

                    if (i == _sortColumnIndex)
                    {
                        string arrow = _sortAscending ? " ▲" : " ▼";
                        var arrowText = new FormattedText(arrow, System.Globalization.CultureInfo.CurrentCulture,
                            System.Windows.FlowDirection.LeftToRight, typeface, 10, brush, 1.0);
                        context.DrawText(arrowText, new System.Windows.Point(cellX + text.Width + 5, headerY + (_headerHeight - arrowText.Height) / 2));
                    }

                    cellX += _columnWidths[i];
                }
            }

            for (int i = 0; i < _data.Count; i++)
            {
                float rowY = _position.Y + _headerHeight + i * _rowHeight - _scrollPosition.Y;

                if (rowY + _rowHeight < _position.Y || rowY > _position.Y + _size.Y)
                    continue;

                Color rowColor = i == _selectedRowIndex ? _selectedRowColor :
                    (i % 2 == 0) ? _alternatingRowColor : _backgroundColor;

                var rowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                    rowColor.GetAByte(), rowColor.GetRByte(), rowColor.GetGByte(), rowColor.GetBByte()));

                context.DrawRectangle(rowBrush, null,
                    new System.Windows.Rect(_position.X, rowY, _size.X, _rowHeight));

                float cellX = _position.X - _scrollPosition.X;
                for (int j = 0; j < _data[i].Count && j < _columnWidths.Count; j++)
                {
                    var typeface = new Typeface("Segoe UI");
                    var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 220, 220, 220));
                    string cellText = _data[i][j]?.ToString() ?? "";
                    var text = new FormattedText(cellText, System.Globalization.CultureInfo.CurrentCulture,
                        System.Windows.FlowDirection.LeftToRight, typeface, 11, brush, 1.0);

                    context.DrawText(text, new System.Windows.Point(cellX + 5, rowY + (_rowHeight - text.Height) / 2));
                    cellX += _columnWidths[j];
                }
            }
        }
    }

    public class UIColorWheel : UIElement
    {
        private Color _selectedColor;
        private Vector2 _selectionPosition;
        private float _hue;
        private float _saturation;
        private float _brightness;
        private bool _isDragging;
        private float _wheelRadius;
        private float _wheelThickness;

        public Color SelectedColor
        {
            get => _selectedColor;
            set
            {
                _selectedColor = value;
                RGBToHSB(value, out _hue, out _saturation, out _brightness);
                UpdateSelectionPosition();
                ColorChanged?.Invoke(this, _selectedColor);
            }
        }

        public float Hue
        {
            get => _hue;
            set
            {
                _hue = value;
                _selectedColor = HSBToRGB(_hue, _saturation, _brightness);
                UpdateSelectionPosition();
                ColorChanged?.Invoke(this, _selectedColor);
            }
        }

        public float Saturation
        {
            get => _saturation;
            set
            {
                _saturation = Math.Clamp(value, 0, 1);
                _selectedColor = HSBToRGB(_hue, _saturation, _brightness);
                UpdateSelectionPosition();
                ColorChanged?.Invoke(this, _selectedColor);
            }
        }

        public float Brightness
        {
            get => _brightness;
            set
            {
                _brightness = Math.Clamp(value, 0, 1);
                _selectedColor = HSBToRGB(_hue, _saturation, _brightness);
                UpdateSelectionPosition();
                ColorChanged?.Invoke(this, _selectedColor);
            }
        }

        public event EventHandler<Color>? ColorChanged;

        public UIColorWheel()
        {
            _selectedColor = Color.White;
            _selectionPosition = new Vector2(0.5f);
            _hue = 0;
            _saturation = 1;
            _brightness = 1;
            _isDragging = false;
            _wheelRadius = 100;
            _wheelThickness = 30;
            _size = new Vector2(250, 250);
        }

        private void UpdateSelectionPosition()
        {
            float angle = _hue / 360f * 2 * (float)Math.PI - (float)Math.PI / 2;
            float distance = _saturation * (_wheelRadius - _wheelThickness / 2);

            _selectionPosition = new Vector2(
                (float)Math.Cos(angle) * distance,
                (float)Math.Sin(angle) * distance);
        }

        private void RGBToHSB(Color color, out float hue, out float saturation, out float brightness)
        {
            float r = color.R;
            float g = color.G;
            float b = color.B;

            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));
            float delta = max - min;

            brightness = max;

            if (delta == 0)
            {
                hue = 0;
                saturation = 0;
            }
            else
            {
                saturation = delta / max;

                if (max == r)
                    hue = 60 * (((g - b) / delta) % 6);
                else if (max == g)
                    hue = 60 * (((b - r) / delta) + 2);
                else
                    hue = 60 * (((r - g) / delta) + 4);

                if (hue < 0) hue += 360;
            }
        }

        private Color HSBToRGB(float hue, float saturation, float brightness)
        {
            float c = brightness * saturation;
            float x = c * (1 - Math.Abs((hue / 60) % 2 - 1));
            float m = brightness - c;

            float r, g, b;

            if (hue < 60) { r = c; g = x; b = 0; }
            else if (hue < 120) { r = x; g = c; b = 0; }
            else if (hue < 180) { r = 0; g = c; b = x; }
            else if (hue < 240) { r = 0; g = x; b = c; }
            else if (hue < 300) { r = x; g = 0; b = c; }
            else { r = c; g = 0; b = x; }

            return new Color(r + m, g + m, b + m);
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            float centerX = _position.X + _size.X / 2;
            float centerY = _position.Y + _size.Y / 2;

            for (int i = 0; i < 360; i++)
            {
                float angle1 = i * Math.PI / 180;
                float angle2 = (i + 1) * Math.PI / 180;

                var color = HSBToRGB(i, 1, 1);
                var brush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(255,
                    (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255)));

                var geometry = new StreamGeometry();
                using (var ctx = geometry.Open())
                {
                    ctx.BeginFigure(new System.Windows.Point(centerX, centerY), true, true);
                    ctx.LineTo(new System.Windows.Point(
                        centerX + (_wheelRadius - _wheelThickness) * (float)Math.Cos(angle1),
                        centerY + (_wheelRadius - _wheelThickness) * (float)Math.Sin(angle1)), false, false);
                    ctx.LineTo(new System.Windows.Point(
                        centerX + (_wheelRadius - _wheelThickness) * (float)Math.Cos(angle2),
                        centerY + (_wheelRadius - _wheelThickness) * (float)Math.Sin(angle2)), false, false);
                    ctx.LineTo(new System.Windows.Point(
                        centerX + _wheelRadius * (float)Math.Cos(angle2),
                        centerY + _wheelRadius * (float)Math.Sin(angle2)), false, false);
                    ctx.LineTo(new System.Windows.Point(
                        centerX + _wheelRadius * (float)Math.Cos(angle1),
                        centerY + _wheelRadius * (float)Math.Sin(angle1)), false, false);
                }

                context.DrawGeometry(brush, null, geometry);
            }

            var selectionPen = new Pen(Brushes.White, 2);
            context.DrawEllipse(null, selectionPen,
                new System.Windows.Point(centerX + _selectionPosition.X, centerY + _selectionPosition.Y), 6, 6);
        }
    }

    public class UIRangeSlider : UIElement
    {
        private float _minValue;
        private float _maxValue;
        private float _rangeMin;
        private float _rangeMax;
        private float _step;
        private Vector2 _thumb1Position;
        private Vector2 _thumb2Position;
        private bool _isDraggingThumb1;
        private bool _isDraggingThumb2;
        private Color _trackColor;
        private Color _rangeColor;
        private Color _thumbColor;
        private float _thumbSize;
        private float _trackHeight;

        public float MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                if (_rangeMin < _minValue) _rangeMin = _minValue;
            }
        }

        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                if (_rangeMax > _maxValue) _rangeMax = _maxValue;
            }
        }

        public float RangeMin
        {
            get => _rangeMin;
            set
            {
                _rangeMin = Math.Clamp(value, _minValue, _maxValue);
                RangeChanged?.Invoke(this, (_rangeMin, _rangeMax));
            }
        }

        public float RangeMax
        {
            get => _rangeMax;
            set
            {
                _rangeMax = Math.Clamp(value, _minValue, _maxValue);
                RangeChanged?.Invoke(this, (_rangeMin, _rangeMax));
            }
        }

        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; }
        }

        public Color RangeColor
        {
            get => _rangeColor;
            set { _rangeColor = value; }
        }

        public Color ThumbColor
        {
            get => _thumbColor;
            set { _thumbColor = value; }
        }

        public event EventHandler<(float Min, float Max)>? RangeChanged;

        public UIRangeSlider()
        {
            _minValue = 0;
            _maxValue = 100;
            _rangeMin = 25;
            _rangeMax = 75;
            _step = 0;
            _trackColor = new Color(0.3f, 0.3f, 0.3f);
            _rangeColor = new Color(0.2f, 0.6f, 0.9f);
            _thumbColor = Color.White;
            _thumbSize = 16;
            _trackHeight = 6;
            _isDraggingThumb1 = false;
            _isDraggingThumb2 = false;
            _size = new Vector2(200, 30);

            UpdateThumbPositions();
        }

        private void UpdateThumbPositions()
        {
            float trackWidth = _size.X - _thumbSize;

            float minRatio = (_rangeMin - _minValue) / (_maxValue - _minValue);
            _thumb1Position = new Vector2(_position.X + minRatio * trackWidth, _position.Y + (_size.Y - _thumbSize) / 2);

            float maxRatio = (_rangeMax - _minValue) / (_maxValue - _minValue);
            _thumb2Position = new Vector2(_position.X + maxRatio * trackWidth, _position.Y + (_size.Y - _thumbSize) / 2);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdateThumbPositions();
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            float trackY = _position.Y + (_size.Y - _trackHeight) / 2;
            float trackWidth = _size.X;

            var trackBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                _trackColor.GetAByte(), _trackColor.GetRByte(), _trackColor.GetGByte(), _trackColor.GetBByte()));

            context.DrawRectangle(trackBrush, null,
                new System.Windows.Rect(_position.X, trackY, trackWidth, _trackHeight));

            float rangeStart = (_rangeMin - _minValue) / (_maxValue - _minValue) * trackWidth;
            float rangeEnd = (_rangeMax - _minValue) / (_maxValue - _minValue) * trackWidth;

            var rangeBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                _rangeColor.GetAByte(), _rangeColor.GetRByte(), _rangeColor.GetGByte(), _rangeColor.GetBByte()));

            context.DrawRectangle(rangeBrush, null,
                new System.Windows.Rect(_position.X + rangeStart, trackY, rangeEnd - rangeStart, _trackHeight));

            var thumbBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                _thumbColor.GetAByte(), _thumbColor.GetRByte(), _thumbColor.GetGByte(), _thumbColor.GetBByte()));

            context.DrawRectangle(thumbBrush, null,
                new System.Windows.Rect(_thumb1Position.X, _thumb1Position.Y, _thumbSize, _thumbSize));

            context.DrawRectangle(thumbBrush, null,
                new System.Windows.Rect(_thumb2Position.X, _thumb2Position.Y, _thumbSize, _thumbSize));
        }
    }

    public class UISplitView : UIElement
    {
        private UIElement? _leftPanel;
        private UIElement? _rightPanel;
        private UIPanel _splitter;
        private float _splitRatio;
        private float _minPanelSize;
        private bool _isVertical;
        private bool _isDragging;

        public UIElement? LeftPanel
        {
            get => _leftPanel;
            set => _leftPanel = value;
        }

        public UIElement? RightPanel
        {
            get => _rightPanel;
            set => _rightPanel = value;
        }

        public float SplitRatio
        {
            get => _splitRatio;
            set
            {
                _splitRatio = Math.Clamp(value, 0.1f, 0.9f);
                UpdateLayout();
            }
        }

        public float MinPanelSize
        {
            get => _minPanelSize;
            set { _minPanelSize = value; }
        }

        public bool IsVertical
        {
            get => _isVertical;
            set { _isVertical = value; UpdateLayout(); }
        }

        public UISplitView()
        {
            _leftPanel = null;
            _rightPanel = null;
            _splitRatio = 0.5f;
            _minPanelSize = 50;
            _isVertical = true;
            _isDragging = false;
            _size = new Vector2(500, 300);

            _splitter = new UIPanel();
            _splitter.Size = new Vector2(_isVertical ? 5 : _size.X, _isVertical ? _size.Y : 5);
            _splitter.BackgroundColor = new Color(0.35f, 0.35f, 0.35f);
        }

        private void UpdateLayout()
        {
            if (_isVertical)
            {
                float leftWidth = (_size.X - _splitter.Size.X) * _splitRatio;
                float rightWidth = _size.X - _splitter.Size.X - leftWidth;

                if (leftWidth >= _minPanelSize && rightWidth >= _minPanelSize)
                {
                    if (_leftPanel != null)
                    {
                        _leftPanel.Position = _position;
                        _leftPanel.Size = new Vector2(leftWidth, _size.Y);
                    }

                    _splitter.Position = new Vector2(_position.X + leftWidth, _position.Y);
                    _splitter.Size = new Vector2(5, _size.Y);

                    if (_rightPanel != null)
                    {
                        _rightPanel.Position = new Vector2(_position.X + leftWidth + 5, _position.Y);
                        _rightPanel.Size = new Vector2(rightWidth, _size.Y);
                    }
                }
            }
            else
            {
                float topHeight = (_size.Y - _splitter.Size.Y) * _splitRatio;
                float bottomHeight = _size.Y - _splitter.Size.Y - topHeight;

                if (topHeight >= _minPanelSize && bottomHeight >= _minPanelSize)
                {
                    if (_leftPanel != null)
                    {
                        _leftPanel.Position = _position;
                        _leftPanel.Size = new Vector2(_size.X, topHeight);
                    }

                    _splitter.Position = new Vector2(_position.X, _position.Y + topHeight);
                    _splitter.Size = new Vector2(_size.X, 5);

                    if (_rightPanel != null)
                    {
                        _rightPanel.Position = new Vector2(_position.X, _position.Y + topHeight + 5);
                        _rightPanel.Size = new Vector2(_size.X, bottomHeight);
                    }
                }
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdateLayout();
        }
    }

    public class UISpinner : UIElement
    {
        private int _currentIndex;
        private List<string> _items;
        private float _rotationAngle;
        private float _rotationSpeed;
        private bool _isSpinning;
        private UILabel _itemLabel;
        private Color _arrowColor;

        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                if (value >= 0 && value < _items.Count)
                {
                    _currentIndex = value;
                    IndexChanged?.Invoke(this, _currentIndex);
                }
            }
        }

        public List<string> Items => _items;
        public float RotationSpeed
        {
            get => _rotationSpeed;
            set { _rotationSpeed = value; }
        }

        public Color ArrowColor
        {
            get => _arrowColor;
            set { _arrowColor = value; }
        }

        public event EventHandler<int>? IndexChanged;

        public UISpinner()
        {
            _currentIndex = 0;
            _items = new List<string>();
            _rotationAngle = 0;
            _rotationSpeed = 180;
            _isSpinning = false;
            _arrowColor = new Color(0.5f, 0.5f, 0.5f);
            _size = new Vector2(150, 40);

            _itemLabel = new UILabel("");
            _itemLabel.HorizontalTextAlignment = Alignment.Center;
            _itemLabel.VerticalTextAlignment = Alignment.Center;
        }

        public void AddItem(string item)
        {
            _items.Add(item);
            if (_currentIndex >= _items.Count)
            {
                _currentIndex = _items.Count - 1;
            }
        }

        public void RemoveItem(int index)
        {
            if (index >= 0 && index < _items.Count)
            {
                _items.RemoveAt(index);
                if (_currentIndex >= _items.Count)
                {
                    _currentIndex = Math.Max(0, _items.Count - 1);
                }
            }
        }

        public void Spin()
        {
            _isSpinning = true;
        }

        public void Stop()
        {
            _isSpinning = false;
        }

        public void Next()
        {
            if (_items.Count > 0)
            {
                _currentIndex = (_currentIndex + 1) % _items.Count;
                IndexChanged?.Invoke(this, _currentIndex);
            }
        }

        public void Previous()
        {
            if (_items.Count > 0)
            {
                _currentIndex = (_currentIndex - 1 + _items.Count) % _items.Count;
                IndexChanged?.Invoke(this, _currentIndex);
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_isSpinning)
            {
                _rotationAngle += _rotationSpeed * deltaTime;
                if (_rotationAngle >= 360)
                {
                    _rotationAngle -= 360;
                    Next();
                }
            }

            if (_items.Count > 0 && _currentIndex >= 0 && _currentIndex < _items.Count)
            {
                _itemLabel.Text = _items[_currentIndex];
            }
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            base.Draw(context);

            _itemLabel.Position = _position;
            _itemLabel.Size = _size;
            _itemLabel.Draw(context);

            float arrowSize = 10;
            float centerX = _position.X + _size.X - arrowSize / 2 - 5;
            float centerY = _position.Y + _size.Y / 2;

            var arrowBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(
                _arrowColor.GetAByte(), _arrowColor.GetRByte(), _arrowColor.GetGByte(), _arrowColor.GetBByte()));

            if (_rotationAngle > 0)
            {
                context.PushTransform(new System.Windows.Media.RotateTransform(_rotationAngle, centerX, centerY));
            }

            context.DrawPolygon(arrowBrush,
                new[] {
                    new System.Windows.Point(centerX - arrowSize / 2, centerY - arrowSize / 2),
                    new System.Windows.Point(centerX + arrowSize / 2, centerY - arrowSize / 2),
                    new System.Windows.Point(centerX, centerY + arrowSize / 2)
                });

            if (_rotationAngle > 0)
            {
                context.Pop();
            }
        }
    }

    public class UIAccordion : UIElement
    {
        private List<UIAccordionSection> _sections;
        private int _expandedIndex;
        private float _sectionHeight;
        private float _contentHeight;
        private float _animationSpeed;

        public List<UIAccordionSection> Sections => _sections;
        public int ExpandedIndex
        {
            get => _expandedIndex;
            set
            {
                if (value >= -1 && value < _sections.Count)
                {
                    _expandedIndex = value;
                    SectionChanged?.Invoke(this, _expandedIndex);
                }
            }
        }

        public float SectionHeight
        {
            get => _sectionHeight;
            set { _sectionHeight = value; }
        }

        public float AnimationSpeed
        {
            get => _animationSpeed;
            set { _animationSpeed = value; }
        }

        public event EventHandler<int>? SectionChanged;

        public UIAccordion()
        {
            _sections = new List<UIAccordionSection>();
            _expandedIndex = -1;
            _sectionHeight = 30;
            _contentHeight = 0;
            _animationSpeed = 5f;
            _size = new Vector2(300, 200);
        }

        public UIAccordionSection AddSection(string title)
        {
            var section = new UIAccordionSection(title);
            section.HeaderHeight = _sectionHeight;
            _sections.Add(section);
            UpdateLayout();
            return section;
        }

        public void RemoveSection(int index)
        {
            if (index >= 0 && index < _sections.Count)
            {
                _sections.RemoveAt(index);
                if (_expandedIndex == index)
                {
                    _expandedIndex = -1;
                }
                else if (_expandedIndex > index)
                {
                    _expandedIndex--;
                }
                UpdateLayout();
            }
        }

        public void ToggleSection(int index)
        {
            if (_expandedIndex == index)
            {
                _expandedIndex = -1;
            }
            else
            {
                _expandedIndex = index;
            }
            SectionChanged?.Invoke(this, _expandedIndex);
        }

        private void UpdateLayout()
        {
            float currentY = _position.Y;

            foreach (var section in _sections)
            {
                section.Position = new Vector2(_position.X, currentY);
                section.Size = new Vector2(_size.X, section.HeaderHeight);
                currentY += section.HeaderHeight;

                if (section == _sections[_expandedIndex] && section.Content != null)
                {
                    currentY += section.ContentHeight;
                }
            }

            _contentHeight = currentY - _position.Y;
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdateLayout();
        }
    }

    public class UIAccordionSection : UIElement
    {
        private string _title;
        private UILabel _headerLabel;
        private UIButton _toggleButton;
        private UIElement? _content;
        private float _headerHeight;
        private float _contentHeight;
        private bool _isExpanded;
        private float _expandProgress;
        private Color _headerColor;
        private Color _expandedHeaderColor;
        private Color _contentColor;

        public string Title
        {
            get => _title;
            set { _title = value; _headerLabel.Text = value; }
        }

        public UIElement? Content
        {
            get => _content;
            set => _content = value;
        }

        public float HeaderHeight
        {
            get => _headerHeight;
            set { _headerHeight = value; }
        }

        public float ContentHeight
        {
            get => _contentHeight;
            set { _contentHeight = value; }
        }

        public bool IsExpanded
        {
            get => _isExpanded;
            set { _isExpanded = value; }
        }

        public Color HeaderColor
        {
            get => _headerColor;
            set { _headerColor = value; }
        }

        public Color ExpandedHeaderColor
        {
            get => _expandedHeaderColor;
            set { _expandedHeaderColor = value; }
        }

        public Color ContentColor
        {
            get => _contentColor;
            set { _contentColor = value; }
        }

        public UIAccordionSection()
        {
            _title = "Section";
            _headerHeight = 30;
            _contentHeight = 100;
            _isExpanded = false;
            _expandProgress = 0;
            _headerColor = new Color(0.25f, 0.25f, 0.25f);
            _expandedHeaderColor = new Color(0.35f, 0.35f, 0.35f);
            _contentColor = new Color(0.2f, 0.2f, 0.2f);

            _headerLabel = new UILabel(_title);
            _headerLabel.BackgroundColor = _headerColor;
            _headerLabel.Size = new Vector2(200, _headerHeight);

            _toggleButton = new UIButton();
            _toggleButton.Size = new Vector2(30, _headerHeight);
            _toggleButton.Text = "+";
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _headerLabel.Position = _position;
            _headerLabel.Size = new Vector2(_size.X - 35, _headerHeight);
            _headerLabel.BackgroundColor = _isExpanded ? _expandedHeaderColor : _headerColor;

            _toggleButton.Position = new Vector2(_position.X + _size.X - 30, _position.Y);
            _toggleButton.Size = new Vector2(30, _headerHeight);
            _toggleButton.Text = _isExpanded ? "-" : "+";

            if (_content != null)
            {
                _content.Position = new Vector2(_position.X, _position.Y + _headerHeight);
                _content.Size = new Vector2(_size.X, _contentHeight);
                _content.IsVisible = _isExpanded;
            }
        }
    }

    public class UISystem : Component
    {
        private UICanvas _rootCanvas;
        private UILayoutManager _layoutManager;
        private UIFocusManager _focusManager;
        private UIClipping _clippingManager;
        private UITheme _currentTheme;
        private UISkin _currentSkin;
        private List<UIElement> _updateQueue;
        private List<UIElement> _drawQueue;
        private bool _isPaused;
        private float _updateInterval;

        public UICanvas RootCanvas => _rootCanvas;
        public UILayoutManager LayoutManager => _layoutManager;
        public UIFocusManager FocusManager => _focusManager;
        public UIClipping ClippingManager => _clippingManager;
        public UITheme CurrentTheme
        {
            get => _currentTheme;
            set { _currentTheme = value; ApplyTheme(); }
        }
        public UISkin CurrentSkin
        {
            get => _currentSkin;
            set { _currentSkin = value; ApplySkin(); }
        }
        public bool IsPaused
        {
            get => _isPaused;
            set { _isPaused = value; }
        }
        public float UpdateInterval
        {
            get => _updateInterval;
            set { _updateInterval = value; }
        }

        public UISystem()
        {
            _rootCanvas = new UICanvas();
            _layoutManager = new UILayoutManager();
            _focusManager = new UIFocusManager();
            _clippingManager = new UIClipping();
            _currentTheme = new UITheme();
            _currentSkin = new UISkin();
            _updateQueue = new List<UIElement>();
            _drawQueue = new List<UIElement>();
            _isPaused = false;
            _updateInterval = 1 / 60f;
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
        }

        public void AddElement(UIElement element)
        {
            _rootCanvas.AddChild(element);
            _updateQueue.Add(element);
            _drawQueue.Add(element);
        }

        public void RemoveElement(UIElement element)
        {
            _rootCanvas.RemoveChild(element);
            _updateQueue.Remove(element);
            _drawQueue.Remove(element);
        }

        public void BringToFront(UIElement element)
        {
            element.SortingOrder = _rootCanvas.Children.Max(c => c.SortingOrder) + 1;
        }

        public void SendToBack(UIElement element)
        {
            element.SortingOrder = _rootCanvas.Children.Min(c => c.SortingOrder) - 1;
        }

        private void ApplyTheme()
        {
            foreach (var element in _updateQueue)
            {
                element.BackgroundColor = _currentTheme.BackgroundColor;
            }
        }

        private void ApplySkin()
        {
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (_isPaused) return;

            foreach (var element in _updateQueue)
            {
                if (element.IsVisible)
                {
                    element.Update(deltaTime);
                }
            }

            _layoutManager.UpdateLayout();
        }

        public void Draw(DrawingContext context)
        {
            _rootCanvas.Draw(context);
        }

        public T CreateElement<T>() where T : UIElement, new()
        {
            var element = new T();
            AddElement(element);
            return element;
        }

        public void ClearAll()
        {
            _rootCanvas.ClearChildren();
            _updateQueue.Clear();
            _drawQueue.Clear();
        }
    }

    public class UILayoutManager
    {
        private List<UIElement> _dirtyElements;
        private bool _isUpdating;
        private float _updateThreshold;

        public List<UIElement> DirtyElements => _dirtyElements;
        public bool IsUpdating => _isUpdating;
        public float UpdateThreshold
        {
            get => _updateThreshold;
            set { _updateThreshold = value; }
        }

        public UILayoutManager()
        {
            _dirtyElements = new List<UIElement>();
            _isUpdating = false;
            _updateThreshold = 0.001f;
        }

        public void MarkDirty(UIElement element)
        {
            if (!_dirtyElements.Contains(element))
            {
                _dirtyElements.Add(element);
            }
        }

        public void UpdateLayout()
        {
            if (_isUpdating) return;

            _isUpdating = true;

            while (_dirtyElements.Count > 0)
            {
                var element = _dirtyElements[0];
                _dirtyElements.RemoveAt(0);

                if (element.IsVisible)
                {
                    element.OnLayoutChanged();
                }
            }

            _isUpdating = false;
        }

        public void ForceUpdateAll()
        {
            _dirtyElements.Clear();
        }
    }

    public class UIFocusManager
    {
        private UIElement? _focusedElement;
        private UIElement? _previouslyFocusedElement;
        private List<UIElement> _focusableElements;
        private bool _isNavigating;

        public UIElement? FocusedElement => _focusedElement;
        public UIElement? PreviouslyFocusedElement => _previouslyFocusedElement;
        public List<UIElement> FocusableElements => _focusableElements;

        public event EventHandler<UIElement>? FocusChanged;

        public UIFocusManager()
        {
            _focusedElement = null;
            _previouslyFocusedElement = null;
            _focusableElements = new List<UIElement>();
            _isNavigating = false;
        }

        public void RegisterFocusable(UIElement element)
        {
            if (!_focusableElements.Contains(element))
            {
                _focusableElements.Add(element);
            }
        }

        public void UnregisterFocusable(UIElement element)
        {
            _focusableElements.Remove(element);

            if (_focusedElement == element)
            {
                SetFocus(null);
            }
        }

        public void SetFocus(UIElement? element)
        {
            if (_focusedElement != element)
            {
                _previouslyFocusedElement = _focusedElement;
                _focusedElement = element;

                _previouslyFocusedElement?.OnLostFocus();
                _focusedElement?.OnGotFocus();

                FocusChanged?.Invoke(this, _focusedElement!);
            }
        }

        public void NavigateNext()
        {
            if (_focusableElements.Count == 0) return;

            int currentIndex = _focusedElement != null ?
                _focusableElements.IndexOf(_focusedElement) : -1;

            int nextIndex = (currentIndex + 1) % _focusableElements.Count;
            SetFocus(_focusableElements[nextIndex]);
        }

        public void NavigatePrevious()
        {
            if (_focusableElements.Count == 0) return;

            int currentIndex = _focusedElement != null ?
                _focusableElements.IndexOf(_focusedElement) : 0;

            int prevIndex = (currentIndex - 1 + _focusableElements.Count) % _focusableElements.Count;
            SetFocus(_focusableElements[prevIndex]);
        }

        public void ClearFocus()
        {
            SetFocus(null);
        }
    }

    public class UIClipping
    {
        private Rect _viewport;
        private List<Rect> _clipRegions;
        private bool _enableClipping;

        public Rect Viewport
        {
            get => _viewport;
            set { _viewport = value; }
        }

        public List<Rect> ClipRegions => _clipRegions;
        public bool EnableClipping
        {
            get => _enableClipping;
            set { _enableClipping = value; }
        }

        public UIClipping()
        {
            _viewport = new Rect(0, 0, 1920, 1080);
            _clipRegions = new List<Rect>();
            _enableClipping = true;
        }

        public void AddClipRegion(Rect region)
        {
            _clipRegions.Add(region);
        }

        public void RemoveClipRegion(Rect region)
        {
            _clipRegions.Remove(region);
        }

        public void ClearClipRegions()
        {
            _clipRegions.Clear();
        }

        public bool IsPointVisible(Vector2 point)
        {
            if (!_enableClipping) return true;

            if (!_viewport.Contains(point)) return false;

            foreach (var region in _clipRegions)
            {
                if (!region.Contains(point)) return false;
            }

            return true;
        }

        public bool IsRectVisible(Rect rect)
        {
            if (!_enableClipping) return true;

            if (!_viewport.Intersects(rect)) return false;

            foreach (var region in _clipRegions)
            {
                if (!region.Intersects(rect)) return false;
            }

            return true;
        }
    }

    public class UITheme
    {
        private Color _backgroundColor;
        private Color _textColor;
        private Color _primaryColor;
        private Color _secondaryColor;
        private Color _accentColor;
        private Color _errorColor;
        private Color _warningColor;
        private Color _successColor;
        private string _fontFamily;
        private float _fontSize;
        private float _borderRadius;
        private float _padding;
        private float _spacing;

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

        public Color PrimaryColor
        {
            get => _primaryColor;
            set => _primaryColor = value;
        }

        public Color SecondaryColor
        {
            get => _secondaryColor;
            set => _secondaryColor = value;
        }

        public Color AccentColor
        {
            get => _accentColor;
            set => _accentColor = value;
        }

        public Color ErrorColor
        {
            get => _errorColor;
            set => _errorColor = value;
        }

        public Color WarningColor
        {
            get => _warningColor;
            set => _warningColor = value;
        }

        public Color SuccessColor
        {
            get => _successColor;
            set => _successColor = value;
        }

        public string FontFamily
        {
            get => _fontFamily;
            set => _fontFamily = value;
        }

        public float FontSize
        {
            get => _fontSize;
            set => _fontSize = value;
        }

        public float BorderRadius
        {
            get => _borderRadius;
            set => _borderRadius = value;
        }

        public float Padding
        {
            get => _padding;
            set => _padding = value;
        }

        public float Spacing
        {
            get => _spacing;
            set => _spacing = value;
        }

        public UITheme()
        {
            _backgroundColor = new Color(0.2f, 0.2f, 0.2f);
            _textColor = Color.White;
            _primaryColor = new Color(0.2f, 0.5f, 0.9f);
            _secondaryColor = new Color(0.4f, 0.4f, 0.4f);
            _accentColor = new Color(0.8f, 0.4f, 0.2f);
            _errorColor = new Color(0.9f, 0.2f, 0.2f);
            _warningColor = new Color(0.9f, 0.8f, 0.2f);
            _successColor = new Color(0.2f, 0.8f, 0.4f);
            _fontFamily = "Segoe UI";
            _fontSize = 14;
            _borderRadius = 4;
            _padding = 10;
            _spacing = 5;
        }

        public static UITheme Dark => new UITheme
        {
            _backgroundColor = new Color(0.15f, 0.15f, 0.15f),
            _textColor = Color.White,
            _primaryColor = new Color(0.3f, 0.5f, 0.9f)
        };

        public static UITheme Light => new UITheme
        {
            _backgroundColor = new Color(0.95f, 0.95f, 0.95f),
            _textColor = Color.Black,
            _primaryColor = new Color(0.2f, 0.4f, 0.8f)
        };
    }

    public class UISkin
    {
        private Dictionary<string, UISkinElement> _elements;
        private UITheme _theme;
        private string _name;

        public Dictionary<string, UISkinElement> Elements => _elements;
        public UITheme Theme
        {
            get => _theme;
            set => _theme = value;
        }
        public string Name
        {
            get => _name;
            set => _name = value;
        }

        public UISkin()
        {
            _elements = new Dictionary<string, UISkinElement>();
            _theme = new UITheme();
            _name = "Default";
        }

        public void RegisterElement(string name, UISkinElement element)
        {
            _elements[name] = element;
        }

        public UISkinElement? GetElement(string name)
        {
            return _elements.TryGetValue(name, out var element) ? element : null;
        }

        public void ApplySkin(UIElement element)
        {
            var skinElement = GetElement(element.GetType().Name);
            if (skinElement != null)
            {
                element.BackgroundColor = skinElement.BackgroundColor;
                element.BorderColor = skinElement.BorderColor;
                element.BorderWidth = skinElement.BorderWidth;
            }
        }
    }

    public class UISkinElement
    {
        private Color _backgroundColor;
        private Color _borderColor;
        private float _borderWidth;
        private float _borderRadius;
        private Color _textColor;
        private string? _fontFamily;
        private float? _fontSize;

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set => _backgroundColor = value;
        }

        public Color BorderColor
        {
            get => _borderColor;
            set => _borderColor = value;
        }

        public float BorderWidth
        {
            get => _borderWidth;
            set => _borderWidth = value;
        }

        public float BorderRadius
        {
            get => _borderRadius;
            set => _borderRadius = value;
        }

        public Color TextColor
        {
            get => _textColor;
            set => _textColor = value;
        }

        public string? FontFamily
        {
            get => _fontFamily;
            set => _fontFamily = value;
        }

        public float? FontSize
        {
            get => _fontSize;
            set => _fontSize = value;
        }

        public UISkinElement()
        {
            _backgroundColor = Color.Transparent;
            _borderColor = Color.Transparent;
            _borderWidth = 0;
            _borderRadius = 0;
            _textColor = Color.White;
        }
    }
}

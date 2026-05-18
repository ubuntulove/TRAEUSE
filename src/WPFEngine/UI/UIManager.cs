using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using WPFEngine.ECS;
using WPFEngine.Scene;

namespace WPFEngine.UI
{
    public enum UIAnchor
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight,
        Stretch
    }

    public enum UIPivot
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        Center,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum UIGridLayout
    {
        Freeform,
        Horizontal,
        Vertical,
        Grid
    }

    public enum UIGridConstraint
    {
        Flexible,
        FixedSize,
        AutoFit
    }

    public class UIManager : Component, IDisposable
    {
        private Canvas rootCanvas;
        private List<UIElement> uiElements;
        private Dictionary<string, UIElement> namedElements;
        private Dictionary<string, UIPanel> panels;
        private List<UIButton> buttons;
        private List<UILabel> labels;
        private UIPanel activePanel;
        private UIElement focusedElement;
        private UIElement hoveredElement;
        private bool isInitialized;
        private float uiScale;
        private Vector2 screenSize;
        private UIFont defaultFont;
        private Color defaultTextColor;
        private Color defaultBackgroundColor;
        private float defaultPadding;
        private float defaultSpacing;
        private bool enableRaycast;
        private bool enableDebugOutline;
        private int nextElementId;
        private Dictionary<int, UIElement> elementLookup;
        private List<UIEventHandler> eventHandlers;
        private bool handleNavigation;
        private bool enableKeyboardNavigation;
        private bool enableMouseNavigation;
        private UIElement navigationUp;
        private UIElement navigationDown;
        private UIElement navigationLeft;
        private UIElement navigationRight;
        private UILayoutController layoutController;
        private bool autoLayout;
        private UIGridLayout gridLayout;
        private int gridColumns;
        private int gridRows;
        private Vector2 cellSize;
        private Vector2 spacing;
        private UIGridConstraint columnConstraint;
        private UIGridConstraint rowConstraint;
        private float startCorner;
        private bool fitInContainer;
        private bool autoUpdateLayout;
        private Theme currentTheme;
        private Dictionary<string, Style> customStyles;

        public event EventHandler<UIEventArgs> ElementClicked;
        public event EventHandler<UIEventArgs> ElementHovered;
        public event EventHandler<UIEventArgs> ElementPressed;
        public event EventHandler<UIEventArgs> ElementReleased;
        public event EventHandler<UIEventArgs> ElementSelected;
        public event EventHandler<UIEventArgs> PanelOpened;
        public event EventHandler<UIEventArgs> PanelClosed;
        public event EventHandler<UIEventArgs> FocusChanged;
        public event EventHandler<UIEventArgs> LayoutChanged;

        public Canvas RootCanvas
        {
            get => rootCanvas;
            internal set => rootCanvas = value;
        }

        public UIPanel ActivePanel
        {
            get => activePanel;
            set
            {
                if (activePanel != value)
                {
                    activePanel = value;
                    if (value != null)
                    {
                        PanelOpened?.Invoke(this, new UIEventArgs(value));
                    }
                }
            }
        }

        public UIElement FocusedElement
        {
            get => focusedElement;
            set
            {
                if (focusedElement != value)
                {
                    focusedElement?.OnFocusLost();
                    focusedElement = value;
                    focusedElement?.OnFocusGained();
                    FocusChanged?.Invoke(this, new UIEventArgs(value));
                }
            }
        }

        public UIElement HoveredElement
        {
            get => hoveredElement;
            private set
            {
                if (hoveredElement != value)
                {
                    hoveredElement?.OnHoverEnd();
                    hoveredElement = value;
                    hoveredElement?.OnHoverBegin();
                    if (value != null)
                    {
                        ElementHovered?.Invoke(this, new UIEventArgs(value));
                    }
                }
            }
        }

        public float UIScale
        {
            get => uiScale;
            set => uiScale = Math.Max(0.1f, value);
        }

        public Vector2 ScreenSize
        {
            get => screenSize;
            set => screenSize = value;
        }

        public UIFont DefaultFont
        {
            get => defaultFont;
            set => defaultFont = value;
        }

        public Color DefaultTextColor
        {
            get => defaultTextColor;
            set => defaultTextColor = value;
        }

        public Color DefaultBackgroundColor
        {
            get => defaultBackgroundColor;
            set => defaultBackgroundColor = value;
        }

        public float DefaultPadding
        {
            get => defaultPadding;
            set => defaultPadding = Math.Max(0f, value);
        }

        public float DefaultSpacing
        {
            get => defaultSpacing;
            set => defaultSpacing = Math.Max(0f, value);
        }

        public bool EnableRaycast
        {
            get => enableRaycast;
            set => enableRaycast = value;
        }

        public bool EnableDebugOutline
        {
            get => enableDebugOutline;
            set => enableDebugOutline = value;
        }

        public bool HandleNavigation
        {
            get => handleNavigation;
            set => handleNavigation = value;
        }

        public bool EnableKeyboardNavigation
        {
            get => enableKeyboardNavigation;
            set => enableKeyboardNavigation = value;
        }

        public bool EnableMouseNavigation
        {
            get => enableMouseNavigation;
            set => enableMouseNavigation = value;
        }

        public UILayoutController LayoutController
        {
            get => layoutController;
            set => layoutController = value;
        }

        public bool AutoLayout
        {
            get => autoLayout;
            set => autoLayout = value;
        }

        public Theme CurrentTheme
        {
            get => currentTheme;
            set
            {
                currentTheme = value;
                ApplyTheme(value);
            }
        }

        public IReadOnlyDictionary<string, Style> CustomStyles => customStyles;
        public IReadOnlyList<UIElement> Elements => uiElements.AsReadOnly();
        public IReadOnlyList<UIPanel> Panels => new List<UIPanel>(panels.Values).AsReadOnly();
        public IReadOnlyList<UIButton> Buttons => buttons.AsReadOnly();
        public IReadOnlyList<UILabel> Labels => labels.AsReadOnly();

        public UIManager()
        {
            uiElements = new List<UIElement>();
            namedElements = new Dictionary<string, UIElement>();
            panels = new Dictionary<string, UIPanel>();
            buttons = new List<UIButton>();
            labels = new List<UILabel>();
            isInitialized = false;
            uiScale = 1f;
            screenSize = new Vector2(1920, 1080);
            defaultFont = new UIFont { Family = "Segoe UI", Size = 14 };
            defaultTextColor = Colors.White;
            defaultBackgroundColor = Colors.Transparent;
            defaultPadding = 8f;
            defaultSpacing = 8f;
            enableRaycast = true;
            enableDebugOutline = false;
            nextElementId = 0;
            elementLookup = new Dictionary<int, UIElement>();
            eventHandlers = new List<UIEventHandler>();
            handleNavigation = true;
            enableKeyboardNavigation = true;
            enableMouseNavigation = true;
            layoutController = new UILayoutController();
            autoLayout = false;
            gridLayout = UIGridLayout.Freeform;
            gridColumns = 4;
            gridRows = 4;
            cellSize = new Vector2(100, 50);
            spacing = new Vector2(8, 8);
            columnConstraint = UIGridConstraint.Flexible;
            rowConstraint = UIGridConstraint.Flexible;
            startCorner = 0f;
            fitInContainer = true;
            autoUpdateLayout = true;
            currentTheme = null;
            customStyles = new Dictionary<string, Style>();
        }

        public override void Initialize(GameObject gameObject)
        {
            base.Initialize(gameObject);
            InitializeRootCanvas();
            isInitialized = true;
        }

        private void InitializeRootCanvas()
        {
            if (rootCanvas == null)
            {
                rootCanvas = new Canvas();
                rootCanvas.Width = screenSize.X;
                rootCanvas.Height = screenSize.Y;
                rootCanvas.Background = Brushes.Transparent;
            }
        }

        public T CreateElement<T>(string name = null) where T : UIElement, new()
        {
            var element = new T
            {
                Name = name ?? $"Element_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            AddElement(element);
            return element;
        }

        public UIButton CreateButton(string text = "", string name = null)
        {
            var button = new UIButton
            {
                Name = name ?? $"Button_{nextElementId}",
                Id = nextElementId++,
                Manager = this,
                Text = text,
                Width = 120,
                Height = 40
            };

            button.Click += OnButtonClick;
            button.MouseEnter += OnButtonMouseEnter;
            button.MouseLeave += OnButtonMouseLeave;
            button.MouseDown += OnButtonMouseDown;
            button.MouseUp += OnButtonMouseUp;

            AddElement(button);
            buttons.Add(button);
            return button;
        }

        private void OnButtonClick(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                ElementClicked?.Invoke(this, new UIEventArgs(button));
            }
        }

        private void OnButtonMouseEnter(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                HoveredElement = button;
            }
        }

        private void OnButtonMouseLeave(object sender, EventArgs e)
        {
            if (sender is UIButton button && HoveredElement == button)
            {
                HoveredElement = null;
            }
        }

        private void OnButtonMouseDown(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                ElementPressed?.Invoke(this, new UIEventArgs(button));
            }
        }

        private void OnButtonMouseUp(object sender, EventArgs e)
        {
            if (sender is UIButton button)
            {
                ElementReleased?.Invoke(this, new UIEventArgs(button));
            }
        }

        public UILabel CreateLabel(string text = "", string name = null)
        {
            var label = new UILabel
            {
                Name = name ?? $"Label_{nextElementId}",
                Id = nextElementId++,
                Manager = this,
                Text = text
            };

            AddElement(label);
            labels.Add(label);
            return label;
        }

        public UIPanel CreatePanel(string name = null)
        {
            var panel = new UIPanel
            {
                Name = name ?? $"Panel_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            AddElement(panel);
            panels[panel.Name] = panel;
            return panel;
        }

        public UITextField CreateTextField(string name = null)
        {
            var textField = new UITextField
            {
                Name = name ?? $"TextField_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            AddElement(textField);
            return textField;
        }

        public UIImage CreateImage(string name = null)
        {
            var image = new UIImage
            {
                Name = name ?? $"Image_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            AddElement(image);
            return image;
        }

        public UIToggle CreateToggle(string text = "", string name = null)
        {
            var toggle = new UIToggle
            {
                Name = name ?? $"Toggle_{nextElementId}",
                Id = nextElementId++,
                Manager = this,
                LabelText = text
            };

            toggle.ValueChanged += OnToggleValueChanged;
            AddElement(toggle);
            return toggle;
        }

        private void OnToggleValueChanged(object sender, EventArgs e)
        {
            if (sender is UIToggle toggle)
            {
                ElementClicked?.Invoke(this, new UIEventArgs(toggle));
            }
        }

        public UISlider CreateSlider(string name = null)
        {
            var slider = new UISlider
            {
                Name = name ?? $"Slider_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            slider.ValueChanged += OnSliderValueChanged;
            AddElement(slider);
            return slider;
        }

        private void OnSliderValueChanged(object sender, EventArgs e)
        {
            if (sender is UISlider slider)
            {
                ElementClicked?.Invoke(this, new UIEventArgs(slider));
            }
        }

        public UIProgressBar CreateProgressBar(string name = null)
        {
            var progressBar = new UIProgressBar
            {
                Name = name ?? $"ProgressBar_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            AddElement(progressBar);
            return progressBar;
        }

        public UIScrollView CreateScrollView(string name = null)
        {
            var scrollView = new UIScrollView
            {
                Name = name ?? $"ScrollView_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            AddElement(scrollView);
            return scrollView;
        }

        public UIDropdown CreateDropdown(string name = null)
        {
            var dropdown = new UIDropdown
            {
                Name = name ?? $"Dropdown_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            AddElement(dropdown);
            return dropdown;
        }

        public UIListView CreateListView(string name = null)
        {
            var listView = new UIListView
            {
                Name = name ?? $"ListView_{nextElementId}",
                Id = nextElementId++,
                Manager = this
            };

            AddElement(listView);
            return listView;
        }

        public UIGridLayoutController CreateGridLayout(int columns, int rows)
        {
            var grid = new UIGridLayoutController
            {
                Columns = columns,
                Rows = rows,
                CellSize = cellSize,
                Spacing = spacing
            };

            return grid;
        }

        public UIHorizontalLayout CreateHorizontalLayout()
        {
            return new UIHorizontalLayout
            {
                Spacing = defaultSpacing,
                Padding = defaultPadding,
                ChildAlignment = UIChildAlignment.MiddleCenter
            };
        }

        public UIVerticalLayout CreateVerticalLayout()
        {
            return new UIVerticalLayout
            {
                Spacing = defaultSpacing,
                Padding = defaultPadding,
                ChildAlignment = UIChildAlignment.MiddleCenter
            };
        }

        private void AddElement(UIElement element)
        {
            if (element == null)
                return;

            uiElements.Add(element);
            elementLookup[element.Id] = element;

            if (!string.IsNullOrEmpty(element.Name))
            {
                namedElements[element.Name] = element;
            }

            if (rootCanvas != null)
            {
                rootCanvas.Children.Add(element.NativeElement);
            }
        }

        public void RemoveElement(UIElement element)
        {
            if (element == null)
                return;

            uiElements.Remove(element);
            elementLookup.Remove(element.Id);

            if (!string.IsNullOrEmpty(element.Name))
            {
                namedElements.Remove(element.Name);
            }

            if (element is UIButton button)
            {
                buttons.Remove(button);
            }
            else if (element is UILabel label)
            {
                labels.Remove(label);
            }
            else if (element is UIPanel panel)
            {
                panels.Remove(panel.Name);
            }

            if (rootCanvas != null && element.NativeElement != null)
            {
                rootCanvas.Children.Remove(element.NativeElement);
            }

            if (FocusedElement == element)
            {
                FocusedElement = null;
            }

            if (HoveredElement == element)
            {
                HoveredElement = null;
            }
        }

        public void RemoveElement(string name)
        {
            if (namedElements.TryGetValue(name, out var element))
            {
                RemoveElement(element);
            }
        }

        public T GetElement<T>(string name) where T : UIElement
        {
            return namedElements.TryGetValue(name, out var element) ? element as T : null;
        }

        public UIElement GetElement(string name)
        {
            return namedElements.TryGetValue(name, out var element) ? element : null;
        }

        public UIElement GetElement(int id)
        {
            return elementLookup.TryGetValue(id, out var element) ? element : null;
        }

        public T[] GetElements<T>() where T : UIElement
        {
            var results = new List<T>();
            foreach (var element in uiElements)
            {
                if (element is T typedElement)
                {
                    results.Add(typedElement);
                }
            }
            return results.ToArray();
        }

        public void ShowPanel(string name)
        {
            if (panels.TryGetValue(name, out var panel))
            {
                panel.IsVisible = true;
                ActivePanel = panel;
            }
        }

        public void HidePanel(string name)
        {
            if (panels.TryGetValue(name, out var panel))
            {
                panel.IsVisible = false;
                if (ActivePanel == panel)
                {
                    ActivePanel = null;
                }
                PanelClosed?.Invoke(this, new UIEventArgs(panel));
            }
        }

        public void ShowAllPanels()
        {
            foreach (var panel in panels.Values)
            {
                panel.IsVisible = true;
            }
        }

        public void HideAllPanels()
        {
            foreach (var panel in panels.Values)
            {
                panel.IsVisible = false;
            }
            ActivePanel = null;
        }

        public void BringToFront(UIElement element)
        {
            if (element?.NativeElement != null && rootCanvas != null)
            {
                var index = System.Windows.Controls.Panel.GetZIndex(element.NativeElement);
                System.Windows.Controls.Panel.SetZIndex(element.NativeElement, index + 1);
            }
        }

        public void SendToBack(UIElement element)
        {
            if (element?.NativeElement != null && rootCanvas != null)
            {
                var index = System.Windows.Controls.Panel.GetZIndex(element.NativeElement);
                System.Windows.Controls.Panel.SetZIndex(element.NativeElement, index - 1);
            }
        }

        public void UpdateLayout()
        {
            foreach (var element in uiElements)
            {
                element.UpdateLayout();
            }
            LayoutChanged?.Invoke(this, new UIEventArgs(null));
        }

        public void RefreshAllElements()
        {
            foreach (var element in uiElements)
            {
                element.Refresh();
            }
        }

        public void SetScreenSize(float width, float height)
        {
            screenSize = new Vector2(width, height);
            if (rootCanvas != null)
            {
                rootCanvas.Width = width;
                rootCanvas.Height = height;
            }
        }

        public Vector2 ScreenToUI(Vector2 screenPosition)
        {
            return new Vector2(
                screenPosition.X * screenSize.X / (rootCanvas?.ActualWidth ?? screenSize.X),
                screenPosition.Y * screenSize.Y / (rootCanvas?.ActualHeight ?? screenSize.Y)
            );
        }

        public Vector2 UIToScreen(Vector2 uiPosition)
        {
            return new Vector2(
                uiPosition.X * (rootCanvas?.ActualWidth ?? screenSize.X) / screenSize.X,
                uiPosition.Y * (rootCanvas?.ActualHeight ?? screenSize.Y) / screenSize.Y
            );
        }

        public void RegisterEventHandler(UIEventHandler handler)
        {
            if (handler != null && !eventHandlers.Contains(handler))
            {
                eventHandlers.Add(handler);
            }
        }

        public void UnregisterEventHandler(UIEventHandler handler)
        {
            eventHandlers.Remove(handler);
        }

        public void ApplyTheme(Theme theme)
        {
            if (theme == null)
                return;

            currentTheme = theme;

            foreach (var element in uiElements)
            {
                element.ApplyTheme(theme);
            }
        }

        public void RegisterStyle(string styleName, Style style)
        {
            customStyles[styleName] = style;
        }

        public Style GetStyle(string styleName)
        {
            return customStyles.TryGetValue(styleName, out var style) ? style : null;
        }

        public void ApplyStyle(string styleName, UIElement element)
        {
            if (customStyles.TryGetValue(styleName, out var style))
            {
                style.Apply(element);
            }
        }

        public void ClearAll()
        {
            var elementsToRemove = new List<UIElement>(uiElements);
            foreach (var element in elementsToRemove)
            {
                RemoveElement(element);
            }

            uiElements.Clear();
            namedElements.Clear();
            panels.Clear();
            buttons.Clear();
            labels.Clear();
            elementLookup.Clear();

            if (rootCanvas != null)
            {
                rootCanvas.Children.Clear();
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            if (!isInitialized)
                return;

            if (enableKeyboardNavigation)
            {
                HandleKeyboardNavigation();
            }

            foreach (var element in uiElements)
            {
                element.Update();
            }

            if (autoUpdateLayout)
            {
                UpdateLayout();
            }
        }

        private void HandleKeyboardNavigation()
        {
            if (FocusedElement == null)
                return;
        }

        public void HandleMouseMove(Vector2 position)
        {
            var uiPosition = ScreenToUI(position);
            UIElement hitElement = null;

            foreach (var element in uiElements)
            {
                if (element.IsVisible && element.IsInteractable && ContainsPoint(element, uiPosition))
                {
                    hitElement = element;
                    break;
                }
            }

            HoveredElement = hitElement;
        }

        public void HandleMouseClick(Vector2 position)
        {
            var uiPosition = ScreenToUI(position);

            foreach (var element in uiElements)
            {
                if (element.IsVisible && element.IsInteractable && ContainsPoint(element, uiPosition))
                {
                    FocusedElement = element;
                    element.OnClick();
                    ElementClicked?.Invoke(this, new UIEventArgs(element));
                    break;
                }
            }
        }

        private bool ContainsPoint(UIElement element, Vector2 point)
        {
            return point.X >= element.Position.X &&
                   point.X <= element.Position.X + element.Width &&
                   point.Y >= element.Position.Y &&
                   point.Y <= element.Position.Y + element.Height;
        }

        public override string ToString()
        {
            return $"UIManager(Elements={uiElements.Count}, Buttons={buttons.Count}, Labels={labels.Count})";
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearAll();
                eventHandlers.Clear();
                customStyles.Clear();
                RootCanvas = null;
            }
        }
    }

    public abstract class UIElement
    {
        protected string name;
        protected int id;
        protected UIManager manager;
        protected FrameworkElement nativeElement;
        protected Vector2 position;
        protected Vector2 size;
        protected float width;
        protected float height;
        protected Vector2 anchor;
        protected Vector2 pivot;
        protected Vector2 anchorOffset;
        protected bool isVisible;
        protected bool isInteractable;
        protected bool isEnabled;
        protected Color backgroundColor;
        protected Color textColor;
        protected float opacity;
        protected float rotation;
        protected int zIndex;
        protected string tooltip;
        protected object userData;
        protected List<UIElement> children;
        protected UIElement parent;
        protected UILayoutElement layoutElement;
        protected bool ignoreLayout;
        protected bool isDraggable;
        protected Vector2 dragOffset;
        protected bool isDirty;

        public event EventHandler<EventArgs> Click;
        public event EventHandler<EventArgs> DoubleClick;
        public event EventHandler<EventArgs> HoverBegin;
        public event EventHandler<EventArgs> HoverEnd;
        public event EventHandler<EventArgs> Press;
        public event EventHandler<EventArgs> Release;
        public event EventHandler<EventArgs> FocusGained;
        public event EventHandler<EventArgs> FocusLost;
        public event EventHandler<EventArgs> ValueChanged;
        public event EventHandler<EventArgs> VisibilityChanged;
        public event EventHandler<EventArgs> SizeChanged;

        public string Name { get => name; set => name = value; }
        public int Id { get => id; set => id = value; }
        public UIManager Manager { get => manager; internal set => manager = value; }
        public FrameworkElement NativeElement => nativeElement;
        public Vector2 Position { get => position; set { position = value; isDirty = true; } }
        public Vector2 Size { get => size; set { size = value; width = value.X; height = value.Y; isDirty = true; } }
        public float Width { get => width; set { width = value; size = new Vector2(value, height); isDirty = true; } }
        public float Height { get => height; set { height = value; size = new Vector2(width, value); isDirty = true; } }
        public Vector2 Anchor { get => anchor; set => anchor = value; }
        public Vector2 Pivot { get => pivot; set => pivot = value; }
        public Vector2 AnchorOffset { get => anchorOffset; set => anchorOffset = value; }
        public bool IsVisible { get => isVisible; set { isVisible = value; OnVisibilityChanged(); } }
        public bool IsInteractable { get => isInteractable; set => isInteractable = value; }
        public bool IsEnabled { get => isEnabled; set { isEnabled = value; OnEnabledChanged(); } }
        public Color BackgroundColor { get => backgroundColor; set { backgroundColor = value; OnBackgroundColorChanged(); } }
        public Color TextColor { get => textColor; set { textColor = value; OnTextColorChanged(); } }
        public float Opacity { get => opacity; set { opacity = Math.Max(0f, Math.Min(1f, value)); OnOpacityChanged(); } }
        public float Rotation { get => rotation; set { rotation = value; OnRotationChanged(); } }
        public int ZIndex { get => zIndex; set { zIndex = value; OnZIndexChanged(); } }
        public string Tooltip { get => tooltip; set => tooltip = value; }
        public object UserData { get => userData; set => userData = value; }
        public IReadOnlyList<UIElement> Children => children.AsReadOnly();
        public UIElement Parent { get => parent; set => parent = value; }
        public UILayoutElement LayoutElement { get => layoutElement; set => layoutElement = value; }
        public bool IgnoreLayout { get => ignoreLayout; set => ignoreLayout = value; }
        public bool IsDraggable { get => isDraggable; set => isDraggable = value; }

        protected UIElement()
        {
            name = "UIElement";
            id = 0;
            position = Vector2.Zero;
            size = new Vector2(100, 50);
            width = 100;
            height = 50;
            anchor = new Vector2(0, 0);
            pivot = new Vector2(0, 0);
            anchorOffset = Vector2.Zero;
            isVisible = true;
            isInteractable = true;
            isEnabled = true;
            backgroundColor = Colors.White;
            textColor = Colors.Black;
            opacity = 1f;
            rotation = 0f;
            zIndex = 0;
            children = new List<UIElement>();
            layoutElement = new UILayoutElement();
            ignoreLayout = false;
            isDraggable = false;
            isDirty = true;
        }

        public virtual void Initialize()
        {
        }

        public virtual void Update()
        {
            if (isDirty)
            {
                UpdateLayout();
                isDirty = false;
            }
        }

        public virtual void UpdateLayout()
        {
            if (nativeElement != null)
            {
                Canvas.SetLeft(nativeElement, position.X);
                Canvas.SetTop(nativeElement, position.Y);
                nativeElement.Width = width;
                nativeElement.Height = height;
            }
        }

        public virtual void Refresh()
        {
            UpdateLayout();
        }

        public virtual void OnClick()
        {
            Click?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnDoubleClick()
        {
            DoubleClick?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnHoverBegin()
        {
            HoverBegin?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnHoverEnd()
        {
            HoverEnd?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnPress()
        {
            Press?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnRelease()
        {
            Release?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnFocusGained()
        {
            FocusGained?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnFocusLost()
        {
            FocusLost?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnValueChanged()
        {
            ValueChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnVisibilityChanged()
        {
            if (nativeElement != null)
            {
                nativeElement.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            VisibilityChanged?.Invoke(this, EventArgs.Empty);
        }

        protected virtual void OnEnabledChanged()
        {
        }

        protected virtual void OnBackgroundColorChanged()
        {
        }

        protected virtual void OnTextColorChanged()
        {
        }

        protected virtual void OnOpacityChanged()
        {
            if (nativeElement != null)
            {
                nativeElement.Opacity = opacity;
            }
        }

        protected virtual void OnRotationChanged()
        {
        }

        protected virtual void OnZIndexChanged()
        {
            if (nativeElement != null)
            {
                System.Windows.Controls.Panel.SetZIndex(nativeElement, zIndex);
            }
        }

        public void ApplyTheme(Theme theme)
        {
            if (theme == null)
                return;

            if (theme.DefaultBackgroundColor.HasValue)
                backgroundColor = theme.DefaultBackgroundColor.Value;
            if (theme.DefaultTextColor.HasValue)
                textColor = theme.DefaultTextColor.Value;
            if (theme.DefaultFont != null)
            {
            }
        }

        public void AddChild(UIElement child)
        {
            if (child != null && !children.Contains(child))
            {
                children.Add(child);
                child.parent = this;
            }
        }

        public void RemoveChild(UIElement child)
        {
            if (child != null && children.Remove(child))
            {
                child.parent = null;
            }
        }

        public virtual Rect GetBounds()
        {
            return new Rect(position.X, position.Y, width, height);
        }

        public virtual bool ContainsPoint(Vector2 point)
        {
            return point.X >= position.X && point.X <= position.X + width &&
                   point.Y >= position.Y && point.Y <= position.Y + height;
        }

        public void SetAnchor(UIAnchor anchorMode)
        {
            switch (anchorMode)
            {
                case UIAnchor.TopLeft:
                    anchor = new Vector2(0, 0);
                    break;
                case UIAnchor.TopCenter:
                    anchor = new Vector2(0.5f, 0);
                    break;
                case UIAnchor.TopRight:
                    anchor = new Vector2(1, 0);
                    break;
                case UIAnchor.MiddleLeft:
                    anchor = new Vector2(0, 0.5f);
                    break;
                case UIAnchor.MiddleCenter:
                    anchor = new Vector2(0.5f, 0.5f);
                    break;
                case UIAnchor.MiddleRight:
                    anchor = new Vector2(1, 0.5f);
                    break;
                case UIAnchor.BottomLeft:
                    anchor = new Vector2(0, 1);
                    break;
                case UIAnchor.BottomCenter:
                    anchor = new Vector2(0.5f, 1);
                    break;
                case UIAnchor.BottomRight:
                    anchor = new Vector2(1, 1);
                    break;
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}({name}, Pos={Position}, Size={Size})";
        }
    }

    public class UIButton : UIElement
    {
        private string text;
        private UIFont font;
        private Thickness padding;
        private Thickness margin;
        private TextBlock textBlock;
        private Border border;
        private Color normalColor;
        private Color hoverColor;
        private Color pressedColor;
        private Color disabledColor;
        private Color normalTextColor;
        private Color hoverTextColor;
        private Color pressedTextColor;
        private Color disabledTextColor;
        private bool useCustomColors;
        private ImageSource icon;
        private Vector2 iconOffset;
        private float iconSpacing;
        private IconPosition iconPosition;

        public string Text { get => text; set { text = value; UpdateText(); } }
        public UIFont Font { get => font; set { font = value; UpdateFont(); } }
        public Thickness Padding { get => padding; set { padding = value; UpdatePadding(); } }
        public Thickness Margin { get => margin; set { margin = value; UpdateMargin(); } }
        public Color NormalColor { get => normalColor; set { normalColor = value; useCustomColors = true; } }
        public Color HoverColor { get => hoverColor; set { hoverColor = value; useCustomColors = true; } }
        public Color PressedColor { get => pressedColor; set { pressedColor = value; useCustomColors = true; } }
        public Color DisabledColor { get => disabledColor; set { disabledColor = value; useCustomColors = true; } }
        public Color NormalTextColor { get => normalTextColor; set { normalTextColor = value; useCustomColors = true; } }
        public Color HoverTextColor { get => hoverTextColor; set { hoverTextColor = value; useCustomColors = true; } }
        public Color PressedTextColor { get => pressedTextColor; set { pressedTextColor = value; useCustomColors = true; } }
        public Color DisabledTextColor { get => disabledTextColor; set { disabledTextColor = value; useCustomColors = true; } }
        public ImageSource Icon { get => icon; set { icon = value; UpdateIcon(); } }
        public Vector2 IconOffset { get => iconOffset; set { iconOffset = value; } }
        public float IconSpacing { get => iconSpacing; set { iconSpacing = value; } }
        public IconPosition IconPosition { get => iconPosition; set { iconPosition = value; } }

        public event EventHandler<EventArgs> MouseEnter;
        public event EventHandler<EventArgs> MouseLeave;
        public event EventHandler<EventArgs> MouseDown;
        public event EventHandler<EventArgs> MouseUp;

        public UIButton()
        {
            text = "Button";
            width = 120;
            height = 40;
            font = new UIFont { Family = "Segoe UI", Size = 14, FontWeight = FontWeights.Normal };
            padding = new Thickness(8);
            margin = new Thickness(0);
            normalColor = Color.FromRgb(68, 68, 68);
            hoverColor = Color.FromRgb(85, 85, 85);
            pressedColor = Color.FromRgb(51, 51, 51);
            disabledColor = Color.FromRgb(128, 128, 128);
            normalTextColor = Colors.White;
            hoverTextColor = Colors.White;
            pressedTextColor = Colors.White;
            disabledTextColor = Color.FromRgb(180, 180, 180);
            useCustomColors = false;
            iconSpacing = 8;
            iconPosition = IconPosition.Left;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            border = new Border
            {
                Background = new SolidColorBrush(normalColor),
                BorderBrush = new SolidColorBrush(Colors.Transparent),
                BorderThickness = new Thickness(0),
                CornerRadius = new CornerRadius(4),
                Padding = padding,
                Margin = margin
            };

            textBlock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(normalTextColor),
                FontFamily = new System.Windows.Media.FontFamily(font?.Family ?? "Segoe UI"),
                FontSize = font?.Size ?? 14,
                FontWeight = (System.Windows.FontWeight)(font?.FontWeight ?? FontWeights.Normal),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextAlignment = TextAlignment.Center
            };

            border.Child = textBlock;

            border.MouseEnter += OnNativeMouseEnter;
            border.MouseLeave += OnNativeMouseLeave;
            border.MouseLeftButtonDown += OnNativeMouseDown;
            border.MouseLeftButtonUp += OnNativeMouseUp;
            border.MouseLeftButtonDown += OnNativeClick;

            nativeElement = border;
        }

        private void OnNativeMouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isEnabled)
                return;

            if (useCustomColors)
            {
                border.Background = new SolidColorBrush(hoverColor);
                textBlock.Foreground = new SolidColorBrush(hoverTextColor);
            }
            MouseEnter?.Invoke(this, EventArgs.Empty);
        }

        private void OnNativeMouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!isEnabled)
                return;

            if (useCustomColors)
            {
                border.Background = new SolidColorBrush(normalColor);
                textBlock.Foreground = new SolidColorBrush(normalTextColor);
            }
            MouseLeave?.Invoke(this, EventArgs.Empty);
        }

        private void OnNativeMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isEnabled)
                return;

            if (useCustomColors)
            {
                border.Background = new SolidColorBrush(pressedColor);
                textBlock.Foreground = new SolidColorBrush(pressedTextColor);
            }
            MouseDown?.Invoke(this, EventArgs.Empty);
        }

        private void OnNativeMouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (!isEnabled)
                return;

            if (useCustomColors)
            {
                border.Background = new SolidColorBrush(hoverColor);
                textBlock.Foreground = new SolidColorBrush(hoverTextColor);
            }
            MouseUp?.Invoke(this, EventArgs.Empty);
        }

        private void OnNativeClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                OnDoubleClick();
            }
            else
            {
                OnClick();
            }
        }

        private void UpdateText()
        {
            if (textBlock != null)
            {
                textBlock.Text = text;
            }
        }

        private void UpdateFont()
        {
            if (textBlock != null && font != null)
            {
                textBlock.FontFamily = new System.Windows.Media.FontFamily(font.Family);
                textBlock.FontSize = font.Size;
                if (font.FontWeight.HasValue)
                    textBlock.FontWeight = (System.Windows.FontWeight)font.FontWeight.Value;
            }
        }

        private void UpdatePadding()
        {
            if (border != null)
            {
                border.Padding = padding;
            }
        }

        private void UpdateMargin()
        {
            if (border != null)
            {
                border.Margin = margin;
            }
        }

        private void UpdateIcon()
        {
        }

        protected override void OnEnabledChanged()
        {
            base.OnEnabledChanged();
            if (border != null)
            {
                border.IsEnabled = isEnabled;
                if (!isEnabled && useCustomColors)
                {
                    border.Background = new SolidColorBrush(disabledColor);
                    textBlock.Foreground = new SolidColorBrush(disabledTextColor);
                }
                else if (useCustomColors)
                {
                    border.Background = new SolidColorBrush(normalColor);
                    textBlock.Foreground = new SolidColorBrush(normalTextColor);
                }
            }
        }

        protected override void OnBackgroundColorChanged()
        {
            if (border != null && !useCustomColors)
            {
                border.Background = new SolidColorBrush(backgroundColor);
            }
        }

        protected override void OnTextColorChanged()
        {
            if (textBlock != null && !useCustomColors)
            {
                textBlock.Foreground = new SolidColorBrush(textColor);
            }
        }
    }

    public enum IconPosition
    {
        Left,
        Right,
        Top,
        Bottom
    }

    public class UILabel : UIElement
    {
        private string text;
        private UIFont font;
        private TextAlignment textAlignment;
        private VerticalAlignment verticalTextAlignment;
        private TextBlock textBlock;
        private bool wordWrap;
        private int lineLimit;
        private bool overflowHidden;
        private float lineSpacing;

        public string Text { get => text; set { text = value; UpdateText(); } }
        public UIFont Font { get => font; set { font = value; UpdateFont(); } }
        public TextAlignment TextAlignment { get => textAlignment; set { textAlignment = value; UpdateAlignment(); } }
        public VerticalAlignment VerticalTextAlignment { get => verticalTextAlignment; set { verticalTextAlignment = value; UpdateAlignment(); } }
        public bool WordWrap { get => wordWrap; set { wordWrap = value; UpdateWordWrap(); } }
        public int LineLimit { get => lineLimit; set { lineLimit = value; } }
        public bool OverflowHidden { get => overflowHidden; set { overflowHidden = value; } }
        public float LineSpacing { get => lineSpacing; set { lineSpacing = value; UpdateLineSpacing(); } }

        public UILabel()
        {
            text = "Label";
            width = 200;
            height = 30;
            font = new UIFont { Family = "Segoe UI", Size = 14 };
            textAlignment = TextAlignment.Left;
            verticalTextAlignment = VerticalAlignment.Center;
            wordWrap = false;
            lineLimit = 0;
            overflowHidden = false;
            lineSpacing = 1f;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            textBlock = new TextBlock
            {
                Text = text,
                Foreground = new SolidColorBrush(textColor),
                FontFamily = new System.Windows.Media.FontFamily(font?.Family ?? "Segoe UI"),
                FontSize = font?.Size ?? 14,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = verticalTextAlignment,
                TextWrapping = wordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap
            };

            nativeElement = textBlock;
        }

        private void UpdateText()
        {
            if (textBlock != null)
            {
                textBlock.Text = text;
            }
        }

        private void UpdateFont()
        {
            if (textBlock != null && font != null)
            {
                textBlock.FontFamily = new System.Windows.Media.FontFamily(font.Family);
                textBlock.FontSize = font.Size;
                if (font.FontWeight.HasValue)
                    textBlock.FontWeight = (System.Windows.FontWeight)font.FontWeight.Value;
            }
        }

        private void UpdateAlignment()
        {
            if (textBlock != null)
            {
                textBlock.TextAlignment = textAlignment;
                textBlock.VerticalAlignment = verticalTextAlignment;
            }
        }

        private void UpdateWordWrap()
        {
            if (textBlock != null)
            {
                textBlock.TextWrapping = wordWrap ? TextWrapping.Wrap : TextWrapping.NoWrap;
            }
        }

        private void UpdateLineSpacing()
        {
        }

        protected override void OnTextColorChanged()
        {
            if (textBlock != null)
            {
                textBlock.Foreground = new SolidColorBrush(textColor);
            }
        }
    }

    public class UIPanel : UIElement
    {
        private Border border;
        private StackPanel stackPanel;
        private bool hasBorder;
        private Thickness borderThickness;
        private Color borderColor;
        private CornerRadius cornerRadius;
        private bool clipContent;
        private ShadowData shadow;

        public bool HasBorder { get => hasBorder; set { hasBorder = value; UpdateBorder(); } }
        public Thickness BorderThickness { get => borderThickness; set { borderThickness = value; UpdateBorder(); } }
        public Color BorderColor { get => borderColor; set { borderColor = value; UpdateBorder(); } }
        public CornerRadius CornerRadius { get => cornerRadius; set { cornerRadius = value; UpdateBorder(); } }
        public bool ClipContent { get => clipContent; set { clipContent = value; UpdateClip(); } }
        public ShadowData Shadow { get => shadow; set { shadow = value; UpdateShadow(); } }

        public UIPanel()
        {
            width = 300;
            height = 200;
            backgroundColor = Color.FromArgb(200, 30, 30, 30);
            hasBorder = false;
            borderThickness = new Thickness(1);
            borderColor = Colors.Gray;
            cornerRadius = new CornerRadius(0);
            clipContent = false;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            border = new Border
            {
                Background = new SolidColorBrush(backgroundColor),
                BorderBrush = hasBorder ? new SolidColorBrush(borderColor) : null,
                BorderThickness = hasBorder ? borderThickness : new Thickness(0),
                CornerRadius = cornerRadius,
                ClipToBounds = clipContent
            };

            stackPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            border.Child = stackPanel;
            nativeElement = border;
        }

        private void UpdateBorder()
        {
            if (border != null)
            {
                border.BorderBrush = hasBorder ? new SolidColorBrush(borderColor) : null;
                border.BorderThickness = hasBorder ? borderThickness : new Thickness(0);
                border.CornerRadius = cornerRadius;
            }
        }

        private void UpdateClip()
        {
            if (border != null)
            {
                border.ClipToBounds = clipContent;
            }
        }

        private void UpdateShadow()
        {
        }

        protected override void OnBackgroundColorChanged()
        {
            if (border != null)
            {
                border.Background = new SolidColorBrush(backgroundColor);
            }
        }
    }

    public class UITextField : UIElement
    {
        private string text;
        private string placeholder;
        private UIFont font;
        private TextBox textBox;
        private int maxLength;
        private bool isReadOnly;
        private bool isPassword;
        private char passwordChar;
        private bool multiline;
        private int lines;
        private string validationRegex;
        private bool showValidation;
        private Color validationErrorColor;

        public string Text { get => text; set { text = value; UpdateText(); } }
        public string Placeholder { get => placeholder; set { placeholder = value; UpdatePlaceholder(); } }
        public UIFont Font { get => font; set { font = value; UpdateFont(); } }
        public int MaxLength { get => maxLength; set { maxLength = value; UpdateMaxLength(); } }
        public bool IsReadOnly { get => isReadOnly; set { isReadOnly = value; UpdateReadOnly(); } }
        public bool IsPassword { get => isPassword; set { isPassword = value; UpdatePasswordMode(); } }
        public char PasswordChar { get => passwordChar; set { passwordChar = value; } }
        public bool Multiline { get => multiline; set { multiline = value; UpdateMultiline(); } }
        public int Lines { get => lines; set { lines = value; } }
        public string ValidationRegex { get => validationRegex; set { validationRegex = value; } }
        public bool ShowValidation { get => showValidation; set { showValidation = value; } }
        public Color ValidationErrorColor { get => validationErrorColor; set { validationErrorColor = value; } }

        public UITextField()
        {
            text = "";
            placeholder = "Enter text...";
            font = new UIFont { Family = "Segoe UI", Size = 14 };
            width = 200;
            height = 30;
            maxLength = 0;
            isReadOnly = false;
            isPassword = false;
            passwordChar = '*';
            multiline = false;
            lines = 1;
            validationRegex = "";
            showValidation = false;
            validationErrorColor = Colors.Red;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            textBox = new TextBox
            {
                Text = text,
                FontFamily = new System.Windows.Media.FontFamily(font?.Family ?? "Segoe UI"),
                FontSize = font?.Size ?? 14,
                Background = new SolidColorBrush(Colors.White),
                Foreground = new SolidColorBrush(textColor),
                BorderBrush = new SolidColorBrush(Colors.Gray),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(4),
                VerticalContentAlignment = VerticalAlignment.Center
            };

            textBox.TextChanged += OnTextChanged;
            nativeElement = textBox;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            text = textBox.Text;
            OnValueChanged();
        }

        private void UpdateText()
        {
            if (textBox != null && textBox.Text != text)
            {
                textBox.Text = text;
            }
        }

        private void UpdatePlaceholder()
        {
        }

        private void UpdateFont()
        {
            if (textBox != null && font != null)
            {
                textBox.FontFamily = new System.Windows.Media.FontFamily(font.Family);
                textBox.FontSize = font.Size;
            }
        }

        private void UpdateMaxLength()
        {
            if (textBox != null && maxLength > 0)
            {
                textBox.MaxLength = maxLength;
            }
        }

        private void UpdateReadOnly()
        {
            if (textBox != null)
            {
                textBox.IsReadOnly = isReadOnly;
            }
        }

        private void UpdatePasswordMode()
        {
            if (textBox != null && isPassword)
            {
                textBox.FontFamily = new System.Windows.Media.FontFamily("*");
            }
        }

        private void UpdateMultiline()
        {
            if (textBox != null)
            {
                if (multiline)
                {
                    textBox.AcceptsReturn = true;
                    textBox.TextWrapping = TextWrapping.Wrap;
                    textBox.MinHeight = lines * 20;
                }
                else
                {
                    textBox.AcceptsReturn = false;
                    textBox.TextWrapping = TextWrapping.NoWrap;
                }
            }
        }

        protected override void OnTextColorChanged()
        {
            if (textBox != null)
            {
                textBox.Foreground = new SolidColorBrush(textColor);
            }
        }
    }

    public class UIImage : UIElement
    {
        private ImageSource imageSource;
        private Image image;
        private Stretch stretch;
        private HorizontalAlignment horizontalImageAlignment;
        private VerticalAlignment verticalImageAlignment;
        private Color tintColor;
        private bool useTint;

        public ImageSource ImageSource { get => imageSource; set { imageSource = value; UpdateImage(); } }
        public Stretch Stretch { get => stretch; set { stretch = value; UpdateStretch(); } }
        public HorizontalAlignment HorizontalImageAlignment { get => horizontalImageAlignment; set { horizontalImageAlignment = value; } }
        public VerticalAlignment VerticalImageAlignment { get => verticalImageAlignment; set { verticalImageAlignment = value; } }
        public Color TintColor { get => tintColor; set { tintColor = value; useTint = true; UpdateTint(); } }
        public bool UseTint { get => useTint; set { useTint = value; UpdateTint(); } }

        public UIImage()
        {
            width = 100;
            height = 100;
            stretch = Stretch.Uniform;
            tintColor = Colors.White;
            useTint = false;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            image = new Image
            {
                Stretch = stretch,
                HorizontalAlignment = horizontalImageAlignment,
                VerticalAlignment = verticalImageAlignment
            };

            nativeElement = image;
        }

        private void UpdateImage()
        {
            if (image != null && imageSource != null)
            {
                image.Source = imageSource;
            }
        }

        private void UpdateStretch()
        {
            if (image != null)
            {
                image.Stretch = stretch;
            }
        }

        private void UpdateTint()
        {
            if (image != null && useTint && imageSource != null)
            {
                var tinted = imageSource.Clone();
                tinted.Freeze();
            }
        }
    }

    public class UIToggle : UIElement
    {
        private bool isOn;
        private string labelText;
        private UIButton toggleButton;
        private UILabel label;
        private StackPanel container;
        private Color onColor;
        private Color offColor;
        private Color thumbColor;

        public bool IsOn { get => isOn; set { isOn = value; UpdateToggle(); } }
        public string LabelText { get => labelText; set { labelText = value; UpdateLabel(); } }
        public Color OnColor { get => onColor; set { onColor = value; UpdateColors(); } }
        public Color OffColor { get => offColor; set { offColor = value; UpdateColors(); } }
        public Color ThumbColor { get => thumbColor; set { thumbColor = value; UpdateColors(); } }

        public event EventHandler<bool> ValueChanged;

        public UIToggle()
        {
            isOn = false;
            labelText = "Toggle";
            width = 150;
            height = 30;
            onColor = Color.FromRgb(76, 175, 80);
            offColor = Color.FromRgb(158, 158, 158);
            thumbColor = Colors.White;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            container = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            toggleButton = new UIButton
            {
                Name = "ToggleButton",
                Width = 50,
                Height = 26
            };
            toggleButton.Click += OnToggleClick;

            label = new UILabel
            {
                Name = "ToggleLabel",
                Text = labelText,
                Width = 100,
                Height = 30
            };

            container.Children.Add(toggleButton.NativeElement);
            container.Children.Add(label.NativeElement);

            nativeElement = container;
            UpdateColors();
        }

        private void OnToggleClick(object sender, EventArgs e)
        {
            IsOn = !IsOn;
            ValueChanged?.Invoke(this, isOn);
        }

        private void UpdateToggle()
        {
            UpdateColors();
        }

        private void UpdateLabel()
        {
            if (label != null)
            {
                label.Text = labelText;
            }
        }

        private void UpdateColors()
        {
            if (toggleButton != null)
            {
                toggleButton.NormalColor = isOn ? onColor : offColor;
            }
        }
    }

    public class UISlider : UIElement
    {
        private float value;
        private float minValue;
        private float maxValue;
        private float step;
        private Slider slider;
        private UILabel valueLabel;
        private bool showValue;
        private string valueFormat;
        private StackPanel container;

        public float Value { get => value; set { this.value = Math.Max(minValue, Math.Min(maxValue, value)); UpdateSlider(); OnValueChanged(); } }
        public float MinValue { get => minValue; set { minValue = value; UpdateSlider(); } }
        public float MaxValue { get => maxValue; set { maxValue = value; UpdateSlider(); } }
        public float Step { get => step; set { step = value; UpdateSlider(); } }
        public bool ShowValue { get => showValue; set { showValue = value; UpdateValueLabel(); } }
        public string ValueFormat { get => valueFormat; set { valueFormat = value; UpdateValueLabel(); } }

        public event EventHandler<float> ValueChanged;

        public UISlider()
        {
            value = 0.5f;
            minValue = 0f;
            maxValue = 1f;
            step = 0f;
            width = 200;
            height = 30;
            showValue = true;
            valueFormat = "F2";

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            container = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center
            };

            slider = new Slider
            {
                Minimum = minValue,
                Maximum = maxValue,
                Value = value,
                Width = 150
            };

            slider.ValueChanged += OnSliderValueChanged;

            valueLabel = new UILabel
            {
                Name = "ValueLabel",
                Text = value.ToString(valueFormat),
                Width = 50,
                Height = 30
            };

            container.Children.Add(slider);
            container.Children.Add(valueLabel.NativeElement);

            nativeElement = container;
        }

        private void OnSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Value = (float)e.NewValue;
        }

        private void UpdateSlider()
        {
            if (slider != null)
            {
                slider.Minimum = minValue;
                slider.Maximum = maxValue;
                slider.Value = value;
            }
            UpdateValueLabel();
        }

        private void UpdateValueLabel()
        {
            if (valueLabel != null && showValue)
            {
                valueLabel.Text = value.ToString(valueFormat);
                valueLabel.IsVisible = showValue;
            }
        }
    }

    public class UIProgressBar : UIElement
    {
        private float value;
        private float minValue;
        private float maxValue;
        private ProgressBar progressBar;
        private UILabel label;
        private bool showLabel;
        private string labelFormat;
        private Color fillColor;
        private Color backgroundColorFill;

        public float Value { get => value; set { this.value = Math.Max(minValue, Math.Min(maxValue, value)); UpdateProgress(); } }
        public float MinValue { get => minValue; set { minValue = value; UpdateProgress(); } }
        public float MaxValue { get => maxValue; set { maxValue = value; UpdateProgress(); } }
        public bool ShowLabel { get => showLabel; set { showLabel = value; UpdateLabel(); } }
        public string LabelFormat { get => labelFormat; set { labelFormat = value; UpdateLabel(); } }
        public Color FillColor { get => fillColor; set { fillColor = value; UpdateColors(); } }
        public Color BackgroundColorFill { get => backgroundColorFill; set { backgroundColorFill = value; UpdateColors(); } }

        public UIProgressBar()
        {
            value = 0.5f;
            minValue = 0f;
            maxValue = 1f;
            width = 200;
            height = 20;
            showLabel = false;
            labelFormat = "P0";
            fillColor = Color.FromRgb(76, 175, 80);
            backgroundColorFill = Color.FromRgb(224, 224, 224);

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            progressBar = new ProgressBar
            {
                Minimum = minValue,
                Maximum = maxValue,
                Value = value,
                Width = 200,
                Height = 20
            };

            nativeElement = progressBar;
            UpdateColors();
        }

        private void UpdateProgress()
        {
            if (progressBar != null)
            {
                progressBar.Minimum = minValue;
                progressBar.Maximum = maxValue;
                progressBar.Value = value;
            }
            UpdateLabel();
        }

        private void UpdateLabel()
        {
        }

        private void UpdateColors()
        {
            if (progressBar != null)
            {
                progressBar.Foreground = new SolidColorBrush(fillColor);
                progressBar.Background = new SolidColorBrush(backgroundColorFill);
            }
        }
    }

    public class UIScrollView : UIElement
    {
        private ScrollViewer scrollViewer;
        private StackPanel contentPanel;
        private Vector2 scrollPosition;
        private Vector2 contentSize;
        private bool horizontalScrollEnabled;
        private bool verticalScrollEnabled;
        private float horizontalScrollSpeed;
        private float verticalScrollSpeed;
        private ScrollBarVisibility horizontalScrollBarVisibility;
        private ScrollBarVisibility verticalScrollBarVisibility;

        public Vector2 ScrollPosition { get => scrollPosition; set { scrollPosition = value; UpdateScrollPosition(); } }
        public Vector2 ContentSize { get => contentSize; set { contentSize = value; UpdateContentSize(); } }
        public bool HorizontalScrollEnabled { get => horizontalScrollEnabled; set { horizontalScrollEnabled = value; UpdateScrollBars(); } }
        public bool VerticalScrollEnabled { get => verticalScrollEnabled; set { verticalScrollEnabled = value; UpdateScrollBars(); } }
        public ScrollBarVisibility HorizontalScrollBarVisibility { get => horizontalScrollBarVisibility; set { horizontalScrollBarVisibility = value; UpdateScrollBars(); } }
        public ScrollBarVisibility VerticalScrollBarVisibility { get => verticalScrollBarVisibility; set { verticalScrollBarVisibility = value; UpdateScrollBars(); } }

        public UIScrollView()
        {
            width = 300;
            height = 200;
            horizontalScrollEnabled = false;
            verticalScrollEnabled = true;
            horizontalScrollSpeed = 1f;
            verticalScrollSpeed = 1f;
            horizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            verticalScrollBarVisibility = ScrollBarVisibility.Auto;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            contentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical
            };

            scrollViewer = new ScrollViewer
            {
                Content = contentPanel,
                HorizontalScrollBarVisibility = horizontalScrollBarVisibility,
                VerticalScrollBarVisibility = verticalScrollBarVisibility,
                Width = width,
                Height = height
            };

            nativeElement = scrollViewer;
        }

        private void UpdateScrollPosition()
        {
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToHorizontalOffset(scrollPosition.X);
                scrollViewer.ScrollToVerticalOffset(scrollPosition.Y);
            }
        }

        private void UpdateContentSize()
        {
        }

        private void UpdateScrollBars()
        {
            if (scrollViewer != null)
            {
                scrollViewer.HorizontalScrollBarVisibility = horizontalScrollBarVisibility;
                scrollViewer.VerticalScrollBarVisibility = verticalScrollBarVisibility;
            }
        }

        public void AddContent(UIElement element)
        {
            if (contentPanel != null && element?.NativeElement != null)
            {
                contentPanel.Children.Add(element.NativeElement);
            }
        }

        public void RemoveContent(UIElement element)
        {
            if (contentPanel != null && element?.NativeElement != null)
            {
                contentPanel.Children.Remove(element.NativeElement);
            }
        }
    }

    public class UIDropdown : UIElement
    {
        private ComboBox comboBox;
        private List<string> options;
        private int selectedIndex;
        private string selectedOption;
        private UILabel placeholder;
        private bool isExpanded;

        public List<string> Options { get => options; set { options = value; UpdateOptions(); } }
        public int SelectedIndex { get => selectedIndex; set { selectedIndex = value; UpdateSelection(); } }
        public string SelectedOption { get => selectedOption; set { selectedOption = value; UpdateSelection(); } }
        public bool IsExpanded { get => isExpanded; set { isExpanded = value; UpdateExpanded(); } }

        public event EventHandler<int> SelectionChanged;

        public UIDropdown()
        {
            width = 200;
            height = 30;
            options = new List<string>();
            selectedIndex = -1;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            comboBox = new ComboBox
            {
                Width = width,
                Height = height
            };

            comboBox.SelectionChanged += OnSelectionChanged;
            nativeElement = comboBox;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (comboBox.SelectedIndex >= 0)
            {
                selectedIndex = comboBox.SelectedIndex;
                selectedOption = options[selectedIndex];
                SelectionChanged?.Invoke(this, selectedIndex);
            }
        }

        private void UpdateOptions()
        {
            if (comboBox != null && options != null)
            {
                comboBox.ItemsSource = options;
            }
        }

        private void UpdateSelection()
        {
            if (comboBox != null && selectedIndex >= 0 && selectedIndex < options.Count)
            {
                comboBox.SelectedIndex = selectedIndex;
            }
        }

        private void UpdateExpanded()
        {
            if (comboBox != null)
            {
                comboBox.IsDropDownOpen = isExpanded;
            }
        }

        public void AddOption(string option)
        {
            options.Add(option);
            UpdateOptions();
        }

        public void RemoveOption(string option)
        {
            options.Remove(option);
            UpdateOptions();
        }

        public void ClearOptions()
        {
            options.Clear();
            UpdateOptions();
        }
    }

    public class UIListView : UIElement
    {
        private ListBox listBox;
        private List<ListItem> items;
        private int selectedIndex;
        private object selectedItem;
        private List<ListItem> selectedItems;
        private bool allowMultipleSelection;
        private bool showScrollbar;
        private ScrollBarVisibility scrollBarVisibility;

        public List<ListItem> Items { get => items; set { items = value; UpdateItems(); } }
        public int SelectedIndex { get => selectedIndex; set { selectedIndex = value; UpdateSelection(); } }
        public object SelectedItem { get => selectedItem; set { selectedItem = value; UpdateSelection(); } }
        public List<ListItem> SelectedItems => selectedItems;
        public bool AllowMultipleSelection { get => allowMultipleSelection; set { allowMultipleSelection = value; UpdateSelectionMode(); } }
        public ScrollBarVisibility ScrollBarVisibility { get => scrollBarVisibility; set { scrollBarVisibility = value; UpdateScrollBar(); } }

        public event EventHandler<int> SelectionChanged;

        public UIListView()
        {
            width = 200;
            height = 200;
            items = new List<ListItem>();
            selectedIndex = -1;
            selectedItems = new List<ListItem>();
            allowMultipleSelection = false;
            scrollBarVisibility = ScrollBarVisibility.Auto;

            InitializeNativeElement();
        }

        private void InitializeNativeElement()
        {
            listBox = new ListBox
            {
                Width = width,
                Height = height,
                SelectionMode = allowMultipleSelection ? SelectionMode.Multiple : SelectionMode.Single
            };

            listBox.SelectionChanged += OnSelectionChanged;
            nativeElement = listBox;
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listBox.SelectedIndex >= 0)
            {
                selectedIndex = listBox.SelectedIndex;
                selectedItem = listBox.SelectedItem;
                SelectionChanged?.Invoke(this, selectedIndex);
            }
        }

        private void UpdateItems()
        {
            if (listBox != null && items != null)
            {
                listBox.ItemsSource = items;
            }
        }

        private void UpdateSelection()
        {
            if (listBox != null && selectedIndex >= 0)
            {
                listBox.SelectedIndex = selectedIndex;
            }
        }

        private void UpdateSelectionMode()
        {
            if (listBox != null)
            {
                listBox.SelectionMode = allowMultipleSelection ? SelectionMode.Multiple : SelectionMode.Single;
            }
        }

        private void UpdateScrollBar()
        {
            if (listBox != null)
            {
                ScrollViewer.SetScrollBarVisibility(listBox, scrollBarVisibility);
            }
        }

        public void AddItem(ListItem item)
        {
            items.Add(item);
            UpdateItems();
        }

        public void RemoveItem(ListItem item)
        {
            items.Remove(item);
            UpdateItems();
        }

        public void ClearItems()
        {
            items.Clear();
            UpdateItems();
        }
    }

    public class ListItem
    {
        public string Text { get; set; }
        public object Value { get; set; }
        public bool IsSelected { get; set; }
        public Color BackgroundColor { get; set; }
        public Color TextColor { get; set; }

        public ListItem()
        {
            Text = "";
            IsSelected = false;
            BackgroundColor = Colors.Transparent;
            TextColor = Colors.Black;
        }

        public ListItem(string text, object value = null)
        {
            Text = text;
            Value = value;
            IsSelected = false;
            BackgroundColor = Colors.Transparent;
            TextColor = Colors.Black;
        }

        public override string ToString() => Text;
    }

    public class UIFont
    {
        public string Family { get; set; }
        public float Size { get; set; }
        public FontWeight? FontWeight { get; set; }
        public FontStyle? FontStyle { get; set; }
        public TextDecorationCollection TextDecorations { get; set; }

        public UIFont()
        {
            Family = "Segoe UI";
            Size = 14;
        }
    }

    public class UILayoutElement
    {
        public float MinWidth { get; set; }
        public float MinHeight { get; set; }
        public float PreferredWidth { get; set; }
        public float PreferredHeight { get; set; }
        public float FlexibleWidth { get; set; }
        public float FlexibleHeight { get; set; }
        public int LayoutPriority { get; set; }

        public UILayoutElement()
        {
            MinWidth = 0;
            MinHeight = 0;
            PreferredWidth = -1;
            PreferredHeight = -1;
            FlexibleWidth = -1;
            FlexibleHeight = -1;
            LayoutPriority = 0;
        }
    }

    public class UILayoutController
    {
        public bool IsActive { get; set; }
        public UILayoutType LayoutType { get; set; }

        public UILayoutController()
        {
            IsActive = true;
            LayoutType = UILayoutType.None;
        }
    }

    public enum UILayoutType
    {
        None,
        Horizontal,
        Vertical,
        Grid
    }

    public class UIHorizontalLayout : UILayoutController
    {
        public float Spacing { get; set; }
        public Thickness Padding { get; set; }
        public UIChildAlignment ChildAlignment { get; set; }
        public bool ReverseArrangement { get; set; }

        public UIHorizontalLayout()
        {
            LayoutType = UILayoutType.Horizontal;
            Spacing = 8;
            Padding = new Thickness(0);
            ChildAlignment = UIChildAlignment.MiddleLeft;
            ReverseArrangement = false;
        }
    }

    public class UIVerticalLayout : UILayoutController
    {
        public float Spacing { get; set; }
        public Thickness Padding { get; set; }
        public UIChildAlignment ChildAlignment { get; set; }
        public bool ReverseArrangement { get; set; }

        public UIVerticalLayout()
        {
            LayoutType = UILayoutType.Vertical;
            Spacing = 8;
            Padding = new Thickness(0);
            ChildAlignment = UIChildAlignment.TopCenter;
            ReverseArrangement = false;
        }
    }

    public class UIGridLayoutController : UILayoutController
    {
        public int Columns { get; set; }
        public int Rows { get; set; }
        public Vector2 CellSize { get; set; }
        public Vector2 Spacing { get; set; }
        public Vector2 StartCorner { get; set; }
        public UILayoutStartAxis StartAxis { get; set; }
        public bool ConstraintEnabled { get; set; }

        public UIGridLayoutController()
        {
            LayoutType = UILayoutType.Grid;
            Columns = 4;
            Rows = 4;
            CellSize = new Vector2(100, 50);
            Spacing = new Vector2(8, 8);
            StartCorner = new Vector2(0, 0);
            StartAxis = UILayoutStartAxis.HorizontalFirst;
            ConstraintEnabled = false;
        }
    }

    public enum UIChildAlignment
    {
        TopLeft,
        TopCenter,
        TopRight,
        MiddleLeft,
        MiddleCenter,
        MiddleRight,
        BottomLeft,
        BottomCenter,
        BottomRight
    }

    public enum UILayoutStartAxis
    {
        HorizontalFirst,
        VerticalFirst
    }

    public class Theme
    {
        public string Name { get; set; }
        public Color? DefaultBackgroundColor { get; set; }
        public Color? DefaultTextColor { get; set; }
        public Color? PrimaryColor { get; set; }
        public Color? SecondaryColor { get; set; }
        public Color? AccentColor { get; set; }
        public Color? ErrorColor { get; set; }
        public Color? WarningColor { get; set; }
        public Color? SuccessColor { get; set; }
        public UIFont DefaultFont { get; set; }
        public float DefaultPadding { get; set; }
        public float DefaultSpacing { get; set; }
        public CornerRadius DefaultCornerRadius { get; set; }
        public ShadowData DefaultShadow { get; set; }

        public Theme()
        {
            Name = "Default";
            DefaultPadding = 8;
            DefaultSpacing = 8;
        }

        public static Theme DarkTheme => new Theme
        {
            Name = "Dark",
            DefaultBackgroundColor = Color.FromRgb(30, 30, 30),
            DefaultTextColor = Colors.White,
            PrimaryColor = Color.FromRgb(33, 150, 243),
            SecondaryColor = Color.FromRgb(97, 97, 97),
            AccentColor = Color.FromRgb(3, 169, 244),
            DefaultFont = new UIFont { Family = "Segoe UI", Size = 14 }
        };

        public static Theme LightTheme => new Theme
        {
            Name = "Light",
            DefaultBackgroundColor = Colors.White,
            DefaultTextColor = Colors.Black,
            PrimaryColor = Color.FromRgb(33, 150, 243),
            SecondaryColor = Color.FromRgb(224, 224, 224),
            AccentColor = Color.FromRgb(3, 169, 244),
            DefaultFont = new UIFont { Family = "Segoe UI", Size = 14 }
        };
    }

    public class ShadowData
    {
        public Color Color { get; set; }
        public Vector2 Offset { get; set; }
        public float BlurRadius { get; set; }
        public float SpreadRadius { get; set; }
        public bool IsEnabled { get; set; }

        public ShadowData()
        {
            Color = Color.FromArgb(100, 0, 0, 0);
            Offset = new Vector2(2, 2);
            BlurRadius = 4;
            SpreadRadius = 0;
            IsEnabled = true;
        }
    }

    public class Style
    {
        public string Name { get; set; }
        public Color? BackgroundColor { get; set; }
        public Color? TextColor { get; set; }
        public UIFont Font { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public Thickness? Margin { get; set; }
        public Thickness? Padding { get; set; }
        public CornerRadius? CornerRadius { get; set; }
        public Color? BorderColor { get; set; }
        public Thickness? BorderThickness { get; set; }

        public void Apply(UIElement element)
        {
            if (BackgroundColor.HasValue)
                element.BackgroundColor = BackgroundColor.Value;
            if (TextColor.HasValue)
                element.TextColor = TextColor.Value;
            if (Width.HasValue)
                element.Width = Width.Value;
            if (Height.HasValue)
                element.Height = Height.Value;
        }
    }

    public class UIEventArgs : EventArgs
    {
        public UIElement Element { get; }
        public string Message { get; }

        public UIEventArgs(UIElement element)
        {
            Element = element;
            Message = element?.Name;
        }

        public UIEventArgs(string message)
        {
            Message = message;
        }
    }

    public class UIEventHandler
    {
        public string Name { get; set; }
        public int Priority { get; set; }
        public bool IsEnabled { get; set; }

        public UIEventHandler()
        {
            IsEnabled = true;
            Priority = 0;
        }

        public virtual void OnElementClick(UIElement element) { }
        public virtual void OnElementHover(UIElement element) { }
        public virtual void OnElementPress(UIElement element) { }
    }

    public class Component
    {
        public GameObject GameObject { get; protected set; }
        public Transform Transform => GameObject?.Transform;
        public bool Enabled { get; set; } = true;

        public virtual void Initialize(GameObject gameObject)
        {
            GameObject = gameObject;
        }

        public virtual void Update(float deltaTime) { }
        public virtual void OnDestroy() { }

        public T GetComponent<T>() where T : Component => GameObject?.GetComponent<T>();
        public T[] GetComponents<T>() where T : Component => GameObject?.GetComponents<T>();
    }
}

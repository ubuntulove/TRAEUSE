using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OfficeSuite.Presentation.Views
{
    public class SlideEditorView : Canvas
    {
        private Presentation.Models.Slide _currentSlide;
        private Presentation.Models.SlideShape _selectedShape;
        private Point _dragStartPoint;
        private bool _isDragging;
        private bool _isResizing;
        private ResizeDirection _resizeDirection;
        private Point _originalPosition;
        private Size _originalSize;

        public SlideEditorView()
        {
            Background = Brushes.White;
            ClipToBounds = true;
            Focusable = true;
            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
            InvalidateVisual();
        }

        public Presentation.Models.Slide CurrentSlide
        {
            get => _currentSlide;
            set
            {
                _currentSlide = value;
                InvalidateVisual();
            }
        }

        public Presentation.Models.SlideShape SelectedShape
        {
            get => _selectedShape;
            set
            {
                _selectedShape = value;
                InvalidateVisual();
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            if (_currentSlide == null)
                return;

            DrawBackground(dc);
            DrawSlideBackground(dc);
            DrawShapes(dc);
            DrawSelection(dc);
            DrawGuides(dc);
        }

        private void DrawBackground(DrawingContext dc)
        {
            var darkBrush = new SolidColorBrush(Color.FromRgb(64, 64, 64));
            dc.DrawRectangle(darkBrush, null, new Rect(0, 0, ActualWidth, ActualHeight));

            var lightBrush = Brushes.White;
            var margin = 50.0;
            dc.DrawRectangle(lightBrush, null, new Rect(margin, margin, ActualWidth - 2 * margin, ActualHeight - 2 * margin));
        }

        private void DrawSlideBackground(DrawingContext dc)
        {
            if (_currentSlide?.Background == null)
                return;

            var bg = _currentSlide.Background;
            if (bg.Fill != null && bg.Fill.Type != Models.FillType.None)
            {
                var color = (Color)ColorConverter.ConvertFromString(bg.Fill.Color ?? "#FFFFFF");
                var brush = new SolidColorBrush(color);
                var slideRect = GetSlideRect();
                dc.DrawRectangle(brush, null, slideRect);
            }
        }

        private void DrawShapes(DrawingContext dc)
        {
            if (_currentSlide?.Shapes == null)
                return;

            foreach (var shape in _currentSlide.Shapes.OrderBy(s => s.ZIndex))
            {
                if (!shape.IsVisible)
                    continue;

                var rect = new Rect(shape.X, shape.Y, shape.Width, shape.Height);

                DrawShape(dc, shape, rect);

                if (shape.TextBox != null && !string.IsNullOrEmpty(shape.TextBox.Text))
                {
                    DrawText(dc, shape, rect);
                }
            }
        }

        private void DrawShape(DrawingContext dc, Presentation.Models.SlideShape shape, Rect rect)
        {
            if (shape.Fill != null && shape.Fill.Type != Models.FillType.None)
            {
                var brush = GetBrush(shape.Fill);
                dc.DrawRectangle(brush, null, rect);
            }

            if (shape.Line != null && shape.Line.IsVisible)
            {
                var pen = GetPen(shape.Line);
                dc.DrawRectangle(null, pen, rect);
            }

            if (shape.Effects != null && shape.Effects.HasShadow)
            {
                DrawShadow(dc, shape, rect);
            }
        }

        private void DrawShadow(DrawingContext dc, Presentation.Models.SlideShape shape, Rect rect)
        {
            var shadow = shape.Effects.Shadow;
            var shadowRect = new Rect(
                rect.X + shadow.Distance * Math.Cos(shadow.Angle * Math.PI / 180),
                rect.Y + shadow.Distance * Math.Sin(shadow.Angle * Math.PI / 180),
                rect.Width,
                rect.Height);

            var alpha = (byte)(255 * (1 - shadow.Transparency));
            var color = (Color)ColorConverter.ConvertFromString(shadow.Color ?? "#000000");
            color.A = alpha;
            var brush = new SolidColorBrush(color);
            var pen = new Pen(brush, shadow.BlurRadius);

            dc.DrawRectangle(brush, pen, shadowRect);
        }

        private void DrawText(DrawingContext dc, Presentation.Models.SlideShape shape, Rect rect)
        {
            var text = shape.TextBox.Text;
            if (string.IsNullOrEmpty(text))
                return;

            var typeface = new Typeface(
                shape.TextBox.DefaultFont?.FontName ?? "Calibri",
                shape.TextBox.DefaultFont?.IsItalic ?? false ? FontStyles.Italic : FontStyles.Normal,
                shape.TextBox.DefaultFont?.IsBold ?? false ? FontWeights.Bold : FontWeights.Normal,
                FontStretches.Normal);

            var fontSize = shape.TextBox.DefaultFont?.FontSize ?? 14;
            var color = (Color)ColorConverter.ConvertFromString(shape.TextBox.DefaultFont?.Color ?? "#000000");
            var brush = new SolidColorBrush(color);

            var ft = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            var textX = rect.X + shape.TextBox.InnerMarginLeft;
            var textY = rect.Y + shape.TextBox.InnerMarginTop;

            dc.DrawText(ft, new Point(textX, textY));
        }

        private void DrawSelection(DrawingContext dc)
        {
            if (_selectedShape == null)
                return;

            var rect = new Rect(_selectedShape.X, _selectedShape.Y, _selectedShape.Width, _selectedShape.Height);

            var pen = new Pen(Brushes.Blue, 1);
            pen.DashStyle = DashStyles.Dash;
            dc.DrawRectangle(null, pen, rect);

            var handleSize = 8;
            var handles = GetResizeHandles(rect);
            foreach (var handle in handles)
            {
                dc.DrawRectangle(Brushes.White, new Pen(Brushes.Blue, 1), new Rect(handle.X - handleSize / 2, handle.Y - handleSize / 2, handleSize, handleSize));
            }
        }

        private void DrawGuides(DrawingContext dc)
        {
            var pen = new Pen(Brushes.Red, 1);
            pen.DashStyle = DashStyles.Dash;

            var centerX = ActualWidth / 2;
            var centerY = ActualHeight / 2;

            dc.DrawLine(pen, new Point(centerX, 0), new Point(centerX, ActualHeight));
            dc.DrawLine(pen, new Point(0, centerY), new Point(ActualWidth, centerY));
        }

        private Rect GetSlideRect()
        {
            return new Rect(50, 50, ActualWidth - 100, ActualHeight - 100);
        }

        private Brush GetBrush(Models.FillFormat fill)
        {
            if (fill.Type == Models.FillType.Solid)
            {
                var color = (Color)ColorConverter.ConvertFromString(fill.Color ?? "#FFFFFF");
                return new SolidColorBrush(color);
            }
            else if (fill.Type == Models.FillType.Gradient && fill.Gradient != null)
            {
                var gradientStops = new GradientStops();
                foreach (var stop in fill.Gradient.Stops)
                {
                    var stopColor = (Color)ColorConverter.ConvertFromString(stop.Color ?? "#FFFFFF");
                    gradientStops.Add(new GradientStop(stopColor, stop.Position));
                }

                var gradientBrush = new LinearGradientBrush(gradientStops, fill.Gradient.Angle);
                return gradientBrush;
            }

            return Brushes.White;
        }

        private Pen GetPen(Models.LineFormat line)
        {
            var color = (Color)ColorConverter.ConvertFromString(line.Color ?? "#000000");
            return new Pen(new SolidColorBrush(color), line.Width)
            {
                DashStyle = GetDashStyle(line.DashStyle)
            };
        }

        private DashStyle GetDashStyle(Models.DashStyle style)
        {
            return style switch
            {
                Models.DashStyle.Dash => DashStyles.Dash,
                Models.DashStyle.DashDot => DashStyles.DashDot,
                Models.DashStyle.DashDotDot => DashStyles.DashDotDot,
                Models.DashStyle.Dot => DashStyles.Dot,
                _ => DashStyles.Solid
            };
        }

        private Point[] GetResizeHandles(Rect rect)
        {
            return new[]
            {
                new Point(rect.Left, rect.Top),
                new Point(rect.Left + rect.Width / 2, rect.Top),
                new Point(rect.Right, rect.Top),
                new Point(rect.Right, rect.Top + rect.Height / 2),
                new Point(rect.Right, rect.Bottom),
                new Point(rect.Left + rect.Width / 2, rect.Bottom),
                new Point(rect.Left, rect.Bottom),
                new Point(rect.Left, rect.Top + rect.Height / 2)
            };
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var pos = e.GetPosition(this);
            _dragStartPoint = pos;
            _originalPosition = pos;

            var clickedShape = GetShapeAtPoint(pos);
            if (clickedShape != null)
            {
                SelectedShape = clickedShape;
                _isDragging = true;
                CaptureMouse();
            }
            else
            {
                SelectedShape = null;
            }

            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e.LeftButton != MouseButtonState.Pressed)
                return;

            var pos = e.GetPosition(this);

            if (_isDragging && _selectedShape != null)
            {
                var deltaX = pos.X - _dragStartPoint.X;
                var deltaY = pos.Y - _dragStartPoint.Y;

                _selectedShape.X += deltaX;
                _selectedShape.Y += deltaY;

                _dragStartPoint = pos;
                InvalidateVisual();
            }
            else if (_isResizing && _selectedShape != null)
            {
                ResizeShape(pos);
                InvalidateVisual();
            }
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            _isDragging = false;
            _isResizing = false;
            ReleaseMouseCapture();
            InvalidateVisual();
        }

        private Presentation.Models.SlideShape GetShapeAtPoint(Point point)
        {
            if (_currentSlide?.Shapes == null)
                return null;

            for (int i = _currentSlide.Shapes.Count - 1; i >= 0; i--)
            {
                var shape = _currentSlide.Shapes[i];
                var rect = new Rect(shape.X, shape.Y, shape.Width, shape.Height);
                if (rect.Contains(point))
                    return shape;
            }

            return null;
        }

        private void ResizeShape(Point currentPoint)
        {
            if (_selectedShape == null)
                return;

            var deltaX = currentPoint.X - _dragStartPoint.X;
            var deltaY = currentPoint.Y - _dragStartPoint.Y;

            switch (_resizeDirection)
            {
                case ResizeDirection.TopLeft:
                    _selectedShape.X += deltaX;
                    _selectedShape.Y += deltaY;
                    _selectedShape.Width -= deltaX;
                    _selectedShape.Height -= deltaY;
                    break;
                case ResizeDirection.Top:
                    _selectedShape.Y += deltaY;
                    _selectedShape.Height -= deltaY;
                    break;
                case ResizeDirection.TopRight:
                    _selectedShape.Y += deltaY;
                    _selectedShape.Width += deltaX;
                    _selectedShape.Height -= deltaY;
                    break;
                case ResizeDirection.Right:
                    _selectedShape.Width += deltaX;
                    break;
                case ResizeDirection.BottomRight:
                    _selectedShape.Width += deltaX;
                    _selectedShape.Height += deltaY;
                    break;
                case ResizeDirection.Bottom:
                    _selectedShape.Height += deltaY;
                    break;
                case ResizeDirection.BottomLeft:
                    _selectedShape.X += deltaX;
                    _selectedShape.Width -= deltaX;
                    _selectedShape.Height += deltaY;
                    break;
                case ResizeDirection.Left:
                    _selectedShape.X += deltaX;
                    _selectedShape.Width -= deltaX;
                    break;
            }

            _dragStartPoint = currentPoint;
        }

        private enum ResizeDirection
        {
            TopLeft,
            Top,
            TopRight,
            Right,
            BottomRight,
            Bottom,
            BottomLeft,
            Left
        }

        public void AddShape(Presentation.Models.SlideShape shape)
        {
            if (_currentSlide == null)
                return;

            shape.Id = _currentSlide.Shapes.Count > 0 ? _currentSlide.Shapes.Max(s => s.Id) + 1 : 1;
            shape.ZIndex = _currentSlide.Shapes.Count > 0 ? _currentSlide.Shapes.Max(s => s.ZIndex) + 1 : 0;
            _currentSlide.Shapes.Add(shape);
            SelectedShape = shape;
            InvalidateVisual();
        }

        public void DeleteSelectedShape()
        {
            if (_selectedShape == null || _currentSlide == null)
                return;

            _currentSlide.Shapes.Remove(_selectedShape);
            _selectedShape = null;
            InvalidateVisual();
        }

        public void BringToFront()
        {
            if (_selectedShape == null || _currentSlide == null)
                return;

            var maxZ = _currentSlide.Shapes.Max(s => s.ZIndex);
            _selectedShape.ZIndex = maxZ + 1;
            InvalidateVisual();
        }

        public void SendToBack()
        {
            if (_selectedShape == null || _currentSlide == null)
                return;

            var minZ = _currentSlide.Shapes.Min(s => s.ZIndex);
            _selectedShape.ZIndex = minZ - 1;
            InvalidateVisual();
        }

        public void AlignLeft()
        {
            if (_selectedShape == null)
                return;

            var left = _currentSlide.Shapes.Where(s => s != _selectedShape).Min(s => s.X);
            _selectedShape.X = left;
            InvalidateVisual();
        }

        public void AlignRight()
        {
            if (_selectedShape == null)
                return;

            var right = _currentSlide.Shapes.Where(s => s != _selectedShape).Max(s => s.X + s.Width);
            _selectedShape.X = right - _selectedShape.Width;
            InvalidateVisual();
        }

        public void AlignTop()
        {
            if (_selectedShape == null)
                return;

            var top = _currentSlide.Shapes.Where(s => s != _selectedShape).Min(s => s.Y);
            _selectedShape.Y = top;
            InvalidateVisual();
        }

        public void AlignBottom()
        {
            if (_selectedShape == null)
                return;

            var bottom = _currentSlide.Shapes.Where(s => s != _selectedShape).Max(s => s.Y + s.Height);
            _selectedShape.Y = bottom - _selectedShape.Height;
            InvalidateVisual();
        }

        public void AlignCenterHorizontal()
        {
            if (_selectedShape == null)
                return;

            var centerX = ActualWidth / 2;
            _selectedShape.X = centerX - _selectedShape.Width / 2;
            InvalidateVisual();
        }

        public void AlignCenterVertical()
        {
            if (_selectedShape == null)
                return;

            var centerY = ActualHeight / 2;
            _selectedShape.Y = centerY - _selectedShape.Height / 2;
            InvalidateVisual();
        }

        public void DistributeHorizontally()
        {
        }

        public void DistributeVertically()
        {
        }

        public void GroupShapes()
        {
        }

        public void UngroupShapes()
        {
        }

        public void Rotate(double angle)
        {
            if (_selectedShape == null)
                return;

            _selectedShape.Rotation += angle;
            InvalidateVisual();
        }

        public void FlipHorizontal()
        {
            if (_selectedShape == null)
                return;

            _selectedShape.Width = -_selectedShape.Width;
            InvalidateVisual();
        }

        public void FlipVertical()
        {
            if (_selectedShape == null)
                return;

            _selectedShape.Height = -_selectedShape.Height;
            InvalidateVisual();
        }

        public void Copy()
        {
            if (_selectedShape == null)
                return;

            Clipboard.SetData("SlideShape", _selectedShape.Clone());
        }

        public void Cut()
        {
            Copy();
            DeleteSelectedShape();
        }

        public void Paste()
        {
            if (!Clipboard.ContainsData("SlideShape"))
                return;

            var shape = Clipboard.GetData("SlideShape") as Presentation.Models.SlideShape;
            if (shape != null)
            {
                shape.X += 20;
                shape.Y += 20;
                AddShape(shape);
            }
        }

        public void Undo()
        {
        }

        public void Redo()
        {
        }
    }

    public class SlideListView : ListBox
    {
        private List<Presentation.Models.Slide> _slides;

        public SlideListView()
        {
            _slides = new List<Presentation.Models.Slide>();
            SelectionChanged += OnSelectionChanged;
        }

        public List<Presentation.Models.Slide> Slides
        {
            get => _slides;
            set
            {
                _slides = value;
                RefreshList();
            }
        }

        public event EventHandler<SlideEventArgs> SlideSelected;

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedItem is SlideListItem item)
            {
                SlideSelected?.Invoke(this, new SlideEventArgs { Slide = item.Slide });
            }
        }

        private void RefreshList()
        {
            Items.Clear();
            foreach (var slide in _slides)
            {
                Items.Add(new SlideListItem { Slide = slide });
            }
        }

        public void AddSlide(Presentation.Models.Slide slide)
        {
            _slides.Add(slide);
            Items.Add(new SlideListItem { Slide = slide });
        }

        public void RemoveSlide(int index)
        {
            if (index >= 0 && index < _slides.Count)
            {
                _slides.RemoveAt(index);
                Items.RemoveAt(index);
            }
        }

        public void MoveSlide(int fromIndex, int toIndex)
        {
            if (fromIndex < 0 || fromIndex >= _slides.Count || toIndex < 0 || toIndex >= _slides.Count)
                return;

            var slide = _slides[fromIndex];
            _slides.RemoveAt(fromIndex);
            _slides.Insert(toIndex, slide);

            RefreshList();
        }
    }

    public class SlideListItem
    {
        public Presentation.Models.Slide Slide { get; set; }
        public int Index => Slide?.Index ?? 0;
        public string Name => Slide?.Name ?? $"Slide {Index + 1}";
    }

    public class SlideEventArgs : EventArgs
    {
        public Presentation.Models.Slide Slide { get; set; }
    }

    public class AnimationPaneView : UserControl
    {
        private ListBox _animationList;
        private Button _addButton;
        private Button _removeButton;
        private Button _moveUpButton;
        private Button _moveDownButton;

        public AnimationPaneView()
        {
            BuildUI();
        }

        private void BuildUI()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(5)
            };

            _addButton = new Button { Content = "Add", Margin = new Thickness(2) };
            _removeButton = new Button { Content = "Remove", Margin = new Thickness(2) };
            _moveUpButton = new Button { Content = "Move Up", Margin = new Thickness(2) };
            _moveDownButton = new Button { Content = "Move Down", Margin = new Thickness(2) };

            buttonPanel.Children.Add(_addButton);
            buttonPanel.Children.Add(_removeButton);
            buttonPanel.Children.Add(_moveUpButton);
            buttonPanel.Children.Add(_moveDownButton);

            Grid.SetRow(buttonPanel, 0);
            grid.Children.Add(buttonPanel);

            _animationList = new ListBox { Margin = new Thickness(5) };
            Grid.SetRow(_animationList, 1);
            grid.Children.Add(_animationList);

            Content = grid;
        }

        public List<Presentation.Models.Animation> Animations
        {
            get => (List<Presentation.Models.Animation>)_animationList.ItemsSource;
            set => _animationList.ItemsSource = value;
        }

        public event EventHandler<AnimationEventArgs> AnimationAdded;
        public event EventHandler<AnimationEventArgs> AnimationRemoved;
    }

    public class AnimationEventArgs : EventArgs
    {
        public Presentation.Models.Animation Animation { get; set; }
    }

    public class MasterPaneView : UserControl
    {
        public MasterPaneView()
        {
            Background = Brushes.LightGray;
        }
    }

    public class ThumbnailPaneView : UserControl
    {
        private SlideListView _slideList;

        public ThumbnailPaneView()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var header = new TextBlock
            {
                Text = "Slides",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };
            Grid.SetRow(header, 0);
            grid.Children.Add(header);

            _slideList = new SlideListView
            {
                Margin = new Thickness(5)
            };
            Grid.SetRow(_slideList, 1);
            grid.Children.Add(_slideList);

            Content = grid;
        }

        public List<Presentation.Models.Slide> Slides
        {
            get => _slideList.Slides;
            set => _slideList.Slides = value;
        }
    }

    public class NotesPaneView : UserControl
    {
        private TextBox _notesTextBox;

        public NotesPaneView()
        {
            var grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            var header = new TextBlock
            {
                Text = "Notes",
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5)
            };
            Grid.SetRow(header, 0);
            grid.Children.Add(header);

            _notesTextBox = new TextBox
            {
                AcceptsReturn = true,
                AcceptsTab = true,
                TextWrapping = TextWrapping.Wrap,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(5)
            };
            Grid.SetRow(_notesTextBox, 1);
            grid.Children.Add(_notesTextBox);

            Content = grid;
        }

        public string Notes
        {
            get => _notesTextBox.Text;
            set => _notesTextBox.Text = value ?? "";
        }
    }

    public class SlideShowView : Window
    {
        private Presentation.Models.PresentationDocument _document;
        private int _currentSlideIndex;
        private bool _isFullScreen;
        private Canvas _slideCanvas;
        private System.Windows.Threading.DispatcherTimer _autoAdvanceTimer;

        public SlideShowView()
        {
            WindowStyle = WindowStyle.None;
            Background = Brushes.Black;
            KeyDown += OnKeyDown;
            MouseDown += OnMouseDown;

            _slideCanvas = new Canvas { Background = Brushes.White };
            Content = _slideCanvas;

            _autoAdvanceTimer = new System.Windows.Threading.DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            _autoAdvanceTimer.Tick += (s, e) => NextSlide();
        }

        public Presentation.Models.PresentationDocument Document
        {
            get => _document;
            set
            {
                _document = value;
                _currentSlideIndex = 0;
                ShowCurrentSlide();
            }
        }

        private void ShowCurrentSlide()
        {
            if (_document == null || _currentSlideIndex < 0 || _currentSlideIndex >= _document.Slides.Count)
                return;

            var slide = _document.Slides[_currentSlideIndex];
            _slideCanvas.Children.Clear();

            foreach (var shape in slide.Shapes.OrderBy(s => s.ZIndex))
            {
                var element = CreateShapeElement(shape);
                Canvas.SetLeft(element, shape.X);
                Canvas.SetTop(element, shape.Y);
                _slideCanvas.Children.Add(element);
            }

            Title = $"Slide {_currentSlideIndex + 1} of {_document.Slides.Count}";
        }

        private FrameworkElement CreateShapeElement(Presentation.Models.SlideShape shape)
        {
            var element = new Border
            {
                Width = shape.Width,
                Height = shape.Height,
                Background = GetBrush(shape.Fill),
                BorderBrush = GetPen(shape.Line)?.Brush,
                BorderThickness = new Thickness(shape.Line?.Width ?? 0)
            };

            if (shape.TextBox != null && !string.IsNullOrEmpty(shape.TextBox.Text))
            {
                element.Child = new TextBlock
                {
                    Text = shape.TextBox.Text,
                    FontFamily = new FontFamily(shape.TextBox.DefaultFont?.FontName ?? "Calibri"),
                    FontSize = shape.TextBox.DefaultFont?.FontSize ?? 14,
                    TextWrapping = TextWrapping.Wrap,
                    Padding = new Thickness(5)
                };
            }

            return element;
        }

        private Brush GetBrush(Models.FillFormat fill)
        {
            if (fill == null || fill.Type == Models.FillType.None)
                return Brushes.Transparent;

            if (fill.Type == Models.FillType.Solid)
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(fill.Color ?? "#FFFFFF"));

            return Brushes.White;
        }

        private Pen GetPen(Models.LineFormat line)
        {
            if (line == null || !line.IsVisible)
                return null;

            return new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString(line.Color ?? "#000000")), line.Width);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    break;
                case Key.Space:
                case Key.Right:
                case Key.Down:
                case Key.PageDown:
                    NextSlide();
                    break;
                case Key.Left:
                case Key.Up:
                case Key.PageUp:
                    PreviousSlide();
                    break;
                case Key.Home:
                    FirstSlide();
                    break;
                case Key.End:
                    LastSlide();
                    break;
                case Key.B:
                    ToggleBlackScreen();
                    break;
                case Key.W:
                    ToggleWhiteScreen();
                    break;
            }

            e.Handled = true;
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                NextSlide();
            else if (e.ChangedButton == MouseButton.Right)
                PreviousSlide();
        }

        public void NextSlide()
        {
            if (_document == null)
                return;

            if (_currentSlideIndex < _document.Slides.Count - 1)
            {
                _currentSlideIndex++;
                ShowCurrentSlide();
            }
        }

        public void PreviousSlide()
        {
            if (_document == null)
                return;

            if (_currentSlideIndex > 0)
            {
                _currentSlideIndex--;
                ShowCurrentSlide();
            }
        }

        public void FirstSlide()
        {
            _currentSlideIndex = 0;
            ShowCurrentSlide();
        }

        public void LastSlide()
        {
            if (_document != null)
            {
                _currentSlideIndex = _document.Slides.Count - 1;
                ShowCurrentSlide();
            }
        }

        public void ToggleBlackScreen()
        {
            Background = Background == Brushes.Black ? Brushes.White : Brushes.Black;
        }

        public void ToggleWhiteScreen()
        {
            Background = Brushes.White;
        }

        public void StartPresentation()
        {
            _isFullScreen = true;
            WindowState = WindowState.Maximized;
            Show();
        }

        public void StartFromSlide(int index)
        {
            _currentSlideIndex = Math.Max(0, Math.Min(index, _document?.Slides.Count - 1 ?? 0));
            StartPresentation();
        }

        public void StartWithAutoAdvance(TimeSpan interval)
        {
            _autoAdvanceTimer.Interval = interval;
            _autoAdvanceTimer.Start();
            StartPresentation();
        }
    }
}

namespace OfficeSuite.Spreadsheet.Views
{
    public class SpreadsheetGridView : Canvas
    {
        private Models.SpreadsheetDocument _document;
        private int _currentWorksheetIndex;
        private int _maxRows = 100;
        private int _maxColumns = 26;
        private double _rowHeight = 20;
        private double _columnWidth = 100;
        private double _headerHeight = 25;
        private double _rowHeaderWidth = 50;

        private int _selectedRow = -1;
        private int _selectedColumn = -1;
        private bool _isSelecting;
        private Point _selectionStart;
        private Models.CellRange _selectionRange;

        public SpreadsheetGridView()
        {
            Background = Brushes.White;
            ClipToBounds = true;
            Focusable = true;
            Loaded += OnLoaded;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            MouseMove += OnMouseMove;
            MouseLeftButtonUp += OnMouseLeftButtonUp;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();
            InvalidateVisual();
        }

        public Models.SpreadsheetDocument Document
        {
            get => _document;
            set
            {
                _document = value;
                _currentWorksheetIndex = 0;
                InvalidateVisual();
            }
        }

        public Models.Worksheet CurrentWorksheet
        {
            get
            {
                if (_document == null || _currentWorksheetIndex < 0 || _currentWorksheetIndex >= _document.Worksheets.Count)
                    return null;
                return _document.Worksheets[_currentWorksheetIndex];
            }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            DrawGridLines(dc);
            DrawCells(dc);
            DrawHeaders(dc);
            DrawSelection(dc);
            DrawCellBorders(dc);
        }

        private void DrawGridLines(DrawingContext dc)
        {
            var pen = new Pen(new SolidColorBrush(Color.FromRgb(200, 200, 200)), 1);
            pen.Freeze();

            for (int row = 0; row < _maxRows; row++)
            {
                var y = _headerHeight + row * _rowHeight;
                dc.DrawLine(pen, new Point(_rowHeaderWidth, y), new Point(ActualWidth, y));
            }

            for (int col = 0; col < _maxColumns; col++)
            {
                var x = _rowHeaderWidth + col * _columnWidth;
                dc.DrawLine(pen, new Point(x, _headerHeight), new Point(x, ActualHeight));
            }
        }

        private void DrawCells(DrawingContext dc)
        {
            if (CurrentWorksheet == null)
                return;

            var typeface = new Typeface("Calibri");
            var fontSize = 11.0;
            var brush = new SolidColorBrush(Colors.Black);

            foreach (var cell in CurrentWorksheet.Cells)
            {
                if (cell.Row < 0 || cell.Row >= _maxRows || cell.Column < 0 || cell.Column >= _maxColumns)
                    continue;

                var x = _rowHeaderWidth + cell.Column * _columnWidth + 2;
                var y = _headerHeight + cell.Row * _rowHeight + 2;
                var width = _columnWidth - 4;
                var height = _rowHeight - 4;

                if (cell.Value != null)
                {
                    var text = cell.Value.ToString();
                    if (cell.Value is double d)
                        text = d.ToString("0.00");
                    else if (cell.Value is DateTime dt)
                        text = dt.ToString("yyyy-MM-dd");

                    var ft = new FormattedText(
                        text,
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        typeface,
                        fontSize,
                        brush,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    ft.MaxTextWidth = width;
                    ft.MaxTextHeight = height;

                    dc.DrawText(ft, new Point(x, y));

                    DrawCellFormat(dc, cell, x - 2, y - 2, width + 4, height + 4);
                }
            }
        }

        private void DrawCellFormat(DrawingContext dc, Models.Cell cell, double x, double y, double width, double height)
        {
            if (cell.Format == null)
                return;

            if (cell.Format.BackgroundColor != null)
            {
                var color = (Color)ColorConverter.ConvertFromString(cell.Format.BackgroundColor);
                dc.DrawRectangle(new SolidColorBrush(color), null, new Rect(x, y, width, height));
            }

            if (cell.Format.Border != null)
            {
                var pen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString(cell.Format.Border.Color)), cell.Format.Border.Width);
                dc.DrawRectangle(null, pen, new Rect(x, y, width, height));
            }
        }

        private void DrawHeaders(DrawingContext dc)
        {
            var headerBrush = new SolidColorBrush(Color.FromRgb(240, 240, 240));
            var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(180, 180, 180)), 1);

            dc.DrawRectangle(headerBrush, borderPen, new Rect(0, 0, ActualWidth, _headerHeight));
            dc.DrawRectangle(headerBrush, borderPen, new Rect(0, 0, _rowHeaderWidth, ActualHeight));

            var typeface = new Typeface(new FontFamily("Calibri"), FontStyles.Normal, FontWeights.Bold, FontStretches.Normal);
            var fontSize = 10.0;
            var brush = new SolidColorBrush(Colors.Black);

            for (int col = 0; col < _maxColumns; col++)
            {
                var colLetter = GetColumnLetter(col);
                var x = _rowHeaderWidth + col * _columnWidth;
                var y = 5.0;

                var ft = new FormattedText(
                    colLetter,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    fontSize,
                    brush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                dc.DrawText(ft, new Point(x + _columnWidth / 2 - ft.Width / 2, y));
            }

            for (int row = 0; row < _maxRows; row++)
            {
                var rowNumber = (row + 1).ToString();
                var x = 5.0;
                var y = _headerHeight + row * _rowHeight + 5;

                var ft = new FormattedText(
                    rowNumber,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    typeface,
                    fontSize,
                    brush,
                    VisualTreeHelper.GetDpi(this).PixelsPerDip);

                dc.DrawText(ft, new Point(x, y));
            }
        }

        private void DrawSelection(DrawingContext dc)
        {
            if (_selectedRow < 0 || _selectedColumn < 0)
                return;

            var x = _rowHeaderWidth + _selectedColumn * _columnWidth;
            var y = _headerHeight + _selectedRow * _rowHeight;

            var pen = new Pen(Brushes.Blue, 2);
            dc.DrawRectangle(null, pen, new Rect(x, y, _columnWidth, _rowHeight));

            if (_selectionRange != null)
            {
                var startX = _rowHeaderWidth + _selectionRange.StartColumn * _columnWidth;
                var startY = _headerHeight + _selectionRange.StartRow * _rowHeight;
                var endX = _rowHeaderWidth + (_selectionRange.EndColumn + 1) * _columnWidth;
                var endY = _headerHeight + (_selectionRange.EndRow + 1) * _rowHeight;

                var highlightBrush = new SolidColorBrush(Color.FromArgb(50, 0, 0, 255));
                dc.DrawRectangle(highlightBrush, null, new Rect(startX, startY, endX - startX, endY - startY));
            }
        }

        private void DrawCellBorders(DrawingContext dc)
        {
            if (CurrentWorksheet == null)
                return;

            foreach (var cell in CurrentWorksheet.Cells)
            {
                if (cell.Row < 0 || cell.Row >= _maxRows || cell.Column < 0 || cell.Column >= _maxColumns)
                    continue;

                if (cell.Format?.Border == null)
                    continue;

                var x = _rowHeaderWidth + cell.Column * _columnWidth;
                var y = _headerHeight + cell.Row * _rowHeight;
                var pen = new Pen(
                    new SolidColorBrush((Color)ColorConverter.ConvertFromString(cell.Format.Border.Color)),
                    cell.Format.Border.Width);

                if ((cell.Format.Border.TopBorder & Models.BorderType.Top) != 0)
                    dc.DrawLine(pen, new Point(x, y), new Point(x + _columnWidth, y));

                if ((cell.Format.Border.BottomBorder & Models.BorderType.Bottom) != 0)
                    dc.DrawLine(pen, new Point(x, y + _rowHeight), new Point(x + _columnWidth, y + _rowHeight));

                if ((cell.Format.Border.LeftBorder & Models.BorderType.Left) != 0)
                    dc.DrawLine(pen, new Point(x, y), new Point(x, y + _rowHeight));

                if ((cell.Format.Border.RightBorder & Models.BorderType.Right) != 0)
                    dc.DrawLine(pen, new Point(x + _columnWidth, y), new Point(x + _columnWidth, y + _rowHeight));
            }
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(this);
            var row = GetRowAtPosition(pos.Y);
            var col = GetColumnAtPosition(pos.X);

            if (row >= 0 && col >= 0)
            {
                _selectedRow = row;
                _selectedColumn = col;
                _selectionStart = pos;
                _isSelecting = true;
                CaptureMouse();
                InvalidateVisual();
            }

            Focus();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!_isSelecting)
                return;

            var pos = e.GetPosition(this);
            var row = GetRowAtPosition(pos.Y);
            var col = GetColumnAtPosition(pos.X);

            if (row >= 0 && col >= 0)
            {
                _selectionRange = new Models.CellRange
                {
                    StartRow = Math.Min(_selectedRow, row),
                    EndRow = Math.Max(_selectedRow, row),
                    StartColumn = Math.Min(_selectedColumn, col),
                    EndColumn = Math.Max(_selectedColumn, col)
                };
                InvalidateVisual();
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isSelecting = false;
            ReleaseMouseCapture();
        }

        private int GetRowAtPosition(double y)
        {
            return (int)((y - _headerHeight) / _rowHeight);
        }

        private int GetColumnAtPosition(double x)
        {
            return (int)((x - _rowHeaderWidth) / _columnWidth);
        }

        private string GetColumnLetter(int column)
        {
            var result = "";
            while (column >= 0)
            {
                result = (char)('A' + column % 26) + result;
                column = column / 26 - 1;
            }
            return result;
        }

        public void SetCellValue(int row, int column, object value)
        {
            if (CurrentWorksheet == null)
                return;

            var cell = CurrentWorksheet.GetCell(row, column);
            if (cell == null)
            {
                cell = new Models.Cell { Row = row, Column = column, Value = value };
                CurrentWorksheet.Cells.Add(cell);
            }
            else
            {
                cell.Value = value;
            }

            InvalidateVisual();
        }

        public object GetCellValue(int row, int column)
        {
            var cell = CurrentWorksheet?.GetCell(row, column);
            return cell?.Value;
        }

        public void InsertRow(int beforeRow)
        {
            if (CurrentWorksheet == null)
                return;

            foreach (var cell in CurrentWorksheet.Cells.Where(c => c.Row >= beforeRow))
            {
                cell.Row++;
            }

            InvalidateVisual();
        }

        public void DeleteRow(int row)
        {
            if (CurrentWorksheet == null)
                return;

            CurrentWorksheet.Cells.RemoveAll(c => c.Row == row);
            foreach (var cell in CurrentWorksheet.Cells.Where(c => c.Row > row))
            {
                cell.Row--;
            }

            InvalidateVisual();
        }

        public void InsertColumn(int beforeColumn)
        {
            if (CurrentWorksheet == null)
                return;

            foreach (var cell in CurrentWorksheet.Cells.Where(c => c.Column >= beforeColumn))
            {
                cell.Column++;
            }

            InvalidateVisual();
        }

        public void DeleteColumn(int column)
        {
            if (CurrentWorksheet == null)
                return;

            CurrentWorksheet.Cells.RemoveAll(c => c.Column == column);
            foreach (var cell in CurrentWorksheet.Cells.Where(c => c.Column > column))
            {
                cell.Column--;
            }

            InvalidateVisual();
        }

        public void AutoFitColumn(int column)
        {
        }

        public void AutoFitRow(int row)
        {
        }

        public void FreezePanes(int row, int column)
        {
        }

        public void UnfreezePanes()
        {
        }

        public void ApplyFilter(int startRow, int startColumn, int endRow, int endColumn)
        {
        }

        public void ClearFilter()
        {
        }

        public void SortRange(int startRow, int startColumn, int endRow, int endColumn, bool ascending)
        {
            if (CurrentWorksheet == null)
                return;

            var cells = CurrentWorksheet.Cells
                .Where(c => c.Row >= startRow && c.Row <= endRow &&
                            c.Column >= startColumn && c.Column <= endColumn)
                .OrderBy(c => c.Value)
                .ToList();

            var row = startRow;
            var col = startColumn;
            foreach (var cell in cells)
            {
                var targetCell = CurrentWorksheet.GetCell(row, col);
                if (targetCell != null)
                    targetCell.Value = cell.Value;
                col++;
                if (col > endColumn)
                {
                    col = startColumn;
                    row++;
                }
            }

            InvalidateVisual();
        }
    }

    public class FormulaBarView : UserControl
    {
        private TextBox _formulaTextBox;
        private ComboBox _functionComboBox;

        public FormulaBarView()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            var label = new TextBlock
            {
                Text = "fx",
                FontWeight = FontWeights.Bold,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            _formulaTextBox = new TextBox
            {
                BorderThickness = new Thickness(1),
                Margin = new Thickness(5)
            };
            Grid.SetColumn(_formulaTextBox, 1);
            grid.Children.Add(_formulaTextBox);

            _functionComboBox = new ComboBox
            {
                Width = 150,
                Margin = new Thickness(5)
            };
            _functionComboBox.Items.Add("SUM");
            _functionComboBox.Items.Add("AVERAGE");
            _functionComboBox.Items.Add("COUNT");
            _functionComboBox.Items.Add("MAX");
            _functionComboBox.Items.Add("MIN");
            _functionComboBox.SelectedIndex = 0;
            Grid.SetColumn(_functionComboBox, 2);
            grid.Children.Add(_functionComboBox);

            Content = grid;
        }

        public string Formula
        {
            get => _formulaTextBox.Text;
            set => _formulaTextBox.Text = value ?? "";
        }

        public event EventHandler<string> FormulaChanged;

        public FormulaBarView()
        {
            _formulaTextBox.TextChanged += (s, e) => FormulaChanged?.Invoke(this, _formulaTextBox.Text);
        }
    }

    public class StatusBarView : UserControl
    {
        private TextBlock _pageInfo;
        private TextBlock _cellInfo;
        private TextBlock _zoomLevel;

        public StatusBarView()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

            _pageInfo = new TextBlock { Text = "Sheet 1", Margin = new Thickness(5) };
            Grid.SetColumn(_pageInfo, 0);
            grid.Children.Add(_pageInfo);

            _cellInfo = new TextBlock { Text = "Ready", Margin = new Thickness(5) };
            Grid.SetColumn(_cellInfo, 1);
            grid.Children.Add(_cellInfo);

            _zoomLevel = new TextBlock { Text = "100%", Margin = new Thickness(5) };
            Grid.SetColumn(_zoomLevel, 2);
            grid.Children.Add(_zoomLevel);

            Content = grid;
        }

        public string PageInfo
        {
            get => _pageInfo.Text;
            set => _pageInfo.Text = value ?? "";
        }

        public string CellInfo
        {
            get => _cellInfo.Text;
            set => _cellInfo.Text = value ?? "";
        }

        public string ZoomLevel
        {
            get => _zoomLevel.Text;
            set => _zoomLevel.Text = value ?? "100%";
        }
    }
}

namespace OfficeSuite.Word.Views
{
    public class WordEditorView : RichTextBox
    {
        private Models.WordDocument _document;
        private bool _isUpdating;

        public WordEditorView()
        {
            AcceptsReturn = true;
            AcceptsTab = true;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            TextChanged += OnTextChanged;
        }

        public Models.WordDocument Document
        {
            get => _document;
            set
            {
                _document = value;
                LoadDocument();
            }
        }

        private void LoadDocument()
        {
            if (_document == null)
                return;

            _isUpdating = true;
            Document.Blocks.Clear();

            foreach (var para in _document.Paragraphs)
            {
                var p = new Paragraph();

                foreach (var run in para.Runs)
                {
                    var r = new Run(run.Text ?? "");

                    if (run.Format != null)
                    {
                        r.FontFamily = new FontFamily(run.Format.FontName ?? "Calibri");
                        r.FontSize = run.Format.FontSize * 2 ?? 22;
                        r.FontWeight = run.Format.IsBold ? FontWeights.Bold : FontWeights.Normal;
                        r.FontStyle = run.Format.IsItalic ? FontStyles.Italic : FontStyles.Normal;
                        r.TextDecorations = run.Format.IsUnderline ? TextDecorations.Underline : null;
                    }

                    p.Inlines.Add(r);
                }

                if (para.Format != null)
                {
                    switch (para.Format.Alignment)
                    {
                        case Models.HorizontalAlignment.Center:
                            p.TextAlignment = TextAlignment.Center;
                            break;
                        case Models.HorizontalAlignment.Right:
                            p.TextAlignment = TextAlignment.Right;
                            break;
                        case Models.HorizontalAlignment.Justify:
                            p.TextAlignment = TextAlignment.Justify;
                            break;
                    }
                }

                Document.Blocks.Add(p);
            }

            _isUpdating = false;
        }

        private void OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (_isUpdating)
                return;

            UpdateDocument();
        }

        private void UpdateDocument()
        {
            if (_document == null)
                return;

            _document.Paragraphs.Clear();

            foreach (var block in Document.Blocks)
            {
                if (block is Paragraph para)
                {
                    var wordPara = new Models.Paragraph();

                    foreach (var inline in para.Inlines)
                    {
                        if (inline is Run run)
                        {
                            var wordRun = new Models.Run { Text = run.Text };

                            if (run.FontFamily != null)
                            {
                                wordRun.Format = wordRun.Format ?? new Models.CharacterFormat();
                                wordRun.Format.FontName = run.FontFamily.Source;
                            }

                            wordRun.Format = wordRun.Format ?? new Models.CharacterFormat();
                            wordRun.Format.FontSize = (run.FontSize ?? 11) / 2;
                            wordRun.Format.IsBold = run.FontWeight == FontWeights.Bold;
                            wordRun.Format.IsItalic = run.FontStyle == FontStyles.Italic;
                            wordRun.Format.IsUnderline = run.TextDecorations == TextDecorations.Underline;

                            wordPara.Runs.Add(wordRun);
                        }
                    }

                    wordPara.Format = new Models.ParagraphFormat();
                    switch (para.TextAlignment)
                    {
                        case TextAlignment.Center:
                            wordPara.Format.Alignment = Models.HorizontalAlignment.Center;
                            break;
                        case TextAlignment.Right:
                            wordPara.Format.Alignment = Models.HorizontalAlignment.Right;
                            break;
                        case TextAlignment.Justify:
                            wordPara.Format.Alignment = Models.HorizontalAlignment.Justify;
                            break;
                        default:
                            wordPara.Format.Alignment = Models.HorizontalAlignment.Left;
                            break;
                    }

                    _document.Paragraphs.Add(wordPara);
                }
            }
        }

        public void InsertText(string text)
        {
            CaretPosition.InsertTextInRun(text);
        }

        public void FormatSelection(Models.CharacterFormat format)
        {
            if (Selection == null || Selection.IsEmpty)
                return;

            foreach (var run in Selection.GetPropertyValue(Run.FontFamilyProperty) as FontFamily)
            {
            }

            Selection.ApplyPropertyValue(Run.FontFamilyProperty, new FontFamily(format.FontName ?? "Calibri"));
            Selection.ApplyPropertyValue(Run.FontSizeProperty, new FontSize(format.FontSize * 2 ?? 22));
            Selection.ApplyPropertyValue(Run.FontWeightProperty, format.IsBold ? FontWeights.Bold : FontWeights.Normal);
            Selection.ApplyPropertyValue(Run.FontStyleProperty, format.IsItalic ? FontStyles.Italic : FontStyles.Normal);

            if (format.IsUnderline)
                Selection.ApplyPropertyValue(Run.TextDecorationsProperty, TextDecorations.Underline);
            else
                Selection.ApplyPropertyValue(Run.TextDecorationsProperty, null);
        }

        public void AlignSelection(Models.HorizontalAlignment alignment)
        {
            if (Selection == null)
                return;

            var paragraph = Selection.Paragraph;
            if (paragraph == null)
                return;

            switch (alignment)
            {
                case Models.HorizontalAlignment.Center:
                    paragraph.TextAlignment = TextAlignment.Center;
                    break;
                case Models.HorizontalAlignment.Right:
                    paragraph.TextAlignment = TextAlignment.Right;
                    break;
                case Models.HorizontalAlignment.Justify:
                    paragraph.TextAlignment = TextAlignment.Justify;
                    break;
                default:
                    paragraph.TextAlignment = TextAlignment.Left;
                    break;
            }
        }

        public void SetSelectionBold()
        {
            if (Selection == null)
                return;

            var currentWeight = Selection.GetPropertyValue(Run.FontWeightProperty);
            var newWeight = currentWeight != null && currentWeight.Equals(FontWeights.Bold) ? FontWeights.Normal : FontWeights.Bold;
            Selection.ApplyPropertyValue(Run.FontWeightProperty, newWeight);
        }

        public void SetSelectionItalic()
        {
            if (Selection == null)
                return;

            var currentStyle = Selection.GetPropertyValue(Run.FontStyleProperty);
            var newStyle = currentStyle != null && currentStyle.Equals(FontStyles.Italic) ? FontStyles.Normal : FontStyles.Italic;
            Selection.ApplyPropertyValue(Run.FontStyleProperty, newStyle);
        }

        public void SetSelectionUnderline()
        {
            if (Selection == null)
                return;

            var currentDecoration = Selection.GetPropertyValue(Run.TextDecorationsProperty);
            var newDecoration = currentDecoration == TextDecorations.Underline ? null : TextDecorations.Underline;
            Selection.ApplyPropertyValue(Run.TextDecorationsProperty, newDecoration);
        }

        public void IncreaseFontSize()
        {
            if (Selection == null)
                return;

            var currentSize = Selection.GetPropertyValue(Run.FontSizeProperty);
            if (currentSize != null && currentSize is double size)
            {
                Selection.ApplyPropertyValue(Run.FontSizeProperty, size + 2);
            }
        }

        public void DecreaseFontSize()
        {
            if (Selection == null)
                return;

            var currentSize = Selection.GetPropertyValue(Run.FontSizeProperty);
            if (currentSize != null && currentSize is double size && size > 8)
            {
                Selection.ApplyPropertyValue(Run.FontSizeProperty, size - 2);
            }
        }

        public void ClearFormatting()
        {
            if (Selection == null)
                return;

            Selection.ApplyPropertyValue(Run.FontFamilyProperty, new FontFamily("Calibri"));
            Selection.ApplyPropertyValue(Run.FontSizeProperty, 11.0);
            Selection.ApplyPropertyValue(Run.FontWeightProperty, FontWeights.Normal);
            Selection.ApplyPropertyValue(Run.FontStyleProperty, FontStyles.Normal);
            Selection.ApplyPropertyValue(Run.TextDecorationsProperty, null);
        }

        public void InsertBulletPoint()
        {
            if (Selection?.Paragraph == null)
                return;

            var paragraph = Selection.Paragraph;
            var text = paragraph.ContentStart.GetTextInRun(LogicalDirection.Forward);

            paragraph.ContentStart.InsertTextInRun("\u2022 ");
        }

        public void InsertNumberedList(int number)
        {
            if (Selection?.Paragraph == null)
                return;

            var paragraph = Selection.Paragraph;
            paragraph.ContentStart.InsertTextInRun($"{number}. ");
        }

        public void IncreaseIndent()
        {
            if (Selection?.Paragraph == null)
                return;

            var paragraph = Selection.Paragraph;
            var margin = paragraph.Margin;
            paragraph.Margin = new Thickness(margin.Left + 20, margin.Top, margin.Right, margin.Bottom);
        }

        public void DecreaseIndent()
        {
            if (Selection?.Paragraph == null)
                return;

            var paragraph = Selection.Paragraph;
            var margin = paragraph.Margin;
            paragraph.Margin = new Thickness(Math.Max(0, margin.Left - 20), margin.Top, margin.Right, margin.Bottom);
        }
    }

    public class RulerView : Canvas
    {
        private double _topMargin;
        private double _bottomMargin;
        private double _leftMargin;
        private double _rightMargin;
        private double _pageWidth = 612;
        private double _pageHeight = 792;

        public RulerView()
        {
            Height = 20;
            Background = Brushes.LightGray;
            ClipToBounds = true;
        }

        public double TopMargin
        {
            get => _topMargin;
            set { _topMargin = value; InvalidateVisual(); }
        }

        public double BottomMargin
        {
            get => _bottomMargin;
            set { _bottomMargin = value; InvalidateVisual(); }
        }

        public double LeftMargin
        {
            get => _leftMargin;
            set { _leftMargin = value; InvalidateVisual(); }
        }

        public double RightMargin
        {
            get => _rightMargin;
            set { _rightMargin = value; InvalidateVisual(); }
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);

            var pen = new Pen(Brushes.Black, 1);
            dc.DrawLine(pen, new Point(0, ActualHeight - 1), new Point(ActualWidth, ActualHeight - 1));

            for (int i = 0; i <= _pageWidth; i += 10)
            {
                var x = i;
                var height = i % 50 == 0 ? 10 : (i % 10 == 0 ? 6 : 3);
                dc.DrawLine(pen, new Point(x, ActualHeight - height), new Point(x, ActualHeight));

                if (i % 50 == 0)
                {
                    var ft = new FormattedText(
                        i.ToString(),
                        System.Globalization.CultureInfo.CurrentCulture,
                        FlowDirection.LeftToRight,
                        new Typeface("Arial"),
                        8,
                        Brushes.Black,
                        VisualTreeHelper.GetDpi(this).PixelsPerDip);

                    dc.DrawText(ft, new Point(x - ft.Width / 2, 0));
                }
            }

            var leftInch = _pageWidth * _leftMargin / 72;
            var rightInch = _pageWidth * _rightMargin / 72;

            dc.DrawRectangle(Brushes.LightBlue, null, new Rect(leftInch, 0, rightInch - leftInch, ActualHeight));
        }
    }

    public class MiniToolbarView : Border
    {
        private Button _boldButton;
        private Button _italicButton;
        private Button _underlineButton;
        private ComboBox _fontSizeComboBox;
        private ComboBox _fontFamilyComboBox;

        public MiniToolbarView()
        {
            Background = Brushes.White;
            BorderBrush = Brushes.Gray;
            BorderThickness = new Thickness(1);
            CornerRadius = new CornerRadius(3);
            Padding = new Thickness(5);
            Visibility = Visibility.Collapsed;

            var stack = new StackPanel { Orientation = Orientation.Horizontal };

            _boldButton = new Button { Content = "B", FontWeight = FontWeights.Bold, Width = 30, Margin = new Thickness(2) };
            _italicButton = new Button { Content = "I", FontStyle = FontStyles.Italic, Width = 30, Margin = new Thickness(2) };
            _underlineButton = new Button { Content = "U", Width = 30, Margin = new Thickness(2) };

            _fontFamilyComboBox = new ComboBox { Width = 120, Margin = new Thickness(2) };
            _fontFamilyComboBox.Items.Add("Calibri");
            _fontFamilyComboBox.Items.Add("Arial");
            _fontFamilyComboBox.Items.Add("Times New Roman");
            _fontFamilyComboBox.SelectedIndex = 0;

            _fontSizeComboBox = new ComboBox { Width = 60, Margin = new Thickness(2) };
            _fontSizeComboBox.Items.Add("8");
            _fontSizeComboBox.Items.Add("10");
            _fontSizeComboBox.Items.Add("11");
            _fontSizeComboBox.Items.Add("12");
            _fontSizeComboBox.Items.Add("14");
            _fontSizeComboBox.Items.Add("16");
            _fontSizeComboBox.Items.Add("18");
            _fontSizeComboBox.Items.Add("20");
            _fontSizeComboBox.Items.Add("24");
            _fontSizeComboBox.Items.Add("36");
            _fontSizeComboBox.Items.Add("48");
            _fontSizeComboBox.SelectedIndex = 2;

            stack.Children.Add(_boldButton);
            stack.Children.Add(_italicButton);
            stack.Children.Add(_underlineButton);
            stack.Children.Add(_fontFamilyComboBox);
            stack.Children.Add(_fontSizeComboBox);

            Child = stack;
        }

        public void Show(Point position)
        {
            Visibility = Visibility.Visible;
        }

        public void Hide()
        {
            Visibility = Visibility.Collapsed;
        }
    }
}

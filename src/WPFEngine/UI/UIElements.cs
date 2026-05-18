using System;
using System.Collections.Generic;
using System.Linq;
using WPFEngine.Math;

namespace WPFEngine.UI
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public enum Alignment
    {
        Start,
        Center,
        End,
        Stretch
    }

    public abstract class UIElement
    {
        protected Vector2 _position;
        protected Vector2 _size;
        protected Vector2 _minSize;
        protected Vector2 _maxSize;
        protected Vector2 _anchor;
        protected Vector2 _pivot;
        protected Vector2 _margin;
        protected Vector2 _padding;
        protected Color _backgroundColor;
        protected Color _borderColor;
        protected float _borderWidth;
        protected bool _isVisible;
        protected bool _isEnabled;
        protected bool _isInteractable;
        protected string? _name;
        protected UIElement? _parent;
        protected List<UIElement> _children;
        protected int _sortingOrder;
        protected string? _tooltip;

        public Vector2 Position
        {
            get => _position;
            set { _position = value; OnLayoutChanged(); }
        }

        public Vector2 Size
        {
            get => _size;
            set { _size = value; OnLayoutChanged(); }
        }

        public Vector2 MinSize
        {
            get => _minSize;
            set { _minSize = value; OnLayoutChanged(); }
        }

        public Vector2 MaxSize
        {
            get => _maxSize;
            set { _maxSize = value; OnLayoutChanged(); }
        }

        public Vector2 Anchor
        {
            get => _anchor;
            set { _anchor = value; OnLayoutChanged(); }
        }

        public Vector2 Pivot
        {
            get => _pivot;
            set { _pivot = value; OnLayoutChanged(); }
        }

        public Vector2 Margin
        {
            get => _margin;
            set { _margin = value; OnLayoutChanged(); }
        }

        public Vector2 Padding
        {
            get => _padding;
            set { _padding = value; OnLayoutChanged(); }
        }

        public Color BackgroundColor
        {
            get => _backgroundColor;
            set { _backgroundColor = value; }
        }

        public Color BorderColor
        {
            get => _borderColor;
            set { _borderColor = value; }
        }

        public float BorderWidth
        {
            get => _borderWidth;
            set { _borderWidth = value; }
        }

        public bool IsVisible
        {
            get => _isVisible;
            set { _isVisible = value; }
        }

        public bool IsEnabled
        {
            get => _isEnabled;
            set { _isEnabled = value; }
        }

        public bool IsInteractable
        {
            get => _isInteractable;
            set { _isInteractable = value; }
        }

        public string? Name
        {
            get => _name;
            set { _name = value; }
        }

        public UIElement? Parent
        {
            get => _parent;
            set { _parent = value; }
        }

        public IReadOnlyList<UIElement> Children => _children;

        public int SortingOrder
        {
            get => _sortingOrder;
            set { _sortingOrder = value; }
        }

        public string? Tooltip
        {
            get => _tooltip;
            set { _tooltip = value; }
        }

        public Rect Bounds => new Rect(_position, _size);

        public event EventHandler? LayoutChanged;
        public event EventHandler? Clicked;
        public event EventHandler? MouseEnter;
        public event EventHandler? MouseExit;
        public event EventHandler? MouseDown;
        public event EventHandler? MouseUp;
        public event EventHandler? GotFocus;
        public event EventHandler? LostFocus;

        protected UIElement()
        {
            _position = Vector2.Zero;
            _size = new Vector2(100, 30);
            _minSize = Vector2.Zero;
            _maxSize = new Vector2(float.MaxValue, float.MaxValue);
            _anchor = new Vector2(0.5f, 0.5f);
            _pivot = new Vector2(0.5f, 0.5f);
            _margin = Vector2.Zero;
            _padding = Vector2.Zero;
            _backgroundColor = Color.Clear;
            _borderColor = Color.Black;
            _borderWidth = 0;
            _isVisible = true;
            _isEnabled = true;
            _isInteractable = true;
            _children = new List<UIElement>();
            _sortingOrder = 0;
        }

        protected virtual void OnLayoutChanged()
        {
            foreach (var child in _children)
            {
                child.OnLayoutChanged();
            }
            LayoutChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual void AddChild(UIElement child)
        {
            if (child._parent != null)
            {
                child._parent._children.Remove(child);
            }
            child._parent = this;
            _children.Add(child);
            OnLayoutChanged();
        }

        public virtual void RemoveChild(UIElement child)
        {
            if (_children.Remove(child))
            {
                child._parent = null;
                OnLayoutChanged();
            }
        }

        public virtual void ClearChildren()
        {
            foreach (var child in _children)
            {
                child._parent = null;
            }
            _children.Clear();
            OnLayoutChanged();
        }

        public T? GetChild<T>(string name) where T : UIElement
        {
            foreach (var child in _children)
            {
                if (child.Name == name && child is T typed)
                    return typed;
            }
            return null;
        }

        public List<T> GetChildren<T>() where T : UIElement
        {
            return _children.OfType<T>().ToList();
        }

        public virtual bool ContainsPoint(Vector2 point)
        {
            return point.X >= _position.X && point.X <= _position.X + _size.X &&
                   point.Y >= _position.Y && point.Y <= _position.Y + _size.Y;
        }

        public virtual void Update(float deltaTime)
        {
            if (!_isVisible) return;

            foreach (var child in _children)
            {
                child.Update(deltaTime);
            }
        }

        public virtual void Draw(DrawingContext context)
        {
            if (!_isVisible) return;

            if (_backgroundColor.A > 0)
            {
                var brush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(_backgroundColor.GetAByte(), _backgroundColor.GetRByte(),
                        _backgroundColor.GetGByte(), _backgroundColor.GetBByte()));
                context.DrawRectangle(brush, null,
                    new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y));
            }

            if (_borderWidth > 0)
            {
                var pen = new System.Windows.Media.Pen(
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(_borderColor.GetAByte(), _borderColor.GetRByte(),
                            _borderColor.GetGByte(), _borderColor.GetBByte())), _borderWidth);
                context.DrawRectangle(null, pen,
                    new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y));
            }

            foreach (var child in _children.OrderBy(c => c.SortingOrder))
            {
                child.Draw(context);
            }
        }

        public virtual void OnClick()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnMouseEnter()
        {
            MouseEnter?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnMouseExit()
        {
            MouseExit?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnMouseDown()
        {
            MouseDown?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnMouseUp()
        {
            MouseUp?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnGotFocus()
        {
            GotFocus?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnLostFocus()
        {
            LostFocus?.Invoke(this, EventArgs.Empty);
        }

        public virtual void OnPointerEnter()
        {
        }

        public virtual void OnPointerExit()
        {
        }

        public virtual void OnPointerDown(Vector2 position)
        {
        }

        public virtual void OnPointerUp(Vector2 position)
        {
        }

        public virtual void OnPointerClick(Vector2 position)
        {
        }
    }

    public class UILabel : UIElement
    {
        private string _text;
        private string _fontFamily;
        private float _fontSize;
        private Color _textColor;
        private Alignment _horizontalAlignment;
        private Alignment _verticalAlignment;
        private bool _wordWrap;
        private bool _autoSize;
        private float _lineSpacing;
        private int _maxLines;

        public string Text
        {
            get => _text;
            set { _text = value; if (_autoSize) CalculateSize(); }
        }

        public string FontFamily
        {
            get => _fontFamily;
            set { _fontFamily = value; }
        }

        public float FontSize
        {
            get => _fontSize;
            set { _fontSize = value; if (_autoSize) CalculateSize(); }
        }

        public Color TextColor
        {
            get => _textColor;
            set { _textColor = value; }
        }

        public Alignment HorizontalTextAlignment
        {
            get => _horizontalAlignment;
            set { _horizontalAlignment = value; }
        }

        public Alignment VerticalTextAlignment
        {
            get => _verticalAlignment;
            set { _verticalAlignment = value; }
        }

        public bool WordWrap
        {
            get => _wordWrap;
            set { _wordWrap = value; }
        }

        public bool AutoSize
        {
            get => _autoSize;
            set { _autoSize = value; if (_autoSize) CalculateSize(); }
        }

        public float LineSpacing
        {
            get => _lineSpacing;
            set { _lineSpacing = value; }
        }

        public int MaxLines
        {
            get => _maxLines;
            set { _maxLines = value; }
        }

        public UILabel()
        {
            _text = "";
            _fontFamily = "Segoe UI";
            _fontSize = 14;
            _textColor = Color.Black;
            _horizontalAlignment = Alignment.Start;
            _verticalAlignment = Alignment.Center;
            _wordWrap = false;
            _autoSize = false;
            _lineSpacing = 1.2f;
            _maxLines = 0;
            _backgroundColor = Color.Clear;
        }

        public UILabel(string text)
        {
            _text = text;
            _fontFamily = "Segoe UI";
            _fontSize = 14;
            _textColor = Color.Black;
            _horizontalAlignment = Alignment.Start;
            _verticalAlignment = Alignment.Center;
            _wordWrap = false;
            _autoSize = false;
            _lineSpacing = 1.2f;
            _maxLines = 0;
            _backgroundColor = Color.Clear;
        }

        private void CalculateSize()
        {
            var typeface = new System.Windows.Media.Typeface(_fontFamily);
            var ft = new System.Windows.Media.FormattedText(
                _text,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                _fontSize,
                System.Windows.Media.Brushes.Black,
                1.0);

            _size = new Vector2(ft.Width + _padding.X * 2, ft.Height + _padding.Y * 2);
            _size = Vector2.Max(_minSize, Vector2.Min(_maxSize, _size));
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible || string.IsNullOrEmpty(_text))
                return;

            base.Draw(context);

            Vector2 textPosition = CalculateTextPosition();
            var brush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(_textColor.GetAByte(), _textColor.GetRByte(),
                    _textColor.GetGByte(), _textColor.GetBByte()));

            var typeface = new System.Windows.Media.Typeface(_fontFamily);
            var ft = new System.Windows.Media.FormattedText(
                _text,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                _fontSize,
                brush,
                1.0);

            if (_wordWrap && _size.X > 0)
            {
                ft.MaxTextWidth = _size.X - _padding.X * 2;
            }

            if (_maxLines > 0)
            {
                ft.MaxTextHeight = _fontSize * _maxLines * _lineSpacing;
            }

            context.DrawText(ft, new System.Windows.Point(textPosition.X, textPosition.Y));
        }

        private Vector2 CalculateTextPosition()
        {
            Vector2 result = _position + _padding;

            var typeface = new System.Windows.Media.Typeface(_fontFamily);
            var ft = new System.Windows.Media.FormattedText(
                _text,
                System.Globalization.CultureInfo.CurrentCulture,
                System.Windows.FlowDirection.LeftToRight,
                typeface,
                _fontSize,
                System.Windows.Media.Brushes.Black,
                1.0);

            float contentWidth = ft.Width;
            float contentHeight = ft.Height;

            switch (_horizontalAlignment)
            {
                case Alignment.Center:
                    result.X = _position.X + (_size.X - contentWidth) / 2;
                    break;
                case Alignment.End:
                    result.X = _position.X + _size.X - contentWidth - _padding.X;
                    break;
                case Alignment.Stretch:
                    result.X = _position.X + _padding.X;
                    break;
            }

            switch (_verticalAlignment)
            {
                case Alignment.Center:
                    result.Y = _position.Y + (_size.Y - contentHeight) / 2;
                    break;
                case Alignment.End:
                    result.Y = _position.Y + _size.Y - contentHeight - _padding.Y;
                    break;
                case Alignment.Stretch:
                    result.Y = _position.Y + _padding.Y;
                    break;
            }

            return result;
        }
    }

    public class UIButton : UIElement
    {
        private string _text;
        private UILabel _label;
        private Color _normalColor;
        private Color _hoverColor;
        private Color _pressedColor;
        private Color _disabledColor;
        private Color _normalTextColor;
        private Color _hoverTextColor;
        private Color _pressedTextColor;
        private Color _disabledTextColor;
        private bool _isPressed;
        private bool _isHovered;
        private System.Windows.Media.ImageSource? _backgroundImage;
        private System.Windows.Media.ImageSource? _hoverImage;
        private System.Windows.Media.ImageSource? _pressedImage;
        private float _transitionDuration;

        public string Text
        {
            get => _text;
            set { _text = value; _label.Text = value; }
        }

        public Color NormalColor
        {
            get => _normalColor;
            set { _normalColor = value; }
        }

        public Color HoverColor
        {
            get => _hoverColor;
            set { _hoverColor = value; }
        }

        public Color PressedColor
        {
            get => _pressedColor;
            set { _pressedColor = value; }
        }

        public Color DisabledColor
        {
            get => _disabledColor;
            set { _disabledColor = value; }
        }

        public Color NormalTextColor
        {
            get => _normalTextColor;
            set { _normalTextColor = value; }
        }

        public Color HoverTextColor
        {
            get => _hoverTextColor;
            set { _hoverTextColor = value; }
        }

        public Color PressedTextColor
        {
            get => _pressedTextColor;
            set { _pressedTextColor = value; }
        }

        public Color DisabledTextColor
        {
            get => _disabledTextColor;
            set { _disabledTextColor = value; }
        }

        public System.Windows.Media.ImageSource? BackgroundImage
        {
            get => _backgroundImage;
            set { _backgroundImage = value; }
        }

        public float TransitionDuration
        {
            get => _transitionDuration;
            set { _transitionDuration = value; }
        }

        public event EventHandler? ButtonPressed;
        public event EventHandler? ButtonReleased;

        public UIButton()
        {
            _text = "Button";
            _label = new UILabel(_text) { Pivot = new Vector2(0.5f, 0.5f) };
            _normalColor = new Color(0.9f, 0.9f, 0.9f);
            _hoverColor = new Color(0.85f, 0.85f, 0.85f);
            _pressedColor = new Color(0.7f, 0.7f, 0.7f);
            _disabledColor = new Color(0.6f, 0.6f, 0.6f);
            _normalTextColor = Color.Black;
            _hoverTextColor = Color.Black;
            _pressedTextColor = Color.Black;
            _disabledTextColor = new Color(0.5f, 0.5f, 0.5f);
            _isPressed = false;
            _isHovered = false;
            _transitionDuration = 0.1f;
            _size = new Vector2(120, 40);

            _label.HorizontalTextAlignment = Alignment.Center;
            _label.VerticalTextAlignment = Alignment.Center;
            AddChild(_label);

            UpdateAppearance();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            _label.Position = _position;
            _label.Size = _size;
        }

        public void UpdateAppearance()
        {
            if (!_isEnabled)
            {
                _backgroundColor = _disabledColor;
                _label.TextColor = _disabledTextColor;
            }
            else if (_isPressed)
            {
                _backgroundColor = _pressedColor;
                _label.TextColor = _pressedTextColor;
            }
            else if (_isHovered)
            {
                _backgroundColor = _hoverColor;
                _label.TextColor = _hoverTextColor;
            }
            else
            {
                _backgroundColor = _normalColor;
                _label.TextColor = _normalTextColor;
            }
        }

        public override void OnPointerEnter()
        {
            if (!_isEnabled) return;
            _isHovered = true;
            UpdateAppearance();
            base.OnPointerEnter();
        }

        public override void OnPointerExit()
        {
            _isHovered = false;
            _isPressed = false;
            UpdateAppearance();
            base.OnPointerExit();
        }

        public override void OnPointerDown(Vector2 position)
        {
            if (!_isEnabled || !_isInteractable) return;
            _isPressed = true;
            UpdateAppearance();
            OnMouseDown();
            ButtonPressed?.Invoke(this, EventArgs.Empty);
        }

        public override void OnPointerUp(Vector2 position)
        {
            if (!_isEnabled || !_isInteractable) return;

            if (_isPressed && ContainsPoint(position))
            {
                OnClick();
            }

            _isPressed = false;
            UpdateAppearance();
            OnMouseUp();
            ButtonReleased?.Invoke(this, EventArgs.Empty);
        }
    }

    public class UIImage : UIElement
    {
        private System.Windows.Media.ImageSource? _source;
        private Rect _sourceRect;
        private Color _tintColor;
        private float _fillAmount;
        private bool _preserveAspect;

        public System.Windows.Media.ImageSource? Source
        {
            get => _source;
            set { _source = value; }
        }

        public Rect SourceRect
        {
            get => _sourceRect;
            set { _sourceRect = value; }
        }

        public Color TintColor
        {
            get => _tintColor;
            set { _tintColor = value; }
        }

        public float FillAmount
        {
            get => _fillAmount;
            set { _fillAmount = Math.Clamp(value, 0f, 1f); }
        }

        public bool PreserveAspect
        {
            get => _preserveAspect;
            set { _preserveAspect = value; }
        }

        public UIImage()
        {
            _sourceRect = new Rect(0, 0, 1, 1);
            _tintColor = Color.White;
            _fillAmount = 1f;
            _preserveAspect = true;
            _backgroundColor = Color.Clear;
        }

        public UIImage(System.Windows.Media.ImageSource source)
        {
            _source = source;
            _sourceRect = new Rect(0, 0, 1, 1);
            _tintColor = Color.White;
            _fillAmount = 1f;
            _preserveAspect = true;
            _backgroundColor = Color.Clear;
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible || _source == null)
                return;

            base.Draw(context);

            if (_fillAmount < 1f)
            {
                var geometry = new System.Windows.Media.RectangleGeometry(
                    new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y * _fillAmount));
                context.PushClip(geometry);
            }

            var brush = new System.Windows.Media.ImageBrush(_source);
            if (_tintColor != Color.White)
            {
                brush = new System.Windows.Media.ImageBrush(_source)
                {
                    Color = System.Windows.Media.Color.FromArgb(_tintColor.GetAByte(), _tintColor.GetRByte(),
                        _tintColor.GetGByte(), _tintColor.GetBByte())
                };
            }

            if (_preserveAspect)
            {
                var imageWidth = _source.Width;
                var imageHeight = _source.Height;
                float aspectRatio = imageWidth / imageHeight;
                float rectAspectRatio = _size.X / _size.Y;

                float drawWidth, drawHeight, offsetX = 0, offsetY = 0;

                if (aspectRatio > rectAspectRatio)
                {
                    drawHeight = _size.Y;
                    drawWidth = drawHeight * aspectRatio;
                    offsetX = (_size.X - drawWidth) / 2;
                }
                else
                {
                    drawWidth = _size.X;
                    drawHeight = drawWidth / aspectRatio;
                    offsetY = (_size.Y - drawHeight) / 2;
                }

                context.DrawImage(_source,
                    new System.Windows.Rect(_position.X + offsetX, _position.Y + offsetY, drawWidth, drawHeight));
            }
            else
            {
                context.DrawImage(_source,
                    new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y));
            }

            if (_fillAmount < 1f)
            {
                context.Pop();
            }
        }
    }

    public class UITextField : UIElement
    {
        private string _text;
        private string _placeholder;
        private int _maxLength;
        private bool _isPassword;
        private char _passwordChar;
        private bool _isFocused;
        private int _caretPosition;
        private int _selectionStart;
        private int _selectionLength;
        private Color _caretColor;
        private Color _selectionColor;
        private Color _placeholderColor;
        private string? _fontFamily;
        private float _fontSize;
        private bool _readOnly;

        public string Text
        {
            get => _text;
            set
            {
                _text = value.Length > _maxLength ? value.Substring(0, _maxLength) : value;
                if (_caretPosition > _text.Length) _caretPosition = _text.Length;
            }
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

        public bool IsPassword
        {
            get => _isPassword;
            set { _isPassword = value; }
        }

        public char PasswordChar
        {
            get => _passwordChar;
            set { _passwordChar = value; }
        }

        public bool IsFocused
        {
            get => _isFocused;
        }

        public Color CaretColor
        {
            get => _caretColor;
            set { _caretColor = value; }
        }

        public Color SelectionColor
        {
            get => _selectionColor;
            set { _selectionColor = value; }
        }

        public Color PlaceholderColor
        {
            get => _placeholderColor;
            set { _placeholderColor = value; }
        }

        public bool ReadOnly
        {
            get => _readOnly;
            set { _readOnly = value; }
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

        public event EventHandler<string>? TextChanged;
        public event EventHandler? EditingEnded;

        public UITextField()
        {
            _text = "";
            _placeholder = "";
            _maxLength = 256;
            _isPassword = false;
            _passwordChar = '*';
            _isFocused = false;
            _caretPosition = 0;
            _selectionStart = 0;
            _selectionLength = 0;
            _caretColor = Color.Black;
            _selectionColor = new Color(0.3f, 0.5f, 0.9f, 0.5f);
            _placeholderColor = new Color(0.5f, 0.5f, 0.5f);
            _fontFamily = "Segoe UI";
            _fontSize = 14;
            _readOnly = false;
            _backgroundColor = Color.White;
            _borderColor = new Color(0.7f, 0.7f, 0.7f);
            _borderWidth = 1;
            _size = new Vector2(200, 30);
            _padding = new Vector2(5);
        }

        public void Focus()
        {
            if (!_isFocused)
            {
                _isFocused = true;
                OnGotFocus();
            }
        }

        public void Unfocus()
        {
            if (_isFocused)
            {
                _isFocused = false;
                _selectionStart = 0;
                _selectionLength = 0;
                OnLostFocus();
                EditingEnded?.Invoke(this, EventArgs.Empty);
            }
        }

        public void InsertText(string text)
        {
            if (_readOnly) return;

            if (_selectionLength > 0)
            {
                _text = _text.Remove(_selectionStart, _selectionLength);
                _caretPosition = _selectionStart;
            }

            string remainingSpace = _text.Substring(_caretPosition);
            int spaceAvailable = _maxLength - _text.Length;
            int charsToInsert = Math.Min(text.Length, spaceAvailable);

            if (charsToInsert > 0)
            {
                _text = _text.Insert(_caretPosition, text.Substring(0, charsToInsert));
                _caretPosition += charsToInsert;
            }

            _selectionStart = _caretPosition;
            _selectionLength = 0;
            TextChanged?.Invoke(this, _text);
        }

        public void DeleteText(int count)
        {
            if (_readOnly) return;

            if (_selectionLength > 0)
            {
                _text = _text.Remove(_selectionStart, _selectionLength);
                _caretPosition = _selectionStart;
                _selectionLength = 0;
            }
            else if (_caretPosition < _text.Length)
            {
                int toDelete = Math.Min(count, _text.Length - _caretPosition);
                _text = _text.Remove(_caretPosition, toDelete);
            }

            TextChanged?.Invoke(this, _text);
        }

        public void MoveCaret(int offset)
        {
            _caretPosition = Math.Clamp(_caretPosition + offset, 0, _text.Length);
            _selectionStart = _caretPosition;
            _selectionLength = 0;
        }

        public override void OnPointerDown(Vector2 position)
        {
            base.OnPointerDown(position);
            Focus();
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible)
                return;

            base.Draw(context);

            string displayText = _isPassword ? new string(_passwordChar, _text.Length) : _text;
            Color textColor = string.IsNullOrEmpty(displayText) ? _placeholderColor : Color.Black;

            if (!string.IsNullOrEmpty(displayText))
            {
                var typeface = new System.Windows.Media.Typeface(_fontFamily ?? "Segoe UI");
                var brush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(textColor.GetAByte(), textColor.GetRByte(),
                        textColor.GetGByte(), textColor.GetBByte()));

                var formattedText = new System.Windows.Media.FormattedText(
                    displayText,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    _fontSize,
                    brush,
                    1.0);

                context.DrawText(formattedText,
                    new System.Windows.Point(_position.X + _padding.X, _position.Y + (_size.Y - formattedText.Height) / 2));
            }
            else if (!string.IsNullOrEmpty(_placeholder))
            {
                var typeface = new System.Windows.Media.Typeface(_fontFamily ?? "Segoe UI");
                var brush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(_placeholderColor.GetAByte(), _placeholderColor.GetRByte(),
                        _placeholderColor.GetGByte(), _placeholderColor.GetBByte()));

                var formattedText = new System.Windows.Media.FormattedText(
                    _placeholder,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    _fontSize,
                    brush,
                    1.0);

                context.DrawText(formattedText,
                    new System.Windows.Point(_position.X + _padding.X, _position.Y + (_size.Y - formattedText.Height) / 2));
            }

            if (_isFocused && _caretPosition <= displayText.Length)
            {
                var typeface = new System.Windows.Media.Typeface(_fontFamily ?? "Segoe UI");
                var measureText = new System.Windows.Media.FormattedText(
                    displayText.Length > 0 ? displayText.Substring(0, _caretPosition) : "",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    _fontSize,
                    System.Windows.Media.Brushes.Black,
                    1.0);

                float caretX = _position.X + _padding.X + measureText.Width;
                var caretPen = new System.Windows.Media.Pen(
                    new System.Windows.Media.SolidColorBrush(
                        System.Windows.Media.Color.FromArgb(_caretColor.GetAByte(), _caretColor.GetRByte(),
                            _caretColor.GetGByte(), _caretColor.GetBByte())), 1);

                context.DrawLine(caretPen,
                    new System.Windows.Point(caretX, _position.Y + _padding.Y),
                    new System.Windows.Point(caretX, _position.Y + _size.Y - _padding.Y));
            }
        }
    }

    public class UISlider : UIElement
    {
        private float _value;
        private float _minValue;
        private float _maxValue;
        private float _step;
        private Color _trackColor;
        private Color _fillColor;
        private Color _thumbColor;
        private float _thumbSize;
        private float _trackHeight;
        private bool _isDragging;
        private Vector2 _thumbPosition;

        public float Value
        {
            get => _value;
            set
            {
                float newValue = Math.Clamp(value, _minValue, _maxValue);
                if (_step > 0)
                {
                    newValue = (float)Math.Round(newValue / _step) * _step;
                }
                if (Math.Abs(_value - newValue) > 0.0001f)
                {
                    _value = newValue;
                    ValueChanged?.Invoke(this, _value);
                }
            }
        }

        public float MinValue
        {
            get => _minValue;
            set
            {
                _minValue = value;
                if (_value < _minValue) _value = _minValue;
            }
        }

        public float MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                if (_value > _maxValue) _value = _maxValue;
            }
        }

        public float Step
        {
            get => _step;
            set { _step = Math.Max(0, value); }
        }

        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; }
        }

        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; }
        }

        public Color ThumbColor
        {
            get => _thumbColor;
            set { _thumbColor = value; }
        }

        public float ThumbSize
        {
            get => _thumbSize;
            set { _thumbSize = Math.Max(4, value); }
        }

        public float TrackHeight
        {
            get => _trackHeight;
            set { _trackHeight = Math.Max(2, value); }
        }

        public event EventHandler<float>? ValueChanged;

        public UISlider()
        {
            _value = 0.5f;
            _minValue = 0;
            _maxValue = 1;
            _step = 0;
            _trackColor = new Color(0.7f, 0.7f, 0.7f);
            _fillColor = new Color(0.2f, 0.4f, 0.8f);
            _thumbColor = Color.White;
            _thumbSize = 16;
            _trackHeight = 6;
            _isDragging = false;
            _size = new Vector2(200, 30);
            CalculateThumbPosition();
        }

        private void CalculateThumbPosition()
        {
            float t = (_value - _minValue) / (_maxValue - _minValue);
            float trackWidth = _size.X - _thumbSize;
            _thumbPosition = new Vector2(_position.X + t * trackWidth, _position.Y + (_size.Y - _thumbSize) / 2);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            CalculateThumbPosition();
        }

        public override void OnPointerDown(Vector2 position)
        {
            base.OnPointerDown(position);
            _isDragging = true;
            UpdateValueFromPosition(position);
        }

        public override void OnPointerUp(Vector2 position)
        {
            base.OnPointerUp(position);
            _isDragging = false;
        }

        public override void OnPointerMove(Vector2 position)
        {
            base.OnPointerMove(position);
            if (_isDragging)
            {
                UpdateValueFromPosition(position);
            }
        }

        private void UpdateValueFromPosition(Vector2 position)
        {
            float trackWidth = _size.X - _thumbSize;
            float t = Math.Clamp((position.X - _position.X - _thumbSize / 2) / trackWidth, 0, 1);
            Value = _minValue + t * (_maxValue - _minValue);
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible)
                return;

            base.Draw(context);

            float trackY = _position.Y + (_size.Y - _trackHeight) / 2;
            float trackWidth = _size.X;
            float fillWidth = (_value - _minValue) / (_maxValue - _minValue) * trackWidth;

            var trackBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(_trackColor.GetAByte(), _trackColor.GetRByte(),
                    _trackColor.GetGByte(), _trackColor.GetBByte()));

            context.DrawRectangle(trackBrush, null,
                new System.Windows.Rect(_position.X, trackY, trackWidth, _trackHeight));

            var fillBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(_fillColor.GetAByte(), _fillColor.GetRByte(),
                    _fillColor.GetGByte(), _fillColor.GetBByte()));

            context.DrawRectangle(fillBrush, null,
                new System.Windows.Rect(_position.X, trackY, fillWidth, _trackHeight));

            CalculateThumbPosition();
            var thumbBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(_thumbColor.GetAByte(), _thumbColor.GetRByte(),
                    _thumbColor.GetGByte(), _thumbColor.GetBByte()));

            context.DrawEllipse(thumbBrush, null,
                new System.Windows.Point(_thumbPosition.X + _thumbSize / 2, _thumbPosition.Y + _thumbSize / 2),
                _thumbSize / 2, _thumbSize / 2);
        }
    }

    public class UIToggle : UIElement
    {
        private bool _isOn;
        private Color _onColor;
        private Color _offColor;
        private float _toggleWidth;
        private float _toggleHeight;
        private float _thumbSize;
        private Vector2 _thumbPosition;
        private bool _isDragging;
        private UILabel? _label;

        public bool IsOn
        {
            get => _isOn;
            set
            {
                if (_isOn != value)
                {
                    _isOn = value;
                    Toggled?.Invoke(this, _isOn);
                }
            }
        }

        public Color OnColor
        {
            get => _onColor;
            set { _onColor = value; }
        }

        public Color OffColor
        {
            get => _offColor;
            set { _offColor = value; }
        }

        public float ToggleWidth
        {
            get => _toggleWidth;
            set { _toggleWidth = Math.Max(20, value); }
        }

        public float ToggleHeight
        {
            get => _toggleHeight;
            set { _toggleHeight = Math.Max(10, value); }
        }

        public string? Label
        {
            get => _label?.Text;
            set
            {
                if (_label == null && !string.IsNullOrEmpty(value))
                {
                    _label = new UILabel(value);
                    AddChild(_label);
                }
                else if (_label != null)
                {
                    _label.Text = value ?? "";
                }
            }
        }

        public event EventHandler<bool>? Toggled;

        public UIToggle()
        {
            _isOn = false;
            _onColor = new Color(0.2f, 0.7f, 0.3f);
            _offColor = new Color(0.6f, 0.6f, 0.6f);
            _toggleWidth = 50;
            _toggleHeight = 26;
            _thumbSize = 22;
            _isDragging = false;
            _size = new Vector2(_toggleWidth + 50, _toggleHeight);
            UpdateThumbPosition();
        }

        private void UpdateThumbPosition()
        {
            float padding = (_toggleHeight - _thumbSize) / 2;
            float targetX = _isOn ? _position.X + _toggleWidth - _thumbSize - padding : _position.X + padding;
            _thumbPosition = new Vector2(targetX, _position.Y + padding);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdateThumbPosition();

            if (_label != null)
            {
                _label.Position = new Vector2(_position.X + _toggleWidth + 10, _position.Y);
                _label.Size = new Vector2(_size.X - _toggleWidth - 10, _size.Y);
            }
        }

        public override void OnPointerDown(Vector2 position)
        {
            base.OnPointerDown(position);
            _isDragging = true;
        }

        public override void OnPointerUp(Vector2 position)
        {
            base.OnPointerUp(position);
            _isDragging = false;

            if (ContainsPoint(position))
            {
                IsOn = !_isOn;
            }
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible)
                return;

            base.Draw(context);

            float toggleY = _position.Y + (_size.Y - _toggleHeight) / 2;
            float trackPadding = (_toggleHeight - _thumbSize) / 2;

            var trackBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb((_isOn ? _onColor : _offColor).GetAByte(),
                    (_isOn ? _onColor : _offColor).GetRByte(),
                    (_isOn ? _onColor : _offColor).GetGByte(),
                    (_isOn ? _onColor : _offColor).GetBByte()));

            context.DrawRoundedRectangle(trackBrush, null,
                new System.Windows.Rect(_position.X, toggleY, _toggleWidth, _toggleHeight),
                _toggleHeight / 2, _toggleHeight / 2);

            UpdateThumbPosition();
            var thumbBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(_thumbColor.GetAByte(), _thumbColor.GetRByte(),
                    _thumbColor.GetGByte(), _thumbColor.GetBByte()));

            context.DrawEllipse(thumbBrush, null,
                new System.Windows.Point(_thumbPosition.X + _thumbSize / 2, _thumbPosition.Y + _thumbSize / 2),
                _thumbSize / 2, _thumbSize / 2);
        }
    }

    public class UIProgressBar : UIElement
    {
        private float _value;
        private float _minValue;
        private float _maxValue;
        private Color _trackColor;
        private Color _fillColor;
        private bool _showLabel;
        private string _labelFormat;

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

        public Color TrackColor
        {
            get => _trackColor;
            set { _trackColor = value; }
        }

        public Color FillColor
        {
            get => _fillColor;
            set { _fillColor = value; }
        }

        public bool ShowLabel
        {
            get => _showLabel;
            set { _showLabel = value; }
        }

        public string LabelFormat
        {
            get => _labelFormat;
            set { _labelFormat = value; }
        }

        public float FillPercent => (_value - _minValue) / (_maxValue - _minValue);

        public UIProgressBar()
        {
            _value = 0.5f;
            _minValue = 0;
            _maxValue = 1;
            _trackColor = new Color(0.3f, 0.3f, 0.3f);
            _fillColor = new Color(0.2f, 0.6f, 0.9f);
            _showLabel = true;
            _labelFormat = "{0:P0}";
            _size = new Vector2(200, 20);
            _backgroundColor = Color.Clear;
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible)
                return;

            base.Draw(context);

            var trackBrush = new System.Windows.Media.SolidColorBrush(
                System.Windows.Media.Color.FromArgb(_trackColor.GetAByte(), _trackColor.GetRByte(),
                    _trackColor.GetGByte(), _trackColor.GetBByte()));

            context.DrawRoundedRectangle(trackBrush, null,
                new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y),
                _size.Y / 2, _size.Y / 2);

            float fillWidth = _size.X * FillPercent;
            if (fillWidth > 0)
            {
                var fillBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(_fillColor.GetAByte(), _fillColor.GetRByte(),
                        _fillColor.GetGByte(), _fillColor.GetBByte()));

                context.DrawRoundedRectangle(fillBrush, null,
                    new System.Windows.Rect(_position.X, _position.Y, fillWidth, _size.Y),
                    _size.Y / 2, _size.Y / 2);
            }

            if (_showLabel)
            {
                string label = string.Format(_labelFormat, FillPercent);
                var typeface = new System.Windows.Media.Typeface("Segoe UI");
                var brush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(255, 255, 255, 255));

                var formattedText = new System.Windows.Media.FormattedText(
                    label,
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    typeface,
                    12,
                    brush,
                    1.0);

                context.DrawText(formattedText,
                    new System.Windows.Point(_position.X + (_size.X - formattedText.Width) / 2,
                        _position.Y + (_size.Y - formattedText.Height) / 2));
            }
        }
    }

    public class UIPanel : UIElement
    {
        private Orientation _orientation;
        private Alignment _childAlignment;
        private float _spacing;
        private bool _fitToContent;
        private Vector2 _contentSize;

        public Orientation PanelOrientation
        {
            get => _orientation;
            set { _orientation = value; OnLayoutChanged(); }
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

        public bool FitToContent
        {
            get => _fitToContent;
            set { _fitToContent = value; }
        }

        public Vector2 ContentSize => _contentSize;

        public UIPanel()
        {
            _orientation = Orientation.Vertical;
            _childAlignment = Alignment.Start;
            _spacing = 5;
            _fitToContent = false;
            _contentSize = Vector2.Zero;
            _backgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            _borderColor = new Color(0.3f, 0.3f, 0.3f);
            _borderWidth = 1;
        }

        public override void AddChild(UIElement child)
        {
            base.AddChild(child);
            if (_fitToContent)
            {
                CalculateContentSize();
            }
            OnLayoutChanged();
        }

        public override void RemoveChild(UIElement child)
        {
            base.RemoveChild(child);
            if (_fitToContent)
            {
                CalculateContentSize();
            }
            OnLayoutChanged();
        }

        protected override void OnLayoutChanged()
        {
            base.OnLayoutChanged();
            LayoutChildren();
        }

        private void LayoutChildren()
        {
            if (_children.Count == 0)
            {
                _contentSize = Vector2.Zero;
                return;
            }

            float totalSize = 0;
            float maxCrossSize = 0;

            foreach (var child in _children)
            {
                Vector2 childSize = child.Size;
                if (_orientation == Orientation.Horizontal)
                {
                    totalSize += childSize.X + (totalSize > 0 ? _spacing : 0);
                    maxCrossSize = Math.Max(maxCrossSize, childSize.Y);
                }
                else
                {
                    totalSize += childSize.Y + (totalSize > 0 ? _spacing : 0);
                    maxCrossSize = Math.Max(maxCrossSize, childSize.X);
                }
            }

            if (_orientation == Orientation.Horizontal)
            {
                totalSize -= _spacing;
            }
            else
            {
                totalSize -= _spacing;
            }

            Vector2 offset = _position;
            float currentPos = 0;

            foreach (var child in _children)
            {
                Vector2 childSize = child.Size;
                Vector2 childPos = offset;

                if (_orientation == Orientation.Horizontal)
                {
                    childPos = new Vector2(offset.X + currentPos, offset.Y);
                    if (_childAlignment == Alignment.Center)
                    {
                        childPos.Y = offset.Y + (maxCrossSize - childSize.Y) / 2;
                    }
                    else if (_childAlignment == Alignment.End)
                    {
                        childPos.Y = offset.Y + maxCrossSize - childSize.Y;
                    }

                    currentPos += childSize.X + _spacing;
                }
                else
                {
                    childPos = new Vector2(offset.X, offset.Y + currentPos);
                    if (_childAlignment == Alignment.Center)
                    {
                        childPos.X = offset.X + (maxCrossSize - childSize.X) / 2;
                    }
                    else if (_childAlignment == Alignment.End)
                    {
                        childPos.X = offset.X + maxCrossSize - childSize.X;
                    }

                    currentPos += childSize.Y + _spacing;
                }

                child.Position = childPos;
            }

            if (_orientation == Orientation.Horizontal)
            {
                _contentSize = new Vector2(totalSize, maxCrossSize);
            }
            else
            {
                _contentSize = new Vector2(maxCrossSize, totalSize);
            }
        }

        private void CalculateContentSize()
        {
            if (_children.Count == 0)
            {
                _contentSize = Vector2.Zero;
                return;
            }

            float width = 0;
            float height = 0;

            foreach (var child in _children)
            {
                width = Math.Max(width, child.Position.X - _position.X + child.Size.X);
                height = Math.Max(height, child.Position.Y - _position.Y + child.Size.Y);
            }

            _contentSize = new Vector2(width, height);
        }
    }

    public class UIScrollView : UIElement
    {
        private UIPanel _content;
        private Vector2 _scrollPosition;
        private Vector2 _contentSize;
        private bool _showHorizontalScrollbar;
        private bool _showVerticalScrollbar;
        private float _scrollSensitivity;
        private UISlider? _horizontalScrollbar;
        private UISlider? _verticalScrollbar;
        private Rect _viewport;

        public UIPanel Content => _content;
        public Vector2 ScrollPosition
        {
            get => _scrollPosition;
            set { _scrollPosition = value; UpdateContentPosition(); }
        }

        public bool ShowHorizontalScrollbar
        {
            get => _showHorizontalScrollbar;
            set { _showHorizontalScrollbar = value; }
        }

        public bool ShowVerticalScrollbar
        {
            get => _showVerticalScrollbar;
            set { _showVerticalScrollbar = value; }
        }

        public float ScrollSensitivity
        {
            get => _scrollSensitivity;
            set { _scrollSensitivity = value; }
        }

        public UIScrollView()
        {
            _content = new UIPanel();
            _scrollPosition = Vector2.Zero;
            _contentSize = Vector2.Zero;
            _showHorizontalScrollbar = true;
            _showVerticalScrollbar = true;
            _scrollSensitivity = 50f;
            _backgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.9f);
            _borderColor = new Color(0.3f, 0.3f, 0.3f);
            _borderWidth = 1;

            _content.Parent = this;
        }

        public void AddContentChild(UIElement child)
        {
            _content.AddChild(child);
            UpdateContentSize();
        }

        public void RemoveContentChild(UIElement child)
        {
            _content.RemoveChild(child);
            UpdateContentSize();
        }

        private void UpdateContentSize()
        {
            _contentSize = _content.ContentSize;
        }

        private void UpdateContentPosition()
        {
            _content.Position = new Vector2(
                _position.X - _scrollPosition.X + _padding.X,
                _position.Y - _scrollPosition.Y + _padding.Y);
        }

        public void ScrollToTop()
        {
            _scrollPosition.Y = 0;
        }

        public void ScrollToBottom()
        {
            _scrollPosition.Y = Math.Max(0, _contentSize.Y - _size.Y);
        }

        public void ScrollToLeft()
        {
            _scrollPosition.X = 0;
        }

        public void ScrollToRight()
        {
            _scrollPosition.X = Math.Max(0, _contentSize.X - _size.X);
        }

        public void ScrollBy(Vector2 delta)
        {
            _scrollPosition += delta * _scrollSensitivity;
            _scrollPosition.X = Math.Max(0, Math.Min(_scrollPosition.X, Math.Max(0, _contentSize.X - _size.X)));
            _scrollPosition.Y = Math.Max(0, Math.Min(_scrollPosition.Y, Math.Max(0, _contentSize.Y - _size.Y)));
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdateContentSize();
            UpdateContentPosition();
            _content.Update(deltaTime);
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible)
                return;

            var clipGeometry = new System.Windows.Media.RectangleGeometry(
                new System.Windows.Rect(_position.X, _position.Y, _size.X, _size.Y));
            context.PushClip(clipGeometry);

            _content.Draw(context);

            context.Pop();
        }
    }

    public class UIDropdown : UIElement
    {
        private List<string> _options;
        private int _selectedIndex;
        private bool _isOpen;
        private UILabel _selectedLabel;
        private UIPanel _optionsPanel;
        private List<UILabel> _optionLabels;
        private float _optionHeight;
        private Color _optionHoverColor;
        private Color _optionSelectedColor;

        public List<string> Options => _options;
        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= -1 && value < _options.Count)
                {
                    _selectedIndex = value;
                    UpdateSelectedLabel();
                    SelectionChanged?.Invoke(this, _selectedIndex);
                }
            }
        }

        public string? SelectedOption => _selectedIndex >= 0 && _selectedIndex < _options.Count
            ? _options[_selectedIndex]
            : null;

        public Color OptionHoverColor
        {
            get => _optionHoverColor;
            set { _optionHoverColor = value; }
        }

        public Color OptionSelectedColor
        {
            get => _optionSelectedColor;
            set { _optionSelectedColor = value; }
        }

        public event EventHandler<int>? SelectionChanged;
        public event EventHandler<int>? DropdownOpened;
        public event EventHandler<int>? DropdownClosed;

        public UIDropdown()
        {
            _options = new List<string>();
            _selectedIndex = -1;
            _isOpen = false;
            _optionLabels = new List<UILabel>();
            _optionHeight = 30;
            _optionHoverColor = new Color(0.3f, 0.3f, 0.3f);
            _optionSelectedColor = new Color(0.2f, 0.4f, 0.8f);
            _size = new Vector2(200, 30);

            _selectedLabel = new UILabel("Select...");
            _selectedLabel.Pivot = Vector2.Zero;
            _selectedLabel.BackgroundColor = Color.White;
            _selectedLabel.BorderColor = new Color(0.7f, 0.7f, 0.7f);
            _selectedLabel.BorderWidth = 1;
            AddChild(_selectedLabel);

            _optionsPanel = new UIPanel();
            _optionsPanel.BackgroundColor = Color.White;
            _optionsPanel.BorderColor = new Color(0.5f, 0.5f, 0.5f);
            _optionsPanel.BorderWidth = 1;
            _optionsPanel.IsVisible = false;
            _optionsPanel.PanelOrientation = Orientation.Vertical;
            AddChild(_optionsPanel);
        }

        public void AddOption(string option)
        {
            _options.Add(option);
            CreateOptionLabel(_options.Count - 1, option);
            UpdateLayout();
        }

        public void RemoveOption(int index)
        {
            if (index < 0 || index >= _options.Count)
                return;

            _options.RemoveAt(index);
            RemoveOptionLabel(index);
            UpdateLayout();

            if (_selectedIndex >= _options.Count)
            {
                _selectedIndex = _options.Count - 1;
                UpdateSelectedLabel();
            }
        }

        public void ClearOptions()
        {
            _options.Clear();
            ClearOptionLabels();
            UpdateLayout();
            _selectedIndex = -1;
            UpdateSelectedLabel();
        }

        private void CreateOptionLabel(int index, string text)
        {
            var label = new UILabel(text);
            label.Size = new Vector2(_size.X, _optionHeight);
            label.BackgroundColor = Color.White;
            label.MouseEnter += (s, e) => label.BackgroundColor = _optionHoverColor;
            label.MouseExit += (s, e) => label.BackgroundColor = Color.White;
            label.MouseUp += (s, e) => SelectOption(index);

            _optionLabels.Insert(index, label);
            _optionsPanel.AddChild(label);
        }

        private void RemoveOptionLabel(int index)
        {
            if (index < 0 || index >= _optionLabels.Count)
                return;

            var label = _optionLabels[index];
            _optionLabels.RemoveAt(index);
            _optionsPanel.RemoveChild(label);
        }

        private void ClearOptionLabels()
        {
            foreach (var label in _optionLabels)
            {
                _optionsPanel.RemoveChild(label);
            }
            _optionLabels.Clear();
        }

        private void UpdateLayout()
        {
            _selectedLabel.Size = _size;
            _optionsPanel.Size = new Vector2(_size.X, _options.Count * _optionHeight);
            _optionsPanel.Position = new Vector2(_position.X, _position.Y + _size.Y);

            for (int i = 0; i < _optionLabels.Count; i++)
            {
                _optionLabels[i].Size = new Vector2(_size.X, _optionHeight);
            }
        }

        private void UpdateSelectedLabel()
        {
            _selectedLabel.Text = SelectedOption ?? "Select...";
        }

        private void SelectOption(int index)
        {
            _selectedIndex = index;
            _isOpen = false;
            _optionsPanel.IsVisible = false;
            UpdateSelectedLabel();
        }

        private void ToggleDropdown()
        {
            _isOpen = !_isOpen;
            _optionsPanel.IsVisible = _isOpen;

            if (_isOpen)
            {
                DropdownOpened?.Invoke(this, _selectedIndex);
            }
            else
            {
                DropdownClosed?.Invoke(this, _selectedIndex);
            }
        }

        public override void OnPointerDown(Vector2 position)
        {
            base.OnPointerDown(position);

            if (_selectedLabel.ContainsPoint(position) || ContainsPoint(position))
            {
                ToggleDropdown();
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            UpdateLayout();
        }
    }

    public class UICanvas : UIElement
    {
        private readonly List<UIElement> _sortedChildren;
        private bool _sortDirty;

        public UICanvas()
        {
            _sortedChildren = new List<UIElement>();
            _sortDirty = true;
        }

        public override void AddChild(UIElement child)
        {
            base.AddChild(child);
            _sortDirty = true;
        }

        public override void RemoveChild(UIElement child)
        {
            base.RemoveChild(child);
            _sortDirty = true;
        }

        public override void ClearChildren()
        {
            base.ClearChildren();
            _sortedChildren.Clear();
        }

        private void SortChildrenIfNeeded()
        {
            if (!_sortDirty)
                return;

            _sortedChildren.Clear();
            _sortedChildren.AddRange(_children);
            _sortedChildren.Sort((a, b) => a.SortingOrder.CompareTo(b.SortingOrder));
            _sortDirty = false;
        }

        public UIElement? HitTest(Vector2 position)
        {
            SortChildrenIfNeeded();

            for (int i = _sortedChildren.Count - 1; i >= 0; i--)
            {
                var child = _sortedChildren[i];
                if (child.IsVisible && child.ContainsPoint(position))
                {
                    return child;
                }
            }

            return null;
        }

        public List<UIElement> GetElementsAtPoint(Vector2 position)
        {
            SortChildrenIfNeeded();

            var result = new List<UIElement>();
            foreach (var child in _sortedChildren)
            {
                if (child.IsVisible && child.ContainsPoint(position))
                {
                    result.Add(child);
                }
            }
            return result;
        }

        public override void Update(float deltaTime)
        {
            if (!_isVisible)
                return;

            SortChildrenIfNeeded();
            foreach (var child in _sortedChildren)
            {
                child.Update(deltaTime);
            }
        }

        public override void Draw(DrawingContext context)
        {
            if (!_isVisible)
                return;

            SortChildrenIfNeeded();
            foreach (var child in _sortedChildren)
            {
                child.Draw(context);
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using WPFEngine.Math;

namespace WPFEngine.Rendering.Graphics
{
    public interface IRenderTarget
    {
        void Clear(Color color);
        void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f);
        void DrawRectangle(Vector2 position, Vector2 size, Color color, bool filled = false, float thickness = 1f);
        void DrawCircle(Vector2 center, float radius, Color color, bool filled = false, float thickness = 1f);
        void DrawPolygon(Vector2[] vertices, Color color, bool filled = false, float thickness = 1f);
        void DrawSprite(Geometry geometry, Vector2 position, Vector2 size, Color tint, float rotation = 0f, Vector2? origin = null);
        void DrawText(string text, Vector2 position, Color color, string fontFamily = "Arial", double fontSize = 12);
        void SetTransform(Matrix3x3 transform);
        void ResetTransform();
        void PushState();
        void PopState();
        void SetClip(Rect clipRect);
        void ClearClip();
    }

    public class RenderContext : IRenderTarget
    {
        private readonly DrawingContext _context;
        private readonly Stack<Matrix3x3> _transformStack;
        private Matrix3x3 _currentTransform;
        private Rect? _clipRect;
        private readonly BrushCache _brushCache;
        private readonly PenCache _penCache;

        public RenderContext(DrawingContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _transformStack = new Stack<Matrix3x3>();
            _currentTransform = Matrix3x3.Identity;
            _brushCache = new BrushCache();
            _penCache = new PenCache();
        }

        public void Clear(Color color)
        {
            var brush = _brushCache.GetSolidBrush(color);
            var clearRect = new Rect(0, 0, 10000, 10000);
            _context.DrawRectangle(brush, null, clearRect);
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f)
        {
            var transformedStart = _currentTransform.Transform(start);
            var transformedEnd = _currentTransform.Transform(end);
            var pen = _penCache.GetPen(color, thickness);
            _context.DrawLine(pen, transformedStart.ToPoint(), transformedEnd.ToPoint());
        }

        public void DrawRectangle(Vector2 position, Vector2 size, Color color, bool filled = false, float thickness = 1f)
        {
            var rect = new Rect(
                _currentTransform.Transform(position).ToPoint(),
                new Size(size.X, size.Y)
            );

            if (filled)
            {
                var brush = _brushCache.GetSolidBrush(color);
                _context.DrawRectangle(brush, null, rect);
            }
            else
            {
                var pen = _penCache.GetPen(color, thickness);
                _context.DrawRectangle(null, pen, rect);
            }
        }

        public void DrawCircle(Vector2 center, float radius, Color color, bool filled = false, float thickness = 1f)
        {
            var transformedCenter = _currentTransform.Transform(center);
            var ellipse = new EllipseGeometry(transformedCenter.ToPoint(), radius, radius);

            if (filled)
            {
                var brush = _brushCache.GetSolidBrush(color);
                _context.DrawGeometry(brush, null, ellipse);
            }
            else
            {
                var pen = _penCache.GetPen(color, thickness);
                _context.DrawGeometry(null, pen, ellipse);
            }
        }

        public void DrawPolygon(Vector2[] vertices, Color color, bool filled = false, float thickness = 1f)
        {
            if (vertices == null || vertices.Length < 2)
                return;

            var pathGeometry = new PathGeometry();
            var figure = new PathFigure { StartPoint = _currentTransform.Transform(vertices[0]).ToPoint() };

            for (int i = 1; i < vertices.Length; i++)
            {
                figure.Segments.Add(new LineSegment(_currentTransform.Transform(vertices[i]).ToPoint(), true));
            }

            if (vertices.Length > 2)
            {
                figure.IsClosed = true;
            }

            pathGeometry.Figures.Add(figure);

            if (filled)
            {
                var brush = _brushCache.GetSolidBrush(color);
                _context.DrawGeometry(brush, null, pathGeometry);
            }
            else
            {
                var pen = _penCache.GetPen(color, thickness);
                _context.DrawGeometry(null, pen, pathGeometry);
            }
        }

        public void DrawSprite(Geometry geometry, Vector2 position, Vector2 size, Color tint, float rotation = 0f, Vector2? origin = null)
        {
            var transformedPos = _currentTransform.Transform(position);

            var drawing = new GeometryDrawing(
                _brushCache.GetSolidBrush(tint),
                null,
                geometry
            );

            var scaleTransform = new ScaleTransform(size.X, size.Y);
            var rotateTransform = rotation != 0
                ? new RotateTransform(rotation * MathHelper.Rad2Deg, origin?.X ?? 0, origin?.Y ?? 0)
                : null;

            TransformGroup transformGroup = new TransformGroup();
            transformGroup.Children.Add(new TranslateTransform(transformedPos.X, transformedPos.Y));
            if (rotateTransform != null)
                transformGroup.Children.Add(rotateTransform);
            transformGroup.Children.Add(scaleTransform);

            drawing.Transform = transformGroup;
            _context.DrawDrawing(drawing);
        }

        public void DrawText(string text, Vector2 position, Color color, string fontFamily = "Arial", double fontSize = 12)
        {
            var transformedPos = _currentTransform.Transform(position);
            var typeface = new Typeface(new FontFamily(fontFamily), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
            var brush = _brushCache.GetSolidBrush(color);
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                typeface,
                fontSize,
                brush,
                1.0
            );

            _context.DrawText(formattedText, transformedPos.ToPoint());
        }

        public void SetTransform(Matrix3x3 transform)
        {
            _currentTransform = transform;
        }

        public void ResetTransform()
        {
            _currentTransform = Matrix3x3.Identity;
        }

        public void PushState()
        {
            _transformStack.Push(_currentTransform);
        }

        public void PopState()
        {
            if (_transformStack.Count > 0)
            {
                _currentTransform = _transformStack.Pop();
            }
        }

        public void SetClip(Rect clipRect)
        {
            _clipRect = clipRect;
            _context.PushClip(new RectangleGeometry(clipRect));
        }

        public void ClearClip()
        {
            _clipRect = null;
            _context.Pop();
        }

        public void Dispose()
        {
            _brushCache.Dispose();
            _penCache.Dispose();
        }
    }

    public class BrushCache : IDisposable
    {
        private readonly Dictionary<Color, SolidColorBrush> _brushes;
        private readonly Dictionary<(Color, Color), LinearGradientBrush> _linearGradients;
        private readonly Dictionary<(Color, Color), RadialGradientBrush> _radialGradients;
        private readonly object _lock = new object();

        public BrushCache()
        {
            _brushes = new Dictionary<Color, SolidColorBrush>();
            _linearGradients = new Dictionary<(Color, Color), LinearGradientBrush>();
            _radialGradients = new Dictionary<(Color, Color), RadialGradientBrush>();
        }

        public SolidColorBrush GetSolidBrush(Color color)
        {
            lock (_lock)
            {
                if (!_brushes.TryGetValue(color, out var brush))
                {
                    var wpfColor = Color.FromArgb(color.GetAByte(), color.GetRByte(), color.GetGByte(), color.GetBByte());
                    brush = new SolidColorBrush(wpfColor);
                    brush.Freeze();
                    _brushes[color] = brush;
                }
                return brush;
            }
        }

        public LinearGradientBrush GetLinearGradientBrush(Color startColor, Color endColor, Vector2 startPoint, Vector2 endPoint)
        {
            lock (_lock)
            {
                var key = (startColor, endColor);
                if (!_linearGradients.TryGetValue(key, out var brush))
                {
                    var wpfStart = Color.FromArgb(startColor.GetAByte(), startColor.GetRByte(), startColor.GetGByte(), startColor.GetBByte());
                    var wpfEnd = Color.FromArgb(endColor.GetAByte(), endColor.GetRByte(), endColor.GetGByte(), endColor.GetBByte());

                    brush = new LinearGradientBrush(wpfStart, wpfEnd, startPoint.ToPoint(), endPoint.ToPoint());
                    brush.Freeze();
                    _linearGradients[key] = brush;
                }
                return brush;
            }
        }

        public RadialGradientBrush GetRadialGradientBrush(Color centerColor, Color outerColor, Vector2 center, float radius)
        {
            lock (_lock)
            {
                var key = (centerColor, outerColor);
                if (!_radialGradients.TryGetValue(key, out var brush))
                {
                    var wpfCenter = Color.FromArgb(centerColor.GetAByte(), centerColor.GetRByte(), centerColor.GetGByte(), centerColor.GetBByte());
                    var wpfOuter = Color.FromArgb(outerColor.GetAByte(), outerColor.GetRByte(), outerColor.GetGByte(), outerColor.GetBByte());

                    brush = new RadialGradientBrush(wpfCenter, wpfOuter);
                    brush.GradientOrigin = center.ToPoint();
                    brush.Freeze();
                    _radialGradients[key] = brush;
                }
                return brush;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _brushes.Clear();
                _linearGradients.Clear();
                _radialGradients.Clear();
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }

    public class PenCache : IDisposable
    {
        private readonly Dictionary<(Color, float), Pen> _pens;
        private readonly object _lock = new object();

        public PenCache()
        {
            _pens = new Dictionary<(Color, float), Pen>();
        }

        public Pen GetPen(Color color, float thickness = 1f)
        {
            lock (_lock)
            {
                var key = (color, thickness);
                if (!_pens.TryGetValue(key, out var pen))
                {
                    var wpfColor = Color.FromArgb(color.GetAByte(), color.GetRByte(), color.GetGByte(), color.GetBByte());
                    pen = new Pen(new SolidColorBrush(wpfColor), thickness);
                    pen.Freeze();
                    _pens[key] = pen;
                }
                return pen;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                _pens.Clear();
            }
        }

        public void Dispose()
        {
            Clear();
        }
    }

    public class SpriteBatch : IDisposable
    {
        private readonly RenderContext _context;
        private readonly List<SpriteCommand> _commandQueue;
        private readonly Comparison<SpriteCommand> _sortComparison;
        private int _maxBatchSize;
        private bool _isDrawing;

        public int MaxBatchSize
        {
            get => _maxBatchSize;
            set => _maxBatchSize = Math.Max(1, value);
        }

        public SpriteBatch(RenderContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _commandQueue = new List<SpriteCommand>();
            _maxBatchSize = 1000;
            _isDrawing = false;
            _sortComparison = CompareSprites;
        }

        public void Begin(SortMode sortMode = SortMode.Deferred)
        {
            _isDrawing = true;
            _commandQueue.Clear();
        }

        public void Draw(
            Geometry geometry,
            Vector2 position,
            Vector2 size,
            Color tint,
            float rotation = 0f,
            Vector2? origin = null,
            float depth = 0f,
            SpriteEffects effects = SpriteEffects.None)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("SpriteBatch.Draw() called without Begin().");

            if (_commandQueue.Count >= _maxBatchSize)
            {
                Flush();
            }

            _commandQueue.Add(new SpriteCommand
            {
                Geometry = geometry,
                Position = position,
                Size = size,
                Tint = tint,
                Rotation = rotation,
                Origin = origin ?? Vector2.Zero,
                Depth = depth,
                Effects = effects
            });
        }

        public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness = 1f, float depth = 0f)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("SpriteBatch.DrawLine() called without Begin().");

            _commandQueue.Add(new SpriteCommand
            {
                Type = CommandType.Line,
                Position = start,
                Size = end - start,
                Tint = color,
                Rotation = thickness,
                Depth = depth
            });
        }

        public void DrawRectangle(Vector2 position, Vector2 size, Color color, bool filled = false, float thickness = 1f, float depth = 0f)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("SpriteBatch.DrawRectangle() called without Begin().");

            _commandQueue.Add(new SpriteCommand
            {
                Type = filled ? CommandType.FilledRectangle : CommandType.Rectangle,
                Position = position,
                Size = size,
                Tint = color,
                Rotation = thickness,
                Depth = depth
            });
        }

        public void DrawCircle(Vector2 center, float radius, Color color, bool filled = false, float thickness = 1f, float depth = 0f)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("SpriteBatch.DrawCircle() called without Begin().");

            _commandQueue.Add(new SpriteCommand
            {
                Type = filled ? CommandType.FilledCircle : CommandType.Circle,
                Position = center,
                Size = new Vector2(radius, radius),
                Tint = color,
                Rotation = thickness,
                Depth = depth
            });
        }

        public void DrawText(string text, Vector2 position, Color color, string fontFamily = "Arial", double fontSize = 12, float depth = 0f)
        {
            if (!_isDrawing)
                throw new InvalidOperationException("SpriteBatch.DrawText() called without Begin().");

            _commandQueue.Add(new SpriteCommand
            {
                Type = CommandType.Text,
                Position = position,
                Tint = color,
                Text = text,
                FontFamily = fontFamily,
                FontSize = fontSize,
                Depth = depth
            });
        }

        public void End()
        {
            Flush();
            _isDrawing = false;
        }

        private void Flush()
        {
            if (_commandQueue.Count == 0)
                return;

            _commandQueue.Sort(_sortComparison);

            foreach (var command in _commandQueue)
            {
                ExecuteCommand(command);
            }

            _commandQueue.Clear();
        }

        private void ExecuteCommand(SpriteCommand command)
        {
            switch (command.Type)
            {
                case CommandType.Sprite:
                    _context.DrawSprite(command.Geometry, command.Position, command.Size, command.Tint, command.Rotation, command.Origin);
                    break;

                case CommandType.Line:
                    _context.DrawLine(command.Position, command.Position + command.Size, command.Tint, command.Rotation);
                    break;

                case CommandType.Rectangle:
                    _context.DrawRectangle(command.Position, command.Size, command.Tint, false, command.Rotation);
                    break;

                case CommandType.FilledRectangle:
                    _context.DrawRectangle(command.Position, command.Size, command.Tint, true);
                    break;

                case CommandType.Circle:
                    _context.DrawCircle(command.Position, command.Size.X, command.Tint, false, command.Rotation);
                    break;

                case CommandType.FilledCircle:
                    _context.DrawCircle(command.Position, command.Size.X, command.Tint, true);
                    break;

                case CommandType.Text:
                    _context.DrawText(command.Text, command.Position, command.Tint, command.FontFamily, command.FontSize);
                    break;
            }
        }

        private static int CompareSprites(SpriteCommand a, SpriteCommand b)
        {
            int depthCompare = a.Depth.CompareTo(b.Depth);
            if (depthCompare != 0) return depthCompare;

            int typeCompare = ((int)a.Type).CompareTo((int)b.Type);
            return typeCompare;
        }

        public void Dispose()
        {
            if (_isDrawing)
            {
                End();
            }
        }

        private enum CommandType
        {
            Sprite,
            Line,
            Rectangle,
            FilledRectangle,
            Circle,
            FilledCircle,
            Text
        }

        private struct SpriteCommand
        {
            public CommandType Type;
            public Geometry? Geometry;
            public Vector2 Position;
            public Vector2 Size;
            public Color Tint;
            public float Rotation;
            public Vector2 Origin;
            public float Depth;
            public SpriteEffects Effects;
            public string? Text;
            public string? FontFamily;
            public double FontSize;
        }
    }

    [Flags]
    public enum SpriteEffects
    {
        None = 0,
        FlipHorizontally = 1,
        FlipVertically = 2
    }

    public enum SortMode
    {
        Deferred,
        Immediate,
        Texture,
        BackToFront,
        FrontToBack
    }
}

namespace WPFEngine.Math
{
    public static class VectorExtensions
    {
        public static Point ToPoint(this Vector2 vector)
        {
            return new Point(vector.X, vector.Y);
        }

        public static System.Windows.Vector ToWpfVector(this Vector2 vector)
        {
            return new System.Windows.Vector(vector.X, vector.Y);
        }
    }
}

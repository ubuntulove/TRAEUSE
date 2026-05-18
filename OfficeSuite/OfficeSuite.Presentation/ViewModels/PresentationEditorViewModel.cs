using System;
using System.Collections.Generic;
using System.Linq;
using OfficeSuite.Core.ViewModels;
using OfficeSuite.Presentation.Models;

namespace OfficeSuite.Presentation.ViewModels
{
    public class PresentationEditorViewModel : ViewModelBase
    {
        private PresentationDocument _document;
        private Slide _currentSlide;
        private int _currentSlideIndex;
        private SlideShape _selectedShape;
        private List<SlideShape> _clipboard;
        private UndoRedoManager _undoRedoManager;
        private bool _isEditing;
        private bool _isSlideShowActive;
        private int _selectedShapeIndex;
        private string _statusMessage;
        private double _zoomLevel;
        private PresentationPlayback _playback;

        public PresentationDocument Document
        {
            get => _document;
            set => SetProperty(ref _document, value);
        }

        public Slide CurrentSlide
        {
            get => _currentSlide;
            set => SetProperty(ref _currentSlide, value);
        }

        public int CurrentSlideIndex
        {
            get => _currentSlideIndex;
            set
            {
                SetProperty(ref _currentSlideIndex, value);
                if (Document != null && value >= 0 && value < Document.Slides.Count)
                    CurrentSlide = Document.Slides[value];
            }
        }

        public SlideShape SelectedShape
        {
            get => _selectedShape;
            set => SetProperty(ref _selectedShape, value);
        }

        public List<SlideShape> Clipboard
        {
            get => _clipboard;
            set => SetProperty(ref _clipboard, value);
        }

        public UndoRedoManager UndoRedoManager
        {
            get => _undoRedoManager;
            set => SetProperty(ref _undoRedoManager, value);
        }

        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        public bool IsSlideShowActive
        {
            get => _isSlideShowActive;
            set => SetProperty(ref _isSlideShowActive, value);
        }

        public int SelectedShapeIndex
        {
            get => _selectedShapeIndex;
            set => SetProperty(ref _selectedShapeIndex, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, Math.Max(25, Math.Min(400, value)));
        }

        public PresentationPlayback Playback
        {
            get => _playback;
            set => SetProperty(ref _playback, value);
        }

        public RelayCommand NewDocumentCommand { get; }
        public RelayCommand OpenDocumentCommand { get; }
        public RelayCommand SaveDocumentCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand PrintCommand { get; }
        public RelayCommand NewSlideCommand { get; }
        public RelayCommand DeleteSlideCommand { get; }
        public RelayCommand DuplicateSlideCommand { get; }
        public RelayCommand NextSlideCommand { get; }
        public RelayCommand PreviousSlideCommand { get; }
        public RelayCommand FirstSlideCommand { get; }
        public RelayCommand LastSlideCommand { get; }
        public RelayCommand StartSlideShowCommand { get; }
        public RelayCommand EndSlideShowCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }
        public RelayCommand CutCommand { get; }
        public RelayCommand CopyCommand { get; }
        public RelayCommand PasteCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand InsertTextBoxCommand { get; }
        public RelayCommand InsertShapeCommand { get; }
        public RelayCommand InsertPictureCommand { get; }
        public RelayCommand InsertTableCommand { get; }
        public RelayCommand InsertChartCommand { get; }
        public RelayCommand InsertSmartArtCommand { get; }
        public RelayCommand BringForwardCommand { get; }
        public RelayCommand SendBackwardCommand { get; }
        public RelayCommand BringToFrontCommand { get; }
        public RelayCommand SendToBackCommand { get; }
        public RelayCommand GroupShapesCommand { get; }
        public RelayCommand UngroupShapesCommand { get; }
        public RelayCommand AlignLeftCommand { get; }
        public RelayCommand AlignCenterCommand { get; }
        public RelayCommand AlignRightCommand { get; }
        public RelayCommand AlignTopCommand { get; }
        public RelayCommand AlignMiddleCommand { get; }
        public RelayCommand AlignBottomCommand { get; }
        public RelayCommand DistributeHorizontalCommand { get; }
        public RelayCommand DistributeVerticalCommand { get; }
        public RelayCommand RotateLeftCommand { get; }
        public RelayCommand RotateRightCommand { get; }
        public RelayCommand FlipHorizontalCommand { get; }
        public RelayCommand FlipVerticalCommand { get; }
        public RelayCommand SetTransitionCommand { get; }
        public RelayCommand ApplyAnimationCommand { get; }
        public RelayCommand ZoomInCommand { get; }
        public RelayCommand ZoomOutCommand { get; }

        public PresentationEditorViewModel()
        {
            Document = new PresentationDocument();
            Clipboard = new List<SlideShape>();
            UndoRedoManager = new UndoRedoManager();
            Playback = new PresentationPlayback();
            ZoomLevel = 100;
            IsEditing = true;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            NewDocumentCommand = new RelayCommand(_ => NewDocument());
            OpenDocumentCommand = new RelayCommand(_ => OpenDocument());
            SaveDocumentCommand = new RelayCommand(_ => SaveDocument());
            SaveAsCommand = new RelayCommand(_ => SaveDocumentAs());
            PrintCommand = new RelayCommand(_ => Print());
            NewSlideCommand = new RelayCommand(_ => NewSlide(), _ => CanAddSlide());
            DeleteSlideCommand = new RelayCommand(_ => DeleteSlide(), _ => CanDeleteSlide());
            DuplicateSlideCommand = new RelayCommand(_ => DuplicateSlide(), _ => CanDeleteSlide());
            NextSlideCommand = new RelayCommand(_ => NextSlide(), _ => CanNavigate());
            PreviousSlideCommand = new RelayCommand(_ => PreviousSlide(), _ => CanNavigate());
            FirstSlideCommand = new RelayCommand(_ => FirstSlide(), _ => CanNavigate());
            LastSlideCommand = new RelayCommand(_ => LastSlide(), _ => CanNavigate());
            StartSlideShowCommand = new RelayCommand(_ => StartSlideShow());
            EndSlideShowCommand = new RelayCommand(_ => EndSlideShow());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo());
            CutCommand = new RelayCommand(_ => Cut(), _ => CanCut());
            CopyCommand = new RelayCommand(_ => Copy(), _ => CanCopy());
            PasteCommand = new RelayCommand(_ => Paste(), _ => CanPaste());
            DeleteCommand = new RelayCommand(_ => DeleteSelection(), _ => CanDelete());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            InsertTextBoxCommand = new RelayCommand(_ => InsertTextBox());
            InsertShapeCommand = new RelayCommand(_ => InsertShape());
            InsertPictureCommand = new RelayCommand(_ => InsertPicture());
            InsertTableCommand = new RelayCommand(_ => InsertTable());
            InsertChartCommand = new RelayCommand(_ => InsertChart());
            InsertSmartArtCommand = new RelayCommand(_ => InsertSmartArt());
            BringForwardCommand = new RelayCommand(_ => BringForward(), _ => CanReorderShape());
            SendBackwardCommand = new RelayCommand(_ => SendBackward(), _ => CanReorderShape());
            BringToFrontCommand = new RelayCommand(_ => BringToFront(), _ => CanReorderShape());
            SendToBackCommand = new RelayCommand(_ => SendToBack(), _ => CanReorderShape());
            GroupShapesCommand = new RelayCommand(_ => GroupShapes(), _ => CanGroupShapes());
            UngroupShapesCommand = new RelayCommand(_ => UngroupShapes(), _ => CanUngroupShapes());
            AlignLeftCommand = new RelayCommand(_ => AlignShapes(AlignmentType.Left), _ => CanAlign());
            AlignCenterCommand = new RelayCommand(_ => AlignShapes(AlignmentType.Center), _ => CanAlign());
            AlignRightCommand = new RelayCommand(_ => AlignShapes(AlignmentType.Right), _ => CanAlign());
            AlignTopCommand = new RelayCommand(_ => AlignShapes(AlignmentType.Top), _ => CanAlign());
            AlignMiddleCommand = new RelayCommand(_ => AlignShapes(AlignmentType.Middle), _ => CanAlign());
            AlignBottomCommand = new RelayCommand(_ => AlignShapes(AlignmentType.Bottom), _ => CanAlign());
            DistributeHorizontalCommand = new RelayCommand(_ => DistributeShapes(true), _ => CanDistribute());
            DistributeVerticalCommand = new RelayCommand(_ => DistributeShapes(false), _ => CanDistribute());
            RotateLeftCommand = new RelayCommand(_ => Rotate(-90), _ => CanRotate());
            RotateRightCommand = new RelayCommand(_ => Rotate(90), _ => CanRotate());
            FlipHorizontalCommand = new RelayCommand(_ => FlipHorizontal(), _ => CanFlip());
            FlipVerticalCommand = new RelayCommand(_ => FlipVertical(), _ => CanFlip());
            SetTransitionCommand = new RelayCommand(_ => ShowTransitionDialog());
            ApplyAnimationCommand = new RelayCommand(_ => ShowAnimationDialog());
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
        }

        public void NewDocument()
        {
            Document = new PresentationDocument();
            Document.AddSlide();
            CurrentSlideIndex = 0;
            CurrentSlide = Document.Slides[0];
            UndoRedoManager.Clear();
            OnPropertyChanged(nameof(Document));
            StatusMessage = "New presentation created";
        }

        public void OpenDocument()
        {
        }

        public void SaveDocument()
        {
        }

        public void SaveDocumentAs()
        {
        }

        public void Print()
        {
        }

        public void NewSlide()
        {
            var slide = Document.AddSlide();
            CurrentSlideIndex = Document.Slides.Count - 1;
            CurrentSlide = slide;
            OnPropertyChanged(nameof(Document));
            StatusMessage = "New slide added";
        }

        public bool CanAddSlide() => Document != null;

        public void DeleteSlide()
        {
            if (Document.Slides.Count <= 1) return;
            Document.RemoveSlide(CurrentSlideIndex);
            CurrentSlideIndex = Math.Max(0, CurrentSlideIndex - 1);
            OnPropertyChanged(nameof(Document));
            StatusMessage = "Slide deleted";
        }

        public bool CanDeleteSlide() => Document != null && Document.Slides.Count > 1;

        public void DuplicateSlide()
        {
            if (CurrentSlide == null) return;
            var newSlide = new Slide
            {
                Name = $"{CurrentSlide.Name} (Copy)",
                Title = CurrentSlide.Title,
                Background = CurrentSlide.Background.Clone() as SlideBackground,
                Layout = CurrentSlide.Layout,
                Transition = CurrentSlide.Transition.Clone() as Transition
            };

            foreach (var shape in CurrentSlide.Shapes)
            {
                var newShape = CloneShape(shape);
                newSlide.Shapes.Add(newShape);
            }

            Document.Slides.Insert(CurrentSlideIndex + 1, newSlide);
            CurrentSlideIndex++;
            CurrentSlide = newSlide;
            OnPropertyChanged(nameof(Document));
            StatusMessage = "Slide duplicated";
        }

        private SlideShape CloneShape(SlideShape source)
        {
            return new TextShape
            {
                Name = source.Name,
                Left = source.Left,
                Top = source.Top,
                Width = source.Width,
                Height = source.Height,
                Rotation = source.Rotation,
                IsLocked = source.IsLocked,
                IsVisible = source.IsVisible,
                ShapeType = source.ShapeType
            };
        }

        public void NextSlide()
        {
            if (CurrentSlideIndex < Document.Slides.Count - 1)
                CurrentSlideIndex++;
        }

        public void PreviousSlide()
        {
            if (CurrentSlideIndex > 0)
                CurrentSlideIndex--;
        }

        public void FirstSlide()
        {
            CurrentSlideIndex = 0;
        }

        public void LastSlide()
        {
            CurrentSlideIndex = Document.Slides.Count - 1;
        }

        public bool CanNavigate() => Document != null && Document.Slides.Count > 1;

        public void StartSlideShow()
        {
            if (Document == null || Document.Slides.Count == 0) return;
            IsSlideShowActive = true;
            CurrentSlideIndex = 0;
            Playback.Start(Document);
        }

        public void EndSlideShow()
        {
            IsSlideShowActive = false;
            Playback.Stop();
        }

        public void Undo()
        {
            UndoRedoManager.Undo();
        }

        public bool CanUndo() => UndoRedoManager.CanUndo;

        public void Redo()
        {
            UndoRedoManager.Redo();
        }

        public bool CanRedo() => UndoRedoManager.CanRedo;

        public void Cut()
        {
            Copy();
            DeleteSelection();
        }

        public bool CanCut() => SelectedShape != null;

        public void Copy()
        {
            if (SelectedShape == null) return;
            Clipboard.Clear();
            Clipboard.Add(SelectedShape);
            StatusMessage = "Shape copied";
        }

        public bool CanCopy() => SelectedShape != null;

        public void Paste()
        {
            if (Clipboard.Count == 0 || CurrentSlide == null) return;
            foreach (var shape in Clipboard)
            {
                var newShape = CloneShape(shape);
                newShape.Left += 10;
                newShape.Top += 10;
                CurrentSlide.Shapes.Add(newShape);
            }
            OnPropertyChanged(nameof(CurrentSlide));
            StatusMessage = $"{Clipboard.Count} shape(s) pasted";
        }

        public bool CanPaste() => Clipboard.Count > 0 && CurrentSlide != null;

        public void DeleteSelection()
        {
            if (SelectedShape == null || CurrentSlide == null) return;
            CurrentSlide.RemoveShape(SelectedShape);
            SelectedShape = null;
            OnPropertyChanged(nameof(CurrentSlide));
            StatusMessage = "Shape deleted";
        }

        public bool CanDelete() => SelectedShape != null;

        public void SelectAll()
        {
        }

        public void InsertTextBox()
        {
            if (CurrentSlide == null) return;
            var textBox = new TextShape
            {
                Name = "Text Box",
                Left = 100,
                Top = 100,
                Width = 200,
                Height = 50,
                Text = "Enter text here"
            };
            CurrentSlide.Shapes.Add(textBox);
            SelectedShape = textBox;
            OnPropertyChanged(nameof(CurrentSlide));
        }

        public void InsertShape()
        {
            if (CurrentSlide == null) return;
            var shape = new RectangleShape
            {
                Name = "Rectangle",
                Left = 100,
                Top = 100,
                Width = 100,
                Height = 100
            };
            CurrentSlide.Shapes.Add(shape);
            SelectedShape = shape;
            OnPropertyChanged(nameof(CurrentSlide));
        }

        public void InsertPicture()
        {
        }

        public void InsertTable()
        {
        }

        public void InsertChart()
        {
        }

        public void InsertSmartArt()
        {
        }

        public void BringForward()
        {
            if (SelectedShape == null || CurrentSlide == null) return;
            int index = CurrentSlide.Shapes.IndexOf(SelectedShape);
            if (index < CurrentSlide.Shapes.Count - 1)
            {
                CurrentSlide.Shapes.RemoveAt(index);
                CurrentSlide.Shapes.Insert(index + 1, SelectedShape);
                OnPropertyChanged(nameof(CurrentSlide));
            }
        }

        public bool CanReorderShape() => SelectedShape != null && CurrentSlide != null;

        public void SendBackward()
        {
            if (SelectedShape == null || CurrentSlide == null) return;
            int index = CurrentSlide.Shapes.IndexOf(SelectedShape);
            if (index > 0)
            {
                CurrentSlide.Shapes.RemoveAt(index);
                CurrentSlide.Shapes.Insert(index - 1, SelectedShape);
                OnPropertyChanged(nameof(CurrentSlide));
            }
        }

        public void BringToFront()
        {
            if (SelectedShape == null || CurrentSlide == null) return;
            CurrentSlide.Shapes.Remove(SelectedShape);
            CurrentSlide.Shapes.Add(SelectedShape);
            OnPropertyChanged(nameof(CurrentSlide));
        }

        public void SendToBack()
        {
            if (SelectedShape == null || CurrentSlide == null) return;
            CurrentSlide.Shapes.Remove(SelectedShape);
            CurrentSlide.Shapes.Insert(0, SelectedShape);
            OnPropertyChanged(nameof(CurrentSlide));
        }

        public void GroupShapes()
        {
        }

        public bool CanGroupShapes() => SelectedShape != null;

        public void UngroupShapes()
        {
        }

        public bool CanUngroupShapes() => SelectedShape is GroupShape;

        public void AlignShapes(AlignmentType alignment)
        {
        }

        public bool CanAlign() => SelectedShape != null;

        public void DistributeShapes(bool horizontal)
        {
        }

        public bool CanDistribute() => SelectedShape != null;

        public void Rotate(double angle)
        {
            if (SelectedShape != null)
            {
                SelectedShape.Rotation += angle;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }

        public bool CanRotate() => SelectedShape != null;

        public void FlipHorizontal()
        {
            if (SelectedShape != null)
            {
                SelectedShape.ScaleX = SelectedShape.ScaleX * -1;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }

        public void FlipVertical()
        {
            if (SelectedShape != null)
            {
                SelectedShape.ScaleY = SelectedShape.ScaleY * -1;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }

        public bool CanFlip() => SelectedShape != null;

        public void ShowTransitionDialog()
        {
        }

        public void ShowAnimationDialog()
        {
        }

        public void ZoomIn()
        {
            ZoomLevel = Math.Min(400, ZoomLevel + 25);
        }

        public void ZoomOut()
        {
            ZoomLevel = Math.Max(25, ZoomLevel - 25);
        }

        public void SetTransition(TransitionType type, TransitionSpeed speed)
        {
            if (CurrentSlide != null)
            {
                CurrentSlide.Transition.Type = type;
                CurrentSlide.Transition.Speed = speed;
                OnPropertyChanged(nameof(CurrentSlide));
            }
        }

        public void ApplyAnimation(AnimationType type, AnimationEffect effect)
        {
            if (CurrentSlide == null) return;
            var animation = new Animation
            {
                Type = type,
                Effect = effect
            };
            CurrentSlide.Animations.Add(animation);
            OnPropertyChanged(nameof(CurrentSlide));
        }

        public void SetSlideBackground(SlideBackground background)
        {
            if (CurrentSlide != null)
            {
                CurrentSlide.Background = background;
                OnPropertyChanged(nameof(CurrentSlide));
            }
        }

        public void SetShapeFill(FillFormat fill)
        {
            if (SelectedShape != null)
            {
                SelectedShape.Fill = fill;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }

        public void SetShapeLine(LineFormat line)
        {
            if (SelectedShape != null)
            {
                SelectedShape.Line = line;
                OnPropertyChanged(nameof(SelectedShape));
            }
        }

        public override void Initialize()
        {
            NewDocument();
        }

        public override void Cleanup()
        {
            Document = null;
            CurrentSlide = null;
            SelectedShape = null;
            Clipboard.Clear();
            UndoRedoManager.Clear();
        }
    }

    public enum AlignmentType
    {
        Left,
        Center,
        Right,
        Top,
        Middle,
        Bottom
    }

    public class PresentationPlayback
    {
        private PresentationDocument _document;
        private int _currentSlideIndex;
        private bool _isPlaying;
        private bool _isPaused;
        private System.Threading.Timer _timer;
        private int _currentAnimationIndex;

        public event EventHandler<SlideChangedEventArgs> SlideChanged;
        public event EventHandler<AnimationEventArgs> AnimationTick;

        public int CurrentSlideIndex
        {
            get => _currentSlideIndex;
            private set => _currentSlideIndex = value;
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            private set => _isPlaying = value;
        }

        public bool IsPaused
        {
            get => _isPaused;
            private set => _isPaused = value;
        }

        public PresentationPlayback()
        {
            _currentSlideIndex = 0;
            _isPlaying = false;
            _isPaused = false;
        }

        public void Start(PresentationDocument document)
        {
            _document = document;
            _currentSlideIndex = 0;
            _isPlaying = true;
            _isPaused = false;
            OnSlideChanged();
        }

        public void Stop()
        {
            _isPlaying = false;
            _isPaused = false;
            _timer?.Dispose();
            _timer = null;
        }

        public void Pause()
        {
            if (_isPlaying && !_isPaused)
            {
                _isPaused = true;
                _timer?.Change(Timeout.Infinite, Timeout.Infinite);
            }
        }

        public void Resume()
        {
            if (_isPlaying && _isPaused)
            {
                _isPaused = false;
                StartTimer();
            }
        }

        public void Next()
        {
            if (_document == null || !_isPlaying) return;
            if (_currentSlideIndex < _document.Slides.Count - 1)
            {
                _currentSlideIndex++;
                OnSlideChanged();
            }
            else
            {
                Stop();
            }
        }

        public void Previous()
        {
            if (_document == null || !_isPlaying) return;
            if (_currentSlideIndex > 0)
            {
                _currentSlideIndex--;
                OnSlideChanged();
            }
        }

        public void GoToSlide(int index)
        {
            if (_document == null || !_isPlaying) return;
            if (index >= 0 && index < _document.Slides.Count)
            {
                _currentSlideIndex = index;
                OnSlideChanged();
            }
        }

        private void StartTimer()
        {
            if (_document?.Slides.Count > _currentSlideIndex)
            {
                var slide = _document.Slides[_currentSlideIndex];
                var duration = (int)(slide.Transition.Duration * 1000);
                _timer = new System.Threading.Timer(OnTimerElapsed, null, duration, Timeout.Infinite);
            }
        }

        private void OnTimerElapsed(object state)
        {
            Next();
        }

        private void OnSlideChanged()
        {
            SlideChanged?.Invoke(this, new SlideChangedEventArgs(_currentSlideIndex));
            if (!_isPaused)
                StartTimer();
        }
    }

    public class SlideChangedEventArgs : EventArgs
    {
        public int SlideIndex { get; }

        public SlideChangedEventArgs(int slideIndex)
        {
            SlideIndex = slideIndex;
        }
    }

    public class AnimationEventArgs : EventArgs
    {
        public int AnimationIndex { get; }

        public AnimationEventArgs(int animationIndex)
        {
            AnimationIndex = animationIndex;
        }
    }

    public class UndoRedoManager
    {
        private readonly Stack<IUndoableAction> _undoStack = new Stack<IUndoableAction>();
        private readonly Stack<IUndoableAction> _redoStack = new Stack<IUndoableAction>();
        private const int MaxUndoLevels = 100;

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;

        public void Execute(IUndoableAction action)
        {
            action.Execute();
            _undoStack.Push(action);
            _redoStack.Clear();

            while (_undoStack.Count > MaxUndoLevels)
            {
                var items = _undoStack.ToArray();
                _undoStack.Clear();
                for (int i = 0; i < items.Length - 1; i++)
                    _undoStack.Push(items[items.Length - 1 - i]);
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var action = _undoStack.Pop();
                action.Undo();
                _redoStack.Push(action);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var action = _redoStack.Pop();
                action.Execute();
                _undoStack.Push(action);
            }
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
    }

    public interface IUndoableAction
    {
        void Execute();
        void Undo();
    }

    public class ShapeChangeAction : IUndoableAction
    {
        private readonly SlideShape _shape;
        private readonly SlideShape _previousState;
        private readonly SlideShape _newState;

        public ShapeChangeAction(SlideShape shape, SlideShape previousState, SlideShape newState)
        {
            _shape = shape;
            _previousState = previousState;
            _newState = newState;
        }

        public void Execute()
        {
        }

        public void Undo()
        {
        }
    }

    public class SlideShape : Core.Models.ModelBase
    {
        private double _left;
        private double _top;
        private double _width;
        private double _height;
        private double _rotation;
        private double _scaleX = 1;
        private double _scaleY = 1;
        private bool _isLocked;
        private bool _isVisible;
        private Models.ShapeType _shapeType;
        private FillFormat _fill;
        private LineFormat _line;

        public double Left { get => _left; set => SetProperty(ref _left, value); }
        public double Top { get => _top; set => SetProperty(ref _top, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
        public double Height { get => _height; set => SetProperty(ref _height, value); }
        public double Rotation { get => _rotation; set => SetProperty(ref _rotation, value); }
        public double ScaleX { get => _scaleX; set => SetProperty(ref _scaleX, value); }
        public double ScaleY { get => _scaleY; set => SetProperty(ref _scaleY, value); }
        public bool IsLocked { get => _isLocked; set => SetProperty(ref _isLocked, value); }
        public bool IsVisible { get => _isVisible; set => SetProperty(ref _isVisible, value); }
        public Models.ShapeType ShapeType { get => _shapeType; set => SetProperty(ref _shapeType, value); }
        public FillFormat Fill { get => _fill; set => SetProperty(ref _fill, value); }
        public LineFormat Line { get => _line; set => SetProperty(ref _line, value); }
    }

    public class FillFormat : Core.Models.ModelBase
    {
        private Models.FillType _type;
        private System.Windows.Media.Color _color;

        public Models.FillType Type { get => _type; set => SetProperty(ref _type, value); }
        public System.Windows.Media.Color Color { get => _color; set => SetProperty(ref _color, value); }
    }

    public class LineFormat : Core.Models.ModelBase
    {
        private Models.LineType _type;
        private System.Windows.Media.Color _color;
        private double _width;

        public Models.LineType Type { get => _type; set => SetProperty(ref _type, value); }
        public System.Windows.Media.Color Color { get => _color; set => SetProperty(ref _color, value); }
        public double Width { get => _width; set => SetProperty(ref _width, value); }
    }
}

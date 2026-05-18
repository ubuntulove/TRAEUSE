using System;
using System.Collections.Generic;
using System.Linq;
using OfficeSuite.Core.ViewModels;
using OfficeSuite.Spreadsheet.Models;

namespace OfficeSuite.Spreadsheet.ViewModels
{
    public class SpreadsheetEditorViewModel : ViewModelBase
    {
        private SpreadsheetDocument _document;
        private Worksheet _activeWorksheet;
        private Cell _selectedCell;
        private CellRange _selection;
        private List<Cell> _clipboard;
        private UndoRedoManager _undoRedoManager;
        private bool _isEditing;
        private int _activeCellRow;
        private int _activeCellColumn;
        private string _statusMessage;
        private double _zoomLevel;
        private FormulaEngine _formulaEngine;

        public SpreadsheetDocument Document
        {
            get => _document;
            set => SetProperty(ref _document, value);
        }

        public Worksheet ActiveWorksheet
        {
            get => _activeWorksheet;
            set => SetProperty(ref _activeWorksheet, value);
        }

        public Cell SelectedCell
        {
            get => _selectedCell;
            set => SetProperty(ref _selectedCell, value);
        }

        public CellRange Selection
        {
            get => _selection;
            set => SetProperty(ref _selection, value);
        }

        public List<Cell> Clipboard
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

        public int ActiveCellRow
        {
            get => _activeCellRow;
            set
            {
                SetProperty(ref _activeCellRow, value);
                OnPropertyChanged(nameof(ActiveCellReference));
            }
        }

        public int ActiveCellColumn
        {
            get => _activeCellColumn;
            set
            {
                SetProperty(ref _activeCellColumn, value);
                OnPropertyChanged(nameof(ActiveCellReference));
            }
        }

        public string ActiveCellReference
        {
            get => CellReference.ToReference(ActiveCellRow, ActiveCellColumn);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, Math.Max(10, Math.Min(400, value)));
        }

        public FormulaEngine FormulaEngine
        {
            get => _formulaEngine;
            set => SetProperty(ref _formulaEngine, value);
        }

        public RelayCommand NewDocumentCommand { get; }
        public RelayCommand OpenDocumentCommand { get; }
        public RelayCommand SaveDocumentCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand PrintCommand { get; }
        public RelayCommand UndoCommand { get; }
        public RelayCommand RedoCommand { get; }
        public RelayCommand CutCommand { get; }
        public RelayCommand CopyCommand { get; }
        public RelayCommand PasteCommand { get; }
        public RelayCommand DeleteCommand { get; }
        public RelayCommand SelectAllCommand { get; }
        public RelayCommand InsertRowCommand { get; }
        public RelayCommand DeleteRowCommand { get; }
        public RelayCommand InsertColumnCommand { get; }
        public RelayCommand DeleteColumnCommand { get; }
        public RelayCommand MergeCellsCommand { get; }
        public RelayCommand UnmergeCellsCommand { get; }
        public RelayCommand FormatCellsCommand { get; }
        public RelayCommand ClearContentsCommand { get; }
        public RelayCommand ClearFormatsCommand { get; }
        public RelayCommand SortAscendingCommand { get; }
        public RelayCommand SortDescendingCommand { get; }
        public RelayCommand AddWorksheetCommand { get; }
        public RelayCommand RemoveWorksheetCommand { get; }
        public RelayCommand RenameWorksheetCommand { get; }
        public RelayCommand ZoomInCommand { get; }
        public RelayCommand ZoomOutCommand { get; }

        public SpreadsheetEditorViewModel()
        {
            Document = new SpreadsheetDocument();
            ActiveWorksheet = Document.ActiveWorksheet;
            UndoRedoManager = new UndoRedoManager();
            FormulaEngine = new FormulaEngine();
            Clipboard = new List<Cell>();
            ZoomLevel = 100;

            InitializeCommands();
        }

        private void InitializeCommands()
        {
            NewDocumentCommand = new RelayCommand(_ => NewDocument());
            OpenDocumentCommand = new RelayCommand(_ => OpenDocument());
            SaveDocumentCommand = new RelayCommand(_ => SaveDocument());
            SaveAsCommand = new RelayCommand(_ => SaveDocumentAs());
            PrintCommand = new RelayCommand(_ => Print());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo());
            CutCommand = new RelayCommand(_ => Cut(), _ => CanCut());
            CopyCommand = new RelayCommand(_ => Copy(), _ => CanCopy());
            PasteCommand = new RelayCommand(_ => Paste(), _ => CanPaste());
            DeleteCommand = new RelayCommand(_ => DeleteSelection());
            SelectAllCommand = new RelayCommand(_ => SelectAll());
            InsertRowCommand = new RelayCommand(_ => InsertRow(), _ => CanInsertRow());
            DeleteRowCommand = new RelayCommand(_ => DeleteRow(), _ => CanDeleteRow());
            InsertColumnCommand = new RelayCommand(_ => InsertColumn(), _ => CanInsertColumn());
            DeleteColumnCommand = new RelayCommand(_ => DeleteColumn(), _ => CanDeleteColumn());
            MergeCellsCommand = new RelayCommand(_ => MergeCells(), _ => CanMergeCells());
            UnmergeCellsCommand = new RelayCommand(_ => UnmergeCells(), _ => CanUnmergeCells());
            FormatCellsCommand = new RelayCommand(_ => ShowFormatDialog());
            ClearContentsCommand = new RelayCommand(_ => ClearContents());
            ClearFormatsCommand = new RelayCommand(_ => ClearFormats());
            SortAscendingCommand = new RelayCommand(_ => SortAscending(), _ => CanSort());
            SortDescendingCommand = new RelayCommand(_ => SortDescending(), _ => CanSort());
            AddWorksheetCommand = new RelayCommand(_ => AddWorksheet());
            RemoveWorksheetCommand = new RelayCommand(_ => RemoveWorksheet(), _ => CanRemoveWorksheet());
            RenameWorksheetCommand = new RelayCommand(_ => RenameWorksheet());
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
        }

        public void NewDocument()
        {
            Document = new SpreadsheetDocument();
            ActiveWorksheet = Document.ActiveWorksheet;
            ActiveCellRow = 0;
            ActiveCellColumn = 0;
            UndoRedoManager.Clear();
            OnPropertyChanged(nameof(Document));
            StatusMessage = "New document created";
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

        public bool CanCut() => Selection != null;

        public void Copy()
        {
            if (Selection == null || ActiveWorksheet == null) return;
            Clipboard.Clear();
            for (int row = Selection.StartRow; row <= Selection.EndRow; row++)
            {
                for (int col = Selection.StartColumn; col <= Selection.EndColumn; col++)
                {
                    var cell = ActiveWorksheet.GetCell(row, col);
                    if (cell != null)
                        Clipboard.Add(cell);
                }
            }
            StatusMessage = $"Copied {Clipboard.Count} cell(s)";
        }

        public bool CanCopy() => Selection != null;

        public void Paste()
        {
        }

        public bool CanPaste() => Clipboard.Count > 0;

        public void DeleteSelection()
        {
            if (Selection == null || ActiveWorksheet == null) return;
            for (int row = Selection.StartRow; row <= Selection.EndRow; row++)
            {
                for (int col = Selection.StartColumn; col <= Selection.EndColumn; col++)
                {
                    var cell = ActiveWorksheet.GetCell(row, col);
                    if (cell != null)
                    {
                        cell.Value = null;
                        cell.Formula = null;
                    }
                }
            }
            OnPropertyChanged(nameof(ActiveWorksheet));
        }

        public void SelectAll()
        {
            if (ActiveWorksheet != null)
            {
                Selection = new CellRange(
                    0, 0,
                    ActiveWorksheet.UsedRange.EndRow,
                    ActiveWorksheet.UsedRange.EndColumn);
            }
        }

        public void InsertRow()
        {
        }

        public bool CanInsertRow() => ActiveWorksheet != null;

        public void DeleteRow()
        {
        }

        public bool CanDeleteRow() => ActiveWorksheet != null && ActiveCellRow >= 0;

        public void InsertColumn()
        {
        }

        public bool CanInsertColumn() => ActiveWorksheet != null;

        public void DeleteColumn()
        {
        }

        public bool CanDeleteColumn() => ActiveWorksheet != null && ActiveCellColumn >= 0;

        public void MergeCells()
        {
        }

        public bool CanMergeCells() => Selection != null && Selection.RowCount > 1 || Selection.ColumnCount > 1;

        public void UnmergeCells()
        {
        }

        public bool CanUnmergeCells() => ActiveWorksheet != null && ActiveWorksheet.MergedCells.Count > 0;

        public void ShowFormatDialog()
        {
        }

        public void ClearContents()
        {
            DeleteSelection();
            StatusMessage = "Contents cleared";
        }

        public void ClearFormats()
        {
            if (Selection == null || ActiveWorksheet == null) return;
            for (int row = Selection.StartRow; row <= Selection.EndRow; row++)
            {
                for (int col = Selection.StartColumn; col <= Selection.EndColumn; col++)
                {
                    var cell = ActiveWorksheet.GetCell(row, col);
                    if (cell != null)
                        cell.Format = new CellFormat();
                }
            }
            OnPropertyChanged(nameof(ActiveWorksheet));
            StatusMessage = "Formats cleared";
        }

        public void SortAscending()
        {
        }

        public void SortDescending()
        {
        }

        public bool CanSort() => Selection != null && Selection.RowCount > 1;

        public void AddWorksheet()
        {
            var name = $"Sheet{Document.Worksheets.Count + 1}";
            var worksheet = Document.AddWorksheet(name);
            ActiveWorksheet = worksheet;
            OnPropertyChanged(nameof(Document));
            StatusMessage = $"Worksheet '{name}' added";
        }

        public void RemoveWorksheet()
        {
        }

        public bool CanRemoveWorksheet() => Document.Worksheets.Count > 1;

        public void RenameWorksheet()
        {
        }

        public void ZoomIn()
        {
            ZoomLevel = Math.Min(400, ZoomLevel + 10);
        }

        public void ZoomOut()
        {
            ZoomLevel = Math.Max(10, ZoomLevel - 10);
        }

        public void SetCellValue(object value)
        {
            if (ActiveWorksheet == null) return;
            var cell = ActiveWorksheet.GetOrCreateCell(ActiveCellRow, ActiveCellColumn);
            cell.Value = value;
            if (value is string s && s.StartsWith("="))
                cell.Formula = s;
            OnPropertyChanged(nameof(ActiveWorksheet));
        }

        public void MoveSelection(int rowOffset, int columnOffset)
        {
            int newRow = Math.Max(0, ActiveCellRow + rowOffset);
            int newCol = Math.Max(0, ActiveCellColumn + columnOffset);
            ActiveCellRow = newRow;
            ActiveCellColumn = newCol;
            UpdateSelectedCell();
        }

        private void UpdateSelectedCell()
        {
            if (ActiveWorksheet != null)
            {
                SelectedCell = ActiveWorksheet.GetCell(ActiveCellRow, ActiveCellColumn);
                if (SelectedCell != null)
                    OnPropertyChanged(nameof(SelectedCell));
            }
        }

        public void ApplyNumberFormat(string format)
        {
            if (Selection == null || ActiveWorksheet == null) return;
            for (int row = Selection.StartRow; row <= Selection.EndRow; row++)
            {
                for (int col = Selection.StartColumn; col <= Selection.EndColumn; col++)
                {
                    var cell = ActiveWorksheet.GetOrCreateCell(row, col);
                    cell.Format.NumberFormat = format;
                }
            }
            OnPropertyChanged(nameof(ActiveWorksheet));
        }

        public void ApplyFontFormat(string fontFamily, double fontSize, bool bold, bool italic)
        {
            if (Selection == null || ActiveWorksheet == null) return;
            for (int row = Selection.StartRow; row <= Selection.EndRow; row++)
            {
                for (int col = Selection.StartColumn; col <= Selection.EndColumn; col++)
                {
                    var cell = ActiveWorksheet.GetOrCreateCell(row, col);
                    cell.Format.FontFamily = fontFamily;
                    cell.Format.FontSize = fontSize;
                    cell.Format.Bold = bold;
                    cell.Format.Italic = italic;
                }
            }
            OnPropertyChanged(nameof(ActiveWorksheet));
        }

        public void ApplyAlignment(System.Windows.HorizontalAlignment horizontal, System.Windows.VerticalAlignment vertical)
        {
            if (Selection == null || ActiveWorksheet == null) return;
            for (int row = Selection.StartRow; row <= Selection.EndRow; row++)
            {
                for (int col = Selection.StartColumn; col <= Selection.EndColumn; col++)
                {
                    var cell = ActiveWorksheet.GetOrCreateCell(row, col);
                    cell.Format.HorizontalAlignment = horizontal;
                    cell.Format.VerticalAlignment = vertical;
                }
            }
            OnPropertyChanged(nameof(ActiveWorksheet));
        }

        public override void Initialize()
        {
            NewDocument();
        }

        public override void Cleanup()
        {
            Document = null;
            ActiveWorksheet = null;
            SelectedCell = null;
            Clipboard.Clear();
            UndoRedoManager.Clear();
        }
    }

    public class FormulaEngine
    {
        private readonly Dictionary<string, IFormulaFunction> _functions = new Dictionary<string, IFormulaFunction>();

        public FormulaEngine()
        {
            RegisterBuiltInFunctions();
        }

        private void RegisterBuiltInFunctions()
        {
            _functions["SUM"] = new SumFunction();
            _functions["AVERAGE"] = new AverageFunction();
            _functions["COUNT"] = new CountFunction();
            _functions["MAX"] = new MaxFunction();
            _functions["MIN"] = new MinFunction();
            _functions["IF"] = new IfFunction();
            _functions["AND"] = new AndFunction();
            _functions["OR"] = new OrFunction();
            _functions["NOT"] = new NotFunction();
            _functions["CONCATENATE"] = new ConcatenateFunction();
            _functions["LEFT"] = new LeftFunction();
            _functions["RIGHT"] = new RightFunction();
            _functions["MID"] = new MidFunction();
            _functions["LEN"] = new LenFunction();
            _functions["UPPER"] = new UpperFunction();
            _functions["LOWER"] = new LowerFunction();
            _functions["TRIM"] = new TrimFunction();
            _functions["ROUND"] = new RoundFunction();
            _functions["SQRT"] = new SqrtFunction();
            _functions["POWER"] = new PowerFunction();
            _functions["ABS"] = new AbsFunction();
            _functions["TODAY"] = new TodayFunction();
            _functions["NOW"] = new NowFunction();
        }

        public double Evaluate(string formula, CellReferenceResolver resolver)
        {
            if (string.IsNullOrEmpty(formula) || !formula.StartsWith("="))
                return 0;

            var expression = formula.Substring(1).Trim();
            return EvaluateExpression(expression, resolver);
        }

        private double EvaluateExpression(string expression, CellReferenceResolver resolver)
        {
            expression = expression.Trim();

            foreach (var func in _functions)
            {
                if (expression.StartsWith(func.Key + "(", StringComparison.OrdinalIgnoreCase))
                {
                    return func.Value.Execute(expression, resolver, this);
                }
            }

            return ParseSimpleExpression(expression, resolver);
        }

        private double ParseSimpleExpression(string expression, CellReferenceResolver resolver)
        {
            if (TryParseNumber(expression, out double number))
                return number;

            if (expression.Contains("+"))
            {
                var parts = expression.Split('+');
                return parts.Sum(p => ParseSimpleExpression(p.Trim(), resolver));
            }

            if (expression.Contains("-") && !expression.StartsWith("-"))
            {
                var parts = expression.Split('-');
                double result = ParseSimpleExpression(parts[0].Trim(), resolver);
                for (int i = 1; i < parts.Length; i++)
                    result -= ParseSimpleExpression(parts[i].Trim(), resolver);
                return result;
            }

            if (expression.Contains("*"))
            {
                var parts = expression.Split('*');
                double result = 1;
                foreach (var p in parts)
                    result *= ParseSimpleExpression(p.Trim(), resolver);
                return result;
            }

            if (expression.Contains("/"))
            {
                var parts = expression.Split('/');
                double result = ParseSimpleExpression(parts[0].Trim(), resolver);
                for (int i = 1; i < parts.Length; i++)
                    result /= ParseSimpleExpression(parts[i].Trim(), resolver);
                return result;
            }

            if (expression.Contains(":"))
            {
                return EvaluateRangeExpression(expression, resolver);
            }

            return resolver.GetCellValue(expression);
        }

        private double EvaluateRangeExpression(string expression, CellReferenceResolver resolver)
        {
            var parts = expression.Split(':');
            if (parts.Length == 2)
            {
                var start = CellReference.Parse(parts[0]);
                var end = CellReference.Parse(parts[1]);
                return resolver.GetRangeValues(start, end).Sum();
            }
            return 0;
        }

        private bool TryParseNumber(string value, out double result)
        {
            return double.TryParse(value, out result);
        }
    }

    public interface IFormulaFunction
    {
        double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine);
    }

    public interface CellReferenceResolver
    {
        double GetCellValue(string reference);
        List<double> GetRangeValues(CellReference start, CellReference end);
    }

    public class SumFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "SUM");
            double sum = 0;
            foreach (var arg in args)
            {
                if (arg.Contains(":"))
                    sum += resolver.GetRangeValues(
                        CellReference.Parse(arg.Split(':')[0]),
                        CellReference.Parse(arg.Split(':')[1])).Sum();
                else
                    sum += resolver.GetCellValue(arg);
            }
            return sum;
        }
    }

    public class AverageFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "AVERAGE");
            List<double> values = new List<double>();
            foreach (var arg in args)
            {
                if (arg.Contains(":"))
                    values.AddRange(resolver.GetRangeValues(
                        CellReference.Parse(arg.Split(':')[0]),
                        CellReference.Parse(arg.Split(':')[1])));
                else
                    values.Add(resolver.GetCellValue(arg));
            }
            return values.Count > 0 ? values.Average() : 0;
        }
    }

    public class CountFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "COUNT");
            int count = 0;
            foreach (var arg in args)
            {
                if (arg.Contains(":"))
                    count += resolver.GetRangeValues(
                        CellReference.Parse(arg.Split(':')[0]),
                        CellReference.Parse(arg.Split(':')[1])).Count;
                else if (double.TryParse(arg, out _))
                    count++;
            }
            return count;
        }
    }

    public class MaxFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "MAX");
            List<double> values = new List<double>();
            foreach (var arg in args)
            {
                if (arg.Contains(":"))
                    values.AddRange(resolver.GetRangeValues(
                        CellReference.Parse(arg.Split(':')[0]),
                        CellReference.Parse(arg.Split(':')[1])));
                else
                    values.Add(resolver.GetCellValue(arg));
            }
            return values.Count > 0 ? values.Max() : 0;
        }
    }

    public class MinFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "MIN");
            List<double> values = new List<double>();
            foreach (var arg in args)
            {
                if (arg.Contains(":"))
                    values.AddRange(resolver.GetRangeValues(
                        CellReference.Parse(arg.Split(':')[0]),
                        CellReference.Parse(arg.Split(':')[1])));
                else
                    values.Add(resolver.GetCellValue(arg));
            }
            return values.Count > 0 ? values.Min() : 0;
        }
    }

    public class IfFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "IF");
            if (args.Count < 3) return 0;
            double condition = engine.Evaluate("=" + args[0], resolver);
            if (condition != 0)
                return engine.Evaluate("=" + args[1], resolver);
            return engine.Evaluate("=" + args[2], resolver);
        }
    }

    public class AndFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "AND");
            foreach (var arg in args)
            {
                double value = engine.Evaluate("=" + arg, resolver);
                if (value == 0) return 0;
            }
            return 1;
        }
    }

    public class OrFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "OR");
            foreach (var arg in args)
            {
                double value = engine.Evaluate("=" + arg, resolver);
                if (value != 0) return 1;
            }
            return 0;
        }
    }

    public class NotFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "NOT");
            if (args.Count == 0) return 1;
            double value = engine.Evaluate("=" + args[0], resolver);
            return value == 0 ? 1 : 0;
        }
    }

    public class ConcatenateFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "CONCATENATE");
            return 0;
        }
    }

    public class LeftFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine) => 0;
    }

    public class RightFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine) => 0;
    }

    public class MidFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine) => 0;
    }

    public class LenFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine) => 0;
    }

    public class UpperFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine) => 0;
    }

    public class LowerFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine) => 0;
    }

    public class TrimFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine) => 0;
    }

    public class RoundFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "ROUND");
            if (args.Count < 2) return 0;
            double number = engine.Evaluate("=" + args[0], resolver);
            int digits = (int)engine.Evaluate("=" + args[1], resolver);
            double multiplier = Math.Pow(10, digits);
            return Math.Round(number * multiplier) / multiplier;
        }
    }

    public class SqrtFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "SQRT");
            if (args.Count == 0) return 0;
            double value = engine.Evaluate("=" + args[0], resolver);
            return Math.Sqrt(value);
        }
    }

    public class PowerFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "POWER");
            if (args.Count < 2) return 0;
            double baseNum = engine.Evaluate("=" + args[0], resolver);
            double exponent = engine.Evaluate("=" + args[1], resolver);
            return Math.Pow(baseNum, exponent);
        }
    }

    public class AbsFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            var args = ExtractArguments(expression, "ABS");
            if (args.Count == 0) return 0;
            double value = engine.Evaluate("=" + args[0], resolver);
            return Math.Abs(value);
        }
    }

    public class TodayFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            return DateTime.Today.Ticks / TimeSpan.TicksPerDay;
        }
    }

    public class NowFunction : IFormulaFunction
    {
        public double Execute(string expression, CellReferenceResolver resolver, FormulaEngine engine)
        {
            return DateTime.Now.Ticks / TimeSpan.TicksPerDay;
        }
    }

    private List<string> ExtractArguments(string expression, string functionName)
    {
        var result = new List<string>();
        int start = expression.IndexOf(functionName + "(", StringComparison.OrdinalIgnoreCase) + functionName.Length + 1;
        int end = expression.LastIndexOf(')');
        if (end <= start) return result;

        string args = expression.Substring(start, end - start);
        int parenCount = 0;
        int argStart = 0;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == '(') parenCount++;
            else if (args[i] == ')') parenCount--;
            else if (args[i] == ',' && parenCount == 0)
            {
                result.Add(args.Substring(argStart, i - argStart).Trim());
                argStart = i + 1;
            }
        }

        if (argStart < args.Length)
            result.Add(args.Substring(argStart).Trim());

        return result;
    }
}

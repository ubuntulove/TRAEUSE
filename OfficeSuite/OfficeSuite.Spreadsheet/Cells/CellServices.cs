using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace OfficeSuite.Spreadsheet.Cells
{
    public class CellManager
    {
        private readonly Dictionary<string, Cell> _cells = new Dictionary<string, Cell>();
        private readonly Worksheet _worksheet;

        public CellManager(Worksheet worksheet)
        {
            _worksheet = worksheet;
        }

        public Cell GetCell(int row, int column)
        {
            string key = CellReference.GetKey(row, column);
            if (_cells.TryGetValue(key, out var cell))
                return cell;
            return null;
        }

        public Cell GetOrCreateCell(int row, int column)
        {
            string key = CellReference.GetKey(row, column);
            if (!_cells.TryGetValue(key, out var cell))
            {
                cell = new Cell { Row = row, Column = column };
                _cells[key] = cell;
            }
            return cell;
        }

        public void SetCellValue(int row, int column, object value)
        {
            var cell = GetOrCreateCell(row, column);
            cell.Value = value;
        }

        public void DeleteCell(int row, int column)
        {
            string key = CellReference.GetKey(row, column);
            _cells.Remove(key);
        }

        public IEnumerable<Cell> GetCellsInRange(int startRow, int startColumn, int endRow, int endColumn)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startColumn; col <= endColumn; col++)
                {
                    var cell = GetCell(row, col);
                    if (cell != null)
                        yield return cell;
                }
            }
        }

        public IEnumerable<Cell> GetNonEmptyCells()
        {
            return _cells.Values.Where(c => c.Value != null && c.ValueType != CellValueType.Empty);
        }

        public IEnumerable<Cell> GetCellsWithFormula()
        {
            return _cells.Values.Where(c => !string.IsNullOrEmpty(c.Formula));
        }

        public void ClearAllCells()
        {
            _cells.Clear();
        }

        public int Count => _cells.Count;

        public IEnumerable<Cell> Cells => _cells.Values;
    }

    public class FormulaParser
    {
        private readonly FormulaEngine _engine;

        public FormulaParser(FormulaEngine engine)
        {
            _engine = engine;
        }

        public Formula Parse(string formula)
        {
            if (string.IsNullOrEmpty(formula) || !formula.StartsWith("="))
                return null;

            var parsed = new Formula
            {
                OriginalText = formula,
                Expression = formula.Substring(1).Trim()
            };

            ParseExpression(parsed);
            return parsed;
        }

        private void ParseExpression(Formula formula)
        {
            var tokens = Tokenize(formula.Expression);
            formula.Tokens = tokens;
            formula.Type = DetermineFormulaType(tokens);
        }

        private List<FormulaToken> Tokenize(string expression)
        {
            var tokens = new List<FormulaToken>();
            int i = 0;

            while (i < expression.Length)
            {
                char c = expression[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                if (char.IsDigit(c) || (c == '.' && i + 1 < expression.Length && char.IsDigit(expression[i + 1])))
                {
                    tokens.Add(ReadNumber(expression, ref i));
                }
                else if (c == '"')
                {
                    tokens.Add(ReadString(expression, ref i));
                }
                else if (char.IsLetter(c) || c == '_' || c == '$')
                {
                    tokens.Add(ReadIdentifier(expression, ref i));
                }
                else if (c == '(')
                {
                    tokens.Add(new FormulaToken { Type = TokenType.LeftParen, Value = "(" });
                    i++;
                }
                else if (c == ')')
                {
                    tokens.Add(new FormulaToken { Type = TokenType.RightParen, Value = ")" });
                    i++;
                }
                else if (c == ',')
                {
                    tokens.Add(new FormulaToken { Type = TokenType.Comma, Value = "," });
                    i++;
                }
                else if (c == ':')
                {
                    tokens.Add(new FormulaToken { Type = TokenType.Range, Value = ":" });
                    i++;
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '&' || c == '=' || c == '<' || c == '>' || c == '!')
                {
                    tokens.Add(ReadOperator(expression, ref i));
                }
                else
                {
                    i++;
                }
            }

            return tokens;
        }

        private FormulaToken ReadNumber(string expression, ref int index)
        {
            var sb = new StringBuilder();
            while (index < expression.Length && (char.IsDigit(expression[index]) || expression[index] == '.'))
            {
                sb.Append(expression[index]);
                index++;
            }
            return new FormulaToken { Type = TokenType.Number, Value = sb.ToString() };
        }

        private FormulaToken ReadString(string expression, ref int index)
        {
            var sb = new StringBuilder();
            index++;
            while (index < expression.Length && expression[index] != '"')
            {
                sb.Append(expression[index]);
                index++;
            }
            index++;
            return new FormulaToken { Type = TokenType.String, Value = sb.ToString() };
        }

        private FormulaToken ReadIdentifier(string expression, ref int index)
        {
            var sb = new StringBuilder();
            while (index < expression.Length && (char.IsLetterOrDigit(expression[index]) || expression[index] == '_' || expression[index] == '$' || expression[index] == '.'))
            {
                sb.Append(expression[index]);
                index++;
            }
            string value = sb.ToString().ToUpper();
            if (value == "TRUE" || value == "FALSE")
                return new FormulaToken { Type = TokenType.Boolean, Value = value };
            if (index < expression.Length && expression[index] == '(')
                return new FormulaToken { Type = TokenType.Function, Value = value };
            if (sb.ToString().Contains(":") || char.IsDigit(sb.ToString().LastOrDefault()))
                return new FormulaToken { Type = TokenType.Reference, Value = sb.ToString() };
            return new FormulaToken { Type = TokenType.Name, Value = value };
        }

        private FormulaToken ReadOperator(string expression, ref int index)
        {
            char c = expression[index];
            string op = c.ToString();
            TokenType type = TokenType.Operator;

            if (c == '<' || c == '>')
            {
                if (index + 1 < expression.Length && expression[index + 1] == '=')
                {
                    op += "=";
                    index++;
                }
            }
            else if (c == '=')
            {
                type = TokenType.Comparison;
            }
            else if (c == '!')
            {
                type = TokenType.NotEqual;
            }

            index++;
            return new FormulaToken { Type = type, Value = op };
        }

        private FormulaType DetermineFormulaType(List<FormulaToken> tokens)
        {
            if (tokens.Any(t => t.Type == TokenType.Function))
                return FormulaType.Function;
            if (tokens.Any(t => t.Type == TokenType.Reference))
                return FormulaType.Reference;
            if (tokens.Any(t => t.Type == TokenType.Comparison))
                return FormulaType.Comparison;
            if (tokens.Any(t => t.Type == TokenType.Operator))
                return FormulaType.Arithmetic;
            return FormulaType.Value;
        }

        public List<string> ExtractReferences(Formula formula)
        {
            var references = new List<string>();
            foreach (var token in formula.Tokens.Where(t => t.Type == TokenType.Reference))
            {
                references.Add(token.Value);
            }
            return references;
        }

        public List<string> ExtractFunctions(Formula formula)
        {
            var functions = new List<string>();
            foreach (var token in formula.Tokens.Where(t => t.Type == TokenType.Function))
            {
                functions.Add(token.Value);
            }
            return functions;
        }
    }

    public class Formula
    {
        public string OriginalText { get; set; }
        public string Expression { get; set; }
        public FormulaType Type { get; set; }
        public List<FormulaToken> Tokens { get; set; }
    }

    public class FormulaToken
    {
        public TokenType Type { get; set; }
        public string Value { get; set; }
    }

    public enum FormulaType
    {
        Value,
        Arithmetic,
        Comparison,
        Function,
        Reference,
        Text
    }

    public enum TokenType
    {
        Number,
        String,
        Boolean,
        Reference,
        Function,
        Operator,
        Comparison,
        NotEqual,
        LeftParen,
        RightParen,
        Comma,
        Range,
        Name
    }

    public class FormulaEvaluator
    {
        private readonly FormulaEngine _engine;
        private readonly CellReferenceResolver _resolver;

        public FormulaEvaluator(FormulaEngine engine, CellReferenceResolver resolver)
        {
            _engine = engine;
            _resolver = resolver;
        }

        public double Evaluate(Formula formula)
        {
            return _engine.Evaluate(formula.OriginalText, _resolver);
        }

        public string EvaluateAsText(Formula formula)
        {
            double result = Evaluate(formula);
            return result.ToString();
        }

        public bool EvaluateAsBoolean(Formula formula)
        {
            double result = Evaluate(formula);
            return result != 0;
        }
    }

    public class ConditionalFormat
    {
        public string Id { get; set; }
        public CellRange Range { get; set; }
        public ConditionalFormatType Type { get; set; }
        public string Formula1 { get; set; }
        public string Formula2 { get; set; }
        public CellFormat Format { get; set; }
        public bool StopIfTrue { get; set; }
        public int Priority { get; set; }

        public ConditionalFormat()
        {
            Id = Guid.NewGuid().ToString();
            StopIfTrue = false;
            Priority = 1;
        }

        public bool Evaluate(Cell cell)
        {
            return Type switch
            {
                ConditionalFormatType.Expression => EvaluateExpression(cell),
                ConditionalFormatType.CellValue => EvaluateCellValue(cell),
                ConditionalFormatType.ColorScale => EvaluateColorScale(cell),
                ConditionalFormatType.DataBar => EvaluateDataBar(cell),
                ConditionalFormatType.IconSet => EvaluateIconSet(cell),
                _ => false
            };
        }

        private bool EvaluateExpression(Cell cell)
        {
            return false;
        }

        private bool EvaluateCellValue(Cell cell)
        {
            if (cell == null || !double.TryParse(cell.Value?.ToString(), out double cellValue))
                return false;

            if (double.TryParse(Formula1, out double value1))
            {
                return Type switch
                {
                    ConditionalFormatType.Between => cellValue >= value1 && cellValue <= double.Parse(Formula2),
                    ConditionalFormatType.NotBetween => cellValue < value1 || cellValue > double.Parse(Formula2),
                    ConditionalFormatType.EqualTo => cellValue == value1,
                    ConditionalFormatType.NotEqualTo => cellValue != value1,
                    ConditionalFormatType.GreaterThan => cellValue > value1,
                    ConditionalFormatType.LessThan => cellValue < value1,
                    ConditionalFormatType.GreaterThanOrEqual => cellValue >= value1,
                    ConditionalFormatType.LessThanOrEqual => cellValue <= value1,
                    _ => false
                };
            }

            return false;
        }

        private bool EvaluateColorScale(Cell cell) => false;
        private bool EvaluateDataBar(Cell cell) => false;
        private bool EvaluateIconSet(Cell cell) => false;
    }

    public enum ConditionalFormatType
    {
        Expression,
        CellValue,
        Between,
        NotBetween,
        EqualTo,
        NotEqualTo,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        ColorScale,
        DataBar,
        IconSet,
        TopN,
        BottomN,
        Unique,
        Duplicate,
        ContainsText,
        NotContainsText,
        BeginsWith,
        EndsWith
    }

    public class ConditionalFormatManager
    {
        private readonly List<ConditionalFormat> _formats = new List<ConditionalFormat>();
        private readonly Worksheet _worksheet;

        public ConditionalFormatManager(Worksheet worksheet)
        {
            _worksheet = worksheet;
        }

        public void AddFormat(ConditionalFormat format)
        {
            _formats.Add(format);
        }

        public void RemoveFormat(string id)
        {
            _formats.RemoveAll(f => f.Id == id);
        }

        public IEnumerable<ConditionalFormat> GetFormatsForCell(int row, int column)
        {
            return _formats.Where(f => f.Range.Contains(row, column));
        }

        public void ClearAllFormats()
        {
            _formats.Clear();
        }

        public void ClearFormatsForRange(CellRange range)
        {
            _formats.RemoveAll(f => f.Range.StartRow >= range.StartRow && 
                                   f.Range.EndRow <= range.EndRow &&
                                   f.Range.StartColumn >= range.StartColumn && 
                                   f.Range.EndColumn <= range.EndColumn);
        }
    }

    public class DataValidation
    {
        public string Id { get; set; }
        public CellRange Range { get; set; }
        public DataValidationType Type { get; set; }
        public string Formula1 { get; set; }
        public string Formula2 { get; set; }
        public DataValidationOperator Operator { get; set; }
        public bool IgnoreBlank { get; set; }
        public bool InCellDropDown { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationErrorStyle ErrorStyle { get; set; }
        public string InputTitle { get; set; }
        public string InputMessage { get; set; }

        public DataValidation()
        {
            Id = Guid.NewGuid().ToString();
            IgnoreBlank = true;
            InCellDropDown = true;
            ErrorStyle = ValidationErrorStyle.Stop;
        }

        public ValidationResult Validate(object value)
        {
            return Type switch
            {
                DataValidationType.AnyValue => ValidationResult.Valid(),
                DataValidationType.WholeNumber => ValidateWholeNumber(value),
                DataValidationType.Decimal => ValidateDecimal(value),
                DataValidationType.List => ValidateList(value),
                DataValidationType.Date => ValidateDate(value),
                DataValidationType.Time => ValidateTime(value),
                DataValidationType.TextLength => ValidateTextLength(value),
                DataValidationType.Custom => ValidateCustom(value),
                _ => ValidationResult.Valid()
            };
        }

        private ValidationResult ValidateWholeNumber(object value)
        {
            if (value == null && IgnoreBlank)
                return ValidationResult.Valid();

            if (!int.TryParse(value?.ToString(), out int intValue))
                return ValidationResult.Invalid("Value must be a whole number");

            return ApplyOperator(intValue);
        }

        private ValidationResult ValidateDecimal(object value)
        {
            if (value == null && IgnoreBlank)
                return ValidationResult.Valid();

            if (!double.TryParse(value?.ToString(), out double dblValue))
                return ValidationResult.Invalid("Value must be a decimal number");

            return ApplyOperator(dblValue);
        }

        private ValidationResult ValidateList(object value)
        {
            if (value == null && IgnoreBlank)
                return ValidationResult.Valid();

            var values = Formula1.Split(',');
            if (!values.Contains(value?.ToString(), StringComparer.OrdinalIgnoreCase))
                return ValidationResult.Invalid("Value is not in the list");

            return ValidationResult.Valid();
        }

        private ValidationResult ValidateDate(object value)
        {
            if (value == null && IgnoreBlank)
                return ValidationResult.Valid();

            if (!DateTime.TryParse(value?.ToString(), out DateTime dateValue))
                return ValidationResult.Invalid("Value must be a valid date");

            return ApplyOperator(dateValue.Ticks);
        }

        private ValidationResult ValidateTime(object value)
        {
            if (value == null && IgnoreBlank)
                return ValidationResult.Valid();

            if (!TimeSpan.TryParse(value?.ToString(), out TimeSpan timeValue))
                return ValidationResult.Invalid("Value must be a valid time");

            return ApplyOperator(timeValue.TotalSeconds);
        }

        private ValidationResult ValidateTextLength(object value)
        {
            if (value == null && IgnoreBlank)
                return ValidationResult.Valid();

            int length = value.ToString()?.Length ?? 0;
            return ApplyOperator(length);
        }

        private ValidationResult ValidateCustom(object value)
        {
            return ValidationResult.Valid();
        }

        private ValidationResult ApplyOperator(double value)
        {
            double? value1 = double.TryParse(Formula1, out double v1) ? v1 : null;
            double? value2 = double.TryParse(Formula2, out double v2) ? v2 : null;

            bool result = Operator switch
            {
                DataValidationOperator.Between => value1.HasValue && value2.HasValue && value >= value1 && value <= value2,
                DataValidationOperator.NotBetween => value1.HasValue && value2.HasValue && (value < value1 || value > value2),
                DataValidationOperator.EqualTo => value1.HasValue && value == value1,
                DataValidationOperator.NotEqualTo => value1.HasValue && value != value1,
                DataValidationOperator.GreaterThan => value1.HasValue && value > value1,
                DataValidationOperator.LessThan => value1.HasValue && value < value1,
                DataValidationOperator.GreaterThanOrEqual => value1.HasValue && value >= value1,
                DataValidationOperator.LessThanOrEqual => value1.HasValue && value <= value1,
                _ => true
            };

            return result ? ValidationResult.Valid() : ValidationResult.Invalid(GetErrorMessage());
        }

        private string GetErrorMessage()
        {
            return ErrorMessage ?? "The value does not meet the validation criteria";
        }
    }

    public enum DataValidationType
    {
        AnyValue,
        WholeNumber,
        Decimal,
        List,
        Date,
        Time,
        TextLength,
        Custom
    }

    public enum DataValidationOperator
    {
        Between,
        NotBetween,
        EqualTo,
        NotEqualTo,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public enum ValidationErrorStyle
    {
        Stop,
        Warning,
        Information
    }

    public class ValidationResult
    {
        public bool IsValid { get; private set; }
        public string ErrorMessage { get; private set; }

        public static ValidationResult Valid() => new ValidationResult { IsValid = true };
        public static ValidationResult Invalid(string message) => new ValidationResult { IsValid = false, ErrorMessage = message };
    }

    public class DataValidationManager
    {
        private readonly List<DataValidation> _validations = new List<DataValidation>();
        private readonly Worksheet _worksheet;

        public DataValidationManager(Worksheet worksheet)
        {
            _worksheet = worksheet;
        }

        public void AddValidation(DataValidation validation)
        {
            _validations.Add(validation);
        }

        public void RemoveValidation(string id)
        {
            _validations.RemoveAll(v => v.Id == id);
        }

        public DataValidation GetValidationForCell(int row, int column)
        {
            return _validations.FirstOrDefault(v => v.Range.Contains(row, column));
        }

        public ValidationResult ValidateCell(int row, int column, object value)
        {
            var validation = GetValidationForCell(row, column);
            if (validation == null)
                return ValidationResult.Valid();

            return validation.Validate(value);
        }

        public void ClearAllValidations()
        {
            _validations.Clear();
        }
    }

    public class TableManager
    {
        private readonly List<ExcelTable> _tables = new List<ExcelTable>();
        private readonly Worksheet _worksheet;

        public TableManager(Worksheet worksheet)
        {
            _worksheet = worksheet;
        }

        public ExcelTable CreateTable(CellRange range, string name)
        {
            var table = new ExcelTable
            {
                Name = name,
                Range = range,
                Id = Guid.NewGuid().ToString()
            };

            InitializeTableColumns(table, range);
            _tables.Add(table);
            return table;
        }

        private void InitializeTableColumns(ExcelTable table, CellRange range)
        {
            for (int col = range.StartColumn; col <= range.EndColumn; col++)
            {
                var header = _worksheet.GetCell(range.StartRow, col);
                table.Columns.Add(new TableColumn
                {
                    Name = header?.Value?.ToString() ?? $"Column{col + 1}",
                    Index = col - range.StartColumn
                });
            }
        }

        public ExcelTable GetTable(string name)
        {
            return _tables.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public ExcelTable GetTableContaining(int row, int column)
        {
            return _tables.FirstOrDefault(t => t.Range.Contains(row, column));
        }

        public void DeleteTable(string name)
        {
            _tables.RemoveAll(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ExcelTable> Tables => _tables.AsReadOnly();
    }

    public class ExcelTable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CellRange Range { get; set; }
        public List<TableColumn> Columns { get; } = new List<TableColumn>();
        public List<TableStyle> Styles { get; } = new List<TableStyle>();
        public TableStyle Style { get; set; }
        public bool ShowHeaderRow { get; set; } = true;
        public bool ShowTotalRow { get; set; }
        public bool ShowBandedRows { get; set; } = true;
        public bool ShowBandedColumns { get; set; }
        public bool ShowFirstColumn { get; set; }
        public bool ShowLastColumn { get; set; }

        public ExcelTable()
        {
            Style = new TableStyle { Name = "TableStyleMedium2" };
        }
    }

    public class TableColumn
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public string Formula { get; set; }
        public TotalFunction TotalFunction { get; set; }
        public CellFormat Format { get; set; }
    }

    public enum TotalFunction
    {
        None,
        Sum,
        Average,
        Count,
        CountNums,
        Max,
        Min,
        StdDev,
        Var,
        Custom
    }

    public class TableStyle
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public TableStyleType Type { get; set; }
    }

    public enum TableStyleType
    {
        TableStyleLight1,
        TableStyleMedium1,
        TableStyleMedium2,
        TableStyleMedium3,
        TableStyleMedium4,
        TableStyleMedium5,
        TableStyleMedium6,
        TableStyleMedium7,
        TableStyleMedium8,
        TableStyleMedium9,
        TableStyleMedium10,
        TableStyleMedium11,
        TableStyleMedium12,
        TableStyleMedium13,
        TableStyleMedium14,
        TableStyleMedium15,
        TableStyleMedium16,
        TableStyleMedium17,
        TableStyleMedium18,
        TableStyleMedium19,
        TableStyleMedium20,
        TableStyleMedium21,
        TableStyleMedium22,
        TableStyleMedium23,
        TableStyleMedium24,
        TableStyleMedium25,
        TableStyleMedium26,
        TableStyleMedium27,
        TableStyleMedium28,
        TableStyleDark1,
        TableStyleDark2,
        TableStyleDark3,
        TableStyleDark4,
        TableStyleDark5,
        TableStyleDark6,
        TableStyleDark7,
        TableStyleDark8,
        TableStyleDark9,
        TableStyleDark10,
        TableStyleDark11
    }

    public class PivotTable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CellRange SourceRange { get; set; }
        public CellRange TargetRange { get; set; }
        public List<PivotField> RowFields { get; } = new List<PivotField>();
        public List<PivotField> ColumnFields { get; } = new List<PivotField>();
        public List<PivotField> DataFields { get; } = new List<PivotField>();
        public List<PivotField> FilterFields { get; } = new List<PivotField>();
        public PivotTableStyle Style { get; set; }

        public PivotTable()
        {
            Id = Guid.NewGuid().ToString();
            Style = new PivotTableStyle { Name = "PivotStyleMedium2" };
        }
    }

    public class PivotField
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public PivotFieldOrientation Orientation { get; set; }
        public PivotCalculation Function { get; set; }
        public string CustomName { get; set; }
        public bool ShowAllItems { get; set; }
        public bool IncludeNewItemsInFilter { get; set; }
    }

    public enum PivotFieldOrientation
    {
        RowField,
        ColumnField,
        DataField,
        PageField,
        ReportFilter
    }

    public enum PivotCalculation
    {
        Sum,
        Count,
        Average,
        Max,
        Min,
        Product,
        CountNumbers,
        StdDev,
        StdDevp,
        Var,
        Varp
    }

    public class PivotTableStyle
    {
        public string Name { get; set; }
        public PivotTableStyleType Type { get; set; }
    }

    public enum PivotTableStyleType
    {
        PivotStyleLight1,
        PivotStyleLight2,
        PivotStyleLight3,
        PivotStyleLight4,
        PivotStyleLight5,
        PivotStyleLight6,
        PivotStyleLight7,
        PivotStyleLight8,
        PivotStyleLight9,
        PivotStyleLight10,
        PivotStyleLight11,
        PivotStyleLight12,
        PivotStyleLight13,
        PivotStyleLight14,
        PivotStyleLight15,
        PivotStyleLight16,
        PivotStyleLight17,
        PivotStyleLight18,
        PivotStyleMedium1,
        PivotStyleMedium2,
        PivotStyleMedium3,
        PivotStyleMedium4,
        PivotStyleMedium5,
        PivotStyleMedium6,
        PivotStyleMedium7,
        PivotStyleMedium8,
        PivotStyleMedium9,
        PivotStyleMedium10,
        PivotStyleMedium11,
        PivotStyleMedium12,
        PivotStyleMedium13,
        PivotStyleMedium14,
        PivotStyleMedium15,
        PivotStyleMedium16,
        PivotStyleMedium17,
        PivotStyleMedium18,
        PivotStyleMedium19,
        PivotStyleMedium20,
        PivotStyleMedium21,
        PivotStyleMedium22,
        PivotStyleMedium23,
        PivotStyleMedium24,
        PivotStyleMedium25,
        PivotStyleMedium26,
        PivotStyleMedium27,
        PivotStyleMedium28,
        PivotStyleDark1,
        PivotStyleDark2,
        PivotStyleDark3,
        PivotStyleDark4,
        PivotStyleDark5,
        PivotStyleDark6,
        PivotStyleDark7,
        PivotStyleDark8,
        PivotStyleDark9,
        PivotStyleDark10,
        PivotStyleDark11
    }

    public class Sparkline
    {
        public string Id { get; set; }
        public CellRange DataRange { get; set; }
        public SparklineType Type { get; set; }
        public SparklineStyle Style { get; set; }
        public System.Windows.Media.Color Color { get; set; } = System.Windows.Media.Colors.Blue;
        public System.Windows.Media.Color ColorNegative { get; set; } = System.Windows.Media.Colors.Red;
        public System.Windows.Media.Color ColorMarkers { get; set; } = System.Windows.Media.Colors.Orange;
        public bool ShowMarkers { get; set; }
        public bool ShowNegative { get; set; }
        public bool ShowHighest { get; set; }
        public bool ShowLowest { get; set; }
        public bool ShowFirst { get; set; }
        public bool ShowLast { get; set; }
        public double LineWeight { get; set; } = 1;

        public Sparkline()
        {
            Id = Guid.NewGuid().ToString();
            Type = SparklineType.Line;
            Style = new SparklineStyle { Name = "SparklineStyle1" };
        }
    }

    public enum SparklineType
    {
        Line,
        Column,
        WinLoss
    }

    public class SparklineStyle
    {
        public string Name { get; set; }
    }

    public class ScenarioManager
    {
        private readonly List<Scenario> _scenarios = new List<Scenario>();
        private Scenario _activeScenario;

        public Scenario CreateScenario(string name, Dictionary<string, object> changingCells)
        {
            var scenario = new Scenario
            {
                Name = name,
                ChangingCells = changingCells,
                CreatedAt = DateTime.Now
            };
            _scenarios.Add(scenario);
            return scenario;
        }

        public void ActivateScenario(string name)
        {
            var scenario = _scenarios.FirstOrDefault(s => s.Name == name);
            if (scenario != null)
                _activeScenario = scenario;
        }

        public Scenario ActiveScenario => _activeScenario;
        public IEnumerable<Scenario> Scenarios => _scenarios.AsReadOnly();
    }

    public class Scenario
    {
        public string Name { get; set; }
        public Dictionary<string, object> ChangingCells { get; } = new Dictionary<string, object>();
        public string Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ModifiedAt { get; set; }
    }

    public class GoalSeekService
    {
        public GoalSeekResult Seek(Worksheet worksheet, string targetCellRef, double targetValue, string variableCellRef)
        {
            var result = new GoalSeekResult();
            var variableRef = CellReference.Parse(variableCellRef);
            var targetRef = CellReference.Parse(targetCellRef);

            var targetCell = worksheet.GetCell(targetRef.Row, targetRef.Column);
            if (targetCell == null)
            {
                result.Success = false;
                result.Message = "Target cell not found";
                return result;
            }

            var variableCell = worksheet.GetOrCreateCell(variableRef.Row, variableRef.Column);
            double originalValue = variableCell.GetNumericValue();

            double low = originalValue - 1000;
            double high = originalValue + 1000;
            const int maxIterations = 100;
            const double tolerance = 0.0001;

            for (int i = 0; i < maxIterations; i++)
            {
                double mid = (low + high) / 2;
                variableCell.Value = mid;

                double currentValue = targetCell.GetNumericValue();
                double diff = currentValue - targetValue;

                if (Math.Abs(diff) < tolerance)
                {
                    result.Success = true;
                    result.FoundValue = mid;
                    result.FinalValue = currentValue;
                    result.Iterations = i + 1;
                    return result;
                }

                variableCell.Value = low;
                double lowValue = targetCell.GetNumericValue();
                double lowDiff = lowValue - targetValue;

                if (lowDiff * diff < 0)
                    high = mid;
                else
                    low = mid;
            }

            result.Success = false;
            result.Message = "Could not find solution within tolerance";
            return result;
        }
    }

    public class GoalSeekResult
    {
        public bool Success { get; set; }
        public double FoundValue { get; set; }
        public double FinalValue { get; set; }
        public int Iterations { get; set; }
        public string Message { get; set; }
    }

    public class SolverService
    {
        public SolverResult Solve(SolverProblem problem)
        {
            var result = new SolverResult();

            foreach (var variable in problem.Variables)
            {
                variable.CurrentValue = variable.InitialValue ?? 0;
            }

            for (int i = 0; i < problem.MaxIterations; i++)
            {
                double objectiveValue = EvaluateObjective(problem);

                foreach (var constraint in problem.Constraints)
                {
                    if (!constraint.IsSatisfied())
                    {
                        AdjustVariables(problem);
                        break;
                    }
                }

                if (problem.Constraints.All(c => c.IsSatisfied()))
                {
                    result.Success = true;
                    result.ObjectiveValue = objectiveValue;
                    result.Iterations = i + 1;
                    break;
                }
            }

            return result;
        }

        private double EvaluateObjective(SolverProblem problem)
        {
            return 0;
        }

        private void AdjustVariables(SolverProblem problem)
        {
        }
    }

    public class SolverProblem
    {
        public List<SolverVariable> Variables { get; } = new List<SolverVariable>();
        public List<SolverConstraint> Constraints { get; } = new List<SolverConstraint>();
        public SolverObjective Objective { get; set; }
        public int MaxIterations { get; set; } = 1000;
    }

    public class SolverVariable
    {
        public string Name { get; set; }
        public CellReference Reference { get; set; }
        public double? InitialValue { get; set; }
        public double CurrentValue { get; set; }
        public double MinValue { get; set; } = double.MinValue;
        public double MaxValue { get; set; } = double.MaxValue;
    }

    public class SolverConstraint
    {
        public CellReference Reference { get; set; }
        public ConstraintOperator Operator { get; set; }
        public double Value { get; set; }

        public bool IsSatisfied()
        {
            return true;
        }
    }

    public enum ConstraintOperator
    {
        LessThanOrEqual,
        Equal,
        GreaterThanOrEqual
    }

    public class SolverObjective
    {
        public CellReference Reference { get; set; }
        public SolverObjectiveType Type { get; set; }
    }

    public enum SolverObjectiveType
    {
        Minimize,
        Maximize,
        ValueOf
    }

    public class SolverResult
    {
        public bool Success { get; set; }
        public double ObjectiveValue { get; set; }
        public int Iterations { get; set; }
        public string Message { get; set; }
    }

    public class WhatIfAnalysis
    {
        public DataTableResult CreateDataTable(Worksheet worksheet, CellRange range, string rowInputCell, string columnInputCell, Dictionary<string, object> rowValues, Dictionary<string, object> columnValues)
        {
            var result = new DataTableResult
            {
                Range = range
            };

            return result;
        }

        public List<Scenario> CreateScenarioSummary(ScenarioManager manager, string resultCell)
        {
            var scenarios = new List<Scenario>();
            var originalScenario = manager.ActiveScenario;

            foreach (var scenario in manager.Scenarios)
            {
                manager.ActivateScenario(scenario.Name);
                scenarios.Add(scenario);
            }

            if (originalScenario != null)
                manager.ActivateScenario(originalScenario.Name);

            return scenarios;
        }
    }

    public class DataTableResult
    {
        public CellRange Range { get; set; }
        public double[,] Values { get; set; }
    }
}

namespace OfficeSuite.Spreadsheet.Cells
{
    using OfficeSuite.Core.Models;
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OfficeSuite.Spreadsheet.Formulas
{
    public class ArrayFormulaEngine
    {
        private readonly FormulaParser _parser;
        private readonly Dictionary<string, object> _namedRanges;

        public ArrayFormulaEngine()
        {
            _parser = new FormulaParser();
            _namedRanges = new Dictionary<string, object>();
        }

        public void RegisterNamedRange(string name, object value)
        {
            _namedRanges[name] = value;
        }

        public object Evaluate(string formula, WorksheetContext context)
        {
            if (string.IsNullOrWhiteSpace(formula) || !formula.StartsWith("="))
                return formula;

            try
            {
                var parsedFormula = _parser.Parse(formula.Substring(1));
                return EvaluateExpression(parsedFormula, context);
            }
            catch (Exception ex)
            {
                return new CellErrorResult { Error = CellError.Value, Message = ex.Message };
            }
        }

        private object EvaluateExpression(ParsedFormula formula, WorksheetContext context)
        {
            switch (formula.Type)
            {
                case FormulaType.Number:
                    return formula.Value;
                case FormulaType.Text:
                    return formula.TextValue;
                case FormulaType.Boolean:
                    return formula.BoolValue;
                case FormulaType.CellReference:
                    return context.GetCellValue(formula.CellReference);
                case FormulaType.RangeReference:
                    return context.GetRangeValues(formula.RangeReference);
                case FormulaType.Function:
                    return EvaluateFunction(formula.FunctionName, formula.Arguments, context);
                case FormulaType.Operator:
                    return EvaluateOperator(formula.Operator, formula.Left, formula.Right, context);
                case FormulaType.Array:
                    return EvaluateArrayFormula(formula.ArrayElements, context);
                default:
                    return new CellErrorResult { Error = CellError.Value, Message = "Unknown formula type" };
            }
        }

        private object EvaluateFunction(string functionName, List<ParsedFormula> arguments, WorksheetContext context)
        {
            var args = arguments.Select(a => EvaluateExpression(a, context)).ToList();
            
            switch (functionName.ToUpper())
            {
                case "TRANSPOSE":
                    return TransposeArray(args);
                case "MMULT":
                    return MatrixMultiply(args);
                case "MINVERSE":
                    return MatrixInverse(args);
                case "MDETERM":
                    return MatrixDeterminant(args);
                case "SUMPRODUCT":
                    return SumProduct(args, context);
                case "SUMIFS":
                    return SumIfs(args, context);
                case "COUNTIFS":
                    return CountIfs(args, context);
                case "AVERAGEIFS":
                    return AverageIfs(args, context);
                case "IFERROR":
                    return IfError(args, context);
                case "CHOOSE":
                    return Choose(args, context);
                case "INDEX":
                    return Index(args, context);
                case "MATCH":
                    return Match(args, context);
                case "OFFSET":
                    return Offset(args, context);
                case "INDIRECT":
                    return Indirect(args, context);
                case "ROW":
                    return Row(args, context);
                case "COLUMN":
                    return Column(args, context);
                case "ROWS":
                    return Rows(args, context);
                case "COLUMNS":
                    return Columns(args, context);
                case "ARRAYFORMULA":
                    return ArrayFormula(args, context);
                default:
                    return new CellErrorResult { Error = CellError.Name, Message = $"Unknown function: {functionName}" };
            }
        }

        private object[,] TransposeArray(List<object> args)
        {
            if (args.Count == 0 || !(args[0] is object[,] array))
                return new object[0, 0];

            int rows = array.GetLength(0);
            int cols = array.GetLength(1);
            var result = new object[cols, rows];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[j, i] = array[i, j];
                }
            }

            return result;
        }

        private object[,] MatrixMultiply(List<object> args)
        {
            if (args.Count < 2)
                return new object[0, 0];

            var matrix1 = args[0] as object[,];
            var matrix2 = args[1] as object[,];

            if (matrix1 == null || matrix2 == null)
                return new object[0, 0];

            int rows1 = matrix1.GetLength(0);
            int cols1 = matrix1.GetLength(1);
            int rows2 = matrix2.GetLength(0);
            int cols2 = matrix2.GetLength(1);

            if (cols1 != rows2)
                return new object[0, 0];

            var result = new object[rows1, cols2];

            for (int i = 0; i < rows1; i++)
            {
                for (int j = 0; j < cols2; j++)
                {
                    double sum = 0;
                    for (int k = 0; k < cols1; k++)
                    {
                        sum += ToDouble(matrix1[i, k]) * ToDouble(matrix2[k, j]);
                    }
                    result[i, j] = sum;
                }
            }

            return result;
        }

        private object[,] MatrixInverse(List<object> args)
        {
            if (args.Count == 0 || !(args[0] is object[,] matrix))
                return new object[0, 0];

            int n = matrix.GetLength(0);
            if (matrix.GetLength(1) != n)
                return new object[0, 0];

            var augmented = new double[n, 2 * n];

            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    augmented[i, j] = ToDouble(matrix[i, j]);
                }
                augmented[i, n + i] = 1;
            }

            for (int i = 0; i < n; i++)
            {
                int maxRow = i;
                for (int k = i + 1; k < n; k++)
                {
                    if (Math.Abs(augmented[k, i]) > Math.Abs(augmented[maxRow, i]))
                        maxRow = k;
                }

                for (int j = 0; j < 2 * n; j++)
                {
                    var temp = augmented[i, j];
                    augmented[i, j] = augmented[maxRow, j];
                    augmented[maxRow, j] = temp;
                }

                double divisor = augmented[i, i];
                if (Math.Abs(divisor) < 1e-10)
                    return new object[0, 0];

                for (int j = 0; j < 2 * n; j++)
                    augmented[i, j] /= divisor;

                for (int k = 0; k < n; k++)
                {
                    if (k != i)
                    {
                        double factor = augmented[k, i];
                        for (int j = 0; j < 2 * n; j++)
                            augmented[k, j] -= factor * augmented[i, j];
                    }
                }
            }

            var result = new object[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                    result[i, j] = augmented[i, n + j];
            }

            return result;
        }

        private double MatrixDeterminant(List<object> args)
        {
            if (args.Count == 0 || !(args[0] is object[,] matrix))
                return 0;

            int n = matrix.GetLength(0);
            if (matrix.GetLength(1) != n || n == 0)
                return 0;

            if (n == 1)
                return ToDouble(matrix[0, 0]);

            if (n == 2)
                return ToDouble(matrix[0, 0]) * ToDouble(matrix[1, 1]) -
                       ToDouble(matrix[0, 1]) * ToDouble(matrix[1, 0]);

            double det = 0;
            for (int j = 0; j < n; j++)
            {
                var subMatrix = new object[n - 1, n - 1];
                for (int i = 1; i < n; i++)
                {
                    for (int k = 0; k < n; k++)
                    {
                        if (k < j)
                            subMatrix[i - 1, k] = matrix[i, k];
                        else if (k > j)
                            subMatrix[i - 1, k - 1] = matrix[i, k];
                    }
                }

                var subArgs = new List<object> { subMatrix };
                det += (j % 2 == 0 ? 1 : -1) * ToDouble(matrix[0, j]) * MatrixDeterminant(subArgs);
            }

            return det;
        }

        private double SumProduct(List<object> args, WorksheetContext context)
        {
            if (args.Count < 2)
                return 0;

            var arrays = args.Select(a => GetArrayValues(a)).ToList();
            int maxLength = arrays.Max(a => a.GetLength(0));

            double sum = 0;
            for (int i = 0; i < maxLength; i++)
            {
                double product = 1;
                foreach (var array in arrays)
                {
                    if (i < array.GetLength(0))
                        product *= ToDouble(array[i, 0]);
                }
                sum += product;
            }

            return sum;
        }

        private double SumIfs(List<object> args, WorksheetContext context)
        {
            if (args.Count < 3)
                return 0;

            var sumRange = GetArrayValues(args[0]);
            var criteriaRange1 = GetArrayValues(args[1]);
            var criteria1 = args[2];

            double sum = 0;
            for (int i = 0; i < criteriaRange1.GetLength(0); i++)
            {
                if (MatchesCriteria(criteriaRange1[i, 0], criteria1))
                {
                    sum += ToDouble(sumRange[i, 0]);
                }
            }

            return sum;
        }

        private int CountIfs(List<object> args, WorksheetContext context)
        {
            if (args.Count < 3)
                return 0;

            var criteriaRange1 = GetArrayValues(args[1]);
            var criteria1 = args[2];

            int count = 0;
            for (int i = 0; i < criteriaRange1.GetLength(0); i++)
            {
                if (MatchesCriteria(criteriaRange1[i, 0], criteria1))
                    count++;
            }

            return count;
        }

        private double AverageIfs(List<object> args, WorksheetContext context)
        {
            if (args.Count < 3)
                return 0;

            var avgRange = GetArrayValues(args[0]);
            var criteriaRange1 = GetArrayValues(args[1]);
            var criteria1 = args[2];

            double sum = 0;
            int count = 0;
            for (int i = 0; i < criteriaRange1.GetLength(0); i++)
            {
                if (MatchesCriteria(criteriaRange1[i, 0], criteria1))
                {
                    sum += ToDouble(avgRange[i, 0]);
                    count++;
                }
            }

            return count > 0 ? sum / count : 0;
        }

        private object IfError(List<object> args, WorksheetContext context)
        {
            if (args.Count < 2)
                return null;

            try
            {
                var value = EvaluateExpression(args[0] as ParsedFormula, context);
                if (value is CellErrorResult)
                    return args[1];
                return value;
            }
            catch
            {
                return args.Count > 1 ? args[1] : 0;
            }
        }

        private object Choose(List<object> args, WorksheetContext context)
        {
            if (args.Count < 2)
                return null;

            int index = (int)ToDouble(args[0]);
            if (index < 1 || index >= args.Count)
                return new CellErrorResult { Error = CellError.Value };

            return args[index];
        }

        private object Index(List<object> args, WorksheetContext context)
        {
            if (args.Count < 2)
                return null;

            var array = GetArrayValues(args[0]);
            int row = (int)ToDouble(args[1]);
            int col = args.Count > 2 ? (int)ToDouble(args[2]) : 1;

            if (row < 1 || row > array.GetLength(0) || col < 1 || col > array.GetLength(1))
                return new CellErrorResult { Error = CellError.Ref };

            return array[row - 1, col - 1];
        }

        private int Match(List<object> args, WorksheetContext context)
        {
            if (args.Count < 2)
                return 0;

            var lookupValue = args[0];
            var lookupArray = GetArrayValues(args[1]);
            string matchType = args.Count > 2 ? args[2]?.ToString() : "1";

            for (int i = 0; i < lookupArray.GetLength(0); i++)
            {
                int comparison = CompareValues(lookupArray[i, 0], lookupValue);
                if ((matchType == "1" && comparison >= 0) ||
                    (matchType == "-1" && comparison <= 0) ||
                    (matchType == "0" && comparison == 0))
                {
                    return i + 1;
                }
            }

            return new CellErrorResult { Error = CellError.NA };
        }

        private object Offset(List<object> args, WorksheetContext context)
        {
            if (args.Count < 3)
                return null;

            var reference = args[0] as ParsedFormula;
            int rows = (int)ToDouble(args[1]);
            int cols = (int)ToDouble(args[2]);
            int height = args.Count > 3 ? (int)ToDouble(args[3]) : 1;
            int width = args.Count > 4 ? (int)ToDouble(args[4]) : 1;

            if (reference != null)
            {
                var cellRef = reference.CellReference;
                int newRow = ParseRow(cellRef) + rows;
                string newCol = ParseColumn(cellRef) + cols;
                return context.GetRangeValues($"{newCol}{newRow}");
            }

            return null;
        }

        private object Indirect(List<object> args, WorksheetContext context)
        {
            if (args.Count == 0)
                return null;

            var refText = args[0]?.ToString();
            if (string.IsNullOrEmpty(refText))
                return null;

            return context.GetCellValue(refText);
        }

        private int Row(List<object> args, WorksheetContext context)
        {
            if (args.Count == 0)
                return 1;

            if (args[0] is ParsedFormula pf && pf.CellReference != null)
                return ParseRow(pf.CellReference);

            return 1;
        }

        private int Column(List<object> args, WorksheetContext context)
        {
            if (args.Count == 0)
                return 1;

            if (args[0] is ParsedFormula pf && pf.CellReference != null)
                return ParseColumnIndex(pf.CellReference);

            return 1;
        }

        private int Rows(List<object> args, WorksheetContext context)
        {
            if (args.Count == 0)
                return 0;

            var array = GetArrayValues(args[0]);
            return array.GetLength(0);
        }

        private int Columns(List<object> args, WorksheetContext context)
        {
            if (args.Count == 0)
                return 0;

            var array = GetArrayValues(args[0]);
            return array.GetLength(1);
        }

        private object ArrayFormula(List<object> args, WorksheetContext context)
        {
            if (args.Count < 2)
                return null;

            var formula = args[0]?.ToString();
            var dimensions = GetArrayValues(args[1]);

            int rows = (int)ToDouble(dimensions[0, 0]);
            int cols = (int)ToDouble(dimensions[0, 1]);

            var result = new object[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    result[i, j] = Evaluate(formula, context);
                }
            }

            return result;
        }

        private object EvaluateOperator(string op, ParsedFormula left, ParsedFormula right, WorksheetContext context)
        {
            var leftVal = EvaluateExpression(left, context);
            var rightVal = EvaluateExpression(right, context);

            switch (op)
            {
                case "+":
                    return ToDouble(leftVal) + ToDouble(rightVal);
                case "-":
                    return ToDouble(leftVal) - ToDouble(rightVal);
                case "*":
                    return ToDouble(leftVal) * ToDouble(rightVal);
                case "/":
                    if (Math.Abs(ToDouble(rightVal)) < 1e-10)
                        return new CellErrorResult { Error = CellError.Div0 };
                    return ToDouble(leftVal) / ToDouble(rightVal);
                case "^":
                    return Math.Pow(ToDouble(leftVal), ToDouble(rightVal));
                case "&":
                    return leftVal?.ToString() + rightVal?.ToString();
                case "=":
                case "==":
                    return CompareValues(leftVal, rightVal) == 0;
                case "<>":
                case "!=":
                    return CompareValues(leftVal, rightVal) != 0;
                case "<":
                    return CompareValues(leftVal, rightVal) < 0;
                case ">":
                    return CompareValues(leftVal, rightVal) > 0;
                case "<=":
                    return CompareValues(leftVal, rightVal) <= 0;
                case ">=":
                    return CompareValues(leftVal, rightVal) >= 0;
                default:
                    return new CellErrorResult { Error = CellError.Value };
            }
        }

        private object EvaluateArrayFormula(List<ParsedFormula> elements, WorksheetContext context)
        {
            if (elements.Count == 0)
                return new object[0, 0];

            int rows = (int)Math.Sqrt(elements.Count);
            int cols = elements.Count / rows;

            var result = new object[rows, cols];
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    int index = i * cols + j;
                    result[i, j] = index < elements.Count
                        ? EvaluateExpression(elements[index], context)
                        : null;
                }
            }

            return result;
        }

        private object[,] GetArrayValues(object value)
        {
            if (value is object[,] array)
                return array;

            if (value is double d)
                return new object[,] { { d } };

            if (value is int i)
                return new object[,] { { (double)i } };

            if (value is string s)
                return new object[,] { { s } };

            if (value is bool b)
                return new object[,] { { b } };

            return new object[,] { { value } };
        }

        private double ToDouble(object value)
        {
            if (value is double d)
                return d;
            if (value is int i)
                return i;
            if (value is long l)
                return l;
            if (value is decimal dec)
                return (double)dec;
            if (value is float f)
                return f;
            if (value is string s && double.TryParse(s, out double result))
                return result;
            if (value is bool b)
                return b ? 1 : 0;
            return 0;
        }

        private int CompareValues(object left, object right)
        {
            double leftNum = ToDouble(left);
            double rightNum = ToDouble(right);

            if (Math.Abs(leftNum - rightNum) < 1e-10)
                return 0;

            return leftNum < rightNum ? -1 : 1;
        }

        private bool MatchesCriteria(object value, object criteria)
        {
            if (criteria == null)
                return true;

            var criteriaStr = criteria.ToString();

            if (criteriaStr.StartsWith("<>"))
            {
                var crit = criteriaStr.Substring(2);
                return value?.ToString() != crit;
            }

            if (criteriaStr.StartsWith(">="))
            {
                var crit = double.Parse(criteriaStr.Substring(2));
                return ToDouble(value) >= crit;
            }

            if (criteriaStr.StartsWith("<="))
            {
                var crit = double.Parse(criteriaStr.Substring(2));
                return ToDouble(value) <= crit;
            }

            if (criteriaStr.StartsWith(">"))
            {
                var crit = double.Parse(criteriaStr.Substring(1));
                return ToDouble(value) > crit;
            }

            if (criteriaStr.StartsWith("<"))
            {
                var crit = double.Parse(criteriaStr.Substring(1));
                return ToDouble(value) < crit;
            }

            if (criteriaStr.StartsWith("="))
            {
                var crit = criteriaStr.Substring(1);
                return value?.ToString() == crit;
            }

            return value?.ToString() == criteriaStr;
        }

        private int ParseRow(string cellRef)
        {
            var match = Regex.Match(cellRef, @"(\d+)$");
            return match.Success ? int.Parse(match.Groups[1].Value) : 1;
        }

        private string ParseColumn(string cellRef)
        {
            return Regex.Match(cellRef, @"^[A-Za-z]+").Value;
        }

        private int ParseColumnIndex(string cellRef)
        {
            var col = ParseColumn(cellRef);
            int result = 0;
            foreach (char c in col)
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result;
        }
    }

    public class FormulaParser
    {
        private int _position;
        private string _formula;

        public ParsedFormula Parse(string formula)
        {
            _position = 0;
            _formula = formula;
            return ParseExpression();
        }

        private ParsedFormula ParseExpression()
        {
            return ParseComparison();
        }

        private ParsedFormula ParseComparison()
        {
            var left = ParseAddition();

            while (_position < _formula.Length)
            {
                var op = "";
                if (Match(">="))
                    op = ">=";
                else if (Match("<="))
                    op = "<=";
                else if (Match("<>"))
                    op = "<>";
                else if (Match("=="))
                    op = "==";
                else if (Match('>'))
                    op = ">";
                else if (Match('<'))
                    op = "<";
                else if (Match('='))
                    op = "=";
                else
                    break;

                var right = ParseAddition();
                left = new ParsedFormula
                {
                    Type = FormulaType.Operator,
                    Operator = op,
                    Left = left,
                    Right = right
                };
            }

            return left;
        }

        private ParsedFormula ParseAddition()
        {
            var left = ParseMultiplication();

            while (_position < _formula.Length)
            {
                if (Peek() == '+' || Peek() == '-' || Match("&"))
                {
                    var op = (Peek(-1) == '&') ? "&" : Peek(-1).ToString();
                    var right = ParseMultiplication();
                    left = new ParsedFormula
                    {
                        Type = FormulaType.Operator,
                        Operator = op,
                        Left = left,
                        Right = right
                    };
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private ParsedFormula ParseMultiplication()
        {
            var left = ParsePower();

            while (_position < _formula.Length)
            {
                if (Peek() == '*' || Peek() == '/')
                {
                    var op = Peek().ToString();
                    Advance();
                    var right = ParsePower();
                    left = new ParsedFormula
                    {
                        Type = FormulaType.Operator,
                        Operator = op,
                        Left = left,
                        Right = right
                    };
                }
                else
                {
                    break;
                }
            }

            return left;
        }

        private ParsedFormula ParsePower()
        {
            var left = ParseUnary();

            while (Peek() == '^')
            {
                Advance();
                var right = ParseUnary();
                left = new ParsedFormula
                {
                    Type = FormulaType.Operator,
                    Operator = "^",
                    Left = left,
                    Right = right
                };
            }

            return left;
        }

        private ParsedFormula ParseUnary()
        {
            if (Peek() == '-')
            {
                Advance();
                var operand = ParseUnary();
                return new ParsedFormula
                {
                    Type = FormulaType.Operator,
                    Operator = "-",
                    Left = new ParsedFormula { Type = FormulaType.Number, Value = 0 },
                    Right = operand
                };
            }

            if (Peek() == '+')
            {
                Advance();
                return ParseUnary();
            }

            return ParsePrimary();
        }

        private ParsedFormula ParsePrimary()
        {
            if (Peek() == '"')
                return ParseString();

            if (Peek() == '(')
                return ParseParenthesis();

            if (char.IsLetter(Peek()) || Peek() == '_')
                return ParseIdentifier();

            return ParseNumber();
        }

        private ParsedFormula ParseNumber()
        {
            var start = _position;
            while (char.IsDigit(Peek()) || Peek() == '.')
                Advance();

            var numStr = _formula.Substring(start, _position - start);
            double.TryParse(numStr, out double value);

            return new ParsedFormula
            {
                Type = FormulaType.Number,
                Value = value
            };
        }

        private ParsedFormula ParseString()
        {
            Expect('"');
            var start = _position;
            while (Peek() != '"' && _position < _formula.Length)
                Advance();
            var text = _formula.Substring(start, _position - start);
            Expect('"');

            return new ParsedFormula
            {
                Type = FormulaType.Text,
                TextValue = text
            };
        }

        private ParsedFormula ParseParenthesis()
        {
            Expect('(');
            var expr = ParseExpression();
            Expect(')');
            return expr;
        }

        private ParsedFormula ParseIdentifier()
        {
            var start = _position;
            while (char.IsLetterOrDigit(Peek()) || Peek() == '_' || Peek() == '.')
                Advance();

            var identifier = _formula.Substring(start, _position - start);

            if (Peek() == '(')
            {
                return ParseFunction(identifier);
            }

            if (Regex.IsMatch(identifier, @"^[A-Za-z]+\d+$") || Regex.IsMatch(identifier, @"^\$[A-Za-z]+\$?\d+$"))
            {
                return new ParsedFormula
                {
                    Type = FormulaType.CellReference,
                    CellReference = identifier.Replace("$", "")
                };
            }

            if (identifier.Contains(':'))
            {
                return new ParsedFormula
                {
                    Type = FormulaType.RangeReference,
                    RangeReference = identifier
                };
            }

            return new ParsedFormula
            {
                Type = FormulaType.Name,
                Name = identifier
            };
        }

        private ParsedFormula ParseFunction(string name)
        {
            Expect('(');
            var arguments = new List<ParsedFormula>();

            if (Peek() != ')')
            {
                arguments.Add(ParseExpression());

                while (Peek() == ',')
                {
                    Advance();
                    arguments.Add(ParseExpression());
                }
            }

            Expect(')');

            return new ParsedFormula
            {
                Type = FormulaType.Function,
                FunctionName = name,
                Arguments = arguments
            };
        }

        private char Peek(int offset = 0)
        {
            int pos = _position + offset;
            return pos < _formula.Length ? _formula[pos] : '\0';
        }

        private void Advance()
        {
            if (_position < _formula.Length)
                _position++;
        }

        private bool Peek(char c)
        {
            return Peek() == c;
        }

        private bool Match(char c)
        {
            if (Peek() == c)
            {
                Advance();
                return true;
            }
            return false;
        }

        private bool Match(string str)
        {
            if (_formula.Substring(_position).StartsWith(str))
            {
                _position += str.Length;
                return true;
            }
            return false;
        }

        private void Expect(char c)
        {
            if (Peek() != c)
                throw new FormatException($"Expected '{c}' at position {_position}");
            Advance();
        }
    }

    public enum FormulaType
    {
        Number,
        Text,
        Boolean,
        CellReference,
        RangeReference,
        Function,
        Operator,
        Array,
        Name
    }

    public class ParsedFormula
    {
        public FormulaType Type { get; set; }
        public double Value { get; set; }
        public string TextValue { get; set; }
        public bool BoolValue { get; set; }
        public string CellReference { get; set; }
        public string RangeReference { get; set; }
        public string FunctionName { get; set; }
        public List<ParsedFormula> Arguments { get; set; } = new List<ParsedFormula>();
        public string Operator { get; set; }
        public ParsedFormula Left { get; set; }
        public ParsedFormula Right { get; set; }
        public List<ParsedFormula> ArrayElements { get; set; } = new List<ParsedFormula>();
        public string Name { get; set; }
    }

    public class CellErrorResult
    {
        public CellError Error { get; set; }
        public string Message { get; set; }

        public override string ToString()
        {
            return Error switch
            {
                CellError.Div0 => "#DIV/0!",
                CellError.Value => "#VALUE!",
                CellError.Ref => "#REF!",
                CellError.Name => "#NAME?",
                CellError.Num => "#NUM!",
                CellError.NA => "#N/A",
                CellError.Null => "#NULL!",
                CellError.GettingData => "#GETTING_DATA",
                _ => "#ERROR!"
            };
        }
    }

    public class WorksheetContext
    {
        private readonly Dictionary<string, object> _cells = new Dictionary<string, object>();
        private readonly Dictionary<string, object[,]> _ranges = new Dictionary<string, object[,]>();

        public void SetCellValue(string cellRef, object value)
        {
            _cells[cellRef] = value;
        }

        public object GetCellValue(string cellRef)
        {
            if (_cells.TryGetValue(cellRef, out var value))
                return value;
            return null;
        }

        public void SetRangeValues(string rangeRef, object[,] values)
        {
            _ranges[rangeRef] = values;
        }

        public object[,] GetRangeValues(string rangeRef)
        {
            if (_ranges.TryGetValue(rangeRef, out var values))
                return values;

            if (rangeRef.Contains(':'))
            {
                var parts = rangeRef.Split(':');
                return new object[,] { { GetCellValue(rangeRef) } };
            }

            return new object[,] { { GetCellValue(rangeRef) } };
        }
    }

    public class ConditionalFormattingEngine
    {
        private readonly List<ConditionalRule> _rules = new List<ConditionalRule>();

        public void AddRule(ConditionalRule rule)
        {
            _rules.Add(rule);
        }

        public void ClearRules()
        {
            _rules.Clear();
        }

        public Dictionary<string, FormatStyle> EvaluateAll(WorksheetContext context)
        {
            var results = new Dictionary<string, FormatStyle>();

            foreach (var rule in _rules)
            {
                var matches = EvaluateRule(rule, context);
                foreach (var cellRef in matches)
                {
                    if (!results.ContainsKey(cellRef))
                        results[cellRef] = rule.Style;
                }
            }

            return results;
        }

        private List<string> EvaluateRule(ConditionalRule rule, WorksheetContext context)
        {
            var matches = new List<string>();
            var range = rule.Range.Split(':');

            if (range.Length < 2)
                return matches;

            int startRow = ParseRow(range[0]);
            int endRow = ParseRow(range[1]);
            int startCol = ParseColumnIndex(range[0]);
            int endCol = ParseColumnIndex(range[1]);

            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    var cellRef = GetCellRef(col, row);
                    var value = context.GetCellValue(cellRef);

                    if (EvaluateCondition(rule.Condition, value))
                    {
                        matches.Add(cellRef);
                    }
                }
            }

            return matches;
        }

        private bool EvaluateCondition(string condition, object value)
        {
            if (string.IsNullOrEmpty(condition))
                return true;

            if (condition.StartsWith(">"))
            {
                var num = double.Parse(condition.Substring(1));
                return ToDouble(value) > num;
            }

            if (condition.StartsWith("<"))
            {
                var num = double.Parse(condition.Substring(1));
                return ToDouble(value) < num;
            }

            if (condition.StartsWith("="))
            {
                return value?.ToString() == condition.Substring(1);
            }

            if (condition.Contains(":"))
            {
                return Regex.IsMatch(value?.ToString() ?? "", condition);
            }

            return value?.ToString() == condition;
        }

        private int ParseRow(string cellRef)
        {
            var match = Regex.Match(cellRef, @"(\d+)$");
            return match.Success ? int.Parse(match.Groups[1].Value) : 1;
        }

        private int ParseColumnIndex(string cellRef)
        {
            var col = Regex.Match(cellRef, @"^[A-Za-z]+").Value;
            int result = 0;
            foreach (char c in col)
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result;
        }

        private string GetCellRef(int col, int row)
        {
            string colStr = "";
            while (col > 0)
            {
                col--;
                colStr = (char)('A' + col % 26) + colStr;
                col /= 26;
            }
            return colStr + row;
        }

        private double ToDouble(object value)
        {
            if (value is double d)
                return d;
            if (value is int i)
                return i;
            if (double.TryParse(value?.ToString(), out double result))
                return result;
            return 0;
        }
    }

    public class ConditionalRule
    {
        public string Range { get; set; }
        public string Condition { get; set; }
        public FormatStyle Style { get; set; }
    }

    public class FormatStyle
    {
        public string FontColor { get; set; }
        public string BackgroundColor { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underline { get; set; }
        public string BorderStyle { get; set; }
    }

    public class DataValidationEngine
    {
        private readonly List<DataValidationRule> _validations = new List<DataValidationRule>();

        public void AddValidation(DataValidationRule rule)
        {
            _validations.Add(rule);
        }

        public List<string> GetInvalidCells(WorksheetContext context)
        {
            var invalidCells = new List<string>();

            foreach (var validation in _validations)
            {
                var cells = GetCellsInRange(validation.Range);
                foreach (var cell in cells)
                {
                    var value = context.GetCellValue(cell);
                    if (!IsValid(value, validation))
                    {
                        invalidCells.Add(cell);
                    }
                }
            }

            return invalidCells;
        }

        private List<string> GetCellsInRange(string range)
        {
            var cells = new List<string>();
            var parts = range.Split(':');

            if (parts.Length < 2)
            {
                cells.Add(range);
                return cells;
            }

            int startRow = ParseRow(parts[0]);
            int endRow = ParseRow(parts[1]);
            int startCol = ParseColumnIndex(parts[0]);
            int endCol = ParseColumnIndex(parts[1]);

            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = startCol; col <= endCol; col++)
                {
                    cells.Add(GetCellRef(col, row));
                }
            }

            return cells;
        }

        private bool IsValid(object value, DataValidationRule rule)
        {
            switch (rule.Type)
            {
                case ValidationType.WholeNumber:
                case ValidationType.Decimal:
                    return IsNumericValid(value, rule);
                case ValidationType.List:
                    return IsListValid(value, rule);
                case ValidationType.Date:
                case ValidationType.Time:
                    return IsDateValid(value, rule);
                case ValidationType.TextLength:
                    return IsTextLengthValid(value, rule);
                case ValidationType.Custom:
                    return IsCustomValid(value, rule);
                default:
                    return true;
            }
        }

        private bool IsNumericValid(object value, DataValidationRule rule)
        {
            if (!double.TryParse(value?.ToString(), out double num))
                return rule.AllowBlank;

            if (rule.Min.HasValue && num < rule.Min.Value)
                return false;

            if (rule.Max.HasValue && num > rule.Max.Value)
                return false;

            return true;
        }

        private bool IsListValid(object value, DataValidationRule rule)
        {
            if (rule.ListValues == null || rule.ListValues.Count == 0)
                return true;

            return rule.ListValues.Contains(value?.ToString());
        }

        private bool IsDateValid(object value, DataValidationRule rule)
        {
            if (!DateTime.TryParse(value?.ToString(), out DateTime date))
                return rule.AllowBlank;

            if (rule.MinDate.HasValue && date < rule.MinDate.Value)
                return false;

            if (rule.MaxDate.HasValue && date > rule.MaxDate.Value)
                return false;

            return true;
        }

        private bool IsTextLengthValid(object value, DataValidationRule rule)
        {
            int length = value?.ToString()?.Length ?? 0;

            if (rule.MinLength.HasValue && length < rule.MinLength.Value)
                return false;

            if (rule.MaxLength.HasValue && length > rule.MaxLength.Value)
                return false;

            return true;
        }

        private bool IsCustomValid(object value, DataValidationRule rule)
        {
            if (string.IsNullOrEmpty(rule.CustomFormula))
                return true;

            return true;
        }

        private int ParseRow(string cellRef)
        {
            var match = Regex.Match(cellRef, @"(\d+)$");
            return match.Success ? int.Parse(match.Groups[1].Value) : 1;
        }

        private int ParseColumnIndex(string cellRef)
        {
            var col = Regex.Match(cellRef, @"^[A-Za-z]+").Value;
            int result = 0;
            foreach (char c in col)
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result;
        }

        private string GetCellRef(int col, int row)
        {
            string colStr = "";
            while (col > 0)
            {
                col--;
                colStr = (char)('A' + col % 26) + colStr;
                col /= 26;
            }
            return colStr + row;
        }
    }

    public enum ValidationType
    {
        Any,
        WholeNumber,
        Decimal,
        List,
        Date,
        Time,
        TextLength,
        Custom
    }

    public enum ValidationOperator
    {
        Between,
        NotBetween,
        Equal,
        NotEqual,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual
    }

    public class DataValidationRule
    {
        public string Range { get; set; }
        public ValidationType Type { get; set; }
        public ValidationOperator Operator { get; set; }
        public double? Min { get; set; }
        public double? Max { get; set; }
        public DateTime? MinDate { get; set; }
        public DateTime? MaxDate { get; set; }
        public int? MinLength { get; set; }
        public int? MaxLength { get; set; }
        public List<string> ListValues { get; set; }
        public string CustomFormula { get; set; }
        public bool AllowBlank { get; set; }
        public bool ShowInputMessage { get; set; }
        public string InputTitle { get; set; }
        public string InputMessage { get; set; }
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
    }
}

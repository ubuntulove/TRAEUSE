using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeSuite.Spreadsheet.Data
{
    public class DataTable
    {
        public string Name { get; set; }
        public List<DataColumn> Columns { get; set; } = new List<DataColumn>();
        public List<object[]> Rows { get; set; } = new List<object[]>();
        public PrimaryKey PrimaryKey { get; set; }
        public List<Index> Indexes { get; set; } = new List<Index>();

        public int RowCount => Rows.Count;
        public int ColumnCount => Columns.Count;

        public object GetValue(int rowIndex, int columnIndex)
        {
            if (rowIndex < 0 || rowIndex >= Rows.Count)
                return null;
            if (columnIndex < 0 || columnIndex >= Columns.Count)
                return null;

            return Rows[rowIndex][columnIndex];
        }

        public void SetValue(int rowIndex, int columnIndex, object value)
        {
            if (rowIndex < 0 || rowIndex >= Rows.Count)
                return;
            if (columnIndex < 0 || columnIndex >= Columns.Count)
                return;

            Rows[rowIndex][columnIndex] = value;
        }

        public DataRow NewRow()
        {
            return new DataRow { Table = this };
        }

        public void AddRow(object[] values)
        {
            if (values.Length != Columns.Count)
                throw new ArgumentException("Row values count must match column count");

            Rows.Add(values);
        }

        public void InsertRow(int index, object[] values)
        {
            if (values.Length != Columns.Count)
                throw new ArgumentException("Row values count must match column count");

            Rows.Insert(index, values);
        }

        public void DeleteRow(int index)
        {
            if (index < 0 || index >= Rows.Count)
                return;

            Rows.RemoveAt(index);
        }

        public DataView Select(string filterExpression)
        {
            var view = new DataView { Table = this };

            if (string.IsNullOrEmpty(filterExpression))
                return view;

            var filteredRows = new List<int>();

            for (int i = 0; i < Rows.Count; i++)
            {
                if (EvaluateFilter(Rows[i], filterExpression))
                    filteredRows.Add(i);
            }

            view.RowIndices = filteredRows;
            return view;
        }

        public DataView Select(string filterExpression, string sortExpression)
        {
            var view = Select(filterExpression);
            view.Sort(sortExpression);
            return view;
        }

        private bool EvaluateFilter(object[] row, string expression)
        {
            try
            {
                return EvaluateExpression(row, expression);
            }
            catch
            {
                return false;
            }
        }

        private bool EvaluateExpression(object[] row, string expression)
        {
            expression = expression.Trim();

            if (expression.Contains("="))
            {
                var parts = expression.Split('=');
                if (parts.Length == 2)
                {
                    var colIndex = FindColumn(parts[0].Trim());
                    if (colIndex >= 0)
                    {
                        var value = parts[1].Trim().Trim('"', '\'');
                        return row[colIndex]?.ToString() == value;
                    }
                }
            }

            if (expression.Contains("<>"))
            {
                var parts = expression.Split(new[] { "<>" }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    var colIndex = FindColumn(parts[0].Trim());
                    if (colIndex >= 0)
                    {
                        var value = parts[1].Trim().Trim('"', '\'');
                        return row[colIndex]?.ToString() != value;
                    }
                }
            }

            if (expression.Contains(">"))
            {
                var parts = expression.Split('>');
                if (parts.Length == 2)
                {
                    var colIndex = FindColumn(parts[0].Trim());
                    if (colIndex >= 0)
                    {
                        var value = Convert.ToDouble(parts[1].Trim());
                        return Convert.ToDouble(row[colIndex]) > value;
                    }
                }
            }

            if (expression.Contains("<"))
            {
                var parts = expression.Split('<');
                if (parts.Length == 2)
                {
                    var colIndex = FindColumn(parts[0].Trim());
                    if (colIndex >= 0)
                    {
                        var value = Convert.ToDouble(parts[1].Trim());
                        return Convert.ToDouble(row[colIndex]) < value;
                    }
                }
            }

            return false;
        }

        public int FindColumn(string columnName)
        {
            for (int i = 0; i < Columns.Count; i++)
            {
                if (Columns[i].ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                    return i;
            }
            return -1;
        }

        public DataColumn AddColumn(string columnName, Type dataType)
        {
            var column = new DataColumn
            {
                ColumnName = columnName,
                DataType = dataType,
                Ordinal = Columns.Count
            };

            Columns.Add(column);

            foreach (var row in Rows)
            {
                var newRow = new object[Columns.Count];
                Array.Copy(row, newRow, row.Length);
                newRow[Columns.Count - 1] = dataType.IsValueType ? Activator.CreateInstance(dataType) : null;
            }

            return column;
        }

        public void DeleteColumn(int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= Columns.Count)
                return;

            Columns.RemoveAt(columnIndex);

            foreach (var row in Rows)
            {
                var newRow = new object[Columns.Count];
                int j = 0;
                for (int i = 0; i < row.Length; i++)
                {
                    if (i != columnIndex)
                        newRow[j++] = row[i];
                }
            }
        }

        public void CreateIndex(string indexName, int[] columnIndices)
        {
            var index = new Index
            {
                Name = indexName,
                Columns = columnIndices.ToList()
            };

            Indexes.Add(index);
        }
    }

    public class DataColumn
    {
        public string ColumnName { get; set; }
        public Type DataType { get; set; }
        public int Ordinal { get; set; }
        public bool AllowNull { get; set; } = true;
        public bool Unique { get; set; } = false;
        public object DefaultValue { get; set; }
        public string Caption { get; set; }
        public int MaxLength { get; set; } = -1;
        public DataColumnExpression Expression { get; set; }
    }

    public class DataColumnExpression
    {
        public string Expression { get; set; }
        public string Dependencies { get; set; }
    }

    public class DataRow
    {
        public DataTable Table { get; set; }
        public int RowIndex { get; set; }

        public object this[int columnIndex]
        {
            get => Table.GetValue(RowIndex, columnIndex);
            set => Table.SetValue(RowIndex, columnIndex, value);
        }

        public object this[string columnName]
        {
            get
            {
                var colIndex = Table.FindColumn(columnName);
                return colIndex >= 0 ? this[colIndex] : null;
            }
            set
            {
                var colIndex = Table.FindColumn(columnName);
                if (colIndex >= 0)
                    this[colIndex] = value;
            }
        }
    }

    public class PrimaryKey
    {
        public string Name { get; set; }
        public int[] Columns { get; set; }
    }

    public class Index
    {
        public string Name { get; set; }
        public List<int> Columns { get; set; }
        public bool IsUnique { get; set; } = false;
        public bool IsClustered { get; set; } = false;
    }

    public class DataView
    {
        public DataTable Table { get; set; }
        public List<int> RowIndices { get; set; } = new List<int>();
        public string SortExpression { get; set; }

        public int Count => RowIndices.Count;

        public object GetValue(int viewRowIndex, int columnIndex)
        {
            if (viewRowIndex < 0 || viewRowIndex >= RowIndices.Count)
                return null;

            return Table.GetValue(RowIndices[viewRowIndex], columnIndex);
        }

        public void Sort(string sortExpression)
        {
            SortExpression = sortExpression;

            if (string.IsNullOrEmpty(sortExpression))
                return;

            var sortColumns = ParseSortExpression(sortExpression);

            for (int i = 0; i < RowIndices.Count - 1; i++)
            {
                for (int j = i + 1; j < RowIndices.Count; j++)
                {
                    if (CompareRows(RowIndices[i], RowIndices[j], sortColumns) > 0)
                    {
                        var temp = RowIndices[i];
                        RowIndices[i] = RowIndices[j];
                        RowIndices[j] = temp;
                    }
                }
            }
        }

        private List<SortColumn> ParseSortExpression(string expression)
        {
            var columns = new List<SortColumn>();
            var parts = expression.Split(',');

            foreach (var part in parts)
            {
                var col = part.Trim().Split(' ');
                var sortCol = new SortColumn
                {
                    ColumnIndex = Table.FindColumn(col[0]),
                    Ascending = col.Length < 2 || col[1].ToUpper() == "ASC"
                };
                columns.Add(sortCol);
            }

            return columns;
        }

        private int CompareRows(int index1, int index2, List<SortColumn> sortColumns)
        {
            foreach (var col in sortColumns)
            {
                var val1 = Table.GetValue(index1, col.ColumnIndex);
                var val2 = Table.GetValue(index2, col.ColumnIndex);

                int comparison = CompareValues(val1, val2);
                if (comparison != 0)
                    return col.Ascending ? comparison : -comparison;
            }

            return 0;
        }

        private int CompareValues(object val1, object val2)
        {
            if (val1 == null && val2 == null)
                return 0;
            if (val1 == null)
                return -1;
            if (val2 == null)
                return 1;

            if (val1 is IComparable comparable)
                return comparable.CompareTo(val2);

            return string.Compare(val1.ToString(), val2.ToString(), StringComparison.Ordinal);
        }
    }

    public class SortColumn
    {
        public int ColumnIndex { get; set; }
        public bool Ascending { get; set; } = true;
    }

    public class PivotTable
    {
        public string Name { get; set; }
        public DataTable SourceData { get; set; }
        public List<PivotField> RowFields { get; set; } = new List<PivotField>();
        public List<PivotField> ColumnFields { get; set; } = new List<PivotField>();
        public List<PivotField> DataFields { get; set; } = new List<PivotField>();
        public List<PivotField> FilterFields { get; set; } = new List<PivotField>();
        public PivotTableStyle Style { get; set; }
        public bool ShowGrandTotals { get; set; } = true;
        public bool ShowSubtotals { get; set; } = true;
        public bool IncludeEmptyRows { get; set; } = false;

        public DataTable Generate()
        {
            var result = new DataTable { Name = Name };

            foreach (var field in ColumnFields)
            {
                result.AddColumn(field.Name, typeof(string));
            }

            foreach (var field in DataFields)
            {
                var columnName = $"{field.Name} ({GetAggregateFunctionName(field.AggregateFunction)})";
                result.AddColumn(columnName, typeof(double));
            }

            var rowGroups = GroupByRows();
            foreach (var group in rowGroups)
            {
                var row = new List<object>();

                foreach (var kvp in group.Key)
                {
                    row.Add(kvp.Value);
                }

                foreach (var dataField in DataFields)
                {
                    var values = group.Select(r => GetValue(r, dataField.SourceColumn)).Where(v => v != null).Cast<double>().ToList();
                    row.Add(AggregateValues(values, dataField.AggregateFunction));
                }

                result.AddRow(row.ToArray());
            }

            return result;
        }

        private IEnumerable<IGrouping<Dictionary<string, object>, object[]>> GroupByRows()
        {
            var query = SourceData.Rows.AsEnumerable();

            foreach (var filter in FilterFields)
            {
                if (filter.CurrentFilter != null)
                {
                    query = query.Where(r => FilterRow(r, filter));
                }
            }

            return query.GroupBy(r =>
            {
                var key = new Dictionary<string, object>();
                foreach (var field in RowFields)
                {
                    key[field.Name] = GetValue(r, field.SourceColumn);
                }
                return key;
            });
        }

        private bool FilterRow(object[] row, PivotField filter)
        {
            var value = GetValue(row, filter.SourceColumn);
            return filter.CurrentFilter.Contains(value);
        }

        private object GetValue(object[] row, int columnIndex)
        {
            if (columnIndex < 0 || columnIndex >= row.Length)
                return null;
            return row[columnIndex];
        }

        private object AggregateValues(List<double> values, AggregateFunction function)
        {
            return function switch
            {
                AggregateFunction.Sum => values.Sum(),
                AggregateFunction.Average => values.Count > 0 ? values.Average() : 0,
                AggregateFunction.Count => values.Count,
                AggregateFunction.Min => values.Count > 0 ? values.Min() : 0,
                AggregateFunction.Max => values.Count > 0 ? values.Max() : 0,
                AggregateFunction.Product => values.Aggregate(1.0, (a, b) => a * b),
                AggregateFunction.StandardDeviation => CalculateStdDev(values),
                AggregateFunction.Variance => CalculateVariance(values),
                AggregateFunction.CountNumbers => values.Count(v => v != 0),
                _ => values.Sum()
            };
        }

        private double CalculateStdDev(List<double> values)
        {
            if (values.Count < 2)
                return 0;

            var avg = values.Average();
            var sumSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumSquares / (values.Count - 1));
        }

        private double CalculateVariance(List<double> values)
        {
            if (values.Count < 2)
                return 0;

            var avg = values.Average();
            return values.Sum(v => Math.Pow(v - avg, 2)) / (values.Count - 1);
        }

        private string GetAggregateFunctionName(AggregateFunction function)
        {
            return function switch
            {
                AggregateFunction.Sum => "Sum",
                AggregateFunction.Count => "Count",
                AggregateFunction.Average => "Average",
                AggregateFunction.Max => "Max",
                AggregateFunction.Min => "Min",
                AggregateFunction.Product => "Product",
                AggregateFunction.StandardDeviation => "StdDev",
                AggregateFunction.Variance => "Variance",
                AggregateFunction.CountNumbers => "Count Numbers",
                _ => "Sum"
            };
        }
    }

    public class PivotField
    {
        public string Name { get; set; }
        public int SourceColumn { get; set; }
        public AggregateFunction AggregateFunction { get; set; } = AggregateFunction.Sum;
        public string NumberFormat { get; set; }
        public bool ShowAllItems { get; set; } = false;
        public object CurrentFilter { get; set; }
        public List<object> FilterValues { get; set; } = new List<object>();
        public bool Subtotals { get; set; } = true;
        public bool ShowDetail { get; set; } = true;
    }

    public enum AggregateFunction
    {
        Sum,
        Count,
        Average,
        Max,
        Min,
        Product,
        StandardDeviation,
        Variance,
        CountNumbers,
        None
    }

    public enum PivotTableStyle
    {
        None,
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
        PivotStyleLight19,
        PivotStyleLight20,
        PivotStyleLight21,
        PivotStyleLight22,
        PivotStyleLight23,
        PivotStyleLight24,
        PivotStyleLight25,
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
        PivotStyleDark11,
        PivotStyleDark12,
        PivotStyleDark13,
        PivotStyleDark14,
        PivotStyleDark15,
        PivotStyleDark16,
        PivotStyleDark17,
        PivotStyleDark18,
        PivotStyleDark19,
        PivotStyleDark20,
        PivotStyleDark21,
        PivotStyleDark22,
        PivotStyleDark23,
        PivotStyleDark24,
        PivotStyleDark25,
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
        PivotStyleMedium28
    }

    public class DataValidation
    {
        public ValidationType Type { get; set; }
        public int FirstRow { get; set; }
        public int LastRow { get; set; }
        public int FirstColumn { get; set; }
        public int LastColumn { get; set; }
        public string Formula1 { get; set; }
        public string Formula2 { get; set; }
        public bool IgnoreBlank { get; set; } = true;
        public bool ShowInputMessage { get; set; } = true;
        public string InputTitle { get; set; }
        public string InputMessage { get; set; }
        public bool ShowErrorMessage { get; set; } = true;
        public string ErrorTitle { get; set; }
        public string ErrorMessage { get; set; }
        public ValidationErrorStyle ErrorStyle { get; set; } = ValidationErrorStyle.Stop;
    }

    public enum ValidationType
    {
        None,
        WholeNumber,
        Decimal,
        List,
        Date,
        Time,
        TextLength,
        Custom,
        DataBar,
        ColorScale,
        IconSet
    }

    public enum ValidationErrorStyle
    {
        Stop,
        Warning,
        Information
    }

    public class Subtotal
    {
        public int GroupByColumn { get; set; }
        public List<SubtotalFunction> Functions { get; set; } = new List<SubtotalFunction>();
        public bool ReplaceExisting { get; set; } = true;
        public bool SummaryBelowData { get; set; } = true;
    }

    public class SubtotalFunction
    {
        public AggregateFunction Function { get; set; }
        public int ColumnIndex { get; set; }
        public int? PageBreakAfter { get; set; }
    }

    public class OutlineGroup
    {
        public int StartRow { get; set; }
        public int EndRow { get; set; }
        public int Level { get; set; }
        public bool IsCollapsed { get; set; } = false;
        public OutlineGroup Parent { get; set; }
        public List<OutlineGroup> Children { get; set; } = new List<OutlineGroup>();
    }

    public class DataTableOperations
    {
        public static DataTable Transpose(DataTable table)
        {
            var result = new DataTable { Name = $"Transpose_{table.Name}" };

            result.AddColumn("Row", typeof(string));
            for (int i = 0; i < table.Rows.Count; i++)
            {
                result.AddColumn($"Row{i + 1}", typeof(object));
            }

            for (int j = 0; j < table.Columns.Count; j++)
            {
                var row = new object[table.Rows.Count + 1];
                row[0] = table.Columns[j].ColumnName;
                for (int i = 0; i < table.Rows.Count; i++)
                {
                    row[i + 1] = table.GetValue(i, j);
                }
                result.AddRow(row);
            }

            return result;
        }

        public static DataTable RemoveDuplicates(DataTable table, int[] keyColumns)
        {
            var result = table.Clone();
            var seenKeys = new HashSet<string>();

            foreach (var row in table.Rows)
            {
                var key = string.Join("|", keyColumns.Select(c => table.GetValue(Array.IndexOf(table.Rows.ToArray(), row), c)?.ToString() ?? ""));

                if (!seenKeys.Contains(key))
                {
                    seenKeys.Add(key);
                    result.AddRow(row);
                }
            }

            return result;
        }

        public static DataTable Merge(DataTable table1, DataTable table2, string keyColumn, JoinType joinType)
        {
            var result = new DataTable { Name = $"Merge_{table1.Name}_{table2.Name}" };

            foreach (var col in table1.Columns)
            {
                result.AddColumn(col.ColumnName, col.DataType);
            }

            foreach (var col in table2.Columns)
            {
                if (!result.Columns.Any(c => c.ColumnName == col.ColumnName))
                {
                    result.AddColumn(col.ColumnName, col.DataType);
                }
            }

            var keyIndex1 = table1.FindColumn(keyColumn);
            var keyIndex2 = table2.FindColumn(keyColumn);

            foreach (var row1 in table1.Rows)
            {
                var key1 = table1.GetValue(Array.IndexOf(table1.Rows.ToArray(), row1), keyIndex1);

                var matchingRows = table2.Rows.Where((row2, idx) =>
                    table2.GetValue(idx, keyIndex2)?.ToString() == key1?.ToString()).ToList();

                if (matchingRows.Count > 0)
                {
                    foreach (var row2 in matchingRows)
                    {
                        var newRow = new List<object>();
                        for (int i = 0; i < table1.Columns.Count; i++)
                        {
                            newRow.Add(table1.GetValue(Array.IndexOf(table1.Rows.ToArray(), row1), i));
                        }
                        for (int i = 0; i < table2.Columns.Count; i++)
                        {
                            if (!result.Columns.Any(c => c.ColumnName == table2.Columns[i].ColumnName))
                            {
                                newRow.Add(table2.GetValue(table2.Rows.IndexOf(matchingRows[0]), i));
                            }
                        }
                        result.AddRow(newRow.ToArray());
                    }
                }
                else if (joinType == JoinType.LeftOuter || joinType == JoinType.FullOuter)
                {
                    var newRow = new List<object>();
                    for (int i = 0; i < table1.Columns.Count; i++)
                    {
                        newRow.Add(table1.GetValue(Array.IndexOf(table1.Rows.ToArray(), row1), i));
                    }
                    for (int i = 0; i < table2.Columns.Count; i++)
                    {
                        if (!result.Columns.Any(c => c.ColumnName == table2.Columns[i].ColumnName))
                        {
                            newRow.Add(DBNull.Value);
                        }
                    }
                    result.AddRow(newRow.ToArray());
                }
            }

            if (joinType == JoinType.FullOuter)
            {
                var keyIndex1Set = table1.Rows.Select((row, idx) => table1.GetValue(idx, keyIndex1)?.ToString()).ToHashSet();

                foreach (var row2 in table2.Rows)
                {
                    var key2 = table2.GetValue(table2.Rows.IndexOf(row2), keyIndex2);
                    if (!keyIndex1Set.Contains(key2?.ToString()))
                    {
                        var newRow = new List<object>();
                        for (int i = 0; i < table1.Columns.Count; i++)
                        {
                            newRow.Add(DBNull.Value);
                        }
                        for (int i = 0; i < table2.Columns.Count; i++)
                        {
                            newRow.Add(table2.GetValue(table2.Rows.IndexOf(row2), i));
                        }
                        result.AddRow(newRow.ToArray());
                    }
                }
            }

            return result;
        }

        public enum JoinType
        {
            Inner,
            LeftOuter,
            RightOuter,
            FullOuter
        }
    }

    public class DataTableStatistics
    {
        public static double Mean(DataTable table, int columnIndex)
        {
            var values = GetNumericValues(table, columnIndex);
            return values.Count > 0 ? values.Average() : 0;
        }

        public static double Median(DataTable table, int columnIndex)
        {
            var values = GetNumericValues(table, columnIndex).OrderBy(v => v).ToList();
            if (values.Count == 0)
                return 0;

            int mid = values.Count / 2;
            return values.Count % 2 != 0 ? values[mid] : (values[mid - 1] + values[mid]) / 2;
        }

        public static double Mode(DataTable table, int columnIndex)
        {
            var groups = table.Rows.Cast<object[]>()
                .Select(r => r[columnIndex])
                .GroupBy(v => v)
                .OrderByDescending(g => g.Count());

            return groups.FirstOrDefault()?.Key as double? ?? 0;
        }

        public static double StandardDeviation(DataTable table, int columnIndex)
        {
            var values = GetNumericValues(table, columnIndex);
            if (values.Count < 2)
                return 0;

            var avg = values.Average();
            var sumSquares = values.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumSquares / (values.Count - 1));
        }

        public static double Variance(DataTable table, int columnIndex)
        {
            var stdDev = StandardDeviation(table, columnIndex);
            return stdDev * stdDev;
        }

        public static double Skewness(DataTable table, int columnIndex)
        {
            var values = GetNumericValues(table, columnIndex);
            if (values.Count < 3)
                return 0;

            var avg = values.Average();
            var stdDev = StandardDeviation(table, columnIndex);

            if (stdDev == 0)
                return 0;

            var sumCubes = values.Sum(v => Math.Pow((v - avg) / stdDev, 3));
            return sumCubes * values.Count / ((values.Count - 1) * (values.Count - 2));
        }

        public static double Kurtosis(DataTable table, int columnIndex)
        {
            var values = GetNumericValues(table, columnIndex);
            if (values.Count < 4)
                return 0;

            var avg = values.Average();
            var stdDev = StandardDeviation(table, columnIndex);

            if (stdDev == 0)
                return 0;

            var sumQuads = values.Sum(v => Math.Pow((v - avg) / stdDev, 4));
            var n = values.Count;
            return (n * (n + 1) * sumQuads) / ((n - 1) * (n - 2) * (n - 3)) -
                   (3 * Math.Pow(n - 1, 2)) / ((n - 2) * (n - 3));
        }

        public static double Percentile(DataTable table, int columnIndex, double percentile)
        {
            var values = GetNumericValues(table, columnIndex).OrderBy(v => v).ToList();
            if (values.Count == 0)
                return 0;

            double index = percentile * (values.Count - 1);
            int lower = (int)Math.Floor(index);
            int upper = (int)Math.Ceiling(index);

            if (lower == upper)
                return values[lower];

            return values[lower] + (values[upper] - values[lower]) * (index - lower);
        }

        public static double Quartile(DataTable table, int columnIndex, int quartileNumber)
        {
            return quartileNumber switch
            {
                1 => Percentile(table, columnIndex, 0.25),
                2 => Percentile(table, columnIndex, 0.5),
                3 => Percentile(table, columnIndex, 0.75),
                _ => 0
            };
        }

        public static double InterquartileRange(DataTable table, int columnIndex)
        {
            return Quartile(table, columnIndex, 3) - Quartile(table, columnIndex, 1);
        }

        public static double Sum(DataTable table, int columnIndex)
        {
            return GetNumericValues(table, columnIndex).Sum();
        }

        public static double Max(DataTable table, int columnIndex)
        {
            var values = GetNumericValues(table, columnIndex);
            return values.Count > 0 ? values.Max() : 0;
        }

        public static double Min(DataTable table, int columnIndex)
        {
            var values = GetNumericValues(table, columnIndex);
            return values.Count > 0 ? values.Min() : 0;
        }

        public static int Count(DataTable table, int columnIndex)
        {
            return GetNumericValues(table, columnIndex).Count;
        }

        public static int CountNonEmpty(DataTable table, int columnIndex)
        {
            return table.Rows.Count(r => r[columnIndex] != null && r[columnIndex] != DBNull.Value);
        }

        private static List<double> GetNumericValues(DataTable table, int columnIndex)
        {
            var values = new List<double>();
            foreach (var row in table.Rows)
            {
                var value = table.GetValue(Array.IndexOf(table.Rows.ToArray(), row), columnIndex);
                if (value is double d)
                    values.Add(d);
                else if (value is int i)
                    values.Add(i);
                else if (value is long l)
                    values.Add(l);
                else if (value is decimal dec)
                    values.Add((double)dec);
                else if (value is float f)
                    values.Add(f);
            }
            return values;
        }
    }

    public class DataTableFilter
    {
        public static DataTable FilterByCondition(DataTable table, Func<object[], bool> predicate)
        {
            var result = table.Clone();
            foreach (var row in table.Rows)
            {
                if (predicate(row))
                {
                    result.AddRow(row);
                }
            }
            return result;
        }

        public static DataTable FilterByValue(DataTable table, int columnIndex, object value)
        {
            return FilterByCondition(table, row =>
                row[columnIndex]?.ToString() == value?.ToString());
        }

        public static DataTable FilterByRange(DataTable table, int columnIndex, double minValue, double maxValue)
        {
            return FilterByCondition(table, row =>
            {
                var value = table.GetValue(Array.IndexOf(table.Rows.ToArray(), row), columnIndex);
                if (value is double d)
                    return d >= minValue && d <= maxValue;
                return false;
            });
        }

        public static DataTable FilterByPattern(DataTable table, int columnIndex, string pattern)
        {
            return FilterByCondition(table, row =>
            {
                var value = table.GetValue(Array.IndexOf(table.Rows.ToArray(), row), columnIndex)?.ToString() ?? "";
                return MatchPattern(value, pattern);
            });
        }

        private static bool MatchPattern(string value, string pattern)
        {
            if (pattern == "*")
                return true;

            if (pattern.Contains("*") || pattern.Contains("?"))
            {
                pattern = "^" + System.Text.RegularExpressions.Regex.Escape(pattern)
                    .Replace("\\*", ".*")
                    .Replace("\\?", ".") + "$";
            }

            return System.Text.RegularExpressions.Regex.IsMatch(value, pattern,
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        }

        public static DataTable FilterTopN(DataTable table, int columnIndex, int n, bool descending = true)
        {
            var result = table.Clone();
            var indexedRows = table.Rows.Select((row, idx) => new { Row = row, Index = idx, Value = table.GetValue(idx, columnIndex) as double? ?? 0 })
                .OrderByDescending(x => x.Value)
                .Take(n)
                .ToList();

            foreach (var item in descending ? item : item)
            {
                result.AddRow(item.Row);
            }

            return result;
        }
    }

    public class DataTableSort
    {
        public static DataTable Sort(DataTable table, int columnIndex, bool ascending = true)
        {
            var result = table.Clone();
            var sortedRows = table.Rows.OrderBy(row => table.GetValue(Array.IndexOf(table.Rows.ToArray(), row), columnIndex)).ToList();

            if (!ascending)
                sortedRows.Reverse();

            foreach (var row in sortedRows)
            {
                result.AddRow(row);
            }

            return result;
        }

        public static DataTable SortMultipleColumns(DataTable table, params (int ColumnIndex, bool Ascending)[] sortSpecs)
        {
            var result = table.Clone();
            var sortedRows = table.Rows.ToList();

            sortedRows = sortedRows.OrderBy(row =>
            {
                var values = sortSpecs.Select(s => table.GetValue(Array.IndexOf(table.Rows.ToArray(), row), s.ColumnIndex)).ToArray();
                return string.Join("|", values);
            }).ToList();

            foreach (var row in sortedRows)
            {
                result.AddRow(row);
            }

            return result;
        }

        public static DataTable Shuffle(DataTable table)
        {
            var result = table.Clone();
            var random = new Random();
            var shuffled = table.Rows.OrderBy(_ => random.Next()).ToList();

            foreach (var row in shuffled)
            {
                result.AddRow(row);
            }

            return result;
        }
    }
}

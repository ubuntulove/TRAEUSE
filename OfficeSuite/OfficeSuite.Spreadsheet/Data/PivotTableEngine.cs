using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace OfficeSuite.Spreadsheet.Data
{
    public class PivotTableEngine
    {
        private PivotTable _pivotTable;
        private List<PivotField> _rowFields;
        private List<PivotField> _columnFields;
        private List<PivotField> _dataFields;
        private List<PivotField> _filterFields;
        private Dictionary<string, AggregateFunction> _customAggregates;

        public PivotTableEngine()
        {
            _rowFields = new List<PivotField>();
            _columnFields = new List<PivotField>();
            _dataFields = new List<PivotField>();
            _filterFields = new List<PivotField>();
            _customAggregates = new Dictionary<string, AggregateFunction>();
            InitializeDefaultAggregates();
        }

        private void InitializeDefaultAggregates()
        {
            _customAggregates["Sum"] = AggregateFunctions.Sum;
            _customAggregates["Count"] = AggregateFunctions.Count;
            _customAggregates["Average"] = AggregateFunctions.Average;
            _customAggregates["Max"] = AggregateFunctions.Max;
            _customAggregates["Min"] = AggregateFunctions.Min;
            _customAggregates["Product"] = AggregateFunctions.Product;
            _customAggregates["CountNumbers"] = AggregateFunctions.CountNumbers;
            _customAggregates["StdDev"] = AggregateFunctions.StandardDeviation;
            _customAggregates["StdDevP"] = AggregateFunctions.StandardDeviationP;
            _customAggregates["Var"] = AggregateFunctions.Variance;
            _customAggregates["VarP"] = AggregateFunctions.VarianceP;
        }

        public PivotTable CreatePivotTable(DataTable sourceData, PivotTableConfiguration config)
        {
            _pivotTable = new PivotTable
            {
                Id = Guid.NewGuid().ToString(),
                SourceData = sourceData,
                Configuration = config,
                CreatedDate = DateTime.Now
            };

            ProcessSourceData();
            return _pivotTable;
        }

        private void ProcessSourceData()
        {
            if (_pivotTable?.SourceData == null) return;

            var groupedData = GroupData();
            _pivotTable.ResultData = CalculateAggregates(groupedData);
            _pivotTable.RowLabels = ExtractRowLabels(groupedData);
            _pivotTable.ColumnLabels = ExtractColumnLabels(groupedData);
        }

        private Dictionary<string, Dictionary<string, List<object>>> GroupData()
        {
            var result = new Dictionary<string, Dictionary<string, List<object>>>();
            var rowKeyGenerator = new Func<DataRow, string>(row => string.Join("|", 
                _rowFields.Select(f => row[f.SourceField]?.ToString() ?? "")));
            var colKeyGenerator = new Func<DataRow, string>(row => string.Join("|",
                _columnFields.Select(f => row[f.SourceField]?.ToString() ?? "")));

            foreach (DataRow row in _pivotTable.SourceData.Rows)
            {
                var rowKey = rowKeyGenerator(row);
                var colKey = colKeyGenerator(row);

                if (!result.ContainsKey(rowKey))
                    result[rowKey] = new Dictionary<string, List<object>>();

                if (!result[rowKey].ContainsKey(colKey))
                    result[rowKey][colKey] = new List<object>();

                foreach (var dataField in _dataFields)
                {
                    var value = row[dataField.SourceField];
                    result[rowKey][colKey].Add(value);
                }
            }

            return result;
        }

        private DataTable CalculateAggregates(Dictionary<string, Dictionary<string, List<object>>> groupedData)
        {
            var result = new DataTable();
            
            foreach (var rowField in _rowFields)
                result.Columns.Add(rowField.Name, typeof(string));

            foreach (var colField in _columnFields)
                result.Columns.Add(colField.Name, typeof(string));

            foreach (var dataField in _dataFields)
            {
                var colName = $"{dataField.Name}_{dataField.AggregateFunction}";
                result.Columns.Add(colName, typeof(object));
            }

            foreach (var rowGroup in groupedData)
            {
                var rowValues = rowGroup.Key.Split('|');
                foreach (var colGroup in rowGroup.Value)
                {
                    var dataRow = result.NewRow();
                    var colValues = colGroup.Key.Split('|');

                    for (int i = 0; i < _rowFields.Count; i++)
                        dataRow[_rowFields[i].Name] = rowValues[i];

                    for (int i = 0; i < _columnFields.Count; i++)
                        dataRow[_columnFields[i].Name] = colValues[i];

                    for (int i = 0; i < _dataFields.Count; i++)
                    {
                        var aggregateFunc = _customAggregates[dataFields[i].AggregateFunction];
                        dataRow[$"{_dataFields[i].Name}_{_dataFields[i].AggregateFunction}"] = 
                            aggregateFunc(colGroup.Value);
                    }

                    result.Rows.Add(dataRow);
                }
            }

            return result;
        }

        private List<string> ExtractRowLabels(Dictionary<string, Dictionary<string, List<object>>> groupedData)
        {
            return groupedData.Keys.Select(k => k.Split('|')[0]).Distinct().ToList();
        }

        private List<string> ExtractColumnLabels(Dictionary<string, Dictionary<string, List<object>>> groupedData)
        {
            return groupedData.Values.SelectMany(v => v.Keys).Distinct().ToList();
        }

        public void AddRowField(PivotField field)
        {
            field.FieldType = PivotFieldType.Row;
            _rowFields.Add(field);
            ProcessSourceData();
        }

        public void AddColumnField(PivotField field)
        {
            field.FieldType = PivotFieldType.Column;
            _columnFields.Add(field);
            ProcessSourceData();
        }

        public void AddDataField(PivotField field)
        {
            field.FieldType = PivotFieldType.Data;
            _dataFields.Add(field);
            ProcessSourceData();
        }

        public void AddFilterField(PivotField field)
        {
            field.FieldType = PivotFieldType.Filter;
            _filterFields.Add(field);
            ProcessSourceData();
        }

        public void RemoveField(string fieldName, PivotFieldType fieldType)
        {
            switch (fieldType)
            {
                case PivotFieldType.Row:
                    _rowFields.RemoveAll(f => f.Name == fieldName);
                    break;
                case PivotFieldType.Column:
                    _columnFields.RemoveAll(f => f.Name == fieldName);
                    break;
                case PivotFieldType.Data:
                    _dataFields.RemoveAll(f => f.Name == fieldName);
                    break;
                case PivotFieldType.Filter:
                    _filterFields.RemoveAll(f => f.Name == fieldName);
                    break;
            }
            ProcessSourceData();
        }

        public void UpdateFieldOrder(PivotFieldType fieldType, List<string> newOrder)
        {
            switch (fieldType)
            {
                case PivotFieldType.Row:
                    _rowFields = ReorderFields(_rowFields, newOrder);
                    break;
                case PivotFieldType.Column:
                    _columnFields = ReorderFields(_columnFields, newOrder);
                    break;
            }
            ProcessSourceData();
        }

        private List<PivotField> ReorderFields(List<PivotField> fields, List<string> newOrder)
        {
            var result = new List<PivotField>();
            foreach (var name in newOrder)
            {
                var field = fields.FirstOrDefault(f => f.Name == name);
                if (field != null)
                    result.Add(field);
            }
            return result;
        }

        public void ApplyFilter(string fieldName, List<object> selectedValues)
        {
            var field = FindField(fieldName);
            if (field != null)
            {
                field.FilterValues = selectedValues;
                ProcessSourceData();
            }
        }

        public void ClearFilter(string fieldName)
        {
            var field = FindField(fieldName);
            if (field != null)
            {
                field.FilterValues = null;
                ProcessSourceData();
            }
        }

        private PivotField FindField(string fieldName)
        {
            return _rowFields.FirstOrDefault(f => f.Name == fieldName) as PivotField ??
                   _columnFields.FirstOrDefault(f => f.Name == fieldName) as PivotField ??
                   _dataFields.FirstOrDefault(f => f.Name == fieldName) as PivotField ??
                   _filterFields.FirstOrDefault(f => f.Name == fieldName);
        }

        public void SetAggregateFunction(string fieldName, string functionName)
        {
            var field = _dataFields.FirstOrDefault(f => f.Name == fieldName);
            if (field != null && _customAggregates.ContainsKey(functionName))
            {
                field.AggregateFunction = functionName;
                ProcessSourceData();
            }
        }

        public void GroupRowItems(string fieldName, GroupingType groupingType, object interval = null)
        {
            var field = _rowFields.FirstOrDefault(f => f.Name == fieldName);
            if (field != null)
            {
                field.GroupingType = groupingType;
                field.Interval = interval;
                ProcessSourceData();
            }
        }

        public void GroupColumnItems(string fieldName, GroupingType groupingType, object interval = null)
        {
            var field = _columnFields.FirstOrDefault(f => f.Name == fieldName);
            if (field != null)
            {
                field.GroupingType = groupingType;
                field.Interval = interval;
                ProcessSourceData();
            }
        }

        public void UngroupItems(string fieldName)
        {
            var field = FindField(fieldName);
            if (field != null)
            {
                field.GroupingType = GroupingType.None;
                field.Interval = null;
                ProcessSourceData();
            }
        }

        public void SetSortOrder(string fieldName, SortOrder sortOrder)
        {
            var field = FindField(fieldName);
            if (field != null)
            {
                field.SortOrder = sortOrder;
                ProcessSourceData();
            }
        }

        public PivotTableStatistics GetStatistics()
        {
            return new PivotTableStatistics
            {
                TotalRows = _pivotTable?.ResultData?.Rows.Count ?? 0,
                TotalColumns = _pivotTable?.ResultData?.Columns.Count ?? 0,
                RowFieldCount = _rowFields.Count,
                ColumnFieldCount = _columnFields.Count,
                DataFieldCount = _dataFields.Count,
                FilterFieldCount = _filterFields.Count,
                CreatedDate = _pivotTable?.CreatedDate ?? DateTime.MinValue
            };
        }
    }

    public class PivotTable
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DataTable SourceData { get; set; }
        public DataTable ResultData { get; set; }
        public PivotTableConfiguration Configuration { get; set; }
        public List<string> RowLabels { get; set; }
        public List<string> ColumnLabels { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string CacheId { get; set; }

        public PivotTable()
        {
            Id = Guid.NewGuid().ToString();
            RowLabels = new List<string>();
            ColumnLabels = new List<string>();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public void Refresh()
        {
            ModifiedDate = DateTime.Now;
        }

        public PivotTable Clone()
        {
            return new PivotTable
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                SourceData = SourceData?.Copy(),
                ResultData = ResultData?.Copy(),
                Configuration = Configuration?.Clone(),
                RowLabels = new List<string>(RowLabels),
                ColumnLabels = new List<string>(ColumnLabels),
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }
    }

    public class PivotTableConfiguration
    {
        public bool ShowRowTotals { get; set; }
        public bool ShowColumnTotals { get; set; }
        public bool ShowGrandTotals { get; set; }
        public bool ShowSubtotals { get; set; }
        public bool CompactRows { get; set; }
        public bool PreserveFormatting { get; set; }
        public bool AutoFitColumns { get; set; }
        public bool MergeAndCenter { get; set; }
        public string NullValueString { get; set; }
        public string ErrorValueString { get; set; }
        public PivotTableStyle Style { get; set; }

        public PivotTableConfiguration()
        {
            ShowRowTotals = true;
            ShowColumnTotals = true;
            ShowGrandTotals = true;
            ShowSubtotals = true;
            CompactRows = false;
            PreserveFormatting = true;
            AutoFitColumns = true;
            MergeAndCenter = false;
            NullValueString = "";
            ErrorValueString = "N/A";
            Style = PivotTableStyle.Default;
        }

        public PivotTableConfiguration Clone()
        {
            return new PivotTableConfiguration
            {
                ShowRowTotals = ShowRowTotals,
                ShowColumnTotals = ShowColumnTotals,
                ShowGrandTotals = ShowGrandTotals,
                ShowSubtotals = ShowSubtotals,
                CompactRows = CompactRows,
                PreserveFormatting = PreserveFormatting,
                AutoFitColumns = AutoFitColumns,
                MergeAndCenter = MergeAndCenter,
                NullValueString = NullValueString,
                ErrorValueString = ErrorValueString,
                Style = Style
            };
        }
    }

    public class PivotField
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string SourceField { get; set; }
        public PivotFieldType FieldType { get; set; }
        public string AggregateFunction { get; set; }
        public string CustomName { get; set; }
        public bool IsHidden { get; set; }
        public bool IsFiltered { get; set; }
        public List<object> FilterValues { get; set; }
        public GroupingType GroupingType { get; set; }
        public object Interval { get; set; }
        public SortOrder SortOrder { get; set; }
        public List<PivotItem> Items { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public PivotField()
        {
            Id = Guid.NewGuid().ToString();
            FilterValues = new List<object>();
            Items = new List<PivotItem>();
            Properties = new Dictionary<string, object>();
            AggregateFunction = "Sum";
            GroupingType = GroupingType.None;
            SortOrder = SortOrder.Ascending;
            IsHidden = false;
            IsFiltered = false;
        }

        public PivotField Clone()
        {
            return new PivotField
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                SourceField = SourceField,
                FieldType = FieldType,
                AggregateFunction = AggregateFunction,
                CustomName = CustomName,
                IsHidden = IsHidden,
                IsFiltered = IsFiltered,
                FilterValues = new List<object>(FilterValues),
                GroupingType = GroupingType,
                Interval = Interval,
                SortOrder = SortOrder,
                Items = Items.Select(i => i.Clone()).ToList(),
                Properties = new Dictionary<string, object>(Properties)
            };
        }
    }

    public class PivotItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }
        public bool IsHidden { get; set; }
        public bool IsSelected { get; set; }
        public bool HasChildren { get; set; }
        public PivotItem ParentItem { get; set; }
        public List<PivotItem> ChildItems { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public PivotItem()
        {
            Id = Guid.NewGuid().ToString();
            ChildItems = new List<PivotItem>();
            Properties = new Dictionary<string, object>();
            IsHidden = false;
            IsSelected = true;
        }

        public PivotItem Clone()
        {
            return new PivotItem
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Value = Value,
                IsHidden = IsHidden,
                IsSelected = IsSelected,
                HasChildren = HasChildren,
                ParentItem = ParentItem,
                ChildItems = ChildItems.Select(i => i.Clone()).ToList(),
                Properties = new Dictionary<string, object>(Properties)
            };
        }
    }

    public class PivotTableStatistics
    {
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public int RowFieldCount { get; set; }
        public int ColumnFieldCount { get; set; }
        public int DataFieldCount { get; set; }
        public int FilterFieldCount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public enum PivotFieldType
    {
        Row,
        Column,
        Data,
        Filter
    }

    public enum GroupingType
    {
        None,
        ByRange,
        ByInterval,
        ByPosition,
        ByDateRange
    }

    public enum SortOrder
    {
        Ascending,
        Descending,
        Manual
    }

    public enum PivotTableStyle
    {
        Default,
        Light1,
        Light2,
        Light3,
        Light4,
        Light5,
        Light6,
        Light7,
        Light8,
        Light9,
        Light10,
        Medium1,
        Medium2,
        Medium3,
        Medium4,
        Medium5,
        Medium6,
        Medium7,
        Medium8,
        Dark1,
        Dark2,
        Dark3,
        Dark4,
        Dark5,
        Dark6,
        Dark7,
        Dark8,
        Dark9,
        Dark10
    }

    public delegate object AggregateFunction(List<object> values);

    public static class AggregateFunctions
    {
        public static object Sum(List<object> values)
        {
            double sum = 0;
            foreach (var value in values)
            {
                if (value != null && value != DBNull.Value)
                {
                    if (double.TryParse(value.ToString(), out double num))
                        sum += num;
                }
            }
            return sum;
        }

        public static object Count(List<object> values)
        {
            return values.Count(v => v != null && v != DBNull.Value);
        }

        public static object CountNumbers(List<object> values)
        {
            return values.Count(v => v != null && v != DBNull.Value && IsNumeric(v));
        }

        public static object Average(List<object> values)
        {
            var numericValues = values.Where(v => v != null && v != DBNull.Value && IsNumeric(v))
                .Select(v => Convert.ToDouble(v)).ToList();
            return numericValues.Count > 0 ? numericValues.Average() : 0;
        }

        public static object Max(List<object> values)
        {
            var numericValues = values.Where(v => v != null && v != DBNull.Value && IsNumeric(v))
                .Select(v => Convert.ToDouble(v)).ToList();
            return numericValues.Count > 0 ? numericValues.Max() : 0;
        }

        public static object Min(List<object> values)
        {
            var numericValues = values.Where(v => v != null && v != DBNull.Value && IsNumeric(v))
                .Select(v => Convert.ToDouble(v)).ToList();
            return numericValues.Count > 0 ? numericValues.Min() : 0;
        }

        public static object Product(List<object> values)
        {
            double product = 1;
            foreach (var value in values)
            {
                if (value != null && value != DBNull.Value && IsNumeric(value))
                {
                    product *= Convert.ToDouble(value);
                }
            }
            return product;
        }

        public static object StandardDeviation(List<object> values)
        {
            var numericValues = values.Where(v => v != null && v != DBNull.Value && IsNumeric(v))
                .Select(v => Convert.ToDouble(v)).ToList();
            if (numericValues.Count < 2) return 0;
            
            double avg = numericValues.Average();
            double sumOfSquares = numericValues.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / (numericValues.Count - 1));
        }

        public static object StandardDeviationP(List<object> values)
        {
            var numericValues = values.Where(v => v != null && v != DBNull.Value && IsNumeric(v))
                .Select(v => Convert.ToDouble(v)).ToList();
            if (numericValues.Count < 1) return 0;
            
            double avg = numericValues.Average();
            double sumOfSquares = numericValues.Sum(v => Math.Pow(v - avg, 2));
            return Math.Sqrt(sumOfSquares / numericValues.Count);
        }

        public static object Variance(List<object> values)
        {
            var numericValues = values.Where(v => v != null && v != DBNull.Value && IsNumeric(v))
                .Select(v => Convert.ToDouble(v)).ToList();
            if (numericValues.Count < 2) return 0;
            
            double avg = numericValues.Average();
            double sumOfSquares = numericValues.Sum(v => Math.Pow(v - avg, 2));
            return sumOfSquares / (numericValues.Count - 1);
        }

        public static object VarianceP(List<object> values)
        {
            var numericValues = values.Where(v => v != null && v != DBNull.Value && IsNumeric(v))
                .Select(v => Convert.ToDouble(v)).ToList();
            if (numericValues.Count < 1) return 0;
            
            double avg = numericValues.Average();
            double sumOfSquares = numericValues.Sum(v => Math.Pow(v - avg, 2));
            return sumOfSquares / numericValues.Count;
        }

        private static bool IsNumeric(object value)
        {
            return value is sbyte || value is byte || value is short || value is ushort ||
                   value is int || value is uint || value is long || value is ulong ||
                   value is float || value is double || value is decimal;
        }
    }

    public class DataConnectionManager
    {
        private Dictionary<string, DataConnection> _connections;
        private Dictionary<string, DataRefreshScheduler> _refreshSchedules;

        public DataConnectionManager()
        {
            _connections = new Dictionary<string, DataConnection>();
            _refreshSchedules = new Dictionary<string, DataRefreshScheduler>();
        }

        public string AddConnection(DataConnection connection)
        {
            connection.Id = Guid.NewGuid().ToString();
            _connections[connection.Id] = connection;
            return connection.Id;
        }

        public void RemoveConnection(string connectionId)
        {
            _connections.Remove(connectionId);
            _refreshSchedules.Remove(connectionId);
        }

        public DataConnection GetConnection(string connectionId)
        {
            return _connections.TryGetValue(connectionId, out var connection) ? connection : null;
        }

        public IEnumerable<DataConnection> GetAllConnections()
        {
            return _connections.Values;
        }

        public void RefreshConnection(string connectionId)
        {
            var connection = GetConnection(connectionId);
            if (connection != null)
            {
                connection.LastRefreshed = DateTime.Now;
                connection.IsRefreshing = true;
            }
        }

        public void RefreshAllConnections()
        {
            foreach (var connection in _connections.Values)
            {
                RefreshConnection(connection.Id);
            }
        }

        public void ScheduleRefresh(string connectionId, DataRefreshScheduler scheduler)
        {
            _refreshSchedules[connectionId] = scheduler;
        }

        public void CancelScheduledRefresh(string connectionId)
        {
            _refreshSchedules.Remove(connectionId);
        }

        public bool TestConnection(string connectionId)
        {
            var connection = GetConnection(connectionId);
            return connection != null && connection.IsActive;
        }
    }

    public class DataConnection
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DataConnectionType Type { get; set; }
        public string ConnectionString { get; set; }
        public string Query { get; set; }
        public bool IsActive { get; set; }
        public bool IsRefreshing { get; set; }
        public DateTime LastRefreshed { get; set; }
        public DateTime CreatedDate { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public DataConnection()
        {
            Id = Guid.NewGuid().ToString();
            Properties = new Dictionary<string, string>();
            IsActive = true;
            CreatedDate = DateTime.Now;
        }
    }

    public class DataRefreshScheduler
    {
        public string ConnectionId { get; set; }
        public RefreshFrequency Frequency { get; set; }
        public TimeSpan RefreshTime { get; set; }
        public bool IsEnabled { get; set; }
        public DateTime? LastScheduledRun { get; set; }
        public DateTime? NextScheduledRun { get; set; }

        public DataRefreshScheduler()
        {
            Frequency = RefreshFrequency.Manual;
            IsEnabled = false;
        }

        public void CalculateNextRun()
        {
            if (!IsEnabled) return;

            switch (Frequency)
            {
                case RefreshFrequency.Minutes:
                    NextScheduledRun = DateTime.Now.AddMinutes(RefreshTime.TotalMinutes);
                    break;
                case RefreshFrequency.Hours:
                    NextScheduledRun = DateTime.Now.AddHours(RefreshTime.TotalHours);
                    break;
                case RefreshFrequency.Daily:
                    NextScheduledRun = DateTime.Now.Date.Add(RefreshTime);
                    if (NextScheduledRun < DateTime.Now)
                        NextScheduledRun = NextScheduledRun.Value.AddDays(1);
                    break;
            }
        }
    }

    public enum DataConnectionType
    {
        SqlServer,
        OleDb,
        Odbc,
        OData,
        Excel,
        Text,
        Web,
        SharePoint,
        XML,
        Access,
        Azure,
        Custom
    }

    public enum RefreshFrequency
    {
        Manual,
        Minutes,
        Hours,
        Daily,
        Weekly,
        Monthly
    }
}

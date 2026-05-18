using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Packaging;

namespace OfficeSuite.Word.MailMerge
{
    public class MailMergeEngine
    {
        private MailMergeDocument _document;
        private MailMergeDataSource _dataSource;
        private MailMergeSettings _settings;
        private List<MailMergeField> _fields;
        private List<MailMergeRecord> _records;
        private Dictionary<string, string> _fieldMappings;

        public MailMergeEngine()
        {
            _fields = new List<MailMergeField>();
            _records = new List<MailMergeRecord>();
            _fieldMappings = new Dictionary<string, string>();
            _settings = new MailMergeSettings();
        }

        public MailMergeDocument Document
        {
            get => _document;
            set => _document = value;
        }

        public MailMergeDataSource DataSource
        {
            get => _dataSource;
            set => _dataSource = value;
        }

        public MailMergeSettings Settings
        {
            get => _settings;
            set => _settings = value;
        }

        public List<MailMergeField> Fields => _fields;
        public List<MailMergeRecord> Records => _records;

        public void Initialize()
        {
            if (_document == null)
                throw new InvalidOperationException("Document must be set before initialization");

            ExtractFields();
        }

        private void ExtractFields()
        {
            _fields.Clear();
            
            if (_document.Content == null) return;

            var content = _document.Content;
            var fieldPattern = @"\{\s*MERGEFIELD\s+(\w+)\s*\}";
            var matches = System.Text.RegularExpressions.Regex.Matches(content, fieldPattern);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                var fieldName = match.Groups[1].Value;
                if (!_fields.Any(f => f.Name == fieldName))
                {
                    _fields.Add(new MailMergeField
                    {
                        Id = Guid.NewGuid().ToString(),
                        Name = fieldName,
                        DisplayName = FormatFieldName(fieldName),
                        Type = GetFieldType(fieldName)
                    });
                }
            }
        }

        private string FormatFieldName(string fieldName)
        {
            var result = fieldName.Replace('_', ' ');
            var words = new List<string>();
            var currentWord = "";

            foreach (var c in result)
            {
                if (char.IsUpper(c) && currentWord.Length > 0)
                {
                    words.Add(currentWord);
                    currentWord = c.ToString();
                }
                else
                {
                    currentWord += c;
                }
            }
            if (currentWord.Length > 0)
                words.Add(currentWord);

            return string.Join(" ", words);
        }

        private MailMergeFieldType GetFieldType(string fieldName)
        {
            var lowerName = fieldName.ToLowerInvariant();

            if (lowerName.Contains("email") || lowerName.Contains("mail"))
                return MailMergeFieldType.Email;
            if (lowerName.Contains("phone") || lowerName.Contains("tel"))
                return MailMergeFieldType.Phone;
            if (lowerName.Contains("address"))
                return MailMergeFieldType.Address;
            if (lowerName.Contains("date"))
                return MailMergeFieldType.Date;
            if (lowerName.Contains("company"))
                return MailMergeFieldType.Company;
            if (lowerName.Contains("name"))
                return MailMergeFieldType.Name;
            if (lowerName.Contains("city") || lowerName.Contains("state") || lowerName.Contains("zip") || lowerName.Contains("country"))
                return MailMergeFieldType.Address;

            return MailMergeFieldType.Text;
        }

        public void SetDataSource(MailMergeDataSource dataSource)
        {
            _dataSource = dataSource;
            _records = dataSource.Records.ToList();
            AutoMapFields();
        }

        private void AutoMapFields()
        {
            _fieldMappings.Clear();

            if (_dataSource == null || _dataSource.Columns.Count == 0) return;

            foreach (var field in _fields)
            {
                var matchingColumn = _dataSource.Columns
                    .FirstOrDefault(c => c.Name.Equals(field.Name, StringComparison.OrdinalIgnoreCase) ||
                                         c.Name.Equals(field.Name.Replace(" ", ""), StringComparison.OrdinalIgnoreCase));

                if (matchingColumn != null)
                {
                    _fieldMappings[field.Name] = matchingColumn.Name;
                }
            }
        }

        public void MapField(string fieldName, string columnName)
        {
            _fieldMappings[fieldName] = columnName;
        }

        public void UnmapField(string fieldName)
        {
            _fieldMappings.Remove(fieldName);
        }

        public string GetMappedColumn(string fieldName)
        {
            return _fieldMappings.TryGetValue(fieldName, out var column) ? column : null;
        }

        public MailMergeResult Execute()
        {
            var result = new MailMergeResult
            {
                StartTime = DateTime.Now,
                TotalRecords = _records.Count
            };

            if (_document == null)
            {
                result.Errors.Add("Document is not set");
                return result;
            }

            if (_dataSource == null)
            {
                result.Errors.Add("Data source is not set");
                return result;
            }

            result.OutputDocuments = new List<MailMergeOutputDocument>();

            for (int i = 0; i < _records.Count; i++)
            {
                try
                {
                    var record = _records[i];
                    var mergedContent = MergeDocument(record);

                    var outputDoc = new MailMergeOutputDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        RecordIndex = i,
                        Content = mergedContent,
                        RecordId = record.Id,
                        MergedFields = new Dictionary<string, string>()
                    };

                    foreach (var mapping in _fieldMappings)
                    {
                        var value = record.GetValue(mapping.Value);
                        outputDoc.MergedFields[mapping.Key] = value?.ToString() ?? "";
                    }

                    result.OutputDocuments.Add(outputDoc);
                    result.SuccessfulRecords++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error merging record {i}: {ex.Message}");
                    result.FailedRecords++;
                }

                result.ProcessedRecords = i + 1;
                OnProgressChanged?.Invoke(this, new MailMergeProgressEventArgs(result.ProcessedRecords, result.TotalRecords));
            }

            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            return result;
        }

        public MailMergeResult Execute(int startIndex, int count)
        {
            var result = new MailMergeResult
            {
                StartTime = DateTime.Now,
                TotalRecords = count,
                ProcessedRecords = 0
            };

            if (_document == null || _dataSource == null)
            {
                result.Errors.Add("Document or data source is not set");
                return result;
            }

            result.OutputDocuments = new List<MailMergeOutputDocument>();

            var endIndex = Math.Min(startIndex + count, _records.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                try
                {
                    var record = _records[i];
                    var mergedContent = MergeDocument(record);

                    var outputDoc = new MailMergeOutputDocument
                    {
                        Id = Guid.NewGuid().ToString(),
                        RecordIndex = i,
                        Content = mergedContent,
                        RecordId = record.Id,
                        MergedFields = new Dictionary<string, string>()
                    };

                    foreach (var mapping in _fieldMappings)
                    {
                        var value = record.GetValue(mapping.Value);
                        outputDoc.MergedFields[mapping.Key] = value?.ToString() ?? "";
                    }

                    result.OutputDocuments.Add(outputDoc);
                    result.SuccessfulRecords++;
                }
                catch (Exception ex)
                {
                    result.Errors.Add($"Error merging record {i}: {ex.Message}");
                    result.FailedRecords++;
                }

                result.ProcessedRecords++;
                OnProgressChanged?.Invoke(this, new MailMergeProgressEventArgs(result.ProcessedRecords, result.TotalRecords));
            }

            result.EndTime = DateTime.Now;
            result.Duration = result.EndTime - result.StartTime;
            return result;
        }

        private string MergeDocument(MailMergeRecord record)
        {
            var content = _document.Content;

            foreach (var field in _fields)
            {
                var fieldCode = $"{{ MERGEFIELD {field.Name} }}";
                var columnName = GetMappedColumn(field.Name);
                var value = columnName != null ? record.GetValue(columnName)?.ToString() ?? "" : "";

                if (_settings.NullHandling == NullHandling.ShowBlank)
                    value = value ?? "";
                else if (_settings.NullHandling == NullHandling.ShowDefault && string.IsNullOrEmpty(value))
                    value = _settings.DefaultNullText;

                content = content.Replace(fieldCode, value);
            }

            if (_settings.RemoveEmptyParagraphs)
            {
                content = RemoveEmptyParagraphs(content);
            }

            return content;
        }

        private string RemoveEmptyParagraphs(string content)
        {
            var lines = content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            var result = new List<string>();
            bool skipEmpty = false;

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    if (!skipEmpty)
                    {
                        result.Add(line);
                        skipEmpty = true;
                    }
                }
                else
                {
                    result.Add(line);
                    skipEmpty = false;
                }
            }

            return string.Join(Environment.NewLine, result);
        }

        public void SaveOutputDocuments(MailMergeResult result, string outputPath, OutputFormat format)
        {
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            for (int i = 0; i < result.OutputDocuments.Count; i++)
            {
                var doc = result.OutputDocuments[i];
                var fileName = GetOutputFileName(i, format);
                var filePath = Path.Combine(outputPath, fileName);

                File.WriteAllText(filePath, doc.Content);
                doc.OutputPath = filePath;
            }
        }

        private string GetOutputFileName(int index, OutputFormat format)
        {
            var extension = format == OutputFormat.Docx ? ".docx" :
                           format == OutputFormat.Doc ? ".doc" :
                           format == OutputFormat.Pdf ? ".pdf" :
                           format == OutputFormat.Txt ? ".txt" : ".txt";

            if (_settings.FileNaming == FileNaming.Sequential)
            {
                return $"Document_{index + 1:D4}{extension}";
            }
            else if (_settings.FileNaming == FileNaming.RecordId)
            {
                var record = _records[index];
                var id = record.Id ?? index.ToString();
                var safeName = RemoveInvalidFileNameChars(id);
                return $"{safeName}{extension}";
            }
            else
            {
                return $"Document_{index + 1}{extension}";
            }
        }

        private string RemoveInvalidFileNameChars(string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var result = fileName;
            foreach (var c in invalid)
            {
                result = result.Replace(c, '_');
            }
            return result;
        }

        public event EventHandler<MailMergeProgressEventArgs> OnProgressChanged;
    }

    public class MailMergeDocument
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Content { get; set; }
        public byte[] ContentBytes { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public MailMergeDocument()
        {
            Id = Guid.NewGuid().ToString();
            Properties = new Dictionary<string, object>();
            CreatedDate = DateTime.Now;
        }
    }

    public class MailMergeDataSource
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
        public DataSourceType Type { get; set; }
        public List<MailMergeColumn> Columns { get; set; }
        public List<MailMergeRecord> Records { get; set; }
        public bool IsConnected { get; set; }
        public DateTime LastModified { get; set; }
        public Dictionary<string, string> ConnectionProperties { get; set; }

        public MailMergeDataSource()
        {
            Id = Guid.NewGuid().ToString();
            Columns = new List<MailMergeColumn>();
            Records = new List<MailMergeRecord>();
            ConnectionProperties = new Dictionary<string, string>();
            IsConnected = false;
        }

        public void LoadFromFile(string filePath)
        {
            FilePath = filePath;
            Name = Path.GetFileNameWithoutExtension(filePath);
            Type = GetTypeFromExtension(Path.GetExtension(filePath));
            
            switch (Type)
            {
                case DataSourceType.Csv:
                case DataSourceType.Txt:
                    LoadFromCsv(filePath);
                    break;
                case DataSourceType.Excel:
                    LoadFromExcel(filePath);
                    break;
                case DataSourceType.Access:
                    LoadFromAccess(filePath);
                    break;
            }

            IsConnected = true;
            LastModified = DateTime.Now;
        }

        private DataSourceType GetTypeFromExtension(string extension)
        {
            switch (extension.ToLowerInvariant())
            {
                case ".csv":
                    return DataSourceType.Csv;
                case ".txt":
                    return DataSourceType.Txt;
                case ".xlsx":
                case ".xls":
                    return DataSourceType.Excel;
                case ".mdb":
                case ".accdb":
                    return DataSourceType.Access;
                default:
                    return DataSourceType.Csv;
            }
        }

        private void LoadFromCsv(string filePath)
        {
            var lines = File.ReadAllLines(filePath);
            if (lines.Length == 0) return;

            var headers = ParseCsvLine(lines[0]);
            Columns = headers.Select((h, i) => new MailMergeColumn
            {
                Name = h,
                Index = i,
                Type = MailMergeFieldType.Text
            }).ToList();

            Records.Clear();
            for (int i = 1; i < lines.Length; i++)
            {
                var values = ParseCsvLine(lines[i]);
                var record = new MailMergeRecord { Id = i.ToString() };
                for (int j = 0; j < values.Length && j < Columns.Count; j++)
                {
                    record.Values[Columns[j].Name] = values[j];
                }
                Records.Add(record);
            }
        }

        private string[] ParseCsvLine(string line)
        {
            var result = new List<string>();
            var current = "";
            bool inQuotes = false;

            foreach (var c in line)
            {
                if (c == '"')
                {
                    inQuotes = !inQuotes;
                }
                else if (c == ',' && !inQuotes)
                {
                    result.Add(current);
                    current = "";
                }
                else
                {
                    current += c;
                }
            }
            result.Add(current);

            return result.ToArray();
        }

        private void LoadFromExcel(string filePath)
        {
        }

        private void LoadFromAccess(string filePath)
        {
        }

        public void AddRecord(MailMergeRecord record)
        {
            record.Id = Guid.NewGuid().ToString();
            Records.Add(record);
        }

        public void RemoveRecord(string recordId)
        {
            Records.RemoveAll(r => r.Id == recordId);
        }

        public MailMergeRecord GetRecord(int index)
        {
            return index >= 0 && index < Records.Count ? Records[index] : null;
        }
    }

    public class MailMergeColumn
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public MailMergeFieldType Type { get; set; }
        public string DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public string Description { get; set; }

        public MailMergeColumn()
        {
            Type = MailMergeFieldType.Text;
            IsRequired = false;
        }
    }

    public class MailMergeRecord
    {
        public string Id { get; set; }
        public Dictionary<string, object> Values { get; set; }

        public MailMergeRecord()
        {
            Id = Guid.NewGuid().ToString();
            Values = new Dictionary<string, object>();
        }

        public object GetValue(string columnName)
        {
            return Values.TryGetValue(columnName, out var value) ? value : null;
        }

        public void SetValue(string columnName, object value)
        {
            Values[columnName] = value;
        }
    }

    public class MailMergeField
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public MailMergeFieldType Type { get; set; }
        public string DefaultValue { get; set; }
        public bool IsMapped { get; set; }
        public string MappedColumn { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public MailMergeField()
        {
            Id = Guid.NewGuid().ToString();
            Properties = new Dictionary<string, object>();
            IsMapped = false;
        }
    }

    public class MailMergeSettings
    {
        public NullHandling NullHandling { get; set; }
        public string DefaultNullText { get; set; }
        public FileNaming FileNaming { get; set; }
        public OutputFormat OutputFormat { get; set; }
        public string OutputDirectory { get; set; }
        public bool RemoveEmptyParagraphs { get; set; }
        public bool PreserveFormatting { get; set; }
        public string ErrorHandling { get; set; }
        public bool GeneratePreview { get; set; }
        public int PreviewRecordCount { get; set; }

        public MailMergeSettings()
        {
            NullHandling = NullHandling.ShowBlank;
            DefaultNullText = "";
            FileNaming = FileNaming.Sequential;
            OutputFormat = OutputFormat.Docx;
            RemoveEmptyParagraphs = false;
            PreserveFormatting = true;
            ErrorHandling = "Skip";
            GeneratePreview = true;
            PreviewRecordCount = 3;
        }
    }

    public class MailMergeResult
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public int TotalRecords { get; set; }
        public int ProcessedRecords { get; set; }
        public int SuccessfulRecords { get; set; }
        public int FailedRecords { get; set; }
        public List<MailMergeOutputDocument> OutputDocuments { get; set; }
        public List<string> Errors { get; set; }
        public Dictionary<string, object> Statistics { get; set; }

        public MailMergeResult()
        {
            OutputDocuments = new List<MailMergeOutputDocument>();
            Errors = new List<string>();
            Statistics = new Dictionary<string, object>();
        }

        public bool HasErrors => Errors.Count > 0;
        public bool IsComplete => ProcessedRecords == TotalRecords;
    }

    public class MailMergeOutputDocument
    {
        public string Id { get; set; }
        public int RecordIndex { get; set; }
        public string RecordId { get; set; }
        public string Content { get; set; }
        public string OutputPath { get; set; }
        public Dictionary<string, string> MergedFields { get; set; }
        public DateTime CreatedDate { get; set; }

        public MailMergeOutputDocument()
        {
            Id = Guid.NewGuid().ToString();
            MergedFields = new Dictionary<string, string>();
            CreatedDate = DateTime.Now;
        }
    }

    public class MailMergeProgressEventArgs : EventArgs
    {
        public int ProcessedRecords { get; }
        public int TotalRecords { get; }
        public double ProgressPercentage => TotalRecords > 0 ? (double)ProcessedRecords / TotalRecords * 100 : 0;

        public MailMergeProgressEventArgs(int processedRecords, int totalRecords)
        {
            ProcessedRecords = processedRecords;
            TotalRecords = totalRecords;
        }
    }

    public enum MailMergeFieldType
    {
        Text,
        Number,
        Date,
        Boolean,
        Name,
        Address,
        Email,
        Phone,
        Company,
        Currency,
        Custom
    }

    public enum DataSourceType
    {
        Csv,
        Txt,
        Excel,
        Access,
        SqlServer,
        Odbc,
        OleDb,
        Custom
    }

    public enum NullHandling
    {
        ShowBlank,
        ShowDefault,
        ShowError
    }

    public enum FileNaming
    {
        Sequential,
        RecordId,
        Custom
    }

    public enum OutputFormat
    {
        Docx,
        Doc,
        Pdf,
        Txt,
        Html
    }

    public class MailMergePreviewGenerator
    {
        private MailMergeEngine _engine;

        public MailMergePreviewGenerator(MailMergeEngine engine)
        {
            _engine = engine;
        }

        public List<string> GeneratePreviews(int count)
        {
            var previews = new List<string>();
            var settings = _engine.Settings;
            var originalSetting = settings.GeneratePreview;
            settings.GeneratePreview = false;

            for (int i = 0; i < count && i < _engine.Records.Count; i++)
            {
                var result = _engine.Execute(i, 1);
                if (result.OutputDocuments.Count > 0)
                {
                    previews.Add(result.OutputDocuments[0].Content);
                }
            }

            settings.GeneratePreview = originalSetting;
            return previews;
        }
    }

    public class MailMergeValidator
    {
        private List<string> _validationErrors;

        public MailMergeValidator()
        {
            _validationErrors = new List<string>();
        }

        public List<string> Validate(MailMergeEngine engine)
        {
            _validationErrors.Clear();

            ValidateDocument(engine.Document);
            ValidateDataSource(engine.DataSource);
            ValidateFieldMappings(engine);

            return _validationErrors;
        }

        private void ValidateDocument(MailMergeDocument document)
        {
            if (document == null)
            {
                _validationErrors.Add("Document is not set");
                return;
            }

            if (string.IsNullOrEmpty(document.Content))
            {
                _validationErrors.Add("Document content is empty");
            }
        }

        private void ValidateDataSource(MailMergeDataSource dataSource)
        {
            if (dataSource == null)
            {
                _validationErrors.Add("Data source is not set");
                return;
            }

            if (dataSource.Records.Count == 0)
            {
                _validationErrors.Add("Data source contains no records");
            }

            if (dataSource.Columns.Count == 0)
            {
                _validationErrors.Add("Data source contains no columns");
            }
        }

        private void ValidateFieldMappings(MailMergeEngine engine)
        {
            var mappedCount = engine.Fields.Count(f => !string.IsNullOrEmpty(engine.GetMappedColumn(f.Name)));

            if (mappedCount == 0 && engine.Fields.Count > 0)
            {
                _validationErrors.Add("No fields are mapped to data source columns");
            }
        }
    }
}

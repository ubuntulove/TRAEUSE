using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OfficeSuite.Word.TrackChanges;

namespace OfficeSuite.Word.Services
{
    public interface ITrackChangesService
    {
        Task StartTrackingAsync(string author);
        Task StopTrackingAsync();
        Task<ChangeInfo> RecordChangeAsync(ChangeType type, string oldValue, string newValue, int position);
        Task AcceptChangeAsync(string changeId);
        Task RejectChangeAsync(string changeId);
        Task AcceptAllChangesAsync();
        Task RejectAllChangesAsync();
        Task<List<ChangeInfo>> GetChangesAsync();
        Task<List<AuthorInfo>> GetAuthorsAsync();
        bool IsTrackingEnabled { get; }
        string? CurrentAuthor { get; }
    }

    public class TrackChangesService : ITrackChangesService
    {
        private readonly List<ChangeInfo> _changes;
        private readonly Dictionary<string, AuthorInfo> _authors;
        private readonly List<ReviewSession> _sessions;
        private bool _isTracking;
        private string? _currentAuthor;

        public bool IsTrackingEnabled => _isTracking;
        public string? CurrentAuthor => _currentAuthor;

        public TrackChangesService()
        {
            _changes = new List<ChangeInfo>();
            _authors = new Dictionary<string, AuthorInfo>();
            _sessions = new List<ReviewSession>();
        }

        public Task StartTrackingAsync(string author)
        {
            _isTracking = true;
            _currentAuthor = author;

            if (!_authors.ContainsKey(author))
            {
                _authors[author] = new AuthorInfo
                {
                    Name = author,
                    Initials = GetInitials(author),
                    FirstSessionDate = DateTime.Now
                };
            }

            var session = new ReviewSession
            {
                StartTime = DateTime.Now,
                Author = author
            };
            _sessions.Add(session);

            return Task.CompletedTask;
        }

        public Task StopTrackingAsync()
        {
            _isTracking = false;

            if (_sessions.Count > 0)
            {
                var currentSession = _sessions[_sessions.Count - 1];
                if (!currentSession.EndTime.HasValue)
                {
                    currentSession.EndTime = DateTime.Now;
                }
            }

            return Task.CompletedTask;
        }

        public Task<ChangeInfo> RecordChangeAsync(ChangeType type, string oldValue, string newValue, int position)
        {
            var change = new ChangeInfo
            {
                Type = type,
                Author = _currentAuthor,
                Timestamp = DateTime.Now,
                Position = position,
                OldValue = oldValue,
                NewValue = newValue,
                State = RevisionState.Pending
            };

            switch (type)
            {
                case ChangeType.Insertion:
                    change.Action = ChangeAction.Added;
                    break;
                case ChangeType.Deletion:
                    change.Action = ChangeAction.Deleted;
                    break;
                case ChangeType.Modification:
                    change.Action = ChangeAction.Modified;
                    break;
            }

            _changes.Add(change);

            if (_currentAuthor != null && _authors.ContainsKey(_currentAuthor))
            {
                _authors[_currentAuthor].ChangeCount++;
            }

            if (_sessions.Count > 0)
            {
                _sessions[_sessions.Count - 1].Changes.Add(change);
            }

            return Task.FromResult(change);
        }

        public Task AcceptChangeAsync(string changeId)
        {
            var change = _changes.Find(c => c.Id == changeId);
            if (change != null)
            {
                change.State = RevisionState.Accepted;
            }

            return Task.CompletedTask;
        }

        public Task RejectChangeAsync(string changeId)
        {
            var change = _changes.Find(c => c.Id == changeId);
            if (change != null)
            {
                change.State = RevisionState.Rejected;
            }

            return Task.CompletedTask;
        }

        public Task AcceptAllChangesAsync()
        {
            foreach (var change in _changes)
            {
                change.State = RevisionState.Accepted;
            }

            return Task.CompletedTask;
        }

        public Task RejectAllChangesAsync()
        {
            foreach (var change in _changes)
            {
                change.State = RevisionState.Rejected;
            }

            return Task.CompletedTask;
        }

        public Task<List<ChangeInfo>> GetChangesAsync()
        {
            return Task.FromResult(_changes);
        }

        public Task<List<AuthorInfo>> GetAuthorsAsync()
        {
            return Task.FromResult(_authors.Values.ToList());
        }

        private string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name))
                return "UN";

            var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0)
                return name.Substring(0, Math.Min(2, name.Length)).ToUpper();

            if (parts.Length == 1)
                return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

            return (parts[0][0] + "" + parts[parts.Length - 1][0]).ToUpper();
        }
    }

    public interface IMailMergeService
    {
        Task<bool> ConnectDataSourceAsync(string connectionString);
        Task<List<string>> GetAvailableFieldsAsync();
        Task<List<Dictionary<string, object>>> GetDataRecordsAsync();
        Task<string> MergeFieldAsync(string fieldName, Dictionary<string, object> record);
        Task<string> ExecuteMergeAsync(string template);
        Task DisconnectDataSourceAsync();
        bool IsConnected { get; }
        string? DataSourceName { get; }
    }

    public class MailMergeService : IMailMergeService
    {
        private string? _connectionString;
        private string? _dataSourceName;
        private List<string> _availableFields;
        private List<Dictionary<string, object>> _dataRecords;

        public bool IsConnected => !string.IsNullOrEmpty(_connectionString);
        public string? DataSourceName => _dataSourceName;

        public MailMergeService()
        {
            _availableFields = new List<string>();
            _dataRecords = new List<Dictionary<string, object>>();
        }

        public Task<bool> ConnectDataSourceAsync(string connectionString)
        {
            _connectionString = connectionString;
            _dataSourceName = ExtractDataSourceName(connectionString);
            LoadMockData();

            return Task.FromResult(true);
        }

        public Task<List<string>> GetAvailableFieldsAsync()
        {
            return Task.FromResult(_availableFields);
        }

        public Task<List<Dictionary<string, object>>> GetDataRecordsAsync()
        {
            return Task.FromResult(_dataRecords);
        }

        public Task<string> MergeFieldAsync(string fieldName, Dictionary<string, object> record)
        {
            if (record.ContainsKey(fieldName))
            {
                return Task.FromResult(record[fieldName]?.ToString() ?? string.Empty);
            }

            return Task.FromResult("«" + fieldName + "»");
        }

        public Task<string> ExecuteMergeAsync(string template)
        {
            var mergedDocument = new System.Text.StringBuilder();
            var lines = template.Split('\n');

            foreach (var line in lines)
            {
                var mergedLine = line;

                foreach (var field in _availableFields)
                {
                    if (_dataRecords.Count > 0)
                    {
                        var placeholder = "«" + field + "»";
                        var value = _dataRecords[0].ContainsKey(field)
                            ? _dataRecords[0][field]?.ToString() ?? string.Empty
                            : placeholder;
                        mergedLine = mergedLine.Replace(placeholder, value);
                    }
                }

                mergedDocument.AppendLine(mergedLine);
            }

            return Task.FromResult(mergedDocument.ToString());
        }

        public Task DisconnectDataSourceAsync()
        {
            _connectionString = null;
            _dataSourceName = null;
            _availableFields.Clear();
            _dataRecords.Clear();

            return Task.CompletedTask;
        }

        private string ExtractDataSourceName(string connectionString)
        {
            if (connectionString.Contains("Data Source="))
            {
                var parts = connectionString.Split(';');
                foreach (var part in parts)
                {
                    if (part.Trim().StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                    {
                        return System.IO.Path.GetFileName(part.Split('=')[1].Trim());
                    }
                }
            }

            return "Data Source";
        }

        private void LoadMockData()
        {
            _availableFields = new List<string>
            {
                "FirstName", "LastName", "Email", "Company", "Address", "City", "State", "ZipCode", "Phone"
            };

            _dataRecords = new List<Dictionary<string, object>>
            {
                new()
                {
                    ["FirstName"] = "John",
                    ["LastName"] = "Doe",
                    ["Email"] = "john.doe@example.com",
                    ["Company"] = "Acme Corp",
                    ["Address"] = "123 Main St",
                    ["City"] = "New York",
                    ["State"] = "NY",
                    ["ZipCode"] = "10001",
                    ["Phone"] = "555-1234"
                },
                new()
                {
                    ["FirstName"] = "Jane",
                    ["LastName"] = "Smith",
                    ["Email"] = "jane.smith@example.com",
                    ["Company"] = "Tech Solutions",
                    ["Address"] = "456 Oak Ave",
                    ["City"] = "Los Angeles",
                    ["State"] = "CA",
                    ["ZipCode"] = "90001",
                    ["Phone"] = "555-5678"
                },
                new()
                {
                    ["FirstName"] = "Bob",
                    ["LastName"] = "Johnson",
                    ["Email"] = "bob.j@example.com",
                    ["Company"] = "Global Inc",
                    ["Address"] = "789 Pine Rd",
                    ["City"] = "Chicago",
                    ["State"] = "IL",
                    ["ZipCode"] = "60601",
                    ["Phone"] = "555-9012"
                }
            };
        }
    }

    public interface ITableOfContentsService
    {
        Task<string> GenerateTableOfContentsAsync(string documentContent, TableOfContentsOptions options = null);
        Task UpdateTableOfContentsAsync(string existingToc, string documentContent);
        List<HeadingInfo> ExtractHeadings(string content, int maxLevel = 10);
    }

    public class HeadingInfo
    {
        public int Level { get; set; }
        public string Text { get; set; } = string.Empty;
        public int PageNumber { get; set; }
        public string? BookmarkName { get; set; }
    }

    public class TableOfContentsOptions
    {
        public int StartLevel { get; set; } = 1;
        public int EndLevel { get; set; } = 10;
        public bool IncludePageNumbers { get; set; } = true;
        public bool IncludeHyperlinks { get; set; } = true;
        public bool UseOutlineStyles { get; set; } = true;
        public string TabLeader { get; set; } = "dot";
        public int RightTabPosition { get; set; } = 9360;
    }

    public class TableOfContentsService : ITableOfContentsService
    {
        public Task<string> GenerateTableOfContentsAsync(string documentContent, TableOfContentsOptions options = null)
        {
            options ??= new TableOfContentsOptions();

            var headings = ExtractHeadings(documentContent, options.EndLevel);
            var filteredHeadings = headings.Where(h => h.Level >= options.StartLevel && h.Level <= options.EndLevel);

            var toc = new System.Text.StringBuilder();
            toc.AppendLine("Table of Contents");
            toc.AppendLine();

            foreach (var heading in filteredHeadings)
            {
                int indent = (heading.Level - options.StartLevel) * 2;
                var indentStr = new string(' ', indent);

                var entry = new System.Text.StringBuilder();
                entry.Append(indentStr);
                entry.Append(heading.Text);

                if (options.IncludePageNumbers)
                {
                    var leader = GetTabLeader(options.TabLeader);
                    var pageNumStr = heading.PageNumber.ToString();
                    var spacesNeeded = Math.Max(1, 50 - heading.Text.Length - indent);
                    entry.Append(new string(' ', spacesNeeded));
                    entry.Append(leader);
                    entry.Append(" ");
                    entry.Append(pageNumStr);
                }

                toc.AppendLine(entry.ToString());
            }

            return Task.FromResult(toc.ToString());
        }

        public Task UpdateTableOfContentsAsync(string existingToc, string documentContent)
        {
            return GenerateTableOfContentsAsync(documentContent);
        }

        public List<HeadingInfo> ExtractHeadings(string content, int maxLevel = 10)
        {
            var headings = new List<HeadingInfo>();
            var lines = content.Split('\n');
            int pageNumber = 1;

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i].TrimEnd();

                if (line.StartsWith("#"))
                {
                    int level = Math.Min(line.TakeWhile(c => c == '#').Count(), maxLevel);
                    var text = line.TrimStart('#').Trim();

                    headings.Add(new HeadingInfo
                    {
                        Level = level,
                        Text = text,
                        PageNumber = pageNumber,
                        BookmarkName = "Heading_" + headings.Count
                    });
                }
                else if (line.StartsWith("Heading") && int.TryParse(line.Split(' ').LastOrDefault(), out int hLevel))
                {
                    headings.Add(new HeadingInfo
                    {
                        Level = Math.Min(hLevel, maxLevel),
                        Text = line,
                        PageNumber = pageNumber,
                        BookmarkName = "Heading_" + headings.Count
                    });
                }
                else if (line.Length == 0)
                {
                    pageNumber++;
                }
            }

            return headings;
        }

        private string GetTabLeader(string leaderType)
        {
            return leaderType.ToLower() switch
            {
                "dot" => " . ",
                "dash" => " - ",
                "underline" => " _ ",
                "line" => " ___ ",
                _ => " . "
            };
        }
    }

    public interface IDocumentProtectionService
    {
        Task<bool> ProtectDocumentAsync(string password, ProtectionType type);
        Task<bool> UnprotectDocumentAsync(string password);
        Task<bool> ValidatePasswordAsync(string password);
        bool IsProtected { get; }
        ProtectionType? ProtectionType { get; }
        Task<bool> HasEditRestrictionAsync(EditRestriction restriction);
    }

    public enum ProtectionType
    {
        ReadOnly,
        CommentsOnly,
        TrackChanges,
        Forms,
        FillingForms
    }

    public enum EditRestriction
    {
        None,
        Comments,
        TrackedChanges,
        ReadOnly,
        Forms
    }

    public class DocumentProtectionService : IDocumentProtectionService
    {
        private string? _passwordHash;
        private ProtectionType? _currentProtectionType;
        private readonly HashSet<EditRestriction> _activeRestrictions;

        public bool IsProtected => _passwordHash != null;
        public ProtectionType? ProtectionType => _currentProtectionType;

        public DocumentProtectionService()
        {
            _activeRestrictions = new HashSet<EditRestriction>();
        }

        public Task<bool> ProtectDocumentAsync(string password, ProtectionType type)
        {
            if (string.IsNullOrEmpty(password))
                return Task.FromResult(false);

            _passwordHash = HashPassword(password);
            _currentProtectionType = type;

            _activeRestrictions.Clear();

            switch (type)
            {
                case ProtectionType.ReadOnly:
                    _activeRestrictions.Add(EditRestriction.ReadOnly);
                    break;
                case ProtectionType.CommentsOnly:
                    _activeRestrictions.Add(EditRestriction.Comments);
                    break;
                case ProtectionType.TrackChanges:
                    _activeRestrictions.Add(EditRestriction.TrackedChanges);
                    break;
                case ProtectionType.Forms:
                case ProtectionType.FillingForms:
                    _activeRestrictions.Add(EditRestriction.Forms);
                    break;
            }

            return Task.FromResult(true);
        }

        public Task<bool> UnprotectDocumentAsync(string password)
        {
            if (!IsProtected)
                return Task.FromResult(true);

            if (ValidatePasswordInternal(password))
            {
                _passwordHash = null;
                _currentProtectionType = null;
                _activeRestrictions.Clear();
                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }

        public Task<bool> ValidatePasswordAsync(string password)
        {
            return Task.FromResult(ValidatePasswordInternal(password));
        }

        public Task<bool> HasEditRestrictionAsync(EditRestriction restriction)
        {
            return Task.FromResult(_activeRestrictions.Contains(restriction));
        }

        private bool ValidatePasswordInternal(string password)
        {
            if (string.IsNullOrEmpty(_passwordHash))
                return false;

            return HashPassword(password) == _passwordHash;
        }

        private string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }
    }
}

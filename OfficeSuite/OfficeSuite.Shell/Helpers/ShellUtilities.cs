using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OfficeSuite.Shell.Helpers
{
    public class DocumentHistoryManager
    {
        private readonly ObservableCollection<RecentDocument> _recentDocuments;
        private readonly ObservableCollection<DocumentVersion> _documentVersions;
        private readonly Dictionary<string, DocumentState> _documentStates;
        private readonly int _maxRecentDocuments;
        private readonly int _maxVersions;

        public DocumentHistoryManager(int maxRecentDocuments = 20, int maxVersions = 50)
        {
            _maxRecentDocuments = maxRecentDocuments;
            _maxVersions = maxVersions;
            _recentDocuments = new ObservableCollection<RecentDocument>();
            _documentVersions = new ObservableCollection<DocumentVersion>();
            _documentStates = new Dictionary<string, DocumentState>();
        }

        public void AddRecentDocument(string filePath, string documentType)
        {
            var existing = _recentDocuments.FirstOrDefault(d => d.FilePath == filePath);
            if (existing != null)
            {
                existing.AccessedTime = DateTime.Now;
                existing.AccessCount++;
                MoveToTop(existing);
            }
            else
            {
                var document = new RecentDocument
                {
                    FilePath = filePath,
                    DocumentType = documentType,
                    Title = Path.GetFileNameWithoutExtension(filePath),
                    AccessedTime = DateTime.Now,
                    AccessCount = 1
                };

                _recentDocuments.Insert(0, document);
                TrimRecentDocuments();
            }

            OnDocumentAccessed?.Invoke(this, new DocumentAccessedEventArgs(filePath));
        }

        public void RemoveRecentDocument(string filePath)
        {
            var document = _recentDocuments.FirstOrDefault(d => d.FilePath == filePath);
            if (document != null)
            {
                _recentDocuments.Remove(document);
            }
        }

        public void ClearRecentDocuments()
        {
            _recentDocuments.Clear();
            OnRecentDocumentsCleared?.Invoke(this, EventArgs.Empty);
        }

        private void MoveToTop(RecentDocument document)
        {
            int index = _recentDocuments.IndexOf(document);
            if (index > 0)
            {
                _recentDocuments.RemoveAt(index);
                _recentDocuments.Insert(0, document);
            }
        }

        private void TrimRecentDocuments()
        {
            while (_recentDocuments.Count > _maxRecentDocuments)
            {
                _recentDocuments.RemoveAt(_recentDocuments.Count - 1);
            }
        }

        public void SaveVersion(string documentId, DocumentState state, string description = null)
        {
            var version = new DocumentVersion
            {
                VersionId = Guid.NewGuid().ToString(),
                DocumentId = documentId,
                State = state,
                Description = description,
                Timestamp = DateTime.Now,
                Size = state.GetSize()
            };

            _documentVersions.Add(version);
            TrimVersions(documentId);
            SaveVersionToStorage(version);
        }

        private void TrimVersions(string documentId)
        {
            var versions = _documentVersions.Where(v => v.DocumentId == documentId).ToList();
            while (versions.Count > _maxVersions)
            {
                var oldest = versions.OrderBy(v => v.Timestamp).First();
                _documentVersions.Remove(oldest);
                versions.Remove(oldest);
                DeleteVersionFromStorage(oldest.VersionId);
            }
        }

        public IEnumerable<DocumentVersion> GetVersions(string documentId)
        {
            return _documentVersions
                .Where(v => v.DocumentId == documentId)
                .OrderByDescending(v => v.Timestamp);
        }

        public DocumentVersion GetVersion(string versionId)
        {
            return _documentVersions.FirstOrDefault(v => v.VersionId == versionId);
        }

        public DocumentState RestoreVersion(string versionId)
        {
            var version = GetVersion(versionId);
            return version?.State;
        }

        public void SaveDocumentState(string documentId, DocumentState state)
        {
            _documentStates[documentId] = state;
            OnDocumentStateChanged?.Invoke(this, new DocumentStateChangedEventArgs(documentId, state));
        }

        public DocumentState GetDocumentState(string documentId)
        {
            return _documentStates.TryGetValue(documentId, out var state) ? state : null;
        }

        public void RemoveDocumentState(string documentId)
        {
            _documentStates.Remove(documentId);
        }

        public void ClearAllStates()
        {
            _documentStates.Clear();
        }

        private void SaveVersionToStorage(DocumentVersion version)
        {
        }

        private void DeleteVersionFromStorage(string versionId)
        {
        }

        public ReadOnlyObservableCollection<RecentDocument> RecentDocuments =>
            new ReadOnlyObservableCollection<RecentDocument>(_recentDocuments);

        public ReadOnlyObservableCollection<DocumentVersion> DocumentVersions =>
            new ReadOnlyObservableCollection<DocumentVersion>(_documentVersions);

        public event EventHandler<DocumentAccessedEventArgs> OnDocumentAccessed;
        public event EventHandler<EventArgs> OnRecentDocumentsCleared;
        public event EventHandler<DocumentStateChangedEventArgs> OnDocumentStateChanged;
    }

    public class RecentDocument : INotifyPropertyChanged
    {
        private string _filePath;
        private string _documentType;
        private string _title;
        private DateTime _accessedTime;
        private int _accessCount;
        private bool _isPinned;

        public string FilePath
        {
            get => _filePath;
            set { _filePath = value; OnPropertyChanged(); }
        }

        public string DocumentType
        {
            get => _documentType;
            set { _documentType = value; OnPropertyChanged(); }
        }

        public string Title
        {
            get => _title;
            set { _title = value; OnPropertyChanged(); }
        }

        public DateTime AccessedTime
        {
            get => _accessedTime;
            set { _accessedTime = value; OnPropertyChanged(); }
        }

        public int AccessCount
        {
            get => _accessCount;
            set { _accessCount = value; OnPropertyChanged(); }
        }

        public bool IsPinned
        {
            get => _isPinned;
            set { _isPinned = value; OnPropertyChanged(); }
        }

        public string Directory => Path.GetDirectoryName(FilePath);
        public string Extension => Path.GetExtension(FilePath);
        public bool Exists => File.Exists(FilePath);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DocumentVersion
    {
        public string VersionId { get; set; }
        public string DocumentId { get; set; }
        public DocumentState State { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public long Size { get; set; }
        public bool IsAutoSave { get; set; }
    }

    public class DocumentState
    {
        public string DocumentId { get; set; }
        public byte[] Content { get; set; }
        public string FilePath { get; set; }
        public DateTime SavedAt { get; set; }
        public Dictionary<string, object> Metadata { get; set; }

        public long GetSize()
        {
            return Content?.Length ?? 0;
        }
    }

    public class DocumentAccessedEventArgs : EventArgs
    {
        public string FilePath { get; }

        public DocumentAccessedEventArgs(string filePath)
        {
            FilePath = filePath;
        }
    }

    public class DocumentStateChangedEventArgs : EventArgs
    {
        public string DocumentId { get; }
        public DocumentState State { get; }

        public DocumentStateChangedEventArgs(string documentId, DocumentState state)
        {
            DocumentId = documentId;
            State = state;
        }
    }

    public class ClipboardManager
    {
        private readonly ObservableCollection<ClipboardEntry> _entries;
        private readonly int _maxEntries;

        public ClipboardManager(int maxEntries = 20)
        {
            _maxEntries = maxEntries;
            _entries = new ObservableCollection<ClipboardEntry>();
        }

        public void AddEntry(ClipboardEntry entry)
        {
            entry.Timestamp = DateTime.Now;
            entry.EntryId = Guid.NewGuid().ToString();

            _entries.Insert(0, entry);
            TrimEntries();
            OnEntryAdded?.Invoke(this, new ClipboardEntryEventArgs(entry));
        }

        public void RemoveEntry(string entryId)
        {
            var entry = _entries.FirstOrDefault(e => e.EntryId == entryId);
            if (entry != null)
            {
                _entries.Remove(entry);
                OnEntryRemoved?.Invoke(this, new ClipboardEntryEventArgs(entry));
            }
        }

        public void ClearEntries()
        {
            _entries.Clear();
            OnEntriesCleared?.Invoke(this, EventArgs.Empty);
        }

        private void TrimEntries()
        {
            while (_entries.Count > _maxEntries)
            {
                _entries.RemoveAt(_entries.Count - 1);
            }
        }

        public IEnumerable<ClipboardEntry> GetEntries(ClipboardEntryType? type = null)
        {
            var entries = _entries.AsEnumerable();
            if (type.HasValue)
                entries = entries.Where(e => e.EntryType == type.Value);
            return entries;
        }

        public ClipboardEntry GetEntry(string entryId)
        {
            return _entries.FirstOrDefault(e => e.EntryId == entryId);
        }

        public ReadOnlyObservableCollection<ClipboardEntry> Entries =>
            new ReadOnlyObservableCollection<ClipboardEntry>(_entries);

        public event EventHandler<ClipboardEntryEventArgs> OnEntryAdded;
        public event EventHandler<ClipboardEntryEventArgs> OnEntryRemoved;
        public event EventHandler<EventArgs> OnEntriesCleared;
    }

    public class ClipboardEntry
    {
        public string EntryId { get; set; }
        public ClipboardEntryType EntryType { get; set; }
        public object Content { get; set; }
        public string Text { get; set; }
        public string Description { get; set; }
        public DateTime Timestamp { get; set; }
        public string SourceApplication { get; set; }
        public byte[] ImageData { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
    }

    public enum ClipboardEntryType
    {
        Text,
        Image,
        File,
        Table,
        Chart,
        Shape,
        Other
    }

    public class ClipboardEntryEventArgs : EventArgs
    {
        public ClipboardEntry Entry { get; }

        public ClipboardEntryEventArgs(ClipboardEntry entry)
        {
            Entry = entry;
        }
    }

    public class MacroManager
    {
        private readonly Dictionary<string, Macro> _macros;
        private readonly ObservableCollection<Macro> _macroHistory;
        private readonly int _maxHistory;

        public MacroManager(int maxHistory = 50)
        {
            _macros = new Dictionary<string, Macro>();
            _macroHistory = new ObservableCollection<Macro>();
            _maxHistory = maxHistory;
        }

        public void RegisterMacro(Macro macro)
        {
            if (macro == null || string.IsNullOrEmpty(macro.Name))
                throw new ArgumentException("Macro name cannot be null or empty");

            _macros[macro.Name] = macro;
            OnMacroRegistered?.Invoke(this, new MacroEventArgs(macro));
        }

        public void UnregisterMacro(string name)
        {
            if (_macros.TryGetValue(name, out var macro))
            {
                _macros.Remove(name);
                OnMacroUnregistered?.Invoke(this, new MacroEventArgs(macro));
            }
        }

        public Macro GetMacro(string name)
        {
            return _macros.TryGetValue(name, out var macro) ? macro : null;
        }

        public IEnumerable<Macro> GetAllMacros()
        {
            return _macros.Values;
        }

        public async Task ExecuteMacroAsync(string name, MacroContext context)
        {
            var macro = GetMacro(name);
            if (macro == null)
                throw new InvalidOperationException($"Macro '{name}' not found");

            macro.LastExecuted = DateTime.Now;
            macro.ExecutionCount++;
            AddToHistory(macro);

            try
            {
                await macro.ExecuteAsync(context);
                OnMacroExecuted?.Invoke(this, new MacroExecutedEventArgs(macro, true, null));
            }
            catch (Exception ex)
            {
                OnMacroExecuted?.Invoke(this, new MacroExecutedEventArgs(macro, false, ex));
                throw;
            }
        }

        public void ExecuteMacro(string name, MacroContext context)
        {
            ExecuteMacroAsync(name, context).Wait();
        }

        private void AddToHistory(Macro macro)
        {
            var historyEntry = new Macro
            {
                Name = macro.Name,
                Description = macro.Description,
                Category = macro.Category,
                Code = macro.Code,
                Parameters = macro.Parameters
            };

            _macroHistory.Insert(0, historyEntry);
            TrimHistory();
        }

        private void TrimHistory()
        {
            while (_macroHistory.Count > _maxHistory)
            {
                _macroHistory.RemoveAt(_macroHistory.Count - 1);
            }
        }

        public ReadOnlyObservableCollection<Macro> MacroHistory =>
            new ReadOnlyObservableCollection<Macro>(_macroHistory);

        public event EventHandler<MacroEventArgs> OnMacroRegistered;
        public event EventHandler<MacroEventArgs> OnMacroUnregistered;
        public event EventHandler<MacroExecutedEventArgs> OnMacroExecuted;
    }

    public class Macro
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Code { get; set; }
        public List<MacroParameter> Parameters { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? LastExecuted { get; set; }
        public int ExecutionCount { get; set; }
        public bool IsEnabled { get; set; } = true;
        public MacroSecurityLevel SecurityLevel { get; set; } = MacroSecurityLevel.Normal;

        public Task ExecuteAsync(MacroContext context)
        {
            return Task.CompletedTask;
        }
    }

    public class MacroParameter
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Type ParameterType { get; set; }
        public object DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public string[] AllowedValues { get; set; }
    }

    public class MacroContext
    {
        public string DocumentId { get; set; }
        public IDocument Document { get; set; }
        public ISelection Selection { get; set; }
        public IDictionary<string, object> Variables { get; set; }
        public IWindow Window { get; set; }
    }

    public enum MacroSecurityLevel
    {
        Disabled,
        Low,
        Normal,
        High,
        VeryHigh
    }

    public interface IDocument { }
    public interface ISelection { }
    public interface IWindow { }

    public class MacroEventArgs : EventArgs
    {
        public Macro Macro { get; }

        public MacroEventArgs(Macro macro)
        {
            Macro = macro;
        }
    }

    public class MacroExecutedEventArgs : EventArgs
    {
        public Macro Macro { get; }
        public bool Success { get; }
        public Exception Error { get; }

        public MacroExecutedEventArgs(Macro macro, bool success, Exception error)
        {
            Macro = macro;
            Success = success;
            Error = error;
        }
    }

    public class TemplateManager
    {
        private readonly Dictionary<string, DocumentTemplate> _templates;
        private readonly ObservableCollection<TemplateCategory> _categories;
        private string _defaultTemplatePath;

        public TemplateManager()
        {
            _templates = new Dictionary<string, DocumentTemplate>();
            _categories = new ObservableCollection<TemplateCategory>();
            InitializeDefaultCategories();
        }

        private void InitializeDefaultCategories()
        {
            _categories.Add(new TemplateCategory { Name = "General", Icon = "Document", Description = "General purpose templates" });
            _categories.Add(new TemplateCategory { Name = "Reports", Icon = "Report", Description = "Business and academic reports" });
            _categories.Add(new TemplateCategory { Name = "Letters", Icon = "Letter", Description = "Business and personal letters" });
            _categories.Add(new TemplateCategory { Name = "Memos", Icon = "Memo", Description = "Internal communications" });
            _categories.Add(new TemplateCategory { Name = "Presentations", Icon = "Presentation", Description = "Slides and presentations" });
            _categories.Add(new TemplateCategory { Name = "Financial", Icon = "Financial", Description = "Budgets, invoices, and financial documents" });
            _categories.Add(new TemplateCategory { Name = "Educational", Icon = "Education", Description = "Academic and educational materials" });
            _categories.Add(new TemplateCategory { Name = "Personal", Icon = "Personal", Description = "Personal use templates" });
        }

        public void RegisterTemplate(DocumentTemplate template)
        {
            if (template == null || string.IsNullOrEmpty(template.Name))
                throw new ArgumentException("Template name cannot be null or empty");

            _templates[template.Name] = template;
            OnTemplateRegistered?.Invoke(this, new TemplateEventArgs(template));
        }

        public void UnregisterTemplate(string name)
        {
            if (_templates.TryGetValue(name, out var template))
            {
                _templates.Remove(name);
                OnTemplateUnregistered?.Invoke(this, new TemplateEventArgs(template));
            }
        }

        public DocumentTemplate GetTemplate(string name)
        {
            return _templates.TryGetValue(name, out var template) ? template : null;
        }

        public IEnumerable<DocumentTemplate> GetTemplates(string category = null, DocumentType? type = null)
        {
            var templates = _templates.Values.AsEnumerable();

            if (!string.IsNullOrEmpty(category))
                templates = templates.Where(t => t.Category == category);

            if (type.HasValue)
                templates = templates.Where(t => t.DocumentType == type.Value);

            return templates.OrderBy(t => t.Name);
        }

        public IEnumerable<DocumentTemplate> SearchTemplates(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return GetTemplates();

            var keywords = query.ToLower().Split(' ');
            return _templates.Values.Where(t =>
                keywords.Any(k =>
                    t.Name.ToLower().Contains(k) ||
                    (t.Description?.ToLower().Contains(k) ?? false) ||
                    (t.Tags?.Any(tag => tag.ToLower().Contains(k)) ?? false)));
        }

        public void SetDefaultTemplate(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                _defaultTemplatePath = GetTemplate(name)?.FilePath;
                OnDefaultTemplateChanged?.Invoke(this, new TemplateEventArgs(GetTemplate(name)));
            }
        }

        public DocumentTemplate GetDefaultTemplate()
        {
            if (!string.IsNullOrEmpty(_defaultTemplatePath))
                return _templates.Values.FirstOrDefault(t => t.FilePath == _defaultTemplatePath);
            return null;
        }

        public async Task<DocumentTemplate> CreateFromTemplateAsync(string templateName, string outputPath)
        {
            var template = GetTemplate(templateName);
            if (template == null)
                throw new InvalidOperationException($"Template '{templateName}' not found");

            if (!File.Exists(template.FilePath))
                throw new FileNotFoundException($"Template file not found: {template.FilePath}");

            var content = await File.ReadAllBytesAsync(template.FilePath);
            await File.WriteAllBytesAsync(outputPath, content);

            OnTemplateUsed?.Invoke(this, new TemplateUsedEventArgs(template, outputPath));
            return template;
        }

        public void AddCategory(TemplateCategory category)
        {
            if (category == null || _categories.Any(c => c.Name == category.Name))
                return;

            _categories.Add(category);
        }

        public ReadOnlyObservableCollection<TemplateCategory> Categories =>
            new ReadOnlyObservableCollection<TemplateCategory>(_categories);

        public event EventHandler<TemplateEventArgs> OnTemplateRegistered;
        public event EventHandler<TemplateEventArgs> OnTemplateUnregistered;
        public event EventHandler<TemplateEventArgs> OnDefaultTemplateChanged;
        public event EventHandler<TemplateUsedEventArgs> OnTemplateUsed;
    }

    public class DocumentTemplate
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string FilePath { get; set; }
        public DocumentType DocumentType { get; set; }
        public string ThumbnailPath { get; set; }
        public string[] Tags { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
        public DateTime? LastModified { get; set; }
        public int UsageCount { get; set; }
        public bool IsBuiltIn { get; set; }
        public Dictionary<string, object> Metadata { get; set; }
    }

    public class TemplateCategory
    {
        public string Name { get; set; }
        public string Icon { get; set; }
        public string Description { get; set; }
        public int TemplateCount { get; set; }
    }

    public enum DocumentType
    {
        Word,
        Excel,
        PowerPoint,
        Unknown
    }

    public class TemplateEventArgs : EventArgs
    {
        public DocumentTemplate Template { get; }

        public TemplateEventArgs(DocumentTemplate template)
        {
            Template = template;
        }
    }

    public class TemplateUsedEventArgs : EventArgs
    {
        public DocumentTemplate Template { get; }
        public string OutputPath { get; }

        public TemplateUsedEventArgs(DocumentTemplate template, string outputPath)
        {
            Template = template;
            OutputPath = outputPath;
        }
    }

    public class KeyboardShortcutManager
    {
        private readonly Dictionary<string, KeyboardShortcut> _shortcuts;
        private readonly Dictionary<string, List<string>> _shortcutConflicts;

        public KeyboardShortcutManager()
        {
            _shortcuts = new Dictionary<string, KeyboardShortcut>();
            _shortcutConflicts = new Dictionary<string, List<string>>();
            InitializeDefaultShortcuts();
        }

        private void InitializeDefaultShortcuts()
        {
            RegisterShortcut(new KeyboardShortcut
            {
                Name = "NewDocument",
                Key = Key.N,
                Modifiers = ModifierKeys.Control,
                Category = "File",
                Description = "Create new document"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "OpenDocument",
                Key = Key.O,
                Modifiers = ModifierKeys.Control,
                Category = "File",
                Description = "Open existing document"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "SaveDocument",
                Key = Key.S,
                Modifiers = ModifierKeys.Control,
                Category = "File",
                Description = "Save current document"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "PrintDocument",
                Key = Key.P,
                Modifiers = ModifierKeys.Control,
                Category = "File",
                Description = "Print current document"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Undo",
                Key = Key.Z,
                Modifiers = ModifierKeys.Control,
                Category = "Edit",
                Description = "Undo last action"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Redo",
                Key = Key.Y,
                Modifiers = ModifierKeys.Control,
                Category = "Edit",
                Description = "Redo last undone action"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Cut",
                Key = Key.X,
                Modifiers = ModifierKeys.Control,
                Category = "Edit",
                Description = "Cut selection"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Copy",
                Key = Key.C,
                Modifiers = ModifierKeys.Control,
                Category = "Edit",
                Description = "Copy selection"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Paste",
                Key = Key.V,
                Modifiers = ModifierKeys.Control,
                Category = "Edit",
                Description = "Paste clipboard contents"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "SelectAll",
                Key = Key.A,
                Modifiers = ModifierKeys.Control,
                Category = "Edit",
                Description = "Select all content"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Find",
                Key = Key.F,
                Modifiers = ModifierKeys.Control,
                Category = "Edit",
                Description = "Find text"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Replace",
                Key = Key.H,
                Modifiers = ModifierKeys.Control,
                Category = "Edit",
                Description = "Find and replace text"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Bold",
                Key = Key.B,
                Modifiers = ModifierKeys.Control,
                Category = "Format",
                Description = "Apply bold formatting"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Italic",
                Key = Key.I,
                Modifiers = ModifierKeys.Control,
                Category = "Format",
                Description = "Apply italic formatting"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Underline",
                Key = Key.U,
                Modifiers = ModifierKeys.Control,
                Category = "Format",
                Description = "Apply underline formatting"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "ZoomIn",
                Key = Key.OemPlus,
                Modifiers = ModifierKeys.Control,
                Category = "View",
                Description = "Zoom in"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "ZoomOut",
                Key = Key.OemMinus,
                Modifiers = ModifierKeys.Control,
                Category = "View",
                Description = "Zoom out"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "CloseDocument",
                Key = Key.W,
                Modifiers = ModifierKeys.Control,
                Category = "File",
                Description = "Close current document"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "ExitApplication",
                Key = Key.F4,
                Modifiers = ModifierKeys.Alt,
                Category = "File",
                Description = "Exit application"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "FullScreen",
                Key = Key.F11,
                Modifiers = ModifierKeys.None,
                Category = "View",
                Description = "Toggle full screen"
            });

            RegisterShortcut(new KeyboardShortcut
            {
                Name = "Help",
                Key = Key.F1,
                Modifiers = ModifierKeys.None,
                Category = "Help",
                Description = "Open help"
            });
        }

        public bool RegisterShortcut(KeyboardShortcut shortcut)
        {
            if (shortcut == null)
                return false;

            var key = shortcut.GetKeyString();
            if (_shortcuts.ContainsKey(key))
            {
                AddConflict(key, shortcut.Name);
                return false;
            }

            _shortcuts[key] = shortcut;
            return true;
        }

        public void UnregisterShortcut(string name)
        {
            var shortcut = _shortcuts.Values.FirstOrDefault(s => s.Name == name);
            if (shortcut != null)
            {
                _shortcuts.Remove(shortcut.GetKeyString());
                OnShortcutUnregistered?.Invoke(this, new ShortcutEventArgs(shortcut));
            }
        }

        public KeyboardShortcut GetShortcut(string name)
        {
            return _shortcuts.Values.FirstOrDefault(s => s.Name == name);
        }

        public KeyboardShortcut GetShortcut(Key key, ModifierKeys modifiers)
        {
            var keyString = GetKeyString(key, modifiers);
            return _shortcuts.TryGetValue(keyString, out var shortcut) ? shortcut : null;
        }

        public IEnumerable<KeyboardShortcut> GetShortcuts(string category)
        {
            return _shortcuts.Values.Where(s => s.Category == category);
        }

        public IEnumerable<KeyboardShortcut> GetAllShortcuts()
        {
            return _shortcuts.Values.OrderBy(s => s.Category).ThenBy(s => s.Name);
        }

        private string GetKeyString(Key key, ModifierKeys modifiers)
        {
            var parts = new List<string>();
            if (modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
            if (modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
            if (modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
            if (modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
            parts.Add(key.ToString());
            return string.Join("+", parts);
        }

        private void AddConflict(string key, string name)
        {
            if (!_shortcutConflicts.ContainsKey(key))
                _shortcutConflicts[key] = new List<string>();
            _shortcutConflicts[key].Add(name);
        }

        public IEnumerable<string> GetConflicts(string name)
        {
            var shortcut = GetShortcut(name);
            if (shortcut == null) return Enumerable.Empty<string>();

            var keyString = shortcut.GetKeyString();
            return _shortcutConflicts.TryGetValue(keyString, out var conflicts) ? conflicts : Enumerable.Empty<string>();
        }

        public void UpdateShortcut(string name, Key newKey, ModifierKeys newModifiers)
        {
            var shortcut = GetShortcut(name);
            if (shortcut == null) return;

            _shortcuts.Remove(shortcut.GetKeyString());
            shortcut.Key = newKey;
            shortcut.Modifiers = newModifiers;
            _shortcuts[shortcut.GetKeyString()] = shortcut;

            OnShortcutUpdated?.Invoke(this, new ShortcutEventArgs(shortcut));
        }

        public event EventHandler<ShortcutEventArgs> OnShortcutRegistered;
        public event EventHandler<ShortcutEventArgs> OnShortcutUnregistered;
        public event EventHandler<ShortcutEventArgs> OnShortcutUpdated;
    }

    public class KeyboardShortcut : INotifyPropertyChanged
    {
        private Key _key;
        private ModifierKeys _modifiers;

        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public ICommand Command { get; set; }
        public object CommandParameter { get; set; }

        public Key Key
        {
            get => _key;
            set { _key = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayString)); }
        }

        public ModifierKeys Modifiers
        {
            get => _modifiers;
            set { _modifiers = value; OnPropertyChanged(); OnPropertyChanged(nameof(DisplayString)); }
        }

        public string DisplayString
        {
            get
            {
                var parts = new List<string>();
                if (Modifiers.HasFlag(ModifierKeys.Control)) parts.Add("Ctrl");
                if (Modifiers.HasFlag(ModifierKeys.Shift)) parts.Add("Shift");
                if (Modifiers.HasFlag(ModifierKeys.Alt)) parts.Add("Alt");
                if (Modifiers.HasFlag(ModifierKeys.Windows)) parts.Add("Win");
                parts.Add(GetKeyString(Key));
                return string.Join("+", parts);
            }
        }

        public bool IsModified { get; set; }

        public string GetKeyString()
        {
            return DisplayString;
        }

        private static string GetKeyString(Key key)
        {
            return key switch
            {
                Key.OemPlus => "+",
                Key.OemMinus => "-",
                Key.OemPeriod => ".",
                Key.OemComma => ",",
                Key.OemQuestion => "/",
                Key.OemTilde => "`",
                Key.OemOpenBrackets => "[",
                Key.Oem6 => "]",
                Key.Oem5 => "\\",
                Key.Space => "Space",
                Key.Return => "Enter",
                Key.Escape => "Esc",
                _ => key.ToString()
            };
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ShortcutEventArgs : EventArgs
    {
        public KeyboardShortcut Shortcut { get; }

        public ShortcutEventArgs(KeyboardShortcut shortcut)
        {
            Shortcut = shortcut;
        }
    }
}

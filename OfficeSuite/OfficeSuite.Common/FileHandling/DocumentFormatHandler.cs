using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OfficeSuite.Common.FileHandling
{
    public class DocumentFormatHandler
    {
        private readonly Dictionary<string, IFormatHandler> _handlers;
        private readonly Dictionary<string, IFormatConverter> _converters;

        public DocumentFormatHandler()
        {
            _handlers = new Dictionary<string, IFormatHandler>();
            _converters = new Dictionary<string, IFormatConverter>();
            InitializeDefaultHandlers();
            InitializeDefaultConverters();
        }

        private void InitializeDefaultHandlers()
        {
            RegisterHandler(".docx", new WordDocumentHandler());
            RegisterHandler(".doc", new WordDocumentHandler());
            RegisterHandler(".xlsx", new ExcelDocumentHandler());
            RegisterHandler(".xls", new ExcelDocumentHandler());
            RegisterHandler(".pptx", new PowerPointDocumentHandler());
            RegisterHandler(".ppt", new PowerPointDocumentHandler());
            RegisterHandler(".txt", new TextDocumentHandler());
            RegisterHandler(".rtf", new RtfDocumentHandler());
            RegisterHandler(".pdf", new PdfDocumentHandler());
            RegisterHandler(".html", new HtmlDocumentHandler());
            RegisterHandler(".xml", new XmlDocumentHandler());
            RegisterHandler(".json", new JsonDocumentHandler());
            RegisterHandler(".csv", new CsvDocumentHandler());
        }

        private void InitializeDefaultConverters()
        {
            RegisterConverter("docx", "pdf", new WordToPdfConverter());
            RegisterConverter("docx", "html", new WordToHtmlConverter());
            RegisterConverter("docx", "txt", new WordToTextConverter());
            RegisterConverter("xlsx", "csv", new ExcelToCsvConverter());
            RegisterConverter("xlsx", "pdf", new ExcelToPdfConverter());
            RegisterConverter("pptx", "pdf", new PowerPointToPdfConverter());
            RegisterConverter("pptx", "png", new PowerPointToImageConverter());
            RegisterConverter("pdf", "txt", new PdfToTextConverter());
            RegisterConverter("html", "pdf", new HtmlToPdfConverter());
        }

        public void RegisterHandler(string extension, IFormatHandler handler)
        {
            _handlers[extension.ToLowerInvariant()] = handler;
        }

        public IFormatHandler GetHandler(string extension)
        {
            return _handlers.TryGetValue(extension.ToLowerInvariant(), out var handler) ? handler : null;
        }

        public void RegisterConverter(string sourceFormat, string targetFormat, IFormatConverter converter)
        {
            var key = $"{sourceFormat}:{targetFormat}";
            _converters[key] = converter;
        }

        public IFormatConverter GetConverter(string sourceFormat, string targetFormat)
        {
            var key = $"{sourceFormat}:{targetFormat}";
            return _converters.TryGetValue(key, out var converter) ? converter : null;
        }

        public bool CanHandle(string extension)
        {
            return _handlers.ContainsKey(extension.ToLowerInvariant());
        }

        public bool CanConvert(string sourceFormat, string targetFormat)
        {
            var key = $"{sourceFormat}:{targetFormat}";
            return _converters.ContainsKey(key);
        }

        public DocumentFile Open(string filePath)
        {
            var extension = Path.GetExtension(filePath);
            var handler = GetHandler(extension);

            if (handler == null)
                throw new NotSupportedException($"File format '{extension}' is not supported");

            return handler.Open(filePath);
        }

        public async Task<DocumentFile> OpenAsync(string filePath)
        {
            return await Task.Run(() => Open(filePath));
        }

        public void Save(DocumentFile file, string filePath)
        {
            var extension = Path.GetExtension(filePath);
            var handler = GetHandler(extension);

            if (handler == null)
                throw new NotSupportedException($"File format '{extension}' is not supported");

            handler.Save(file, filePath);
        }

        public async Task SaveAsync(DocumentFile file, string filePath)
        {
            await Task.Run(() => Save(file, filePath));
        }

        public DocumentFile Convert(DocumentFile file, string targetFormat)
        {
            var sourceFormat = file.Format;
            var converter = GetConverter(sourceFormat, targetFormat);

            if (converter == null)
                throw new NotSupportedException($"Conversion from '{sourceFormat}' to '{targetFormat}' is not supported");

            return converter.Convert(file);
        }

        public async Task<DocumentFile> ConvertAsync(DocumentFile file, string targetFormat)
        {
            return await Task.Run(() => Convert(file, targetFormat));
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options = null)
        {
            options = options ?? new ExportOptions();

            var extension = Path.GetExtension(filePath);
            var handler = GetHandler(extension);

            if (handler == null)
                throw new NotSupportedException($"Export format '{extension}' is not supported");

            handler.Export(file, filePath, options);
        }

        public DocumentFile Import(string filePath, ImportOptions options = null)
        {
            options = options ?? new ImportOptions();

            var extension = Path.GetExtension(filePath);
            var handler = GetHandler(extension);

            if (handler == null)
                throw new NotSupportedException($"Import format '{extension}' is not supported");

            return handler.Import(filePath, options);
        }

        public IEnumerable<string> GetSupportedFormats()
        {
            return _handlers.Keys;
        }

        public IEnumerable<string> GetSupportedImportFormats()
        {
            return _handlers.Keys;
        }

        public IEnumerable<string> GetSupportedExportFormats()
        {
            return _handlers.Keys;
        }

        public FileFormatInfo GetFormatInfo(string extension)
        {
            var handler = GetHandler(extension);
            if (handler == null)
                return null;

            return new FileFormatInfo
            {
                Extension = extension,
                Name = handler.FormatName,
                Description = handler.FormatDescription,
                MimeType = handler.MimeType,
                IsReadOnly = handler.IsReadOnly,
                SupportsCompression = handler.SupportsCompression,
                SupportsEncryption = handler.SupportsEncryption,
                MaxFileSize = handler.MaxFileSize
            };
        }
    }

    public interface IFormatHandler
    {
        string FormatName { get; }
        string FormatDescription { get; }
        string MimeType { get; }
        bool IsReadOnly { get; }
        bool SupportsCompression { get; }
        bool SupportsEncryption { get; }
        long MaxFileSize { get; }

        DocumentFile Open(string filePath);
        void Save(DocumentFile file, string filePath);
        void Export(DocumentFile file, string filePath, ExportOptions options);
        DocumentFile Import(string filePath, ImportOptions options);
    }

    public interface IFormatConverter
    {
        string SourceFormat { get; }
        string TargetFormat { get; }
        DocumentFile Convert(DocumentFile source);
    }

    public class DocumentFile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public string FilePath { get; set; }
        public byte[] Content { get; set; }
        public long Size { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public DateTime AccessedDate { get; set; }
        public DocumentMetadata Metadata { get; set; }
        public DocumentPermissions Permissions { get; set; }
        public Dictionary<string, string> Properties { get; set; }

        public DocumentFile()
        {
            Id = Guid.NewGuid().ToString();
            Metadata = new DocumentMetadata();
            Permissions = new DocumentPermissions();
            Properties = new Dictionary<string, string>();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            AccessedDate = DateTime.Now;
        }

        public DocumentFile Clone()
        {
            return new DocumentFile
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Format = Format,
                FilePath = FilePath,
                Content = Content?.Clone() as byte[],
                Size = Size,
                CreatedDate = CreatedDate,
                ModifiedDate = DateTime.Now,
                AccessedDate = AccessedDate,
                Metadata = Metadata?.Clone(),
                Permissions = Permissions?.Clone(),
                Properties = new Dictionary<string, string>(Properties)
            };
        }
    }

    public class DocumentMetadata
    {
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Author { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Version { get; set; }
        public string ContentType { get; set; }
        public string Language { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime Printed { get; set; }
        public string LastModifiedBy { get; set; }
        public int Revision { get; set; }

        public DocumentMetadata Clone()
        {
            return new DocumentMetadata
            {
                Title = Title,
                Subject = Subject,
                Author = Author,
                Keywords = Keywords,
                Description = Description,
                Category = Category,
                Version = Version,
                ContentType = ContentType,
                Language = Language,
                Created = Created,
                Modified = Modified,
                Printed = Printed,
                LastModifiedBy = LastModifiedBy,
                Revision = Revision
            };
        }
    }

    public class DocumentPermissions
    {
        public bool CanRead { get; set; }
        public bool CanWrite { get; set; }
        public bool CanDelete { get; set; }
        public bool CanCopy { get; set; }
        public bool CanPrint { get; set; }
        public bool CanEdit { get; set; }
        public bool CanAnnotate { get; set; }
        public bool CanFillForms { get; set; }
        public bool CanExtract { get; set; }
        public string Owner { get; set; }
        public DateTime? ExpiresDate { get; set; }

        public DocumentPermissions Clone()
        {
            return new DocumentPermissions
            {
                CanRead = CanRead,
                CanWrite = CanWrite,
                CanDelete = CanDelete,
                CanCopy = CanCopy,
                CanPrint = CanPrint,
                CanEdit = CanEdit,
                CanAnnotate = CanAnnotate,
                CanFillForms = CanFillForms,
                CanExtract = CanExtract,
                Owner = Owner,
                ExpiresDate = ExpiresDate
            };
        }
    }

    public class ExportOptions
    {
        public bool IncludeMetadata { get; set; }
        public bool IncludeThumbnails { get; set; }
        public bool EmbedFonts { get; set; }
        public bool OptimizeForPrinting { get; set; }
        public bool OptimizeForScreen { get; set; }
        public bool ExportComments { get; set; }
        public bool ExportTrackChanges { get; set; }
        public bool IncludeHiddenContent { get; set; }
        public int QualityLevel { get; set; }
        public string Password { get; set; }
        public Dictionary<string, object> CustomOptions { get; set; }

        public ExportOptions()
        {
            IncludeMetadata = true;
            IncludeThumbnails = true;
            EmbedFonts = true;
            OptimizeForScreen = true;
            ExportComments = true;
            ExportTrackChanges = false;
            IncludeHiddenContent = false;
            QualityLevel = 100;
            CustomOptions = new Dictionary<string, object>();
        }
    }

    public class ImportOptions
    {
        public bool PreserveFormatting { get; set; }
        public bool PreserveImages { get; set; }
        public bool PreserveLinks { get; set; }
        public bool ImportMetadata { get; set; }
        public bool DetectEncoding { get; set; }
        public string PreferredEncoding { get; set; }
        public bool SkipHiddenContent { get; set; }
        public bool RepairCorrupted { get; set; }
        public Dictionary<string, object> CustomOptions { get; set; }

        public ImportOptions()
        {
            PreserveFormatting = true;
            PreserveImages = true;
            PreserveLinks = true;
            ImportMetadata = true;
            DetectEncoding = true;
            SkipHiddenContent = false;
            RepairCorrupted = true;
            CustomOptions = new Dictionary<string, object>();
        }
    }

    public class FileFormatInfo
    {
        public string Extension { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string MimeType { get; set; }
        public bool IsReadOnly { get; set; }
        public bool SupportsCompression { get; set; }
        public bool SupportsEncryption { get; set; }
        public long MaxFileSize { get; set; }
    }

    public class WordDocumentHandler : IFormatHandler
    {
        public string FormatName => "Microsoft Word Document";
        public string FormatDescription => "Word processing document";
        public string MimeType => "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
        public bool IsReadOnly => false;
        public bool SupportsCompression => true;
        public bool SupportsEncryption => true;
        public long MaxFileSize => 512 * 1024 * 1024;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            var file = new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };

            return file;
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class ExcelDocumentHandler : IFormatHandler
    {
        public string FormatName => "Microsoft Excel Document";
        public string FormatDescription => "Spreadsheet document";
        public string MimeType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public bool IsReadOnly => false;
        public bool SupportsCompression => true;
        public bool SupportsEncryption => true;
        public long MaxFileSize => 100 * 1024 * 1024;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class PowerPointDocumentHandler : IFormatHandler
    {
        public string FormatName => "Microsoft PowerPoint Document";
        public string FormatDescription => "Presentation document";
        public string MimeType => "application/vnd.openxmlformats-officedocument.presentationml.presentation";
        public bool IsReadOnly => false;
        public bool SupportsCompression => true;
        public bool SupportsEncryption => true;
        public long MaxFileSize => 100 * 1024 * 1024;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class TextDocumentHandler : IFormatHandler
    {
        public string FormatName => "Text Document";
        public string FormatDescription => "Plain text document";
        public string MimeType => "text/plain";
        public bool IsReadOnly => false;
        public bool SupportsCompression => false;
        public bool SupportsEncryption => false;
        public long MaxFileSize => long.MaxValue;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class RtfDocumentHandler : IFormatHandler
    {
        public string FormatName => "Rich Text Format";
        public string FormatDescription => "Rich text document";
        public string MimeType => "application/rtf";
        public bool IsReadOnly => false;
        public bool SupportsCompression => false;
        public bool SupportsEncryption => false;
        public long MaxFileSize => long.MaxValue;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class PdfDocumentHandler : IFormatHandler
    {
        public string FormatName => "PDF Document";
        public string FormatDescription => "Portable Document Format";
        public string MimeType => "application/pdf";
        public bool IsReadOnly => true;
        public bool SupportsCompression => true;
        public bool SupportsEncryption => true;
        public long MaxFileSize => long.MaxValue;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            throw new NotSupportedException("PDF files are read-only");
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            throw new NotSupportedException("PDF files are read-only");
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class HtmlDocumentHandler : IFormatHandler
    {
        public string FormatName => "HTML Document";
        public string FormatDescription => "Hypertext Markup Language";
        public string MimeType => "text/html";
        public bool IsReadOnly => false;
        public bool SupportsCompression => true;
        public bool SupportsEncryption => false;
        public long MaxFileSize => long.MaxValue;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class XmlDocumentHandler : IFormatHandler
    {
        public string FormatName => "XML Document";
        public string FormatDescription => "Extensible Markup Language";
        public string MimeType => "application/xml";
        public bool IsReadOnly => false;
        public bool SupportsCompression => true;
        public bool SupportsEncryption => false;
        public long MaxFileSize => long.MaxValue;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class JsonDocumentHandler : IFormatHandler
    {
        public string FormatName => "JSON Document";
        public string FormatDescription => "JavaScript Object Notation";
        public string MimeType => "application/json";
        public bool IsReadOnly => false;
        public bool SupportsCompression => true;
        public bool SupportsEncryption => false;
        public long MaxFileSize => long.MaxValue;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class CsvDocumentHandler : IFormatHandler
    {
        public string FormatName => "CSV Document";
        public string FormatDescription => "Comma Separated Values";
        public string MimeType => "text/csv";
        public bool IsReadOnly => false;
        public bool SupportsCompression => true;
        public bool SupportsEncryption => false;
        public long MaxFileSize => long.MaxValue;

        public DocumentFile Open(string filePath)
        {
            var content = File.ReadAllBytes(filePath);
            return new DocumentFile
            {
                Name = Path.GetFileNameWithoutExtension(filePath),
                Format = Path.GetExtension(filePath),
                FilePath = filePath,
                Content = content,
                Size = content.Length,
                ModifiedDate = File.GetLastWriteTime(filePath)
            };
        }

        public void Save(DocumentFile file, string filePath)
        {
            File.WriteAllBytes(filePath, file.Content);
            file.FilePath = filePath;
            file.ModifiedDate = DateTime.Now;
        }

        public void Export(DocumentFile file, string filePath, ExportOptions options)
        {
            Save(file, filePath);
        }

        public DocumentFile Import(string filePath, ImportOptions options)
        {
            return Open(filePath);
        }
    }

    public class WordToPdfConverter : IFormatConverter
    {
        public string SourceFormat => "docx";
        public string TargetFormat => "pdf";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class WordToHtmlConverter : IFormatConverter
    {
        public string SourceFormat => "docx";
        public string TargetFormat => "html";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class WordToTextConverter : IFormatConverter
    {
        public string SourceFormat => "docx";
        public string TargetFormat => "txt";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class ExcelToCsvConverter : IFormatConverter
    {
        public string SourceFormat => "xlsx";
        public string TargetFormat => "csv";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class ExcelToPdfConverter : IFormatConverter
    {
        public string SourceFormat => "xlsx";
        public string TargetFormat => "pdf";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class PowerPointToPdfConverter : IFormatConverter
    {
        public string SourceFormat => "pptx";
        public string TargetFormat => "pdf";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class PowerPointToImageConverter : IFormatConverter
    {
        public string SourceFormat => "pptx";
        public string TargetFormat => "png";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class PdfToTextConverter : IFormatConverter
    {
        public string SourceFormat => "pdf";
        public string TargetFormat => "txt";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class HtmlToPdfConverter : IFormatConverter
    {
        public string SourceFormat => "html";
        public string TargetFormat => "pdf";

        public DocumentFile Convert(DocumentFile source)
        {
            return source.Clone();
        }
    }

    public class DocumentTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string FilePath { get; set; }
        public DocumentFile TemplateFile { get; set; }
        public bool IsBuiltIn { get; set; }
        public DateTime CreatedDate { get; set; }
        public int UsageCount { get; set; }
        public List<string> Tags { get; set; }
        public Dictionary<string, object> Properties { get; set; }

        public DocumentTemplate()
        {
            Id = Guid.NewGuid().ToString();
            Tags = new List<string>();
            Properties = new Dictionary<string, object>();
            CreatedDate = DateTime.Now;
            IsBuiltIn = false;
        }

        public DocumentFile CreateFromTemplate(Dictionary<string, object> placeholders = null)
        {
            if (TemplateFile == null && !string.IsNullOrEmpty(FilePath))
            {
                var handler = new DocumentFormatHandler();
                TemplateFile = handler.Open(FilePath);
            }

            var document = TemplateFile?.Clone();
            if (document != null)
            {
                document.Id = Guid.NewGuid().ToString();
                document.Name = $"{document.Name} (Copy)";
            }

            return document;
        }
    }

    public class TemplateManager
    {
        private Dictionary<string, DocumentTemplate> _templates;
        private List<TemplateCategory> _categories;

        public TemplateManager()
        {
            _templates = new Dictionary<string, DocumentTemplate>();
            _categories = new List<TemplateCategory>();
            InitializeBuiltInTemplates();
        }

        private void InitializeBuiltInTemplates()
        {
            _categories.Add(new TemplateCategory { Name = "Word", Icon = "Word" });
            _categories.Add(new TemplateCategory { Name = "Excel", Icon = "Excel" });
            _categories.Add(new TemplateCategory { Name = "PowerPoint", Icon = "PowerPoint" });

            AddTemplate(new DocumentTemplate
            {
                Name = "Blank Document",
                Category = "Word",
                IsBuiltIn = true
            });

            AddTemplate(new DocumentTemplate
            {
                Name = "Blank Workbook",
                Category = "Excel",
                IsBuiltIn = true
            });

            AddTemplate(new DocumentTemplate
            {
                Name = "Blank Presentation",
                Category = "PowerPoint",
                IsBuiltIn = true
            });
        }

        public void AddTemplate(DocumentTemplate template)
        {
            _templates[template.Id] = template;
        }

        public void RemoveTemplate(string templateId)
        {
            _templates.Remove(templateId);
        }

        public DocumentTemplate GetTemplate(string templateId)
        {
            return _templates.TryGetValue(templateId, out var template) ? template : null;
        }

        public IEnumerable<DocumentTemplate> GetAllTemplates()
        {
            return _templates.Values;
        }

        public IEnumerable<DocumentTemplate> GetTemplatesByCategory(string category)
        {
            return _templates.Values.Where(t => t.Category == category);
        }

        public IEnumerable<DocumentTemplate> SearchTemplates(string query)
        {
            var lowerQuery = query.ToLowerInvariant();
            return _templates.Values.Where(t =>
                t.Name.ToLowerInvariant().Contains(lowerQuery) ||
                t.Description.ToLowerInvariant().Contains(lowerQuery) ||
                t.Tags.Any(tag => tag.ToLowerInvariant().Contains(lowerQuery)));
        }

        public IEnumerable<TemplateCategory> GetCategories()
        {
            return _categories;
        }
    }

    public class TemplateCategory
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string ThumbnailPath { get; set; }
        public int TemplateCount { get; set; }

        public TemplateCategory()
        {
            Id = Guid.NewGuid().ToString();
        }
    }

    public class DocumentRecovery
    {
        private readonly string _recoveryPath;

        public DocumentRecovery(string recoveryPath)
        {
            _recoveryPath = recoveryPath;
            if (!Directory.Exists(_recoveryPath))
                Directory.CreateDirectory(_recoveryPath);
        }

        public void SaveRecoveryPoint(DocumentFile file)
        {
            var fileName = $"{file.Id}_{DateTime.Now:yyyyMMddHHmmss}.recovery";
            var filePath = Path.Combine(_recoveryPath, fileName);

            var recoveryData = new DocumentRecoveryData
            {
                Id = file.Id,
                Name = file.Name,
                Format = file.Format,
                Content = file.Content,
                Metadata = file.Metadata,
                CreatedDate = DateTime.Now,
                Version = 1
            };

            var json = JsonSerializer.Serialize(recoveryData);
            File.WriteAllText(filePath, json);
        }

        public DocumentRecoveryData LoadRecoveryPoint(string recoveryId)
        {
            var files = Directory.GetFiles(_recoveryPath, $"{recoveryId}*.recovery");
            if (files.Length == 0)
                return null;

            var latestFile = files.OrderByDescending(f => File.GetLastWriteTime(f)).First();
            var json = File.ReadAllText(latestFile);
            return JsonSerializer.Deserialize<DocumentRecoveryData>(json);
        }

        public IEnumerable<DocumentRecoveryData> GetRecoveryPoints(string documentId)
        {
            var files = Directory.GetFiles(_recoveryPath, $"{documentId}*.recovery");
            var recoveryPoints = new List<DocumentRecoveryData>();

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var data = JsonSerializer.Deserialize<DocumentRecoveryData>(json);
                recoveryPoints.Add(data);
            }

            return recoveryPoints.OrderByDescending(r => r.CreatedDate);
        }

        public void DeleteRecoveryPoint(string recoveryId)
        {
            var files = Directory.GetFiles(_recoveryPath, $"{recoveryId}*.recovery");
            foreach (var file in files)
            {
                File.Delete(file);
            }
        }

        public void CleanupOldRecoveryPoints(int maxAgeInDays)
        {
            var cutoffDate = DateTime.Now.AddDays(-maxAgeInDays);
            var files = Directory.GetFiles(_recoveryPath, "*.recovery");

            foreach (var file in files)
            {
                if (File.GetCreationTime(file) < cutoffDate)
                {
                    File.Delete(file);
                }
            }
        }
    }

    public class DocumentRecoveryData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Format { get; set; }
        public byte[] Content { get; set; }
        public DocumentMetadata Metadata { get; set; }
        public DateTime CreatedDate { get; set; }
        public int Version { get; set; }
    }

    public class DocumentWatcher
    {
        private FileSystemWatcher _watcher;
        private readonly string _directory;
        private readonly string _filter;

        public event EventHandler<DocumentChangedEventArgs> DocumentChanged;
        public event EventHandler<DocumentCreatedEventArgs> DocumentCreated;
        public event EventHandler<DocumentDeletedEventArgs> DocumentDeleted;
        public event EventHandler<DocumentRenamedEventArgs> DocumentRenamed;

        public DocumentWatcher(string directory, string filter = "*.*")
        {
            _directory = directory;
            _filter = filter;
        }

        public void StartWatching()
        {
            _watcher = new FileSystemWatcher(_directory, _filter)
            {
                NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.Size,
                EnableRaisingEvents = true
            };

            _watcher.Changed += OnChanged;
            _watcher.Created += OnCreated;
            _watcher.Deleted += OnDeleted;
            _watcher.Renamed += OnRenamed;
        }

        public void StopWatching()
        {
            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnChanged;
                _watcher.Created -= OnCreated;
                _watcher.Deleted -= OnDeleted;
                _watcher.Renamed -= OnRenamed;
                _watcher.Dispose();
                _watcher = null;
            }
        }

        private void OnChanged(object sender, FileSystemEventArgs e)
        {
            DocumentChanged?.Invoke(this, new DocumentChangedEventArgs(e.FullPath));
        }

        private void OnCreated(object sender, FileSystemEventArgs e)
        {
            DocumentCreated?.Invoke(this, new DocumentCreatedEventArgs(e.FullPath));
        }

        private void OnDeleted(object sender, FileSystemEventArgs e)
        {
            DocumentDeleted?.Invoke(this, new DocumentDeletedEventArgs(e.FullPath));
        }

        private void OnRenamed(object sender, RenamedEventArgs e)
        {
            DocumentRenamed?.Invoke(this, new DocumentRenamedEventArgs(e.OldFullPath, e.FullPath));
        }
    }

    public class DocumentChangedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public DocumentChangedEventArgs(string filePath) => FilePath = filePath;
    }

    public class DocumentCreatedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public DocumentCreatedEventArgs(string filePath) => FilePath = filePath;
    }

    public class DocumentDeletedEventArgs : EventArgs
    {
        public string FilePath { get; }
        public DocumentDeletedEventArgs(string filePath) => FilePath = filePath;
    }

    public class DocumentRenamedEventArgs : EventArgs
    {
        public string OldFilePath { get; }
        public string NewFilePath { get; }
        public DocumentRenamedEventArgs(string oldFilePath, string newFilePath)
        {
            OldFilePath = oldFilePath;
            NewFilePath = newFilePath;
        }
    }
}

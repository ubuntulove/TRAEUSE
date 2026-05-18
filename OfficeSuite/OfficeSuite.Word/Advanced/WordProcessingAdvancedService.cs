using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Security.Cryptography;
using OfficeSuite.Word.Models;

namespace OfficeSuite.Word.Advanced
{
    public class WordProcessingAdvancedService
    {
        private readonly Dictionary<string, List<StyleDefinition>> _styleCache;
        private readonly List<DocumentTemplate> _templates;
        private readonly MacroSecurityManager _macroSecurity;
        private readonly DocumentSigner _documentSigner;

        public WordProcessingAdvancedService()
        {
            _styleCache = new Dictionary<string, List<StyleDefinition>>();
            _templates = new List<DocumentTemplate>();
            _macroSecurity = new MacroSecurityManager();
            _documentSigner = new DocumentSigner();
        }

        public void RegisterStyle(string documentId, List<StyleDefinition> styles)
        {
            _styleCache[documentId] = styles;
        }

        public List<StyleDefinition> GetStyles(string documentId)
        {
            return _styleCache.TryGetValue(documentId, out var styles) ? styles : new List<StyleDefinition>();
        }

        public StyleDefinition FindStyleByName(string documentId, string styleName)
        {
            var styles = GetStyles(documentId);
            return styles.FirstOrDefault(s => s.Name.Equals(styleName, StringComparison.OrdinalIgnoreCase));
        }

        public StyleDefinition FindStyleById(string documentId, string styleId)
        {
            var styles = GetStyles(documentId);
            return styles.FirstOrDefault(s => s.StyleId.Equals(styleId, StringComparison.OrdinalIgnoreCase));
        }

        public List<StyleDefinition> FindStylesByType(string documentId, StyleType type)
        {
            var styles = GetStyles(documentId);
            return styles.Where(s => s.Type == type).ToList();
        }

        public bool ApplyStyle(string documentId, string paragraphId, string styleId)
        {
            var style = FindStyleById(documentId, styleId);
            if (style == null) return false;
            return true;
        }

        public StyleDefinition CreateStyle(string name, StyleType type)
        {
            return new StyleDefinition
            {
                StyleId = GenerateStyleId(name),
                Name = name,
                Type = type,
                BasedOn = "Normal",
                NextParagraphStyle = "Normal",
                IsQuickStyle = true,
                IsDefault = false
            };
        }

        private string GenerateStyleId(string name)
        {
            var id = name.ToLowerInvariant().Replace(" ", "_");
            id = Regex.Replace(id, @"[^a-z0-9_]", "");
            return id;
        }

        public DocumentTemplate CreateTemplate(string name)
        {
            var template = new DocumentTemplate
            {
                TemplateId = Guid.NewGuid().ToString(),
                Name = name,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Version = "1.0"
            };
            _templates.Add(template);
            return template;
        }

        public DocumentTemplate FindTemplate(string name)
        {
            return _templates.FirstOrDefault(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public List<DocumentTemplate> GetRecentTemplates(int count = 10)
        {
            return _templates.OrderByDescending(t => t.ModifiedDate).Take(count).ToList();
        }

        public bool SaveAsTemplate(string documentId, string templatePath)
        {
            return true;
        }

        public bool LoadFromTemplate(string templateId, string documentId)
        {
            var template = _templates.FirstOrDefault(t => t.TemplateId == templateId);
            return template != null;
        }

        public MacroSecurityLevel GetMacroSecurityLevel()
        {
            return _macroSecurity.SecurityLevel;
        }

        public void SetMacroSecurityLevel(MacroSecurityLevel level)
        {
            _macroSecurity.SecurityLevel = level;
        }

        public bool SignDocument(string documentPath, string certificatePath, string password)
        {
            return _documentSigner.SignDocument(documentPath, certificatePath, password);
        }

        public bool VerifySignature(string documentPath)
        {
            return _documentSigner.VerifySignature(documentPath);
        }

        public List<DigitalSignature> GetSignatures(string documentPath)
        {
            return _documentSigner.GetSignatures(documentPath);
        }

        public bool RemoveSignature(string documentPath, string signatureId)
        {
            return _documentSigner.RemoveSignature(documentPath, signatureId);
        }

        public List<ContentControl> ExtractContentControls(string documentPath)
        {
            var controls = new List<ContentControl>();
            return controls;
        }

        public bool UpdateContentControl(string documentPath, string controlId, object newValue)
        {
            return true;
        }

        public Dictionary<string, object> GetDocumentStatistics(string documentPath)
        {
            var stats = new Dictionary<string, object>
            {
                { "CharacterCount", 0 },
                { "CharacterCountWithSpaces", 0 },
                { "WordCount", 0 },
                { "ParagraphCount", 0 },
                { "LineCount", 0 },
                { "PageCount", 0 },
                { "UniqueWords", 0 },
                { "AverageWordsPerParagraph", 0.0 },
                { "AverageCharactersPerWord", 0.0 }
            };
            return stats;
        }

        public List<string> ExtractMetadata(string documentPath)
        {
            var metadata = new List<string>();
            return metadata;
        }

        public bool SetDocumentProperty(string documentPath, string propertyName, object value)
        {
            return true;
        }

        public object GetDocumentProperty(string documentPath, string propertyName)
        {
            return null;
        }

        public bool ProtectDocument(string documentPath, ProtectionType type, string password)
        {
            return true;
        }

        public bool UnprotectDocument(string documentPath, string password)
        {
            return true;
        }

        public ProtectionType GetProtectionType(string documentPath)
        {
            return ProtectionType.None;
        }

        public List<string> GetProtectedRanges(string documentPath)
        {
            return new List<string>();
        }

        public bool ExportToPdf(string documentPath, string outputPath)
        {
            return true;
        }

        public bool ExportToHtml(string documentPath, string outputPath, HtmlExportOptions options)
        {
            return true;
        }

        public bool ExportToRtf(string documentPath, string outputPath)
        {
            return true;
        }

        public bool ExportToText(string documentPath, string outputPath, TextExportOptions options)
        {
            return true;
        }

        public bool ExportToXml(string documentPath, string outputPath)
        {
            return true;
        }

        public bool ImportFromHtml(string htmlPath, string documentPath)
        {
            return true;
        }

        public bool ImportFromRtf(string rtfPath, string documentPath)
        {
            return true;
        }

        public bool ImportFromText(string textPath, string documentPath, Encoding encoding)
        {
            return true;
        }

        public bool ImportFromXml(string xmlPath, string documentPath)
        {
            return true;
        }

        public List<Comment> GetComments(string documentPath)
        {
            return new List<Comment>();
        }

        public bool AddComment(string documentPath, string author, string text, int startPosition, int endPosition)
        {
            return true;
        }

        public bool RemoveComment(string documentPath, string commentId)
        {
            return true;
        }

        public bool ReplyToComment(string documentPath, string parentCommentId, string author, string text)
        {
            return true;
        }

        public List<Revision> GetRevisions(string documentPath)
        {
            return new List<Revision>();
        }

        public bool AcceptRevision(string documentPath, string revisionId)
        {
            return true;
        }

        public bool RejectRevision(string documentPath, string revisionId)
        {
            return true;
        }

        public bool AcceptAllRevisions(string documentPath)
        {
            return true;
        }

        public bool RejectAllRevisions(string documentPath)
        {
            return true;
        }

        public List<Bookmark> GetBookmarks(string documentPath)
        {
            return new List<Bookmark>();
        }

        public bool AddBookmark(string documentPath, string name, int startPosition, int endPosition)
        {
            return true;
        }

        public bool RemoveBookmark(string documentPath, string name)
        {
            return true;
        }

        public bool GoToBookmark(string documentPath, string name)
        {
            return true;
        }

        public List<Hyperlink> GetHyperlinks(string documentPath)
        {
            return new List<Hyperlink>();
        }

        public bool InsertHyperlink(string documentPath, string url, string displayText, int position)
        {
            return true;
        }

        public bool UpdateHyperlink(string documentPath, string hyperlinkId, string newUrl)
        {
            return true;
        }

        public List<TableOfContentsEntry> GenerateTableOfContents(string documentPath, TocOptions options)
        {
            return new List<TableOfContentsEntry>();
        }

        public bool UpdateTableOfContents(string documentPath, string tocId)
        {
            return true;
        }

        public List<IndexEntry> GenerateIndex(string documentPath, IndexOptions options)
        {
            return new List<IndexEntry>();
        }

        public List<TableOfFiguresEntry> GenerateTableOfFigures(string documentPath, string captionLabel)
        {
            return new List<TableOfFiguresEntry>();
        }

        public bool InsertField(string documentPath, string fieldCode, int position)
        {
            return true;
        }

        public bool UpdateField(string documentPath, string fieldId)
        {
            return true;
        }

        public bool UpdateAllFields(string documentPath)
        {
            return true;
        }

        public List<SectionBreak> GetSectionBreaks(string documentPath)
        {
            return new List<SectionBreak>();
        }

        public bool InsertSectionBreak(string documentPath, SectionBreakType type, int position)
        {
            return true;
        }

        public SectionProperties GetSectionProperties(string documentPath, int sectionIndex)
        {
            return new SectionProperties();
        }

        public bool SetSectionProperties(string documentPath, int sectionIndex, SectionProperties properties)
        {
            return true;
        }

        public List<HeaderReference> GetHeaders(string documentPath)
        {
            return new List<HeaderReference>();
        }

        public List<FooterReference> GetFooters(string documentPath)
        {
            return new List<FooterReference>();
        }

        public bool SetHeader(string documentPath, int sectionIndex, HeaderFooterType type, string content)
        {
            return true;
        }

        public bool SetFooter(string documentPath, int sectionIndex, HeaderFooterType type, string content)
        {
            return true;
        }

        public List<MailMergeField> GetMailMergeFields(string documentPath)
        {
            return new List<MailMergeField>();
        }

        public bool ConnectToDataSource(string documentPath, string dataSourcePath, DataSourceType sourceType)
        {
            return true;
        }

        public bool DisconnectDataSource(string documentPath)
        {
            return true;
        }

        public bool ExecuteMailMerge(string documentPath, string outputPath, MailMergeOptions options)
        {
            return true;
        }

        public List<Macros> GetMacros(string documentPath)
        {
            return new List<Macros>();
        }

        public bool RemoveMacros(string documentPath)
        {
            return true;
        }

        public List<CustomProperty> GetCustomProperties(string documentPath)
        {
            return new List<CustomProperty>();
        }

        public bool SetCustomProperty(string documentPath, string name, object value)
        {
            return true;
        }

        public bool RemoveCustomProperty(string documentPath, string name)
        {
            return true;
        }

        public List<LanguageInfo> GetDocumentLanguages(string documentPath)
        {
            return new List<LanguageInfo>();
        }

        public bool SetDocumentLanguage(string documentPath, string languageCode)
        {
            return true;
        }

        public List<BuildingBlock> GetBuildingBlocks(string documentPath, string category)
        {
            return new List<BuildingBlock>();
        }

        public bool InsertBuildingBlock(string documentPath, string blockName, int position)
        {
            return true;
        }

        public List<SmartTag> GetSmartTags(string documentPath)
        {
            return new List<SmartTag>();
        }

        public bool RemoveSmartTags(string documentPath)
        {
            return true;
        }

        public Dictionary<string, List<string>> AnalyzeDocumentStructure(string documentPath)
        {
            var structure = new Dictionary<string, List<string>>
            {
                { "Headings", new List<string>() },
                { "Paragraphs", new List<string>() },
                { "Tables", new List<string>() },
                { "Lists", new List<string>() },
                { "Images", new List<string>() }
            };
            return structure;
        }

        public bool OptimizeDocument(string documentPath, OptimizationOptions options)
        {
            return true;
        }

        public bool RepairDocument(string documentPath, string outputPath)
        {
            return true;
        }

        public List<CompatibilityIssue> CheckCompatibility(string documentPath, string targetVersion)
        {
            return new List<CompatibilityIssue>();
        }

        public bool ConvertDocument(string documentPath, string outputPath, DocumentFormat targetFormat)
        {
            return true;
        }

        public DocumentVersionInfo GetVersionInfo(string documentPath)
        {
            return new DocumentVersionInfo();
        }

        public bool CreateVersion(string documentPath, string comment)
        {
            return true;
        }

        public bool DeleteVersion(string documentPath, string versionId)
        {
            return true;
        }

        public List<Variable> GetVariables(string documentPath)
        {
            return new List<Variable>();
        }

        public bool SetVariable(string documentPath, string name, string value)
        {
            return true;
        }

        public bool RemoveVariable(string documentPath, string name)
        {
            return true;
        }
    }

    public class MacroSecurityManager
    {
        public MacroSecurityLevel SecurityLevel { get; set; } = MacroSecurityLevel.AllMacrosDisable;
        public bool IsVbaProjectSigned { get; set; }
        public List<string> TrustedLocations { get; set; } = new List<string>();
        public List<string> TrustedPublishers { get; set; } = new List<string>();

        public bool IsLocationTrusted(string path)
        {
            return TrustedLocations.Any(loc => path.StartsWith(loc, StringComparison.OrdinalIgnoreCase));
        }

        public bool IsPublisherTrusted(string publisher)
        {
            return TrustedPublishers.Contains(publisher);
        }

        public void AddTrustedLocation(string path)
        {
            if (!TrustedLocations.Contains(path))
                TrustedLocations.Add(path);
        }

        public void RemoveTrustedLocation(string path)
        {
            TrustedLocations.Remove(path);
        }
    }

    public class DocumentSigner
    {
        private readonly List<DigitalSignature> _signatures;

        public DocumentSigner()
        {
            _signatures = new List<DigitalSignature>();
        }

        public bool SignDocument(string documentPath, string certificatePath, string password)
        {
            var signature = new DigitalSignature
            {
                SignatureId = Guid.NewGuid().ToString(),
                CertificateSubject = certificatePath,
                SignedBy = Environment.UserName,
                SigningTime = DateTime.Now,
                IsValid = true
            };
            _signatures.Add(signature);
            return true;
        }

        public bool VerifySignature(string documentPath)
        {
            return _signatures.Any(s => s.IsValid);
        }

        public List<DigitalSignature> GetSignatures(string documentPath)
        {
            return _signatures.ToList();
        }

        public bool RemoveSignature(string documentPath, string signatureId)
        {
            var signature = _signatures.FirstOrDefault(s => s.SignatureId == signatureId);
            if (signature != null)
            {
                _signatures.Remove(signature);
                return true;
            }
            return false;
        }
    }

    public class DocumentTemplate
    {
        public string TemplateId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Version { get; set; }
        public string Author { get; set; }
        public List<string> Categories { get; set; } = new List<string>();
    }

    public class ContentControl
    {
        public string Id { get; set; }
        public string Tag { get; set; }
        public ContentControlType Type { get; set; }
        public string Title { get; set; }
        public string PlaceholderText { get; set; }
        public object Value { get; set; }
        public bool IsLocked { get; set; }
    }

    public enum ContentControlType
    {
        RichText,
        PlainText,
        CheckBox,
        DropDownList,
        ComboBox,
        DatePicker,
        Picture,
        BuildingBlockGallery,
        RepeatingSection,
        RepeatingSectionItem,
        CheckBoxOld
    }

    public class DigitalSignature
    {
        public string SignatureId { get; set; }
        public string CertificateSubject { get; set; }
        public string CertificateIssuer { get; set; }
        public string CertificateThumbprint { get; set; }
        public DateTime CertificateValidFrom { get; set; }
        public DateTime CertificateValidTo { get; set; }
        public string SignedBy { get; set; }
        public DateTime SigningTime { get; set; }
        public string SignatureComments { get; set; }
        public bool IsValid { get; set; }
        public string SignatureValue { get; set; }
    }

    public enum MacroSecurityLevel
    {
        DisableAllMacrosWithoutNotification,
        DisableAllMacrosWithNotification,
        DisableAllMacrosExceptDigitallySignedMacros,
        EnableAllMacros,
        NoSecurity
    }

    public class HtmlExportOptions
    {
        public bool IncludeStyles { get; set; } = true;
        public bool IncludeImages { get; set; } = true;
        public bool IncludeFonts { get; set; } = true;
        public bool UseEmbeddedCss { get; set; } = true;
        public bool ExportXmlData { get; set; } = false;
        public int ImageQuality { get; set; } = 85;
        public string CssPrefix { get; set; } = "docx";
    }

    public class TextExportOptions
    {
        public Encoding Encoding { get; set; } = Encoding.UTF8;
        public bool IncludeTableOfContents { get; set; } = false;
        public bool IncludeFootnotes { get; set; } = false;
        public bool IncludeEndnotes { get; set; } = false;
        public bool PreserveImages { get; set; } = false;
        public LineEnding LineEndingType { get; set; } = LineEnding.CRLF;
    }

    public enum LineEnding
    {
        CRLF,
        LF,
        CR
    }

    public class DocumentVersionInfo
    {
        public string CurrentVersion { get; set; }
        public DateTime LastModified { get; set; }
        public List<VersionHistoryEntry> History { get; set; } = new List<VersionHistoryEntry>();
    }

    public class VersionHistoryEntry
    {
        public string VersionId { get; set; }
        public DateTime CreatedDate { get; set; }
        public string Author { get; set; }
        public string Comment { get; set; }
        public string RevisionId { get; set; }
    }

    public class CompatibilityIssue
    {
        public string IssueId { get; set; }
        public string Description { get; set; }
        public CompatibilitySeverity Severity { get; set; }
        public string AffectedElement { get; set; }
        public string SuggestedFix { get; set; }
    }

    public enum CompatibilitySeverity
    {
        Information,
        Warning,
        Error
    }

    public class DocumentFormat
    {
        public static readonly DocumentFormat Docx = new DocumentFormat("docx", "Word Document");
        public static readonly DocumentFormat Doc = new DocumentFormat("doc", "Word 97-2003 Document");
        public static readonly DocumentFormat Pdf = new DocumentFormat("pdf", "PDF");
        public static readonly DocumentFormat Html = new DocumentFormat("html", "HTML");
        public static readonly DocumentFormat Rtf = new DocumentFormat("rtf", "Rich Text Format");
        public static readonly DocumentFormat Text = new DocumentFormat("txt", "Plain Text");
        public static readonly DocumentFormat Xml = new DocumentFormat("xml", "XML");

        public string Extension { get; set; }
        public string Description { get; set; }

        public DocumentFormat(string extension, string description)
        {
            Extension = extension;
            Description = description;
        }
    }

    public class OptimizationOptions
    {
        public bool RemoveMetadata { get; set; } = true;
        public bool CompressImages { get; set; } = true;
        public int ImageCompressionQuality { get; set; } = 80;
        public bool RemoveUnusedStyles { get; set; } = true;
        public bool RemoveEmptyParagraphs { get; set; } = false;
        public bool FlattenFields { get; set; } = false;
    }

    public class IndexOptions
    {
        public string IndexHeading { get; set; } = "Index";
        public int Columns { get; set; } = 2;
        public bool RunInOrder { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public string Separator { get; set; } = ", ";
        public string RangeSeparator { get; set; } = "-";
    }

    public class IndexEntry
    {
        public string PrimaryEntry { get; set; }
        public List<string> CrossReferences { get; set; } = new List<string>();
        public List<int> PageNumbers { get; set; } = new List<int>();
        public int Level { get; set; }
    }

    public class TableOfFiguresEntry
    {
        public string Caption { get; set; }
        public string Label { get; set; }
        public int PageNumber { get; set; }
        public int Level { get; set; }
    }

    public class BuildingBlock
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Gallery { get; set; }
        public string Description { get; set; }
        public string Content { get; set; }
    }

    public class SmartTag
    {
        public string Namespace { get; set; }
        public string Name { get; set; }
        public string Properties { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
    }

    public class LanguageInfo
    {
        public string LanguageCode { get; set; }
        public string LanguageName { get; set; }
        public int CharacterCount { get; set; }
        public int WordCount { get; set; }
    }

    public class Macros
    {
        public string ModuleName { get; set; }
        public string ProcedureName { get; set; }
        public string Code { get; set; }
        public bool IsDigitallySigned { get; set; }
    }

    public class CustomProperty
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
    }

    public class MailMergeField
    {
        public string FieldName { get; set; }
        public int StartPosition { get; set; }
        public int EndPosition { get; set; }
        public MailMergeFieldType Type { get; set; }
    }

    public enum MailMergeFieldType
    {
        Simple,
        Conditional,
        NextRecord,
        NextIf,
        SkipIf,
        MERGEFIELD,
        IF,
        SET,
        FILLIN,
        ASK
    }

    public class DataSourceType
    {
        public static readonly DataSourceType Access = new DataSourceType("Access Database", ".accdb", ".mdb");
        public static readonly DataSourceType Excel = new DataSourceType("Excel Workbook", ".xlsx", ".xls");
        public static readonly DataSourceType Word = new DataSourceType("Word Document", ".docx", ".doc");
        public static readonly DataSourceType Text = new DataSourceType("Text File", ".txt", ".csv");
        public static readonly DataSourceType Odbc = new DataSourceType("ODBC Data Source", "", "");

        public string Description { get; set; }
        public string[] Extensions { get; set; }

        public DataSourceType(string description, params string[] extensions)
        {
            Description = description;
            Extensions = extensions;
        }
    }

    public class MailMergeOptions
    {
        public bool MergeToNewDocument { get; set; } = true;
        public bool MergeToEmail { get; set; } = false;
        public bool MergeToPrinter { get; set; } = false;
        public string OutputPath { get; set; }
        public string EmailSubject { get; set; }
        public string EmailToField { get; set; }
        public List<string> Recipients { get; set; } = new List<string>();
        public int StartRecord { get; set; } = 1;
        public int EndRecord { get; set; } = -1;
    }

    public class Variable
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public DateTime ModifiedDate { get; set; }
    }

    public class SectionBreak
    {
        public int SectionIndex { get; set; }
        public SectionBreakType Type { get; set; }
        public SectionProperties Properties { get; set; }
    }

    public enum SectionBreakType
    {
        NextPage,
        Continuous,
        EvenPage,
        OddPage
    }

    public class SectionProperties
    {
        public PageSize PageSize { get; set; } = new PageSize();
        public PageMargin PageMargin { get; set; } = new PageMargin();
        public string PageOrientation { get; set; } = "portrait";
        public int SectionType { get; set; } = 1;
        public bool HasDifferentFirstPageHeaderFooter { get; set; } = false;
        public bool HasTitlePage { get; set; } = false;
        public LineNumberingOptions LineNumbering { get; set; }
        public PageNumberOptions PageNumbers { get; set; }
        public TextDirection TextDirection { get; set; } = TextDirection.LRTB;
    }

    public class PageSize
    {
        public double Width { get; set; } = 12240;
        public double Height { get; set; } = 15840;
        public string MeasureUnit { get; set; } = "twips";
    }

    public class PageMargin
    {
        public double Top { get; set; } = 1440;
        public double Bottom { get; set; } = 1440;
        public double Left { get; set; } = 1440;
        public double Right { get; set; } = 1440;
        public double Header { get; set; } = 720;
        public double Footer { get; set; } = 720;
    }

    public enum TextDirection
    {
        LRTB,
        TBRB,
        TBRL,
        TBLR,
        BTC,
        TBLR,
        LRTBV,
        TBRV
    }

    public class LineNumberingOptions
    {
        public bool IsEnabled { get; set; } = false;
        public LineNumberingRestart Restart { get; set; } = LineNumberingRestart.NewPage;
        public int StartNumber { get; set; } = 1;
        public int Distance { get; set; } = 360;
        public int CountBy { get; set; } = 1;
    }

    public enum LineNumberingRestart
    {
        Continuous,
        NewPage,
        NewSection
    }

    public class PageNumberOptions
    {
        public bool IsEnabled { get; set; } = false;
        public PageNumberFormat Format { get; set; } = PageNumberFormat.Decimal;
        public bool StartAtTitlePage { get; set; } = false;
        public int StartingNumber { get; set; } = 1;
    }

    public enum PageNumberFormat
    {
        Decimal,
        UpperRoman,
        LowerRoman,
        UpperLetter,
        LowerLetter,
        Ordinal,
        CardinalText,
        OrdinalText
    }

    public class HeaderReference
    {
        public int SectionIndex { get; set; }
        public HeaderFooterType Type { get; set; }
        public string Content { get; set; }
        public bool IsLinkedToPrevious { get; set; } = false;
    }

    public class FooterReference
    {
        public int SectionIndex { get; set; }
        public HeaderFooterType Type { get; set; }
        public string Content { get; set; }
        public bool IsLinkedToPrevious { get; set; } = false;
    }

    public enum HeaderFooterType
    {
        Primary,
        First,
        Even
    }

    public class TocOptions
    {
        public string Title { get; set; } = "Table of Contents";
        public int Levels { get; set; } = 3;
        public bool IncludePageNumbers { get; set; } = true;
        public bool UseOutlineLevels { get; set; } = true;
        public bool UseStyles { get; set; } = true;
        public bool RightAlignPageNumbers { get; set; } = true;
        public string TabLeader { get; set; } = "dot";
        public string RightTabPosition { get; set; } = "right";
    }

    public class TableOfContentsEntry
    {
        public string Text { get; set; }
        public int Level { get; set; }
        public int PageNumber { get; set; }
        public string BookmarkName { get; set; }
    }

    public class Revision
    {
        public string RevisionId { get; set; }
        public RevisionType Type { get; set; }
        public string Author { get; set; }
        public DateTime Date { get; set; }
        public string OldText { get; set; }
        public string NewText { get; set; }
        public int Position { get; set; }
    }

    public enum RevisionType
    {
        Insertion,
        Deletion,
        Formatting,
        StyleDefinition,
        Replacement,
        Moving
    }
}

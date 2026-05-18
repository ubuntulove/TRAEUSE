using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using OfficeSuite.Word.Models;
using OfficeSuite.Spreadsheet.Models;
using OfficeSuite.Presentation.Models;

namespace OfficeSuite.Common.Schema
{
    public class DocumentValidationService
    {
        private readonly List<ValidationRule> _rules;
        private readonly List<ValidationError> _errors;

        public DocumentValidationService()
        {
            _rules = new List<ValidationRule>();
            _errors = new List<ValidationError>();
            InitializeRules();
        }

        private void InitializeRules()
        {
            _rules.Add(new ValidationRule
            {
                RuleId = "DOC001",
                RuleName = "FileExtension",
                Description = "Validates that the file has a valid Office document extension",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.FileFormat
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC002",
                RuleName = "FileSignature",
                Description = "Validates the file signature (magic bytes) for Office Open XML files",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.FileFormat
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC003",
                RuleName = "RootElement",
                Description = "Validates the root XML element of the document",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.Structure
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC004",
                RuleName = "NamespaceDeclaration",
                Description = "Validates that required namespaces are declared",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.Structure
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC005",
                RuleName = "ContentTypes",
                Description = "Validates the [Content_Types].xml file",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.Structure
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC006",
                RuleName = "Relationships",
                Description = "Validates the document relationships",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.Structure
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC007",
                RuleName = "DocumentProperties",
                Description = "Validates document properties are present and valid",
                Severity = ValidationSeverity.Warning,
                Category = ValidationCategory.Metadata
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC008",
                RuleName = "SchemaCompliance",
                Description = "Validates XML schema compliance",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.Schema
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC009",
                RuleName = "CharacterEncoding",
                Description = "Validates character encoding is UTF-8",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.Content
            });

            _rules.Add(new ValidationRule
            {
                RuleId = "DOC010",
                RuleName = "XmlWellFormed",
                Description = "Validates that all XML is well-formed",
                Severity = ValidationSeverity.Error,
                Category = ValidationCategory.Xml
            });
        }

        public ValidationResult ValidateDocument(string filePath, DocumentType documentType)
        {
            var result = new ValidationResult
            {
                FilePath = filePath,
                DocumentType = documentType,
                ValidationTime = DateTime.Now
            };

            _errors.Clear();

            if (!File.Exists(filePath))
            {
                _errors.Add(new ValidationError
                {
                    RuleId = "DOC001",
                    Message = $"File not found: {filePath}",
                    Severity = ValidationSeverity.Error
                });
                result.Errors = _errors.ToList();
                result.IsValid = false;
                return result;
            }

            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var validExtensions = GetValidExtensions(documentType);
            if (!validExtensions.Contains(extension))
            {
                _errors.Add(new ValidationError
                {
                    RuleId = "DOC001",
                    Message = $"Invalid file extension '{extension}' for document type {documentType}",
                    Severity = ValidationSeverity.Error
                });
            }

            var isValidSignature = ValidateFileSignature(filePath);
            if (!isValidSignature)
            {
                _errors.Add(new ValidationError
                {
                    RuleId = "DOC002",
                    Message = "Invalid file signature. File may be corrupted or not a valid Office document.",
                    Severity = ValidationSeverity.Error
                });
            }

            try
            {
                using (var archive = ZipFile.OpenRead(filePath))
                {
                    ValidateContentTypes(archive, documentType, result);
                    ValidateRelationships(archive, documentType, result);
                    ValidateDocumentPart(archive, documentType, result);
                    ValidateProperties(archive, result);
                    ValidateXmlContent(archive, result);
                }
            }
            catch (InvalidDataException ex)
            {
                _errors.Add(new ValidationError
                {
                    RuleId = "DOC010",
                    Message = $"Invalid ZIP archive: {ex.Message}",
                    Severity = ValidationSeverity.Error
                });
            }
            catch (Exception ex)
            {
                _errors.Add(new ValidationError
                {
                    RuleId = "DOC010",
                    Message = $"Error reading document: {ex.Message}",
                    Severity = ValidationSeverity.Error
                });
            }

            result.Errors = _errors.ToList();
            result.IsValid = !_errors.Any(e => e.Severity == ValidationSeverity.Error);
            result.ErrorCount = _errors.Count(e => e.Severity == ValidationSeverity.Error);
            result.WarningCount = _errors.Count(e => e.Severity == ValidationSeverity.Warning);
            return result;
        }

        private bool ValidateFileSignature(string filePath)
        {
            using (var stream = File.OpenRead(filePath))
            {
                var buffer = new byte[4];
                stream.Read(buffer, 0, 4);
                return buffer[0] == 0x50 && buffer[1] == 0x4B && buffer[2] == 0x03 && buffer[3] == 0x04;
            }
        }

        private List<string> GetValidExtensions(DocumentType documentType)
        {
            switch (documentType)
            {
                case DocumentType.Word:
                    return new List<string> { ".docx", ".docm", ".dotx", ".dotm" };
                case DocumentType.Spreadsheet:
                    return new List<string> { ".xlsx", ".xlsm", ".xlsb", ".xltx", ".xltm" };
                case DocumentType.Presentation:
                    return new List<string> { ".pptx", ".pptm", ".potx", ".potm" };
                default:
                    return new List<string>();
            }
        }

        private void ValidateContentTypes(ZipArchive archive, DocumentType documentType, ValidationResult result)
        {
            var contentTypesEntry = archive.GetEntry("[Content_Types].xml");
            if (contentTypesEntry == null)
            {
                _errors.Add(new ValidationError
                {
                    RuleId = "DOC005",
                    Message = "Missing [Content_Types].xml file",
                    Severity = ValidationSeverity.Error
                });
                return;
            }

            using (var stream = contentTypesEntry.Open())
            {
                var doc = XDocument.Load(stream);
                var root = doc.Root;
                if (root == null || root.Name.LocalName != "Types")
                {
                    _errors.Add(new ValidationError
                    {
                        RuleId = "DOC005",
                        Message = "Invalid [Content_Types].xml structure",
                        Severity = ValidationSeverity.Error
                    });
                }
            }
        }

        private void ValidateRelationships(ZipArchive archive, DocumentType documentType, ValidationResult result)
        {
            var relsPath = "_rels/.rels";
            var relsEntry = archive.GetEntry(relsPath);
            if (relsEntry == null)
            {
                _errors.Add(new ValidationError
                {
                    RuleId = "DOC006",
                    Message = "Missing _rels/.rels file",
                    Severity = ValidationSeverity.Warning
                });
            }
        }

        private void ValidateDocumentPart(ZipArchive archive, DocumentType documentType, ValidationResult result)
        {
            string mainPartPath;
            switch (documentType)
            {
                case DocumentType.Word:
                    mainPartPath = "word/document.xml";
                    break;
                case DocumentType.Spreadsheet:
                    mainPartPath = "xl/workbook.xml";
                    break;
                case DocumentType.Presentation:
                    mainPartPath = "ppt/presentation.xml";
                    break;
                default:
                    return;
            }

            var mainPart = archive.GetEntry(mainPartPath);
            if (mainPart == null)
            {
                _errors.Add(new ValidationError
                {
                    RuleId = "DOC003",
                    Message = $"Missing main document part: {mainPartPath}",
                    Severity = ValidationSeverity.Error
                });
                return;
            }

            using (var stream = mainPart.Open())
            {
                var buffer = new byte[1024];
                var bytesRead = stream.Read(buffer, 0, buffer.Length);
                var content = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                if (!content.Contains("<?xml"))
                {
                    _errors.Add(new ValidationError
                    {
                        RuleId = "DOC010",
                        Message = "Document part does not contain valid XML declaration",
                        Severity = ValidationSeverity.Error
                    });
                }
            }
        }

        private void ValidateProperties(ZipArchive archive, ValidationResult result)
        {
            var properties = new[] { "docProps/core.xml", "docProps/app.xml" };
            foreach (var prop in properties)
            {
                if (archive.GetEntry(prop) == null)
                {
                    _errors.Add(new ValidationError
                    {
                        RuleId = "DOC007",
                        Message = $"Missing document property: {prop}",
                        Severity = ValidationSeverity.Warning
                    });
                }
            }
        }

        private void ValidateXmlContent(ZipArchive archive, ValidationResult result)
        {
            foreach (var entry in archive.Entries)
            {
                if (entry.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        using (var stream = entry.Open())
                        using (var reader = XmlReader.Create(stream))
                        {
                            while (reader.Read()) { }
                        }
                    }
                    catch (XmlException ex)
                    {
                        _errors.Add(new ValidationError
                        {
                            RuleId = "DOC010",
                            Message = $"XML error in {entry.FullName}: {ex.Message}",
                            Severity = ValidationSeverity.Error
                        });
                    }
                }
            }
        }

        public List<ValidationRule> GetRules() => _rules.ToList();

        public ValidationRule GetRule(string ruleId) => _rules.FirstOrDefault(r => r.RuleId == ruleId);
    }

    public class OpenXmlSchemaValidator
    {
        private readonly Dictionary<string, XmlSchemaSet> _schemas;
        private readonly Dictionary<string, List<XmlSchemaElement>> _rootElements;

        public OpenXmlSchemaValidator()
        {
            _schemas = new Dictionary<string, XmlSchemaSet>();
            _rootElements = new Dictionary<string, List<XmlSchemaElement>>();
            InitializeSchemas();
        }

        private void InitializeSchemas()
        {
            _rootElements["document"] = new List<XmlSchemaElement>
            {
                new XmlSchemaElement { Name = "document", Namespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main" }
            };
            _rootElements["workbook"] = new List<XmlSchemaElement>
            {
                new XmlSchemaElement { Name = "workbook", Namespace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main" }
            };
            _rootElements["presentation"] = new List<XmlSchemaElement>
            {
                new XmlSchemaElement { Name = "presentation", Namespace = "http://schemas.openxmlformats.org/presentationml/2006/main" }
            };
        }

        public SchemaValidationResult ValidateAgainstSchema(string xmlContent, string schemaType)
        {
            var result = new SchemaValidationResult
            {
                SchemaType = schemaType,
                ValidationTime = DateTime.Now
            };

            try
            {
                var settings = new XmlReaderSettings
                {
                    DtdProcessing = DtdProcessing.Prohibit,
                    XmlResolver = null,
                    MaxCharactersFromEntities = 1000000
                };

                using (var reader = XmlReader.Create(new StringReader(xmlContent), settings))
                {
                    while (reader.Read()) { }
                }

                result.IsValid = true;
            }
            catch (XmlException ex)
            {
                result.IsValid = false;
                result.Errors.Add(new SchemaError
                {
                    Line = ex.LineNumber,
                    LinePosition = ex.LinePosition,
                    Message = ex.Message
                });
            }

            return result;
        }

        public List<SchemaElement> GetDocumentStructure(string filePath)
        {
            var elements = new List<SchemaElement>();

            try
            {
                using (var archive = ZipFile.OpenRead(filePath))
                {
                    foreach (var entry in archive.Entries.Where(e => e.FullName.EndsWith(".xml")))
                    {
                        using (var stream = entry.Open())
                        {
                            var doc = XDocument.Load(stream);
                            ExtractElements(doc.Root, entry.FullName, elements, 0);
                        }
                    }
                }
            }
            catch { }

            return elements;
        }

        private void ExtractElements(XElement element, string sourceFile, List<SchemaElement> elements, int depth)
        {
            if (element == null) return;

            elements.Add(new SchemaElement
            {
                Name = element.Name.LocalName,
                Namespace = element.Name.Namespace.ToString(),
                SourceFile = sourceFile,
                Depth = depth,
                HasAttributes = element.HasAttributes,
                HasElements = element.HasElements,
                LineNumber = ((IXmlLineInfo)element).LineNumber
            });

            foreach (var child in element.Elements())
            {
                ExtractElements(child, sourceFile, elements, depth + 1);
            }
        }
    }

    public class WordDocumentSchema
    {
        public const string WordprocessingML = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        public const string OfficeDocument = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        public const string Package = "http://schemas.openxmlformats.org/package/2006/relationships";
        public const string DublinCore = "http://purl.org/dc/elements/1.1/";
        public const string DublinCoreTerms = "http://purl.org/dc/terms/";

        public static readonly string[] RequiredParts = new[]
        {
            "[Content_Types].xml",
            "_rels/.rels",
            "word/document.xml",
            "word/_rels/document.xml.rels",
            "docProps/core.xml",
            "docProps/app.xml"
        };

        public static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>
        {
            { "word/document.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml" },
            { "word/styles.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml" },
            { "word/settings.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml" },
            { "word/webSettings.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.webSettings+xml" },
            { "word/fontTable.xml", "application/vnd.openxmlformats-officedocument.wordprocessingml.fontTable+xml" },
            { "word/theme/theme1.xml", "application/vnd.openxmlformats-officedocument.theme+xml" },
            { "docProps/core.xml", "application/vnd.openxmlformats-package.core-properties+xml" },
            { "docProps/app.xml", "application/vnd.openxmlformats-officedocument.extended-properties+xml" }
        };

        public static bool ValidateDocumentStructure(ZipArchive archive, out List<string> missingParts)
        {
            missingParts = new List<string>();
            var entries = archive.Entries.Select(e => e.FullName).ToHashSet();

            foreach (var required in RequiredParts)
            {
                if (!entries.Contains(required))
                {
                    missingParts.Add(required);
                }
            }

            return missingParts.Count == 0;
        }

        public static bool ValidateContentTypes(ZipArchive archive, out List<string> invalidTypes)
        {
            invalidTypes = new List<string>();
            var contentTypesEntry = archive.GetEntry("[Content_Types].xml");
            if (contentTypesEntry == null)
            {
                invalidTypes.Add("[Content_Types].xml not found");
                return false;
            }

            return true;
        }

        public static XNamespace W = WordprocessingML;
        public static XNamespace R = OfficeDocument;
        public static XNamespace PackageRel = Package;
        public static XNamespace DC = DublinCore;
        public static XNamespace DCTERMS = DublinCoreTerms;

        public static readonly Dictionary<string, string> ElementDescriptions = new Dictionary<string, string>
        {
            { "document", "Root element of the document" },
            { "body", "Container for document content" },
            { "p", "Paragraph element" },
            { "r", "Run (text run) element" },
            { "t", "Text element" },
            { "tbl", "Table element" },
            { "tr", "Table row element" },
            { "tc", "Table cell element" },
            { "sectPr", "Section properties" },
            { "pPr", "Paragraph properties" },
            { "rPr", "Run properties" }
        };
    }

    public class SpreadsheetDocumentSchema
    {
        public const string SpreadsheetML = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        public const string OfficeDocument = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        public const string Package = "http://schemas.openxmlformats.org/package/2006/relationships";
        public const string DublinCore = "http://purl.org/dc/elements/1.1/";
        public const string DublinCoreTerms = "http://purl.org/dc/terms/";
        public const string RelationshipOfficeDocument = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        public const string ExtendedProperties = "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";

        public static readonly string[] RequiredParts = new[]
        {
            "[Content_Types].xml",
            "_rels/.rels",
            "xl/workbook.xml",
            "xl/_rels/workbook.xml.rels",
            "xl/sharedStrings.xml",
            "xl/styles.xml",
            "docProps/core.xml",
            "docProps/app.xml"
        };

        public static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>
        {
            { "xl/workbook.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml" },
            { "xl/worksheets/sheet1.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml" },
            { "xl/sharedStrings.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml" },
            { "xl/styles.xml", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml" },
            { "xl/charts/chart1.xml", "application/vnd.openxmlformats-officedocument.drawingml.chart+xml" },
            { "xl/drawings/drawing1.xml", "application/vnd.openxmlformats-officedocument.drawingml.drawing+xml" },
            { "docProps/core.xml", "application/vnd.openxmlformats-package.core-properties+xml" },
            { "docProps/app.xml", "application/vnd.openxmlformats-officedocument.extended-properties+xml" }
        };

        public static XNamespace S = SpreadsheetML;
        public static XNamespace R = RelationshipOfficeDocument;
        public static XNamespace PackageRel = Package;
        public static XNamespace DC = DublinCore;
        public static XNamespace DCTERMS = DublinCoreTerms;

        public static readonly Dictionary<string, string> CellTypeReferences = new Dictionary<string, string>
        {
            { "b", "Boolean" },
            { "d", "Date" },
            { "e", "Error" },
            { "inlineStr", "Inline String" },
            { "n", "Number" },
            { "s", "Shared String" },
            { "str", "String (Formula)" }
        };

        public static readonly Dictionary<string, string> ElementDescriptions = new Dictionary<string, string>
        {
            { "workbook", "Root element of the workbook" },
            { "sheets", "Collection of sheet definitions" },
            { "sheet", "Individual sheet definition" },
            { "worksheet", "Individual worksheet" },
            { "sheetData", "Container for cell data" },
            { "row", "Row element containing cells" },
            { "c", "Cell element" },
            { "v", "Cell value" },
            { "f", "Cell formula" },
            { "definedNames", "Named ranges and formulas" }
        };
    }

    public class PresentationDocumentSchema
    {
        public const string PresentationML = "http://schemas.openxmlformats.org/presentationml/2006/main";
        public const string DrawingML = "http://schemas.openxmlformats.org/drawingml/2006/main";
        public const string OfficeDocument = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        public const string Package = "http://schemas.openxmlformats.org/package/2006/relationships";
        public const string DublinCore = "http://purl.org/dc/elements/1.1/";
        public const string DublinCoreTerms = "http://purl.org/dc/terms/";
        public const string A = DrawingML;
        public const string P = PresentationML;
        public const string R = OfficeDocument;

        public static readonly string[] RequiredParts = new[]
        {
            "[Content_Types].xml",
            "_rels/.rels",
            "ppt/presentation.xml",
            "ppt/_rels/presentation.xml.rels",
            "ppt/slides/slide1.xml",
            "ppt/slideLayouts/slideLayout1.xml",
            "ppt/slideMasters/slideMaster1.xml",
            "ppt/theme/theme1.xml",
            "docProps/core.xml",
            "docProps/app.xml"
        };

        public static readonly Dictionary<string, string> ContentTypes = new Dictionary<string, string>
        {
            { "ppt/presentation.xml", "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml" },
            { "ppt/slides/slide1.xml", "application/vnd.openxmlformats-officedocument.presentationml.slide+xml" },
            { "ppt/slideLayouts/slideLayout1.xml", "application/vnd.openxmlformats-officedocument.presentationml.slideLayout+xml" },
            { "ppt/slideMasters/slideMaster1.xml", "application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml" },
            { "ppt/theme/theme1.xml", "application/vnd.openxmlformats-officedocument.theme+xml" },
            { "ppt/tableStyles.xml", "application/vnd.openxmlformats-officedocument.presentationml.tableStyles+xml" },
            { "ppt/viewProps.xml", "application/vnd.openxmlformats-officedocument.presentationml.viewProps+xml" },
            { "ppt/presProps.xml", "application/vnd.openxmlformats-officedocument.presentationml.presProps+xml" },
            { "docProps/core.xml", "application/vnd.openxmlformats-package.core-properties+xml" },
            { "docProps/app.xml", "application/vnd.openxmlformats-officedocument.extended-properties+xml" }
        };

        public static readonly Dictionary<string, string> ElementDescriptions = new Dictionary<string, string>
        {
            { "presentation", "Root element of the presentation" },
            { "sldIdLst", "Slide identifier list" },
            { "sldId", "Slide identifier" },
            { "sldMasterIdLst", "Slide master identifier list" },
            { "sldSz", "Slide size" },
            { "notesSz", "Notes page size" },
            { "slide", "Individual slide" },
            { "cSld", "Common slide data" },
            { "spTree", "Shape tree containing all shapes" },
            { "nvGrpSpPr", "Non-visual group shape properties" },
            { "grpSpPr", "Group shape properties" },
            { "txBody", "Text body" }
        };

        public static readonly Dictionary<string, string> TransitionTypes = new Dictionary<string, string>
        {
            { "blinds", "Blinds transition" },
            { "checker", "Checkerboard transition" },
            { "cover", "Cover transition" },
            { "dissolve", "Dissolve transition" },
            { "fade", "Fade transition" },
            { "pull", "Pull transition" },
            { "push", "Push transition" },
            { "randomBar", "Random bar transition" },
            { "strips", "Strips transition" },
            { "wipe", "Wipe transition" }
        };

        public static readonly Dictionary<string, string> AnimationTypes = new Dictionary<string, string>
        {
            { "appear", "Appear animation" },
            { "fade", "Fade animation" },
            { "fly", "Fly animation" },
            { "wipe", "Wipe animation" },
            { "format", "Format animation" },
            { "verb", "Verb animation" }
        };
    }

    public enum DocumentType
    {
        Word,
        Spreadsheet,
        Presentation,
        Unknown
    }

    public enum ValidationSeverity
    {
        Information,
        Warning,
        Error,
        Critical
    }

    public enum ValidationCategory
    {
        FileFormat,
        Structure,
        Schema,
        Content,
        Metadata,
        Xml,
        Relationships
    }

    public class ValidationRule
    {
        public string RuleId { get; set; }
        public string RuleName { get; set; }
        public string Description { get; set; }
        public ValidationSeverity Severity { get; set; }
        public ValidationCategory Category { get; set; }
        public bool IsEnabled { get; set; } = true;
    }

    public class ValidationError
    {
        public string RuleId { get; set; }
        public string Message { get; set; }
        public ValidationSeverity Severity { get; set; }
        public int Line { get; set; }
        public int Position { get; set; }
        public string FilePath { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public class ValidationResult
    {
        public string FilePath { get; set; }
        public DocumentType DocumentType { get; set; }
        public bool IsValid { get; set; }
        public List<ValidationError> Errors { get; set; } = new List<ValidationError>();
        public int ErrorCount { get; set; }
        public int WarningCount { get; set; }
        public int InfoCount { get; set; }
        public DateTime ValidationTime { get; set; }
        public TimeSpan ValidationDuration { get; set; }
        public Dictionary<string, object> Metadata { get; set; } = new Dictionary<string, object>();
    }

    public class SchemaElement
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string SourceFile { get; set; }
        public int Depth { get; set; }
        public bool HasAttributes { get; set; }
        public bool HasElements { get; set; }
        public int LineNumber { get; set; }
        public List<SchemaAttribute> Attributes { get; set; } = new List<SchemaAttribute>();
    }

    public class SchemaAttribute
    {
        public string Name { get; set; }
        public string Namespace { get; set; }
        public string Type { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultValue { get; set; }
    }

    public class SchemaError
    {
        public int Line { get; set; }
        public int LinePosition { get; set; }
        public string Message { get; set; }
        public string SourceUri { get; set; }
    }

    public class SchemaValidationResult
    {
        public string SchemaType { get; set; }
        public bool IsValid { get; set; }
        public List<SchemaError> Errors { get; set; } = new List<SchemaError>();
        public DateTime ValidationTime { get; set; }
        public TimeSpan ValidationDuration { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace OfficeSuite.Common.FileHandling
{
    public enum OfficeDocumentType
    {
        Word,
        Excel,
        PowerPoint,
        Unknown
    }

    public class OpenXmlPackage
    {
        public string FilePath { get; set; }
        public OfficeDocumentType DocumentType { get; set; }
        public PackageStructure Structure { get; set; }
        public Dictionary<string, byte[]> Parts { get; set; } = new Dictionary<string, byte[]>();
        public PackageMetadata Metadata { get; set; } = new PackageMetadata();
        public List<PackageRelationship> Relationships { get; set; } = new List<PackageRelationship>();
    }

    public class PackageStructure
    {
        public string RootDirectory { get; set; }
        public List<string> ContentTypes { get; set; } = new List<string>();
        public List<string> Parts { get; set; } = new List<string>();
        public Dictionary<string, List<string>> Relationships { get; set; } = new Dictionary<string, List<string>>();
        public List<string> SharedStrings { get; set; } = new List<string>();
        public PackageSecurity Security { get; set; } = new PackageSecurity();
    }

    public class PackageMetadata
    {
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Creator { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public string LastModifiedBy { get; set; }
        public string Revision { get; set; }
        public DateTime Created { get; set; }
        public DateTime Modified { get; set; }
        public DateTime? LastPrinted { get; set; }
        public string Category { get; set; }
        public string ContentStatus { get; set; }
        public string Identifier { get; set; }
        public string Language { get; set; }
        public string Version { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
    }

    public class PackageRelationship
    {
        public string Id { get; set; }
        public string Type { get; set; }
        public string Target { get; set; }
        public string Source { get; set; }
        public RelationshipTargetMode TargetMode { get; set; } = RelationshipTargetMode.Internal;
    }

    public enum RelationshipTargetMode
    {
        Internal,
        External
    }

    public class PackageSecurity
    {
        public bool IsEncrypted { get; set; } = false;
        public bool IsSigned { get; set; } = false;
        public bool IsMacroEnabled { get; set; } = false;
        public string EncryptionProvider { get; set; }
        public int EncryptionFlags { get; set; }
        public X509Certificate2 SignatureCertificate { get; set; }
        public List<DigitalSignature> Signatures { get; set; } = new List<DigitalSignature>();
    }

    public class DigitalSignature
    {
        public string Id { get; set; }
        public string Issuer { get; set; }
        public string SerialNumber { get; set; }
        public DateTime SignedDate { get; set; }
        public string SignatureValue { get; set; }
        public string CertificateThumbprint { get; set; }
        public string SignatureAlgorithm { get; set; }
        public string HashAlgorithm { get; set; }
        public string Description { get; set; }
        public string ContactInfo { get; set; }
        public string OfficeSigningCertificateDisplay { get; set; }
        public bool IsValid { get; set; }
        public DateTime? ValidationTime { get; set; }
    }

    public class OpenXmlPackageReader
    {
        public OpenXmlPackage Read(string filePath)
        {
            var package = new OpenXmlPackage
            {
                FilePath = filePath,
                DocumentType = DetermineDocumentType(filePath)
            };

            using (var archive = ZipFile.OpenRead(filePath))
            {
                foreach (var entry in archive.Entries)
                {
                    using (var stream = entry.Open())
                    {
                        using (var memoryStream = new MemoryStream())
                        {
                            stream.CopyTo(memoryStream);
                            package.Parts[entry.FullName] = memoryStream.ToArray();
                        }
                    }
                }

                ExtractMetadata(package);
                ExtractRelationships(package);
            }

            return package;
        }

        private OfficeDocumentType DetermineDocumentType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();

            return extension switch
            {
                ".docx" => OfficeDocumentType.Word,
                ".xlsx" => OfficeDocumentType.Excel,
                ".pptx" => OfficeDocumentType.PowerPoint,
                ".docm" => OfficeDocumentType.Word,
                ".xlsm" => OfficeDocumentType.Excel,
                ".pptm" => OfficeDocumentType.PowerPoint,
                _ => OfficeDocumentType.Unknown
            };
        }

        private void ExtractMetadata(OpenXmlPackage package)
        {
            if (package.Parts.TryGetValue("docProps/core.xml", out var coreXml))
            {
                try
                {
                    var doc = XDocument.Parse(Encoding.UTF8.GetString(coreXml));
                    var ns = doc.Root.GetDefaultNamespace();

                    package.Metadata.Title = doc.Root.Element(ns + "title")?.Value;
                    package.Metadata.Subject = doc.Root.Element(ns + "subject")?.Value;
                    package.Metadata.Creator = doc.Root.Element(ns + "creator")?.Value;
                    package.Metadata.Keywords = doc.Root.Element(ns + "keywords")?.Value;
                    package.Metadata.Description = doc.Root.Element(ns + "description")?.Value;
                    package.Metadata.LastModifiedBy = doc.Root.Element(ns + "lastModifiedBy")?.Value;
                    package.Metadata.Revision = doc.Root.Element(ns + "revision")?.Value;

                    if (DateTime.TryParse(doc.Root.Element(ns + "created")?.Value, out var created))
                        package.Metadata.Created = created;

                    if (DateTime.TryParse(doc.Root.Element(ns + "modified")?.Value, out var modified))
                        package.Metadata.Modified = modified;
                }
                catch { }
            }

            if (package.Parts.TryGetValue("docProps/app.xml", out var appXml))
            {
                try
                {
                    var doc = XDocument.Parse(Encoding.UTF8.GetString(appXml));
                    var ns = doc.Root.GetDefaultNamespace();

                    package.Metadata.Category = doc.Root.Element(ns + "Category")?.Value;
                    package.Metadata.ContentStatus = doc.Root.Element(ns + "ContentStatus")?.Value;
                    package.Metadata.Version = doc.Root.Element(ns + "AppVersion")?.Value;
                }
                catch { }
            }
        }

        private void ExtractRelationships(OpenXmlPackage package)
        {
            var relsPaths = package.Parts.Keys
                .Where(k => k.EndsWith(".rels"))
                .ToList();

            foreach (var relsPath in relsPaths)
            {
                if (package.Parts.TryGetValue(relsPath, out var relsXml))
                {
                    try
                    {
                        var doc = XDocument.Parse(Encoding.UTF8.GetString(relsXml));
                        var source = relsPath.Replace("_rels/", "").Replace(".rels", "");

                        foreach (var rel in doc.Root.Elements())
                        {
                            var relationship = new PackageRelationship
                            {
                                Id = rel.Attribute("Id")?.Value,
                                Type = rel.Attribute("Type")?.Value,
                                Target = rel.Attribute("Target")?.Value,
                                Source = source,
                                TargetMode = rel.Attribute("TargetMode")?.Value == "External"
                                    ? RelationshipTargetMode.External
                                    : RelationshipTargetMode.Internal
                            };
                            package.Relationships.Add(relationship);
                        }
                    }
                    catch { }
                }
            }
        }

        public bool Validate(OpenXmlPackage package)
        {
            var errors = new List<string>();

            if (!package.Parts.ContainsKey("[Content_Types].xml"))
                errors.Add("Missing [Content_Types].xml");

            if (!package.Parts.ContainsKey("_rels/.rels"))
                errors.Add("Missing _rels/.rels");

            switch (package.DocumentType)
            {
                case OfficeDocumentType.Word:
                    if (!package.Parts.ContainsKey("word/document.xml"))
                        errors.Add("Missing word/document.xml");
                    break;

                case OfficeDocumentType.Excel:
                    if (!package.Parts.ContainsKey("xl/workbook.xml"))
                        errors.Add("Missing xl/workbook.xml");
                    break;

                case OfficeDocumentType.PowerPoint:
                    if (!package.Parts.ContainsKey("ppt/presentation.xml"))
                        errors.Add("Missing ppt/presentation.xml");
                    break;
            }

            return errors.Count == 0;
        }

        public List<string> GetContentErrors(OpenXmlPackage package)
        {
            var errors = new List<string>();

            if (!Validate(package))
                errors.Add("Package validation failed");

            foreach (var rel in package.Relationships)
            {
                if (rel.TargetMode == RelationshipTargetMode.Internal)
                {
                    var targetPath = ResolveTargetPath(rel.Source, rel.Target);
                    if (!package.Parts.ContainsKey(targetPath))
                        errors.Add($"Broken relationship: {rel.Id} -> {rel.Target}");
                }
            }

            return errors;
        }

        private string ResolveTargetPath(string source, string target)
        {
            if (target.StartsWith("/"))
                return target.TrimStart('/');

            var sourceDir = Path.GetDirectoryName(source);
            return Path.Combine(sourceDir, target).Replace('\\', '/');
        }
    }

    public class OpenXmlPackageWriter
    {
        private readonly List<byte[]> _buffers = new List<byte[]>();

        public void Create(OpenXmlPackage package)
        {
            if (File.Exists(package.FilePath))
                File.Delete(package.FilePath);

            using (var archive = ZipFile.Open(package.FilePath, ZipArchiveMode.Create))
            {
                foreach (var part in package.Parts)
                {
                    var entry = archive.CreateEntry(part.Key, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(part.Value, 0, part.Value.Length);
                    }
                }
            }
        }

        public void Update(OpenXmlPackage package)
        {
            var tempPath = package.FilePath + ".tmp";

            using (var archive = ZipFile.Open(tempPath, ZipArchiveMode.Create))
            {
                foreach (var part in package.Parts)
                {
                    var entry = archive.CreateEntry(part.Key, CompressionLevel.Optimal);
                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(part.Value, 0, part.Value.Length);
                    }
                }
            }

            if (File.Exists(package.FilePath))
                File.Delete(package.FilePath);

            File.Move(tempPath, package.FilePath);
        }

        public void AddPart(OpenXmlPackage package, string path, byte[] content)
        {
            package.Parts[path] = content;
        }

        public void RemovePart(OpenXmlPackage package, string path)
        {
            package.Parts.Remove(path);
        }

        public void UpdateMetadata(OpenXmlPackage package)
        {
            var coreXml = CreateCoreXml(package.Metadata);
            var appXml = CreateAppXml(package.Metadata);

            package.Parts["docProps/core.xml"] = Encoding.UTF8.GetBytes(coreXml);
            package.Parts["docProps/app.xml"] = Encoding.UTF8.GetBytes(appXml);
        }

        private string CreateCoreXml(PackageMetadata metadata)
        {
            var ns = XNamespace.Get("http://schemas.openxmlformats.org/package/2006/metadata/core-properties");
            var dc = XNamespace.Get("http://purl.org/dc/elements/1.1/");
            var dcterms = XNamespace.Get("http://purl.org/dc/terms/");
            var xsi = XNamespace.Get("http://www.w3.org/2001/XMLSchema-instance");

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(ns + "coreProperties",
                    new XAttribute(XNamespace.Xmlns + "cp", ns),
                    new XAttribute(XNamespace.Xmlns + "dc", dc),
                    new XAttribute(XNamespace.Xmlns + "dcterms", dcterms),
                    new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                    new XElement(dc + "title", metadata.Title ?? ""),
                    new XElement(dc + "subject", metadata.Subject ?? ""),
                    new XElement(dc + "creator", metadata.Creator ?? ""),
                    new XElement(cp + "keywords", metadata.Keywords ?? ""),
                    new XElement(dc + "description", metadata.Description ?? ""),
                    new XElement(cp + "lastModifiedBy", metadata.LastModifiedBy ?? ""),
                    new XElement(cp + "revision", metadata.Revision ?? "1"),
                    new XElement(dcterms + "created",
                        new XAttribute(xsi + "type", "dcterms:W3CDTF"),
                        metadata.Created.ToString("yyyy-MM-ddTHH:mm:ssZ")),
                    new XElement(dcterms + "modified",
                        new XAttribute(xsi + "type", "dcterms:W3CDTF"),
                        metadata.Modified.ToString("yyyy-MM-ddTHH:mm:ssZ"))))
                .ToString();
        }

        private string CreateAppXml(PackageMetadata metadata)
        {
            var ns = XNamespace.Get("http://schemas.openxmlformats.org/officeDocument/2006/extended-properties");

            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(ns + "Properties",
                    new XAttribute(XNamespace.Xmlns, ns),
                    new XElement(ns + "Application", "OfficeSuite"),
                    new XElement(ns + "AppVersion", metadata.Version ?? "1.0"),
                    new XElement(ns + "DocSecurity", "0"),
                    new XElement(ns + "ScaleCrop", "false"),
                    new XElement(ns + "Company", ""),
                    new XElement(ns + "LinksUpToDate", "false"),
                    new XElement(ns + "SharedDoc", "false"),
                    new XElement(ns + "HyperlinksChanged", "false"),
                    new XElement(ns + "AppName", "OfficeSuite")))
                .ToString();
        }

        public void AddRelationship(OpenXmlPackage package, PackageRelationship relationship)
        {
            var relsPath = GetRelsPath(relationship.Source);
            var existingRels = package.Parts.ContainsKey(relsPath)
                ? Encoding.UTF8.GetString(package.Parts[relsPath])
                : CreateEmptyRels(relationship.Source);

            var doc = XDocument.Parse(existingRels);
            var relsElement = doc.Root;

            relsElement.Add(new XElement("Relationship",
                new XAttribute("Id", relationship.Id),
                new XAttribute("Type", relationship.Type),
                new XAttribute("Target", relationship.Target),
                relationship.TargetMode == RelationshipTargetMode.External
                    ? new XAttribute("TargetMode", "External")
                    : null));

            package.Parts[relsPath] = Encoding.UTF8.GetBytes(doc.ToString());
        }

        private string GetRelsPath(string source)
        {
            var dir = Path.GetDirectoryName(source);
            return Path.Combine(dir, "_rels", Path.GetFileName(source) + ".rels")
                .Replace('\\', '/');
        }

        private string CreateEmptyRels(string source)
        {
            return new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement("Relationships",
                    new XAttribute(XNamespace.Xmlns + "rel",
                        "http://schemas.openxmlformats.org/package/2006/relationships")))
                .ToString();
        }

        public void Sign(OpenXmlPackage package, X509Certificate2 certificate)
        {
            package.Security.IsSigned = true;
            package.Security.SignatureCertificate = certificate;

            var signature = new DigitalSignature
            {
                Id = Guid.NewGuid().ToString("N"),
                Issuer = certificate.Issuer,
                SerialNumber = certificate.SerialNumber,
                SignedDate = DateTime.Now,
                CertificateThumbprint = certificate.Thumbprint,
                SignatureAlgorithm = "RSA",
                HashAlgorithm = "SHA256",
                IsValid = true,
                ValidationTime = DateTime.Now
            };

            package.Security.Signatures.Add(signature);
        }

        public bool Verify(OpenXmlPackage package)
        {
            foreach (var signature in package.Security.Signatures)
            {
                if (!signature.IsValid)
                    return false;
            }

            return package.Security.Signatures.Count > 0;
        }

        public void Encrypt(OpenXmlPackage package, string password)
        {
            package.Security.IsEncrypted = true;
            package.Security.EncryptionProvider = "Microsoft Enhanced Cryptographic Provider v1.0";
        }

        public void Decrypt(OpenXmlPackage package, string password)
        {
            package.Security.IsEncrypted = false;
        }
    }

    public class OpenXmlDocumentTemplate
    {
        public static OpenXmlPackage CreateFromTemplate(string templatePath, string outputPath)
        {
            File.Copy(templatePath, outputPath, true);

            var reader = new OpenXmlPackageReader();
            var package = reader.Read(outputPath);
            return package;
        }

        public static OpenXmlPackage CreateBlank(OfficeDocumentType documentType, string filePath)
        {
            var package = new OpenXmlPackage
            {
                FilePath = filePath,
                DocumentType = documentType
            };

            switch (documentType)
            {
                case OfficeDocumentType.Word:
                    CreateBlankWordDocument(package);
                    break;
                case OfficeDocumentType.Excel:
                    CreateBlankExcelDocument(package);
                    break;
                case OfficeDocumentType.PowerPoint:
                    CreateBlankPowerPointDocument(package);
                    break;
            }

            var writer = new OpenXmlPackageWriter();
            writer.Create(package);

            return package;
        }

        private static void CreateBlankWordDocument(OpenXmlPackage package)
        {
            package.Parts["[Content_Types].xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/word/document.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml""/>
  <Override PartName=""/word/styles.xml"" ContentType=""application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml""/>
  <Override PartName=""/docProps/core.xml"" ContentType=""application/vnd.openxmlformats-package.core-properties+xml""/>
  <Override PartName=""/docProps/app.xml"" ContentType=""application/vnd.openxmlformats-officedocument.extended-properties+xml""/>
</Types>");

            package.Parts["_rels/.rels"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""word/document.xml""/>
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"" Target=""docProps/core.xml""/>
  <Relationship Id=""rId3"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"" Target=""docProps/app.xml""/>
</Relationships>");

            package.Parts["word/document.xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:document xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:body>
    <w:p>
      <w:r>
        <w:t>Click here to enter text</w:t>
      </w:r>
    </w:p>
  </w:body>
</w:document>");

            package.Parts["word/styles.xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<w:styles xmlns:w=""http://schemas.openxmlformats.org/wordprocessingml/2006/main"">
  <w:docDefaults>
    <w:rPrDefault>
      <w:rPr>
        <w:rFonts w:ascii=""Calibri"" w:hAnsi=""Calibri""/>
        <w:sz w:val=""22""/>
      </w:rPr>
    </w:rPrDefault>
  </w:docDefaults>
  <w:style w:type=""paragraph"" w:styleId=""Normal"">
    <w:name w:val=""Normal""/>
    <w:qFormat/>
  </w:style>
</w:styles>");
        }

        private static void CreateBlankExcelDocument(OpenXmlPackage package)
        {
            package.Parts["[Content_Types].xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/xl/workbook.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml""/>
  <Override PartName=""/xl/worksheets/sheet1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml""/>
  <Override PartName=""/docProps/core.xml"" ContentType=""application/vnd.openxmlformats-package.core-properties+xml""/>
  <Override PartName=""/docProps/app.xml"" ContentType=""application/vnd.openxmlformats-officedocument.extended-properties+xml""/>
</Types>");

            package.Parts["_rels/.rels"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""xl/workbook.xml""/>
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"" Target=""docProps/core.xml""/>
  <Relationship Id=""rId3"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"" Target=""docProps/app.xml""/>
</Relationships>");

            package.Parts["xl/workbook.xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<workbook xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <sheets>
    <sheet name=""Sheet1"" sheetId=""1"" r:id=""rId1""/>
  </sheets>
</workbook>");

            package.Parts["xl/worksheets/sheet1.xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<worksheet xmlns=""http://schemas.openxmlformats.org/spreadsheetml/2006/main"">
  <sheetData/>
</worksheet>");
        }

        private static void CreateBlankPowerPointDocument(OpenXmlPackage package)
        {
            package.Parts["[Content_Types].xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Types xmlns=""http://schemas.openxmlformats.org/package/2006/content-types"">
  <Default Extension=""rels"" ContentType=""application/vnd.openxmlformats-package.relationships+xml""/>
  <Default Extension=""xml"" ContentType=""application/xml""/>
  <Override PartName=""/ppt/presentation.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml""/>
  <Override PartName=""/ppt/slides/slide1.xml"" ContentType=""application/vnd.openxmlformats-officedocument.presentationml.slide+xml""/>
  <Override PartName=""/docProps/core.xml"" ContentType=""application/vnd.openxmlformats-package.core-properties+xml""/>
  <Override PartName=""/docProps/app.xml"" ContentType=""application/vnd.openxmlformats-officedocument.extended-properties+xml""/>
</Types>");

            package.Parts["_rels/.rels"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<Relationships xmlns=""http://schemas.openxmlformats.org/package/2006/relationships"">
  <Relationship Id=""rId1"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"" Target=""ppt/presentation.xml""/>
  <Relationship Id=""rId2"" Type=""http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties"" Target=""docProps/core.xml""/>
  <Relationship Id=""rId3"" Type=""http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties"" Target=""docProps/app.xml""/>
</Relationships>");

            package.Parts["ppt/presentation.xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:presentation xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:sldIdLst>
    <p:sldId id=""256"" r:id=""rId2""/>
  </p:sldIdLst>
</p:presentation>");

            package.Parts["ppt/slides/slide1.xml"] = Encoding.UTF8.GetBytes(
                @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""yes""?>
<p:sld xmlns:p=""http://schemas.openxmlformats.org/presentationml/2006/main"">
  <p:cSld>
    <p:spTree>
      <p:nvGrpSpPr>
        <p:cNvPr id=""1"" name=""""/>
        <p:cNvGrpSpPr/>
        <p:nvPr/>
      </p:nvGrpSpPr>
      <p:grpSpPr/>
    </p:spTree>
  </p:cSld>
</p:sld>");
        }
    }

    public class OfficeDocumentConverter
    {
        public bool Convert(string inputPath, string outputPath, OfficeDocumentType targetType)
        {
            try
            {
                var reader = new OpenXmlPackageReader();
                var package = reader.Read(inputPath);

                switch (targetType)
                {
                    case OfficeDocumentType.Word:
                        return ConvertToWord(package, outputPath);
                    case OfficeDocumentType.Excel:
                        return ConvertToExcel(package, outputPath);
                    case OfficeDocumentType.PowerPoint:
                        return ConvertToPowerPoint(package, outputPath);
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool ConvertToWord(OpenXmlPackage package, string outputPath)
        {
            return false;
        }

        private bool ConvertToExcel(OpenXmlPackage package, string outputPath)
        {
            return false;
        }

        private bool ConvertToPowerPoint(OpenXmlPackage package, string outputPath)
        {
            return false;
        }
    }

    public class OfficeDocumentComparator
    {
        public ComparisonResult Compare(OpenXmlPackage original, OpenXmlPackage modified)
        {
            var result = new ComparisonResult();

            foreach (var part in original.Parts.Keys)
            {
                if (!modified.Parts.ContainsKey(part))
                {
                    result.RemovedParts.Add(part);
                }
                else if (!original.Parts[part].SequenceEqual(modified.Parts[part]))
                {
                    result.ModifiedParts.Add(part);
                    result.ModificationDetails[part] = CalculateModification(original.Parts[part], modified.Parts[part]);
                }
            }

            foreach (var part in modified.Parts.Keys)
            {
                if (!original.Parts.ContainsKey(part))
                {
                    result.AddedParts.Add(part);
                }
            }

            result.AreEqual = result.AddedParts.Count == 0 &&
                             result.RemovedParts.Count == 0 &&
                             result.ModifiedParts.Count == 0;

            return result;
        }

        private ModificationDetails CalculateModification(byte[] original, byte[] modified)
        {
            return new ModificationDetails
            {
                OriginalSize = original.Length,
                ModifiedSize = modified.Length,
                SizeDifference = modified.Length - original.Length,
                IsBinaryDifference = IsBinaryDifferent(original, modified)
            };
        }

        private bool IsBinaryDifferent(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
                return true;

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                    return true;
            }

            return false;
        }
    }

    public class ComparisonResult
    {
        public bool AreEqual { get; set; }
        public List<string> AddedParts { get; set; } = new List<string>();
        public List<string> RemovedParts { get; set; } = new List<string>();
        public List<string> ModifiedParts { get; set; } = new List<string>();
        public Dictionary<string, ModificationDetails> ModificationDetails { get; set; } = new Dictionary<string, ModificationDetails>();
    }

    public class ModificationDetails
    {
        public int OriginalSize { get; set; }
        public int ModifiedSize { get; set; }
        public int SizeDifference { get; set; }
        public bool IsBinaryDifference { get; set; }
    }
}

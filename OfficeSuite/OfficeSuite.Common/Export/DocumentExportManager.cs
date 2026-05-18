using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Packaging;
using System.Xml.Linq;
using System.Text;
using System.Security.Cryptography;

namespace OfficeSuite.Word.Export
{
    public class WordExportManager
    {
        private readonly WordDocumentExporter _wordExporter;
        private readonly WordDocumentImporter _wordImporter;

        public WordExportManager()
        {
            _wordExporter = new WordDocumentExporter();
            _wordImporter = new WordDocumentImporter();
        }

        public void ExportToFormat(Models.WordDocument document, string filePath, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Docx:
                    _wordExporter.ExportToDocx(document, filePath);
                    break;
                case ExportFormat.Pdf:
                    ExportToPdf(document, filePath);
                    break;
                case ExportFormat.Html:
                    ExportToHtml(document, filePath);
                    break;
                case ExportFormat.Rtf:
                    ExportToRtf(document, filePath);
                    break;
                case ExportFormat.Text:
                    ExportToText(document, filePath);
                    break;
            }
        }

        public Models.WordDocument ImportFromFormat(string filePath, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Docx:
                    return _wordImporter.ImportFromDocx(filePath);
                case ExportFormat.Rtf:
                    return ImportFromRtf(filePath);
                case ExportFormat.Text:
                    return ImportFromText(filePath);
                default:
                    return null;
            }
        }

        private void ExportToPdf(Models.WordDocument document, string filePath)
        {
        }

        private void ExportToHtml(Models.WordDocument document, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine($"<title>{document.Title}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { font-family: 'Calibri', Arial, sans-serif; line-height: 1.6; }");
            sb.AppendLine("p { margin-bottom: 10px; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            foreach (var paragraph in document.Paragraphs)
            {
                sb.AppendLine(ConvertParagraphToHtml(paragraph));
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");

            File.WriteAllText(filePath, sb.ToString());
        }

        private string ConvertParagraphToHtml(Models.Paragraph paragraph)
        {
            var sb = new StringBuilder();
            sb.Append("<p");

            if (paragraph.Format != null)
            {
                var styles = new List<string>();

                if (paragraph.Format.Alignment == Models.HorizontalAlignment.Center)
                    styles.Add("text-align: center");
                else if (paragraph.Format.Alignment == Models.HorizontalAlignment.Right)
                    styles.Add("text-align: right");
                else if (paragraph.Format.Alignment == Models.HorizontalAlignment.Justify)
                    styles.Add("text-align: justify");

                if (paragraph.Format.LineSpacing > 1.0)
                    styles.Add($"line-height: {paragraph.Format.LineSpacing}");

                if (styles.Count > 0)
                    sb.Append($" style=\"{string.Join("; ", styles)}\"");
            }

            sb.Append(">");

            foreach (var run in paragraph.Runs)
            {
                sb.Append(ConvertRunToHtml(run));
            }

            sb.AppendLine("</p>");
            return sb.ToString();
        }

        private string ConvertRunToHtml(Models.Run run)
        {
            var text = System.Web.HttpUtility.HtmlEncode(run.Text);

            if (run.Format == null || string.IsNullOrEmpty(text))
                return text;

            var styles = new List<string>();

            if (run.Format.IsBold)
                text = $"<strong>{text}</strong>";

            if (run.Format.IsItalic)
                text = $"<em>{text}</em>";

            if (run.Format.IsUnderline)
                text = $"<u>{text}</u>";

            if (run.Format.IsStrikethrough)
                text = $"<s>{text}</s>";

            if (run.Format.ScriptType == Models.ScriptType.Superscript)
                text = $"<sup>{text}</sup>";
            else if (run.Format.ScriptType == Models.ScriptType.Subscript)
                text = $"<sub>{text}</sub>";

            return text;
        }

        private void ExportToRtf(Models.WordDocument document, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine(@"{\rtf1\ansi\deff0");
            sb.AppendLine(@"{\fonttbl");
            sb.AppendLine(@"{\f0 Calibri;}");
            sb.AppendLine(@"}");
            sb.AppendLine($@"{{\title {document.Title}}}");
            sb.AppendLine($@"{{\author {document.Properties?.Author}}}");

            foreach (var paragraph in document.Paragraphs)
            {
                sb.Append(ConvertParagraphToRtf(paragraph));
            }

            sb.AppendLine("}");
            File.WriteAllText(filePath, sb.ToString());
        }

        private string ConvertParagraphToRtf(Models.Paragraph paragraph)
        {
            var sb = new StringBuilder();
            sb.Append(@"\pard");

            if (paragraph.Format != null)
            {
                switch (paragraph.Format.Alignment)
                {
                    case Models.HorizontalAlignment.Center:
                        sb.Append(@"\qc");
                        break;
                    case Models.HorizontalAlignment.Right:
                        sb.Append(@"\qr");
                        break;
                    case Models.HorizontalAlignment.Justify:
                        sb.Append(@"\qj");
                        break;
                }

                if (paragraph.Format.LeftIndent > 0)
                    sb.Append($@"\li{paragraph.Format.LeftIndent}");

                if (paragraph.Format.FirstLineIndent > 0)
                    sb.Append($@"\fi{paragraph.Format.FirstLineIndent}");
            }

            foreach (var run in paragraph.Runs)
            {
                sb.Append(ConvertRunToRtf(run));
            }

            sb.AppendLine(@"\par");
            return sb.ToString();
        }

        private string ConvertRunToRtf(Models.Run run)
        {
            var sb = new StringBuilder();

            if (run.Format != null)
            {
                if (run.Format.IsBold)
                    sb.Append(@"\b");
                if (run.Format.IsItalic)
                    sb.Append(@"\i");
                if (run.Format.IsUnderline)
                    sb.Append(@"\ul");
                if (run.Format.IsStrikethrough)
                    sb.Append(@"\strike");
                if (run.Format.ScriptType == Models.ScriptType.Superscript)
                    sb.Append(@"\super");
                else if (run.Format.ScriptType == Models.ScriptType.Subscript)
                    sb.Append(@"\sub");
            }

            sb.Append(run.Text);
            sb.Append(" ");

            return sb.ToString();
        }

        private void ExportToText(Models.WordDocument document, string filePath)
        {
            var sb = new StringBuilder();
            foreach (var paragraph in document.Paragraphs)
            {
                foreach (var run in paragraph.Runs)
                {
                    sb.Append(run.Text);
                }
                sb.AppendLine();
            }
            File.WriteAllText(filePath, sb.ToString());
        }

        private Models.WordDocument ImportFromRtf(string filePath)
        {
            var document = new Models.WordDocument();
            var content = File.ReadAllText(filePath);

            var paragraph = new Models.Paragraph();
            var run = new Models.Run();
            var i = 0;

            while (i < content.Length)
            {
                if (content.Substring(i).StartsWith(@"\par"))
                {
                    if (!string.IsNullOrEmpty(run.Text))
                        paragraph.Runs.Add(run);
                    if (paragraph.Runs.Count > 0)
                        document.Paragraphs.Add(paragraph);
                    paragraph = new Models.Paragraph();
                    run = new Models.Run();
                    i += 4;
                }
                else if (content[i] == '{')
                {
                    var groupEnd = FindMatchingBrace(content, i);
                    var group = content.Substring(i, groupEnd - i + 1);
                    if (group.Contains(@"\f0"))
                    {
                    }
                    i = groupEnd + 1;
                }
                else if (content[i] == '}')
                {
                    i++;
                }
                else
                {
                    run.Text += content[i];
                    i++;
                }
            }

            if (!string.IsNullOrEmpty(run.Text))
                paragraph.Runs.Add(run);
            if (paragraph.Runs.Count > 0)
                document.Paragraphs.Add(paragraph);

            return document;
        }

        private int FindMatchingBrace(string content, int start)
        {
            var depth = 1;
            for (var i = start + 1; i < content.Length; i++)
            {
                if (content[i] == '{')
                    depth++;
                else if (content[i] == '}')
                    depth--;

                if (depth == 0)
                    return i;
            }
            return content.Length - 1;
        }

        private Models.WordDocument ImportFromText(string filePath)
        {
            var document = new Models.WordDocument();
            var lines = File.ReadAllLines(filePath);

            foreach (var line in lines)
            {
                var paragraph = new Models.Paragraph();
                var run = new Models.Run { Text = line };
                paragraph.Runs.Add(run);
                document.Paragraphs.Add(paragraph);
            }

            return document;
        }

        public void SetDocumentProperties(Models.WordDocument document, Models.DocumentProperties properties)
        {
            document.Properties = properties;
        }

        public Models.DocumentProperties GetDocumentProperties(Models.WordDocument document)
        {
            return document.Properties;
        }

        public void SetPageSettings(Models.WordDocument document, Models.PageSettings settings)
        {
            document.PageSettings = settings;
        }

        public Models.PageSettings GetPageSettings(Models.WordDocument document)
        {
            return document.PageSettings;
        }

        public void ValidateDocument(Models.WordDocument document, List<string> errors)
        {
            if (document == null)
            {
                errors.Add("Document is null");
                return;
            }

            if (document.Paragraphs == null || document.Paragraphs.Count == 0)
            {
                errors.Add("Document has no paragraphs");
            }

            foreach (var para in document.Paragraphs)
            {
                if (para.Runs == null)
                    errors.Add("Paragraph has no runs collection");
            }
        }

        public byte[] EncryptDocument(Models.WordDocument document, string password)
        {
            using (var aes = Aes.Create())
            {
                var key = DeriveKey(password);
                aes.Key = key;
                aes.GenerateIV();

                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);

                    var docXml = SerializeDocument(document);
                    var docBytes = Encoding.UTF8.GetBytes(docXml);

                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(docBytes, 0, docBytes.Length);
                    }

                    return ms.ToArray();
                }
            }
        }

        public Models.WordDocument DecryptDocument(byte[] encryptedData, string password)
        {
            using (var aes = Aes.Create())
            {
                var key = DeriveKey(password);

                var iv = new byte[16];
                Array.Copy(encryptedData, 0, iv, 0, iv.Length);
                aes.IV = iv;
                aes.Key = key;

                var encryptedBytes = new byte[encryptedData.Length - iv.Length];
                Array.Copy(encryptedData, iv.Length, encryptedBytes, 0, encryptedBytes.Length);

                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(new MemoryStream(encryptedBytes), aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        using (var reader = new StreamReader(cs))
                        {
                            var docXml = reader.ReadToEnd();
                            return DeserializeDocument(docXml);
                        }
                    }
                }
            }
        }

        private byte[] DeriveKey(string password)
        {
            var salt = Encoding.UTF8.GetBytes("OfficeSuiteSalt");
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256))
            {
                return deriveBytes.GetBytes(32);
            }
        }

        private string SerializeDocument(Models.WordDocument document)
        {
            return System.Text.Json.JsonSerializer.Serialize(document);
        }

        private Models.WordDocument DeserializeDocument(string xml)
        {
            return System.Text.Json.JsonSerializer.Deserialize<Models.WordDocument>(xml);
        }

        public void OptimizeDocument(Models.WordDocument document)
        {
            var emptyParagraphs = document.Paragraphs.Where(p => p.Runs.Count == 0 || p.Runs.All(r => string.IsNullOrEmpty(r.Text))).ToList();
            foreach (var para in emptyParagraphs)
            {
                document.Paragraphs.Remove(para);
            }

            var emptyRuns = document.Paragraphs.SelectMany(p => p.Runs).Where(r => string.IsNullOrEmpty(r.Text)).ToList();
            foreach (var run in emptyRuns)
            {
                foreach (var para in document.Paragraphs)
                {
                    para.Runs.Remove(run);
                }
            }

            foreach (var para in document.Paragraphs)
            {
                if (para.Runs.Count == 0)
                    para.Runs.Add(new Models.Run { Text = "" });
            }
        }

        public void MergeDocuments(Models.WordDocument target, Models.WordDocument source)
        {
            foreach (var para in source.Paragraphs)
            {
                target.Paragraphs.Add(para.Clone());
            }
        }

        public void SplitDocument(Models.WordDocument document, int paragraphCount)
        {
            var documents = new List<Models.WordDocument>();
            for (int i = 0; i < document.Paragraphs.Count; i += paragraphCount)
            {
                var doc = new Models.WordDocument();
                var paragraphs = document.Paragraphs.Skip(i).Take(paragraphCount);
                foreach (var para in paragraphs)
                {
                    doc.Paragraphs.Add(para.Clone());
                }
                documents.Add(doc);
            }
        }
    }

    public enum ExportFormat
    {
        Docx,
        Pdf,
        Html,
        Rtf,
        Text
    }

    public class WordDocumentExporter
    {
        public void ExportToDocx(Models.WordDocument document, string filePath)
        {
            using (var package = Package.Open(filePath, FileMode.Create))
            {
                CreateDocumentPart(package, document);
                CreateContentTypes(package);
                CreateRelationships(package);
            }
        }

        private void CreateDocumentPart(Package package, Models.WordDocument document)
        {
            var uri = new Uri("/word/document.xml", UriKind.Relative);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml");

            var xdoc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(W.NameSpace.W + "document",
                    new XAttribute(W.NameSpace.RelsId, "rId1"),
                    new XAttribute(XNamespace.Xmlns + "w", W.NameSpace.W),
                    new XAttribute(XNamespace.Xmlns + "r", W.NameSpace.RelsId),
                    new XElement(W.NameSpace.W + "body",
                        CreateBodyContent(document)
                    )
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }

        private XElement CreateBodyContent(Models.WordDocument document)
        {
            var body = new XElement(W.NameSpace.W + "body");

            foreach (var paragraph in document.Paragraphs)
            {
                body.Add(CreateParagraphElement(paragraph));
            }

            body.Add(new XElement(W.NameSpace.W + "sectPr",
                new XElement(W.NameSpace.W + "pgSz",
                    new XAttribute(W.NameSpace.W + "w", "12240"),
                    new XAttribute(W.NameSpace.W + "h", "15840")
                ),
                new XElement(W.NameSpace.W + "pgMar",
                    new XAttribute(W.NameSpace.W + "top", "1440"),
                    new XAttribute(W.NameSpace.W + "right", "1440"),
                    new XAttribute(W.NameSpace.W + "bottom", "1440"),
                    new XAttribute(W.NameSpace.W + "left", "1440"),
                    new XAttribute(W.NameSpace.W + "header", "720"),
                    new XAttribute(W.NameSpace.W + "footer", "720")
                )
            ));

            return body;
        }

        private XElement CreateParagraphElement(Models.Paragraph paragraph)
        {
            var paraElement = new XElement(W.NameSpace.W + "p");

            if (paragraph.Format != null)
            {
                var pPr = new XElement(W.NameSpace.W + "pPr");

                if (paragraph.Format.Alignment != Models.HorizontalAlignment.Left)
                {
                    var alignValue = paragraph.Format.Alignment switch
                    {
                        Models.HorizontalAlignment.Center => "center",
                        Models.HorizontalAlignment.Right => "right",
                        Models.HorizontalAlignment.Justify => "both",
                        _ => "left"
                    };
                    pPr.Add(new XElement(W.NameSpace.W + "jc", new XAttribute(W.NameSpace.W + "val", alignValue)));
                }

                if (paragraph.Format.LeftIndent > 0)
                {
                    pPr.Add(new XElement(W.NameSpace.W + "ind",
                        new XAttribute(W.NameSpace.W + "left", paragraph.Format.LeftIndent.ToString())));
                }

                paraElement.Add(pPr);
            }

            foreach (var run in paragraph.Runs)
            {
                paraElement.Add(CreateRunElement(run));
            }

            return paraElement;
        }

        private XElement CreateRunElement(Models.Run run)
        {
            var runElement = new XElement(W.NameSpace.W + "r");

            if (run.Format != null)
            {
                var rPr = new XElement(W.NameSpace.W + "rPr");

                if (run.Format.IsBold)
                    rPr.Add(new XElement(W.NameSpace.W + "b"));
                if (run.Format.IsItalic)
                    rPr.Add(new XElement(W.NameSpace.W + "i"));
                if (run.Format.IsUnderline)
                    rPr.Add(new XElement(W.NameSpace.W + "u", new XAttribute(W.NameSpace.W + "val", "single")));
                if (run.Format.IsStrikethrough)
                    rPr.Add(new XElement(W.NameSpace.W + "strike"));
                if (run.Format.IsDoubleStrikethrough)
                    rPr.Add(new XElement(W.NameSpace.W + "dstrike"));

                if (run.Format.ScriptType == Models.ScriptType.Superscript)
                    rPr.Add(new XElement(W.NameSpace.W + "sup"));
                else if (run.Format.ScriptType == Models.ScriptType.Subscript)
                    rPr.Add(new XElement(W.NameSpace.W + "sub"));

                if (!string.IsNullOrEmpty(run.Format.FontName))
                {
                    rPr.Add(new XElement(W.NameSpace.W + "rFonts",
                        new XAttribute(W.NameSpace.W + "ascii", run.Format.FontName),
                        new XAttribute(W.NameSpace.W + "hAnsi", run.Format.FontName)));
                }

                if (run.Format.FontSize > 0)
                {
                    rPr.Add(new XElement(W.NameSpace.W + "sz",
                        new XAttribute(W.NameSpace.W + "val", (run.Format.FontSize * 2).ToString())));
                }

                if (!string.IsNullOrEmpty(run.Format.Color))
                {
                    rPr.Add(new XElement(W.NameSpace.W + "color",
                        new XAttribute(W.NameSpace.W + "val", run.Format.Color.Replace("#", ""))));
                }

                if (rPr.HasElements)
                    runElement.Add(rPr);
            }

            runElement.Add(new XElement(W.NameSpace.W + "t",
                new XAttribute(XNamespace.Xml + "space", "preserve"),
                run.Text ?? string.Empty));

            return runElement;
        }

        private void CreateContentTypes(Package package)
        {
            var uri = new Uri("/[Content_Types].xml", UriKind.Absolute);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-package.content-types+xml");

            var xdoc = new XDocument(
                new XElement("Types",
                    new XAttribute(XNamespace.Xmlns, "http://schemas.openxmlformats.org/package/2006/content-types"),
                    new XElement("Default", new XAttribute("Extension", "rels"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                    new XElement("Default", new XAttribute("Extension", "xml"),
                        new XAttribute("ContentType", "application/xml")),
                    new XElement("Override", new XAttribute("PartName", "/word/document.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"))
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }

        private void CreateRelationships(Package package)
        {
            var uri = new Uri("/_rels/.rels", UriKind.Absolute);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-package.relationships+xml");

            var xdoc = new XDocument(
                new XElement("Relationships",
                    new XAttribute(XNamespace.Xmlns, "http://schemas.openxmlformats.org/package/2006/relationships"),
                    new XElement("Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "word/document.xml"))
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }
    }

    public class WordDocumentImporter
    {
        public Models.WordDocument ImportFromDocx(string filePath)
        {
            var document = new Models.WordDocument();

            using (var package = Package.Open(filePath, FileMode.Open))
            {
                var documentPart = package.GetPart(new Uri("/word/document.xml", UriKind.Relative));
                if (documentPart != null)
                {
                    using (var stream = documentPart.GetStream(FileMode.Open))
                    {
                        var xdoc = XDocument.Load(stream);
                        ParseDocumentXml(xdoc, document);
                    }
                }
            }

            return document;
        }

        private void ParseDocumentXml(XDocument xdoc, Models.WordDocument document)
        {
            var body = xdoc.Descendants(W.NameSpace.W + "body").FirstOrDefault();
            if (body == null)
                return;

            foreach (var paraElement in body.Elements(W.NameSpace.W + "p"))
            {
                var paragraph = ParseParagraph(paraElement);
                document.Paragraphs.Add(paragraph);
            }
        }

        private Models.Paragraph ParseParagraph(XElement paraElement)
        {
            var paragraph = new Models.Paragraph();

            var pPr = paraElement.Element(W.NameSpace.W + "pPr");
            if (pPr != null)
            {
                paragraph.Format = ParseParagraphFormat(pPr);
            }

            foreach (var runElement in paraElement.Elements(W.NameSpace.W + "r"))
            {
                var run = ParseRun(runElement);
                paragraph.Runs.Add(run);
            }

            return paragraph;
        }

        private Models.ParagraphFormat ParseParagraphFormat(XElement pPr)
        {
            var format = new Models.ParagraphFormat();

            var jc = pPr.Element(W.NameSpace.W + "jc");
            if (jc != null)
            {
                var val = jc.Attribute(W.NameSpace.W + "val")?.Value;
                format.Alignment = val switch
                {
                    "center" => Models.HorizontalAlignment.Center,
                    "right" => Models.HorizontalAlignment.Right,
                    "both" => Models.HorizontalAlignment.Justify,
                    _ => Models.HorizontalAlignment.Left
                };
            }

            var ind = pPr.Element(W.NameSpace.W + "ind");
            if (ind != null)
            {
                if (ind.Attribute(W.NameSpace.W + "left") != null)
                    format.LeftIndent = double.Parse(ind.Attribute(W.NameSpace.W + "left").Value);
                if (ind.Attribute(W.NameSpace.W + "firstLine") != null)
                    format.FirstLineIndent = double.Parse(ind.Attribute(W.NameSpace.W + "firstLine").Value);
            }

            var spacing = pPr.Element(W.NameSpace.W + "spacing");
            if (spacing != null)
            {
                if (spacing.Attribute(W.NameSpace.W + "line") != null)
                    format.LineSpacing = double.Parse(spacing.Attribute(W.NameSpace.W + "line").Value) / 240.0;
            }

            return format;
        }

        private Models.Run ParseRun(XElement runElement)
        {
            var run = new Models.Run { Text = "" };

            var t = runElement.Element(W.NameSpace.W + "t");
            if (t != null)
            {
                run.Text = t.Value;
            }

            var rPr = runElement.Element(W.NameSpace.W + "rPr");
            if (rPr != null)
            {
                run.Format = ParseRunFormat(rPr);
            }

            return run;
        }

        private Models.CharacterFormat ParseRunFormat(XElement rPr)
        {
            var format = new Models.CharacterFormat();

            if (rPr.Element(W.NameSpace.W + "b") != null)
                format.IsBold = true;
            if (rPr.Element(W.NameSpace.W + "i") != null)
                format.IsItalic = true;
            if (rPr.Element(W.NameSpace.W + "u") != null)
                format.IsUnderline = true;
            if (rPr.Element(W.NameSpace.W + "strike") != null)
                format.IsStrikethrough = true;
            if (rPr.Element(W.NameSpace.W + "dstrike") != null)
                format.IsDoubleStrikethrough = true;

            var sz = rPr.Element(W.NameSpace.W + "sz");
            if (sz != null && sz.Attribute(W.NameSpace.W + "val") != null)
            {
                format.FontSize = double.Parse(sz.Attribute(W.NameSpace.W + "val").Value) / 2.0;
            }

            var color = rPr.Element(W.NameSpace.W + "color");
            if (color != null && color.Attribute(W.NameSpace.W + "val") != null)
            {
                format.Color = "#" + color.Attribute(W.NameSpace.W + "val").Value;
            }

            return format;
        }
    }

    public static class W
    {
        public static XNamespace NameSpace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        public static XNamespace RelsId = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    }
}

namespace OfficeSuite.Spreadsheet.Export
{
    public class SpreadsheetExportManager
    {
        private readonly SpreadsheetExporter _exporter;
        private readonly SpreadsheetImporter _importer;

        public SpreadsheetExportManager()
        {
            _exporter = new SpreadsheetExporter();
            _importer = new SpreadsheetImporter();
        }

        public void ExportToFormat(Models.SpreadsheetDocument document, string filePath, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Xlsx:
                    _exporter.ExportToXlsx(document, filePath);
                    break;
                case ExportFormat.Csv:
                    ExportToCsv(document, filePath);
                    break;
                case ExportFormat.Html:
                    ExportToHtml(document, filePath);
                    break;
                case ExportFormat.Pdf:
                    ExportToPdf(document, filePath);
                    break;
            }
        }

        public Models.SpreadsheetDocument ImportFromFormat(string filePath, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Xlsx:
                    return _importer.ImportFromXlsx(filePath);
                case ExportFormat.Csv:
                    return ImportFromCsv(filePath);
                default:
                    return null;
            }
        }

        private void ExportToCsv(Models.SpreadsheetDocument document, string filePath)
        {
            var sb = new StringBuilder();
            foreach (var worksheet in document.Worksheets)
            {
                sb.AppendLine($"# {worksheet.Name}");
                for (int row = 0; row < worksheet.MaxRow; row++)
                {
                    var rowValues = new List<string>();
                    for (int col = 0; col < worksheet.MaxColumn; col++)
                    {
                        var cell = worksheet.GetCell(row, col);
                        rowValues.Add(cell?.Value?.ToString() ?? "");
                    }
                    sb.AppendLine(string.Join(",", rowValues));
                }
                sb.AppendLine();
            }
            File.WriteAllText(filePath, sb.ToString());
        }

        private void ExportToHtml(Models.SpreadsheetDocument document, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine("<style>");
            sb.AppendLine("table { border-collapse: collapse; width: 100%; }");
            sb.AppendLine("th, td { border: 1px solid #ddd; padding: 8px; }");
            sb.AppendLine("th { background-color: #4472C4; color: white; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            foreach (var worksheet in document.Worksheets)
            {
                sb.AppendLine($"<h2>{worksheet.Name}</h2>");
                sb.AppendLine("<table>");
                sb.AppendLine("<tr>");
                for (int col = 0; col < worksheet.MaxColumn; col++)
                {
                    var colLetter = GetColumnLetter(col);
                    sb.AppendLine($"<th>{colLetter}</th>");
                }
                sb.AppendLine("</tr>");

                for (int row = 0; row < worksheet.MaxRow; row++)
                {
                    sb.AppendLine("<tr>");
                    for (int col = 0; col < worksheet.MaxColumn; col++)
                    {
                        var cell = worksheet.GetCell(row, col);
                        var value = cell?.Value?.ToString() ?? "";
                        sb.AppendLine($"<td>{System.Web.HttpUtility.HtmlEncode(value)}</td>");
                    }
                    sb.AppendLine("</tr>");
                }
                sb.AppendLine("</table>");
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            File.WriteAllText(filePath, sb.ToString());
        }

        private void ExportToPdf(Models.SpreadsheetDocument document, string filePath)
        {
        }

        private Models.SpreadsheetDocument ImportFromCsv(string filePath)
        {
            var document = new Models.SpreadsheetDocument();
            var worksheet = new Models.Worksheet { Name = "Sheet1" };
            var lines = File.ReadAllLines(filePath);
            var currentRow = 0;

            foreach (var line in lines)
            {
                if (line.StartsWith("#"))
                    continue;

                var values = line.Split(',');
                for (int col = 0; col < values.Length; col++)
                {
                    var cell = new Models.Cell
                    {
                        Row = currentRow,
                        Column = col,
                        Value = values[col].Trim('"')
                    };
                    worksheet.Cells.Add(cell);
                }
                currentRow++;
            }

            document.Worksheets.Add(worksheet);
            return document;
        }

        private string GetColumnLetter(int columnIndex)
        {
            var result = "";
            while (columnIndex >= 0)
            {
                result = (char)('A' + columnIndex % 26) + result;
                columnIndex = columnIndex / 26 - 1;
            }
            return result;
        }

        public void SetWorksheetName(Models.SpreadsheetDocument document, int worksheetIndex, string name)
        {
            if (worksheetIndex >= 0 && worksheetIndex < document.Worksheets.Count)
            {
                document.Worksheets[worksheetIndex].Name = name;
            }
        }

        public string GetWorksheetName(Models.SpreadsheetDocument document, int worksheetIndex)
        {
            if (worksheetIndex >= 0 && worksheetIndex < document.Worksheets.Count)
            {
                return document.Worksheets[worksheetIndex].Name;
            }
            return null;
        }

        public void ProtectSheet(Models.SpreadsheetDocument document, int worksheetIndex, string password)
        {
            if (worksheetIndex >= 0 && worksheetIndex < document.Worksheets.Count)
            {
                document.Worksheets[worksheetIndex].IsProtected = true;
                document.Worksheets[worksheetIndex].ProtectionPassword = ComputePasswordHash(password);
            }
        }

        public void UnprotectSheet(Models.SpreadsheetDocument document, int worksheetIndex, string password)
        {
            if (worksheetIndex >= 0 && worksheetIndex < document.Worksheets.Count)
            {
                var worksheet = document.Worksheets[worksheetIndex];
                if (worksheet.ProtectionPassword == ComputePasswordHash(password))
                {
                    worksheet.IsProtected = false;
                    worksheet.ProtectionPassword = null;
                }
            }
        }

        private string ComputePasswordHash(string password)
        {
            using (var sha = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }

    public class SpreadsheetExporter
    {
        public void ExportToXlsx(Models.SpreadsheetDocument document, string filePath)
        {
            using (var package = Package.Open(filePath, FileMode.Create))
            {
                CreateWorkbookPart(package, document);
                CreateWorksheetParts(package, document);
                CreateContentTypes(package, document);
                CreateRelationships(package);
            }
        }

        private void CreateWorkbookPart(Package package, Models.SpreadsheetDocument document)
        {
            var uri = new Uri("/xl/workbook.xml", UriKind.Relative);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml");

            var xdoc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(S.NameSpace + "workbook",
                    new XAttribute(XNamespace.Xmlns + "r", S.RelationshipNamespace),
                    new XElement(S.NameSpace + "sheets",
                        document.Worksheets.Select((ws, index) =>
                            new XElement(S.NameSpace + "sheet",
                                new XAttribute(S.NameSpace + "name", ws.Name),
                                new XAttribute(S.NameSpace + "sheetId", index + 1),
                                new XAttribute("r:id", $"rId{index + 1}")
                            )
                        )
                    )
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }

        private void CreateWorksheetParts(Package package, Models.SpreadsheetDocument document)
        {
            for (int i = 0; i < document.Worksheets.Count; i++)
            {
                var worksheet = document.Worksheets[i];
                var uri = new Uri($"/xl/worksheets/sheet{i + 1}.xml", UriKind.Relative);
                var part = package.CreatePart(uri, "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml");

                var xdoc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", "yes"),
                    new XElement(S.NameSpace + "worksheet",
                        new XElement(S.NameSpace + "sheetData",
                            CreateSheetData(worksheet)
                        )
                    )
                );

                using (var stream = part.GetStream(FileMode.Create))
                {
                    xdoc.Save(stream);
                }
            }
        }

        private IEnumerable<XElement> CreateSheetData(Models.Worksheet worksheet)
        {
            var maxRow = worksheet.Cells.Count > 0 ? worksheet.Cells.Max(c => c.Row) + 1 : 0;
            var maxCol = worksheet.Cells.Count > 0 ? worksheet.Cells.Max(c => c.Column) + 1 : 0;

            for (int row = 0; row < maxRow; row++)
            {
                var rowElement = new XElement(S.NameSpace + "row",
                    new XAttribute(S.NameSpace + "r", row + 1));

                for (int col = 0; col < maxCol; col++)
                {
                    var cell = worksheet.GetCell(row, col);
                    if (cell != null && cell.Value != null)
                    {
                        var cellRef = GetCellReference(col, row);
                        var cellElement = new XElement(S.NameSpace + "c",
                            new XAttribute(S.NameSpace + "r", cellRef));

                        if (cell.Value is double || cell.Value is int || cell.Value is decimal)
                        {
                            cellElement.Add(new XAttribute(S.NameSpace + "t", "n"));
                            cellElement.Add(new XElement(S.NameSpace + "v", cell.Value.ToString()));
                        }
                        else
                        {
                            cellElement.Add(new XAttribute(S.NameSpace + "t", "s"));
                            var sharedString = GetOrAddSharedString(cell.Value.ToString());
                            cellElement.Add(new XElement(S.NameSpace + "v", sharedString));
                        }

                        rowElement.Add(cellElement);
                    }
                }

                yield return rowElement;
            }
        }

        private string GetCellReference(int column, int row)
        {
            return GetColumnLetter(column) + (row + 1).ToString();
        }

        private string GetColumnLetter(int columnIndex)
        {
            var result = "";
            while (columnIndex >= 0)
            {
                result = (char)('A' + columnIndex % 26) + result;
                columnIndex = columnIndex / 26 - 1;
            }
            return result;
        }

        private int GetOrAddSharedString(string value)
        {
            return 0;
        }

        private void CreateContentTypes(Package package, Models.SpreadsheetDocument document)
        {
            var uri = new Uri("/[Content_Types].xml", UriKind.Absolute);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-package.content-types+xml");

            var xdoc = new XDocument(
                new XElement("Types",
                    new XAttribute(XNamespace.Xmlns, "http://schemas.openxmlformats.org/package/2006/content-types"),
                    new XElement("Default", new XAttribute("Extension", "rels"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                    new XElement("Override", new XAttribute("PartName", "/xl/workbook.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"))
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }

        private void CreateRelationships(Package package)
        {
            var uri = new Uri("/_rels/.rels", UriKind.Absolute);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-package.relationships+xml");

            var xdoc = new XDocument(
                new XElement("Relationships",
                    new XAttribute(XNamespace.Xmlns, "http://schemas.openxmlformats.org/package/2006/relationships"),
                    new XElement("Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "xl/workbook.xml"))
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }
    }

    public class SpreadsheetImporter
    {
        private readonly List<string> _sharedStrings = new List<string>();

        public Models.SpreadsheetDocument ImportFromXlsx(string filePath)
        {
            var document = new Models.SpreadsheetDocument();

            using (var package = Package.Open(filePath, FileMode.Open))
            {
                LoadSharedStrings(package);

                var workbookPart = package.GetPart(new Uri("/xl/workbook.xml", UriKind.Relative));
                if (workbookPart != null)
                {
                    using (var stream = workbookPart.GetStream(FileMode.Open))
                    {
                        var xdoc = XDocument.Load(stream);
                        ParseWorkbook(xdoc, document, package);
                    }
                }
            }

            return document;
        }

        private void LoadSharedStrings(Package package)
        {
            _sharedStrings.Clear();
            var sharedStringsPart = package.GetPart(new Uri("/xl/sharedStrings.xml", UriKind.Relative));
            if (sharedStringsPart != null)
            {
                using (var stream = sharedStringsPart.GetStream(FileMode.Open))
                {
                    var xdoc = XDocument.Load(stream);
                    foreach (var si in xdoc.Descendants(S.NameSpace + "si"))
                    {
                        var t = si.Descendants(S.NameSpace + "t").FirstOrDefault();
                        _sharedStrings.Add(t?.Value ?? "");
                    }
                }
            }
        }

        private void ParseWorkbook(XDocument xdoc, Models.SpreadsheetDocument document, Package package)
        {
            var sheets = xdoc.Descendants(S.NameSpace + "sheet");
            foreach (var sheet in sheets)
            {
                var name = sheet.Attribute(S.NameSpace + "name")?.Value;
                var sheetId = sheet.Attribute("r:id")?.Value;

                var worksheet = new Models.Worksheet { Name = name };

                var worksheetUri = new Uri($"/xl/worksheets/sheet{GetSheetNumber(sheetId)}.xml", UriKind.Relative);
                var worksheetPart = package.GetPart(worksheetUri);
                if (worksheetPart != null)
                {
                    using (var stream = worksheetPart.GetStream(FileMode.Open))
                    {
                        var worksheetXdoc = XDocument.Load(stream);
                        ParseWorksheet(worksheetXdoc, worksheet);
                    }
                }

                document.Worksheets.Add(worksheet);
            }
        }

        private int GetSheetNumber(string rId)
        {
            if (int.TryParse(rId.Replace("rId", ""), out int number))
                return number;
            return 1;
        }

        private void ParseWorksheet(XDocument xdoc, Models.Worksheet worksheet)
        {
            var sheetData = xdoc.Descendants(S.NameSpace + "sheetData").FirstOrDefault();
            if (sheetData == null)
                return;

            foreach (var rowElement in sheetData.Elements(S.NameSpace + "row"))
            {
                var rowIndex = int.Parse(rowElement.Attribute(S.NameSpace + "r").Value) - 1;

                foreach (var cellElement in rowElement.Elements(S.NameSpace + "c"))
                {
                    var cellRef = cellElement.Attribute(S.NameSpace + "r").Value;
                    var colIndex = GetColumnIndex(cellRef);

                    var cell = new Models.Cell
                    {
                        Row = rowIndex,
                        Column = colIndex
                    };

                    var type = cellElement.Attribute(S.NameSpace + "t")?.Value;
                    var valueElement = cellElement.Element(S.NameSpace + "v");

                    if (valueElement != null)
                    {
                        switch (type)
                        {
                            case "s":
                                var index = int.Parse(valueElement.Value);
                                if (index >= 0 && index < _sharedStrings.Count)
                                    cell.Value = _sharedStrings[index];
                                break;
                            case "n":
                            case null:
                                if (double.TryParse(valueElement.Value, out double numValue))
                                    cell.Value = numValue;
                                break;
                            case "b":
                                cell.Value = valueElement.Value == "1";
                                break;
                            default:
                                cell.Value = valueElement.Value;
                                break;
                        }
                    }

                    worksheet.Cells.Add(cell);
                }
            }
        }

        private int GetColumnIndex(string cellRef)
        {
            var colLetters = "";
            foreach (var c in cellRef)
            {
                if (char.IsLetter(c))
                    colLetters += c;
                else
                    break;
            }

            var result = 0;
            foreach (var c in colLetters)
            {
                result = result * 26 + (c - 'A' + 1);
            }
            return result - 1;
        }
    }

    public static class S
    {
        public static XNamespace NameSpace = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        public static XNamespace RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    }

    public class PresentationExportManager
    {
        private readonly PresentationExporter _exporter;
        private readonly PresentationImporter _importer;

        public PresentationExportManager()
        {
            _exporter = new PresentationExporter();
            _importer = new PresentationImporter();
        }

        public void ExportToFormat(Models.PresentationDocument document, string filePath, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Pptx:
                    _exporter.ExportToPptx(document, filePath);
                    break;
                case ExportFormat.Pdf:
                    ExportToPdf(document, filePath);
                    break;
                case ExportFormat.Html:
                    ExportToHtml(document, filePath);
                    break;
                case ExportFormat.Images:
                    ExportToImages(document, filePath);
                    break;
            }
        }

        public Models.PresentationDocument ImportFromFormat(string filePath, ExportFormat format)
        {
            switch (format)
            {
                case ExportFormat.Pptx:
                    return _importer.ImportFromPptx(filePath);
                default:
                    return null;
            }
        }

        private void ExportToPdf(Models.PresentationDocument document, string filePath)
        {
        }

        private void ExportToHtml(Models.PresentationDocument document, string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html>");
            sb.AppendLine("<head>");
            sb.AppendLine("<meta charset=\"UTF-8\">");
            sb.AppendLine($"<title>{document.Title}</title>");
            sb.AppendLine("<style>");
            sb.AppendLine("body { margin: 0; font-family: Arial, sans-serif; }");
            sb.AppendLine(".slide { width: 960px; height: 720px; margin: 20px auto; border: 1px solid #ccc; }");
            sb.AppendLine("</style>");
            sb.AppendLine("</head>");
            sb.AppendLine("<body>");

            int slideNum = 1;
            foreach (var slide in document.Slides)
            {
                sb.AppendLine($"<div class=\"slide\" id=\"slide{slideNum}\">");
                sb.AppendLine($"<h1>Slide {slideNum}: {slide.Title}</h1>");
                foreach (var shape in slide.Shapes)
                {
                    sb.AppendLine($"<div style=\"position: absolute; left: {shape.X}px; top: {shape.Y}px; width: {shape.Width}px; height: {shape.Height}px;\">");
                    if (shape is Models.TextShape textShape)
                    {
                        sb.AppendLine($"<p>{System.Web.HttpUtility.HtmlEncode(textShape.Text)}</p>");
                    }
                    sb.AppendLine("</div>");
                }
                sb.AppendLine("</div>");
                slideNum++;
            }

            sb.AppendLine("</body>");
            sb.AppendLine("</html>");
            File.WriteAllText(filePath, sb.ToString());
        }

        private void ExportToImages(Models.PresentationDocument document, string filePath)
        {
        }

        public void SetPresentationTitle(Models.PresentationDocument document, string title)
        {
            document.Title = title;
        }

        public string GetPresentationTitle(Models.PresentationDocument document)
        {
            return document.Title;
        }
    }

    public class PresentationExporter
    {
        public void ExportToPptx(Models.PresentationDocument document, string filePath)
        {
            using (var package = Package.Open(filePath, FileMode.Create))
            {
                CreatePresentationPart(package, document);
                CreateSlideParts(package, document);
                CreateContentTypes(package);
                CreateRelationships(package);
            }
        }

        private void CreatePresentationPart(Package package, Models.PresentationDocument document)
        {
            var uri = new Uri("/ppt/presentation.xml", UriKind.Relative);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml");

            var xdoc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(P.NameSpace + "presentation",
                    new XAttribute(XNamespace.Xmlns + "a", P.AnimationNamespace),
                    new XAttribute(XNamespace.Xmlns + "r", P.RelationshipNamespace),
                    new XElement(P.NameSpace + "slideMasterIdLst",
                        new XElement(P.NameSpace + "slideMasterId", new XAttribute("id", "2147483648"),
                            new XAttribute("r:id", "rId1"))
                    ),
                    new XElement(P.NameSpace + "slideIdLst",
                        document.Slides.Select((slide, index) =>
                            new XElement(P.NameSpace + "slideId", new XAttribute("id", 256 + index),
                                new XAttribute("r:id", $"rId{index + 10}"))
                        )
                    ),
                    new XElement(P.NameSpace + "sldSz",
                        new XAttribute("cx", "9144000"),
                        new XAttribute("cy", "6858000")
                    )
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }

        private void CreateSlideParts(Package package, Models.PresentationDocument document)
        {
            for (int i = 0; i < document.Slides.Count; i++)
            {
                var slide = document.Slides[i];
                var uri = new Uri($"/ppt/slides/slide{i + 1}.xml", UriKind.Relative);
                var part = package.CreatePart(uri, "application/vnd.openxmlformats-officedocument.presentationml.slide+xml");

                var xdoc = new XDocument(
                    new XDeclaration("1.0", "UTF-8", "yes"),
                    new XElement(P.NameSpace + "sld",
                        new XElement(P.NameSpace + "cSld",
                            new XElement(P.NameSpace + "spTree",
                                CreateShapeTree(slide)
                            )
                        ),
                        new XElement(P.NameSpace + "clrMapOvr",
                            new XElement(P.NameSpace + "masterClrMapping")
                        )
                    )
                );

                using (var stream = part.GetStream(FileMode.Create))
                {
                    xdoc.Save(stream);
                }
            }
        }

        private IEnumerable<XElement> CreateShapeTree(Models.Slide slide)
        {
            var nvGrpSpPr = new XElement(P.NameSpace + "nvGrpSpPr",
                new XElement(P.NameSpace + "cNvPr", new XAttribute("id", "1"), new XAttribute("name", "")),
                new XElement(P.NameSpace + "cNvGrpSpPr"),
                new XElement(P.NameSpace + "nvPr")
            );

            var grpSpPr = new XElement(P.NameSpace + "grpSpPr",
                new XElement(A.NameSpace + "xfrm",
                    new XElement(A.NameSpace + "off", new XAttribute("x", "0"), new XAttribute("y", "0")),
                    new XElement(A.NameSpace + "ext", new XAttribute("cx", "9144000"), new XAttribute("cy", "6858000")),
                    new XElement(A.NameSpace + "chOff", new XAttribute("x", "0"), new XAttribute("y", "0")),
                    new XElement(A.NameSpace + "chExt", new XAttribute("cx", "9144000"), new XAttribute("cy", "6858000"))
                )
            );

            yield return new XElement(P.NameSpace + "nvGrpSpPr", nvGrpSpPr);
            yield return new XElement(P.NameSpace + "grpSpPr", grpSpPr);

            int shapeId = 2;
            foreach (var shape in slide.Shapes)
            {
                yield return CreateShapeElement(shape, shapeId++);
            }
        }

        private XElement CreateShapeElement(Models.SlideShape shape, int id)
        {
            var sp = new XElement(P.NameSpace + "sp");

            var nvSpPr = new XElement(P.NameSpace + "nvSpPr",
                new XElement(P.NameSpace + "cNvPr", new XAttribute("id", id), new XAttribute("name", shape.Name ?? $"Shape {id}")),
                new XElement(P.NameSpace + "cNvSpPr"),
                new XElement(P.NameSpace + "nvPr")
            );

            var spPr = new XElement(P.NameSpace + "spPr",
                new XElement(A.NameSpace + "xfrm",
                    new XElement(A.NameSpace + "off", new XAttribute("x", shape.X * 914.4), new XAttribute("y", shape.Y * 914.4)),
                    new XElement(A.NameSpace + "ext", new XAttribute("cx", shape.Width * 914.4), new XAttribute("cy", shape.Height * 914.4))
                ),
                new XElement(P.NameSpace + "prstGeom", new XAttribute("prst", "rect"),
                    new XElement(A.NameSpace + "avLst")
                )
            );

            if (shape.Fill != null && shape.Fill.Type != Models.FillType.None)
            {
                spPr.Add(new XElement(A.NameSpace + "solidFill",
                    new XElement(A.NameSpace + "srgbClr", new XAttribute("val", shape.Fill.Color ?? "FFFFFF"))
                ));
            }

            if (shape.Line != null)
            {
                spPr.Add(new XElement(A.NameSpace + "ln",
                    new XElement(A.NameSpace + "solidFill",
                        new XElement(A.NameSpace + "srgbClr", new XAttribute("val", shape.Line.Color ?? "000000"))
                    )
                ));
            }

            var txBody = new XElement(P.NameSpace + "txBody",
                new XElement(A.NameSpace + "bodyPr"),
                new XElement(A.NameSpace + "lstStyle"),
                new XElement(A.NameSpace + "p",
                    new XElement(A.NameSpace + "pPr", new XAttribute("algn", "l")),
                    new XElement(A.NameSpace + "endParaRPr")
                )
            );

            if (shape is Models.TextShape textShape && !string.IsNullOrEmpty(textShape.Text))
            {
                txBody.Element(A.NameSpace + "p").AddFirst(
                    new XElement(A.NameSpace + "r",
                        new XElement(A.NameSpace + "t", textShape.Text)
                    )
                );
            }

            sp.Add(nvSpPr);
            sp.Add(spPr);
            sp.Add(txBody);

            return sp;
        }

        private void CreateContentTypes(Package package)
        {
            var uri = new Uri("/[Content_Types].xml", UriKind.Absolute);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-package.content-types+xml");

            var xdoc = new XDocument(
                new XElement("Types",
                    new XAttribute(XNamespace.Xmlns, "http://schemas.openxmlformats.org/package/2006/content-types"),
                    new XElement("Default", new XAttribute("Extension", "rels"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                    new XElement("Default", new XAttribute("Extension", "xml"),
                        new XAttribute("ContentType", "application/xml")),
                    new XElement("Override", new XAttribute("PartName", "/ppt/presentation.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml"))
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }

        private void CreateRelationships(Package package)
        {
            var uri = new Uri("/_rels/.rels", UriKind.Absolute);
            var part = package.CreatePart(uri, "application/vnd.openxmlformats-package.relationships+xml");

            var xdoc = new XDocument(
                new XElement("Relationships",
                    new XAttribute(XNamespace.Xmlns, "http://schemas.openxmlformats.org/package/2006/relationships"),
                    new XElement("Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "ppt/presentation.xml"))
                )
            );

            using (var stream = part.GetStream(FileMode.Create))
            {
                xdoc.Save(stream);
            }
        }
    }

    public class PresentationImporter
    {
        public Models.PresentationDocument ImportFromPptx(string filePath)
        {
            var document = new Models.PresentationDocument();

            using (var package = Package.Open(filePath, FileMode.Open))
            {
                var presentationPart = package.GetPart(new Uri("/ppt/presentation.xml", UriKind.Relative));
                if (presentationPart != null)
                {
                    using (var stream = presentationPart.GetStream(FileMode.Open))
                    {
                        var xdoc = XDocument.Load(stream);
                        ParsePresentation(xdoc, document, package);
                    }
                }
            }

            return document;
        }

        private void ParsePresentation(XDocument xdoc, Models.PresentationDocument document, Package package)
        {
            var slideIdLst = xdoc.Descendants(P.NameSpace + "slideIdLst").FirstOrDefault();
            if (slideIdLst == null)
                return;

            foreach (var slideId in slideIdLst.Elements(P.NameSpace + "slideId"))
            {
                var rId = slideId.Attribute("r:id")?.Value;
                if (string.IsNullOrEmpty(rId))
                    continue;

                var slideUri = GetSlideUri(rId);
                if (slideUri == null)
                    continue;

                var slidePart = package.GetPart(slideUri);
                if (slidePart == null)
                    continue;

                using (var stream = slidePart.GetStream(FileMode.Open))
                {
                    var slideXdoc = XDocument.Load(stream);
                    var slide = ParseSlide(slideXdoc);
                    document.Slides.Add(slide);
                }
            }
        }

        private Uri GetSlideUri(string rId)
        {
            return new Uri($"/ppt/slides/slide{GetSlideNumber(rId)}.xml", UriKind.Relative);
        }

        private int GetSlideNumber(string rId)
        {
            if (int.TryParse(rId.Replace("rId", ""), out int number))
                return number - 9;
            return 1;
        }

        private Models.Slide ParseSlide(XDocument xdoc)
        {
            var slide = new Models.Slide();

            var sld = xdoc.Descendants(P.NameSpace + "sld").FirstOrDefault();
            if (sld == null)
                return slide;

            var spTree = sld.Descendants(P.NameSpace + "spTree").FirstOrDefault();
            if (spTree == null)
                return slide;

            foreach (var sp in spTree.Elements(P.NameSpace + "sp"))
            {
                var shape = ParseShape(sp);
                if (shape != null)
                    slide.Shapes.Add(shape);
            }

            return slide;
        }

        private Models.SlideShape ParseShape(XElement sp)
        {
            var shape = new Models.SlideShape();

            var nvSpPr = sp.Element(P.NameSpace + "nvSpPr");
            if (nvSpPr != null)
            {
                var cNvPr = nvSpPr.Element(P.NameSpace + "cNvPr");
                if (cNvPr != null)
                {
                    shape.Name = cNvPr.Attribute("name")?.Value;
                }
            }

            var spPr = sp.Element(P.NameSpace + "spPr");
            if (spPr != null)
            {
                var xfrm = spPr.Element(A.NameSpace + "xfrm");
                if (xfrm != null)
                {
                    var off = xfrm.Element(A.NameSpace + "off");
                    if (off != null)
                    {
                        shape.X = double.Parse(off.Attribute("x")?.Value ?? "0") / 914.4;
                        shape.Y = double.Parse(off.Attribute("y")?.Value ?? "0") / 914.4;
                    }

                    var ext = xfrm.Element(A.NameSpace + "ext");
                    if (ext != null)
                    {
                        shape.Width = double.Parse(ext.Attribute("cx")?.Value ?? "100") / 914.4;
                        shape.Height = double.Parse(ext.Attribute("cy")?.Value ?? "100") / 914.4;
                    }
                }
            }

            var txBody = sp.Element(P.NameSpace + "txBody");
            if (txBody != null)
            {
                var textShape = new Models.TextShape();
                textShape.X = shape.X;
                textShape.Y = shape.Y;
                textShape.Width = shape.Width;
                textShape.Height = shape.Height;

                var p = txBody.Element(A.NameSpace + "p");
                if (p != null)
                {
                    var r = p.Element(A.NameSpace + "r");
                    if (r != null)
                    {
                        var t = r.Element(A.NameSpace + "t");
                        if (t != null)
                        {
                            textShape.Text = t.Value;
                        }
                    }
                }

                return textShape;
            }

            return shape;
        }
    }

    public static class P
    {
        public static XNamespace NameSpace = "http://schemas.openxmlformats.org/presentationml/2006/main";
        public static XNamespace AnimationNamespace = "http://schemas.openxmlformats.org/drawingml/2006/main";
        public static XNamespace RelationshipNamespace = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
    }

    public static class A
    {
        public static XNamespace NameSpace = "http://schemas.openxmlformats.org/drawingml/2006/main";
    }
}

using System;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using OfficeSuite.Word.Models;

namespace OfficeSuite.Word.Export
{
    public class WordDocumentExporter
    {
        public void ExportToDocx(WordDocument document, string filePath)
        {
            using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
            
            CreateContentTypes(archive);
            CreateDocumentRelationships(archive);
            CreateRootRelationships(archive);
            CreateAppProperties(archive);
            CreateStyles(document, archive);
            CreateDocument(document, archive);
            CreateSettings(archive);
            CreateHeadersFooters(document, archive);
        }

        private void CreateContentTypes(ZipArchive archive)
        {
            var contentTypes = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Namespaces.ContentTypes + "Types",
                    new XAttribute(XNamespace.Xmlns + "ct", Namespaces.ContentTypes),
                    new XElement(Namespaces.ContentTypes + "Default",
                        new XAttribute("Extension", "rels"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                    new XElement(Namespaces.ContentTypes + "Default",
                        new XAttribute("Extension", "xml"),
                        new XAttribute("ContentType", "application/xml")),
                    new XElement(Namespaces.ContentTypes + "Override",
                        new XAttribute("PartName", "/word/document.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml")),
                    new XElement(Namespaces.ContentTypes + "Override",
                        new XAttribute("PartName", "/word/styles.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml")),
                    new XElement(Namespaces.ContentTypes + "Override",
                        new XAttribute("PartName", "/word/settings.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml"))
                ));

            using var entry = archive.CreateEntry("[Content_Types].xml");
            using var stream = entry.Open();
            contentTypes.Save(stream);
        }

        private void CreateDocumentRelationships(ZipArchive archive)
        {
            var relationships = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Namespaces.Relationships + "Relationships",
                    new XAttribute(XNamespace.Xmlns + "r", Namespaces.Relationships),
                    new XElement(Namespaces.Relationships + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                        new XAttribute("Target", "styles.xml")),
                    new XElement(Namespaces.Relationships + "Relationship",
                        new XAttribute("Id", "rId2"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings"),
                        new XAttribute("Target", "settings.xml"))
                ));

            using var entry = archive.CreateEntry("word/_rels/document.xml.rels");
            using var stream = entry.Open();
            relationships.Save(stream);
        }

        private void CreateRootRelationships(ZipArchive archive)
        {
            var relationships = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Namespaces.Relationships + "Relationships",
                    new XAttribute(XNamespace.Xmlns + "r", Namespaces.Relationships),
                    new XElement(Namespaces.Relationships + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "word/document.xml"))
                ));

            using var entry = archive.CreateEntry("_rels/.rels");
            using var stream = entry.Open();
            relationships.Save(stream);
        }

        private void CreateAppProperties(ZipArchive archive)
        {
            var properties = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Namespaces.DocumentProperties + "Properties",
                    new XAttribute(XNamespace.Xmlns + "vt", Namespaces.VariantTypes),
                    new XElement(Namespaces.DocumentProperties + "Application", "OfficeSuite"),
                    new XElement(Namespaces.DocumentProperties + "DocSecurity", "0"),
                    new XElement(Namespaces.DocumentProperties + "ScaleCrop", "false"),
                    new XElement(Namespaces.DocumentProperties + "Company", ""),
                    new XElement(Namespaces.DocumentProperties + "LinksUpToDate", "false"),
                    new XElement(Namespaces.DocumentProperties + "SharedDoc", "false"),
                    new XElement(Namespaces.DocumentProperties + "HyperlinksChanged", "false"),
                    new XElement(Namespaces.DocumentProperties + "AppVersion", "16.0000")
                ));

            using var entry = archive.CreateEntry("docProps/app.xml");
            using var stream = entry.Open();
            properties.Save(stream);
        }

        private void CreateStyles(WordDocument document, ZipArchive archive)
        {
            var styles = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Namespaces.Word + "styles",
                    new XAttribute(Namespaces.Word + "docDefaults",
                        new XElement(Namespaces.Word + "rPrDefault",
                            new XElement(Namespaces.Word + "rPr",
                                new XElement(Namespaces.Word + "rFonts",
                                    new XAttribute(Namespaces.Word + "ascii", "Calibri"),
                                    new XAttribute(Namespaces.Word + "highAnsi", "Calibri"),
                                    new XAttribute(Namespaces.Word + "eastAsia", "Calibri"),
                                    new XAttribute(Namespaces.Word + "complexScript", "Calibri")),
                                new XElement(Namespaces.Word + "sz", new XAttribute(Namespaces.Word + "val", "22")),
                                new XElement(Namespaces.Word + "szCs", new XAttribute(Namespaces.Word + "val", "22"))
                            )
                        )
                    ),
                    CreateStyleElement("Normal", "Normal", "paragraph", "Normal")
                )
            );

            using var entry = archive.CreateEntry("word/styles.xml");
            using var stream = entry.Open();
            styles.Save(stream);
        }

        private XElement CreateStyleElement(string styleId, string styleName, string type, string basedOn)
        {
            return new XElement(Namespaces.Word + "style",
                new XAttribute(Namespaces.Word + "type", type),
                new XAttribute(Namespaces.Word + "styleId", styleId),
                new XElement(Namespaces.Word + "name", new XAttribute(Namespaces.Word + "val", styleName)),
                new XElement(Namespaces.Word + "basedOn", new XAttribute(Namespaces.Word + "val", basedOn))
            );
        }

        private void CreateDocument(WordDocument document, ZipArchive archive)
        {
            var body = new XElement(Namespaces.Word + "body");

            foreach (var paragraph in document.Paragraphs)
            {
                var paraElement = new XElement(Namespaces.Word + "p");

                foreach (var run in paragraph.Runs)
                {
                    var runElement = new XElement(Namespaces.Word + "r");

                    if (run.Format.Bold)
                        runElement.Add(new XElement(Namespaces.Word + "rPr",
                            new XElement(Namespaces.Word + "b")));

                    if (run.Format.Italic)
                        runElement.Add(new XElement(Namespaces.Word + "rPr",
                            new XElement(Namespaces.Word + "i")));

                    runElement.Add(new XElement(Namespaces.Word + "t",
                        new XAttribute(XNamespace.Xml + "space", "preserve"),
                        run.Text));

                    paraElement.Add(runElement);
                }

                body.Add(paraElement);
            }

            var sections = new XElement(Namespaces.Word + "sectPr",
                new XElement(Namespaces.Word + "pgSz",
                    new XAttribute(Namespaces.Word + "w", document.PageSettings.Width.ToString()),
                    new XAttribute(Namespaces.Word + "h", document.PageSettings.Height.ToString())),
                new XElement(Namespaces.Word + "pgMar",
                    new XAttribute(Namespaces.Word + "top", document.PageSettings.MarginTop.ToString()),
                    new XAttribute(Namespaces.Word + "right", document.PageSettings.MarginRight.ToString()),
                    new XAttribute(Namespaces.Word + "bottom", document.PageSettings.MarginBottom.ToString()),
                    new XAttribute(Namespaces.Word + "left", document.PageSettings.MarginLeft.ToString()),
                    new XAttribute(Namespaces.Word + "header", "720"),
                    new XAttribute(Namespaces.Word + "footer", "720"),
                    new XAttribute(Namespaces.Word + "gutter", "0")));

            body.Add(sections);

            var documentXml = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Namespaces.Word + "document",
                    new XAttribute(Namespaces.Word + "conformance", "transitional"),
                    body));

            using var entry = archive.CreateEntry("word/document.xml");
            using var stream = entry.Open();
            documentXml.Save(stream);
        }

        private void CreateSettings(ZipArchive archive)
        {
            var settings = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(Namespaces.Word + "settings",
                    new XElement(Namespaces.Word + "zoom",
                        new XAttribute(Namespaces.Word + "percent", "100")),
                    new XElement(Namespaces.Word + "defaultTabStop",
                        new XAttribute(Namespaces.Word + "val", "720")),
                    new XElement(Namespaces.Word + "compat")
                ));

            using var entry = archive.CreateEntry("word/settings.xml");
            using var stream = entry.Open();
            settings.Save(stream);
        }

        private void CreateHeadersFooters(WordDocument document, ZipArchive archive)
        {
        }

        public WordDocument ImportFromDocx(string filePath)
        {
            var document = new WordDocument();

            using var archive = ZipFile.OpenRead(filePath);
            var documentEntry = archive.GetEntry("word/document.xml");
            if (documentEntry != null)
            {
                using var stream = documentEntry.Open();
                var docXml = XDocument.Load(stream);
                ParseDocumentXml(docXml, document);
            }

            return document;
        }

        private void ParseDocumentXml(XDocument docXml, WordDocument document)
        {
            var body = docXml.Root?.Element(Namespaces.Word + "body");
            if (body == null) return;

            foreach (var paraElement in body.Elements(Namespaces.Word + "p"))
            {
                var paragraph = new Paragraph();

                foreach (var runElement in paraElement.Elements(Namespaces.Word + "r"))
                {
                    var run = new Run();
                    var textElement = runElement.Element(Namespaces.Word + "t");
                    if (textElement != null)
                    {
                        run.Text = textElement.Value;
                    }

                    var rPr = runElement.Element(Namespaces.Word + "rPr");
                    if (rPr != null)
                    {
                        var bold = rPr.Element(Namespaces.Word + "b");
                        if (bold != null) run.Format.Bold = true;

                        var italic = rPr.Element(Namespaces.Word + "i");
                        if (italic != null) run.Format.Italic = true;

                        var underline = rPr.Element(Namespaces.Word + "u");
                        if (underline != null) run.Format.Underline = true;

                        var color = rPr.Element(Namespaces.Word + "color");
                        if (color != null)
                        {
                            var colorValue = color.Attribute(Namespaces.Word + "val")?.Value;
                            if (colorValue != null)
                            {
                                try
                                {
                                    run.Format.FontColor = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(colorValue);
                                }
                                catch { }
                            }
                        }

                        var sz = rPr.Element(Namespaces.Word + "sz");
                        if (sz != null)
                        {
                            var sizeValue = sz.Attribute(Namespaces.Word + "val")?.Value;
                            if (sizeValue != null && double.TryParse(sizeValue, out double size))
                            {
                                run.Format.FontSize = size / 2;
                            }
                        }
                    }

                    paragraph.Runs.Add(run);
                }

                document.Paragraphs.Add(paragraph);
            }

            var sectPr = body.Element(Namespaces.Word + "sectPr");
            if (sectPr != null)
            {
                var pgSz = sectPr.Element(Namespaces.Word + "pgSz");
                if (pgSz != null)
                {
                    var width = pgSz.Attribute(Namespaces.Word + "w")?.Value;
                    var height = pgSz.Attribute(Namespaces.Word + "h")?.Value;

                    if (int.TryParse(width, out int w))
                        document.PageSettings.Width = w;
                    if (int.TryParse(height, out int h))
                        document.PageSettings.Height = h;
                }

                var pgMar = sectPr.Element(Namespaces.Word + "pgMar");
                if (pgMar != null)
                {
                    var top = pgMar.Attribute(Namespaces.Word + "top")?.Value;
                    var bottom = pgMar.Attribute(Namespaces.Word + "bottom")?.Value;
                    var left = pgMar.Attribute(Namespaces.Word + "left")?.Value;
                    var right = pgMar.Attribute(Namespaces.Word + "right")?.Value;

                    if (double.TryParse(top, out double t))
                        document.PageSettings.MarginTop = t;
                    if (double.TryParse(bottom, out double b))
                        document.PageSettings.MarginBottom = b;
                    if (double.TryParse(left, out double l))
                        document.PageSettings.MarginLeft = l;
                    if (double.TryParse(right, out double r))
                        document.PageSettings.MarginRight = r;
                }
            }
        }
    }

    public static class Namespaces
    {
        public static XNamespace Wp => "http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing";
        public static XNamespace A => "http://schemas.openxmlformats.org/drawingml/2006/main";
        public static XNamespace Pic => "http://schemas.openxmlformats.org/drawingml/2006/picture";
        public static XNamespace Wps => "http://schemas.microsoft.com/office/word/2010/wordprocessingShape";
        public static XNamespace Wpg => "http://schemas.microsoft.com/office/word/2010/wordprocessingGroup";
        public static XNamespace Wpc => "http://schemas.microsoft.com/office/word/2010/wordprocessingCanvas";
        public static XNamespace W10 => "urn:schemas-microsoft-com:vml";
        public static XNamespace Ovml => "urn:schemas-microsoft-com:office:office";
        public static XNamespace M => "http://schemas.openxmlformats.org/officeDocument/2006/math";
        public static XNamespace W14 => "http://schemas.microsoft.com/office/word/2010/wordml";
        public static XNamespace W15 => "http://schemas.microsoft.com/office/word/2012/wordml";
        public static XNamespace W16cex => "http://schemas.microsoft.com/office/word/2018/wordml/cex";
        public static XNamespace W16cid => "http://schemas.microsoft.com/office/word/2016/wordml/cid";
        public static XNamespace W16 => "http://schemas.microsoft.com/office/word/2018/wordml";
        public static XNamespace W16sdtdh => "http://schemas.microsoft.com/office/word/2020/wordml/sdtdatahash";
        public static XNamespace W16se => "http://schemas.microsoft.com/office/word/2015/wordml/symex";
        public static XNamespace V => "urn:schemas-microsoft-com:vml";
        public static XNamespace OfficeExtensions => "http://schemas.microsoft.com/office/2011/inc";

        public static XNamespace Word => "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        public static XNamespace Relationships => "http://schemas.openxmlformats.org/package/2006/relationships";
        public static XNamespace ContentTypes => "http://schemas.openxmlformats.org/package/2006/content-types";
        public static XNamespace DocumentProperties => "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";
        public static XNamespace VariantTypes => "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";
    }
}

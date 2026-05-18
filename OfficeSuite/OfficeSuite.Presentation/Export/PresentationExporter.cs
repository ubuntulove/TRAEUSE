using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using OfficeSuite.Presentation.Models;

namespace OfficeSuite.Presentation.Export
{
    public class PresentationExporter
    {
        public void ExportToPptx(PresentationDocument document, string filePath)
        {
            using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
            
            CreateContentTypes(archive);
            CreatePresentationRelationships(archive, document);
            CreateRootRelationships(archive);
            CreatePresentation(document, archive);
            CreatePresentationSettings(document, archive);
            CreateAppProperties(archive);
            CreateTableStyles(archive);

            int slideIndex = 1;
            foreach (var slide in document.Slides)
            {
                CreateSlide(slide, archive, slideIndex);
                CreateSlideRelationships(slide, archive, slideIndex);
                slideIndex++;
            }

            CreateSlideMasters(document, archive);
            CreateSlideLayouts(document, archive);
        }

        private void CreateContentTypes(ZipArchive archive)
        {
            var contentTypes = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.ContentTypes + "Types",
                    new XAttribute(XNamespace.Xmlns + "ct", NamespacesP.ContentTypes),
                    new XElement(NamespacesP.ContentTypes + "Default",
                        new XAttribute("Extension", "rels"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                    new XElement(NamespacesP.ContentTypes + "Default",
                        new XAttribute("Extension", "xml"),
                        new XAttribute("ContentType", "application/xml")),
                    new XElement(NamespacesP.ContentTypes + "Default",
                        new XAttribute("Extension", "jpeg"),
                        new XAttribute("ContentType", "image/jpeg")),
                    new XElement(NamespacesP.ContentTypes + "Default",
                        new XAttribute("Extension", "png"),
                        new XAttribute("ContentType", "image/png")),
                    new XElement(NamespacesP.ContentTypes + "Override",
                        new XAttribute("PartName", "/ppt/presentation.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.presentation.main+xml")),
                    new XElement(NamespacesP.ContentTypes + "Override",
                        new XAttribute("PartName", "/ppt/slideMasters/slideMaster1.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.slideMaster+xml")),
                    new XElement(NamespacesP.ContentTypes + "Override",
                        new XAttribute("PartName", "/ppt/slides/slide1.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.presentationml.slide+xml"))
                ));

            using var entry = archive.CreateEntry("[Content_Types].xml");
            using var stream = entry.Open();
            contentTypes.Save(stream);
        }

        private void CreatePresentationRelationships(ZipArchive archive, PresentationDocument document)
        {
            var relationships = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Relationships + "Relationships",
                    new XAttribute(XNamespace.Xmlns + "r", NamespacesP.Relationships),
                    new XElement(NamespacesP.Relationships + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "presentation.xml")),
                    new XElement(NamespacesP.Relationships + "Relationship",
                        new XAttribute("Id", "rId2"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideMaster"),
                        new XAttribute("Target", "slideMasters/slideMaster1.xml"))
                ));

            int slideId = 3;
            foreach (var slide in document.Slides)
            {
                relationships.Root?.Add(
                    new XElement(NamespacesP.Relationships + "Relationship",
                        new XAttribute("Id", $"rId{slideId}"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/slide"),
                        new XAttribute("Target", $"slides/slide{slideId - 2}.xml")));
                slideId++;
            }

            using var entry = archive.CreateEntry("ppt/_rels/presentation.xml.rels");
            using var stream = entry.Open();
            relationships.Save(stream);
        }

        private void CreateRootRelationships(ZipArchive archive)
        {
            var relationships = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Relationships + "Relationships",
                    new XAttribute(XNamespace.Xmlns + "r", NamespacesP.Relationships),
                    new XElement(NamespacesP.Relationships + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "ppt/presentation.xml"))
                ));

            using var entry = archive.CreateEntry("_rels/.rels");
            using var stream = entry.Open();
            relationships.Save(stream);
        }

        private void CreatePresentation(PresentationDocument document, ZipArchive archive)
        {
            var presentation = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Presentation + "presentation",
                    new XAttribute(NamespacesP.Presentation + "useAnsiLens", "0"),
                    new XAttribute(NamespacesP.Presentation + "useAdjHighlightingNesting", "0"),
                    new XAttribute(NamespacesP.Presentation + "embedStandardFonts", "0"),
                    new XAttribute(NamespacesP.Presentation + "addPane", "0"),
                    new XAttribute(NamespacesP.Presentation + "showFormatting", "0"),
                    new XAttribute(NamespacesP.Presentation + "sldSz",
                        new XAttribute("cx", document.Settings.SlideWidth.ToString()),
                        new XAttribute("cy", document.Settings.SlideHeight.ToString())),
                    new XAttribute(NamespacesP.Presentation + "notesSz",
                        new XAttribute("cx", "6858000"),
                        new XAttribute("cy", "9144000")),
                    new XElement(NamespacesP.Presentation + "sldIdList")
                ));

            var slideIdList = presentation.Root?.Element(NamespacesP.Presentation + "sldIdList");
            int slideId = 256;
            int relationshipId = 3;
            foreach (var slide in document.Slides)
            {
                slideIdList?.Add(
                    new XElement(NamespacesP.Presentation + "sldId",
                        new XAttribute("id", slideId.ToString()),
                        new XAttribute(NamespacesP.Relationships + "id", $"rId{relationshipId}")));
                slideId++;
                relationshipId++;
            }

            presentation.Root?.Add(
                new XElement(NamespacesP.Presentation + "sldClrMap",
                    new XAttribute("bg1", "lt1"),
                    new XAttribute("tx1", "dk1"),
                    new XAttribute("bg2", "lt2"),
                    new XAttribute("tx2", "dk2"),
                    new XAttribute("accent1", "accent1"),
                    new XAttribute("accent2", "accent2"),
                    new XAttribute("accent3", "accent3"),
                    new XAttribute("accent4", "accent4"),
                    new XAttribute("accent5", "accent5"),
                    new XAttribute("accent6", "accent6"),
                    new XAttribute("hlink", "hlink"),
                    new XAttribute("folHlink", "folHlink")));

            using var entry = archive.CreateEntry("ppt/presentation.xml");
            using var stream = entry.Open();
            presentation.Save(stream);
        }

        private void CreatePresentationSettings(PresentationDocument document, ZipArchive archive)
        {
            var settings = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Presentation + "presentationPr",
                    new XAttribute(NamespacesP.Presentation + "showSpecialPlacementsOnTitleBar", "0"),
                    new XAttribute(NamespacesP.Presentation + "showStatusbar", "1"),
                    new XAttribute(NamespacesP.Presentation + "showVScrollbar", "1"),
                    new XAttribute(NamespacesP.Presentation + "showHScrollbar", "1"),
                    new XAttribute(NamespacesP.Presentation + "rightToLeft", "0"),
                    new XAttribute(NamespacesP.Presentation + "slideViews", "1"),
                    new XAttribute(NamespacesP.Presentation + "masterSpouse", "1"),
                    new XAttribute(NamespacesP.Presentation + "zoom", "0"),
                    new XAttribute(NamespacesP.Presentation + "recommendReadOnly", "0"),
                    new XAttribute(NamespacesP.Presentation + "defaultShapeId", "1"),
                    new XAttribute(NamespacesP.Presentation + "saveSubsetFonts", "1"),
                    new XAttribute(NamespacesP.Presentation + "showInkAnnotation", "0"),
                    new XAttribute(NamespacesP.Presentation + "forceUpgrade", "0"),
                    new XAttribute(NamespacesP.Presentation + "removePersonalInformation", "0")
                ));

            using var entry = archive.CreateEntry("ppt/presentation.xml");
            using var stream = entry.Open();
            var existingDoc = XDocument.Load(stream);
            existingDoc.Root?.AddFirst(settings.Root);
        }

        private void CreateSlide(Slide slide, ZipArchive archive, int index)
        {
            var slideXml = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Presentation + "sld",
                    new XAttribute(NamespacesP.Presentation + "type", "main"),
                    new XAttribute(NamespacesP.Presentation + "saveSubsetFonts", "1"),
                    new XElement(NamespacesP.Presentation + "cSld",
                        new XElement(NamespacesP.Presentation + "stCustShow"),
                        new XElement(NamespacesP.Presentation + "nvGrpSpPr",
                            new XElement(NamespacesP.Presentation + "cNvPr",
                                new XAttribute("id", "1"),
                                new XAttribute("name", "")),
                            new XElement(NamespacesP.Presentation + "cNvGroupSpPr"),
                            new XElement(NamespacesP.Presentation + "nvPr")),
                        new XElement(NamespacesP.Presentation + "grpSpPr",
                            new XElement(NamespacesP.Presentation + "xfrm",
                                new XElement(NamespacesP.Presentation + "off",
                                    new XAttribute("x", "0"),
                                    new XAttribute("y", "0")),
                                new XElement(NamespacesP.Presentation + "ext",
                                    new XAttribute("cx", "0"),
                                    new XAttribute("cy", "0")),
                                new XElement(NamespacesP.Presentation + "chOff",
                                    new XAttribute("x", "0"),
                                    new XAttribute("y", "0")),
                                new XElement(NamespacesP.Presentation + "chExt",
                                    new XAttribute("cx", "0"),
                                    new XAttribute("cy", "0")))),
                        new XElement(NamespacesP.Presentation + "spTree",
                            new XElement(NamespacesP.Presentation + "nvGrpSpPr",
                                new XElement(NamespacesP.Presentation + "cNvPr",
                                    new XAttribute("id", "2"),
                                    new XAttribute("name", "")),
                                new XElement(NamespacesP.Presentation + "cNvGroupSpPr"),
                                new XElement(NamespacesP.Presentation + "nvPr")),
                            new XElement(NamespacesP.Presentation + "grpSpPr",
                                new XElement(NamespacesP.Presentation + "xfrm",
                                    new XElement(NamespacesP.Presentation + "off",
                                        new XAttribute("x", "0"),
                                        new XAttribute("y", "0")),
                                    new XElement(NamespacesP.Presentation + "ext",
                                        new XAttribute("cx", "0"),
                                        new XAttribute("cy", "0")),
                                    new XElement(NamespacesP.Presentation + "chOff",
                                        new XAttribute("x", "0"),
                                        new XAttribute("y", "0")),
                                    new XElement(NamespacesP.Presentation + "chExt",
                                        new XAttribute("cx", "0"),
                                        new XAttribute("cy", "0"))))))));

            var spTree = slideXml.Root?.Element(NamespacesP.Presentation + "cSld")
                .Element(NamespacesP.Presentation + "spTree");

            int shapeId = 3;
            foreach (var shape in slide.Shapes)
            {
                if (shape is TextShape textShape)
                {
                    var spElement = CreateTextShapeElement(textShape, shapeId);
                    spTree?.Add(spElement);
                }
                else
                {
                    var spElement = CreateGenericShapeElement(shape, shapeId);
                    spTree?.Add(spElement);
                }
                shapeId++;
            }

            using var entry = archive.CreateEntry($"ppt/slides/slide{index}.xml");
            using var stream = entry.Open();
            slideXml.Save(stream);
        }

        private XElement CreateTextShapeElement(TextShape shape, int shapeId)
        {
            return new XElement(NamespacesP.Presentation + "sp",
                new XElement(NamespacesP.Presentation + "nvSpPr",
                    new XElement(NamespacesP.Presentation + "cNvPr",
                        new XAttribute("id", shapeId.ToString()),
                        new XAttribute("name", shape.Name ?? $"Text {shapeId}")),
                    new XElement(NamespacesP.Presentation + "cNvSpPr"),
                    new XElement(NamespacesP.Presentation + "nvPr")),
                new XElement(NamespacesP.Presentation + "spPr",
                    new XElement(NamespacesP.Presentation + "xfrm",
                        new XElement(NamespacesP.Presentation + "off",
                            new XAttribute("x", ((int)shape.Left).ToString()),
                            new XAttribute("y", ((int)shape.Top).ToString())),
                        new XElement(NamespacesP.Presentation + "ext",
                            new XAttribute("cx", ((int)shape.Width).ToString()),
                            new XAttribute("cy", ((int)shape.Height).ToString()))),
                    new XElement(NamespacesP.Presentation + "prstGeom",
                        new XAttribute("prst", "rect"),
                        new XElement(NamespacesP.Presentation + "avLst"))),
                new XElement(NamespacesP.Presentation + "txBody",
                    new XElement(NamespacesP.Presentation + "bodyPr"),
                    new XElement(NamespacesP.Presentation + "lstStyle"),
                    new XElement(NamespacesP.Presentation + "p",
                        new XElement(NamespacesP.Presentation + "pPr",
                            new XAttribute("algn", "l")),
                        new XElement(NamespacesP.Presentation + "r",
                            new XElement(NamespacesP.Presentation + "rPr",
                                new XAttribute("lang", "en-US"),
                                new XElement(NamespacesP.Presentation + "sz",
                                    new XAttribute("val", ((int)(shape.TextFormat?.FontSize ?? 18) * 100).ToString()))),
                            new XElement(NamespacesP.Presentation + "t", shape.Text ?? "")))));
        }

        private XElement CreateGenericShapeElement(SlideShape shape, int shapeId)
        {
            return new XElement(NamespacesP.Presentation + "sp",
                new XElement(NamespacesP.Presentation + "nvSpPr",
                    new XElement(NamespacesP.Presentation + "cNvPr",
                        new XAttribute("id", shapeId.ToString()),
                        new XAttribute("name", shape.Name ?? $"Shape {shapeId}")),
                    new XElement(NamespacesP.Presentation + "cNvSpPr"),
                    new XElement(NamespacesP.Presentation + "nvPr")),
                new XElement(NamespacesP.Presentation + "spPr",
                    new XElement(NamespacesP.Presentation + "xfrm",
                        new XElement(NamespacesP.Presentation + "off",
                            new XAttribute("x", ((int)shape.Left).ToString()),
                            new XAttribute("y", ((int)shape.Top).ToString())),
                        new XElement(NamespacesP.Presentation + "ext",
                            new XAttribute("cx", ((int)shape.Width).ToString()),
                            new XAttribute("cy", ((int)shape.Height).ToString()))),
                    new XElement(NamespacesP.Presentation + "prstGeom",
                        new XAttribute("prst", "rect"),
                        new XElement(NamespacesP.Presentation + "avLst")))));
        }

        private void CreateSlideRelationships(Slide slide, ZipArchive archive, int index)
        {
            var relationships = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Relationships + "Relationships",
                    new XAttribute(XNamespace.Xmlns + "r", NamespacesP.Relationships),
                    new XElement(NamespacesP.Relationships + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/slideLayout"),
                        new XAttribute("Target", "../slideLayouts/slideLayout1.xml"))
                ));

            using var entry = archive.CreateEntry($"ppt/slides/_rels/slide{index}.xml.rels");
            using var stream = entry.Open();
            relationships.Save(stream);
        }

        private void CreateSlideMasters(PresentationDocument document, ZipArchive archive)
        {
            var slideMaster = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Presentation + "sldMaster",
                    new XElement(NamespacesP.Presentation + "cSld",
                        new XElement(NamespacesP.Presentation + "bg"),
                        new XElement(NamespacesP.Presentation + "spTree",
                            new XElement(NamespacesP.Presentation + "nvGrpSpPr",
                                new XElement(NamespacesP.Presentation + "cNvPr",
                                    new XAttribute("id", "1"),
                                    new XAttribute("name", "")),
                                new XElement(NamespacesP.Presentation + "cNvGroupSpPr"),
                                new XElement(NamespacesP.Presentation + "nvPr")),
                            new XElement(NamespacesP.Presentation + "grpSpPr",
                                new XElement(NamespacesP.Presentation + "xfrm",
                                    new XElement(NamespacesP.Presentation + "off",
                                        new XAttribute("x", "0"),
                                        new XAttribute("y", "0")),
                                    new XElement(NamespacesP.Presentation + "ext",
                                        new XAttribute("cx", "0"),
                                        new XAttribute("cy", "0")),
                                    new XElement(NamespacesP.Presentation + "chOff",
                                        new XAttribute("x", "0"),
                                        new XAttribute("y", "0")),
                                    new XElement(NamespacesP.Presentation + "chExt",
                                        new XAttribute("cx", "0"),
                                        new XAttribute("cy", "0"))))))),
                    new XElement(NamespacesP.Presentation + "sldLayoutIdLst")));

            using var entry = archive.CreateEntry("ppt/slideMasters/slideMaster1.xml");
            using var stream = entry.Open();
            slideMaster.Save(stream);
        }

        private void CreateSlideLayouts(PresentationDocument document, ZipArchive archive)
        {
            var slideLayout = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Presentation + "sldLayout",
                    new XAttribute(NamespacesP.Presentation + "type", "title"),
                    new XAttribute(NamespacesP.Presentation + "preserve", "1"),
                    new XElement(NamespacesP.Presentation + "cSld",
                        new XAttribute("name", "Title Slide"),
                        new XElement(NamespacesP.Presentation + "spTree",
                            new XElement(NamespacesP.Presentation + "nvGrpSpPr",
                                new XElement(NamespacesP.Presentation + "cNvPr",
                                    new XAttribute("id", "1"),
                                    new XAttribute("name", "")),
                                new XElement(NamespacesP.Presentation + "cNvGroupSpPr"),
                                new XElement(NamespacesP.Presentation + "nvPr")),
                            new XElement(NamespacesP.Presentation + "grpSpPr",
                                new XElement(NamespacesP.Presentation + "xfrm",
                                    new XElement(NamespacesP.Presentation + "off",
                                        new XAttribute("x", "0"),
                                        new XAttribute("y", "0")),
                                    new XElement(NamespacesP.Presentation + "ext",
                                        new XAttribute("cx", "0"),
                                        new XAttribute("cy", "0")),
                                    new XElement(NamespacesP.Presentation + "chOff",
                                        new XAttribute("x", "0"),
                                        new XAttribute("y", "0")),
                                    new XElement(NamespacesP.Presentation + "chExt",
                                        new XAttribute("cx", "0"),
                                        new XAttribute("cy", "0")))))))));

            using var entry = archive.CreateEntry("ppt/slideLayouts/slideLayout1.xml");
            using var stream = entry.Open();
            slideLayout.Save(stream);
        }

        private void CreateTableStyles(ZipArchive archive)
        {
            var tableStyles = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.Presentation + "tblStyleLst",
                    new XAttribute("def", "0")));

            using var entry = archive.CreateEntry("ppt/tableStyles.xml");
            using var stream = entry.Open();
            tableStyles.Save(stream);
        }

        private void CreateAppProperties(ZipArchive archive)
        {
            var properties = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesP.ExtendedProperties + "Properties",
                    new XAttribute(XNamespace.Xmlns + "vt", NamespacesP.VariantTypes),
                    new XElement(NamespacesP.ExtendedProperties + "Application", "OfficeSuite"),
                    new XElement(NamespacesP.ExtendedProperties + "DocSecurity", "0"),
                    new XElement(NamespacesP.ExtendedProperties + "ScaleCrop", "false"),
                    new XElement(NamespacesP.ExtendedProperties + "Company", ""),
                    new XElement(NamespacesP.ExtendedProperties + "LinksUpToDate", "false"),
                    new XElement(NamespacesP.ExtendedProperties + "SharedDoc", "false"),
                    new XElement(NamespacesP.ExtendedProperties + "HyperlinksChanged", "false"),
                    new XElement(NamespacesP.ExtendedProperties + "AppVersion", "16.0000")
                ));

            using var entry = archive.CreateEntry("docProps/app.xml");
            using var stream = entry.Open();
            properties.Save(stream);
        }
    }

    public static class NamespacesP
    {
        public static XNamespace Presentation => "http://schemas.openxmlformats.org/presentationml/2006/main";
        public static XNamespace Relationships => "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        public static XNamespace ContentTypes => "http://schemas.openxmlformats.org/package/2006/content-types";
        public static XNamespace ExtendedProperties => "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";
        public static XNamespace VariantTypes => "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";
        public static XNamespace Drawing => "http://schemas.openxmlformats.org/drawingml/2006/main";
        public static XNamespace A => Drawing;
    }
}

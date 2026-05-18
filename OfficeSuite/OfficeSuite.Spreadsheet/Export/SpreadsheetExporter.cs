using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Xml.Linq;
using OfficeSuite.Spreadsheet.Models;

namespace OfficeSuite.Spreadsheet.Export
{
    public class SpreadsheetExporter
    {
        private readonly Dictionary<int, CellReference> _cellCache = new Dictionary<int, CellReference>();

        public void ExportToXlsx(SpreadsheetDocument document, string filePath)
        {
            using var archive = ZipFile.Open(filePath, ZipArchiveMode.Create);
            
            CreateContentTypes(archive);
            CreateWorkbookRelationships(archive, document);
            CreateRootRelationships(archive);
            CreateWorkbook(document, archive);
            CreateSharedStrings(archive);
            CreateStyles(archive);

            int sheetIndex = 0;
            foreach (var worksheet in document.Worksheets)
            {
                CreateWorksheet(worksheet, archive, sheetIndex);
                sheetIndex++;
            }

            CreateAppProperties(archive);
        }

        private void CreateContentTypes(ZipArchive archive)
        {
            var contentTypes = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesS.ContentTypes + "Types",
                    new XAttribute(XNamespace.Xmlns + "ct", NamespacesS.ContentTypes),
                    new XElement(NamespacesS.ContentTypes + "Default",
                        new XAttribute("Extension", "rels"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-package.relationships+xml")),
                    new XElement(NamespacesS.ContentTypes + "Default",
                        new XAttribute("Extension", "xml"),
                        new XAttribute("ContentType", "application/xml")),
                    new XElement(NamespacesS.ContentTypes + "Override",
                        new XAttribute("PartName", "/xl/workbook.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml")),
                    new XElement(NamespacesS.ContentTypes + "Override",
                        new XAttribute("PartName", "/xl/worksheets/sheet1.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml")),
                    new XElement(NamespacesS.ContentTypes + "Override",
                        new XAttribute("PartName", "/xl/styles.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml")),
                    new XElement(NamespacesS.ContentTypes + "Override",
                        new XAttribute("PartName", "/xl/sharedStrings.xml"),
                        new XAttribute("ContentType", "application/vnd.openxmlformats-officedocument.spreadsheetml.sharedStrings+xml"))
                ));

            using var entry = archive.CreateEntry("[Content_Types].xml");
            using var stream = entry.Open();
            contentTypes.Save(stream);
        }

        private void CreateWorkbookRelationships(ZipArchive archive, SpreadsheetDocument document)
        {
            var relationships = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesS.Relationships + "Relationships",
                    new XAttribute(XNamespace.Xmlns + "r", NamespacesS.Relationships),
                    new XElement(NamespacesS.Relationships + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles"),
                        new XAttribute("Target", "styles.xml")),
                    new XElement(NamespacesS.Relationships + "Relationship",
                        new XAttribute("Id", "rId2"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/sharedStrings"),
                        new XAttribute("Target", "sharedStrings.xml"))
                ));

            int sheetId = 3;
            foreach (var worksheet in document.Worksheets)
            {
                relationships.Root?.Add(
                    new XElement(NamespacesS.Relationships + "Relationship",
                        new XAttribute("Id", $"rId{sheetId}"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet"),
                        new XAttribute("Target", $"worksheets/sheet{sheetId - 2}.xml")));
                sheetId++;
            }

            using var entry = archive.CreateEntry("xl/_rels/workbook.xml.rels");
            using var stream = entry.Open();
            relationships.Save(stream);
        }

        private void CreateRootRelationships(ZipArchive archive)
        {
            var relationships = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesS.Relationships + "Relationships",
                    new XAttribute(XNamespace.Xmlns + "r", NamespacesS.Relationships),
                    new XElement(NamespacesS.Relationships + "Relationship",
                        new XAttribute("Id", "rId1"),
                        new XAttribute("Type", "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument"),
                        new XAttribute("Target", "xl/workbook.xml"))
                ));

            using var entry = archive.CreateEntry("_rels/.rels");
            using var stream = entry.Open();
            relationships.Save(stream);
        }

        private void CreateWorkbook(SpreadsheetDocument document, ZipArchive archive)
        {
            var workbook = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesS.Spreadsheet + "workbook",
                    new XAttribute(NamespacesS.Spreadsheet + "docProps",
                        new XAttribute("xmlns:cp", "http://schemas.openxmlformats.org/package/2006/metadata/core-properties"),
                        new XAttribute("xmlns:dc", "http://purl.org/dc/elements/1.1/"),
                        new XAttribute("xmlns:dcterms", "http://purl.org/dc/terms/"),
                        new XAttribute("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance")),
                    new XElement(NamespacesS.Spreadsheet + "sheets")));

            var sheetsElement = workbook.Root?.Element(NamespacesS.Spreadsheet + "sheets");
            int sheetId = 1;
            foreach (var worksheet in document.Worksheets)
            {
                sheetsElement?.Add(
                    new XElement(NamespacesS.Spreadsheet + "sheet",
                        new XAttribute("name", worksheet.Name),
                        new XAttribute("sheetId", sheetId.ToString()),
                        new XAttribute(NamespacesS.Relationships + "id", $"rId{sheetId + 2}")));
                sheetId++;
            }

            using var entry = archive.CreateEntry("xl/workbook.xml");
            using var stream = entry.Open();
            workbook.Save(stream);
        }

        private void CreateSharedStrings(ZipArchive archive)
        {
            var sharedStrings = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesS.Spreadsheet + "sst",
                    new XAttribute("count", "0"),
                    new XAttribute("uniqueCount", "0")));

            using var entry = archive.CreateEntry("xl/sharedStrings.xml");
            using var stream = entry.Open();
            sharedStrings.Save(stream);
        }

        private void CreateStyles(ZipArchive archive)
        {
            var styles = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesS.Spreadsheet + "styleSheet",
                    new XElement(NamespacesS.Spreadsheet + "fonts",
                        new XAttribute("count", "1"),
                        new XElement(NamespacesS.Spreadsheet + "font",
                            new XElement(NamespacesS.Spreadsheet + "sz",
                                new XAttribute("val", "11")),
                            new XElement(NamespacesS.Spreadsheet + "name",
                                new XAttribute("val", "Calibri")))),
                    new XElement(NamespacesS.Spreadsheet + "fills",
                        new XAttribute("count", "2"),
                        new XElement(NamespacesS.Spreadsheet + "fill",
                            new XElement(NamespacesS.Spreadsheet + "patternFill",
                                new XAttribute("patternType", "none"))),
                        new XElement(NamespacesS.Spreadsheet + "fill",
                            new XElement(NamespacesS.Spreadsheet + "patternFill",
                                new XAttribute("patternType", "gray125")))),
                    new XElement(NamespacesS.Spreadsheet + "borders",
                        new XAttribute("count", "1"),
                        new XElement(NamespacesS.Spreadsheet + "border",
                            new XElement(NamespacesS.Spreadsheet + "left"),
                            new XElement(NamespacesS.Spreadsheet + "right"),
                            new XElement(NamespacesS.Spreadsheet + "top"),
                            new XElement(NamespacesS.Spreadsheet + "bottom"),
                            new XElement(NamespacesS.Spreadsheet + "diagonal"))),
                    new XElement(NamespacesS.Spreadsheet + "cellStyleXfs",
                        new XAttribute("count", "1"),
                        new XElement(NamespacesS.Spreadsheet + "xf")),
                    new XElement(NamespacesS.Spreadsheet + "cellXfs",
                        new XAttribute("count", "1"),
                        new XElement(NamespacesS.Spreadsheet + "xf",
                            new XAttribute("numFmtId", "0"),
                            new XAttribute("fontId", "0"),
                            new XAttribute("fillId", "0"),
                            new XAttribute("borderId", "0"))),
                    new XElement(NamespacesS.Spreadsheet + "dxfs",
                        new XAttribute("count", "0")),
                    new XElement(NamespacesS.Spreadsheet + "tableStyles",
                        new XAttribute("count", "0")),
                    new XElement(NamespacesS.Spreadsheet + "colors"),
                    new XElement(NamespacesS.Spreadsheet + "extLst")));

            using var entry = archive.CreateEntry("xl/styles.xml");
            using var stream = entry.Open();
            styles.Save(stream);
        }

        private void CreateWorksheet(Worksheet worksheet, ZipArchive archive, int index)
        {
            var worksheetXml = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesS.Spreadsheet + "worksheet",
                    new XElement(NamespacesS.Spreadsheet + "sheetViews",
                        new XElement(NamespacesS.Spreadsheet + "sheetView",
                            new XAttribute("workbookViewId", "0"),
                            new XElement(NamespacesS.Spreadsheet + "selection",
                                new XAttribute("activeCell", $"A1"),
                                new XAttribute("sqref", "A1"))))),
                    new XElement(NamespacesS.Spreadsheet + "sheetFormatPr",
                        new XAttribute("defaultRowHeight", "15")),
                    new XElement(NamespacesS.Spreadsheet + "cols"),
                    new XElement(NamespacesS.Spreadsheet + "sheetData")));

            var sheetData = worksheetXml.Root?.Element(NamespacesS.Spreadsheet + "sheetData");

            foreach (var cell in worksheet.Cells.Values)
            {
                var row = GetOrCreateRow(sheetData, cell.Row);
                var cellRef = CellReference.ToReference(cell.Row, cell.Column);

                XElement cellElement;
                switch (cell.ValueType)
                {
                    case CellValueType.Number:
                    case CellValueType.Formula:
                        cellElement = new XElement(NamespacesS.Spreadsheet + "c",
                            new XAttribute("r", cellRef),
                            new XElement(NamespacesS.Spreadsheet + "v",
                                cell.Value?.ToString() ?? "0"));
                        if (!string.IsNullOrEmpty(cell.Formula))
                        {
                            cellElement.AddFirst(
                                new XElement(NamespacesS.Spreadsheet + "f",
                                    cell.Formula.Substring(1)));
                        }
                        break;

                    case CellValueType.Text:
                        cellElement = new XElement(NamespacesS.Spreadsheet + "c",
                            new XAttribute("r", cellRef),
                            new XAttribute("t", "s"),
                            new XElement(NamespacesS.Spreadsheet + "v", "0"));
                        break;

                    case CellValueType.Boolean:
                        cellElement = new XElement(NamespacesS.Spreadsheet + "c",
                            new XAttribute("r", cellRef),
                            new XAttribute("t", "b"),
                            new XElement(NamespacesS.Spreadsheet + "v",
                                cell.Value?.ToString()?.ToLower() ?? "false"));
                        break;

                    case CellValueType.DateTime:
                        cellElement = new XElement(NamespacesS.Spreadsheet + "c",
                            new XAttribute("r", cellRef),
                            new XAttribute("t", "n"),
                            new XElement(NamespacesS.Spreadsheet + "v",
                                cell.Value?.ToString() ?? "0"));
                        break;

                    default:
                        cellElement = new XElement(NamespacesS.Spreadsheet + "c",
                            new XAttribute("r", cellRef),
                            new XElement(NamespacesS.Spreadsheet + "v",
                                cell.Value?.ToString() ?? ""));
                        break;
                }

                row.Add(cellElement);
            }

            using var entry = archive.CreateEntry($"xl/worksheets/sheet{index + 1}.xml");
            using var stream = entry.Open();
            worksheetXml.Save(stream);
        }

        private XElement GetOrCreateRow(XElement sheetData, int rowIndex)
        {
            var rowElement = sheetData.Elements(NamespacesS.Spreadsheet + "row")
                .FirstOrDefault(r => r.Attribute("r")?.Value == rowIndex.ToString());

            if (rowElement == null)
            {
                rowElement = new XElement(NamespacesS.Spreadsheet + "row",
                    new XAttribute("r", rowIndex.ToString()));
                InsertRowInOrder(sheetData, rowElement);
            }

            return rowElement;
        }

        private void InsertRowInOrder(XElement sheetData, XElement newRow)
        {
            var rows = sheetData.Elements(NamespacesS.Spreadsheet + "row").ToList();
            int newRowIndex = int.Parse(newRow.Attribute("r")?.Value ?? "0");

            XElement insertBefore = null;
            foreach (var row in rows)
            {
                int rowIndex = int.Parse(row.Attribute("r")?.Value ?? "0");
                if (rowIndex > newRowIndex)
                {
                    insertBefore = row;
                    break;
                }
            }

            if (insertBefore != null)
                newRow.AddBeforeSelf(insertBefore);
            else
                sheetData.Add(newRow);
        }

        private void CreateAppProperties(ZipArchive archive)
        {
            var properties = new XDocument(
                new XDeclaration("1.0", "UTF-8", "yes"),
                new XElement(NamespacesS.ExtendedProperties + "Properties",
                    new XAttribute(XNamespace.Xmlns + "vt", NamespacesS.VariantTypes),
                    new XElement(NamespacesS.ExtendedProperties + "Application", "OfficeSuite"),
                    new XElement(NamespacesS.ExtendedProperties + "DocSecurity", "0"),
                    new XElement(NamespacesS.ExtendedProperties + "ScaleCrop", "false"),
                    new XElement(NamespacesS.ExtendedProperties + "Company", ""),
                    new XElement(NamespacesS.ExtendedProperties + "LinksUpToDate", "false"),
                    new XElement(NamespacesS.ExtendedProperties + "SharedDoc", "false"),
                    new XElement(NamespacesS.ExtendedProperties + "HyperlinksChanged", "false"),
                    new XElement(NamespacesS.ExtendedProperties + "AppVersion", "16.0000")
                ));

            using var entry = archive.CreateEntry("docProps/app.xml");
            using var stream = entry.Open();
            properties.Save(stream);
        }

        public SpreadsheetDocument ImportFromXlsx(string filePath)
        {
            var document = new SpreadsheetDocument();
            document.Worksheets.Clear();

            using var archive = ZipFile.OpenRead(filePath);

            var workbookEntry = archive.GetEntry("xl/workbook.xml");
            if (workbookEntry != null)
            {
                using var stream = workbookEntry.Open();
                var workbookXml = XDocument.Load(stream);
                ParseWorkbookXml(workbookXml, document, archive);
            }

            return document;
        }

        private void ParseWorkbookXml(XDocument workbookXml, SpreadsheetDocument document, ZipArchive archive)
        {
            var sheets = workbookXml.Root?.Element(NamespacesS.Spreadsheet + "sheets");
            if (sheets == null) return;

            int sheetIndex = 1;
            foreach (var sheetElement in sheets.Elements(NamespacesS.Spreadsheet + "sheet"))
            {
                string name = sheetElement.Attribute("name")?.Value ?? $"Sheet{sheetIndex}";
                var worksheet = document.AddWorksheet(name);

                var sheetEntry = archive.GetEntry($"xl/worksheets/sheet{sheetIndex}.xml");
                if (sheetEntry != null)
                {
                    using var stream = sheetEntry.Open();
                    var sheetXml = XDocument.Load(stream);
                    ParseSheetXml(sheetXml, worksheet);
                }

                sheetIndex++;
            }
        }

        private void ParseSheetXml(XDocument sheetXml, Worksheet worksheet)
        {
            var sheetData = sheetXml.Root?.Element(NamespacesS.Spreadsheet + "sheetData");
            if (sheetData == null) return;

            foreach (var rowElement in sheetData.Elements(NamespacesS.Spreadsheet + "row"))
            {
                foreach (var cellElement in rowElement.Elements(NamespacesS.Spreadsheet + "c"))
                {
                    string cellRef = cellElement.Attribute("r")?.Value;
                    if (string.IsNullOrEmpty(cellRef)) continue;

                    var cellRefObj = CellReference.Parse(cellRef);
                    var cell = worksheet.GetOrCreateCell(cellRefObj.Row, cellRefObj.Column);

                    string cellType = cellElement.Attribute("t")?.Value;
                    var valueElement = cellElement.Element(NamespacesS.Spreadsheet + "v");
                    var formulaElement = cellElement.Element(NamespacesS.Spreadsheet + "f");

                    if (formulaElement != null)
                    {
                        cell.Formula = "=" + formulaElement.Value;
                    }

                    if (valueElement != null)
                    {
                        switch (cellType)
                        {
                            case "s":
                                cell.Value = valueElement.Value;
                                cell.ValueType = CellValueType.Text;
                                break;
                            case "b":
                                cell.Value = valueElement.Value == "1" || valueElement.Value?.ToLower() == "true";
                                cell.ValueType = CellValueType.Boolean;
                                break;
                            case "n":
                            case null:
                                if (double.TryParse(valueElement.Value, out double numValue))
                                {
                                    cell.Value = numValue;
                                    cell.ValueType = numValue == Math.Floor(numValue) ? CellValueType.Number : CellValueType.Number;
                                }
                                else
                                {
                                    cell.Value = valueElement.Value;
                                    cell.ValueType = CellValueType.Text;
                                }
                                break;
                            default:
                                cell.Value = valueElement.Value;
                                cell.ValueType = CellValueType.Text;
                                break;
                        }
                    }
                }
            }
        }
    }

    public static class NamespacesS
    {
        public static XNamespace Spreadsheet => "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        public static XNamespace Relationships => "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        public static XNamespace ContentTypes => "http://schemas.openxmlformats.org/package/2006/content-types";
        public static XNamespace ExtendedProperties => "http://schemas.openxmlformats.org/officeDocument/2006/extended-properties";
        public static XNamespace VariantTypes => "http://schemas.openxmlformats.org/officeDocument/2006/docPropsVTypes";
        public static XNamespace Drawing => "http://schemas.openxmlformats.org/drawingml/2006/spreadsheetDrawing";
        public static XNamespace Chart => "http://schemas.openxmlformats.org/drawingml/2006/chart";
    }
}

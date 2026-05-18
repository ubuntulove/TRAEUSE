using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OfficeSuite.Word.Formatting
{
    public class WordStyleManager
    {
        private readonly Dictionary<string, StyleDefinition> _styleDefinitions;
        private readonly List<StyleRule> _styleRules;
        private StyleSet _currentStyleSet;

        public WordStyleManager()
        {
            _styleDefinitions = new Dictionary<string, StyleDefinition>();
            _styleRules = new List<StyleRule>();
            _currentStyleSet = StyleSet.Normal;
            InitializeDefaultStyles();
        }

        private void InitializeDefaultStyles()
        {
            AddStyleDefinition(new StyleDefinition
            {
                Name = "Normal",
                Type = StyleType.Paragraph,
                BasedOn = null,
                NextStyle = "Normal",
                IsDefault = true,
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 0,
                    SpaceAfter = 8,
                    Color = "#000000"
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 1",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri Light",
                    FontSize = 28,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontWeight = FontWeight.Bold,
                    Color = "#1F4E79",
                    LineSpacing = 1.15,
                    SpaceBefore = 16,
                    SpaceAfter = 8,
                    KeepNext = true,
                    KeepLines = true
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 2",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri Light",
                    FontSize = 22,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontWeight = FontWeight.Bold,
                    Color = "#2E75B6",
                    LineSpacing = 1.15,
                    SpaceBefore = 14,
                    SpaceAfter = 6,
                    KeepNext = true,
                    KeepLines = true
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 3",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 16,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontWeight = FontWeight.Bold,
                    Color = "#1F4E79",
                    LineSpacing = 1.15,
                    SpaceBefore = 12,
                    SpaceAfter = 4,
                    KeepNext = true
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 4",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 14,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontWeight = FontWeight.Bold,
                    Color = "#1F4E79",
                    LineSpacing = 1.15,
                    SpaceBefore = 10,
                    SpaceAfter = 4
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 5",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 12,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontWeight = FontWeight.Bold,
                    Color = "#1F4E79",
                    LineSpacing = 1.15,
                    SpaceBefore = 8,
                    SpaceAfter = 4
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 6",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontWeight = FontWeight.Bold,
                    Color = "#1F4E79",
                    LineSpacing = 1.15,
                    SpaceBefore = 8,
                    SpaceAfter = 4
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Title",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri Light",
                    FontSize = 32,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontWeight = FontWeight.Bold,
                    Color = "#1F4E79",
                    LineSpacing = 1.15,
                    SpaceBefore = 0,
                    SpaceAfter = 12
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Subtitle",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 14,
                    FontSizeUnit = FontSizeUnit.Point,
                    Color = "#5A5A5A",
                    LineSpacing = 1.15,
                    SpaceBefore = 0,
                    SpaceAfter = 16
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Quote",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontStyle = FontStyle.Italic,
                    Color = "#5A5A5A",
                    LineSpacing = 1.5,
                    SpaceBefore = 6,
                    SpaceAfter = 6,
                    LeftIndent = 24,
                    RightIndent = 24,
                    BorderLeft = new BorderSettings { Width = 3, Color = "#2E75B6" }
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Intense Quote",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 14,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontStyle = FontStyle.Italic,
                    Color = "#1F4E79",
                    LineSpacing = 1.5,
                    SpaceBefore = 8,
                    SpaceAfter = 8,
                    LeftIndent = 24,
                    RightIndent = 24,
                    BorderLeft = new BorderSettings { Width = 3, Color = "#1F4E79" },
                    Shading = new ShadingSettings { Color = "#D6DCE4", Pattern = ShadingPattern.Clear }
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "List Paragraph",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 4,
                    SpaceAfter = 4,
                    LeftIndent = 36
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Caption",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 10,
                    FontSizeUnit = FontSizeUnit.Point,
                    FontStyle = FontStyle.Italic,
                    Color = "#5A5A5A",
                    LineSpacing = 1.15,
                    SpaceBefore = 4,
                    SpaceAfter = 8
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Footnote Text",
                Type = StyleType.Character,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 9,
                    FontSizeUnit = FontSizeUnit.Point,
                    Color = "#000000",
                    LineSpacing = 1.15
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Header",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 10,
                    FontSizeUnit = FontSizeUnit.Point,
                    Color = "#5A5A5A",
                    LineSpacing = 1.15
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Footer",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 10,
                    FontSizeUnit = FontSizeUnit.Point,
                    Color = "#5A5A5A",
                    LineSpacing = 1.15
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Hyperlink",
                Type = StyleType.Character,
                BasedOn = "Default Paragraph Font",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    Color = "#0563C1",
                    Underline = UnderlineStyle.Single
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Strong",
                Type = StyleType.Character,
                BasedOn = "Default Paragraph Font",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontWeight = FontWeight.Bold
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Emphasis",
                Type = StyleType.Character,
                BasedOn = "Default Paragraph Font",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontStyle = FontStyle.Italic
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Default Paragraph Font",
                Type = StyleType.Character,
                BasedOn = null,
                NextStyle = null,
                IsDefault = true
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Balloon Text",
                Type = StyleType.Character,
                BasedOn = "Default Paragraph Font",
                NextStyle = "Normal",
                Formatting = new TextFormatting
                {
                    FontFamily = "Segoe UI",
                    FontSize = 9,
                    FontSizeUnit = FontSizeUnit.Point,
                    Color = "#000000"
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Body Text",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 0,
                    SpaceAfter = 8
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Body Text 2",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 0,
                    SpaceAfter = 8,
                    LeftIndent = 24
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Body Text 3",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 0,
                    SpaceAfter = 8,
                    LeftIndent = 48
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "List Bullet",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 4,
                    SpaceAfter = 4,
                    LeftIndent = 36
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "List Bullet 2",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 4,
                    SpaceAfter = 4,
                    LeftIndent = 72
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "List Bullet 3",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 4,
                    SpaceAfter = 4,
                    LeftIndent = 108
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "List Number",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 4,
                    SpaceAfter = 4,
                    LeftIndent = 36
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "List Number 2",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 4,
                    SpaceAfter = 4,
                    LeftIndent = 72
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "List Number 3",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                NextStyle = "Normal",
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri",
                    FontSize = 11,
                    FontSizeUnit = FontSizeUnit.Point,
                    LineSpacing = 1.15,
                    SpaceBefore = 4,
                    SpaceAfter = 4,
                    LeftIndent = 108
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Table Normal",
                Type = StyleType.Table,
                BasedOn = null,
                NextStyle = null,
                IsQuickStyle = true,
                TableFormatting = new TableFormatting
                {
                    CellMargin = new TableCellMargins
                    {
                        Top = 85,
                        Bottom = 85,
                        Left = 108,
                        Right = 108
                    }
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Grid Table Light",
                Type = StyleType.Table,
                BasedOn = "Table Normal",
                NextStyle = null,
                IsQuickStyle = true,
                TableFormatting = new TableFormatting
                {
                    CellMargin = new TableCellMargins
                    {
                        Top = 85,
                        Bottom = 85,
                        Left = 108,
                        Right = 108
                    },
                    Borders = new TableBorders
                    {
                        Top = new BorderSettings { Width = 1, Color = "#FFFFFF", Style = BorderStyle.Single },
                        Bottom = new BorderSettings { Width = 1, Color = "#FFFFFF", Style = BorderStyle.Single },
                        Left = new BorderSettings { Width = 1, Color = "#FFFFFF", Style = BorderStyle.Single },
                        Right = new BorderSettings { Width = 1, Color = "#FFFFFF", Style = BorderStyle.Single },
                        InsideHorizontal = new BorderSettings { Width = 1, Color = "#DBDEE1", Style = BorderStyle.Single },
                        InsideVertical = new BorderSettings { Width = 1, Color = "#DBDEE1", Style = BorderStyle.Single }
                    },
                    RowBanding = 2
                }
            });
        }

        public void AddStyleDefinition(StyleDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrEmpty(definition.Name))
                throw new ArgumentException("Style name cannot be null or empty", nameof(definition));

            _styleDefinitions[definition.Name] = definition;
            OnStyleAdded?.Invoke(this, new StyleEventArgs(definition));
        }

        public StyleDefinition GetStyle(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return _styleDefinitions.TryGetValue(name, out var style) ? style : null;
        }

        public IEnumerable<StyleDefinition> GetAllStyles(StyleType? type = null)
        {
            var styles = _styleDefinitions.Values.AsEnumerable();
            if (type.HasValue)
                styles = styles.Where(s => s.Type == type.Value);
            return styles.OrderBy(s => s.Name);
        }

        public IEnumerable<StyleDefinition> GetQuickStyles()
        {
            return _styleDefinitions.Values
                .Where(s => s.IsQuickStyle)
                .OrderBy(s => s.Name);
        }

        public bool RemoveStyle(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            var style = GetStyle(name);
            if (style == null || style.IsDefault)
                return false;

            _styleDefinitions.Remove(name);
            OnStyleRemoved?.Invoke(this, new StyleEventArgs(style));
            return true;
        }

        public void UpdateStyle(StyleDefinition definition)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (string.IsNullOrEmpty(definition.Name))
                throw new ArgumentException("Style name cannot be null or empty", nameof(definition));

            if (!_styleDefinitions.ContainsKey(definition.Name))
                throw new InvalidOperationException($"Style '{definition.Name}' does not exist");

            _styleDefinitions[definition.Name] = definition;
            OnStyleChanged?.Invoke(this, new StyleEventArgs(definition));
        }

        public TextFormatting GetEffectiveFormatting(string styleName)
        {
            var style = GetStyle(styleName);
            if (style == null)
                return new TextFormatting();

            var formatting = new TextFormatting();

            if (style.BasedOn != null)
            {
                var parentFormatting = GetEffectiveFormatting(style.BasedOn);
                formatting.Merge(parentFormatting);
            }

            if (style.Formatting != null)
            {
                formatting.Merge(style.Formatting);
            }

            return formatting;
        }

        public void ApplyStyleSet(StyleSet styleSet)
        {
            _currentStyleSet = styleSet;
            ClearCustomStyles();

            switch (styleSet)
            {
                case StyleSet.Normal:
                    InitializeDefaultStyles();
                    break;
                case StyleSet.Simple:
                    CreateSimpleStyleSet();
                    break;
                case StyleSet.Elegant:
                    CreateElegantStyleSet();
                    break;
                case StyleSet.Professional:
                    CreateProfessionalStyleSet();
                    break;
                case StyleSet.Casual:
                    CreateCasualStyleSet();
                    break;
            }

            OnStyleSetChanged?.Invoke(this, new StyleSetChangedEventArgs(styleSet));
        }

        private void CreateSimpleStyleSet()
        {
            AddStyleDefinition(new StyleDefinition
            {
                Name = "Normal",
                Type = StyleType.Paragraph,
                IsDefault = true,
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Arial",
                    FontSize = 11,
                    LineSpacing = 1.5
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 1",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Arial",
                    FontSize = 24,
                    FontWeight = FontWeight.Bold
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 2",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Arial",
                    FontSize = 18,
                    FontWeight = FontWeight.Bold
                }
            });
        }

        private void CreateElegantStyleSet()
        {
            AddStyleDefinition(new StyleDefinition
            {
                Name = "Normal",
                Type = StyleType.Paragraph,
                IsDefault = true,
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Times New Roman",
                    FontSize = 12,
                    LineSpacing = 1.15
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 1",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Georgia",
                    FontSize = 26,
                    FontWeight = FontWeight.Bold,
                    Color = "#4A4A4A"
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 2",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Georgia",
                    FontSize = 18,
                    FontWeight = FontWeight.Bold,
                    Color = "#4A4A4A"
                }
            });
        }

        private void CreateProfessionalStyleSet()
        {
            AddStyleDefinition(new StyleDefinition
            {
                Name = "Normal",
                Type = StyleType.Paragraph,
                IsDefault = true,
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Cambria",
                    FontSize = 12,
                    LineSpacing = 1.15
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 1",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri Light",
                    FontSize = 28,
                    FontWeight = FontWeight.Bold,
                    Color = "#1F4E79"
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 2",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Calibri Light",
                    FontSize = 20,
                    FontWeight = FontWeight.Bold,
                    Color = "#1F4E79"
                }
            });
        }

        private void CreateCasualStyleSet()
        {
            AddStyleDefinition(new StyleDefinition
            {
                Name = "Normal",
                Type = StyleType.Paragraph,
                IsDefault = true,
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Comic Sans MS",
                    FontSize = 11,
                    LineSpacing = 1.5
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 1",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Comic Sans MS",
                    FontSize = 24,
                    FontWeight = FontWeight.Bold
                }
            });

            AddStyleDefinition(new StyleDefinition
            {
                Name = "Heading 2",
                Type = StyleType.Paragraph,
                BasedOn = "Normal",
                IsQuickStyle = true,
                Formatting = new TextFormatting
                {
                    FontFamily = "Comic Sans MS",
                    FontSize = 16,
                    FontWeight = FontWeight.Bold
                }
            });
        }

        private void ClearCustomStyles()
        {
            var defaultStyles = new[] { "Normal", "Default Paragraph Font" };
            var stylesToRemove = _styleDefinitions.Keys
                .Where(k => !defaultStyles.Contains(k))
                .ToList();

            foreach (var styleName in stylesToRemove)
            {
                _styleDefinitions.Remove(styleName);
            }
        }

        public void AddStyleRule(StyleRule rule)
        {
            if (rule == null)
                throw new ArgumentNullException(nameof(rule));

            _styleRules.Add(rule);
        }

        public IEnumerable<StyleRule> GetStyleRules(StyleRuleType? type = null)
        {
            var rules = _styleRules.AsEnumerable();
            if (type.HasValue)
                rules = rules.Where(r => r.Type == type.Value);
            return rules;
        }

        public void ClearStyleRules()
        {
            _styleRules.Clear();
        }

        public string ExportStyles(string filePath, StyleExportFormat format)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            XDocument doc;
            switch (format)
            {
                case StyleExportFormat.XML:
                    doc = ExportStylesToXml();
                    doc.Save(filePath);
                    break;
                case StyleExportFormat.Html:
                    var html = ExportStylesToHtml();
                    File.WriteAllText(filePath, html);
                    break;
                case StyleExportFormat.Css:
                    var css = ExportStylesToCss();
                    File.WriteAllText(filePath, css);
                    break;
                default:
                    throw new ArgumentException($"Unknown export format: {format}", nameof(format));
            }

            return filePath;
        }

        private XDocument ExportStylesToXml()
        {
            var doc = new XDocument(
                new XElement("Styles",
                    new XAttribute("Version", "1.0"),
                    new XAttribute("Generated", DateTime.UtcNow.ToString("o")),
                    new XElement("StyleDefinitions",
                        _styleDefinitions.Values.Select(s => new XElement("Style",
                            new XAttribute("Name", s.Name),
                            new XAttribute("Type", s.Type.ToString()),
                            new XElement("BasedOn", s.BasedOn),
                            new XElement("NextStyle", s.NextStyle),
                            new XElement("IsQuickStyle", s.IsQuickStyle),
                            new XElement("IsDefault", s.IsDefault),
                            s.Formatting?.ToXml() ?? new XElement("Formatting")
                        ))
                    )
                )
            );
            return doc;
        }

        private string ExportStylesToHtml()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html>");
            sb.AppendLine("<html><head><title>Document Styles</title>");
            sb.AppendLine("<style>");

            foreach (var style in _styleDefinitions.Values.Where(s => s.Formatting != null))
            {
                sb.AppendLine($"h1, h2, h3 {{ margin: 0; padding: 0; }}");
                sb.AppendLine($".{style.Name.ToLower().Replace(" ", "-")} {{");
                if (style.Formatting.FontFamily != null)
                    sb.AppendLine($"  font-family: {style.Formatting.FontFamily};");
                if (style.Formatting.FontSize > 0)
                    sb.AppendLine($"  font-size: {style.Formatting.FontSize}pt;");
                if (style.Formatting.FontWeight != FontWeight.Normal)
                    sb.AppendLine($"  font-weight: bold;");
                if (style.Formatting.FontStyle != FontStyle.Normal)
                    sb.AppendLine($"  font-style: italic;");
                if (style.Formatting.Color != null)
                    sb.AppendLine($"  color: {style.Formatting.Color};");
                if (style.Formatting.LineSpacing > 0)
                    sb.AppendLine($"  line-height: {style.Formatting.LineSpacing};");
                sb.AppendLine("}");
            }

            sb.AppendLine("</style></head><body>");
            sb.AppendLine("<h1>Document Styles</h1>");

            foreach (var style in _styleDefinitions.Values)
            {
                sb.AppendLine($"<div class=\"{style.Name.ToLower().Replace(" ", "-")}\">");
                sb.AppendLine($"<h2>{style.Name}</h2>");
                sb.AppendLine($"<p>Type: {style.Type}</p>");
                sb.AppendLine($"</div>");
            }

            sb.AppendLine("</body></html>");
            return sb.ToString();
        }

        private string ExportStylesToCss()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"/* Generated on {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC */");
            sb.AppendLine();

            foreach (var style in _styleDefinitions.Values.Where(s => s.Formatting != null))
            {
                sb.AppendLine($"/* {style.Name} */");
                sb.AppendLine($".{SanitizeCssClassName(style.Name)} {{");
                if (style.Formatting.FontFamily != null)
                    sb.AppendLine($"    font-family: '{style.Formatting.FontFamily}', sans-serif;");
                if (style.Formatting.FontSize > 0)
                    sb.AppendLine($"    font-size: {style.Formatting.FontSize}pt;");
                if (style.Formatting.FontWeight != FontWeight.Normal)
                    sb.AppendLine($"    font-weight: bold;");
                if (style.Formatting.FontStyle != FontStyle.Normal)
                    sb.AppendLine($"    font-style: italic;");
                if (style.Formatting.Underline != UnderlineStyle.None)
                    sb.AppendLine($"    text-decoration: underline;");
                if (style.Formatting.Color != null)
                    sb.AppendLine($"    color: {style.Formatting.Color};");
                if (style.Formatting.LineSpacing > 0)
                    sb.AppendLine($"    line-height: {style.Formatting.LineSpacing};");
                if (style.Formatting.SpaceBefore > 0)
                    sb.AppendLine($"    margin-top: {style.Formatting.SpaceBefore}px;");
                if (style.Formatting.SpaceAfter > 0)
                    sb.AppendLine($"    margin-bottom: {style.Formatting.SpaceAfter}px;");
                if (style.Formatting.LeftIndent > 0)
                    sb.AppendLine($"    margin-left: {style.Formatting.LeftIndent}px;");
                if (style.Formatting.RightIndent > 0)
                    sb.AppendLine($"    margin-right: {style.Formatting.RightIndent}px;");
                if (style.Formatting.TextAlignment != HorizontalAlignment.Left)
                    sb.AppendLine($"    text-align: {style.Formatting.TextAlignment.ToString().ToLower()};");
                sb.AppendLine("}");
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private string SanitizeCssClassName(string name)
        {
            return Regex.Replace(name.ToLower(), @"[^a-z0-9\-]", "-");
        }

        public void ImportStyles(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var extension = Path.GetExtension(filePath).ToLower();
            switch (extension)
            {
                case ".xml":
                    ImportStylesFromXml(filePath);
                    break;
                case ".css":
                    ImportStylesFromCss(filePath);
                    break;
                default:
                    throw new ArgumentException($"Unsupported file format: {extension}", nameof(filePath));
            }
        }

        private void ImportStylesFromXml(string filePath)
        {
            var doc = XDocument.Load(filePath);
            var styleElements = doc.Descendants("Style");

            foreach (var element in styleElements)
            {
                var name = element.Attribute("Name")?.Value;
                if (string.IsNullOrEmpty(name))
                    continue;

                var typeStr = element.Attribute("Type")?.Value;
                var type = Enum.TryParse<StyleType>(typeStr, out var parsedType) ? parsedType : StyleType.Character;

                var formattingElement = element.Element("Formatting");
                TextFormatting formatting = null;
                if (formattingElement != null)
                {
                    formatting = TextFormatting.FromXml(formattingElement);
                }

                AddStyleDefinition(new StyleDefinition
                {
                    Name = name,
                    Type = type,
                    BasedOn = element.Element("BasedOn")?.Value,
                    NextStyle = element.Element("NextStyle")?.Value,
                    IsQuickStyle = bool.TryParse(element.Element("IsQuickStyle")?.Value, out var qs) && qs,
                    IsDefault = bool.TryParse(element.Element("IsDefault")?.Value, out var def) && def,
                    Formatting = formatting
                });
            }
        }

        private void ImportStylesFromCss(string filePath)
        {
            var css = File.ReadAllText(filePath);
            var rulePattern = new Regex(@"\.([a-z0-9\-]+)\s*\{([^}]*)\}", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            var matches = rulePattern.Matches(css);

            foreach (Match match in matches)
            {
                var className = match.Groups[1].Value;
                var declarations = match.Groups[2].Value;

                var formatting = new TextFormatting();
                var propertyPattern = new Regex(@"([a-z\-]+):\s*([^;]+);?", RegexOptions.IgnoreCase);
                var propertyMatches = propertyPattern.Matches(declarations);

                foreach (Match propMatch in propertyMatches)
                {
                    var property = propMatch.Groups[1].Value;
                    var value = propMatch.Groups[2].Value.Trim();

                    switch (property)
                    {
                        case "font-family":
                            formatting.FontFamily = value.Trim('\'');
                            break;
                        case "font-size":
                            formatting.FontSize = ParseFontSize(value);
                            break;
                        case "font-weight":
                            formatting.FontWeight = value == "bold" ? FontWeight.Bold : FontWeight.Normal;
                            break;
                        case "font-style":
                            formatting.FontStyle = value == "italic" ? FontStyle.Italic : FontStyle.Normal;
                            break;
                        case "color":
                            formatting.Color = value;
                            break;
                        case "line-height":
                            formatting.LineSpacing = double.TryParse(value, out var ls) ? ls : 1.15;
                            break;
                        case "text-align":
                            formatting.TextAlignment = Enum.TryParse<HorizontalAlignment>(value, true, out var align) ? align : HorizontalAlignment.Left;
                            break;
                        case "margin-top":
                            formatting.SpaceBefore = ParsePixelValue(value);
                            break;
                        case "margin-bottom":
                            formatting.SpaceAfter = ParsePixelValue(value);
                            break;
                        case "margin-left":
                            formatting.LeftIndent = ParsePixelValue(value);
                            break;
                        case "margin-right":
                            formatting.RightIndent = ParsePixelValue(value);
                            break;
                    }
                }

                AddStyleDefinition(new StyleDefinition
                {
                    Name = className,
                    Type = StyleType.Paragraph,
                    Formatting = formatting
                });
            }
        }

        private double ParseFontSize(string value)
        {
            value = value.Trim();
            if (value.EndsWith("pt"))
                value = value.Substring(0, value.Length - 2);
            if (double.TryParse(value, out var size))
                return size;
            return 11;
        }

        private double ParsePixelValue(string value)
        {
            value = value.Trim();
            if (value.EndsWith("px"))
                value = value.Substring(0, value.Length - 2);
            if (double.TryParse(value, out var px))
                return px;
            return 0;
        }

        public event EventHandler<StyleEventArgs> OnStyleAdded;
        public event EventHandler<StyleEventArgs> OnStyleRemoved;
        public event EventHandler<StyleEventArgs> OnStyleChanged;
        public event EventHandler<StyleSetChangedEventArgs> OnStyleSetChanged;
    }

    public class StyleDefinition
    {
        public string Name { get; set; }
        public StyleType Type { get; set; }
        public string BasedOn { get; set; }
        public string NextStyle { get; set; }
        public bool IsQuickStyle { get; set; }
        public bool IsDefault { get; set; }
        public TextFormatting Formatting { get; set; }
        public TableFormatting TableFormatting { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string UiName { get; set; }
        public string Aliases { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Modified { get; set; } = DateTime.UtcNow;
    }

    public enum StyleType
    {
        Character,
        Paragraph,
        Table,
        List
    }

    public enum StyleSet
    {
        Normal,
        Simple,
        Elegant,
        Professional,
        Casual,
        Custom
    }

    public enum StyleExportFormat
    {
        XML,
        Html,
        Css
    }

    public class StyleRule
    {
        public string Pattern { get; set; }
        public StyleRuleType Type { get; set; }
        public string StyleName { get; set; }
        public bool AutoApply { get; set; }
    }

    public enum StyleRuleType
    {
        Conditional,
        AutoFormat,
        QuickStyle
    }

    public class StyleEventArgs : EventArgs
    {
        public StyleDefinition Style { get; }

        public StyleEventArgs(StyleDefinition style)
        {
            Style = style;
        }
    }

    public class StyleSetChangedEventArgs : EventArgs
    {
        public StyleSet NewStyleSet { get; }

        public StyleSetChangedEventArgs(StyleSet styleSet)
        {
            NewStyleSet = styleSet;
        }
    }

    public class TextFormatting
    {
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public FontSizeUnit FontSizeUnit { get; set; } = FontSizeUnit.Point;
        public System.Windows.FontWeight FontWeight { get; set; } = System.Windows.FontWeight.Normal;
        public System.Windows.FontStyle FontStyle { get; set; } = System.Windows.FontStyle.Normal;
        public UnderlineStyle Underline { get; set; } = UnderlineStyle.None;
        public string Color { get; set; }
        public double LineSpacing { get; set; } = 1.15;
        public double SpaceBefore { get; set; }
        public double SpaceAfter { get; set; }
        public double LeftIndent { get; set; }
        public double RightIndent { get; set; }
        public HorizontalAlignment TextAlignment { get; set; } = HorizontalAlignment.Left;
        public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
        public bool KeepNext { get; set; }
        public bool KeepLines { get; set; }
        public bool WidowControl { get; set; } = true;
        public BorderSettings BorderLeft { get; set; }
        public BorderSettings BorderRight { get; set; }
        public BorderSettings BorderTop { get; set; }
        public BorderSettings BorderBottom { get; set; }
        public ShadingSettings Shading { get; set; }

        public void Merge(TextFormatting other)
        {
            if (other == null) return;

            if (other.FontFamily != null) FontFamily = other.FontFamily;
            if (other.FontSize > 0) FontSize = other.FontSize;
            FontSizeUnit = other.FontSizeUnit;
            if (other.FontWeight != System.Windows.FontWeight.Normal) FontWeight = other.FontWeight;
            if (other.FontStyle != System.Windows.FontStyle.Normal) FontStyle = other.FontStyle;
            if (other.Underline != UnderlineStyle.None) Underline = other.Underline;
            if (other.Color != null) Color = other.Color;
            if (other.LineSpacing > 0) LineSpacing = other.LineSpacing;
            if (other.SpaceBefore > 0) SpaceBefore = other.SpaceBefore;
            if (other.SpaceAfter > 0) SpaceAfter = other.SpaceAfter;
            if (other.LeftIndent > 0) LeftIndent = other.LeftIndent;
            if (other.RightIndent > 0) RightIndent = other.RightIndent;
            TextAlignment = other.TextAlignment;
            VerticalAlignment = other.VerticalAlignment;
            KeepNext = other.KeepNext || KeepNext;
            KeepLines = other.KeepLines || KeepLines;
            WidowControl = other.WidowControl || WidowControl;

            if (other.BorderLeft != null) BorderLeft = other.BorderLeft;
            if (other.BorderRight != null) BorderRight = other.BorderRight;
            if (other.BorderTop != null) BorderTop = other.BorderTop;
            if (other.BorderBottom != null) BorderBottom = other.BorderBottom;
            if (other.Shading != null) Shading = other.Shading;
        }

        public XElement ToXml()
        {
            var element = new XElement("Formatting");
            if (FontFamily != null) element.Add(new XElement("FontFamily", FontFamily));
            if (FontSize > 0) element.Add(new XElement("FontSize", FontSize));
            element.Add(new XElement("FontSizeUnit", FontSizeUnit));
            element.Add(new XElement("FontWeight", FontWeight));
            element.Add(new XElement("FontStyle", FontStyle));
            element.Add(new XElement("Underline", Underline));
            if (Color != null) element.Add(new XElement("Color", Color));
            if (LineSpacing > 0) element.Add(new XElement("LineSpacing", LineSpacing));
            if (SpaceBefore > 0) element.Add(new XElement("SpaceBefore", SpaceBefore));
            if (SpaceAfter > 0) element.Add(new XElement("SpaceAfter", SpaceAfter));
            if (LeftIndent > 0) element.Add(new XElement("LeftIndent", LeftIndent));
            if (RightIndent > 0) element.Add(new XElement("RightIndent", RightIndent));
            element.Add(new XElement("TextAlignment", TextAlignment));
            element.Add(new XElement("VerticalAlignment", VerticalAlignment));
            element.Add(new XElement("KeepNext", KeepNext));
            element.Add(new XElement("KeepLines", KeepLines));
            element.Add(new XElement("WidowControl", WidowControl));
            return element;
        }

        public static TextFormatting FromXml(XElement element)
        {
            var formatting = new TextFormatting();
            formatting.FontFamily = element.Element("FontFamily")?.Value;
            formatting.FontSize = double.TryParse(element.Element("FontSize")?.Value, out var fs) ? fs : 0;
            if (Enum.TryParse<FontSizeUnit>(element.Element("FontSizeUnit")?.Value, out var unit))
                formatting.FontSizeUnit = unit;
            if (Enum.TryParse<System.Windows.FontWeight>(element.Element("FontWeight")?.Value, out var weight))
                formatting.FontWeight = weight;
            if (Enum.TryParse<System.Windows.FontStyle>(element.Element("FontStyle")?.Value, out var style))
                formatting.FontStyle = style;
            if (Enum.TryParse<UnderlineStyle>(element.Element("Underline")?.Value, out var underline))
                formatting.Underline = underline;
            formatting.Color = element.Element("Color")?.Value;
            formatting.LineSpacing = double.TryParse(element.Element("LineSpacing")?.Value, out var ls) ? ls : 1.15;
            formatting.SpaceBefore = double.TryParse(element.Element("SpaceBefore")?.Value, out var sb) ? sb : 0;
            formatting.SpaceAfter = double.TryParse(element.Element("SpaceAfter")?.Value, out var sa) ? sa : 0;
            formatting.LeftIndent = double.TryParse(element.Element("LeftIndent")?.Value, out var li) ? li : 0;
            formatting.RightIndent = double.TryParse(element.Element("RightIndent")?.Value, out var ri) ? ri : 0;
            if (Enum.TryParse<HorizontalAlignment>(element.Element("TextAlignment")?.Value, out var align))
                formatting.TextAlignment = align;
            if (Enum.TryParse<VerticalAlignment>(element.Element("VerticalAlignment")?.Value, out var vAlign))
                formatting.VerticalAlignment = vAlign;
            formatting.KeepNext = bool.TryParse(element.Element("KeepNext")?.Value, out var kn) && kn;
            formatting.KeepLines = bool.TryParse(element.Element("KeepLines")?.Value, out var kl) && kl;
            formatting.WidowControl = bool.TryParse(element.Element("WidowControl")?.Value, out var wc) && wc;
            return formatting;
        }
    }

    public enum FontSizeUnit
    {
        Point,
        Pica,
        Inch,
        Centimeter,
        Millimeter
    }

    public enum UnderlineStyle
    {
        None,
        Single,
        Double,
        Thick,
        Dotted,
        DottedHeavy,
        Dash,
        DashedHeavy,
        LongDash,
        LongDashHeavy,
        DotDash,
        DashDotHeavy,
        DotDotDash,
        DashDotDotHeavy,
        Wave,
        WavyHeavy,
        WavyDouble,
        Words
    }

    public enum HorizontalAlignment
    {
        Left,
        Center,
        Right,
        Justify
    }

    public enum VerticalAlignment
    {
        Top,
        Center,
        Bottom,
        Justify
    }

    public enum FontWeight
    {
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        Normal = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900
    }

    public class BorderSettings
    {
        public double Width { get; set; }
        public string Color { get; set; }
        public BorderStyle Style { get; set; } = BorderStyle.Single;
        public double Spacing { get; set; }
        public bool Shadow { get; set; }
    }

    public enum BorderStyle
    {
        None,
        Single,
        Double,
        Triple,
        Dotted,
        Dashed,
        Hairline,
        Thick,
        DoubleHairline,
        ThickHairline
    }

    public class ShadingSettings
    {
        public string Color { get; set; }
        public ShadingPattern Pattern { get; set; } = ShadingPattern.Clear;
        public double FillPattern { get; set; }
    }

    public enum ShadingPattern
    {
        Clear,
        Solid,
        Horizontal,
        Vertical,
        DiagonalDown,
        DiagonalUp,
        DiagonalCross,
        Cross,
        DarkHorizontal,
        DarkVertical,
        DarkDiagonalDown,
        DarkDiagonalUp,
        DarkDiagonalCross,
        DarkCross,
        Horizontal,
        Vertical
    }

    public class TableFormatting
    {
        public TableCellMargins CellMargin { get; set; }
        public TableBorders Borders { get; set; }
        public TableRowFormatting DefaultRowFormatting { get; set; }
        public int RowBanding { get; set; }
        public int ColumnBanding { get; set; }
        public ShadingSettings Shading { get; set; }
        public bool AllowAutoFit { get; set; } = true;
        public bool DoNotAllowAutoFit { get; set; }
    }

    public class TableCellMargins
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
    }

    public class TableBorders
    {
        public BorderSettings Top { get; set; }
        public BorderSettings Bottom { get; set; }
        public BorderSettings Left { get; set; }
        public BorderSettings Right { get; set; }
        public BorderSettings InsideHorizontal { get; set; }
        public BorderSettings InsideVertical { get; set; }
        public BorderSettings DiagonalUp { get; set; }
        public BorderSettings DiagonalDown { get; set; }
    }

    public class TableRowFormatting
    {
        public double Height { get; set; }
        public HeightRule HeightRule { get; set; } = HeightRule.Auto;
        public bool IsHeaderRow { get; set; }
        public bool IsTotalRow { get; set; }
    }

    public enum HeightRule
    {
        Auto,
        AtLeast,
        Exact
    }
}

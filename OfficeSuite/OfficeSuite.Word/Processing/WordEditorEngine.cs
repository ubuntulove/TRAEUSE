using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace OfficeSuite.Core
{
    public class WordEditorEngine
    {
        private readonly Dictionary<string, Func<string[], string>> _functions;
        private readonly Dictionary<string, string> _variables;

        public WordEditorEngine()
        {
            _functions = new Dictionary<string, Func<string[], string>>();
            _variables = new Dictionary<string, string>();
            InitializeBuiltInFunctions();
        }

        private void InitializeBuiltInFunctions()
        {
            _functions["UPPER"] = args => args[0].ToUpper();
            _functions["LOWER"] = args => args[0].ToLower();
            _functions["PROPER"] = args => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(args[0].ToLower());
            _functions["TRIM"] = args => args[0].Trim();
            _functions["CONCATENATE"] = args => string.Join("", args);
            _functions["LEFT"] = args => args[0].Substring(0, int.Parse(args[1]));
            _functions["RIGHT"] = args => args[0].Substring(args[0].Length - int.Parse(args[1]));
            _functions["MID"] = args => args[0].Substring(int.Parse(args[1]) - 1, int.Parse(args[2]));
            _functions["LEN"] = args => args[0].Length.ToString();
            _functions["FIND"] = args => (args[0].IndexOf(args[1]) + 1).ToString();
            _functions["REPLACE"] = args => args[0].Replace(args[1], args[2]);
            _functions["SUBSTITUTE"] = args => args[0].Replace(args[1], args[2]);
            _functions["REPT"] = args => string.Concat(Enumerable.Repeat(args[0], int.Parse(args[1])));
            _functions["TEXT"] = args => args[0];
            _functions["VALUE"] = args => args[0];
            _functions["FIXED"] = args => Math.Round(double.Parse(args[0]), int.Parse(args[1])).ToString();
            _functions["DOLLAR"] = args => "$" + args[0];
            _functions["SEARCH"] = args => (args[0].IndexOf(args[1], StringComparison.OrdinalIgnoreCase) + 1).ToString();
            _functions["CHAR"] = args => ((char)int.Parse(args[0])).ToString();
            _functions["CODE"] = args => ((int)args[0][0]).ToString();
            _functions["CLEAN"] = args => new string(args[0].Where(c => !char.IsControl(c)).ToArray());
            _functions["EXACT"] = args => (args[0] == args[1]).ToString();
            _functions["TEXTJOIN"] = args => string.Join(args[0], args.Skip(1));
            _functions["CONCAT"] = args => string.Concat(args);
            _functions["REVERSE"] = args => new string(args[0].Reverse().ToArray());
            _functions["STARTSWITH"] = args => args[0].StartsWith(args[1]).ToString();
            _functions["ENDSWITH"] = args => args[0].EndsWith(args[1]).ToString();
            _functions["CONTAINS"] = args => args[0].Contains(args[1]).ToString();
            _functions["INDEXOF"] = args => args[0].IndexOf(args[1]).ToString();
            _functions["LASTINDEXOF"] = args => args[0].LastIndexOf(args[1]).ToString();
            _functions["INSERT"] = args => args[0].Insert(int.Parse(args[1]), args[2]);
            _functions["REMOVE"] = args => args[0].Remove(int.Parse(args[1]), int.Parse(args[2]));
            _functions["PADLEFT"] = args => args[0].PadLeft(int.Parse(args[1]), args.Length > 2 ? args[2][0] : ' ');
            _functions["PADRIGHT"] = args => args[0].PadRight(int.Parse(args[1]), args.Length > 2 ? args[2][0] : ' ');
            _functions["TRUNCATE"] = args => args[0].Length > int.Parse(args[1]) ? args[0].Substring(0, int.Parse(args[1])) : args[0];
            _functions["WORDCOUNT"] = args => args[0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length.ToString();
            _functions["LINECOUNT"] = args => args[0].Split('\n').Length.ToString();
        }

        public string Evaluate(string expression)
        {
            if (string.IsNullOrEmpty(expression))
                return string.Empty;

            var result = ProcessExpression(expression);
            return result;
        }

        private string ProcessExpression(string expression)
        {
            expression = expression.Trim();

            if (expression.StartsWith("="))
                expression = expression.Substring(1);

            foreach (var func in _functions)
            {
                var pattern = $@"{func.Key}\s*\(([^)]*)\)";
                var match = Regex.Match(expression, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var argsStr = match.Groups[1].Value;
                    var args = ParseArguments(argsStr);
                    var funcResult = func.Value(args);
                    expression = expression.Replace(match.Value, funcResult);
                }
            }

            foreach (var variable in _variables)
            {
                expression = expression.Replace(variable.Key, variable.Value);
            }

            return expression;
        }

        private string[] ParseArguments(string argsStr)
        {
            var args = new List<string>();
            var current = "";
            var depth = 0;
            var inQuotes = false;

            foreach (var c in argsStr)
            {
                if (c == '"')
                    inQuotes = !inQuotes;
                else if (c == '(' && !inQuotes)
                    depth++;
                else if (c == ')' && !inQuotes)
                    depth--;
                else if (c == ',' && depth == 0 && !inQuotes)
                {
                    args.Add(current.Trim());
                    current = "";
                    continue;
                }
                current += c;
            }

            if (!string.IsNullOrEmpty(current))
                args.Add(current.Trim());

            return args.ToArray();
        }

        public void SetVariable(string name, string value)
        {
            _variables[name] = value;
        }

        public string GetVariable(string name)
        {
            return _variables.ContainsKey(name) ? _variables[name] : null;
        }

        public void ClearVariables()
        {
            _variables.Clear();
        }

        public void RegisterFunction(string name, Func<string[], string> function)
        {
            _functions[name.ToUpper()] = function;
        }

        public List<string> GetFunctionNames()
        {
            return _functions.Keys.ToList();
        }
    }

    public class DocumentMerger
    {
        public void MergeDocuments(Models.WordDocument target, Models.WordDocument source, MergeOptions options)
        {
            switch (options.MergeMode)
            {
                case MergeMode.Append:
                    AppendDocuments(target, source);
                    break;
                case MergeMode.InsertAfter:
                    InsertAfter(target, source, options.InsertAfterParagraph);
                    break;
                case MergeMode.InsertBefore:
                    InsertBefore(target, source, options.InsertBeforeParagraph);
                    break;
            }
        }

        private void AppendDocuments(Models.WordDocument target, Models.WordDocument source)
        {
            foreach (var para in source.Paragraphs)
            {
                target.Paragraphs.Add(para.Clone());
            }
        }

        private void InsertAfter(Models.WordDocument target, Models.WordDocument source, int paragraphIndex)
        {
            var insertIndex = paragraphIndex + 1;
            foreach (var para in source.Paragraphs)
            {
                target.Paragraphs.Insert(insertIndex++, para.Clone());
            }
        }

        private void InsertBefore(Models.WordDocument target, Models.WordDocument source, int paragraphIndex)
        {
            foreach (var para in source.Paragraphs)
            {
                target.Paragraphs.Insert(paragraphIndex++, para.Clone());
            }
        }

        public Models.WordDocument CompareDocuments(Models.WordDocument doc1, Models.WordDocument doc2)
        {
            var result = new Models.WordDocument();
            int maxParagraphs = Math.Max(doc1.Paragraphs.Count, doc2.Paragraphs.Count);

            for (int i = 0; i < maxParagraphs; i++)
            {
                var para1 = i < doc1.Paragraphs.Count ? doc1.Paragraphs[i] : null;
                var para2 = i < doc2.Paragraphs.Count ? doc2.Paragraphs[i] : null;

                if (para1 == null)
                {
                    result.Paragraphs.Add(para2.Clone());
                }
                else if (para2 == null)
                {
                    result.Paragraphs.Add(para1.Clone());
                }
                else
                {
                    var mergedPara = CompareParagraphs(para1, para2);
                    if (mergedPara != null)
                        result.Paragraphs.Add(mergedPara);
                }
            }

            return result;
        }

        private Models.Paragraph CompareParagraphs(Models.Paragraph para1, Models.Paragraph para2)
        {
            if (para1.GetText() == para2.GetText())
                return para1.Clone();

            var merged = new Models.Paragraph();
            foreach (var run in para1.Runs)
                merged.Runs.Add(run.Clone());
            foreach (var run in para2.Runs)
                merged.Runs.Add(run.Clone());

            return merged;
        }

        public List<DifferenceInfo> FindDifferences(Models.WordDocument doc1, Models.WordDocument doc2)
        {
            var differences = new List<DifferenceInfo>();

            for (int i = 0; i < Math.Max(doc1.Paragraphs.Count, doc2.Paragraphs.Count); i++)
            {
                var para1 = i < doc1.Paragraphs.Count ? doc1.Paragraphs[i] : null;
                var para2 = i < doc2.Paragraphs.Count ? doc2.Paragraphs[i] : null;

                if (para1 == null && para2 != null)
                {
                    differences.Add(new DifferenceInfo
                    {
                        ParagraphIndex = i,
                        Type = DifferenceType.Added,
                        Text = para2.GetText()
                    });
                }
                else if (para1 != null && para2 == null)
                {
                    differences.Add(new DifferenceInfo
                    {
                        ParagraphIndex = i,
                        Type = DifferenceType.Deleted,
                        Text = para1.GetText()
                    });
                }
                else if (para1.GetText() != para2.GetText())
                {
                    differences.Add(new DifferenceInfo
                    {
                        ParagraphIndex = i,
                        Type = DifferenceType.Modified,
                        OldText = para1.GetText(),
                        NewText = para2.GetText()
                    });
                }
            }

            return differences;
        }
    }

    public class MergeOptions
    {
        public MergeMode MergeMode { get; set; }
        public int InsertAfterParagraph { get; set; }
        public int InsertBeforeParagraph { get; set; }
        public bool PreserveFormatting { get; set; }
        public bool KeepSourceStyles { get; set; }
    }

    public enum MergeMode
    {
        Append,
        InsertAfter,
        InsertBefore,
        Compare
    }

    public class DifferenceInfo
    {
        public int ParagraphIndex { get; set; }
        public DifferenceType Type { get; set; }
        public string Text { get; set; }
        public string OldText { get; set; }
        public string NewText { get; set; }
    }

    public enum DifferenceType
    {
        Added,
        Deleted,
        Modified
    }

    public class StyleManager
    {
        private readonly Dictionary<string, StyleDefinition> _styles;

        public StyleManager()
        {
            _styles = new Dictionary<string, StyleDefinition>();
            LoadDefaultStyles();
        }

        private void LoadDefaultStyles()
        {
            _styles["Normal"] = new StyleDefinition
            {
                Name = "Normal",
                BasedOn = null,
                NextStyle = "Normal",
                FontName = "Calibri",
                FontSize = 11,
                IsBold = false,
                IsItalic = false
            };

            _styles["Heading1"] = new StyleDefinition
            {
                Name = "Heading 1",
                BasedOn = "Normal",
                NextStyle = "Normal",
                FontName = "Calibri",
                FontSize = 28,
                IsBold = true,
                Color = "#2F5496"
            };

            _styles["Heading2"] = new StyleDefinition
            {
                Name = "Heading 2",
                BasedOn = "Normal",
                NextStyle = "Normal",
                FontName = "Calibri",
                FontSize = 24,
                IsBold = true,
                Color = "#2F5496"
            };

            _styles["Heading3"] = new StyleDefinition
            {
                Name = "Heading 3",
                BasedOn = "Normal",
                NextStyle = "Normal",
                FontName = "Calibri",
                FontSize = 18,
                IsBold = true
            };

            _styles["Title"] = new StyleDefinition
            {
                Name = "Title",
                BasedOn = "Normal",
                NextStyle = "Normal",
                FontName = "Calibri Light",
                FontSize = 36,
                IsBold = true,
                Color = "#172B4D"
            };

            _styles["Subtitle"] = new StyleDefinition
            {
                Name = "Subtitle",
                BasedOn = "Normal",
                NextStyle = "Normal",
                FontName = "Calibri",
                FontSize = 14,
                Color = "#5A5A5A"
            };

            _styles["Quote"] = new StyleDefinition
            {
                Name = "Quote",
                BasedOn = "Normal",
                NextStyle = "Normal",
                FontName = "Calibri",
                FontSize = 11,
                IsItalic = true,
                LeftIndent = 40
            };

            _styles["Code"] = new StyleDefinition
            {
                Name = "Code",
                BasedOn = "Normal",
                NextStyle = "Normal",
                FontName = "Consolas",
                FontSize = 10,
                LeftIndent = 20
            };
        }

        public StyleDefinition GetStyle(string name)
        {
            return _styles.ContainsKey(name) ? _styles[name] : _styles["Normal"];
        }

        public void ApplyStyle(Models.Paragraph paragraph, string styleName)
        {
            var style = GetStyle(styleName);
            if (style == null)
                return;

            if (paragraph.Format == null)
                paragraph.Format = new Models.ParagraphFormat();

            paragraph.StyleName = styleName;

            if (style.LeftIndent > 0)
                paragraph.Format.LeftIndent = style.LeftIndent;

            foreach (var run in paragraph.Runs)
            {
                ApplyRunStyle(run, style);
            }
        }

        public void ApplyStyle(Models.Run run, string styleName)
        {
            var style = GetStyle(styleName);
            if (style == null)
                return;

            ApplyRunStyle(run, style);
        }

        private void ApplyRunStyle(Models.Run run, StyleDefinition style)
        {
            if (run.Format == null)
                run.Format = new Models.CharacterFormat();

            if (!string.IsNullOrEmpty(style.FontName))
                run.Format.FontName = style.FontName;

            if (style.FontSize > 0)
                run.Format.FontSize = style.FontSize;

            run.Format.IsBold = style.IsBold;
            run.Format.IsItalic = style.IsItalic;

            if (!string.IsNullOrEmpty(style.Color))
                run.Format.Color = style.Color;
        }

        public void AddStyle(StyleDefinition style)
        {
            _styles[style.Name] = style;
        }

        public void RemoveStyle(string name)
        {
            if (_styles.ContainsKey(name) && name != "Normal")
                _styles.Remove(name);
        }

        public List<string> GetStyleNames()
        {
            return _styles.Keys.ToList();
        }

        public List<StyleDefinition> GetAllStyles()
        {
            return _styles.Values.ToList();
        }
    }

    public class StyleDefinition
    {
        public string Name { get; set; }
        public string BasedOn { get; set; }
        public string NextStyle { get; set; }
        public string FontName { get; set; }
        public double FontSize { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public string Color { get; set; }
        public double LeftIndent { get; set; }
        public Models.HorizontalAlignment Alignment { get; set; }
        public double LineSpacing { get; set; }
        public Models.UnderlineType UnderlineStyle { get; set; }
    }

    public class TableOfContentsGenerator
    {
        private readonly StyleManager _styleManager;

        public TableOfContentsGenerator(StyleManager styleManager)
        {
            _styleManager = styleManager;
        }

        public List<TableOfContentsEntry> GenerateToc(Models.WordDocument document, TocOptions options)
        {
            var entries = new List<TableOfContentsEntry>();

            for (int i = 0; i < document.Paragraphs.Count; i++)
            {
                var para = document.Paragraphs[i];
                var styleName = para.StyleName;

                if (string.IsNullOrEmpty(styleName))
                    continue;

                if (options.StyleNames.Contains(styleName))
                {
                    var level = GetHeadingLevel(styleName);
                    if (level >= options.FromLevel && level <= options.ToLevel)
                    {
                        entries.Add(new TableOfContentsEntry
                        {
                            Text = para.GetText(),
                            Level = level,
                            ParagraphIndex = i,
                            PageNumber = 1
                        });
                    }
                }
            }

            return entries;
        }

        private int GetHeadingLevel(string styleName)
        {
            if (styleName.Contains("Heading"))
            {
                var match = Regex.Match(styleName, @"Heading\s*(\d+)");
                if (match.Success && int.TryParse(match.Groups[1].Value, out int level))
                    return level;
            }
            return 0;
        }

        public Models.WordDocument GenerateTocDocument(List<TableOfContentsEntry> entries, TocOptions options)
        {
            var document = new Models.WordDocument();

            var titlePara = new Models.Paragraph();
            var titleRun = new Models.Run { Text = options.Title ?? "Table of Contents" };
            titleRun.Format = new Models.CharacterFormat { IsBold = true, FontSize = 16 };
            titlePara.Runs.Add(titleRun);
            document.Paragraphs.Add(titlePara);

            foreach (var entry in entries)
            {
                var para = new Models.Paragraph();

                if (options.IncludePageNumbers)
                {
                    var text = entry.Text;
                    if (options.IncludeDots)
                    {
                        var dots = new string('.', Math.Max(1, 60 - entry.Text.Length - entry.PageNumber.ToString().Length));
                        text = entry.Text + " " + dots + " " + entry.PageNumber;
                    }
                    else
                    {
                        text = entry.Text + " " + entry.PageNumber;
                    }

                    para.Runs.Add(new Models.Run { Text = text });
                }
                else
                {
                    var indent = (entry.Level - 1) * options.IndentSize;
                    if (indent > 0)
                    {
                        para.Format = new Models.ParagraphFormat { LeftIndent = indent };
                    }
                    para.Runs.Add(new Models.Run { Text = entry.Text });
                }

                document.Paragraphs.Add(para);
            }

            return document;
        }

        public void UpdatePageNumbers(List<TableOfContentsEntry> entries, Dictionary<int, int> paragraphToPageMap)
        {
            foreach (var entry in entries)
            {
                if (paragraphToPageMap.ContainsKey(entry.ParagraphIndex))
                {
                    entry.PageNumber = paragraphToPageMap[entry.ParagraphIndex];
                }
            }
        }
    }

    public class TableOfContentsEntry
    {
        public string Text { get; set; }
        public int Level { get; set; }
        public int ParagraphIndex { get; set; }
        public int PageNumber { get; set; }
    }

    public class TocOptions
    {
        public string Title { get; set; }
        public List<string> StyleNames { get; set; }
        public int FromLevel { get; set; }
        public int ToLevel { get; set; }
        public bool IncludePageNumbers { get; set; }
        public bool IncludeDots { get; set; }
        public double IndentSize { get; set; }
        public string TabLeader { get; set; }
        public int RightTabPosition { get; set; }

        public TocOptions()
        {
            StyleNames = new List<string> { "Heading 1", "Heading 2", "Heading 3" };
            FromLevel = 1;
            ToLevel = 3;
            IncludePageNumbers = true;
            IncludeDots = true;
            IndentSize = 20;
            TabLeader = "dot";
            RightTabPosition = 450;
        }
    }

    public class DocumentStatistics
    {
        public int TotalCharacters { get; set; }
        public int TotalCharactersWithSpaces { get; set; }
        public int TotalWords { get; set; }
        public int TotalParagraphs { get; set; }
        public int TotalLines { get; set; }
        public int TotalPages { get; set; }
        public int TotalSentences { get; set; }
        public double AverageWordsPerSentence { get; set; }
        public double AverageCharactersPerWord { get; set; }
        public double AverageCharactersPerParagraph { get; set; }
    }

    public class StatisticsCalculator
    {
        public DocumentStatistics CalculateStatistics(Models.WordDocument document)
        {
            var stats = new DocumentStatistics();

            var allText = new StringBuilder();
            var wordCount = 0;
            var sentenceCount = 0;

            foreach (var para in document.Paragraphs)
            {
                var paraText = para.GetText();
                allText.Append(paraText);
                allText.Append(" ");

                var words = paraText.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                wordCount += words.Length;

                var sentences = CountSentences(paraText);
                sentenceCount += sentences;

                if (!string.IsNullOrWhiteSpace(paraText))
                    stats.TotalParagraphs++;
            }

            stats.TotalWords = wordCount;
            stats.TotalSentences = sentenceCount;
            stats.TotalCharacters = allText.Length;
            stats.TotalCharactersWithSpaces = allText.Length;
            stats.TotalLines = allText.ToString().Split('\n').Length;

            stats.TotalPages = CalculatePageCount(document);
            stats.AverageWordsPerSentence = sentenceCount > 0 ? (double)wordCount / sentenceCount : 0;
            stats.AverageCharactersPerWord = wordCount > 0 ? (double)stats.TotalCharacters / wordCount : 0;
            stats.AverageCharactersPerParagraph = stats.TotalParagraphs > 0 ? (double)stats.TotalCharacters / stats.TotalParagraphs : 0;

            return stats;
        }

        private int CountSentences(string text)
        {
            var count = 0;
            var sentenceEnders = new[] { '.', '!', '?' };
            var prevChar = ' ';

            foreach (var c in text)
            {
                if (sentenceEnders.Contains(c) && prevChar != '.' && prevChar != '!' && prevChar != '?')
                    count++;
                prevChar = c;
            }

            return Math.Max(1, count);
        }

        private int CalculatePageCount(Models.WordDocument document)
        {
            var totalChars = document.Paragraphs.Sum(p => p.GetText().Length);
            var charsPerPage = 3000;
            return Math.Max(1, (int)Math.Ceiling((double)totalChars / charsPerPage));
        }
    }

    public class SearchAndReplaceEngine
    {
        public List<SearchMatch> Search(Models.WordDocument document, SearchCriteria criteria)
        {
            var matches = new List<SearchMatch>();

            for (int paraIndex = 0; paraIndex < document.Paragraphs.Count; paraIndex++)
            {
                var para = document.Paragraphs[paraIndex];
                var paraText = para.GetText();

                var searchResults = SearchInText(paraText, criteria);
                foreach (var result in searchResults)
                {
                    matches.Add(new SearchMatch
                    {
                        ParagraphIndex = paraIndex,
                        StartIndex = result.StartIndex,
                        EndIndex = result.EndIndex,
                        MatchedText = result.MatchedText
                    });
                }
            }

            return matches;
        }

        private List<SearchMatchResult> SearchInText(string text, SearchCriteria criteria)
        {
            var results = new List<SearchMatchResult>();

            var searchText = criteria.SearchText;
            var comparison = criteria.IgnoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

            int index = 0;
            while ((index = text.IndexOf(searchText, index, comparison)) != -1)
            {
                results.Add(new SearchMatchResult
                {
                    StartIndex = index,
                    EndIndex = index + searchText.Length,
                    MatchedText = text.Substring(index, searchText.Length)
                });
                index += searchText.Length;
            }

            return results;
        }

        public int Replace(Models.WordDocument document, SearchCriteria criteria, string replaceText)
        {
            var matches = Search(document, criteria);
            var count = 0;

            foreach (var match in matches.OrderByDescending(m => m.ParagraphIndex))
            {
                var para = document.Paragraphs[match.ParagraphIndex];
                var paraText = para.GetText();

                paraText = paraText.Substring(0, match.StartIndex) + replaceText + paraText.Substring(match.EndIndex);

                para.Runs.Clear();
                para.Runs.Add(new Models.Run { Text = paraText });
                count++;
            }

            return count;
        }

        public int ReplaceAll(Models.WordDocument document, SearchCriteria criteria, string replaceText)
        {
            return Replace(document, criteria, replaceText);
        }
    }

    public class SearchCriteria
    {
        public string SearchText { get; set; }
        public bool IgnoreCase { get; set; }
        public bool MatchWholeWord { get; set; }
        public bool UseWildcards { get; set; }
        public bool UseRegex { get; set; }
        public bool SearchUp { get; set; }
    }

    public class SearchMatch
    {
        public int ParagraphIndex { get; set; }
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string MatchedText { get; set; }
    }

    public class SearchMatchResult
    {
        public int StartIndex { get; set; }
        public int EndIndex { get; set; }
        public string MatchedText { get; set; }
    }

    public class FormatPainter
    {
        public CharacterFormat CopiedCharacterFormat { get; set; }
        public ParagraphFormat CopiedParagraphFormat { get; set; }
        public bool HasFormat { get; set; }

        public void CopyFormat(Models.Run run)
        {
            CopiedCharacterFormat = run.Format?.Clone();
            HasFormat = true;
        }

        public void CopyFormat(Models.Paragraph para)
        {
            CopiedParagraphFormat = para.Format?.Clone();
            if (para.Runs.Count > 0)
                CopyFormat(para.Runs[0]);
        }

        public void PasteFormat(Models.Run run)
        {
            if (CopiedCharacterFormat != null)
            {
                run.Format = CopiedCharacterFormat.Clone();
            }
        }

        public void PasteFormat(Models.Paragraph para)
        {
            if (CopiedParagraphFormat != null)
            {
                para.Format = CopiedParagraphFormat.Clone();
            }

            if (CopiedCharacterFormat != null)
            {
                foreach (var run in para.Runs)
                {
                    PasteFormat(run);
                }
            }
        }

        public void PasteFormat(Models.WordDocument document, List<int> paragraphIndices)
        {
            foreach (var index in paragraphIndices)
            {
                if (index >= 0 && index < document.Paragraphs.Count)
                {
                    PasteFormat(document.Paragraphs[index]);
                }
            }
        }

        public void Clear()
        {
            CopiedCharacterFormat = null;
            CopiedParagraphFormat = null;
            HasFormat = false;
        }
    }

    public class AutoCorrectEngine
    {
        private readonly Dictionary<string, string> _entries;
        private readonly List<string> _ignoredWords;

        public AutoCorrectEngine()
        {
            _entries = new Dictionary<string, string>();
            _ignoredWords = new List<string>();
            LoadDefaultEntries();
        }

        private void LoadDefaultEntries()
        {
            _entries["(c)"] = "\u00a9";
            _entries["(r)"] = "\u00ae";
            _entries["(tm)"] = "\u2122";
            _entries["..."] = "\u2026";
            _entries["--"] = "\u2014";
            _entries["<<"] = "\u00ab";
            _entries[">>"] = "\u00bb";
            _entries["1/2"] = "\u00bd";
            _entries["1/4"] = "\u00bc";
            _entries["3/4"] = "\u00be";
            _entries["+-"] = "\u00b1";
            _entries["!="] = "\u2260";
            _entries["<="] = "\u2264";
            _entries[">="] = "\u2265";
            _entries["->"] = "\u2192";
            _entries["<-"] = "\u2190";
            _entries["=>"] = "\u21d2";
            _entries["..."] = "\u2026";
        }

        public string ProcessAutoCorrect(string text)
        {
            var result = text;

            foreach (var entry in _entries)
            {
                result = result.Replace(entry.Key, entry.Value);
            }

            return result;
        }

        public string ProcessSentence(string text)
        {
            var result = text;

            result = Regex.Replace(result, @"^\s*(\w)", m => m.Groups[1].Value.ToUpper());

            result = Regex.Replace(result, @"([.!?])\s*(\w)", m => m.Groups[1].Value + " " + m.Groups[2].Value.ToUpper());

            return result;
        }

        public void AddEntry(string trigger, string replacement)
        {
            _entries[trigger] = replacement;
        }

        public void RemoveEntry(string trigger)
        {
            _entries.Remove(trigger);
        }

        public void IgnoreWord(string word)
        {
            if (!_ignoredWords.Contains(word))
                _ignoredWords.Add(word);
        }

        public List<KeyValuePair<string, string>> GetAllEntries()
        {
            return _entries.ToList();
        }

        public void LoadEntries(string filePath)
        {
        }

        public void SaveEntries(string filePath)
        {
        }
    }

    public class BookmarkManager
    {
        private readonly Dictionary<string, Models.Bookmark> _bookmarks;

        public BookmarkManager()
        {
            _bookmarks = new Dictionary<string, Models.Bookmark>();
        }

        public void AddBookmark(Models.WordDocument document, string name, int startIndex, int endIndex)
        {
            if (_bookmarks.ContainsKey(name))
                throw new InvalidOperationException($"Bookmark '{name}' already exists.");

            var bookmark = new Models.Bookmark
            {
                Name = name,
                StartIndex = startIndex,
                EndIndex = endIndex
            };

            _bookmarks[name] = bookmark;
        }

        public void RemoveBookmark(string name)
        {
            _bookmarks.Remove(name);
        }

        public Models.Bookmark GetBookmark(string name)
        {
            return _bookmarks.ContainsKey(name) ? _bookmarks[name] : null;
        }

        public List<Models.Bookmark> GetAllBookmarks()
        {
            return _bookmarks.Values.ToList();
        }

        public bool BookmarkExists(string name)
        {
            return _bookmarks.ContainsKey(name);
        }

        public void NavigateToBookmark(Models.WordDocument document, string name)
        {
            var bookmark = GetBookmark(name);
            if (bookmark == null)
                return;
        }

        public string GetBookmarkText(Models.WordDocument document, string name)
        {
            var bookmark = GetBookmark(name);
            if (bookmark == null)
                return null;

            var paraIndex = bookmark.StartIndex;
            if (paraIndex >= 0 && paraIndex < document.Paragraphs.Count)
            {
                var para = document.Paragraphs[paraIndex];
                var text = para.GetText();

                var start = Math.Max(0, bookmark.StartIndex);
                var end = Math.Min(text.Length, bookmark.EndIndex);

                return text.Substring(start, end - start);
            }

            return null;
        }

        public void UpdateBookmarkRange(string name, int startIndex, int endIndex)
        {
            if (_bookmarks.ContainsKey(name))
            {
                _bookmarks[name].StartIndex = startIndex;
                _bookmarks[name].EndIndex = endIndex;
            }
        }

        public void ClearAll()
        {
            _bookmarks.Clear();
        }
    }

    public class HyperlinkManager
    {
        private readonly Dictionary<int, Models.Hyperlink> _hyperlinks;

        public HyperlinkManager()
        {
            _hyperlinks = new Dictionary<int, Models.Hyperlink>();
        }

        public void AddHyperlink(Models.WordDocument document, int startIndex, int endIndex, string url, string displayText)
        {
            var hyperlink = new Models.Hyperlink
            {
                StartIndex = startIndex,
                EndIndex = endIndex,
                Url = url,
                DisplayText = displayText ?? url
            };

            _hyperlinks[startIndex] = hyperlink;
        }

        public void RemoveHyperlink(int startIndex)
        {
            _hyperlinks.Remove(startIndex);
        }

        public Models.Hyperlink GetHyperlink(int startIndex)
        {
            return _hyperlinks.ContainsKey(startIndex) ? _hyperlinks[startIndex] : null;
        }

        public List<Models.Hyperlink> GetAllHyperlinks()
        {
            return _hyperlinks.Values.ToList();
        }

        public void UpdateHyperlink(int startIndex, string url, string displayText)
        {
            if (_hyperlinks.ContainsKey(startIndex))
            {
                _hyperlinks[startIndex].Url = url;
                _hyperlinks[startIndex].DisplayText = displayText ?? url;
            }
        }

        public bool ValidateUrl(string url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }

        public string GetLinkInfo(Models.Hyperlink hyperlink)
        {
            return $"URL: {hyperlink.Url}\nDisplay: {hyperlink.DisplayText}\nTarget: {hyperlink.Target}";
        }
    }

    public class ProtectionManager
    {
        private string _passwordHash;
        private ProtectionType _protectionType;
        private bool _isProtected;

        public ProtectionManager()
        {
            _isProtected = false;
            _protectionType = ProtectionType.None;
        }

        public void Protect(Models.WordDocument document, string password, ProtectionType type)
        {
            _passwordHash = ComputePasswordHash(password);
            _protectionType = type;
            _isProtected = true;

            document.Protection = new DocumentProtectionInfo
            {
                IsProtected = true,
                Type = type,
                PasswordHash = _passwordHash
            };
        }

        public void Unprotect(Models.WordDocument document, string password)
        {
            if (!IsProtected)
                return;

            if (_passwordHash == ComputePasswordHash(password))
            {
                _isProtected = false;
                _passwordHash = null;
                _protectionType = ProtectionType.None;

                document.Protection = new DocumentProtectionInfo
                {
                    IsProtected = false
                };
            }
        }

        public bool IsProtected => _isProtected;

        public ProtectionType ProtectionType => _protectionType;

        public void SetEditingRestrictions(Models.WordDocument document, EditingRestriction restriction)
        {
            if (!IsProtected)
                return;

            document.Protection.EditingRestriction = restriction;
        }

        private string ComputePasswordHash(string password)
        {
            using (var sha = System.Security.Cryptography.SHA256.Create())
            {
                var bytes = System.Text.Encoding.UTF8.GetBytes(password);
                var hash = sha.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }

    public enum ProtectionType
    {
        None,
        ReadOnly,
        Comments,
        TrackChanges,
        Forms
    }

    public class DocumentProtectionInfo
    {
        public bool IsProtected { get; set; }
        public ProtectionType Type { get; set; }
        public string PasswordHash { get; set; }
        public EditingRestriction EditingRestriction { get; set; }
    }

    public enum EditingRestriction
    {
        None,
        ReadOnly,
        Comments,
        TrackChanges,
        Forms,
        FillingForms
    }

    public class ClipboardManager
    {
        private readonly Stack<ClipboardItem> _undoStack;
        private readonly Stack<ClipboardItem> _redoStack;
        private ClipboardItem _currentItem;

        public ClipboardManager()
        {
            _undoStack = new Stack<ClipboardItem>();
            _redoStack = new Stack<ClipboardItem>();
        }

        public void Copy(Models.WordDocument document, int startPara, int startIndex, int endPara, int endIndex)
        {
            var item = new ClipboardItem
            {
                Content = ExtractText(document, startPara, startIndex, endPara, endIndex),
                ClipboardType = ClipboardContentType.Text,
                SourceStartPara = startPara,
                SourceStartIndex = startIndex,
                SourceEndPara = endPara,
                SourceEndIndex = endIndex
            };

            _currentItem = item;
            _undoStack.Push(item);
            _redoStack.Clear();
        }

        public void Cut(Models.WordDocument document, int startPara, int startIndex, int endPara, int endIndex)
        {
            Copy(document, startPara, startIndex, endPara, endIndex);
            DeleteSelection(document, startPara, startIndex, endPara, endIndex);
        }

        public ClipboardItem Paste()
        {
            return _currentItem;
        }

        public bool CanUndo => _undoStack.Count > 1;
        public bool CanRedo => _redoStack.Count > 0;

        public void Undo()
        {
            if (!CanUndo)
                return;

            var current = _undoStack.Pop();
            _redoStack.Push(current);
            _currentItem = _undoStack.Peek();
        }

        public void Redo()
        {
            if (!CanRedo)
                return;

            var item = _redoStack.Pop();
            _undoStack.Push(item);
            _currentItem = item;
        }

        private string ExtractText(Models.WordDocument document, int startPara, int startIndex, int endPara, int endIndex)
        {
            var sb = new StringBuilder();

            for (int i = startPara; i <= endPara; i++)
            {
                if (i >= document.Paragraphs.Count)
                    break;

                var para = document.Paragraphs[i];
                var paraText = para.GetText();

                int start = i == startPara ? startIndex : 0;
                int end = i == endPara ? endIndex : paraText.Length;

                start = Math.Max(0, Math.Min(start, paraText.Length));
                end = Math.Max(0, Math.Min(end, paraText.Length));

                if (start < end)
                    sb.Append(paraText.Substring(start, end - start));

                if (i < endPara)
                    sb.AppendLine();
            }

            return sb.ToString();
        }

        private void DeleteSelection(Models.WordDocument document, int startPara, int startIndex, int endPara, int endIndex)
        {
            for (int i = startPara; i <= endPara && i < document.Paragraphs.Count; i++)
            {
                var para = document.Paragraphs[i];
                var paraText = para.GetText();

                int start = i == startPara ? startIndex : 0;
                int end = i == endPara ? endIndex : paraText.Length;

                start = Math.Max(0, Math.Min(start, paraText.Length));
                end = Math.Max(0, Math.Min(end, paraText.Length));

                if (start < end)
                {
                    var newText = paraText.Remove(start, end - start);
                    para.Runs.Clear();
                    para.Runs.Add(new Models.Run { Text = newText });
                }
            }
        }
    }

    public class ClipboardItem
    {
        public string Content { get; set; }
        public ClipboardContentType ClipboardType { get; set; }
        public int SourceStartPara { get; set; }
        public int SourceStartIndex { get; set; }
        public int SourceEndPara { get; set; }
        public int SourceEndIndex { get; set; }
        public DateTime CopiedAt { get; set; }
    }

    public enum ClipboardContentType
    {
        Text,
        RichText,
        Html,
        Image
    }

    public class NavigationManager
    {
        private readonly List<NavigationPoint> _history;
        private int _currentIndex;

        public NavigationManager()
        {
            _history = new List<NavigationPoint>();
            _currentIndex = -1;
        }

        public void AddPoint(int paragraphIndex, int characterIndex)
        {
            var point = new NavigationPoint
            {
                ParagraphIndex = paragraphIndex,
                CharacterIndex = characterIndex,
                Timestamp = DateTime.Now
            };

            if (_currentIndex < _history.Count - 1)
            {
                _history.RemoveRange(_currentIndex + 1, _history.Count - _currentIndex - 1);
            }

            _history.Add(point);
            _currentIndex = _history.Count - 1;

            if (_history.Count > 100)
            {
                _history.RemoveAt(0);
                _currentIndex--;
            }
        }

        public NavigationPoint GoBack()
        {
            if (_currentIndex > 0)
            {
                _currentIndex--;
                return _history[_currentIndex];
            }
            return null;
        }

        public NavigationPoint GoForward()
        {
            if (_currentIndex < _history.Count - 1)
            {
                _currentIndex++;
                return _history[_currentIndex];
            }
            return null;
        }

        public bool CanGoBack => _currentIndex > 0;
        public bool CanGoForward => _currentIndex < _history.Count - 1;

        public NavigationPoint GetCurrentPoint()
        {
            return _currentIndex >= 0 && _currentIndex < _history.Count
                ? _history[_currentIndex]
                : null;
        }

        public void Clear()
        {
            _history.Clear();
            _currentIndex = -1;
        }
    }

    public class NavigationPoint
    {
        public int ParagraphIndex { get; set; }
        public int CharacterIndex { get; set; }
        public DateTime Timestamp { get; set; }
    }

    public class SelectionManager
    {
        private SelectionRange _selection;

        public SelectionManager()
        {
            _selection = new SelectionRange();
        }

        public void SetSelection(int startPara, int startIndex, int endPara, int endIndex)
        {
            _selection = new SelectionRange
            {
                StartPara = startPara,
                StartIndex = startIndex,
                EndPara = endPara,
                EndIndex = endIndex,
                IsSelection = true
            };
        }

        public void ClearSelection()
        {
            _selection = new SelectionRange { IsSelection = false };
        }

        public SelectionRange GetSelection()
        {
            return _selection;
        }

        public bool HasSelection => _selection.IsSelection;

        public bool IsInSelection(int para, int index)
        {
            if (!HasSelection)
                return false;

            if (para < _selection.StartPara || para > _selection.EndPara)
                return false;

            if (para == _selection.StartPara && index < _selection.StartIndex)
                return false;

            if (para == _selection.EndPara && index > _selection.EndIndex)
                return false;

            return true;
        }
    }

    public class SelectionRange
    {
        public int StartPara { get; set; }
        public int StartIndex { get; set; }
        public int EndPara { get; set; }
        public int EndIndex { get; set; }
        public bool IsSelection { get; set; }

        public int Length
        {
            get
            {
                if (!IsSelection)
                    return 0;

                if (StartPara == EndPara)
                    return EndIndex - StartIndex;

                var length = StartPara < EndPara ? EndIndex : StartIndex;
                length -= StartIndex;
                length += (EndPara - StartPara - 1) * 50;
                return length;
            }
        }
    }
}

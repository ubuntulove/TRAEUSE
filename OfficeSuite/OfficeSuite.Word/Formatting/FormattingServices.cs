using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeSuite.Word.Formatting
{
    public class FontManager
    {
        private readonly List<FontDefinition> _fonts = new List<FontDefinition>();
        private readonly Dictionary<string, FontFamily> _fontCache = new Dictionary<string, FontFamily>();

        public FontManager()
        {
            InitializeDefaultFonts();
        }

        private void InitializeDefaultFonts()
        {
            _fonts.Add(new FontDefinition { Name = "Calibri", Category = "Sans Serif", IsDefault = true });
            _fonts.Add(new FontDefinition { Name = "Arial", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Times New Roman", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Verdana", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Georgia", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Helvetica", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Tahoma", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Trebuchet MS", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Courier New", Category = "Monospace" });
            _fonts.Add(new FontDefinition { Name = "Consolas", Category = "Monospace" });
            _fonts.Add(new FontDefinition { Name = "Lucida Console", Category = "Monospace" });
            _fonts.Add(new FontDefinition { Name = "Impact", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Comic Sans MS", Category = "Casual" });
            _fonts.Add(new FontDefinition { Name = "Palatino Linotype", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Book Antiqua", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Garamond", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Century Gothic", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Franklin Gothic Medium", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Candara", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Constantia", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Corbel", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Cambria", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Segoe UI", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Agency FB", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Arial Black", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Arial Narrow", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Baskerville Old Face", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Bauhaus 93", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Bell MT", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Berlin Sans FB", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Bernard MT Condensed", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Bodoni MT", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Bookshelf Symbol 7", Category = "Symbol" });
            _fonts.Add(new FontDefinition { Name = "Broadway", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Brush Script MT", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Californian FB", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Centaur", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Century Schoolbook", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Chiller", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Colonna MT", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Cooper Black", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Copperplate Gothic Bold", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Copperplate Gothic Light", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Courier", Category = "Monospace" });
            _fonts.Add(new FontDefinition { Name = "Dubai", Category = "Arabic" });
            _fonts.Add(new FontDefinition { Name = "Elephant", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Engravers MT", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Eras Bold ITC", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Eras Demi ITC", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Eras Light ITC", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Eras Medium ITC", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Felix Titling", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Footlight MT Light", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Forte", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Franklin Gothic Book", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Franklin Gothic Demi", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Franklin Gothic Heavy", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Freestyle Script", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "French Script MT", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Gabriola", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Gigi", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Gill Sans MT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Gill Sans MT Condensed", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Gill Sans MT Ext Condensed", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Gill Sans Ultra Bold", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Gill Sans Ultra Bold Condensed", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Gloucester MT Extra Condensed", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Goudy Old Style", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Goudy Stout", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Haettenschweiler", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Harlow Solid Italic", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Harrington", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "High Tower Text", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Hollywood Hills", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "HP Simplified", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "HP Simplified Light", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Imprint MT Shadow", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Informal Roman", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Jokerman", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Juice ITC", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Kristen ITC", Category = "Casual" });
            _fonts.Add(new FontDefinition { Name = "Kunstler Script", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "LastResort", Category = "Special" });
            _fonts.Add(new FontDefinition { Name = "Latha", Category = "Indic" });
            _fonts.Add(new FontDefinition { Name = "Leelawadee", Category = "Indic" });
            _fonts.Add(new FontDefinition { Name = "Lucida Bright", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Lucida Calligraphy", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Lucida Fax", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Lucida Handwriting", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Lucida Sans", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Lucida Sans Typewriter", Category = "Monospace" });
            _fonts.Add(new FontDefinition { Name = "Magneto", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Maiandra GD", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Malgun Gothic", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Mangal", Category = "Indic" });
            _fonts.Add(new FontDefinition { Name = "Marlett", Category = "Symbol" });
            _fonts.Add(new FontDefinition { Name = "Matura MT Script Capitals", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Meiryo", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Meiryo UI", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Microsoft Himalaya", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Microsoft JhengHei", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Microsoft New Tai Lue", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Microsoft PhagsPa", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Microsoft Sans Serif", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Microsoft Tai Le", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Microsoft YaHei", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Microsoft Yi Baiti", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "MingLiU", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "MingLiU_HKSCS", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Mongolian Baiti", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Monotype Corsiva", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "MV Boli", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Narkisim", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Niagara Engraved", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Niagara Solid", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Noteworthy", Category = "Casual" });
            _fonts.Add(new FontDefinition { Name = "OCR A Extended", Category = "Monospace" });
            _fonts.Add(new FontDefinition { Name = "Old English Text MT", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Onyx", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Open Sans", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Palace Script MT", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Papyrus", Category = "Casual" });
            _fonts.Add(new FontDefinition { Name = "Parchment", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Perpetua", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Perpetua Titling MT", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Playbill", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Poor Richard", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Princess Sofia", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Rage Italic", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Ravie", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Rockwell", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Rockwell Condensed", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Rockwell Extra Bold", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Roman", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Sans MS", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Script MT Bold", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Segoe Marker", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Segoe Print", Category = "Casual" });
            _fonts.Add(new FontDefinition { Name = "Segoe Script", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Segoe UI Light", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Segoe UI Semibold", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Segoe UI Symbol", Category = "Symbol" });
            _fonts.Add(new FontDefinition { Name = "Showcard Gothic", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "SimSun", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "SimSun-ExtB", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Sitka Banner", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Sitka Display", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Sitka Heading", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Sitka Small", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Sitka Subheading", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Sitka Text", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Snap ITC", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Stencil", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "Superior", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 BlkCn BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 BlkCn Md BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 BlkEx BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 Cn BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 Lt BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 LtCn BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 MdCn BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 MdEx BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 ThCn BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Swis721 ThEx BT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Sylfaen", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Symbol", Category = "Symbol" });
            _fonts.Add(new FontDefinition { Name = "Tahoma", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "TeamViewer", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Tempus Sans ITC", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Thorndale", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Times New Roman", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Traditional Arabic", Category = "Arabic" });
            _fonts.Add(new FontDefinition { Name = "Trebuchet MS", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Tw Cen MT", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Tw Cen MT Condensed", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Tw Cen MT Condensed Extra Bold", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Tunga", Category = "Indic" });
            _fonts.Add(new FontDefinition { Name = "Ubuntu", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Ubuntu Light", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Ubuntu Medium", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "Unicorn", Category = "Display" });
            _fonts.Add(new FontDefinition { Name = "URW Chancery L", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "URW Gothic L", Category = "Sans Serif" });
            _fonts.Add(new FontDefinition { Name = "URW Bookman L", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "URW Classical L", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Vani", Category = "Indic" });
            _fonts.Add(new FontDefinition { Name = "Vijaya", Category = "Indic" });
            _fonts.Add(new FontDefinition { Name = "Viner Hand ITC", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Vivaldi", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Vladimir Script", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Webdings", Category = "Symbol" });
            _fonts.Add(new FontDefinition { Name = "Wendy LP", Category = "Script" });
            _fonts.Add(new FontDefinition { Name = "Wide Latin", Category = "Serif" });
            _fonts.Add(new FontDefinition { Name = "Wingdings", Category = "Symbol" });
            _fonts.Add(new FontDefinition { Name = "Wingdings 2", Category = "Symbol" });
            _fonts.Add(new FontDefinition { Name = "Wingdings 3", Category = "Symbol" });
            _fonts.Add(new FontDefinition { Name = "Yu Gothic", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Yu Gothic Light", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Yu Gothic Medium", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Yu Gothic UI", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Yu Gothic UI Light", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Yu Gothic UI Semibold", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Yu Gothic UI Semilight", Category = "East Asian" });
            _fonts.Add(new FontDefinition { Name = "Zapfino", Category = "Script" });
        }

        public IEnumerable<FontDefinition> GetAllFonts() => _fonts.AsReadOnly();

        public IEnumerable<FontDefinition> GetFontsByCategory(string category)
        {
            return _fonts.Where(f => f.Category.Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        public FontDefinition GetFont(string name)
        {
            return _fonts.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetCategories()
        {
            return _fonts.Select(f => f.Category).Distinct().OrderBy(c => c);
        }

        public IEnumerable<FontDefinition> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return _fonts;

            return _fonts.Where(f =>
                f.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                f.Category.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<FontDefinition> GetRecentFonts()
        {
            return _fonts.Where(f => f.IsRecent).Take(5);
        }

        public void AddToRecent(FontDefinition font)
        {
            foreach (var f in _fonts)
                f.IsRecent = false;
            font.IsRecent = true;
        }
    }

    public class FontDefinition
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public bool IsDefault { get; set; }
        public bool IsRecent { get; set; }
        public byte[] Preview { get; set; }
    }

    public class FontFamily
    {
        public string Name { get; set; }
        public string Source { get; set; }
        public IEnumerable<FontStyle> Styles { get; set; }
    }

    public class FontStyle
    {
        public string Name { get; set; }
        public FontWeight Weight { get; set; }
        public FontStretch Stretch { get; set; }
    }

    public enum FontWeight
    {
        Thin = 100,
        ExtraLight = 200,
        Light = 300,
        Regular = 400,
        Medium = 500,
        SemiBold = 600,
        Bold = 700,
        ExtraBold = 800,
        Black = 900
    }

    public enum FontStretch
    {
        UltraCondensed = 1,
        ExtraCondensed = 2,
        Condensed = 3,
        SemiCondensed = 4,
        Normal = 5,
        SemiExpanded = 6,
        Expanded = 7,
        ExtraExpanded = 8,
        UltraExpanded = 9
    }

    public class SpellCheckService
    {
        private readonly Dictionary<string, HashSet<string>> _dictionaries = new Dictionary<string, HashSet<string>>();
        private readonly List<string> _customWords = new List<string>();
        private string _currentLanguage = "en-US";

        public SpellCheckService()
        {
            InitializeDictionaries();
        }

        private void InitializeDictionaries()
        {
            _dictionaries["en-US"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "be", "to", "of", "and", "a", "in", "that", "have", "it",
                "for", "not", "on", "with", "he", "as", "you", "do", "at", "this",
                "but", "his", "by", "from", "they", "we", "say", "her", "she", "or",
                "an", "will", "my", "one", "all", "would", "there", "their", "what",
                "so", "up", "out", "if", "about", "who", "get", "which", "go", "me",
                "when", "make", "can", "like", "time", "no", "just", "him", "know",
                "take", "people", "into", "year", "your", "good", "some", "could",
                "them", "see", "other", "than", "then", "now", "look", "only", "come",
                "its", "over", "think", "also", "back", "after", "use", "two", "how",
                "our", "work", "first", "well", "way", "even", "new", "want", "because",
                "any", "these", "give", "day", "most", "us"
            };

            _dictionaries["en-GB"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "the", "be", "to", "of", "and", "a", "in", "that", "have", "it",
                "for", "not", "on", "with", "he", "as", "you", "do", "at", "this",
                "but", "his", "by", "from", "they", "we", "say", "her", "she", "or",
                "an", "will", "my", "one", "all", "would", "there", "their", "what",
                "colour", "favour", "honour", "behaviour", "neighbour"
            };

            _dictionaries["es-ES"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "el", "la", "los", "las", "un", "una", "de", "en", "que", "es",
                "por", "para", "con", "su", "al", "lo", "se", "como", "más", "pero",
                "sus", "le", "ya", "o", "este", "sí", "porque", "esta", "entre",
                "cuando", "muy", "sin", "sobre", "también", "me", "hasta", "hay",
                "donde", "quien", "desde", "todo", "nos", "durante", "todos", "uno",
                "les", "ni", "contra", "otros", "ese", "eso", "ante", "ellos", "e"
            };

            _dictionaries["fr-FR"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "le", "la", "les", "un", "une", "de", "du", "des", "et", "en",
                "que", "qui", "dans", "ce", "il", "ne", "sur", "se", "pas", "plus",
                "par", "je", "avec", "tout", "faire", "son", "comme", "nous", "mais",
                "ou", "si", "leur", "y", "dire", "ces", "elle", "tous", "lui", "dont",
                "autre", "moi", "été", "avoir", "aux", "cette", "sans", "très", "bien",
                "encore", "cela", "aussi", "ici", "pourquoi"
            };

            _dictionaries["de-DE"] = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "der", "die", "und", "in", "den", "von", "zu", "das", "mit", "sich",
                "des", "auf", "für", "ist", "im", "dem", "nicht", "ein", "eine", "als",
                "auch", "es", "an", "er", "werden", "aus", "wurde", "hat", "nach", "am",
                "bei", "einer", "um", "im", "dann", "ist", "war", "werden", "noch",
                "wie", "aber", "nur", "oder", "kann", "diese", "ich", "mehr", "sein",
                "wenn", "sein", "durch", "schon", "über"
            };
        }

        public void SetLanguage(string language)
        {
            _currentLanguage = language;
        }

        public string CurrentLanguage => _currentLanguage;

        public IEnumerable<string> AvailableLanguages => _dictionaries.Keys;

        public bool IsWordCorrect(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return true;

            if (_customWords.Contains(word, StringComparer.OrdinalIgnoreCase))
                return true;

            if (_dictionaries.TryGetValue(_currentLanguage, out var dictionary))
                return dictionary.Contains(word);

            return true;
        }

        public IEnumerable<string> GetSuggestions(string word, int maxSuggestions = 5)
        {
            var suggestions = new List<Tuple<string, int>>();
            
            if (string.IsNullOrWhiteSpace(word))
                return suggestions.Select(s => s.Item1);

            foreach (var dict in _dictionaries.Values)
            {
                foreach (var candidate in dict)
                {
                    int distance = LevenshteinDistance(word, candidate);
                    if (distance <= 2)
                    {
                        suggestions.Add(Tuple.Create(candidate, distance));
                    }
                }
            }

            return suggestions
                .Where(s => s.Item2 > 0)
                .OrderBy(s => s.Item2)
                .ThenBy(s => s.Item1)
                .Take(maxSuggestions)
                .Select(s => s.Item1);
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int j = 1; j <= s2.Length; j++)
            {
                for (int i = 1; i <= s1.Length; i++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }

        public void AddToDictionary(string word)
        {
            if (!string.IsNullOrWhiteSpace(word) && !_customWords.Contains(word, StringComparer.OrdinalIgnoreCase))
                _customWords.Add(word);
        }

        public void RemoveFromDictionary(string word)
        {
            _customWords.RemoveAll(w => w.Equals(word, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<string> GetCustomWords()
        {
            return _customWords.AsReadOnly();
        }

        public SpellCheckResult CheckText(string text)
        {
            var result = new SpellCheckResult();
            var words = text.Split(new[] { ' ', '\t', '\n', '\r', '.', ',', '!', '?', ';', ':', '"', '\'', '(', ')', '[', ']', '{', '}' }, 
                StringSplitOptions.RemoveEmptyEntries);

            int position = 0;
            foreach (var word in words)
            {
                if (!IsWordCorrect(word))
                {
                    result.Errors.Add(new SpellCheckError
                    {
                        Word = word,
                        Position = position,
                        Suggestions = GetSuggestions(word).ToList()
                    });
                }
                position += word.Length + 1;
            }

            return result;
        }
    }

    public class SpellCheckResult
    {
        public bool HasErrors => Errors.Count > 0;
        public List<SpellCheckError> Errors { get; } = new List<SpellCheckError>();
        public int TotalWords { get; set; }
        public int CorrectWords => TotalWords - Errors.Count;
    }

    public class SpellCheckError
    {
        public string Word { get; set; }
        public int Position { get; set; }
        public List<string> Suggestions { get; set; }
    }

    public class AutoSaveService
    {
        private readonly List<AutoSaveDocument> _documents = new List<AutoSaveDocument>();
        private System.Threading.Timer _timer;
        private readonly int _intervalSeconds;

        public event EventHandler<AutoSaveEventArgs> DocumentSaved;
        public event EventHandler<AutoSaveEventArgs> AutoSaveTriggered;

        public AutoSaveService(int intervalSeconds = 300)
        {
            _intervalSeconds = intervalSeconds;
        }

        public void Start()
        {
            _timer = new System.Threading.Timer(OnTimerElapsed, null, _intervalSeconds * 1000, _intervalSeconds * 1000);
        }

        public void Stop()
        {
            _timer?.Dispose();
            _timer = null;
        }

        public void RegisterDocument(string documentId, string filePath, Func<byte[]> getContent)
        {
            var existing = _documents.FirstOrDefault(d => d.DocumentId == documentId);
            if (existing != null)
            {
                existing.FilePath = filePath;
                existing.GetContent = getContent;
            }
            else
            {
                _documents.Add(new AutoSaveDocument
                {
                    DocumentId = documentId,
                    FilePath = filePath,
                    GetContent = getContent,
                    LastSaved = DateTime.Now
                });
            }
        }

        public void UnregisterDocument(string documentId)
        {
            _documents.RemoveAll(d => d.DocumentId == documentId);
        }

        private void OnTimerElapsed(object state)
        {
            foreach (var doc in _documents)
            {
                try
                {
                    AutoSaveTriggered?.Invoke(this, new AutoSaveEventArgs { DocumentId = doc.DocumentId });

                    var content = doc.GetContent?.Invoke();
                    if (content != null && doc.Save(content))
                    {
                        doc.LastSaved = DateTime.Now;
                        DocumentSaved?.Invoke(this, new AutoSaveEventArgs 
                        { 
                            DocumentId = doc.DocumentId,
                            Success = true,
                            SavedAt = doc.LastSaved
                        });
                    }
                }
                catch
                {
                }
            }
        }

        public IEnumerable<AutoSaveDocument> GetRegisteredDocuments()
        {
            return _documents.AsReadOnly();
        }
    }

    public class AutoSaveDocument
    {
        public string DocumentId { get; set; }
        public string FilePath { get; set; }
        public Func<byte[]> GetContent { get; set; }
        public DateTime LastSaved { get; set; }
        public bool IsDirty { get; set; }

        public bool Save(byte[] content)
        {
            return false;
        }
    }

    public class AutoSaveEventArgs : EventArgs
    {
        public string DocumentId { get; set; }
        public bool Success { get; set; }
        public DateTime SavedAt { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class DocumentHistoryService
    {
        private readonly Dictionary<string, List<DocumentVersion>> _versionHistory = new Dictionary<string, List<DocumentVersion>>();
        private readonly int _maxVersions;

        public DocumentHistoryService(int maxVersions = 100)
        {
            _maxVersions = maxVersions;
        }

        public void CreateVersion(string documentId, byte[] content, string description = null)
        {
            if (!_versionHistory.ContainsKey(documentId))
                _versionHistory[documentId] = new List<DocumentVersion>();

            var version = new DocumentVersion
            {
                VersionNumber = GetVersionCount(documentId) + 1,
                Content = content,
                Description = description,
                CreatedAt = DateTime.Now,
                Size = content?.Length ?? 0
            };

            _versionHistory[documentId].Insert(0, version);

            while (_versionHistory[documentId].Count > _maxVersions)
                _versionHistory[documentId].RemoveAt(_versionHistory[documentId].Count - 1);
        }

        public IEnumerable<DocumentVersion> GetVersions(string documentId)
        {
            if (_versionHistory.TryGetValue(documentId, out var versions))
                return versions.AsReadOnly();
            return Enumerable.Empty<DocumentVersion>();
        }

        public DocumentVersion GetVersion(string documentId, int versionNumber)
        {
            if (_versionHistory.TryGetValue(documentId, out var versions))
                return versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
            return null;
        }

        public int GetVersionCount(string documentId)
        {
            if (_versionHistory.TryGetValue(documentId, out var versions))
                return versions.Count;
            return 0;
        }

        public void DeleteVersion(string documentId, int versionNumber)
        {
            if (_versionHistory.TryGetValue(documentId, out var versions))
            {
                var version = versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
                if (version != null)
                    versions.Remove(version);
            }
        }

        public void ClearHistory(string documentId)
        {
            _versionHistory.Remove(documentId);
        }

        public byte[] GetContent(string documentId, int versionNumber)
        {
            var version = GetVersion(documentId, versionNumber);
            return version?.Content;
        }
    }

    public class DocumentVersion
    {
        public int VersionNumber { get; set; }
        public byte[] Content { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public long Size { get; set; }
    }

    public class UndoRedoService
    {
        private readonly Stack<UndoableAction> _undoStack = new Stack<UndoableAction>();
        private readonly Stack<UndoableAction> _redoStack = new Stack<UndoableAction>();
        private readonly int _maxHistory;

        public event EventHandler UndoStackChanged;
        public event EventHandler RedoStackChanged;

        public UndoRedoService(int maxHistory = 100)
        {
            _maxHistory = maxHistory;
        }

        public bool CanUndo => _undoStack.Count > 0;
        public bool CanRedo => _redoStack.Count > 0;
        public int UndoCount => _undoStack.Count;
        public int RedoCount => _redoStack.Count;

        public void Execute(UndoableAction action)
        {
            action.Do();
            _undoStack.Push(action);
            _redoStack.Clear();

            while (_undoStack.Count > _maxHistory)
            {
                var items = _undoStack.ToArray();
                _undoStack.Clear();
                for (int i = 0; i < items.Length - 1; i++)
                    _undoStack.Push(items[items.Length - 1 - i]);
            }

            UndoStackChanged?.Invoke(this, EventArgs.Empty);
            RedoStackChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Undo()
        {
            if (!CanUndo) return;

            var action = _undoStack.Pop();
            action.Undo();
            _redoStack.Push(action);

            UndoStackChanged?.Invoke(this, EventArgs.Empty);
            RedoStackChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Redo()
        {
            if (!CanRedo) return;

            var action = _redoStack.Pop();
            action.Do();
            _undoStack.Push(action);

            UndoStackChanged?.Invoke(this, EventArgs.Empty);
            RedoStackChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Clear()
        {
            _undoStack.Clear();
            _redoStack.Clear();
            UndoStackChanged?.Invoke(this, EventArgs.Empty);
            RedoStackChanged?.Invoke(this, EventArgs.Empty);
        }

        public IEnumerable<string> GetUndoDescriptions()
        {
            return _undoStack.Select(a => a.Description);
        }

        public IEnumerable<string> GetRedoDescriptions()
        {
            return _redoStack.Select(a => a.Description);
        }
    }

    public abstract class UndoableAction
    {
        public string Description { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public abstract void Do();
        public abstract void Undo();
    }

    public class TextInsertAction : UndoableAction
    {
        private readonly Action _doInsert;
        private readonly Action _undoInsert;

        public TextInsertAction(string text, Action doInsert, Action undoInsert)
        {
            Description = $"Insert: {text}";
            _doInsert = doInsert;
            _undoInsert = undoInsert;
        }

        public override void Do() => _doInsert?.Invoke();
        public override void Undo() => _undoInsert?.Invoke();
    }

    public class TextDeleteAction : UndoableAction
    {
        private readonly string _deletedText;
        private readonly Action _doDelete;
        private readonly Action<string> _undoDelete;

        public TextDeleteAction(string deletedText, Action doDelete, Action<string> undoDelete)
        {
            Description = $"Delete: {deletedText}";
            _deletedText = deletedText;
            _doDelete = doDelete;
            _undoDelete = undoDelete;
        }

        public override void Do() => _doDelete?.Invoke();
        public override void Undo() => _undoDelete?.Invoke(_deletedText);
    }

    public class FormattingAction : UndoableAction
    {
        private readonly Action _doFormat;
        private readonly Action _undoFormat;

        public FormattingAction(string formatType, Action doFormat, Action undoFormat)
        {
            Description = $"Format: {formatType}";
            _doFormat = doFormat;
            _undoFormat = undoFormat;
        }

        public override void Do() => _doFormat?.Invoke();
        public override void Undo() => _undoFormat?.Invoke();
    }

    public class DocumentComparisonService
    {
        public ComparisonResult Compare(string text1, string text2)
        {
            var result = new ComparisonResult();
            var lines1 = text1.Split('\n');
            var lines2 = text2.Split('\n');

            int maxLines = Math.Max(lines1.Length, lines2.Length);
            for (int i = 0; i < maxLines; i++)
            {
                string line1 = i < lines1.Length ? lines1[i].TrimEnd('\r') : null;
                string line2 = i < lines2.Length ? lines2[i].TrimEnd('\r') : null;

                if (line1 == null)
                {
                    result.Changes.Add(new LineChange
                    {
                        LineNumber = i + 1,
                        ChangeType = ChangeType.Added,
                        Text = line2
                    });
                }
                else if (line2 == null)
                {
                    result.Changes.Add(new LineChange
                    {
                        LineNumber = i + 1,
                        ChangeType = ChangeType.Deleted,
                        Text = line1
                    });
                }
                else if (line1 != line2)
                {
                    result.Changes.Add(new LineChange
                    {
                        LineNumber = i + 1,
                        ChangeType = ChangeType.Modified,
                        OldText = line1,
                        NewText = line2
                    });
                }
            }

            result.Similarity = CalculateSimilarity(text1, text2);
            return result;
        }

        private double CalculateSimilarity(string text1, string text2)
        {
            if (string.IsNullOrEmpty(text1) && string.IsNullOrEmpty(text2))
                return 1.0;

            if (string.IsNullOrEmpty(text1) || string.IsNullOrEmpty(text2))
                return 0.0;

            int distance = LevenshteinDistance(text1, text2);
            int maxLength = Math.Max(text1.Length, text2.Length);
            return 1.0 - (double)distance / maxLength;
        }

        private int LevenshteinDistance(string s1, string s2)
        {
            int[,] d = new int[s1.Length + 1, s2.Length + 1];

            for (int i = 0; i <= s1.Length; i++)
                d[i, 0] = i;
            for (int j = 0; j <= s2.Length; j++)
                d[0, j] = j;

            for (int j = 1; j <= s2.Length; j++)
            {
                for (int i = 1; i <= s1.Length; i++)
                {
                    int cost = (s1[i - 1] == s2[j - 1]) ? 0 : 1;
                    d[i, j] = Math.Min(
                        Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + cost);
                }
            }

            return d[s1.Length, s2.Length];
        }
    }

    public class ComparisonResult
    {
        public List<LineChange> Changes { get; } = new List<LineChange>();
        public double Similarity { get; set; }
        public bool HasChanges => Changes.Count > 0;
        public int AddedCount => Changes.Count(c => c.ChangeType == ChangeType.Added);
        public int DeletedCount => Changes.Count(c => c.ChangeType == ChangeType.Deleted);
        public int ModifiedCount => Changes.Count(c => c.ChangeType == ChangeType.Modified);
    }

    public class LineChange
    {
        public int LineNumber { get; set; }
        public ChangeType ChangeType { get; set; }
        public string Text { get; set; }
        public string OldText { get; set; }
        public string NewText { get; set; }
    }

    public enum ChangeType
    {
        Added,
        Deleted,
        Modified
    }

    public class MailMergeService
    {
        private readonly Dictionary<string, MailMergeField> _fields = new Dictionary<string, MailMergeField>();
        private readonly List<Dictionary<string, object>> _dataSource = new List<Dictionary<string, object>>();

        public void AddField(string name, MailMergeField field)
        {
            _fields[name] = field;
        }

        public void SetDataSource(IEnumerable<Dictionary<string, object>> data)
        {
            _dataSource.Clear();
            _dataSource.AddRange(data);
        }

        public IEnumerable<string> GetFieldNames()
        {
            return _fields.Keys;
        }

        public string Merge(string template)
        {
            if (_dataSource.Count == 0)
                return template;

            var data = _dataSource[0];
            return MergeWithData(template, data);
        }

        public IEnumerable<string> MergeAll()
        {
            foreach (var data in _dataSource)
            {
                yield return MergeWithData(_fields.Values.FirstOrDefault()?.Template ?? string.Empty, data);
            }
        }

        private string MergeWithData(string template, Dictionary<string, object> data)
        {
            var result = template;
            foreach (var field in _fields)
            {
                string placeholder = $"{{{field.Key}}}";
                string value = data.TryGetValue(field.Key, out var val) ? val?.ToString() ?? string.Empty : string.Empty;
                result = result.Replace(placeholder, value);
            }
            return result;
        }

        public int RecordCount => _dataSource.Count;
    }

    public class MailMergeField
    {
        public string Name { get; set; }
        public string Template { get; set; }
        public MailMergeFieldType FieldType { get; set; }
    }

    public enum MailMergeFieldType
    {
        Text,
        Number,
        Date,
        Boolean,
        Image
    }

    public class TableOfContentsService
    {
        public string Generate(IEnumerable<TocEntry> entries, TocOptions options = null)
        {
            options ??= new TocOptions();
            var sb = new StringBuilder();

            if (options.IncludeTitle)
            {
                sb.AppendLine(options.Title);
                sb.AppendLine(new string('=', options.Title.Length));
                sb.AppendLine();
            }

            foreach (var entry in entries)
            {
                int indent = entry.Level * options.IndentSize;
                string indentedText = new string(' ', indent) + entry.Text;

                if (options.IncludePageNumbers)
                {
                    string pageNumber = entry.PageNumber.ToString().PadLeft(options.MinPageWidth);
                    sb.Append(indentedText);
                    if (options.TabLeader != TabLeader.None)
                        sb.Append(new string(GetTabLeaderChar(options.TabLeader), options.TabWidth));
                    else
                        sb.Append(' ', options.TabWidth);
                    sb.AppendLine(pageNumber);
                }
                else
                {
                    sb.AppendLine(indentedText);
                }
            }

            return sb.ToString();
        }

        private char GetTabLeaderChar(TabLeader leader)
        {
            return leader switch
            {
                TabLeader.Dots => '.',
                TabLeader.Dashes => '-',
                TabLeader.Lines => '_',
                _ => ' '
            };
        }

        public IEnumerable<TocEntry> ExtractFromDocument(string content, string headingPattern)
        {
            var entries = new List<TocEntry>();
            var lines = content.Split('\n');

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                int level = GetHeadingLevel(line, headingPattern);
                if (level > 0)
                {
                    entries.Add(new TocEntry
                    {
                        Text = line.TrimStart('#').Trim(),
                        Level = level,
                        PageNumber = (i / 20) + 1
                    });
                }
            }

            return entries;
        }

        private int GetHeadingLevel(string line, string pattern)
        {
            if (pattern.StartsWith("#"))
            {
                int level = 0;
                foreach (char c in line)
                {
                    if (c == '#')
                        level++;
                    else
                        break;
                }
                return level;
            }

            return 0;
        }
    }

    public class TocEntry
    {
        public string Text { get; set; }
        public int Level { get; set; }
        public int PageNumber { get; set; }
    }

    public class TocOptions
    {
        public string Title { get; set; } = "Table of Contents";
        public bool IncludeTitle { get; set; } = true;
        public bool IncludePageNumbers { get; set; } = true;
        public int MinPageWidth { get; set; } = 4;
        public TabLeader TabLeader { get; set; } = TabLeader.Dots;
        public int TabWidth { get; set; } = 8;
        public int IndentSize { get; set; } = 2;
    }

    public enum TabLeader
    {
        None,
        Dots,
        Dashes,
        Lines
    }

    public class SectionBreakService
    {
        private readonly List<Section> _sections = new List<Section>();

        public void AddSection(Section section)
        {
            _sections.Add(section);
        }

        public void RemoveSection(int index)
        {
            if (index >= 0 && index < _sections.Count)
                _sections.RemoveAt(index);
        }

        public Section GetSection(int index)
        {
            return index >= 0 && index < _sections.Count ? _sections[index] : null;
        }

        public int SectionCount => _sections.Count;

        public IEnumerable<Section> GetAllSections()
        {
            return _sections.AsReadOnly();
        }

        public void ApplySectionProperties(int sectionIndex, SectionProperties properties)
        {
            var section = GetSection(sectionIndex);
            if (section != null)
                section.Properties = properties;
        }
    }

    public class Section
    {
        public int Index { get; set; }
        public string Content { get; set; }
        public SectionProperties Properties { get; set; }
        public SectionType Type { get; set; }
    }

    public class SectionProperties
    {
        public SectionOrientation Orientation { get; set; } = SectionOrientation.Portrait;
        public double PageWidth { get; set; } = 612;
        public double PageHeight { get; set; } = 792;
        public double LeftMargin { get; set; } = 72;
        public double RightMargin { get; set; } = 72;
        public double TopMargin { get; set; } = 72;
        public double BottomMargin { get; set; } = 72;
        public double HeaderDistance { get; set; } = 36;
        public double FooterDistance { get; set; } = 36;
        public SectionStartType StartType { get; set; } = SectionStartType.NewPage;
        public bool DifferentFirstPage { get; set; }
        public bool DifferentOddEvenPages { get; set; }
        public int LineNumberingCountBy { get; set; } = 1;
        public LineNumberingRestart RestartType { get; set; } = LineNumberingRestart.NewPage;
        public double LineNumberingDistance { get; set; } = 0;
    }

    public enum SectionOrientation
    {
        Portrait,
        Landscape
    }

    public enum SectionStartType
    {
        NewPage,
        Continuous,
        EvenPage,
        OddPage
    }

    public enum LineNumberingRestart
    {
        NewPage,
        NewSection,
        Continuous
    }

    public enum SectionType
    {
        Normal,
        Title,
        TableOfContents,
        Index,
        Bibliography
    }
}

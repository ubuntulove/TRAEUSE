using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using OfficeSuite.Presentation.Enums;

namespace OfficeSuite.Presentation.Themes
{
    public class PresentationTheme
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ThemeCategory Category { get; set; }
        public ThemeColors Colors { get; set; }
        public ThemeFonts Fonts { get; set; }
        public ThemeEffects Effects { get; set; }
        public ThemeBackgrounds Backgrounds { get; set; }
        public Dictionary<string, ThemeVariant> Variants { get; set; }
        public bool IsBuiltIn { get; set; }
        public string ThumbnailPath { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Author { get; set; }
        public List<string> Tags { get; set; }
        public int UsageCount { get; set; }
        public bool IsReadOnly { get; set; }
        public ThemeMetadata Metadata { get; set; }

        public PresentationTheme()
        {
            Id = Guid.NewGuid().ToString();
            Colors = new ThemeColors();
            Fonts = new ThemeFonts();
            Effects = new ThemeEffects();
            Backgrounds = new ThemeBackgrounds();
            Variants = new Dictionary<string, ThemeVariant>();
            Tags = new List<string>();
            Metadata = new ThemeMetadata();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            IsBuiltIn = false;
            IsReadOnly = false;
        }

        public ThemeVariant GetVariant(string variantName)
        {
            return Variants.TryGetValue(variantName, out var variant) ? variant : null;
        }

        public void AddVariant(string name, ThemeVariant variant)
        {
            Variants[name] = variant;
            ModifiedDate = DateTime.Now;
        }

        public PresentationTheme Clone()
        {
            return new PresentationTheme
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"{Name} (Copy)",
                Description = Description,
                Category = Category,
                Colors = Colors.Clone(),
                Fonts = Fonts.Clone(),
                Effects = Effects.Clone(),
                Backgrounds = Backgrounds.Clone(),
                Variants = Variants.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Clone()),
                IsBuiltIn = false,
                Author = Author,
                Tags = new List<string>(Tags),
                Metadata = Metadata.Clone()
            };
        }
    }

    public class ThemeColors
    {
        public string Primary { get; set; }
        public string Secondary { get; set; }
        public string Accent { get; set; }
        public string Background { get; set; }
        public string Foreground { get; set; }
        public string Text { get; set; }
        public string TextSecondary { get; set; }
        public string TextLight { get; set; }
        public string Border { get; set; }
        public string BorderLight { get; set; }
        public string Shadow { get; set; }
        public string Highlight { get; set; }
        public string Selection { get; set; }
        public string Error { get; set; }
        public string Warning { get; set; }
        public string Success { get; set; }
        public string Info { get; set; }
        public string Link { get; set; }
        public string LinkVisited { get; set; }
        public string Heading1 { get; set; }
        public string Heading2 { get; set; }
        public string Heading3 { get; set; }
        public string Heading4 { get; set; }
        public string Heading5 { get; set; }
        public string Heading6 { get; set; }
        public string ListBullet { get; set; }
        public string ListNumber { get; set; }
        public string TableBorder { get; set; }
        public string TableBackground { get; set; }
        public string TableBackgroundAlt { get; set; }
        public string ChartColors { get; set; }
        public string ChartColor1 { get; set; }
        public string ChartColor2 { get; set; }
        public string ChartColor3 { get; set; }
        public string ChartColor4 { get; set; }
        public string ChartColor5 { get; set; }
        public string ChartColor6 { get; set; }

        public ThemeColors()
        {
            Primary = "#0078D4";
            Secondary = "#106EBE";
            Accent = "#FFB900";
            Background = "#FFFFFF";
            Foreground = "#323130";
            Text = "#323130";
            TextSecondary = "#605E5C";
            TextLight = "#A19F9D";
            Border = "#E1DFDD";
            BorderLight = "#F3F2F1";
            Shadow = "#000000";
            Highlight = "#FFF3CD";
            Selection = "#0078D4";
            Error = "#D13438";
            Warning = "#FFB900";
            Success = "#107C10";
            Info = "#0078D4";
            Link = "#0078D4";
            LinkVisited = "#4B0082";
            ChartColor1 = "#0078D4";
            ChartColor2 = "#00B294";
            ChartColor3 = "#F7630C";
            ChartColor4 = "#744DA9";
            ChartColor5 = "#00A4EF";
            ChartColor6 = "#E74856";
        }

        public Color GetPrimaryColor() => (Color)ColorConverter.ConvertFromString(Primary);
        public Color GetSecondaryColor() => (Color)ColorConverter.ConvertFromString(Secondary);
        public Color GetAccentColor() => (Color)ColorConverter.ConvertFromString(Accent);

        public ThemeColors Clone()
        {
            return new ThemeColors
            {
                Primary = Primary,
                Secondary = Secondary,
                Accent = Accent,
                Background = Background,
                Foreground = Foreground,
                Text = Text,
                TextSecondary = TextSecondary,
                TextLight = TextLight,
                Border = Border,
                BorderLight = BorderLight,
                Shadow = Shadow,
                Highlight = Highlight,
                Selection = Selection,
                Error = Error,
                Warning = Warning,
                Success = Success,
                Info = Info,
                Link = Link,
                LinkVisited = LinkVisited,
                Heading1 = Heading1,
                Heading2 = Heading2,
                Heading3 = Heading3,
                Heading4 = Heading4,
                Heading5 = Heading5,
                Heading6 = Heading6,
                ListBullet = ListBullet,
                ListNumber = ListNumber,
                TableBorder = TableBorder,
                TableBackground = TableBackground,
                TableBackgroundAlt = TableBackgroundAlt,
                ChartColors = ChartColors,
                ChartColor1 = ChartColor1,
                ChartColor2 = ChartColor2,
                ChartColor3 = ChartColor3,
                ChartColor4 = ChartColor4,
                ChartColor5 = ChartColor5,
                ChartColor6 = ChartColor6
            };
        }

        public Dictionary<string, Color> GetAllColors()
        {
            return new Dictionary<string, Color>
            {
                ["Primary"] = GetPrimaryColor(),
                ["Secondary"] = GetSecondaryColor(),
                ["Accent"] = GetAccentColor(),
                ["Background"] = (Color)ColorConverter.ConvertFromString(Background),
                ["Foreground"] = (Color)ColorConverter.ConvertFromString(Foreground),
                ["Text"] = (Color)ColorConverter.ConvertFromString(Text),
                ["TextSecondary"] = (Color)ColorConverter.ConvertFromString(TextSecondary),
                ["TextLight"] = (Color)ColorConverter.ConvertFromString(TextLight),
                ["Border"] = (Color)ColorConverter.ConvertFromString(Border),
                ["Error"] = (Color)ColorConverter.ConvertFromString(Error),
                ["Warning"] = (Color)ColorConverter.ConvertFromString(Warning),
                ["Success"] = (Color)ColorConverter.ConvertFromString(Success)
            };
        }
    }

    public class ThemeFonts
    {
        public string PrimaryFontFamily { get; set; }
        public string SecondaryFontFamily { get; set; }
        public string HeadingFontFamily { get; set; }
        public string BodyFontFamily { get; set; }
        public string MonoFontFamily { get; set; }
        public double HeadingSize { get; set; }
        public double SubheadingSize { get; set; }
        public double BodySize { get; set; }
        public double SmallSize { get; set; }
        public double CaptionSize { get; set; }
        public FontWeight HeadingWeight { get; set; }
        public FontWeight BodyWeight { get; set; }
        public double LineSpacing { get; set; }
        public double LetterSpacing { get; set; }

        public ThemeFonts()
        {
            PrimaryFontFamily = "Calibri";
            SecondaryFontFamily = "Segoe UI";
            HeadingFontFamily = "Calibri Light";
            BodyFontFamily = "Calibri";
            MonoFontFamily = "Consolas";
            HeadingSize = 28;
            SubheadingSize = 20;
            BodySize = 14;
            SmallSize = 12;
            CaptionSize = 10;
            HeadingWeight = FontWeights.Bold;
            BodyWeight = FontWeights.Normal;
            LineSpacing = 1.5;
            LetterSpacing = 0;
        }

        public FontFamily GetPrimaryFont() => new FontFamily(PrimaryFontFamily);
        public FontFamily GetHeadingFont() => new FontFamily(HeadingFontFamily);
        public FontFamily GetBodyFont() => new FontFamily(BodyFontFamily);

        public ThemeFonts Clone()
        {
            return new ThemeFonts
            {
                PrimaryFontFamily = PrimaryFontFamily,
                SecondaryFontFamily = SecondaryFontFamily,
                HeadingFontFamily = HeadingFontFamily,
                BodyFontFamily = BodyFontFamily,
                MonoFontFamily = MonoFontFamily,
                HeadingSize = HeadingSize,
                SubheadingSize = SubheadingSize,
                BodySize = BodySize,
                SmallSize = SmallSize,
                CaptionSize = CaptionSize,
                HeadingWeight = HeadingWeight,
                BodyWeight = BodyWeight,
                LineSpacing = LineSpacing,
                LetterSpacing = LetterSpacing
            };
        }
    }

    public class ThemeEffects
    {
        public double ShadowBlur { get; set; }
        public double ShadowOffset { get; set; }
        public double ShadowOpacity { get; set; }
        public double CornerRadius { get; set; }
        public double BorderWidth { get; set; }
        public bool EnableGradients { get; set; }
        public bool EnableAnimations { get; set; }
        public AnimationDuration DefaultAnimationDuration { get; set; }
        public AnimationEasing DefaultEasing { get; set; }

        public ThemeEffects()
        {
            ShadowBlur = 8;
            ShadowOffset = 4;
            ShadowOpacity = 0.2;
            CornerRadius = 4;
            BorderWidth = 1;
            EnableGradients = true;
            EnableAnimations = true;
            DefaultAnimationDuration = AnimationDuration.Medium;
            DefaultEasing = AnimationEasing.EaseOut;
        }

        public ThemeEffects Clone()
        {
            return new ThemeEffects
            {
                ShadowBlur = ShadowBlur,
                ShadowOffset = ShadowOffset,
                ShadowOpacity = ShadowOpacity,
                CornerRadius = CornerRadius,
                BorderWidth = BorderWidth,
                EnableGradients = EnableGradients,
                EnableAnimations = EnableAnimations,
                DefaultAnimationDuration = DefaultAnimationDuration,
                DefaultEasing = DefaultEasing
            };
        }
    }

    public class ThemeBackgrounds
    {
        public string Solid { get; set; }
        public string GradientStart { get; set; }
        public string GradientEnd { get; set; }
        public double GradientAngle { get; set; }
        public string Pattern { get; set; }
        public string Texture { get; set; }
        public bool UseSlideBackground { get; set; }

        public ThemeBackgrounds()
        {
            Solid = "#FFFFFF";
            GradientStart = "#FFFFFF";
            GradientEnd = "#E6E6E6";
            GradientAngle = 90;
            Pattern = "None";
            Texture = "None";
            UseSlideBackground = false;
        }

        public ThemeBackgrounds Clone()
        {
            return new ThemeBackgrounds
            {
                Solid = Solid,
                GradientStart = GradientStart,
                GradientEnd = GradientEnd,
                GradientAngle = GradientAngle,
                Pattern = Pattern,
                Texture = Texture,
                UseSlideBackground = UseSlideBackground
            };
        }
    }

    public class ThemeVariant
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public ThemeColors Colors { get; set; }
        public ThemeFonts Fonts { get; set; }
        public bool IsDark { get; set; }
        public bool IsHighContrast { get; set; }

        public ThemeVariant()
        {
            Name = "Default";
            Colors = new ThemeColors();
            Fonts = new ThemeFonts();
            IsDark = false;
            IsHighContrast = false;
        }

        public ThemeVariant Clone()
        {
            return new ThemeVariant
            {
                Name = Name,
                Description = Description,
                Colors = Colors.Clone(),
                Fonts = Fonts.Clone(),
                IsDark = IsDark,
                IsHighContrast = IsHighContrast
            };
        }
    }

    public class ThemeMetadata
    {
        public string Version { get; set; }
        public string Author { get; set; }
        public string Copyright { get; set; }
        public string License { get; set; }
        public string Website { get; set; }
        public string SupportEmail { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }

        public ThemeMetadata()
        {
            Version = "1.0";
            CustomProperties = new Dictionary<string, string>();
        }

        public ThemeMetadata Clone()
        {
            return new ThemeMetadata
            {
                Version = Version,
                Author = Author,
                Copyright = Copyright,
                License = License,
                Website = Website,
                SupportEmail = SupportEmail,
                CustomProperties = new Dictionary<string, string>(CustomProperties)
            };
        }
    }

    public class ThemeManager
    {
        private Dictionary<string, PresentationTheme> _themes;
        private PresentationTheme _currentTheme;
        private List<ThemeChangedEventHandler> _subscribers;

        public PresentationTheme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                OnThemeChanged(new ThemeChangedEventArgs(value));
            }
        }

        public ThemeManager()
        {
            _themes = new Dictionary<string, PresentationTheme>();
            _subscribers = new List<ThemeChangedEventHandler>();
            InitializeBuiltInThemes();
        }

        private void InitializeBuiltInThemes()
        {
            AddTheme(CreateOfficeTheme());
            AddTheme(CreateDarkTheme());
            AddTheme(CreateLightTheme());
            AddTheme(CreateColorfulTheme());
            AddTheme(CreateHighContrastTheme());
            AddTheme(CreateCorporateTheme());
            AddTheme(CreateCreativeTheme());
            AddTheme(CreateProfessionalTheme());
            AddTheme(CreateMinimalistTheme());
            AddTheme(CreateVibrantTheme());
        }

        private PresentationTheme CreateOfficeTheme()
        {
            return new PresentationTheme
            {
                Id = "office-default",
                Name = "Office",
                Description = "Default Microsoft Office theme",
                Category = ThemeCategory.Professional,
                IsBuiltIn = true,
                Author = "Microsoft",
                Tags = new List<string> { "office", "professional", "default" }
            };
        }

        private PresentationTheme CreateDarkTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "dark-theme",
                Name = "Dark",
                Description = "Dark theme for presentations",
                Category = ThemeCategory.Dark,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Background = "#1E1E1E";
            theme.Colors.Foreground = "#FFFFFF";
            theme.Colors.Primary = "#4FC3F7";
            theme.Colors.Text = "#E0E0E0";
            theme.Colors.IsDark = true;
            return theme;
        }

        private PresentationTheme CreateLightTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "light-theme",
                Name = "Light",
                Description = "Light theme for presentations",
                Category = ThemeCategory.Light,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Background = "#FFFFFF";
            theme.Colors.Foreground = "#333333";
            theme.Colors.Primary = "#2196F3";
            theme.Colors.Text = "#333333";
            theme.Colors.IsDark = false;
            return theme;
        }

        private PresentationTheme CreateColorfulTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "colorful-theme",
                Name = "Colorful",
                Description = "Colorful vibrant theme",
                Category = ThemeCategory.Colorful,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Primary = "#FF6B6B";
            theme.Colors.Secondary = "#4ECDC4";
            theme.Colors.Accent = "#FFE66D";
            theme.Colors.Background = "#F7F7F7";
            return theme;
        }

        private PresentationTheme CreateHighContrastTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "high-contrast",
                Name = "High Contrast",
                Description = "High contrast theme for accessibility",
                Category = ThemeCategory.Accessibility,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Background = "#000000";
            theme.Colors.Foreground = "#FFFFFF";
            theme.Colors.Primary = "#FFFF00";
            theme.Colors.Text = "#FFFFFF";
            theme.Colors.IsHighContrast = true;
            return theme;
        }

        private PresentationTheme CreateCorporateTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "corporate-theme",
                Name = "Corporate",
                Description = "Corporate professional theme",
                Category = ThemeCategory.Professional,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Primary = "#003366";
            theme.Colors.Secondary = "#006699";
            theme.Colors.Accent = "#CC9900";
            theme.Colors.Background = "#F5F5F5";
            return theme;
        }

        private PresentationTheme CreateCreativeTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "creative-theme",
                Name = "Creative",
                Description = "Creative and artistic theme",
                Category = ThemeCategory.Creative,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Primary = "#9C27B0";
            theme.Colors.Secondary = "#E91E63";
            theme.Colors.Accent = "#FF9800";
            theme.Colors.Background = "#FAFAFA";
            return theme;
        }

        private PresentationTheme CreateProfessionalTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "professional-theme",
                Name = "Professional Blue",
                Description = "Professional blue theme",
                Category = ThemeCategory.Professional,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Primary = "#1976D2";
            theme.Colors.Secondary = "#424242";
            theme.Colors.Accent = "#FFC107";
            theme.Colors.Background = "#ECEFF1";
            return theme;
        }

        private PresentationTheme CreateMinimalistTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "minimalist-theme",
                Name = "Minimalist",
                Description = "Minimalist clean theme",
                Category = ThemeCategory.Minimal,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Primary = "#212121";
            theme.Colors.Secondary = "#757575";
            theme.Colors.Accent = "#2196F3";
            theme.Colors.Background = "#FFFFFF";
            return theme;
        }

        private PresentationTheme CreateVibrantTheme()
        {
            var theme = new PresentationTheme
            {
                Id = "vibrant-theme",
                Name = "Vibrant",
                Description = "Vibrant energetic theme",
                Category = ThemeCategory.Colorful,
                IsBuiltIn = true,
                Author = "OfficeSuite"
            };
            theme.Colors.Primary = "#FF5722";
            theme.Colors.Secondary = "#00BCD4";
            theme.Colors.Accent = "#8BC34A";
            theme.Colors.Background = "#FAFAFA";
            return theme;
        }

        public void AddTheme(PresentationTheme theme)
        {
            _themes[theme.Id] = theme;
        }

        public void RemoveTheme(string themeId)
        {
            var theme = GetTheme(themeId);
            if (theme != null && !theme.IsReadOnly)
            {
                _themes.Remove(themeId);
            }
        }

        public PresentationTheme GetTheme(string themeId)
        {
            return _themes.TryGetValue(themeId, out var theme) ? theme : null;
        }

        public IEnumerable<PresentationTheme> GetAllThemes()
        {
            return _themes.Values;
        }

        public IEnumerable<PresentationTheme> GetThemesByCategory(ThemeCategory category)
        {
            return _themes.Values.Where(t => t.Category == category);
        }

        public IEnumerable<PresentationTheme> SearchThemes(string query)
        {
            var lowerQuery = query.ToLowerInvariant();
            return _themes.Values.Where(t =>
                t.Name.ToLowerInvariant().Contains(lowerQuery) ||
                t.Description.ToLowerInvariant().Contains(lowerQuery) ||
                t.Tags.Any(tag => tag.ToLowerInvariant().Contains(lowerQuery)));
        }

        public void ApplyTheme(string themeId)
        {
            var theme = GetTheme(themeId);
            if (theme != null)
            {
                CurrentTheme = theme;
                theme.UsageCount++;
            }
        }

        public PresentationTheme CreateCustomTheme(string name, ThemeColors colors, ThemeFonts fonts)
        {
            var theme = new PresentationTheme
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Description = "Custom theme",
                Category = ThemeCategory.Custom,
                Colors = colors,
                Fonts = fonts,
                IsBuiltIn = false,
                Author = "User"
            };
            AddTheme(theme);
            return theme;
        }

        public void Subscribe(ThemeChangedEventHandler handler)
        {
            _subscribers.Add(handler);
        }

        public void Unsubscribe(ThemeChangedEventHandler handler)
        {
            _subscribers.Remove(handler);
        }

        protected virtual void OnThemeChanged(ThemeChangedEventArgs e)
        {
            foreach (var handler in _subscribers)
            {
                handler(this, e);
            }
        }

        public void ExportTheme(string themeId, string filePath)
        {
            var theme = GetTheme(themeId);
            if (theme != null)
            {
                var serializer = new System.Text.Json.JsonSerializer();
            }
        }

        public PresentationTheme ImportTheme(string filePath)
        {
            return null;
        }

        public IEnumerable<PresentationTheme> GetRecentThemes(int count = 5)
        {
            return _themes.Values
                .OrderByDescending(t => t.UsageCount)
                .Take(count);
        }

        public void UpdateTheme(PresentationTheme theme)
        {
            if (_themes.ContainsKey(theme.Id))
            {
                theme.ModifiedDate = DateTime.Now;
                _themes[theme.Id] = theme;
            }
        }

        public bool ThemeExists(string themeId)
        {
            return _themes.ContainsKey(themeId);
        }

        public int GetThemeCount()
        {
            return _themes.Count;
        }

        public void ClearCustomThemes()
        {
            var toRemove = _themes.Values.Where(t => !t.IsBuiltIn).Select(t => t.Id).ToList();
            foreach (var id in toRemove)
            {
                _themes.Remove(id);
            }
        }

        public ThemeStatistics GetStatistics()
        {
            return new ThemeStatistics
            {
                TotalThemes = _themes.Count,
                BuiltInThemes = _themes.Values.Count(t => t.IsBuiltIn),
                CustomThemes = _themes.Values.Count(t => !t.IsBuiltIn),
                Categories = _themes.Values.GroupBy(t => t.Category).ToDictionary(g => g.Key, g => g.Count()),
                MostUsedTheme = _themes.Values.OrderByDescending(t => t.UsageCount).FirstOrDefault()?.Name
            };
        }
    }

    public class ThemeChangedEventArgs : EventArgs
    {
        public PresentationTheme Theme { get; }
        public DateTime ChangedAt { get; }

        public ThemeChangedEventArgs(PresentationTheme theme)
        {
            Theme = theme;
            ChangedAt = DateTime.Now;
        }
    }

    public delegate void ThemeChangedEventHandler(object sender, ThemeChangedEventArgs e);

    public class ThemeStatistics
    {
        public int TotalThemes { get; set; }
        public int BuiltInThemes { get; set; }
        public int CustomThemes { get; set; }
        public Dictionary<ThemeCategory, int> Categories { get; set; }
        public string MostUsedTheme { get; set; }
    }

    namespace Enums
    {
        public enum ThemeCategory
        {
            Professional,
            Dark,
            Light,
            Colorful,
            Creative,
            Minimal,
            Corporate,
            Accessibility,
            Custom
        }

        public enum AnimationDuration
        {
            Short,
            Medium,
            Long
        }

        public enum AnimationEasing
        {
            Linear,
            EaseIn,
            EaseOut,
            EaseInOut,
            Bounce,
            Elastic
        }
    }

    public class ThemePreset
    {
        public string Name { get; set; }
        public PresentationTheme Theme { get; set; }
        public List<string> RecommendedSlideTypes { get; set; }

        public ThemePreset()
        {
            RecommendedSlideTypes = new List<string>();
        }

        public static ThemePreset CreateTitleSlidePreset(PresentationTheme theme)
        {
            return new ThemePreset
            {
                Name = "Title Slide",
                Theme = theme,
                RecommendedSlideTypes = new List<string> { "Title", "Title and Subtitle" }
            };
        }

        public static ThemePreset CreateContentSlidePreset(PresentationTheme theme)
        {
            return new ThemePreset
            {
                Name = "Content Slide",
                Theme = theme,
                RecommendedSlideTypes = new List<string> { "Title and Content", "Two Content", "Comparison" }
            };
        }

        public static ThemePreset CreateSectionHeaderPreset(PresentationTheme theme)
        {
            return new ThemePreset
            {
                Name = "Section Header",
                Theme = theme,
                RecommendedSlideTypes = new List<string> { "Section Header", "Section" }
            };
        }
    }

    public class ThemePresetLibrary
    {
        private Dictionary<string, ThemePreset> _presets;

        public ThemePresetLibrary()
        {
            _presets = new Dictionary<string, ThemePreset>();
            InitializePresets();
        }

        private void InitializePresets()
        {
            _presets["Title Slide"] = ThemePreset.CreateTitleSlidePreset(new PresentationTheme());
            _presets["Content Slide"] = ThemePreset.CreateContentSlidePreset(new PresentationTheme());
            _presets["Section Header"] = ThemePreset.CreateSectionHeaderPreset(new PresentationTheme());
        }

        public ThemePreset GetPreset(string name)
        {
            return _presets.TryGetValue(name, out var preset) ? preset : null;
        }

        public void AddPreset(ThemePreset preset)
        {
            _presets[preset.Name] = preset;
        }

        public IEnumerable<ThemePreset> GetAllPresets()
        {
            return _presets.Values;
        }
    }
}

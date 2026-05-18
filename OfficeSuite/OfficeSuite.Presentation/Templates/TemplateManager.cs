using System;
using System.Collections.Generic;
using System.Linq;
using OfficeSuite.Presentation.Enums;

namespace OfficeSuite.Presentation.Templates
{
    public class SlideTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SlideType Type { get; set; }
        public SlideLayout Layout { get; set; }
        public SlideTheme Theme { get; set; }
        public List<PlaceholderDefinition> Placeholders { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public bool IsBuiltIn { get; set; }
        public string ThumbnailPath { get; set; }
        public byte[] ThumbnailData { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public string Author { get; set; }
        public List<string> Tags { get; set; }
        public int UsageCount { get; set; }

        public SlideTemplate()
        {
            Id = Guid.NewGuid().ToString();
            Placeholders = new List<PlaceholderDefinition>();
            Properties = new Dictionary<string, object>();
            Tags = new List<string>();
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            IsBuiltIn = false;
        }

        public SlideTemplate Clone()
        {
            return new SlideTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Name = $"{Name} (Copy)",
                Description = Description,
                Type = Type,
                Layout = Layout?.Clone(),
                Theme = Theme?.Clone(),
                Placeholders = Placeholders.Select(p => p.Clone()).ToList(),
                Properties = new Dictionary<string, object>(Properties),
                IsBuiltIn = false,
                Author = Author,
                Tags = new List<string>(Tags)
            };
        }

        public void AddPlaceholder(PlaceholderDefinition placeholder)
        {
            Placeholders.Add(placeholder);
            ModifiedDate = DateTime.Now;
        }

        public void RemovePlaceholder(string placeholderId)
        {
            var placeholder = Placeholders.FirstOrDefault(p => p.Id == placeholderId);
            if (placeholder != null)
            {
                Placeholders.Remove(placeholder);
                ModifiedDate = DateTime.Now;
            }
        }

        public PlaceholderDefinition GetPlaceholder(string placeholderId)
        {
            return Placeholders.FirstOrDefault(p => p.Id == placeholderId);
        }
    }

    public class PlaceholderDefinition
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public PlaceholderType Type { get; set; }
        public PlaceholderPosition Position { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsVisible { get; set; }
        public bool IsLocked { get; set; }
        public bool IsRequired { get; set; }
        public string DefaultContent { get; set; }
        public Dictionary<string, object> Attributes { get; set; }
        public List<string> AllowedContentTypes { get; set; }

        public PlaceholderDefinition()
        {
            Id = Guid.NewGuid().ToString();
            IsVisible = true;
            IsLocked = false;
            IsRequired = false;
            Attributes = new Dictionary<string, object>();
            AllowedContentTypes = new List<string>();
        }

        public PlaceholderDefinition Clone()
        {
            return new PlaceholderDefinition
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Type = Type,
                Position = Position,
                Left = Left,
                Top = Top,
                Width = Width,
                Height = Height,
                IsVisible = IsVisible,
                IsLocked = IsLocked,
                IsRequired = IsRequired,
                DefaultContent = DefaultContent,
                Attributes = new Dictionary<string, object>(Attributes),
                AllowedContentTypes = new List<string>(AllowedContentTypes)
            };
        }
    }

    public class SlideLayout
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SlideType Type { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public List<LayoutElement> Elements { get; set; }
        public SlideBackground Background { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public Dictionary<string, string> Footers { get; set; }
        public SlideNumberPosition SlideNumberPosition { get; set; }
        public bool ShowSlideNumber { get; set; }
        public bool ShowDateTime { get; set; }
        public bool ShowFooter { get; set; }

        public SlideLayout()
        {
            Id = Guid.NewGuid().ToString();
            Elements = new List<LayoutElement>();
            Headers = new Dictionary<string, string>();
            Footers = new Dictionary<string, string>();
            Width = 960;
            Height = 540;
            ShowSlideNumber = true;
            ShowDateTime = true;
            ShowFooter = true;
        }

        public SlideLayout Clone()
        {
            return new SlideLayout
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Description = Description,
                Type = Type,
                Width = Width,
                Height = Height,
                Elements = Elements.Select(e => e.Clone()).ToList(),
                Background = Background?.Clone(),
                Headers = new Dictionary<string, string>(Headers),
                Footers = new Dictionary<string, string>(Footers),
                SlideNumberPosition = SlideNumberPosition,
                ShowSlideNumber = ShowSlideNumber,
                ShowDateTime = ShowDateTime,
                ShowFooter = ShowFooter
            };
        }

        public void AddElement(LayoutElement element)
        {
            Elements.Add(element);
        }

        public LayoutElement GetElement(string elementId)
        {
            return Elements.FirstOrDefault(e => e.Id == elementId);
        }

        public void RemoveElement(string elementId)
        {
            var element = GetElement(elementId);
            if (element != null)
            {
                Elements.Remove(element);
            }
        }
    }

    public class LayoutElement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public LayoutElementType Type { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
        public double Opacity { get; set; }
        public bool IsVisible { get; set; }
        public bool IsLocked { get; set; }
        public LayoutElementStyle Style { get; set; }
        public string PlaceholderId { get; set; }

        public LayoutElement()
        {
            Id = Guid.NewGuid().ToString();
            IsVisible = true;
            IsLocked = false;
            Opacity = 1.0;
            Rotation = 0;
            Style = new LayoutElementStyle();
        }

        public LayoutElement Clone()
        {
            return new LayoutElement
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                Type = Type,
                Left = Left,
                Top = Top,
                Width = Width,
                Height = Height,
                Rotation = Rotation,
                Opacity = Opacity,
                IsVisible = IsVisible,
                IsLocked = IsLocked,
                Style = Style.Clone(),
                PlaceholderId = PlaceholderId
            };
        }
    }

    public class LayoutElementStyle
    {
        public string FillColor { get; set; }
        public string StrokeColor { get; set; }
        public double StrokeWidth { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public string FontColor { get; set; }
        public bool FontBold { get; set; }
        public bool FontItalic { get; set; }
        public TextAlignment TextAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public double CornerRadius { get; set; }
        public string ShadowColor { get; set; }
        public double ShadowBlur { get; set; }
        public double ShadowOffsetX { get; set; }
        public double ShadowOffsetY { get; set; }

        public LayoutElementStyle()
        {
            FillColor = "#FFFFFF";
            StrokeColor = "#000000";
            StrokeWidth = 0;
            FontFamily = "Calibri";
            FontSize = 14;
            FontColor = "#000000";
            FontBold = false;
            FontItalic = false;
            TextAlignment = TextAlignment.Left;
            VerticalAlignment = VerticalAlignment.Top;
            CornerRadius = 0;
        }

        public LayoutElementStyle Clone()
        {
            return new LayoutElementStyle
            {
                FillColor = FillColor,
                StrokeColor = StrokeColor,
                StrokeWidth = StrokeWidth,
                FontFamily = FontFamily,
                FontSize = FontSize,
                FontColor = FontColor,
                FontBold = FontBold,
                FontItalic = FontItalic,
                TextAlignment = TextAlignment,
                VerticalAlignment = VerticalAlignment,
                CornerRadius = CornerRadius,
                ShadowColor = ShadowColor,
                ShadowBlur = ShadowBlur,
                ShadowOffsetX = ShadowOffsetX,
                ShadowOffsetY = ShadowOffsetY
            };
        }
    }

    public class SlideTheme
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ThemeColorScheme ColorScheme { get; set; }
        public ThemeFontScheme FontScheme { get; set; }
        public ThemeEffectScheme EffectScheme { get; set; }
        public SlideBackground Background { get; set; }
        public Dictionary<string, object> CustomProperties { get; set; }

        public SlideTheme()
        {
            Id = Guid.NewGuid().ToString();
            ColorScheme = new ThemeColorScheme();
            FontScheme = new ThemeFontScheme();
            EffectScheme = new ThemeEffectScheme();
            CustomProperties = new Dictionary<string, object>();
        }

        public SlideTheme Clone()
        {
            return new SlideTheme
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                ColorScheme = ColorScheme.Clone(),
                FontScheme = FontScheme.Clone(),
                EffectScheme = EffectScheme.Clone(),
                Background = Background?.Clone(),
                CustomProperties = new Dictionary<string, object>(CustomProperties)
            };
        }
    }

    public class ThemeColorScheme
    {
        public string Name { get; set; }
        public string BackgroundColor { get; set; }
        public string TextColor { get; set; }
        public string AccentColor1 { get; set; }
        public string AccentColor2 { get; set; }
        public string AccentColor3 { get; set; }
        public string AccentColor4 { get; set; }
        public string AccentColor5 { get; set; }
        public string AccentColor6 { get; set; }
        public string HyperlinkColor { get; set; }
        public string FollowedHyperlinkColor { get; set; }

        public ThemeColorScheme()
        {
            Name = "Office";
            BackgroundColor = "#FFFFFF";
            TextColor = "#000000";
            AccentColor1 = "#0078D4";
            AccentColor2 = "#00B294";
            AccentColor3 = "#F7630C";
            AccentColor4 = "#744DA9";
            AccentColor5 = "#00A4EF";
            AccentColor6 = "#E74856";
            HyperlinkColor = "#0078D4";
            FollowedHyperlinkColor = "#4B0082";
        }

        public ThemeColorScheme Clone()
        {
            return new ThemeColorScheme
            {
                Name = Name,
                BackgroundColor = BackgroundColor,
                TextColor = TextColor,
                AccentColor1 = AccentColor1,
                AccentColor2 = AccentColor2,
                AccentColor3 = AccentColor3,
                AccentColor4 = AccentColor4,
                AccentColor5 = AccentColor5,
                AccentColor6 = AccentColor6,
                HyperlinkColor = HyperlinkColor,
                FollowedHyperlinkColor = FollowedHyperlinkColor
            };
        }
    }

    public class ThemeFontScheme
    {
        public string Name { get; set; }
        public string MajorFontLatin { get; set; }
        public string MajorFontEastAsian { get; set; }
        public string MajorFontComplexScript { get; set; }
        public string MinorFontLatin { get; set; }
        public string MinorFontEastAsian { get; set; }
        public string MinorFontComplexScript { get; set; }

        public ThemeFontScheme()
        {
            Name = "Office";
            MajorFontLatin = "Calibri Light";
            MajorFontEastAsian = "Calibri Light";
            MajorFontComplexScript = "Calibri Light";
            MinorFontLatin = "Calibri";
            MinorFontEastAsian = "Calibri";
            MinorFontComplexScript = "Calibri";
        }

        public ThemeFontScheme Clone()
        {
            return new ThemeFontScheme
            {
                Name = Name,
                MajorFontLatin = MajorFontLatin,
                MajorFontEastAsian = MajorFontEastAsian,
                MajorFontComplexScript = MajorFontComplexScript,
                MinorFontLatin = MinorFontLatin,
                MinorFontEastAsian = MinorFontEastAsian,
                MinorFontComplexScript = MinorFontComplexScript
            };
        }
    }

    public class ThemeEffectScheme
    {
        public string Name { get; set; }
        public string FillStyle { get; set; }
        public string LineStyle { get; set; }
        public string EffectStyle { get; set; }

        public ThemeEffectScheme()
        {
            Name = "Office";
            FillStyle = "FillSolid";
            LineStyle = "LineSolid";
            EffectStyle = "Subtle";
        }

        public ThemeEffectScheme Clone()
        {
            return new ThemeEffectScheme
            {
                Name = Name,
                FillStyle = FillStyle,
                LineStyle = LineStyle,
                EffectStyle = EffectStyle
            };
        }
    }

    public class SlideBackground
    {
        public string Id { get; set; }
        public BackgroundType Type { get; set; }
        public string FillColor { get; set; }
        public string GradientColor1 { get; set; }
        public string GradientColor2 { get; set; }
        public double GradientAngle { get; set; }
        public double GradientStops { get; set; }
        public string ImagePath { get; set; }
        public byte[] ImageData { get; set; }
        public BackgroundImageStyle ImageStyle { get; set; }
        public double ImageOpacity { get; set; }
        public bool IsHidden { get; set; }

        public SlideBackground()
        {
            Id = Guid.NewGuid().ToString();
            Type = BackgroundType.Solid;
            FillColor = "#FFFFFF";
            GradientColor1 = "#FFFFFF";
            GradientColor2 = "#E6E6E6";
            GradientAngle = 90;
            GradientStops = 1;
            ImageStyle = BackgroundImageStyle.Fit;
            ImageOpacity = 1.0;
            IsHidden = false;
        }

        public SlideBackground Clone()
        {
            return new SlideBackground
            {
                Id = Guid.NewGuid().ToString(),
                Type = Type,
                FillColor = FillColor,
                GradientColor1 = GradientColor1,
                GradientColor2 = GradientColor2,
                GradientAngle = GradientAngle,
                GradientStops = GradientStops,
                ImagePath = ImagePath,
                ImageData = ImageData?.Clone() as byte[],
                ImageStyle = ImageStyle,
                ImageOpacity = ImageOpacity,
                IsHidden = IsHidden
            };
        }
    }

    public class TemplateManager
    {
        private Dictionary<string, SlideTemplate> _templates;
        private Dictionary<string, SlideLayout> _layouts;
        private SlideLayout _currentLayout;
        private List<TemplateEventHandler> _subscribers;

        public event EventHandler<TemplateChangedEventArgs> TemplateChanged;
        public event EventHandler<LayoutChangedEventArgs> LayoutChanged;

        public SlideLayout CurrentLayout
        {
            get => _currentLayout;
            set
            {
                _currentLayout = value;
                OnLayoutChanged(new LayoutChangedEventArgs(value));
            }
        }

        public TemplateManager()
        {
            _templates = new Dictionary<string, SlideTemplate>();
            _layouts = new Dictionary<string, SlideLayout>();
            _subscribers = new List<TemplateEventHandler>();
            InitializeBuiltInTemplates();
            InitializeBuiltInLayouts();
        }

        private void InitializeBuiltInTemplates()
        {
            AddTemplate(CreateTitleSlideTemplate());
            AddTemplate(CreateTitleAndContentTemplate());
            AddTemplate(CreateTwoContentTemplate());
            AddTemplate(CreateComparisonTemplate());
            AddTemplate(CreateBlankTemplate());
            AddTemplate(CreateSectionHeaderTemplate());
            AddTemplate(CreatePictureWithCaptionTemplate());
            AddTemplate(CreateTitleOnlyTemplate());
            AddTemplate(CreateContentWithCaptionTemplate());
            AddTemplate(CreateTableTemplate());
        }

        private void InitializeBuiltInLayouts()
        {
            AddLayout(CreateTitleSlideLayout());
            AddLayout(CreateTitleAndContentLayout());
            AddLayout(CreateTwoContentLayout());
            AddLayout(CreateBlankLayout());
            AddLayout(CreateSectionHeaderLayout());
            AddLayout(CreatePictureLayout());
            AddLayout(CreateTableLayout());
            AddLayout(CreateChartLayout());
            AddLayout(CreateMediaLayout());
            AddLayout(CreateTitleOnlyLayout());
        }

        private SlideTemplate CreateTitleSlideTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "title-slide",
                Name = "Title Slide",
                Description = "Title slide with large title and subtitle",
                Type = SlideType.Title,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateTitlePlaceholder());
            template.AddPlaceholder(CreateSubtitlePlaceholder());
            return template;
        }

        private SlideTemplate CreateTitleAndContentTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "title-content",
                Name = "Title and Content",
                Description = "Standard slide with title and content area",
                Type = SlideType.TitleAndContent,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateTitlePlaceholder());
            template.AddPlaceholder(CreateContentPlaceholder());
            return template;
        }

        private SlideTemplate CreateTwoContentTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "two-content",
                Name = "Two Content",
                Description = "Slide with title and two content areas side by side",
                Type = SlideType.TwoContent,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateTitlePlaceholder());
            template.AddPlaceholder(CreateLeftContentPlaceholder());
            template.AddPlaceholder(CreateRightContentPlaceholder());
            return template;
        }

        private SlideTemplate CreateComparisonTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "comparison",
                Name = "Comparison",
                Description = "Slide for comparing two items",
                Type = SlideType.Comparison,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateTitlePlaceholder());
            template.AddPlaceholder(CreateLeftContentPlaceholder());
            template.AddPlaceholder(CreateRightContentPlaceholder());
            return template;
        }

        private SlideTemplate CreateBlankTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "blank",
                Name = "Blank",
                Description = "Blank slide with no placeholders",
                Type = SlideType.Blank,
                IsBuiltIn = true
            };
            return template;
        }

        private SlideTemplate CreateSectionHeaderTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "section-header",
                Name = "Section Header",
                Description = "Section header slide",
                Type = SlideType.SectionHeader,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateSectionTitlePlaceholder());
            template.AddPlaceholder(CreateSectionSubtitlePlaceholder());
            return template;
        }

        private SlideTemplate CreatePictureWithCaptionTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "picture-caption",
                Name = "Picture with Caption",
                Description = "Slide with picture and caption",
                Type = SlideType.PictureWithCaption,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateTitlePlaceholder());
            template.AddPlaceholder(CreatePicturePlaceholder());
            template.AddPlaceholder(CreateCaptionPlaceholder());
            return template;
        }

        private SlideTemplate CreateTitleOnlyTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "title-only",
                Name = "Title Only",
                Description = "Slide with title only",
                Type = SlideType.TitleOnly,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateTitlePlaceholder());
            return template;
        }

        private SlideTemplate CreateContentWithCaptionTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "content-caption",
                Name = "Content with Caption",
                Description = "Slide with content and caption",
                Type = SlideType.ContentWithCaption,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateTitlePlaceholder());
            template.AddPlaceholder(CreateContentPlaceholder());
            template.AddPlaceholder(CreateCaptionPlaceholder());
            return template;
        }

        private SlideTemplate CreateTableTemplate()
        {
            var template = new SlideTemplate
            {
                Id = "table",
                Name = "Table",
                Description = "Slide with table placeholder",
                Type = SlideType.Table,
                IsBuiltIn = true
            };
            template.AddPlaceholder(CreateTitlePlaceholder());
            template.AddPlaceholder(CreateTablePlaceholder());
            return template;
        }

        private PlaceholderDefinition CreateTitlePlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "title",
                Name = "Title",
                Type = PlaceholderType.Title,
                Position = PlaceholderPosition.Center,
                Left = 50,
                Top = 50,
                Width = 860,
                Height = 100,
                IsRequired = true,
                DefaultContent = "Click to edit title"
            };
        }

        private PlaceholderDefinition CreateSubtitlePlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "subtitle",
                Name = "Subtitle",
                Type = PlaceholderType.Subtitle,
                Position = PlaceholderPosition.Center,
                Left = 50,
                Top = 150,
                Width = 860,
                Height = 60,
                DefaultContent = "Click to edit subtitle"
            };
        }

        private PlaceholderDefinition CreateContentPlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "content",
                Name = "Content",
                Type = PlaceholderType.Body,
                Position = PlaceholderPosition.Center,
                Left = 50,
                Top = 150,
                Width = 860,
                Height = 350,
                IsRequired = true,
                DefaultContent = "Click to add content"
            };
        }

        private PlaceholderDefinition CreateLeftContentPlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "left-content",
                Name = "Left Content",
                Type = PlaceholderType.Body,
                Position = PlaceholderPosition.Left,
                Left = 50,
                Top = 150,
                Width = 410,
                Height = 350,
                DefaultContent = "Click to add left content"
            };
        }

        private PlaceholderDefinition CreateRightContentPlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "right-content",
                Name = "Right Content",
                Type = PlaceholderType.Body,
                Position = PlaceholderPosition.Right,
                Left = 500,
                Top = 150,
                Width = 410,
                Height = 350,
                DefaultContent = "Click to add right content"
            };
        }

        private PlaceholderDefinition CreateSectionTitlePlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "section-title",
                Name = "Section Title",
                Type = PlaceholderType.SectionTitle,
                Position = PlaceholderPosition.Center,
                Left = 50,
                Top = 200,
                Width = 860,
                Height = 100,
                IsRequired = true,
                DefaultContent = "Section"
            };
        }

        private PlaceholderDefinition CreateSectionSubtitlePlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "section-subtitle",
                Name = "Section Subtitle",
                Type = PlaceholderType.SectionSubtitle,
                Position = PlaceholderPosition.Center,
                Left = 50,
                Top = 300,
                Width = 860,
                Height = 60,
                DefaultContent = "Section subtitle"
            };
        }

        private PlaceholderDefinition CreatePicturePlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "picture",
                Name = "Picture",
                Type = PlaceholderType.Picture,
                Position = PlaceholderPosition.Center,
                Left = 50,
                Top = 150,
                Width = 500,
                Height = 350
            };
        }

        private PlaceholderDefinition CreateCaptionPlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "caption",
                Name = "Caption",
                Type = PlaceholderType.Caption,
                Position = PlaceholderPosition.Bottom,
                Left = 50,
                Top = 480,
                Width = 860,
                Height = 30
            };
        }

        private PlaceholderDefinition CreateTablePlaceholder()
        {
            return new PlaceholderDefinition
            {
                Id = "table",
                Name = "Table",
                Type = PlaceholderType.Table,
                Position = PlaceholderPosition.Center,
                Left = 50,
                Top = 150,
                Width = 860,
                Height = 350,
                AllowedContentTypes = new List<string> { "Table" }
            };
        }

        private SlideLayout CreateTitleSlideLayout()
        {
            return new SlideLayout
            {
                Id = "title-slide-layout",
                Name = "Title Slide",
                Type = SlideType.Title,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreateTitleAndContentLayout()
        {
            return new SlideLayout
            {
                Id = "title-content-layout",
                Name = "Title and Content",
                Type = SlideType.TitleAndContent,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreateTwoContentLayout()
        {
            return new SlideLayout
            {
                Id = "two-content-layout",
                Name = "Two Content",
                Type = SlideType.TwoContent,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreateBlankLayout()
        {
            return new SlideLayout
            {
                Id = "blank-layout",
                Name = "Blank",
                Type = SlideType.Blank,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreateSectionHeaderLayout()
        {
            return new SlideLayout
            {
                Id = "section-header-layout",
                Name = "Section Header",
                Type = SlideType.SectionHeader,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreatePictureLayout()
        {
            return new SlideLayout
            {
                Id = "picture-layout",
                Name = "Picture",
                Type = SlideType.PictureWithCaption,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreateTableLayout()
        {
            return new SlideLayout
            {
                Id = "table-layout",
                Name = "Table",
                Type = SlideType.Table,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreateChartLayout()
        {
            return new SlideLayout
            {
                Id = "chart-layout",
                Name = "Chart",
                Type = SlideType.Chart,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreateMediaLayout()
        {
            return new SlideLayout
            {
                Id = "media-layout",
                Name = "Media",
                Type = SlideType.Media,
                Width = 960,
                Height = 540
            };
        }

        private SlideLayout CreateTitleOnlyLayout()
        {
            return new SlideLayout
            {
                Id = "title-only-layout",
                Name = "Title Only",
                Type = SlideType.TitleOnly,
                Width = 960,
                Height = 540
            };
        }

        public void AddTemplate(SlideTemplate template)
        {
            _templates[template.Id] = template;
        }

        public void AddLayout(SlideLayout layout)
        {
            _layouts[layout.Id] = layout;
        }

        public SlideTemplate GetTemplate(string templateId)
        {
            return _templates.TryGetValue(templateId, out var template) ? template : null;
        }

        public SlideLayout GetLayout(string layoutId)
        {
            return _layouts.TryGetValue(layoutId, out var layout) ? layout : null;
        }

        public IEnumerable<SlideTemplate> GetAllTemplates()
        {
            return _templates.Values;
        }

        public IEnumerable<SlideLayout> GetAllLayouts()
        {
            return _layouts.Values;
        }

        public IEnumerable<SlideTemplate> GetTemplatesByType(SlideType type)
        {
            return _templates.Values.Where(t => t.Type == type);
        }

        public IEnumerable<SlideLayout> GetLayoutsByType(SlideType type)
        {
            return _layouts.Values.Where(l => l.Type == type);
        }

        public SlideTemplate CreateCustomTemplate(string name, SlideType type)
        {
            var template = new SlideTemplate
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Type = type,
                IsBuiltIn = false,
                Author = "User"
            };
            AddTemplate(template);
            return template;
        }

        public SlideLayout CreateCustomLayout(string name, SlideType type)
        {
            var layout = new SlideLayout
            {
                Id = Guid.NewGuid().ToString(),
                Name = name,
                Type = type
            };
            AddLayout(layout);
            return layout;
        }

        public void ApplyLayout(string layoutId)
        {
            var layout = GetLayout(layoutId);
            if (layout != null)
            {
                CurrentLayout = layout;
            }
        }

        public void RemoveTemplate(string templateId)
        {
            var template = GetTemplate(templateId);
            if (template != null && !template.IsBuiltIn)
            {
                _templates.Remove(templateId);
            }
        }

        public void RemoveLayout(string layoutId)
        {
            var layout = GetLayout(layoutId);
            if (layout != null)
            {
                _layouts.Remove(layoutId);
            }
        }

        public void Subscribe(TemplateEventHandler handler)
        {
            _subscribers.Add(handler);
        }

        public void Unsubscribe(TemplateEventHandler handler)
        {
            _subscribers.Remove(handler);
        }

        protected virtual void OnTemplateChanged(TemplateChangedEventArgs e)
        {
            TemplateChanged?.Invoke(this, e);
        }

        protected virtual void OnLayoutChanged(LayoutChangedEventArgs e)
        {
            LayoutChanged?.Invoke(this, e);
        }

        public TemplateStatistics GetStatistics()
        {
            return new TemplateStatistics
            {
                TotalTemplates = _templates.Count,
                TotalLayouts = _layouts.Count,
                BuiltInTemplates = _templates.Values.Count(t => t.IsBuiltIn),
                BuiltInLayouts = _layouts.Values.Count(l => l.IsBuiltIn),
                TemplatesByType = _templates.Values.GroupBy(t => t.Type).ToDictionary(g => g.Key, g => g.Count())
            };
        }
    }

    public class TemplateEventArgs : EventArgs
    {
        public string TemplateId { get; }
        public DateTime ChangedAt { get; }

        public TemplateEventArgs(string templateId)
        {
            TemplateId = templateId;
            ChangedAt = DateTime.Now;
        }
    }

    public class TemplateChangedEventArgs : TemplateEventArgs
    {
        public SlideTemplate Template { get; }

        public TemplateChangedEventArgs(SlideTemplate template) : base(template.Id)
        {
            Template = template;
        }
    }

    public class LayoutChangedEventArgs : EventArgs
    {
        public SlideLayout Layout { get; }
        public DateTime ChangedAt { get; }

        public LayoutChangedEventArgs(SlideLayout layout)
        {
            Layout = layout;
            ChangedAt = DateTime.Now;
        }
    }

    public delegate void TemplateEventHandler(object sender, TemplateEventArgs e);

    public class TemplateStatistics
    {
        public int TotalTemplates { get; set; }
        public int TotalLayouts { get; set; }
        public int BuiltInTemplates { get; set; }
        public int BuiltInLayouts { get; set; }
        public Dictionary<SlideType, int> TemplatesByType { get; set; }
    }

    namespace Enums
    {
        public enum SlideType
        {
            Title,
            TitleAndContent,
            TwoContent,
            Comparison,
            Blank,
            SectionHeader,
            PictureWithCaption,
            TitleOnly,
            ContentWithCaption,
            Table,
            Chart,
            Media,
            Diagram,
            Custom
        }

        public enum PlaceholderType
        {
            Title,
            Subtitle,
            Body,
            SectionTitle,
            SectionSubtitle,
            Picture,
            Caption,
            Table,
            Chart,
            Diagram,
            Media,
            Notes,
            Header,
            Footer,
            SlideNumber,
            DateTime,
            Custom
        }

        public enum PlaceholderPosition
        {
            Top,
            Bottom,
            Left,
            Right,
            Center,
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight,
            Custom
        }

        public enum LayoutElementType
        {
            Rectangle,
            TextBox,
            Image,
            Shape,
            Table,
            Chart,
            Media,
            Group,
            Connector,
            Custom
        }

        public enum BackgroundType
        {
            Solid,
            Gradient,
            Picture,
            Pattern,
            Texture
        }

        public enum BackgroundImageStyle
        {
            Fit,
            Stretch,
            Tile,
            Center,
            Cover
        }

        public enum SlideNumberPosition
        {
            BottomLeft,
            BottomCenter,
            BottomRight,
            TopLeft,
            TopCenter,
            TopRight
        }

        public enum TextAlignment
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
            Bottom
        }
    }

    public class SlideMaster
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public SlideLayout BaseLayout { get; set; }
        public SlideTheme Theme { get; set; }
        public SlideBackground Background { get; set; }
        public List<LayoutElement> HeaderElements { get; set; }
        public List<LayoutElement> FooterElements { get; set; }
        public List<LayoutElement> SlideNumberElements { get; set; }
        public Dictionary<string, string> HeaderText { get; set; }
        public Dictionary<string, string> FooterText { get; set; }
        public bool ShowHeaders { get; set; }
        public bool ShowFooters { get; set; }
        public bool ShowSlideNumbers { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }

        public SlideMaster()
        {
            Id = Guid.NewGuid().ToString();
            HeaderElements = new List<LayoutElement>();
            FooterElements = new List<LayoutElement>();
            SlideNumberElements = new List<LayoutElement>();
            HeaderText = new Dictionary<string, string>();
            FooterText = new Dictionary<string, string>();
            ShowHeaders = false;
            ShowFooters = true;
            ShowSlideNumbers = true;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
        }

        public SlideMaster Clone()
        {
            return new SlideMaster
            {
                Id = Guid.NewGuid().ToString(),
                Name = Name,
                BaseLayout = BaseLayout?.Clone(),
                Theme = Theme?.Clone(),
                Background = Background?.Clone(),
                HeaderElements = HeaderElements.Select(e => e.Clone()).ToList(),
                FooterElements = FooterElements.Select(e => e.Clone()).ToList(),
                SlideNumberElements = SlideNumberElements.Select(e => e.Clone()).ToList(),
                HeaderText = new Dictionary<string, string>(HeaderText),
                FooterText = new Dictionary<string, string>(FooterText),
                ShowHeaders = ShowHeaders,
                ShowFooters = ShowFooters,
                ShowSlideNumbers = ShowSlideNumbers,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now
            };
        }
    }

    public class SlideMasterManager
    {
        private Dictionary<string, SlideMaster> _masters;

        public SlideMasterManager()
        {
            _masters = new Dictionary<string, SlideMaster>();
            InitializeBuiltInMasters();
        }

        private void InitializeBuiltInMasters()
        {
            AddMaster(CreateTitleMaster());
            AddMaster(CreateTitleAndContentMaster());
            AddMaster(CreateSectionHeaderMaster());
            AddMaster(CreateBlankMaster());
        }

        private SlideMaster CreateTitleMaster()
        {
            return new SlideMaster
            {
                Id = "title-master",
                Name = "Title Slide Master"
            };
        }

        private SlideMaster CreateTitleAndContentMaster()
        {
            return new SlideMaster
            {
                Id = "title-content-master",
                Name = "Title and Content Master"
            };
        }

        private SlideMaster CreateSectionHeaderMaster()
        {
            return new SlideMaster
            {
                Id = "section-header-master",
                Name = "Section Header Master"
            };
        }

        private SlideMaster CreateBlankMaster()
        {
            return new SlideMaster
            {
                Id = "blank-master",
                Name = "Blank Master"
            };
        }

        public void AddMaster(SlideMaster master)
        {
            _masters[master.Id] = master;
        }

        public SlideMaster GetMaster(string masterId)
        {
            return _masters.TryGetValue(masterId, out var master) ? master : null;
        }

        public IEnumerable<SlideMaster> GetAllMasters()
        {
            return _masters.Values;
        }

        public void RemoveMaster(string masterId)
        {
            _masters.Remove(masterId);
        }

        public SlideMaster CreateCustomMaster(string name)
        {
            var master = new SlideMaster
            {
                Id = Guid.NewGuid().ToString(),
                Name = name
            };
            AddMaster(master);
            return master;
        }
    }
}

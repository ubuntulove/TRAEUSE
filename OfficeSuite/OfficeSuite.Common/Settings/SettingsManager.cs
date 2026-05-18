using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace OfficeSuite.Common.Settings
{
    public class ApplicationSettings
    {
        public string ApplicationName { get; set; }
        public string Version { get; set; }
        public UserPreferences UserPreferences { get; set; }
        public AppearanceSettings Appearance { get; set; }
        public EditorSettings Editor { get; set; }
        public FileSettings Files { get; set; }
        public SecuritySettings Security { get; set; }
        public PerformanceSettings Performance { get; set; }
        public Dictionary<string, ModuleSettings> ModuleSettings { get; set; }
        public Dictionary<string, string> CustomSettings { get; set; }

        public ApplicationSettings()
        {
            ApplicationName = "OfficeSuite";
            Version = "1.0.0";
            UserPreferences = new UserPreferences();
            Appearance = new AppearanceSettings();
            Editor = new EditorSettings();
            Files = new FileSettings();
            Security = new SecuritySettings();
            Performance = new PerformanceSettings();
            ModuleSettings = new Dictionary<string, ModuleSettings>();
            CustomSettings = new Dictionary<string, string>();
        }
    }

    public class UserPreferences
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string UserInitials { get; set; }
        public string DefaultAuthor { get; set; }
        public string DefaultCompany { get; set; }
        public string DefaultLanguage { get; set; }
        public string DefaultSaveLocation { get; set; }
        public string DefaultOpenLocation { get; set; }
        public bool RememberWindowPosition { get; set; }
        public bool RememberWindowSize { get; set; }
        public WindowSettings WindowSettings { get; set; }
        public RecentItemsSettings RecentItems { get; set; }
        public AutoSaveSettings AutoSave { get; set; }
        public BackupSettings Backup { get; set; }

        public UserPreferences()
        {
            DefaultLanguage = "en-US";
            RememberWindowPosition = true;
            RememberWindowSize = true;
            WindowSettings = new WindowSettings();
            RecentItems = new RecentItemsSettings();
            AutoSave = new AutoSaveSettings();
            Backup = new BackupSettings();
        }
    }

    public class WindowSettings
    {
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public WindowState State { get; set; }
        public bool IsMaximized { get; set; }
        public bool IsMinimized { get; set; }
        public bool AlwaysOnTop { get; set; }
    }

    public enum WindowState
    {
        Normal,
        Maximized,
        Minimized
    }

    public class RecentItemsSettings
    {
        public int MaxRecentItems { get; set; }
        public int MaxRecentFiles { get; set; }
        public int MaxRecentFolders { get; set; }
        public List<RecentItem> RecentFiles { get; set; }
        public List<RecentItem> RecentFolders { get; set; }
        public List<RecentItem> RecentTemplates { get; set; }

        public RecentItemsSettings()
        {
            MaxRecentItems = 20;
            MaxRecentFiles = 20;
            MaxRecentFolders = 10;
            MaxRecentTemplates = 10;
            RecentFiles = new List<RecentItem>();
            RecentFolders = new List<RecentItem>();
            RecentTemplates = new List<RecentItem>();
        }
    }

    public class RecentItem
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public string Type { get; set; }
        public DateTime AccessedDate { get; set; }
        public int AccessCount { get; set; }
        public bool IsPinned { get; set; }

        public RecentItem()
        {
            Id = Guid.NewGuid().ToString();
            AccessedDate = DateTime.Now;
            AccessCount = 1;
            IsPinned = false;
        }
    }

    public class AutoSaveSettings
    {
        public bool Enabled { get; set; }
        public int IntervalSeconds { get; set; }
        public string SaveLocation { get; set; }
        public bool ShowNotification { get; set; }
        public bool SaveOnBlur { get; set; }
        public int MaxAutoSaveVersions { get; set; }
        public List<AutoSaveVersion> Versions { get; set; }

        public AutoSaveSettings()
        {
            Enabled = true;
            IntervalSeconds = 300;
            ShowNotification = true;
            SaveOnBlur = true;
            MaxAutoSaveVersions = 10;
            Versions = new List<AutoSaveVersion>();
        }
    }

    public class AutoSaveVersion
    {
        public string DocumentId { get; set; }
        public string FilePath { get; set; }
        public DateTime SavedDate { get; set; }
        public long Size { get; set; }
    }

    public class BackupSettings
    {
        public bool Enabled { get; set; }
        public int MaxBackupCopies { get; set; }
        public string BackupLocation { get; set; }
        public bool IncludeTimestamps { get; set; }
        public bool CompressBackups { get; set; }
        public bool EncryptBackups { get; set; }
        public List<BackupCopy> BackupCopies { get; set; }

        public BackupSettings()
        {
            Enabled = true;
            MaxBackupCopies = 5;
            IncludeTimestamps = true;
            CompressBackups = true;
            EncryptBackups = false;
            BackupCopies = new List<BackupCopy>();
        }
    }

    public class BackupCopy
    {
        public string DocumentId { get; set; }
        public string FilePath { get; set; }
        public DateTime CreatedDate { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
    }

    public class AppearanceSettings
    {
        public string Theme { get; set; }
        public string ColorScheme { get; set; }
        public string AccentColor { get; set; }
        public string FontFamily { get; set; }
        public double FontSize { get; set; }
        public double UiFontSize { get; set; }
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
        public bool UseSystemAccent { get; set; }
        public bool EnableAnimations { get; set; }
        public bool EnableTransitions { get; set; }
        public int AnimationSpeed { get; set; }
        public HighContrastSettings HighContrast { get; set; }
        public RibbonSettings Ribbon { get; set; }
        public StatusBarSettings StatusBar { get; set; }
        public NavigationPaneSettings NavigationPane { get; set; }

        public AppearanceSettings()
        {
            Theme = "Light";
            ColorScheme = "Default";
            AccentColor = "#0078D4";
            FontFamily = "Segoe UI";
            FontSize = 14;
            UiFontSize = 12;
            BackgroundColor = "#FFFFFF";
            ForegroundColor = "#000000";
            UseSystemAccent = false;
            EnableAnimations = true;
            EnableTransitions = true;
            AnimationSpeed = 100;
            HighContrast = new HighContrastSettings();
            Ribbon = new RibbonSettings();
            StatusBar = new StatusBarSettings();
            NavigationPane = new NavigationPaneSettings();
        }
    }

    public class HighContrastSettings
    {
        public bool Enabled { get; set; }
        public string BackgroundColor { get; set; }
        public string ForegroundColor { get; set; }
        public string AccentColor { get; set; }
        public bool UseSystemColors { get; set; }

        public HighContrastSettings()
        {
            Enabled = false;
            BackgroundColor = "#000000";
            ForegroundColor = "#FFFFFF";
            AccentColor = "#FFFF00";
            UseSystemColors = true;
        }
    }

    public class RibbonSettings
    {
        public bool MinimizeRibbon { get; set; }
        public bool ShowTabs { get; set; }
        public bool ShowLabels { get; set; }
        public bool AutoCollapse { get; set; }
        public bool ExpandOnMouseOver { get; set; }
        public bool UseCompactMode { get; set; }
        public string RibbonStyle { get; set; }
        public int TabCount { get; set; }
        public List<string> VisibleTabs { get; set; }
        public List<string> PinnedTabs { get; set; }

        public RibbonSettings()
        {
            MinimizeRibbon = false;
            ShowTabs = true;
            ShowLabels = true;
            AutoCollapse = false;
            ExpandOnMouseOver = true;
            UseCompactMode = false;
            RibbonStyle = "Standard";
            TabCount = 8;
            VisibleTabs = new List<string>();
            PinnedTabs = new List<string>();
        }
    }

    public class StatusBarSettings
    {
        public bool Visible { get; set; }
        public bool ShowZoom { get; set; }
        public bool ShowPageNumber { get; set; }
        public bool ShowWordCount { get; set; }
        public bool ShowLanguage { get; set; }
        public bool ShowZoomSlider { get; set; }
        public int DefaultZoom { get; set; }
        public List<string> VisiblePanels { get; set; }

        public StatusBarSettings()
        {
            Visible = true;
            ShowZoom = true;
            ShowPageNumber = true;
            ShowWordCount = true;
            ShowLanguage = true;
            ShowZoomSlider = true;
            DefaultZoom = 100;
            VisiblePanels = new List<string>();
        }
    }

    public class NavigationPaneSettings
    {
        public bool Visible { get; set; }
        public bool AutoHide { get; set; }
        public double Width { get; set; }
        public double MinWidth { get; set; }
        public double MaxWidth { get; set; }
        public bool ShowThumbnails { get; set; }
        public bool ShowOutline { get; set; }
        public bool ShowSearch { get; set; }
        public string DefaultView { get; set; }
        public List<string> VisibleViews { get; set; }

        public NavigationPaneSettings()
        {
            Visible = true;
            AutoHide = false;
            Width = 250;
            MinWidth = 150;
            MaxWidth = 400;
            ShowThumbnails = true;
            ShowOutline = true;
            ShowSearch = true;
            DefaultView = "Thumbnails";
            VisibleViews = new List<string>();
        }
    }

    public class EditorSettings
    {
        public GeneralEditorSettings General { get; set; }
        public WordEditorSettings Word { get; set; }
        public ExcelEditorSettings Excel { get; set; }
        public PowerPointEditorSettings PowerPoint { get; set; }

        public EditorSettings()
        {
            General = new GeneralEditorSettings();
            Word = new WordEditorSettings();
            Excel = new ExcelEditorSettings();
            PowerPoint = new PowerPointEditorSettings();
        }
    }

    public class GeneralEditorSettings
    {
        public bool AutoCorrect { get; set; }
        public bool AutoFormat { get; set; }
        public bool ShowFormattingMarks { get; set; }
        public bool ShowHiddenText { get; set; }
        public bool ShowRuler { get; set; }
        public bool ShowGridlines { get; set; }
        public bool SnapToGrid { get; set; }
        public bool SmartCutPaste { get; set; }
        public bool TrackChanges { get; set; }
        public bool HighlightChanges { get; set; }
        public string DefaultMeasurementUnit { get; set; }
        public double ZoomLevel { get; set; }
    }

    public class WordEditorSettings
    {
        public bool AutoSpellingCorrection { get; set; }
        public bool AutoGrammarCorrection { get; set; }
        public bool AutoSentenceCorrection { get; set; }
        public bool CapitalizeFirstLetter { get; set; }
        public bool CapitalizeDays { get; set; }
        public bool CorrectCapsLock { get; set; }
        public bool AutoFormatDates { get; set; }
        public bool AutoFormatNumbers { get; set; }
        public bool AutoBullets { get; set; }
        public bool AutoNumbering { get; set; }
        public bool SmartQuotes { get; set; }
        public bool UseSpellCheck { get; set; }
        public string DefaultProofingLanguage { get; set; }
        public List<string> CustomDictionary { get; set; }
        public List<string> IgnoreWords { get; set; }
        public int DefaultTabStop { get; set; }
        public int DefaultZoom { get; set; }
        public bool ShowVerticalRuler { get; set; }
        public bool ShowHorizontalRuler { get; set; }
        public bool ShowScrollBars { get; set; }
        public bool EnableStyleSeparators { get; set; }
        public string DefaultFontFamily { get; set; }
        public double DefaultFontSize { get; set; }
        public string DefaultFontColor { get; set; }
        public string DefaultPageColor { get; set; }
        public MarginsSettings DefaultMargins { get; set; }
        public PageSizeSettings DefaultPageSize { get; set; }

        public WordEditorSettings()
        {
            AutoSpellingCorrection = true;
            AutoGrammarCorrection = true;
            AutoSentenceCorrection = true;
            CapitalizeFirstLetter = true;
            CapitalizeDays = true;
            CorrectCapsLock = true;
            AutoFormatDates = true;
            AutoFormatNumbers = true;
            AutoBullets = true;
            AutoNumbering = true;
            SmartQuotes = true;
            UseSpellCheck = true;
            DefaultProofingLanguage = "en-US";
            CustomDictionary = new List<string>();
            IgnoreWords = new List<string>();
            DefaultTabStop = 36;
            DefaultZoom = 100;
            ShowVerticalRuler = true;
            ShowHorizontalRuler = true;
            ShowScrollBars = true;
            EnableStyleSeparators = false;
            DefaultFontFamily = "Calibri";
            DefaultFontSize = 11;
            DefaultFontColor = "#000000";
            DefaultPageColor = "#FFFFFF";
            DefaultMargins = new MarginsSettings();
            DefaultPageSize = new PageSizeSettings();
        }
    }

    public class ExcelEditorSettings
    {
        public bool AutoComplete { get; set; }
        public bool AutoFill { get; set; }
        public bool AutoPercent { get; set; }
        public bool ExtendFormulas { get; set; }
        public bool Animate { get; set; }
        public bool ShowFormulaBar { get; set; }
        public bool ShowNameBox { get; set; }
        public bool ShowRowHeaders { get; set; }
        public bool ShowColumnHeaders { get; set; }
        public bool ShowGridlines { get; set; }
        public bool ShowRuler { get; set; }
        public bool ShowOutline { get; set; }
        public bool ShowZeros { get; set; }
        public bool ShowFormulas { get; set; }
        public string DefaultFontFamily { get; set; }
        public double DefaultFontSize { get; set; }
        public string GridlineColor { get; set; }
        public bool UseSystemSeparators { get; set; }
        public string DecimalSeparator { get; set; }
        public string ThousandSeparator { get; set; }
        public string DefaultDecimalPlaces { get; set; }
        public int DefaultRowHeight { get; set; }
        public int DefaultColumnWidth { get; set; }
        public bool AutoCalculate { get; set; }
        public CalculationMode CalculationMode { get; set; }
        public bool ShowPageBreaks { get; set; }
        public bool ShowComments { get; set; }
        public bool ShowErrorIndicators { get; set; }

        public ExcelEditorSettings()
        {
            AutoComplete = true;
            AutoFill = true;
            AutoPercent = true;
            ExtendFormulas = true;
            Animate = true;
            ShowFormulaBar = true;
            ShowNameBox = true;
            ShowRowHeaders = true;
            ShowColumnHeaders = true;
            ShowGridlines = true;
            ShowRuler = false;
            ShowOutline = false;
            ShowZeros = true;
            ShowFormulas = false;
            DefaultFontFamily = "Calibri";
            DefaultFontSize = 11;
            GridlineColor = "#D4D4D4";
            UseSystemSeparators = true;
            DecimalSeparator = ".";
            ThousandSeparator = ",";
            DefaultDecimalPlaces = "General";
            DefaultRowHeight = 15;
            DefaultColumnWidth = 8.43;
            AutoCalculate = true;
            CalculationMode = CalculationMode.Automatic;
            ShowPageBreaks = true;
            ShowComments = true;
            ShowErrorIndicators = true;
        }
    }

    public class PowerPointEditorSettings
    {
        public bool AutoLayout { get; set; }
        public bool SnapToGrid { get; set; }
        public bool SnapToShape { get; set; }
        public bool ShowGuides { get; set; }
        public bool ShowRulers { get; set; }
        public bool ShowGridlines { get; set; }
        public bool ShowSlideMiniature { get; set; }
        public bool ShowPresenterView { get; set; }
        public bool AutoScale { get; set; }
        public bool OptimizeForPixels { get; set; }
        public bool PreserveAnimation { get; set; }
        public string DefaultSlideSize { get; set; }
        public int DefaultZoom { get; set; }
        public bool ShowFormatPane { get; set; }
        public bool ShowSelectionPane { get; set; }
        public bool ShowAnimationPane { get; set; }
        public bool ShowNotes { get; set; }
        public bool AutoAdvanceTime { get; set; }
        public int SlideTransitionDuration { get; set; }
        public string DefaultTransition { get; set; }
        public bool LoopUntilEscaped { get; set; }
        public bool ShowNarration { get; set; }
        public bool ShowMediaControls { get; set; }
        public bool UseRehearseTimings { get; set; }
        public SlideMasterSettings SlideMaster { get; set; }
        public HandoutSettings Handouts { get; set; }

        public PowerPointEditorSettings()
        {
            AutoLayout = true;
            SnapToGrid = true;
            SnapToShape = true;
            ShowGuides = true;
            ShowRulers = false;
            ShowGridlines = true;
            ShowSlideMiniature = true;
            ShowPresenterView = true;
            AutoScale = true;
            OptimizeForPixels = true;
            PreserveAnimation = true;
            DefaultSlideSize = "Widescreen16x9";
            DefaultZoom = 100;
            ShowFormatPane = true;
            ShowSelectionPane = false;
            ShowAnimationPane = true;
            ShowNotes = true;
            AutoAdvanceTime = false;
            SlideTransitionDuration = 2;
            DefaultTransition = "None";
            LoopUntilEscaped = false;
            ShowNarration = true;
            ShowMediaControls = true;
            UseRehearseTimings = false;
            SlideMaster = new SlideMasterSettings();
            Handouts = new HandoutSettings();
        }
    }

    public class MarginsSettings
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }
        public string Unit { get; set; }

        public MarginsSettings()
        {
            Top = 72;
            Bottom = 72;
            Left = 72;
            Right = 72;
            Unit = "points";
        }
    }

    public class PageSizeSettings
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Unit { get; set; }
        public Orientation Orientation { get; set; }

        public PageSizeSettings()
        {
            Name = "Letter";
            Width = 8.5;
            Height = 11;
            Unit = "inches";
            Orientation = Orientation.Portrait;
        }
    }

    public enum Orientation
    {
        Portrait,
        Landscape
    }

    public enum CalculationMode
    {
        Automatic,
        AutomaticExceptTables,
        Manual
    }

    public class SlideMasterSettings
    {
        public bool AutoUpdateStyles { get; set; }
        public bool PreserveBackground { get; set; }
        public bool KeepTextPlaceholders { get; set; }
        public bool UpdateLayoutOnMasterChange { get; set; }
    }

    public class HandoutSettings
    {
        public int SlidesPerPage { get; set; }
        public bool IncludeNotes { get; set; }
        public bool IncludeFrame { get; set; }
        public bool DateAndTime { get; set; }
        public bool HeaderAndFooter { get; set; }
    }

    public class FileSettings
    {
        public string DefaultFileFormat { get; set; }
        public string DefaultSaveLocation { get; set; }
        public string DefaultOpenLocation { get; set; }
        public string DefaultTemplateLocation { get; set; }
        public bool PromptForSaveOnClose { get; set; }
        public bool PromptForFormat { get; set; }
        public bool ConfirmConversions { get; set; }
        public bool AddExtension { get; set; }
        public bool BackupOnSave { get; set; }
        public bool AutoRecover { get; set; }
        public int AutoRecoverInterval { get; set; }
        public string AutoRecoverLocation { get; set; }
        public List<FileAssociation> FileAssociations { get; set; }
        public List<string> RecentFileTypes { get; set; }
        public ExportSettings Export { get; set; }
        public ImportSettings Import { get; set; }

        public FileSettings()
        {
            DefaultFileFormat = "docx";
            PromptForSaveOnClose = true;
            PromptForFormat = true;
            ConfirmConversions = true;
            AddExtension = true;
            BackupOnSave = false;
            AutoRecover = true;
            AutoRecoverInterval = 600;
            FileAssociations = new List<FileAssociation>();
            RecentFileTypes = new List<string>();
            Export = new ExportSettings();
            Import = new ImportSettings();
        }
    }

    public class FileAssociation
    {
        public string Extension { get; set; }
        public string FileType { get; set; }
        public string Description { get; set; }
        public string IconPath { get; set; }
        public string ApplicationId { get; set; }
        public bool IsRegistered { get; set; }
    }

    public class ExportSettings
    {
        public string DefaultExportFormat { get; set; }
        public string ExportLocation { get; set; }
        public bool EmbedFonts { get; set; }
        public bool EmbedLicensing { get; set; }
        public bool OptimizeForPrint { get; set; }
        public bool OptimizeForScreen { get; set; }
        public bool ExportMetadata { get; set; }
        public bool ExportComments { get; set; }
        public bool ExportTrackChanges { get; set; }
        public int ImageQuality { get; set; }
        public int PdfVersion { get; set; }
        public bool PdfACompliant { get; set; }
        public bool PdfXCompliant { get; set; }

        public ExportSettings()
        {
            DefaultExportFormat = "pdf";
            EmbedFonts = true;
            EmbedLicensing = true;
            OptimizeForScreen = true;
            ExportMetadata = true;
            ExportComments = true;
            ExportTrackChanges = false;
            ImageQuality = 100;
            PdfVersion = 1;
            PdfACompliant = false;
            PdfXCompliant = false;
        }
    }

    public class ImportSettings
    {
        public string DefaultImportFormat { get; set; }
        public string ImportLocation { get; set; }
        public bool PreserveFormatting { get; set; }
        public bool PreserveImages { get; set; }
        public bool PreserveHyperlinks { get; set; }
        public bool DetectEncoding { get; set; }
        public string PreferredEncoding { get; set; }
        public bool SkipHiddenContent { get; set; }
        public bool IgnoreFormatting { get; set; }
        public bool ImportStyles { get; set; }
        public bool ImportHeadersFooters { get; set; }
        public bool ImportFootnotes { get; set; }
        public bool ImportComments { get; set; }

        public ImportSettings()
        {
            PreserveFormatting = true;
            PreserveImages = true;
            PreserveHyperlinks = true;
            DetectEncoding = true;
            PreferredEncoding = "utf-8";
            SkipHiddenContent = false;
            IgnoreFormatting = false;
            ImportStyles = true;
            ImportHeadersFooters = true;
            ImportFootnotes = true;
            ImportComments = true;
        }
    }

    public class SecuritySettings
    {
        public bool EnablePasswordProtection { get; set; }
        public string DefaultPassword { get; set; }
        public bool RequirePasswordToOpen { get; set; }
        public bool RequirePasswordToEdit { get; set; }
        public bool EncryptDocumentProperties { get; set; }
        public bool HideDocumentProperties { get; set; }
        public bool MarkAsFinal { get; set; }
        public bool RestrictEditing { get; set; }
        public bool RestrictCopying { get; set; }
        public bool RestrictPrinting { get; set; }
        public bool RestrictComments { get; set; }
        public bool AllowOnlyComments { get; set; }
        public bool AllowOnlyFormFields { get; set; }
        public bool AllowOnlyRevisions { get; set; }
        public bool DigitalSignatureRequired { get; set; }
        public bool TrustCenterAccess { get; set; }
        public MacroSettings Macros { get; set; }
        public PrivacySettings Privacy { get; set; }
        public TrustedLocationsSettings TrustedLocations { get; set; }
        public AddinSecuritySettings Addins { get; set; }

        public SecuritySettings()
        {
            EnablePasswordProtection = false;
            RequirePasswordToOpen = false;
            RequirePasswordToEdit = false;
            EncryptDocumentProperties = false;
            HideDocumentProperties = false;
            MarkAsFinal = false;
            RestrictEditing = false;
            RestrictCopying = false;
            RestrictPrinting = false;
            RestrictComments = false;
            AllowOnlyComments = false;
            AllowOnlyFormFields = false;
            AllowOnlyRevisions = false;
            DigitalSignatureRequired = false;
            TrustCenterAccess = true;
            Macros = new MacroSettings();
            Privacy = new PrivacySettings();
            TrustedLocations = new TrustedLocationsSettings();
            Addins = new AddinSecuritySettings();
        }
    }

    public class MacroSettings
    {
        public MacroSecurityLevel SecurityLevel { get; set; }
        public bool DisableAllMacros { get; set; }
        public bool DisableMacrosWithNotification { get; set; }
        public bool EnableAllMacros { get; set; }
        public bool RequireSignedMacros { get; set; }
        public string TrustedPublishers { get; set; }
        public bool DisableVbaProjectLocking { get; set; }
        public bool StoreMacroInTemplate { get; set; }
    }

    public enum MacroSecurityLevel
    {
        DisableAllMacrosWithoutNotification,
        DisableMacrosWithNotification,
        DisableMacrosExceptDigitallySigned,
        EnableAllMacros
    }

    public class PrivacySettings
    {
        public bool RemovePersonalInfoOnSave { get; set; }
        public bool RemoveCommentsOnSave { get; set; }
        public bool RemoveMetadataOnSave { get; set; }
        public bool StoreRandomNumberOnSave { get; set; }
        public bool NeverShowStartExperience { get; set; }
        public bool ShowMiniaturesInMail { get; set; }
        public bool UseAutoComplete { get; set; }
        public bool SendPersonalInfo { get; set; }
        public bool CheckMicrosoftForSolutions { get; set; }
    }

    public class TrustedLocationsSettings
    {
        public bool AllowTrustedNetworkLocations { get; set; }
        public bool AllowTrustedPublishers { get; set; }
        public List<TrustedLocation> Locations { get; set; }
        public List<string> TrustedPublishers { get; set; }

        public TrustedLocationsSettings()
        {
            AllowTrustedNetworkLocations = true;
            AllowTrustedPublishers = true;
            Locations = new List<TrustedLocation>();
            TrustedPublishers = new List<string>();
        }
    }

    public class TrustedLocation
    {
        public string Id { get; set; }
        public string Path { get; set; }
        public bool IsNetworkLocation { get; set; }
        public bool IsReadOnly { get; set; }
        public bool IncludeSubFolders { get; set; }
        public string Description { get; set; }
        public DateTime? LastModified { get; set; }

        public TrustedLocation()
        {
            Id = Guid.NewGuid().ToString();
            IncludeSubFolders = true;
        }
    }

    public class AddinSecuritySettings
    {
        public bool DisableAllApplicationAddins { get; set; }
        public bool RequireAddinSignatures { get; set; }
        public List<string> DisabledAddins { get; set; }
        public List<string> EnabledAddins { get; set; }

        public AddinSecuritySettings()
        {
            DisableAllApplicationAddins = false;
            RequireAddinSignatures = false;
            DisabledAddins = new List<string>();
            EnabledAddins = new List<string>();
        }
    }

    public class PerformanceSettings
    {
        public bool HardwareAcceleration { get; set; }
        public bool UseGpuAcceleration { get; set; }
        public bool UseMultipleThreads { get; set; }
        public int MaxThreads { get; set; }
        public bool EnableAnimations { get; set; }
        public bool EnableTransitions { get; set; }
        public bool EnableLivePreview { get; set; }
        public bool EnableAutoSave { get; set; }
        public int CacheSize { get; set; }
        public string CacheLocation { get; set; }
        public bool ClearCacheOnExit { get; set; }
        public bool OptimizeForBattery { get; set; }
        public bool ReduceMemoryUsage { get; set; }
        public int ThumbnailQuality { get; set; }
        public int ImageCompression { get; set; }
        public bool DisableSkydriveSync { get; set; }
        public bool ShowOpenInOtherApps { get; set; }

        public PerformanceSettings()
        {
            HardwareAcceleration = true;
            UseGpuAcceleration = true;
            UseMultipleThreads = true;
            MaxThreads = Environment.ProcessorCount;
            EnableAnimations = true;
            EnableTransitions = true;
            EnableLivePreview = true;
            EnableAutoSave = true;
            CacheSize = 100;
            ClearCacheOnExit = false;
            OptimizeForBattery = false;
            ReduceMemoryUsage = false;
            ThumbnailQuality = 80;
            ImageCompression = 85;
            DisableSkydriveSync = false;
            ShowOpenInOtherApps = true;
        }
    }

    public class ModuleSettings
    {
        public string ModuleId { get; set; }
        public string ModuleName { get; set; }
        public bool IsEnabled { get; set; }
        public Dictionary<string, object> Settings { get; set; }

        public ModuleSettings()
        {
            IsEnabled = true;
            Settings = new Dictionary<string, object>();
        }
    }

    public class SettingsManager
    {
        private ApplicationSettings _settings;
        private readonly string _settingsPath;
        private readonly ISettingsStorage _storage;

        public ApplicationSettings Settings => _settings;

        public SettingsManager(string settingsPath)
        {
            _settingsPath = settingsPath;
            _storage = new JsonSettingsStorage();
            _settings = new ApplicationSettings();
        }

        public void Load()
        {
            if (System.IO.File.Exists(_settingsPath))
            {
                _settings = _storage.Load<ApplicationSettings>(_settingsPath);
            }
        }

        public void Save()
        {
            _storage.Save(_settings, _settingsPath);
        }

        public void Reset()
        {
            _settings = new ApplicationSettings();
        }

        public void Reset(string category)
        {
            switch (category.ToLower())
            {
                case "appearance":
                    _settings.Appearance = new AppearanceSettings();
                    break;
                case "editor":
                    _settings.Editor = new EditorSettings();
                    break;
                case "file":
                case "files":
                    _settings.Files = new FileSettings();
                    break;
                case "security":
                    _settings.Security = new SecuritySettings();
                    break;
                case "performance":
                    _settings.Performance = new PerformanceSettings();
                    break;
            }
        }

        public T GetSetting<T>(string key)
        {
            if (_settings.CustomSettings.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return default;
        }

        public void SetSetting<T>(string key, T value)
        {
            _settings.CustomSettings[key] = value.ToString();
        }
    }

    public interface ISettingsStorage
    {
        void Save<T>(T settings, string path);
        T Load<T>(string path) where T : new();
        bool Exists(string path);
        void Delete(string path);
    }

    public class JsonSettingsStorage : ISettingsStorage
    {
        public void Save<T>(T settings, string path)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            System.IO.File.WriteAllText(path, json);
        }

        public T Load<T>(string path) where T : new()
        {
            var json = System.IO.File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json) ?? new T();
        }

        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public void Delete(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }

    public class XmlSettingsStorage : ISettingsStorage
    {
        public void Save<T>(T settings, string path)
        {
            var serializer = new XmlSerializer(typeof(T));
            using var writer = new System.IO.FileStream(path, System.IO.FileMode.Create);
            serializer.Serialize(writer, settings);
        }

        public T Load<T>(string path) where T : new()
        {
            var serializer = new XmlSerializer(typeof(T));
            using var reader = new System.IO.FileStream(path, System.IO.FileMode.Open);
            return (T)serializer.Deserialize(reader) ?? new T();
        }

        public bool Exists(string path)
        {
            return System.IO.File.Exists(path);
        }

        public void Delete(string path)
        {
            if (System.IO.File.Exists(path))
            {
                System.IO.File.Delete(path);
            }
        }
    }

    public class SettingsMigrator
    {
        public bool Migrate(string fromVersion, string toVersion, ApplicationSettings settings)
        {
            var migrators = GetMigrators();
            foreach (var migrator in migrators)
            {
                if (migrator.CanMigrate(fromVersion, toVersion))
                {
                    migrator.Migrate(settings);
                    return true;
                }
            }
            return false;
        }

        private IEnumerable<ISettingsMigrator> GetMigrators()
        {
            return new List<ISettingsMigrator>();
        }
    }

    public interface ISettingsMigrator
    {
        bool CanMigrate(string fromVersion, string toVersion);
        void Migrate(ApplicationSettings settings);
    }

    public class DefaultSettingsProvider
    {
        public static ApplicationSettings GetDefaultSettings()
        {
            return new ApplicationSettings();
        }

        public static UserPreferences GetDefaultUserPreferences()
        {
            return new UserPreferences();
        }

        public static AppearanceSettings GetDefaultAppearanceSettings()
        {
            return new AppearanceSettings();
        }

        public static EditorSettings GetDefaultEditorSettings()
        {
            return new EditorSettings();
        }

        public static FileSettings GetDefaultFileSettings()
        {
            return new FileSettings();
        }

        public static SecuritySettings GetDefaultSecuritySettings()
        {
            return new SecuritySettings();
        }

        public static PerformanceSettings GetDefaultPerformanceSettings()
        {
            return new PerformanceSettings();
        }
    }

    public class SettingsValidator
    {
        public List<SettingsValidationError> Validate(ApplicationSettings settings)
        {
            var errors = new List<SettingsValidationError>();

            if (string.IsNullOrEmpty(settings.ApplicationName))
                errors.Add(new SettingsValidationError("ApplicationName", "Application name is required"));

            if (settings.Appearance.FontSize < 8 || settings.Appearance.FontSize > 72)
                errors.Add(new SettingsValidationError("Appearance.FontSize", "Font size must be between 8 and 72"));

            if (settings.Editor.Excel.DefaultRowHeight < 1 || settings.Editor.Excel.DefaultRowHeight > 409)
                errors.Add(new SettingsValidationError("Editor.Excel.DefaultRowHeight", "Row height must be between 1 and 409"));

            if (settings.Performance.MaxThreads < 1 || settings.Performance.MaxThreads > 64)
                errors.Add(new SettingsValidationError("Performance.MaxThreads", "Max threads must be between 1 and 64"));

            return errors;
        }
    }

    public class SettingsValidationError
    {
        public string PropertyName { get; set; }
        public string Message { get; set; }
        public string Severity { get; set; }

        public SettingsValidationError(string propertyName, string message, string severity = "Error")
        {
            PropertyName = propertyName;
            Message = message;
            Severity = severity;
        }
    }

    public class SettingsBackup
    {
        private readonly string _backupPath;

        public SettingsBackup(string backupPath)
        {
            _backupPath = backupPath;
            if (!System.IO.Directory.Exists(_backupPath))
                System.IO.Directory.CreateDirectory(_backupPath);
        }

        public void CreateBackup(ApplicationSettings settings, string name = null)
        {
            name = name ?? DateTime.Now.ToString("yyyyMMddHHmmss");
            var fileName = $"settings_backup_{name}.json";
            var filePath = System.IO.Path.Combine(_backupPath, fileName);

            var storage = new JsonSettingsStorage();
            storage.Save(settings, filePath);
        }

        public ApplicationSettings RestoreBackup(string name)
        {
            var fileName = $"settings_backup_{name}.json";
            var filePath = System.IO.Path.Combine(_backupPath, fileName);

            var storage = new JsonSettingsStorage();
            return storage.Load<ApplicationSettings>(filePath);
        }

        public IEnumerable<string> GetAvailableBackups()
        {
            return System.IO.Directory.GetFiles(_backupPath, "settings_backup_*.json")
                .Select(System.IO.Path.GetFileName)
                .Select(name => name.Replace("settings_backup_", "").Replace(".json", ""))
                .OrderByDescending(name => name);
        }

        public void DeleteBackup(string name)
        {
            var fileName = $"settings_backup_{name}.json";
            var filePath = System.IO.Path.Combine(_backupPath, fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }
        }

        public void CleanupOldBackups(int maxBackups)
        {
            var backups = GetAvailableBackups().ToList();
            while (backups.Count > maxBackups)
            {
                DeleteBackup(backups.Last());
                backups.RemoveAt(backups.Count - 1);
            }
        }
    }
}

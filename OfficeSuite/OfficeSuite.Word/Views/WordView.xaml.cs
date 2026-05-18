using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OfficeSuite.Word.Views
{
    public class WordView : UserControl
    {
        private WordViewModel _viewModel;
        private Grid _mainGrid;
        private Ribbon _ribbon;
        private Border _editorArea;
        private StatusBar _statusBar;
        private Grid _rulerArea;

        public WordView()
        {
            InitializeView();
            DataContextChanged += OnDataContextChanged;
        }

        private void InitializeView()
        {
            _mainGrid = new Grid();
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            _mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(24) });

            _ribbon = new Ribbon();
            _ribbon.Height = 120;

            var documentContainer = new Grid();
            documentContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            documentContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            documentContainer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });

            _rulerArea = CreateHorizontalRuler();
            Grid.SetRow(_rulerArea, 0);

            _editorArea = CreateEditorArea();
            Grid.SetRow(_editorArea, 1);

            var bottomRuler = CreateHorizontalRuler();
            Grid.SetRow(bottomRuler, 2);

            _statusBar = new StatusBar();
            InitializeStatusBar();
            Grid.SetRow(_statusBar, 1);

            var sidePanel = new Grid();
            sidePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            sidePanel.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(250) });

            var documentPanel = new Grid();
            documentPanel.Children.Add(_rulerArea);
            documentPanel.Children.Add(_editorArea);
            documentPanel.Children.Add(bottomRuler);

            Grid.SetColumn(documentPanel, 0);
            sidePanel.Children.Add(documentPanel);

            var sideContent = CreateSideContent();
            Grid.SetColumn(sideContent, 1);
            sidePanel.Children.Add(sideContent);

            _mainGrid.Children.Add(_ribbon);
            _mainGrid.Children.Add(sidePanel);
            _mainGrid.Children.Add(_statusBar);

            Content = _mainGrid;
        }

        private Grid CreateHorizontalRuler()
        {
            var ruler = new Grid { Height = 20, Background = Brushes.LightGray };
            return ruler;
        }

        private Border CreateEditorArea()
        {
            var border = new Border
            {
                Background = Brushes.White,
                BorderBrush = Brushes.Gray,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(50)
            };

            var document = new Grid { Background = Brushes.White };
            border.Child = document;

            return border;
        }

        private Grid CreateSideContent()
        {
            var panel = new Grid { Width = 250, Background = Brushes.LightYellow };

            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(200) });

            var navigationPanel = CreateNavigationPanel();
            Grid.SetRow(navigationPanel, 0);

            var propertiesPanel = CreatePropertiesPanel();
            Grid.SetRow(propertiesPanel, 1);

            panel.Children.Add(navigationPanel);
            panel.Children.Add(propertiesPanel);

            return panel;
        }

        private Grid CreateNavigationPanel()
        {
            var panel = new Grid { Background = Brushes.White };

            var tabs = new TabControl();
            tabs.Items.Add(new TabItem { Header = "Navigation", Content = CreateNavigationContent() });
            tabs.Items.Add(new TabItem { Header = "Thumbnails", Content = CreateThumbnailsContent() });

            return new Grid { Background = Brushes.White, Child = tabs };
        }

        private StackPanel CreateNavigationContent()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            panel.Children.Add(new TextBlock { Text = "Navigation Pane", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) });

            var searchBox = new TextBox { PlaceholderText = "Search..." };
            panel.Children.Add(searchBox);

            var resultsList = new ListBox { Height = 200, Margin = new Thickness(0, 10, 0, 0) };
            panel.Children.Add(resultsList);

            return panel;
        }

        private StackPanel CreateThumbnailsContent()
        {
            var panel = new StackPanel { Margin = new Thickness(10) };
            panel.Children.Add(new TextBlock { Text = "Page Thumbnails", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) });

            var thumbnailsList = new ListBox { Height = 300 };
            panel.Children.Add(thumbnailsList);

            return panel;
        }

        private Grid CreatePropertiesPanel()
        {
            var panel = new Grid { Background = Brushes.LightGray };

            var expander = new Expander { Header = "Properties", IsExpanded = true };
            var content = new StackPanel { Margin = new Thickness(10) };

            content.Children.Add(new TextBlock { Text = "Document Properties", FontWeight = FontWeights.Bold });
            content.Children.Add(new TextBox { PlaceholderText = "Title" });
            content.Children.Add(new TextBox { PlaceholderText = "Author" });
            content.Children.Add(new TextBox { PlaceholderText = "Subject" });

            expander.Content = content;
            panel.Children.Add(expander);

            return panel;
        }

        private void InitializeStatusBar()
        {
            var pageInfo = new StatusBarItem { Content = "Page 1 of 1" };
            var wordCount = new StatusBarItem { Content = "0 words" };
            var language = new StatusBarItem { Content = "English (US)" };
            var zoomInfo = new StatusBarItem { Content = "100%" };

            _statusBar.Items.Add(pageInfo);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(wordCount);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(language);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(zoomInfo);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as WordViewModel;
            if (_viewModel != null)
            {
                SetupRibbon();
            }
        }

        private void SetupRibbon()
        {
            _ribbon.Tabs.Clear();

            var homeTab = CreateHomeTab();
            var insertTab = CreateInsertTab();
            var pageLayoutTab = CreatePageLayoutTab();
            var referencesTab = CreateReferencesTab();
            var mailingsTab = CreateMailingsTab();
            var reviewTab = CreateReviewTab();
            var viewTab = CreateViewTab();

            _ribbon.Tabs.Add(homeTab);
            _ribbon.Tabs.Add(insertTab);
            _ribbon.Tabs.Add(pageLayoutTab);
            _ribbon.Tabs.Add(referencesTab);
            _ribbon.Tabs.Add(mailingsTab);
            _ribbon.Tabs.Add(reviewTab);
            _ribbon.Tabs.Add(viewTab);

            _ribbon.SelectedTabIndex = 0;
        }

        private RibbonTab CreateHomeTab()
        {
            var tab = new RibbonTab { Header = "Home" };

            var clipboardGroup = CreateRibbonGroup("Clipboard");
            clipboardGroup.Items.Add(CreateRibbonButton("Paste", "Paste", _viewModel?.PasteCommand));
            clipboardGroup.Items.Add(CreateRibbonButton("Cut", "Cut", _viewModel?.CutCommand));
            clipboardGroup.Items.Add(CreateRibbonButton("Copy", "Copy", _viewModel?.CopyCommand));
            clipboardGroup.Items.Add(CreateRibbonButton("Format Painter", "FormatPainter", _viewModel?.FormatPainterCommand));

            var fontGroup = CreateRibbonGroup("Font");
            fontGroup.Items.Add(CreateFontControls());

            var paragraphGroup = CreateRibbonGroup("Paragraph");
            paragraphGroup.Items.Add(CreateParagraphControls());

            var stylesGroup = CreateRibbonGroup("Styles");
            stylesGroup.Items.Add(CreateStylesControls());

            var editingGroup = CreateRibbonGroup("Editing");
            editingGroup.Items.Add(CreateRibbonButton("Find", "Find", _viewModel?.FindCommand));
            editingGroup.Items.Add(CreateRibbonButton("Replace", "Replace", _viewModel?.ReplaceCommand));
            editingGroup.Items.Add(CreateRibbonButton("Select", "Select", _viewModel?.SelectCommand));

            tab.Groups.Add(clipboardGroup);
            tab.Groups.Add(fontGroup);
            tab.Groups.Add(paragraphGroup);
            tab.Groups.Add(stylesGroup);
            tab.Groups.Add(editingGroup);

            return tab;
        }

        private RibbonTab CreateInsertTab()
        {
            var tab = new RibbonTab { Header = "Insert" };

            var pagesGroup = CreateRibbonGroup("Pages");
            pagesGroup.Items.Add(CreateRibbonButton("Cover Page", "CoverPage", _viewModel?.InsertCoverPageCommand));
            pagesGroup.Items.Add(CreateRibbonButton("Blank Page", "BlankPage", _viewModel?.InsertBlankPageCommand));
            pagesGroup.Items.Add(CreateRibbonButton("Page Break", "PageBreak", _viewModel?.InsertPageBreakCommand));

            var tablesGroup = CreateRibbonGroup("Tables");
            tablesGroup.Items.Add(CreateRibbonButton("Table", "Table", _viewModel?.InsertTableCommand));

            var illustrationsGroup = CreateRibbonGroup("Illustrations");
            illustrationsGroup.Items.Add(CreateRibbonButton("Pictures", "Pictures", _viewModel?.InsertPictureCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("Shapes", "Shapes", _viewModel?.InsertShapeCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("SmartArt", "SmartArt", _viewModel?.InsertSmartArtCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("Chart", "Chart", _viewModel?.InsertChartCommand));

            var linksGroup = CreateRibbonGroup("Links");
            linksGroup.Items.Add(CreateRibbonButton("Link", "Link", _viewModel?.InsertHyperlinkCommand));
            linksGroup.Items.Add(CreateRibbonButton("Bookmark", "Bookmark", _viewModel?.InsertBookmarkCommand));
            linksGroup.Items.Add(CreateRibbonButton("Cross-reference", "CrossRef", _viewModel?.InsertCrossReferenceCommand));

            var headerFooterGroup = CreateRibbonGroup("Header & Footer");
            headerFooterGroup.Items.Add(CreateRibbonButton("Header", "Header", _viewModel?.InsertHeaderCommand));
            headerFooterGroup.Items.Add(CreateRibbonButton("Footer", "Footer", _viewModel?.InsertFooterCommand));
            headerFooterGroup.Items.Add(CreateRibbonButton("Page Number", "PageNumber", _viewModel?.InsertPageNumberCommand));

            var textGroup = CreateRibbonGroup("Text");
            textGroup.Items.Add(CreateRibbonButton("Text Box", "TextBox", _viewModel?.InsertTextBoxCommand));
            textGroup.Items.Add(CreateRibbonButton("Quick Parts", "QuickParts", _viewModel?.InsertQuickPartCommand));
            textGroup.Items.Add(CreateRibbonButton("Date & Time", "DateTime", _viewModel?.InsertDateTimeCommand));
            textGroup.Items.Add(CreateRibbonButton("WordArt", "WordArt", _viewModel?.InsertWordArtCommand));

            var symbolsGroup = CreateRibbonGroup("Symbols");
            symbolsGroup.Items.Add(CreateRibbonButton("Equation", "Equation", _viewModel?.InsertEquationCommand));
            symbolsGroup.Items.Add(CreateRibbonButton("Symbol", "Symbol", _viewModel?.InsertSymbolCommand));

            tab.Groups.Add(pagesGroup);
            tab.Groups.Add(tablesGroup);
            tab.Groups.Add(illustrationsGroup);
            tab.Groups.Add(linksGroup);
            tab.Groups.Add(headerFooterGroup);
            tab.Groups.Add(textGroup);
            tab.Groups.Add(symbolsGroup);

            return tab;
        }

        private RibbonTab CreatePageLayoutTab()
        {
            var tab = new RibbonTab { Header = "Layout" };

            var pageSetupGroup = CreateRibbonGroup("Page Setup");
            pageSetupGroup.Items.Add(CreateRibbonButton("Margins", "Margins", _viewModel?.SetMarginsCommand));
            pageSetupGroup.Items.Add(CreateRibbonButton("Orientation", "Orientation", _viewModel?.SetOrientationCommand));
            pageSetupGroup.Items.Add(CreateRibbonButton("Size", "Size", _viewModel?.SetPageSizeCommand));
            pageSetupGroup.Items.Add(CreateRibbonButton("Columns", "Columns", _viewModel?.SetColumnsCommand));

            var pageSetupGroup2 = CreateRibbonGroup("Page Setup");
            pageSetupGroup2.Items.Add(CreateRibbonButton("Breaks", "Breaks", _viewModel?.InsertBreakCommand));
            pageSetupGroup2.Items.Add(CreateRibbonButton("Line Numbers", "LineNumbers", _viewModel?.SetLineNumbersCommand));

            var paragraphGroup = CreateRibbonGroup("Paragraph");
            paragraphGroup.Items.Add(CreateRibbonButton("Indent", "Indent", _viewModel?.SetIndentationCommand));
            paragraphGroup.Items.Add(CreateRibbonButton("Spacing", "Spacing", _viewModel?.SetLineSpacingCommand));

            var arrangeGroup = CreateRibbonGroup("Arrange");
            arrangeGroup.Items.Add(CreateRibbonButton("Position", "Position", _viewModel?.SetPositionCommand));
            arrangeGroup.Items.Add(CreateRibbonButton("Wrap Text", "WrapText", _viewModel?.SetTextWrapCommand));

            tab.Groups.Add(pageSetupGroup);
            tab.Groups.Add(pageSetupGroup2);
            tab.Groups.Add(paragraphGroup);
            tab.Groups.Add(arrangeGroup);

            return tab;
        }

        private RibbonTab CreateReferencesTab()
        {
            var tab = new RibbonTab { Header = "References" };

            var tableOfContentsGroup = CreateRibbonGroup("Table of Contents");
            tableOfContentsGroup.Items.Add(CreateRibbonButton("Table of Contents", "TOC", _viewModel?.InsertTableOfContentsCommand));
            tableOfContentsGroup.Items.Add(CreateRibbonButton("Add Text", "AddText", _viewModel?.AddTextToTocCommand));
            tableOfContentsGroup.Items.Add(CreateRibbonButton("Update Table", "UpdateToc", _viewModel?.UpdateTocCommand));

            var footnotesGroup = CreateRibbonGroup("Footnotes");
            footnotesGroup.Items.Add(CreateRibbonButton("Insert Footnote", "InsertFootnote", _viewModel?.InsertFootnoteCommand));
            footnotesGroup.Items.Add(CreateRibbonButton("Insert Endnote", "InsertEndnote", _viewModel?.InsertEndnoteCommand));
            footnotesGroup.Items.Add(CreateRibbonButton("Next Footnote", "NextFootnote", _viewModel?.NextFootnoteCommand));

            var citationsGroup = CreateRibbonGroup("Citations & Bibliography");
            citationsGroup.Items.Add(CreateRibbonButton("Insert Citation", "InsertCitation", _viewModel?.InsertCitationCommand));
            citationsGroup.Items.Add(CreateRibbonButton("Bibliography", "Bibliography", _viewModel?.InsertBibliographyCommand));
            citationsGroup.Items.Add(CreateRibbonButton("Manage Sources", "ManageSources", _viewModel?.ManageSourcesCommand));

            var captionsGroup = CreateRibbonGroup("Captions");
            captionsGroup.Items.Add(CreateRibbonButton("Insert Caption", "InsertCaption", _viewModel?.InsertCaptionCommand));
            captionsGroup.Items.Add(CreateRibbonButton("Insert Table of Figures", "InsertTof", _viewModel?.InsertTableOfFiguresCommand));
            captionsGroup.Items.Add(CreateRibbonButton("Cross-reference", "CrossRef", _viewModel?.InsertCrossReferenceCommand));

            var indexGroup = CreateRibbonGroup("Index");
            indexGroup.Items.Add(CreateRibbonButton("Mark Entry", "MarkEntry", _viewModel?.MarkIndexEntryCommand));
            indexGroup.Items.Add(CreateRibbonButton("Insert Index", "InsertIndex", _viewModel?.InsertIndexCommand));
            indexGroup.Items.Add(CreateRibbonButton("Update Index", "UpdateIndex", _viewModel?.UpdateIndexCommand));

            tab.Groups.Add(tableOfContentsGroup);
            tab.Groups.Add(footnotesGroup);
            tab.Groups.Add(citationsGroup);
            tab.Groups.Add(captionsGroup);
            tab.Groups.Add(indexGroup);

            return tab;
        }

        private RibbonTab CreateMailingsTab()
        {
            var tab = new RibbonTab { Header = "Mailings" };

            var createGroup = CreateRibbonGroup("Create");
            createGroup.Items.Add(CreateRibbonButton("Letters", "Letters", _viewModel?.CreateLetterCommand));
            createGroup.Items.Add(CreateRibbonButton("Envelopes", "Envelopes", _viewModel?.CreateEnvelopeCommand));
            createGroup.Items.Add(CreateRibbonButton("Labels", "Labels", _viewModel?.CreateLabelCommand));

            var startMailMergeGroup = CreateRibbonGroup("Start Mail Merge");
            startMailMergeGroup.Items.Add(CreateRibbonButton("Letters", "Letters", _viewModel?.StartLetterMergeCommand));
            startMailMergeGroup.Items.Add(CreateRibbonButton("Email", "Email", _viewModel?.StartEmailMergeCommand));
            startMailMergeGroup.Items.Add(CreateRibbonButton("Labels", "Labels", _viewModel?.StartLabelMergeCommand));
            startMailMergeGroup.Items.Add(CreateRibbonButton("Directory", "Directory", _viewModel?.StartDirectoryMergeCommand));

            var writeAndInsertGroup = CreateRibbonGroup("Write & Insert Fields");
            writeAndInsertGroup.Items.Add(CreateRibbonButton("Address Block", "AddressBlock", _viewModel?.InsertAddressBlockCommand));
            writeAndInsertGroup.Items.Add(CreateRibbonButton("Greeting Line", "GreetingLine", _viewModel?.InsertGreetingLineCommand));
            writeAndInsertGroup.Items.Add(CreateRibbonButton("Date & Time", "DateTime", _viewModel?.InsertDateTimeCommand));
            writeAndInsertGroup.Items.Add(CreateRibbonButton("Merge Field", "MergeField", _viewModel?.InsertMergeFieldCommand));
            writeAndInsertGroup.Items.Add(CreateRibbonButton("Rules", "Rules", _viewModel?.InsertRuleCommand));

            var previewResultsGroup = CreateRibbonGroup("Preview Results");
            previewResultsGroup.Items.Add(CreateRibbonButton("Preview Results", "Preview", _viewModel?.PreviewMergeCommand));
            previewResultsGroup.Items.Add(CreateRibbonButton("First Record", "First", _viewModel?.FirstRecordCommand));
            previewResultsGroup.Items.Add(CreateRibbonButton("Previous Record", "Previous", _viewModel?.PreviousRecordCommand));
            previewResultsGroup.Items.Add(CreateRibbonButton("Next Record", "Next", _viewModel?.NextRecordCommand));
            previewResultsGroup.Items.Add(CreateRibbonButton("Last Record", "Last", _viewModel?.LastRecordCommand));

            var finishGroup = CreateRibbonGroup("Finish");
            finishGroup.Items.Add(CreateRibbonButton("Finish & Merge", "FinishMerge", _viewModel?.FinishMergeCommand));

            tab.Groups.Add(createGroup);
            tab.Groups.Add(startMailMergeGroup);
            tab.Groups.Add(writeAndInsertGroup);
            tab.Groups.Add(previewResultsGroup);
            tab.Groups.Add(finishGroup);

            return tab;
        }

        private RibbonTab CreateReviewTab()
        {
            var tab = new RibbonTab { Header = "Review" };

            var proofingGroup = CreateRibbonGroup("Proofing");
            proofingGroup.Items.Add(CreateRibbonButton("Spelling", "Spelling", _viewModel?.CheckSpellingCommand));
            proofingGroup.Items.Add(CreateRibbonButton("Research", "Research", _viewModel?.DoResearchCommand));
            proofingGroup.Items.Add(CreateRibbonButton("Thesaurus", "Thesaurus", _viewModel?.ShowThesaurusCommand));
            proofingGroup.Items.Add(CreateRibbonButton("Word Count", "WordCount", _viewModel?.ShowWordCountCommand));
            proofingGroup.Items.Add(CreateRibbonButton("Translate", "Translate", _viewModel?.TranslateCommand));

            var commentsGroup = CreateRibbonGroup("Comments");
            commentsGroup.Items.Add(CreateRibbonButton("New Comment", "NewComment", _viewModel?.NewCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Delete", "Delete", _viewModel?.DeleteCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Previous", "Previous", _viewModel?.PreviousCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Next", "Next", _viewModel?.NextCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Show Comments", "Show", _viewModel?.ToggleCommentsCommand));

            var trackingGroup = CreateRibbonGroup("Tracking");
            trackingGroup.Items.Add(CreateRibbonButton("Track Changes", "TrackChanges", _viewModel?.ToggleTrackChangesCommand));
            trackingGroup.Items.Add("Bold", "Accept", _viewModel?.AcceptChangeCommand);
            trackingGroup.Items.Add("Bold", "Reject", _viewModel?.RejectChangeCommand));
            trackingGroup.Items.Add(CreateRibbonButton("Accept All", "AcceptAll", _viewModel?.AcceptAllChangesCommand));
            trackingGroup.Items.Add(CreateRibbonButton("Reject All", "RejectAll", _viewModel?.RejectAllChangesCommand));
            trackingGroup.Items.Add(CreateRibbonButton("Previous", "Previous", _viewModel?.PreviousChangeCommand));
            trackingGroup.Items.Add(CreateRibbonButton("Next", "Next", _viewModel?.NextChangeCommand));

            var changesGroup = CreateRibbonGroup("Compare");
            changesGroup.Items.Add(CreateRibbonButton("Compare", "Compare", _viewModel?.CompareDocumentsCommand));
            changesGroup.Items.Add(CreateRibbonButton("Combine", "Combine", _viewModel?.CombineRevisionsCommand));
            changesGroup.Items.Add(CreateRibbonButton("Show Markup", "ShowMarkup", _viewModel?.ShowMarkupCommand));

            var protectGroup = CreateRibbonGroup("Protect");
            protectGroup.Items.Add(CreateRibbonButton("Protect Document", "Protect", _viewModel?.ProtectDocumentCommand));
            protectGroup.Items.Add(CreateRibbonButton("Restrict Editing", "Restrict", _viewModel?.RestrictEditingCommand));
            protectGroup.Items.Add(CreateRibbonButton("Inspect Document", "Inspect", _viewModel?.InspectDocumentCommand));
            protectGroup.Items.Add(CreateRibbonButton("Mark as Final", "MarkFinal", _viewModel?.MarkAsFinalCommand));

            tab.Groups.Add(proofingGroup);
            tab.Groups.Add(commentsGroup);
            tab.Groups.Add(trackingGroup);
            tab.Groups.Add(changesGroup);
            tab.Groups.Add(protectGroup);

            return tab;
        }

        private RibbonTab CreateViewTab()
        {
            var tab = new RibbonTab { Header = "View" };

            var documentViewsGroup = CreateRibbonGroup("Document Views");
            documentViewsGroup.Items.Add(CreateRibbonButton("Print Layout", "PrintLayout", _viewModel?.ShowPrintLayoutCommand));
            documentViewsGroup.Items.Add(CreateRibbonButton("Read Mode", "ReadMode", _viewModel?.ShowReadModeCommand));
            documentViewsGroup.Items.Add(CreateRibbonButton("Web Layout", "WebLayout", _viewModel?.ShowWebLayoutCommand));
            documentViewsGroup.Items.Add(CreateRibbonButton("Outline", "Outline", _viewModel?.ShowOutlineViewCommand));
            documentViewsGroup.Items.Add(CreateRibbonButton("Draft", "Draft", _viewModel?.ShowDraftViewCommand));

            var showGroup = CreateRibbonGroup("Show");
            showGroup.Items.Add(CreateRibbonButton("Ruler", "Ruler", _viewModel?.ToggleRulerCommand));
            showGroup.Items.Add(CreateRibbonButton("Gridlines", "Gridlines", _viewModel?.ToggleGridlinesCommand));
            showGroup.Items.Add(CreateRibbonButton("Navigation Pane", "NavPane", _viewModel?.ToggleNavigationPaneCommand));

            var zoomGroup = CreateRibbonGroup("Zoom");
            zoomGroup.Items.Add(CreateRibbonButton("Zoom", "Zoom", _viewModel?.SetZoomCommand));
            zoomGroup.Items.Add(CreateRibbonButton("100%", "Zoom100", _viewModel?.SetZoom100Command));
            zoomGroup.Items.Add(CreateRibbonButton("Fit to Window", "FitWindow", _viewModel?.FitToWindowCommand));
            zoomGroup.Items.Add(CreateRibbonButton("One Page", "OnePage", _viewModel?.ShowOnePageCommand));
            zoomGroup.Items.Add(CreateRibbonButton("Two Pages", "TwoPages", _viewModel?.ShowTwoPagesCommand));

            var windowGroup = CreateRibbonGroup("Window");
            windowGroup.Items.Add(CreateRibbonButton("New Window", "NewWindow", _viewModel?.NewWindowCommand));
            windowGroup.Items.Add(CreateRibbonButton("Arrange All", "ArrangeAll", _viewModel?.ArrangeAllWindowsCommand));
            windowGroup.Items.Add(CreateRibbonButton("Split", "Split", _viewModel?.SplitWindowCommand));

            tab.Groups.Add(documentViewsGroup);
            tab.Groups.Add(showGroup);
            tab.Groups.Add(zoomGroup);
            tab.Groups.Add(windowGroup);

            return tab;
        }

        private RibbonGroup CreateRibbonGroup(string name)
        {
            return new RibbonGroup { Header = name };
        }

        private RibbonButton CreateRibbonButton(string label, string name, ICommand command)
        {
            return new RibbonButton
            {
                Label = label,
                Name = name,
                Command = command,
                Width = 50
            };
        }

        private StackPanel CreateFontControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var fontCombo = new ComboBox { Width = 150 };
            fontCombo.ItemsSource = GetFontFamilies();

            var fontSizeCombo = new ComboBox { Width = 60 };
            fontSizeCombo.ItemsSource = GetFontSizes();

            var boldButton = new ToggleButton { Content = "B", FontWeight = FontWeights.Bold, Width = 30 };
            var italicButton = new ToggleButton { Content = "I", FontStyle = FontStyles.Italic, Width = 30 };
            var underlineButton = new ToggleButton { Content = "U", Width = 30 };

            var fontColorButton = new Button { Content = "A", Width = 30 };
            var highlightButton = new Button { Content = "H", Width = 30 };

            panel.Children.Add(fontCombo);
            panel.Children.Add(fontSizeCombo);
            panel.Children.Add(boldButton);
            panel.Children.Add(italicButton);
            panel.Children.Add(underlineButton);
            panel.Children.Add(fontColorButton);
            panel.Children.Add(highlightButton);

            return panel;
        }

        private StackPanel CreateParagraphControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var alignLeftButton = new Button { Content = "Left", Width = 40 };
            var alignCenterButton = new Button { Content = "Center", Width = 50 };
            var alignRightButton = new Button { Content = "Right", Width = 40 };
            var alignJustifyButton = new Button { Content = "Justify", Width = 50 };

            var bulletButton = new ToggleButton { Content = "Bullet", Width = 50 };
            var numberingButton = new ToggleButton { Content = "Number", Width = 50 };
            var decreaseIndentButton = new Button { Content = "Decrease", Width = 60 };
            var increaseIndentButton = new Button { Content = "Increase", Width = 60 };

            var shadingButton = new Button { Content = "Shading", Width = 60 };
            var borderButton = new Button { Content = "Border", Width = 50 };

            panel.Children.Add(alignLeftButton);
            panel.Children.Add(alignCenterButton);
            panel.Children.Add(alignRightButton);
            panel.Children.Add(alignJustifyButton);
            panel.Children.Add(bulletButton);
            panel.Children.Add(numberingButton);
            panel.Children.Add(decreaseIndentButton);
            panel.Children.Add(increaseIndentButton);
            panel.Children.Add(shadingButton);
            panel.Children.Add(borderButton);

            return panel;
        }

        private StackPanel CreateStylesControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var stylesCombo = new ComboBox { Width = 150 };
            stylesCombo.ItemsSource = GetStyles();

            var applyButton = new Button { Content = "Apply", Width = 60 };
            var newStyleButton = new Button { Content = "New", Width = 40 };

            panel.Children.Add(stylesCombo);
            panel.Children.Add(applyButton);
            panel.Children.Add(newStyleButton);

            return panel;
        }

        private string[] GetFontFamilies()
        {
            return new[] { "Calibri", "Arial", "Times New Roman", "Verdana", "Tahoma", "Georgia", "Cambria", "Consolas" };
        }

        private string[] GetFontSizes()
        {
            return new[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72", "96" };
        }

        private string[] GetStyles()
        {
            return new[] { "Normal", "Heading 1", "Heading 2", "Heading 3", "Title", "Subtitle", "Quote", "Caption" };
        }
    }

    public class WordViewModel : ViewModelBase
    {
        private string _documentContent;
        private string _selectedText;
        private int _currentPage;
        private int _totalPages;
        private int _wordCount;
        private bool _isTrackChangesEnabled;
        private bool _showFormattingMarks;
        private double _zoomLevel;

        public string DocumentContent
        {
            get => _documentContent;
            set => SetProperty(ref _documentContent, value);
        }

        public string SelectedText
        {
            get => _selectedText;
            set => SetProperty(ref _selectedText, value);
        }

        public int CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public int TotalPages
        {
            get => _totalPages;
            set => SetProperty(ref _totalPages, value);
        }

        public int WordCount
        {
            get => _wordCount;
            set => SetProperty(ref _wordCount, value);
        }

        public bool IsTrackChangesEnabled
        {
            get => _isTrackChangesEnabled;
            set => SetProperty(ref _isTrackChangesEnabled, value);
        }

        public bool ShowFormattingMarks
        {
            get => _showFormattingMarks;
            set => SetProperty(ref _showFormattingMarks, value);
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        public ICommand PasteCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand FormatPainterCommand { get; }
        public ICommand FindCommand { get; }
        public ICommand ReplaceCommand { get; }
        public ICommand SelectCommand { get; }
        public ICommand InsertCoverPageCommand { get; }
        public ICommand InsertBlankPageCommand { get; }
        public ICommand InsertPageBreakCommand { get; }
        public ICommand InsertTableCommand { get; }
        public ICommand InsertPictureCommand { get; }
        public ICommand InsertShapeCommand { get; }
        public ICommand InsertSmartArtCommand { get; }
        public ICommand InsertChartCommand { get; }
        public ICommand InsertHyperlinkCommand { get; }
        public ICommand InsertBookmarkCommand { get; }
        public ICommand InsertCrossReferenceCommand { get; }
        public ICommand InsertHeaderCommand { get; }
        public ICommand InsertFooterCommand { get; }
        public ICommand InsertPageNumberCommand { get; }
        public ICommand InsertTextBoxCommand { get; }
        public ICommand InsertQuickPartCommand { get; }
        public ICommand InsertDateTimeCommand { get; }
        public ICommand InsertWordArtCommand { get; }
        public ICommand InsertEquationCommand { get; }
        public ICommand InsertSymbolCommand { get; }
        public ICommand SetMarginsCommand { get; }
        public ICommand SetOrientationCommand { get; }
        public ICommand SetPageSizeCommand { get; }
        public ICommand SetColumnsCommand { get; }
        public ICommand InsertBreakCommand { get; }
        public ICommand SetLineNumbersCommand { get; }
        public ICommand SetIndentationCommand { get; }
        public ICommand SetLineSpacingCommand { get; }
        public ICommand SetPositionCommand { get; }
        public ICommand SetTextWrapCommand { get; }
        public ICommand InsertTableOfContentsCommand { get; }
        public ICommand AddTextToTocCommand { get; }
        public ICommand UpdateTocCommand { get; }
        public ICommand InsertFootnoteCommand { get; }
        public ICommand InsertEndnoteCommand { get; }
        public ICommand NextFootnoteCommand { get; }
        public ICommand InsertCitationCommand { get; }
        public ICommand InsertBibliographyCommand { get; }
        public ICommand ManageSourcesCommand { get; }
        public ICommand InsertCaptionCommand { get; }
        public ICommand InsertTableOfFiguresCommand { get; }
        public ICommand MarkIndexEntryCommand { get; }
        public ICommand InsertIndexCommand { get; }
        public ICommand UpdateIndexCommand { get; }
        public ICommand CreateLetterCommand { get; }
        public ICommand CreateEnvelopeCommand { get; }
        public ICommand CreateLabelCommand { get; }
        public ICommand StartLetterMergeCommand { get; }
        public ICommand StartEmailMergeCommand { get; }
        public ICommand StartLabelMergeCommand { get; }
        public ICommand StartDirectoryMergeCommand { get; }
        public ICommand InsertAddressBlockCommand { get; }
        public ICommand InsertGreetingLineCommand { get; }
        public ICommand InsertMergeFieldCommand { get; }
        public ICommand InsertRuleCommand { get; }
        public ICommand PreviewMergeCommand { get; }
        public ICommand FirstRecordCommand { get; }
        public ICommand PreviousRecordCommand { get; }
        public ICommand NextRecordCommand { get; }
        public ICommand LastRecordCommand { get; }
        public ICommand FinishMergeCommand { get; }
        public ICommand CheckSpellingCommand { get; }
        public ICommand DoResearchCommand { get; }
        public ICommand ShowThesaurusCommand { get; }
        public ICommand ShowWordCountCommand { get; }
        public ICommand TranslateCommand { get; }
        public ICommand NewCommentCommand { get; }
        public ICommand DeleteCommentCommand { get; }
        public ICommand PreviousCommentCommand { get; }
        public ICommand NextCommentCommand { get; }
        public ICommand ToggleCommentsCommand { get; }
        public ICommand ToggleTrackChangesCommand { get; }
        public ICommand AcceptChangeCommand { get; }
        public ICommand RejectChangeCommand { get; }
        public ICommand AcceptAllChangesCommand { get; }
        public ICommand RejectAllChangesCommand { get; }
        public ICommand PreviousChangeCommand { get; }
        public ICommand NextChangeCommand { get; }
        public ICommand CompareDocumentsCommand { get; }
        public ICommand CombineRevisionsCommand { get; }
        public ICommand ShowMarkupCommand { get; }
        public ICommand ProtectDocumentCommand { get; }
        public ICommand RestrictEditingCommand { get; }
        public ICommand InspectDocumentCommand { get; }
        public ICommand MarkAsFinalCommand { get; }
        public ICommand ShowPrintLayoutCommand { get; }
        public ICommand ShowReadModeCommand { get; }
        public ICommand ShowWebLayoutCommand { get; }
        public ICommand ShowOutlineViewCommand { get; }
        public ICommand ShowDraftViewCommand { get; }
        public ICommand ToggleRulerCommand { get; }
        public ICommand ToggleGridlinesCommand { get; }
        public ICommand ToggleNavigationPaneCommand { get; }
        public ICommand SetZoomCommand { get; }
        public ICommand SetZoom100Command { get; }
        public ICommand FitToWindowCommand { get; }
        public ICommand ShowOnePageCommand { get; }
        public ICommand ShowTwoPagesCommand { get; }
        public ICommand NewWindowCommand { get; }
        public ICommand ArrangeAllWindowsCommand { get; }
        public ICommand SplitWindowCommand { get; }

        public WordViewModel()
        {
            PasteCommand = new RelayCommand(_ => Paste());
            CutCommand = new RelayCommand(_ => Cut());
            CopyCommand = new RelayCommand(_ => Copy());
            FormatPainterCommand = new RelayCommand(_ => FormatPainter());
            FindCommand = new RelayCommand(_ => Find());
            ReplaceCommand = new RelayCommand(_ => Replace());
            SelectCommand = new RelayCommand(_ => SelectAll());
            InsertCoverPageCommand = new RelayCommand(_ => InsertCoverPage());
            InsertBlankPageCommand = new RelayCommand(_ => InsertBlankPage());
            InsertPageBreakCommand = new RelayCommand(_ => InsertPageBreak());
            InsertTableCommand = new RelayCommand(_ => InsertTable());
            InsertPictureCommand = new RelayCommand(_ => InsertPicture());
            InsertShapeCommand = new RelayCommand(_ => InsertShape());
            InsertSmartArtCommand = new RelayCommand(_ => InsertSmartArt());
            InsertChartCommand = new RelayCommand(_ => InsertChart());
            InsertHyperlinkCommand = new RelayCommand(_ => InsertHyperlink());
            InsertBookmarkCommand = new RelayCommand(_ => InsertBookmark());
            InsertCrossReferenceCommand = new RelayCommand(_ => InsertCrossReference());
            InsertHeaderCommand = new RelayCommand(_ => InsertHeader());
            InsertFooterCommand = new RelayCommand(_ => InsertFooter());
            InsertPageNumberCommand = new RelayCommand(_ => InsertPageNumber());
            InsertTextBoxCommand = new RelayCommand(_ => InsertTextBox());
            InsertQuickPartCommand = new RelayCommand(_ => InsertQuickPart());
            InsertDateTimeCommand = new RelayCommand(_ => InsertDateTime());
            InsertWordArtCommand = new RelayCommand(_ => InsertWordArt());
            InsertEquationCommand = new RelayCommand(_ => InsertEquation());
            InsertSymbolCommand = new RelayCommand(_ => InsertSymbol());
            SetMarginsCommand = new RelayCommand(_ => SetMargins());
            SetOrientationCommand = new RelayCommand(_ => SetOrientation());
            SetPageSizeCommand = new RelayCommand(_ => SetPageSize());
            SetColumnsCommand = new RelayCommand(_ => SetColumns());
            InsertBreakCommand = new RelayCommand(_ => InsertBreak());
            SetLineNumbersCommand = new RelayCommand(_ => SetLineNumbers());
            SetIndentationCommand = new RelayCommand(_ => SetIndentation());
            SetLineSpacingCommand = new RelayCommand(_ => SetLineSpacing());
            SetPositionCommand = new RelayCommand(_ => SetPosition());
            SetTextWrapCommand = new RelayCommand(_ => SetTextWrap());
            InsertTableOfContentsCommand = new RelayCommand(_ => InsertTableOfContents());
            AddTextToTocCommand = new RelayCommand(_ => AddTextToToc());
            UpdateTocCommand = new RelayCommand(_ => UpdateToc());
            InsertFootnoteCommand = new RelayCommand(_ => InsertFootnote());
            InsertEndnoteCommand = new RelayCommand(_ => InsertEndnote());
            NextFootnoteCommand = new RelayCommand(_ => NextFootnote());
            InsertCitationCommand = new RelayCommand(_ => InsertCitation());
            InsertBibliographyCommand = new RelayCommand(_ => InsertBibliography());
            ManageSourcesCommand = new RelayCommand(_ => ManageSources());
            InsertCaptionCommand = new RelayCommand(_ => InsertCaption());
            InsertTableOfFiguresCommand = new RelayCommand(_ => InsertTableOfFigures());
            MarkIndexEntryCommand = new RelayCommand(_ => MarkIndexEntry());
            InsertIndexCommand = new RelayCommand(_ => InsertIndex());
            UpdateIndexCommand = new RelayCommand(_ => UpdateIndex());
            CreateLetterCommand = new RelayCommand(_ => CreateLetter());
            CreateEnvelopeCommand = new RelayCommand(_ => CreateEnvelope());
            CreateLabelCommand = new RelayCommand(_ => CreateLabel());
            StartLetterMergeCommand = new RelayCommand(_ => StartLetterMerge());
            StartEmailMergeCommand = new RelayCommand(_ => StartEmailMerge());
            StartLabelMergeCommand = new RelayCommand(_ => StartLabelMerge());
            StartDirectoryMergeCommand = new RelayCommand(_ => StartDirectoryMerge());
            InsertAddressBlockCommand = new RelayCommand(_ => InsertAddressBlock());
            InsertGreetingLineCommand = new RelayCommand(_ => InsertGreetingLine());
            InsertMergeFieldCommand = new RelayCommand(_ => InsertMergeField());
            InsertRuleCommand = new RelayCommand(_ => InsertRule());
            PreviewMergeCommand = new RelayCommand(_ => PreviewMerge());
            FirstRecordCommand = new RelayCommand(_ => FirstRecord());
            PreviousRecordCommand = new RelayCommand(_ => PreviousRecord());
            NextRecordCommand = new RelayCommand(_ => NextRecord());
            LastRecordCommand = new RelayCommand(_ => LastRecord());
            FinishMergeCommand = new RelayCommand(_ => FinishMerge());
            CheckSpellingCommand = new RelayCommand(_ => CheckSpelling());
            DoResearchCommand = new RelayCommand(_ => DoResearch());
            ShowThesaurusCommand = new RelayCommand(_ => ShowThesaurus());
            ShowWordCountCommand = new RelayCommand(_ => ShowWordCount());
            TranslateCommand = new RelayCommand(_ => Translate());
            NewCommentCommand = new RelayCommand(_ => NewComment());
            DeleteCommentCommand = new RelayCommand(_ => DeleteComment());
            PreviousCommentCommand = new RelayCommand(_ => PreviousComment());
            NextCommentCommand = new RelayCommand(_ => NextComment());
            ToggleCommentsCommand = new RelayCommand(_ => ToggleComments());
            ToggleTrackChangesCommand = new RelayCommand(_ => ToggleTrackChanges());
            AcceptChangeCommand = new RelayCommand(_ => AcceptChange());
            RejectChangeCommand = new RelayCommand(_ => RejectChange());
            AcceptAllChangesCommand = new RelayCommand(_ => AcceptAllChanges());
            RejectAllChangesCommand = new RelayCommand(_ => RejectAllChanges());
            PreviousChangeCommand = new RelayCommand(_ => PreviousChange());
            NextChangeCommand = new RelayCommand(_ => NextChange());
            CompareDocumentsCommand = new RelayCommand(_ => CompareDocuments());
            CombineRevisionsCommand = new RelayCommand(_ => CombineRevisions());
            ShowMarkupCommand = new RelayCommand(_ => ShowMarkup());
            ProtectDocumentCommand = new RelayCommand(_ => ProtectDocument());
            RestrictEditingCommand = new RelayCommand(_ => RestrictEditing());
            InspectDocumentCommand = new RelayCommand(_ => InspectDocument());
            MarkAsFinalCommand = new RelayCommand(_ => MarkAsFinal());
            ShowPrintLayoutCommand = new RelayCommand(_ => ShowPrintLayout());
            ShowReadModeCommand = new RelayCommand(_ => ShowReadMode());
            ShowWebLayoutCommand = new RelayCommand(_ => ShowWebLayout());
            ShowOutlineViewCommand = new RelayCommand(_ => ShowOutlineView());
            ShowDraftViewCommand = new RelayCommand(_ => ShowDraftView());
            ToggleRulerCommand = new RelayCommand(_ => ToggleRuler());
            ToggleGridlinesCommand = new RelayCommand(_ => ToggleGridlines());
            ToggleNavigationPaneCommand = new RelayCommand(_ => ToggleNavigationPane());
            SetZoomCommand = new RelayCommand(_ => SetZoom());
            SetZoom100Command = new RelayCommand(_ => SetZoom100());
            FitToWindowCommand = new RelayCommand(_ => FitToWindow());
            ShowOnePageCommand = new RelayCommand(_ => ShowOnePage());
            ShowTwoPagesCommand = new RelayCommand(_ => ShowTwoPages());
            NewWindowCommand = new RelayCommand(_ => NewWindow());
            ArrangeAllWindowsCommand = new RelayCommand(_ => ArrangeAllWindows());
            SplitWindowCommand = new RelayCommand(_ => SplitWindow());

            _zoomLevel = 100;
            _isTrackChangesEnabled = false;
            _showFormattingMarks = false;
        }

        private void Paste() { }
        private void Cut() { }
        private void Copy() { }
        private void FormatPainter() { }
        private void Find() { }
        private void Replace() { }
        private void SelectAll() { }
        private void InsertCoverPage() { }
        private void InsertBlankPage() { }
        private void InsertPageBreak() { }
        private void InsertTable() { }
        private void InsertPicture() { }
        private void InsertShape() { }
        private void InsertSmartArt() { }
        private void InsertChart() { }
        private void InsertHyperlink() { }
        private void InsertBookmark() { }
        private void InsertCrossReference() { }
        private void InsertHeader() { }
        private void InsertFooter() { }
        private void InsertPageNumber() { }
        private void InsertTextBox() { }
        private void InsertQuickPart() { }
        private void InsertDateTime() { }
        private void InsertWordArt() { }
        private void InsertEquation() { }
        private void InsertSymbol() { }
        private void SetMargins() { }
        private void SetOrientation() { }
        private void SetPageSize() { }
        private void SetColumns() { }
        private void InsertBreak() { }
        private void SetLineNumbers() { }
        private void SetIndentation() { }
        private void SetLineSpacing() { }
        private void SetPosition() { }
        private void SetTextWrap() { }
        private void InsertTableOfContents() { }
        private void AddTextToToc() { }
        private void UpdateToc() { }
        private void InsertFootnote() { }
        private void InsertEndnote() { }
        private void NextFootnote() { }
        private void InsertCitation() { }
        private void InsertBibliography() { }
        private void ManageSources() { }
        private void InsertCaption() { }
        private void InsertTableOfFigures() { }
        private void MarkIndexEntry() { }
        private void InsertIndex() { }
        private void UpdateIndex() { }
        private void CreateLetter() { }
        private void CreateEnvelope() { }
        private void CreateLabel() { }
        private void StartLetterMerge() { }
        private void StartEmailMerge() { }
        private void StartLabelMerge() { }
        private void StartDirectoryMerge() { }
        private void InsertAddressBlock() { }
        private void InsertGreetingLine() { }
        private void InsertMergeField() { }
        private void InsertRule() { }
        private void PreviewMerge() { }
        private void FirstRecord() { }
        private void PreviousRecord() { }
        private void NextRecord() { }
        private void LastRecord() { }
        private void FinishMerge() { }
        private void CheckSpelling() { }
        private void DoResearch() { }
        private void ShowThesaurus() { }
        private void ShowWordCount() { }
        private void Translate() { }
        private void NewComment() { }
        private void DeleteComment() { }
        private void PreviousComment() { }
        private void NextComment() { }
        private void ToggleComments() { }
        private void ToggleTrackChanges() { }
        private void AcceptChange() { }
        private void RejectChange() { }
        private void AcceptAllChanges() { }
        private void RejectAllChanges() { }
        private void PreviousChange() { }
        private void NextChange() { }
        private void CompareDocuments() { }
        private void CombineRevisions() { }
        private void ShowMarkup() { }
        private void ProtectDocument() { }
        private void RestrictEditing() { }
        private void InspectDocument() { }
        private void MarkAsFinal() { }
        private void ShowPrintLayout() { }
        private void ShowReadMode() { }
        private void ShowWebLayout() { }
        private void ShowOutlineView() { }
        private void ShowDraftView() { }
        private void ToggleRuler() { }
        private void ToggleGridlines() { }
        private void ToggleNavigationPane() { }
        private void SetZoom() { }
        private void SetZoom100() { ZoomLevel = 100; }
        private void FitToWindow() { }
        private void ShowOnePage() { }
        private void ShowTwoPages() { }
        private void NewWindow() { }
        private void ArrangeAllWindows() { }
        private void SplitWindow() { }
    }

    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return false;

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);
        public void Execute(object parameter) => _execute(parameter);
    }
}

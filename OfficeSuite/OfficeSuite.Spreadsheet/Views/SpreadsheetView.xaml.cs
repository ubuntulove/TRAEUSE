using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OfficeSuite.Spreadsheet.Views
{
    public class SpreadsheetView : UserControl
    {
        private SpreadsheetViewModel _viewModel;
        private Grid _mainGrid;
        private Ribbon _ribbon;
        private Grid _sheetArea;
        private StatusBar _statusBar;
        private Grid _formulaBar;

        public SpreadsheetView()
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

            _sheetArea = new Grid();
            _sheetArea.RowDefinitions.Add(new RowDefinition { Height = new GridLength(30) });
            _sheetArea.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });
            _sheetArea.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            _formulaBar = CreateFormulaBar();
            Grid.SetRow(_formulaBar, 0);

            var columnHeaders = CreateColumnHeaders();
            Grid.SetRow(columnHeaders, 1);

            var gridContainer = CreateGridContainer();
            Grid.SetRow(gridContainer, 2);

            _sheetArea.Children.Add(_formulaBar);
            _sheetArea.Children.Add(columnHeaders);
            _sheetArea.Children.Add(gridContainer);

            _statusBar = new StatusBar();
            InitializeStatusBar();

            _mainGrid.Children.Add(_ribbon);
            _mainGrid.Children.Add(_sheetArea);
            _mainGrid.Children.Add(_statusBar);

            Content = _mainGrid;
        }

        private Grid CreateFormulaBar()
        {
            var grid = new Grid { Height = 30, Background = Brushes.LightGray };

            var nameBox = new ComboBox
            {
                Width = 100,
                Margin = new Thickness(5),
                VerticalContentAlignment = VerticalAlignment.Center
            };
            nameBox.Items.Add("A1");
            nameBox.SelectedIndex = 0;

            var separator = new Rectangle
            {
                Width = 1,
                Fill = Brushes.Gray,
                Margin = new Thickness(5, 5, 5, 5)
            };

            var formulaTextBox = new TextBox
            {
                Margin = new Thickness(0, 2, 5, 2),
                VerticalContentAlignment = VerticalAlignment.Center
            };

            grid.Children.Add(nameBox);
            grid.Children.Add(separator);
            grid.Children.Add(formulaTextBox);

            return grid;
        }

        private Grid CreateColumnHeaders()
        {
            var grid = new Grid { Background = Brushes.LightGray };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });

            var headersPanel = new StackPanel { Orientation = Orientation.Horizontal };
            var columns = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T" };

            foreach (var col in columns)
            {
                var header = new Border
                {
                    Width = 75,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 1, 0),
                    Background = Brushes.LightGray,
                    Child = new TextBlock
                    {
                        Text = col,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                headersPanel.Children.Add(header);
            }

            Grid.SetColumn(headersPanel, 1);
            grid.Children.Add(headersPanel);

            return grid;
        }

        private Grid CreateGridContainer()
        {
            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(50) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var rowHeaders = CreateRowHeaders();
            Grid.SetColumn(rowHeaders, 0);

            var cellsGrid = CreateCellsGrid();
            Grid.SetColumn(cellsGrid, 1);

            grid.Children.Add(rowHeaders);
            grid.Children.Add(cellsGrid);

            return grid;
        }

        private StackPanel CreateRowHeaders()
        {
            var panel = new StackPanel { Background = Brushes.LightGray };

            for (int i = 1; i <= 50; i++)
            {
                var header = new Border
                {
                    Height = 20,
                    BorderBrush = Brushes.Gray,
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Background = Brushes.LightGray,
                    Child = new TextBlock
                    {
                        Text = i.ToString(),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    }
                };
                panel.Children.Add(header);
            }

            return panel;
        }

        private Grid CreateCellsGrid()
        {
            var grid = new Grid { Background = Brushes.White };

            var rows = 50;
            var columns = 20;

            for (int i = 0; i < rows; i++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(20) });
            }

            for (int i = 0; i < columns; i++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(75) });
            }

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < columns; col++)
                {
                    var cell = new Border
                    {
                        BorderBrush = Brushes.LightGray,
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Background = Brushes.White,
                        Child = new TextBox
                        {
                            BorderThickness = new Thickness(0),
                            Padding = new Thickness(2)
                        }
                    };
                    Grid.SetRow(cell, row);
                    Grid.SetColumn(cell, col);
                    grid.Children.Add(cell);
                }
            }

            return grid;
        }

        private void InitializeStatusBar()
        {
            var cellMode = new StatusBarItem { Content = "Ready" };
            var sumDisplay = new StatusBarItem { Content = "Sum: 0" };
            var averageDisplay = new StatusBarItem { Content = "Average: 0" };
            var countDisplay = new StatusBarItem { Content = "Count: 0" };
            var zoomDisplay = new StatusBarItem { Content = "100%" };

            _statusBar.Items.Add(cellMode);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(sumDisplay);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(averageDisplay);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(countDisplay);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(zoomDisplay);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as SpreadsheetViewModel;
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
            var formulasTab = CreateFormulasTab();
            var dataTab = CreateDataTab();
            var reviewTab = CreateReviewTab();
            var viewTab = CreateViewTab();

            _ribbon.Tabs.Add(homeTab);
            _ribbon.Tabs.Add(insertTab);
            _ribbon.Tabs.Add(formulasTab);
            _ribbon.Tabs.Add(dataTab);
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

            var alignmentGroup = CreateRibbonGroup("Alignment");
            alignmentGroup.Items.Add(CreateAlignmentControls());

            var numberGroup = CreateRibbonGroup("Number");
            numberGroup.Items.Add(CreateNumberControls());

            var stylesGroup = CreateRibbonGroup("Styles");
            stylesGroup.Items.Add(CreateRibbonButton("Format as Table", "FormatTable", _viewModel?.FormatAsTableCommand));
            stylesGroup.Items.Add(CreateRibbonButton("Conditional Formatting", "Conditional", _viewModel?.ConditionalFormattingCommand));
            stylesGroup.Items.Add(CreateRibbonButton("Cell Styles", "CellStyles", _viewModel?.CellStylesCommand));

            var cellsGroup = CreateRibbonGroup("Cells");
            cellsGroup.Items.Add(CreateRibbonButton("Insert", "Insert", _viewModel?.InsertCellCommand));
            cellsGroup.Items.Add(CreateRibbonButton("Delete", "Delete", _viewModel?.DeleteCellCommand));
            cellsGroup.Items.Add(CreateRibbonButton("Format", "Format", _viewModel?.FormatCellCommand));

            tab.Groups.Add(clipboardGroup);
            tab.Groups.Add(fontGroup);
            tab.Groups.Add(alignmentGroup);
            tab.Groups.Add(numberGroup);
            tab.Groups.Add(stylesGroup);
            tab.Groups.Add(cellsGroup);

            return tab;
        }

        private RibbonTab CreateInsertTab()
        {
            var tab = new RibbonTab { Header = "Insert" };

            var tablesGroup = CreateRibbonGroup("Tables");
            tablesGroup.Items.Add(CreateRibbonButton("Table", "Table", _viewModel?.InsertTableCommand));
            tablesGroup.Items.Add(CreateRibbonButton("PivotTable", "PivotTable", _viewModel?.InsertPivotTableCommand));

            var chartsGroup = CreateRibbonGroup("Charts");
            chartsGroup.Items.Add(CreateRibbonButton("Column", "Column", _viewModel?.InsertColumnChartCommand));
            chartsGroup.Items.Add(CreateRibbonButton("Line", "Line", _viewModel?.InsertLineChartCommand));
            chartsGroup.Items.Add(CreateRibbonButton("Pie", "Pie", _viewModel?.InsertPieChartCommand));
            chartsGroup.Items.Add(CreateRibbonButton("Bar", "Bar", _viewModel?.InsertBarChartCommand));
            chartsGroup.Items.Add(CreateRibbonButton("Area", "Area", _viewModel?.InsertAreaChartCommand));
            chartsGroup.Items.Add(CreateRibbonButton("Scatter", "Scatter", _viewModel?.InsertScatterChartCommand));
            chartsGroup.Items.Add(CreateRibbonButton("Stock", "Stock", _viewModel?.InsertStockChartCommand));
            chartsGroup.Items.Add(CreateRibbonButton("Combo", "Combo", _viewModel?.InsertComboChartCommand));

            var illustrationsGroup = CreateRibbonGroup("Illustrations");
            illustrationsGroup.Items.Add(CreateRibbonButton("Pictures", "Pictures", _viewModel?.InsertPictureCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("Shapes", "Shapes", _viewModel?.InsertShapeCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("SmartArt", "SmartArt", _viewModel?.InsertSmartArtCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("Chart", "Chart", _viewModel?.InsertChartCommand));

            var linksGroup = CreateRibbonGroup("Links");
            linksGroup.Items.Add(CreateRibbonButton("Hyperlink", "Hyperlink", _viewModel?.InsertHyperlinkCommand));

            var textGroup = CreateRibbonGroup("Text");
            textGroup.Items.Add(CreateRibbonButton("Text Box", "TextBox", _viewModel?.InsertTextBoxCommand));
            textGroup.Items.Add(CreateRibbonButton("Header & Footer", "HeaderFooter", _viewModel?.InsertHeaderFooterCommand));
            textGroup.Items.Add(CreateRibbonButton("WordArt", "WordArt", _viewModel?.InsertWordArtCommand));

            var symbolsGroup = CreateRibbonGroup("Symbols");
            symbolsGroup.Items.Add(CreateRibbonButton("Equation", "Equation", _viewModel?.InsertEquationCommand));
            symbolsGroup.Items.Add(CreateRibbonButton("Symbol", "Symbol", _viewModel?.InsertSymbolCommand));

            tab.Groups.Add(tablesGroup);
            tab.Groups.Add(chartsGroup);
            tab.Groups.Add(illustrationsGroup);
            tab.Groups.Add(linksGroup);
            tab.Groups.Add(textGroup);
            tab.Groups.Add(symbolsGroup);

            return tab;
        }

        private RibbonTab CreateFormulasTab()
        {
            var tab = new RibbonTab { Header = "Formulas" };

            var functionLibraryGroup = CreateRibbonGroup("Function Library");
            functionLibraryGroup.Items.Add(CreateRibbonButton("Insert Function", "InsertFunction", _viewModel?.InsertFunctionCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("AutoSum", "AutoSum", _viewModel?.AutoSumCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("Recently Used", "Recent", _viewModel?.RecentFunctionsCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("Financial", "Financial", _viewModel?.FinancialFunctionsCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("Logical", "Logical", _viewModel?.LogicalFunctionsCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("Text", "Text", _viewModel?.TextFunctionsCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("Date & Time", "DateTime", _viewModel?.DateTimeFunctionsCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("Lookup & Reference", "Lookup", _viewModel?.LookupFunctionsCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("Math & Trig", "Math", _viewModel?.MathFunctionsCommand));
            functionLibraryGroup.Items.Add(CreateRibbonButton("More Functions", "More", _viewModel?.MoreFunctionsCommand));

            var definedNamesGroup = CreateRibbonGroup("Defined Names");
            definedNamesGroup.Items.Add(CreateRibbonButton("Name Manager", "NameManager", _viewModel?.NameManagerCommand));
            definedNamesGroup.Items.Add(CreateRibbonButton("Define Name", "DefineName", _viewModel?.DefineNameCommand));
            definedNamesGroup.Items.Add(CreateRibbonButton("Use in Formula", "UseInFormula", _viewModel?.UseInFormulaCommand));
            definedNamesGroup.Items.Add(CreateRibbonButton("Create from Selection", "CreateFromSelection", _viewModel?.CreateFromSelectionCommand));

            var formulaAuditingGroup = CreateRibbonGroup("Formula Auditing");
            formulaAuditingGroup.Items.Add(CreateRibbonButton("Trace Precedents", "TracePrecedents", _viewModel?.TracePrecedentsCommand));
            formulaAuditingGroup.Items.Add(CreateRibbonButton("Trace Dependents", "TraceDependents", _viewModel?.TraceDependentsCommand));
            formulaAuditingGroup.Items.Add(CreateRibbonButton("Remove Arrows", "RemoveArrows", _viewModel?.RemoveArrowsCommand));
            formulaAuditingGroup.Items.Add(CreateRibbonButton("Show Formulas", "ShowFormulas", _viewModel?.ShowFormulasCommand));
            formulaAuditingGroup.Items.Add(CreateRibbonButton("Error Checking", "ErrorChecking", _viewModel?.ErrorCheckingCommand));
            formulaAuditingGroup.Items.Add(CreateRibbonButton("Evaluate Formula", "Evaluate", _viewModel?.EvaluateFormulaCommand));
            formulaAuditingGroup.Items.Add(CreateRibbonButton("Watch Window", "WatchWindow", _viewModel?.WatchWindowCommand));

            var calculationGroup = CreateRibbonGroup("Calculation");
            calculationGroup.Items.Add(CreateRibbonButton("Calculate Now", "Calculate", _viewModel?.CalculateNowCommand));
            calculationGroup.Items.Add(CreateRibbonButton("Calculation Options", "CalcOptions", _viewModel?.CalculationOptionsCommand));

            tab.Groups.Add(functionLibraryGroup);
            tab.Groups.Add(definedNamesGroup);
            tab.Groups.Add(formulaAuditingGroup);
            tab.Groups.Add(calculationGroup);

            return tab;
        }

        private RibbonTab CreateDataTab()
        {
            var tab = new RibbonTab { Header = "Data" };

            var getExternalDataGroup = CreateRibbonGroup("Get External Data");
            getExternalDataGroup.Items.Add(CreateRibbonButton("From Text", "FromText", _viewModel?.FromTextCommand));
            getExternalDataGroup.Items.Add(CreateRibbonButton("From Web", "FromWeb", _viewModel?.FromWebCommand));
            getExternalDataGroup.Items.Add(CreateRibbonButton("From Other Sources", "OtherSources", _viewModel?.OtherSourcesCommand));
            getExternalDataGroup.Items.Add(CreateRibbonButton("Existing Connections", "Connections", _viewModel?.ExistingConnectionsCommand));

            var connectionsGroup = CreateRibbonGroup("Connections");
            connectionsGroup.Items.Add(CreateRibbonButton("Refresh All", "RefreshAll", _viewModel?.RefreshAllCommand));
            connectionsGroup.Items.Add(CreateRibbonButton("Properties", "Properties", _viewModel?.ConnectionPropertiesCommand));
            connectionsGroup.Items.Add(CreateRibbonButton("Links", "Links", _viewModel?.EditLinksCommand));

            var sortAndFilterGroup = CreateRibbonGroup("Sort & Filter");
            sortAndFilterGroup.Items.Add(CreateRibbonButton("Sort", "Sort", _viewModel?.SortCommand));
            sortAndFilterGroup.Items.Add(CreateRibbonButton("Filter", "Filter", _viewModel?.FilterCommand));
            sortAndFilterGroup.Items.Add(CreateRibbonButton("Clear", "Clear", _viewModel?.ClearFilterCommand));
            sortAndFilterGroup.Items.Add(CreateRibbonButton("Reapply", "Reapply", _viewModel?.ReapplyFilterCommand));
            sortAndFilterGroup.Items.Add(CreateRibbonButton("Advanced", "Advanced", _viewModel?.AdvancedFilterCommand));

            var dataToolsGroup = CreateRibbonGroup("Data Tools");
            dataToolsGroup.Items.Add(CreateRibbonButton("What-If Analysis", "WhatIf", _viewModel?.WhatIfAnalysisCommand));
            dataToolsGroup.Items.Add(CreateRibbonButton("Consolidate", "Consolidate", _viewModel?.ConsolidateCommand));
            dataToolsGroup.Items.Add(CreateRibbonButton("Data Validation", "Validation", _viewModel?.DataValidationCommand));
            dataToolsGroup.Items.Add(CreateRibbonButton("Remove Duplicates", "RemoveDuplicates", _viewModel?.RemoveDuplicatesCommand));
            dataToolsGroup.Items.Add(CreateRibbonButton("Text to Columns", "TextToColumns", _viewModel?.TextToColumnsCommand));

            var outlineGroup = CreateRibbonGroup("Outline");
            outlineGroup.Items.Add(CreateRibbonButton("Group", "Group", _viewModel?.GroupCommand));
            outlineGroup.Items.Add(CreateRibbonButton("Ungroup", "Ungroup", _viewModel?.UngroupCommand));
            outlineGroup.Items.Add(CreateRibbonButton("Subtotal", "Subtotal", _viewModel?.SubtotalCommand));
            outlineGroup.Items.Add(CreateRibbonButton("Hide & Unhide", "Hide", _viewModel?.HideRowsColumnsCommand));

            tab.Groups.Add(getExternalDataGroup);
            tab.Groups.Add(connectionsGroup);
            tab.Groups.Add(sortAndFilterGroup);
            tab.Groups.Add(dataToolsGroup);
            tab.Groups.Add(outlineGroup);

            return tab;
        }

        private RibbonTab CreateReviewTab()
        {
            var tab = new RibbonTab { Header = "Review" };

            var proofingGroup = CreateRibbonGroup("Proofing");
            proofingGroup.Items.Add(CreateRibbonButton("Spelling", "Spelling", _viewModel?.CheckSpellingCommand));
            proofingGroup.Items.Add(CreateRibbonButton("Research", "Research", _viewModel?.DoResearchCommand));
            proofingGroup.Items.Add(CreateRibbonButton("Thesaurus", "Thesaurus", _viewModel?.ShowThesaurusCommand));

            var commentsGroup = CreateRibbonGroup("Comments");
            commentsGroup.Items.Add(CreateRibbonButton("New Comment", "NewComment", _viewModel?.NewCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Delete", "Delete", _viewModel?.DeleteCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Previous", "Previous", _viewModel?.PreviousCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Next", "Next", _viewModel?.NextCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Show Comments", "Show", _viewModel?.ToggleCommentsCommand));

            var changesGroup = CreateRibbonGroup("Changes");
            changesGroup.Items.Add(CreateRibbonButton("Track Changes", "TrackChanges", _viewModel?.ToggleTrackChangesCommand));
            changesGroup.Items.Add(CreateRibbonButton("Accept", "Accept", _viewModel?.AcceptChangeCommand));
            changesGroup.Items.Add(CreateRibbonButton("Reject", "Reject", _viewModel?.RejectChangeCommand));
            changesGroup.Items.Add(CreateRibbonButton("Accept All", "AcceptAll", _viewModel?.AcceptAllChangesCommand));
            changesGroup.Items.Add(CreateRibbonButton("Reject All", "RejectAll", _viewModel?.RejectAllChangesCommand));

            var protectGroup = CreateRibbonGroup("Protect");
            protectGroup.Items.Add(CreateRibbonButton("Protect Sheet", "ProtectSheet", _viewModel?.ProtectSheetCommand));
            protectGroup.Items.Add(CreateRibbonButton("Protect Workbook", "ProtectWorkbook", _viewModel?.ProtectWorkbookCommand));
            protectGroup.Items.Add(CreateRibbonButton("Share Workbook", "Share", _viewModel?.ShareWorkbookCommand));

            tab.Groups.Add(proofingGroup);
            tab.Groups.Add(commentsGroup);
            tab.Groups.Add(changesGroup);
            tab.Groups.Add(protectGroup);

            return tab;
        }

        private RibbonTab CreateViewTab()
        {
            var tab = new RibbonTab { Header = "View" };

            var workbookViewsGroup = CreateRibbonGroup("Workbook Views");
            workbookViewsGroup.Items.Add(CreateRibbonButton("Normal", "Normal", _viewModel?.NormalViewCommand));
            workbookViewsGroup.Items.Add(CreateRibbonButton("Page Layout", "PageLayout", _viewModel?.PageLayoutViewCommand));
            workbookViewsGroup.Items.Add(CreateRibbonButton("Page Break Preview", "PageBreak", _viewModel?.PageBreakPreviewCommand));

            var showGroup = CreateRibbonGroup("Show");
            showGroup.Items.Add(CreateRibbonButton("Formula Bar", "FormulaBar", _viewModel?.ToggleFormulaBarCommand));
            showGroup.Items.Add(CreateRibbonButton("Headers", "Headers", _viewModel?.ToggleHeadersCommand));
            showGroup.Items.Add(CreateRibbonButton("Gridlines", "Gridlines", _viewModel?.ToggleGridlinesCommand));
            showGroup.Items.Add(CreateRibbonButton("Comments", "Comments", _viewModel?.ToggleCommentsCommand));
            showGroup.Items.Add(CreateRibbonButton("Formulas", "Formulas", _viewModel?.ToggleFormulasCommand));

            var zoomGroup = CreateRibbonGroup("Zoom");
            zoomGroup.Items.Add(CreateRibbonButton("Zoom", "Zoom", _viewModel?.SetZoomCommand));
            zoomGroup.Items.Add(CreateRibbonButton("100%", "Zoom100", _viewModel?.Zoom100Command));
            zoomGroup.Items.Add(CreateRibbonButton("Fit to Window", "FitWindow", _viewModel?.FitToWindowCommand));

            var windowGroup = CreateRibbonGroup("Window");
            windowGroup.Items.Add(CreateRibbonButton("New Window", "NewWindow", _viewModel?.NewWindowCommand));
            windowGroup.Items.Add(CreateRibbonButton("Arrange All", "ArrangeAll", _viewModel?.ArrangeAllWindowsCommand));
            windowGroup.Items.Add(CreateRibbonButton("Freeze Panes", "Freeze", _viewModel?.FreezePanesCommand));
            windowGroup.Items.Add(CreateRibbonButton("Split", "Split", _viewModel?.SplitWindowCommand));
            windowGroup.Items.Add(CreateRibbonButton("Hide", "Hide", _viewModel?.HideWindowCommand));

            tab.Groups.Add(workbookViewsGroup);
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
            var strikethroughButton = new ToggleButton { Content = "S", Width = 30 };

            panel.Children.Add(fontCombo);
            panel.Children.Add(fontSizeCombo);
            panel.Children.Add(boldButton);
            panel.Children.Add(italicButton);
            panel.Children.Add(underlineButton);
            panel.Children.Add(strikethroughButton);

            return panel;
        }

        private StackPanel CreateAlignmentControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var alignLeftButton = new Button { Content = "Left", Width = 35 };
            var alignCenterButton = new Button { Content = "Center", Width = 45 };
            var alignRightButton = new Button { Content = "Right", Width = 35 };
            var alignJustifyButton = new Button { Content = "Justify", Width = 45 };

            var orientButton = new Button { Content = "Orientation", Width = 70 };
            var wrapTextButton = new ToggleButton { Content = "Wrap", Width = 40 };
            var mergeButton = new DropDownButton { Content = "Merge" };

            panel.Children.Add(alignLeftButton);
            panel.Children.Add(alignCenterButton);
            panel.Children.Add(alignRightButton);
            panel.Children.Add(alignJustifyButton);
            panel.Children.Add(orientButton);
            panel.Children.Add(wrapTextButton);
            panel.Children.Add(mergeButton);

            return panel;
        }

        private StackPanel CreateNumberControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var formatCombo = new ComboBox { Width = 100 };
            formatCombo.ItemsSource = GetNumberFormats();

            var currencyButton = new Button { Content = "$", Width = 30 };
            var percentButton = new Button { Content = "%", Width = 30 };
            var commaButton = new Button { Content = ",", Width = 30 };
            var decimalButton = new Button { Content = ".0", Width = 30 };
            var moreFormatsButton = new Button { Content = "More...", Width = 60 };

            panel.Children.Add(formatCombo);
            panel.Children.Add(currencyButton);
            panel.Children.Add(percentButton);
            panel.Children.Add(commaButton);
            panel.Children.Add(decimalButton);
            panel.Children.Add(moreFormatsButton);

            return panel;
        }

        private string[] GetFontFamilies()
        {
            return new[] { "Calibri", "Arial", "Times New Roman", "Verdana", "Tahoma", "Georgia" };
        }

        private string[] GetFontSizes()
        {
            return new[] { "8", "9", "10", "11", "12", "14", "16", "18", "20", "22", "24", "26", "28", "36", "48", "72" };
        }

        private string[] GetNumberFormats()
        {
            return new[] { "General", "Number", "Currency", "Accounting", "Date", "Time", "Percentage", "Fraction", "Scientific", "Text" };
        }
    }

    public class SpreadsheetViewModel : ViewModelBase
    {
        private string _selectedCell;
        private string _cellValue;
        private string _formula;
        private int _currentSheet;
        private int _totalSheets;
        private double _zoomLevel;
        private bool _showGridlines;
        private bool _showHeaders;
        private bool _showFormulas;

        public string SelectedCell
        {
            get => _selectedCell;
            set => SetProperty(ref _selectedCell, value);
        }

        public string CellValue
        {
            get => _cellValue;
            set => SetProperty(ref _cellValue, value);
        }

        public string Formula
        {
            get => _formula;
            set => SetProperty(ref _formula, value);
        }

        public int CurrentSheet
        {
            get => _currentSheet;
            set => SetProperty(ref _currentSheet, value);
        }

        public int TotalSheets
        {
            get => _totalSheets;
            set => SetProperty(ref _totalSheets, value);
        }

        public double ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }

        public bool ShowGridlines
        {
            get => _showGridlines;
            set => SetProperty(ref _showGridlines, value);
        }

        public bool ShowHeaders
        {
            get => _showHeaders;
            set => SetProperty(ref _showHeaders, value);
        }

        public bool ShowFormulas
        {
            get => _showFormulas;
            set => SetProperty(ref _showFormulas, value);
        }

        public ICommand PasteCommand { get; }
        public ICommand CutCommand { get; }
        public ICommand CopyCommand { get; }
        public ICommand FormatPainterCommand { get; }
        public ICommand FormatAsTableCommand { get; }
        public ICommand ConditionalFormattingCommand { get; }
        public ICommand CellStylesCommand { get; }
        public ICommand InsertCellCommand { get; }
        public ICommand DeleteCellCommand { get; }
        public ICommand FormatCellCommand { get; }
        public ICommand InsertTableCommand { get; }
        public ICommand InsertPivotTableCommand { get; }
        public ICommand InsertColumnChartCommand { get; }
        public ICommand InsertLineChartCommand { get; }
        public ICommand InsertPieChartCommand { get; }
        public ICommand InsertBarChartCommand { get; }
        public ICommand InsertAreaChartCommand { get; }
        public ICommand InsertScatterChartCommand { get; }
        public ICommand InsertStockChartCommand { get; }
        public ICommand InsertComboChartCommand { get; }
        public ICommand InsertChartCommand { get; }
        public ICommand InsertPictureCommand { get; }
        public ICommand InsertShapeCommand { get; }
        public ICommand InsertSmartArtCommand { get; }
        public ICommand InsertHyperlinkCommand { get; }
        public ICommand InsertTextBoxCommand { get; }
        public ICommand InsertHeaderFooterCommand { get; }
        public ICommand InsertWordArtCommand { get; }
        public ICommand InsertEquationCommand { get; }
        public ICommand InsertSymbolCommand { get; }
        public ICommand InsertFunctionCommand { get; }
        public ICommand AutoSumCommand { get; }
        public ICommand RecentFunctionsCommand { get; }
        public ICommand FinancialFunctionsCommand { get; }
        public ICommand LogicalFunctionsCommand { get; }
        public ICommand TextFunctionsCommand { get; }
        public ICommand DateTimeFunctionsCommand { get; }
        public ICommand LookupFunctionsCommand { get; }
        public ICommand MathFunctionsCommand { get; }
        public ICommand MoreFunctionsCommand { get; }
        public ICommand NameManagerCommand { get; }
        public ICommand DefineNameCommand { get; }
        public ICommand UseInFormulaCommand { get; }
        public ICommand CreateFromSelectionCommand { get; }
        public ICommand TracePrecedentsCommand { get; }
        public ICommand TraceDependentsCommand { get; }
        public ICommand RemoveArrowsCommand { get; }
        public ICommand ShowFormulasCommand { get; }
        public ICommand ErrorCheckingCommand { get; }
        public ICommand EvaluateFormulaCommand { get; }
        public ICommand WatchWindowCommand { get; }
        public ICommand CalculateNowCommand { get; }
        public ICommand CalculationOptionsCommand { get; }
        public ICommand FromTextCommand { get; }
        public ICommand FromWebCommand { get; }
        public ICommand OtherSourcesCommand { get; }
        public ICommand ExistingConnectionsCommand { get; }
        public ICommand RefreshAllCommand { get; }
        public ICommand ConnectionPropertiesCommand { get; }
        public ICommand EditLinksCommand { get; }
        public ICommand SortCommand { get; }
        public ICommand FilterCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand ReapplyFilterCommand { get; }
        public ICommand AdvancedFilterCommand { get; }
        public ICommand WhatIfAnalysisCommand { get; }
        public ICommand ConsolidateCommand { get; }
        public ICommand DataValidationCommand { get; }
        public ICommand RemoveDuplicatesCommand { get; }
        public ICommand TextToColumnsCommand { get; }
        public ICommand GroupCommand { get; }
        public ICommand UngroupCommand { get; }
        public ICommand SubtotalCommand { get; }
        public ICommand HideRowsColumnsCommand { get; }
        public ICommand CheckSpellingCommand { get; }
        public ICommand DoResearchCommand { get; }
        public ICommand ShowThesaurusCommand { get; }
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
        public ICommand ProtectSheetCommand { get; }
        public ICommand ProtectWorkbookCommand { get; }
        public ICommand ShareWorkbookCommand { get; }
        public ICommand NormalViewCommand { get; }
        public ICommand PageLayoutViewCommand { get; }
        public ICommand PageBreakPreviewCommand { get; }
        public ICommand ToggleFormulaBarCommand { get; }
        public ICommand ToggleHeadersCommand { get; }
        public ICommand ToggleGridlinesCommand { get; }
        public ICommand ToggleFormulasCommand { get; }
        public ICommand SetZoomCommand { get; }
        public ICommand Zoom100Command { get; }
        public ICommand FitToWindowCommand { get; }
        public ICommand NewWindowCommand { get; }
        public ICommand ArrangeAllWindowsCommand { get; }
        public ICommand FreezePanesCommand { get; }
        public ICommand SplitWindowCommand { get; }
        public ICommand HideWindowCommand { get; }

        public SpreadsheetViewModel()
        {
            PasteCommand = new RelayCommand(_ => Paste());
            CutCommand = new RelayCommand(_ => Cut());
            CopyCommand = new RelayCommand(_ => Copy());
            FormatPainterCommand = new RelayCommand(_ => FormatPainter());
            FormatAsTableCommand = new RelayCommand(_ => FormatAsTable());
            ConditionalFormattingCommand = new RelayCommand(_ => ConditionalFormatting());
            CellStylesCommand = new RelayCommand(_ => CellStyles());
            InsertCellCommand = new RelayCommand(_ => InsertCell());
            DeleteCellCommand = new RelayCommand(_ => DeleteCell());
            FormatCellCommand = new RelayCommand(_ => FormatCell());
            InsertTableCommand = new RelayCommand(_ => InsertTable());
            InsertPivotTableCommand = new RelayCommand(_ => InsertPivotTable());
            InsertColumnChartCommand = new RelayCommand(_ => InsertColumnChart());
            InsertLineChartCommand = new RelayCommand(_ => InsertLineChart());
            InsertPieChartCommand = new RelayCommand(_ => InsertPieChart());
            InsertBarChartCommand = new RelayCommand(_ => InsertBarChart());
            InsertAreaChartCommand = new RelayCommand(_ => InsertAreaChart());
            InsertScatterChartCommand = new RelayCommand(_ => InsertScatterChart());
            InsertStockChartCommand = new RelayCommand(_ => InsertStockChart());
            InsertComboChartCommand = new RelayCommand(_ => InsertComboChart());
            InsertChartCommand = new RelayCommand(_ => InsertChart());
            InsertPictureCommand = new RelayCommand(_ => InsertPicture());
            InsertShapeCommand = new RelayCommand(_ => InsertShape());
            InsertSmartArtCommand = new RelayCommand(_ => InsertSmartArt());
            InsertHyperlinkCommand = new RelayCommand(_ => InsertHyperlink());
            InsertTextBoxCommand = new RelayCommand(_ => InsertTextBox());
            InsertHeaderFooterCommand = new RelayCommand(_ => InsertHeaderFooter());
            InsertWordArtCommand = new RelayCommand(_ => InsertWordArt());
            InsertEquationCommand = new RelayCommand(_ => InsertEquation());
            InsertSymbolCommand = new RelayCommand(_ => InsertSymbol());
            InsertFunctionCommand = new RelayCommand(_ => InsertFunction());
            AutoSumCommand = new RelayCommand(_ => AutoSum());
            RecentFunctionsCommand = new RelayCommand(_ => RecentFunctions());
            FinancialFunctionsCommand = new RelayCommand(_ => FinancialFunctions());
            LogicalFunctionsCommand = new RelayCommand(_ => LogicalFunctions());
            TextFunctionsCommand = new RelayCommand(_ => TextFunctions());
            DateTimeFunctionsCommand = new RelayCommand(_ => DateTimeFunctions());
            LookupFunctionsCommand = new RelayCommand(_ => LookupFunctions());
            MathFunctionsCommand = new RelayCommand(_ => MathFunctions());
            MoreFunctionsCommand = new RelayCommand(_ => MoreFunctions());
            NameManagerCommand = new RelayCommand(_ => NameManager());
            DefineNameCommand = new RelayCommand(_ => DefineName());
            UseInFormulaCommand = new RelayCommand(_ => UseInFormula());
            CreateFromSelectionCommand = new RelayCommand(_ => CreateFromSelection());
            TracePrecedentsCommand = new RelayCommand(_ => TracePrecedents());
            TraceDependentsCommand = new RelayCommand(_ => TraceDependents());
            RemoveArrowsCommand = new RelayCommand(_ => RemoveArrows());
            ShowFormulasCommand = new RelayCommand(_ => ShowFormulas());
            ErrorCheckingCommand = new RelayCommand(_ => ErrorChecking());
            EvaluateFormulaCommand = new RelayCommand(_ => EvaluateFormula());
            WatchWindowCommand = new RelayCommand(_ => WatchWindow());
            CalculateNowCommand = new RelayCommand(_ => CalculateNow());
            CalculationOptionsCommand = new RelayCommand(_ => CalculationOptions());
            FromTextCommand = new RelayCommand(_ => FromText());
            FromWebCommand = new RelayCommand(_ => FromWeb());
            OtherSourcesCommand = new RelayCommand(_ => OtherSources());
            ExistingConnectionsCommand = new RelayCommand(_ => ExistingConnections());
            RefreshAllCommand = new RelayCommand(_ => RefreshAll());
            ConnectionPropertiesCommand = new RelayCommand(_ => ConnectionProperties());
            EditLinksCommand = new RelayCommand(_ => EditLinks());
            SortCommand = new RelayCommand(_ => Sort());
            FilterCommand = new RelayCommand(_ => Filter());
            ClearFilterCommand = new RelayCommand(_ => ClearFilter());
            ReapplyFilterCommand = new RelayCommand(_ => ReapplyFilter());
            AdvancedFilterCommand = new RelayCommand(_ => AdvancedFilter());
            WhatIfAnalysisCommand = new RelayCommand(_ => WhatIfAnalysis());
            ConsolidateCommand = new RelayCommand(_ => Consolidate());
            DataValidationCommand = new RelayCommand(_ => DataValidation());
            RemoveDuplicatesCommand = new RelayCommand(_ => RemoveDuplicates());
            TextToColumnsCommand = new RelayCommand(_ => TextToColumns());
            GroupCommand = new RelayCommand(_ => Group());
            UngroupCommand = new RelayCommand(_ => Ungroup());
            SubtotalCommand = new RelayCommand(_ => Subtotal());
            HideRowsColumnsCommand = new RelayCommand(_ => HideRowsColumns());
            CheckSpellingCommand = new RelayCommand(_ => CheckSpelling());
            DoResearchCommand = new RelayCommand(_ => DoResearch());
            ShowThesaurusCommand = new RelayCommand(_ => ShowThesaurus());
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
            ProtectSheetCommand = new RelayCommand(_ => ProtectSheet());
            ProtectWorkbookCommand = new RelayCommand(_ => ProtectWorkbook());
            ShareWorkbookCommand = new RelayCommand(_ => ShareWorkbook());
            NormalViewCommand = new RelayCommand(_ => NormalView());
            PageLayoutViewCommand = new RelayCommand(_ => PageLayoutView());
            PageBreakPreviewCommand = new RelayCommand(_ => PageBreakPreview());
            ToggleFormulaBarCommand = new RelayCommand(_ => ToggleFormulaBar());
            ToggleHeadersCommand = new RelayCommand(_ => ToggleHeaders());
            ToggleGridlinesCommand = new RelayCommand(_ => ToggleGridlines());
            ToggleFormulasCommand = new RelayCommand(_ => ToggleFormulas());
            SetZoomCommand = new RelayCommand(_ => SetZoom());
            Zoom100Command = new RelayCommand(_ => Zoom100());
            FitToWindowCommand = new RelayCommand(_ => FitToWindow());
            NewWindowCommand = new RelayCommand(_ => NewWindow());
            ArrangeAllWindowsCommand = new RelayCommand(_ => ArrangeAllWindows());
            FreezePanesCommand = new RelayCommand(_ => FreezePanes());
            SplitWindowCommand = new RelayCommand(_ => SplitWindow());
            HideWindowCommand = new RelayCommand(_ => HideWindow());

            _zoomLevel = 100;
            _showGridlines = true;
            _showHeaders = true;
            _showFormulas = false;
        }

        private void Paste() { }
        private void Cut() { }
        private void Copy() { }
        private void FormatPainter() { }
        private void FormatAsTable() { }
        private void ConditionalFormatting() { }
        private void CellStyles() { }
        private void InsertCell() { }
        private void DeleteCell() { }
        private void FormatCell() { }
        private void InsertTable() { }
        private void InsertPivotTable() { }
        private void InsertColumnChart() { }
        private void InsertLineChart() { }
        private void InsertPieChart() { }
        private void InsertBarChart() { }
        private void InsertAreaChart() { }
        private void InsertScatterChart() { }
        private void InsertStockChart() { }
        private void InsertComboChart() { }
        private void InsertChart() { }
        private void InsertPicture() { }
        private void InsertShape() { }
        private void InsertSmartArt() { }
        private void InsertHyperlink() { }
        private void InsertTextBox() { }
        private void InsertHeaderFooter() { }
        private void InsertWordArt() { }
        private void InsertEquation() { }
        private void InsertSymbol() { }
        private void InsertFunction() { }
        private void AutoSum() { }
        private void RecentFunctions() { }
        private void FinancialFunctions() { }
        private void LogicalFunctions() { }
        private void TextFunctions() { }
        private void DateTimeFunctions() { }
        private void LookupFunctions() { }
        private void MathFunctions() { }
        private void MoreFunctions() { }
        private void NameManager() { }
        private void DefineName() { }
        private void UseInFormula() { }
        private void CreateFromSelection() { }
        private void TracePrecedents() { }
        private void TraceDependents() { }
        private void RemoveArrows() { }
        private void ShowFormulas() { ShowFormulas = !ShowFormulas; }
        private void ErrorChecking() { }
        private void EvaluateFormula() { }
        private void WatchWindow() { }
        private void CalculateNow() { }
        private void CalculationOptions() { }
        private void FromText() { }
        private void FromWeb() { }
        private void OtherSources() { }
        private void ExistingConnections() { }
        private void RefreshAll() { }
        private void ConnectionProperties() { }
        private void EditLinks() { }
        private void Sort() { }
        private void Filter() { }
        private void ClearFilter() { }
        private void ReapplyFilter() { }
        private void AdvancedFilter() { }
        private void WhatIfAnalysis() { }
        private void Consolidate() { }
        private void DataValidation() { }
        private void RemoveDuplicates() { }
        private void TextToColumns() { }
        private void Group() { }
        private void Ungroup() { }
        private void Subtotal() { }
        private void HideRowsColumns() { }
        private void CheckSpelling() { }
        private void DoResearch() { }
        private void ShowThesaurus() { }
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
        private void ProtectSheet() { }
        private void ProtectWorkbook() { }
        private void ShareWorkbook() { }
        private void NormalView() { }
        private void PageLayoutView() { }
        private void PageBreakPreview() { }
        private void ToggleFormulaBar() { }
        private void ToggleHeaders() { ShowHeaders = !ShowHeaders; }
        private void ToggleGridlines() { ShowGridlines = !ShowGridlines; }
        private void ToggleFormulas() { ShowFormulas = !ShowFormulas; }
        private void SetZoom() { }
        private void Zoom100() { ZoomLevel = 100; }
        private void FitToWindow() { }
        private void NewWindow() { }
        private void ArrangeAllWindows() { }
        private void FreezePanes() { }
        private void SplitWindow() { }
        private void HideWindow() { }
    }
}

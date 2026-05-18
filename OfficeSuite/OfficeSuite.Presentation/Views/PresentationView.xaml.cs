using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OfficeSuite.Presentation.ViewModels;

namespace OfficeSuite.Presentation.Views
{
    public class PresentationView : UserControl
    {
        private PresentationViewModel _viewModel;
        private Grid _mainGrid;
        private Ribbon _ribbon;
        private Grid _contentArea;
        private StatusBar _statusBar;

        public PresentationView()
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
            _ribbon.Height = 100;

            _contentArea = new Grid();
            Grid.SetRow(_contentArea, 0);

            _statusBar = new StatusBar();
            Grid.SetRow(_statusBar, 1);
            InitializeStatusBar();

            _mainGrid.Children.Add(_ribbon);
            _mainGrid.Children.Add(_contentArea);
            _mainGrid.Children.Add(_statusBar);

            Content = _mainGrid;
        }

        private void InitializeStatusBar()
        {
            var slideIndicator = new StatusBarItem { Content = "Slide 1 of 1" };
            var zoomIndicator = new StatusBarItem { Content = "100%" };
            var presenterStatus = new StatusBarItem { Content = "Ready" };

            _statusBar.Items.Add(slideIndicator);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(zoomIndicator);
            _statusBar.Items.Add(new Separator());
            _statusBar.Items.Add(presenterStatus);
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            _viewModel = e.NewValue as PresentationViewModel;
            if (_viewModel != null)
            {
                SetupRibbon();
                SetupContentArea();
            }
        }

        private void SetupRibbon()
        {
            _ribbon.Tabs.Clear();

            var homeTab = CreateHomeTab();
            var insertTab = CreateInsertTab();
            var designTab = CreateDesignTab();
            var transitionsTab = CreateTransitionsTab();
            var animationsTab = CreateAnimationsTab();
            var slideshowTab = CreateSlideShowTab();
            var reviewTab = CreateReviewTab();
            var viewTab = CreateViewTab();

            _ribbon.Tabs.Add(homeTab);
            _ribbon.Tabs.Add(insertTab);
            _ribbon.Tabs.Add(designTab);
            _ribbon.Tabs.Add(transitionsTab);
            _ribbon.Tabs.Add(animationsTab);
            _ribbon.Tabs.Add(slideshowTab);
            _ribbon.Tabs.Add(reviewTab);
            _ribbon.Tabs.Add(viewTab);

            _ribbon.SelectedTabIndex = 0;
        }

        private RibbonTab CreateHomeTab()
        {
            var tab = new RibbonTab { Header = "Home" };

            var clipboardGroup = CreateRibbonGroup("Clipboard");
            clipboardGroup.Items.Add(CreateRibbonButton("Paste", "Paste", PasteCommand));
            clipboardGroup.Items.Add(CreateRibbonButton("Cut", "Cut", CutCommand));
            clipboardGroup.Items.Add(CreateRibbonButton("Copy", "Copy", CopyCommand));
            clipboardGroup.Items.Add(CreateRibbonButton("Select", "Select", SelectCommand));

            var slidesGroup = CreateRibbonGroup("Slides");
            slidesGroup.Items.Add(CreateRibbonButton("New Slide", "NewSlide", NewSlideCommand));
            slidesGroup.Items.Add(CreateRibbonButton("Delete", "Delete", DeleteSlideCommand));
            slidesGroup.Items.Add(CreateRibbonButton("Duplicate", "Duplicate", DuplicateSlideCommand));
            slidesGroup.Items.Add(CreateRibbonButton("Layout", "Layout", ChangeLayoutCommand));

            var fontGroup = CreateRibbonGroup("Font");
            fontGroup.Items.Add(CreateFontControls());

            var paragraphGroup = CreateRibbonGroup("Paragraph");
            paragraphGroup.Items.Add(CreateParagraphControls());

            var editingGroup = CreateRibbonGroup("Editing");
            editingGroup.Items.Add(CreateRibbonButton("Find", "Find", FindCommand));
            editingGroup.Items.Add(CreateRibbonButton("Replace", "Replace", ReplaceCommand));

            var drawingGroup = CreateRibbonGroup("Drawing");
            drawingGroup.Items.Add(CreateShapeTools());

            tab.Groups.Add(clipboardGroup);
            tab.Groups.Add(slidesGroup);
            tab.Groups.Add(fontGroup);
            tab.Groups.Add(paragraphGroup);
            tab.Groups.Add(editingGroup);
            tab.Groups.Add(drawingGroup);

            return tab;
        }

        private RibbonTab CreateInsertTab()
        {
            var tab = new RibbonTab { Header = "Insert" };

            var tablesGroup = CreateRibbonGroup("Tables");
            tablesGroup.Items.Add(CreateRibbonButton("Table", "Table", InsertTableCommand));

            var illustrationsGroup = CreateRibbonGroup("Illustrations");
            illustrationsGroup.Items.Add(CreateRibbonButton("Picture", "Picture", InsertPictureCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("Shapes", "Shapes", InsertShapeCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("SmartArt", "SmartArt", InsertSmartArtCommand));
            illustrationsGroup.Items.Add(CreateRibbonButton("Chart", "Chart", InsertChartCommand));

            var linksGroup = CreateRibbonGroup("Links");
            linksGroup.Items.Add(CreateRibbonButton("Hyperlink", "Hyperlink", InsertHyperlinkCommand));
            linksGroup.Items.Add(CreateRibbonButton("Action", "Action", InsertActionCommand));

            var commentsGroup = CreateRibbonGroup("Comments");
            commentsGroup.Items.Add(CreateRibbonButton("Comment", "Comment", InsertCommentCommand));

            var textGroup = CreateRibbonGroup("Text");
            textGroup.Items.Add(CreateRibbonButton("Text Box", "TextBox", InsertTextBoxCommand));
            textGroup.Items.Add(CreateRibbonButton("Header", "Header", InsertHeaderCommand));
            textGroup.Items.Add(CreateRibbonButton("Footer", "Footer", InsertFooterCommand));
            textGroup.Items.Add(CreateRibbonButton("Date Time", "DateTime", InsertDateTimeCommand));
            textGroup.Items.Add(CreateRibbonButton("Slide Number", "SlideNumber", InsertSlideNumberCommand));

            var mediaGroup = CreateRibbonGroup("Media");
            mediaGroup.Items.Add(CreateRibbonButton("Video", "Video", InsertVideoCommand));
            mediaGroup.Items.Add(CreateRibbonButton("Audio", "Audio", InsertAudioCommand));

            tab.Groups.Add(tablesGroup);
            tab.Groups.Add(illustrationsGroup);
            tab.Groups.Add(linksGroup);
            tab.Groups.Add(commentsGroup);
            tab.Groups.Add(textGroup);
            tab.Groups.Add(mediaGroup);

            return tab;
        }

        private RibbonTab CreateDesignTab()
        {
            var tab = new RibbonTab { Header = "Design" };

            var themesGroup = CreateRibbonGroup("Themes");
            themesGroup.Items.Add(CreateRibbonButton("Themes", "Themes", ShowThemesCommand));
            themesGroup.Items.Add(CreateRibbonButton("Colors", "Colors", CustomizeColorsCommand));
            themesGroup.Items.Add(CreateRibbonButton("Fonts", "Fonts", CustomizeFontsCommand));
            themesGroup.Items.Add(CreateRibbonButton("Effects", "Effects", CustomizeEffectsCommand));

            var variantGroup = CreateRibbonGroup("Variants");
            variantGroup.Items.Add(CreateVariantControls());

            var backgroundGroup = CreateRibbonGroup("Background");
            backgroundGroup.Items.Add(CreateRibbonButton("Background Styles", "Background", ShowBackgroundStylesCommand));
            backgroundGroup.Items.Add(CreateRibbonButton("Format Background", "FormatBG", FormatBackgroundCommand));

            tab.Groups.Add(themesGroup);
            tab.Groups.Add(variantGroup);
            tab.Groups.Add(backgroundGroup);

            return tab;
        }

        private RibbonTab CreateTransitionsTab()
        {
            var tab = new RibbonTab { Header = "Transitions" };

            var previewGroup = CreateRibbonGroup("Preview");
            previewGroup.Items.Add(CreateRibbonButton("Preview", "Preview", PreviewTransitionCommand));

            var transitionGroup = CreateRibbonGroup("Transition To This Slide");
            transitionGroup.Items.Add(CreateTransitionGallery());

            var timingGroup = CreateRibbonGroup("Timing");
            timingGroup.Items.Add(CreateRibbonButton("Sound", "Sound", SetTransitionSoundCommand));
            timingGroup.Items.Add(CreateRibbonButton("Duration", "Duration", SetTransitionDurationCommand));
            timingGroup.Items.Add(CreateRibbonButton("Apply To All", "ApplyToAll", ApplyTransitionToAllCommand));

            tab.Groups.Add(previewGroup);
            tab.Groups.Add(transitionGroup);
            tab.Groups.Add(timingGroup);

            return tab;
        }

        private RibbonTab CreateAnimationsTab()
        {
            var tab = new RibbonTab { Header = "Animations" };

            var previewGroup = CreateRibbonGroup("Preview");
            previewGroup.Items.Add(CreateRibbonButton("Preview", "Preview", PreviewAnimationCommand));

            var animationsGroup = CreateRibbonGroup("Animations");
            animationsGroup.Items.Add(CreateAnimationGallery());

            var advancedGroup = CreateRibbonGroup("Advanced Animation");
            advancedGroup.Items.Add(CreateRibbonButton("Add Animation", "AddAnimation", AddAnimationCommand));
            advancedGroup.Items.Add(CreateRibbonButton("Animation Pane", "AnimationPane", ShowAnimationPaneCommand));
            advancedGroup.Items.Add(CreateRibbonButton("Trigger", "Trigger", SetAnimationTriggerCommand));
            advancedGroup.Items.Add(CreateRibbonButton("Animation Painter", "AnimationPainter", UseAnimationPainterCommand));

            var timingGroup = CreateRibbonGroup("Timing");
            timingGroup.Items.Add(CreateRibbonButton("On Click", "OnClick", SetAnimationTimingClickCommand));
            timingGroup.Items.Add(CreateRibbonButton("With Previous", "WithPrevious", SetAnimationTimingPreviousCommand));
            timingGroup.Items.Add(CreateRibbonButton("After Previous", "AfterPrevious", SetAnimationTimingAfterCommand));
            timingGroup.Items.Add(CreateRibbonButton("Duration", "Duration", SetAnimationDurationCommand));
            timingGroup.Items.Add(CreateRibbonButton("Delay", "Delay", SetAnimationDelayCommand));

            var reorderGroup = CreateRibbonGroup("Reorder");
            reorderGroup.Items.Add(CreateRibbonButton("Move Earlier", "MoveEarlier", MoveAnimationEarlierCommand));
            reorderGroup.Items.Add(CreateRibbonButton("Move Later", "MoveLater", MoveAnimationLaterCommand));

            tab.Groups.Add(previewGroup);
            tab.Groups.Add(animationsGroup);
            tab.Groups.Add(advancedGroup);
            tab.Groups.Add(timingGroup);
            tab.Groups.Add(reorderGroup);

            return tab;
        }

        private RibbonTab CreateSlideShowTab()
        {
            var tab = new RibbonTab { Header = "Slide Show" };

            var startGroup = CreateRibbonGroup("Start Slide Show");
            startGroup.Items.Add(CreateRibbonButton("From Beginning", "FromBeginning", StartFromBeginningCommand));
            startGroup.Items.Add(CreateRibbonButton("From Current", "FromCurrent", StartFromCurrentCommand));
            startGroup.Items.Add(CreateRibbonButton("Broadcast", "Broadcast", BroadcastShowCommand));

            var setupGroup = CreateRibbonGroup("Setup");
            setupGroup.Items.Add(CreateRibbonButton("Slide Show Settings", "Settings", ShowSettingsCommand));
            setupGroup.Items.Add(CreateRibbonButton("Hide Slide", "Hide", HideSlideCommand));
            setupGroup.Items.Add(CreateRibbonButton("Rehearse Timings", "Rehearse", RehearseTimingsCommand));

            var monitorsGroup = CreateRibbonGroup("Monitors");
            monitorsGroup.Items.Add(CreateRibbonButton("Presenter View", "Presenter", ShowPresenterViewCommand));
            monitorsGroup.Items.Add(CreateRibbonButton("Presenter Display", "Display", SetPresenterDisplayCommand));

            var navigationGroup = CreateRibbonGroup("Navigation");
            navigationGroup.Items.Add(CreateRibbonButton("Next", "Next", NextSlideCommand));
            navigationGroup.Items.Add(CreateRibbonButton("Previous", "Previous", PreviousSlideCommand));
            navigationGroup.Items.Add(CreateRibbonButton("Go To Slide", "GoTo", GoToSlideCommand));

            tab.Groups.Add(startGroup);
            tab.Groups.Add(setupGroup);
            tab.Groups.Add(monitorsGroup);
            tab.Groups.Add(navigationGroup);

            return tab;
        }

        private RibbonTab CreateReviewTab()
        {
            var tab = new RibbonTab { Header = "Review" };

            var proofingGroup = CreateRibbonGroup("Proofing");
            proofingGroup.Items.Add(CreateRibbonButton("Spelling", "Spelling", CheckSpellingCommand));
            proofingGroup.Items.Add(CreateRibbonButton("Research", "Research", DoResearchCommand));
            proofingGroup.Items.Add(CreateRibbonButton("Thesaurus", "Thesaurus", ShowThesaurusCommand));

            var commentsGroup = CreateRibbonGroup("Comments");
            commentsGroup.Items.Add(CreateRibbonButton("New Comment", "NewComment", NewCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Delete", "DeleteComment", DeleteCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Previous", "PreviousComment", PreviousCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Next", "NextComment", NextCommentCommand));
            commentsGroup.Items.Add(CreateRibbonButton("Show Comments", "ShowComments", ToggleCommentsCommand));

            var compareGroup = CreateRibbonGroup("Compare");
            compareGroup.Items.Add(CreateRibbonButton("Compare", "Compare", CompareDocumentsCommand));
            compareGroup.Items.Add(CreateRibbonButton("Reject", "Reject", RejectChangeCommand));
            compareGroup.Items.Add(CreateRibbonButton("Accept", "Accept", AcceptChangeCommand));

            var protectGroup = CreateRibbonGroup("Protect");
            protectGroup.Items.Add(CreateRibbonButton("Protect", "Protect", ProtectPresentationCommand));
            protectGroup.Items.Add(CreateRibbonButton("Inspect", "Inspect", InspectPresentationCommand));

            tab.Groups.Add(proofingGroup);
            tab.Groups.Add(commentsGroup);
            tab.Groups.Add(compareGroup);
            tab.Groups.Add(protectGroup);

            return tab;
        }

        private RibbonTab CreateViewTab()
        {
            var tab = new RibbonTab { Header = "View" };

            var presentationViewsGroup = CreateRibbonGroup("Presentation Views");
            presentationViewsGroup.Items.Add(CreateRibbonButton("Normal", "Normal", ShowNormalViewCommand));
            presentationViewsGroup.Items.Add(CreateRibbonButton("Outline View", "Outline", ShowOutlineViewCommand));
            presentationViewsGroup.Items.Add(CreateRibbonButton("Slide Sorter", "Sorter", ShowSlideSorterCommand));
            presentationViewsGroup.Items.Add(CreateRibbonButton("Notes Page", "Notes", ShowNotesPageCommand));
            presentationViewsGroup.Items.Add(CreateRibbonButton("Read Mode", "ReadMode", ShowReadModeCommand));

            var masterViewsGroup = CreateRibbonGroup("Master Views");
            masterViewsGroup.Items.Add(CreateRibbonButton("Slide Master", "SlideMaster", ShowSlideMasterCommand));
            masterViewsGroup.Items.Add(CreateRibbonButton("Handout Master", "HandoutMaster", ShowHandoutMasterCommand));
            masterViewsGroup.Items.Add(CreateRibbonButton("Notes Master", "NotesMaster", ShowNotesMasterCommand));

            var showGroup = CreateRibbonGroup("Show");
            showGroup.Items.Add(CreateRibbonButton("Ruler", "Ruler", ToggleRulerCommand));
            showGroup.Items.Add(CreateRibbonButton("Gridlines", "Gridlines", ToggleGridlinesCommand));
            showGroup.Items.Add(CreateRibbonButton("Guides", "Guides", ToggleGuidesCommand));
            showGroup.Items.Add(CreateRibbonButton("Snap To Grid", "SnapToGrid", ToggleSnapToGridCommand));

            var zoomGroup = CreateRibbonGroup("Zoom");
            zoomGroup.Items.Add(CreateRibbonButton("Zoom", "Zoom", SetZoomCommand));
            zoomGroup.Items.Add(CreateRibbonButton("Fit", "Fit", FitToWindowCommand));

            var windowGroup = CreateRibbonGroup("Window");
            windowGroup.Items.Add(CreateRibbonButton("New Window", "NewWindow", NewWindowCommand));
            windowGroup.Items.Add(CreateRibbonButton("Arrange All", "ArrangeAll", ArrangeAllWindowsCommand));

            tab.Groups.Add(presentationViewsGroup);
            tab.Groups.Add(masterViewsGroup);
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

            var fontCombo = new ComboBox { Width = 150, ItemsSource = GetFontFamilies() };
            var fontSizeCombo = new ComboBox { Width = 60, ItemsSource = GetFontSizes() };

            var boldButton = new ToggleButton { Content = "B", FontWeight = FontWeights.Bold };
            var italicButton = new ToggleButton { Content = "I", FontStyle = FontStyles.Italic };
            var underlineButton = new ToggleButton { Content = "U" };

            panel.Children.Add(fontCombo);
            panel.Children.Add(fontSizeCombo);
            panel.Children.Add(boldButton);
            panel.Children.Add(italicButton);
            panel.Children.Add(underlineButton);

            return panel;
        }

        private StackPanel CreateParagraphControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var alignLeftButton = new Button { Content = "Left" };
            var alignCenterButton = new Button { Content = "Center" };
            var alignRightButton = new Button { Content = "Right" };
            var alignJustifyButton = new Button { Content = "Justify" };

            var bulletButton = new ToggleButton { Content = "Bullet" };
            var numberingButton = new ToggleButton { Content = "Numbering" };
            var decreaseIndentButton = new Button { Content = "Decrease" };
            var increaseIndentButton = new Button { Content = "Increase" };

            panel.Children.Add(alignLeftButton);
            panel.Children.Add(alignCenterButton);
            panel.Children.Add(alignRightButton);
            panel.Children.Add(alignJustifyButton);
            panel.Children.Add(bulletButton);
            panel.Children.Add(numberingButton);
            panel.Children.Add(decreaseIndentButton);
            panel.Children.Add(increaseIndentButton);

            return panel;
        }

        private StackPanel CreateShapeTools()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var shapesButton = new DropDownButton { Content = "Shapes" };
            var linesButton = new DropDownButton { Content = "Lines" };
            var connectorsButton = new DropDownButton { Content = "Connectors" };

            panel.Children.Add(shapesButton);
            panel.Children.Add(linesButton);
            panel.Children.Add(connectorsButton);

            return panel;
        }

        private StackPanel CreateVariantControls()
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };

            var variantsButton = new DropDownButton { Content = "Variants" };
            var colorsButton = new Button { Content = "Colors" };
            var fontsButton = new Button { Content = "Fonts" };
            var effectsButton = new Button { Content = "Effects" };

            panel.Children.Add(variantsButton);
            panel.Children.Add(colorsButton);
            panel.Children.Add(fontsButton);
            panel.Children.Add(effectsButton);

            return panel;
        }

        private ComboBox CreateTransitionGallery()
        {
            var gallery = new ComboBox { Width = 200 };
            gallery.ItemsSource = GetTransitions();
            return gallery;
        }

        private ComboBox CreateAnimationGallery()
        {
            var gallery = new ComboBox { Width = 200 };
            gallery.ItemsSource = GetAnimations();
            return gallery;
        }

        private void SetupContentArea()
        {
            _contentArea.Children.Clear();

            var splitView = new Grid();
            splitView.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            splitView.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            splitView.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });

            var slideNavigator = CreateSlideNavigator();
            Grid.SetColumn(slideNavigator, 0);

            var mainEditor = CreateMainEditor();
            Grid.SetColumn(mainEditor, 1);

            var notesPane = CreateNotesPane();
            Grid.SetColumn(notesPane, 2);

            splitView.Children.Add(slideNavigator);
            splitView.Children.Add(mainEditor);
            splitView.Children.Add(notesPane);

            _contentArea.Children.Add(splitView);
        }

        private ListBox CreateSlideNavigator()
        {
            var listBox = new ListBox
            {
                Name = "SlideNavigator",
                Background = System.Windows.Media.Brushes.LightGray
            };

            if (_viewModel?.Slides != null)
            {
                foreach (var slide in _viewModel.Slides)
                {
                    var item = new ListBoxItem
                    {
                        Content = slide.Name,
                        Tag = slide.Id
                    };
                    listBox.Items.Add(item);
                }
            }

            return listBox;
        }

        private Border CreateMainEditor()
        {
            return new Border
            {
                Name = "MainEditor",
                Background = System.Windows.Media.Brushes.White,
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
        }

        private Border CreateNotesPane()
        {
            return new Border
            {
                Name = "NotesPane",
                Background = System.Windows.Media.Brushes.LightYellow,
                BorderBrush = System.Windows.Media.Brushes.Gray,
                BorderThickness = new Thickness(1)
            };
        }

        private string[] GetFontFamilies()
        {
            return new[] { "Calibri", "Arial", "Times New Roman", "Verdana", "Tahoma", "Georgia" };
        }

        private double[] GetFontSizes()
        {
            return new[] { 8.0, 10.0, 12.0, 14.0, 16.0, 18.0, 20.0, 24.0, 28.0, 32.0, 36.0, 40.0, 48.0, 60.0, 72.0 };
        }

        private string[] GetTransitions()
        {
            return new[] { "None", "Fade", "Push", "Wipe", "Split", "Reveal", "Random", "Cover", "Glitter", "Zoom" };
        }

        private string[] GetAnimations()
        {
            return new[] { "None", "Appear", "Fade", "Fly In", "Float In", "Zoom In", "Wipe", "Grow & Turn", "Spin", "Bounce" };
        }

        private ICommand PasteCommand => _viewModel?.PasteCommand;
        private ICommand CutCommand => _viewModel?.CutCommand;
        private ICommand CopyCommand => _viewModel?.CopyCommand;
        private ICommand SelectCommand => _viewModel?.SelectCommand;
        private ICommand NewSlideCommand => _viewModel?.NewSlideCommand;
        private ICommand DeleteSlideCommand => _viewModel?.DeleteSlideCommand;
        private ICommand DuplicateSlideCommand => _viewModel?.DuplicateSlideCommand;
        private ICommand ChangeLayoutCommand => _viewModel?.ChangeLayoutCommand;
        private ICommand FindCommand => _viewModel?.FindCommand;
        private ICommand ReplaceCommand => _viewModel?.ReplaceCommand;
        private ICommand InsertTableCommand => _viewModel?.InsertTableCommand;
        private ICommand InsertPictureCommand => _viewModel?.InsertPictureCommand;
        private ICommand InsertShapeCommand => _viewModel?.InsertShapeCommand;
        private ICommand InsertSmartArtCommand => _viewModel?.InsertSmartArtCommand;
        private ICommand InsertChartCommand => _viewModel?.InsertChartCommand;
        private ICommand InsertHyperlinkCommand => _viewModel?.InsertHyperlinkCommand;
        private ICommand InsertActionCommand => _viewModel?.InsertActionCommand;
        private ICommand InsertCommentCommand => _viewModel?.InsertCommentCommand;
        private ICommand InsertTextBoxCommand => _viewModel?.InsertTextBoxCommand;
        private ICommand InsertHeaderCommand => _viewModel?.InsertHeaderCommand;
        private ICommand InsertFooterCommand => _viewModel?.InsertFooterCommand;
        private ICommand InsertDateTimeCommand => _viewModel?.InsertDateTimeCommand;
        private ICommand InsertSlideNumberCommand => _viewModel?.InsertSlideNumberCommand;
        private ICommand InsertVideoCommand => _viewModel?.InsertVideoCommand;
        private ICommand InsertAudioCommand => _viewModel?.InsertAudioCommand;
        private ICommand ShowThemesCommand => _viewModel?.ShowThemesCommand;
        private ICommand CustomizeColorsCommand => _viewModel?.CustomizeColorsCommand;
        private ICommand CustomizeFontsCommand => _viewModel?.CustomizeFontsCommand;
        private ICommand CustomizeEffectsCommand => _viewModel?.CustomizeEffectsCommand;
        private ICommand ShowBackgroundStylesCommand => _viewModel?.ShowBackgroundStylesCommand;
        private ICommand FormatBackgroundCommand => _viewModel?.FormatBackgroundCommand;
        private ICommand PreviewTransitionCommand => _viewModel?.PreviewTransitionCommand;
        private ICommand SetTransitionSoundCommand => _viewModel?.SetTransitionSoundCommand;
        private ICommand SetTransitionDurationCommand => _viewModel?.SetTransitionDurationCommand;
        private ICommand ApplyTransitionToAllCommand => _viewModel?.ApplyTransitionToAllCommand;
        private ICommand PreviewAnimationCommand => _viewModel?.PreviewAnimationCommand;
        private ICommand AddAnimationCommand => _viewModel?.AddAnimationCommand;
        private ICommand ShowAnimationPaneCommand => _viewModel?.ShowAnimationPaneCommand;
        private ICommand SetAnimationTriggerCommand => _viewModel?.SetAnimationTriggerCommand;
        private ICommand UseAnimationPainterCommand => _viewModel?.UseAnimationPainterCommand;
        private ICommand SetAnimationTimingClickCommand => _viewModel?.SetAnimationTimingClickCommand;
        private ICommand SetAnimationTimingPreviousCommand => _viewModel?.SetAnimationTimingPreviousCommand;
        private ICommand SetAnimationTimingAfterCommand => _viewModel?.SetAnimationTimingAfterCommand;
        private ICommand SetAnimationDurationCommand => _viewModel?.SetAnimationDurationCommand;
        private ICommand SetAnimationDelayCommand => _viewModel?.SetAnimationDelayCommand;
        private ICommand MoveAnimationEarlierCommand => _viewModel?.MoveAnimationEarlierCommand;
        private ICommand MoveAnimationLaterCommand => _viewModel?.MoveAnimationLaterCommand;
        private ICommand StartFromBeginningCommand => _viewModel?.StartFromBeginningCommand;
        private ICommand StartFromCurrentCommand => _viewModel?.StartFromCurrentCommand;
        private ICommand BroadcastShowCommand => _viewModel?.BroadcastShowCommand;
        private ICommand ShowSettingsCommand => _viewModel?.ShowSettingsCommand;
        private ICommand HideSlideCommand => _viewModel?.HideSlideCommand;
        private ICommand RehearseTimingsCommand => _viewModel?.RehearseTimingsCommand;
        private ICommand ShowPresenterViewCommand => _viewModel?.ShowPresenterViewCommand;
        private ICommand SetPresenterDisplayCommand => _viewModel?.SetPresenterDisplayCommand;
        private ICommand NextSlideCommand => _viewModel?.NextSlideCommand;
        private ICommand PreviousSlideCommand => _viewModel?.PreviousSlideCommand;
        private ICommand GoToSlideCommand => _viewModel?.GoToSlideCommand;
        private ICommand CheckSpellingCommand => _viewModel?.CheckSpellingCommand;
        private ICommand DoResearchCommand => _viewModel?.DoResearchCommand;
        private ICommand ShowThesaurusCommand => _viewModel?.ShowThesaurusCommand;
        private ICommand NewCommentCommand => _viewModel?.NewCommentCommand;
        private ICommand DeleteCommentCommand => _viewModel?.DeleteCommentCommand;
        private ICommand PreviousCommentCommand => _viewModel?.PreviousCommentCommand;
        private ICommand NextCommentCommand => _viewModel?.NextCommentCommand;
        private ICommand ToggleCommentsCommand => _viewModel?.ToggleCommentsCommand;
        private ICommand CompareDocumentsCommand => _viewModel?.CompareDocumentsCommand;
        private ICommand RejectChangeCommand => _viewModel?.RejectChangeCommand;
        private ICommand AcceptChangeCommand => _viewModel?.AcceptChangeCommand;
        private ICommand ProtectPresentationCommand => _viewModel?.ProtectPresentationCommand;
        private ICommand InspectPresentationCommand => _viewModel?.InspectPresentationCommand;
        private ICommand ShowNormalViewCommand => _viewModel?.ShowNormalViewCommand;
        private ICommand ShowOutlineViewCommand => _viewModel?.ShowOutlineViewCommand;
        private ICommand ShowSlideSorterCommand => _viewModel?.ShowSlideSorterCommand;
        private ICommand ShowNotesPageCommand => _viewModel?.ShowNotesPageCommand;
        private ICommand ShowReadModeCommand => _viewModel?.ShowReadModeCommand;
        private ICommand ShowSlideMasterCommand => _viewModel?.ShowSlideMasterCommand;
        private ICommand ShowHandoutMasterCommand => _viewModel?.ShowHandoutMasterCommand;
        private ICommand ShowNotesMasterCommand => _viewModel?.ShowNotesMasterCommand;
        private ICommand ToggleRulerCommand => _viewModel?.ToggleRulerCommand;
        private ICommand ToggleGridlinesCommand => _viewModel?.ToggleGridlinesCommand;
        private ICommand ToggleGuidesCommand => _viewModel?.ToggleGuidesCommand;
        private ICommand ToggleSnapToGridCommand => _viewModel?.ToggleSnapToGridCommand;
        private ICommand SetZoomCommand => _viewModel?.SetZoomCommand;
        private ICommand FitToWindowCommand => _viewModel?.FitToWindowCommand;
        private ICommand NewWindowCommand => _viewModel?.NewWindowCommand;
        private ICommand ArrangeAllWindowsCommand => _viewModel?.ArrangeAllWindowsCommand;
    }

    public class Ribbon : StackPanel
    {
        public RibbonTabCollection Tabs { get; } = new RibbonTabCollection();
        public int SelectedTabIndex { get; set; }
    }

    public class RibbonTab : StackPanel
    {
        public string Header { get; set; }
        public RibbonGroupCollection Groups { get; } = new RibbonGroupCollection();
    }

    public class RibbonGroup : StackPanel
    {
        public string Header { get; set; }
    }

    public class RibbonButton : Button
    {
        public string Label { get; set; }
    }

    public class RibbonTabCollection : List<RibbonTab> { }
    public class RibbonGroupCollection : List<RibbonGroup> { }

    public class DropDownButton : Button
    {
        public object ItemsSource { get; set; }
    }

    public class ToggleButton : System.Windows.Controls.Primitives.ToggleButton
    {
    }
}

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace OfficeSuite.Common.Controls
{
    public enum DialogButtonType
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel,
        RetryCancel,
        AbortRetryIgnore,
        Cancel
    }

    public enum DialogIconType
    {
        None,
        Information,
        Question,
        Warning,
        Error,
        Success,
        Security,
        Help
    }

    public class OfficeDialog : Window
    {
        public StackPanel ContentPanel { get; private set; }
        public StackPanel ButtonPanel { get; private set; }
        public Image IconImage { get; private set; }
        public TextBlock MessageText { get; private set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DialogButtonType Buttons { get; set; }
        public DialogIconType Icon { get; set; }
        public MessageBoxResult Result { get; set; }

        public OfficeDialog()
        {
            Width = 400;
            Height = 200;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = false;

            ContentPanel = new StackPanel { Margin = new Thickness(20) };
            ButtonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 10, 20, 20)
            };
            IconImage = new Image { Width = 32, Height = 32, Margin = new Thickness(0, 0, 10, 0) };
            MessageText = new TextBlock
            {
                FontSize = 14,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 0, 0, 10)
            };
        }

        public static MessageBoxResult Show(string message, string title = "", DialogButtonType buttons = DialogButtonType.Ok, DialogIconType icon = DialogIconType.Information)
        {
            var dialog = new OfficeDialog
            {
                Title = title,
                Message = message,
                Buttons = buttons,
                Icon = icon
            };
            dialog.SetupDialog();
            dialog.ShowDialog();
            return dialog.Result;
        }

        private void SetupDialog()
        {
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var contentGrid = new Grid { Margin = new Thickness(20) };
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            contentGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            if (Icon != DialogIconType.None)
            {
                IconImage.Source = GetIconImageSource(Icon);
                Grid.SetColumn(IconImage, 0);
                contentGrid.Children.Add(IconImage);
            }

            MessageText.Text = Message;
            Grid.SetColumn(MessageText, Icon != DialogIconType.None ? 1 : 0);
            if (Icon != DialogIconType.None)
                MessageText.Margin = new Thickness(10, 0, 0, 0);
            contentGrid.Children.Add(MessageText);

            Grid.SetRow(contentGrid, 0);
            mainGrid.Children.Add(contentGrid);
            Grid.SetRow(ButtonPanel, 1);
            mainGrid.Children.Add(ButtonPanel);

            Content = mainGrid;

            CreateButtons();
        }

        private void CreateButtons()
        {
            var buttonList = GetButtonsForType(Buttons);
            foreach (var btnInfo in buttonList)
            {
                var button = new OfficeButton
                {
                    Content = btnInfo.Text,
                    Width = 80,
                    Height = 28,
                    Margin = new Thickness(5, 0, 0, 0),
                    IsDefault = btnInfo.IsDefault,
                    IsCancel = btnInfo.IsCancel
                };
                button.Click += (s, e) =>
                {
                    Result = btnInfo.Result;
                    DialogResult = true;
                    Close();
                };
                ButtonPanel.Children.Add(button);
            }
        }

        private List<(string Text, MessageBoxResult Result, bool IsDefault, bool IsCancel)> GetButtonsForType(DialogButtonType type)
        {
            var buttons = new List<(string, MessageBoxResult, bool, bool)>();
            switch (type)
            {
                case DialogButtonType.Ok:
                    buttons.Add(("OK", MessageBoxResult.OK, true, false));
                    break;
                case DialogButtonType.OkCancel:
                    buttons.Add(("OK", MessageBoxResult.OK, true, false));
                    buttons.Add(("Cancel", MessageBoxResult.Cancel, false, true));
                    break;
                case DialogButtonType.YesNo:
                    buttons.Add(("Yes", MessageBoxResult.Yes, true, false));
                    buttons.Add(("No", MessageBoxResult.No, false, false));
                    break;
                case DialogButtonType.YesNoCancel:
                    buttons.Add(("Yes", MessageBoxResult.Yes, true, false));
                    buttons.Add(("No", MessageBoxResult.No, false, false));
                    buttons.Add(("Cancel", MessageBoxResult.Cancel, false, true));
                    break;
                case DialogButtonType.RetryCancel:
                    buttons.Add(("Retry", MessageBoxResult.Retry, true, false));
                    buttons.Add(("Cancel", MessageBoxResult.Cancel, false, true));
                    break;
                case DialogButtonType.AbortRetryIgnore:
                    buttons.Add(("Abort", MessageBoxResult.Abort, false, false));
                    buttons.Add(("Retry", MessageBoxResult.Retry, true, false));
                    buttons.Add(("Ignore", MessageBoxResult.Ignore, false, false));
                    break;
                case DialogButtonType.Cancel:
                    buttons.Add(("Cancel", MessageBoxResult.Cancel, true, true));
                    break;
            }
            return buttons;
        }

        private static ImageSource GetIconImageSource(DialogIconType icon)
        {
            return null;
        }
    }

    public class OfficeInputDialog : Window
    {
        public TextBlock PromptText { get; private set; }
        public OfficeTextBox InputTextBox { get; private set; }
        public StackPanel ButtonPanel { get; private set; }
        public string InputText { get; private set; }
        public bool IsConfirmed { get; private set; }

        public OfficeInputDialog()
        {
            Width = 400;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = "Input";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            PromptText = new TextBlock
            {
                Margin = new Thickness(20, 20, 20, 10),
                FontSize = 14
            };
            Grid.SetRow(PromptText, 0);
            mainGrid.Children.Add(PromptText);

            InputTextBox = new OfficeTextBox
            {
                Margin = new Thickness(20, 0, 20, 10),
                Height = 26
            };
            InputTextBox.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Enter)
                {
                    Confirm();
                    e.Handled = true;
                }
            };
            Grid.SetRow(InputTextBox, 1);
            mainGrid.Children.Add(InputTextBox);

            ButtonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 0, 20, 20)
            };
            var okButton = new OfficeButton { Content = "OK", Width = 80, Height = 28, Margin = new Thickness(5, 0, 0, 0) };
            okButton.Click += (s, e) => { Confirm(); };
            var cancelButton = new OfficeButton { Content = "Cancel", Width = 80, Height = 28, Margin = new Thickness(5, 0, 0, 0) };
            cancelButton.Click += (s, e) => { Cancel(); };
            ButtonPanel.Children.Add(okButton);
            ButtonPanel.Children.Add(cancelButton);
            Grid.SetRow(ButtonPanel, 2);
            mainGrid.Children.Add(ButtonPanel);

            Content = mainGrid;
        }

        public static string Show(string prompt, string title = "", string defaultValue = "")
        {
            var dialog = new OfficeInputDialog
            {
                Title = title,
                PromptText = { Text = prompt },
                InputTextBox = { Text = defaultValue }
            };
            dialog.InputTextBox.SelectAll();
            dialog.InputTextBox.Focus();
            dialog.ShowDialog();
            return dialog.IsConfirmed ? dialog.InputText : null;
        }

        private void Confirm()
        {
            InputText = InputTextBox.Text;
            IsConfirmed = true;
            DialogResult = true;
            Close();
        }

        private void Cancel()
        {
            InputText = null;
            IsConfirmed = false;
            DialogResult = false;
            Close();
        }
    }

    public class OfficeProgressDialog : Window
    {
        public TextBlock TitleText { get; private set; }
        public TextBlock StatusText { get; private set; }
        public ProgressBar ProgressBarControl { get; private set; }
        public TextBlock PercentageText { get; private set; }
        public OfficeButton CancelButton { get; private set; }
        public bool IsCancelled { get; private set; }
        private bool _closeRequested;

        public OfficeProgressDialog()
        {
            Width = 450;
            Height = 170;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            ShowInTaskbar = true;
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));
            IsCancelled = false;
            _closeRequested = false;

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TitleText = new TextBlock
            {
                Margin = new Thickness(20, 20, 20, 5),
                FontSize = 14,
                FontWeight = FontWeights.Bold
            };
            Grid.SetRow(TitleText, 0);
            mainGrid.Children.Add(TitleText);

            StatusText = new TextBlock
            {
                Margin = new Thickness(20, 0, 20, 10),
                FontSize = 12
            };
            Grid.SetRow(StatusText, 1);
            mainGrid.Children.Add(StatusText);

            var progressPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(20, 0, 20, 10)
            };
            ProgressBarControl = new ProgressBar { Width = 350, Height = 20 };
            PercentageText = new TextBlock
            {
                Width = 50,
                Margin = new Thickness(10, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            progressPanel.Children.Add(ProgressBarControl);
            progressPanel.Children.Add(PercentageText);
            Grid.SetRow(progressPanel, 2);
            mainGrid.Children.Add(progressPanel);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(20, 0, 20, 20)
            };
            CancelButton = new OfficeButton { Content = "Cancel", Width = 80, Height = 28 };
            CancelButton.Click += (s, e) =>
            {
                IsCancelled = true;
                Close();
            };
            buttonPanel.Children.Add(CancelButton);
            Grid.SetRow(buttonPanel, 3);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }

        public void UpdateProgress(int value, string status = null)
        {
            if (_closeRequested) return;
            ProgressBarControl.Value = value;
            PercentageText.Text = $"{value}%";
            if (status != null)
                StatusText.Text = status;
        }

        public void UpdateStatus(string status)
        {
            if (_closeRequested) return;
            StatusText.Text = status;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            _closeRequested = true;
            base.OnClosing(e);
        }
    }

    public class OfficeFindReplaceDialog : Window
    {
        public OfficeTextBox FindTextBox { get; private set; }
        public OfficeTextBox ReplaceTextBox { get; private set; }
        public OfficeButton FindNextButton { get; private set; }
        public OfficeButton ReplaceButton { get; private set; }
        public OfficeButton ReplaceAllButton { get; private set; }
        public OfficeButton CloseButton { get; private set; }
        public CheckBox MatchCaseCheckBox { get; private set; }
        public CheckBox MatchWholeWordCheckBox { get; private set; }
        public CheckBox SearchUpCheckBox { get; private set; }

        public string FindText => FindTextBox.Text;
        public string ReplaceText => ReplaceTextBox.Text;
        public bool IsMatchCase => MatchCaseCheckBox.IsChecked == true;
        public bool IsMatchWholeWord => MatchWholeWordCheckBox.IsChecked == true;
        public bool IsSearchUp => SearchUpCheckBox.IsChecked == true;

        public event EventHandler FindNext;
        public event EventHandler Replace;
        public event EventHandler ReplaceAll;

        public OfficeFindReplaceDialog(bool showReplace = true)
        {
            Width = 400;
            Height = showReplace ? 280 : 180;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = showReplace ? "Find and Replace" : "Find";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainStack = new StackPanel { Margin = new Thickness(15) };

            var findLabel = new TextBlock { Text = "Find what:", Margin = new Thickness(0, 0, 0, 5) };
            mainStack.Children.Add(findLabel);

            FindTextBox = new OfficeTextBox { Margin = new Thickness(0, 0, 0, 10), Height = 26 };
            mainStack.Children.Add(FindTextBox);

            if (showReplace)
            {
                var replaceLabel = new TextBlock { Text = "Replace with:", Margin = new Thickness(0, 0, 0, 5) };
                mainStack.Children.Add(replaceLabel);

                ReplaceTextBox = new OfficeTextBox { Margin = new Thickness(0, 0, 0, 10), Height = 26 };
                mainStack.Children.Add(ReplaceTextBox);
            }

            var optionsPanel = new WrapPanel { Margin = new Thickness(0, 0, 0, 10) };
            MatchCaseCheckBox = new CheckBox { Content = "Match case", Margin = new Thickness(0, 0, 15, 0) };
            MatchWholeWordCheckBox = new CheckBox { Content = "Match whole word", Margin = new Thickness(0, 0, 15, 0) };
            SearchUpCheckBox = new CheckBox { Content = "Search up" };
            optionsPanel.Children.Add(MatchCaseCheckBox);
            optionsPanel.Children.Add(MatchWholeWordCheckBox);
            optionsPanel.Children.Add(SearchUpCheckBox);
            mainStack.Children.Add(optionsPanel);

            var buttonPanel = new WrapPanel { Orientation = Orientation.Horizontal };
            FindNextButton = new OfficeButton { Content = "Find Next", Width = 90, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
            FindNextButton.Click += (s, e) => FindNext?.Invoke(this, EventArgs.Empty);

            if (showReplace)
            {
                ReplaceButton = new OfficeButton { Content = "Replace", Width = 90, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
                ReplaceButton.Click += (s, e) => Replace?.Invoke(this, EventArgs.Empty);
                ReplaceAllButton = new OfficeButton { Content = "Replace All", Width = 90, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
                ReplaceAllButton.Click += (s, e) => ReplaceAll?.Invoke(this, EventArgs.Empty);
                buttonPanel.Children.Add(ReplaceButton);
                buttonPanel.Children.Add(ReplaceAllButton);
            }

            CloseButton = new OfficeButton { Content = "Close", Width = 90, Height = 28 };
            CloseButton.Click += (s, e) => Close();
            buttonPanel.Children.Add(CloseButton);
            mainStack.Children.Add(buttonPanel);

            Content = mainStack;
        }
    }

    public class OfficeGoToDialog : Window
    {
        public ComboBox PageTypeComboBox { get; private set; }
        public OfficeTextBox PageNumberTextBox { get; private set; }
        public OfficeButton GoToButton { get; private set; }
        public OfficeButton CloseButton { get; private set; }

        public string SelectedPageType => PageTypeComboBox.SelectedItem?.ToString();
        public string PageNumber => PageNumberTextBox.Text;

        public event EventHandler GoTo;

        public OfficeGoToDialog()
        {
            Width = 300;
            Height = 160;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = "Go To";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid { Margin = new Thickness(15) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var typePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            var typeLabel = new TextBlock { Text = "Go to what:", Width = 80, VerticalAlignment = VerticalAlignment.Center };
            PageTypeComboBox = new ComboBox { Width = 150 };
            PageTypeComboBox.Items.Add("Page");
            PageTypeComboBox.Items.Add("Section");
            PageTypeComboBox.Items.Add("Line");
            PageTypeComboBox.Items.Add("Bookmark");
            PageTypeComboBox.SelectedIndex = 0;
            typePanel.Children.Add(typeLabel);
            typePanel.Children.Add(PageTypeComboBox);
            Grid.SetRow(typePanel, 0);
            mainGrid.Children.Add(typePanel);

            var numberPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 15) };
            var numberLabel = new TextBlock { Text = "Enter page number:", Width = 120, VerticalAlignment = VerticalAlignment.Center };
            PageNumberTextBox = new OfficeTextBox { Width = 110, Height = 26 };
            numberPanel.Children.Add(numberLabel);
            numberPanel.Children.Add(PageNumberTextBox);
            Grid.SetRow(numberPanel, 1);
            mainGrid.Children.Add(numberPanel);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            GoToButton = new OfficeButton { Content = "Go To", Width = 80, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
            GoToButton.Click += (s, e) => GoTo?.Invoke(this, EventArgs.Empty);
            CloseButton = new OfficeButton { Content = "Close", Width = 80, Height = 28 };
            CloseButton.Click += (s, e) => Close();
            buttonPanel.Children.Add(GoToButton);
            buttonPanel.Children.Add(CloseButton);
            Grid.SetRow(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }
    }

    public class OfficeFormatDialog : Window
    {
        public TabControl TabControl { get; private set; }
        public OfficeButton OkButton { get; private set; }
        public OfficeButton CancelButton { get; private set; }

        public OfficeFormatDialog(string title)
        {
            Width = 450;
            Height = 400;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = title;
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TabControl = new TabControl { Margin = new Thickness(10) };
            Grid.SetRow(TabControl, 0);
            mainGrid.Children.Add(TabControl);

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(10, 0, 10, 10)
            };
            OkButton = new OfficeButton { Content = "OK", Width = 80, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
            OkButton.Click += (s, e) => { DialogResult = true; Close(); };
            CancelButton = new OfficeButton { Content = "Cancel", Width = 80, Height = 28 };
            CancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(OkButton);
            buttonPanel.Children.Add(CancelButton);
            Grid.SetRow(buttonPanel, 1);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }
    }

    public class OfficePageSetupDialog : Window
    {
        public ComboBox PaperSizeComboBox { get; private set; }
        public ComboBox PaperSourceComboBox { get; private set; }
        public OfficeTextBox WidthTextBox { get; private set; }
        public OfficeTextBox HeightTextBox { get; private set; }
        public ComboBox OrientationComboBox { get; private set; }
        public OfficeButton OkButton { get; private set; }
        public OfficeButton CancelButton { get; private set; }

        public OfficePageSetupDialog()
        {
            Width = 400;
            Height = 350;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = "Page Setup";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid { Margin = new Thickness(15) };
            for (int i = 0; i < 6; i++)
                mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var paperSizeLabel = new TextBlock { Text = "Paper size:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(paperSizeLabel, 0);
            mainGrid.Children.Add(paperSizeLabel);

            PaperSizeComboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 10) };
            PaperSizeComboBox.Items.Add("Letter");
            PaperSizeComboBox.Items.Add("A4");
            PaperSizeComboBox.Items.Add("A3");
            PaperSizeComboBox.Items.Add("Legal");
            PaperSizeComboBox.SelectedIndex = 0;
            Grid.SetRow(PaperSizeComboBox, 1);
            mainGrid.Children.Add(PaperSizeComboBox);

            var orientationLabel = new TextBlock { Text = "Orientation:", Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(orientationLabel, 2);
            mainGrid.Children.Add(orientationLabel);

            OrientationComboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 10) };
            OrientationComboBox.Items.Add("Portrait");
            OrientationComboBox.Items.Add("Landscape");
            OrientationComboBox.SelectedIndex = 0;
            Grid.SetRow(OrientationComboBox, 3);
            mainGrid.Children.Add(OrientationComboBox);

            var sizePanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            var widthLabel = new TextBlock { Text = "Width:", Width = 60, VerticalAlignment = VerticalAlignment.Center };
            WidthTextBox = new OfficeTextBox { Width = 80, Height = 26 };
            var heightLabel = new TextBlock { Text = "Height:", Width = 60, Margin = new Thickness(20, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
            HeightTextBox = new OfficeTextBox { Width = 80, Height = 26 };
            sizePanel.Children.Add(widthLabel);
            sizePanel.Children.Add(WidthTextBox);
            sizePanel.Children.Add(heightLabel);
            sizePanel.Children.Add(HeightTextBox);
            Grid.SetRow(sizePanel, 4);
            mainGrid.Children.Add(sizePanel);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            OkButton = new OfficeButton { Content = "OK", Width = 80, Height = 28, Margin = new Thickness(0, 10, 10, 0) };
            OkButton.Click += (s, e) => { DialogResult = true; Close(); };
            CancelButton = new OfficeButton { Content = "Cancel", Width = 80, Height = 28, Margin = new Thickness(0, 10, 0, 0) };
            CancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(OkButton);
            buttonPanel.Children.Add(CancelButton);
            Grid.SetRow(buttonPanel, 5);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }
    }

    public class OfficePrintDialog : Window
    {
        public ComboBox PrinterComboBox { get; private set; }
        public ComboBox PageRangeComboBox { get; private set; }
        public OfficeTextBox FromPageTextBox { get; private set; }
        public OfficeTextBox ToPageTextBox { get; private set; }
        public OfficeTextBox CopiesTextBox { get; private set; }
        public CheckBox CollateCheckBox { get; private set; }
        public RadioButton AllPagesRadio { get; private set; }
        public RadioButton SelectionRadio { get; private set; }
        public RadioButton PagesRadio { get; private set; }
        public OfficeButton PrintButton { get; private set; }
        public OfficeButton CancelButton { get; private set; }
        public OfficeButton PropertiesButton { get; private set; }

        public string SelectedPrinter => PrinterComboBox.SelectedItem?.ToString();
        public string PageRangeType => AllPagesRadio.IsChecked == true ? "All" : (SelectionRadio.IsChecked == true ? "Selection" : "Pages");
        public int FromPage => int.TryParse(FromPageTextBox.Text, out int fp) ? fp : 1;
        public int ToPage => int.TryParse(ToPageTextBox.Text, out int tp) ? tp : 1;
        public int Copies => int.TryParse(CopiesTextBox.Text, out int c) ? c : 1;
        public bool Collate => CollateCheckBox.IsChecked == true;

        public event EventHandler Print;
        public event EventHandler ShowProperties;

        public OfficePrintDialog()
        {
            Width = 450;
            Height = 450;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = "Print";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid { Margin = new Thickness(15) };

            var printerPanel = new StackPanel { Margin = new Thickness(0, 0, 0, 15) };
            printerPanel.Children.Add(new TextBlock { Text = "Select printer:", Margin = new Thickness(0, 0, 0, 5) });
            PrinterComboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 5) };
            PrinterComboBox.Items.Add("Microsoft Print to PDF");
            PrinterComboBox.Items.Add("Microsoft XPS Document Writer");
            PrinterComboBox.SelectedIndex = 0;
            PropertiesButton = new OfficeButton { Content = "Properties...", Width = 100, Height = 28, HorizontalAlignment = HorizontalAlignment.Left };
            PropertiesButton.Click += (s, e) => ShowProperties?.Invoke(this, EventArgs.Empty);
            printerPanel.Children.Add(PrinterComboBox);
            printerPanel.Children.Add(PropertiesButton);
            mainGrid.Children.Add(printerPanel);

            var pageRangeGroup = new GroupBox { Header = "Page range", Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            var pageRangeStack = new StackPanel();
            AllPagesRadio = new RadioButton { Content = "All pages", IsChecked = true, Margin = new Thickness(0, 0, 0, 5) };
            SelectionRadio = new RadioButton { Content = "Selection", Margin = new Thickness(0, 0, 0, 5) };
            PagesRadio = new RadioButton { Content = "Pages:", Margin = new Thickness(0, 0, 0, 5) };
            var pagesPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(20, 0, 0, 0) };
            pagesPanel.Children.Add(new TextBlock { Text = "From:", VerticalAlignment = VerticalAlignment.Center });
            FromPageTextBox = new OfficeTextBox { Width = 60, Height = 26, Margin = new Thickness(5, 0, 15, 0) };
            pagesPanel.Children.Add(FromPageTextBox);
            pagesPanel.Children.Add(new TextBlock { Text = "To:", VerticalAlignment = VerticalAlignment.Center });
            ToPageTextBox = new OfficeTextBox { Width = 60, Height = 26, Margin = new Thickness(5, 0, 0, 0) };
            pagesPanel.Children.Add(ToPageTextBox);
            pageRangeStack.Children.Add(AllPagesRadio);
            pageRangeStack.Children.Add(SelectionRadio);
            pageRangeStack.Children.Add(PagesRadio);
            pageRangeStack.Children.Add(pagesPanel);
            pageRangeGroup.Content = pageRangeStack;
            mainGrid.Children.Add(pageRangeGroup);

            var copiesGroup = new GroupBox { Header = "Copies", Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            var copiesStack = new StackPanel();
            var copiesPanel = new StackPanel { Orientation = Orientation.Horizontal };
            copiesPanel.Children.Add(new TextBlock { Text = "Number of copies:", VerticalAlignment = VerticalAlignment.Center });
            CopiesTextBox = new OfficeTextBox { Width = 60, Height = 26, Margin = new Thickness(10, 0, 0, 0), Text = "1" };
            copiesPanel.Children.Add(CopiesTextBox);
            CollateCheckBox = new CheckBox { Content = "Collate", IsChecked = true, Margin = new Thickness(0, 5, 0, 0) };
            copiesStack.Children.Add(copiesPanel);
            copiesStack.Children.Add(CollateCheckBox);
            copiesGroup.Content = copiesStack;
            mainGrid.Children.Add(copiesGroup);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            PrintButton = new OfficeButton { Content = "Print", Width = 80, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
            PrintButton.Click += (s, e) => { Print?.Invoke(this, EventArgs.Empty); Close(); };
            CancelButton = new OfficeButton { Content = "Cancel", Width = 80, Height = 28 };
            CancelButton.Click += (s, e) => Close();
            buttonPanel.Children.Add(PrintButton);
            buttonPanel.Children.Add(CancelButton);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }
    }

    public class OfficeAboutDialog : Window
    {
        public OfficeAboutDialog(string productName, string version, string description)
        {
            Width = 450;
            Height = 350;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = $"About {productName}";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid { Margin = new Thickness(20) };
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var iconPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center };
            var iconBorder = new Border
            {
                Width = 80,
                Height = 80,
                Background = new SolidColorBrush(Color.FromRgb(0, 102, 204)),
                CornerRadius = new CornerRadius(10)
            };
            iconBorder.Child = new TextBlock
            {
                Text = "OFFICE",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconPanel.Children.Add(iconBorder);
            Grid.SetRow(iconPanel, 0);
            mainGrid.Children.Add(iconPanel);

            var titlePanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, Margin = new Thickness(0, 15, 0, 10) };
            titlePanel.Children.Add(new TextBlock
            {
                Text = productName,
                FontSize = 24,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center
            });
            titlePanel.Children.Add(new TextBlock
            {
                Text = $"Version {version}",
                FontSize = 14,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(0, 5, 0, 0)
            });
            Grid.SetRow(titlePanel, 1);
            mainGrid.Children.Add(titlePanel);

            var descriptionText = new TextBlock
            {
                Text = description,
                TextWrapping = TextWrapping.Wrap,
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(0, 0, 0, 20)
            };
            Grid.SetRow(descriptionText, 2);
            mainGrid.Children.Add(descriptionText);

            var okButton = new OfficeButton
            {
                Content = "OK",
                Width = 80,
                Height = 28,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            okButton.Click += (s, e) => Close();
            Grid.SetRow(okButton, 3);
            mainGrid.Children.Add(okButton);

            Content = mainGrid;
        }
    }

    public class OfficeTableDialog : Window
    {
        public OfficeTextBox RowsTextBox { get; private set; }
        public OfficeTextBox ColumnsTextBox { get; private set; }
        public ComboBox TableStyleComboBox { get; private set; }
        public CheckBox HeaderRowCheckBox { get; private set; }
        public CheckBox TotalRowCheckBox { get; private set; }
        public CheckBox BandedRowsCheckBox { get; private set; }
        public CheckBox FirstColumnCheckBox { get; private set; }
        public CheckBox LastColumnCheckBox { get; private set; }
        public CheckBox BandedColumnsCheckBox { get; private set; }
        public OfficeButton OkButton { get; private set; }
        public OfficeButton CancelButton { get; private set; }

        public int Rows => int.TryParse(RowsTextBox.Text, out int r) ? r : 3;
        public int Columns => int.TryParse(ColumnsTextBox.Text, out int c) ? c : 3;
        public bool IncludeHeaderRow => HeaderRowCheckBox.IsChecked == true;
        public bool IncludeTotalRow => TotalRowCheckBox.IsChecked == true;
        public bool IncludeBandedRows => BandedRowsCheckBox.IsChecked == true;
        public bool IncludeFirstColumn => FirstColumnCheckBox.IsChecked == true;
        public bool IncludeLastColumn => LastColumnCheckBox.IsChecked == true;
        public bool IncludeBandedColumns => BandedColumnsCheckBox.IsChecked == true;

        public OfficeTableDialog()
        {
            Width = 400;
            Height = 380;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = "Insert Table";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid { Margin = new Thickness(15) };

            var sizeGroup = new GroupBox { Header = "Table size", Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            var sizeStack = new StackPanel();
            var rowsPanel = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 0, 0, 10) };
            rowsPanel.Children.Add(new TextBlock { Text = "Number of rows:", Width = 120, VerticalAlignment = VerticalAlignment.Center });
            RowsTextBox = new OfficeTextBox { Width = 80, Height = 26, Text = "3" };
            rowsPanel.Children.Add(RowsTextBox);
            sizeStack.Children.Add(rowsPanel);
            var colsPanel = new StackPanel { Orientation = Orientation.Horizontal };
            colsPanel.Children.Add(new TextBlock { Text = "Number of columns:", Width = 120, VerticalAlignment = VerticalAlignment.Center });
            ColumnsTextBox = new OfficeTextBox { Width = 80, Height = 26, Text = "3" };
            colsPanel.Children.Add(ColumnsTextBox);
            sizeStack.Children.Add(colsPanel);
            sizeGroup.Content = sizeStack;
            mainGrid.Children.Add(sizeGroup);

            var styleGroup = new GroupBox { Header = "AutoFit behavior", Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            var styleStack = new StackPanel();
            TableStyleComboBox = new ComboBox { Margin = new Thickness(0, 0, 0, 5) };
            TableStyleComboBox.Items.Add("Fixed column width");
            TableStyleComboBox.Items.Add("AutoFit to contents");
            TableStyleComboBox.Items.Add("AutoFit to window");
            TableStyleComboBox.SelectedIndex = 0;
            styleStack.Children.Add(TableStyleComboBox);
            styleGroup.Content = styleStack;
            mainGrid.Children.Add(styleGroup);

            var optionsGroup = new GroupBox { Header = "Table style options", Margin = new Thickness(0, 0, 0, 15), Padding = new Thickness(10) };
            var optionsStack = new StackPanel();
            HeaderRowCheckBox = new CheckBox { Content = "Header row", IsChecked = true, Margin = new Thickness(0, 0, 0, 5) };
            TotalRowCheckBox = new CheckBox { Content = "Total row", Margin = new Thickness(0, 0, 0, 5) };
            BandedRowsCheckBox = new CheckBox { Content = "Banded rows", IsChecked = true, Margin = new Thickness(0, 0, 0, 5) };
            FirstColumnCheckBox = new CheckBox { Content = "First column", Margin = new Thickness(0, 0, 0, 5) };
            LastColumnCheckBox = new CheckBox { Content = "Last column", Margin = new Thickness(0, 0, 0, 5) };
            BandedColumnsCheckBox = new CheckBox { Content = "Banded columns", Margin = new Thickness(0, 0, 0, 5) };
            optionsStack.Children.Add(HeaderRowCheckBox);
            optionsStack.Children.Add(TotalRowCheckBox);
            optionsStack.Children.Add(BandedRowsCheckBox);
            optionsStack.Children.Add(FirstColumnCheckBox);
            optionsStack.Children.Add(LastColumnCheckBox);
            optionsStack.Children.Add(BandedColumnsCheckBox);
            optionsGroup.Content = optionsStack;
            mainGrid.Children.Add(optionsGroup);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            OkButton = new OfficeButton { Content = "OK", Width = 80, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
            OkButton.Click += (s, e) => { DialogResult = true; Close(); };
            CancelButton = new OfficeButton { Content = "Cancel", Width = 80, Height = 28 };
            CancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(OkButton);
            buttonPanel.Children.Add(CancelButton);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }
    }

    public class OfficeChartDialog : Window
    {
        public ListBox ChartTypeListBox { get; private set; }
        public ListBox SubtypeListBox { get; private set; }
        public Image PreviewImage { get; private set; }
        public OfficeButton OkButton { get; private set; }
        public OfficeButton CancelButton { get; private set; }
        public OfficeButton NextButton { get; private set; }

        public string SelectedChartType { get; private set; }
        public string SelectedSubtype { get; private set; }

        public OfficeChartDialog()
        {
            Width = 600;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = "Insert Chart";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid { Margin = new Thickness(15) };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var leftPanel = new StackPanel();
            ChartTypeListBox = new ListBox { Height = 350, Margin = new Thickness(0, 0, 10, 10) };
            ChartTypeListBox.Items.Add("Column");
            ChartTypeListBox.Items.Add("Bar");
            ChartTypeListBox.Items.Add("Line");
            ChartTypeListBox.Items.Add("Pie");
            ChartTypeListBox.Items.Add("Area");
            ChartTypeListBox.Items.Add("Scatter");
            ChartTypeListBox.Items.Add("Stock");
            ChartTypeListBox.Items.Add("Surface");
            ChartTypeListBox.Items.Add("Doughnut");
            ChartTypeListBox.Items.Add("Radar");
            ChartTypeListBox.SelectedIndex = 0;
            ChartTypeListBox.SelectionChanged += (s, e) => UpdateSubtypes();
            leftPanel.Children.Add(ChartTypeListBox);

            SubtypeListBox = new ListBox { Height = 150 };
            leftPanel.Children.Add(SubtypeListBox);
            Grid.SetColumn(leftPanel, 0);
            mainGrid.Children.Add(leftPanel);

            var rightPanel = new StackPanel { Margin = new Thickness(10, 0, 0, 0) };
            rightPanel.Children.Add(new TextBlock { Text = "Preview:", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) });
            PreviewImage = new Image { Height = 280, HorizontalAlignment = HorizontalAlignment.Center, Background = Brushes.LightGray };
            rightPanel.Children.Add(PreviewImage);
            Grid.SetColumn(rightPanel, 1);
            mainGrid.Children.Add(rightPanel);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 15, 0, 0) };
            OkButton = new OfficeButton { Content = "OK", Width = 80, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
            OkButton.Click += (s, e) =>
            {
                SelectedChartType = ChartTypeListBox.SelectedItem?.ToString();
                SelectedSubtype = SubtypeListBox.SelectedItem?.ToString();
                DialogResult = true;
                Close();
            };
            CancelButton = new OfficeButton { Content = "Cancel", Width = 80, Height = 28 };
            CancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(OkButton);
            buttonPanel.Children.Add(CancelButton);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
            UpdateSubtypes();
        }

        private void UpdateSubtypes()
        {
            SubtypeListBox.Items.Clear();
            switch (ChartTypeListBox.SelectedIndex)
            {
                case 0:
                    SubtypeListBox.Items.Add("Clustered Column");
                    SubtypeListBox.Items.Add("Stacked Column");
                    SubtypeListBox.Items.Add("100% Stacked Column");
                    SubtypeListBox.Items.Add("3D Clustered Column");
                    SubtypeListBox.Items.Add("3D Stacked Column");
                    SubtypeListBox.Items.Add("3D 100% Stacked Column");
                    break;
                case 1:
                    SubtypeListBox.Items.Add("Clustered Bar");
                    SubtypeListBox.Items.Add("Stacked Bar");
                    SubtypeListBox.Items.Add("100% Stacked Bar");
                    break;
                case 2:
                    SubtypeListBox.Items.Add("Line");
                    SubtypeListBox.Items.Add("Stacked Line");
                    SubtypeListBox.Items.Add("100% Stacked Line");
                    SubtypeListBox.Items.Add("Line with Markers");
                    break;
                case 3:
                    SubtypeListBox.Items.Add("Pie");
                    SubtypeListBox.Items.Add("Exploded Pie");
                    SubtypeListBox.Items.Add("3D Pie");
                    SubtypeListBox.Items.Add("3D Exploded Pie");
                    break;
                case 4:
                    SubtypeListBox.Items.Add("Area");
                    SubtypeListBox.Items.Add("Stacked Area");
                    SubtypeListBox.Items.Add("100% Stacked Area");
                    break;
                case 5:
                    SubtypeListBox.Items.Add("Scatter with Markers");
                    SubtypeListBox.Items.Add("Scatter with Lines");
                    SubtypeListBox.Items.Add("Scatter with Lines and Markers");
                    break;
                default:
                    SubtypeListBox.Items.Add("Default");
                    break;
            }
            SubtypeListBox.SelectedIndex = 0;
        }
    }

    public class OfficeOptionsDialog : Window
    {
        public TreeView CategoryTreeView { get; private set; }
        public Frame ContentFrame { get; private set; }
        public OfficeButton OkButton { get; private set; }
        public OfficeButton CancelButton { get; private set; }
        public OfficeButton ApplyButton { get; private set; }

        public OfficeOptionsDialog()
        {
            Width = 700;
            Height = 500;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            Title = "Options";
            Background = new SolidColorBrush(Color.FromRgb(255, 255, 255));

            var mainGrid = new Grid { Margin = new Thickness(10) };
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(200) });
            mainGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            CategoryTreeView = new TreeView { Margin = new Thickness(0, 0, 10, 0) };
            var generalNode = new TreeViewItem { Header = "General" };
            generalNode.Items.Add(new TreeViewItem { Header = "Options" });
            var editingNode = new TreeViewItem { Header = "Editing" };
            editingNode.Items.Add(new TreeViewItem { Header = "Advanced" });
            var saveNode = new TreeViewItem { Header = "Save" };
            var displayNode = new TreeViewItem { Header = "Display" };
            var proofingNode = new TreeViewItem { Header = "Proofing" };
            CategoryTreeView.Items.Add(generalNode);
            CategoryTreeView.Items.Add(editingNode);
            CategoryTreeView.Items.Add(saveNode);
            CategoryTreeView.Items.Add(displayNode);
            CategoryTreeView.Items.Add(proofingNode);
            Grid.SetColumn(CategoryTreeView, 0);
            Grid.SetRow(CategoryTreeView, 0);
            mainGrid.Children.Add(CategoryTreeView);

            ContentFrame = new Frame { Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetColumn(ContentFrame, 1);
            Grid.SetRow(ContentFrame, 0);
            mainGrid.Children.Add(ContentFrame);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            OkButton = new OfficeButton { Content = "OK", Width = 80, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
            OkButton.Click += (s, e) => { DialogResult = true; Close(); };
            ApplyButton = new OfficeButton { Content = "Apply", Width = 80, Height = 28, Margin = new Thickness(0, 0, 10, 0) };
            CancelButton = new OfficeButton { Content = "Cancel", Width = 80, Height = 28 };
            CancelButton.Click += (s, e) => { DialogResult = false; Close(); };
            buttonPanel.Children.Add(OkButton);
            buttonPanel.Children.Add(ApplyButton);
            buttonPanel.Children.Add(CancelButton);
            Grid.SetColumn(buttonPanel, 0);
            Grid.SetRow(buttonPanel, 1);
            Grid.SetColumnSpan(buttonPanel, 2);
            mainGrid.Children.Add(buttonPanel);

            Content = mainGrid;
        }
    }
}

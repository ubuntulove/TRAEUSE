using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace OfficeSuite.Common.Controls
{
    public class OfficeButton : Button
    {
        public static readonly DependencyProperty OfficeIconProperty =
            DependencyProperty.Register("OfficeIcon", typeof(string), typeof(OfficeButton), new PropertyMetadata(null));

        public string OfficeIcon
        {
            get => (string)GetValue(OfficeIconProperty);
            set => SetValue(OfficeIconProperty, value);
        }

        public static readonly DependencyProperty OfficeSizeProperty =
            DependencyProperty.Register("OfficeSize", typeof(double), typeof(OfficeButton), new PropertyMetadata(16.0));

        public double OfficeSize
        {
            get => (double)GetValue(OfficeSizeProperty);
            set => SetValue(OfficeSizeProperty, value);
        }

        public static readonly DependencyProperty OfficeStyleProperty =
            DependencyProperty.Register("OfficeStyle", typeof(OfficeButtonStyle), typeof(OfficeButton), new PropertyMetadata(OfficeButtonStyle.Normal));

        public OfficeButtonStyle OfficeStyle
        {
            get => (OfficeButtonStyle)GetValue(OfficeStyleProperty);
            set => SetValue(OfficeStyleProperty, value);
        }

        static OfficeButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OfficeButton), new FrameworkPropertyMetadata(typeof(OfficeButton)));
        }
    }

    public enum OfficeButtonStyle
    {
        Normal,
        Large,
        Small,
        Toggle
    }

    public class OfficeToggleButton : ToggleButton
    {
        public static readonly DependencyProperty OfficeIconProperty =
            DependencyProperty.Register("OfficeIcon", typeof(string), typeof(OfficeToggleButton), new PropertyMetadata(null));

        public string OfficeIcon
        {
            get => (string)GetValue(OfficeIconProperty);
            set => SetValue(OfficeIconProperty, value);
        }

        public static readonly DependencyProperty OfficeSizeProperty =
            DependencyProperty.Register("OfficeSize", typeof(double), typeof(OfficeToggleButton), new PropertyMetadata(16.0));

        public double OfficeSize
        {
            get => (double)GetValue(OfficeSizeProperty);
            set => SetValue(OfficeSizeProperty, value);
        }

        static OfficeToggleButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OfficeToggleButton), new FrameworkPropertyMetadata(typeof(OfficeToggleButton)));
        }
    }

    public class OfficeTextBox : TextBox
    {
        public static readonly DependencyProperty PlaceholderProperty =
            DependencyProperty.Register("Placeholder", typeof(string), typeof(OfficeTextBox), new PropertyMetadata(string.Empty));

        public string Placeholder
        {
            get => (string)GetValue(PlaceholderProperty);
            set => SetValue(PlaceholderProperty, value);
        }

        public static readonly DependencyProperty HasErrorProperty =
            DependencyProperty.Register("HasError", typeof(bool), typeof(OfficeTextBox), new PropertyMetadata(false));

        public bool HasError
        {
            get => (bool)GetValue(HasErrorProperty);
            set => SetValue(HasErrorProperty, value);
        }

        public static readonly DependencyProperty ErrorMessageProperty =
            DependencyProperty.Register("ErrorMessage", typeof(string), typeof(OfficeTextBox), new PropertyMetadata(string.Empty));

        public string ErrorMessage
        {
            get => (string)GetValue(ErrorMessageProperty);
            set => SetValue(ErrorMessageProperty, value);
        }
    }

    public class OfficeComboBox : ComboBox
    {
        public static readonly DependencyProperty OfficeModeProperty =
            DependencyProperty.Register("OfficeMode", typeof(ComboBoxMode), typeof(OfficeComboBox), new PropertyMetadata(ComboBoxMode.Normal));

        public ComboBoxMode OfficeMode
        {
            get => (ComboBoxMode)GetValue(OfficeModeProperty);
            set => SetValue(OfficeModeProperty, value);
        }

        static OfficeComboBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OfficeComboBox), new FrameworkPropertyMetadata(typeof(OfficeComboBox)));
        }
    }

    public enum ComboBoxMode
    {
        Normal,
        DropDown,
        DropDownList
    }

    public class OfficeProgressBar : ProgressBar
    {
        public static readonly DependencyProperty OfficeStateProperty =
            DependencyProperty.Register("OfficeState", typeof(ProgressState), typeof(OfficeProgressBar), new PropertyMetadata(ProgressState.Normal));

        public ProgressState OfficeState
        {
            get => (ProgressState)GetValue(OfficeStateProperty);
            set => SetValue(OfficeStateProperty, value);
        }

        static OfficeProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OfficeProgressBar), new FrameworkPropertyMetadata(typeof(OfficeProgressBar)));
        }
    }

    public enum ProgressState
    {
        Normal,
        Paused,
        Error,
        Indeterminate
    }

    public class OfficeSlider : Slider
    {
        public static readonly DependencyProperty OfficeTrackProperty =
            DependencyProperty.Register("OfficeTrack", typeof(bool), typeof(OfficeSlider), new PropertyMetadata(true));

        public bool OfficeTrack
        {
            get => (bool)GetValue(OfficeTrackProperty);
            set => SetValue(OfficeTrackProperty, value);
        }

        static OfficeSlider()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OfficeSlider), new FrameworkPropertyMetadata(typeof(OfficeSlider)));
        }
    }

    public class RibbonButton : Button
    {
        public static readonly DependencyProperty IsDropDownProperty =
            DependencyProperty.Register("IsDropDown", typeof(bool), typeof(RibbonButton), new PropertyMetadata(false));

        public bool IsDropDown
        {
            get => (bool)GetValue(IsDropDownProperty);
            set => SetValue(IsDropDownProperty, value);
        }

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource", typeof(string), typeof(RibbonButton), new PropertyMetadata(null));

        public string ImageSource
        {
            get => (string)GetValue(ImageSourceProperty);
            set => SetValue(ImageSourceProperty, value);
        }

        public static readonly DependencyProperty LargeImageProperty =
            DependencyProperty.Register("LargeImage", typeof(string), typeof(RibbonButton), new PropertyMetadata(null));

        public string LargeImage
        {
            get => (string)GetValue(LargeImageProperty);
            set => SetValue(LargeImageProperty, value);
        }

        public static readonly DependencyProperty ToolTipDescriptionProperty =
            DependencyProperty.Register("ToolTipDescription", typeof(string), typeof(RibbonButton), new PropertyMetadata(string.Empty));

        public string ToolTipDescription
        {
            get => (string)GetValue(ToolTipDescriptionProperty);
            set => SetValue(ToolTipDescriptionProperty, value);
        }

        public static readonly DependencyProperty KeyTipProperty =
            DependencyProperty.Register("KeyTip", typeof(string), typeof(RibbonButton), new PropertyMetadata(string.Empty));

        public string KeyTip
        {
            get => (string)GetValue(KeyTipProperty);
            set => SetValue(KeyTipProperty, value);
        }
    }

    public class RibbonDropDown : ComboBox
    {
        public static readonly DependencyProperty OfficeItemsProperty =
            DependencyProperty.Register("OfficeItems", typeof(RibbonItemCollection), typeof(RibbonDropDown),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public RibbonItemCollection OfficeItems
        {
            get => (RibbonItemCollection)GetValue(OfficeItemsProperty);
            set => SetValue(OfficeItemsProperty, value);
        }

        public static readonly DependencyProperty OfficeImageProperty =
            DependencyProperty.Register("OfficeImage", typeof(string), typeof(RibbonDropDown), new PropertyMetadata(null));

        public string OfficeImage
        {
            get => (string)GetValue(OfficeImageProperty);
            set => SetValue(OfficeImageProperty, value);
        }

        static RibbonDropDown()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RibbonDropDown), new FrameworkPropertyMetadata(typeof(RibbonDropDown)));
        }
    }

    public class RibbonItemCollection : System.Collections.ObjectModel.ObservableCollection<RibbonItem> { }

    public class RibbonItem : DependencyObject
    {
        public string Text { get; set; }
        public string Image { get; set; }
        public bool IsSeparator { get; set; }
    }

    public class RibbonGroupBox : GroupBox
    {
        public static readonly DependencyProperty IsCollapsedProperty =
            DependencyProperty.Register("IsCollapsed", typeof(bool), typeof(RibbonGroupBox), new PropertyMetadata(false));

        public bool IsCollapsed
        {
            get => (bool)GetValue(IsCollapsedProperty);
            set => SetValue(IsCollapsedProperty, value);
        }

        public static readonly DependencyProperty HasDialogLauncherProperty =
            DependencyProperty.Register("HasDialogLauncher", typeof(bool), typeof(RibbonGroupBox), new PropertyMetadata(false));

        public bool HasDialogLauncher
        {
            get => (bool)GetValue(HasDialogLauncherProperty);
            set => SetValue(HasDialogLauncherProperty, value);
        }
    }

    public class StatusControl : ContentControl
    {
        public static readonly DependencyProperty StatusTextProperty =
            DependencyProperty.Register("StatusText", typeof(string), typeof(StatusControl), new PropertyMetadata(string.Empty));

        public string StatusText
        {
            get => (string)GetValue(StatusTextProperty);
            set => SetValue(StatusTextProperty, value);
        }

        public static readonly DependencyProperty StatusTypeProperty =
            DependencyProperty.Register("StatusType", typeof(StatusType), typeof(StatusControl), new PropertyMetadata(StatusType.Text));

        public StatusType StatusType
        {
            get => (StatusType)GetValue(StatusTypeProperty);
            set => SetValue(StatusTypeProperty, value);
        }
    }

    public enum StatusType
    {
        Text,
        Progress,
        Indicator,
        Language
    }

    public class OfficeTabControl : TabControl
    {
        public static readonly DependencyProperty OfficeStyleProperty =
            DependencyProperty.Register("OfficeStyle", typeof(TabControlStyle), typeof(OfficeTabControl),
                new FrameworkPropertyMetadata(TabControlStyle.Normal, FrameworkPropertyMetadataOptions.AffectsRender));

        public TabControlStyle OfficeStyle
        {
            get => (TabControlStyle)GetValue(OfficeStyleProperty);
            set => SetValue(OfficeStyleProperty, value);
        }

        static OfficeTabControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(OfficeTabControl), new FrameworkPropertyMetadata(typeof(OfficeTabControl)));
        }
    }

    public enum TabControlStyle
    {
        Normal,
        Frame,
        None
    }

    public class OfficeTabItem : TabItem
    {
        public static readonly DependencyProperty OfficeImageProperty =
            DependencyProperty.Register("OfficeImage", typeof(string), typeof(OfficeTabItem), new PropertyMetadata(null));

        public string OfficeImage
        {
            get => (string)GetValue(OfficeImageProperty);
            set => SetValue(OfficeImageProperty, value);
        }

        public static readonly DependencyProperty OfficeBadgeProperty =
            DependencyProperty.Register("OfficeBadge", typeof(string), typeof(OfficeTabItem), new PropertyMetadata(null));

        public string OfficeBadge
        {
            get => (string)GetValue(OfficeBadgeProperty);
            set => SetValue(OfficeBadgeProperty, value);
        }
    }

    public class ColorPicker : Control
    {
        public static readonly DependencyProperty SelectedColorProperty =
            DependencyProperty.Register("SelectedColor", typeof(System.Windows.Media.Color), typeof(ColorPicker),
                new FrameworkPropertyMetadata(System.Windows.Media.Colors.Black, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public System.Windows.Media.Color SelectedColor
        {
            get => (System.Windows.Media.Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static readonly DependencyProperty ColorPaletteProperty =
            DependencyProperty.Register("ColorPalette", typeof(ColorCollection), typeof(ColorPicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public ColorCollection ColorPalette
        {
            get => (ColorCollection)GetValue(ColorPaletteProperty);
            set => SetValue(ColorPaletteProperty, value);
        }

        static ColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ColorPicker), new FrameworkPropertyMetadata(typeof(ColorPicker)));
        }
    }

    public class ColorCollection : System.Collections.ObjectModel.ObservableCollection<System.Windows.Media.Color> { }

    public class FontPicker : Control
    {
        public static readonly DependencyProperty SelectedFontProperty =
            DependencyProperty.Register("SelectedFont", typeof(string), typeof(FontPicker),
                new FrameworkPropertyMetadata("Calibri", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public string SelectedFont
        {
            get => (string)GetValue(SelectedFontProperty);
            set => SetValue(SelectedFontProperty, value);
        }

        public static readonly DependencyProperty FontFamilyListProperty =
            DependencyProperty.Register("FontFamilyList", typeof(System.Collections.ObjectModel.ObservableCollection<string>), typeof(FontPicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public System.Collections.ObjectModel.ObservableCollection<string> FontFamilyList
        {
            get => (System.Collections.ObjectModel.ObservableCollection<string>)GetValue(FontFamilyListProperty);
            set => SetValue(FontFamilyListProperty, value);
        }

        static FontPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontPicker), new FrameworkPropertyMetadata(typeof(FontPicker)));
        }
    }

    public class FontSizePicker : Control
    {
        public static readonly DependencyProperty SelectedSizeProperty =
            DependencyProperty.Register("SelectedSize", typeof(double), typeof(FontSizePicker),
                new FrameworkPropertyMetadata(11.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public double SelectedSize
        {
            get => (double)GetValue(SelectedSizeProperty);
            set => SetValue(SelectedSizeProperty, value);
        }

        public static readonly DependencyProperty FontSizeListProperty =
            DependencyProperty.Register("FontSizeList", typeof(System.Collections.ObjectModel.ObservableCollection<double>), typeof(FontSizePicker),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

        public System.Collections.ObjectModel.ObservableCollection<double> FontSizeList
        {
            get => (System.Collections.ObjectModel.ObservableCollection<double>)GetValue(FontSizeListProperty);
            set => SetValue(FontSizeListProperty, value);
        }

        static FontSizePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FontSizePicker), new FrameworkPropertyMetadata(typeof(FontSizePicker)));
        }
    }

    public class OfficeScrollBar : ScrollBar
    {
        public static readonly DependencyProperty OfficeThumbProperty =
            DependencyProperty.Register("OfficeThumb", typeof(Thumb), typeof(OfficeScrollBar), new PropertyMetadata(null));

        public Thumb OfficeThumb
        {
            get => (Thumb)GetValue(OfficeThumbProperty);
            set => SetValue(OfficeThumbProperty, value);
        }
    }

    public class OfficeGridSplitter : GridSplitter
    {
        public static readonly DependencyProperty OfficeResizeModeProperty =
            DependencyProperty.Register("OfficeResizeMode", typeof(GridResizeMode), typeof(OfficeGridSplitter),
                new FrameworkPropertyMetadata(GridResizeMode.Resize, FrameworkPropertyMetadataOptions.AffectsRender));

        public GridResizeMode OfficeResizeMode
        {
            get => (GridResizeMode)GetValue(OfficeResizeModeProperty);
            set => SetValue(OfficeResizeModeProperty, value);
        }
    }

    public class OfficeBorder : Border
    {
        public static readonly DependencyProperty OfficeCornerRadiusProperty =
            DependencyProperty.Register("OfficeCornerRadius", typeof(System.Windows.CornerRadius), typeof(OfficeBorder),
                new FrameworkPropertyMetadata(new System.Windows.CornerRadius(0), FrameworkPropertyMetadataOptions.AffectsRender));

        public System.Windows.CornerRadius OfficeCornerRadius
        {
            get => (System.Windows.CornerRadius)GetValue(OfficeCornerRadiusProperty);
            set => SetValue(OfficeCornerRadiusProperty, value);
        }

        public static readonly DependencyProperty OfficeBorderStyleProperty =
            DependencyProperty.Register("OfficeBorderStyle", typeof(BorderStyle), typeof(OfficeBorder),
                new FrameworkPropertyMetadata(BorderStyle.Single, FrameworkPropertyMetadataOptions.AffectsRender));

        public BorderStyle OfficeBorderStyle
        {
            get => (BorderStyle)GetValue(OfficeBorderStyleProperty);
            set => SetValue(OfficeBorderStyleProperty, value);
        }
    }

    public enum BorderStyle
    {
        None,
        Single,
        Thick,
        Double,
        Dotted,
        Dashed
    }
}

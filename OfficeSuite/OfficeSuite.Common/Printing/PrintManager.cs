using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace OfficeSuite.Common.Printing
{
    public class PrintManager : INotifyPropertyChanged
    {
        private static PrintManager? _instance;
        public static PrintManager Instance => _instance ??= new PrintManager();

        public event PropertyChangedEventHandler? PropertyChanged;

        public event EventHandler<PrintPageEventArgs>? PrintPage;
        public event EventHandler<PrintEventArgs>? BeginPrint;
        public event EventHandler<PrintEventArgs>? EndPrint;

        private PrintDocumentType _currentDocumentType = PrintDocumentType.Word;
        private PageOrientation _pageOrientation = PageOrientation.Portrait;
        private PaperSize _paperSize = PaperSize.A4;
        private PaperSource _paperSource = PaperSource.Auto;
        private Margins _margins = new(25, 25, 25, 25);
        private int _copies = 1;
        private bool _collate = true;
        private bool _colorPrint = true;
        private PrintQuality _quality = PrintQuality.High;
        private bool _duplexPrinting = false;
        private int _pagesPerSheet = 1;
        private bool _scaleToFit = true;
        private bool _fitToPage = true;
        private string _printerName = string.Empty;
        private List<int> _selectedPages = new();

        public PrintDocumentType CurrentDocumentType
        {
            get => _currentDocumentType;
            set { _currentDocumentType = value; OnPropertyChanged(nameof(CurrentDocumentType)); }
        }

        public PageOrientation PageOrientation
        {
            get => _pageOrientation;
            set { _pageOrientation = value; OnPropertyChanged(nameof(PageOrientation)); }
        }

        public PaperSize PaperSize
        {
            get => _paperSize;
            set { _paperSize = value; OnPropertyChanged(nameof(PaperSize)); }
        }

        public PaperSource PaperSource
        {
            get => _paperSource;
            set { _paperSource = value; OnPropertyChanged(nameof(PaperSource)); }
        }

        public Margins Margins
        {
            get => _margins;
            set { _margins = value; OnPropertyChanged(nameof(Margins)); }
        }

        public int Copies
        {
            get => _copies;
            set { _copies = value; OnPropertyChanged(nameof(Copies)); }
        }

        public bool Collate
        {
            get => _collate;
            set { _collate = value; OnPropertyChanged(nameof(Collate)); }
        }

        public bool ColorPrint
        {
            get => _colorPrint;
            set { _colorPrint = value; OnPropertyChanged(nameof(ColorPrint)); }
        }

        public PrintQuality Quality
        {
            get => _quality;
            set { _quality = value; OnPropertyChanged(nameof(Quality)); }
        }

        public bool DuplexPrinting
        {
            get => _duplexPrinting;
            set { _duplexPrinting = value; OnPropertyChanged(nameof(DuplexPrinting)); }
        }

        public int PagesPerSheet
        {
            get => _pagesPerSheet;
            set { _pagesPerSheet = value; OnPropertyChanged(nameof(PagesPerSheet)); }
        }

        public bool ScaleToFit
        {
            get => _scaleToFit;
            set { _scaleToFit = value; OnPropertyChanged(nameof(ScaleToFit)); }
        }

        public bool FitToPage
        {
            get => _fitToPage;
            set { _fitToPage = value; OnPropertyChanged(nameof(FitToPage)); }
        }

        public string PrinterName
        {
            get => _printerName;
            set { _printerName = value; OnPropertyChanged(nameof(PrinterName)); }
        }

        public List<int> SelectedPages
        {
            get => _selectedPages;
            set { _selectedPages = value; OnPropertyChanged(nameof(SelectedPages)); }
        }

        public List<string> AvailablePrinters { get; } = new() { "Default Printer" };

        public List<PaperSize> AvailablePaperSizes { get; } = new()
        {
            PaperSize.A4,
            PaperSize.A3,
            PaperSize.A5,
            PaperSize.Letter,
            PaperSize.Legal,
            PaperSize.Tabloid,
            PaperSize.Custom
        };

        public List<PrintQuality> AvailableQualities { get; } = new()
        {
            PrintQuality.Draft,
            PrintQuality.Low,
            PrintQuality.Medium,
            PrintQuality.High,
            PrintQuality.Highest
        };

        private PrintManager()
        {
        }

        public PrintResult Print(PrintContent content)
        {
            var result = new PrintResult
            {
                Success = true,
                PagesPrinted = 0,
                StartTime = DateTime.Now
            };

            OnBeginPrint(new PrintEventArgs { DocumentType = _currentDocumentType });

            try
            {
                int totalPages = CalculateTotalPages(content);

                for (int page = 1; page <= totalPages; page++)
                {
                    if (_selectedPages.Count > 0 && !_selectedPages.Contains(page))
                        continue;

                    OnPrintPage(new PrintPageEventArgs
                    {
                        PageNumber = page,
                        TotalPages = totalPages,
                        Content = content,
                        PageSize = GetPageSize(),
                        Margins = _margins
                    });

                    result.PagesPrinted++;
                }

                result.EndTime = DateTime.Now;
                result.Duration = result.EndTime.Value - result.StartTime;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            OnEndPrint(new PrintEventArgs { DocumentType = _currentDocumentType });

            return result;
        }

        public PrintPreviewResult Preview(PrintContent content)
        {
            var result = new PrintPreviewResult
            {
                TotalPages = CalculateTotalPages(content),
                PageSize = GetPageSize(),
                Margins = _margins,
                Orientation = _pageOrientation,
                Scale = CalculateScale(content)
            };

            return result;
        }

        private int CalculateTotalPages(PrintContent content)
        {
            return content switch
            {
                WordPrintContent wordContent => CalculateWordPages(wordContent),
                SpreadsheetPrintContent spreadsheetContent => CalculateSpreadsheetPages(spreadsheetContent),
                PresentationPrintContent presentationContent => presentationContent.SlideCount,
                _ => 1
            };
        }

        private int CalculateWordPages(WordPrintContent content)
        {
            int linesPerPage = GetLinesPerPage();
            int totalLines = content.LineCount;
            return Math.Max(1, (int)Math.Ceiling((double)totalLines / linesPerPage));
        }

        private int CalculateSpreadsheetPages(SpreadsheetPrintContent content)
        {
            int rowsPerPage = GetRowsPerPage();
            int columnsPerPage = GetColumnsPerPage();
            int totalRowPages = (int)Math.Ceiling((double)content.TotalRows / rowsPerPage);
            int totalColumnPages = (int)Math.Ceiling((double)content.TotalColumns / columnsPerPage);
            return totalRowPages * totalColumnPages;
        }

        private int GetLinesPerPage()
        {
            double pageHeight = PageOrientation == PageOrientation.Portrait ? 297 : 420;
            double lineHeight = 4.5;
            double marginTopBottom = Margins.Top + Margins.Bottom;
            return (int)((pageHeight - marginTopBottom) / lineHeight);
        }

        private int GetRowsPerPage()
        {
            double pageHeight = PageOrientation == PageOrientation.Portrait ? 297 : 420;
            double rowHeight = 15;
            double marginTopBottom = Margins.Top + Margins.Bottom;
            return (int)((pageHeight - marginTopBottom) / rowHeight);
        }

        private int GetColumnsPerPage()
        {
            double pageWidth = PageOrientation == PageOrientation.Portrait ? 210 : 297;
            double columnWidth = 50;
            double marginLeftRight = Margins.Left + Margins.Right;
            return (int)((pageWidth - marginLeftRight) / columnWidth);
        }

        private Size GetPageSize()
        {
            double width = PageOrientation == PageOrientation.Portrait ? 210 : 297;
            double height = PageOrientation == PageOrientation.Portrait ? 297 : 210;

            if (PaperSize == PaperSize.Letter)
            {
                width = 215.9;
                height = 279.4;
                if (PageOrientation == PageOrientation.Landscape)
                {
                    (width, height) = (height, width);
                }
            }

            return new Size(width, height);
        }

        private double CalculateScale(PrintContent content)
        {
            if (!ScaleToFit)
                return 1.0;

            var pageSize = GetPageSize();
            double scaleX = (pageSize.Width - Margins.Left - Margins.Right) / content.ContentWidth;
            double scaleY = (pageSize.Height - Margins.Top - Margins.Bottom) / content.ContentHeight;

            return Math.Min(scaleX, scaleY);
        }

        protected virtual void OnPrintPage(PrintPageEventArgs e)
        {
            PrintPage?.Invoke(this, e);
        }

        protected virtual void OnBeginPrint(PrintEventArgs e)
        {
            BeginPrint?.Invoke(this, e);
        }

        protected virtual void OnEndPrint(PrintEventArgs e)
        {
            EndPrint?.Invoke(this, e);
        }

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public enum PrintDocumentType
    {
        Word,
        Spreadsheet,
        Presentation
    }

    public enum PageOrientation
    {
        Portrait,
        Landscape,
        ReversePortrait,
        ReverseLandscape
    }

    public enum PaperSize
    {
        A4,
        A3,
        A5,
        Letter,
        Legal,
        Tabloid,
        Custom
    }

    public enum PaperSource
    {
        Auto,
        Manual,
        Envelope,
        SheetFeed,
        Cassette
    }

    public enum PrintQuality
    {
        Draft,
        Low,
        Medium,
        High,
        Highest
    }

    public class Margins
    {
        public double Top { get; set; }
        public double Bottom { get; set; }
        public double Left { get; set; }
        public double Right { get; set; }

        public Margins(double top, double bottom, double left, double right)
        {
            Top = top;
            Bottom = bottom;
            Left = left;
            Right = right;
        }
    }

    public class PrintContent
    {
        public double ContentWidth { get; set; }
        public double ContentHeight { get; set; }
        public int TotalPages { get; set; }
    }

    public class WordPrintContent : PrintContent
    {
        public int LineCount { get; set; }
        public string DocumentText { get; set; } = string.Empty;
        public List<PrintHeaderFooter>? Headers { get; set; }
        public List<PrintHeaderFooter>? Footers { get; set; }
    }

    public class SpreadsheetPrintContent : PrintContent
    {
        public int TotalRows { get; set; }
        public int TotalColumns { get; set; }
        public string SheetName { get; set; } = string.Empty;
        public bool PrintGridLines { get; set; }
        public bool PrintRowColumnHeaders { get; set; }
    }

    public class PresentationPrintContent : PrintContent
    {
        public int SlideCount { get; set; }
        public List<int>? SelectedSlides { get; set; }
        public bool PrintHandouts { get; set; }
        public int HandoutSlidesPerPage { get; set; } = 2;
    }

    public class PrintHeaderFooter
    {
        public string LeftText { get; set; } = string.Empty;
        public string CenterText { get; set; } = string.Empty;
        public string RightText { get; set; } = string.Empty;
        public bool ShowOnFirstPage { get; set; }
        public bool ShowPageNumbers { get; set; }
        public string DateFormat { get; set; } = "MMMM dd, yyyy";
    }

    public class PrintResult
    {
        public bool Success { get; set; }
        public int PagesPrinted { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string? ErrorMessage { get; set; }
    }

    public class PrintPreviewResult
    {
        public int TotalPages { get; set; }
        public Size PageSize { get; set; }
        public Margins Margins { get; set; } = new(25, 25, 25, 25);
        public PageOrientation Orientation { get; set; }
        public double Scale { get; set; }
    }

    public class PrintPageEventArgs : EventArgs
    {
        public int PageNumber { get; set; }
        public int TotalPages { get; set; }
        public PrintContent Content { get; set; } = new();
        public Size PageSize { get; set; }
        public Margins Margins { get; set; } = new(25, 25, 25, 25);
    }

    public class PrintEventArgs : EventArgs
    {
        public PrintDocumentType DocumentType { get; set; }
    }

    public class Size
    {
        public double Width { get; set; }
        public double Height { get; set; }

        public Size(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }

    public class PrintPreviewControl : FrameworkElement
    {
        public static readonly DependencyProperty ContentProperty =
            DependencyProperty.Register(nameof(Content), typeof(object), typeof(PrintPreviewControl));

        public static readonly DependencyProperty ZoomLevelProperty =
            DependencyProperty.Register(nameof(ZoomLevel), typeof(double), typeof(PrintPreviewControl),
                new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender));

        public object Content
        {
            get => GetValue(ContentProperty);
            set => SetValue(ContentProperty, value);
        }

        public double ZoomLevel
        {
            get => (double)GetValue(ZoomLevelProperty);
            set => SetValue(ZoomLevelProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            var bounds = new Rect(0, 0, ActualWidth, ActualHeight);
            drawingContext.DrawRectangle(Brushes.White, new Pen(Brushes.Gray, 1), bounds);
        }
    }

    public class PrintPreviewManager
    {
        private readonly PrintManager _printManager;
        private PrintPreviewResult? _currentPreview;
        private int _currentPageIndex;

        public PrintPreviewManager(PrintManager printManager)
        {
            _printManager = printManager;
        }

        public PrintPreviewResult GeneratePreview(PrintContent content)
        {
            _currentPreview = _printManager.Preview(content);
            _currentPageIndex = 0;
            return _currentPreview;
        }

        public bool HasNextPage => _currentPreview != null && _currentPageIndex < _currentPreview.TotalPages - 1;
        public bool HasPreviousPage => _currentPageIndex > 0;

        public void NextPage()
        {
            if (HasNextPage)
                _currentPageIndex++;
        }

        public void PreviousPage()
        {
            if (HasPreviousPage)
                _currentPageIndex--;
        }

        public void GoToPage(int pageIndex)
        {
            if (_currentPreview != null && pageIndex >= 0 && pageIndex < _currentPreview.TotalPages)
                _currentPageIndex = pageIndex;
        }

        public int CurrentPage => _currentPageIndex + 1;
    }
}

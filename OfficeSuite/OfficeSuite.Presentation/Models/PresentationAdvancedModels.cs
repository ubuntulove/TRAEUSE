using System;
using System.Collections.Generic;
using System.Linq;

namespace OfficeSuite.Presentation.Models
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<TableRow> Rows { get; set; }
        public TableStyle Style { get; set; }
        public TableProperties Properties { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVisible { get; set; }
        public double ZIndex { get; set; }
        public string AlternativeText { get; set; }

        public Table()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            X = 0;
            Y = 0;
            Width = 100;
            Height = 100;
            RowCount = 3;
            ColumnCount = 3;
            Rows = new List<TableRow>();
            Style = new TableStyle();
            Properties = new TableProperties();
            IsLocked = false;
            IsVisible = true;
            ZIndex = 0;
        }

        public Table Clone()
        {
            return new Table
            {
                Id = this.Id,
                Name = this.Name,
                X = this.X,
                Y = this.Y,
                Width = this.Width,
                Height = this.Height,
                RowCount = this.RowCount,
                ColumnCount = this.ColumnCount,
                Rows = this.Rows.Select(r => r.Clone()).ToList(),
                Style = this.Style.Clone(),
                Properties = this.Properties.Clone(),
                IsLocked = this.IsLocked,
                IsVisible = this.IsVisible,
                ZIndex = this.ZIndex,
                AlternativeText = this.AlternativeText
            };
        }
    }

    public class TableRow
    {
        public int Index { get; set; }
        public List<TableCell> Cells { get; set; }
        public double Height { get; set; }
        public TableRowProperties Properties { get; set; }

        public TableRow()
        {
            Index = 0;
            Cells = new List<TableCell>();
            Height = 20;
            Properties = new TableRowProperties();
        }

        public TableRow Clone()
        {
            return new TableRow
            {
                Index = this.Index,
                Cells = this.Cells.Select(c => c.Clone()).ToList(),
                Height = this.Height,
                Properties = this.Properties.Clone()
            };
        }
    }

    public class TableCell
    {
        public int RowIndex { get; set; }
        public int ColumnIndex { get; set; }
        public int RowSpan { get; set; }
        public int ColumnSpan { get; set; }
        public List<Paragraph> Paragraphs { get; set; }
        public CellProperties Properties { get; set; }
        public string Text { get; set; }

        public TableCell()
        {
            RowIndex = 0;
            ColumnIndex = 0;
            RowSpan = 1;
            ColumnSpan = 1;
            Paragraphs = new List<Paragraph> { new Paragraph() };
            Properties = new CellProperties();
            Text = string.Empty;
        }

        public TableCell Clone()
        {
            return new TableCell
            {
                RowIndex = this.RowIndex,
                ColumnIndex = this.ColumnIndex,
                RowSpan = this.RowSpan,
                ColumnSpan = this.ColumnSpan,
                Paragraphs = this.Paragraphs.Select(p => p.Clone()).ToList(),
                Properties = this.Properties.Clone(),
                Text = this.Text
            };
        }
    }

    public class TableStyle
    {
        public string Name { get; set; }
        public bool IsFirstRow { get; set; }
        public bool IsLastRow { get; set; }
        public bool IsFirstColumn { get; set; }
        public bool IsLastColumn { get; set; }
        public bool IsRowBanded { get; set; }
        public bool IsColumnBanded { get; set; }
        public FillFormat HeaderRowFill { get; set; }
        public FillFormat DataRowFill { get; set; }
        public FillFormat FirstColumnFill { get; set; }
        public FillFormat LastColumnFill { get; set; }
        public BorderFormat Border { get; set; }

        public TableStyle()
        {
            Name = "Medium Style 2";
            IsFirstRow = true;
            IsLastRow = false;
            IsFirstColumn = false;
            IsLastColumn = false;
            IsRowBanded = true;
            IsColumnBanded = false;
            HeaderRowFill = new FillFormat();
            DataRowFill = new FillFormat();
            FirstColumnFill = new FillFormat();
            LastColumnFill = new FillFormat();
            Border = new BorderFormat();
        }

        public TableStyle Clone()
        {
            return new TableStyle
            {
                Name = this.Name,
                IsFirstRow = this.IsFirstRow,
                IsLastRow = this.IsLastRow,
                IsFirstColumn = this.IsFirstColumn,
                IsLastColumn = this.IsLastColumn,
                IsRowBanded = this.IsRowBanded,
                IsColumnBanded = this.IsColumnBanded,
                HeaderRowFill = this.HeaderRowFill.Clone(),
                DataRowFill = this.DataRowFill.Clone(),
                FirstColumnFill = this.FirstColumnFill.Clone(),
                LastColumnFill = this.LastColumnFill.Clone(),
                Border = this.Border.Clone()
            };
        }
    }

    public class BorderFormat
    {
        public string Color { get; set; }
        public double Width { get; set; }
        public BorderStyle Style { get; set; }
        public BorderType Type { get; set; }

        public BorderFormat()
        {
            Color = "#000000";
            Width = 0.5;
            Style = BorderStyle.Single;
            Type = BorderType.All;
        }

        public BorderFormat Clone()
        {
            return new BorderFormat
            {
                Color = this.Color,
                Width = this.Width,
                Style = this.Style,
                Type = this.Type
            };
        }
    }

    public enum BorderStyle
    {
        None,
        Single,
        Double,
        Thick,
        Thin,
        Dotted,
        Dashed
    }

    public enum BorderType
    {
        All,
        Top,
        Bottom,
        Left,
        Right,
        Inside
    }

    public class TableProperties
    {
        public HorizontalTextAlignment Alignment { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public double TopMargin { get; set; }
        public double BottomMargin { get; set; }
        public bool AllowOverlap { get; set; }
        public TextDirection TextDirection { get; set; }
        public CellVerticalAlignment CellVerticalAlignment { get; set; }
        public bool HasHeaderRow { get; set; }
        public bool HasTotalRow { get; set; }
        public bool HasBandedRows { get; set; }
        public bool HasBandedColumns { get; set; }
        public bool HasFirstColumn { get; set; }
        public bool HasLastColumn { get; set; }

        public TableProperties()
        {
            Alignment = HorizontalTextAlignment.Left;
            LeftMargin = 0.1;
            RightMargin = 0.1;
            TopMargin = 0;
            BottomMargin = 0;
            AllowOverlap = false;
            TextDirection = TextDirection.Horizontal;
            CellVerticalAlignment = CellVerticalAlignment.Top;
            HasHeaderRow = true;
            HasTotalRow = false;
            HasBandedRows = true;
            HasBandedColumns = false;
            HasFirstColumn = false;
            HasLastColumn = false;
        }

        public TableProperties Clone()
        {
            return new TableProperties
            {
                Alignment = this.Alignment,
                LeftMargin = this.LeftMargin,
                RightMargin = this.RightMargin,
                TopMargin = this.TopMargin,
                BottomMargin = this.BottomMargin,
                AllowOverlap = this.AllowOverlap,
                TextDirection = this.TextDirection,
                CellVerticalAlignment = this.CellVerticalAlignment,
                HasHeaderRow = this.HasHeaderRow,
                HasTotalRow = this.HasTotalRow,
                HasBandedRows = this.HasBandedRows,
                HasBandedColumns = this.HasBandedColumns,
                HasFirstColumn = this.HasFirstColumn,
                HasLastColumn = this.HasLastColumn
            };
        }
    }

    public class TableRowProperties
    {
        public HeightRule HeightRule { get; set; }
        public double Height { get; set; }
        public bool IsHeader { get; set; }
        public bool IsTotal { get; set; }
        public bool CanBreak { get; set; }
        public Shading Shading { get; set; }

        public TableRowProperties()
        {
            HeightRule = HeightRule.AtLeast;
            Height = 20;
            IsHeader = false;
            IsTotal = false;
            CanBreak = true;
            Shading = new Shading();
        }

        public TableRowProperties Clone()
        {
            return new TableRowProperties
            {
                HeightRule = this.HeightRule,
                Height = this.Height,
                IsHeader = this.IsHeader,
                IsTotal = this.IsTotal,
                CanBreak = this.CanBreak,
                Shading = this.Shading.Clone()
            };
        }
    }

    public class CellProperties
    {
        public double Width { get; set; }
        public Shading Shading { get; set; }
        public BorderFormat TopBorder { get; set; }
        public BorderFormat BottomBorder { get; set; }
        public BorderFormat LeftBorder { get; set; }
        public BorderFormat RightBorder { get; set; }
        public double TopMargin { get; set; }
        public double BottomMargin { get; set; }
        public double LeftMargin { get; set; }
        public double RightMargin { get; set; }
        public CellVerticalAlignment VerticalAlignment { get; set; }
        public TextDirection TextDirection { get; set; }
        public bool IsMerged { get; set; }
        public bool IsFirstInMerge { get; set; }
        public bool IsLastInMerge { get; set; }

        public CellProperties()
        {
            Width = 50;
            Shading = new Shading();
            TopBorder = new BorderFormat();
            BottomBorder = new BorderFormat();
            LeftBorder = new BorderFormat();
            RightBorder = new BorderFormat();
            TopMargin = 0;
            BottomMargin = 0;
            LeftMargin = 0.08;
            RightMargin = 0.08;
            VerticalAlignment = CellVerticalAlignment.Top;
            TextDirection = TextDirection.Horizontal;
            IsMerged = false;
            IsFirstInMerge = false;
            IsLastInMerge = false;
        }

        public CellProperties Clone()
        {
            return new CellProperties
            {
                Width = this.Width,
                Shading = this.Shading.Clone(),
                TopBorder = this.TopBorder.Clone(),
                BottomBorder = this.BottomBorder.Clone(),
                LeftBorder = this.LeftBorder.Clone(),
                RightBorder = this.RightBorder.Clone(),
                TopMargin = this.TopMargin,
                BottomMargin = this.BottomMargin,
                LeftMargin = this.LeftMargin,
                RightMargin = this.RightMargin,
                VerticalAlignment = this.VerticalAlignment,
                TextDirection = this.TextDirection,
                IsMerged = this.IsMerged,
                IsFirstInMerge = this.IsFirstInMerge,
                IsLastInMerge = this.IsLastInMerge
            };
        }
    }

    public class Shading
    {
        public ShadingType Type { get; set; }
        public string Color { get; set; }
        public string FillColor { get; set; }
        public string Pattern { get; set; }

        public Shading()
        {
            Type = ShadingType.Clear;
            Color = "auto";
            FillColor = string.Empty;
            Pattern = string.Empty;
        }

        public Shading Clone()
        {
            return new Shading
            {
                Type = this.Type,
                Color = this.Color,
                FillColor = this.FillColor,
                Pattern = this.Pattern
            };
        }
    }

    public enum ShadingType
    {
        Clear,
        Solid,
        HorizontalStripe,
        VerticalStripe,
        DiagonalUp,
        DiagonalDown,
        Cross,
        ReverseDiagonal
    }

    public enum HeightRule
    {
        AtLeast,
        Exactly,
        Auto
    }

    public enum CellVerticalAlignment
    {
        Top,
        Center,
        Bottom
    }

    public class PresentationDocument
    {
        public string Title { get; set; }
        public string Subject { get; set; }
        public string Author { get; set; }
        public string Keywords { get; set; }
        public string Description { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ModifiedDate { get; set; }
        public List<Slide> Slides { get; set; }
        public SlideMaster Master { get; set; }
        public Theme Theme { get; set; }
        public SlideSize Size { get; set; }
        public int CurrentSlideIndex { get; set; }
        public string FilePath { get; set; }
        public bool IsModified { get; set; }
        public DocumentProtection Protection { get; set; }

        public PresentationDocument()
        {
            Title = "Untitled Presentation";
            Subject = string.Empty;
            Author = string.Empty;
            Keywords = string.Empty;
            Description = string.Empty;
            CreatedDate = DateTime.Now;
            ModifiedDate = DateTime.Now;
            Slides = new List<Slide>();
            Master = new SlideMaster();
            Theme = new Theme();
            Size = new SlideSize();
            CurrentSlideIndex = 0;
            IsModified = false;
            Protection = new DocumentProtection();
        }

        public Slide AddSlide()
        {
            var slide = new Slide
            {
                Index = Slides.Count,
                Name = $"Slide {Slides.Count + 1}"
            };
            Slides.Add(slide);
            return slide;
        }

        public void RemoveSlide(int index)
        {
            if (index >= 0 && index < Slides.Count)
            {
                Slides.RemoveAt(index);
                for (int i = index; i < Slides.Count; i++)
                {
                    Slides[i].Index = i;
                    Slides[i].Name = $"Slide {i + 1}";
                }
            }
        }

        public PresentationDocument Clone()
        {
            return new PresentationDocument
            {
                Title = this.Title,
                Subject = this.Subject,
                Author = this.Author,
                Keywords = this.Keywords,
                Description = this.Description,
                CreatedDate = this.CreatedDate,
                ModifiedDate = this.ModifiedDate,
                Slides = this.Slides.Select(s => s.Clone()).ToList(),
                Master = this.Master.Clone(),
                Theme = this.Theme.Clone(),
                Size = this.Size.Clone(),
                CurrentSlideIndex = this.CurrentSlideIndex,
                FilePath = this.FilePath,
                IsModified = this.IsModified,
                Protection = this.Protection.Clone()
            };
        }
    }

    public class DocumentProtection
    {
        public bool IsProtected { get; set; }
        public ProtectionType Type { get; set; }
        public string Password { get; set; }
        public string PasswordHash { get; set; }
        public bool IsEditingRestricted { get; set; }
        public bool IsCommentsRestricted { get; set; }
        public bool IsTrackedChangesRestricted { get; set; }
        public bool IsReadOnly { get; set; }
        public DateTime? ProtectionExpiryDate { get; set; }
        public string ProtectionScope { get; set; }

        public DocumentProtection()
        {
            IsProtected = false;
            Type = ProtectionType.None;
            Password = string.Empty;
            PasswordHash = string.Empty;
            IsEditingRestricted = false;
            IsCommentsRestricted = false;
            IsTrackedChangesRestricted = false;
            IsReadOnly = false;
            ProtectionScope = "Document";
        }

        public DocumentProtection Clone()
        {
            return new DocumentProtection
            {
                IsProtected = this.IsProtected,
                Type = this.Type,
                Password = this.Password,
                PasswordHash = this.PasswordHash,
                IsEditingRestricted = this.IsEditingRestricted,
                IsCommentsRestricted = this.IsCommentsRestricted,
                IsTrackedChangesRestricted = this.IsTrackedChangesRestricted,
                IsReadOnly = this.IsReadOnly,
                ProtectionExpiryDate = this.ProtectionExpiryDate,
                ProtectionScope = this.ProtectionScope
            };
        }
    }

    public enum ProtectionType
    {
        None,
        ReadOnly,
        Comments,
        TrackChanges,
        Forms
    }

    public class GroupShape
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Rotation { get; set; }
        public List<Shape> Shapes { get; set; }
        public FillFormat Fill { get; set; }
        public LineFormat Line { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVisible { get; set; }
        public double ZIndex { get; set; }

        public GroupShape()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            X = 0;
            Y = 0;
            Width = 100;
            Height = 100;
            Rotation = 0;
            Shapes = new List<Shape>();
            Fill = new FillFormat();
            Line = new LineFormat();
            IsLocked = false;
            IsVisible = true;
            ZIndex = 0;
        }

        public GroupShape Clone()
        {
            return new GroupShape
            {
                Id = this.Id,
                Name = this.Name,
                X = this.X,
                Y = this.Y,
                Width = this.Width,
                Height = this.Height,
                Rotation = this.Rotation,
                Shapes = this.Shapes.Select(s => s.Clone()).ToList(),
                Fill = this.Fill.Clone(),
                Line = this.Line.Clone(),
                IsLocked = this.IsLocked,
                IsVisible = this.IsVisible,
                ZIndex = this.ZIndex
            };
        }
    }

    public class Connector
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ConnectorType Type { get; set; }
        public Shape StartShape { get; set; }
        public Shape EndShape { get; set; }
        public ConnectionSite StartSite { get; set; }
        public ConnectionSite EndSite { get; set; }
        public LineFormat Line { get; set; }
        public bool IsLocked { get; set; }
        public bool IsVisible { get; set; }
        public double ZIndex { get; set; }
        public ArrowheadStyle StartArrowhead { get; set; }
        public ArrowheadStyle EndArrowhead { get; set; }

        public Connector()
        {
            Id = Guid.NewGuid().GetHashCode();
            Name = string.Empty;
            Type = ConnectorType.Straight;
            StartShape = null;
            EndShape = null;
            StartSite = new ConnectionSite();
            EndSite = new ConnectionSite();
            Line = new LineFormat();
            IsLocked = false;
            IsVisible = true;
            ZIndex = 0;
            StartArrowhead = ArrowheadStyle.None;
            EndArrowhead = ArrowheadStyle.None;
        }

        public Connector Clone()
        {
            return new Connector
            {
                Id = this.Id,
                Name = this.Name,
                Type = this.Type,
                StartShape = this.StartShape?.Clone(),
                EndShape = this.EndShape?.Clone(),
                StartSite = this.StartSite.Clone(),
                EndSite = this.EndSite.Clone(),
                Line = this.Line.Clone(),
                IsLocked = this.IsLocked,
                IsVisible = this.IsVisible,
                ZIndex = this.ZIndex,
                StartArrowhead = this.StartArrowhead,
                EndArrowhead = this.EndArrowhead
            };
        }
    }

    public enum ConnectorType
    {
        Straight,
        Bent,
        Curved,
        Elbow
    }

    public class Comment
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public string AuthorInitials { get; set; }
        public DateTime DateTime { get; set; }
        public string Text { get; set; }
        public int SlideIndex { get; set; }
        public int ShapeId { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int? ParentCommentId { get; set; }
        public bool IsResolved { get; set; }
        public bool IsDeleted { get; set; }

        public Comment()
        {
            Id = Guid.NewGuid().GetHashCode();
            Author = string.Empty;
            AuthorInitials = string.Empty;
            DateTime = DateTime.Now;
            Text = string.Empty;
            SlideIndex = 0;
            ShapeId = 0;
            IsResolved = false;
            IsDeleted = false;
        }

        public Comment Clone()
        {
            return new Comment
            {
                Id = this.Id,
                Author = this.Author,
                AuthorInitials = this.AuthorInitials,
                DateTime = this.DateTime,
                Text = this.Text,
                SlideIndex = this.SlideIndex,
                ShapeId = this.ShapeId,
                X = this.X,
                Y = this.Y,
                ParentCommentId = this.ParentCommentId,
                IsResolved = this.IsResolved,
                IsDeleted = this.IsDeleted
            };
        }
    }

    public class Reviewer
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Initials { get; set; }
        public string Color { get; set; }
        public string LastInitials { get; set; }
        public bool IsVisible { get; set; }

        public Reviewer()
        {
            Id = Guid.NewGuid().ToString();
            Name = string.Empty;
            Initials = string.Empty;
            Color = "#FF0000";
            LastInitials = string.Empty;
            IsVisible = true;
        }

        public Reviewer Clone()
        {
            return new Reviewer
            {
                Id = this.Id,
                Name = this.Name,
                Initials = this.Initials,
                Color = this.Color,
                LastInitials = this.LastInitials,
                IsVisible = this.IsVisible
            };
        }
    }

    public class InkAnnotation
    {
        public int Id { get; set; }
        public string Author { get; set; }
        public DateTime DateTime { get; set; }
        public byte[] InkData { get; set; }
        public int SlideIndex { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool IsVisible { get; set; }
        public bool IsLocked { get; set; }
        public string Color { get; set; }
        public double StrokeWidth { get; set; }
        public InkPressureSensitivity PressureSensitivity { get; set; }

        public InkAnnotation()
        {
            Id = Guid.NewGuid().GetHashCode();
            Author = string.Empty;
            DateTime = DateTime.Now;
            InkData = new byte[0];
            SlideIndex = 0;
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            IsVisible = true;
            IsLocked = false;
            Color = "#000000";
            StrokeWidth = 1;
            PressureSensitivity = InkPressureSensitivity.Medium;
        }

        public InkAnnotation Clone()
        {
            return new InkAnnotation
            {
                Id = this.Id,
                Author = this.Author,
                DateTime = this.DateTime,
                InkData = this.InkData != null ? (byte[])this.InkData.Clone() : null,
                SlideIndex = this.SlideIndex,
                X = this.X,
                Y = this.Y,
                Width = this.Width,
                Height = this.Height,
                IsVisible = this.IsVisible,
                IsLocked = this.IsLocked,
                Color = this.Color,
                StrokeWidth = this.StrokeWidth,
                PressureSensitivity = this.PressureSensitivity
            };
        }
    }

    public enum InkPressureSensitivity
    {
        Off,
        Low,
        Medium,
        High
    }

    public class Hyperlink
    {
        public string Url { get; set; }
        public string DisplayText { get; set; }
        public string ToolTip { get; set; }
        public HyperlinkAction Action { get; set; }
        public string FrameName { get; set; }
        public string SoundEffect { get; set; }
        public bool IsExternal { get; set; }
        public int? TargetSlideIndex { get; set; }
        public string BookmarkName { get; set; }

        public Hyperlink()
        {
            Url = string.Empty;
            DisplayText = string.Empty;
            ToolTip = string.Empty;
            Action = HyperlinkAction.OpenHyperlink;
            FrameName = "_self";
            IsExternal = true;
        }

        public Hyperlink Clone()
        {
            return new Hyperlink
            {
                Url = this.Url,
                DisplayText = this.DisplayText,
                ToolTip = this.ToolTip,
                Action = this.Action,
                FrameName = this.FrameName,
                SoundEffect = this.SoundEffect,
                IsExternal = this.IsExternal,
                TargetSlideIndex = this.TargetSlideIndex,
                BookmarkName = this.BookmarkName
            };
        }
    }

    public enum HyperlinkAction
    {
        OpenHyperlink,
        GoToSlide,
        GoToPreviousSlide,
        GoToNextSlide,
        GoToLastSlideViewed,
        GoToFirstSlide,
        GoToLastSlide,
        PlaySound,
        StartCustomShow,
        GoToCustomShow,
        EndShow,
        OpenFile,
        Custom,
        Unknown
    }

    public class ActionSettings
    {
        public Hyperlink Hyperlink { get; set; }
        public ActionType Type { get; set; }
        public string Program { get; set; }
        public string Arguments { get; set; }
        public bool IsStopPreviousSound { get; set; }
        public ActionAnimateAction AnimateAction { get; set; }
        public bool IsPlay { get; set; }
        public string CustomShowName { get; set; }

        public ActionSettings()
        {
            Hyperlink = new Hyperlink();
            Type = ActionType.Hyperlink;
            Program = string.Empty;
            Arguments = string.Empty;
            IsStopPreviousSound = false;
            AnimateAction = ActionAnimateAction.None;
            IsPlay = true;
        }

        public ActionSettings Clone()
        {
            return new ActionSettings
            {
                Hyperlink = this.Hyperlink.Clone(),
                Type = this.Type,
                Program = this.Program,
                Arguments = this.Arguments,
                IsStopPreviousSound = this.IsStopPreviousSound,
                AnimateAction = this.AnimateAction,
                IsPlay = this.IsPlay,
                CustomShowName = this.CustomShowName
            };
        }
    }

    public enum ActionType
    {
        Hyperlink,
        Custom,
        RunProgram,
        Macro,
        Action,
        PlaySound,
        GoToSlide,
        None
    }

    public enum ActionAnimateAction
    {
        None,
        Null,
        ObjectAction,
        Call,
        Command
    }

    public class CustomShow
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<int> SlideIndices { get; set; }
        public string Description { get; set; }
        public bool IsHidden { get; set; }

        public CustomShow()
        {
            Id = Guid.NewGuid().ToString();
            Name = "Custom Show";
            SlideIndices = new List<int>();
            IsHidden = false;
        }

        public CustomShow Clone()
        {
            return new CustomShow
            {
                Id = this.Id,
                Name = this.Name,
                SlideIndices = new List<int>(this.SlideIndices),
                Description = this.Description,
                IsHidden = this.IsHidden
            };
        }
    }

    public class PrintSettings
    {
        public PrintWhat PrintWhat { get; set; }
        public PrintColorMode ColorMode { get; set; }
        public PrintQuality Quality { get; set; }
        public PrintOrder Order { get; set; }
        public PaperSize PaperSize { get; set; }
        public PaperOrientation PaperOrientation { get; set; }
        public PaperSource PaperSource { get; set; }
        public string CustomPaperSize { get; set; }
        public int Copies { get; set; }
        public bool Collate { get; set; }
        public bool ScaleToFit { get; set; }
        public bool FitToPage { get; set; }
        public bool PrintHiddenSlides { get; set; }
        public bool PrintComments { get; set; }
        public bool PrintInkAnnotations { get; set; }
        public bool FrameSlides { get; set; }
        public bool HighQuality { get; set; }
        public PageRange PageRange { get; set; }
        public string PrinterName { get; set; }
        public bool UseActivePrinter { get; set; }
        public bool PrintToFile { get; set; }
        public string OutputFileName { get; set; }

        public PrintSettings()
        {
            PrintWhat = PrintWhat.Slides;
            ColorMode = PrintColorMode.Color;
            Quality = PrintQuality.High;
            Order = PrintOrder.SlideOrder;
            PaperSize = PaperSize.Custom;
            PaperOrientation = PaperOrientation.Landscape;
            PaperSource = PaperSource.Automatic;
            Copies = 1;
            Collate = true;
            ScaleToFit = false;
            FitToPage = false;
            PrintHiddenSlides = false;
            PrintComments = false;
            PrintInkAnnotations = false;
            FrameSlides = false;
            HighQuality = false;
            PageRange = new PageRange();
            PrinterName = string.Empty;
            UseActivePrinter = true;
            PrintToFile = false;
            OutputFileName = string.Empty;
        }

        public PrintSettings Clone()
        {
            return new PrintSettings
            {
                PrintWhat = this.PrintWhat,
                ColorMode = this.ColorMode,
                Quality = this.Quality,
                Order = this.Order,
                PaperSize = this.PaperSize,
                PaperOrientation = this.PaperOrientation,
                PaperSource = this.PaperSource,
                CustomPaperSize = this.CustomPaperSize,
                Copies = this.Copies,
                Collate = this.Collate,
                ScaleToFit = this.ScaleToFit,
                FitToPage = this.FitToPage,
                PrintHiddenSlides = this.PrintHiddenSlides,
                PrintComments = this.PrintComments,
                PrintInkAnnotations = this.PrintInkAnnotations,
                FrameSlides = this.FrameSlides,
                HighQuality = this.HighQuality,
                PageRange = this.PageRange.Clone(),
                PrinterName = this.PrinterName,
                UseActivePrinter = this.UseActivePrinter,
                PrintToFile = this.PrintToFile,
                OutputFileName = this.OutputFileName
            };
        }
    }

    public enum PrintWhat
    {
        Slides,
        Handouts,
        Notes,
        Outline,
        BuildSlides
    }

    public enum PrintColorMode
    {
        Color,
        Grayscale,
        PureBlackAndWhite
    }

    public enum PrintQuality
    {
        Draft,
        Low,
        Medium,
        High
    }

    public enum PrintOrder
    {
        SlideOrder,
        HorizontalFirst,
        HorizontalFirstReversed,
        VerticalFirst,
        VerticalFirstReversed
    }

    public enum PaperSize
    {
        Custom,
        Letter,
        Legal,
        A4,
        A3,
        B4,
        B5,
        Envelope,
        Banner
    }

    public enum PaperOrientation
    {
        Landscape,
        Portrait
    }

    public enum PaperSource
    {
        Automatic,
        Manual,
        Envelope,
        Cassette
    }

    public class PageRange
    {
        public bool IsAllPages { get; set; }
        public string Range { get; set; }
        public List<int> SpecificPages { get; set; }

        public PageRange()
        {
            IsAllPages = true;
            Range = string.Empty;
            SpecificPages = new List<int>();
        }

        public PageRange Clone()
        {
            return new PageRange
            {
                IsAllPages = this.IsAllPages,
                Range = this.Range,
                SpecificPages = new List<int>(this.SpecificPages)
            };
        }
    }

    public class SlideShowSettings
    {
        public SlideShowType Type { get; set; }
        public bool IsLoopUntilEscaped { get; set; }
        public bool IsKioskMode { get; set; }
        public bool IsFullScreen { get; set; }
        public bool IsBrowsedByIndividual { get; set; }
        public int? CustomShowId { get; set; }
        public int StartSlideIndex { get; set; }
        public int EndSlideIndex { get; set; }
        public bool ShowNarration { get; set; }
        public bool ShowAnimation { get; set; }
        public bool UseTimings { get; set; }
        public bool IsPortrait { get; set; }
        public PenColor PenColor { get; set; }
        public bool IsEraserEnabled { get; set; }
        public bool Is激光笔 { get; set; }
        public SlideShowPointerType PointerType { get; set; }
        public bool IsZoomEnabled { get; set; }
        public bool IsHideCursor { get; set; }
        public double PointerSize { get; set; }

        public SlideShowSettings()
        {
            Type = SlideShowType.PresentedBySpeaker;
            IsLoopUntilEscaped = false;
            IsKioskMode = false;
            IsFullScreen = true;
            IsBrowsedByIndividual = false;
            StartSlideIndex = 0;
            EndSlideIndex = -1;
            ShowNarration = true;
            ShowAnimation = true;
            UseTimings = true;
            IsPortrait = false;
            PenColor = new PenColor();
            IsEraserEnabled = false;
            Is激光笔 = false;
            PointerType = SlideShowPointerType.Arrow;
            IsZoomEnabled = true;
            IsHideCursor = false;
            PointerSize = 5;
        }

        public SlideShowSettings Clone()
        {
            return new SlideShowSettings
            {
                Type = this.Type,
                IsLoopUntilEscaped = this.IsLoopUntilEscaped,
                IsKioskMode = this.IsKioskMode,
                IsFullScreen = this.IsFullScreen,
                IsBrowsedByIndividual = this.IsBrowsedByIndividual,
                CustomShowId = this.CustomShowId,
                StartSlideIndex = this.StartSlideIndex,
                EndSlideIndex = this.EndSlideIndex,
                ShowNarration = this.ShowNarration,
                ShowAnimation = this.ShowAnimation,
                UseTimings = this.UseTimings,
                IsPortrait = this.IsPortrait,
                PenColor = this.PenColor.Clone(),
                IsEraserEnabled = this.IsEraserEnabled,
                Is激光笔 = this.Is激光笔,
                PointerType = this.PointerType,
                IsZoomEnabled = this.IsZoomEnabled,
                IsHideCursor = this.IsHideCursor,
                PointerSize = this.PointerSize
            };
        }
    }

    public enum SlideShowType
    {
        PresentedBySpeaker,
        BrowsedByIndividual,
        BrowsedAtKiosk
    }

    public class PenColor
    {
        public string Color { get; set; }
        public double Transparency { get; set; }

        public PenColor()
        {
            Color = "#FF0000";
            Transparency = 0;
        }

        public PenColor Clone()
        {
            return new PenColor
            {
                Color = this.Color,
                Transparency = this.Transparency
            };
        }
    }

    public enum SlideShowPointerType
    {
        Arrow,
        Pen,
        Eraser,
        Highlighter,
        Laser
    }

    public class ExportSettings
    {
        public ExportFormat Format { get; set; }
        public string Quality { get; set; }
        public string ColorMode { get; set; }
        public bool IncludeHiddenSlides { get; set; }
        public bool EmbedFonts { get; set; }
        public bool EmbedImages { get; set; }
        public bool OptimizeImages { get; set; }
        public int ImageQuality { get; set; }
        public bool IncludeComments { get; set; }
        public bool IncludeNotes { get; set; }
        public bool IncludeSlideNumbers { get; set; }
        public bool FitToPage { get; set; }
        public bool FrameSlides { get; set; }
        public SlideRange SlideRange { get; set; }
        public string CustomProperties { get; set; }
        public Dictionary<string, string> Metadata { get; set; }

        public ExportSettings()
        {
            Format = ExportFormat.PPTX;
            Quality = "High";
            ColorMode = "Color";
            IncludeHiddenSlides = false;
            EmbedFonts = true;
            EmbedImages = true;
            OptimizeImages = true;
            ImageQuality = 85;
            IncludeComments = false;
            IncludeNotes = false;
            IncludeSlideNumbers = true;
            FitToPage = false;
            FrameSlides = false;
            SlideRange = new SlideRange();
            CustomProperties = string.Empty;
            Metadata = new Dictionary<string, string>();
        }

        public ExportSettings Clone()
        {
            return new ExportSettings
            {
                Format = this.Format,
                Quality = this.Quality,
                ColorMode = this.ColorMode,
                IncludeHiddenSlides = this.IncludeHiddenSlides,
                EmbedFonts = this.EmbedFonts,
                EmbedImages = this.EmbedImages,
                OptimizeImages = this.OptimizeImages,
                ImageQuality = this.ImageQuality,
                IncludeComments = this.IncludeComments,
                IncludeNotes = this.IncludeNotes,
                IncludeSlideNumbers = this.IncludeSlideNumbers,
                FitToPage = this.FitToPage,
                FrameSlides = this.FrameSlides,
                SlideRange = this.SlideRange.Clone(),
                CustomProperties = this.CustomProperties,
                Metadata = new Dictionary<string, string>(this.Metadata)
            };
        }
    }

    public enum ExportFormat
    {
        PPTX,
        PPT,
        PDF,
        XPS,
        RTF,
        GIF,
        JPG,
        PNG,
        BMP,
        TIFF,
        EMF,
        WMF,
        MPEG4,
        WMV,
        AVI,
        HTML
    }

    public class SlideRange
    {
        public bool IsAllSlides { get; set; }
        public List<int> SlideIndices { get; set; }
        public string CustomRange { get; set; }

        public SlideRange()
        {
            IsAllSlides = true;
            SlideIndices = new List<int>();
            CustomRange = string.Empty;
        }

        public SlideRange Clone()
        {
            return new SlideRange
            {
                IsAllSlides = this.IsAllSlides,
                SlideIndices = new List<int>(this.SlideIndices),
                CustomRange = this.CustomRange
            };
        }
    }

    public class PresentationPackage
    {
        public PresentationDocument Document { get; set; }
        public List<Media> Media { get; set; }
        public List<CustomShow> CustomShows { get; set; }
        public List<Reviewer> Reviewers { get; set; }
        public List<Comment> Comments { get; set; }
        public List<InkAnnotation> InkAnnotations { get; set; }
        public PrintSettings PrintSettings { get; set; }
        public SlideShowSettings SlideShowSettings { get; set; }
        public ExportSettings ExportSettings { get; set; }
        public Dictionary<string, byte[]> EmbeddedFiles { get; set; }
        public Dictionary<string, string> CustomProperties { get; set; }

        public PresentationPackage()
        {
            Document = new PresentationDocument();
            Media = new List<Media>();
            CustomShows = new List<CustomShow>();
            Reviewers = new List<Reviewer>();
            Comments = new List<Comment>();
            InkAnnotations = new List<InkAnnotation>();
            PrintSettings = new PrintSettings();
            SlideShowSettings = new SlideShowSettings();
            ExportSettings = new ExportSettings();
            EmbeddedFiles = new Dictionary<string, byte[]>();
            CustomProperties = new Dictionary<string, string>();
        }

        public PresentationPackage Clone()
        {
            return new PresentationPackage
            {
                Document = this.Document.Clone(),
                Media = this.Media.Select(m => m.Clone()).ToList(),
                CustomShows = this.CustomShows.Select(c => c.Clone()).ToList(),
                Reviewers = this.Reviewers.Select(r => r.Clone()).ToList(),
                Comments = this.Comments.Select(c => c.Clone()).ToList(),
                InkAnnotations = this.InkAnnotations.Select(i => i.Clone()).ToList(),
                PrintSettings = this.PrintSettings.Clone(),
                SlideShowSettings = this.SlideShowSettings.Clone(),
                ExportSettings = this.ExportSettings.Clone(),
                EmbeddedFiles = new Dictionary<string, byte[]>(this.EmbeddedFiles),
                CustomProperties = new Dictionary<string, string>(this.CustomProperties)
            };
        }
    }
}

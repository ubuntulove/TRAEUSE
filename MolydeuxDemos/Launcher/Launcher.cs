using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MolydeuxLauncher
{
    public class MainWindow : Window
    {
        private TabControl tabs = new TabControl();
        private string baseDir = AppDomain.CurrentDomain.BaseDirectory;

        public MainWindow()
        {
            Width = 1100; Height = 750;
            Title = "Molydeux Demos Launcher";
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Background = new SolidColorBrush(Color.FromRgb(15, 18, 28));
            Content = tabs;

            var existing = DemoCatalog.All.Where(e => !e.IsNew).ToList();
            var fresh = DemoCatalog.All.Where(e => e.IsNew).ToList();

            tabs.Items.Add(MakeTab($"Existing Molydeux Tweets ({existing.Count})", existing, "Original tweets by @petermolydeux, realized as playable mini-games."));
            tabs.Items.Add(MakeTab($"New Inspired Concepts ({fresh.Count})", fresh, "Fresh Molydeux-style game ideas - original 'what if' pitches in the same spirit, also realized as playable demos."));

            // Status bar at bottom
            var status = new TextBlock
            {
                Text = $"Total: {DemoCatalog.All.Count} demos  |  Tip: build the whole solution first, then click Play. ESC to quit.",
                Foreground = Brushes.LightGray,
                FontSize = 12,
                Margin = new Thickness(12, 4, 12, 8),
            };
            var outer = new Grid();
            outer.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            outer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            Grid.SetRow(tabs, 0);
            Grid.SetRow(status, 1);
            outer.Children.Add(tabs);
            outer.Children.Add(status);
            Content = outer;

            KeyDown += (s, e) => { if (e.Key == Key.Escape) Close(); };
        }

        private TabItem MakeTab(string header, List<DemoEntry> entries, string blurb)
        {
            var scroll = new ScrollViewer { VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Padding = new Thickness(8) };
            var container = new StackPanel();

            container.Children.Add(new TextBlock
            {
                Text = header,
                FontSize = 20,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                Margin = new Thickness(4, 8, 4, 4),
            });
            container.Children.Add(new TextBlock
            {
                Text = blurb,
                FontSize = 12,
                Foreground = Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(4, 0, 4, 12),
                MaxWidth = 1000,
            });

            // Group by template
            var groups = entries.GroupBy(e => e.Template).OrderBy(g => g.Key);
            foreach (var g in groups)
            {
                var info = TemplateInfo(g.Key);
                var groupHeader = new Border
                {
                    Background = new SolidColorBrush(ParseHex(info.Item2)),
                    CornerRadius = new CornerRadius(4),
                    Margin = new Thickness(4, 8, 4, 4),
                    Padding = new Thickness(8, 4, 8, 4),
                    Child = new TextBlock
                    {
                        Text = $"{info.Item1} ({g.Count()})  -  {g.Key}",
                        FontSize = 13,
                        FontWeight = FontWeights.Bold,
                        Foreground = Brushes.Black,
                    },
                };
                container.Children.Add(groupHeader);

                var wrap = new WrapPanel { Margin = new Thickness(4) };
                foreach (var entry in g.OrderBy(e => e.Id))
                    wrap.Children.Add(MakeCard(entry, info.Item2));
                container.Children.Add(wrap);
            }

            scroll.Content = container;
            return new TabItem { Header = header, Content = scroll, Foreground = Brushes.White };
        }

        private Border MakeCard(DemoEntry entry, string accentHex)
        {
            var accent = ParseHex(accentHex);
            var panel = new StackPanel { Width = 300, Margin = new Thickness(4) };

            panel.Children.Add(new TextBlock
            {
                Text = entry.Title,
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Foreground = Brushes.White,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2),
            });
            panel.Children.Add(new TextBlock
            {
                Text = entry.Instructions,
                FontSize = 11,
                Foreground = Brushes.LightGray,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(2, 4, 2, 4),
                MaxHeight = 80,
            });

            var badge = new Border
            {
                Background = new SolidColorBrush(accent),
                CornerRadius = new CornerRadius(3),
                Padding = new Thickness(6, 1, 6, 1),
                Margin = new Thickness(2, 2, 0, 4),
                Child = new TextBlock
                {
                    Text = entry.Template,
                    FontSize = 10,
                    Foreground = Brushes.Black,
                    FontWeight = FontWeights.Bold,
                },
            };
            panel.Children.Add(badge);

            var play = new Button
            {
                Content = "Play",
                Padding = new Thickness(12, 4, 12, 4),
                Margin = new Thickness(2),
                HorizontalAlignment = HorizontalAlignment.Left,
                Tag = entry,
            };
            play.Click += OnPlayClick;
            panel.Children.Add(play);

            return new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(28, 32, 44)),
                BorderBrush = new SolidColorBrush(accent),
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(6),
                Padding = new Thickness(8),
                Margin = new Thickness(3),
                Child = panel,
            };
        }

        private void OnPlayClick(object sender, RoutedEventArgs e)
        {
            if (!(sender is Button b) || !(b.Tag is DemoEntry entry)) return;
            string exe = FindExe(entry.ExeName);
            if (exe == null)
            {
                MessageBox.Show(
                    $"Could not find the compiled executable for '{entry.ExeName}'.\n\n" +
                    "Build the whole solution in Visual Studio first (Build > Build Solution),\n" +
                    "then click Play again.",
                    "Demo not built",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(exe),
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to launch: " + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string FindExe(string name)
        {
            // Candidate paths - covers per-project output and shared output layouts,
            // and both Debug and Release configurations, for .NET 6 Windows desktop.
            string[] relPaths = new[]
            {
                name + ".exe",
                name + ".dll",
                Path.Combine("..", name, "bin", "Debug", "net6.0-windows", name + ".exe"),
                Path.Combine("..", name, "bin", "Debug", "net6.0-windows", name + ".dll"),
                Path.Combine("..", name, "bin", "Release", "net6.0-windows", name + ".exe"),
                Path.Combine("..", name, "bin", "Release", "net6.0-windows", name + ".dll"),
                Path.Combine("..", "Demos", name, "bin", "Debug", "net6.0-windows", name + ".exe"),
                Path.Combine("..", "Demos", name, "bin", "Debug", "net6.0-windows", name + ".dll"),
                Path.Combine("..", "Demos", name, "bin", "Release", "net6.0-windows", name + ".exe"),
                Path.Combine("..", "Demos", name, "bin", "Release", "net6.0-windows", name + ".dll"),
            };
            foreach (var rel in relPaths)
            {
                var full = Path.GetFullPath(Path.Combine(baseDir, rel));
                if (File.Exists(full)) return full;
            }
            return null;
        }

        private static (string, string) TemplateInfo(string tpl)
        {
            switch (tpl)
            {
                case "CatchGame": return ("Catch", "#D4A017");
                case "DodgeGame": return ("Dodge", "#CD5C5C");
                case "FollowGame": return ("Follow", "#32CD32");
                case "ClickGame": return ("Click", "#FF69B4");
                case "TypeGame": return ("Type", "#9370DB");
                case "ProtectGame": return ("Protect", "#7CFC00");
                case "MoveGame": return ("Move", "#00CED1");
                case "StillGame": return ("Still", "#A9A9A9");
                case "RhythmGame": return ("Rhythm", "#FF8C00");
                case "IdleGame": return ("Idle", "#6495ED");
                default: return ("Other", "#888888");
            }
        }

        private static Color ParseHex(string hex)
        {
            string h = hex.StartsWith("#") ? hex.Substring(1) : hex;
            byte r = Convert.ToByte(h.Substring(0, 2), 16);
            byte g = Convert.ToByte(h.Substring(2, 2), 16);
            byte b = Convert.ToByte(h.Substring(4, 2), 16);
            return Color.FromRgb(r, g, b);
        }

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }
}

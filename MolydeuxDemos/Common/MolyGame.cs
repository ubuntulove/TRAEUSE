using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Molydeux.Common
{
    public abstract class MolyGame : Window
    {
        protected Canvas canvas = new Canvas();
        protected DispatcherTimer timer = new DispatcherTimer();
        protected TextBlock hud = new TextBlock();
        protected TextBlock instructionsLabel = new TextBlock();

        protected int score = 0;
        protected double elapsed = 0;
        protected Random rng = new Random();

        public string DemoTitle { get => base.Title; set { base.Title = value; } }
        public string Instructions { get => instructionsLabel.Text; set { instructionsLabel.Text = value; } }
        public string WinText = "You Win!";
        public string LoseText = "Game Over";
        public Brush GameBackground { set => canvas.Background = value; }

        protected MolyGame()
        {
            Width = 900; Height = 650;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            Content = canvas;
            canvas.Background = new SolidColorBrush(Color.FromRgb(15, 18, 28));
            Title = "Molydeux Demo";

            hud.Foreground = Brushes.White;
            hud.FontSize = 15; hud.FontWeight = FontWeights.Bold;
            Canvas.SetLeft(hud, 12); Canvas.SetTop(hud, 8);
            canvas.Children.Add(hud);

            instructionsLabel.Foreground = Brushes.LightGray;
            instructionsLabel.FontSize = 12;
            instructionsLabel.TextWrapping = TextWrapping.Wrap;
            instructionsLabel.MaxWidth = 700;
            Canvas.SetLeft(instructionsLabel, 12); Canvas.SetTop(instructionsLabel, 32);
            canvas.Children.Add(instructionsLabel);

            timer.Interval = TimeSpan.FromMilliseconds(16);
            timer.Tick += (s, e) => { elapsed += 0.016; Update(); UpdateHud(); };
            Loaded += (s, e) => { Setup(); timer.Start(); };
            KeyDown += OnKey;
            KeyUp += OnKeyUp;
            MouseDown += OnMouse;
            MouseMove += OnMouseMove;
        }

        protected abstract void Setup();
        protected virtual void Update() {}
        protected virtual void UpdateHud() => hud.Text = $"Score: {score}  Time: {elapsed:F1}s  [R: restart, ESC: quit]";
        protected virtual void OnKey(KeyEventArgs e)
        {
            if (e.Key == Key.Escape) Close();
            else if (e.Key == Key.R) Restart();
        }
        protected virtual void OnKeyUp(KeyEventArgs e) {}
        protected virtual void OnMouse(MouseButtonEventArgs e) {}
        protected virtual void OnMouseMove(MouseEventArgs e) {}

        protected Ellipse AddEllipse(double x, double y, double r, Brush fill)
        {
            var el = new Ellipse { Width = r * 2, Height = r * 2, Fill = fill };
            Canvas.SetLeft(el, x - r); Canvas.SetTop(el, y - r);
            canvas.Children.Add(el);
            return el;
        }
        protected Rectangle AddRect(double x, double y, double w, double h, Brush fill)
        {
            var r = new Rectangle { Width = w, Height = h, Fill = fill };
            Canvas.SetLeft(r, x); Canvas.SetTop(r, y);
            canvas.Children.Add(r);
            return r;
        }
        protected Polygon AddPolygon(Point[] pts, Brush fill)
        {
            var p = new Polygon { Fill = fill };
            foreach (var pt in pts) p.Points.Add(pt);
            canvas.Children.Add(p);
            return p;
        }
        protected TextBlock AddLabel(double x, double y, string text, double size = 14, Brush color = null)
        {
            var t = new TextBlock { Text = text, FontSize = size, Foreground = color ?? Brushes.White };
            Canvas.SetLeft(t, x); Canvas.SetTop(t, y);
            canvas.Children.Add(t);
            return t;
        }
        protected double Dist(Point a, Point b) => Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));

        protected void Win(string msg = null)
        {
            timer.Stop();
            var r = MessageBox.Show((msg ?? WinText) + $"\n\nScore: {score}\nTime: {elapsed:F1}s\n\nRestart?", "Victory", MessageBoxButton.YesNo, MessageBoxImage.Information);
            if (r == MessageBoxResult.Yes) Restart(); else Close();
        }
        protected void Lose(string msg = null)
        {
            timer.Stop();
            var r = MessageBox.Show((msg ?? LoseText) + $"\n\nScore: {score}\nTime: {elapsed:F1}s\n\nRestart?", "Defeat", MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (r == MessageBoxResult.Yes) Restart(); else Close();
        }
        protected void Restart()
        {
            var kids = canvas.Children.OfType<UIElement>().Skip(2).ToList();
            foreach (var k in kids) canvas.Children.Remove(k);
            score = 0; elapsed = 0;
            Setup();
            timer.Start();
        }
    }

    // ----- Template: CatchGame -----
    // Items fall from top. Click to collect. Win at `need` caught, lose if `miss` missed.
    public class CatchGame : MolyGame
    {
        protected int need = 10, miss = 3, missed = 0;
        protected double fallSpeed = 220;
        protected Brush itemBrush = Brushes.Gold;
        protected double itemRadius = 18;
        protected string itemShape = "circle";
        protected List<(UIElement el, double x, double y, double r)> items = new();

        protected override void Setup() { missed = 0; SpawnItem(); SpawnItem(); }
        protected virtual void SpawnItem()
        {
            var x = rng.NextDouble() * 820 + 40;
            UIElement el;
            if (itemShape == "square")
                el = AddRect(x - itemRadius, -itemRadius * 2, itemRadius * 2, itemRadius * 2, itemBrush);
            else if (itemShape == "triangle")
            {
                var pts = new[] { new Point(x, -itemRadius*2), new Point(x - itemRadius, 0), new Point(x + itemRadius, 0) };
                el = AddPolygon(pts, itemBrush);
            }
            else
                el = AddEllipse(x, 0, itemRadius, itemBrush);
            items.Add((el, x, 0, itemRadius));
        }
        protected override void Update()
        {
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var (el, x, y, r) = items[i];
                y += fallSpeed * 0.016;
                Canvas.SetTop(el, y - r);
                items[i] = (el, x, y, r);
                if (y > 620)
                {
                    canvas.Children.Remove(el); items.RemoveAt(i); missed++;
                    if (missed >= miss) { Lose(); return; }
                }
            }
            if (items.Count < 4 && rng.NextDouble() < 0.04) SpawnItem();
            if (score >= need) Win();
        }
        protected override void UpdateHud() => hud.Text = $"Caught: {score}/{need}  Missed: {missed}/{miss}  Time: {elapsed:F1}s  [R: restart, ESC: quit]";
        protected override void OnMouse(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(canvas);
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var (el, x, y, r) = items[i];
                if (Dist(p, new Point(x, y)) < r + 14)
                {
                    canvas.Children.Remove(el); items.RemoveAt(i); score++; return;
                }
            }
        }
    }

    // ----- Template: DodgeGame -----
    // Items fall; cursor IS the player. Survive `timeSec`. 3 lives.
    public class DodgeGame : MolyGame
    {
        protected double timeSec = 30;
        protected int lives = 3;
        protected double fallSpeed = 220;
        protected Brush itemBrush = Brushes.OrangeRed;
        protected double itemRadius = 18;
        protected string itemShape = "circle";
        protected List<(UIElement el, double x, double y, double r)> items = new();
        protected Ellipse player;
        protected double invuln = 0;

        protected override void Setup()
        {
            player = AddEllipse(450, 500, 14, Brushes.Cyan);
            for (int i = 0; i < 3; i++) SpawnItem();
        }
        protected void SpawnItem()
        {
            var x = rng.NextDouble() * 820 + 40;
            UIElement el;
            if (itemShape == "square")
                el = AddRect(x - itemRadius, -itemRadius * 2, itemRadius * 2, itemRadius * 2, itemBrush);
            else if (itemShape == "triangle")
            {
                var pts = new[] { new Point(x, -itemRadius*2), new Point(x - itemRadius, 0), new Point(x + itemRadius, 0) };
                el = AddPolygon(pts, itemBrush);
            }
            else
                el = AddEllipse(x, 0, itemRadius, itemBrush);
            items.Add((el, x, 0, itemRadius));
        }
        protected override void Update()
        {
            if (invuln > 0) invuln -= 0.016;
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var (el, x, y, r) = items[i];
                y += fallSpeed * 0.016;
                Canvas.SetTop(el, y - r);
                items[i] = (el, x, y, r);
                if (y > 620) { canvas.Children.Remove(el); items.RemoveAt(i); }
                else if (invuln <= 0 && player != null)
                {
                    var pp = new Point(Canvas.GetLeft(player) + 14, Canvas.GetTop(player) + 14);
                    if (Dist(pp, new Point(x, y)) < r + 14)
                    {
                        canvas.Children.Remove(el); items.RemoveAt(i);
                        lives--; invuln = 1.0;
                        if (lives <= 0) { Lose(); return; }
                    }
                }
            }
            if (items.Count < 4 && rng.NextDouble() < 0.05) SpawnItem();
            if (elapsed >= timeSec) Win();
        }
        protected override void UpdateHud() => hud.Text = $"Lives: {lives}  Survived: {elapsed:F1}s/{timeSec}s  [Mouse to dodge, R: restart, ESC: quit]";
        protected override void OnMouseMove(MouseEventArgs e)
        {
            var p = e.GetPosition(canvas);
            if (player != null) { Canvas.SetLeft(player, p.X - 14); Canvas.SetTop(player, p.Y - 14); }
        }
    }

    // ----- Template: FollowGame -----
    // A target moves; player (cursor) must stay within `radius`. Survive `timeSec`.
    public class FollowGame : MolyGame
    {
        protected double timeSec = 25;
        protected double radius = 90;
        protected double targetX = 450, targetY = 325;
        protected double targetVx = 120, targetVy = 90;
        protected Brush targetBrush = Brushes.LimeGreen;
        protected double targetRadius = 22;
        protected double outTime = 0;
        protected Ellipse target;

        protected override void Setup()
        {
            targetX = 450; targetY = 325;
            targetVx = (rng.NextDouble() * 2 - 1) * 200 + 100;
            targetVy = (rng.NextDouble() * 2 - 1) * 200 + 100;
            target = AddEllipse(targetX, targetY, targetRadius, targetBrush);
            outTime = 0;
        }
        protected override void Update()
        {
            targetX += targetVx * 0.016; targetY += targetVy * 0.016;
            if (targetX < 40 || targetX > 860) targetVx *= -1;
            if (targetY < 60 || targetY > 600) targetVy *= -1;
            Canvas.SetLeft(target, targetX - targetRadius);
            Canvas.SetTop(target, targetY - targetRadius);
            var p = Mouse.GetPosition(canvas);
            if (Dist(p, new Point(targetX, targetY)) < radius + targetRadius)
            {
                outTime = 0;
                if (target != null) target.Fill = targetBrush;
            }
            else
            {
                outTime += 0.016;
                if (target != null) target.Fill = Brushes.Red;
                if (outTime > 1.5) Lose();
            }
            if (elapsed >= timeSec) Win();
        }
        protected override void UpdateHud() => hud.Text = $"Stay close to the target! Time: {elapsed:F1}s/{timeSec}s  [Mouse to follow, R: restart, ESC: quit]";
    }

    // ----- Template: ClickGame -----
    // Click a moving target `need` times before timeSec runs out.
    public class ClickGame : MolyGame
    {
        protected int need = 10;
        protected double timeSec = 20;
        protected double targetRadius = 22;
        protected Brush targetBrush = Brushes.DeepPink;
        protected double targetX, targetY;
        protected Ellipse target;
        protected double moveTimer = 0;

        protected override void Setup()
        {
            targetX = 450; targetY = 325;
            target = AddEllipse(targetX, targetY, targetRadius, targetBrush);
        }
        protected override void Update()
        {
            moveTimer += 0.016;
            if (moveTimer > 0.6)
            {
                targetX = rng.NextDouble() * 800 + 50;
                targetY = rng.NextDouble() * 500 + 80;
                Canvas.SetLeft(target, targetX - targetRadius);
                Canvas.SetTop(target, targetY - targetRadius);
                moveTimer = 0;
            }
            if (elapsed >= timeSec) Lose();
        }
        protected override void UpdateHud() => hud.Text = $"Clicks: {score}/{need}  Time: {elapsed:F1}s/{timeSec}s  [R: restart, ESC: quit]";
        protected override void OnMouse(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(canvas);
            if (Dist(p, new Point(targetX, targetY)) < targetRadius + 8)
            {
                score++;
                targetX = rng.NextDouble() * 800 + 50;
                targetY = rng.NextDouble() * 500 + 80;
                Canvas.SetLeft(target, targetX - targetRadius);
                Canvas.SetTop(target, targetY - targetRadius);
                moveTimer = 0;
                if (score >= need) Win();
            }
        }
    }

    // ----- Template: TypeGame -----
    // Type shown word; need N words; timeSec total.
    public class TypeGame : MolyGame
    {
        protected int need = 8;
        protected double timeSec = 60;
        protected string[] words = { "love", "rage", "fear", "hope", "joy", "loss", "dream", "tears", "echo", "ghost", "void", "light", "soul", "time", "born", "dust" };
        protected string current = "";
        protected string typed = "";
        protected TextBlock wordLabel;

        protected override void Setup()
        {
            wordLabel = AddLabel(300, 280, "", 48, Brushes.Yellow);
            NextWord();
        }
        protected void NextWord()
        {
            current = words[rng.Next(words.Length)];
            typed = "";
            if (wordLabel != null) wordLabel.Text = current;
        }
        protected override void UpdateHud() => hud.Text = $"Typed: {score}/{need}  Time: {elapsed:F1}s/{timeSec}s  [Type letters, Backspace, R: restart, ESC: quit]";
        protected override void Update()
        {
            if (elapsed >= timeSec) Lose();
        }
        protected override void OnKey(KeyEventArgs e)
        {
            base.OnKey(e);
            if (e.Key == Key.Back && typed.Length > 0) typed = typed.Substring(0, typed.Length - 1);
            else
            {
                var ch = KeyToChar(e.Key);
                if (ch != null)
                {
                    typed += ch;
                    if (typed == current) { score++; if (score >= need) Win(); else NextWord(); }
                    else if (!current.StartsWith(typed)) typed = "";
                }
            }
            if (wordLabel != null) wordLabel.Text = typed.Length > 0 ? typed + "_" : current;
        }
        protected string KeyToChar(Key k)
        {
            if (k >= Key.A && k <= Key.Z) return ((char)('a' + (k - Key.A))).ToString();
            return null;
        }
    }

    // ----- Template: ProtectGame -----
    // Enemies approach center; click to destroy. Survive `timeSec`.
    public class ProtectGame : MolyGame
    {
        protected double timeSec = 30;
        protected int spawnEvery = 50;
        protected int tick = 0;
        protected double centerX = 450, centerY = 325;
        protected double coreRadius = 30;
        protected Ellipse core;
        protected List<(Ellipse e, double x, double y, double speed)> enemies = new();

        protected override void Setup()
        {
            core = AddEllipse(centerX, centerY, coreRadius, Brushes.LawnGreen);
            tick = 0; enemies.Clear();
        }
        protected override void Update()
        {
            tick++;
            if (tick >= spawnEvery) { SpawnEnemy(); tick = 0; }
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var (e, x, y, sp) = enemies[i];
                var dx = centerX - x, dy = centerY - y;
                var d = Math.Sqrt(dx * dx + dy * dy);
                if (d < 1) { Lose(); return; }
                var nx = x + dx / d * sp * 0.016;
                var ny = y + dy / d * sp * 0.016;
                Canvas.SetLeft(e, nx - e.Width / 2);
                Canvas.SetTop(e, ny - e.Height / 2);
                enemies[i] = (e, nx, ny, sp);
            }
            if (elapsed >= timeSec) Win();
        }
        protected void SpawnEnemy()
        {
            var angle = rng.NextDouble() * Math.PI * 2;
            var x = centerX + Math.Cos(angle) * 500;
            var y = centerY + Math.Sin(angle) * 350;
            var e = AddEllipse(x, y, 14, Brushes.Red);
            enemies.Add((e, x, y, 60 + rng.NextDouble() * 40));
        }
        protected override void UpdateHud() => hud.Text = $"Defend the core! Killed: {score}  Survive: {elapsed:F1}s/{timeSec}s  [R: restart, ESC: quit]";
        protected override void OnMouse(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(canvas);
            for (int i = enemies.Count - 1; i >= 0; i--)
            {
                var (el, x, y, sp) = enemies[i];
                if (Dist(p, new Point(x, y)) < 20)
                {
                    canvas.Children.Remove(el); enemies.RemoveAt(i); score++; return;
                }
            }
        }
    }

    // ----- Template: MoveGame -----
    // Arrows/WASD to move; collect `need` items.
    public class MoveGame : MolyGame
    {
        protected int need = 8;
        protected double px = 450, py = 325;
        protected double pradius = 14;
        protected double speed = 220;
        protected Brush playerBrush = Brushes.Cyan;
        protected Brush itemBrush = Brushes.Gold;
        protected double itemRadius = 14;
        protected bool wallsKill = false;
        protected Ellipse player;
        protected List<(Ellipse e, double x, double y)> items = new();
        protected bool left, right, up, down;

        protected override void Setup()
        {
            px = 450; py = 325;
            player = AddEllipse(px, py, pradius, playerBrush);
            for (int i = 0; i < 5; i++) SpawnItem();
            left = right = up = down = false;
        }
        protected void SpawnItem()
        {
            var x = rng.NextDouble() * 800 + 40;
            var y = rng.NextDouble() * 480 + 80;
            var e = AddEllipse(x, y, itemRadius, itemBrush);
            items.Add((e, x, y));
        }
        protected override void Update()
        {
            double dx = 0, dy = 0;
            if (left) dx -= 1;
            if (right) dx += 1;
            if (up) dy -= 1;
            if (down) dy += 1;
            if (dx != 0 || dy != 0) { var len = Math.Sqrt(dx * dx + dy * dy); dx /= len; dy /= len; }
            px += dx * speed * 0.016;
            py += dy * speed * 0.016;
            if (wallsKill)
            {
                if (px < pradius || px > 900 - pradius || py < pradius || py > 650 - pradius) { Lose(); return; }
            }
            else
            {
                px = Math.Max(pradius, Math.Min(900 - pradius, px));
                py = Math.Max(pradius, Math.Min(650 - pradius, py));
            }
            Canvas.SetLeft(player, px - pradius);
            Canvas.SetTop(player, py - pradius);
            for (int i = items.Count - 1; i >= 0; i--)
            {
                var (e, x, y) = items[i];
                if (Dist(new Point(px, py), new Point(x, y)) < pradius + itemRadius)
                {
                    canvas.Children.Remove(e); items.RemoveAt(i); score++;
                    if (score >= need) { Win(); return; }
                    SpawnItem();
                }
            }
        }
        protected override void UpdateHud() => hud.Text = $"Collected: {score}/{need}  Time: {elapsed:F1}s  [Arrows/WASD to move, R: restart, ESC: quit]";
        protected override void OnKey(KeyEventArgs e)
        {
            base.OnKey(e);
            if (e.Key == Key.Left || e.Key == Key.A) left = true;
            if (e.Key == Key.Right || e.Key == Key.D) right = true;
            if (e.Key == Key.Up || e.Key == Key.W) up = true;
            if (e.Key == Key.Down || e.Key == Key.S) down = true;
        }
        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.A) left = false;
            if (e.Key == Key.Right || e.Key == Key.D) right = false;
            if (e.Key == Key.Up || e.Key == Key.W) up = false;
            if (e.Key == Key.Down || e.Key == Key.S) down = false;
        }
    }

    // ----- Template: StillGame -----
    // Don't move the mouse for `timeSec` seconds. Any movement restarts timer.
    public class StillGame : MolyGame
    {
        protected double timeSec = 10;
        protected double stillTime = 0;
        protected Point lastPos;
        protected Ellipse indicator;

        protected override void Setup()
        {
            lastPos = Mouse.GetPosition(canvas);
            indicator = AddEllipse(450, 325, 60, Brushes.Red);
            stillTime = 0;
        }
        protected override void Update()
        {
            var p = Mouse.GetPosition(canvas);
            if (Dist(p, lastPos) > 1)
            {
                stillTime = 0;
                if (indicator != null) indicator.Fill = Brushes.Red;
            }
            else
            {
                stillTime += 0.016;
                if (indicator != null) indicator.Fill = Brushes.LimeGreen;
                if (stillTime >= timeSec) Win();
            }
            lastPos = p;
            if (indicator != null)
            {
                indicator.Width = 60 + 40 * (stillTime / timeSec);
                indicator.Height = indicator.Width;
                Canvas.SetLeft(indicator, 450 - indicator.Width / 2);
                Canvas.SetTop(indicator, 325 - indicator.Height / 2);
            }
        }
        protected override void UpdateHud() => hud.Text = $"Stay perfectly still! Still: {stillTime:F1}s/{timeSec}s  [R: restart, ESC: quit]";
    }

    // ----- Template: RhythmGame -----
    // Shrinking ring; click when it matches inner circle. Need N hits, M misses = lose.
    public class RhythmGame : MolyGame
    {
        protected int need = 8, misses = 0, maxMisses = 3;
        protected double outerR = 80, innerR = 20;
        protected double ringR = 80;
        protected Ellipse ring, inner;

        protected override void Setup()
        {
            inner = AddEllipse(450, 325, innerR, Brushes.Yellow);
            ring = AddEllipse(450, 325, ringR, null);
            ring.Stroke = Brushes.White; ring.StrokeThickness = 3;
            ringR = 80; misses = 0;
        }
        protected override void Update()
        {
            ringR -= 30 * 0.016;
            if (ringR < innerR)
            {
                misses++; ringR = 80;
                if (misses >= maxMisses) Lose();
            }
            ring.Width = ringR * 2; ring.Height = ringR * 2;
            Canvas.SetLeft(ring, 450 - ringR);
            Canvas.SetTop(ring, 325 - ringR);
        }
        protected override void UpdateHud() => hud.Text = $"Click when ring matches inner circle! Hits: {score}/{need}  Misses: {misses}/{maxMisses}  [R: restart, ESC: quit]";
        protected override void OnMouse(MouseButtonEventArgs e)
        {
            var p = e.GetPosition(canvas);
            if (Dist(p, new Point(450, 325)) < ringR + 10)
            {
                if (ringR < innerR + 12 && ringR > innerR - 4)
                {
                    score++; ringR = 80;
                    if (score >= need) Win();
                }
                else
                {
                    misses++; ringR = 80;
                    if (misses >= maxMisses) Lose();
                }
            }
        }
    }

    // ----- Template: IdleGame -----
    // Game plays itself. Don't interfere. Win after timeSec.
    public class IdleGame : MolyGame
    {
        protected double timeSec = 20;
        protected List<Ellipse> dots = new();
        protected override void Setup()
        {
            for (int i = 0; i < 12; i++)
            {
                var d = AddEllipse(rng.Next(50, 850), rng.Next(80, 580), 10, Brushes.CornflowerBlue);
                dots.Add(d);
            }
        }
        protected override void Update()
        {
            foreach (var d in dots)
            {
                var x = Canvas.GetLeft(d) + 10;
                var y = Canvas.GetTop(d) + 8;
                if (x > 880) x = 0;
                if (y > 600) y = 80;
                Canvas.SetLeft(d, x);
                Canvas.SetTop(d, y);
            }
            if (elapsed >= timeSec) Win();
        }
        protected override void OnMouse(MouseButtonEventArgs e) { Lose(); }
        protected override void OnKey(KeyEventArgs e) { if (e.Key == Key.R) Restart(); else if (e.Key == Key.Escape) Close(); else Lose(); }
        protected override void UpdateHud() => hud.Text = $"Don't touch anything! Wait: {elapsed:F1}s/{timeSec}s  [R: restart, ESC: quit]";
    }
}

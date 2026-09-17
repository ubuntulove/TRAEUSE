#!/usr/bin/env python3
"""Generate a single WPF .sln with one .csproj per Molydeux tweet demo.

Each demo is a small WPF project that references a shared Common library
containing the MolyGame base class and ~10 reusable playable templates
(Catch, Dodge, Follow, Click, Type, Protect, Move, Still, Rhythm, Idle).
"""
import os
import re

ROOT = "/workspace/MolydeuxDemos"
DEMOS_DIR = ROOT + "/Demos"
COMMON_DIR = ROOT + "/Common"

# ----------------------------------------------------------------------------
# Common library
# ----------------------------------------------------------------------------

COMMON_CSPROJ = """<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net6.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <EnableDefaultApplicationDefinition>false</EnableDefaultApplicationDefinition>
    <RootNamespace>Molydeux.Common</RootNamespace>
    <AssemblyName>Molydeux.Common</AssemblyName>
  </PropertyGroup>

</Project>
"""

MOLY_GAME_CS = r'''using System;
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
'''

# ----------------------------------------------------------------------------
# Demo project templates
# ----------------------------------------------------------------------------

def make_csproj(proj_name):
    return f"""<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>WinExe</OutputType>
    <TargetFramework>net6.0-windows</TargetFramework>
    <UseWPF>true</UseWPF>
    <EnableDefaultApplicationDefinition>false</EnableDefaultApplicationDefinition>
    <AssemblyName>{proj_name}</AssemblyName>
    <RootNamespace>{proj_name}</RootNamespace>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\\..\\Common\\Common.csproj" />
  </ItemGroup>

</Project>
"""


def esc(s):
    """Escape a string for use inside a C# double-quoted string literal."""
    return s.replace("\\", "\\\\").replace("\"", "\\\"")


def make_game_cs(ns_name, base_class, title, instr, win, lose, bg_rgb, extras):
    return f"""using Molydeux.Common;
using System.Windows;
using System.Windows.Media;

namespace {ns_name}
{{
    public class MainWindow : {base_class}
    {{
        public MainWindow()
        {{
            DemoTitle = "{esc(title)}";
            Instructions = "{esc(instr)}";
            WinText = "{esc(win)}";
            LoseText = "{esc(lose)}";
            GameBackground = new SolidColorBrush(Color.FromRgb({bg_rgb}));
            {extras}
        }}

        [STAThread]
        public static int Main() => new Application().Run(new MainWindow());
    }}
}}
"""

# ----------------------------------------------------------------------------
# Demo definitions
# Each tuple: (id, slug, template, short_title, instr, win, lose, bg_rgb, extras)
# ----------------------------------------------------------------------------

DEMOS = [
    (1, "SupermarketArm", "CatchGame", "Supermarket Arm",
     "Your left arm IS a supermarket! Catch falling groceries before they hit the floor.",
     "Arm fully stocked - checkout complete!", "Too many groceries spilled on the floor!",
     "20,25,35",
     'itemBrush=Brushes.Gold; itemShape="square"; need=10; miss=3; fallSpeed=220;'),
    (2, "IAmTheGun", "FollowGame", "I Am The Gun",
     "You ARE a gun. Keep close to your owner - if you wander too far for too long, you'll be lost forever.",
     "Faithful gun reunited with its owner!", "You lost your owner in the crowd!",
     "30,30,40",
     'timeSec=18; radius=110; targetBrush=Brushes.Silver; targetRadius=18;'),
    (3, "BrokenGame", "ClickGame", "Broken Game",
     "Dying broke the game. Normally you'd have to fix it in ANOTHER game, but here: click the broken bits to patch it.",
     "Game patched! Continue from save.", "Unfixable corruption - install disc error!",
     "10,5,15",
     'need=8; timeSec=25; targetBrush=Brushes.OrangeRed;'),
    (4, "SharedArms", "MoveGame", "Shared Arms Co-op",
     "Co-op: you and a friend control one arm each of the protagonist. Imagine your friend is here too - use arrows + WASD.",
     "Both arms in sync - reached the goal!", "Arms tangled - cooperation failed!",
     "20,30,40",
     'need=8; speed=240; playerBrush=Brushes.Cyan; itemBrush=Brushes.MediumPurple;'),
    (5, "GameWithoutPlayer", "IdleGame", "Game Without A Player",
     "All games require someone to play them. This game goes against that very concept. Don't interfere.",
     "The game played itself - perfection!", "You interfered with the experiment!",
     "5,5,5",
     'timeSec=15;'),
    (6, "BeTheBall", "DodgeGame", "Be The Ball",
     "In this football game you don't control the players - you ARE the ball. Avoid the boots!",
     "You survived the entire match!", "Punted into the stands!",
     "30,55,30",
     'timeSec=22; itemBrush=Brushes.SaddleBrown; itemShape="square"; itemRadius=22; fallSpeed=260;'),
    (7, "OneHourBehind", "ClickGame", "One Hour Behind",
     "You're 1 hour behind reality. Wait for the enemy to finish their phone call, THEN click the booth.",
     "Time finally caught up with you.", "Too soon! You're still 1 hour behind!",
     "30,20,30",
     'need=6; timeSec=30;'),
    (8, "NoGraphics", "IdleGame", "No Graphics",
     "Is it possible to make a videogame with NO graphics? Find out. Just wait.",
     "You experienced pure nothingness.", "You interfered with the void!",
     "0,0,0",
     'timeSec=10;'),
    (9, "MagicCarpet", "MoveGame", "Magic Carpet",
     "All these years and still nobody dared to make another magic carpet game. Ride through the skies.",
     "Magic carpet ride complete!", "You fell off the carpet!",
     "30,40,80",
     'need=10; speed=260; playerBrush=Brushes.MediumPurple; itemBrush=Brushes.White; itemRadius=12;'),
    (10, "BeAStatue", "StillGame", "Be A Statue",
     "Play as a statue. Statues don't move. Don't move.",
     "Statue perfection - utterly still!", "Statues don't move - you moved!",
     "40,40,50",
     'timeSec=12;'),
    (11, "NolanNorthVoices", "TypeGame", "Nolan North, Voice Actor",
     "Play as Nolan North recording lines. Each enemy needs a unique voice - type the word on screen with flair!",
     "All lines delivered - performance of a lifetime!", "Director fired you - fluffed your lines!",
     "25,20,35",
     'need=10; timeSec=75;'),
    (12, "WorldIsHeartbeat", "IdleGame", "World Is Your Heartbeat",
     "'The world around you is your heartbeat.' (Phil Collins). Watch the world pulse with your heart. Don't interfere.",
     "Your heart kept beating - the world lived!", "Your heart skipped - the world shattered!",
     "60,15,30",
     'timeSec=18;'),
    (13, "RealOxygen", "StillGame", "A Real Sense Of Oxygen",
     "Games always show oxygen as a draining bar. Imagine a real sense of oxygen around you - breathe slowly, don't move.",
     "You felt the oxygen - pure and present!", "You suffocated on panic!",
     "10,30,50",
     'timeSec=15;'),
    (14, "LoveAsEnemy", "DodgeGame", "Love Is The Enemy",
     "Love in games is always emotion. Here, love is a physical enemy. Dodge the kisses of doom.",
     "You resisted love - cold and alone!", "Love struck - game over!",
     "60,20,40",
     'timeSec=25; itemBrush=Brushes.HotPink; itemShape="triangle"; fallSpeed=240;'),
    (15, "CunningAnt", "ClickGame", "The Cunning Ant",
     "Why must bosses always be MASSIVE monsters? Fight a very cunning ant. It's tiny and fast.",
     "Ant squashed - victory!", "The ant outwitted you!",
     "30,30,30",
     'need=10; timeSec=20; targetRadius=10; targetBrush=Brushes.Black;'),
    (16, "TextWorldAdventure", "TypeGame", "Text World Adventure",
     "Imagine a 3D adventure where all objects are made of text. Type the words you encounter to absorb them.",
     "You absorbed the lexicon!", "The words escaped you!",
     "20,20,30",
     'need=10; timeSec=75;'),
    (17, "GameoverLocked", "ClickGame", "Gameover, Can't Continue",
     "The game begins with a gameover screen. You can't continue. But maybe... click enough times?",
     "You found the secret reset code!", "Game over. You can't continue.",
     "5,5,5",
     'need=12; timeSec=30; targetBrush=Brushes.DarkRed;'),
    (18, "PassTheController", "IdleGame", "Pass The Controller",
     "Play for 5 minutes, then send to a friend who plays 5 more. Imagine the handoff. Just wait your turn.",
     "Your 5 minutes are up - pass it on!", "You hogged the controller!",
     "30,25,40",
     'timeSec=15;'),
    (19, "MicrowaveHead", "CatchGame", "Microwave For A Head",
     "You have a microwave for a head. Grab enemies and toss them in to cook them.",
     "All enemies thoroughly cooked!", "Too many raw enemies slipped past!",
     "40,30,20",
     'itemBrush=Brushes.OrangeRed; itemShape="square"; need=10; miss=3;'),
    (20, "FourPairsSpecs", "ClickGame", "Four Pairs Of Specs",
     "The game comes with 4 pairs of specs. Each one shows the world differently. Find the right lens for each target.",
     "You saw through every lens!", "Blind without the right spectacles!",
     "30,30,50",
     'need=8; timeSec=30;'),
    (21, "GhostOfZombie", "MoveGame", "Ghost Of A Zombie",
     "You're the ghost of a zombie, reliving memory flashbacks of the person you were. Collect your memories.",
     "All memories recovered - peace at last!", "Your memories scattered to the wind!",
     "20,15,25",
     'need=10; playerBrush=Brushes.PaleGreen; itemBrush=Brushes.Lavender;'),
    (22, "NoGuidePossible", "MoveGame", "Impossible To Guide",
     "Design an adventure game that is impossible to write a gamefaqs guide for. The items move randomly every restart.",
     "You beat the unguideable!", "Lost forever - no guide can save you!",
     "25,20,30",
     'need=12; speed=300;'),
    (23, "CharityEntity", "IdleGame", "Charity Single Entity",
     "Everyone pays to control one single entity. The further it gets, the more cash raised. Don't touch - watch it go.",
     "Massive funds raised!", "You hijacked the charity run!",
     "20,40,30",
     'timeSec=20;'),
    (24, "WW2Game", "DodgeGame", "World War 2, Unlike Any Other",
     "A WW2 game unlike any other. You're a single civilian dodging falling bombs.",
     "You survived the bombing raid!", "Caught in the blast!",
     "30,20,20",
     'timeSec=25; itemBrush=Brushes.DarkGray; itemShape="triangle"; fallSpeed=300;'),
    (25, "TransparentSpy", "DodgeGame", "Transparent World Spy",
     "You're a spy in a world where the entire environment is transparent - you can't hide. Dodge the searchlights.",
     "Evaded detection!", "Spotted in the transparent world!",
     "20,40,40",
     'timeSec=22; itemBrush=Brushes.White; itemShape="square";'),
    (26, "GameTurnsBoard", "IdleGame", "Videogame Becomes Boardgame",
     "A videogame that turns into a boardgame halfway through. Wait for the transformation.",
     "Boardgame mode unlocked!", "You skipped the transformation!",
     "30,25,40",
     'timeSec=12;'),
    (27, "KinectStillHorror", "StillGame", "Kinect Survival Horror",
     "Survival horror where at key moments you must stay ABSOLUTELY still to avoid being found.",
     "You weren't discovered - escaped alive!", "The monster heard you!",
     "5,5,15",
     'timeSec=14;'),
    (28, "NoAnimation", "IdleGame", "No Animation",
     "Is it possible to make a videogame with NO animation? Does input always require animation? Wait and see.",
     "Static victory!", "You disturbed the stillness!",
     "10,10,10",
     'timeSec=10;'),
    (29, "TextYourCharacter", "ClickGame", "Text Your Character",
     "Text your character and watch them receive it. They might refuse to look. Click to nudge them to check their phone.",
     "They finally read your text!", "They refused to look - heartbroken!",
     "30,25,35",
     'need=8; timeSec=30;'),
    (30, "BeACoin", "MoveGame", "Be A Coin",
     "Play as a coin being passed around. Travel as far as possible by placing yourself where you'll be picked up.",
     "You traveled the whole economy!", "You got lost in a sofa cushion!",
     "50,40,15",
     'need=12; playerBrush=Brushes.Goldenrod; itemBrush=Brushes.LimeGreen;'),
    (31, "LifeSupportRat", "ProtectGame", "Life Support Machine / Diseased Rat",
     "By day you're a life support machine. By night you turn into a diseased rat. Defend the patient!",
     "The patient survived the night!", "The patient flatlined!",
     "15,30,15",
     'timeSec=28;'),
    (32, "VerticalOnly", "MoveGame", "Horizontal Is Impossible",
     "In this world horizontal is impossible - you're forever falling or attached to verticals. Climb up only.",
     "Reached the top of the vertical world!", "You fell into the horizontal void!",
     "20,20,30",
     'need=8; speed=180; playerBrush=Brushes.White;'),
    (33, "OnlyHug", "FollowGame", "Only Hug",
     "In a world where all anyone can do is hurt each other, your only ability is to hug. Reach people to hug them.",
     "You hugged the hurting world better!", "You couldn't reach them in time!",
     "40,20,40",
     'timeSec=20; radius=60; targetBrush=Brushes.Pink;'),
    (34, "NeverEndingPlane", "IdleGame", "Never Ending Plane",
     "You awake on a plane and walk down the aisle, but it keeps looping. What's going on? Just observe the loop.",
     "You accepted the loop - peace!", "You broke the loop - the plane crashed!",
     "20,25,35",
     'timeSec=15;'),
    (35, "ConstantlyFalling", "DodgeGame", "Constantly Falling",
     "An action game where your character is constantly falling. You can hang onto things but only for 1-3 seconds.",
     "You survived the endless fall!", "You fell too fast - splat!",
     "30,30,50",
     'timeSec=22; itemBrush=Brushes.Brown; itemShape="square"; fallSpeed=180;'),
    (36, "PlayAsCrate", "CatchGame", "Play As A Crate",
     "So many games have crates, but in none can you actually PLAY as a crate. Catch falling items into your crate-top!",
     "Crate fully loaded!", "Your crate tipped over!",
     "55,40,20",
     'itemBrush=Brushes.Sienna; itemShape="square"; need=10; miss=3;'),
    (37, "SesameActors", "TypeGame", "Sesame Street: The Actors",
     "Don't play the Sesame Street characters - play the actors INSIDE the costumes. Type the cues the director shouts.",
     "Curtain call - bravo!", "Forgotten lines - booed off stage!",
     "40,30,50",
     'need=10; timeSec=70;'),
    (38, "TouringClown", "MoveGame", "Touring Clown To Chaplin",
     "You're a touring clown - start in the streets, work through circuses, become a film star like Chaplin. Collect applause!",
     "From street to silver screen - a star!", "The audience hated you - tomatoes!",
     "30,25,40",
     'need=12; playerBrush=Brushes.White; itemBrush=Brushes.Gold;'),
    (39, "BabyOnChest", "DodgeGame", "Baby Strapped To Chest",
     "Action game: female protagonist with a baby strapped to her chest. Must not wake the baby. Move smoothly to avoid waking.",
     "Baby slept through the whole mission!", "The baby woke up crying!",
     "30,25,30",
     'timeSec=22; itemBrush=Brushes.LightPink; fallSpeed=180;'),
    (40, "VelcroMan", "CatchGame", "Velcro Man",
     "You're VELCRO MAN. Stuff sticks to you automatically - but only soft items. Catch the right ones!",
     "Fully velcroed!", "You grabbed non-velcro junk!",
     "30,40,30",
     'itemBrush=Brushes.Yellow; itemShape="triangle"; need=10; miss=3;'),
    (41, "SandpaperMan", "DodgeGame", "Sandpaper Man",
     "SANDPAPER MAN. Everything abrasive grates on you. Dodge the rough patches!",
     "Smooth sailing - sanded to perfection!", "You got grated - ouch!",
     "50,30,15",
     'timeSec=22; itemBrush=Brushes.Peru; itemShape="square"; fallSpeed=260;'),
    (42, "BauerGameShow", "ClickGame", "Bauer In A Gameshow",
     "You're like Jack Bauer, but to kill enemies you're transported to a gameshow. Correct answers = kills. Click fast!",
     "Show won - season finale kill count reached!", "Wrong answers - the terrorists win!",
     "20,20,30",
     'need=10; timeSec=18;'),
    (43, "ReflectionsShowSolutions", "ClickGame", "Reflections Show Solutions",
     "Can't get past a door? Make a puddle and look into it for the answer. Click the puddles to see solutions.",
     "All puzzles solved via reflection!", "You stared too long at your own reflection!",
     "20,30,40",
     'need=8; timeSec=30; targetBrush=Brushes.LightBlue;'),
    (44, "LookAway", "StillGame", "Press X To Look Away",
     "'Press X to look AWAY from the big exciting event about to happen.' Don't look. Stay still.",
     "You missed the spectacle - perfect!", "You peeked - the event overwhelmed you!",
     "30,15,15",
     'timeSec=12;'),
    (45, "WasCPU", "ClickGame", "Was It Your Friend, Or The CPU?",
     "Play against a friend online... afterwards you find it was actually the CPU. Click the imposters!",
     "You unmasked the CPU impostor!", "The CPU fooled you completely!",
     "15,15,30",
     'need=10; timeSec=22;'),
    (46, "WarToWoo", "ClickGame", "Explosive War Turns To Woo",
     "A fast paced explosive war game that halfway through suddenly becomes: woo a lady. Click hearts!",
     "She said yes - mission accomplished!", "She left you standing at the altar!",
     "30,20,40",
     'need=10; timeSec=25; targetBrush=Brushes.Pink;'),
    (47, "SaveByQTE", "ClickGame", "Save Your Game Via QTE",
     "What if the way to save your game was through a QTE? Click fast to write the save file before it corrupts!",
     "Game saved successfully!", "Save file corrupted!",
     "20,30,20",
     'need=12; timeSec=15;'),
    (48, "ViolinTears", "CatchGame", "Violin Tears As Currency",
     "Play sad music on your violin to suck in people's tears, then trade them for weapons. Catch the tears!",
     "Wallet full of tears - gun shop opens!", "Not enough tears for a single bullet!",
     "30,25,35",
     'itemBrush=Brushes.LightBlue; itemShape="triangle"; need=12; miss=4;'),
    (49, "MagnetVictims", "FollowGame", "War Hero With Magnetic Victims",
     "A war hero whose victims stick to him like magnets - Katamari but with a message. Keep close to gather them all.",
     "You carried your burden home.", "You dropped your burden - they scattered!",
     "30,20,25",
     'timeSec=22; radius=120; targetBrush=Brushes.DarkRed;'),
    (50, "FirstVideogame", "ClickGame", "Invent The First Videogame",
     "Go back in time and create the first ever videogame. Click the pixels into existence!",
     "You birthed an entire medium!", "The pixels wouldn't cooperate!",
     "20,20,20",
     'need=15; timeSec=30;'),
    (51, "HundredChildren", "ClickGame", "100 Children, 1 Bullet Each",
     "You have 100 children. Each time you fire a bullet, a child dies. Be precise - one shot, one target.",
     "All targets neutralized - minimal losses!", "Too many wasted bullets - too few children!",
     "30,15,15",
     'need=6; timeSec=20; targetBrush=Brushes.DarkRed;'),
    (52, "OneMinuteStreamed", "IdleGame", "One Minute, One Person, Streamed",
     "A game that can only be played 1 minute at a time by 1 different person worldwide, streamed 24/7. Wait your minute.",
     "Your minute was perfect!", "You overran your minute!",
     "20,20,40",
     'timeSec=12;'),
    (53, "PauseIsWeapon", "ClickGame", "Pause Button Is A Weapon",
     "What if the pause button was a weapon? Click pause at exactly the right moment to freeze enemies.",
     "All enemies frozen in time!", "You paused at the wrong moment!",
     "30,15,40",
     'need=10; timeSec=22;'),
    (54, "CommunicateWithSand", "IdleGame", "Communicate With Sand",
     "Communication with sand. Listen to the grains. They have stories. Don't disturb them.",
     "The sand spoke to you!", "You scattered the conversation!",
     "50,40,25",
     'timeSec=15;'),
    (55, "TutorialAfterCredits", "IdleGame", "Tutorial After Credits",
     "I'd like to make a game where the tutorial comes up AFTER the credits. Wait through them all.",
     "Tutorial unlocked - finally you can play!", "You skipped the credits - no tutorial for you!",
     "10,10,20",
     'timeSec=18;'),
    (56, "SportsIntervalsWoo", "ClickGame", "Sports + Woo + Killer",
     "A sports game where during intervals you must woo a special lady while tracking her killer stalker. Multitask!",
     "Won the match AND caught the stalker!", "Distracted by love - the killer escaped!",
     "30,25,35",
     'need=10; timeSec=25;'),
    (57, "BeACloud", "IdleGame", "Be A Cloud",
     "When people talk about cloud gaming, I'm not thinking about the internet. Imagine how it must feel to BE a cloud.",
     "You rained at the perfect moment!", "You evaporated from pressure!",
     "100,100,120",
     'timeSec=15;'),
    (58, "BoyAndHands", "ClickGame", "Boy And His Big Hands",
     "Control a boy playing with action figures - you only control his big hands and his mouth for SFX. Click the toys!",
     "The boy's playtime was epic!", "The boy got bored of you!",
     "30,25,35",
     'need=10; timeSec=22;'),
    (59, "VendingMachine", "ClickGame", "Play As A Vending Machine",
     "Could playing as a vending machine be fun? Dispense the right items to the right customers!",
     "All customers served - profit!", "Wrong items dispensed - complaints!",
     "30,30,40",
     'need=10; timeSec=25;'),
    (60, "ColorInEnemy", "ClickGame", "Color In Your Enemy",
     "A fighting game where the mechanic is to 'colour in' parts of your enemy to create weak spots. Click to color!",
     "All weak spots revealed - KO!", "You colored outside the lines!",
     "20,20,40",
     'need=12; timeSec=25;'),
    (61, "BabyCycle5s", "IdleGame", "Baby Born Cycle In 5 Seconds",
     "Play as a baby, grow up, give birth, play as the newborn - all in 5 second cycles. Watch the generations roll.",
     "Witnessed infinite generations!", "You interrupted the cycle of life!",
     "40,30,50",
     'timeSec=15;'),
    (62, "TVAndEmotion", "ClickGame", "Character On TV, Emotion Off-Screen",
     "Control a character on the TV screen, but on ANOTHER screen you see their emotion. Click to manage both.",
     "Emotions perfectly managed!", "Emotional breakdown on screen two!",
     "20,20,40",
     'need=10; timeSec=25;'),
    (63, "MagicianNoPowers", "ClickGame", "Magician With No Powers",
     "Play as a magician with no magic powers. Entertain goblins to escape a castle by deceiving them with illusions.",
     "Goblins amazed - you escaped the castle!", "Goblins saw through your tricks - devoured!",
     "20,15,40",
     'need=10; timeSec=25;'),
    (64, "RepeatingStairs", "IdleGame", "Repeating Staircase",
     "Love repeating staircases like in M64 and FF7 - disappointed when they end. Here, they never end. Keep climbing.",
     "You accepted eternal ascent!", "You stopped - the staircase collapsed!",
     "30,25,30",
     'timeSec=20;'),
    (65, "PianoHeadEnemies", "RhythmGame", "Piano-Keyboard Head Enemies",
     "Enemies have dirty piano keyboards for heads. Kill them by cleaning the piano without making a noise. Tap on beat!",
     "Pianos cleaned in perfect silence!", "You made a noise - they devoured you!",
     "30,25,20",
     'need=10;'),
    (66, "HalfManHalfTree", "MoveGame", "Half Man, Half Tree",
     "Half man, half tree. You can only fight when in soil and you constantly grow. Collect soil patches to grow stronger.",
     "Fully grown - you became a forest!", "You withered without soil!",
     "20,40,15",
     'need=10; playerBrush=Brushes.Olive; itemBrush=Brushes.Sienna;'),
    (67, "WhiteBoxDreams", "MoveGame", "White Box Open World",
     "Open world game set in a white box. At night you dream and place dream elements into the blank world. Collect them.",
     "Your dreams filled the blank world!", "Your dreams turned to nightmares!",
     "240,240,240",
     'need=10; playerBrush=Brushes.Black; itemBrush=Brushes.MediumPurple;'),
    (68, "TransformersAbuse", "DodgeGame", "Transformers In Abusive Home",
     "A Transformers game where you control a boy playing with figures in a house filled with family abuse. Dodge the yelling.",
     "You escaped into imagination!", "The violence found you in your fantasy!",
     "20,15,25",
     'timeSec=22; itemBrush=Brushes.DarkRed; itemShape="triangle";'),
    (69, "FireworksCalm", "DodgeGame", "Fireworks Calm / Weapon",
     "Stealth game: the only way to calm down is by setting off fireworks - which can also be weapons. Dodge debris!",
     "You kept calm through the storm!", "Panic overwhelmed you!",
     "30,20,40",
     'timeSec=22; itemBrush=Brushes.Gold; itemShape="triangle"; fallSpeed=280;'),
    (70, "PsychopathArms", "DodgeGame", "FPS With Psychopath Arms",
     "FPS where your arms are controlled by a psychopath firing at innocents. Turn away from them and run. Dodge the bullets!",
     "You escaped the psychopath's arms!", "The arms gunned down innocents through you!",
     "30,15,20",
     'timeSec=22; itemBrush=Brushes.Yellow; fallSpeed=320;'),
    (71, "OneRoomAdventure", "ClickGame", "One Room Adventure",
     "An entire adventure game based in ONE room. Movies have done it, why not games? Click everything to find every secret.",
     "Every secret of the room uncovered!", "The room kept its mysteries!",
     "30,25,30",
     'need=12; timeSec=30;'),
    (72, "InsideOut", "IdleGame", "Game Turns Itself Inside Out",
     "We've had games that go sideways, up, down, rotate - but what about a game that turns itself inside out? Watch it transform.",
     "You witnessed the inversion!", "You disturbed the inversion!",
     "20,20,40",
     'timeSec=12;'),
    (73, "StretchSpell", "ClickGame", "Stretch And Squeeze Spells",
     "Imagine being able to create a spell and stretch and squeeze it with your fingertips. Click to mold the spell to shape.",
     "Spell perfectly molded - cast!", "Spell slipped through your fingers!",
     "30,20,50",
     'need=10; timeSec=22;'),
    (74, "AliveHelicopterNuke", "DodgeGame", "Alive Helicopter With A Nuke",
     "You're a small boy with an alive remote control helicopter - your friend. Then you discover a nuclear missile inside it.",
     "You disarmed the missile safely!", "The nuke detonated - end of the city!",
     "20,20,30",
     'timeSec=22; itemBrush=Brushes.OrangeRed; itemShape="triangle"; fallSpeed=280;'),
    (75, "WashingMachineBody", "CatchGame", "Body Is A Washing Machine",
     "Your body is a washing machine - you must wash your enemies before you can kill them. Catch them and rinse!",
     "All enemies washed and rinsed!", "Too many dirty enemies slipped away!",
     "20,40,60",
     'itemBrush=Brushes.LightBlue; itemShape="square"; need=10; miss=3;'),
    (76, "DeathCutscene", "IdleGame", "1 Minute Cutscene Per Kill",
     "An action game where upon killing each enemy you see a 1 minute cutscene flashing through their entire life. Watch respectfully.",
     "You mourned every life you took.", "You skipped the mourning - disrespectful!",
     "20,15,25",
     'timeSec=15;'),
    (77, "ScarecrowOneBird", "StillGame", "Scarecrow With One Bird",
     "You are a scarecrow in a world with just 1 bird. Stay still - if you scare the bird away, you've failed.",
     "The bird rested on your shoulder - peace.", "You flinched - the bird flew away!",
     "60,50,15",
     'timeSec=12;'),
    (78, "HDvsSD", "ClickGame", "HD And SD Both Vital",
     "A game where you can switch between HD and SD - both play a vital part in solving puzzles. Click HD/SD targets!",
     "Both resolutions solved every puzzle!", "Wrong resolution - puzzle unsolvable!",
     "20,30,50",
     'need=10; timeSec=25;'),
    (79, "PramInBattle", "DodgeGame", "Pram In The Battlezone",
     "Battle out of a battlezone while maneuvering a pram with a baby. Armor the pram and use it as cover. Dodge incoming fire!",
     "Baby extracted from the warzone alive!", "The pram took a direct hit!",
     "30,25,20",
     'timeSec=24; itemBrush=Brushes.DarkGray; fallSpeed=260;'),
    (80, "GraveyardSilence", "IdleGame", "Graveyard Of The Fallen",
     "A war game where at the end you walk through a graveyard looking at every headstone of those you killed - then 1 minute silence.",
     "You honored every one of your victims.", "You broke the silence - dishonorable!",
     "30,25,25",
     'timeSec=20;'),
    (81, "EightPersonPerspective", "IdleGame", "Eight Person Perspective (EPS)",
     "The world's first EPS - 8 peoples' visions overlaid on top of each other. Watch the chaos of 8 perspectives.",
     "You witnessed the 8-fold vision!", "You closed your eyes - missed the moment!",
     "20,20,30",
     'timeSec=12;'),
    (82, "PlayAsTheRoad", "DodgeGame", "Play As The Road",
     "Have you ever played a racing game and wanted to play as the road rather than the cars? You ARE the road. Dodge the cars!",
     "You survived the entire race day!", "A car drove right through you!",
     "20,25,30",
     'timeSec=22; itemBrush=Brushes.Silver; itemShape="square"; fallSpeed=320;'),
    (83, "FindEmotions", "MoveGame", "Find 120 Emotions",
     "Start with no emotion. Find and obtain them, then use them to find more. Collect 8 distinct emotions to win.",
     "You became fully emotional!", "You remained emotionally dead!",
     "30,20,40",
     'need=8; playerBrush=Brushes.White; itemBrush=Brushes.HotPink;'),
    (84, "EmotionBlob", "MoveGame", "Force Emotions On Your Blob",
     "Like 'A Boy and his Blob' but instead of beans you force emotions on him. Collect the right emotions to feed your blob.",
     "Your blob became content with its feelings!", "Your blob became emotionally unstable!",
     "20,15,40",
     'need=10; playerBrush=Brushes.PaleVioletRed; itemBrush=Brushes.Gold;'),
    (85, "FaceIsMirror", "IdleGame", "Your Face Is A Mirror",
     "What if your face was a mirror? Reflect the world. Don't move - just reflect.",
     "You reflected the world perfectly!", "You cracked the mirror - bad luck!",
     "40,40,60",
     'timeSec=12;'),
    (86, "HouseOfGuns", "ProtectGame", "House Made Of Guns",
     "You live in a little house made of guns. You need many guns to fight invaders but also need a roof over your children. Defend them!",
     "Family safe and armed!", "Invaders breached the gun-house!",
     "30,25,25",
     'timeSec=30;'),
    (87, "HoldMothersHand", "StillGame", "Hold Mother's Hand",
     "Hold onto your mother's hand. Let go for more than 1 second and it's game over. Don't move the mouse at all.",
     "You held her hand the whole way home!", "You let go - lost in the crowd!",
     "30,25,40",
     'timeSec=12;'),
    (88, "EnticeToSofa", "ClickGame", "Entice People To The Sofa",
     "Like Crazy Taxi but without wheels. Entice as many people as possible to sit on the sofa within the time limit.",
     "The sofa is full - party started!", "Not enough people on the sofa!",
     "30,25,40",
     'need=10; timeSec=22;'),
    (89, "BodyClocks", "ClickGame", "Adjust Visible Body Clocks",
     "What if everyone had visible body clocks above them and you had the power to adjust those clocks? Click to advance them!",
     "All body clocks synchronized!", "Time ran out for everyone!",
     "20,30,40",
     'need=10; timeSec=25;'),
    (90, "SnowmanLovedOne", "DodgeGame", "Snowman Loved One, Hot Body",
     "Your loved one turned into a snowman. Yet your body needs to be as hot as an oven to survive. Dodge the cold snowballs!",
     "You stayed hot enough to survive!", "You froze - your snowman wept!",
     "60,30,30",
     'timeSec=22; itemBrush=Brushes.White; fallSpeed=240;'),
    (91, "ExplosiveTelephone", "ClickGame", "Explosive Telephones In Bodies",
     "Everyone has an explosive telephone in their body. Find out their number to detonate! Click the right numbers.",
     "All targets detonated!", "Wrong number - you blew yourself up!",
     "30,20,30",
     'need=10; timeSec=22;'),
    (92, "TransferFear", "ClickGame", "Extract And Transfer Fear",
     "How to get past a fearless guard? Extract the fear of a pigeon you attacked and transfer it to the guard. Sequence the clicks!",
     "Guard now fears everything - walk past!", "The pigeon pecked you to death!",
     "30,25,20",
     'need=8; timeSec=25;'),
    (93, "OnlyToolIsRuler", "ClickGame", "Your Only Tool Is A Ruler",
     "Designer workshop challenge: make a game where the player's only tool is a ruler. Click to measure precisely!",
     "All measurements perfect!", "Your ruler snapped under pressure!",
     "30,25,40",
     'need=12; timeSec=22;'),
    (94, "KatamariParts", "MoveGame", "Attach Every Enemy Part",
     "Attach a part from EVERY enemy to yourself Katamari-style, building your powers. Collect the parts!",
     "You became a patchwork god!", "You missed too many vital parts!",
     "30,20,30",
     'need=12; playerBrush=Brushes.MediumSlateBlue; itemBrush=Brushes.OrangeRed;'),
    (95, "BearHugOxygen", "FollowGame", "Bear Whose Oxygen Is Hugs",
     "You're a bear whose oxygen comes from hugging people. Problem: hugging people breaks their bones. Stay close to refill!",
     "Oxygen refilled - you didn't break anyone!", "You suffocated - or broke too many bones!",
     "40,25,15",
     'timeSec=20; radius=80; targetBrush=Brushes.Pink;'),
    (96, "TwoHundredSixBones", "IdleGame", "206 Bones Multiplayer",
     "Imagine a 206-player multiplayer game where each person controls a single bone. Imagine the coordination. Just watch.",
     "The body moved in perfect unison!", "The skeleton collapsed - discoordination!",
     "30,30,40",
     'timeSec=15;'),
    (97, "KillForFurniture", "ClickGame", "Every Kill = New Furniture",
     "Want our game to appeal to both sexes: for every kill you get a new piece of furniture to decorate your house. Click enemies!",
     "Your house is fully furnished!", "Your house remains bare and empty!",
     "30,25,40",
     'need=10; timeSec=25;'),
    (98, "BabyOldManLoop", "IdleGame", "Baby To Old Man Loop",
     "You grow from baby to old man and back to baby, looping every 10 seconds. Time small gaps to slip through. Watch the loop.",
     "You accepted the eternal loop!", "You broke the cycle of life!",
     "30,25,35",
     'timeSec=15;'),
    (99, "NoStartNoEnd", "IdleGame", "No Start, No End, Just Middle",
     "What if there was a game that had no start or end, but just a middle? You're already in the middle. Stay there.",
     "You existed in the eternal middle!", "You tried to find an end - there is none!",
     "30,25,30",
     'timeSec=15;'),
    (100, "SingLullabiesHorror", "StillGame", "Sing Lullabies In Survival Horror",
     "Survival horror: sing lullabies to your 2 year old to stop them screaming and giving your position away. Don't move a muscle.",
     "The baby slept - you survived the night!", "The baby cried - the monsters found you!",
     "15,10,20",
     'timeSec=14;'),
    (101, "CupidLasers", "ClickGame", "Cupid's Laser Match",
     "You're Cupid. Find compatible partners - one arrow in each, connected with lasers. If any NPC touches the laser, game over!",
     "All compatible couples matched!", "An NPC walked into your laser - awkward!",
     "30,20,50",
     'need=8; timeSec=25; targetBrush=Brushes.Pink;'),
    (102, "FallThroughFloors", "DodgeGame", "Falling Through Floors",
     "What if falling through floors was a FEATURE not a bug? What lies below? Risk your life to find out. Fall through safely!",
     "You reached the very bottom of the world!", "You fell into the void!",
     "30,20,40",
     'timeSec=22; itemBrush=Brushes.Brown; itemShape="square"; fallSpeed=200;'),
    (103, "ListenForKeywords", "TypeGame", "Listen For Keywords",
     "Go around listening to people, taking key words and transforming them into real objects to help you progress. Type the keywords!",
     "You wove their words into objects!", "You couldn't catch the right words!",
     "25,20,35",
     'need=10; timeSec=70;'),
    (104, "SleepAgeSwap", "IdleGame", "Sleep Cycle Age Swap",
     "Every hour you sleep, your child grows older by a week. Every hour they sleep, you get younger. Watch the impossible math.",
     "You both reached perfect equilibrium!", "The math broke reality!",
     "30,25,40",
     'timeSec=15;'),
    (105, "SlowFrameRate", "IdleGame", "Slow Frame Rate As A Feature",
     "Bring in multiple characters to slow the frame rate down, enabling you to pass obstacles. Watch the slowdown work.",
     "You exploited the lag to pass!", "Too many characters - crash!",
     "20,20,30",
     'timeSec=15;'),
    (106, "PuppetEmotions", "ClickGame", "Steal Puppets To Show Emotion",
     "In a world where people convey feelings only through puppets, steal someone else's puppet to show your emotion. Click puppets!",
     "All emotions stolen and expressed!", "You remained emotionless - puppetless!",
     "30,25,35",
     'need=10; timeSec=25;'),
    (107, "ResistCompleting", "IdleGame", "Resist Completing The Game",
     "What if the aim of the game was to RESIST actually completing it? That kept me up all night. Don't touch anything.",
     "You successfully avoided finishing it!", "You accidentally completed it - damn!",
     "20,20,30",
     'timeSec=15;'),
    (108, "StealAnimationFrames", "ClickGame", "Steal Animation Frames",
     "Steal animation frames from other NPCs' animation cycles and give them to yourself or others to solve puzzles. Click frames!",
     "You assembled a perfect new animation!", "Your stolen frames glitched the world!",
     "30,25,40",
     'need=12; timeSec=25;'),
    (109, "Boss10Minions", "ClickGame", "Boss With 10 Minions",
     "A final boss has 10 minions. Your choice of interactions alters how the final boss battles you. Click each one carefully!",
     "You shaped the final battle!", "You antagonized the wrong minion!",
     "30,20,40",
     'need=10; timeSec=30;'),
    (110, "SandcastleWarGames", "IdleGame", "Sandcastles Like WarGames",
     "Build/destroy sandcastles but discover it's having an impact on the world. Like the movie 'WarGames' but with nature. Observe.",
     "You saw the world change with every castle!", "You built too many - tsunami!",
     "40,40,25",
     'timeSec=15;'),
    (111, "TownOnBody", "ProtectGame", "Town On Your Body",
     "Play a character with a fully functional town on his body. Need to fix your leg? Build and fund a hospital on it. Defend the town!",
     "Your body-town prospered!", "Invaders sacked your body-town!",
     "30,30,40",
     'timeSec=30;'),
    (112, "GrandadSaxaphone", "ClickGame", "Grandad's Saxophone Escape",
     "Play as a grandad. You can only interact with your grandchildren by playing a saxaphone - required to escape a dream castle.",
     "You saxophoned your way out of the dream!", "You hit a sour note - the dream trapped you!",
     "30,25,40",
     'need=10; timeSec=25;'),
    (113, "StepFatherSpace", "IdleGame", "Step Father In Space",
     "Your abusive step father chases you into a space rocket that accidentally blasts off. Just you, an 8 year old, and him. Alone.",
     "You survived the long silence of space!", "The stepfather found you in the void!",
     "10,10,30",
     'timeSec=15;'),
    (114, "BodyIsHourglass", "ClickGame", "Body Is An Hourglass",
     "Your body is essentially an hourglass - an intense acrobatic game where you must make sure sand is always falling. Click to flip!",
     "Sand never stopped - perfect flow!", "The sand ran out - time's up!",
     "50,40,20",
     'need=12; timeSec=22;'),
    (115, "EarthquakePlatformer", "StillGame", "Earthquake Jumps",
     "A platformer where landing on the floor creates earthquakes destroying little tribes. Don't jump. Stay perfectly still.",
     "The tribes survived - you held still!", "You jumped - a tribe was wiped out!",
     "30,25,30",
     'timeSec=14;'),
    (116, "MusicSolvesPuzzles", "TypeGame", "Custom Music Solves Puzzles",
     "Custom music is the key to solving puzzles. Need cure? Classical. Attack? Rock. Fire? A song with the word 'fire' in it. Type!",
     "You orchestrated every solution!", "Your playlist was off-key!",
     "30,25,40",
     'need=10; timeSec=70;'),
    (117, "StickyNotesReality", "TypeGame", "Sticky Notes Become Reality",
     "Write and place sticky notes in a virtual world and what they say becomes reality. Type the words to reshape the world!",
     "Reality rewritten to your liking!", "Your sticky notes contradicted each other!",
     "30,40,25",
     'need=8; timeSec=70;'),
    (118, "GameFallsAsleep", "IdleGame", "Game Falls Asleep If Tedious",
     "What if a game fell asleep if your actions were so tedious and repetitive? Wait... don't bore it.",
     "The game stayed awake for you!", "You bored the game to sleep!",
     "20,20,40",
     'timeSec=15;'),
    (119, "SleepBecomeBed", "IdleGame", "Sleep, Become The Bed",
     "You're in a room with just a bed. When you sleep you become the bed. How does it feel to be a bed? Degrading? Observe.",
     "You accepted your new life as a bed!", "You refused to become furniture!",
     "30,25,35",
     'timeSec=15;'),
    (120, "RobotSerialKiller", "ClickGame", "Robot And The Boy Serial Killer",
     "You play a robot that must entertain a child. As he grows up, the boy becomes a serial killer. Do you stop him or help him? Click decisions!",
     "You shaped the boy's future!", "You failed your programming directive!",
     "20,15,30",
     'need=8; timeSec=25;'),
    (121, "HideAndSeekGhosts", "MoveGame", "Hide And Seek Ghosts",
     "You're a child playing hide and seek. As you hide, the world fast forwards. Everyone has died. Find your friend's ghosts.",
     "All friend ghosts found - peace!", "The ghosts eluded you forever!",
     "20,15,30",
     'need=10; playerBrush=Brushes.PaleVioletRed; itemBrush=Brushes.LightGray;'),
    (122, "ReadGuardsFace", "ClickGame", "Read Guard's Face",
     "Spy on a guard's face from afar and know which route he'll take just by reading his expressions. Click the correct route!",
     "You read every guard perfectly!", "You misread the face - caught!",
     "30,25,35",
     'need=10; timeSec=25;'),
    (123, "PretendBlind", "DodgeGame", "Pretend To Be Blind",
     "Play a character PRETENDING to be blind. Bump into things to not arouse suspicion, but dodge the truly dangerous obstacles.",
     "Nobody suspected you can see!", "Your fake blindness got you killed!",
     "30,25,30",
     'timeSec=22; itemBrush=Brushes.Gray; fallSpeed=200;'),
    (124, "ThreeDayJump", "IdleGame", "Jump With 3 Day Countdown",
     "Imagine pressing jump and having a 3 day countdown until you jump. Search for a gap within 3 days. Wait for the right moment.",
     "The 3 day wait paid off - perfect jump!", "Impatience ruined the jump!",
     "30,25,40",
     'timeSec=15;'),
    (125, "FortressOfPlants", "MoveGame", "Fortress Of Plant Family",
     "Awake in a fortress. Your family are little plants. Escape with all plants - water them and keep them in sunlight. Collect them all!",
     "Every plant family member rescued!", "Your family withered in the dark!",
     "20,40,20",
     'need=10; playerBrush=Brushes.Sienna; itemBrush=Brushes.LimeGreen;'),
    (126, "OneBulletSlowMotion", "ClickGame", "1 Bullet, Slow Motion",
     "You have 1 bullet. Once fired it enters slow motion, taking hours to reach its target. Things you do affect its direction. Steer it!",
     "The bullet found its target!", "The bullet veered off course!",
     "20,20,40",
     'need=8; timeSec=30;'),
    (127, "ToxicPuppetWorld", "ProtectGame", "Toxic Puppet World",
     "A large open world full of toxic poison. Everyone wears gas masks and uses puppets for communication. Defend the puppet-master!",
     "The puppet-master survived!", "The gas seeped through the masks!",
     "20,30,30",
     'timeSec=28;'),
    (128, "TwitterDoorCodes", "IdleGame", "Twitter Door Codes Daily",
     "Adventure game where you need to follow a twitter account that provides door codes every day. Wait for today's code.",
     "Today's code arrived - door unlocked!", "You missed today's tweet - locked out!",
     "30,25,40",
     'timeSec=15;'),
    (129, "TimeRiverFishing", "CatchGame", "Fish From Time Rivers",
     "A fishing game where rivers are different dimensions in time. Reel in an old toy from your childhood. Catch the temporal fish!",
     "You reeled in pieces of your past!", "The river carried away your memories!",
     "20,30,50",
     'itemBrush=Brushes.CornflowerBlue; itemShape="triangle"; need=10; miss=3;'),
    (130, "SuckPolygons1995", "ClickGame", "Suck Polygons To 1995",
     "Suck polygons out of enemies - initially they look next-gen but you can drain them to look like they're from 1995. Click to drain!",
     "All enemies reduced to PS1 graphics!", "You couldn't quite downgrade them!",
     "20,20,30",
     'need=12; timeSec=25;'),
    (131, "ThreePausesGameover", "IdleGame", "3 Pauses And Game Over",
     "3 pauses and it's game over. The game plays door bells and ringtones trying to make you accidentally pause. Don't touch anything!",
     "You never accidentally paused!", "A doorbell tricked you - pause 3, game over!",
     "30,25,40",
     'timeSec=15;'),
    (132, "DemonPsychologist", "ClickGame", "Demon Psychologist Disguise",
     "The world is being taken over by demons. You play a human disguised as the demon psychologist of the demon leader. Choose your words!",
     "You psychoanalyzed the demon leader to death!", "The demons saw through your disguise!",
     "30,15,40",
     'need=8; timeSec=25;'),
    (133, "LegoChildBridges", "MoveGame", "Lego Child Bridges",
     "Your child is made of lego. Detach bricks to make bridges. Keep adding bricks to stop him getting depressed. Collect bricks!",
     "Your child is whole and smiling!", "Your child crumbled - lego depression!",
     "30,25,40",
     'need=12; playerBrush=Brushes.Cyan; itemBrush=Brushes.Red;'),
    (134, "MirrorEnemiesHorror", "ClickGame", "Enemies Only In Mirrors",
     "Survival horror where the enemies can ONLY be seen in mirrors. Dirty mirrors need cleaning first. Click the mirror-images!",
     "All mirror-enemies destroyed!", "An enemy crept up behind you unseen!",
     "15,15,30",
     'need=10; timeSec=25; targetBrush=Brushes.Silver;'),
    (135, "EnemiesUseCheats", "DodgeGame", "Enemies Use Cheat Codes",
     "Imagine a game where the enemies are actually able to use cheat codes! Dodge their god-mode projectiles!",
     "You beat the cheating AIs!", "They no-clipped behind you - dead!",
     "20,15,30",
     'timeSec=22; itemBrush=Brushes.Magenta; fallSpeed=300;'),
    (136, "BulletEvolvesCreature", "DodgeGame", "Bullets Evolve Into Creatures",
     "Every bullet you fire hits the floor and spends 10 minutes evolving into a fast creature that chases you down. Dodge your own bullets!",
     "You survived your own creations!", "Your bullets came back to haunt you!",
     "30,25,20",
     'timeSec=22; itemBrush=Brushes.Gold; fallSpeed=200;'),
    (137, "CorpsesPointExit", "ClickGame", "Corpses Point To Exit",
     "Killing enemies makes them fall to the floor and point towards the exit - that's the only way to find your way out. Click them!",
     "The corpses guided you out!", "You killed in the wrong direction!",
     "30,25,30",
     'need=10; timeSec=25;'),
    (138, "InvisibleWallShooter", "DodgeGame", "Corridor Shooter Invisible Walls",
     "A corridor shooter where all walls are invisible. Constantly worry if that enemy has open space to aim at you. Dodge!",
     "You navigated the invisible maze!", "You walked into an invisible wall - shot!",
     "20,20,30",
     'timeSec=22; itemBrush=Brushes.Red; fallSpeed=260;'),
    (139, "CutUmbilicalCord", "ClickGame", "Cut The Umbilical Cord",
     "Like 'Cut the Rope' but called 'Cut the umbilical cord'. Don't care about giving candy to a frog. Click to cut precisely!",
     "The cord was cut cleanly!", "You severed too much - bleeding!",
     "30,20,40",
     'need=10; timeSec=22;'),
    (140, "EntertainmentUnit", "IdleGame", "Family Entertainment Unit",
     "Control the entertainment unit in a large family home. Choose their games, music, films - all of which influence their future. Watch.",
     "You shaped the family's destiny!", "The family fell apart under your curation!",
     "30,25,40",
     'timeSec=15;'),
    (141, "EmotionGasClouds", "DodgeGame", "Emotion Gas Clouds",
     "People's emotions create gas clouds in the air which can be inhaled, mixed or blown into other people's directions. Dodge the bad ones!",
     "You inhaled only good vibes!", "You choked on someone's rage cloud!",
     "30,25,40",
     'timeSec=22; itemBrush=Brushes.Purple; fallSpeed=180;'),
    (142, "CutsceneLocksSave", "IdleGame", "Cutscene Locks Your Save",
     "You know in cut scenes when it says '3 months later...'? What if the game ACTUALLY locked your save file for 3 months? Wait.",
     "You waited the full 3 months!", "You couldn't wait - save corrupted!",
     "20,15,30",
     'timeSec=15;'),
    (143, "EatBulletsCreature", "DodgeGame", "Eat Bullets To Survive",
     "You're a creature who can only eat bullets for survival. Get people to shoot you, then eat the bullets. Dodge the non-bullet hazards!",
     "You gorged on enough bullets!", "You starved - or choked on a non-bullet!",
     "30,25,20",
     'timeSec=22; itemBrush=Brushes.Yellow; fallSpeed=280;'),
    (144, "ClimbText", "MoveGame", "Climb The Text Around People",
     "Everyone has text around them expressing feelings. Imagine in this game you can CLIMB the text! Collect the words to build your ladder.",
     "You climbed every emotional word!", "You slipped on someone's lie!",
     "20,20,40",
     'need=12; playerBrush=Brushes.White; itemBrush=Brushes.LightYellow;'),
    (145, "TwoSpeedsRacing", "DodgeGame", "Racing: 0 MPH Or 10,000 MPH",
     "Racing game with only 2 speeds: 0 MPH and 10,000 MPH. Did you just run over a small child? Who knows? Dodge the obstacles!",
     "You survived the brutal commute!", "At 10,000 MPH you hit a wall!",
     "30,25,30",
     'timeSec=22; itemBrush=Brushes.Silver; fallSpeed=320;'),
    (146, "BossAndGhost", "ClickGame", "Boss Has A Ghost - Spot Difference",
     "A boss that also has a ghost. Play spot the difference to find weak spots. Ghost has 7 eyes? Shoot the 8th eye on the boss!",
     "All weak spots found - boss defeated!", "You shot the wrong eye - the ghost cursed you!",
     "20,20,40",
     'need=10; timeSec=30;'),
    (147, "KillYourselfAdapts", "ClickGame", "Kill Yourself - World Adapts",
     "A game with 1 goal: kill yourself. But the environment constantly adapts to stop you. Click to attempt suicide - the world reacts!",
     "You finally found a way out!", "The world kept saving you - eternal suffering!",
     "30,20,40",
     'need=12; timeSec=30; targetBrush=Brushes.DarkRed;'),
    (148, "AngleOnlyOneDayStep", "IdleGame", "Angle Only, 1 Day = 1 Step",
     "A game where you only control the angle of your character. 1 day = 1 step forward. Imagine the patience required. Just wait.",
     "You took the perfect step!", "Impatience ruined the angle!",
     "30,25,40",
     'timeSec=15;'),
    (149, "DeathAnimationBurns", "IdleGame", "Death Animation Burns Into Screen",
     "When you kill someone, their death animation loops and burns into your screen so you have to watch it for the rest of the game. Mourn.",
     "You carried every death to the end.", "You couldn't bear the burning images!",
     "20,15,25",
     'timeSec=15;'),
    (150, "UndergroundWords", "MoveGame", "Hunt Words: I, Love, You",
     "A world where people don't talk - physical words are hidden underground. Travel the world hunting 'I', 'love', 'you'. Collect them!",
     "You assembled 'I love you'!", "The words remained scattered!",
     "30,25,40",
     'need=12; playerBrush=Brushes.White; itemBrush=Brushes.HotPink;'),
    (151, "MuseumTribute", "ClickGame", "Build A Tribute Museum",
     "A game where you must pay tribute to someone - collect everything about them and create a museum in their honor. Click the artifacts!",
     "The museum opened to critical acclaim!", "You couldn't find enough artifacts!",
     "30,25,40",
     'need=12; timeSec=30;'),
    (152, "HeartbeatRumble", "IdleGame", "Pad Heartbeat Stops At Death",
     "Imagine a constant rumble in the pad throughout the game - your heartbeat. At the end, you die and it just stops. Feel it fade.",
     "Your heartbeat faded peacefully.", "Your heart stopped suddenly - tragedy!",
     "30,15,40",
     'timeSec=15;'),
    (153, "EightyEightFloors", "ClickGame", "88 Floors, 88 Souls",
     "Awake in an 88 floor skyscraper alone. Every floor is haunted by one of 88 innocent people you killed in an illegal war. Click to ascend.",
     "You reached the top - atoned for 88 souls!", "The guilt crushed you on a lower floor!",
     "20,15,25",
     'need=12; timeSec=30;'),
    (154, "DetectiveEarthShakes", "ClickGame", "Detective - Talking Shakes Earth",
     "You're a detective solving a vital case, but every time you talk to someone the Earth shakes and the environment falls apart. Click carefully!",
     "Case solved before the city collapsed!", "Your questions leveled the city!",
     "30,25,30",
     'need=8; timeSec=25;'),
    (155, "InventorySeed", "IdleGame", "Inventory Becomes A Seed",
     "When you're about to die, make your inventory a seed and plant it. Then die, go to the spot, and wait for your inventory to grow back.",
     "Your inventory sprouted anew!", "You forgot where you planted it!",
     "30,40,25",
     'timeSec=15;'),
    (156, "FatherFacePortal", "ClickGame", "Father's Face Is A Portal",
     "Your face and your father's face is a portal. You're on Earth, he is in Space. Pass items to each other to get back together. Click the items!",
     "Father and child reunited across dimensions!", "The portal closed forever!",
     "20,20,40",
     'need=10; timeSec=25;'),
    (157, "PigeonPersuadeJumpers", "ClickGame", "Pigeon Saves Jumpers",
     "You're a pigeon. Persuade businessmen not to jump off buildings by retrieving items from their homes. Click to deliver items in time!",
     "All jumpers persuaded to step back!", "A businessman jumped - you failed!",
     "30,25,35",
     'need=10; timeSec=22;'),
    (158, "PowerMeterBridge", "ClickGame", "Power Meter As A Bridge",
     "Use your power meter as a bridge. To make the bridge reach bigger gaps, you need to build up your power meter. Click to charge!",
     "Your bridge reached the far shore!", "Your power meter was too low - fell in!",
     "30,25,40",
     'need=12; timeSec=22;'),
    (159, "SecretlyMakeFight", "ClickGame", "Make People Fight For Art",
     "Secretly make people fight each other, then use their bodies to make artwork that enriches NPC lives. Click to orchestrate fights!",
     "Your masterpiece enriched the whole city!", "The fights escalated - city in chaos!",
     "30,25,30",
     'need=10; timeSec=25;'),
    (160, "TouchFloorDie", "MoveGame", "Touch Floor And Die",
     "If you touch the floor you die. The only way to progress is to kill enemies and use their bodies as platforms. Collect enemies to stand on!",
     "You crossed the chasm of bodies!", "You touched the floor - dead!",
     "20,20,40",
     'need=10; wallsKill=true; speed=200; playerBrush=Brushes.White; itemBrush=Brushes.DarkRed;'),
    (161, "TattooPasswords", "ClickGame", "Tattoo Passwords On Enemies",
     "Enemies have tattoo passwords for doors/upgrades. Get close to see - harder enemies have smaller, more discreet tattoos. Click precisely!",
     "All tattoos read - doors unlocked!", "You misread a tattoo - locked forever!",
     "30,25,40",
     'need=10; timeSec=25;'),
    (162, "WaterPistolFPS", "DodgeGame", "Water Pistol FPS In Contaminated Town",
     "An FPS with school children and water pistols, waterbombs, sprinklers in a town where the water has been contaminated. Dodge the splashes!",
     "You stayed dry through the whole town!", "You got contaminated - mutation!",
     "30,40,50",
     'timeSec=22; itemBrush=Brushes.LightBlue; fallSpeed=240;'),
    (163, "PluggedInGuns", "DodgeGame", "Guns Plugged Into Wall Sockets",
     "All guns are required to be plugged into wall sockets. Now THAT would be a tactical shooter. Dodge enemies who can't reach their sockets!",
     "You outmaneuvered the tethered shooters!", "An enemy found a long extension cord!",
     "30,25,30",
     'timeSec=22; itemBrush=Brushes.Yellow; fallSpeed=200;'),
    (164, "MysticalRabbitBus", "ClickGame", "Mystical Rabbit Misses Buses",
     "Control a mystical rabbit at a bus stop in winter. Find creative ways to make people miss their buses. Click at the right moments!",
     "Everyone missed their buses - chaos!", "A bus got away - the rabbit wept!",
     "30,40,50",
     'need=10; timeSec=22;'),
    (165, "CelebritySerialKiller", "ClickGame", "Celebrity Serial Killer Autographs",
     "Play a celebrity serial killer whose cult following exchanges ammunition/weapons for autographs. Sign autographs to get ammo!",
     "Fully stocked on ammo from adoring fans!", "You ran out of ink - and ammo!",
     "30,25,30",
     'need=10; timeSec=25;'),
    (166, "GhostInWaitingRoom", "ClickGame", "Ghost Inspires Lego In Waiting Room",
     "You're a ghost in a doctor's waiting room - only children can see you. Inspire them to make the best lego constructions. Click to inspire!",
     "Children built masterpieces - healed!", "The children grew bored - left!",
     "30,25,40",
     'need=10; timeSec=25;'),
    (167, "BinByDayArtist", "IdleGame", "Bin By Day, Modern Art By Night",
     "You're a bin. By day people throw litter in you, but by night you use that litter to highlight issues in the city through modern art. Observe.",
     "Your art changed the city!", "The bin was emptied prematurely!",
     "20,20,30",
     'timeSec=15;'),
    (168, "ControlATear", "MoveGame", "Control A Tear On A Body",
     "Imagine controlling a tear, similar to Flower but instead of fields you navigate a naked body. Travel the contours. Collect the path!",
     "Your tear reached its destination!", "Your tear dried up too soon!",
     "30,25,40",
     'need=10; playerBrush=Brushes.LightBlue; itemBrush=Brushes.Pink;'),
    (169, "MotherInLawBully", "ClickGame", "Mother-In-Law Bully",
     "Play a mother-in-law who bullies your son's young wife. Aim: have her thrown out within X months. Click to scheme!",
     "She was thrown out - victory!", "Your son defended her - you're out!",
     "30,25,40",
     'need=10; timeSec=25;'),
    (170, "BuskerGuitarHero", "IdleGame", "Busker Whose Music Changes The City",
     "In new Guitar Hero you play as a busker. Witness your city evolve as your music changes the decisions of people around you. Watch it change.",
     "Your music reshaped the city!", "Your music was drowned out!",
     "30,25,40",
     'timeSec=15;'),
    (171, "NPCSaveGame", "ClickGame", "NPCs Save Your Game",
     "NPCs can load and save your game. Keep on their side or they won't save your game - and load old saves. Click them to stay friendly!",
     "All NPCs on your side - saves secured!", "An NPC betrayed you - save wiped!",
     "30,25,35",
     'need=10; timeSec=25;'),
    (172, "TurnBasedBoneFighter", "ClickGame", "Turn-Based Bone-Level Fighter",
     "Turn-based 1v1 fighting: level up individual bones and set which order joints move and rotate to create attacks. Click to power up bones!",
     "Your skeleton warrior is unmatched!", "Your bones shattered in the ring!",
     "30,25,30",
     'need=10; timeSec=25;'),
    (173, "GirlAndKite", "ClickGame", "Girl And Talking Kite",
     "You're a small girl flying a talking kite. The kite seems to know about an upcoming major terrorist attack and floats towards clues. Follow the kite!",
     "You uncovered the terrorist plot!", "The kite led you astray - trap!",
     "30,40,60",
     'need=10; timeSec=25;'),
    (174, "TummyEarthquakes", "StillGame", "Tummy Rumble Earthquakes",
     "What if your tummy rumbling created earthquakes? In a world where food is rare. Stay perfectly still to avoid rumbling!",
     "No rumbles - the world is safe!", "Your tummy rumbled - earthquake!",
     "30,25,35",
     'timeSec=12;'),
    (175, "BuryPartner", "IdleGame", "Co-op Bury Your Partner",
     "Co-op game where if your partner dies, you spend 30 minutes digging a hole to bury them before continuing. Mourn and wait.",
     "You laid your partner to rest - continuing!", "You abandoned the body - cursed!",
     "20,15,30",
     'timeSec=15;'),
    (176, "PlayAsAHole", "MoveGame", "You Play A Hole",
     "You play a hole. Move around an environment making certain elements fall into correct targets at the right time. Collect falling items!",
     "All elements fell through you correctly!", "Items missed your hole - disaster!",
     "20,15,30",
     'need=10; playerBrush=Brushes.Black; itemBrush=Brushes.White;'),
    (177, "ControlWaterfall", "IdleGame", "Control A Waterfall",
     "Control a waterfall and travel the world hiding outdoor passages from adventurers. Observe the eternal flow.",
     "You hid every passage - secret kept!", "An adventurer found a way through!",
     "20,40,50",
     'timeSec=15;'),
    (178, "RomanticParkour", "MoveGame", "Romantic Parkour Holding Hands",
     "Romantic parkour where you and the love of your life must hold hands and jump around a city evading death and injury. Collect the moments!",
     "You and your love escaped the city!", "Death found you both - tragic end!",
     "50,30,40",
     'need=10; playerBrush=Brushes.Pink; itemBrush=Brushes.Gold;'),
    (179, "ProtestToChange", "ClickGame", "Join Protests To Change Rules",
     "Join protests to make changes to the rules within the game's world. Click to join each protest and force the changes!",
     "All rules changed - revolution complete!", "The protests were crushed!",
     "30,25,40",
     'need=10; timeSec=25;'),
    (180, "VentilationFamily", "IdleGame", "Secretly Support Family Via Vents",
     "Imagine having to secretly support your family via complex ventilation passages in your large industrial home. Stay hidden.",
     "Your family thrived without ever seeing you!", "They found you in the vents - police!",
     "20,20,30",
     'timeSec=15;'),
    (181, "RadioactiveBabyTorch", "MoveGame", "Radioactive Baby As Torch",
     "Carry a radioactive baby in a pitch black environment - the baby acts as a torch. Rocking the baby intensifies the glow. Find your way!",
     "You found your way through the dark!", "The baby's glow faded - lost forever!",
     "10,10,20",
     'need=10; playerBrush=Brushes.LawnGreen; itemBrush=Brushes.Yellow;'),
    (182, "FatherReturnsAsDog", "IdleGame", "Father Dies, Returns As Dog",
     "Play a father whose family dog has gone missing. You die looking for it and return home in the body of the missing dog. Return home.",
     "You returned to your family as the dog.", "The family shooed you away - strays!",
     "30,25,35",
     'timeSec=15;'),
    (183, "AvatarFearsYou", "StillGame", "Avatar Fears You",
     "Imagine if your player avatar feared YOU. Press the buttons too hard and fast and the avatar becomes anxious. Stay gentle.",
     "Your avatar trusts you now!", "You terrified your avatar - it ran!",
     "30,25,40",
     'timeSec=12;'),
    (184, "AmnesiaMuseum", "MoveGame", "Amnesia Museum Of Your Life",
     "3D adventure where you have amnesia and wake up in a gigantic museum where every room is devoted to a year of your life. Collect memories!",
     "You recovered your entire life!", "Some years remain locked away forever!",
     "30,25,35",
     'need=12; playerBrush=Brushes.White; itemBrush=Brushes.Goldenrod;'),
    (185, "ImaginaryFriendDoubts", "ClickGame", "Convince Imaginary Friend",
     "An imaginary friend who doesn't believe you are real. You must convince them that you are. Click to make your case!",
     "You convinced your imaginary friend!", "Your imaginary friend faded away!",
     "30,25,40",
     'need=10; timeSec=25;'),
    (186, "PostmanXRays", "ClickGame", "Postman With X-Ray Eyes",
     "You're a postman with x-ray eyes. Do you post that nasty letter? Do you add extra money to the little boy's birthday card? Make choices!",
     "You delivered justice through the mail!", "Your choices ruined too many lives!",
     "30,25,35",
     'need=10; timeSec=25;'),
    (187, "NoNumbersJustEmotion", "IdleGame", "No Numbers, Just Smile Or Tear",
     "Fed up with numbers in games. Don't show me how I'm doing with a number - show me with a smile or a tear. Just feel the result.",
     "You felt the smile of victory!", "You felt the tear of defeat!",
     "30,25,40",
     'timeSec=15;'),
    (188, "PlayAsAFullStop", "RhythmGame", "Play As A Full Stop",
     "You control a full stop. Enemies read you stories and you have to jump in between words at the right time. Tap on the beat!",
     "All sentences punctuated perfectly!", "Your timing was off - run-on sentence!",
     "20,20,30",
     'need=10;'),
]

# ----------------------------------------------------------------------------
# Solution file
# ----------------------------------------------------------------------------

def make_sln(demos):
    lines = []
    lines.append("Microsoft Visual Studio Solution File, Format Version 12.00")
    lines.append("# Visual Studio Version 17")
    lines.append("Project(\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\") = \"Molydeux.Common\", \"Common\\Common.csproj\", \"{" + COMMON_GUID + "}\"")
    lines.append("EndProject")
    for d in demos:
        n = d[0]
        slug = d[1]
        proj_name = f"Demo{n:02d}_{slug}"
        path = f"Demos\\{proj_name}\\{proj_name}.csproj"
        guid = PROJECT_GUIDS[n]
        lines.append(f"Project(\"{{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}}\") = \"{proj_name}\", \"{path}\", \"{{{guid}}}\"")
        lines.append("EndProject")
    lines.append("Global")
    lines.append("\tGlobalSection(SolutionConfigurationPlatforms) = preSolution")
    lines.append("\t\tDebug|Any CPU = Debug|Any CPU")
    lines.append("\t\tRelease|Any CPU = Release|Any CPU")
    lines.append("\tEndGlobalSection")
    lines.append("\tGlobalSection(ProjectConfigurationPlatforms) = postSolution")
    lines.append(f"\t\t{{{COMMON_GUID}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU")
    lines.append(f"\t\t{{{COMMON_GUID}}}.Debug|Any CPU.Build.0 = Debug|Any CPU")
    lines.append(f"\t\t{{{COMMON_GUID}}}.Release|Any CPU.ActiveCfg = Release|Any CPU")
    lines.append(f"\t\t{{{COMMON_GUID}}}.Release|Any CPU.Build.0 = Release|Any CPU")
    for d in demos:
        n = d[0]
        guid = PROJECT_GUIDS[n]
        lines.append(f"\t\t{{{guid}}}.Debug|Any CPU.ActiveCfg = Debug|Any CPU")
        lines.append(f"\t\t{{{guid}}}.Debug|Any CPU.Build.0 = Debug|Any CPU")
        lines.append(f"\t\t{{{guid}}}.Release|Any CPU.ActiveCfg = Release|Any CPU")
        lines.append(f"\t\t{{{guid}}}.Release|Any CPU.Build.0 = Release|Any CPU")
    lines.append("\tEndGlobalSection")
    lines.append("\tGlobalSection(SolutionProperties) = preSolution")
    lines.append("\t\tHideSolutionNode = FALSE")
    lines.append("\tEndGlobalSection")
    lines.append("EndGlobal")
    return "\n".join(lines) + "\n"


# Stable per-project GUIDs (deterministic; derived from index)
COMMON_GUID = "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA"


def _guid_for(n: int) -> str:
    hi = (0x10000000 + n) & 0xFFFFFFFF
    return f"{hi:08X}-0000-0000-0000-{n:012X}"


PROJECT_GUIDS = {n: _guid_for(n) for n in range(1, 1000)}


# ----------------------------------------------------------------------------
# File generation
# ----------------------------------------------------------------------------

def gen():
    os.makedirs(COMMON_DIR, exist_ok=True)
    os.makedirs(DEMOS_DIR, exist_ok=True)
    with open(os.path.join(COMMON_DIR, "Common.csproj"), "w") as f:
        f.write(COMMON_CSPROJ)
    with open(os.path.join(COMMON_DIR, "MolyGame.cs"), "w") as f:
        f.write(MOLY_GAME_CS)
    for d in DEMOS:
        n, slug, tpl, title, instr, win, lose, bg, extras = d
        proj_name = f"Demo{n:02d}_{slug}"
        proj_dir = os.path.join(DEMOS_DIR, proj_name)
        os.makedirs(proj_dir, exist_ok=True)
        with open(os.path.join(proj_dir, f"{proj_name}.csproj"), "w") as f:
            f.write(make_csproj(proj_name))
        full_title = f"Demo {n:02d}: {title}"
        with open(os.path.join(proj_dir, "Game.cs"), "w") as f:
            f.write(make_game_cs(proj_name, tpl, full_title, instr, win, lose, bg, extras))
    with open(os.path.join(ROOT, "MolydeuxDemos.sln"), "w") as f:
        f.write(make_sln(DEMOS))
    print(f"Generated {len(DEMOS)} demos under {ROOT}")


if __name__ == "__main__":
    gen()

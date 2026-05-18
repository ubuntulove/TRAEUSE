using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPFEngine.Rendering.Graphics;

namespace WPFEngine.Rendering.TileMap
{
    [Flags]
    public enum TileFlags
    {
        None = 0,
        Collidable = 1,
        Interactable = 2,
        Destructible = 4,
        Animated = 8,
        Hazard = 16,
        OneWay = 32,
        Slope = 64,
        Liquid = 128,
        Platform = 256,
        Climbable = 512,
        Trigger = 1024
    }

    public enum TileType
    {
        Air,
        Solid,
        Liquid,
        Platform,
        Slope,
        Hazard,
        Climbable,
        Custom
    }

    public enum SlopeDirection
    {
        None,
        LeftDown,
        LeftUp,
        RightDown,
        RightUp
    }

    public enum CollisionSide
    {
        None,
        Top,
        Bottom,
        Left,
        Right,
        Any
    }

    public enum StampMode
    {
        Single,
        Random,
        Brush
    }

    public enum TileColliderType
    {
        None,
        Full,
        TopOnly,
        LeftHalf,
        RightHalf,
        BottomOnly,
        SlopeLeftDown,
        SlopeLeftUp,
        SlopeRightDown,
        SlopeRightUp
    }

    public struct TileRegion
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }

        public TileRegion(int x, int y, int width, int height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public Rect ToRect()
        {
            return new Rect(X, Y, Width, Height);
        }

        public bool Contains(int x, int y)
        {
            return x >= X && x < X + Width && y >= Y && y < Y + Height;
        }

        public override string ToString()
        {
            return $"({X}, {Y}, {Width}, {Height})";
        }
    }

    public class TileData
    {
        public int TileId { get; set; }
        public int SpriteIndex { get; set; }
        public TileType Type { get; set; }
        public TileFlags Flags { get; set; }
        public SlopeDirection SlopeDir { get; set; }
        public TileColliderType ColliderType { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public Color? TintColor { get; set; }
        public double Opacity { get; set; } = 1.0;
        public int Rotation { get; set; }
        public bool FlipHorizontally { get; set; }
        public bool FlipVertically { get; set; }

        public TileData()
        {
            TileId = -1;
            Type = TileType.Air;
            Properties = new Dictionary<string, object>();
        }

        public TileData(int tileId, TileType type, TileFlags flags = TileFlags.None)
        {
            TileId = tileId;
            Type = type;
            Flags = flags;
            Properties = new Dictionary<string, object>();
            ColliderType = GetDefaultColliderType(type);
        }

        public static TileColliderType GetDefaultColliderType(TileType type)
        {
            return type switch
            {
                TileType.Solid => TileColliderType.Full,
                TileType.Platform => TileColliderType.TopOnly,
                TileType.Slope => TileColliderType.SlopeRightDown,
                TileType.Liquid => TileColliderType.None,
                _ => TileColliderType.None
            };
        }

        public bool HasFlag(TileFlags flag)
        {
            return (Flags & flag) == flag;
        }

        public T GetProperty<T>(string key, T defaultValue = default)
        {
            if (Properties.TryGetValue(key, out var value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        public void SetProperty(string key, object value)
        {
            Properties[key] = value;
        }

        public TileData Clone()
        {
            return new TileData
            {
                TileId = TileId,
                SpriteIndex = SpriteIndex,
                Type = Type,
                Flags = Flags,
                SlopeDir = SlopeDir,
                ColliderType = ColliderType,
                Properties = new Dictionary<string, object>(Properties),
                TintColor = TintColor,
                Opacity = Opacity,
                Rotation = Rotation,
                FlipHorizontally = FlipHorizontally,
                FlipVertically = FlipVertically
            };
        }
    }

    public class TileLayer
    {
        public string Name { get; set; }
        public bool IsVisible { get; set; } = true;
        public double Opacity { get; set; } = 1.0;
        public int Order { get; set; }
        private TileData[,] tiles;

        public int Width { get; }
        public int Height { get; }

        public TileLayer(int width, int height, string name = "Layer")
        {
            Width = width;
            Height = height;
            Name = name;
            tiles = new TileData[width, height];
        }

        public TileData GetTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return null;
            return tiles[x, y];
        }

        public TileData GetTileOrEmpty(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return new TileData(-1, TileType.Air);
            return tiles[x, y] ?? new TileData(-1, TileType.Air);
        }

        public void SetTile(int x, int y, TileData tile)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;
            tiles[x, y] = tile;
        }

        public void ClearTile(int x, int y)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height)
                return;
            tiles[x, y] = null;
        }

        public void Clear()
        {
            Array.Clear(tiles, 0, tiles.Length);
        }

        public IEnumerable<(int X, int Y, TileData Tile)> GetAllTiles()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (tiles[x, y] != null)
                    {
                        yield return (x, y, tiles[x, y]);
                    }
                }
            }
        }

        public TileData[,] GetTileArray()
        {
            return tiles;
        }
    }

    public class TileMap
    {
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int CellSize { get; set; }
        public List<TileLayer> Layers { get; }
        public Dictionary<int, TileData> TileDefinitions { get; }
        public TileMapBounds Bounds { get; }

        public TileMap(int width, int height, int cellSize = 16)
        {
            Width = width;
            Height = height;
            CellSize = cellSize;
            Layers = new List<TileLayer>();
            TileDefinitions = new Dictionary<int, TileData>();
            Bounds = new TileMapBounds(this);

            Layers.Add(new TileLayer(width, height, "Ground"));
            Layers.Add(new TileLayer(width, height, "Decoration"));
            Layers.Add(new TileLayer(width, height, "Overlay"));
        }

        public void Resize(int newWidth, int newHeight)
        {
            foreach (var layer in Layers)
            {
                var newTiles = new TileData[newWidth, newHeight];
                Array.Copy(layer.GetTileArray(), newTiles, 
                    Math.Min(Width, newWidth) * Math.Min(Height, newHeight));
            }
            Width = newWidth;
            Height = newHeight;
        }

        public void AddLayer(string name = null)
        {
            Layers.Add(new TileLayer(Width, Height, name ?? $"Layer{Layers.Count}"));
        }

        public void RemoveLayer(int index)
        {
            if (index >= 0 && index < Layers.Count && Layers.Count > 1)
            {
                Layers.RemoveAt(index);
            }
        }

        public TileLayer GetLayer(int index)
        {
            return index >= 0 && index < Layers.Count ? Layers[index] : null;
        }

        public TileLayer GetLayer(string name)
        {
            return Layers.FirstOrDefault(l => l.Name == name);
        }

        public TileData GetTile(int x, int y, int layerIndex = 0)
        {
            var layer = GetLayer(layerIndex);
            return layer?.GetTile(x, y);
        }

        public TileData GetTileWorldPosition(double worldX, double worldY, int layerIndex = 0)
        {
            int tileX = (int)(worldX / CellSize);
            int tileY = (int)(worldY / CellSize);
            return GetTile(tileX, tileY, layerIndex);
        }

        public void SetTile(int x, int y, TileData tile, int layerIndex = 0)
        {
            var layer = GetLayer(layerIndex);
            layer?.SetTile(x, y, tile);
        }

        public void SetTile(int x, int y, int tileId, int layerIndex = 0)
        {
            if (TileDefinitions.TryGetValue(tileId, out var tileData))
            {
                SetTile(x, y, tileData.Clone(), layerIndex);
            }
        }

        public void ClearTile(int x, int y, int layerIndex = 0)
        {
            var layer = GetLayer(layerIndex);
            layer?.ClearTile(x, y);
        }

        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }

        public bool IsInBounds(TilePosition pos)
        {
            return IsInBounds(pos.X, pos.Y);
        }

        public Rect GetWorldBounds()
        {
            return new Rect(0, 0, Width * CellSize, Height * CellSize);
        }

        public void DefineTile(int id, TileData data)
        {
            data.TileId = id;
            TileDefinitions[id] = data;
        }

        public TileData GetTileDefinition(int id)
        {
            return TileDefinitions.TryGetValue(id, out var data) ? data : null;
        }

        public IEnumerable<TilePosition> GetTilePositions()
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    yield return new TilePosition(x, y);
                }
            }
        }

        public IEnumerable<TilePosition> GetTilesInRect(TileRegion region)
        {
            int startX = Math.Max(0, region.X);
            int startY = Math.Max(0, region.Y);
            int endX = Math.Min(Width, region.X + region.Width);
            int endY = Math.Min(Height, region.Y + region.Height);

            for (int x = startX; x < endX; x++)
            {
                for (int y = startY; y < endY; y++)
                {
                    yield return new TilePosition(x, y);
                }
            }
        }

        public void Clear()
        {
            foreach (var layer in Layers)
            {
                layer.Clear();
            }
        }

        public TileMap Clone()
        {
            var clone = new TileMap(Width, Height, CellSize);
            clone.Layers.Clear();
            foreach (var layer in Layers)
            {
                var newLayer = new TileLayer(Width, Height, layer.Name)
                {
                    IsVisible = layer.IsVisible,
                    Opacity = layer.Opacity,
                    Order = layer.Order
                };
                Array.Copy(layer.GetTileArray(), newLayer.GetTileArray(), Width * Height);
                clone.Layers.Add(newLayer);
            }
            foreach (var kvp in TileDefinitions)
            {
                clone.TileDefinitions[kvp.Key] = kvp.Value.Clone();
            }
            return clone;
        }
    }

    public struct TilePosition : IEquatable<TilePosition>
    {
        public int X { get; set; }
        public int Y { get; set; }

        public TilePosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public TilePosition(double x, double y)
        {
            X = (int)x;
            Y = (int)y;
        }

        public TilePosition[] GetNeighbors4()
        {
            return new[]
            {
                new TilePosition(X, Y - 1),
                new TilePosition(X + 1, Y),
                new TilePosition(X, Y + 1),
                new TilePosition(X - 1, Y)
            };
        }

        public TilePosition[] GetNeighbors8()
        {
            return new[]
            {
                new TilePosition(X, Y - 1),
                new TilePosition(X + 1, Y - 1),
                new TilePosition(X + 1, Y),
                new TilePosition(X + 1, Y + 1),
                new TilePosition(X, Y + 1),
                new TilePosition(X - 1, Y + 1),
                new TilePosition(X - 1, Y),
                new TilePosition(X - 1, Y - 1)
            };
        }

        public double DistanceTo(TilePosition other)
        {
            return Math.Sqrt(Math.Pow(X - other.X, 2) + Math.Pow(Y - other.Y, 2));
        }

        public int ManhattanDistanceTo(TilePosition other)
        {
            return Math.Abs(X - other.X) + Math.Abs(Y - other.Y);
        }

        public bool Equals(TilePosition other)
        {
            return X == other.X && Y == other.Y;
        }

        public override bool Equals(object obj)
        {
            return obj is TilePosition other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }

        public override string ToString()
        {
            return $"({X}, {Y})";
        }

        public static bool operator ==(TilePosition left, TilePosition right) => left.Equals(right);
        public static bool operator !=(TilePosition left, TilePosition right) => !left.Equals(right);
    }

    public class TileMapBounds
    {
        private readonly TileMap tileMap;

        public TileMapBounds(TileMap tileMap)
        {
            this.tileMap = tileMap;
        }

        public int MinX => 0;
        public int MinY => 0;
        public int MaxX => tileMap.Width - 1;
        public int MaxY => tileMap.Height - 1;
        public int Width => tileMap.Width;
        public int Height => tileMap.Height;
        public int CellSize => tileMap.CellSize;

        public double WorldMinX => 0;
        public double WorldMinY => 0;
        public double WorldMaxX => tileMap.Width * tileMap.CellSize;
        public double WorldMaxY => tileMap.Height * tileMap.CellSize;

        public Rect WorldBounds => new Rect(WorldMinX, WorldMinY, WorldMaxX, WorldMaxY);

        public bool Contains(int tileX, int tileY)
        {
            return tileX >= MinX && tileX <= MaxX && tileY >= MinY && tileY <= MaxY;
        }

        public bool Contains(TilePosition pos)
        {
            return Contains(pos.X, pos.Y);
        }

        public bool ContainsWorld(double worldX, double worldY)
        {
            return worldX >= WorldMinX && worldX < WorldMaxX && 
                   worldY >= WorldMinY && worldY < WorldMaxY;
        }

        public TilePosition WorldToTile(double worldX, double worldY)
        {
            return new TilePosition(
                (int)(worldX / tileMap.CellSize),
                (int)(worldY / tileMap.CellSize)
            );
        }

        public Point TileToWorld(int tileX, int tileY)
        {
            return new Point(tileX * tileMap.CellSize, tileY * tileMap.CellSize);
        }

        public Point TileToWorldCenter(int tileX, int tileY)
        {
            return new Point(
                tileX * tileMap.CellSize + tileMap.CellSize / 2.0,
                tileY * tileMap.CellSize + tileMap.CellSize / 2.0
            );
        }

        public Rect TileToWorldRect(int tileX, int tileY)
        {
            return new Rect(
                tileX * tileMap.CellSize,
                tileY * tileMap.CellSize,
                tileMap.CellSize,
                tileMap.CellSize
            );
        }
    }

    public class SpriteSheet
    {
        public BitmapSource Atlas { get; set; }
        public int TileWidth { get; set; }
        public int TileHeight { get; set; }
        public int Columns { get; set; }
        public int Rows { get; set; }
        public int Spacing { get; set; }
        public int Margin { get; set; }
        public Dictionary<int, TileRegion> SpriteRegions { get; }
        public Dictionary<string, int> NamedSprites { get; }

        public SpriteSheet(BitmapSource atlas, int tileWidth, int tileHeight, int spacing = 0, int margin = 0)
        {
            Atlas = atlas;
            TileWidth = tileWidth;
            TileHeight = tileHeight;
            Spacing = spacing;
            Margin = margin;
            Columns = (int)((atlas.PixelWidth - 2 * margin + spacing) / (tileWidth + spacing));
            Rows = (int)((atlas.PixelHeight - 2 * margin + spacing) / (tileHeight + spacing));
            SpriteRegions = new Dictionary<int, TileRegion>();
            NamedSprites = new Dictionary<string, int>();

            InitializeRegions();
        }

        private void InitializeRegions()
        {
            for (int y = 0; y < Rows; y++)
            {
                for (int x = 0; x < Columns; x++)
                {
                    int index = y * Columns + x;
                    int regionX = Margin + x * (TileWidth + Spacing);
                    int regionY = Margin + y * (TileHeight + Spacing);
                    SpriteRegions[index] = new TileRegion(regionX, regionY, TileWidth, TileHeight);
                }
            }
        }

        public TileRegion GetRegion(int index)
        {
            return SpriteRegions.TryGetValue(index, out var region) ? region : TileRegion.Empty;
        }

        public void SetRegion(int index, TileRegion region)
        {
            SpriteRegions[index] = region;
        }

        public void AddNamedSprite(string name, int index)
        {
            NamedSprites[name] = index;
        }

        public int? GetNamedSprite(string name)
        {
            return NamedSprites.TryGetValue(name, out var index) ? index : null;
        }

        public TileRegion? GetRegion(string name)
        {
            if (NamedSprites.TryGetValue(name, out var index))
            {
                return GetRegion(index);
            }
            return null;
        }

        public BitmapSource GetSprite(int index)
        {
            if (!SpriteRegions.TryGetValue(index, out var region))
                return null;

            return new CroppedBitmap(Atlas, new Int32Rect(region.X, region.Y, region.Width, region.Height));
        }

        public IEnumerable<int> GetAllSpriteIndices()
        {
            return SpriteRegions.Keys.OrderBy(k => k);
        }

        public void PackRectangles(List<TileRegion> regions, int atlasWidth, int atlasHeight)
        {
            regions.Sort((a, b) => b.Height * b.Width - a.Height * a.Width);

            int currentX = Margin;
            int currentY = Margin;
            int maxHeightInRow = 0;

            foreach (var region in regions)
            {
                if (currentX + region.Width > atlasWidth - Margin)
                {
                    currentX = Margin;
                    currentY += maxHeightInRow + Spacing;
                    maxHeightInRow = 0;
                }

                region.X = currentX;
                region.Y = currentY;
                maxHeightInRow = Math.Max(maxHeightInRow, region.Height);
                currentX += region.Width + Spacing;
            }
        }
    }

    public class SpriteAnimation
    {
        public string Name { get; set; }
        public List<int> Frames { get; }
        public double FrameDuration { get; set; }
        public bool Loop { get; set; } = true;

        public SpriteAnimation(string name, double frameDuration = 0.2)
        {
            Name = name;
            Frames = new List<int>();
            FrameDuration = frameDuration;
        }

        public void AddFrame(int spriteIndex)
        {
            Frames.Add(spriteIndex);
        }

        public int GetFrameAtTime(double time)
        {
            if (Frames.Count == 0) return -1;
            if (Frames.Count == 1) return Frames[0];

            int frameIndex = (int)(time / FrameDuration) % Frames.Count;
            return Frames[frameIndex];
        }

        public double GetDuration()
        {
            return Frames.Count * FrameDuration;
        }
    }

    public class AnimatedTile
    {
        public int TileId { get; set; }
        public Dictionary<string, SpriteAnimation> Animations { get; }
        public string CurrentAnimation { get; set; }
        public double CurrentTime { get; set; }
        public bool IsPlaying { get; set; } = true;

        public AnimatedTile(int tileId)
        {
            TileId = tileId;
            Animations = new Dictionary<string, SpriteAnimation>();
        }

        public void AddAnimation(string name, SpriteAnimation animation)
        {
            Animations[name] = animation;
            if (CurrentAnimation == null)
            {
                CurrentAnimation = name;
            }
        }

        public void Play(string animationName)
        {
            if (Animations.ContainsKey(animationName))
            {
                CurrentAnimation = animationName;
                CurrentTime = 0;
                IsPlaying = true;
            }
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Resume()
        {
            IsPlaying = true;
        }

        public void Update(double deltaTime)
        {
            if (!IsPlaying || !Animations.TryGetValue(CurrentAnimation, out var animation))
                return;

            CurrentTime += deltaTime;

            if (!animation.Loop && CurrentTime >= animation.GetDuration())
            {
                IsPlaying = false;
                CurrentTime = animation.GetDuration();
            }
            else
            {
                CurrentTime %= animation.GetDuration();
            }
        }

        public int GetCurrentFrame()
        {
            if (!Animations.TryGetValue(CurrentAnimation, out var animation))
                return -1;
            return animation.GetFrameAtTime(CurrentTime);
        }

        public static AnimatedTile CreateWater(int baseTileId)
        {
            var tile = new AnimatedTile(baseTileId);
            var waveAnim = new SpriteAnimation("wave", 0.4);
            waveAnim.AddFrame(baseTileId);
            waveAnim.AddFrame(baseTileId + 1);
            waveAnim.AddFrame(baseTileId + 2);
            waveAnim.AddFrame(baseTileId + 1);
            tile.AddAnimation("wave", waveAnim);
            return tile;
        }

        public static AnimatedTile CreateLava(int baseTileId)
        {
            var tile = new AnimatedTile(baseTileId);
            var flowAnim = new SpriteAnimation("flow", 0.3);
            flowAnim.AddFrame(baseTileId);
            flowAnim.AddFrame(baseTileId + 1);
            flowAnim.AddFrame(baseTileId + 2);
            flowAnim.AddFrame(baseTileId + 3);
            tile.AddAnimation("flow", flowAnim);
            return tile;
        }

        public static AnimatedTile CreateGrass(int baseTileId)
        {
            var tile = new AnimatedTile(baseTileId);
            var swayAnim = new SpriteAnimation("sway", 0.5);
            swayAnim.AddFrame(baseTileId);
            swayAnim.AddFrame(baseTileId + 1);
            swayAnim.AddFrame(baseTileId + 2);
            swayAnim.AddFrame(baseTileId + 1);
            tile.AddAnimation("sway", swayAnim);
            return tile;
        }
    }

    public class TileMapRenderer
    {
        private readonly TileMap tileMap;
        private readonly SpriteSheet spriteSheet;
        private Dictionary<int, BitmapSource> spriteCache;
        private int chunkSize;
        private Dictionary<TilePosition, TileChunk> chunks;
        private List<AnimatedTile> animatedTiles;

        public TileMapRenderer(TileMap tileMap, SpriteSheet spriteSheet, int chunkSize = 16)
        {
            this.tileMap = tileMap;
            this.spriteSheet = spriteSheet;
            this.chunkSize = chunkSize;
            chunks = new Dictionary<TilePosition, TileChunk>();
            spriteCache = new Dictionary<int, BitmapSource>();
            animatedTiles = new List<AnimatedTile>();
        }

        public void RegisterAnimatedTile(AnimatedTile animatedTile)
        {
            animatedTiles.Add(animatedTile);
        }

        public void Update(double deltaTime)
        {
            foreach (var tile in animatedTiles)
            {
                tile.Update(deltaTime);
            }
        }

        public TileChunk GetChunk(int chunkX, int chunkY)
        {
            var pos = new TilePosition(chunkX, chunkY);
            if (!chunks.TryGetValue(pos, out var chunk))
            {
                chunk = GenerateChunk(chunkX, chunkY);
                chunks[pos] = chunk;
            }
            return chunk;
        }

        private TileChunk GenerateChunk(int chunkX, int chunkY)
        {
            var chunk = new TileChunk(chunkX, chunkY, chunkSize, tileMap.CellSize);

            int startX = chunkX * chunkSize;
            int startY = chunkY * chunkSize;
            int endX = Math.Min(startX + chunkSize, tileMap.Width);
            int endY = Math.Min(startY + chunkSize, tileMap.Height);

            for (int layerIdx = 0; layerIdx < tileMap.Layers.Count; layerIdx++)
            {
                var layer = tileMap.Layers[layerIdx];
                if (!layer.IsVisible) continue;

                for (int x = startX; x < endX; x++)
                {
                    for (int y = startY; y < endY; y++)
                    {
                        var tile = layer.GetTile(x, y);
                        if (tile != null)
                        {
                            int spriteIndex = GetAnimatedSpriteIndex(tile);
                            chunk.AddTile(x - startX, y - startY, spriteIndex, layerIdx, tile);
                        }
                    }
                }
            }

            return chunk;
        }

        private int GetAnimatedSpriteIndex(TileData tile)
        {
            if (tile.HasFlag(TileFlags.Animated))
            {
                var animated = animatedTiles.FirstOrDefault(a => a.TileId == tile.TileId);
                if (animated != null)
                {
                    return animated.GetCurrentFrame();
                }
            }
            return tile.SpriteIndex;
        }

        public IEnumerable<TileChunk> GetVisibleChunks(Rect viewport)
        {
            int startChunkX = Math.Max(0, (int)(viewport.X / (chunkSize * tileMap.CellSize)));
            int startChunkY = Math.Max(0, (int)(viewport.Y / (chunkSize * tileMap.CellSize)));
            int endChunkX = (int)Math.Ceiling((viewport.X + viewport.Width) / (chunkSize * tileMap.CellSize));
            int endChunkY = (int)Math.Ceiling((viewport.Y + viewport.Height) / (chunkSize * tileMap.CellSize));

            for (int x = startChunkX; x <= endChunkX; x++)
            {
                for (int y = startChunkY; y <= endChunkY; y++)
                {
                    yield return GetChunk(x, y);
                }
            }
        }

        public void Render(RenderContext context, Rect viewport)
        {
            var visibleChunks = GetVisibleChunks(viewport);

            foreach (var chunk in visibleChunks)
            {
                RenderChunk(context, chunk, viewport);
            }
        }

        private void RenderChunk(RenderContext context, TileChunk chunk, Rect viewport)
        {
            foreach (var tileRender in chunk.GetTiles())
            {
                double worldX = (chunk.StartX + tileRender.LocalX) * tileMap.CellSize;
                double worldY = (chunk.StartY + tileRender.LocalY) * tileMap.CellSize;

                if (worldX + tileMap.CellSize < viewport.X || 
                    worldX > viewport.X + viewport.Width ||
                    worldY + tileMap.CellSize < viewport.Y || 
                    worldY > viewport.Y + viewport.Height)
                {
                    continue;
                }

                var sprite = GetSprite(tileRender.SpriteIndex);
                if (sprite != null)
                {
                    var layer = tileMap.Layers[tileRender.LayerIndex];
                    context.DrawSprite(sprite, (float)worldX, (float)worldY, 
                        layer.Opacity, tileRender.Tile);
                }
            }
        }

        private BitmapSource GetSprite(int index)
        {
            if (!spriteCache.TryGetValue(index, out var sprite))
            {
                sprite = spriteSheet.GetSprite(index);
                if (sprite != null)
                {
                    spriteCache[index] = sprite;
                }
            }
            return sprite;
        }

        public void InvalidateChunk(int chunkX, int chunkY)
        {
            chunks.Remove(new TilePosition(chunkX, chunkY));
        }

        public void InvalidateAll()
        {
            chunks.Clear();
        }

        public List<Rect> GenerateColliders(TileMap map)
        {
            var colliders = new List<Rect>();

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.GetTile(x, y, 0);
                    if (tile != null && tile.HasFlag(TileFlags.Collidable))
                    {
                        var rect = GetColliderRect(tile, x, y, map.CellSize);
                        if (rect != Rect.Empty)
                        {
                            colliders.Add(rect);
                        }
                    }
                }
            }

            return MergeColliders(colliders);
        }

        private Rect GetColliderRect(TileData tile, int x, int y, int cellSize)
        {
            return tile.ColliderType switch
            {
                TileColliderType.Full => new Rect(x * cellSize, y * cellSize, cellSize, cellSize),
                TileColliderType.TopOnly => new Rect(x * cellSize, y * cellSize, cellSize, cellSize * 0.2),
                TileColliderType.LeftHalf => new Rect(x * cellSize, y * cellSize, cellSize * 0.5, cellSize),
                TileColliderType.RightHalf => new Rect(x * cellSize + cellSize * 0.5, y * cellSize, cellSize * 0.5, cellSize),
                TileColliderType.BottomOnly => new Rect(x * cellSize, y * cellSize + cellSize * 0.8, cellSize, cellSize * 0.2),
                _ => Rect.Empty
            };
        }

        private List<Rect> MergeColliders(List<Rect> colliders)
        {
            if (colliders.Count == 0) return colliders;

            colliders.Sort((a, b) => a.X.CompareTo(b.X) != 0 ? a.X.CompareTo(b.X) : a.Y.CompareTo(b.Y));

            var merged = new List<Rect>();
            var current = colliders[0];

            for (int i = 1; i < colliders.Count; i++)
            {
                var next = colliders[i];
                if (CanMerge(current, next))
                {
                    current = Merge(current, next);
                }
                else
                {
                    merged.Add(current);
                    current = next;
                }
            }
            merged.Add(current);

            return merged;
        }

        private bool CanMerge(Rect a, Rect b)
        {
            return Math.Abs(a.X - b.X) < 1 && Math.Abs(a.Width - b.Width) < 1 &&
                   a.Y + a.Height >= b.Y && b.Y + b.Height >= a.Y;
        }

        private Rect Merge(Rect a, Rect b)
        {
            double minY = Math.Min(a.Y, b.Y);
            double maxY = Math.Max(a.Y + a.Height, b.Y + b.Height);
            return new Rect(a.X, minY, a.Width, maxY - minY);
        }
    }

    public class TileChunk
    {
        public int StartX { get; }
        public int StartY { get; }
        public int Size { get; }
        public int CellSize { get; }
        private List<TileRenderData> tiles;

        public TileChunk(int chunkX, int chunkY, int size, int cellSize)
        {
            StartX = chunkX * size;
            StartY = chunkY * size;
            Size = size;
            CellSize = cellSize;
            tiles = new List<TileRenderData>();
        }

        public void AddTile(int localX, int localY, int spriteIndex, int layerIndex, TileData tile)
        {
            tiles.Add(new TileRenderData
            {
                LocalX = localX,
                LocalY = localY,
                SpriteIndex = spriteIndex,
                LayerIndex = layerIndex,
                Tile = tile
            });
        }

        public IEnumerable<TileRenderData> GetTiles()
        {
            return tiles;
        }

        public int TileCount => tiles.Count;
    }

    public class TileRenderData
    {
        public int LocalX { get; set; }
        public int LocalY { get; set; }
        public int SpriteIndex { get; set; }
        public int LayerIndex { get; set; }
        public TileData Tile { get; set; }
    }

    public class TileUtility
    {
        public static HashSet<TilePosition> FloodFill(TileMap map, int startX, int startY, 
            Func<TileData, bool> shouldInclude, int layerIndex = 0)
        {
            var result = new HashSet<TilePosition>();
            var queue = new Queue<TilePosition>();

            if (!map.IsInBounds(startX, startY))
                return result;

            var startTile = map.GetTile(startX, startY, layerIndex);
            if (startTile == null || !shouldInclude(startTile))
                return result;

            queue.Enqueue(new TilePosition(startX, startY));

            while (queue.Count > 0)
            {
                var pos = queue.Dequeue();
                if (!map.IsInBounds(pos) || result.Contains(pos))
                    continue;

                var tile = map.GetTile(pos.X, pos.Y, layerIndex);
                if (tile == null || !shouldInclude(tile))
                    continue;

                result.Add(pos);

                queue.Enqueue(new TilePosition(pos.X, pos.Y - 1));
                queue.Enqueue(new TilePosition(pos.X + 1, pos.Y));
                queue.Enqueue(new TilePosition(pos.X, pos.Y + 1));
                queue.Enqueue(new TilePosition(pos.X - 1, pos.Y));
            }

            return result;
        }

        public static List<TilePosition> FindPath(TileMap map, TilePosition start, TilePosition end,
            Func<TilePosition, bool> isWalkable, bool allowDiagonal = false)
        {
            if (!map.IsInBounds(start) || !map.IsInBounds(end))
                return null;

            if (!isWalkable(end))
                return null;

            var openSet = new PriorityQueue<TilePosition, double>();
            var cameFrom = new Dictionary<TilePosition, TilePosition>();
            var gScore = new Dictionary<TilePosition, double>();
            var fScore = new Dictionary<TilePosition, double>();

            gScore[start] = 0;
            fScore[start] = start.DistanceTo(end);
            openSet.Enqueue(start, fScore[start]);

            var neighbors = allowDiagonal ? start.GetNeighbors8() : start.GetNeighbors4();

            while (openSet.Count > 0)
            {
                var current = openSet.Dequeue();

                if (current.Equals(end))
                {
                    return ReconstructPath(cameFrom, current);
                }

                foreach (var neighbor in allowDiagonal ? current.GetNeighbors8() : current.GetNeighbors4())
                {
                    if (!map.IsInBounds(neighbor) || !isWalkable(neighbor))
                        continue;

                    double tentativeG = gScore.GetValueOrDefault(current, double.MaxValue);

                    if (allowDiagonal)
                    {
                        if (current.X != neighbor.X && current.Y != neighbor.Y)
                        {
                            var diag1 = new TilePosition(current.X, neighbor.Y);
                            var diag2 = new TilePosition(neighbor.X, current.Y);
                            if (!isWalkable(diag1) || !isWalkable(diag2))
                                continue;
                        }
                        tentativeG += 1.414;
                    }
                    else
                    {
                        tentativeG += 1;
                    }

                    if (tentativeG < gScore.GetValueOrDefault(neighbor, double.MaxValue))
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeG;
                        fScore[neighbor] = tentativeG + neighbor.DistanceTo(end);
                        openSet.Enqueue(neighbor, fScore[neighbor]);
                    }
                }
            }

            return null;
        }

        private static List<TilePosition> ReconstructPath(Dictionary<TilePosition, TilePosition> cameFrom, TilePosition current)
        {
            var path = new List<TilePosition> { current };
            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Insert(0, current);
            }
            return path;
        }

        public static List<TilePosition> GetNeighbors4(TileMap map, TilePosition pos)
        {
            return pos.GetNeighbors4()
                .Where(n => map.IsInBounds(n))
                .ToList();
        }

        public static List<TilePosition> GetNeighbors8(TileMap map, TilePosition pos)
        {
            return pos.GetNeighbors8()
                .Where(n => map.IsInBounds(n))
                .ToList();
        }

        public static List<TilePosition> GetConnectedTiles(TileMap map, TilePosition start,
            Func<TileData, bool> matches, int layerIndex = 0)
        {
            return FloodFill(map, start.X, start.Y, 
                tile => tile != null && matches(tile), layerIndex)
                .ToList();
        }

        public static int CountTilesOfType(TileMap map, TileType type, int layerIndex = 0)
        {
            int count = 0;
            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.GetTile(x, y, layerIndex);
                    if (tile != null && tile.Type == type)
                        count++;
                }
            }
            return count;
        }

        public static Rect GetTileRegionBounds(IEnumerable<TilePosition> tiles, int cellSize)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;

            foreach (var pos in tiles)
            {
                minX = Math.Min(minX, pos.X);
                minY = Math.Min(minY, pos.Y);
                maxX = Math.Max(maxX, pos.X);
                maxY = Math.Max(maxY, pos.Y);
            }

            return new Rect(minX * cellSize, minY * cellSize, 
                (maxX - minX + 1) * cellSize, (maxY - minY + 1) * cellSize);
        }
    }

    public class TilePalette
    {
        public string Name { get; set; }
        public List<TileData> Tiles { get; }
        public TileBrush CurrentBrush { get; private set; }
        public StampMode CurrentStampMode { get; set; } = StampMode.Single;

        public TilePalette(string name = "Default")
        {
            Name = name;
            Tiles = new List<TileData>();
            CurrentBrush = new TileBrush();
        }

        public void AddTile(TileData tile)
        {
            Tiles.Add(tile);
        }

        public void RemoveTile(int index)
        {
            if (index >= 0 && index < Tiles.Count)
            {
                Tiles.RemoveAt(index);
            }
        }

        public TileData GetTile(int index)
        {
            return index >= 0 && index < Tiles.Count ? Tiles[index] : null;
        }

        public void SelectTile(int index)
        {
            if (index >= 0 && index < Tiles.Count)
            {
                CurrentBrush.Clear();
                CurrentBrush.AddTile(Tiles[index]);
            }
        }

        public void CreateBrush(IEnumerable<int> tileIndices)
        {
            CurrentBrush.Clear();
            foreach (var index in tileIndices)
            {
                if (index >= 0 && index < Tiles.Count)
                {
                    CurrentBrush.AddTile(Tiles[index]);
                }
            }
        }

        public void SetBrush(TileBrush brush)
        {
            CurrentBrush = brush.Clone();
        }

        public void ApplyStamp(TileMap map, int tileX, int tileY, int layerIndex = 0)
        {
            switch (CurrentStampMode)
            {
                case StampMode.Single:
                    ApplySingle(map, tileX, tileY, layerIndex);
                    break;
                case StampMode.Random:
                    ApplyRandom(map, tileX, tileY, layerIndex);
                    break;
                case StampMode.Brush:
                    ApplyBrush(map, tileX, tileY, layerIndex);
                    break;
            }
        }

        private void ApplySingle(TileMap map, int tileX, int tileY, int layerIndex)
        {
            if (CurrentBrush.Tiles.Count > 0)
            {
                var tile = CurrentBrush.Tiles[0].Clone();
                map.SetTile(tileX, tileY, tile, layerIndex);
            }
        }

        private void ApplyRandom(TileMap map, int tileX, int tileY, int layerIndex)
        {
            if (CurrentBrush.Tiles.Count > 0)
            {
                var random = new Random();
                var tile = CurrentBrush.Tiles[random.Next(CurrentBrush.Tiles.Count)].Clone();
                map.SetTile(tileX, tileY, tile, layerIndex);
            }
        }

        private void ApplyBrush(TileMap map, int tileX, int tileY, int layerIndex)
        {
            int offsetX = 0;
            int offsetY = 0;

            foreach (var brushTile in CurrentBrush.Tiles)
            {
                int targetX = tileX + brushTile.BrushX - CurrentBrush.OffsetX;
                int targetY = tileY + brushTile.BrushY - CurrentBrush.OffsetY;

                if (map.IsInBounds(targetX, targetY))
                {
                    var tile = brushTile.Tile.Clone();
                    map.SetTile(targetX, targetY, tile, layerIndex);
                }
            }
        }

        public void Erase(TileMap map, int tileX, int tileY, int layerIndex = 0)
        {
            map.ClearTile(tileX, tileY, layerIndex);
        }

        public void EraseArea(TileMap map, TileRegion region, int layerIndex = 0)
        {
            foreach (var pos in map.GetTilesInRect(region))
            {
                map.ClearTile(pos.X, pos.Y, layerIndex);
            }
        }

        public void Fill(TileMap map, int tileX, int tileY, int layerIndex = 0)
        {
            if (CurrentBrush.Tiles.Count == 0) return;

            var existingTile = map.GetTile(tileX, tileY, layerIndex);
            if (existingTile != null && CurrentBrush.Tiles.Any(t => t.Tile.TileId == existingTile.TileId))
                return;

            var floodTiles = TileUtility.FloodFill(map, tileX, tileY,
                t => t == null || existingTile == null || t.TileId != existingTile.TileId, layerIndex);

            var fillTile = CurrentBrush.Tiles[0].Tile.Clone();
            foreach (var pos in floodTiles)
            {
                map.SetTile(pos.X, pos.Y, fillTile.Clone(), layerIndex);
            }
        }
    }

    public class TileBrush
    {
        public List<BrushTile> Tiles { get; }
        public int OffsetX { get; set; }
        public int OffsetY { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public TileBrush()
        {
            Tiles = new List<BrushTile>();
        }

        public void AddTile(TileData tile)
        {
            AddTile(0, 0, tile);
        }

        public void AddTile(int x, int y, TileData tile)
        {
            Tiles.Add(new BrushTile { BrushX = x, BrushY = y, Tile = tile });
            UpdateBounds();
        }

        public void RemoveTile(int x, int y)
        {
            Tiles.RemoveAll(t => t.BrushX == x && t.BrushY == y);
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            if (Tiles.Count == 0)
            {
                Width = 0;
                Height = 0;
                OffsetX = 0;
                OffsetY = 0;
                return;
            }

            int minX = Tiles.Min(t => t.BrushX);
            int minY = Tiles.Min(t => t.BrushY);
            int maxX = Tiles.Max(t => t.BrushX);
            int maxY = Tiles.Max(t => t.BrushY);

            Width = maxX - minX + 1;
            Height = maxY - minY + 1;
            OffsetX = minX;
            OffsetY = minY;

            foreach (var tile in Tiles)
            {
                tile.BrushX -= minX;
                tile.BrushY -= minY;
            }
        }

        public void Clear()
        {
            Tiles.Clear();
            Width = 0;
            Height = 0;
        }

        public TileBrush Clone()
        {
            var clone = new TileBrush();
            foreach (var tile in Tiles)
            {
                clone.AddTile(tile.BrushX, tile.BrushY, tile.Tile.Clone());
            }
            return clone;
        }

        public void CreatePattern(int width, int height, Func<int, int, TileData> patternFunc)
        {
            Clear();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = patternFunc(x, y);
                    if (tile != null)
                    {
                        AddTile(x, y, tile);
                    }
                }
            }
        }

        public static TileBrush CreateRandom(List<TileData> tiles, int width, int height)
        {
            var brush = new TileBrush();
            var random = new Random();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = tiles[random.Next(tiles.Count)].Clone();
                    brush.AddTile(x, y, tile);
                }
            }
            return brush;
        }

        public static TileBrush CreateCheckerboard(TileData tile1, TileData tile2, int width, int height)
        {
            var brush = new TileBrush();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = ((x + y) % 2 == 0 ? tile1 : tile2).Clone();
                    brush.AddTile(x, y, tile);
                }
            }
            return brush;
        }
    }

    public class BrushTile
    {
        public int BrushX { get; set; }
        public int BrushY { get; set; }
        public TileData Tile { get; set; }
    }

    public class TileCollider
    {
        public TilePosition Position { get; set; }
        public TileColliderType Type { get; set; }
        public Rect Bounds { get; set; }

        public TileCollider(TilePosition position, TileColliderType type, int cellSize)
        {
            Position = position;
            Type = type;
            Bounds = CalculateBounds(position, type, cellSize);
        }

        private Rect CalculateBounds(TilePosition pos, TileColliderType type, int cellSize)
        {
            int x = pos.X * cellSize;
            int y = pos.Y * cellSize;

            return type switch
            {
                TileColliderType.Full => new Rect(x, y, cellSize, cellSize),
                TileColliderType.TopOnly => new Rect(x, y, cellSize, cellSize * 0.2),
                TileColliderType.LeftHalf => new Rect(x, y, cellSize * 0.5, cellSize),
                TileColliderType.RightHalf => new Rect(x + cellSize * 0.5, y, cellSize * 0.5, cellSize),
                TileColliderType.BottomOnly => new Rect(x, y + cellSize * 0.8, cellSize, cellSize * 0.2),
                TileColliderType.SlopeLeftDown => CreateSlopeBounds(x, y, cellSize, true, true),
                TileColliderType.SlopeLeftUp => CreateSlopeBounds(x, y, cellSize, true, false),
                TileColliderType.SlopeRightDown => CreateSlopeBounds(x, y, cellSize, false, true),
                TileColliderType.SlopeRightUp => CreateSlopeBounds(x, y, cellSize, false, false),
                _ => Rect.Empty
            };
        }

        private Rect CreateSlopeBounds(int x, int y, int cellSize, bool left, bool down)
        {
            int slopeHeight = cellSize;
            if (down)
            {
                return left ? 
                    new Rect(x, y, cellSize, cellSize) : 
                    new Rect(x, y, cellSize, cellSize);
            }
            return new Rect(x, y, cellSize, cellSize);
        }

        public bool Intersects(Rect rect)
        {
            return Bounds.IntersectsWith(rect);
        }

        public Point GetCollisionPoint(Point position, Vector velocity)
        {
            if (!Intersects(new Rect(position.X - 1, position.Y - 1, 2, 2)))
                return position;

            switch (Type)
            {
                case TileColliderType.Full:
                case TileColliderType.TopOnly:
                    return new Point(position.X, Bounds.Y);

                case TileColliderType.LeftHalf:
                    return new Point(Bounds.X + Bounds.Width, position.Y);

                case TileColliderType.RightHalf:
                    return new Point(Bounds.X, position.Y);

                case TileColliderType.BottomOnly:
                    return new Point(position.X, Bounds.Y + Bounds.Height);

                default:
                    return position;
            }
        }
    }

    public class TilePhysicsSystem
    {
        private readonly TileMap tileMap;
        private readonly List<TileCollider> colliders;
        private readonly int cellSize;

        public TilePhysicsSystem(TileMap tileMap)
        {
            this.tileMap = tileMap;
            this.cellSize = tileMap.CellSize;
            this.colliders = new List<TileCollider>();
            BuildColliders();
        }

        public void BuildColliders()
        {
            colliders.Clear();

            for (int x = 0; x < tileMap.Width; x++)
            {
                for (int y = 0; y < tileMap.Height; y++)
                {
                    var tile = tileMap.GetTile(x, y, 0);
                    if (tile != null && tile.HasFlag(TileFlags.Collidable))
                    {
                        colliders.Add(new TileCollider(
                            new TilePosition(x, y), 
                            tile.ColliderType, 
                            cellSize
                        ));
                    }
                }
            }
        }

        public CollisionSide CheckCollision(Rect bounds)
        {
            foreach (var collider in colliders)
            {
                if (collider.Intersects(bounds))
                {
                    if (collider.Type == TileColliderType.TopOnly)
                        return CollisionSide.Top;
                    if (collider.Type == TileColliderType.BottomOnly)
                        return CollisionSide.Bottom;
                    if (collider.Type == TileColliderType.LeftHalf)
                        return CollisionSide.Left;
                    if (collider.Type == TileColliderType.RightHalf)
                        return CollisionSide.Right;
                    return CollisionSide.Any;
                }
            }
            return CollisionSide.None;
        }

        public bool IsColliding(Rect bounds)
        {
            return CheckCollision(bounds) != CollisionSide.None;
        }

        public bool IsCollidingOneWay(Rect bounds, Vector velocity)
        {
            foreach (var collider in colliders)
            {
                if (collider.Type == TileColliderType.TopOnly && velocity.Y >= 0)
                {
                    var feetBounds = new Rect(bounds.X, bounds.Y, bounds.Width, 2);
                    if (collider.Intersects(feetBounds))
                    {
                        double tileTop = collider.Bounds.Y;
                        double feetY = bounds.Y + bounds.Height;
                        if (feetY <= tileTop + 4)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public Rect ResolveCollision(Rect bounds, Vector velocity)
        {
            var result = bounds;

            double remainingX = velocity.X;
            double remainingY = velocity.Y;

            while (Math.Abs(remainingX) > 0.01 || Math.Abs(remainingY) > 0.01)
            {
                double stepX = Math.Abs(remainingX) < 1 ? remainingX : Math.Sign(remainingX);
                double stepY = Math.Abs(remainingY) < 1 ? remainingY : Math.Sign(remainingY);

                result.X += stepX;
                result.Y += stepY;

                var collision = CheckCollision(result);
                if (collision != CollisionSide.None)
                {
                    result.X -= stepX;
                    result.Y -= stepY;

                    if (collision == CollisionSide.Top || collision == CollisionSide.Bottom)
                    {
                        remainingY = 0;
                    }
                    if (collision == CollisionSide.Left || collision == CollisionSide.Right)
                    {
                        remainingX = 0;
                    }
                    if (collision == CollisionSide.Any)
                    {
                        remainingX = 0;
                        remainingY = 0;
                    }
                }
                else
                {
                    remainingX -= stepX;
                    remainingY -= stepY;
                }
            }

            return result;
        }

        public bool IsHazard(TilePosition pos)
        {
            var tile = tileMap.GetTile(pos.X, pos.Y, 0);
            return tile != null && tile.HasFlag(TileFlags.Hazard);
        }

        public bool IsClimbable(TilePosition pos)
        {
            var tile = tileMap.GetTile(pos.X, pos.Y, 0);
            return tile != null && tile.HasFlag(TileFlags.Climbable);
        }
    }

    public class TileMapGenerator
    {
        private static readonly Random random = new Random();

        public static TileMap GenerateFromNoise(int width, int height, int cellSize,
            double scale, double threshold, Func<double, TileData> tileFunc)
        {
            var map = new TileMap(width, height, cellSize);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double nx = x / (double)width * scale;
                    double ny = y / (double)height * scale;
                    double noise = PerlinNoise(nx, ny);

                    var tile = tileFunc(noise);
                    if (noise > threshold)
                    {
                        map.SetTile(x, y, tile.Clone(), 0);
                    }
                }
            }

            return map;
        }

        public static TileMap GenerateDungeon(int width, int height, int cellSize,
            int roomCount, int minRoomSize, int maxRoomSize, 
            Func<TileData> floorTile, Func<TileData> wallTile, Func<TileData> doorTile)
        {
            var map = new TileMap(width, height, cellSize);

            var solidTile = new TileData(1, TileType.Solid, TileFlags.Collidable);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    map.SetTile(x, y, solidTile.Clone(), 0);
                }
            }

            var rooms = new List<Rect>();
            for (int i = 0; i < roomCount; i++)
            {
                int roomWidth = random.Next(minRoomSize, maxRoomSize + 1);
                int roomHeight = random.Next(minRoomSize, maxRoomSize + 1);
                int roomX = random.Next(1, width - roomWidth - 1);
                int roomY = random.Next(1, height - roomHeight - 1);

                var room = new Rect(roomX, roomY, roomWidth, roomHeight);

                if (!rooms.Any(r => r.Intersects(room)))
                {
                    rooms.Add(room);
                    CarveRoom(map, room, floorTile);

                    if (rooms.Count > 1)
                    {
                        var prevRoom = rooms[rooms.Count - 2];
                        CarveHallway(map, GetRoomCenter(prevRoom), GetRoomCenter(room), doorTile);
                    }
                }
            }

            return map;
        }

        private static void CarveRoom(TileMap map, Rect room, Func<TileData> floorTile)
        {
            for (int x = (int)room.X; x < room.X + room.Width; x++)
            {
                for (int y = (int)room.Y; y < room.Y + room.Height; y++)
                {
                    if (x == room.X || x == room.X + room.Width - 1 ||
                        y == room.Y || y == room.Y + room.Height - 1)
                    {
                        map.ClearTile(x, y, 0);
                    }
                    else
                    {
                        map.SetTile(x, y, floorTile().Clone(), 0);
                    }
                }
            }
        }

        private static void CarveHallway(TileMap map, Point start, Point end, Func<TileData> doorTile)
        {
            int x = (int)start.X;
            int y = (int)start.Y;
            int targetX = (int)end.X;
            int targetY = (int)end.Y;

            bool horizontal = random.NextDouble() > 0.5;

            if (horizontal)
            {
                while (x != targetX)
                {
                    map.ClearTile(x, y, 0);
                    map.ClearTile(x, y - 1, 0);
                    x += x < targetX ? 1 : -1;
                }
                while (y != targetY)
                {
                    map.ClearTile(x, y, 0);
                    map.ClearTile(x - 1, y, 0);
                    y += y < targetY ? 1 : -1;
                }
            }
            else
            {
                while (y != targetY)
                {
                    map.ClearTile(x, y, 0);
                    map.ClearTile(x - 1, y, 0);
                    y += y < targetY ? 1 : -1;
                }
                while (x != targetX)
                {
                    map.ClearTile(x, y, 0);
                    map.ClearTile(x, y - 1, 0);
                    x += x < targetX ? 1 : -1;
                }
            }
        }

        private static Point GetRoomCenter(Rect room)
        {
            return new Point(
                (int)(room.X + room.Width / 2),
                (int)(room.Y + room.Height / 2)
            );
        }

        public static TileMap GenerateCave(int width, int height, int cellSize,
            double fillProbability, int smoothingIterations,
            Func<TileData> floorTile, Func<TileData> wallTile)
        {
            var map = new TileMap(width, height, cellSize);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (x == 0 || x == width - 1 || y == 0 || y == height - 1)
                    {
                        map.SetTile(x, y, wallTile().Clone(), 0);
                    }
                    else
                    {
                        if (random.NextDouble() < fillProbability)
                        {
                            map.SetTile(x, y, floorTile().Clone(), 0);
                        }
                    }
                }
            }

            for (int i = 0; i < smoothingIterations; i++)
            {
                map = SmoothCave(map, 4, 3, floorTile, wallTile);
            }

            return map;
        }

        private static TileMap SmoothCave(TileMap map, int birthLimit, int deathLimit,
            Func<TileData> floorTile, Func<TileData> wallTile)
        {
            var newMap = map.Clone();

            for (int x = 1; x < map.Width - 1; x++)
            {
                for (int y = 1; y < map.Height - 1; y++)
                {
                    int neighbors = CountFloorNeighbors(map, x, y);
                    var tile = map.GetTile(x, y, 0);

                    if (tile != null)
                    {
                        if (neighbors > birthLimit)
                        {
                            newMap.SetTile(x, y, floorTile().Clone(), 0);
                        }
                        else if (neighbors < deathLimit)
                        {
                            newMap.SetTile(x, y, wallTile().Clone(), 0);
                        }
                    }
                    else
                    {
                        if (neighbors > birthLimit)
                        {
                            newMap.SetTile(x, y, floorTile().Clone(), 0);
                        }
                    }
                }
            }

            return newMap;
        }

        private static int CountFloorNeighbors(TileMap map, int x, int y)
        {
            int count = 0;
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (i == 0 && j == 0) continue;
                    if (map.GetTile(x + i, y + j, 0) != null)
                        count++;
                }
            }
            return count;
        }

        public static TileMap GenerateIslands(int width, int height, int cellSize,
            double landScale, double moistureScale,
            Func<TileData> deepWater, Func<TileData> shallowWater,
            Func<TileData> sand, Func<TileData> grass, Func<TileData> stone, Func<TileData> mountain)
        {
            var map = new TileMap(width, height, cellSize);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    double nx = x / (double)width - 0.5;
                    double ny = y / (double)height - 0.5;
                    double distance = Math.Sqrt(nx * nx + ny * ny);
                    double elevation = PerlinNoise(x * landScale, y * landScale) - distance * 1.5;
                    double moisture = PerlinNoise(x * moistureScale + 100, y * moistureScale + 100);

                    TileData tile;
                    if (elevation < -0.3)
                        tile = deepWater();
                    else if (elevation < 0)
                        tile = shallowWater();
                    else if (elevation < 0.1)
                        tile = sand();
                    else if (elevation < 0.5)
                        tile = moisture > 0.5 ? grass() : grass();
                    else if (elevation < 0.7)
                        tile = stone();
                    else
                        tile = mountain();

                    map.SetTile(x, y, tile.Clone(), 0);
                }
            }

            return map;
        }

        private static double PerlinNoise(double x, double y)
        {
            int xi = (int)Math.Floor(x) & 255;
            int yi = (int)Math.Floor(y) & 255;
            double xf = x - Math.Floor(x);
            double yf = y - Math.Floor(y);

            double u = Fade(xf);
            double v = Fade(yf);

            int aa = Hash(xi, yi);
            int ab = Hash(xi, yi + 1);
            int ba = Hash(xi + 1, yi);
            int bb = Hash(xi + 1, yi + 1);

            double x1 = Lerp(Grad(aa, xf, yf), Grad(ba, xf - 1, yf), u);
            double x2 = Lerp(Grad(ab, xf, yf - 1), Grad(bb, xf - 1, yf - 1), u);

            return (Lerp(x1, x2, v) + 1) / 2;
        }

        private static double Fade(double t) => t * t * t * (t * (t * 6 - 15) + 10);
        private static double Lerp(double a, double b, double t) => a + t * (b - a);
        private static double Grad(int hash, double x, double y)
        {
            int h = hash & 3;
            double u = h < 2 ? x : y;
            double v = h < 2 ? y : x;
            return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
        }

        private static int Hash(int x, int y)
        {
            int hash = (x * 374761393 + y * 668265263) ^ (x * 1274126177);
            hash = (hash ^ (hash >> 13)) * 1274126177;
            return Math.Abs(hash) % 256;
        }

        public static TileMap LoadFromArray(int[,] tileIds, int cellSize, TileData[] definitions)
        {
            int width = tileIds.GetLength(0);
            int height = tileIds.GetLength(1);
            var map = new TileMap(width, height, cellSize);

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    int tileId = tileIds[x, y];
                    if (tileId >= 0 && tileId < definitions.Length)
                    {
                        var tile = definitions[tileId].Clone();
                        tile.TileId = tileId;
                        map.SetTile(x, y, tile, 0);
                    }
                }
            }

            return map;
        }

        public static int[,] SaveToArray(TileMap map, int emptyId = -1, int layerIndex = 0)
        {
            var array = new int[map.Width, map.Height];

            for (int x = 0; x < map.Width; x++)
            {
                for (int y = 0; y < map.Height; y++)
                {
                    var tile = map.GetTile(x, y, layerIndex);
                    array[x, y] = tile?.TileId ?? emptyId;
                }
            }

            return array;
        }
    }

    public class TileMapEventArgs : EventArgs
    {
        public TilePosition Position { get; set; }
        public TileData Tile { get; set; }
        public int LayerIndex { get; set; }
        public TileData PreviousTile { get; set; }

        public TileMapEventArgs(TilePosition position, TileData tile, TileData previousTile, int layerIndex)
        {
            Position = position;
            Tile = tile;
            PreviousTile = previousTile;
            LayerIndex = layerIndex;
        }
    }

    public class TileMapEvents
    {
        private readonly TileMap tileMap;
        private readonly Dictionary<TilePosition, List<Action<TileMapEventArgs>>> listeners;

        public event EventHandler<TileMapEventArgs> TileChanged;
        public event EventHandler<TileMapEventArgs> TilePlaced;
        public event EventHandler<TileMapEventArgs> TileRemoved;

        public TileMapEvents(TileMap tileMap)
        {
            this.tileMap = tileMap;
            listeners = new Dictionary<TilePosition, List<Action<TileMapEventArgs>>>();
        }

        public void Subscribe(TilePosition position, Action<TileMapEventArgs> callback)
        {
            if (!listeners.ContainsKey(position))
            {
                listeners[position] = new List<Action<TileMapEventArgs>>();
            }
            listeners[position].Add(callback);
        }

        public void Unsubscribe(TilePosition position, Action<TileMapEventArgs> callback)
        {
            if (listeners.ContainsKey(position))
            {
                listeners[position].Remove(callback);
            }
        }

        public void NotifyTileChanged(TilePosition position, TileData newTile, TileData oldTile, int layerIndex)
        {
            var args = new TileMapEventArgs(position, newTile, oldTile, layerIndex);
            TileChanged?.Invoke(this, args);

            if (listeners.TryGetValue(position, out var callbacks))
            {
                foreach (var callback in callbacks)
                {
                    callback(args);
                }
            }
        }

        public void NotifyTilePlaced(TilePosition position, TileData tile, int layerIndex)
        {
            var args = new TileMapEventArgs(position, tile, null, layerIndex);
            TilePlaced?.Invoke(this, args);
        }

        public void NotifyTileRemoved(TilePosition position, TileData tile, int layerIndex)
        {
            var args = new TileMapEventArgs(position, null, tile, layerIndex);
            TileRemoved?.Invoke(this, args);
        }
    }

    public class TileRegionQuery
    {
        private readonly TileMap tileMap;

        public TileRegionQuery(TileMap tileMap)
        {
            this.tileMap = tileMap;
        }

        public List<TilePosition> QueryTilesInRect(Rect worldRect)
        {
            var result = new List<TilePosition>();
            var start = tileMap.Bounds.WorldToTile(worldRect.X, worldRect.Y);
            var end = tileMap.Bounds.WorldToTile(worldRect.Right, worldRect.Bottom);

            for (int x = start.X; x <= end.X; x++)
            {
                for (int y = start.Y; y <= end.Y; y++)
                {
                    if (tileMap.IsInBounds(x, y))
                    {
                        result.Add(new TilePosition(x, y));
                    }
                }
            }

            return result;
        }

        public List<TilePosition> QueryTilesOfType(TileType type, int layerIndex = 0)
        {
            var result = new List<TilePosition>();

            for (int x = 0; x < tileMap.Width; x++)
            {
                for (int y = 0; y < tileMap.Height; y++)
                {
                    var tile = tileMap.GetTile(x, y, layerIndex);
                    if (tile != null && tile.Type == type)
                    {
                        result.Add(new TilePosition(x, y));
                    }
                }
            }

            return result;
        }

        public List<TilePosition> QueryTilesWithFlag(TileFlags flag, int layerIndex = 0)
        {
            var result = new List<TilePosition>();

            for (int x = 0; x < tileMap.Width; x++)
            {
                for (int y = 0; y < tileMap.Height; y++)
                {
                    var tile = tileMap.GetTile(x, y, layerIndex);
                    if (tile != null && tile.HasFlag(flag))
                    {
                        result.Add(new TilePosition(x, y));
                    }
                }
            }

            return result;
        }

        public List<TilePosition> QueryTilesNear(TilePosition center, int radius)
        {
            var result = new List<TilePosition>();

            for (int x = center.X - radius; x <= center.X + radius; x++)
            {
                for (int y = center.Y - radius; y <= center.Y + radius; y++)
                {
                    if (tileMap.IsInBounds(x, y) && 
                        Math.Abs(x - center.X) + Math.Abs(y - center.Y) <= radius)
                    {
                        result.Add(new TilePosition(x, y));
                    }
                }
            }

            return result;
        }
    }

    public class TileMapSerializer
    {
        public static string SerializeToJson(TileMap map)
        {
            var json = new System.Text.StringBuilder();
            json.AppendLine("{");
            json.AppendLine($"\"width\": {map.Width},");
            json.AppendLine($"\"height\": {map.Height},");
            json.AppendLine($"\"cellSize\": {map.CellSize},");
            json.AppendLine($"\"layers\": [");

            for (int l = 0; l < map.Layers.Count; l++)
            {
                var layer = map.Layers[l];
                json.AppendLine("{");
                json.AppendLine($"\"name\": \"{layer.Name}\",");
                json.AppendLine($"\"visible\": {layer.IsVisible.ToString().ToLower()},");
                json.AppendLine($"\"opacity\": {layer.Opacity},");
                json.AppendLine($"\"tiles\": [");

                var tiles = layer.GetAllTiles().ToList();
                for (int i = 0; i < tiles.Count; i++)
                {
                    var (x, y, tile) = tiles[i];
                    json.Append("{");
                    json.Append($"\"x\":{x},\"y\":{y},\"id\":{tile.TileId}");
                    json.Append("}");
                    if (i < tiles.Count - 1) json.Append(",");
                }

                json.AppendLine("]");
                json.Append("}");
                if (l < map.Layers.Count - 1) json.Append(",");
                json.AppendLine();
            }

            json.AppendLine("]");
            json.AppendLine("}");

            return json.ToString();
        }

        public static TileMap DeserializeFromJson(string json, TileData[] definitions)
        {
            var parser = new JsonParser(json);
            var obj = parser.Parse();

            int width = (int)obj["width"];
            int height = (int)obj["height"];
            int cellSize = (int)obj["cellSize"];

            var map = new TileMap(width, height, cellSize);
            map.Layers.Clear();

            var layers = (JsonArray)obj["layers"];
            foreach (var layerObj in layers)
            {
                var layerDict = (JsonObject)layerObj;
                var layer = new TileLayer(width, height, (string)layerDict["name"])
                {
                    IsVisible = (bool)layerDict["visible"],
                    Opacity = (double)layerDict["opacity"]
                };

                var tiles = (JsonArray)layerDict["tiles"];
                foreach (var tileObj in tiles)
                {
                    var tileDict = (JsonObject)tileObj;
                    int x = (int)tileDict["x"];
                    int y = (int)tileDict["y"];
                    int id = (int)tileDict["id"];

                    if (id >= 0 && id < definitions.Length)
                    {
                        var tile = definitions[id].Clone();
                        tile.TileId = id;
                        layer.SetTile(x, y, tile);
                    }
                }

                map.Layers.Add(layer);
            }

            return map;
        }
    }

    internal class JsonValue
    {
        public object Value { get; set; }
        public JsonValue(object value) { Value = value; }
        public static implicit operator int(JsonValue v) => (int)v.Value;
        public static implicit operator double(JsonValue v) => (double)v.Value;
        public static implicit operator bool(JsonValue v) => (bool)v.Value;
        public static implicit operator string(JsonValue v) => (string)v.Value;
    }

    internal class JsonObject : Dictionary<string, JsonValue> { }

    internal class JsonArray : List<JsonValue> { }

    internal class JsonParser
    {
        private string json;
        private int index;

        public JsonParser(string json)
        {
            this.json = json;
            index = 0;
        }

        public JsonObject Parse()
        {
            SkipWhitespace();
            return ParseObject();
        }

        private JsonObject ParseObject()
        {
            var obj = new JsonObject();
            Expect('{');
            SkipWhitespace();

            if (Peek() != '}')
            {
                do
                {
                    SkipWhitespace();
                    string key = ParseString();
                    SkipWhitespace();
                    Expect(':');
                    SkipWhitespace();
                    obj[key] = ParseValue();
                    SkipWhitespace();
                } while (TryExpect(','));
            }

            Expect('}');
            return obj;
        }

        private JsonValue ParseValue()
        {
            SkipWhitespace();
            char c = Peek();

            if (c == '{') return new JsonValue(ParseObject());
            if (c == '[') return new JsonValue(ParseArray());
            if (c == '"') return new JsonValue(ParseString());
            if (c == 't' || c == 'f') return new JsonValue(ParseBool());
            if (c == '-' || char.IsDigit(c)) return new JsonValue(ParseNumber());

            return new JsonValue(null);
        }

        private JsonArray ParseArray()
        {
            var arr = new JsonArray();
            Expect('[');
            SkipWhitespace();

            if (Peek() != ']')
            {
                do
                {
                    arr.Add(ParseValue());
                    SkipWhitespace();
                } while (TryExpect(','));
            }

            Expect(']');
            return arr;
        }

        private string ParseString()
        {
            Expect('"');
            var sb = new System.Text.StringBuilder();

            while (Peek() != '"')
            {
                sb.Append(Next());
            }

            Expect('"');
            return sb.ToString();
        }

        private double ParseNumber()
        {
            var sb = new System.Text.StringBuilder();

            if (Peek() == '-') sb.Append(Next());

            while (char.IsDigit(Peek()))
            {
                sb.Append(Next());
            }

            if (Peek() == '.')
            {
                sb.Append(Next());
                while (char.IsDigit(Peek()))
                {
                    sb.Append(Next());
                }
            }

            return double.Parse(sb.ToString());
        }

        private bool ParseBool()
        {
            if (Peek() == 't')
            {
                Expect("true");
                return true;
            }
            Expect("false");
            return false;
        }

        private char Peek() => index < json.Length ? json[index] : '\0';
        private char Next() => json[index++];

        private void Expect(char c)
        {
            if (Next() != c)
                throw new Exception($"Expected '{c}'");
        }

        private bool TryExpect(char c)
        {
            if (Peek() == c)
            {
                index++;
                return true;
            }
            return false;
        }

        private void Expect(string s)
        {
            foreach (char c in s)
            {
                Expect(c);
            }
        }

        private void SkipWhitespace()
        {
            while (index < json.Length && char.IsWhiteSpace(json[index]))
            {
                index++;
            }
        }
    }
}

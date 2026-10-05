using System;
using System.Text.Json.Serialization;

namespace IndoorSceneEditor.Models
{
    /// <summary>
    /// 场景中的一个可交换室内物品。
    /// 所有字段都是可序列化的，用于跨应用交换的 JSON 格式。
    /// </summary>
    public class SceneItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString("N");

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Item";

        [JsonPropertyName("type")]
        public ItemType Type { get; set; } = ItemType.Sofa;

        [JsonPropertyName("position")]
        public Vec3 Position { get; set; } = new Vec3();

        /// <summary>绕 Y 轴的旋转角度（度）。</summary>
        [JsonPropertyName("rotationY")]
        public double RotationY { get; set; }

        [JsonPropertyName("scale")]
        public double Scale { get; set; } = 1.0;

        /// <summary>十六进制颜色，例如 "#8B4513"。</summary>
        [JsonPropertyName("color")]
        public string Color { get; set; } = "#8B4513";

        public SceneItem Clone() => new SceneItem
        {
            Id = Id,
            Name = Name,
            Type = Type,
            Position = new Vec3(Position.X, Position.Y, Position.Z),
            RotationY = RotationY,
            Scale = Scale,
            Color = Color
        };
    }

    /// <summary>三维向量，用于 JSON 序列化。</summary>
    public class Vec3
    {
        [JsonPropertyName("x")]
        public double X { get; set; }
        [JsonPropertyName("y")]
        public double Y { get; set; }
        [JsonPropertyName("z")]
        public double Z { get; set; }

        public Vec3() { }
        public Vec3(double x, double y, double z) { X = x; Y = y; Z = z; }
    }
}

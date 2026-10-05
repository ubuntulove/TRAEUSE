using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace IndoorSceneEditor.Models
{
    /// <summary>
    /// 可交换的 3D 室内场景文档。
    /// 这是该工具的核心交换格式（indoor-scene/v1）。
    /// </summary>
    public class SceneDocument
    {
        /// <summary>格式标识符，便于其他应用识别。</summary>
        [JsonPropertyName("format")]
        public string Format { get; set; } = "indoor-scene/v1";

        [JsonPropertyName("version")]
        public int Version { get; set; } = 1;

        [JsonPropertyName("name")]
        public string Name { get; set; } = "Untitled Scene";

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("modifiedAt")]
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        [JsonPropertyName("items")]
        public List<SceneItem> Items { get; set; } = new List<SceneItem>();
    }
}

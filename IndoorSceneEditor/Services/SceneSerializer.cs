using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using IndoorSceneEditor.Models;

namespace IndoorSceneEditor.Services
{
    /// <summary>
    /// 场景文档的 JSON 序列化/反序列化。
    /// 使用 System.Text.Json，输出紧凑且可读的可交换格式。
    /// </summary>
    public static class SceneSerializer
    {
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public static string Serialize(SceneDocument doc)
        {
            doc.ModifiedAt = System.DateTime.Now;
            return JsonSerializer.Serialize(doc, Options);
        }

        public static SceneDocument? Deserialize(string json)
        {
            return JsonSerializer.Deserialize<SceneDocument>(json, Options);
        }

        public static void Save(SceneDocument doc, string path)
        {
            File.WriteAllText(path, Serialize(doc));
        }

        public static SceneDocument? Load(string path)
        {
            var json = File.ReadAllText(path);
            return Deserialize(json);
        }
    }
}

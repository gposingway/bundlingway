using System.Text.Json;

namespace Bundlingway.Utilities
{
    public static class Serialization
    {

        private static JsonSerializerOptions _options = new() { WriteIndented = true };

        public static string ToJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        public static T? FromJson<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }
        public static void ToJsonFile<T>(this T obj, string filePath)
        {
            var json = JsonSerializer.Serialize(obj, _options);
            File.WriteAllText(filePath, json);
        }

        public static T FromJsonFile<T>(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return JsonSerializer.Deserialize<T>(json);
            }
            return default;
        }

    }
}

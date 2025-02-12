using Serilog;
using System.Text.Json;

namespace Bundlingway.Utilities.Extensions
{
    public static class SerializationExtensions
    {

        private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

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
            try
            {
                File.WriteAllText(filePath, json);

            }
            catch (Exception e)
            {
                Log.Information($"Error writing to file {filePath}: {e.Message}");

            }
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

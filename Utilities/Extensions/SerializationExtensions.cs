using Serilog;
using System.Text.Json;

namespace Bundlingway.Utilities.Extensions
{
    /// <summary>
    /// Provides extension methods for serializing and deserializing objects to and from JSON.
    /// </summary>
    public static class SerializationExtensions
    {
        // JSON serializer options with indented formatting
        private static readonly JsonSerializerOptions _options = new() { WriteIndented = true };

        /// <summary>
        /// Serializes an object to a JSON string.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <returns>A JSON string representation of the object.</returns>
        public static string ToJson<T>(this T obj)
        {
            return JsonSerializer.Serialize(obj, _options);
        }

        /// <summary>
        /// Deserializes a JSON string to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An object of type T deserialized from the JSON string.</returns>
        public static T? FromJson<T>(this string json)
        {
            return JsonSerializer.Deserialize<T>(json);
        }

        /// <summary>
        /// Serializes an object to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="filePath">The file path where the JSON will be saved.</param>
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

        /// <summary>
        /// Deserializes a JSON file to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The file path of the JSON file to deserialize.</param>
        /// <returns>An object of type T deserialized from the JSON file.</returns>
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

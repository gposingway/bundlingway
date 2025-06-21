using Newtonsoft.Json;
using Serilog;
using System.ComponentModel;
using System.Reflection;

namespace Bundlingway.Utilities.Extensions
{



    /// <summary>
    /// Provides extension methods for serializing and deserializing objects to and from JSON.
    /// </summary>
    public static class SerializationExtensions
    {
        /// <summary>
        /// Returns the description from the DescriptionAttribute of an enum value, if present.
        /// Otherwise, returns the enum value's name as a string.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>The description or enum name as a string.</returns>
        public static string GetDescription(this Enum enumValue)
        {
            if (enumValue == null)
            {
                return null; // Or throw an ArgumentNullException if null is not allowed
            }

            return enumValue.GetType()
                       .GetMember(enumValue.ToString())
                       .FirstOrDefault()
                       ?.GetCustomAttribute<DescriptionAttribute>()
                       ?.Description
                   ?? enumValue.ToString(); // Fallback to enum name if no description
        }

        public static string? ToJson<T>(this T obj, Formatting format = Formatting.Indented)
        {
             try
            {
                return JsonConvert.SerializeObject(obj, format);
            }            catch (Exception ex)
            {
                Log.Warning($"Failed to serialize object to JSON: {ex.Message}");
                return null;
            }
        }

        public static string ToSingleLineJson<T>(this T obj) => ToJson(obj, Formatting.None);

        /// <summary>
        /// Deserializes a JSON string to an object of type T.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="json">The JSON string to deserialize.</param>
        /// <returns>An object of type T deserialized from the JSON string.</returns>

        public static T FromJson<T>(this string obj) => obj == null ? default : JsonConvert.DeserializeObject<T>(obj);

        /// <summary>
        /// Serializes an object to a JSON file.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="obj">The object to serialize.</param>
        /// <param name="filePath">The file path where the JSON will be saved.</param>
        public static void ToJsonFile<T>(this T obj, string filePath)
        {
            var json = obj.ToJson();
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
        public static T FromJsonFile<T>(this string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                return json.FromJson<T>();
            }
            return default;
        }
    }
}

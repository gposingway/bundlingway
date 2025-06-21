using Newtonsoft.Json;
using System.ComponentModel;
using System.Reflection;

namespace Bundlingway.Utilities.Extensions
{
    public class DescriptionConverter : JsonConverter<Enum> // Inherit from JsonConverter<Enum>
    {
        public override void WriteJson(JsonWriter writer, Enum? value, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }
            DescriptionAttribute? attribute = value
                .GetType()
                .GetMember(value.ToString())
                .FirstOrDefault()
                ?.GetCustomAttribute<DescriptionAttribute>();

            writer.WriteValue(attribute?.Description ?? value.ToString()); // Serialize description or enum name
        }

        public override Enum ReadJson(JsonReader reader, Type objectType, Enum? existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string? description = reader.Value?.ToString();
                if (description == null)
                    return existingValue ?? (Enum)Activator.CreateInstance(objectType)!;
                foreach (string name in Enum.GetNames(objectType))
                {
                    var member = objectType.GetMember(name).FirstOrDefault();
                    var attribute = member?.GetCustomAttribute<DescriptionAttribute>();
                    if (attribute != null && attribute.Description == description)
                    {
                        return (Enum)Enum.Parse(objectType, name);
                    }
                }
                // Fallback to parsing by name if description not found (for robustness)
                return (Enum)Enum.Parse(objectType, description); // Try parsing by name as fallback
            }
            return existingValue ?? (Enum)Activator.CreateInstance(objectType)!; // Or throw an exception if non-string input is unexpected
        }
    }
}

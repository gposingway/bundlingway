using Newtonsoft.Json.Linq;

namespace Bundlingway.Utilities.Extensions
{
    public static class JsonExtensions
    {
        public static string GetTokenValue(this string jsonString, string token)
        {
            return JObject.Parse(jsonString).SelectToken(token)?.ToString() ?? string.Empty;
        }

        public static string GetTokenValueFromFile(this string filePath, string token)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"The file at path {filePath} does not exist.");
            }

            var jsonString = File.ReadAllText(filePath);
            return jsonString.GetTokenValue(token);
        }
    }
}

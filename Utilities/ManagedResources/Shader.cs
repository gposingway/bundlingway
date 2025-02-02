using Bundlingway.Utilities.Extensions;
using System.Text.RegularExpressions;

namespace Bundlingway.Utilities.ManagedResources
{
    public static class Shader
    {
        public static async Task<Dictionary<string, string>> GetSignatures(string path)
        {
            var signatures = new Dictionary<string, string>();
            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                 .Where(file => Constants.TextureExtensions.Contains(Path.GetExtension(file).ToLower()))
                                 .ToArray();
            foreach (var file in files)
            {
                var content = await File.ReadAllTextAsync(file);
                var signatureList = GetUniqueTechniqueIdentifiers(content);

                foreach (var signature in signatureList)
                {
                        signatures[signature.Sha512()] = signature;
                }


            }
            return signatures;
        }

        public static List<string> GetUniqueTechniqueIdentifiers(string shaderFilePath)
        {
            List<string> identifiers = [];

            try
            {
                string fileName = Path.GetFileNameWithoutExtension(shaderFilePath).ToLower(); // Get file name without extension and lowercase it
                string fileContent = File.ReadAllText(shaderFilePath);

                // Regular expression to find technique blocks
                // Improved regex to handle variations in whitespace and case-insensitivity
                string techniquePattern = @"\btechnique\s+(?<techniqueName>\w+)\b\s*{[^}]*}"; // Matches "technique <name> { ... }"

                MatchCollection techniqueMatches = Regex.Matches(fileContent, techniquePattern, RegexOptions.IgnoreCase);

                foreach (Match match in techniqueMatches)
                {
                    string techniqueName = match.Groups["techniqueName"].Value.ToLower(); // Extract technique name and lowercase it

                    identifiers.Add($"{fileName}.{techniqueName}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading or parsing shader file: {ex.Message}");
                // Handle the exception appropriately, e.g., return an empty list or throw the exception.
                return new List<string>(); // Return empty list in case of error.
            }

            return identifiers;
        }
    }
}

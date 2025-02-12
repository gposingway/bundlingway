using Bundlingway.Utilities.Extensions;
using Serilog;

namespace Bundlingway.Utilities.ManagedResources
{
    public class ShaderSignature
    {
        public string FileName { get; set; }
        public string Location { get; set; }
        public Dictionary<string, string> Techniques { get; set; }
        public string Hash { get; set; }
        public List<string> Dependencies { get; set; }

        public override string ToString()
        {
            return $"{FileName} - {Hash}";
        }
    }

    public static class Shader
    {

        public static async Task<object> CheckForConflicts(this Dictionary<string, ShaderSignature> shaderSignatures)
        {
            var repeatedTechniqueGroups = shaderSignatures
                .SelectMany(kvp => kvp.Value.Techniques.Select(t => new { Hash = kvp.Key, Technique = t.Value }))
                .GroupBy(t => t.Technique)
                .Where(g => g.Count() > 1)
                .ToList();

            return repeatedTechniqueGroups;
        }

        public class ShaderConflictAnalysis
        {
            public List<ShaderSignature> Conflicting { get; set; }
            public List<ShaderSignature> NonConflicting { get; set; }
        }

        public static async Task<ShaderConflictAnalysis> CheckForConflictsWith(this Dictionary<string, ShaderSignature> signatureSet1, Dictionary<string, ShaderSignature> signatureSet2)
        {
            var result = new ShaderConflictAnalysis();

            var techniques1 = signatureSet1
                .SelectMany(kvp => kvp.Value.Techniques.Select(t => new { Hash = kvp.Key, Technique = t.Value }))
                .ToList();

            var techniques2 = signatureSet2
                .SelectMany(kvp => kvp.Value.Techniques.Select(t => new { Hash = kvp.Key, Technique = t.Value }))
                .ToList();

            var conflictingTechniques = techniques1
                .Join(techniques2, t1 => t1.Technique, t2 => t2.Technique, (t1, t2) => new { t1.Technique, Hash1 = t1.Hash, Hash2 = t2.Hash })
                .GroupBy(t => t.Technique)
                .ToList();

            var conflictingFilesMap = conflictingTechniques
                .SelectMany(g => g.Select(t => t.Hash2))
                .Distinct()
                .ToDictionary(i => i, i => signatureSet2[i]);

            ShaderSignature newConflict = null;


            try
            {
                do
                {
                    newConflict = null;

                    foreach (var item in conflictingFilesMap)
                    {
                        if (item.Value.Dependencies?.Count > 0)
                        {
                            foreach (var item2 in item.Value.Dependencies)
                            {
                                var preTarget = signatureSet2.Where(i => i.Value.FileName == item2);

                                if (preTarget.Count() == 0) continue;
                                var target = preTarget.First();

                                if (!conflictingFilesMap.ContainsKey(target.Key))
                                {
                                    newConflict = target.Value;
                                    break;
                                }
                            }

                        }

                        if (newConflict != null) break;

                    }

                    if (newConflict != null)
                    {
                        conflictingFilesMap[newConflict.Hash] = newConflict;
                    }

                } while (newConflict != null);
            }
            catch (Exception e)
            {
                Log.Warning($"Error while checking for conflicts: {e.Message}");
            }


            var installedFiles = signatureSet1.Select(i => i.Value.FileName.ToLower()).ToList();
            var repeatFiles = signatureSet2.Where(i => installedFiles.Contains(i.Value.FileName.ToLower())).ToList();



                foreach (var entry in repeatFiles)
                {
                    if (!conflictingFilesMap.ContainsKey(entry.Key))
                    {
                        conflictingFilesMap[entry.Key] = entry.Value;
                    }
                }
     
            
            var nonConflictingFiles = signatureSet2
                .Where(kvp => !conflictingFilesMap.Any(i => i.Key == kvp.Key))
                .Select(kvp => kvp.Value)
                .ToList();

            var conflictingFiles = signatureSet2
                .Where(kvp => conflictingFilesMap.Any(i => i.Key == kvp.Key))
                .Select(kvp => kvp.Value)
                .ToList();

            result.Conflicting = conflictingFiles;
            result.NonConflicting = nonConflictingFiles;

            return result;
        }

        public static async Task SaveShaderAnalysisToPath(string shaderFolderPath, string destination)
        {
            var analysis = await GetShaderFolderSignatures(shaderFolderPath);
            analysis.ToJsonFile(destination);

            _ = analysis.CheckForConflicts();
        }

        public static async Task<Dictionary<string, ShaderSignature>> GetShaderFolderSignatures(string path)
        {
            var signatures = new Dictionary<string, ShaderSignature>();

            var files = Directory.GetFiles(path, "*.*", SearchOption.AllDirectories)
                                 .Where(file => Constants.ShaderExtensions.Contains(Path.GetExtension(file).ToLower()))
                                 .ToArray();

            foreach (var file in files)
            {
                var shaderSig = await GetShaderSignature(file);
                signatures[shaderSig.Hash] = shaderSig;
            }

            return signatures;
        }

        public static async Task<ShaderSignature> GetShaderSignature(string shaderFilePath)
        {

            var result = new ShaderSignature();


            result.GetTechniqueIdentifiers(shaderFilePath);

            result.Location = Path.GetDirectoryName(shaderFilePath);
            result.FileName = Path.GetFileName(shaderFilePath);
            result.Hash = CriptographyExtensions.MD5FromFile(shaderFilePath);


            return result;
        }

        public static void GetTechniqueIdentifiers(this ShaderSignature shaderSignature, string shaderFilePath)
        {
            Dictionary<string, string> identifiers = [];

            try
            {
                string fileName = Path.GetFileName(shaderFilePath);
                string fileContent = File.ReadAllText(shaderFilePath);

                List<string> fileLines = fileContent.Split('\n', StringSplitOptions.None).ToList();

                List<string> techniqueMatches = fileLines
                    .Where(i => i.Trim().StartsWith("technique ", StringComparison.InvariantCultureIgnoreCase))
                    .Select(i => i.Trim().Split(' ')[1].Split('<')[0])
                    .ToList();

                foreach (string match in techniqueMatches)
                {
                    var entry = $"{fileName}::{match}";
                    identifiers[entry.MD5()] = entry;
                }

                shaderSignature.Techniques = identifiers;

                List<string> dependencies = fileLines
                    .Where(i => i.Trim().StartsWith("#include", StringComparison.InvariantCultureIgnoreCase))
                    .Select(i => i.Trim().Split('"')[1].Split('<')[0])
                    .ToList();

                shaderSignature.Dependencies = dependencies;

            }
            catch (Exception ex)
            {
                Log.Warning($"Error reading or parsing shader file: {ex.Message}");
            }
        }
    }
}

using Bundlingway.Model;
using Bundlingway.PostProcess.PresetItem;
using IniParser;
using Serilog;
using System.Text.RegularExpressions;

namespace Bundlingway.Utilities.Extensions
{
    public static class PostProcessorExtensions
    {

        public static void ReplaceValues(string folderPath, Dictionary<string, string> replacements)
        {
            if (!Directory.Exists(folderPath))
            {
                Log.Information($"Folder '{folderPath}' does not exist.");
                return;
            }

            foreach (var filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                try
                {
                    string originalContent = File.ReadAllText(filePath);
                    string content = File.ReadAllText(filePath);

                    foreach (var kvp in replacements)
                    {
                        content = content.Replace(kvp.Key, kvp.Value);
                    }

                    if (originalContent != content)
                    {
                        Log.Information("[Replacement] - " + filePath);
                        File.WriteAllText(filePath, content);
                    }
                }
                catch (Exception ex)
                {
                    Log.Information($"Error processing file '{filePath}': {ex.Message}");
                }
            }
        }

        public static void RunRawFilePipeline(this ResourcePackage package, Logging _logger)
        {
            var baseline = Path.Combine(Instances.PackageFolder, package.Name);
            var presetPath = Path.Combine(baseline, "Presets");

            List<string> textureFiles = Directory.GetFiles(presetPath, "*.*", SearchOption.AllDirectories).ToList();

            foreach (var processor in Instances.RawFileProcessors)
            {
                var renameMap = processor.GetReplacementMap(package, textureFiles, baseline, _logger);

                if (renameMap == null) continue;
                if (renameMap.Count == 0) continue;

                if (processor.ApplyToPresets) ReplaceValues(presetPath, renameMap);
            }
        }

        public static Preset RunPostProcessorPipeline(this Preset preset, ResourcePackage package, IniParser.Model.IniData iniData, Logging _logger)
        {
            var baseline = Path.Combine(Instances.PackageFolder, package.Name);
            var presetPath = Path.Combine(baseline, "Presets");
            List<string> textureFiles = Directory.GetFiles(presetPath, "*.*", SearchOption.AllDirectories).ToList();
            var parser = new FileIniDataParser();
            var mustUpdate = false;

            foreach (var process in Instances.PresetProcessors)
            {
                ITemplatePostProcess templateProcess = null;

                if (process is ITemplatePostProcess)
                    templateProcess = (ITemplatePostProcess)process;

                var techniqueList = preset.Techniques.ToList();

                if (templateProcess != null)
                {
                    if (templateProcess.PresetExclusionList?.Any(i => preset.Filename.EndsWith(i, StringComparison.InvariantCultureIgnoreCase)) == true) continue;

                    if (templateProcess?.Techniques?.StartWith?.Count != 0)
                    {
                        // There's a list of techniques we should start with. Let's invert and iterate.

                        var preList = templateProcess.Techniques.StartWith;
                        preList.Reverse();

                        foreach (var item in preList)
                        {
                            techniqueList.RemoveAll(i => i.Key == item);
                            techniqueList.Insert(0, new KeyValuePair<string, bool>(item, true));
                        }
                    }

                    if (templateProcess?.Techniques?.EndWith?.Count != 0)
                    {
                        // There's a list of techniques we should end with.

                        foreach (var item in templateProcess?.Techniques?.EndWith)
                        {
                            techniqueList.RemoveAll(i => i.Key == item);
                            techniqueList.Add(new KeyValuePair<string, bool>(item, true));
                        }
                    }

                    preset.Techniques = techniqueList.ToDictionary(i => i.Key, i => i.Value);
                }

                if (process?.PostProcess(preset) == true) mustUpdate = true;

                if (templateProcess != null)
                {
                    if (templateProcess?.RootElements != null)
                    {
                        foreach (var re in templateProcess.RootElements)
                        {
                            if (iniData.Global[re.Key] != re.Value)
                            {
                                iniData.Global[re.Key] = re.Value;
                                mustUpdate = true;
                            }
                        }
                    }

                    if (templateProcess?.Sections.Any() == true)
                    {
                        foreach (var section in templateProcess.Sections)
                        {
                            if (!iniData.Sections.Any(i => i.SectionName == section.Name))
                            {
                                iniData.Sections.AddSection(section.Name);
                                mustUpdate = true;
                            }

                            foreach (var sItem in section.Parameters)
                            {
                                iniData[section.Name][sItem.Key] = sItem.Value;
                            }
                        }
                    }
                }
            }

            if (preset.Techniques.ToJson() != preset.OriginalTechniques.ToJson())
            {
                var activeTechniques = preset.Techniques.ToList().Where(i => i.Value == true).Select(i => i.Key).ToList();

                var activeList = string.Join(',', activeTechniques);
                var allList = string.Join(',', preset.Techniques.Select(i => i.Key).ToList());

                //Write to the preset.
                iniData.Global["Techniques"] = activeList;
                iniData.Global["TechniqueSorting"] = allList;

                mustUpdate = true;
            }

            if (mustUpdate) parser.WriteFile(preset.Filename, iniData);

            return preset;
        }

        private static readonly List<string> TechniqueIgnoreList = [""];

        public static List<string> GetTextures(string filePath)
        {
            string fileContent = File.ReadAllText(filePath);

            // Regex pattern to match filenames in quotes
            string pattern = "\"([^\"]*\\.(png|jpg|jpeg|gif|bmp))\"";

            return Regex.Matches(fileContent, pattern).Select(i => i.Value.Substring(1, i.Value.Length - 2)).ToList();
        }

        public static Preset ToPreset(this IniParser.Model.IniData source, string iniFile)
        {
            var model = new Preset
            {
                Filename = iniFile,
                IniHandler = source,
                TextureFiles = GetTextures(iniFile)
            };

            try
            {
                var sourceTechniqueList = source.Global["Techniques"];
                var sourceTechSortList = source.Global["TechniqueSorting"] ?? sourceTechniqueList;

                if (sourceTechSortList == null) return model;

                var techniques = sourceTechniqueList.Split(',', StringSplitOptions.None).ToList();
                var techSort = sourceTechSortList.Split(',', StringSplitOptions.None).Where(i => !TechniqueIgnoreList.Contains(i)).ToList();

                foreach (var technique in techSort)
                    model.Techniques[technique] = false;

                foreach (var technique in techniques)
                    if (model.Techniques.ContainsKey(technique)) model.Techniques[technique] = true;

                var techniqueList = model.Techniques.ToList();

                // First, determine the last active technique:
                var first = techniqueList.Where(i => i.Value).FirstOrDefault();

                if (first.Value != false)
                {
                    var firstActiveIndex = techniqueList.IndexOf(first);
                    // Remove all mentions that aren't active and placed after the last active
                    techniqueList = techniqueList.Skip(firstActiveIndex).ToList();
                }

                var last = techniqueList.Where(i => i.Value).LastOrDefault();

                if (last.Value != false)
                {
                    var lastActiveIndex = techniqueList.IndexOf(last);
                    // Remove all mentions that aren't active and placed after the last active
                    techniqueList = techniqueList.Take(lastActiveIndex + 1).ToList();
                }

                model.Techniques = techniqueList.ToDictionary(i => i.Key, i => i.Value);

                model.OriginalTechniques = model.Techniques.ToJson().FromJson<Dictionary<string, bool>>();

            }
            catch (Exception)
            {
                //TODO: Add proper exception handling
            }

            return model;
        }
    }
}

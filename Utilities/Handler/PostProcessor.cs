using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using IniParser;
using IniParser.Parser;
using Serilog;

namespace Bundlingway.Utilities.Handler
{
    public static class PostProcessor
    {
        private static IniDataParser iniDataParser = new IniDataParser(new IniParser.Model.Configuration.IniParserConfiguration()
        {
            AllowCreateSectionsOnFly = true,
            AllowDuplicateKeys = true,
            AllowDuplicateSections = true,
            AllowKeysWithoutSection = true,
            AssigmentSpacer = "",
        });


        internal static void RunPipeline(ResourcePackage package)
        {

            Logging _logger = new();

            var baseline = Path.Combine(Instances.PackageFolder, package.Name);

            var presetPath = Path.Combine(baseline, "Presets");
            var texturePath = Path.Combine(baseline, @"Shaders\Textures");

            var iniParser = new FileIniDataParser(iniDataParser);

            var iniFiles = Directory.GetFiles(presetPath, "*.ini", SearchOption.AllDirectories)
                .Where(i => !i.EndsWith(@"\Off.ini")).ToList();

            List<string> textureFiles = Directory.Exists(texturePath) ? Directory.GetFiles(texturePath, "*.*", SearchOption.AllDirectories).ToList() : new();

            var techGraph = new Dictionary<string, int>();

            package.RunRawFilePipeline(_logger);

            foreach (string iniFile in iniFiles)
            {
                IniParser.Model.IniData ini_filedata = iniParser.ReadFile(iniFile);

                var preset = ini_filedata.ToPreset(iniFile);

                var techniqueList = string.Join(",", preset.Techniques.Select(i => i.Key).ToList());

                var techniques = techniqueList?
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Where(i => i.Contains('@', StringComparison.CurrentCulture))
                    .Select(i => i.Split('@')[1])
                    .Where(i => !i.Contains(".fx+", StringComparison.CurrentCulture))
                    .ToList() ?? [];

                foreach (var item in techniques)
                {

                    if (!techGraph.ContainsKey(item))
                        techGraph[item] = 0;

                    techGraph[item]++;
                }

                // Validate texture names
                foreach (var tex in preset.TextureFiles)
                {
                    if (!tex.Contains("/"))
                    {
                        var textNotFound = !textureFiles.Any(i => i.EndsWith("\\" + tex, StringComparison.CurrentCulture));

                        if (textNotFound)
                        {
                            Log.Information("[Missing Textures] " + tex + " @ " + Path.GetFileName(iniFile));
                        }
                    }
                }

                preset.RunPostProcessorPipeline(package, ini_filedata, _logger);
            }


            _logger.WriteLogToConsole();
            _logger.WriteLogToFile(Path.Combine(baseline, "installation-log.txt"));

        }
    }
}

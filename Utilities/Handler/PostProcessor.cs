using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using IniParser;
using IniParser.Parser;
using Serilog;
using System.Numerics;

namespace Bundlingway.Utilities.Handler
{
    public static class PostProcessor
    {
        private static readonly object _lock = new object();

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
            lock (_lock)
            {
                Logging _logger = new();

                var baseline = Path.Combine(Instances.PackageFolder, package.Name);

                var presetPath = Path.Combine(baseline, Constants.Folders.PackagePresets);

                if (!Directory.Exists(presetPath)) return;



                var texturePath = Path.Combine(baseline, Constants.Folders.PackageShaders);

                var iniParser = new FileIniDataParser(iniDataParser);

                var iniFiles = Directory.GetFiles(presetPath, "*.ini", SearchOption.AllDirectories)
                    .Where(i => !i.EndsWith(@"\Off.ini")).ToList();

                List<string> textureFiles = Directory.Exists(texturePath) ? Directory.GetFiles(texturePath, "*.*", SearchOption.AllDirectories).ToList() : new();

                var techGraph = new Dictionary<string, int>();

                package.RunRawFilePipeline(_logger);

                foreach (string iniFile in iniFiles)
                {
                    if (!File.Exists(iniFile))
                    {
                        Log.Warning("File not found while running pipeline: " + iniFile);
                        continue;
                    }

                    IniParser.Model.IniData ini_filedata = null;
                    Preset preset = null;

                    try
                    {
                        ini_filedata = iniParser.ReadFile(iniFile);
                        preset = ini_filedata.ToPreset(iniFile);
                    }
                    catch (Exception e)
                    {
                        Log.Warning("Failure to load INI: " + e.Message);
                    }

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
}

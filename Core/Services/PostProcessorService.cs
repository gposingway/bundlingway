using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Bundlingway.Utilities;
using IniParser;
using IniParser.Parser;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Bundlingway.Core.Services
{
    public class PostProcessorService
    {
        private static readonly object _lock = new object();
        private static IniDataParser iniDataParser = new IniDataParser(new IniParser.Model.Configuration.IniParserConfiguration()
        {
            AllowCreateSectionsOnFly = true,
            AllowDuplicateKeys = true,
            AllowDuplicateSections = true,
            AllowKeysWithoutSection = true,
            AssigmentSpacer = "",
            SkipInvalidLines = true,
        });

        private readonly IAppEnvironmentService _envService;

        public PostProcessorService(IAppEnvironmentService envService)
        {
            _envService = envService;
        }

        public void RunPipeline(ResourcePackage package)
        {
            lock (_lock)
            {
                Logging _logger = new();
                var baseline = Path.Combine(_envService.PackageFolder, package.Name);
                var presetPath = Path.Combine(baseline, Constants.Folders.PackagePresets);
                if (!Directory.Exists(presetPath)) return;
                var texturePath = Path.Combine(baseline, Constants.Folders.PackageShaders);
                List<string> textureFiles = Directory.Exists(texturePath) ? Directory.GetFiles(texturePath, "*.*", SearchOption.AllDirectories).ToList() : new();
                var iniParser = new FileIniDataParser(iniDataParser);
                var iniFiles = Directory.GetFiles(presetPath, "*.ini", SearchOption.AllDirectories)
                    .Where(i => !i.EndsWith(@"\Off.ini")).ToList();
                var techGraph = new Dictionary<string, int>();
                package.RunRawFilePipeline(_logger, _envService);
                foreach (string iniFile in iniFiles)
                {
                    if (!File.Exists(iniFile))
                    {
                        Log.Warning("File not found while running pipeline: " + iniFile);
                        continue;
                    }
                    IniParser.Model.IniData? ini_filedata = null;
                    Preset? preset = null;
                    try
                    {
                        ini_filedata = iniParser.ReadFile(iniFile);
                        preset = ini_filedata.ToPreset(iniFile);
                    }
                    catch (Exception e)
                    {
                        Log.Warning("Failure to load INI: " + e.Message);
                    }
                    if (preset?.Techniques != null)
                    {
                        var techniqueList = string.Join(",", preset.Techniques.Select(i => i.Key).ToList());
                        var techniques = techniqueList?
                            .Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Where(i => i.Contains('@', StringComparison.CurrentCulture))
                            .Select(i => i.Split('@')[1])
                            .Where(i => !i.Contains(".fx+", StringComparison.CurrentCulture))
                            .ToList() ?? new List<string>();
                        foreach (var item in techniques)
                        {
                            if (!techGraph.ContainsKey(item))
                                techGraph[item] = 0;
                            techGraph[item]++;
                        }
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
                        if (ini_filedata != null)
                        {
                            preset.RunPostProcessorPipeline(package, ini_filedata, _logger, _envService);
                        }
                    }
                }
                _logger.WriteLogToConsole();
                _logger.WriteLogToFile(Path.Combine(baseline, "installation-log.txt"));
            }
        }
    }
}

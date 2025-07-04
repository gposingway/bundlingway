﻿using Bundlingway.Core.Services;
using Bundlingway.Model;
using Bundlingway.Utilities;
using Serilog;
using System.Text.RegularExpressions;

namespace Bundlingway.PostProcess.RawFile
{
    public class FixInvalidINIGroupHeaders : IRawFileProcess
    {
        private readonly IAppEnvironmentService _envService;

        public bool ApplyToPresets { get; set; } = true;
        public bool ApplyToShaders { get; set; } = false;
        public int Order { get; set; } = 5;

        public FixInvalidINIGroupHeaders(IAppEnvironmentService envService)
        {
            _envService = envService;
        }

        public bool PostProcess(Preset preset)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetReplacementMap(ResourcePackage package, List<string> presetFileList, string baselinePath, Logging _logger)
        {
            var result = new Dictionary<string, string>();

            var baseline = Path.Combine(_envService.PackageFolder, package.Name);

            var presetPath = Path.Combine(baseline, "Presets");


            var iniFiles = Directory.GetFiles(presetPath, "*.ini", SearchOption.AllDirectories)
                .Where(i => !i.EndsWith(@"\Off.ini")).ToList();


            foreach (var ini in iniFiles)
            {
                var replacementMap = CreateReplacementMap(ini);
                foreach (var kvp in replacementMap)
                {
                    if (!result.ContainsKey(kvp.Key))
                    {
                        _logger.Log("Adjusted Presets", Path.GetFileName(ini), "FixInvalidINIGroupHeaders");
                        result.Add(kvp.Key, kvp.Value);
                    }
                }

            }

            if (result.Count > 0)
            {
                foreach (var kvp in result)
                {
                    _logger.Log("FixInvalidINIGroupHeaders", kvp.Key, kvp.Value);
                }
            }


            return result;
        }

        public static Dictionary<string, string> CreateReplacementMap(string iniFilePath)
        {
            var replacementMap = new Dictionary<string, string>();

            if (!File.Exists(iniFilePath))
            {
                Log.Information($"File '{iniFilePath}' does not exist.");
                return replacementMap;
            }

            var lines = File.ReadAllLines(iniFilePath);
            var headerPattern = new Regex(@"^\[(.+?)([\+\|].+)?\]$");

            foreach (var line in lines)
            {
                var match = headerPattern.Match(line);
                if (match.Success)
                {
                    var originalHeader = match.Value;
                    var cleanedHeader = $"[{match.Groups[1].Value}]";
                    if (!replacementMap.ContainsKey(originalHeader))
                    {
                        if (originalHeader != cleanedHeader && !originalHeader.EndsWith("+.fx]"))
                            replacementMap.Add(originalHeader, cleanedHeader);
                    }
                }
            }

            return replacementMap;
        }
    }
}
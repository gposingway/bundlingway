﻿using Bundlingway.Core.Services;
using Bundlingway.Model;
using Bundlingway.Utilities;
using System;
using System.Collections.Generic;

namespace Bundlingway.PostProcess.RawFile
{
    public class FixTexturePaths : IRawFileProcess
    {
        private readonly IAppEnvironmentService _envService;

        public bool ApplyToPresets { get; set; } = true;
        public bool ApplyToShaders { get; set; } = false;
        public int Order { get; set; } = 6;

        public FixTexturePaths(IAppEnvironmentService envService)
        {
            _envService = envService;
        }

        public bool PostProcess(Preset preset)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetReplacementMap(ResourcePackage package, List<string> presetFileList, string baselinePath, Logging _logger)
        {
            var replacementMap = new Dictionary<string, string>();
            var textureFolder = Path.Combine(_envService.PackageFolder, package.Name, Constants.Folders.PackageTextures);
            var presetFolder = Path.Combine(_envService.PackageFolder, package.Name, Constants.Folders.PackagePresets);

            foreach (var presetFile in presetFileList)
            {
                if (File.Exists(presetFile))
                {
                    var lines = File.ReadAllText(presetFile);
                    var texturePaths = ExtractTexturePaths(lines);
                    foreach (var texturePath in texturePaths)
                    {
                        if (!string.IsNullOrEmpty(texturePath) && texturePath.Contains("\\"))
                        {
                            var textureFileName = Path.GetFileName(texturePath);
                            var relativeTexturePath = Path.Combine(textureFolder, textureFileName);
                            if (File.Exists(relativeTexturePath))
                            {
                                _logger.Log("Adjusted Presets", Path.GetFileName(presetFile), "FixTexturePaths");

                                var finalTexturePlace = Path.Combine(package.Name, textureFileName).Replace("\\", "\\\\");
                                replacementMap[texturePath] = finalTexturePlace;
                            }
                        }
                    }
                }
            }

            if (replacementMap.Count > 0)
            {
                foreach (var kvp in replacementMap)
                {
                    _logger.Log("FixTexturePaths", kvp.Key, kvp.Value);
                }
            }

            return replacementMap;
        }

        private List<string> ExtractTexturePaths(string line)
        {
            var texturePaths = new List<string>();
            int startIndex = 0;

            while (true)
            {
                startIndex = line.IndexOf("\"", startIndex);
                if (startIndex == -1)
                    break;

                int endIndex = line.IndexOf("\"", startIndex + 1);
                if (endIndex == -1)
                    break;

                string texturePath = line.Substring(startIndex + 1, endIndex - startIndex - 1);

                if (Constants.TextureExtensions.Any(texturePath.ToLowerInvariant().EndsWith))
                {
                    texturePaths.Add(texturePath);
                }

                startIndex = endIndex + 1;
            }

            return texturePaths;
        }
    }
}

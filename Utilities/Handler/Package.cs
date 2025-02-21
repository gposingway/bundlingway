using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Serilog;
using SharpCompress.Archives;
using System.IO.Compression;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using SharpCompress.Common;
using System.Collections.Concurrent;
using Bundlingway.Utilities.ManagedResources;

namespace Bundlingway.Utilities.Handler
{
    public static class Package
    {
        internal static async Task<ResourcePackage> OnboardSinglePresetFile(string filePath)
        {
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            string newFileName = $"{fileName}{fileExtension}";

            if (!System.Text.RegularExpressions.Regex.IsMatch(filePath, @"\[\w{6}\]"))
            {
                string hashSignature = GetHashSignature(filePath);
                Log.Information($"Package.OnboardSinglePresetFile: File hash signature: [{hashSignature}]");

                newFileName = $"{fileName} [{hashSignature}]{fileExtension}";
            }

            var targetPath = Path.Combine(Instances.SinglePresetsFolder, Constants.Folders.PackagePresets);

            string targetFileName = Path.Combine(targetPath, newFileName);


            if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            File.Copy(filePath, targetFileName, true);

            Log.Information($"Package.OnboardSinglePresetFile: File copied to {targetFileName}");

            PostProcessor.RunPipeline(Constants.SingleFileCatalog);

            await Install(Instances.SinglePresetsFolder);
            Log.Information("Package.Onboard: Package installation completed.");

            return new ResourcePackage() { Name = fileName, Type = Constants.PackageCategories.SinglePreset };

        }

        private static string GetHashSignature(string filePath)
        {
            using var stream = File.OpenRead(filePath);
            byte[] hashBytes = SHA256.HashData(stream);
            StringBuilder hash = new();
            foreach (byte b in hashBytes)
            {
                hash.Append(b.ToString("X2"));
            }
            return hash.ToString().Substring(0, 6);
        }

        internal static async Task<ResourcePackage> Onboard(string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() == ".ini")
            {
                return OnboardSinglePresetFile(filePath).Result;
            }

            var packageHasPresets = false;
            var packageHasShaders = false;

            Log.Information($"Package.Onboard: Preparing package catalog for: {filePath}");

            var newCatalogEntry = new ResourcePackage
            {
                Type = Constants.PackageCategories.PresetCollection,
                Source = filePath
            };

            string fileExtension = Path.GetExtension(filePath).ToLower();
            Log.Information($"Package.Onboard: File extension: {fileExtension}");

            string collectionName = Path.GetFileNameWithoutExtension(filePath);
            newCatalogEntry.Name = collectionName;
            newCatalogEntry.Status = "Unpacking...";

            string originalTempFolderPath = Path.Combine(Instances.TempFolder, Guid.NewGuid().ToString());
            string tempFolderPath = originalTempFolderPath;

            if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            Directory.CreateDirectory(tempFolderPath);
            Log.Information($"Package.Onboard: Temporary folder created at: {tempFolderPath}");

            if (fileExtension == ".zip")
            {
                await Task.Run(() => ZipFile.ExtractToDirectory(filePath, tempFolderPath, true));
                Log.Information("Package.Onboard: ZIP file extracted.");
            }
            else if (fileExtension == ".rar")
            {
                await Task.Run(() =>
                {
                    using var archive = SharpCompress.Archives.Rar.RarArchive.Open(filePath);
                    archive.WriteToDirectory(tempFolderPath, new ExtractionOptions() { ExtractFullPath = true });
                });
                Log.Information("Package.Onboard: RAR file extracted.");
            }

            else if (fileExtension == ".7z")
            {
                await Task.Run(() =>
                {
                    using var archive = SharpCompress.Archives.SevenZip.SevenZipArchive.Open(filePath);
                    archive.WriteToDirectory(tempFolderPath, new ExtractionOptions() { ExtractFullPath = true });
                });
                Log.Information("Package.Onboard: 7z file extracted.");
            }


            if (!ValidatePackage(tempFolderPath))
            {
                Log.Information("Package.Onboard: Invalid package structure.");
                return null;
            }

            var changeEval = false;
            var isValidCatalogPayload = false;

            string catalogFilePath = Path.Combine(tempFolderPath, Constants.Files.CatalogEntry);
            if (File.Exists(catalogFilePath))
            {
                try
                {
                    var catalogEntry = SerializationExtensions.FromJsonFile<ResourcePackage>(catalogFilePath);
                    if (catalogEntry != null && !string.IsNullOrEmpty(catalogEntry.Name))
                    {
                        newCatalogEntry = catalogEntry;
                        Log.Information("Package.Onboard: Valid catalog-entry.json found and loaded.");
                    }
                }
                catch
                {
                    Log.Information("Package.Onboard: Invalid catalog-entry.json structure.");
                }
            }

            string targetPackagePath = "";

            if (!isValidCatalogPayload)
            {
                newCatalogEntry.Status = "Installing...";

                // First, handle Presets.

                do
                {
                    changeEval = false;

                    var reshadePresetsDir = Directory.GetDirectories(tempFolderPath, Constants.Folders.GamePresets, SearchOption.AllDirectories).FirstOrDefault();
                    if (reshadePresetsDir != null)
                    {
                        tempFolderPath = reshadePresetsDir;
                        changeEval = true;
                        Log.Information("Package.Onboard: Found 'reshade-presets' directory.");
                    }

                    if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath, "*.ini").Length == 0)
                    {
                        var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();

                        var probe = Path.GetFileName(singleFolderPath);

                        if (probe != Constants.Folders.PackageShaders && probe != Constants.Folders.PackageTextures && probe != Constants.Folders.PackagePresets)
                            collectionName = Path.GetFileName(singleFolderPath);

                        tempFolderPath = Path.Combine(tempFolderPath, probe);

                        newCatalogEntry.Name = collectionName;
                        changeEval = true;
                        Log.Information($"Package.Onboard: Single folder found, updated collection name to: {collectionName}");
                    }

                    if (Directory.GetDirectories(tempFolderPath, "Presets").Length == 1 && Directory.GetFiles(tempFolderPath, "*.ini").Length == 0)
                    {
                        var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();
                        tempFolderPath = Path.Combine(tempFolderPath, collectionName);

                        newCatalogEntry.Name = collectionName;
                        changeEval = true;
                        Log.Information($"Package.Onboard: Single folder found, updated collection name to: {collectionName}");
                    }

                } while (changeEval);


                if (newCatalogEntry.Name.EndsWith("-main"))
                {
                    newCatalogEntry.Name = newCatalogEntry.Name.Replace("-main", "");
                }

                targetPackagePath = Path.Combine(Instances.PackageFolder, newCatalogEntry.Name);

                if (!Directory.Exists(targetPackagePath)) Directory.CreateDirectory(targetPackagePath);
                Log.Information($"Package.Onboard: Target package path created at: {targetPackagePath}");

                Directory.CreateDirectory(Path.Combine(targetPackagePath, Constants.Folders.SourcePackage));
                var target = Path.Combine(targetPackagePath, Constants.Folders.SourcePackage, Path.GetFileName(filePath));

                File.Copy(filePath, target, true);
                Log.Information("Package.Onboard: Original file copied to target folder.");

                var presetsFolder = Path.Combine(targetPackagePath, Constants.Folders.PackagePresets);
                if (!Directory.Exists(presetsFolder)) Directory.CreateDirectory(presetsFolder);
                Log.Information("Package.Onboard: Presets folder created.");

                foreach (var file in Directory.GetFiles(tempFolderPath, "*.ini", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(tempFolderPath, file);
                    var targetPath = Path.Combine(presetsFolder, relativePath);

                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    File.Copy(file, targetPath, true);
                    packageHasPresets = true;
                }

                Log.Information("Package.Onboard: INI files copied to presets folder: " + presetsFolder);
                newCatalogEntry.LocalPresetFolder = presetsFolder;

                // Now, handle Textures.
                var texturesFolder = Path.Combine(targetPackagePath, Constants.Folders.PackageTextures);
                if (!Directory.Exists(texturesFolder)) Directory.CreateDirectory(texturesFolder);
                Log.Information("Package.Onboard: Shaders/Textures folder created.");

                tempFolderPath = originalTempFolderPath;

                do
                {
                    changeEval = false;

                    var reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories)
                        .FirstOrDefault(d => Path.GetFileName(d).Equals(Constants.Folders.GameShaders, StringComparison.OrdinalIgnoreCase));
                    if (reshadeShadersDir != null)
                    {
                        tempFolderPath = reshadeShadersDir;
                        changeEval = true;
                        Log.Information("Package.Onboard: Found 'reshade-shaders' directory.");
                    }

                    reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories)
                        .FirstOrDefault(d => Path.GetFileName(d).Equals("textures", StringComparison.OrdinalIgnoreCase));
                    if (reshadeShadersDir != null)
                    {
                        tempFolderPath = reshadeShadersDir;
                        changeEval = true;
                        Log.Information("Package.Onboard: Found 'textures' directory.");
                    }

                    if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath).Length == 0)
                    {
                        var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();

                        if (Path.GetFileName(singleFolderPath).ToLower() != "shaders")
                        {
                            tempFolderPath = singleFolderPath;
                            changeEval = true;
                        }
                    }

                } while (changeEval);

                foreach (var file in Directory.GetFiles(tempFolderPath, "*.*", SearchOption.AllDirectories))
                {
                    var textureFileExtension = Path.GetExtension(file).ToLower();
                    if (Constants.TextureExtensions.Contains(textureFileExtension))
                    {
                        if (!Constants.NonTextureImageMarkers.Any(marker => file.ToLower().Contains(marker.ToLower())))
                        {
                            var relativePath = Path.GetRelativePath(tempFolderPath, file);
                            var targetPath = Path.Combine(texturesFolder, relativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            File.Copy(file, targetPath, true);
                        }
                    }
                }
                Log.Information("Package.Onboard: Texture files copied to shaders folder: " + texturesFolder);
                newCatalogEntry.LocalTextureFolder = texturesFolder;

                foreach (var dir in Directory.GetDirectories(texturesFolder, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(dir).Equals("Previews", StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Delete(dir, true);
                        Log.Information("Package.Onboard: Removed 'Previews' directory.");
                    }
                }

                // Finally, handle Shader files.

                var shadersFolder = Path.Combine(targetPackagePath, Constants.Folders.PackageShaders);
                if (Directory.Exists(shadersFolder)) Directory.Delete(shadersFolder, true);
                Directory.CreateDirectory(shadersFolder);
                Log.Information("Package.Onboard: Shaders/shaders folder created.");

                tempFolderPath = originalTempFolderPath;

                do
                {
                    changeEval = false;

                    var reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories)
                        .FirstOrDefault(d => Path.GetFileName(d).Equals(Constants.Folders.GameShaders, StringComparison.OrdinalIgnoreCase));
                    if (reshadeShadersDir != null)
                    {
                        tempFolderPath = reshadeShadersDir;
                        changeEval = true;
                        Log.Information("Package.Onboard: Found 'reshade-shaders' directory.");
                    }

                    reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories)
                        .FirstOrDefault(d => Path.GetFileName(d).Equals("shaders", StringComparison.OrdinalIgnoreCase));
                    if (reshadeShadersDir != null)
                    {
                        tempFolderPath = reshadeShadersDir;
                        changeEval = true;
                        Log.Information("Package.Onboard: Found 'shaders' directory.");
                    }

                    if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath).Length == 0)
                    {
                        var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();

                        if (Path.GetFileName(singleFolderPath).ToLower() != "shaders")
                        {
                            tempFolderPath = singleFolderPath;
                            changeEval = true;
                        }
                    }

                } while (changeEval);

                foreach (var file in Directory.GetFiles(tempFolderPath, "*.*", SearchOption.AllDirectories))
                {
                    var shaderFileExtension = Path.GetExtension(file).ToLower();
                    if (Constants.ShaderExtensions.Contains(shaderFileExtension))
                    {
                        var relativePath = Path.GetRelativePath(tempFolderPath, file);
                        var targetPath = Path.Combine(shadersFolder, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                        File.Copy(file, targetPath, true);

                        packageHasShaders = true;
                    }
                }

                if (packageHasShaders) await Shader.SaveShaderAnalysisToPath(shadersFolder, Path.Combine(targetPackagePath, Constants.Files.ShaderAnalysis));

                Log.Information("Package.Onboard: shader files copied to shaders folder: " + shadersFolder);
                newCatalogEntry.LocalShaderFolder = shadersFolder;

                if (packageHasPresets && !packageHasShaders)
                    newCatalogEntry.Type = Constants.PackageCategories.PresetCollection;

                if (!packageHasPresets && packageHasShaders)
                    newCatalogEntry.Type = Constants.PackageCategories.ShaderCollection;

                if (packageHasPresets && packageHasShaders)
                    newCatalogEntry.Type = Constants.PackageCategories.MixedCollection;

                string localCatalogFilePath = Path.Combine(targetPackagePath, Constants.Files.CatalogEntry);
                newCatalogEntry.ToJsonFile(localCatalogFilePath);
                Log.Information("Package.Onboard: Catalog entry saved locally.");
            }

            // Remove all directories and files whose name contain any of the parts mentioned in Constants.NonTextureImageMarkers
            foreach (var dir in Directory.GetDirectories(targetPackagePath, "*", SearchOption.AllDirectories))
            {
                if (Constants.NonTextureImageMarkers.Any(marker => dir.Contains(marker, StringComparison.OrdinalIgnoreCase)))
                {
                    Directory.Delete(dir, true);
                    Log.Information($"Package.Onboard: Removed directory containing marker: {dir}");
                }
            }

            foreach (var file in Directory.GetFiles(targetPackagePath, "*", SearchOption.AllDirectories))
            {
                if (Constants.NonTextureImageMarkers.Any(marker => Path.GetFileName(file).Contains(marker, StringComparison.OrdinalIgnoreCase)))
                {
                    File.Delete(file);
                    Log.Information($"Package.Onboard: Removed file containing marker: {file}");
                }
            }

            // Clean all empty directories under targetPackagePath
            bool emptyDirFound = false;
            do
            {
                emptyDirFound = false;

                foreach (var dir in Directory.GetDirectories(targetPackagePath, "*", SearchOption.AllDirectories))
                {
                    if (Directory.GetFiles(dir).Length == 0 && Directory.GetDirectories(dir).Length == 0)
                    {
                        Directory.Delete(dir, false);
                        emptyDirFound = true;
                    }
                }
            } while (emptyDirFound);

            await Task.Run(() => PostProcessor.RunPipeline(newCatalogEntry));

            await Task.Run(() => Install(targetPackagePath));
            Log.Information("Package.Onboard: Package installation completed.");

            return newCatalogEntry;
        }

        private static bool ValidatePackage(string tempFolderPath)
        {
            // Validate if the directory exists
            if (!Directory.Exists(tempFolderPath))
            {
                return false;
            }

            // Run individual validation methods
            bool hasIniFiles = ValidateIniFiles(tempFolderPath);
            bool hasFxFiles = ValidateFxFiles(tempFolderPath);

            // A package with no INIs but with FX is a shader package.
            bool validationFlag = hasIniFiles || (!hasIniFiles && hasFxFiles);

            Console.WriteLine(validationFlag ? "Package is valid." : "Package is not valid.");
            return validationFlag;
        }

        private static bool ValidateIniFiles(string tempFolderPath)
        {
            // Search for .INI files in the directory and its subdirectories
            string[] iniFiles = Directory.GetFiles(tempFolderPath, "*.INI", SearchOption.AllDirectories);

            // Validate if there is at least one .INI file
            return iniFiles.Length > 0;
        }

        private static bool ValidateFxFiles(string tempFolderPath)
        {
            // Search for .fx files in the directory and its subdirectories
            string[] fxFiles = Directory.GetFiles(tempFolderPath, "*.fx", SearchOption.AllDirectories);

            // Validate if there is at least one .fx file
            return fxFiles.Length > 0;
        }

        public static async Task<ResourcePackage> Install(string sourcePackagePath)
        {
            Log.Information($"Package.Install: Installing package at: {sourcePackagePath}");

            string localCatalogFilePath = Path.Combine(sourcePackagePath, Constants.Files.CatalogEntry);

            if (!File.Exists(localCatalogFilePath))
            {
                Log.Information("Package.Install: catalog-entry.json not found. Exiting.");
                return null;
            }

            ResourcePackage catalogEntry = new();

            catalogEntry = SerializationExtensions.FromJsonFile<ResourcePackage>(localCatalogFilePath);

            string installationShaderAnalysisFilePath = Path.Combine(Instances.BundlingwayDataFolder, Constants.Files.ShaderAnalysis);
            string localShaderAnalysisFilePath = Path.Combine(sourcePackagePath, Constants.Files.ShaderAnalysis);

            var installationShaderAnalysis = SerializationExtensions.FromJsonFile<Dictionary<string, ShaderSignature>>(installationShaderAnalysisFilePath);
            var localShaderAnalysis = SerializationExtensions.FromJsonFile<Dictionary<string, ShaderSignature>>(localShaderAnalysisFilePath);

            Log.Information("Package.Install: Loaded catalog entry from file.");

            Log.Information($"Package.Install: catalogEntry.Name: {catalogEntry.Name}");
            Log.Information($"Package.Install: Game.InstallationFolder: {Instances.LocalConfigProvider.Configuration.Game.InstallationFolder}");

            var collectionName = catalogEntry.Name;
            var sourcePresetsFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackagePresets);
            var sourceTexturesFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackageTextures);
            var sourceShadersFolder = Path.Combine(sourcePackagePath, Constants.Folders.PackageShaders);

            string gamePresetsFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder, Constants.Folders.GamePresets, collectionName);
            string gameTexturesFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder, Constants.Folders.GameShaders, Constants.Folders.PackageTextures, collectionName);
            string gameShaderFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder, Constants.Folders.GameShaders, Constants.Folders.PackageShaders, collectionName);

            Log.Information($"Package.Install: sourcePresetsFolder: {sourcePresetsFolder}");
            Log.Information($"Package.Install: sourceTexturesFolder: {sourceTexturesFolder}");
            Log.Information($"Package.Install: sourceShadersFolder: {sourceShadersFolder}");
            Log.Information($"Package.Install: gamePresetsFolder: {gamePresetsFolder}");
            Log.Information($"Package.Install: gameTexturesFolder: {gameTexturesFolder}");

            try
            {

                catalogEntry.LocalPresetFolder = gamePresetsFolder;
                catalogEntry.LocalTextureFolder = gameTexturesFolder;
                catalogEntry.LocalShaderFolder = gameShaderFolder;


                // Presets:

                if (Directory.Exists(sourcePresetsFolder))
                {
                    Directory.CreateDirectory(gamePresetsFolder);
                    Log.Information($"Package.Install: Created game presets folder at: {gamePresetsFolder}");

                    foreach (var file in Directory.GetFiles(sourcePresetsFolder, "*.ini", SearchOption.AllDirectories))
                    {
                        var relativePath = Path.GetRelativePath(sourcePresetsFolder, file);
                        var targetPath = Path.Combine(gamePresetsFolder, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                        File.Copy(file, targetPath, true);
                        Log.Information($"Package.Install: Copied preset file {file} to {targetPath}");
                    }

                    var replacements = Instances.LocalConfigProvider.Configuration.Shortcuts.ToDictionary(k => "%" + k.Key + "%", v => v.Value);

                    PostProcessorExtensions.ReplaceValues(gamePresetsFolder, replacements);
                }


                // Textures:

                if (Directory.Exists(sourceTexturesFolder))
                    if (Directory.EnumerateFileSystemEntries(sourceTexturesFolder).ToList().Count != 0)
                    {
                        Directory.CreateDirectory(gameTexturesFolder);
                        Log.Information($"Package.Install: Created game textures folder at: {gameTexturesFolder}");

                        foreach (var file in Directory.GetFiles(sourceTexturesFolder, "*.*", SearchOption.AllDirectories))
                        {
                            var relativePath = Path.GetRelativePath(sourceTexturesFolder, file);
                            var targetPath = Path.Combine(gameTexturesFolder, relativePath);
                            Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                            File.Copy(file, targetPath, true);
                            Log.Information($"Package.Install: Copied texture file {file} to {targetPath}");
                        }
                    }

                // Shaders:

                if (catalogEntry.Type == Constants.PackageCategories.ShaderCollection)


                    if (Directory.Exists(sourceShadersFolder))
                    {
                        Directory.CreateDirectory(gameShaderFolder);

                        var referenceFolder = Path.Combine(Instances.PackageFolder, catalogEntry.Name, Constants.Folders.PackageShaders);


                        foreach (var file in Directory.GetFiles(sourceShadersFolder, "*.*", SearchOption.AllDirectories))
                        {
                            var shaderFileExtension = Path.GetExtension(file).ToLower();
                            if (Constants.ShaderExtensions.Contains(shaderFileExtension))
                            {
                                var relativePath = Path.GetRelativePath(referenceFolder, file);
                                var targetPath = Path.Combine(gameShaderFolder, relativePath);
                                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                                File.Copy(file, targetPath, true);
                                Log.Information($"Package.Install: Copied shader file {file} to {targetPath}");
                            }
                        }

                        //var analysisResult = await installationShaderAnalysis.CheckForConflictsWith(localShaderAnalysis);

                        //if (analysisResult.NonConflicting.Count > 0)
                        //{
                        //    // Copy non-conflicting files
                        //    foreach (var file in analysisResult.NonConflicting)
                        //    {
                        //        var source = Path.Combine(file.Location, file.FileName);
                        //        var target = Path.GetRelativePath(referenceFolder, source);

                        //        var destination = Path.Combine(gameShaderFolder, target);

                        //        Directory.CreateDirectory(Path.GetDirectoryName(destination));

                        //        File.Copy(source, destination, true);
                        //        Log.Information($"Package.Install: Copied texture file {file.FileName} to {gameShaderFolder}");
                        //    }
                        //}
                    }


                catalogEntry.Status = "Installed";
                catalogEntry.Installed = true;
                Log.Information("Package.Install: Updated catalog entry status to Installed.");

                catalogEntry.ToJsonFile(localCatalogFilePath);
                Log.Information("Package.Install: Saved updated catalog entry to file.");
                Log.Information("Package.Install: Package installed successfully.");
            }
            catch (Exception e)
            {
                catalogEntry.Status = "Error";
                catalogEntry.ToJsonFile(localCatalogFilePath);
            }

            return catalogEntry;
        }

        public static async Task RefreshInstalled()
        {
            Log.Information("Package.RefreshInstalled: Refreshing installed packages.");
            var installedPackages = Instances.ResourcePackages.Where(p => p.Installed).ToList();


            _= UI.Announce($"Updating {installedPackages.Count} installed packages, please wait...");

            int maxDegreeOfParallelism = 10;

            var tasks = Partitioner.Create(installedPackages)
                .GetPartitions(maxDegreeOfParallelism)
                .Select(async partition =>
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            var package = partition.Current;
                            Log.Information($"Package.RefreshInstalled: Refreshing package: {package.Name}");
                            Reinstall(package);
                        }
                    }
                });

            await Task.WhenAll(tasks);
            Log.Information("Package.RefreshInstalled: Installed packages refreshed.");
        }

        public static async Task Scan()
        {
            try
            {
                if (!Directory.Exists(Instances.PackageFolder)) return;

                Instances.ResourcePackages = [];

                var packageFiles = Directory.GetFiles(Instances.PackageFolder, Constants.Files.CatalogEntry, SearchOption.AllDirectories);
                Log.Information($"Package.Scan: Found {packageFiles.Length} package files.");

                foreach (var packageFile in packageFiles)
                {
                    var package = SerializationExtensions.FromJsonFile<ResourcePackage>(packageFile);

                    if (package != null && Validate(package))
                    {
                        Instances.ResourcePackages.Add(package);
                    }
                }

                // Scan for .ini files in SinglePresetsFolder
                var iniFiles = Directory.GetFiles(Instances.SinglePresetsFolder, "*.ini", SearchOption.AllDirectories);
                Log.Information($"Package.Scan: Found {iniFiles.Length} .ini files in SinglePresetsFolder.");

                foreach (var iniFile in iniFiles)
                {

                    //Clearly a hack. If the game folder doesn't exist, we're in local mode (and it should always report as 'not installed'). Sooo... wrong folder on purpose.
                    var baseFolder = Directory.Exists(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder) ?
                        Instances.LocalConfigProvider.Configuration.Game.InstallationFolder :
                        Instances.BundlingwayDataFolder;

                    var gameProbeFile = Path.Combine(baseFolder, Constants.Folders.GamePresets, Constants.SingleFileCatalog.Name, Path.GetFileName(iniFile));

                    var singlePresetPackage = new ResourcePackage
                    {
                        Name = Path.GetFileNameWithoutExtension(iniFile),
                        Source = iniFile,
                        Type = Constants.PackageCategories.SinglePreset,
                        Status = File.Exists(gameProbeFile) ? "Installed" : "Uninstalled",
                        Installed = File.Exists(gameProbeFile),
                        LocalPresetFolder = Path.GetDirectoryName(iniFile)
                    };

                    Instances.ResourcePackages.Add(singlePresetPackage);
                }

            }
            catch (Exception ex)
            {
                Log.Error($"Error in ScanPackages: {ex.Message}");
            }

            Instances.ResourcePackages = Instances.ResourcePackages.OrderBy(p => p.Name).ToList();
        }

        private static bool Validate(ResourcePackage package)
        {
            if (package.Hidden) return false;
            return true;
        }

        internal static void Remove(ResourcePackage package)
        {
            if (package.Locked) return;

            if (package.Type == Constants.PackageCategories.SinglePreset)
            {
                RemoveSinglePresetFile(package);
                return;
            }

            Log.Information($"Package.Remove: Removing package: {package.Name}");

            Uninstall(package);

            string targetPackagePath = Path.Combine(Instances.PackageFolder, package.Name);
            if (Directory.Exists(targetPackagePath))
            {
                Directory.Delete(targetPackagePath, true);
                Log.Information($"Package.Remove: Package {package.Name} removed successfully.");
            }
            else
            {
                Log.Information($"Package.Remove: Package {package.Name} not found.");
            }

        }

        private static void RemoveSinglePresetFile(ResourcePackage package)
        {
            UninstallSinglePresetFile(package);

            Log.Information($"Package.RemoveSinglePresetFile: Removing single preset file: {package.Name}");
            string targetPresetFile = package.Source;
            if (File.Exists(targetPresetFile))
            {
                File.Delete(targetPresetFile);
                Log.Information($"Package.RemoveSinglePresetFile: Single preset file {package.Name} removed successfully.");
            }
            else
            {
                Log.Information($"Package.RemoveSinglePresetFile: Single preset file {package.Name} not found.");
            }

        }

        private static void UninstallSinglePresetFile(ResourcePackage package)
        {
            Log.Information($"Package.UninstallSinglePresetFile: Uninstalling single preset file: {package.Name}");
            string targetPresetFile = Path.Combine(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder, Constants.Folders.GamePresets, Constants.Folders.SinglePresets, $"{package.Name}.ini");
            if (File.Exists(targetPresetFile))
            {
                File.Delete(targetPresetFile);
                Log.Information($"Package.UninstallSinglePresetFile: Single preset file {package.Name} uninstalled successfully.");
            }
            else
            {
                Log.Information($"Package.UninstallSinglePresetFile: Single preset file {package.Name} not found.");
            }
        }

        internal static void Uninstall(ResourcePackage package)
        {
            if (package.Locked) return;

            if (package.Type == Constants.PackageCategories.SinglePreset)
            {
                UninstallSinglePresetFile(package);
                return;
            }

            Log.Information($"Package.Uninstall: Uninstalling package: {package.Name}");

            if (Directory.Exists(package.LocalPresetFolder))
                Directory.Delete(package.LocalPresetFolder, true);

            if (Directory.Exists(package.LocalTextureFolder))
                Directory.Delete(package.LocalTextureFolder, true);

            if (Directory.Exists(package.LocalShaderFolder))
                Directory.Delete(package.LocalShaderFolder, true);

            package.Status = "Uninstalled";
            package.Installed = false;

            string localCatalogFilePath = Path.Combine(Instances.PackageFolder, package.Name, Constants.Files.CatalogEntry);
            package.ToJsonFile(localCatalogFilePath);
        }

        internal static void Reinstall(ResourcePackage? package)
        {

            if (package.Type == Constants.PackageCategories.SinglePreset)
            {
                InstallSinglePresetFile(package);
                return;
            }

            string localCatalogFilePath = Path.Combine(Instances.PackageFolder, package.Name);
            Install(localCatalogFilePath);
        }

        private static void InstallSinglePresetFile(ResourcePackage package)
        {
            Log.Information($"Package.InstallSinglePresetFile: Installing single preset file: {package.Name}");
            string source = package.Source;
            string target = Path.Combine(Instances.LocalConfigProvider.Configuration.Game.InstallationFolder, Constants.Folders.GamePresets, Constants.Folders.SinglePresets, $"{package.Name}.ini");

            if (File.Exists(source))
            {
                File.Copy(source, target, true);
                Log.Information($"Package.InstallSinglePresetFile: Single preset file {package.Name} installed successfully.");
            }
            else
            {
                Log.Information($"Package.InstallSinglePresetFile: Single preset file {package.Name} not found.");
            }
        }

        internal static async Task Onboard(List<string> selectedFiles)
        {
            var count = selectedFiles.Count;
            int current = 0;
            int maxDegreeOfParallelism = 5;

            var tasks = Partitioner.Create(selectedFiles)
                .GetPartitions(maxDegreeOfParallelism)
                .Select(async partition =>
                {
                    using (partition)
                    {
                        while (partition.MoveNext())
                        {
                            var file = partition.Current;
                            Interlocked.Increment(ref current);
                            _ = UI.Announce(Constants.MessageCategory.AddPackage, count.ToString(), current.ToString(), Path.GetFileName(file));

                            try
                            {
                                await Onboard(file);
                            }
                            catch (Exception e)
                            {
                                Log.Error(e, $"Error onboarding {file}: {e.Message}");
                            }

                        }
                    }
                });

            await Task.WhenAll(tasks);
        }

        public static async Task<string> DownloadAndInstall(string url)
        {
            // Log the start of the method
            using (HttpClient client = new())
            {
                try
                {
                    // Log the URL being downloaded
                    Log.Information($"DownloadAndInstall: Downloading from URL: {url}");

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    var contentDisposition = response.Content.Headers.ContentDisposition;
                    var filename = contentDisposition != null && !string.IsNullOrEmpty(contentDisposition.FileName)
                        ? contentDisposition.FileName.Trim('"')
                        : Path.GetFileName(HttpUtility.UrlDecode(url));

                    Directory.CreateDirectory(Instances.CacheFolder);

                    var filePath = Path.Combine(Instances.CacheFolder, filename);
                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }

                    // Log the file path where the content is saved
                    Log.Information($"DownloadAndInstall: File saved to: {filePath}");

                    // Call your install method here with the filePath
                    ResourcePackage package = Onboard(filePath).Result;

                    Maintenance.RemoveTempDir();

                    return package.Name + " installed successfully!";
                }
                catch (Exception ex)
                {
                    Log.Information($"Error downloading or installing file: {ex.Message}");
                    return "Error installing from " + url + ": " + ex.Message;
                }
            }

            // Log the end of the method
            return null;
        }

        public static void ToggleFavorite(ResourcePackage package)
        {
            package.Favorite = !package.Favorite;
            package.Save();
        }

        public static void Save(this ResourcePackage package)
        {
            string localCatalogFilePath = Path.Combine(Instances.PackageFolder, package.Name, Constants.Files.CatalogEntry);
            package.ToJsonFile(localCatalogFilePath);
        }

        internal static void ToggleLocked(ResourcePackage package)
        {
            package.Locked = !package.Locked;
            package.Save();
        }
    }
}

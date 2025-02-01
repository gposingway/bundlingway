using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using Serilog;
using SharpCompress.Archives;
using System.IO.Compression;
using System.Web;
using System.Security.Cryptography;
using System.Text;
using SharpCompress.Common;

namespace Bundlingway.Utilities.Handler
{
    public static class Package
    {
        internal static async Task OnboardSinglePresetFile(string filePath)
        {
            Log.Information("Package.OnboardSinglePresetFile: Start");

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string fileExtension = Path.GetExtension(filePath);

            string newFileName = $"{fileName}{fileExtension}";

            if (!System.Text.RegularExpressions.Regex.IsMatch(filePath, @"\[\w{6}\]"))
            {
                string hashSignature = GetHashSignature(filePath);
                Log.Information($"Package.OnboardSinglePresetFile: File hash signature: [{hashSignature}]");

                newFileName = $"{fileName} [{hashSignature}]{fileExtension}";
            }

            var targetPath = Path.Combine(Instances.SinglePresetsFolder, Constants.WellKnown.PresetFolder);

            string targetFileName = Path.Combine(targetPath, newFileName);


            if (!Directory.Exists(Path.GetDirectoryName(targetPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));

            File.Copy(filePath, targetFileName, true);

            Log.Information($"Package.OnboardSinglePresetFile: File copied to {targetFileName}");

            await Task.Run(() => PostProcessor.RunPipeline(Constants.SingleFileCatalog));

            await Task.Run(() => Install(Instances.SinglePresetsFolder));
            Log.Information("Package.Onboard: Package installation completed.");

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

        internal static async Task Onboard(string filePath)
        {
            if (Path.GetExtension(filePath).ToLower() == ".ini")
            {
                await OnboardSinglePresetFile(filePath);
                return;
            }

            Log.Information("Package.Onboard: Start");
            Log.Information($"Package.Onboard: Preparing package catalog for: {filePath}");

            var newCatalogEntry = new ResourcePackage
            {
                Type = "Preset Collection",
                Source = filePath
            };

            string fileExtension = Path.GetExtension(filePath).ToLower();
            Log.Information($"Package.Onboard: File extension: {fileExtension}");

            string collectionName = Path.GetFileNameWithoutExtension(filePath);
            newCatalogEntry.Name = collectionName;
            newCatalogEntry.Status = "Unzipping...";

            string originalTempFolderPath = Path.Combine(Instances.TempFolder, "presetUnpack.tmp");
            string tempFolderPath = originalTempFolderPath;

            if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            Directory.CreateDirectory(tempFolderPath);
            Log.Information($"Package.Onboard: Temporary folder created at: {tempFolderPath}");

            if (fileExtension == ".zip")
            {
                await Task.Run(() => ZipFile.ExtractToDirectory(filePath, tempFolderPath));
                Log.Information("Package.Onboard: ZIP file extracted.");
            }
            else if (fileExtension == ".rar")
            {
                await Task.Run(() =>
                {
                    using var archive = SharpCompress.Archives.Rar.RarArchive.Open(filePath);
                    archive.WriteToDirectory(tempFolderPath, new SharpCompress.Common.ExtractionOptions() { ExtractFullPath = true });
                });
                Log.Information("Package.Onboard: RAR file extracted.");
            }

            var changeEval = false;
            var isValidCatalogPayload = false;

            string catalogFilePath = Path.Combine(tempFolderPath, Constants.WellKnown.CatalogEntryFile);
            if (File.Exists(catalogFilePath))
            {
                try
                {
                    var catalogEntry = Serialization.FromJsonFile<ResourcePackage>(catalogFilePath);
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

                do
                {
                    changeEval = false;

                    var reshadePresetsDir = Directory.GetDirectories(tempFolderPath, Constants.WellKnown.GamePresetsFolder, SearchOption.AllDirectories).FirstOrDefault();
                    if (reshadePresetsDir != null)
                    {
                        tempFolderPath = reshadePresetsDir;
                        changeEval = true;
                        Log.Information("Package.Onboard: Found 'reshade-presets' directory.");
                    }

                    if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath).Length == 0)
                    {
                        var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();
                        collectionName = Path.GetFileName(singleFolderPath);
                        tempFolderPath = Path.Combine(tempFolderPath, collectionName);

                        newCatalogEntry.Name = collectionName;
                        changeEval = true;
                        Log.Information($"Package.Onboard: Single folder found, updated collection name to: {collectionName}");
                    }

                } while (changeEval);

                targetPackagePath = Path.Combine(Instances.PackageFolder, newCatalogEntry.Name);

                if (Directory.Exists(targetPackagePath)) Directory.Delete(targetPackagePath, true);
                Directory.CreateDirectory(targetPackagePath);
                Log.Information($"Package.Onboard: Target package path created at: {targetPackagePath}");

                var target = Path.Combine(targetPackagePath, "Source", Path.GetFileName(filePath));
                Directory.CreateDirectory(Path.Combine(targetPackagePath, "Source"));
                File.Copy(filePath, target, true);
                Log.Information("Package.Onboard: Original file copied to target folder.");

                var presetsFolder = Path.Combine(targetPackagePath, Constants.WellKnown.PresetFolder);
                if (Directory.Exists(presetsFolder)) Directory.Delete(presetsFolder, true);
                Directory.CreateDirectory(presetsFolder);
                Log.Information("Package.Onboard: Presets folder created.");

                foreach (var file in Directory.GetFiles(tempFolderPath, "*.ini", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(tempFolderPath, file);
                    var targetPath = Path.Combine(presetsFolder, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    File.Copy(file, targetPath, true);
                }
                Log.Information("Package.Onboard: INI files copied to presets folder: " + presetsFolder);
                newCatalogEntry.LocalPresetFolder = presetsFolder;

                // Now, handle Textures.
                var shadersFolder = Path.Combine(targetPackagePath, Constants.WellKnown.ShaderFolder, Constants.WellKnown.TextureFolder);
                if (Directory.Exists(shadersFolder)) Directory.Delete(shadersFolder, true);
                Directory.CreateDirectory(shadersFolder);
                Log.Information("Package.Onboard: Shaders/Textures folder created.");

                tempFolderPath = originalTempFolderPath;

                do
                {
                    changeEval = false;

                    var reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories)
                        .FirstOrDefault(d => Path.GetFileName(d).Equals(Constants.WellKnown.GameShadersFolder, StringComparison.OrdinalIgnoreCase));
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
                    if (textureFileExtension == ".jpg" || textureFileExtension == ".jpeg" || textureFileExtension == ".png")
                    {
                        var relativePath = Path.GetRelativePath(tempFolderPath, file);
                        var targetPath = Path.Combine(shadersFolder, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                        File.Copy(file, targetPath, true);
                    }
                }
                Log.Information("Package.Onboard: Texture files copied to shaders folder: " + shadersFolder);
                newCatalogEntry.LocalTextureFolder = shadersFolder;

                foreach (var dir in Directory.GetDirectories(shadersFolder, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(dir).Equals("Previews", StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Delete(dir, true);
                        Log.Information("Package.Onboard: Removed 'Previews' directory.");
                    }
                }

                string localCatalogFilePath = Path.Combine(targetPackagePath, Constants.WellKnown.CatalogEntryFile);
                newCatalogEntry.ToJsonFile(localCatalogFilePath);
                Log.Information("Package.Onboard: Catalog entry saved locally.");
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
        }

        internal static void Install(string targetPackagePath)
        {
            Log.Information("Package.Install: Start");
            Log.Information($"Package.Install: Installing package at: {targetPackagePath}");

            string localCatalogFilePath = Path.Combine(targetPackagePath, Constants.WellKnown.CatalogEntryFile);

            if (!File.Exists(localCatalogFilePath))
            {
                Log.Information("Package.Install: catalog-entry.json not found. Exiting.");
                return;
            }

            ResourcePackage catalogEntry = new ResourcePackage();
            catalogEntry = Serialization.FromJsonFile<ResourcePackage>(localCatalogFilePath);
            Log.Information("Package.Install: Loaded catalog entry from file.");

            Log.Information("Package.Install: presetsFolder: " + catalogEntry.Name);
            Log.Information("Package.Install: GameFolder: " + Instances.LocalConfigProvider.Configuration.GameFolder);

            var collectionName = catalogEntry.Name;
            var presetsFolder = Path.Combine(targetPackagePath, Constants.WellKnown.PresetFolder);
            var shadersFolder = Path.Combine(targetPackagePath, Constants.WellKnown.ShaderFolder, Constants.WellKnown.TextureFolder);

            string gamePresetsFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, Constants.WellKnown.GamePresetsFolder, collectionName);
            string gameTexturesFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, Constants.WellKnown.GameShadersFolder, Constants.WellKnown.TextureFolder, collectionName);

            Log.Information("Package.Install: presetsFolder: " + presetsFolder);
            Log.Information("Package.Install: shadersFolder: " + shadersFolder);
            Log.Information("Package.Install: gamePresetsFolder: " + gamePresetsFolder);
            Log.Information("Package.Install: gameTexturesFolder: " + gameTexturesFolder);


            Directory.CreateDirectory(gamePresetsFolder);
            Log.Information($"Package.Install: Created game presets folder at: {gamePresetsFolder}");

            catalogEntry.LocalPresetFolder = gamePresetsFolder;
            catalogEntry.LocalTextureFolder = gameTexturesFolder;

            foreach (var file in Directory.GetFiles(presetsFolder, "*.ini", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(presetsFolder, file);
                var targetPath = Path.Combine(gamePresetsFolder, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(file, targetPath, true);
                Log.Information($"Package.Install: Copied preset file {file} to {targetPath}");
            }

            if (Directory.Exists(shadersFolder))
                if (Directory.EnumerateFileSystemEntries(shadersFolder).ToList().Count != 0)
                {
                    Directory.CreateDirectory(gameTexturesFolder);
                    Log.Information($"Package.Install: Created game textures folder at: {gameTexturesFolder}");

                    foreach (var file in Directory.GetFiles(shadersFolder, "*.*", SearchOption.AllDirectories))
                    {
                        var relativePath = Path.GetRelativePath(shadersFolder, file);
                        var targetPath = Path.Combine(gameTexturesFolder, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                        File.Copy(file, targetPath, true);
                        Log.Information($"Package.Install: Copied texture file {file} to {targetPath}");
                    }
                }

            catalogEntry.Status = "Installed";
            catalogEntry.Installed = true;
            Log.Information("Package.Install: Updated catalog entry status to Installed.");

            if (Directory.Exists(Instances.TempFolder)) Directory.Delete(Instances.TempFolder, true);
            Log.Information("Package.Install: Deleted temporary folder.");

            catalogEntry.ToJsonFile(localCatalogFilePath);
            Log.Information("Package.Install: Saved updated catalog entry to file.");
            Log.Information("Package.Install: Package installed successfully.");
        }

        public static async Task Scan()
        {
            Log.Information("Package.Scan: Start");
            try
            {
                if (!Directory.Exists(Instances.PackageFolder)) return;

                Instances.ResourcePackages = [];

                var packageFiles = Directory.GetFiles(Instances.PackageFolder, Constants.WellKnown.CatalogEntryFile, SearchOption.AllDirectories);
                Log.Information($"Package.Scan: Found {packageFiles.Length} package files.");

                foreach (var packageFile in packageFiles)
                {
                    var package = Serialization.FromJsonFile<ResourcePackage>(packageFile);

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
                    var gameProbeFile = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, Constants.WellKnown.GamePresetsFolder, Constants.SingleFileCatalog.Name, Path.GetFileName(iniFile));

                    var singlePresetPackage = new ResourcePackage
                    {
                        Name = Path.GetFileNameWithoutExtension(iniFile),
                        Source = iniFile,
                        Type = Constants.WellKnown.SinglePresetFile,
                        Status = File.Exists(gameProbeFile) ? "Installed" : "Uninstalled",
                        Installed = File.Exists(gameProbeFile),
                        LocalPresetFolder = Path.GetDirectoryName(iniFile)
                    };

                    Instances.ResourcePackages.Add(singlePresetPackage);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error in ScanPackages: {ex.Message}");
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

            if (package.Type == Constants.WellKnown.SinglePresetFile)
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
            if (Directory.Exists(Instances.TempFolder)) Directory.Delete(Instances.TempFolder, true);

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
            string targetPresetFile = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, Constants.WellKnown.GamePresetsFolder, Constants.WellKnown.SinglePresetsFolder, $"{package.Name}.ini");
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

            if (package.Type == Constants.WellKnown.SinglePresetFile)
            {
                UninstallSinglePresetFile(package);
                return;
            }

            Log.Information("Package.Uninstall: Start");
            Log.Information($"Package.Uninstall: Uninstalling package: {package.Name}");

            if (Directory.Exists(package.LocalPresetFolder))
                Directory.Delete(package.LocalPresetFolder, true);
            if (Directory.Exists(package.LocalTextureFolder))
                Directory.Delete(package.LocalTextureFolder, true);

            package.Status = "Uninstalled";
            package.Installed = false;

            string localCatalogFilePath = Path.Combine(Instances.PackageFolder, package.Name, Constants.WellKnown.CatalogEntryFile);
            package.ToJsonFile(localCatalogFilePath);
        }

        internal static void Reinstall(ResourcePackage? package)
        {

            if (package.Type == Constants.WellKnown.SinglePresetFile)
            {
                InstallSinglePresetFile(package);
                return;
            }

            Log.Information("Package.Reinstall: Start");
            string localCatalogFilePath = Path.Combine(Instances.PackageFolder, package.Name);
            Install(localCatalogFilePath);
        }

        private static void InstallSinglePresetFile(ResourcePackage package)
        {
            Log.Information($"Package.InstallSinglePresetFile: Installing single preset file: {package.Name}");
            string source = package.Source;
            string target = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, Constants.WellKnown.GamePresetsFolder, Constants.WellKnown.SinglePresetsFolder, $"{package.Name}.ini");

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

            foreach (var file in selectedFiles)
            {
                current++;
                UI.Announce(Constants.MessageCategory.AddPackage, count.ToString(), current.ToString(), Path.GetFileName(file));

                await Onboard(file);
            }
        }

        public static async Task DownloadAndInstall(string url)
        {
            // Log the start of the method
            Log.Information("DownloadAndInstall: Start");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Log the URL being downloaded
                    Log.Information($"DownloadAndInstall: Downloading from URL: {url}");

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    // Log the successful download
                    Log.Information("DownloadAndInstall: Download successful");

                    var filename = Path.GetFileName(HttpUtility.UrlDecode(url));
                    Directory.CreateDirectory(Instances.CacheFolder);

                    var filePath = Path.Combine(Instances.CacheFolder, filename);
                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }

                    // Log the file path where the content is saved
                    Log.Information($"DownloadAndInstall: File saved to: {filePath}");

                    // Call your install method here with the filePath
                    await Onboard(filePath);

                    Maintenance.RemoveTempDir();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // Debugging message box
                    // Log any errors that occur during download or installation
                    Log.Information($"Error downloading or installing file: {ex.Message}");
                }
            }

            // Log the end of the method
            Log.Information("DownloadAndInstall: End");
        }
    }
}

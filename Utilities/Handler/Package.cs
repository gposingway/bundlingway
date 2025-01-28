using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using SharpCompress.Archives;
using System.IO.Compression;

namespace Bundlingway.Utilities.Handler
{
    public static class Package
    {
        internal static async Task Onboard(string filePath)
        {
            Console.WriteLine("Package.Onboard: Start");
            Console.WriteLine($"Package.Onboard: Preparing package catalog for: {filePath}");

            var newCatalogEntry = new ResourcePackage
            {
                Type = "Preset Collection",
                Source = filePath
            };

            string fileExtension = Path.GetExtension(filePath).ToLower();
            Console.WriteLine($"Package.Onboard: File extension: {fileExtension}");

            string collectionName = Path.GetFileNameWithoutExtension(filePath);
            newCatalogEntry.Name = collectionName;
            newCatalogEntry.Status = "Unzipping...";

            string originalTempFolderPath = Path.Combine(Instances.TempFolder, "presetUnpack.tmp");
            string tempFolderPath = originalTempFolderPath;

            if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            Directory.CreateDirectory(tempFolderPath);
            Console.WriteLine($"Package.Onboard: Temporary folder created at: {tempFolderPath}");

            if (fileExtension == ".zip")
            {
                await Task.Run(() => ZipFile.ExtractToDirectory(filePath, tempFolderPath));
                Console.WriteLine("Package.Onboard: ZIP file extracted.");
            }
            else if (fileExtension == ".rar")
            {
                await Task.Run(() =>
                {
                    using var archive = SharpCompress.Archives.Rar.RarArchive.Open(filePath);
                    archive.WriteToDirectory(tempFolderPath, new SharpCompress.Common.ExtractionOptions() { ExtractFullPath = true });
                });
                Console.WriteLine("Package.Onboard: RAR file extracted.");
            }

            var changeEval = false;
            var isValidCatalogPayload = false;

            string catalogFilePath = Path.Combine(tempFolderPath, "catalog-entry.json");
            if (File.Exists(catalogFilePath))
            {
                try
                {
                    var catalogEntry = Serialization.FromJsonFile<ResourcePackage>(catalogFilePath);
                    if (catalogEntry != null && !string.IsNullOrEmpty(catalogEntry.Name))
                    {
                        newCatalogEntry = catalogEntry;
                        Console.WriteLine("Package.Onboard: Valid catalog-entry.json found and loaded.");
                    }
                }
                catch
                {
                    Console.WriteLine("Package.Onboard: Invalid catalog-entry.json structure.");
                }
            }

            string targetPackagePath = "";

            if (!isValidCatalogPayload)
            {
                newCatalogEntry.Status = "Installing...";

                do
                {
                    changeEval = false;

                    var reshadePresetsDir = Directory.GetDirectories(tempFolderPath, "reshade-presets", SearchOption.AllDirectories).FirstOrDefault();
                    if (reshadePresetsDir != null)
                    {
                        tempFolderPath = reshadePresetsDir;
                        changeEval = true;
                        Console.WriteLine("Package.Onboard: Found 'reshade-presets' directory.");
                    }

                    if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath).Length == 0)
                    {
                        var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();
                        collectionName = Path.GetFileName(singleFolderPath);
                        tempFolderPath = Path.Combine(tempFolderPath, collectionName);

                        newCatalogEntry.Name = collectionName;
                        changeEval = true;
                        Console.WriteLine($"Package.Onboard: Single folder found, updated collection name to: {collectionName}");
                    }

                } while (changeEval);

                targetPackagePath = Path.Combine(Instances.PackageFolder, newCatalogEntry.Name);

                if (Directory.Exists(targetPackagePath)) Directory.Delete(targetPackagePath, true);
                Directory.CreateDirectory(targetPackagePath);
                Console.WriteLine($"Package.Onboard: Target package path created at: {targetPackagePath}");

                var target = Path.Combine(targetPackagePath, "Source", Path.GetFileName(filePath));
                Directory.CreateDirectory(Path.Combine(targetPackagePath, "Source"));
                File.Copy(filePath, target, true);
                Console.WriteLine("Package.Onboard: Original file copied to target folder.");

                var presetsFolder = Path.Combine(targetPackagePath, "Presets");
                if (Directory.Exists(presetsFolder)) Directory.Delete(presetsFolder, true);
                Directory.CreateDirectory(presetsFolder);
                Console.WriteLine("Package.Onboard: Presets folder created.");

                foreach (var file in Directory.GetFiles(tempFolderPath, "*.ini", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(tempFolderPath, file);
                    var targetPath = Path.Combine(presetsFolder, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    File.Copy(file, targetPath, true);
                }
                Console.WriteLine("Package.Onboard: INI files copied to presets folder.");

                // Now, handle Textures.
                var shadersFolder = Path.Combine(targetPackagePath, "Shaders", "Textures");
                if (Directory.Exists(shadersFolder)) Directory.Delete(shadersFolder, true);
                Directory.CreateDirectory(shadersFolder);
                Console.WriteLine("Package.Onboard: Shaders/Textures folder created.");

                tempFolderPath = originalTempFolderPath;

                do
                {
                    changeEval = false;

                    var reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories)
                        .FirstOrDefault(d => Path.GetFileName(d).Equals("reshade-shaders", StringComparison.OrdinalIgnoreCase));
                    if (reshadeShadersDir != null)
                    {
                        tempFolderPath = reshadeShadersDir;
                        changeEval = true;
                        Console.WriteLine("Package.Onboard: Found 'reshade-shaders' directory.");
                    }

                    reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "*", SearchOption.AllDirectories)
                        .FirstOrDefault(d => Path.GetFileName(d).Equals("textures", StringComparison.OrdinalIgnoreCase));
                    if (reshadeShadersDir != null)
                    {
                        tempFolderPath = reshadeShadersDir;
                        changeEval = true;
                        Console.WriteLine("Package.Onboard: Found 'textures' directory.");
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
                Console.WriteLine("Package.Onboard: Texture files copied to shaders folder.");

                foreach (var dir in Directory.GetDirectories(shadersFolder, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(dir).Equals("Previews", StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Delete(dir, true);
                        Console.WriteLine("Package.Onboard: Removed 'Previews' directory.");
                    }
                }

                string localCatalogFilePath = Path.Combine(targetPackagePath, "catalog-entry.json");
                newCatalogEntry.ToJsonFile(localCatalogFilePath);
                Console.WriteLine("Package.Onboard: Catalog entry saved locally.");
            }

            await Task.Run(() => PostProcessor.RunPipeline(newCatalogEntry));

            await Task.Run(() => Install(targetPackagePath));
            Console.WriteLine("Package.Onboard: Package installation completed.");
        }

        internal static void Install(string targetPackagePath)
        {
            Console.WriteLine("Package.Install: Start");
            Console.WriteLine($"Package.Install: Installing package at: {targetPackagePath}");

            string localCatalogFilePath = Path.Combine(targetPackagePath, "catalog-entry.json");

            if (!File.Exists(localCatalogFilePath))
            {
                Console.WriteLine("Package.Install: catalog-entry.json not found. Exiting.");
                return;
            }

            ResourcePackage catalogEntry = new ResourcePackage();
            catalogEntry = Serialization.FromJsonFile<ResourcePackage>(localCatalogFilePath);

            var collectionName = catalogEntry.Name;
            var presetsFolder = Path.Combine(targetPackagePath, "Presets");
            var shadersFolder = Path.Combine(targetPackagePath, "Shaders", "Textures");

            string gamePresetsFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "reshade-presets", collectionName);
            string gameTexturesFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "reshade-shaders", "Textures", collectionName);

            Directory.CreateDirectory(gamePresetsFolder);

            catalogEntry.LocalPresetFolder = gamePresetsFolder;
            catalogEntry.LocalTextureFolder = gameTexturesFolder;

            foreach (var file in Directory.GetFiles(presetsFolder, "*.ini", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(presetsFolder, file);
                var targetPath = Path.Combine(gamePresetsFolder, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(file, targetPath, true);
            }

            if (Directory.EnumerateFileSystemEntries(shadersFolder).ToList().Count != 0)
            {
                Directory.CreateDirectory(gameTexturesFolder);

                foreach (var file in Directory.GetFiles(shadersFolder, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(shadersFolder, file);
                    var targetPath = Path.Combine(gameTexturesFolder, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    File.Copy(file, targetPath, true);
                }
            }

            catalogEntry.Status = "Installed";
            catalogEntry.Installed = true;

            if (Directory.Exists(Instances.TempFolder)) Directory.Delete(Instances.TempFolder, true);

            catalogEntry.ToJsonFile(localCatalogFilePath);
            Console.WriteLine("Package.Install: Package installed successfully.");
        }

        public static async Task Scan()
        {
            Console.WriteLine("Package.Scan: Start");
            try
            {
                if (!Directory.Exists(Instances.PackageFolder)) return;

                Instances.ResourcePackages = [];

                var packageFiles = Directory.GetFiles(Instances.PackageFolder, "catalog-entry.json", SearchOption.AllDirectories);
                Console.WriteLine($"Package.Scan: Found {packageFiles.Length} package files.");

                foreach (var packageFile in packageFiles)
                {
                    var package = Serialization.FromJsonFile<ResourcePackage>(packageFile);

                    if (package != null && Validate(package))
                    {
                        Instances.ResourcePackages.Add(package);
                    }

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
            return true;
        }

        internal static void Remove(ResourcePackage package)
        {
            Console.WriteLine($"Package.Remove: Removing package: {package.Name}");

            Uninstall(package);

            string targetPackagePath = Path.Combine(Instances.PackageFolder, package.Name);
            if (Directory.Exists(targetPackagePath))
            {
                Directory.Delete(targetPackagePath, true);
                Console.WriteLine($"Package.Remove: Package {package.Name} removed successfully.");
            }
            else
            {
                Console.WriteLine($"Package.Remove: Package {package.Name} not found.");
            }
            if (Directory.Exists(Instances.TempFolder)) Directory.Delete(Instances.TempFolder, true);

        }

        internal static void Uninstall(ResourcePackage package)
        {
            Console.WriteLine("Package.Uninstall: Start");
            Console.WriteLine($"Package.Uninstall: Uninstalling package: {package.Name}");

            if (Directory.Exists(package.LocalPresetFolder))
                Directory.Delete(package.LocalPresetFolder, true);
            if (Directory.Exists(package.LocalTextureFolder))
                Directory.Delete(package.LocalTextureFolder, true);

            package.Status = "Uninstalled";
            package.Installed = false;

            string localCatalogFilePath = Path.Combine(Instances.PackageFolder, package.Name, "catalog-entry.json");
            package.ToJsonFile(localCatalogFilePath);
        }

        internal static void Reinstall(ResourcePackage? package)
        {
            Console.WriteLine("Package.Reinstall: Start");
            string localCatalogFilePath = Path.Combine(Instances.PackageFolder, package.Name);
            Install(localCatalogFilePath);
        }

        internal static async Task Onboard(List<string> selectedFiles)
        {
            foreach (var file in selectedFiles)
            {
                await Onboard(file);
            }
        }
    }
}

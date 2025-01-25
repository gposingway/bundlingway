using Bundlingway.Model;
using SharpCompress.Archives;
using System.IO.Compression;

namespace Bundlingway.Utilities
{
    public static class PackageManager
    {
        internal static void PreparePackageCatalog(string filePath)
        {
            Console.WriteLine("PackageManager.PreparePackageCatalog: Start");
            Console.WriteLine($"PackageManager.PreparePackageCatalog: Preparing package catalog for: {filePath}");

            var newCatalogEntry = new ResourcePackage
            {
                Type = "Preset Collection",
                Source = filePath
            };

            string fileExtension = Path.GetExtension(filePath).ToLower();
            Console.WriteLine($"PackageManager.PreparePackageCatalog: File extension: {fileExtension}");

            string collectionName = Path.GetFileNameWithoutExtension(filePath);
            newCatalogEntry.Name = collectionName;
            newCatalogEntry.Status = "Unzipping...";

            string originalTempFolderPath = Path.Combine(Instances.AppDataTempFolder, "presetUnpack.tmp");
            string tempFolderPath = originalTempFolderPath;

            if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            Directory.CreateDirectory(tempFolderPath);
            Console.WriteLine($"PackageManager.PreparePackageCatalog: Temporary folder created at: {tempFolderPath}");

            if (fileExtension == ".zip")
            {
                ZipFile.ExtractToDirectory(filePath, tempFolderPath);
                Console.WriteLine("PackageManager.PreparePackageCatalog: ZIP file extracted.");
            }
            else if (fileExtension == ".rar")
            {
                using var archive = SharpCompress.Archives.Rar.RarArchive.Open(filePath);
                archive.WriteToDirectory(tempFolderPath, new SharpCompress.Common.ExtractionOptions() { ExtractFullPath = true });
                Console.WriteLine("PackageManager.PreparePackageCatalog: RAR file extracted.");
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
                        Console.WriteLine("PackageManager.PreparePackageCatalog: Valid catalog-entry.json found and loaded.");
                    }
                }
                catch
                {
                    Console.WriteLine("PackageManager.PreparePackageCatalog: Invalid catalog-entry.json structure.");
                }
            }

            string targetPackagePath = Path.Combine(Instances.AppDataFolder, "Packages", newCatalogEntry.Name);
            if (Directory.Exists(targetPackagePath)) Directory.Delete(targetPackagePath, true);
            Directory.CreateDirectory(targetPackagePath);
            Console.WriteLine($"PackageManager.PreparePackageCatalog: Target package path created at: {targetPackagePath}");

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
                        Console.WriteLine("PackageManager.PreparePackageCatalog: Found 'reshade-presets' directory.");
                    }

                    if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath).Length == 0)
                    {
                        var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();
                        collectionName = Path.GetFileName(singleFolderPath);
                        tempFolderPath = Path.Combine(tempFolderPath, collectionName);

                        newCatalogEntry.Name = collectionName;
                        changeEval = true;
                        Console.WriteLine($"PackageManager.PreparePackageCatalog: Single folder found, updated collection name to: {collectionName}");
                    }

                } while (changeEval);

                var target = Path.Combine(targetPackagePath, "Source", Path.GetFileName(filePath));
                Directory.CreateDirectory(Path.Combine(targetPackagePath, "Source"));
                File.Copy(filePath, target, true);
                Console.WriteLine("PackageManager.PreparePackageCatalog: Original file copied to target folder.");

                var presetsFolder = Path.Combine(targetPackagePath, "Presets");
                if (Directory.Exists(presetsFolder)) Directory.Delete(presetsFolder, true);
                Directory.CreateDirectory(presetsFolder);
                Console.WriteLine("PackageManager.PreparePackageCatalog: Presets folder created.");

                foreach (var file in Directory.GetFiles(tempFolderPath, "*.ini", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(tempFolderPath, file);
                    var targetPath = Path.Combine(presetsFolder, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    File.Copy(file, targetPath, true);
                }
                Console.WriteLine("PackageManager.PreparePackageCatalog: INI files copied to presets folder.");

                var shadersFolder = Path.Combine(targetPackagePath, "Shaders", "Textures");
                if (Directory.Exists(shadersFolder)) Directory.Delete(shadersFolder, true);
                Directory.CreateDirectory(shadersFolder);
                Console.WriteLine("PackageManager.PreparePackageCatalog: Shaders/Textures folder created.");

                tempFolderPath = originalTempFolderPath;

                do
                {
                    changeEval = false;

                    var reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "reshade-shaders", SearchOption.AllDirectories).FirstOrDefault();
                    if (reshadeShadersDir != null)
                    {
                        tempFolderPath = reshadeShadersDir;
                        changeEval = true;
                        Console.WriteLine("PackageManager.PreparePackageCatalog: Found 'reshade-shaders' directory.");
                    }

                    reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "textures", SearchOption.AllDirectories).FirstOrDefault();
                    if (reshadeShadersDir != null)
                    {
                        tempFolderPath = reshadeShadersDir;
                        changeEval = true;
                        Console.WriteLine("PackageManager.PreparePackageCatalog: Found 'textures' directory.");
                    }

                    if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath).Length == 0)
                    {
                        var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();
                        collectionName = Path.GetFileName(singleFolderPath);
                        tempFolderPath = Path.Combine(tempFolderPath, collectionName);

                        newCatalogEntry.Name = collectionName;
                        changeEval = true;
                        Console.WriteLine($"PackageManager.PreparePackageCatalog: Single folder found, updated collection name to: {collectionName}");
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
                Console.WriteLine("PackageManager.PreparePackageCatalog: Texture files copied to shaders folder.");

                foreach (var dir in Directory.GetDirectories(shadersFolder, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(dir).Equals("Previews", StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Delete(dir, true);
                        Console.WriteLine("PackageManager.PreparePackageCatalog: Removed 'Previews' directory.");
                    }
                }

                string localCatalogFilePath = Path.Combine(targetPackagePath, "catalog-entry.json");
                newCatalogEntry.ToJsonFile(localCatalogFilePath);
                Console.WriteLine("PackageManager.PreparePackageCatalog: Catalog entry saved locally.");
            }

            InstallPackage(targetPackagePath);
            Console.WriteLine("PackageManager.PreparePackageCatalog: Package installation completed.");
        }

        internal static void InstallPackage(string targetPackagePath)
        {
            Console.WriteLine("PackageManager.InstallPackage: Start");
            Console.WriteLine($"PackageManager.InstallPackage: Installing package at: {targetPackagePath}");

            string localCatalogFilePath = Path.Combine(targetPackagePath, "catalog-entry.json");

            ResourcePackage catalogEntry = new ResourcePackage();
            catalogEntry = Serialization.FromJsonFile<ResourcePackage>(localCatalogFilePath);

            var collectionName = catalogEntry.Name;
            var presetsFolder = Path.Combine(targetPackagePath, "Presets");
            var shadersFolder = Path.Combine(targetPackagePath, "Shaders", "Textures");

            string gamePresetsFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "reshade-presets", collectionName);
            string gameTexturesFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "reshade-shaders", "Textures", collectionName);

            catalogEntry.LocalBasePath = gamePresetsFolder;

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

            if (Directory.Exists(Instances.AppDataTempFolder)) Directory.Delete(Instances.AppDataTempFolder, true);

            catalogEntry.ToJsonFile(localCatalogFilePath);
            Console.WriteLine("PackageManager.InstallPackage: Package installed successfully.");
        }

        public static async Task ScanPackages()
        {
            Console.WriteLine("PackageManager.ScanPackages: Start");
            try
            {
                string packagesFolder = Path.Combine(Instances.AppDataFolder, "Packages");
                if (!Directory.Exists(packagesFolder)) return;

                Instances.ResourcePackages = [];

                var packageFiles = Directory.GetFiles(packagesFolder, "catalog-entry.json", SearchOption.AllDirectories);
                Console.WriteLine($"PackageManager.ScanPackages: Found {packageFiles.Length} package files.");

                foreach (var packageFile in packageFiles)
                {
                    Console.WriteLine($"PackageManager.ScanPackages: Reading file: {packageFile}");

                    var package = Serialization.FromJsonFile<ResourcePackage>(packageFile);

                    Console.WriteLine($"PackageManager.ScanPackages: Streaming finished.");

                    if (package != null && ValidatePackageCatalog(package))
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
            Console.WriteLine("PackageManager.ScanPackages: Completed");
        }

        private static bool ValidatePackageCatalog(ResourcePackage package)
        {
            Console.WriteLine("PackageManager.ValidatePackageCatalog: Start");
            return true;
        }
    }
}

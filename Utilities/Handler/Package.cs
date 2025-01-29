using Bundlingway.Model;
using Bundlingway.Utilities.Extensions;
using SharpCompress.Archives;
using System.IO.Compression;
using System.Web;

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

            string catalogFilePath = Path.Combine(tempFolderPath, Constants.WellKnown.CatalogEntryFile);
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

                var presetsFolder = Path.Combine(targetPackagePath, Constants.WellKnown.PresetFolder);
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
                Console.WriteLine("Package.Onboard: INI files copied to presets folder: " + presetsFolder);
                newCatalogEntry.LocalPresetFolder = presetsFolder;


                // Now, handle Textures.
                var shadersFolder = Path.Combine(targetPackagePath, Constants.WellKnown.ShaderFolder, Constants.WellKnown.TextureFolder);
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
                Console.WriteLine("Package.Onboard: Texture files copied to shaders folder: " + shadersFolder);
                newCatalogEntry.LocalTextureFolder = shadersFolder;


                foreach (var dir in Directory.GetDirectories(shadersFolder, "*", SearchOption.AllDirectories))
                {
                    if (Path.GetFileName(dir).Equals("Previews", StringComparison.OrdinalIgnoreCase))
                    {
                        Directory.Delete(dir, true);
                        Console.WriteLine("Package.Onboard: Removed 'Previews' directory.");
                    }
                }

                string localCatalogFilePath = Path.Combine(targetPackagePath, Constants.WellKnown.CatalogEntryFile);
                newCatalogEntry.ToJsonFile(localCatalogFilePath);
                Console.WriteLine("Package.Onboard: Catalog entry saved locally.");
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
            Console.WriteLine("Package.Onboard: Package installation completed.");
        }

        internal static void Install(string targetPackagePath)
        {
            Console.WriteLine("Package.Install: Start");
            Console.WriteLine($"Package.Install: Installing package at: {targetPackagePath}");

            string localCatalogFilePath = Path.Combine(targetPackagePath, Constants.WellKnown.CatalogEntryFile);

            if (!File.Exists(localCatalogFilePath))
            {
                Console.WriteLine("Package.Install: catalog-entry.json not found. Exiting.");
                return;
            }

            ResourcePackage catalogEntry = new ResourcePackage();
            catalogEntry = Serialization.FromJsonFile<ResourcePackage>(localCatalogFilePath);
            Console.WriteLine("Package.Install: Loaded catalog entry from file.");

            Console.WriteLine("Package.Install: presetsFolder: " + catalogEntry.Name);
            Console.WriteLine("Package.Install: GameFolder: " + Instances.LocalConfigProvider.Configuration.GameFolder);

            var collectionName = catalogEntry.Name;
            var presetsFolder = Path.Combine(targetPackagePath, Constants.WellKnown.PresetFolder);
            var shadersFolder = Path.Combine(targetPackagePath, Constants.WellKnown.ShaderFolder, Constants.WellKnown.TextureFolder);

            string gamePresetsFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "reshade-presets", collectionName);
            string gameTexturesFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "reshade-shaders", "Textures", collectionName);

            Console.WriteLine("Package.Install: presetsFolder: " + presetsFolder);
            Console.WriteLine("Package.Install: shadersFolder: " + shadersFolder);
            Console.WriteLine("Package.Install: gamePresetsFolder: " + gamePresetsFolder);
            Console.WriteLine("Package.Install: gameTexturesFolder: " + gameTexturesFolder);


            Directory.CreateDirectory(gamePresetsFolder);
            Console.WriteLine($"Package.Install: Created game presets folder at: {gamePresetsFolder}");

            catalogEntry.LocalPresetFolder = gamePresetsFolder;
            catalogEntry.LocalTextureFolder = gameTexturesFolder;

            foreach (var file in Directory.GetFiles(presetsFolder, "*.ini", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(presetsFolder, file);
                var targetPath = Path.Combine(gamePresetsFolder, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(file, targetPath, true);
                Console.WriteLine($"Package.Install: Copied preset file {file} to {targetPath}");
            }

            if (Directory.Exists(shadersFolder))
                if (Directory.EnumerateFileSystemEntries(shadersFolder).ToList().Count != 0)
                {
                    Directory.CreateDirectory(gameTexturesFolder);
                    Console.WriteLine($"Package.Install: Created game textures folder at: {gameTexturesFolder}");

                    foreach (var file in Directory.GetFiles(shadersFolder, "*.*", SearchOption.AllDirectories))
                    {
                        var relativePath = Path.GetRelativePath(shadersFolder, file);
                        var targetPath = Path.Combine(gameTexturesFolder, relativePath);
                        Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                        File.Copy(file, targetPath, true);
                        Console.WriteLine($"Package.Install: Copied texture file {file} to {targetPath}");
                    }
                }

            catalogEntry.Status = "Installed";
            catalogEntry.Installed = true;
            Console.WriteLine("Package.Install: Updated catalog entry status to Installed.");

            if (Directory.Exists(Instances.TempFolder)) Directory.Delete(Instances.TempFolder, true);
            Console.WriteLine("Package.Install: Deleted temporary folder.");

            catalogEntry.ToJsonFile(localCatalogFilePath);
            Console.WriteLine("Package.Install: Saved updated catalog entry to file.");
            Console.WriteLine("Package.Install: Package installed successfully.");
        }

        public static async Task Scan()
        {
            Console.WriteLine("Package.Scan: Start");
            try
            {
                if (!Directory.Exists(Instances.PackageFolder)) return;

                Instances.ResourcePackages = [];

                var packageFiles = Directory.GetFiles(Instances.PackageFolder, Constants.WellKnown.CatalogEntryFile, SearchOption.AllDirectories);
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

            string localCatalogFilePath = Path.Combine(Instances.PackageFolder, package.Name, Constants.WellKnown.CatalogEntryFile);
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
            Console.WriteLine("DownloadAndInstall: Start");

            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Log the URL being downloaded
                    Console.WriteLine($"DownloadAndInstall: Downloading from URL: {url}");

                    var response = await client.GetAsync(url);
                    response.EnsureSuccessStatusCode();

                    // Log the successful download
                    Console.WriteLine("DownloadAndInstall: Download successful");

                    var filename = Path.GetFileName(HttpUtility.UrlDecode(url));
                    Directory.CreateDirectory(Instances.CacheFolder);

                    var filePath = Path.Combine(Instances.CacheFolder, filename);
                    using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        await response.Content.CopyToAsync(fs);
                    }

                    // Log the file path where the content is saved
                    Console.WriteLine($"DownloadAndInstall: File saved to: {filePath}");

                    // Call your install method here with the filePath
                    await Onboard(filePath);

                    Maintenance.RemoveTempDir();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message); // Debugging message box
                    // Log any errors that occur during download or installation
                    Console.WriteLine($"Error downloading or installing file: {ex.Message}");
                }
            }

            // Log the end of the method
            Console.WriteLine("DownloadAndInstall: End");
        }
    }
}

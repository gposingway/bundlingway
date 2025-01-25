using Bundlingway.Model;
using SharpCompress.Archives;
using System.IO.Compression;

namespace Bundlingway.Utilities
{
    public static class PackageManager
    {
        internal static void InstallPackage(string filePath, ResourcePackage newCatalogEntry)
        {

            newCatalogEntry.Type = "Preset Collection";
            newCatalogEntry.Source = filePath;


            // Step 1 - Extract the contents of the package to a temp folder.

            string fileExtension = Path.GetExtension(filePath).ToLower();

            string collectionName = Path.GetFileNameWithoutExtension(filePath);
            newCatalogEntry.Name = collectionName;
            newCatalogEntry.Status = "Unzipping...";
            Instances.MainDataSource.ResetBindings(false);

            string originalTempFolderPath = Path.Combine(Instances.AppDataFolder, "cache", "presetUnpack.tmp");

            string tempFolderPath = originalTempFolderPath;

            // Drop then recreate the temp folder.

            if (Directory.Exists(tempFolderPath)) Directory.Delete(tempFolderPath, true);
            Directory.CreateDirectory(tempFolderPath);

            if (fileExtension == ".zip")
            {
                ZipFile.ExtractToDirectory(filePath, tempFolderPath);
            }
            else if (fileExtension == ".rar")
            {
                // For .RAR files, you'll need a third-party library like SharpCompress.
                // Install-Package SharpCompress
                using var archive = SharpCompress.Archives.Rar.RarArchive.Open(filePath);
                archive.WriteToDirectory(tempFolderPath, new SharpCompress.Common.ExtractionOptions() { ExtractFullPath = true });
            }

            var changeEval = false;

            do
            {
                changeEval = false;

                // If one of the subdirectories of the temp folder is named 'reshade-presets' (case-insensitive), use it as tempFolderPath.
                var reshadePresetsDir = Directory.GetDirectories(tempFolderPath, "reshade-presets", SearchOption.AllDirectories).FirstOrDefault();
                if (reshadePresetsDir != null)
                {
                    tempFolderPath = reshadePresetsDir;
                    changeEval = true;
                }

                // If the temp folder contains only one folder, use it as reference for the collection name.
                if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath).Length == 0)
                {
                    var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();
                    collectionName = Path.GetFileName(singleFolderPath);
                    tempFolderPath = Path.Combine(tempFolderPath, collectionName);

                    newCatalogEntry.Name = collectionName;

                    changeEval = true;
                }



            } while (changeEval);


            newCatalogEntry.Status = "Installing...";
            Instances.MainDataSource.ResetBindings(false);

            // Step 2 - Create the local reference folder for the package, and move things around as necessary.

            string targetPackagePath = Path.Combine(Instances.AppDataFolder, "Packages", collectionName);
            if (Directory.Exists(targetPackagePath)) Directory.Delete(targetPackagePath, true);
            Directory.CreateDirectory(targetPackagePath);

            // Copy the original file to the target folder as a backup. 
            var target = Path.Combine(targetPackagePath, Path.GetFileName(filePath));
            File.Copy(filePath, target, true);

            // Create the reference Presets folder. 
            var presetsFolder = Path.Combine(targetPackagePath, "Presets");
            if (Directory.Exists(presetsFolder)) Directory.Delete(presetsFolder, true);
            Directory.CreateDirectory(presetsFolder);

            // Copy all .ini files to the presets folder, preserving the folder structure.

            foreach (var file in Directory.GetFiles(tempFolderPath, "*.ini", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(tempFolderPath, file);
                var targetPath = Path.Combine(presetsFolder, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(file, targetPath, true);
            }

            // create the reference Shaders/Textures folder.
            var shadersFolder = Path.Combine(targetPackagePath, "Shaders", "Textures");
            if (Directory.Exists(shadersFolder)) Directory.Delete(shadersFolder, true);
            Directory.CreateDirectory(shadersFolder);


            // Reset the temp folder path.
            tempFolderPath = originalTempFolderPath;

            do
            {
                changeEval = false;

                // If one of the subdirectories of the temp folder is named 'reshade-shaders', use it as tempFolderPath.
                var reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "reshade-shaders", SearchOption.AllDirectories).FirstOrDefault();
                if (reshadeShadersDir != null)
                {
                    tempFolderPath = reshadeShadersDir;
                    changeEval = true;
                }

                // If one of the subdirectories of the temp folder is named 'textures', use it as tempFolderPath.
                reshadeShadersDir = Directory.GetDirectories(tempFolderPath, "textures", SearchOption.AllDirectories).FirstOrDefault();
                if (reshadeShadersDir != null)
                {
                    tempFolderPath = reshadeShadersDir;
                    changeEval = true;
                }

                // If the temp folder contains only one folder, use it as reference for the collection name.
                if (Directory.GetDirectories(tempFolderPath).Length == 1 && Directory.GetFiles(tempFolderPath).Length == 0)
                {
                    var singleFolderPath = Directory.GetDirectories(tempFolderPath).First();
                    collectionName = Path.GetFileName(singleFolderPath);
                    tempFolderPath = Path.Combine(tempFolderPath, collectionName);

                    newCatalogEntry.Name = collectionName;

                    changeEval = true;
                }



            } while (changeEval);


            // Copy all .jpg, .jpeg and .png files to the Textures folder, under the same folder name as the presets and preserving the folder structure.

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

            // Remove all subfolders named 'Preview' from the target shaders folder.
            foreach (var dir in Directory.GetDirectories(shadersFolder, "*", SearchOption.AllDirectories))
            {
                if (Path.GetFileName(dir).Equals("Previews", StringComparison.OrdinalIgnoreCase))
                {
                    Directory.Delete(dir, true);
                }
            }

            // Step 3: Copy the content from the presets folder to the game folder

            string gamePresetsFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "reshade-presets", collectionName);

            newCatalogEntry.LocalBasePath = gamePresetsFolder;

            Directory.CreateDirectory(gamePresetsFolder);

            foreach (var file in Directory.GetFiles(presetsFolder, "*.ini", SearchOption.AllDirectories))
            {
                var relativePath = Path.GetRelativePath(presetsFolder, file);
                var targetPath = Path.Combine(gamePresetsFolder, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                File.Copy(file, targetPath, true);
            }

            // check if the targetShadersFolder directory has no files.

            if (Directory.EnumerateFileSystemEntries(shadersFolder).ToList().Count != 0)
            {
                string gameTexturesFolder = Path.Combine(Instances.LocalConfigProvider.Configuration.GameFolder, "reshade-shaders", "Textures", collectionName);
                Directory.CreateDirectory(gameTexturesFolder);

                foreach (var file in Directory.GetFiles(shadersFolder, "*.*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(shadersFolder, file);
                    var targetPath = Path.Combine(gameTexturesFolder, relativePath);
                    Directory.CreateDirectory(Path.GetDirectoryName(targetPath));
                    File.Copy(file, targetPath, true);
                }

            }

            newCatalogEntry.Status = "Installed";
            newCatalogEntry.Installed = true;
            Instances.MainDataSource.ResetBindings(false);

        }
    }
}
